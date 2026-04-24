using UnityEngine;

public class CamerClamp : MonoBehaviour
{
    public Transform target; // player
    public BoxCollider2D bounds;

    private float halfHeight;
    private float halfWidth;

    void Start()
    {
        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;
    }

    void LateUpdate()
    {
        if (target == null || bounds == null) return;

        Bounds b = bounds.bounds;

        float clampX = Mathf.Clamp(target.position.x, b.min.x + halfWidth, b.max.x - halfWidth);
        float clampY = Mathf.Clamp(target.position.y, b.min.y + halfHeight, b.max.y - halfHeight);

        transform.position = new Vector3(clampX, clampY, transform.position.z);
    }
}