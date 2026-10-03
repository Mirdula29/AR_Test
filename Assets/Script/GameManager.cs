using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI GameObjects")]
    [Tooltip("The 'How to Play' instruction modal card")]
    [SerializeField] private GameObject howToPlayUI;

    [Tooltip("Parent object containing Fire Button and Reticle/Crosshair")]
    [SerializeField] private GameObject fireButtonUI;

    [Tooltip("The 'MISSED!' badge popup")]
    [SerializeField] private GameObject missedPopupUI;

    [Tooltip("The 'WAVE CLEARED' graphic")]
    [SerializeField] private GameObject waveClearedUI;

    [Header("Placement Manager Reference")]
    [SerializeField] private ARPlacementManager placementManager;

    [Header("Ballistic Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform arCamera;
    [SerializeField] private float launchSpeed = 9.0f;
    [SerializeField] private float upwardArc = 0.12f;

    private TextMeshProUGUI worldScoreText;
    private int score = 0;
    private int targetsRemaining = 3;
    private Coroutine missRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (placementManager == null)
        {
            placementManager = FindFirstObjectByType<ARPlacementManager>();
        }
    }

    void Start()
    {
        // 1. Initial State: Only show How to Play
        if (howToPlayUI != null) howToPlayUI.SetActive(true);
        if (fireButtonUI != null) fireButtonUI.SetActive(false);
        if (missedPopupUI != null) missedPopupUI.SetActive(false);
        if (waveClearedUI != null) waveClearedUI.SetActive(false);

        UpdateScoreUI();
    }

    // Hooked to the "Start" button inside How To Play
    public void CloseHowToPlay()
    {
        if (howToPlayUI != null) howToPlayUI.SetActive(false);

        // Turn on plane detection and allow tapping floor directly
        if (placementManager != null)
        {
            placementManager.EnablePlacement();
        }
    }

    // Called automatically by ARPlacementManager when the base is dropped
    public void OnBasePlaced(GameObject spawnedBase)
    {
        // Turn on combat controls (Fire Button + Crosshair)
        if (fireButtonUI != null) fireButtonUI.SetActive(true);

        if (spawnedBase != null)
        {
            TextMeshProUGUI[] textComponents = spawnedBase.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in textComponents)
            {
                if (txt.gameObject.name == "ScoreText")
                {
                    worldScoreText = txt;
                    break;
                }
            }

            if (worldScoreText == null && textComponents.Length > 0)
            {
                worldScoreText = textComponents[0];
            }

            UpdateScoreUI();
        }
    }

    public void FireCannon()
    {
        if (projectilePrefab == null || arCamera == null) return;

        GameObject bullet = Instantiate(projectilePrefab, arCamera.position, arCamera.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 trajectory = (arCamera.forward + (arCamera.up * upwardArc)).normalized;
            rb.linearVelocity = trajectory * launchSpeed;
        }
    }

    public void OnTargetHit()
    {
        score += 10;
        targetsRemaining--;
        UpdateScoreUI();

        // Wave Cleared: Show banner, hide Fire Button
        if (targetsRemaining <= 0)
        {
            if (waveClearedUI != null) waveClearedUI.SetActive(true);
            if (fireButtonUI != null) fireButtonUI.SetActive(false);
        }
    }

    public void OnProjectileMiss()
    {
        if (missedPopupUI == null) return;

        if (missRoutine != null) StopCoroutine(missRoutine);
        missRoutine = StartCoroutine(ShowMissBadge());
    }

    private IEnumerator ShowMissBadge()
    {
        missedPopupUI.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        missedPopupUI.SetActive(false);
    }

    public void UpdateScoreUI()
    {
        if (worldScoreText != null)
        {
            worldScoreText.text = $"{score}";
        }
    }
}