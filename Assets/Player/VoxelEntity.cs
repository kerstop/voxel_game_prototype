using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voxels;

public class VoxelEntity : MonoBehaviour
{
    public VoxelWorld world;

    Vector3 velocity = Vector3.zero;
    public Vector3 targetVelocity = Vector3.zero;

    public Vector3 offset;
    public Vector3 size;
    public Vector3 p1 { 
        get {
            return transform.TransformPoint(offset - (size / 2.0f));
        }
    }
    public Vector3 p2 { 
        get {
            return transform.TransformPoint(offset + (size / 2.0f));
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 prevPosition = transform.position;
        transform.Translate(velocity);
        velocity = Vector3.MoveTowards(velocity, targetVelocity, 0.1f);
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.TransformVector(offset), size);
    }
}


