using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionController : MonoBehaviour
{
    public Vector3 size;
    public List<VoxelRigidBody> rigidBodies = new List<VoxelRigidBody>();
    public Vector3 gravity = new Vector3(0f,-9.8f,0f);

    void FixedUpdate()
    {
        foreach (VoxelRigidBody rb in rigidBodies) {
            rb.transform.Translate(rb.velocity);
            if (isIntersecting(rb.transform.position, rb.pointRadius)) {
                Ray closestPoint = findClosestPoint(rb.transform.position);
                rb.transform.position = closestPoint.origin + closestPoint.direction * rb.pointRadius;
                rb.velocity = ReflectVector(closestPoint.direction, rb.velocity);
            }

            // apply gravity
            //rb.velocity += gravity * Time.fixedDeltaTime;

        }
    }

    public bool isIntersecting(Vector3 point)
    {
        Vector3 pointTransformed = point - transform.position;
        Vector3 boxMin = -(size * 0.5f);
        Vector3 boxMax = (size * 0.5f);

        return
            pointTransformed.x >= boxMin.x && pointTransformed.x <= boxMax.x &&
            pointTransformed.y >= boxMin.y && pointTransformed.y <= boxMax.y &&
            pointTransformed.z >= boxMin.z && pointTransformed.z <= boxMax.z;
    }

    public bool isIntersecting(Vector3 center, float radius)
    {
        return (findClosestPoint(center).origin - center).magnitude < radius;
    }

    /// <summary>
    /// find the closest point on the surface of the AABB 
    /// </summary>
    /// <param name="srcPoint"></param>
    /// <returns>a ray representing the point and its normal</returns>
    public Ray findClosestPoint(Vector3 srcPoint)
    {
        Vector3 boxRadius = size * 0.5f;
        Vector3 srcPointBoxSpace = srcPoint - transform.position;
        if (isIntersecting(srcPoint))
        {
            Vector3 closestSide = Vector3.positiveInfinity;
            Vector3[] pointProjectedToSides = new Vector3[6];
            for (int i = 0; i < 3; i++)
            {
                pointProjectedToSides[i * 2] = srcPointBoxSpace;
                pointProjectedToSides[i * 2][i] = size[i] * 0.5f;
                pointProjectedToSides[i * 2 + 1] = srcPointBoxSpace;
                pointProjectedToSides[i * 2 + 1][i] = size[i] * -0.5f;
            }

            foreach (Vector3 projectedPoint in pointProjectedToSides)
            {
                if ((projectedPoint - srcPointBoxSpace).magnitude < (closestSide - srcPointBoxSpace).magnitude)
                    closestSide = projectedPoint;
            }

            return new Ray(closestSide + transform.position, (closestSide - srcPoint).normalized);

        }
        else
        {
            Vector3 pointClamped = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                pointClamped[i] = Mathf.Clamp(srcPointBoxSpace[i], -boxRadius[i], boxRadius[i]);
            }

            return new Ray(pointClamped + transform.position, (srcPointBoxSpace - pointClamped).normalized);
        }
    }

    public Vector3 ReflectVector(Vector3 normal, Vector3 vec) {
        return vec - Vector3.Dot(normal.normalized, vec) * normal.normalized * 2.0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, size);

        foreach (VoxelRigidBody rb in rigidBodies) {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(findClosestPoint(rb.transform.position));

        }
    }
}

