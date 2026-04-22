using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Disattiva i GameObject fuori dal frustum della camera.
/// Ottimizzato: check distribuiti su più frame, piani ricalcolati solo quando la camera si muove.
/// </summary>
public class FrustumCullingManager : MonoBehaviour
{
    [System.Serializable]
    public class ConfigTag
    {
        public string tag = "Raccoglibile";
        [Tooltip("Distanza massima oltre cui disattivare")]
        public float distanzaMassima = 50f;
        [Tooltip("Margine extra attorno al frustum")]
        public float margine = 0.1f;
    }

    [Header("Configurazione")]
    [SerializeField] private ConfigTag[] tagOggetti = { new ConfigTag() };
    [SerializeField] private bool includiChilds = true;
    [SerializeField] private string[] tagEsclusi = { "Ground", "Terrain" };

    [Header("Performance")]
    [Tooltip("Oggetti controllati per frame — tienilo basso (5-10)")]
    [SerializeField] private int controlliPerFrame = 8;
    [Tooltip("Soglia di movimento camera oltre cui ricalcolare i piani frustum (gradi/metri)")]
    [SerializeField] private float sogliaCameraUpdate = 0.5f;

    [Header("Fade In")]
    [SerializeField] private float durateFadeIn = 0.4f;
    [SerializeField] private bool soloConRenderer = true;

    private List<GameObject> oggettiRegistrati = new List<GameObject>();
    private Dictionary<GameObject, Coroutine> fadeAttivi = new Dictionary<GameObject, Coroutine>();
    private Camera cam;
    private int indiceCorrente = 0;

