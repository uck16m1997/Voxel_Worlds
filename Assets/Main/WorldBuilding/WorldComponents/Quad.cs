using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{
    public Mesh mesh;
    public Quad(MeshUtils.BlockSide side, Vector3 offset, MeshUtils.BlocType bType)
    {
        // // We can add mesh filter programtically 
        // MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        // // We can add mesh renderer programtically
        // MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();

        // Create a new mesh 
        mesh = new Mesh();
        mesh.name = "ScriptedQuad";

        // Quad has 4 corners for vertices and 4 normals for each vertex
        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        // UV coordinates one for each vertex
        Vector2[] uvs = new Vector2[4];
        // Two polygons with 3 vertices for each
        int[] polygons = new int[6];

        Vector2 uv00 = MeshUtils.blockUVs[(int)bType, 0];
        Vector2 uv01 = MeshUtils.blockUVs[(int)bType, 1];
        Vector2 uv10 = MeshUtils.blockUVs[(int)bType, 2];
        Vector2 uv11 = MeshUtils.blockUVs[(int)bType, 3];
        Debug.Log(uv00 + "," + uv01 + "," + uv10 + "," + uv11);

        // (X , Y ,Z)
        // Positive X Left , Negative X Right
        // Positive Y Up , Negative Y Down
        // Positive Z Front , Negative Z Back

        // Cube corners are 8 vertices
        Vector3 p0 = new Vector3(-0.5f, -0.5f, 0.5f) + offset; // Front Bottom Right
        Vector3 p1 = new Vector3(0.5f, -0.5f, 0.5f) + offset; // Front Bottom Left
        Vector3 p2 = new Vector3(0.5f, -0.5f, -0.5f) + offset; // Back Bottom Left
        Vector3 p3 = new Vector3(-0.5f, -0.5f, -0.5f) + offset; // Back Bottom Right 
        Vector3 p4 = new Vector3(-0.5f, 0.5f, 0.5f) + offset; // Front Top Right
        Vector3 p5 = new Vector3(0.5f, 0.5f, 0.5f) + offset; // Front Top Left
        Vector3 p6 = new Vector3(0.5f, 0.5f, -0.5f) + offset; // Back Top Left
        Vector3 p7 = new Vector3(-0.5f, 0.5f, -0.5f) + offset; // Back Top Right

        switch (side)
        {
            case MeshUtils.BlockSide.FRONT:
                {
                    // Specific points we take all of them have positive Z which is their normal
                    vertices = new Vector3[] { p5, p4, p0, p1 };
                    normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                    //  hmm
                    uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                    // values are indexes for the vertices
                    // 3 1 0 is the polygon formed by p0,p5,p4
                    // 3 2 1 is the polygon formed by p0,p1,p5
                    // It matters that each triangle is specified clockwise to determine which side will be visible
                    polygons = new int[] { 0, 1, 2, 2, 3, 0 };
                    break;
                }
            case MeshUtils.BlockSide.BACK:
                {
                    // Specific points we take all of them have positive Z which is their normal
                    vertices = new Vector3[] { p6, p7, p3, p2 };
                    normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                    //  hmm
                    uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                    // It matters that each triangle is specified clockwise to determine which side will be visible
                    polygons = new int[] { 2, 1, 0, 0, 3, 2 };
                    break;
                }
            case MeshUtils.BlockSide.LEFT:
                {
                    // Specific points we take all of them have positive Z which is their normal
                    vertices = new Vector3[] { p6, p5, p1, p2 };
                    normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                    //  hmm
                    uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                    // It matters that each triangle is specified clockwise to determine which side will be visible
                    polygons = new int[] { 0, 1, 2, 2, 3, 0 };
                    break;
                }
            case MeshUtils.BlockSide.RIGHT:
                {
                    // Specific points we take all of them have positive Z which is their normal
                    vertices = new Vector3[] { p7, p4, p0, p3 };
                    normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                    //  hmm
                    uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                    // It matters that each triangle is specified clockwise to determine which side will be visible
                    polygons = new int[] { 2, 1, 0, 0, 3, 2 };
                    break;
                }
            case MeshUtils.BlockSide.TOP:
                {
                    // Specific points we take all of them have positive Z which is their normal
                    vertices = new Vector3[] { p6, p7, p4, p5 };
                    normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                    //  hmm
                    uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                    // It matters that each triangle is specified clockwise to determine which side will be visible
                    polygons = new int[] { 0, 1, 2, 2, 3, 0 };
                    break;
                }
            case MeshUtils.BlockSide.BOTTOM:
                {
                    // Specific points we take all of them have positive Z which is their normal
                    vertices = new Vector3[] { p2, p3, p0, p1 };
                    normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                    //  hmm
                    uvs = new Vector2[] { uv11, uv01, uv00, uv10 };
                    // It matters that each triangle is specified clockwise to determine which side will be visible
                    polygons = new int[] { 2, 1, 0, 0, 3, 2 };
                    break;
                }

        }



        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = polygons;

        mesh.RecalculateBounds();
        // mf.mesh = mesh;

    }


}
