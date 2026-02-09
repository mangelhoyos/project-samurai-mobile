using TMPro;
using UnityEngine;

public class SetRandomKey : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text textField;

    [Header("Letters")]
    [Tooltip("Conjunto de caracteres válidos para la sopa de letras.")]
    [SerializeField] private string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private void Awake()
    {
        if (textField == null)
            textField = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        AssignRandomLetter();
    }

    private void AssignRandomLetter()
    {
        if (textField == null || string.IsNullOrEmpty(letters))
            return;

        int index = Random.Range(0, letters.Length);
        char randomChar = letters[index];

        textField.text = randomChar.ToString();
    }
}
