using UnityEngine;

public class TransitionUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject transitionPanel;

    [SerializeField] private GameObject btnConsultorio;
    [SerializeField] private GameObject btnLaboratorio;
    [SerializeField] private GameObject btnMercadoNegro;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private CharacterController characterController;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnConsultorio;
    [SerializeField] private Transform spawnLaboratorio;
    [SerializeField] private Transform spawnMercadoNegro;

    private LocationType currentLocation;

    void Start()
    {
        CloseTransitionUI();
    }

    public void OpenTransitionUI(LocationType location)
    {
        currentLocation = location;

        transitionPanel.SetActive(true);

        // esconder botão do local atual
        if (btnConsultorio != null)
            btnConsultorio.SetActive(location != LocationType.Consultorio);

        if (btnLaboratorio != null)
            btnLaboratorio.SetActive(location != LocationType.Laboratorio);

        if (btnMercadoNegro != null)
            btnMercadoNegro.SetActive(location != LocationType.MercadoNegro);

        // bloquear controlo do jogador
        if (fpsController != null)
            fpsController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseTransitionUI()
    {
        transitionPanel.SetActive(false);

        if (fpsController != null)
            fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToConsultorio()
    {
        TeleportTo(spawnConsultorio);
    }

    public void GoToLaboratorio()
    {
        TeleportTo(spawnLaboratorio);
    }

    public void GoToMercadoNegro()
    {
        TeleportTo(spawnMercadoNegro);
    }

    private void TeleportTo(Transform targetSpawn)
    {
        if (targetSpawn == null) return;

        if (characterController != null)
            characterController.enabled = false;

        player.position = targetSpawn.position;
        player.rotation = targetSpawn.rotation;

        if (characterController != null)
            characterController.enabled = true;

        CloseTransitionUI();
    }
}