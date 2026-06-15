using UnityEngine;

public class TargetArrow : MonoBehaviour
{
    public Transform player;

    [Tooltip("Assign this and the arrow will automatically track whatever objective is currently active.")]
    public DeliveryManager deliveryManager;

    [Tooltip("Manual override. If deliveryManager is assigned and has an active target, that takes priority.")]
    public Transform target;

    void Update()
    {
        if (deliveryManager != null)
        {
            target = deliveryManager.CurrentTarget != null
                ? deliveryManager.CurrentTarget.transform
                : null;
        }

        if (player == null || target == null)
        {
            return;
        }

        Vector2 direction = target.position - player.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
