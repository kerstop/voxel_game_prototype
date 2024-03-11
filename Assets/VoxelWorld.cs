using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Voxels
{

    public class VoxelWorld : MonoBehaviour
    {
        public Vector3Int worldSize;
        public GameObject playerPrefab;

        Voxel[,,] voxels;
        public Vector3Int? highlightedVoxel;
        Transform highlightMesh;

        Mesh my_mesh;

        static List<Material> materials;

        Voxel GetVoxel(Vector3Int pos)
        {
            if (pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
                pos.x < worldSize.x && pos.y < worldSize.y && pos.z < worldSize.z)
            {
                return voxels[pos.x, pos.y, pos.z];
            }
            return null;
        }

        Vector3Int GetVoxelPos(Vector3 pos)
        {
            return new Vector3Int(Mathf.FloorToInt(pos.x + 0.5f), Mathf.FloorToInt(pos.y + 0.5f), Mathf.FloorToInt(pos.z + 0.5f));
        }

        private void RegenerateMesh()
        {
            DateTime t1 = DateTime.Now;
            List<Vector3> vertecies = new();
            List<int> triangles = new();
            List<Vector2> uvs = new();
            List<Vector3> normals = new();
            Quaternion[] rotations = {
            Quaternion.Euler(  0,  0,  0),
            Quaternion.Euler( 90,  0,  0),
            Quaternion.Euler( 90, 90,  0),
            Quaternion.Euler( 90,180,  0),
            Quaternion.Euler( 90,270,  0),
            Quaternion.Euler(180,  0,  0),
            };
            Vector3Int[] basis_dirs =
            {
            new Vector3Int( 0, 1, 0),
            new Vector3Int( 0, 0, 1),
            new Vector3Int( 1, 0, 0),
            new Vector3Int( 0, 0,-1),
            new Vector3Int(-1, 0, 0),
            new Vector3Int( 0,-1, 0),
            };
            Vector2[] vertexUvs = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 0.25f), new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0f)};
            Vector2[] sideUvOffsets = new Vector2[] 
            {
                new Vector2(0.25f, 0.75f),
                new Vector2(0.25f, 0.5f ),
                new Vector2(0f   , 0.5f ),
                new Vector2(0.5f , 0.5f ),
                new Vector2(0.75f, 0.5f ),
                new Vector2(0.25f, 0.25f)
            };
            foreach (Vector3Int voxelPos in GetPositionsInRange(Vector3Int.zero, worldSize - Vector3Int.one))
            {
                Voxel voxel = GetVoxel(voxelPos);
                if (voxel != null && voxel.isSolid)
                {
                    Vector3 voxelOffset = ((Vector3)voxelPos);
                    for (int side = 0; side < 6; side++)
                    {
                        Vector3Int neighborPos = voxelPos + basis_dirs[side];
                        Voxel neighbor = GetVoxel(neighborPos);
                        bool shouldDrawFace;
                        if (neighbor == null)
                        {
                            shouldDrawFace = true;
                        }
                        else if (neighbor.isSolid)
                        {
                            shouldDrawFace = false;
                        }
                        else
                        {
                            shouldDrawFace = true;
                        }
                        if (shouldDrawFace)
                        {
                            int i = vertecies.Count;
                            vertecies.Add(voxelOffset + (rotations[side] * new Vector3(0.5f, 0.5f, 0.5f)));
                            uvs.Add(vertexUvs[0] + sideUvOffsets[side]);
                            normals.Add(rotations[side] * new Vector3(1,1,1).normalized);
                            //
                            vertecies.Add(voxelOffset + (rotations[side] * new Vector3(0.5f, 0.5f, -0.5f)));
                            uvs.Add(vertexUvs[1] + sideUvOffsets[side]);
                            normals.Add(rotations[side] * new Vector3(1,1,-1).normalized);
                            //
                            vertecies.Add(voxelOffset + (rotations[side] * new Vector3(-0.5f, 0.5f, -0.5f)));
                            uvs.Add(vertexUvs[2] + sideUvOffsets[side]);
                            normals.Add(rotations[side] * new Vector3(-1,1,-1).normalized);
                            //
                            vertecies.Add(voxelOffset + (rotations[side] * new Vector3(-0.5f, 0.5f, 0.5f)));
                            uvs.Add(vertexUvs[3] + sideUvOffsets[side]);
                            normals.Add(rotations[side] * new Vector3(-1,1,1).normalized);
                            //
                            triangles.AddRange(new[] { i, i + 1, i + 2 });
                            triangles.AddRange(new[] { i + 2, i + 3, i + 0 });
                        }
                    }
                }
            }
            my_mesh.Clear();
            my_mesh.SetVertices(vertecies);
            my_mesh.SetTriangles(triangles, 0);
            my_mesh.SetNormals(normals);
            my_mesh.SetUVs(0, uvs);

            TimeSpan delta = DateTime.Now - t1;
            Debug.Log(String.Format("Mesh regenerated in {0}ms", delta.Milliseconds));
        }

        private void GenerateWorld()
        {
            foreach (Vector3Int pos in GetPositionsInRange(Vector3Int.zero, worldSize - Vector3Int.one))
            {
                Voxel v;
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.9f)
                {
                    v = new Voxel(Substance.Stone, 1.0f);
                }
                else
                {
                    v = new Voxel(Substance.Oxygen, 1.0f);
                }
                voxels[pos.x, pos.y, pos.z] = v;

            }
        }

        private void OnValidate()
        {
            if (worldSize.x < 0) { worldSize.x *= -1; }
            if (worldSize.y < 0) { worldSize.y *= -1; }
            if (worldSize.z < 0) { worldSize.z *= -1; }
        }

        // Start is called before the first frame update
        void Start()
        {
            my_mesh = new Mesh();
            voxels = new Voxel[worldSize.x,worldSize.y,worldSize.z];
            highlightMesh = transform.Find("Highlight");

            LoadMaterials();
            
            GenerateWorld();

            MeshFilter mf = GetComponent<MeshFilter>();
            mf.mesh = my_mesh;

            RegenerateMesh();

            placePlayer();
        }

        // Update is called once per frame
        void Update()
        {
            if (highlightedVoxel == null)
            {
                highlightMesh.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                highlightMesh.GetComponent<MeshRenderer>().enabled = true;
                highlightMesh.position = (Vector3)highlightedVoxel;
            }
        }

        Vector3 raysource;
        Vector3 raydir;
        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(raysource, raydir);
        }

        void placePlayer()
        {
            for (int attempt = 0; attempt < 1000; attempt++)
            {
                Vector3Int pos = new Vector3Int(
                    UnityEngine.Random.Range(0, worldSize.x),
                    UnityEngine.Random.Range(0, worldSize.y),
                    UnityEngine.Random.Range(0, worldSize.z)
                    );

                Voxel feetSpot = GetVoxel(pos);
                Voxel headSpot = GetVoxel(pos + Vector3Int.up);
                if (feetSpot == null || headSpot == null) { continue; }
                if (!feetSpot.isSolid && !headSpot.isSolid)
                {
                    GameObject player = Instantiate(playerPrefab, ((Vector3)pos) + Vector3.down * (0.5f), Quaternion.identity, transform);
                    break;
                }

            }
        }

        public bool intersectionTest(Vector3 pos)
        {
            Vector3Int voxelPos = GetVoxelPos(pos);
            Voxel voxel = GetVoxel(voxelPos);
            if (voxel == null) { return false; }
            return voxel.isSolid;

        }

        public Vector3 toVoxelSpace(Vector3 worldSpace)
        {
            Vector3 objectSpace = transform.worldToLocalMatrix * worldSpace;
            return objectSpace + (0.5f * Vector3.one);
        }

        // This range is inclusive on both ends
        public static IEnumerable<Vector3Int> GetPositionsInRange(Vector3Int p1_in, Vector3Int p2_in)
        {
            // make sure for all components p1 < p2
            Vector3Int p1 = Vector3Int.zero;
            Vector3Int p2 = Vector3Int.zero;
            for (int i = 0; i < 3; i++)
            {
                if (p1_in[i] > p2_in[i])
                {
                    p1[i] = p2_in[i];
                    p2[i] = p1_in[i];
                }
                else
                {
                    p1[i] = p1_in[i];
                    p2[i] = p2_in[i];

                }
            }
            for (int x = p1.x; x <= p2.x; x++)
            {
                for (int y = p1.y; y <= p2.y; y++)
                {
                    for (int z = p1.z; z <= p2.z; z++)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }

/*        float signedDstToVoxel(Vector3 point, Vector3Int voxel)
        {
            Vector3 offset = (point - ((Vector3)voxel - Vector3.one * 0.5f));
            for (int i = 0; i <3; i++)
            {
                if (offset[i] < 0) offset[i] *= -1;
            }
            offset -= Vector3.one * 0.5f;

            float unsignedDst = Math.Max(offset.magnitude, 0);
        }
*/
        public Vector3Int? RaySelect(Vector3 source, Vector3 _direction)
        {
            raydir = _direction;
            raysource = source;
            Vector3 testPoint = source;
            Vector3 direction = _direction.normalized;
            while ((source-testPoint).magnitude < 6)
            {
                Vector3Int currentVoxelPos = GetVoxelPos(testPoint);
                Voxel currentVoxel = GetVoxel(currentVoxelPos);
                if (currentVoxel != null && currentVoxel.isSolid)
                {
                    return currentVoxelPos;
                }
                testPoint += direction * 0.05f;
            }
            return null;

        }

        void LoadMaterials()
        {
            materials = new List<Material>();
            foreach(String s in Enum.GetNames(typeof(Substance)))
            {
                materials.Add((Material) Resources.Load(s + "Material"));
            }
        }
    }



    public class Voxel
    {
        Substance type;
        float mass;


        public Voxel(Substance type, float mass)
        {
            this.type = type;
            this.mass = mass;
        }

        public bool isSolid
        {
            get
            {
                switch (type)
                {
                    case Substance.Stone: return true;
                    case Substance.Oxygen: return false;
                    case Substance.Water: return true;
                    default: return false;
                }
            }
        }

    }

    public enum Substance : int
    {
        Stone = 0,
        Oxygen,
        Water,
        Test
    }
}
