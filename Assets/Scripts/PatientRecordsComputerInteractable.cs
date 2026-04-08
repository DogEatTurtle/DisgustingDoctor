using UnityEngine;

public class PatientRecordsComputerInteractable : MonoBehaviour
{
    [SerializeField] private PatientRecordsComputerUI patientRecordsComputerUI;

    public void Interact()
    {
        if (patientRecordsComputerUI == null)
            return;

        patientRecordsComputerUI.OpenUI();
    }
}