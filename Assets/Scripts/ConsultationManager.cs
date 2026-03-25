using TMPro;
using UnityEngine;

public class ConsultationManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool consultationActive = false;
    [SerializeField] private NPCActor currentPatient;

    [Header("Patient")]
    [SerializeField] private Transform patientConsultPoint;

    [Header("Doctor Seating")]
    [SerializeField] private PlayerSeatController playerSeatController;
    [SerializeField] private Transform doctorSeatPoint;
    [SerializeField] private Transform doctorExitPoint;

    [Header("Timer")]
    [SerializeField] private float consultationDuration = 240f; // 4 minutos
    [SerializeField] private TMP_Text consultationTimerText;

    private float currentTime;

    public bool ConsultationActive => consultationActive;
    public NPCActor CurrentPatient => currentPatient;

    private void Start()
    {
        if (consultationTimerText != null)
        {
            consultationTimerText.text = "00:00";
            consultationTimerText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!consultationActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateTimerUI(currentTime);

            Debug.Log("Tempo da consulta terminou.");
            EndConsultation();
            return;
        }

        UpdateTimerUI(currentTime);
    }

    public void StartConsultation(NPCActor patient)
    {
        if (consultationActive || patient == null)
            return;

        consultationActive = true;
        currentPatient = patient;

        if (patientConsultPoint != null)
        {
            patient.transform.position = patientConsultPoint.position;
            patient.transform.rotation = patientConsultPoint.rotation;
        }

        if (playerSeatController != null && doctorSeatPoint != null && doctorExitPoint != null)
        {
            playerSeatController.SitDown(doctorSeatPoint, doctorExitPoint);
        }

        currentTime = consultationDuration;

        if (consultationTimerText != null)
        {
            consultationTimerText.gameObject.SetActive(true);
            UpdateTimerUI(currentTime);
        }

        Debug.Log($"Consulta iniciada com {patient.npcName}");
    }

    public void EndConsultation()
    {
        if (!consultationActive || currentPatient == null)
            return;

        currentPatient.ReturnToOriginalPosition();
        currentPatient.willVisitClinic = false;

        if (consultationTimerText != null)
            consultationTimerText.gameObject.SetActive(false);

        if (playerSeatController != null && playerSeatController.IsSeated)
        {
            playerSeatController.StandUp();
        }

        Debug.Log($"Consulta terminada com {currentPatient.npcName}");

        currentPatient = null;
        consultationActive = false;
    }

    private void UpdateTimerUI(float time)
    {
        if (consultationTimerText == null) return;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        consultationTimerText.text = $"{minutes:00}:{seconds:00}";
    }
}