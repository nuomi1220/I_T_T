using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("要跟随的目标")]
    public Transform target;

    public Vector3 offset = new Vector3(0f, 6f, -8f);
    public float smoothTime = 0.12f;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}