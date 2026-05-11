using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GuideController : MonoBehaviour
{
    [Serializable]
    public class GuideTopic
    {
        public string topicName;

        [TextArea(6, 20)]
        public string topicBody;
    }

    [Header("Panels")]
    [SerializeField] private GameObject topicsMenuPanel;
    [SerializeField] private GameObject topicViewPanel;

    [Header("Topics Menu (one button per topic)")]
    [SerializeField] private Button topicButtonDiagnosis;
    [SerializeField] private Button topicButtonBlackMarket;
    [SerializeField] private Button topicButtonLab;
    [SerializeField] private Button topicButtonExternalVirus;
    [SerializeField] private Button topicButtonSecretary;
    [SerializeField] private Button topicButtonEndings;
    [SerializeField] private Button backButton;

    [Header("Topic View")]
    [SerializeField] private TMP_Text topicTitleText;
    [SerializeField] private TMP_Text topicBodyText;
    [SerializeField] private Button closeButton;

    [Header("Topics Content")]
    [SerializeField] private GuideTopic topicDiagnosis;
    [SerializeField] private GuideTopic topicBlackMarket;
    [SerializeField] private GuideTopic topicLab;
    [SerializeField] private GuideTopic topicExternalVirus;
    [SerializeField] private GuideTopic topicSecretary;
    [SerializeField] private GuideTopic topicEndings;

    [Header("Events")]
    [Tooltip("Called when the player clicks the back button inside the guide. Use this to return to the main menu, the pause menu, or wherever the guide was opened from.")]
    public UnityEvent onBackPressed;

    private void Start()
    {
        WireButtons();

        if (topicsMenuPanel != null) topicsMenuPanel.SetActive(false);
        if (topicViewPanel != null) topicViewPanel.SetActive(false);
    }

    private void WireButtons()
    {
        if (topicButtonDiagnosis != null)
        {
            topicButtonDiagnosis.onClick.RemoveAllListeners();
            topicButtonDiagnosis.onClick.AddListener(() => OpenTopic(topicDiagnosis));
        }
        if (topicButtonBlackMarket != null)
        {
            topicButtonBlackMarket.onClick.RemoveAllListeners();
            topicButtonBlackMarket.onClick.AddListener(() => OpenTopic(topicBlackMarket));
        }
        if (topicButtonLab != null)
        {
            topicButtonLab.onClick.RemoveAllListeners();
            topicButtonLab.onClick.AddListener(() => OpenTopic(topicLab));
        }
        if (topicButtonExternalVirus != null)
        {
            topicButtonExternalVirus.onClick.RemoveAllListeners();
            topicButtonExternalVirus.onClick.AddListener(() => OpenTopic(topicExternalVirus));
        }
        if (topicButtonSecretary != null)
        {
            topicButtonSecretary.onClick.RemoveAllListeners();
            topicButtonSecretary.onClick.AddListener(() => OpenTopic(topicSecretary));
        }
        if (topicButtonEndings != null)
        {
            topicButtonEndings.onClick.RemoveAllListeners();
            topicButtonEndings.onClick.AddListener(() => OpenTopic(topicEndings));
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseTopicClicked);
        }
    }

    public void ShowTopicsMenu()
    {
        if (topicsMenuPanel != null) topicsMenuPanel.SetActive(true);
        if (topicViewPanel != null) topicViewPanel.SetActive(false);
    }

    private void OpenTopic(GuideTopic topic)
    {
        if (topic == null)
        {
            Debug.LogWarning("[Guide] Topic is null.");
            return;
        }

        if (topicsMenuPanel != null) topicsMenuPanel.SetActive(false);
        if (topicViewPanel != null) topicViewPanel.SetActive(true);

        if (topicTitleText != null) topicTitleText.text = topic.topicName;
        if (topicBodyText != null) topicBodyText.text = topic.topicBody;
    }

    private void OnCloseTopicClicked()
    {
        ShowTopicsMenu();
    }

    private void OnBackClicked()
    {
        onBackPressed?.Invoke();
    }
}