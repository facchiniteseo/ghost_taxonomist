using UnityEngine;

public class LightController : MonoBehaviour
{
    [Header("Riferimento")]
    public WorldClock worldClock;

    [Header("Colori")]
    [Tooltip("Colore a mezzanotte / inizio livello")]
    public Color nightColor = new Color(0.05f, 0.05f, 0.2f);
    [Tooltip("Colore alle 8:00 / fine livello")]
    public Color morningColor = new Color(1f, 0.95f, 0.7f);

    [Header("Intensità")]
    public float nightIntensity = 0.1f;
    public float morningIntensity = 1.2f;

    private Light _light;

    void Start()
    {
        _light = GetComponent<Light>();
    }

    void Update()
    {
        if (worldClock == null) return;

        float t = worldClock.T;
        _light.color = Color.Lerp(nightColor, morningColor, t);
        _light.intensity = Mathf.Lerp(nightIntensity, morningIntensity, t);
    }
}