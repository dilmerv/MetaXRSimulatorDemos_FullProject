using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

public class LightSaberRay : MonoBehaviour
{
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private GameObject onHitParticleEffectPrefab;
    [SerializeField] private float rayOffset = 0.25f;
    [SerializeField] private float rayMaxDistance = 0.6f;
    [SerializeField] private float rayRadius = 0.025f;
    [SerializeField] private LayerMask includedLayers;
    
    private bool collisionStarted;

    [SerializeField]
    private bool collisionAllowed;

    public UnityEvent<Vector3> OnLightSaberRayHit = new();
    
    private void Start()
    {
        grabbable.WhenPointerEventRaised += GrabbableOnWhenPointerEventRaised;
    }

    private void GrabbableOnWhenPointerEventRaised(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
        {
            collisionAllowed = true;
        }
        else if (pointerEvent.Type == PointerEventType.Unselect)
        {
            collisionAllowed = false;
        }
    }

    private void Update()
    {
        if (!collisionAllowed) return;

        if (Physics.SphereCast(transform.position + (transform.forward * rayOffset), rayRadius,
                transform.forward, out RaycastHit hitInfo, rayMaxDistance, includedLayers))
        {
            if (!collisionStarted)
            {
                SpawnParticle(hitInfo.point);
                OnLightSaberRayHit.Invoke(hitInfo.point);
            }
            collisionStarted = true;
        }
        else
            collisionStarted = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + (transform.forward * rayOffset), rayRadius);
    }

    private void SpawnParticle(Vector3 position)
    {
        Instantiate(onHitParticleEffectPrefab, position, Quaternion.identity);
        LightSaberAudioManager.Instance.PlayLightSaberSparkSound();
    }
}
