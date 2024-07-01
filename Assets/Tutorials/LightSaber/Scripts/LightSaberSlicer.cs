using System.Collections;
using System.Collections.Generic;
using EzySlice;
using UnityEngine;
using Logger = LearnXR.Core.Logger;

public class LightSaberSlicer : MonoBehaviour
{
    [SerializeField] private bool debugVisualsOn = true;
    
    [SerializeField] private string targetSliceObjectLayerName = "SliceObject";
    
    [SerializeField] private Material cutMaterial;

    [SerializeField] private float explosionForce = 2.0f;
    
    [SerializeField] private float explosionRadius = 2.0f;

    [SerializeField] private float reactivateOriginalObjectsAfter = 2.0f;

    [SerializeField] private Transform startSlicePoint;
    [SerializeField] private Transform endSlicePoint;

    private Queue<SliceObjectState> inactiveObjects = new();

    // for saber velocity calculation
    private Vector3 previousSaberPosition;
    private Vector3 saberVelocity;
    
    private void Start() => StartCoroutine(MonitorInactivatedObjects());

    void Update()
    {
        CalculateSwordVelocity();
        if (!debugVisualsOn) return;
        
        // Calculate the plane normal
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, saberVelocity);
        planeNormal.Normalize();

        // Draw the start to end slice point line
        Debug.DrawLine(startSlicePoint.position, endSlicePoint.position, Color.red);

        // Draw the saber velocity vector
        Vector3 saberVelocityEndPoint = startSlicePoint.position + saberVelocity.normalized * 10;
        Debug.DrawLine(startSlicePoint.position, saberVelocityEndPoint, Color.blue);

        // Draw the plane normal vector
        Vector3 planeNormalEndPoint = startSlicePoint.position + planeNormal * 10;
        Debug.DrawLine(startSlicePoint.position, planeNormalEndPoint, Color.green);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.layer == LayerMask.NameToLayer(targetSliceObjectLayerName))
        {
            Logger.Instance.LogInfo($"Slicing object: {other.transform.name} with saber velocity: {saberVelocity}");
            SliceCube(other.transform.gameObject);
        }
    }
    
    private void SliceCube(GameObject sliceObject)
    {
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, saberVelocity);
        planeNormal.Normalize();
        SlicedHull hull = sliceObject.gameObject.Slice(endSlicePoint.position, planeNormal);
        
        if (hull != null)
        {
            CreateHullPieceAndExplode(hull, sliceObject, true);
            CreateHullPieceAndExplode(hull, sliceObject, false);
            
            inactiveObjects.Enqueue(sliceObject.GetComponent<SliceObjectState>());
        }
    }

    private void CreateHullPieceAndExplode(SlicedHull hull, GameObject originalObject, bool isTop)
    {
        var piece = isTop ? hull.CreateUpperHull(originalObject, cutMaterial) :
            hull.CreateLowerHull(originalObject, cutMaterial);
        
        var pieceCollider = piece.AddComponent<MeshCollider>();
        pieceCollider.convex = true;
        
        var physics = piece.AddComponent<Rigidbody>();
        physics.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        
        originalObject.SetActive(false);
        
        Destroy(piece, reactivateOriginalObjectsAfter);
    }

    private IEnumerator MonitorInactivatedObjects()
    {
        while (true)
        {
            // reactive based on reactivateOriginalObjectsAfter + 10% of the original time
            yield return new WaitForSeconds(reactivateOriginalObjectsAfter + (reactivateOriginalObjectsAfter * 0.1f));
            if (inactiveObjects.Count > 0)
            {
                inactiveObjects.Dequeue()
                    .Reactivate();
            }
        }
    }
    
    void CalculateSwordVelocity()
    {
        saberVelocity = (transform.position - previousSaberPosition) / Time.deltaTime;
        previousSaberPosition = transform.position;
    }
}
