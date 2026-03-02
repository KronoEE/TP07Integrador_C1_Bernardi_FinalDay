using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseGame : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] Button pauseBtn;
    [SerializeField] Button resumeBtn;
    [SerializeField] Button settingsBtn;
    [SerializeField] Button homeBtn;
    [SerializeField] Button settingsBackBtn;

    public bool isPaused = false;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        pauseBtn.onClick.AddListener(OnPauseClicked);
        resumeBtn.onClick.AddListener(ResumeGame);
        settingsBtn.onClick.AddListener(OnSettingsClicked);
        homeBtn.onClick.AddListener(OnHomeClicked);
        settingsBackBtn.onClick.AddListener(OnSettingsBack);

        AddHoverSound(pauseBtn);
        AddHoverSound(resumeBtn);
        AddHoverSound(settingsBtn);
        AddHoverSound(homeBtn);
        AddHoverSound(settingsBackBtn);
    }
    private void OnDestroy()
    {
        pauseBtn.onClick.RemoveAllListeners();
        resumeBtn.onClick.RemoveAllListeners();
        settingsBtn.onClick.RemoveAllListeners();
        homeBtn.onClick.RemoveAllListeners();
        settingsBackBtn.onClick.RemoveAllListeners();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                OnPauseClicked();
            }
        }
    }
    private void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }
    private void OnPauseClicked()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
    }
    private void OnSettingsClicked()
    {
        settingsMenu.SetActive(true);
    }
    private void OnHomeClicked()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
    }
    private void OnSettingsBack()
    {
        settingsMenu.SetActive(false);
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