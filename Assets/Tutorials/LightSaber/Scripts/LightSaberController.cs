using System.Collections;
using Oculus.Interaction;
using UnityEngine;
using Logger = LearnXR.Core.Logger;
using EzySlice;
using LearnXR.Core;

public class LightSaberController : Singleton<LightSaberController>
{
    [SerializeField] private float minimumSfxPlayThreshold = 0.5f;
    [SerializeField] private float lightSaberRayFadeDuration = 5.0f;
    [SerializeField] private Material saberRayMaterial;
    [SerializeField] private float minimumYAxisBeforeAutoReposition = 0.5f;
    [SerializeField] private float frequencyToWatchSaberPose = 0.5f;
    
    // controller interactors
    [SerializeField] private GrabInteractor leftGrabInteractor;
    [SerializeField] private GrabInteractor rightGrabInteractor;

    // light saber 
    [SerializeField] private GrabInteractable grabInteractable;
    
    private Rigidbody physics;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    // track itself to re-position in case of falling beyond limits
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Grabbable grabbable;
    
    void Start()
    {
        grabbable = grabInteractable.PointableElement as Grabbable;
        physics = GetComponentInChildren<Rigidbody>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        saberRayMaterial.SetColor(BaseColor, Color.clear);
        grabbable.WhenPointerEventRaised += GrabbableOnWhenPointerEventRaised;
        StartCoroutine(WatchSaberPose());
    }

    private void GrabbableOnWhenPointerEventRaised(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
        {
            StartCoroutine(FadeInAlpha());
            LightSaberAudioManager.Instance.PlayLightSaberStateSound(true);
        }
        else if (pointerEvent.Type == PointerEventType.Unselect)
        {
            StartCoroutine(FadeOutAlpha());
            LightSaberAudioManager.Instance.PlayLightSaberStateSound(false);
        }
    }
    
    void Update()
    {
        if (grabInteractable.SelectingInteractors.Count > 0)
        {
            PlaySaberSoundEffects();
        }
    }

    private void PlaySaberSoundEffects()
    {
        if (IsGrabbedAndMovedAtMinVelocity(leftGrabInteractor, OVRInput.Controller.LTouch)
            || IsGrabbedAndMovedAtMinVelocity(rightGrabInteractor, OVRInput.Controller.RTouch))
        {
            LightSaberAudioManager.Instance.PlayLightSaberMovementSound();
        }
    }

    private bool IsGrabbedAndMovedAtMinVelocity(GrabInteractor grabInteractor, 
        OVRInput.Controller controller)
    {
        if (grabInteractor.SelectedInteractable == grabInteractable &&
            OVRInput.GetLocalControllerVelocity(controller).magnitude >= minimumSfxPlayThreshold)
        {
            return true;
        }

        return false;
    }

   
    private IEnumerator FadeInAlpha()
    {
        float elapsedTime = 0.0f;
        Color baseColor = saberRayMaterial.color;
        while (baseColor.a < 1)
        {
            yield return new WaitForEndOfFrame(); // Wait until the end of the frame
            elapsedTime += Time.deltaTime;
            baseColor.a = Mathf.Clamp01(elapsedTime / lightSaberRayFadeDuration); // Scale the alpha based on elapsed time
            saberRayMaterial.SetColor(BaseColor, baseColor);
        }

        // Ensure the alpha is fully set to 1 at the end of the coroutine
        baseColor.a = 1;
        saberRayMaterial.SetColor(BaseColor, baseColor);
    }

    public SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null) {
        // slice the provided object using the transforms of this object
        return obj.Slice(transform.position, transform.up, crossSectionMaterial);
    }
    
    private IEnumerator FadeOutAlpha()
    {
        float elapsedTime = 0.0f;
        Color baseColor = saberRayMaterial.color;
        while (baseColor.a > 0)
        {
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            baseColor.a = Mathf.Clamp01(1 - (elapsedTime / lightSaberRayFadeDuration));
            saberRayMaterial.SetColor(BaseColor, baseColor);
        }

        baseColor.a = 0;
        saberRayMaterial.SetColor(BaseColor, baseColor);
    }

    private IEnumerator WatchSaberPose()
    {
        while (true)
        {
            yield return new WaitForSeconds(frequencyToWatchSaberPose);
            if (transform.position.y <= minimumYAxisBeforeAutoReposition &&
                grabInteractable.SelectingInteractors.Count == 0)
            {
                physics.isKinematic = true;
                physics.velocity = Vector3.zero;
                physics.angularVelocity = Vector3.zero;
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                physics.isKinematic = false;
            }
        }
    }
    
    private void OnDestroy()
    {
        grabbable.WhenPointerEventRaised -= GrabbableOnWhenPointerEventRaised;
    }
}
