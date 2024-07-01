using UnityEngine;

public class SliceObjectState : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody physics;
    
    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        physics = GetComponent<Rigidbody>();
    }

    public void Reactivate()
    {
        physics.isKinematic = true;
        physics.velocity = Vector3.zero;
        physics.angularVelocity = Vector3.zero;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        physics.isKinematic = false;
        gameObject.SetActive(true);
    }
}