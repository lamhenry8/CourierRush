using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public DeliveryManager deliveryManager;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip collisionSound;

    float acceleration = 5f;
    float deceleration = 5f;
    float maxSpeed = 6f;
    float turnSpeed = 200f;

    Rigidbody2D rb;
    Vector2 moveInput;
    float currentSpeed = 0f;

    Vector2 spawnPosition;
    float spawnRotation;

    int oilSpillCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = rb.position;
        spawnRotation = rb.rotation;

        if (deliveryManager != null)
            deliveryManager.OnRestart += HandleRestart;
    }

    void OnDestroy()
    {
        if (deliveryManager != null)
            deliveryManager.OnRestart -= HandleRestart;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<SolidObstacle>() != null)
            if (audioSource != null && collisionSound != null)
                audioSource.PlayOneShot(collisionSound);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<OilSpill>() != null)
            oilSpillCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<OilSpill>() != null)
            oilSpillCount = Mathf.Max(0, oilSpillCount - 1);
    }

    void FixedUpdate()
    {
        bool onOil = oilSpillCount > 0;

        float effectiveAcceleration = onOil ? acceleration * 0.4f : acceleration;
        float effectiveDeceleration = onOil ? deceleration * 0.2f : deceleration;
        float effectiveMaxSpeed     = onOil ? maxSpeed * 0.7f    : maxSpeed;
        float effectiveTurnSpeed    = onOil ? turnSpeed * 0.25f  : turnSpeed;

        float moveAmount = moveInput.y;
        if (moveAmount != 0)
        {
            currentSpeed += moveAmount * effectiveAcceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -effectiveMaxSpeed, effectiveMaxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, effectiveDeceleration * Time.fixedDeltaTime);
        }

        float turnAmount = -moveInput.x * effectiveTurnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + turnAmount);

        rb.linearVelocity = transform.up * currentSpeed;
    }

    void HandleRestart()
    {
        currentSpeed = 0f;
        moveInput = Vector2.zero;
        oilSpillCount = 0;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.MovePosition(spawnPosition);
        rb.MoveRotation(spawnRotation);
    }
}
