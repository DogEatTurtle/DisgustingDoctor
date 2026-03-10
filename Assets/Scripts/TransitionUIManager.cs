using UnityEngine;

public class TransitionUIManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject transitionPanel;

    [Header("Player")]
    public Transform player;

    [Header("Spawners")]
    public Transform spawnConsultorio;
    public Transform spawnLaboratorio;
    public Transform spawnMercadoNegro;

    private LocationType currentLocation;
    private bool isUIOpen = false;

    void Start()
    {
        CloseTransitionUI();
    }

    public void OpenTransitionUI(LocationType location)
    {
        currentLocation = location;
        transitionPanel.SetActive(true);
        isUIOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void CloseTransitionUI()
    {
        transitionPanel.SetActive(false);
        isUIOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    public void GoToConsultorio()
    {
        TeleportTo(LocationType.Consultorio);
    }

    public void GoToLaboratorio()
    {
        TeleportTo(LocationType.Laboratorio);
    }

    public void GoToMercadoNegro()
    {
        TeleportTo(LocationType.MercadoNegro);
    }

    private void TeleportTo(LocationType targetLocation)
    {
        Transform targetSpawn = null;

        switch (targetLocation)
        {
            case LocationType.Consultorio:
                targetSpawn = spawnConsultorio;
                break;
            case LocationType.Laboratorio:
                targetSpawn = spawnLaboratorio;
                break;
            case LocationType.MercadoNegro:
                targetSpawn = spawnMercadoNegro;
                break;
        }

        if (targetSpawn != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();

            if (cc != null)
            {
                cc.enabled = false;
                player.position = targetSpawn.position;
                player.rotation = targetSpawn.rotation;
                cc.enabled = true;
            }
            else
            {
                player.position = targetSpawn.position;
                player.rotation = targetSpawn.rotation;
            }
        }

        CloseTransitionUI();
    }
}