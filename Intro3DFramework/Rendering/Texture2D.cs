using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;


namespace Intro3DFramework.Rendering
{
    /// <summary>
    /// Simple 2D texture wrapper for loading textures from common image files.
    /// </summary>
    public class Texture2D : ResourceSystem.BaseResource<Texture2D, Texture2D.LoadDescription>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// The OpenGL texture resource.
        /// </summary>
        public int Texture { get { return texture; } }
        private int texture = -1;

        /// <summary>
        /// Load from file texture resource descriptor.
        /// </summary>
        public struct LoadDescription : ResourceSystem.IResourceDescription
        {
            /// <summary>
            /// List of supported file format endings.
            /// </summary>
            public static readonly string[] SupportedFormats = { "bmp", "gif", "exif", "jpg", "jpeg", "png", "tiff" };

            public static implicit operator LoadDescription(string filename)
            {
                return new LoadDescription(filename);
            }

            public LoadDescription(string filename, bool generateMipMaps = true)
            {
                string fileExtension = Path.GetExtension(filename);
                if(fileExtension == null)
                    throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.LOAD_ERROR, "Texture2D file has no extension");
                if(!SupportedFormats.Contains(fileExtension.ToLower().Substring(1)))
                    throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.LOAD_ERROR, "File extension \"" + fileExtension + "\" is not supported for Texture2D!");

                this.filename = filename;
                this.generateMipMaps = generateMipMaps;
            }

            public override bool Equals(object other)
            {
                LoadDescription? otherDesc = other as LoadDescription?;
                return otherDesc.HasValue &&
                       Path.GetFullPath(filename) == Path.GetFullPath(otherDesc.Value.filename) &&
                       generateMipMaps == otherDesc.Value.generateMipMaps;
            }

            public override int GetHashCode()
            {
                return Path.GetFullPath(filename).GetHashCode() * (generateMipMaps ? 2 : 1);
            }

            public string filename;
            public bool generateMipMaps;
        }

        /// <summary>
        /// Empty constructor, only for creation from file via ResourceManager
        /// </summary>
        public Texture2D()
        {
        }


        internal override void Load(Texture2D.LoadDescription description)
        {
            try
            {
                using (var bmp = new Bitmap(description.filename))
                {
                    Width = bmp.Width;
                    Height = bmp.Height;

                    BitmapData bmpData;
                    try
                    {
                        bmpData = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }
                    catch (System.Exception e)
                    {
                        throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.LOAD_ERROR, "Couldn't read from texture \"" + description.filename + "\"!", e);
                    }

                    // Create and bind texture.
                    texture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, texture);

                    // Fill with data.
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                    bmp.UnlockBits(bmpData);
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.NOT_FOUND, "Texture file \"" + description.filename + "\" was not found!", e);
            }

            // Create mip maps on demand
            if (description.generateMipMaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                // Set trilinear filter as default.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear); // Linear + MipMap Linear for reduction.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear); // Linear for enlargement.
            }
            // Use a different filter if no mipmaps are available.
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            System.Diagnostics.Debug.WriteLine("Loaded texture " + description.filename);
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
                GL.DeleteTexture(texture);
                texture = -1;
            }

            GC.SuppressFinalize(this); // Avoid unnecessary destructor call.
            disposed = true;
        }

        ~Texture2D()
        {
            Dispose();
        }

        #endregion
    }
}
