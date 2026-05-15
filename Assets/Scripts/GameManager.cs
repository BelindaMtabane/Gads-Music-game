using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the pre-game countdown (5-4-3-2-1-GO!) then enables gameplay.
/// Also handles the Game Over sequence when the player is caught.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool GameStarted { get; private set; } = false;

    [Header("Countdown")]
    [Tooltip("TextMeshProUGUI element to show the countdown numbers")]
    public TextMeshProUGUI countdownText;
    [Tooltip("How many seconds to count down from")]
    public int countdownSeconds = 5;

    [Header("Game Over")]
    [Tooltip("TextMeshProUGUI element to show GAME OVER")]
    public TextMeshProUGUI gameOverText;

    [Header("Scene References")]
    public PlayerMovement   playerMovement;
    public EnemyBase        enemyBase;
    public Animator         playerAnimator;
    public Animator         enemyAnimator;

    private bool gameOverTriggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GameStarted = false;
    }

    private void Start()
    {
        // Auto-find references if not assigned
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (enemyBase == null)
            enemyBase = FindAnyObjectByType<EnemyBase>();

        GameObject player = playerMovement != null ? playerMovement.gameObject : null;
        if (playerAnimator == null && player != null)
            playerAnimator = player.GetComponentInChildren<Animator>();

        GameObject enemy = enemyBase != null ? enemyBase.gameObject : null;
        if (enemyAnimator == null && enemy != null)
            enemyAnimator = enemy.GetComponentInChildren<Animator>();

        // Hide game over text at start
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        // Freeze player and enemy movement
        if (playerMovement != null) playerMovement.enabled = false;
        if (enemyBase      != null) enemyBase.enabled      = false;

        // Force idle animations
        if (playerAnimator != null) playerAnimator.SetFloat("Speed", 0f);
        if (enemyAnimator  != null) enemyAnimator.SetFloat("Speed", 0f);

        // Show countdown
        for (int i = countdownSeconds; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(1f);
        }

        // "GO!"
        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.8f);
            countdownText.gameObject.SetActive(false);
        }

        // Start game
        GameStarted = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (enemyBase      != null) enemyBase.enabled      = true;
    }

    /// <summary>
    /// Called the moment the player dies (from PlayerAnimationController or EnemyBase).
    /// Plays the Defeated animation, stops the enemy, then shows GAME OVER.
    /// </summary>
    public void TriggerGameOver()
    {
        if (gameOverTriggered) return;
        gameOverTriggered = true;
        GameStarted = false;

        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // Stop player movement immediately
        if (playerMovement != null) playerMovement.enabled = false;

        // Force player into Defeated animation
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
            playerAnimator.SetBool("IsDead", true);
        }

        // Brief pause so Defeated anim can start
        yield return new WaitForSeconds(0.3f);

        // Stop enemy — EnemyAnimationController detects zero movement → plays idle
        if (enemyBase     != null) enemyBase.enabled = false;
        if (enemyAnimator != null) enemyAnimator.SetFloat("Speed", 0f);

        // Wait for Defeated animation to play before showing text
        yield return new WaitForSeconds(1.5f);

        // Show GAME OVER
        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
            gameOverText.gameObject.SetActive(true);
        }
    }
}
