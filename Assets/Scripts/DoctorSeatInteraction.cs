using UnityEngine;

public class DoctorSeatInteraction : MonoBehaviour
{
    [SerializeField] private PlayerSeatController playerSeatController;

    [Header("Seat Points")]
    [SerializeField] private Transform doctorSeatPoint;
    [SerializeField] private Transform doctorExitPoint;

    public void Interact()
    {
        if (playerSeatController == null)
            return;

        if (playerSeatController.IsSeated)
            return;

        if (doctorSeatPoint == null || doctorExitPoint == null)
            return;

        playerSeatController.SitDown(doctorSeatPoint, doctorExitPoint);
    }
}