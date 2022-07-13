using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{

    [System.Serializable]
    public enum BlockSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };
    public Material atlas;
    private MeshFilter mf;
    private Quad[] quads;
    // Start is called before the first frame update
    void Start()
    {
        // We can add mesh filter programtically 
        mf = this.gameObject.AddComponent<MeshFilter>();
        // We can add mesh renderer programtically
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        mr.material = atlas;
        Vector3 pos = transform.position;
        MeshUtils.BlocType bType = MeshUtils.BlocType.STONE;
        quads = new Quad[6];
        quads[0] = new Quad(BlockSide.BOTTOM, pos, bType);
        quads[1] = new Quad(BlockSide.TOP, pos, bType);
        quads[2] = new Quad(BlockSide.LEFT, pos, bType);
        quads[3] = new Quad(BlockSide.RIGHT, pos, bType);
        quads[4] = new Quad(BlockSide.FRONT, pos, bType);
        quads[5] = new Quad(BlockSide.BACK, pos, bType);


        Mesh[] sideMeshes = new Mesh[6];
        for (int i = 0; i < 6; i++)
        {
            sideMeshes[i] = quads[i].mesh;
        }

        mf.mesh = MeshUtils.MergeMeshes(sideMeshes);
        mf.mesh.name = "Cube_0_0_0";

    }

    // Update i s called once per frame
    void Update()
    {
        // mf.mesh = quads[0].Build(side, new Vector3(1, 1, 1));
    }
}
