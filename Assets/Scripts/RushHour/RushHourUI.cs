using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RushHourUI : MonoBehaviour
{
    [Header("Root همیشه روشن؛ Visual خاموش/روشن")]
    [SerializeField] private GameObject visualRoot;

    [SerializeField] private TextMeshProUGUI timerText; // فقط عدد، لیبل در یونیتی
    [SerializeField] private Image timerFill;

    [Header("اختیاری — حس هیجان")]
    [SerializeField] private AudioSource rushLoopSfx;
    [SerializeField] private GameObject rushParticles;

    private void Awake()
    {
        if (visualRoot != null)
            visualRoot.SetActive(false);

        SetExtras(false);
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void Start()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (RushHourManager.Instance == null)
            return;

        RushHourManager.Instance.OnRushStarted -= OnStarted;
        RushHourManager.Instance.OnRushEnded -= OnEnded;
        RushHourManager.Instance.OnRushUpdated -= Refresh;

        RushHourManager.Instance.OnRushStarted += OnStarted;
        RushHourManager.Instance.OnRushEnded += OnEnded;
        RushHourManager.Instance.OnRushUpdated += Refresh;
    }

    private void Unsubscribe()
    {
        if (RushHourManager.Instance == null)
            return;

        RushHourManager.Instance.OnRushStarted -= OnStarted;
        RushHourManager.Instance.OnRushEnded -= OnEnded;
        RushHourManager.Instance.OnRushUpdated -= Refresh;
    }

    private void OnStarted()
    {
        if (visualRoot != null)
            visualRoot.SetActive(true);

        SetExtras(true);
        Refresh();
    }

    private void OnEnded()
    {
        if (visualRoot != null)
            visualRoot.SetActive(false);

        SetExtras(false);
    }

    private void Refresh()
    {
        if (RushHourManager.Instance == null)
            return;

        if (!RushHourManager.Instance.IsRushActive)
            return;

        float t = RushHourManager.Instance.RushTimeLeft;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(t).ToString();

        if (timerFill != null)
            timerFill.fillAmount = RushHourManager.Instance.RushTimeNormalized;
    }

    private void SetExtras(bool on)
    {
        if (rushParticles != null)
            rushParticles.SetActive(on);

        if (rushLoopSfx != null)
        {
            if (on)
            {
                if (!rushLoopSfx.isPlaying)
                    rushLoopSfx.Play();
            }
            else
            {
                rushLoopSfx.Stop();
            }
        }
    }
}