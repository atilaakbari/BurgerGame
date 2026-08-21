using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
    [SerializeField] private float smoothSpeed = 5f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            1f / Mathf.Max(smoothSpeed, 0.01f)
        );
    }
}
