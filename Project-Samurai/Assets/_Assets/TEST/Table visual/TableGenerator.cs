using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public class TableGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [Min(1)] public int gridWidth = 10;   // columnas (X)
    [Min(1)] public int gridHeight = 8;  // filas (Y)

    [Header("UI References")]
    public Transform gridParent;                 // debe tener GridLayoutGroup + RectTransform
    public WordSearchLetter letterPrefab;

    [Header("Words (WILL BE UPPERCASED)")]
    public string[] words;

    [Header("Generation")]
    [Tooltip("0 = aleatorio, otro = determinista")]
    public int randomSeed = 0;

    [Header("Auto Fit Layout")]
    [Tooltip("Si está activo, ajusta cellSize para ocupar TODO el rect disponible.")]
    public bool autoFit = true;

    [Tooltip("Celdas cuadradas (recomendado para sopa de letras).")]
    public bool squareCells = true;

    [Tooltip("Offset en pixeles que se RESTA al tamaño base de celda (X=ancho, Y=alto). " +
         "Útil para evitar que se vean pegadas. Ej: (2,2).")]
    public Vector2 cellSizeOffset = new Vector2(2f, 2f);

    [Tooltip("Mínimo tamaño permitido por celda (por seguridad).")]
    public float minCellSize = 8f;

    private System.Random rng;
    private MatchSettingsModel matchSettingsModel;

    private char[,] board;                 // [x, y]
    private WordSearchLetter[,] spawned;   // [x, y]

    private GridLayoutGroup gridLayout;
    private RectTransform gridRect;

    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  0),
        new Vector2Int( 0,  1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1,  1),
        new Vector2Int(-1, -1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1,  1),
    };

    private readonly List<string> normalizedWords = new List<string>(64);
    private readonly List<Placement> candidates = new List<Placement>(512);

    // Reusamos un solo StringBuilder para evitar GC
    private readonly StringBuilder sb = new StringBuilder(64);

    private struct Placement
    {
        public int x;
        public int y;
        public int dirIndex;
        public Placement(int x, int y, int dirIndex)
        {
            this.x = x;
            this.y = y;
            this.dirIndex = dirIndex;
        }
    }

    private void Awake()
    {
        CacheLayoutRefs();
        matchSettingsModel = FindAnyObjectByType<MatchSettingsModel>();
        matchSettingsModel.SelectCathegory(WsCategory.Professions);
    }

    private void CacheLayoutRefs()
    {
        if (gridParent == null) return;

        gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        gridRect = gridParent.GetComponent<RectTransform>();

        if (gridLayout == null)
            Debug.LogError("TableGenerator: gridParent no tiene GridLayoutGroup.");

        if (gridRect == null)
            Debug.LogError("TableGenerator: gridParent no tiene RectTransform (debe ser UI).");
    }

    [ContextMenu("GenerateWordSearch")]
    public void GenerateWordSearch()
    {
        if (gridParent == null)
        {
            Debug.LogError("TableGenerator: gridParent no está asignado.");
            return;
        }
        if (letterPrefab == null)
        {
            Debug.LogError("TableGenerator: letterPrefab no está asignado.");
            return;
        }

        CacheLayoutRefs();
        if (gridLayout == null || gridRect == null)
            return;

        rng = (randomSeed == 0) ? new System.Random() : new System.Random(randomSeed);

        ClearChildren(gridParent);

        if (autoFit)
            AutoFitGridLayout();

        board = new char[gridWidth, gridHeight];
        spawned = new WordSearchLetter[gridWidth, gridHeight];

        words = matchSettingsModel.Words.ToArray();
        matchSettingsModel.GetWordList();
        NormalizeWordsToUppercase(matchSettingsModel.Words.ToArray(), normalizedWords);
        SortByLengthDesc(normalizedWords);

        for (int i = 0; i < normalizedWords.Count; i++)
        {
            string w = normalizedWords[i];
            if (w.Length == 0) continue;

            BuildCandidates(w, candidates);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"SE DESCARTÓ LA PALABRA '{w}' (NO HAY ESPACIO COMPATIBLE).");
                continue;
            }

            Placement chosen = candidates[rng.Next(candidates.Count)];
            Vector2Int dir = Directions[chosen.dirIndex];
            PlaceWord(w, chosen.x, chosen.y, dir);
        }

        FillEmptyWithRandomLetters();
        SpawnGridUI();

        // fuerza recálculo layout (útil si generas en runtime)
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void NormalizeWordsToUppercase(string[] input, List<string> output)
    {
        output.Clear();
        if (input == null) return;

        for (int i = 0; i < input.Length; i++)
        {
            string raw = input[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;

            sb.Clear();

            // TRIM MANUAL (evita asignaciones de Trim)
            int start = 0;
            int end = raw.Length - 1;

            while (start <= end && char.IsWhiteSpace(raw[start])) start++;
            while (end >= start && char.IsWhiteSpace(raw[end])) end--;

            for (int c = start; c <= end; c++)
            {
                char ch = raw[c];

                // ignorar espacios internos
                if (ch == ' ') continue;

                // solo letras
                if (char.IsLetter(ch))
                {
                    sb.Append(char.ToUpperInvariant(ch));
                }
            }

            if (sb.Length > 0)
                output.Add(sb.ToString());
        }
    }

    private static void SortByLengthDesc(List<string> list)
    {
        list.Sort((a, b) => b.Length.CompareTo(a.Length));
    }

    private void AutoFitGridLayout()
    {
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;

        float rectW = gridRect.rect.width;
        float rectH = gridRect.rect.height;

        if (rectW <= 0f || rectH <= 0f)
        {
            Debug.LogWarning("AutoFitGridLayout: RectTransform aún no tiene tamaño válido.");
            return;
        }

        RectOffset pad = gridLayout.padding;
        float availableW = rectW - pad.left - pad.right;
        float availableH = rectH - pad.top - pad.bottom;

        if (availableW <= 0f || availableH <= 0f)
        {
            Debug.LogWarning("AutoFitGridLayout: No hay espacio disponible (padding consume todo).");
            return;
        }

        int cols = gridWidth;
        int rows = gridHeight;

        int gapsX = Mathf.Max(0, cols - 1);
        int gapsY = Mathf.Max(0, rows - 1);

        // Base (sin spacing)
        float baseCellW = availableW / cols;
        float baseCellH = availableH / rows;

        if (squareCells)
        {
            float baseCell = Mathf.Min(baseCellW, baseCellH);

            // Aplicar offset (resta) y clamp
            float cell = Mathf.Max(minCellSize, baseCell - Mathf.Max(cellSizeOffset.x, cellSizeOffset.y));

            // Calcular spacing exacto para ocupar todo
            float spacingX = 0f;
            float spacingY = 0f;

            if (gapsX > 0) spacingX = (availableW - (cols * cell)) / gapsX;
            if (gapsY > 0) spacingY = (availableH - (rows * cell)) / gapsY;

            if (spacingX < 0f) spacingX = 0f;
            if (spacingY < 0f) spacingY = 0f;

            gridLayout.cellSize = new Vector2(cell, cell);
            gridLayout.spacing = new Vector2(spacingX, spacingY);
            return;
        }
        else
        {
            // Aplicar offset por eje (resta) y clamp
            float cellW = Mathf.Max(minCellSize, baseCellW - cellSizeOffset.x);
            float cellH = Mathf.Max(minCellSize, baseCellH - cellSizeOffset.y);

            // Calcular spacing exacto para ocupar todo
            float spacingX = 0f;
            float spacingY = 0f;

            if (gapsX > 0) spacingX = (availableW - (cols * cellW)) / gapsX;
            if (gapsY > 0) spacingY = (availableH - (rows * cellH)) / gapsY;

            if (spacingX < 0f) spacingX = 0f;
            if (spacingY < 0f) spacingY = 0f;

            gridLayout.cellSize = new Vector2(cellW, cellH);
            gridLayout.spacing = new Vector2(spacingX, spacingY);
            return;
        }
    }

    private void BuildCandidates(string word, List<Placement> outCandidates)
    {
        outCandidates.Clear();
        int len = word.Length;

        for (int d = 0; d < Directions.Length; d++)
        {
            Vector2Int dir = Directions[d];
            int dx = dir.x;
            int dy = dir.y;

            int minX = 0, maxX = gridWidth - 1;
            int minY = 0, maxY = gridHeight - 1;

            if (dx == 1) maxX = gridWidth - len;
            else if (dx == -1) minX = len - 1;

            if (dy == 1) maxY = gridHeight - len;
            else if (dy == -1) minY = len - 1;

            if (minX > maxX || minY > maxY)
                continue;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (CanPlace(word, x, y, dx, dy))
                        outCandidates.Add(new Placement(x, y, d));
                }
            }
        }
    }

    private bool CanPlace(string word, int startX, int startY, int dx, int dy)
    {
        for (int i = 0; i < word.Length; i++)
        {
            int x = startX + dx * i;
            int y = startY + dy * i;

            char existing = board[x, y];
            char needed = word[i];

            if (existing != '\0' && existing != needed)
                return false;
        }
        return true;
    }

    private void PlaceWord(string word, int startX, int startY, Vector2Int dir)
    {
        int dx = dir.x;
        int dy = dir.y;

        for (int i = 0; i < word.Length; i++)
        {
            int x = startX + dx * i;
            int y = startY + dy * i;
            board[x, y] = word[i];
        }
    }

    private void FillEmptyWithRandomLetters()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (board[x, y] == '\0')
                    board[x, y] = RandomLetterAZ();
            }
        }
    }

    private char RandomLetterAZ()
    {
        int v = rng.Next(0, 26);
        return (char)('A' + v);
    }

    private void SpawnGridUI()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                WordSearchLetter instance = Instantiate(letterPrefab, gridParent);
                instance.SetLetter(board[x, y]);
                spawned[x, y] = instance;
            }
        }
    }
}
