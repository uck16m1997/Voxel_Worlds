using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Material atlas;
    public int width = 2;
    public int height = 2;
    public int depth = 2;
    public Block[,,] blocks;
    // Start is called before the first frame update
    void Start()
    {
        // We can add mesh filter programtically 
        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        // We can add mesh renderer programtically
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        mr.material = atlas;
        Vector3 pos = transform.position;
        blocks = new Block[width, height, depth];

        // We will have a mesh for each block
        var inputMeshes = new List<Mesh>(width * height * depth);
        // Counters for the vertices and triangles
        int vertexStart = 0;
        int triStart = 0;
        // total mesh (and block) count
        int meshCount = width * height * depth;
        // mesh counter
        int m = 0;

        // link to our job
        var jobs = new ProcessMeshDataJob();
        // Allocator temp is for temporary allocated data structure
        // Uninitialized Memory is more optimized for writing 
        jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        jobs.triStart = new NativeArray<int>(meshCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);


        // Create blocks for the chunk array
        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    blocks[x, y, z] = new Block(pos, MeshUtils.BlocType.DIRT);
                    inputMeshes.Add(blocks[x, y, z].mesh);
                    var vcount = blocks[x, y, z].mesh.vertexCount;
                    // GetIndexCount get how many triangles there will be/are in this mesh
                    // 0 is the submesh for us it is the only mesh and corresponds to block
                    var icount = (int)blocks[x, y, z].mesh.GetIndexCount(0);

                    //update the counters
                    jobs.vertexStart[m] = vertexStart;
                    jobs.triStart[m] = triStart;
                    vertexStart += vcount;
                    triStart += icount;
                    m++;
                }
            }
        }
        // We need to update the data for mf and mr to see the chunk
        // job system expects a native array or unmanaged block of memory

    }
    // Compiler directive speeds up the code
    [BurstCompile]
    struct ProcessMeshDataJob : IJobParallelFor
    {
        // MeshDataArray holds mesh data will be the incoming mesh data
        // It is read only and won't be changed inside the job
        [ReadOnly] public Mesh.MeshDataArray meshData;
        public Mesh.MeshData outputMesh;
        public NativeArray<int> vertexStart;
        public NativeArray<int> triStart;

        // Part of the job which actually does the parallel processing
        public void Execute(int index)
        {
            var data = meshData[index];
            var vCount = data.vertexCount;
            var vStart = vertexStart[index];
            // vCount length, temporary allocated unintiliazed memory native array
            var verts = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            // Gets our mesh data/indices from the mesh and put it in to the Native Array verts
            // Reinterpret takes the data indices as Vector3 and reinterprets them as floats that Native Array supports
            data.GetVertices(verts.Reinterpret<Vector3>());

            var normals = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.GetNormals(normals.Reinterpret<Vector3>());

            var uvs = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            // 0 is the channel vertexes can have multiple uv sets 0 is the first
            data.GetUVs(0, normals.Reinterpret<Vector3>());

            // placeholders for outputs different streams for each
            var outputVerts = outputMesh.GetVertexData<Vector3>();
            var outputNormals = outputMesh.GetVertexData<Vector3>(stream: 1);
            var outputUVs = outputMesh.GetVertexData<Vector3>(stream: 2);

            for (int i = 0; i < vCount; i++)
            {
                // get data from the NativeArrays
                outputVerts[i + vStart] = verts[i];
                outputNormals[i + vStart] = normals[i];
                outputUVs[i + vStart] = uvs[i];
            }
            // Clean the memory
            verts.Dispose();
            normals.Dispose();
            uvs.Dispose();

            var tStart = triStart[index];
            var tCount = data.GetSubMesh(0).indexCount; // counts of triangles match index apparently
            var outputTris = outputMesh.GetIndexData<int>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
