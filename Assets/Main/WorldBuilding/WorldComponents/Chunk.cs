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
    // Flattened Array are much quicker to loop through
    // Flat[x+WIDTH*(y+ DEPTH*z)] = Original[x,y,z]
    public MeshUtils.BlocType[] chunkData;
    // We need to know blocks type before we draw them
    void BuildChunk()
    {
        int blockCount = width * depth * height;
        // An array of Type BlocType with length equal to the blockCount 
        chunkData = new MeshUtils.BlocType[blockCount];
        for (int i = 0; i < blockCount; i++)
        {
            chunkData[i] = MeshUtils.BlocType.DIRT;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        // We can add mesh filter programtically 
        MeshFilter mf = this.gameObject.AddComponent<MeshFilter>();
        // We can add mesh renderer programtically
        MeshRenderer mr = this.gameObject.AddComponent<MeshRenderer>();
        mr.material = atlas;
        // Vector3 pos = transform.position;
        blocks = new Block[width, height, depth];
        // Populate flat array chunkData that is holding block types
        BuildChunk();

        // Meshes will contain the visible parts
        var inputMeshes = new List<Mesh>();
        // Counters for the vertices and triangles
        int vertexStart = 0;
        int triStart = 0;
        // total block (and maximum possible mesh) count
        int meshCount = width * height * depth;
        // mesh counter
        int m = 0;

        // link to our job
        var jobs = new ProcessMeshDataJob();
        // Allocator temp is for temporary allocated data structure
        // Uninitialized Memory is more optimized for writing 
        jobs.vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        jobs.triStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);


        // Create blocks for the chunk array
        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get the block type from chunkData array
                    blocks[x, y, z] = new Block(new Vector3(x, y, z), chunkData[x + width * y + z * height * width], this);

                    // if block actually has a visible mesh
                    if (blocks[x, y, z].mesh != null)
                    {
                        inputMeshes.Add(blocks[x, y, z].mesh);
                    }
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
        jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        var outputMeshData = Mesh.AllocateWritableMeshData(1);
        jobs.outputMesh = outputMeshData[0];
        // Triangles are index
        jobs.outputMesh.SetIndexBufferParams(triStart, IndexFormat.UInt32);
        // Vertices , Normals ,Uvs data
        // Links the information to streams down in execute
        jobs.outputMesh.SetVertexBufferParams(vertexStart,
                        new VertexAttributeDescriptor(VertexAttribute.Position),
                        new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1),
                        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2)
                        );

        // Create the parallel job (4 jobs in parallel)
        var handle = jobs.Schedule(inputMeshes.Count, 4);
        // This mesh will go on to the chunk and hold everything
        var newMesh = new Mesh();
        newMesh.name = "Chunk";
        // This is the format mesh which will be in Triangles
        var sm = new SubMeshDescriptor(0, triStart, MeshTopology.Triangles);
        sm.firstVertex = 0;
        sm.vertexCount = vertexStart;
        // Wait for the job to complete
        handle.Complete();
        // 
        jobs.outputMesh.subMeshCount = 1;
        // sm is the mesh description
        jobs.outputMesh.SetSubMesh(0, sm);
        // Write the output to the new mesh and dispose the original
        Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new Mesh[] { newMesh });
        // Get rid of Native Arrays
        jobs.meshData.Dispose();
        jobs.vertexStart.Dispose();
        jobs.triStart.Dispose();
        // Help us recalculate colliders
        newMesh.RecalculateBounds();

        mf.mesh = newMesh;
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
            // vertexStart.Dispose();
            // triStart.Dispose();

            var tStart = triStart[index];
            var tCount = data.GetSubMesh(0).indexCount; // counts of triangles match index apparently
            var outputTris = outputMesh.GetIndexData<int>(); //
            // Some systems represent the data of a mesh in short(int16) some int32
            if (data.indexFormat == IndexFormat.UInt16)
            {
                // UInt16s are also known as unsigned shorts
                var tris = data.GetIndexData<ushort>();
                for (int i = 0; i < tCount; ++i)
                {
                    // get the index for the triangle
                    int idx = tris[i];
                    // 
                    outputTris[i + tStart] = vStart + idx;

                }
            }
            else
            {
                // Regular int for index data 
                var tris = data.GetIndexData<int>();
                for (int i = 0; i < tCount; ++i)
                {
                    // get the index for the triangle
                    int idx = tris[i];
                    // 
                    outputTris[i + tStart] = vStart + idx;

                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
