using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intro3DFramework.ResourceSystem;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Intro3DFramework.Rendering
{
    public class Model : ResourceSystem.BaseResource<Model, Model.LoadDescription>
    {
        private int vertexBuffer = -1;
        private int indexBuffer = -1;

        private bool using32BitIndices = false;

        public bool HasTangents     { get; private set; }
        public uint NumVertices     { get; private set; }
        public uint NumTriangles    { get; private set; }

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

            public override int GetHashCode()
            {
                return filename.GetHashCode();
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
            }

            // Tangents are needed if any of the meshes has them.
            HasTangents = false;
            NumVertices = 0;
            NumTriangles = 0;
            foreach(Assimp.Mesh mesh in scene.Meshes)
            {
                if(mesh.HasTangentBasis)
                {
                    HasTangents = true;
                }
                System.Diagnostics.Debug.Assert(mesh.HasFaces && mesh.HasVertices && mesh.PrimitiveType == Assimp.PrimitiveType.Triangle);
                NumVertices += (uint)mesh.VertexCount;
                NumTriangles += (uint)mesh.FaceCount;
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
                            *(Assimp.Vector2D*)pVertex = new Assimp.Vector2D(mesh.TextureCoordinateChannels[0][meshVertexIndex].X, mesh.TextureCoordinateChannels[0][meshVertexIndex].Y);
                        }
                        pVertex -= vertexSize * mesh.VertexCount;
                    }
                    else
                        Console.Error.WriteLine("Model \"{0}\", mesh \"{1}\" has not the required texture coordinates!", description.filename, mesh.Name);
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

                    pVertex = pVertex + mesh.VertexCount * (vertexSize - 1); // Go to the next vertex. Attention the offsets summed up to one vertex!
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
                if(using32BitIndices)
                {
                    indexSize = 4;
                    pIndices = Marshal.AllocHGlobal((int)NumTriangles * indexSize * 3);
                    UInt32* pIndex = (UInt32*)pIndices;
                    foreach(Assimp.Mesh mesh in scene.Meshes)
                    {
                        for (int faceIdx = 0; faceIdx < mesh.Faces.Count; ++faceIdx)
                        {
                            *pIndex = (UInt32)mesh.Faces[faceIdx].Indices[0];
                            ++pIndex;
                            *pIndex = (UInt32)mesh.Faces[faceIdx].Indices[1];
                            ++pIndex;
                            *pIndex = (UInt32)mesh.Faces[faceIdx].Indices[2];
                            ++pIndex;
                        }
                    }
                }
                else
                {
                    indexSize = 2;
                    pIndices = Marshal.AllocHGlobal((int)NumTriangles * indexSize * 3);
                    UInt16* pIndex = (UInt16*)pIndices;
                    foreach (Assimp.Mesh mesh in scene.Meshes)
                    {
                        for (int faceIdx = 0; faceIdx < mesh.Faces.Count; ++faceIdx)
                        {
                            *pIndex = (UInt16)mesh.Faces[faceIdx].Indices[0];
                            ++pIndex;
                            *pIndex = (UInt16)mesh.Faces[faceIdx].Indices[1];
                            ++pIndex;
                            *pIndex = (UInt16)mesh.Faces[faceIdx].Indices[2];
                            ++pIndex;
                        }
                    }
                }


                indexBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexSize * NumTriangles * 3), pIndices, BufferUsageHint.StaticDraw);

                Marshal.FreeHGlobal(pIndices);
            }

        }

        public void Draw()
        {
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
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 8);
            GL.EnableVertexAttribArray(3);
            if(HasTangents)
            {
                GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 11);
                GL.EnableVertexAttribArray(4);
            }

            // TODO: shader?
            // TODO: textures?

            GL.DrawElements(PrimitiveType.TriangleStrip, (int)NumTriangles * 3, using32BitIndices ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort, 0);
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
