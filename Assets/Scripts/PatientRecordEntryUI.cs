using TMPro;
using UnityEngine;

public class PatientRecordEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    private NPCActor npc;
    private PatientRecordsUI patientRecordsUI;

    public void Setup(NPCActor targetNPC, PatientRecordsUI ui)
    { 

        npc = targetNPC;
        patientRecordsUI = ui;

        if (nameText != null)
            nameText.text = npc != null ? npc.npcName : "Unknown";
    }

    public void OnClicked()
    {
        
        if (npc == null || patientRecordsUI == null) return;
        patientRecordsUI.OpenPatientRecord(npc);
    }
}