using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class WorldClock : MonoBehaviour
{
    [Header("Durata livello")]
    [Tooltip("Minuti reali per completare il livello")]
    public float levelDurationMinutes = 8f;

    [Header("Orario narrativo")]
    [Tooltip("Ora di partenza (es. 0 = mezzanotte)")]
    public float narrativeStartHour = 0f;
    [Tooltip("Ora di arrivo (es. 8 = 08:00)")]
    public float narrativeEndHour = 8f;

    [Header("Display")]
    public TextMeshPro clockText;

    [Header("Penalità respawn")]
    [Tooltip("Minuti narrativi sottratti al respawn")]
    public float penaltyNarrativeMinutes = 30f;
    [Tooltip("Secondi di lampeggio dopo il respawn")]
    public float blinkDuration = 5f;
    [Tooltip("Velocità lampeggio — più alto = più veloce")]
    public float blinkSpeed = 8f;

    [Header("Evento fine livello")]
    public UnityEvent onLevelComplete;

    public static WorldClock Instance;

    private float realElapsed = 0f;
    private float realDuration;
    private bool isFinished = false;
    private bool isBlinking = false;
    private float blinkTimer = 0f;

    // Rosso scuro normale — rosso brillante durante il lampeggio
    private readonly Color colorNormal = new Color(0.8f, 0f, 0f);
    private readonly Color colorBright = new Color(1f, 1f, 1f);

    public float T => Mathf.Clamp01(realElapsed / realDuration);

    void Awake()
    {
        Instance = this;
        realDuration = levelDurationMinutes * 60f;
    }

    void Start()
    {
        clockText.color = colorNormal;
    }

    void Update()
    {
        if (isFinished) return;

        realElapsed += Time.deltaTime;

        if (realElapsed >= realDuration)
        {
            realElapsed = realDuration;
            isFinished = true;
            isBlinking = false;
            clockText.color = colorNormal;
            UpdateDisplay();
            onLevelComplete?.Invoke();
            return;
        }

        if (isBlinking)
        {
            blinkTimer -= Time.deltaTime;

            // Oscilla tra rosso brillante e rosso scuro
            float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
            clockText.color = Color.Lerp(colorNormal, colorBright, t);

            if (blinkTimer <= 0f)
            {
                isBlinking = false;
                clockText.color = colorNormal;
            }
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        float narrativeHours = Mathf.Lerp(narrativeStartHour, narrativeEndHour, T);
        int h = Mathf.FloorToInt(narrativeHours);
        int m = Mathf.FloorToInt((narrativeHours - h) * 60f);
        clockText.text = string.Format("{0:00}:{1:00}", h, m);
    }

    public void ApplyRespawnPenalty()
    {
        float narrativeRange = (narrativeEndHour - narrativeStartHour) * 60f;
        float penaltyFraction = penaltyNarrativeMinutes / narrativeRange;
        float penaltyReal = penaltyFraction * realDuration;

        realElapsed = Mathf.Min(realElapsed + penaltyReal, realDuration);
        isBlinking = true;
        blinkTimer = blinkDuration;
    }
}