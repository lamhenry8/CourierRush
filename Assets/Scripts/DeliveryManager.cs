using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public enum ObjectiveState
    {
        Pickup,
        Delivery
    }

    [Header("References")]
    public TargetSpawner targetSpawner;
    public Transform player;
    public AudioSource audioSource;

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip deliverySound;

    [Header("Settings")]
    [Tooltip("How close the player must be to collect/deliver the current objective.")]
    public float collectDistance = 1f;

    [Tooltip("How often (in seconds) to check whether the current target's chunk is still active.")]
    public float chunkCheckInterval = 1f;

    [Tooltip("Seconds the player has to deliver a package before it expires. Set to 0 to disable.")]
    public float deliveryTimeLimit = 30f;

    public ObjectiveState CurrentState { get; private set; }
    public GameObject CurrentTarget { get; private set; }

    public event System.Action<GameObject> OnObjectiveChanged;
    public event System.Action<float> OnDeliveryCompleted;
    public event System.Action<int> OnGameOver;
    public event System.Action OnRestart;

    public bool IsGameOver { get; private set; }
    public float DeliveryTimerSeconds { get; private set; }
    public float TimeRemaining => TimerRunning ? deliveryTimeLimit - DeliveryTimerSeconds : -1f;
    public bool TimerRunning { get; private set; }
    public int DeliveryScore { get; private set; }

    private float chunkCheckTimer;

    void Start()
    {
        CurrentState = ObjectiveState.Pickup;
        Invoke(nameof(SpawnCurrentObjective), 0.1f);
    }

    void Update()
    {
        if (IsGameOver || player == null || targetSpawner == null)
            return;

        if (CurrentTarget == null)
        {
            chunkCheckTimer += Time.deltaTime;
            if (chunkCheckTimer >= chunkCheckInterval)
            {
                chunkCheckTimer = 0f;
                SpawnCurrentObjective();
            }
            return;
        }

        if (TimerRunning)
        {
            DeliveryTimerSeconds += Time.deltaTime;

            if (deliveryTimeLimit > 0f && DeliveryTimerSeconds >= deliveryTimeLimit)
            {
                TimerRunning = false;
                IsGameOver = true;
                targetSpawner.DespawnCurrentTarget();
                CurrentTarget = null;
                OnGameOver?.Invoke(DeliveryScore);
                return;
            }
        }

        float distance = Vector3.Distance(player.position, CurrentTarget.transform.position);

        if (distance <= collectDistance)
        {
            HandleObjectiveReached();
            return;
        }

        chunkCheckTimer += Time.deltaTime;
        if (chunkCheckTimer >= chunkCheckInterval)
        {
            chunkCheckTimer = 0f;

            if (!targetSpawner.IsPositionInActiveChunk(CurrentTarget.transform.position))
                SpawnCurrentObjective();
        }
    }

    private void HandleObjectiveReached()
    {
        switch (CurrentState)
        {
            case ObjectiveState.Pickup:
                if (audioSource != null && pickupSound != null)
                    audioSource.PlayOneShot(pickupSound);

                DeliveryTimerSeconds = 0f;
                TimerRunning = true;
                CurrentState = ObjectiveState.Delivery;
                break;

            case ObjectiveState.Delivery:
                if (audioSource != null && deliverySound != null)
                    audioSource.PlayOneShot(deliverySound);

                TimerRunning = false;
                DeliveryScore++;
                OnDeliveryCompleted?.Invoke(DeliveryTimerSeconds);
                CurrentState = ObjectiveState.Pickup;
                break;
        }

        SpawnCurrentObjective();
    }

    public void ForceGameOver()
    {
        if (IsGameOver) return;
        TimerRunning = false;
        IsGameOver = true;
        targetSpawner.DespawnCurrentTarget();
        CurrentTarget = null;
        OnGameOver?.Invoke(DeliveryScore);
    }

    public void Restart()
    {
        IsGameOver = false;
        TimerRunning = false;
        DeliveryTimerSeconds = 0f;
        DeliveryScore = 0;
        chunkCheckTimer = 0f;
        CurrentState = ObjectiveState.Pickup;
        CurrentTarget = null;
        OnRestart?.Invoke();
        Invoke(nameof(SpawnCurrentObjective), 0.1f);
    }

    private void SpawnCurrentObjective()
    {
        CurrentTarget = CurrentState == ObjectiveState.Pickup
            ? targetSpawner.SpawnPickupTarget()
            : targetSpawner.SpawnDeliveryTarget();

        if (CurrentTarget == null)
            Debug.LogWarning($"DeliveryManager: failed to spawn {CurrentState} target, will retry.");

        OnObjectiveChanged?.Invoke(CurrentTarget);
    }
}
