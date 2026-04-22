using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class PlayerRespawn : MonoBehaviour
{
    public static PlayerRespawn Instance;

    [Header("Spawn")]
    [Tooltip("Drag the empty GameObject that marks the spawn point here.")]
    public Transform spawnPoint;

    [Tooltip("Seconds of invincibility after respawn.")]
    public float invincibilityDuration = 3f;

    [Header("Bloom Effect")]
    [Tooltip("Drag the global post-processing Volume here.")]
    public Volume postProcessVolume;

    [Tooltip("Intensity of bloom during invincibility.")]
    public float bloomIntensityOn = 20f;

    [Header("Depth of Field Effect")]
    [Tooltip("Transition duration for depth of field effect.")]
    public float dofTransitionDuration = 1f;
    [Range(0.01f, 1000f)]
    public float focusDistance = 5f;
    [Range(0.01f, 32f)]
    public float aperture = 8f;

    public bool IsInvincible { get; private set; } = false;

    private CharacterController cc;
    private Bloom bloomEffect;
    private DepthOfField depthOfFieldEffect;
    private float originalFocusDistance;
    private float originalAperture;

    void Awake()
    {
        Instance = this;
        cc = GetComponent<CharacterController>();

        // Retrieve the Bloom component from the Volume
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGet(out bloomEffect);

        // Retrieve the Depth of Field component from the Volume
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGet(out depthOfFieldEffect);

        // Store original values for Depth of Field
        if (depthOfFieldEffect != null)
        {
            originalFocusDistance = depthOfFieldEffect.focusDistance.value;
            originalAperture = depthOfFieldEffect.aperture.value;
        }
    }

    /// Called by GhostAI when it catches the player.
    public void Respawn()
    {
        StartCoroutine(DoRespawn());
    }

    private IEnumerator DoRespawn()
    {
        // 1. Apply penalty to the clock
        WorldClock.Instance.ApplyRespawnPenalty();

        // 2. Teleport to spawn point
        //    CharacterController is disabled before moving the transform
        cc.enabled = false;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        cc.enabled = true;

        // 3. Activate bloom + invincibility
        EnableBloom(true);
        IsInvincible = true;

        yield return new WaitForSeconds(invincibilityDuration);

        // 4. Deactivate bloom + invincibility together
        IsInvincible = false;
        EnableBloom(false);

        // Apply the depth of field effect for 5 seconds after respawn
        if (depthOfFieldEffect != null)
        {
            StartCoroutine(EnableDepthOfField(5f));
        }
    }

    public void EnableBloom(bool enable)
    {
        if (bloomEffect == null) return;
        bloomEffect.intensity.value = enable ? bloomIntensityOn : 0f;
    }

    private IEnumerator EnableDepthOfField(float duration)
    {
        // Transition to the depth of field effect
        depthOfFieldEffect.focusDistance.value = focusDistance;
        depthOfFieldEffect.aperture.value = aperture;

        yield return new WaitForSeconds(duration);

        // Transition back to original values
        depthOfFieldEffect.focusDistance.value = originalFocusDistance;
        depthOfFieldEffect.aperture.value = originalAperture;
    }
}
