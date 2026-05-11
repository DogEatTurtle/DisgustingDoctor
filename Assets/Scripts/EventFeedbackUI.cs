using System.Collections;
using TMPro;
using UnityEngine;

public class EventFeedbackUI : MonoBehaviour
{
    public enum FeedbackType
    {
        PlayerVirusExtinct,
        ExternalVirusCured,
        ExternalVirusExtinctNaturally
    }

    [Header("Feedback Texts (3 separate GameObjects)")]
    [SerializeField] private GameObject playerVirusExtinctText;
    [SerializeField] private GameObject externalVirusCuredText;
    [SerializeField] private GameObject externalVirusExtinctNaturallyText;

    [Header("Settings")]
    [SerializeField, Min(0.1f)] private float displaySeconds = 3f;

    private Coroutine activeRoutine;

    private void Start()
    {
        // Make sure everything starts hidden
        if (playerVirusExtinctText != null) playerVirusExtinctText.SetActive(false);
        if (externalVirusCuredText != null) externalVirusCuredText.SetActive(false);
        if (externalVirusExtinctNaturallyText != null) externalVirusExtinctNaturallyText.SetActive(false);
    }

    public void Show(FeedbackType type)
    {
        // Hide all and stop any current routine so a new event takes priority
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        HideAll();

        GameObject target = GetTarget(type);
        if (target == null)
        {
            Debug.LogWarning($"[EventFeedback] No GameObject assigned for {type}.");
            return;
        }

        target.SetActive(true);
        activeRoutine = StartCoroutine(HideAfterSeconds(target, displaySeconds));
    }

    private GameObject GetTarget(FeedbackType type)
    {
        switch (type)
        {
            case FeedbackType.PlayerVirusExtinct: return playerVirusExtinctText;
            case FeedbackType.ExternalVirusCured: return externalVirusCuredText;
            case FeedbackType.ExternalVirusExtinctNaturally: return externalVirusExtinctNaturallyText;
        }
        return null;
    }

    private void HideAll()
    {
        if (playerVirusExtinctText != null) playerVirusExtinctText.SetActive(false);
        if (externalVirusCuredText != null) externalVirusCuredText.SetActive(false);
        if (externalVirusExtinctNaturallyText != null) externalVirusExtinctNaturallyText.SetActive(false);
    }

    private IEnumerator HideAfterSeconds(GameObject go, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (go != null) go.SetActive(false);
        activeRoutine = null;
    }
}