    // Cache piani frustum — ricalcolati solo se la camera si muove
    private Plane[] pianiCache = new Plane[6];
    private Vector3 posCameraPrec;
    private Quaternion rotCameraPrec;
    private bool pianiValidi = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("[FrustumCulling] Nessuna Camera trovata su questo GameObject!");
            enabled = false;
        }
    }

    void Start()
    {
        TrovaOggettiPerTag();
        StartCoroutine(CullingLoop());
    }

    public void TrovaOggettiPerTag()
    {
        oggettiRegistrati.Clear();
        int totale = 0;
        foreach (var cfg in tagOggetti)
        {
            if (cfg == null || string.IsNullOrEmpty(cfg.tag)) continue;
            try
            {
                GameObject[] trovati = GameObject.FindGameObjectsWithTag(cfg.tag);
                foreach (var go in trovati) RegistraOggetto(go);
                totale += trovati.Length;
            }
            catch { Debug.LogWarning($"[FrustumCulling] Tag '{cfg.tag}' non esiste, ignorato."); }
        }
        Debug.Log($"[FrustumCulling] Registrati {totale} oggetti.");
    }

    public void RegistraOggetto(GameObject go)
    {
        if (!oggettiRegistrati.Contains(go)) oggettiRegistrati.Add(go);
    }

    public void DeregistraOggetto(GameObject go)
    {
        oggettiRegistrati.Remove(go);
        if (fadeAttivi.ContainsKey(go)) fadeAttivi.Remove(go);
    }

    private Plane[] OttieniPiani()
    {
        // Ricalcola piani solo se la camera si è mossa o ruotata
        bool cameraMossa = Vector3.Distance(cam.transform.position, posCameraPrec) > sogliaCameraUpdate * 0.1f;
        bool cameraRuotata = Quaternion.Angle(cam.transform.rotation, rotCameraPrec) > sogliaCameraUpdate;

        if (!pianiValidi || cameraMossa || cameraRuotata)
        {
            GeometryUtility.CalculateFrustumPlanes(cam, pianiCache);
            posCameraPrec = cam.transform.position;
            rotCameraPrec = cam.transform.rotation;
            pianiValidi = true;
        }
        return pianiCache;
    }

    private IEnumerator CullingLoop()
    {
        // Aspetta un frame prima di iniziare
        yield return null;

        while (true)
        {
            if (oggettiRegistrati.Count == 0) { yield return null; continue; }

            Plane[] piani = OttieniPiani();
            int controllati = 0;

            while (controllati < controlliPerFrame)
            {
                // Rimuovi null
                if (indiceCorrente < oggettiRegistrati.Count && oggettiRegistrati[indiceCorrente] == null)
                {
                    oggettiRegistrati.RemoveAt(indiceCorrente);
                    if (oggettiRegistrati.Count == 0) break;
                    continue;
                }

                if (oggettiRegistrati.Count == 0) break;

                indiceCorrente %= oggettiRegistrati.Count;
                AggiornaCulling(oggettiRegistrati[indiceCorrente], piani);
                indiceCorrente++;
                controllati++;

                if (indiceCorrente >= oggettiRegistrati.Count)
                    indiceCorrente = 0;
            }

            yield return null; // un frame tra ogni batch
        }
    }

    private ConfigTag TrovaConfigPerTag(GameObject go)
    {
        foreach (var cfg in tagOggetti)
            if (cfg != null && go.CompareTag(cfg.tag)) return cfg;
        return null;
    }

    private void AggiornaCulling(GameObject go, Plane[] piani)
    {
        foreach (var tag in tagEsclusi)
            if (!string.IsNullOrEmpty(tag) && go.CompareTag(tag)) return;

        ConfigTag cfg = TrovaConfigPerTag(go);
        float distMax = cfg != null ? cfg.distanzaMassima : 50f;
        float marg = cfg != null ? cfg.margine : 0.1f;

        float distanza = Vector3.Distance(cam.transform.position, go.transform.position);
        bool deveEssereAttivo = distanza <= distMax;

        if (deveEssereAttivo)
        {
            Bounds bounds;
            if (!TryGetBounds(go, out bounds))
                bounds = new Bounds(go.transform.position, Vector3.one * 0.5f);
            bounds.Expand(marg);
            deveEssereAttivo = GeometryUtility.TestPlanesAABB(piani, bounds);
        }

        if (deveEssereAttivo && !go.activeSelf)
        {
            go.SetActive(true);
            AvviaFadeIn(go);
        }
        else if (!deveEssereAttivo && go.activeSelf)
        {
            if (fadeAttivi.TryGetValue(go, out Coroutine c) && c != null)
            {
                StopCoroutine(c);
                fadeAttivi.Remove(go);
                RipristinaAlpha(go, 1f);
            }
            go.SetActive(false);
        }
    }

    private void AvviaFadeIn(GameObject go)
    {
        if (fadeAttivi.TryGetValue(go, out Coroutine vecchio) && vecchio != null)
            StopCoroutine(vecchio);

        Renderer[] renderers = includiChilds
            ? go.GetComponentsInChildren<Renderer>()
            : go.GetComponents<Renderer>();

        if (renderers.Length == 0 && soloConRenderer) return;

        fadeAttivi[go] = StartCoroutine(FadeIn(go, renderers));
    }

    private IEnumerator FadeIn(GameObject go, Renderer[] renderers)
    {
        ImpostaAlphaRenderers(renderers, 0f);
        float t = 0f;
        while (t < durateFadeIn)
        {
            if (go == null || !go.activeSelf) yield break;
            t += Time.deltaTime;
            ImpostaAlphaRenderers(renderers, Mathf.Clamp01(t / durateFadeIn));
            yield return null;
        }
        ImpostaAlphaRenderers(renderers, 1f);
        if (fadeAttivi.ContainsKey(go)) fadeAttivi.Remove(go);
    }

    private void ImpostaAlphaRenderers(Renderer[] renderers, float alpha)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                if (mat == null) continue;
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor"); c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color c = mat.GetColor("_Color"); c.a = alpha;
                    mat.SetColor("_Color", c);
                }
            }
        }
    }

    private void RipristinaAlpha(GameObject go, float alpha)
    {
        Renderer[] renderers = includiChilds
            ? go.GetComponentsInChildren<Renderer>()
            : go.GetComponents<Renderer>();
        ImpostaAlphaRenderers(renderers, alpha);
    }

    private bool TryGetBounds(GameObject go, out Bounds bounds)
    {
        Renderer r = includiChilds ? go.GetComponentInChildren<Renderer>() : go.GetComponent<Renderer>();
        if (r != null) { bounds = r.bounds; return true; }
        Collider c = includiChilds ? go.GetComponentInChildren<Collider>() : go.GetComponent<Collider>();
        if (c != null) { bounds = c.bounds; return true; }
        bounds = default;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (tagOggetti == null) return;
        Color[] colori = { new Color(0f, 1f, 1f, 0.15f), new Color(1f, 1f, 0f, 0.15f), new Color(1f, 0.5f, 0f, 0.15f) };
        for (int i = 0; i < tagOggetti.Length; i++)
        {
            if (tagOggetti[i] == null) continue;
            Gizmos.color = colori[i % colori.Length];
            Gizmos.DrawWireSphere(transform.position, tagOggetti[i].distanzaMassima);
        }
    }
}