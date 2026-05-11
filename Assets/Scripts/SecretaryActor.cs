using UnityEngine;

public class SecretaryActor : MonoBehaviour
{
    public enum SecretaryState
    {
        Active,
        FarewellDay,           // visible for one final day, last chance to talk to her
        AbandonedAndLeftLetter, // gone, letter on the desk
        AbandonedSilently       // gone, no letter (player said goodbye in person)
    }

    [Header("Identity")]
    public string secretaryName = "Secretary";

    [Header("Personality (assigned in editor or randomized at start)")]
    public PersonalitySO basePersonality;
    public PersonalityTraitSO socialTrait;

    [Header("State")]
    [SerializeField] private SecretaryState state = SecretaryState.Active;

    [Header("Optional Visual")]
    [Tooltip("If assigned, this GameObject is hidden when the secretary leaves.")]
    [SerializeField] private GameObject secretaryVisual;

    [Header("Farewell Letter")]
    [Tooltip("The letter object on the desk. Activated when the secretary leaves with a letter.")]
    [SerializeField] private GameObject farewellLetterObject;

    public SecretaryState State => state;
    public bool IsActive => state == SecretaryState.Active;
    public bool IsOnFarewellDay => state == SecretaryState.FarewellDay;
    public bool HasLeft => state == SecretaryState.AbandonedAndLeftLetter || state == SecretaryState.AbandonedSilently;

    // Tracks whether the player talked to her on the farewell day
    private bool playerSaidGoodbyeInPerson;

    private void Start()
    {
        // Make sure the letter starts hidden
        if (farewellLetterObject != null)
            farewellLetterObject.SetActive(false);
    }

    public void EnterFarewellDay()
    {
        if (state != SecretaryState.Active) return;
        state = SecretaryState.FarewellDay;
        playerSaidGoodbyeInPerson = false;
        Debug.Log("[Secretary] Has entered her farewell day. She will leave tomorrow.");
    }

    public void RegisterPlayerVisitedDuringFarewell()
    {
        if (state == SecretaryState.FarewellDay)
            playerSaidGoodbyeInPerson = true;
    }

    public void Leave()
    {
        if (state != SecretaryState.FarewellDay)
        {
            Debug.LogWarning("[Secretary] Leave() called from unexpected state: " + state);
        }

        if (playerSaidGoodbyeInPerson)
        {
            state = SecretaryState.AbandonedSilently;
            Debug.Log("[Secretary] Left silently (player said goodbye in person yesterday).");
        }
        else
        {
            state = SecretaryState.AbandonedAndLeftLetter;
            if (farewellLetterObject != null)
                farewellLetterObject.SetActive(true);
            Debug.Log("[Secretary] Left, leaving a farewell letter on the desk.");
        }

        if (secretaryVisual != null)
            secretaryVisual.SetActive(false);
    }
}