using TMPro;
using UnityEngine;

public class PatientRecordEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    private NPCActor npc;
    private PatientRecordsUI patientRecordsUI;

    public void Setup(NPCActor targetNPC, PatientRecordsUI ui)
    {
        Debug.Log($"[Entry] Setup id={GetInstanceID()} go={gameObject.name} npc={(targetNPC != null ? targetNPC.npcName : "NULL")}");

        npc = targetNPC;
        patientRecordsUI = ui;

        if (nameText != null)
            nameText.text = npc != null ? npc.npcName : "Unknown";
    }

    public void OnClicked()
    {
        Debug.Log($"[Entry] Clicked id={GetInstanceID()} go={gameObject.name} npc={(npc != null ? npc.npcName : "NULL")}");
        if (npc == null || patientRecordsUI == null) return;
        patientRecordsUI.OpenPatientRecord(npc);
    }
}