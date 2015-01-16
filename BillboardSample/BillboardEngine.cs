using Intro3DFramework.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BillboardSample
{
    /// <summary>
    /// Simplistic system for rendering of multiple camera oriented billboards.
    /// billboards must be re-added every frame to keep them camera orientated!
    /// There are many possibilities to improve performance - meant for learning purpose only.
    /// 
    /// Note that the index buffer could be shared among all billboard engines (same content).
    /// The vertex buffer could also be shared among all billboard engines since it is rewriten every frame anyways.
    /// </summary>
    class BillboardEngine : IDisposable
    {
        [StructLayout(LayoutKind.Explicit, Size = sizeof(float) * 9)]
        struct BillboardVertex
        {
            [FieldOffset(0)]
            public Vector3 Position;
            [FieldOffset(sizeof(float) * 3)]
            public Vector2 Texcoord;
            [FieldOffset(sizeof(float) * 5)]
            public Vector4 Color; // 8bit per channel would be much more compact and usually sufficient, but this is easier for starters ;)
        }

        private BillboardVertex[] billboardVertices;

        public int MaxBillboardCount { private set; get; }
        public int NumBillboards { private set; get; }

        private int vertexBuffer;
        private int vertexArray;
        private int indexBuffer;

        private Vector3 camX;
        private Vector3 camY;
        private Vector3 camPos;

        private bool beginWasCalled = false;

        public BillboardEngine(int maxNumBillboards)
        {
            MaxBillboardCount = maxNumBillboards;
            NumBillboards = 0;
            billboardVertices = new BillboardVertex[MaxBillboardCount * 4];

            int vertexSize = Marshal.SizeOf(typeof(BillboardVertex));

            // Create vertex buffer. There we will store all the little quads.
            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(Marshal.SizeOf(typeof(BillboardVertex)) * billboardVertices.Length), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Create an vertex array object. This specifies the vertex format.
            vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(vertexArray);
                // Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, 0);
            GL.EnableVertexAttribArray(0);
                // Texcoord
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 3);
            GL.EnableVertexAttribArray(1);
                // Color
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 5);
            GL.EnableVertexAttribArray(2);
                // Disable vertex array again.
            GL.BindVertexArray(0); 

            // Create index buffer.
            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            uint[] indices = new uint[maxNumBillboards * 6];
            for(uint billboard=0; billboard<maxNumBillboards; ++billboard)
            {
                indices[billboard * 6 + 0] = billboard*4 + 0;
                indices[billboard * 6 + 1] = billboard*4 + 1;
                indices[billboard * 6 + 2] = billboard*4 + 2;

                indices[billboard * 6 + 3] = billboard*4 + 2;
                indices[billboard * 6 + 4] = billboard*4 + 3;
                indices[billboard * 6 + 5] = billboard*4 + 0;
            }
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(maxNumBillboards * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
        }

        /// <summary>
        /// Begin with adding new sprites.
        /// You need to re-add all sprites every time the camera changes its orientation.
        /// </summary>
        public void Begin(Camera camera)
        {
            NumBillboards = 0;
            beginWasCalled = true;

            camX = new Vector3(camera.ViewMatrix.M11, camera.ViewMatrix.M21, camera.ViewMatrix.M31);
            camY = new Vector3(camera.ViewMatrix.M12, camera.ViewMatrix.M22, camera.ViewMatrix.M32);
            camPos = camera.Position;
        }

        /// <summary>
        /// Adds a billboard and orientates it properly
        /// </summary>
        public void AddBillboard(Vector3 position, Vector4 color, float size, Vector2 texTopLeft, Vector2 texBottomRight, bool vieplaneOriented = true)
        {
            if (billboardVertices.Length == NumBillboards)
                throw new Exception("Maximum number of billboards is too small - can't add more billboards!");


            // need half size all the time
            size *= 0.5f;
            Vector3 xAxis, yAxis;
            if (vieplaneOriented)
            {
                xAxis = camX * size;
                yAxis = camY * size;
            }
            else
            {
                Vector3 zAxis = Vector3.Normalize(camPos - position);
                xAxis = Vector3.Cross(zAxis, Vector3.UnitY);
                yAxis = Vector3.Cross(xAxis, zAxis);
            }

            // computes edge positions
            int startVertexIndex = NumBillboards * 4;
            billboardVertices[startVertexIndex].Position = position - xAxis + yAxis;
            billboardVertices[startVertexIndex].Texcoord = texTopLeft;
            billboardVertices[startVertexIndex].Color = color;

            billboardVertices[startVertexIndex + 1].Position = position + xAxis + yAxis;
            billboardVertices[startVertexIndex + 1].Texcoord = new Vector2(texBottomRight.X, texTopLeft.Y);
            billboardVertices[startVertexIndex + 1].Color = color;

            billboardVertices[startVertexIndex + 2].Position = position + xAxis - yAxis;
            billboardVertices[startVertexIndex + 2].Texcoord = texBottomRight;
            billboardVertices[startVertexIndex + 2].Color = color;

            billboardVertices[startVertexIndex + 3].Position = position - xAxis - yAxis;
            billboardVertices[startVertexIndex + 3].Texcoord = new Vector2(texTopLeft.X, texBottomRight.Y);
            billboardVertices[startVertexIndex + 3].Color = color;

            ++NumBillboards;
        }

        public void End()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferSubData<BillboardVertex>(BufferTarget.ArrayBuffer, IntPtr.Zero, 
                    new IntPtr(Marshal.SizeOf(typeof(BillboardVertex)) * NumBillboards * 4), billboardVertices);

            beginWasCalled = false;
        }

        /// <summary>
        /// draws all sprites with given effect settings
        /// </summary>
        public void Draw()
        {
            if (beginWasCalled)
                throw new Exception("You need to call End() after Begin() before drawing any billboards!");

            // Disable culling!
            GL.Disable(EnableCap.CullFace);

            GL.BindVertexArray(vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);

            GL.DrawElements(PrimitiveType.Triangles, NumBillboards * 6, DrawElementsType.UnsignedInt, 0);


            // Reset states.
            GL.Enable(EnableCap.CullFace);
            GL.BindVertexArray(0);
        }

        #region Disposing

        /// <summary>
        /// Has dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Destroys vertex & index buffer.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            // Only delete buffer if there is still a context (may be already deleted on shutdown)
            if (OpenTK.Graphics.GraphicsContext.CurrentContext != null)
            {
                GL.DeleteBuffer(vertexBuffer);
            }

            GC.SuppressFinalize(this); // Avoid unnecessary destructor call.
            disposed = true;
        }

        ~BillboardEngine()
        {
            Dispose();
        }

        #endregion
    }
}
