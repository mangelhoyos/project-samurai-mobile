using TMPro;
using UnityEngine;

public class WordSearchLetter : MonoBehaviour
{
    [SerializeField] private TMP_Text letterText;
    
    public void SetLetter(char letter)
    {
        letterText.text = char.ToString(letter);
    }
}
