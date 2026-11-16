using UnityEngine;

[ExecuteAlways]
public class AlignToGrid : MonoBehaviour
{
    [SerializeField] private Vector3 cellSize = Vector3.one;
    [SerializeField] private bool alignX = true;
    [SerializeField] private bool alignY = true;
    [SerializeField] private bool alignZ = true;
    [SerializeField] private bool keepAlignedWhileMoving = false;

    private Vector3 lastPosition;

    private void OnEnable()
    {
        Align();
        lastPosition = transform.position;
    }

    private void Update()
    {
        // In edit mode we always track movement so dragging a prefab into
        // the Scene view (which moves the object every frame without
        // re-triggering OnEnable) still ends up snapped to the grid.
        bool shouldTrack = !Application.isPlaying || keepAlignedWhileMoving;

        if (shouldTrack && transform.position != lastPosition)
        {
            Align();
            lastPosition = transform.position;
        }
    }

    private void Align()
    {
        Vector3 position = transform.position;

        if (alignX && cellSize.x > 0f)
            position.x = Mathf.Round(position.x / cellSize.x) * cellSize.x;
        if (alignY && cellSize.y > 0f)
            position.y = Mathf.Round(position.y / cellSize.y) * cellSize.y;
        if (alignZ && cellSize.z > 0f)
            position.z = Mathf.Round(position.z / cellSize.z) * cellSize.z;

        transform.position = position;
    }
}
