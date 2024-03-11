using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voxels;

public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    float speed = 5.0f;
    [SerializeField] 
    float lookSensitivity = 1.0f;
    public bool isIntersecting = false;
    public Vector3 voxelSpace = Vector3.zero;
    Vector3 forwardDir = new Vector3(0.0f, 0.0f, 1.0f);
    float inclination;
    bool isMouseLocked = false;
    VoxelWorld world;
    new Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        world  = GetComponentInParent<VoxelWorld>();
        camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Vector3.zero;
        Vector3 right = Vector3.Cross(Vector3.up, forwardDir);

        if (Input.GetKey("w"))
        {
            movement += forwardDir;
        }
        if (Input.GetKey("s"))
        {
            movement -= forwardDir;
        }
        if (Input.GetKey("d"))
        {
            movement += right;
        }
        if (Input.GetKey("a"))
        {
            movement -= right;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            movement += Vector3.up;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            movement -= Vector3.up;
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isMouseLocked = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isMouseLocked = true;
        }
        float h = lookSensitivity * Input.GetAxis("Mouse X");
        float v = lookSensitivity * Input.GetAxis("Mouse Y");

        if (isMouseLocked)
        {
            forwardDir = Quaternion.Euler(0, h, 0) * forwardDir;
            inclination += v;
        }

        if (inclination > 90.0f) { inclination = 90.0f; }
        if (inclination < -90.0f) { inclination = -90.0f; }

        transform.Translate(movement.normalized * speed * Time.deltaTime, Space.World);
        camera.transform.LookAt( forwardDir + camera.transform.position, Vector3.up);
        camera.transform.Rotate( Vector3.left, inclination );

        Vector3 lookDir = camera.transform.localToWorldMatrix * Vector3.forward;
        Vector3Int? selected = world.RaySelect(camera.transform.position, lookDir);
        world.highlightedVoxel = selected;

        isIntersecting = world.intersectionTest(transform.position);
        voxelSpace = world.toVoxelSpace(transform.position);
    }
}
