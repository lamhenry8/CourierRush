using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    public DeliveryManager deliveryManager;

    [Header("UI Text")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestTimeText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public Button restartButton;

    [Header("Low Time Warning")]
    [Tooltip("Seconds remaining at which the timer text turns red.")]
    public float warningThreshold = 5f;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    private float bestTime = float.MaxValue;
    private bool hasBestTime;

    void Start()
    {
        if (deliveryManager != null)
        {
            deliveryManager.OnDeliveryCompleted += HandleDeliveryCompleted;
            deliveryManager.OnGameOver += HandleGameOver;
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(HandleRestart);
            restartButton.gameObject.SetActive(false);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameOverScoreText != null)
            gameOverScoreText.gameObject.SetActive(false);

        if (statusText != null)
            statusText.text = "Pick up a package!";

        if (scoreText != null)
            scoreText.text = "Deliveries: 0";

        if (bestTimeText != null)
            bestTimeText.text = "";
    }

    void OnDestroy()
    {
        if (deliveryManager != null)
        {
            deliveryManager.OnDeliveryCompleted -= HandleDeliveryCompleted;
            deliveryManager.OnGameOver -= HandleGameOver;
        }
    }

    void Update()
    {
        if (deliveryManager == null || statusText == null || deliveryManager.IsGameOver)
            return;

#if UNITY_EDITOR
        if (Keyboard.current.rKey.wasPressedThisFrame)
            deliveryManager.ForceGameOver();
#endif

        if (!deliveryManager.TimerRunning)
            return;

        float remaining = deliveryManager.TimeRemaining;
        statusText.text = FormatTime(remaining);
        statusText.color = remaining <= warningThreshold ? warningColor : normalColor;
    }

    private void HandleDeliveryCompleted(float elapsed)
    {
        if (statusText != null)
        {
            statusText.text = "Delivered!";
            statusText.color = normalColor;
        }

        if (elapsed < bestTime)
        {
            bestTime = elapsed;
            hasBestTime = true;
        }

        if (scoreText != null)
            scoreText.text = $"Deliveries: {deliveryManager.DeliveryScore}";

        if (bestTimeText != null && hasBestTime)
            bestTimeText.text = $"Best: {FormatTime(bestTime)}";
    }

    private void HandleGameOver(int finalScore)
    {
        Time.timeScale = 0f;

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverScoreText != null)
        {
            gameOverScoreText.gameObject.SetActive(true);
            gameOverScoreText.text = $"Deliveries Made: {finalScore}";
        }

        if (statusText != null)
        {
            statusText.text = "Time's up!";
            statusText.color = warningColor;
        }
    }

    private void HandleRestart()
    {
        Time.timeScale = 1f;

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameOverScoreText != null)
        {
            gameOverScoreText.gameObject.SetActive(false);
            gameOverScoreText.text = "";
        }

        if (statusText != null)
        {
            statusText.text = "Pick up a package!";
            statusText.color = normalColor;
        }

        if (scoreText != null)
            scoreText.text = "Deliveries: 0";

        bestTime = float.MaxValue;
        hasBestTime = false;

        if (bestTimeText != null)
            bestTimeText.text = "";

        if (deliveryManager != null)
            deliveryManager.Restart();
    }

    private string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int mins = (int)(seconds / 60f);
        float secs = seconds % 60f;
        return mins > 0 ? $"{mins}:{secs:00.0}" : $"{secs:0.0}s";
    }
}
