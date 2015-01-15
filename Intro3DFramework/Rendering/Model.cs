using System;
using Intro3DFramework.ResourceSystem;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.IO;

namespace Intro3DFramework.Rendering
{
    /// <summary>
    /// Simple model, consisting of multiple meshes.
    /// Will load textures automatically. Does not handle shaders!
    /// </summary>
    public class Model : ResourceSystem.BaseResource<Model, Model.LoadDescription>
    {
        public int VertexBuffer { get { return vertexBuffer; } }
        private int vertexBuffer = -1;

        public int IndexBuffer { get { return indexBuffer; } }
        private int indexBuffer = -1;

        public bool Using32BitIndices { get { return using32BitIndices; } }
        private bool using32BitIndices = false;

        /// <summary>
        /// True if each vertex of this model has a tangent vector.
        /// </summary>
        public bool HasTangents     { get; private set; }
        /// <summary>
        /// Total number of vertices in this model.
        /// </summary>
        public uint NumVertices     { get; private set; }
        /// <summary>
        /// Total number of triangles in this model.
        /// </summary>
        public uint NumTriangles    { get; private set; }

        /// <summary>
        /// Meshes act as submeshes.
        /// </summary>
        public struct Mesh
        {
            /// <summary>
            /// At which index in the index buffer this mesh starts.
            /// </summary>
            public int startIndex;
            /// <summary>
            /// How many indices the mesh contains.
            /// </summary>
            public int numIndices;

            /// <summary>
            /// null or texture from material.TextureDiffuse.FilePath.
            /// </summary>
            public Texture2D texture;
            public Assimp.Material material;
        }

        /// <summary>
        /// List of meshes, this model consists of.
        /// </summary>
        public Mesh[] Meshes { get; private set; }

        #region Vertex Definitions

        /// <summary>
        /// Default vertex consisting of the most usual vertex data.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = sizeof(float) * 8)]
        private struct Vertex
        {
            [FieldOffset(0)]
            private OpenTK.Vector3 position;
            [FieldOffset(sizeof(float) * 3)]
            private OpenTK.Vector3 normal;
            [FieldOffset(sizeof(float) * 6)]
            private OpenTK.Vector2 texcoord;
        }

