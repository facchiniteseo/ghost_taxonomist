using UnityEngine;

public class BookSlot : MonoBehaviour
{
    public bool IsOccupied { get; private set; } = false;
    private InteractiveBook currentBook = null;
    private Light slotLight;

    void Start()
    {
        slotLight = GetComponent<Light>();
        if (slotLight == null)
        {
            slotLight = gameObject.AddComponent<Light>();
            slotLight.type = LightType.Point;
            slotLight.color = new Color(1f, 0.9f, 0.5f); // giallo fioco caldo
            slotLight.intensity = 0.4f;
            slotLight.range = 0.5f;
        }

        // Accesa di default (slot libero)
        slotLight.enabled = true;
    }

    public bool TryOccupy(InteractiveBook book)
    {
        if (IsOccupied) return false;
        IsOccupied = true;
        currentBook = book;

        // Spegni quando occupato
        slotLight.enabled = false;
        return true;
    }

    public void Free()
    {
        IsOccupied = false;
        currentBook = null;

        // Riaccendi quando libero
        slotLight.enabled = true;
    }
}