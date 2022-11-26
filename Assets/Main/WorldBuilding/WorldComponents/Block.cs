using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block // : MonoBehaviour
{

    // [System.Serializable]
    // public enum BlockSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };
    // public Material atlas;
    // private MeshFilter mf;
    // private Quad[] quads;
    public Mesh mesh;
    // Start is called before the first frame update
    Chunk parentChunk;
    // void Start()
    public Block(Vector3 pos, MeshUtils.BlocType bType, Chunk chunk)
    {
        // Assign parent
        parentChunk = chunk;
        if (bType != MeshUtils.BlocType.AIR)
        {
            List<Quad> quads = new List<Quad>();
            // Draw BOTTOM quad if BOTTOM has no solid neighbour
            if (!HasSolidNeighbour((int)pos.x, (int)pos.y - 1, (int)pos.z))
            {
                quads.Add(new Quad(MeshUtils.BlockSide.BOTTOM, pos, bType));
            }
            // Draw TOP quad if TOP has no solid neighbour
            if (!HasSolidNeighbour((int)pos.x, (int)pos.y + 1, (int)pos.z))
            {
                quads.Add(new Quad(MeshUtils.BlockSide.TOP, pos, bType));
            }
            // Draw LEFT quad if LEFT has no solid neighbour
            if (!HasSolidNeighbour((int)pos.x + 1, (int)pos.y, (int)pos.z))
            {
                quads.Add(new Quad(MeshUtils.BlockSide.LEFT, pos, bType));
            }
            // Draw RIGHT quad if RIGHT has no solid neighbour
            if (!HasSolidNeighbour((int)pos.x - 1, (int)pos.y, (int)pos.z))
            {
                quads.Add(new Quad(MeshUtils.BlockSide.RIGHT, pos, bType));
            }
            // Draw FRONT quad if FRONT has no solid neighbour
            if (!HasSolidNeighbour((int)pos.x, (int)pos.y, (int)pos.z + 1))
            {
                quads.Add(new Quad(MeshUtils.BlockSide.FRONT, pos, bType));
            }
            // Draw BACK quad if BACK has no solid neighbour
            if (!HasSolidNeighbour((int)pos.x, (int)pos.y, (int)pos.z - 1))
            {
                quads.Add(new Quad(MeshUtils.BlockSide.BACK, pos, bType));
            }

            if (quads.Count == 0) { return; }
            int m = 0;
            Mesh[] sideMeshes = new Mesh[quads.Count];
            foreach (Quad q in quads)
            {
                sideMeshes[m] = q.mesh;
                m++;
            }

            mesh = MeshUtils.MergeMeshes(sideMeshes);
            mesh.name = "Cube_0_0_0";
        }
    }
    public bool HasSolidNeighbour(int x, int y, int z)
    {

        // Neighbour is not in this chunk
        if (x < 0 || x >= parentChunk.width || y < 0 || y >= parentChunk.height || z < 0 || z >= parentChunk.depth)
        {
            return false;
        }
        // Block is transparent
        if (parentChunk.chunkData[x + parentChunk.width * y + z * parentChunk.height * parentChunk.width] == MeshUtils.BlocType.AIR ||
            parentChunk.chunkData[x + parentChunk.width * y + z * parentChunk.height * parentChunk.width] == MeshUtils.BlocType.WATER)
        {
            return false;
        }
        return true;
    }

}
