using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSeatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ConversationManager conversationManager;

    [Header("State")]
    [SerializeField] private bool isSeated = false;

    private Transform currentSeatPoint;
    private Transform exitPoint;

    public bool IsSeated => isSeated;

    private void Update()
    {
        if (!isSeated) return;

        // NÃO permitir levantar durante conversa
        if (conversationManager != null && conversationManager.IsOpen)
            return;

        if (PlayerWantsToMove())
        {
            StandUp();
        }
    }

    public void SitDown(Transform seatPoint, Transform standPoint)
    {
        if (seatPoint == null || standPoint == null) return;

        currentSeatPoint = seatPoint;
        exitPoint = standPoint;
        isSeated = true;

        TeleportTo(seatPoint);
    }

    public void StandUp()
    {
        if (!isSeated) return;

        isSeated = false;

        if (exitPoint != null)
            TeleportTo(exitPoint);

        currentSeatPoint = null;
        exitPoint = null;
    }

    private void TeleportTo(Transform target)
    {
        if (target == null) return;

        if (characterController != null)
            characterController.enabled = false;

        transform.position = target.position;
        transform.rotation = target.rotation;

        if (characterController != null)
            characterController.enabled = true;
    }

    private bool PlayerWantsToMove()
    {
        if (Keyboard.current == null) return false;

        return Keyboard.current.wKey.wasPressedThisFrame ||
               Keyboard.current.aKey.wasPressedThisFrame ||
               Keyboard.current.sKey.wasPressedThisFrame ||
               Keyboard.current.dKey.wasPressedThisFrame;
    }
}