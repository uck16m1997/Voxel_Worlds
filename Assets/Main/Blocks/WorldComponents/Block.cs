using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block // : MonoBehaviour
{

    // [System.Serializable]
    // public enum BlockSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };
    // public Material atlas;
    // private MeshFilter mf;
    private Quad[] quads;
    public Mesh mesh;
    // Start is called before the first frame update

    // void Start()
    public Block(Vector3 pos, MeshUtils.BlocType bType)
    {
        // // We can add mesh filter programtically 
        // mf = this.gameObject.AddComponent<MeshFilter>();
        // // We can add mesh renderer programtically
        // MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        // mr.material = atlas;
        // Vector3 pos = transform.position;
        // Vector3 pos = new Vector3(0, 0, 0);

        // MeshUtils.BlocType bType = MeshUtils.BlocType.STONE;
        quads = new Quad[6];
        quads[0] = new Quad(MeshUtils.BlockSide.BOTTOM, pos, bType);
        quads[1] = new Quad(MeshUtils.BlockSide.TOP, pos, bType);
        quads[2] = new Quad(MeshUtils.BlockSide.LEFT, pos, bType);
        quads[3] = new Quad(MeshUtils.BlockSide.RIGHT, pos, bType);
        quads[4] = new Quad(MeshUtils.BlockSide.FRONT, pos, bType);
        quads[5] = new Quad(MeshUtils.BlockSide.BACK, pos, bType);


        Mesh[] sideMeshes = new Mesh[6];
        for (int i = 0; i < 6; i++)
        {
            sideMeshes[i] = quads[i].mesh;
        }

        /*mf.*/
        mesh = MeshUtils.MergeMeshes(sideMeshes);
        /*mf.*/
        mesh.name = "Cube_0_0_0";

    }

}
