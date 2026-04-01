using UnityEngine;

public class DiagnosisInteractable : MonoBehaviour
{
    [SerializeField] private DiagnosisUI diagnosisUI;
    [SerializeField] private ConsultationManager consultationManager;

    public void Interact()
    {
        if (diagnosisUI == null || consultationManager == null)
            return;

        if (!consultationManager.ConsultationActive)
        {
            Debug.Log("Não há consulta ativa.");
            return;
        }

        diagnosisUI.OpenDiagnosisUI();
    }
}