using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Deathpanel : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsButtonbackBtn;
    [SerializeField] private Button restartBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button homeBtn;
    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        restartBtn.onClick.AddListener(OnRestartClick);
        settingsBtn.onClick.AddListener(OnSettingsClicked);
        homeBtn.onClick.AddListener(OnHomeClicked);
        settingsButtonbackBtn.onClick.AddListener(OnSettingsBack);

        // HOVER EVENTS
        AddHoverSound(restartBtn);
        AddHoverSound(settingsBtn);
        AddHoverSound(homeBtn);
        AddHoverSound(settingsButtonbackBtn);
    }

    private void OnDestroy()
    {
        restartBtn.onClick.RemoveAllListeners();
        settingsBtn.onClick.RemoveAllListeners();
        homeBtn.onClick.RemoveAllListeners();
        settingsButtonbackBtn.onClick.RemoveAllListeners();
    }

    private void OnRestartClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

    private void OnSettingsClicked()
    {
        settingsPanel.SetActive(true);
    }

    private void OnHomeClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnSettingsBack()
    {
        settingsPanel.SetActive(false);
    }
    private void AddHoverSound(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;

        entry.callback.AddListener
            (
                (eventData) => { audioManager.PlayUI(audioManager.HoverUi); }
            );

        trigger.triggers.Add(entry);
    }
}