        /// <summary>
        /// Vertex with tangent (useful for bumpmapping).
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = sizeof(float) * 11)]
        private struct VertexWithTangent
        {
            [FieldOffset(0)]
            private OpenTK.Vector3 position;
            [FieldOffset(sizeof(float) * 3)]
            private OpenTK.Vector3 normal;
            [FieldOffset(sizeof(float) * 6)]
            private OpenTK.Vector2 texcoord;
            [FieldOffset(sizeof(float) * 8)]
            private OpenTK.Vector3 tangent;
        }

        #endregion


        /// <summary>
        /// Resource description to load a model via Assimp from file.
        /// </summary>
        /// <see cref="Load"/>
        /// <see cref="GetResource"/>
        public struct LoadDescription : ResourceSystem.IResourceDescription
        {
            public static implicit operator LoadDescription(string filename)
            {
                return new LoadDescription(filename);
            }

            public LoadDescription(string filename)
            {
                this.filename = filename;
                this.assimpPostprocessSteps = Assimp.PostProcessSteps.Triangulate |
                                              Assimp.PostProcessSteps.GenerateNormals |
                                              Assimp.PostProcessSteps.OptimizeMeshes |
                                              Assimp.PostProcessSteps.JoinIdenticalVertices |
                                              Assimp.PostProcessSteps.ImproveCacheLocality;
            }

            public override bool Equals(object other)
            {
                LoadDescription? otherDesc = other as LoadDescription?;
                return otherDesc.HasValue &&
                       Path.GetFullPath(filename) == Path.GetFullPath(otherDesc.Value.filename);
            }

            public override int GetHashCode()
            {
                return Path.GetFullPath(filename).GetHashCode();
            }

            public string filename;

            public Assimp.PostProcessSteps assimpPostprocessSteps;
        }

        /// <summary>
        /// Empty constructor, only for creation from file via ResourceManager
        /// </summary>
        public Model()
        {
        }

        /// <summary>
        /// Loads a model from file via assimp.
        /// </summary>
        /// <param name="description">Description object describing where to find the model and how to treat it.</param>
        internal override void Load(Model.LoadDescription description)
        {
            string modelDirectory = Path.GetDirectoryName(description.filename);
            modelDirectory = modelDirectory.Replace('\\', '/'); // For our Unix friends :)

            Assimp.Scene scene;
            using (Assimp.AssimpContext importer = new Assimp.AssimpContext())
            {
                try
                {
                    scene = importer.ImportFile(description.filename, description.assimpPostprocessSteps);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    throw new ResourceException(ResourceException.Type.NOT_FOUND, "Model file \"" + description.filename + "\" was not found!", e);
                }
                catch (Assimp.AssimpException e)
                {
                    throw new ResourceException(ResourceException.Type.LOAD_ERROR, "Error during model loading via Assimp (see inner exception)!", e);
                }
                catch (System.ObjectDisposedException e)
                {
                    throw new ResourceException(ResourceException.Type.LOAD_ERROR, "Invalid Assimp context!", e);
                }
                catch (System.Exception e)
                {
                    throw new ResourceException(ResourceException.Type.LOAD_ERROR, "Unknown error during loading file \"" + description.filename + " via Assimp!", e);
                }
                if(scene == null)
                {
                    throw new ResourceException(ResourceException.Type.LOAD_ERROR, "Unknown error during loading file \"" + description.filename + " via Assimp!");
                }
            }
            
            // Tangents are needed if any of the meshes has them.
            // Fill mesh list simultaneously.
            Meshes = new Mesh[scene.MeshCount];
            HasTangents = false;
            NumVertices = 0;
            NumTriangles = 0;
            for (int meshIdx = 0; meshIdx < Meshes.Length; ++meshIdx)
            {
                System.Diagnostics.Debug.Assert(scene.Meshes[meshIdx].HasFaces && scene.Meshes[meshIdx].HasVertices && scene.Meshes[meshIdx].PrimitiveType == Assimp.PrimitiveType.Triangle);
                if(scene.Meshes[meshIdx].HasTangentBasis)
                {
                    HasTangents = true;
                }

                Meshes[meshIdx].startIndex = (int)NumTriangles * 3;
                Meshes[meshIdx].numIndices = scene.Meshes[meshIdx].FaceCount * 3;
                Meshes[meshIdx].material = scene.Materials[scene.Meshes[meshIdx].MaterialIndex];
                if (Meshes[meshIdx].material.TextureDiffuse.FilePath != null)
                {
                    try
                    {
                        string texturePath = Path.Combine(modelDirectory, Meshes[meshIdx].material.TextureDiffuse.FilePath);
                        texturePath = texturePath.Replace('\\', '/'); // For our Unix friends :)
                        Meshes[meshIdx].texture = Texture2D.GetResource(texturePath);
                    }
                    catch (ResourceException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }
                }

                NumVertices += (uint)scene.Meshes[meshIdx].VertexCount;
                NumTriangles += (uint)scene.Meshes[meshIdx].FaceCount;
            }

            // Create and fill vertex buffer.
            // Working with raw bytes makes it easier to handle the different types and the underlying raw memory.
            unsafe
            {
                // Allocate temporary storage
                int vertexSize;
                if(HasTangents)
                {
                    vertexSize = Marshal.SizeOf(typeof(VertexWithTangent));
                }
                else
                {
                    vertexSize = Marshal.SizeOf(typeof(Vertex));
                }
                IntPtr pVertices = Marshal.AllocHGlobal((int)(vertexSize * NumVertices));


                // Fill temporary storage
                byte* pVertex = (byte*)pVertices;
                foreach (Assimp.Mesh mesh in scene.Meshes)
                {
                    // positions
                    for (int meshVertexIndex = 0; meshVertexIndex < mesh.VertexCount; ++meshVertexIndex, pVertex += vertexSize)
                    {
                        *(Assimp.Vector3D*)pVertex = mesh.Vertices[meshVertexIndex];
                    }
                    pVertex -= vertexSize * mesh.VertexCount;
                    pVertex += sizeof(float) * 3; // sizeof position

                    // normals
                    if(mesh.HasNormals)
                    {
                        for (int meshVertexIndex = 0; meshVertexIndex < mesh.VertexCount; ++meshVertexIndex, pVertex += vertexSize)
                        {
                            *(Assimp.Vector3D*)pVertex = mesh.Normals[meshVertexIndex];
                        }
                        pVertex -= vertexSize * mesh.VertexCount;
                    }
                    else
                        Console.Error.WriteLine("Model \"{0}\", mesh \"{1}\" has not the required normals!", description.filename, mesh.Name);
                    pVertex += sizeof(float) * 3; // sizeof normal

                    // texcoords
                    if (mesh.TextureCoordinateChannelCount > 0)
                    {
                        for (int meshVertexIndex = 0; meshVertexIndex < mesh.VertexCount; ++meshVertexIndex, pVertex += vertexSize)
                        {
                            *(Assimp.Vector2D*)pVertex = new Assimp.Vector2D(mesh.TextureCoordinateChannels[0][meshVertexIndex].X, 
                                                                            1.0f - mesh.TextureCoordinateChannels[0][meshVertexIndex].Y); // OpenGL texture coordinate system needs flipping.
                        }
                        pVertex -= vertexSize * mesh.VertexCount;
                    }
                    else
                        Console.Error.WriteLine("Warning: Model \"{0}\", mesh \"{1}\" has no texture coordinates!", description.filename, mesh.Name);
                    pVertex += sizeof(float) * 2; // sizeof texcoord

                    // tangents
                    if(HasTangents)
                    {
                        if (mesh.HasTangentBasis)
                        {
                            for (int meshVertexIndex = 0; meshVertexIndex < mesh.VertexCount; ++meshVertexIndex, pVertex += vertexSize)
                            {
                                *(Assimp.Vector3D*)pVertex = mesh.Tangents[meshVertexIndex];
                            }
                            pVertex -= vertexSize * mesh.VertexCount;
                        }
                        else
                            Console.Error.WriteLine("Model \"{0}\", mesh \"{1}\" has not the required tangents!", description.filename, mesh.Name);
                        pVertex += sizeof(float) * 3; // sizeof tangent
                    }

                    pVertex = pVertex + (mesh.VertexCount - 1) * vertexSize; // Go to the next vertex. Attention the offsets summed up to one vertex!
                }

                // Create and fill OpenGL vertex buffer.
                vertexBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexSize * NumVertices), pVertices, BufferUsageHint.StaticDraw);

                // Free temporary data!
                Marshal.FreeHGlobal(pVertices);
            }

            // Create and fill index buffer.
            using32BitIndices = NumVertices > UInt16.MaxValue;
            unsafe
            {
                // Allocate temporary storage
                IntPtr pIndices;
                int indexSize = 0;
                if(using32BitIndices) // Any idea how to avoid this code duplication? Generics have no arithmetic type restriction possibility!
                {
                    indexSize = 4;
                    pIndices = Marshal.AllocHGlobal((int)NumTriangles * indexSize * 3);
                    UInt32* pIndex = (UInt32*)pIndices;
                    int baseIndex = 0;
                    foreach(Assimp.Mesh mesh in scene.Meshes)
                    {
                        for (int faceIdx = 0; faceIdx < mesh.Faces.Count; ++faceIdx)
                        {
                            *pIndex = (UInt32)(mesh.Faces[faceIdx].Indices[0] + baseIndex);
                            ++pIndex;
                            *pIndex = (UInt32)(mesh.Faces[faceIdx].Indices[1] + baseIndex);
                            ++pIndex;
                            *pIndex = (UInt32)(mesh.Faces[faceIdx].Indices[2] + baseIndex);
                            ++pIndex;
                        }
                        baseIndex += mesh.VertexCount;
                    }
                }
                else
                {
                    indexSize = 2;
                    pIndices = Marshal.AllocHGlobal((int)NumTriangles * indexSize * 3);
                    UInt16* pIndex = (UInt16*)pIndices;
                    int baseIndex = 0;
                    foreach (Assimp.Mesh mesh in scene.Meshes)
                    {
                        for (int faceIdx = 0; faceIdx < mesh.Faces.Count; ++faceIdx)
                        {
                            *pIndex = (UInt16)(mesh.Faces[faceIdx].Indices[0] + baseIndex);
                            ++pIndex;
                            *pIndex = (UInt16)(mesh.Faces[faceIdx].Indices[1] + baseIndex);
                            ++pIndex;
                            *pIndex = (UInt16)(mesh.Faces[faceIdx].Indices[2] + baseIndex);
                            ++pIndex;
                        }
                        baseIndex += mesh.VertexCount;
                    }
                }

                indexBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexSize * NumTriangles * 3), pIndices, BufferUsageHint.StaticDraw);

                Marshal.FreeHGlobal(pIndices);
            }
        }

        /// <summary>
        /// Callback for mesh rendering
        /// </summary>
        /// <param name="currentMesh">Mesh that is about to be rendered.</param>
        public delegate void OnMeshRenderCallback(ref Mesh currentMesh);

        public void Draw(OnMeshRenderCallback onRenderMesh = null)
        {
            // Assert the object exists and is valid.
            System.Diagnostics.Debug.Assert(!disposed && vertexBuffer > 0 && indexBuffer > 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);


            // Set vertex type
            int vertexSize = Marshal.SizeOf(HasTangents ? typeof(VertexWithTangent) : typeof(Vertex));
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 3);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 6);
            GL.EnableVertexAttribArray(2);
            if (HasTangents)
            {
                GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 8);
                GL.EnableVertexAttribArray(3);
            }

            for (int meshIdx = 0; meshIdx < Meshes.Length; ++meshIdx)
            {
                if (Meshes[meshIdx].texture != null)
                    GL.BindTexture(TextureTarget.Texture2D, Meshes[meshIdx].texture.Texture);
                else
                    GL.BindTexture(TextureTarget.Texture2D, 0);

                if (onRenderMesh != null)
                    onRenderMesh(ref Meshes[meshIdx]);
                
                if (using32BitIndices)
                    GL.DrawElements(PrimitiveType.Triangles, Meshes[meshIdx].numIndices, DrawElementsType.UnsignedInt, Meshes[meshIdx].startIndex * 4);
                else
                    GL.DrawElements(PrimitiveType.Triangles, Meshes[meshIdx].numIndices, DrawElementsType.UnsignedShort, Meshes[meshIdx].startIndex * 2);
            }
        }

        #region Disposing

        /// <summary>
        /// Has dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Destroys vertex & index buffer.
        /// </summary>
        public override void Dispose()
        {
            if (disposed)
                return;

            // Only delete buffer if there is still a context (may be already deleted on shutdown)
            if (OpenTK.Graphics.GraphicsContext.CurrentContext != null)
            {
                GL.DeleteBuffer(vertexBuffer);
                vertexBuffer = -1;
                GL.DeleteBuffer(indexBuffer);
                indexBuffer = -1;
            }

            GC.SuppressFinalize(this); // Avoid unnecessary destructor call.
            disposed = true;
        }

        ~Model()
        {
            Dispose();
        }

        #endregion
    }
}
