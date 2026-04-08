using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatientRecordsUI : MonoBehaviour
{
    [Header("NPC Source")]
    [SerializeField] private List<NPCActor> npcs = new();

    [Header("List UI")]
    [SerializeField] private Transform listContainer;
    [SerializeField] private PatientRecordEntryUI entryPrefab;

    [Header("Panels")]
    [SerializeField] private GameObject patientListPanel;
    [SerializeField] private GameObject patientRecordPanel;

    [Header("Record Fields")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text ageText;
    [SerializeField] private TMP_Text professionText;
    [SerializeField] private TMP_Text personalityText;
    [SerializeField] private TMP_Text socialTraitText;
    [SerializeField] private TMP_Text skillTraitText;
    [SerializeField] private TMP_Text lastDiseaseText;
    [SerializeField] private TMP_Text clinicVisitChanceText;

    [Header("Optional")]
    [SerializeField] private DailySystem dailySystem;
    [SerializeField] private Button backButton;

    private void Start()
    {
        BuildPatientList();

        if (patientListPanel != null)
            patientListPanel.SetActive(false);

        if (patientRecordPanel != null)
            patientRecordPanel.SetActive(false);

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ShowPatientList);
        }
    }

    public void BuildPatientList()
    {
        if (listContainer == null || entryPrefab == null) return;

        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var npc in npcs)
        {
            if (npc == null) continue;

            PatientRecordEntryUI entry = Instantiate(entryPrefab, listContainer);
            entry.Setup(npc, this);
        }
    }

    public void OpenListView()
    {
        if (patientRecordPanel != null)
            patientRecordPanel.SetActive(false);

        if (patientListPanel != null)
            patientListPanel.SetActive(true);
    }

    public void OpenPatientRecord(NPCActor npc)
    {
        Debug.Log($"[PRUI] OpenPatientRecord npc={(npc != null ? npc.npcName : "NULL")} listPanel={patientListPanel} recordPanel={patientRecordPanel}");
        if (npc == null) return;

        if (patientListPanel != null)
            patientListPanel.SetActive(false);

        if (patientRecordPanel != null)
            patientRecordPanel.SetActive(true);

        PatientRecordData record = npc.patientRecord;

        SetText(nameText, string.IsNullOrEmpty(record.patientName) ? npc.npcName : record.patientName);
        SetText(ageText, record.ageUnlocked ? record.age.ToString() : "???");
        SetText(professionText, record.professionUnlocked ? record.professionName : "???");
        SetText(personalityText, record.personalityUnlocked ? record.personalityName : "???");
        SetText(socialTraitText, record.socialTraitUnlocked ? record.socialTraitName : "???");
        SetText(skillTraitText, record.skillTraitUnlocked ? record.skillTraitName : "???");
        SetText(lastDiseaseText, record.lastDiseaseUnlocked ? record.lastDiseaseName : "???");

        bool canShowVisitChance =
            record.personalityUnlocked &&
            record.socialTraitUnlocked &&
            record.skillTraitUnlocked;

        if (canShowVisitChance && dailySystem != null)
            SetText(clinicVisitChanceText, dailySystem.GetFormattedClinicVisitChance(npc));
        else
            SetText(clinicVisitChanceText, "???");
    }

    public void ShowPatientList()
    {
        OpenListView();
    }

    private void SetText(TMP_Text textField, string value)
    {
        if (textField != null)
            textField.text = value;
    }
}