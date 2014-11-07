using Intro3DFramework.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Sample
{
    /// <summary>
    /// Class for drawing a screen aligned quad.
    /// </summary>
    class Quad2D
    {
        /// <summary>
        /// Vertex for 2D geometry.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = sizeof(float) * 4)]
        private struct Vertex2D
        {
            [FieldOffset(0)]
            public OpenTK.Vector2 position;
            [FieldOffset(sizeof(float) * 2)]
            public OpenTK.Vector2 texcoord;
        }

        /// <summary>
        /// Index of Vertex Buffer.
        /// </summary>
        private int vertexBuffer;
        /// <summary>
        /// The texture that will be drawn to the quad.
        /// </summary>
        public Texture2D Texture { get; set; }

        public Quad2D(OpenTK.Vector2 upperLeftCorner, OpenTK.Vector2 lowerRightCorner)
        {
            // Saving vertices in array
            Vertex2D[] vertices = new Vertex2D[6];

            vertices[0].position = new OpenTK.Vector2(lowerRightCorner.X, upperLeftCorner.Y);
            vertices[1].position = upperLeftCorner;
            vertices[2].position = new OpenTK.Vector2(upperLeftCorner.X, lowerRightCorner.Y);
            vertices[5].position = lowerRightCorner;

            vertices[0].texcoord = new OpenTK.Vector2(1,0);
            vertices[1].texcoord = new OpenTK.Vector2(0,0);
            vertices[2].texcoord = new OpenTK.Vector2(0,1);
            vertices[5].texcoord = new OpenTK.Vector2(1,1);

            vertices[3] = vertices[0];
            vertices[4] = vertices[2];
            
            // Create and fill OpenGL vertex buffer.
            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 
                (IntPtr)(Marshal.SizeOf(typeof(Vertex2D)) * 6), 
                vertices, BufferUsageHint.StaticDraw);
        }

        public void Draw()
        {
            // Assert the object exists and is valid.
            System.Diagnostics.Debug.Assert(vertexBuffer > 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            // Set vertex type
            int vertexSize = Marshal.SizeOf(typeof(Vertex2D));
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, vertexSize, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 2);
            GL.EnableVertexAttribArray(1);

            // Setting the texture.
            if (Texture != null)
                GL.BindTexture(TextureTarget.Texture2D, Texture.Texture);
            else
                GL.BindTexture(TextureTarget.Texture2D, 0);

            // Finally drawing the vertex buffer.
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
        }
    }
}
