using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiseaseButtonSelectionUI : MonoBehaviour
{
    [SerializeField] private List<Button> diseaseButtons = new();

    private void Awake()
    {
        // Garantir que tudo começa sem outline
        ClearAll();
    }

    public void Select(Button clicked)
    {
        ClearAll();

        if (clicked == null) return;

        var outline = clicked.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = true;
    }

    public void ClearAll()
    {
        foreach (var b in diseaseButtons)
        {
            if (b == null) continue;

            var outline = b.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;
        }
    }
}

