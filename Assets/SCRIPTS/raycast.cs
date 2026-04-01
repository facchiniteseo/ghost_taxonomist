using UnityEngine;
using UnityEngine.InputSystem; 

public class ClickRaycast : MonoBehaviour //classe script monobehavoir per attaccarlo a un oggetto
{
    [Header("Raycast")] // finestrella inspector
    public float distance = 5f; //massimo interagibilità
    public LayerMask interactableLayer; //assegnazione layer

    [Header("Crosshair UI")] 
    public GameObject crosshairDefault;
    public GameObject crosshairActive;

    [Header("Optional Settings UI (can be empty)")]
    public GameObject SettingsUi;

    void Start() //disattivare il mirino allo start
    {
        if (crosshairActive != null) 
            crosshairActive.SetActive(false);
    }

    void Update()
    {
        
        if (SettingsUi != null && SettingsUi.activeSelf)   // Se SettingsUI è attivo, nascondi crosshair
        {
            if (crosshairDefault != null) crosshairDefault.SetActive(false);
            if (crosshairActive != null) crosshairActive.SetActive(false);
            return;
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f)); // crea raggio dalla camera

       
        bool hitInteractable = Physics.Raycast(ray, out RaycastHit hit, distance, interactableLayer, QueryTriggerInteraction.Collide); //impostazione mirino interattivo sui collider

        // Aggiorna crosshair
        if (crosshairDefault != null) crosshairDefault.SetActive(!hitInteractable);
        if (crosshairActive != null) crosshairActive.SetActive(hitInteractable);

        //check del click del giocatore
        bool clicked = false;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            clicked = true;

        if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame)
            clicked = true;

        if (clicked && hitInteractable) //se il giocatore clicca invio del messaggio di interazione
        {
            hit.collider.SendMessage("OnClicked", SendMessageOptions.DontRequireReceiver);
        }
    }
}