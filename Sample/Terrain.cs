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
    class Terrain
    {
        /// <summary>
        /// Vertex for a terrain.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = sizeof(float) * 5)]
        private struct VertexTerrain
        {
            [FieldOffset(0)]
            public OpenTK.Vector2 position;
            [FieldOffset(sizeof(float) * 2)]
            public OpenTK.Vector2 texcoord;
            [FieldOffset(sizeof(float) * 4)]
            public float height;
        }

        /// <summary>
        /// Index of Vertex Buffer.
        /// </summary>
        private int vertexBuffer;

        /// <summary>
        /// Index of Index Buffer.
        /// </summary>
        private int indexBuffer;
        private int numIndices;

        /// <summary>
        /// The texture that will be drawn to the quad.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// A class for creating a simple quad-based Terrain.
        /// </summary>
        /// <param name="sizeX">The number of quads in x direction.</param>
        /// <param name="sizeY">The number of quads in y direction.</param>
        /// <param name="fieldSize">The edge length (in 2D) of one field.</param>
        public Terrain(int sizeX, int sizeY, float fieldSize = 1)
        {
            numIndices = sizeX * sizeY * 2 * 3;
            System.Diagnostics.Debug.Assert((ulong)(sizeX * sizeY * 2 * 3) <= uint.MaxValue);
            // Saving vertices in array
            VertexTerrain[] vertices = new VertexTerrain[(sizeX+1) * (sizeY+1)];

            for (int x = 0; x <= sizeX; ++x)
                for (int y = 0; y <= sizeY; ++y)
                {
                    float xPos = x * fieldSize - fieldSize * sizeX / 2;
                    float yPos = y * fieldSize - fieldSize * sizeY / 2;
                    // Assign position and texcoord basedon index. 
                    vertices[y*(sizeX+1) + x] = new VertexTerrain{
                        position = new OpenTK.Vector2(xPos, yPos),
                        texcoord = new OpenTK.Vector2((float)x/(sizeX), (float)y/(sizeY)),
                        // Boring function to add some height.
                        height = (float)(Math.Sin(xPos) - Math.Cos(yPos))
                    };
                }

            // Link the vertices to triangles via index buffer.
            // Creating sizeX * sizeY quads with 2 triangles at 3 vertices each.

            uint[] indices = new uint[numIndices];

            for (int x = 0; x < sizeX; ++x)
                for (int y = 0; y < sizeY; ++y)
                {
                    uint arrayIndex = (uint)(x + (y * sizeX)) * 6;
                    indices[arrayIndex + 0] = (uint)(x + 1 + (y * (sizeX + 1)));
                    indices[arrayIndex + 1] = (uint)(x + (y * (sizeX + 1)));
                    indices[arrayIndex + 2] = (uint)(x + ((y + 1) * (sizeX + 1)));
                    indices[arrayIndex + 3] = indices[arrayIndex + 0];
                    indices[arrayIndex + 4] = indices[arrayIndex + 2];
                    indices[arrayIndex + 5] = (uint)(x + 1 + ((y + 1) * (sizeX + 1)));
                }

            // Create and fill OpenGL vertex buffer.
            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Marshal.SizeOf(typeof(VertexTerrain)) * (sizeX + 1) * (sizeY + 1)), vertices, BufferUsageHint.StaticDraw);

            // Create and fill OpenGL index buffer.
            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            IntPtr tmp = (IntPtr)(Marshal.SizeOf(typeof(VertexTerrain)) * numIndices);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * numIndices), indices, BufferUsageHint.StaticDraw);
        }

        public void Draw()
        {
            // Assert the object exists and is valid.
            System.Diagnostics.Debug.Assert(vertexBuffer > 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);

            // Set vertex type
            int vertexSize = Marshal.SizeOf(typeof(VertexTerrain));
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, vertexSize, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 2);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 4);
            GL.EnableVertexAttribArray(2);

            // Setting the texture.
            if (Texture != null)
                GL.BindTexture(TextureTarget.Texture2D, Texture.Texture);
            else
                GL.BindTexture(TextureTarget.Texture2D, 0);

            // Finally drawing the vertex buffer.
            GL.DrawElements(PrimitiveType.Triangles, numIndices, DrawElementsType.UnsignedInt, 0);
        }
    }
}
