using System.Collections;
using UnityEngine;

public class RushHourFOV : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Header("FOV")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float rushFOV = 66f;
    [SerializeField] private float blendSeconds = 0.35f;

    private Coroutine blendRoutine;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            normalFOV = targetCamera.fieldOfView;
    }

    private void OnEnable()
    {
        if (RushHourManager.Instance == null)
            return;

        RushHourManager.Instance.OnRushStarted += OnRushStarted;
        RushHourManager.Instance.OnRushEnded += OnRushEnded;
    }

    private void OnDisable()
    {
        if (RushHourManager.Instance == null)
            return;

        RushHourManager.Instance.OnRushStarted -= OnRushStarted;
        RushHourManager.Instance.OnRushEnded -= OnRushEnded;
    }

    private void Start()
    {
        // اگر Manager دیر لود شد
        if (RushHourManager.Instance != null)
        {
            RushHourManager.Instance.OnRushStarted -= OnRushStarted;
            RushHourManager.Instance.OnRushEnded -= OnRushEnded;
            RushHourManager.Instance.OnRushStarted += OnRushStarted;
            RushHourManager.Instance.OnRushEnded += OnRushEnded;
        }
    }

    private void OnRushStarted()
    {
        BlendTo(rushFOV);
    }

    private void OnRushEnded()
    {
        BlendTo(normalFOV);
    }

    private void BlendTo(float target)
    {
        if (targetCamera == null)
            return;

        if (blendRoutine != null)
            StopCoroutine(blendRoutine);

        blendRoutine = StartCoroutine(BlendRoutine(target));
    }

    private IEnumerator BlendRoutine(float target)
    {
        float start = targetCamera.fieldOfView;
        float t = 0f;

        if (blendSeconds <= 0.01f)
        {
            targetCamera.fieldOfView = target;
            blendRoutine = null;
            yield break;
        }

        while (t < blendSeconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / blendSeconds);
            // smooth
            a = a * a * (3f - 2f * a);
            targetCamera.fieldOfView = Mathf.Lerp(start, target, a);
            yield return null;
        }

        targetCamera.fieldOfView = target;
        blendRoutine = null;
    }
}