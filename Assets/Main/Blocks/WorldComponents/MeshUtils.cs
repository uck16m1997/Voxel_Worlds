using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// VertexData will hold the vertex vector, normal vector and the uv vector
using VertexData = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector2>;
public static class MeshUtils
{
    public enum BlockSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK }

    public enum BlocType
    {
        GRASSTOP, GRASSSIDE, DIRT, WATER, STONE, SAND
    }
    // [,] means it is an two dimensional array
    public static Vector2[,] blockUVs = {
        /*GRASSTOP*/ {new Vector2(0.125f,0.375f), new Vector2(0.1875f,0.375f),new Vector2(0.125f,0.4375f),new Vector2(0.1875f,0.4375f)},
        /*GRASSSIDE*/ {new Vector2(0.1875f,0.9375f), new Vector2(0.25f,0.9375f),new Vector2(0.1875f,1.0f),new Vector2(0.25f,1.0f)},
        /*DIRT*/ {new Vector2(0.125f,0.9375f), new Vector2(0.1875f,0.9375f),new Vector2(0.125f,1.0f),new Vector2(0.1875f,1.0f)},
        /*WATER*/ {new Vector2(0.875f,0.125f), new Vector2(0.9375f,0.125f),new Vector2(0.875f,0.1875f),new Vector2(0.9375f,0.1875f)},
        /*STONE*/ {new Vector2(0.0f,0.875f), new Vector2(0.0625f,0.875f),new Vector2(0.0f,0.9375f),new Vector2(0.0625f,0.9375f)},
        /*SAND*/ {new Vector2(0.125f,0.875f), new Vector2(0.1875f,0.875f),new Vector2(0.125f,0.9375f),new Vector2(0.1875f,0.9375f)}

    };
    public static Mesh MergeMeshes(Mesh[] meshes)
    {
        // Create a mesh to hold all the data
        Mesh mesh = new Mesh();

        // Will keep track of the order of vertices
        // Ordering is important for triangle indexes
        Dictionary<VertexData, int> pointsOrder = new Dictionary<VertexData, int>();
        // Hold the data coming in and help reorder, append and remove duplicates
        HashSet<VertexData> pointsHash = new HashSet<VertexData>();
        List<int> tris = new List<int>();
        // Tracker for the index we are currently working on
        int pIndex = 0;
        // loop through meshes
        for (int i = 0; i < meshes.Length; i++)
        {
            // If there are null meshes continue
            if (meshes[i] == null)
            {
                continue;
            }
            // loop through the vertices of the mesh
            for (int j = 0; j < meshes[i].vertices.Length; j++)
            {
                Vector3 v = meshes[i].vertices[j];
                Vector3 n = meshes[i].normals[j];
                Vector2 uv = meshes[i].uv[j];
                VertexData p = new VertexData(v, n, uv);

                //Hash is faster for looking up if it is already existing then dictionary
                if (!pointsHash.Contains(p))
                {
                    // add to dictionary and hashset
                    pointsOrder.Add(p, pIndex);
                    pointsHash.Add(p);
                    // update seen index tracker
                    pIndex++;
                }
            }

            // Get the triangles that form the mesh
            for (int t = 0; t < meshes[i].triangles.Length; t++)
            {
                // Each value contains the index value of the vertexes that form the triangle
                int triPoint = meshes[i].triangles[t];

                Vector3 v = meshes[i].vertices[triPoint];
                Vector3 n = meshes[i].normals[triPoint];
                Vector2 uv = meshes[i].uv[triPoint];
                VertexData p = new VertexData(v, n, uv);

                // If the vertex needed for the triangle is in the dictionary get its index value
                int index;
                pointsOrder.TryGetValue(p, out index);
                // Add the vertex index
                tris.Add(index);
            }
            // Delete the ith mesh 
            meshes[i] = null;
        }
        ExtractArrays(pointsOrder, mesh);
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        return mesh;
    }

    // Another class function to turn the dictionary to Vector3s
    public static void ExtractArrays(Dictionary<VertexData, int> list, Mesh mesh)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        foreach (VertexData v in list.Keys)
        {
            verts.Add(v.Item1);
            norms.Add(v.Item2);
            uvs.Add(v.Item3);
        }
        mesh.vertices = verts.ToArray();
        mesh.normals = norms.ToArray();
        mesh.uv = uvs.ToArray();
    }
}
