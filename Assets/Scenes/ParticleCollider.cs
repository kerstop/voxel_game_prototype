using UnityEditor;
using UnityEngine;

public class ParticleCollider : MonoBehaviour
{

    public Transform targetPosition;
    public VoxelCollider collidingVoxel;
    public float pointRadius;
    public float initialVelocity;
    Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        velocity = (targetPosition.position - transform.position).normalized * initialVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(velocity);
        if (collidingVoxel.isIntersecting(transform.position, pointRadius))
        {
            Ray closestPoint = collidingVoxel.findClosestPoint(transform.position);
            transform.position = closestPoint.origin + closestPoint.direction * pointRadius;
            velocity = velocity - Vector3.Dot(closestPoint.direction, velocity) * closestPoint.direction * 2.0f;
        }
        
    }

    private void OnDrawGizmos()
    {
        bool collisionHappened = collidingVoxel.isIntersecting(targetPosition.position);
        Vector3 closestPoint = collidingVoxel.findClosestPoint(targetPosition.position).origin;
        Vector3 simVelocity = (targetPosition.position - transform.position).normalized;

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.position, pointRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(closestPoint, pointRadius);

        Gizmos.color = collisionHappened ? Color.red : Color.black;
        Gizmos.DrawLine(transform.position, targetPosition.position);
        Gizmos.DrawSphere(targetPosition.position, pointRadius);

        if (collisionHappened)
        {
            Vector3 collisionNormal = (closestPoint - targetPosition.position).normalized;
            Handles.Label(closestPoint, "Collision Happened");
            Handles.color = Color.blue;
            Handles.DrawLine(closestPoint, collisionNormal + closestPoint);
            Vector3 newVelocity = simVelocity - Vector3.Dot(simVelocity, collisionNormal) * collisionNormal * 2.0f;
            Handles.DrawLine(closestPoint, closestPoint + newVelocity);
        }

    }

}
