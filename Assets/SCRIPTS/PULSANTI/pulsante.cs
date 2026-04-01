using UnityEngine;

[RequireComponent(typeof(Collider))]  // Lo script dipende dal collider
public class ButtonWithMessage : MonoBehaviour  // classe dello script e MonoBehaviour (lo rende funzionante in unity)
{
    [Header("Audio")]  // Creazione finestrella nell'inspector
    public AudioClip clickSound; // Spazio nell'inspector per inserire il suono del click

    [Header("Numero da inviare")] // Creazione finestrella nell'inspector
    public int number = 1; // Numero intercambiabile in ogni script

    private AudioSource audioSource;

    void Awake() // script che si avvia allo start
    {
        audioSource = GetComponent<AudioSource>(); // Prende il lettore audio attaccato a questo bottone
    }

    public void OnClicked() // script che si avvia al click del bottone
    {
        if (clickSound != null) // Se non dovesse esserci clicksound nell'inspector il gioco non crasha
        {
            audioSource.PlayOneShot(clickSound); // Al click riproduci clicksound 
        }

        ConfirmButton.Instance.ReceiveNumber(number); // Invia il messaggio alla conferma

        Debug.Log("Numero " + number + " inviato a Conferma!");
    }
}