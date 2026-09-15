using UnityEngine;

// این رو روی هر آبجکتی (مثلاً یه Canvas تو World Space) بذار که بخوای
// همیشه رو به دوربین بچرخه (Billboard).
//
// [DefaultExecutionOrder(32000)] باعث می‌شه این اسکریپت همیشه آخرین چیزی باشه
// که تو کل صحنه (تو اون فریم) اجرا می‌شه - یعنی هیچ اسکریپت دیگه‌ای
// (حتی اگه بعد از این چرخش این آبجکت رو عوض کنه) نمی‌تونه روش غالب بشه.
[DefaultExecutionOrder(32000)]
public class FaceCamera : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam != null)
            transform.forward = cam.transform.forward;
    }
}