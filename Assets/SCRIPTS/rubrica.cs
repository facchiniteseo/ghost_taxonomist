using UnityEngine;
using System.Text; // per usare StringBuilder e lavorare con le stringhe

[RequireComponent(typeof(AudioSource))]  //richiesta componente audio source
public class Rubrica : MonoBehaviour //enuncio classe dello script, monobehaviour per attaccarla a un game object
{
    public static Rubrica Instance; //variabile statica per implementare singleton

    [Header("Numeri salvati")] //finestrella sull'inspector
    public AudioClip wrong;
    public AudioClip ghostbusterSbagliato;
    public AudioClip numero1;
    public AudioClip numero2;

    private AudioSource audioSource; // variabile privata per far riferimento al gameobject assegnato
    private StringBuilder currentNumber = new StringBuilder(); //variabile privata per lo stringbuilding interno

    void Awake() //come reagisce lo script allo start, controlla singleton
    {
        if (Instance == null) //se non esiste un'altra istanza
            Instance = this; //istanza diventa questo game object per non creare duplicati
        else
        {
            Destroy(gameObject); //altrimenti distrugge questo game object
            return;
        }

        audioSource = GetComponent<AudioSource>(); //prende la componente audiosource
        audioSource.playOnAwake = false; //play allo start false per non far partire tutti i soni contemporaneamente
    }

   
    public void AddNumber(int num) //aggiungere un numero alla stringa corrente
    {
        currentNumber.Append(num); //aggiunge il numero allo string builder
        Debug.Log("Numero attuale: " + currentNumber.ToString());
    }

    public string GetCurrentNumber() //variabile per prendere il nuero corrente e convertirlo in string
    {
        return currentNumber.ToString();
    }
   
    public void ConfirmNumber() //arrivo della conferma 
    {
        string numero = currentNumber.ToString(); //conversione del numero arrivato in stringa
        Debug.Log("Numero confermato: " + numero);

        if (numero == "1111")
            PlayGhostbusterSbagliato();           //numeri salvati
        else if (numero == "1211")
            PlayNumero1();
        else if (numero == "1221")
            PlayNumero2();
        else
            PlayWrong();

        ClearCurrentNumber(); //cancellazione del numero ricevuto
    }
    public void ClearCurrentNumber() //variabile per cancellare il numero digitato dopo la conferma
    {
        currentNumber.Clear();
    }
    private void PlayGhostbusterSbagliato()  //hai sbagliato ghostbuster
    {
        if (ghostbusterSbagliato != null)
            audioSource.PlayOneShot(ghostbusterSbagliato);
        else
            Debug.LogWarning("ghostbusterSbagliato NON assegnato!");
    }

    private void PlayNumero1() //azzeccato 
    {
        if (numero1 != null)
            audioSource.PlayOneShot(numero1);
        else
            Debug.LogWarning("numero1 NON assegnato!");
    }

    private void PlayNumero2() //azzeccato
    {
        if (numero2 != null)  
            audioSource.PlayOneShot(numero2);
        else
            Debug.LogWarning("numero2 NON assegnato!");
    }

    private void PlayWrong() //numero inesistente
    {
        if (wrong != null)
            audioSource.PlayOneShot(wrong);
        else
            Debug.LogWarning("wrong NON assegnato!");
    }



  
}