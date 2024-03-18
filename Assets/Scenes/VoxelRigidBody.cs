using UnityEditor;
using UnityEngine;

public class VoxelRigidBody : MonoBehaviour
{

    public Transform targetPosition;
    public CollisionController collidingVoxel;
    public float pointRadius;
    public float initialVelocity;
    public Vector3 velocity;

    void OnEnable() {
        collidingVoxel.rigidBodies.Add(this);
    }

    void OnDisable() {
        collidingVoxel.rigidBodies.Remove(this);
    }

    void Start()
    {
        velocity = (targetPosition.position - transform.position).normalized * initialVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        // transform.Translate(velocity);
        // if (collidingVoxel.isIntersecting(transform.position, pointRadius))
        // {
        //     Ray closestPoint = collidingVoxel.findClosestPoint(transform.position);
        //     transform.position = closestPoint.origin + closestPoint.direction * pointRadius;
        //     velocity = velocity - Vector3.Dot(closestPoint.direction, velocity) * closestPoint.direction * 2.0f;
        // }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(collidingVoxel.findClosestPoint(transform.position));

    }

}
