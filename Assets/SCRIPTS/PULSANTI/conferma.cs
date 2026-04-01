using UnityEngine;
using System.Collections.Generic; // permette di usare List collezioni

[RequireComponent(typeof(Collider))] // Assicura che il GameObject abbia un Collider
public class ConfirmButton : MonoBehaviour //classe confirmbutton e monobehaviour per attaccarlo a un game object
{
    
    public static ConfirmButton Instance; // Singleton

    [Header("Audio Conferma")] //finestrella inspector
    public AudioClip confirmSound; // Clip audio da riprodurre alla conferma

    private AudioSource audioSource; // Componente per riprodurre l'audio

    
    private List<int> pendingNumbers = new List<int>(); //lista numeri ricevuti

    void Awake() // cosa fa lo script allo start
    {
        // Imposta il singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Se esiste già un altro ConfirmButton, distruggi questo
            return; // esce dall'Awake
        }

      
        audioSource = GetComponent<AudioSource>(); //chiama la audiosource inserita prima
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; //impedisce che parta allo start
        }
    }


    public void ReceiveNumber(int number)
    {
        if (Instance == this)
            Debug.Log("Ricevuto numero su istanza corretta: " + number);
        else
            Debug.LogWarning("Ricevuto numero su istanza sbagliata! " + number);

        pendingNumbers.Add(number);
    }


    public void OnClicked() // Funzione chiamata quando premi il pulsante Conferma
    {
        
        if (confirmSound != null)      // Riproduce l'audio di conferma 1 sola volta
        {
            audioSource.PlayOneShot(confirmSound);
        }

        foreach (int num in pendingNumbers) // Aggiunge tutti i numeri accumulati alla Rubrica
        {
            Rubrica.Instance.AddNumber(num);
        }

      
        Rubrica.Instance.ConfirmNumber(); // Aggiunge tutti i numeri accumulati alla Rubrica che farà il controllo finale

     
        pendingNumbers.Clear(); // finiti i processi svuota la lista

        Debug.Log("Buffer numeri svuotato dopo la conferma!");
    }
}