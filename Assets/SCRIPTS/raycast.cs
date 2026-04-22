using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ClickRaycast : MonoBehaviour
{
    [Header("Cutscene Delay")]
    [Tooltip("Secondi di attesa prima di attivare il raycast (durata della cutscene)")]
    public float raycastDelay = 60f;

    [Header("Raycast")]
    public float distance = 5f;
    public LayerMask interactableLayer;

    [Header("Crosshair UI")]
    public GameObject crosshairDefault;
    public GameObject crosshairActive;

    [Header("Prompt UI")]
    public TextMeshProUGUI promptText;

    [Header("Optional Settings UI (can be empty)")]
    public GameObject SettingsUi;

    private bool _raycastEnabled = false;
    private float _timer = 0f;

    void Start()
    {
        if (crosshairDefault != null) crosshairDefault.SetActive(false);
        if (crosshairActive != null) crosshairActive.SetActive(false);
        if (promptText != null) promptText.text = "";
    }

    void Update()
    {
        // Aspetta il delay prima di attivare il raycast
        if (!_raycastEnabled)
        {
            _timer += Time.deltaTime;
            if (_timer >= raycastDelay)
            {
                _raycastEnabled = true;
                if (crosshairDefault != null) crosshairDefault.SetActive(true);
            }
            return;
        }

        if (SettingsUi != null && SettingsUi.activeSelf)
        {
            if (crosshairDefault != null) crosshairDefault.SetActive(false);
            if (crosshairActive != null) crosshairActive.SetActive(false);
            if (promptText != null) promptText.text = "";
            return;
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool hitInteractable = Physics.Raycast(ray, out RaycastHit hit, distance, interactableLayer, QueryTriggerInteraction.Collide);

        if (crosshairDefault != null) crosshairDefault.SetActive(!hitInteractable);
        if (crosshairActive != null) crosshairActive.SetActive(hitInteractable);

        if (hitInteractable)
        {
            UpdatePrompt(hit.collider.gameObject);
            hit.collider.SendMessage("OnRaycastHover", SendMessageOptions.DontRequireReceiver);

            bool clicked = false;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                clicked = true;
            if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
                clicked = true;

            if (clicked)
                hit.collider.SendMessage("OnClicked", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            if (promptText != null) promptText.text = "";
        }
    }

    void UpdatePrompt(GameObject target)
    {
        if (promptText == null) return;

        if (target.CompareTag("Readable"))       promptText.text = " Hold [RB] Leggi";
        else if (target.CompareTag("1"))         promptText.text = " [RB] Digita 1";
        else if (target.CompareTag("Tavolo"))         promptText.text = "[x] Appoggia libro";
        else if (target.CompareTag("2"))         promptText.text = " [RB] Digita 2";
        else if (target.CompareTag("3"))         promptText.text = " [RB] Digita 3";
        else if (target.CompareTag("4"))         promptText.text = " [RB] Digita 4";
        else if (target.CompareTag("5"))         promptText.text = " [RB] Digita 5";
        else if (target.CompareTag("6"))         promptText.text = " [RB] Digita 6";
        else if (target.CompareTag("7"))         promptText.text = " [RB] Digita 7";
        else if (target.CompareTag("8"))         promptText.text = " [RB] Digita 8";
        else if (target.CompareTag("9"))         promptText.text = " [RB] Digita 9";
        else if (target.CompareTag("conferma"))  promptText.text = " [RB] Chiama ";
        else if (target.CompareTag("0"))         promptText.text = " [RB] Digita 0";
    }
}