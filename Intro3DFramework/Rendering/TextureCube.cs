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
    public class TextureCube : ResourceSystem.BaseResource<TextureCube, TextureCube.LoadDescription>
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
        public class LoadDescription : ResourceSystem.IResourceDescription
        {
            /// <summary>
            /// List of supported file format endings.
            /// </summary>
            public static readonly string[] SupportedFormats = { "bmp", "gif", "exif", "jpg", "jpeg", "png", "tiff" };

            public LoadDescription(string[] filenames, bool generateMipMaps = true)
            {
                this.filenames = filenames;
                this.generateMipMaps = generateMipMaps;

                CheckFilenames();
            }

            public LoadDescription(string filenameXPos, string filenameXNeg, 
                                   string filenameYPos, string filenameYNeg,
                                   string filenameZPos, string filenameZNeg,
                                   bool generateMipMaps = true)
            {
                filenames = new string[6];
                filenames[0] = filenameXPos;
                filenames[1] = filenameXNeg;
                filenames[2] = filenameYPos;
                filenames[3] = filenameYNeg;
                filenames[4] = filenameZPos;
                filenames[5] = filenameZNeg;

                this.generateMipMaps = generateMipMaps;

                CheckFilenames();
            }

            private void CheckFilenames()
            {
                if(filenames.Length != 6)
                    throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.LOAD_ERROR, "TextureCube needs exactly 6 texture filenames, provided were " + filenames.Length);

                for(int i=0; i<6; ++i)
                {
                    string fileExtension = Path.GetExtension(filenames[i]);
                    if(fileExtension == null)
                        throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.LOAD_ERROR, "TextureCube texture file " + i + " has no extension");
                    if(!SupportedFormats.Contains(fileExtension.ToLower().Substring(1)))
                        throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.LOAD_ERROR, "File extension \"" + fileExtension + "\" is not supported for Texture2D!");
                }
            }

            public override bool Equals(object other)
            {
                LoadDescription otherDesc = other as LoadDescription;
                return otherDesc != null &&
                       Path.GetFullPath(filenames[0]) == Path.GetFullPath(otherDesc.filenames[0]) &&
                       Path.GetFullPath(filenames[1]) == Path.GetFullPath(otherDesc.filenames[1]) &&
                       Path.GetFullPath(filenames[2]) == Path.GetFullPath(otherDesc.filenames[2]) &&
                       Path.GetFullPath(filenames[3]) == Path.GetFullPath(otherDesc.filenames[3]) &&
                       Path.GetFullPath(filenames[4]) == Path.GetFullPath(otherDesc.filenames[4]) &&
                       Path.GetFullPath(filenames[5]) == Path.GetFullPath(otherDesc.filenames[5]) &&
                       generateMipMaps == otherDesc.generateMipMaps;
            }

            public override int GetHashCode()
            {
                return Path.GetFullPath(filenames[0]).GetHashCode() * (generateMipMaps ? 2 : 1);
            }
            
            public string[] filenames;
            public bool generateMipMaps;
        }

        /// <summary>
        /// Empty constructor, only for creation from file via ResourceManager
        /// </summary>
        public TextureCube()
        {
        }

        internal override void Load(TextureCube.LoadDescription description)
        {
            // Create and bind texture.
            texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, texture);

            // Fill with data.
            for (int i = 0; i < 6; ++i)
            {
                try
                {
                    using (var bmp = new Bitmap(description.filenames[i]))
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
                            throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.LOAD_ERROR, "Couldn't read from texture \"" + description.filenames[i] + "\"!", e);
                        }


                        // Fill with data.
                        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                        bmp.UnlockBits(bmpData);
                    }
                }
                catch (System.IO.FileNotFoundException e)
                {
                    throw new ResourceSystem.ResourceException(ResourceSystem.ResourceException.Type.NOT_FOUND, "Texture file \"" + description.filenames[i] + "\" was not found!", e);
                }

                System.Diagnostics.Debug.WriteLine("Loaded cubemap side " + i + " " + description.filenames[i]);
            }

            // Create mip maps on demand
            if (description.generateMipMaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

                // Set trilinear filter as default.
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear); // Linear + MipMap Linear for reduction.
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear); // Linear for enlargement.
            }
            // Use a different filter if no mipmaps are available.
            else
            {
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            // User usually expects clamp to edge for cubemaps.
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToEdge);
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

        ~TextureCube()
        {
            Dispose();
        }

        #endregion
    }
}
