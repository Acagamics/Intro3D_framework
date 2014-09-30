using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Intro3DFramework.Rendering
{
    /// <summary>
    /// Primitive font renderer. CPU rendering to texture, which then can be rendered as quad somewhere on the screen.
    /// For more sophisticated font rendering refer to http://www.opentk.com/project/QuickFont
    /// </summary>
    public class FontOverlay : IDisposable
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        private int texture = -1;
        private Bitmap textBitmap;
        private Graphics gfx;
        private bool dirty = true;

        private static Shader overlayShader;

        [StructLayout(LayoutKind.Sequential)]
        private struct QuadVertex
        {
            public OpenTK.Vector2 position;
            public OpenTK.Vector2 texcoord;
        }
        private int vertexBuffer = -1;

        public FontOverlay(uint Width, uint Height)
        {
            this.Width = Width;
            this.Height = Height;

            textBitmap = new Bitmap((int)Width, (int)Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfx = Graphics.FromImage(textBitmap);

            overlayShader = Shader.GetResource(new Shader.LoadDescription(
                "#version 330\n" +
                "layout(location = 0) in vec2 inPosition;\n" +
                "layout(location = 1) in vec2 inTexcoord;\n" +
                "out vec2 Texcoord;\n" +
                "void main(void)\n" +
                "{\n" +
                  "gl_Position = vec4(inPosition, 0.0, 1.0);\n" +
                  "Texcoord = inTexcoord;\n" +
                "}\n",
                "#version 330\n" +
                "uniform sampler2D TextTexture;\n" +
                "in vec2 Texcoord;\n" +
                "out vec4 OutputColor;\n" +
                "void main()  \n" +
                "{     \n" +
                "  OutputColor = texture(TextTexture, Texcoord);\n" +
                "}\n", Shader.LoadDescription.LoadType.RAWCODE));

            // Create vertex buffer.
            vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(Marshal.SizeOf(typeof(QuadVertex)) * 4), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Create texture.
            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)Width, (int)Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
        }

        public void Clear()
        {
            Clear(Color.Transparent); // Not compile time constant, therefore not allowed as standard parameter.
        }

        public void Clear(Color clearColor)
        {
            dirty = true;
            gfx.Clear(clearColor);
        }



        public void AddText(string text, OpenTK.Vector2 position)
        {
            AddText(text, position, SystemFonts.DefaultFont); // Not compile time constant, therefore not allowed as standard parameter.
        }
        public void AddText(string text, OpenTK.Vector2 position, Font font)
        {
            AddText(text, position, font, Brushes.Black); // Not compile time constant, therefore not allowed as standard parameter.
        }

        public void AddText(string text, OpenTK.Vector2 position, Font font, Brush brush)
        {
            dirty = true;
            gfx.DrawString(text, font, brush, new PointF(position.X, position.Y));
        }

        public void Draw()
        {
            Draw(new OpenTK.Vector2(-1.0f, 1.0f), new OpenTK.Vector2(2.0f, 2.0f));
        }

        public void Draw(OpenTK.Vector2 topLeftScreenPosition, OpenTK.Vector2 screenSize)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);

            // Update texture if anything has changed.
            if (dirty)
            {
                BitmapData data = textBitmap.LockBits(new Rectangle(0, 0, textBitmap.Width, textBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)Width, (int)Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                textBitmap.UnlockBits(data);
                dirty = false;
            }

            // Update vertex buffer. Not much data, so updating it every time shouldn't hurt much.
            int vertexSize = Marshal.SizeOf(typeof(QuadVertex));
            QuadVertex[] vertices = new QuadVertex[]
            {
                new QuadVertex() { position = topLeftScreenPosition, texcoord = OpenTK.Vector2.Zero},
                new QuadVertex() { position = new OpenTK.Vector2(topLeftScreenPosition.X, topLeftScreenPosition.Y - screenSize.Y), texcoord = new OpenTK.Vector2(0.0f, 1.0f)},
                new QuadVertex() { position = new OpenTK.Vector2(topLeftScreenPosition.X + screenSize.X, topLeftScreenPosition.Y), texcoord = new OpenTK.Vector2(1.0f, 0.0f)},
                new QuadVertex() { position = new OpenTK.Vector2(topLeftScreenPosition.X + screenSize.X, topLeftScreenPosition.Y - screenSize.Y), texcoord = OpenTK.Vector2.One }
            };
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferSubData<QuadVertex>(BufferTarget.ArrayBuffer, IntPtr.Zero, new IntPtr(vertexSize * 4), vertices);

            // Set vertex format.
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, vertexSize, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexSize, sizeof(float) * 2);
            GL.EnableVertexAttribArray(1);

            // Draw!
            GL.UseProgram(overlayShader.Program);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.Disable(EnableCap.Blend);
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
                GL.DeleteTexture(texture);
                texture = -1;

                GL.DeleteBuffer(vertexBuffer);
                vertexBuffer = -1;
            }

            GC.SuppressFinalize(this); // Avoid unnecessary destructor call.
            disposed = true;
        }

        ~FontOverlay()
        {
            Dispose();
        }

        #endregion
    }
}
