using UnityEngine;

public class LabBenchInteractable : MonoBehaviour
{
    [SerializeField] private VirusLabUI labUI;

    public void Interact()
    {
        if (labUI != null)
            labUI.OpenLab();
    }
}