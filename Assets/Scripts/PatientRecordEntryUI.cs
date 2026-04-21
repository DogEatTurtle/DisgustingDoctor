using TMPro;
using UnityEngine;

public class PatientRecordEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    private NPCActor npc;
    private PatientRecordsUI patientRecordsUI;
    private System.Action customClickAction;

    public void Setup(NPCActor targetNPC, PatientRecordsUI ui)
    { 

        npc = targetNPC;
        patientRecordsUI = ui;

        if (nameText != null)
            nameText.text = npc != null ? npc.npcName : "Unknown";
    }

    public void SetCustomClickAction(System.Action action)
    {
        customClickAction = action;
    }

    public void OnClicked()
    {
        if (customClickAction != null)
        {
            customClickAction.Invoke();
            return;
        }

        Debug.Log($"[Entry] Clicked npc={(npc != null ? npc.npcName : "NULL")}");
        if (npc == null || patientRecordsUI == null) return;
        patientRecordsUI.OpenPatientRecord(npc);
    }
}