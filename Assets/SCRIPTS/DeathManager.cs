using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }

    [Header("Camera principale")]
    public Camera playerCamera;

    [Header("Tempi")]
    [Tooltip("Secondi dall'inizio della chiamata prima che parta l'alternanza")]
    public float attesaPrimaAlternanza = 10f;
    [Tooltip("Secondi di alternanza tra le cam dei fantasmi")]
    public float durataAlternanza = 5f;
    [Tooltip("Secondi per ogni switch di camera")]
    public float tempoPerCamera = 1f;
    [Tooltip("Secondi prima della fine del dissolve in cui torna la player cam")]
    public float anticipoRitornoPlayer = 0.5f;

    void Awake() => Instance = this;

    public void AvviaSequenzaMorte(List<GhostAI> fantasmi, float durataChiamata)
    {
        StartCoroutine(SequenzaMorte(fantasmi, durataChiamata));
    }

    private IEnumerator SequenzaMorte(List<GhostAI> fantasmi, float durataChiamata)
    {
        Debug.Log($"[DeathManager] AVVIO — fantasmi: {fantasmi.Count}, durata: {durataChiamata}");

        float dissolveDur = 2f;
        foreach (GhostAI f in fantasmi)
        {
            if (f != null) { dissolveDur = f.GetDissolveDuration(); break; }
        }

        // FASE 1: aspetta prima dell'alternanza
        yield return new WaitForSeconds(attesaPrimaAlternanza);

        // Raccogli ghost cam
        List<Camera> ghostCams = new List<Camera>();
        List<GhostAI> fantasmiValidi = new List<GhostAI>();

        foreach (GhostAI f in fantasmi)
        {
            if (f != null && f.ghostCamera != null)
            {
                ghostCams.Add(f.ghostCamera);
                fantasmiValidi.Add(f);
            }
            else
                Debug.LogWarning($"[DeathManager] Fantasma null o senza cam: {f?.name}");
        }

        if (ghostCams.Count == 0)
        {
            Debug.LogError("[DeathManager] Nessuna ghost cam!");
            yield break;
        }

        // FASE 2: alternanza — dura esattamente durataAlternanza secondi
        float tempoRimasto = durataAlternanza;
        int indice = 0;

        while (tempoRimasto > 0f)
        {
            for (int i = 0; i < ghostCams.Count; i++)
                ghostCams[i].depth = (i == indice) ? 2 : -2;

            float attesaSwitch = Mathf.Min(tempoPerCamera, tempoRimasto);
            yield return new WaitForSeconds(attesaSwitch);
            tempoRimasto -= tempoPerCamera;
            indice = (indice + 1) % ghostCams.Count;
        }

        // FASE 3: la chiamata è finita — triggera il dissolve
        for (int i = 0; i < ghostCams.Count; i++)
            ghostCams[i].depth = (i == 0) ? 2 : -2;

        foreach (GhostAI f in fantasmiValidi)
            f.TriggerDissolve();

        Debug.Log($"[DeathManager] Dissolve avviato — durata: {dissolveDur}s");

        // FASE 4: aspetta tutto il dissolve
        yield return new WaitForSeconds(dissolveDur - anticipoRitornoPlayer);

        // Riporta la player cam
        foreach (Camera c in ghostCams)
            c.depth = -2;

        yield return new WaitForSeconds(anticipoRitornoPlayer);

        // Prima notifica il GameManager per ogni fantasma eliminato
        foreach (GhostAI f in fantasmiValidi)
            GameEvents.GhostKilled();

        // Poi distrugge i GameObject
        foreach (GhostAI f in fantasmiValidi)
        {
            if (f != null)
                Destroy(f.gameObject);
        }

        Debug.Log("[DeathManager] Sequenza completata");
    }
}