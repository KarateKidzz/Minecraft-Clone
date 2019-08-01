using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CubeSpawner : MonoBehaviour
{
    public int xSize = 256;
    public int zSize = 256;
    [Range(1, 10)]
    public int heightModifier = 10;

    public GameObject DirtBlock;

    ThreeDimensionalArray<GameObject> Blocks;

    static readonly Vector3[] Corners = {
        new Vector3(-0.5f, -0.5f, -0.5f),   //0
        new Vector3(0.5f, -0.5f, -0.5f),    //1
        new Vector3(0.5f, 0.5f, -0.5f),     //2
        new Vector3(-0.5f, 0.5f, -0.5f),    //3
        new Vector3(-0.5f, 0.5f, 0.5f),     //4
        new Vector3(0.5f, 0.5f, 0.5f),      //5
        new Vector3(0.5f, -0.5f, 0.5f),     //6
        new Vector3(-0.5f, -0.5f, 0.5f)     //7
    };

    void Start()
    {
        Blocks = new ThreeDimensionalArray<GameObject>(xSize, heightModifier, zSize);

        for (float x = 0; x < xSize; x++)
        {
            for (float z = 0; z < zSize; z++)
            {
                float noiseX = x / xSize;
                float noiseZ = z / zSize;

                float noise = Mathf.PerlinNoise(noiseX, noiseZ);
                int h = (int)(noise * heightModifier);
                if (h == 0)
                {
                    h = 1;
                }
                for (int i = 0; i < h; i++)
                {
                    GameObject instantiated = Instantiate(DirtBlock, new Vector3(x, i, z), Quaternion.identity, transform);
                    Blocks[(int)x, h - 1, (int)z] = instantiated;
                }
            }
        }

        var meshFilters = GetComponentsInChildren<MeshFilter>();
        var combine = new CombineInstance[meshFilters.Length - 1];
        int index = 0;
        for (int a = 0; a < meshFilters.Length; a++)
        {
            if (meshFilters[a].sharedMesh == null) continue;
            combine[index].mesh = meshFilters[a].sharedMesh;
            combine[index++].transform = meshFilters[a].transform.localToWorldMatrix;
            meshFilters[a].gameObject.SetActive(false);
        }

        GetComponent<MeshFilter>().mesh = new Mesh();
        GetComponent<MeshFilter>().mesh.CombineMeshes(combine);


        //MeshFilter baseMeshFilter = gameObject.AddComponent<MeshFilter>();
        //Mesh newMesh = new Mesh();
        //newMesh.Clear();
        //baseMeshFilter.mesh = newMesh;
        //List<Vector3> vertices = new List<Vector3>();

        //// We loop and check if any block is to our left
        //// If there isn't, and our own block isn't null, we add a bottom back left corner
        //// If we are the last block in the 1D array, we add our bottom front left corner

        //for (int x = 0; x < Blocks.GetLength(0); x++)
        //{
        //    for (int y = 0; y < Blocks.GetLength(1); y++)
        //    {
        //        for (int z = 0; z < Blocks.GetLength(2); z++)
        //        {
        //            if (Blocks.ElementExists(x, y, z))
        //            {
        //                for (int i = 0; i < Corners.Length; i++)
        //                {
        //                    Vector3 vertex = new Vector3(x, y, z) + Corners[i];
        //                    vertices.Add(vertex);
        //                }
        //            }
        //        }
        //    }
        //}

        //newMesh.vertices = vertices.ToArray();

        //int[] triangles = {
        //    0, 2, 1, //face front
        //    0, 3, 2,
        //    2, 3, 4, //face top
        //    2, 4, 5,
        //    1, 2, 5, //face right
        //    1, 5, 6,
        //    0, 7, 4, //face left
        //    0, 4, 3,
        //    5, 4, 7, //face back
        //    5, 7, 6,
        //    0, 6, 7, //face bottom
        //    0, 1, 6
        //};

        //int[] plusTris = new int[triangles.Length * 2];
        //for (int i = 0; i < plusTris.Length; i++)
        //{
        //    if (i < triangles.Length)
        //        plusTris[i] = triangles[i];
        //    else
        //        plusTris[i] = triangles[i / 2] + 8;
        //}


        //newMesh.triangles = plusTris;


        //newMesh.Optimize();
        //newMesh.RecalculateNormals();
    }
}
