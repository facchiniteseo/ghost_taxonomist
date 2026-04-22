using UnityEngine;
using System.Text;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Rubrica : MonoBehaviour
{
    public static Rubrica Instance;

    [Header("Numeri salvati")]
    public AudioClip wrong;
    public AudioClip ghostbusterSbagliato;
    public AudioClip numero1;
    public AudioClip numero2;

    [Header("Durate chiamate (secondi)")]
    public float durataNumero1 = 19f;
    public float durataNumero2 = 17f;

    [Header("Fantasmi")]
    public GhostAI fantasma1;
    public GhostAI fantasma2;
    public GhostAI fantasma3;

    private AudioSource audioSource;
    private StringBuilder currentNumber = new StringBuilder();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void AddNumber(int num)
    {
        currentNumber.Append(num);
        Debug.Log("Numero attuale: " + currentNumber.ToString());
    }

    public string GetCurrentNumber()
    {
        return currentNumber.ToString();
    }

    public void ConfirmNumber()
    {
        string numero = currentNumber.ToString();
        Debug.Log("Numero confermato: " + numero);

        if (numero == "328333")
            PlayGhostbusterSbagliato();
        else if (numero == "391999")
            PlayNumero1();
        else if (numero == "370222")
            PlayNumero2();
        else
            PlayWrong();

        ClearCurrentNumber();
    }

    public void ClearCurrentNumber()
    {
        currentNumber.Clear();
    }

    private void PlayGhostbusterSbagliato()
    {
        if (ghostbusterSbagliato != null)
            audioSource.PlayOneShot(ghostbusterSbagliato);
        else
            Debug.LogWarning("ghostbusterSbagliato NON assegnato!");
    }

    private void PlayNumero1()
    {
        if (numero1 != null)
            audioSource.PlayOneShot(numero1);
        else
            Debug.LogWarning("numero1 NON assegnato!");

        List<GhostAI> lista = new List<GhostAI>();
        if (fantasma1 != null) lista.Add(fantasma1);
        if (fantasma2 != null) lista.Add(fantasma2);

        if (lista.Count > 0)
            DeathManager.Instance.AvviaSequenzaMorte(lista, durataNumero1);
    }

    private void PlayNumero2()
    {
        if (numero2 != null)
            audioSource.PlayOneShot(numero2);
        else
            Debug.LogWarning("numero2 NON assegnato!");

        List<GhostAI> lista = new List<GhostAI>();
        if (fantasma3 != null) lista.Add(fantasma3); // controlla fantasma3 direttamente

        if (lista.Count > 0)
            DeathManager.Instance.AvviaSequenzaMorte(lista, durataNumero2); // usa durataNumero2
    }

    private void PlayWrong()
    {
        if (wrong != null)
            audioSource.PlayOneShot(wrong);
        else
            Debug.LogWarning("wrong NON assegnato!");
    }
}