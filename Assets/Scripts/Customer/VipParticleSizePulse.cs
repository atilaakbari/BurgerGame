using UnityEngine;

public class VipParticleSizePulse : MonoBehaviour
{
    [SerializeField] private ParticleSystem targetParticles;

    [SerializeField] private float minSize = 1.5f;
    [SerializeField] private float maxSize = 3.5f;
    [SerializeField] private float speed = 2.5f;

    private float phase;
    private ParticleSystem.MainModule main;

    private void Awake()
    {
        if (targetParticles == null)
            targetParticles = GetComponent<ParticleSystem>();

        if (targetParticles != null)
            main = targetParticles.main;

        phase = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (targetParticles == null || !targetParticles.isPlaying)
            return;

        float t = (Mathf.Sin((Time.time + phase) * speed) + 1f) * 0.5f;
        float size = Mathf.Lerp(minSize, maxSize, t);

        // فقط Start Size — Alpha دست نمی‌خورد
        main.startSize = size;
    }
}