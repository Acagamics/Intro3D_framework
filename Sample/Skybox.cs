using Intro3DFramework.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Sample
{
    class Skybox
    {
        private int vertexBuffer;
        private int indexBuffer;
        private TextureCube texture;
        private Shader shader;

        public Skybox(TextureCube.LoadDescription cubemapDesc, Shader.LoadDescription shaderDesc)
        {
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, -1.0f)
            };

            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Marshal.SizeOf(typeof(Vector3)) * vertices.Length), vertices, BufferUsageHint.StaticDraw);

            short[] indices = new short[]
                       {7, 3, 0,   4, 7, 0,		// front
						5, 1, 2,   6, 5, 2,		// back
						4, 0, 1,   5, 4, 1,		// left
						6, 2, 3,   7, 6, 3,		// right
						2, 1, 0,   3, 2, 0,		// top
						4, 5, 6,   7, 4, 6};	// down
            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * indices.Length), indices, BufferUsageHint.StaticDraw);


            texture = TextureCube.GetResource(cubemapDesc);
            shader = Shader.GetResource(shaderDesc);

            // On some drivers its needed to activate this flag, otherwise there may be artifacts across different cubemaps faces.
            GL.Enable(EnableCap.TextureCubeMapSeamless);
        }

        public void Draw()
        {
            // Assert the object exists and is valid.
            System.Diagnostics.Debug.Assert(vertexBuffer > 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);

            // Set vertex type
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float)*3, 0);
            GL.EnableVertexAttribArray(0);

            // Setting the texture.
            if (texture != null)
                GL.BindTexture(TextureTarget.TextureCubeMap, texture.Texture);
            else
                GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            // Disable culling and depth write.
            GL.Disable(EnableCap.CullFace);
            GL.DepthMask(false);

            // Setup shader
            GL.UseProgram(shader.Program);

            // Finally draw.
            GL.DrawElements(PrimitiveType.Triangles, 2 * 3 * 6, DrawElementsType.UnsignedShort, 0);
        
            // Reenable culling and depth write.
            GL.Enable(EnableCap.CullFace);
            GL.DepthMask(true);
        }
    }
}
