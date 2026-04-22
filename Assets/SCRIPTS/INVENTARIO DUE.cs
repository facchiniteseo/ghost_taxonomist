using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BookInventory : MonoBehaviour
{
    public static BookInventory Instance { get; private set; }

    [Header("UI - Contenitore slot")]
    public RectTransform slot1Container;
    public RectTransform slot2Container;
    public GameObject slot3Blocked;

    [Header("Dimensioni slot")]
    public Vector2 slot1Size = new Vector2(80f, 100f);
    public Vector2 slot2Size = new Vector2(56f, 70f);

    [Header("Interazione")]
    public float interactDistance = 5f;
    public LayerMask interactableLayer;

    private Queue<InteractiveBook> libri = new Queue<InteractiveBook>();
    public int MaxSlot => 2;
    public bool IsPieno => libri.Count >= MaxSlot;

    private Image slot1Image;
    private Image slot2Image;
    private Camera cam;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cam = Camera.main;
        slot1Image = CreaSlotImage(slot1Container, slot1Size);
        slot2Image = CreaSlotImage(slot2Container, slot2Size);
        slot1Image.gameObject.SetActive(false);
        slot2Image.gameObject.SetActive(false);
        slot3Blocked.SetActive(false);
    }

    void Update()
    {
        // FIX: è BookInventory stesso a intercettare X quando ha libri in coda,
        // così funziona anche quando tutti i libri Idle sono disattivati
        bool x = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
        if (x && libri.Count > 0)
            TentaDeposito();
    }

    Image CreaSlotImage(RectTransform container, Vector2 size)
    {
        GameObject go = new GameObject("SlotImage");
        go.transform.SetParent(container, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.preserveAspect = true;
        return img;
    }

    public void TentaDeposito()
    {
        if (libri.Count == 0) return;

        BookSlot slot = TrovaSlotGuardato();
        if (slot == null) return;

        InteractiveBook libro = libri.Dequeue();
        AggiornaUI();
        libro.gameObject.SetActive(true);
        libro.EsciDaInventarioVersoDato(slot);
    }

    public BookSlot TrovaSlotGuardato()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            BookSlot slot = hit.collider.GetComponent<BookSlot>();
            if (slot == null)
                slot = hit.collider.GetComponentInChildren<BookSlot>();
            if (slot == null)
                slot = hit.collider.GetComponentInParent<BookSlot>();

            if (slot != null && !slot.IsOccupied)
                return slot;
        }
        return null;
    }

    public bool AggiungiLibro(InteractiveBook libro)
    {
        if (IsPieno) return false;
        libri.Enqueue(libro);
        AggiornaUI();
        return true;
    }

    void AggiornaUI()
    {
        InteractiveBook[] array = libri.ToArray();

        if (array.Length >= 1)
        {
            slot1Image.gameObject.SetActive(true);
            slot1Image.sprite = array[0].spriteInventario;
        }
        else
        {
            slot1Image.gameObject.SetActive(false);
        }

        if (array.Length >= 2)
        {
            slot2Image.gameObject.SetActive(true);
            slot2Image.sprite = array[1].spriteInventario;
        }
        else
        {
            slot2Image.gameObject.SetActive(false);
        }

        slot3Blocked.SetActive(array.Length >= 2);
    }
}