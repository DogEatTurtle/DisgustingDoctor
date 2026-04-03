using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Entry : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;

    public void Setup(string text)
    {
        textField.SetText(text);
    }
}
