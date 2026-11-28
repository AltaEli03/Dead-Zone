using UnityEngine;

namespace DeadZone.Level
{
    // Runs after Cinemachine moves the camera so the background doesn't jitter.
    [DefaultExecutionOrder(10000)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ParallaxBackground : MonoBehaviour
    {
        [Tooltip("0 = moves with the level, 1 = locked to the camera. 0.9 feels far away.")]
        [SerializeField, Range(0f, 1f)] float followX = 0.9f;
        [Tooltip("1 keeps the background steady when the player jumps or climbs.")]
        [SerializeField, Range(0f, 1f)] float followY = 1f;
        [Tooltip("Lines up the image's left edge with the camera's left edge when the level starts.")]
        [SerializeField] bool alignLeftEdgeOnStart = true;
        [Tooltip("Optional (use the level's CameraBounds). Overrides Follow X so the image spans the whole level, from its left edge to its right edge.")]
        [SerializeField] Collider2D fitToBounds;

        Transform cameraTransform;
        Vector3 cameraStart;
        Vector3 startPosition;

        void Start()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                enabled = false;
                return;
            }

            cameraTransform = cam.transform;
            cameraStart = cameraTransform.position;
            startPosition = transform.position;

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            float halfView = cam.orthographicSize * cam.aspect;

            if (fitToBounds != null)
            {
                Bounds level = fitToBounds.bounds;
                float travel = level.size.x - halfView * 2f;
                float spare = spriteRenderer.bounds.size.x - halfView * 2f;
                if (travel > 0f) followX = Mathf.Clamp01(1f - spare / travel);

                // Image's left edge meets the level's left edge when the camera is all the way left.
                float cameraMinX = level.min.x + halfView;
                float leftEdge = level.min.x - (cameraMinX - cameraStart.x) * followX;
                startPosition.x += leftEdge - spriteRenderer.bounds.min.x;
            }
            else if (alignLeftEdgeOnStart)
            {
                float cameraLeft = cameraStart.x - halfView;
                startPosition.x += cameraLeft - spriteRenderer.bounds.min.x;
            }
        }

        void LateUpdate()
        {
            Vector3 cameraMoved = cameraTransform.position - cameraStart;
            transform.position = new Vector3(
                startPosition.x + cameraMoved.x * followX,
                startPosition.y + cameraMoved.y * followY,
                startPosition.z);
        }
    }
}
