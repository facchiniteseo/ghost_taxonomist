using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class InteractiveBook : MonoBehaviour
{
    [Header("Parti del libro")]
    public Transform costola;
    public Transform copertinaSinistra;

    [Header("Rotazioni apertura (locali)")]
    public Vector3 cosolaOpenRotation = new Vector3(0f, 90f, 0f);
    public Vector3 copertinaOpenRotation = new Vector3(0f, -90f, 0f);

    [Header("Posizione copertina")]
    public Vector3 copertinaOpenPositionOffset = new Vector3(0f, 0f, 0.2f);

    [Header("Posizione davanti alla camera")]
    public float distanzaCamera = 0.6f;
    public float altezzaOffset = -0.2f;
    public float lateraleOffset = 0f;

    [Header("Posizione chiuso (basso destra)")]
    public float chiusoAltezzaOffset = -0.5f;
    public float chiusoLateraleOffset = 0.3f;
    public float distanzaCameraChiuso = 0.9f;

    [Header("Rotazione libro aperto")]
    public Vector3 libroOpenRotationEuler = new Vector3(-90f, 90f, 0f);

    [Header("Tempi")]
    public float moveDuration = 0.5f;
    public float openDuration = 0.4f;
    public float closeDuration = 0.4f;

    [Header("Follow")]
    public float followSpeed = 8f;

    [Header("Inventario")]
    public Sprite spriteInventario;

    [Header("Prompt UI")]
    public TextMeshProUGUI promptText; // ← stesso oggetto del ClickRaycast

    private enum State { Idle, MovingIn, Open, Closed, MovingOut, InInventario }
    private State stato = State.Idle;

    private bool raycastHit = false;
    private bool isAnimating = false;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private Transform originalParent;

    private Quaternion cosolaChiusa, copertinaChiusa;
    private Quaternion cosolaAperta, copertinaAperta;
    private Vector3 copertinaOriginalLocalPos;

    private Camera cam;
    private BookSlot currentSlot = null;

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
        originalParent = transform.parent;

        cosolaChiusa = costola.localRotation;
        copertinaChiusa = copertinaSinistra.localRotation;
        copertinaOriginalLocalPos = copertinaSinistra.localPosition;

        cosolaAperta = Quaternion.Euler(cosolaOpenRotation);
        copertinaAperta = Quaternion.Euler(copertinaOpenRotation);
    }

    Vector3 TargetOpenPosition()
    {
        return cam.transform.position
             + cam.transform.forward * distanzaCamera
             + cam.transform.up * altezzaOffset
             + cam.transform.right * lateraleOffset;
    }

    Vector3 TargetClosedPosition()
    {
        return cam.transform.position
             + cam.transform.forward * distanzaCameraChiuso
             + cam.transform.up * chiusoAltezzaOffset
             + cam.transform.right * chiusoLateraleOffset;
    }

    void UpdatePrompt()
    {
        if (promptText == null) return;

        switch (stato)
        {
            case State.Open:
                // Libro aperto davanti → mostra opzioni
                bool rbOpen = Gamepad.current != null && Gamepad.current.rightShoulder.isPressed;
                if (rbOpen && BookInventory.Instance != null && !BookInventory.Instance.IsPieno)
                    promptText.text = "[X] Porta con te";
                else
                    promptText.text = "[X] Chiudi    [X+RB] Inventario";
                break;

            case State.Closed:
                // Libro chiuso in mano → mostra opzioni
                bool rbClosed = Gamepad.current != null && Gamepad.current.rightShoulder.isPressed;
                if (rbClosed && BookInventory.Instance != null && !BookInventory.Instance.IsPieno)
                    promptText.text = "[X] In inventario";
                else
                    promptText.text = "[X] Apri    [X+RB] Inventario";
                break;

            case State.Idle:
                // Il ClickRaycast gestisce il testo quando il libro è a riposo
                break;

            default:
                // Durante animazioni non mostrare nulla
                promptText.text = "";
                break;
        }
    }

    void Update()
    {
        if (isAnimating) return;

        bool rb = Gamepad.current != null && Gamepad.current.rightShoulder.isPressed;
        bool x = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;

        switch (stato)
        {
            case State.Idle:
                if (x && BookInventory.Instance != null)
                    BookInventory.Instance.TentaDeposito();

                if (rb && raycastHit)
                {
                    if (currentSlot != null)
                    {
                        currentSlot.Free();
                        currentSlot = null;
                    }
                    StartCoroutine(PrendiEApriLibro());
                }
                break;

            case State.Open:
                SeguiCamera(TargetOpenPosition(),
                    cam.transform.rotation * Quaternion.Euler(libroOpenRotationEuler));

                if (x && rb)
                {
                    if (BookInventory.Instance != null && !BookInventory.Instance.IsPieno)
                        StartCoroutine(ChiudiEVaiInInventario());
                }
                else if (x)
                    StartCoroutine(ChiudiLibro());
                else if (!rb)
                    StartCoroutine(ChiudiEVaiASlot());
                break;

            case State.Closed:
                SeguiCamera(TargetClosedPosition(), transform.rotation);

                if (x && rb)
                {
                    if (BookInventory.Instance != null && !BookInventory.Instance.IsPieno)
                        StartCoroutine(VaiInInventario());
                }
                else if (x)
                    StartCoroutine(ApriLibro());
                else if (!rb)
                    StartCoroutine(VaiASlot(cam.transform.position));
                break;
        }

        // Aggiorna il prompt ogni frame in base allo stato
        UpdatePrompt();

        raycastHit = false;
    }

    // -- tutto il resto invariato --

    public void EsciDaInventario()
    {
        transform.position = TargetClosedPosition();
        transform.rotation = cam.transform.rotation;
        StartCoroutine(VaiASlot(cam.transform.position));
    }

    public void EsciDaInventarioVersoDato(BookSlot slot)
    {
        transform.position = TargetClosedPosition();
        transform.rotation = cam.transform.rotation;
        StartCoroutine(VaiASlotDato(slot));
    }

    BookSlot TrovaSlotPiuVicino(Vector3 riferimento)
    {
        BookSlot[] tuttiSlot = FindObjectsByType<BookSlot>(FindObjectsSortMode.None);
        BookSlot migliore = null;
        float distanzaMinima = float.MaxValue;

        foreach (BookSlot slot in tuttiSlot)
        {
            if (slot.IsOccupied) continue;
            float d = Vector3.Distance(riferimento, slot.transform.position);
            if (d < distanzaMinima)
            {
                distanzaMinima = d;
                migliore = slot;
            }
        }

        return migliore;
    }

    void SeguiCamera(Vector3 targetPos, Quaternion targetRot)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
    }

    public void OnRaycastHover() => raycastHit = true;

    IEnumerator PrendiEApriLibro()
    {
        isAnimating = true;
        stato = State.MovingIn;
        transform.SetParent(null);

        Vector3 startPos = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float s = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, TargetOpenPosition(), s);
            yield return null;
        }

        yield return StartCoroutine(ApriLibro());
        isAnimating = false;
    }

    IEnumerator ApriLibro()
    {
        isAnimating = true;
        stato = State.Open;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            float s = Mathf.SmoothStep(0f, 1f, t);

            costola.localRotation = Quaternion.Slerp(cosolaChiusa, cosolaAperta, s);
            copertinaSinistra.localRotation = Quaternion.Slerp(copertinaChiusa, copertinaAperta, s);
            copertinaSinistra.localPosition = Vector3.Lerp(
                copertinaOriginalLocalPos,
                copertinaOriginalLocalPos + copertinaOpenPositionOffset, s);

            yield return null;
        }

        isAnimating = false;
    }

    IEnumerator ChiudiLibro()
    {
        isAnimating = true;
        stato = State.Closed;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / closeDuration;
            float s = Mathf.SmoothStep(0f, 1f, t);

            costola.localRotation = Quaternion.Slerp(cosolaAperta, cosolaChiusa, s);
            copertinaSinistra.localRotation = Quaternion.Slerp(copertinaAperta, copertinaChiusa, s);
            copertinaSinistra.localPosition = Vector3.Lerp(
                copertinaOriginalLocalPos + copertinaOpenPositionOffset,
                copertinaOriginalLocalPos, s);

            yield return null;
        }

        isAnimating = false;
    }

    IEnumerator ChiudiEVaiInInventario()
    {
        yield return StartCoroutine(ChiudiLibro());
        yield return StartCoroutine(VaiInInventario());
    }

    IEnumerator VaiInInventario()
    {
        isAnimating = true;
        stato = State.InInventario;

        if (promptText != null) promptText.text = ""; // ← pulisce quando va in inventario

        BookInventory.Instance.AggiungiLibro(this);
        gameObject.SetActive(false);

        isAnimating = false;
        yield break;
    }

    IEnumerator ChiudiEVaiASlot()
    {
        yield return StartCoroutine(ChiudiLibro());
        yield return StartCoroutine(VaiASlot(cam.transform.position));
    }

    IEnumerator VaiASlot(Vector3 riferimento)
    {
        isAnimating = true;
        stato = State.MovingOut;

        if (promptText != null) promptText.text = ""; // ← pulisce durante il movimento

        BookSlot slot = TrovaSlotPiuVicino(riferimento);

        Vector3 targetPos = slot != null ? slot.transform.position : originalPos;
        Quaternion targetRot = slot != null ? slot.transform.rotation : originalRot;

        if (slot != null)
        {
            slot.TryOccupy(this);
            currentSlot = slot;
        }

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float s = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, targetPos, s);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, s);
            yield return null;
        }

        transform.SetParent(originalParent);
        stato = State.Idle;
        isAnimating = false;
    }

    IEnumerator VaiASlotDato(BookSlot slot)
    {
        isAnimating = true;
        stato = State.MovingOut;

        if (promptText != null) promptText.text = ""; // ← pulisce durante il movimento

        slot.TryOccupy(this);
        currentSlot = slot;

        Vector3 targetPos = slot.transform.position;
        Quaternion targetRot = slot.transform.rotation;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float s = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, targetPos, s);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, s);
            yield return null;
        }

        transform.SetParent(originalParent);
        stato = State.Idle;
        isAnimating = false;
    }
}