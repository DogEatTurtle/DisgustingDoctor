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

    private Vector3 patientPreviousPosition;
    private Quaternion patientPreviousRotation;

    public bool ConsultationActive => consultationActive;
    public NPCActor CurrentPatient => currentPatient;

    public void StartConsultation(NPCActor patient)
    {
        if (consultationActive || patient == null)
            return;

        consultationActive = true;
        currentPatient = patient;

        patientPreviousPosition = patient.transform.position;
        patientPreviousRotation = patient.transform.rotation;

        if (patientConsultPoint != null)
        {
            patient.transform.position = patientConsultPoint.position;
            patient.transform.rotation = patientConsultPoint.rotation;
        }

        if (playerSeatController != null && doctorSeatPoint != null && doctorExitPoint != null)
        {
            playerSeatController.SitDown(doctorSeatPoint, doctorExitPoint);
        }

        Debug.Log($"Consulta iniciada com {patient.npcName}");
    }

    public void EndConsultation()
    {
        if (!consultationActive || currentPatient == null)
            return;

        currentPatient.transform.position = patientPreviousPosition;
        currentPatient.transform.rotation = patientPreviousRotation;

        Debug.Log($"Consulta terminada com {currentPatient.npcName}");

        currentPatient = null;
        consultationActive = false;
    }
}