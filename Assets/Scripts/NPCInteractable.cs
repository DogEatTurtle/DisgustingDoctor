using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private NPCActor npcActor;
    [SerializeField] private ConsultationManager consultationManager;

    [Header("Conversation")]
    [SerializeField] private ConversationManager conversationManager;

    private void Awake()
    {
        if (npcActor == null)
            npcActor = GetComponent<NPCActor>();
    }

    public void Interact()
    {
        if (npcActor == null || consultationManager == null)
            return;

        // Se não há consulta ativa, este NPC pode iniciar consulta
        if (!consultationManager.ConsultationActive)
        {
            if (!npcActor.willVisitClinic)
            {
                Debug.Log($"{npcActor.npcName} não está no consultório hoje.");
                return;
            }

            consultationManager.StartConsultation(npcActor);
            return;
        }

        // Se há consulta ativa, só o paciente atual pode abrir conversa
        if (consultationManager.CurrentPatient == npcActor)
        {

            if (conversationManager != null)
            {
                conversationManager.OpenConversation(npcActor);
            }
        }
        else
        {
            Debug.Log("Já existe uma consulta ativa com outro paciente.");
        }
    }
}