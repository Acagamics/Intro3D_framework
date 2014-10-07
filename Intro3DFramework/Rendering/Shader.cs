using Intro3DFramework.ResourceSystem;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intro3DFramework.Rendering
{
    /// <summary>
    /// Simple shader helper for shader programs consisting of a vertex & a fragment program.
    /// </summary>
    public class Shader : ResourceSystem.BaseResource<Shader, Shader.LoadDescription>
    {
        /// <summary>
        /// The ready linked shader program.
        /// </summary>
        public int Program { get { return program; } }
        private int program = -1;

        private int vertexShader = -1;
        private int fragmentShader = -1;

        /// <summary>
        /// Resource description to load shader from file.
        /// </summary>
        /// <see cref="Load"/>
        /// <see cref="GetResource"/>
        public struct LoadDescription : ResourceSystem.IResourceDescription
        {
            public enum LoadType
            {
                /// <summary>
                /// Strings reprsent filenames to text files.
                /// </summary>
                FILE,

                /// <summary>
                /// Strings represents raw code.
                /// </summary>
                RAWCODE
            }

            public LoadDescription(string vertexShader, string fragmentShader, LoadType loadingType = LoadType.FILE)
            {
                this.type = loadingType;
                this.vertexShader = vertexShader;
                this.fragmentShader = fragmentShader;
            }

            public override bool Equals(object other)
            {
                LoadDescription? otherDesc = other as LoadDescription?;
                if (type == LoadType.FILE)
                {
                    return otherDesc.HasValue &&
                       Path.GetFullPath(vertexShader) == Path.GetFullPath(otherDesc.Value.vertexShader) &&
                       Path.GetFullPath(fragmentShader) == Path.GetFullPath(otherDesc.Value.fragmentShader);
                }
                else
                    return otherDesc.HasValue &&
                       vertexShader == otherDesc.Value.vertexShader &&
                       fragmentShader == otherDesc.Value.fragmentShader;
            }

            public override int GetHashCode()
            {
                if(type == LoadType.FILE)
                    return Path.GetFullPath(vertexShader).GetHashCode() + Path.GetFullPath(fragmentShader).GetHashCode();
                else
                    return vertexShader.GetHashCode() + fragmentShader.GetHashCode();
            }

            public LoadType type;
            public string vertexShader;
            public string fragmentShader;
        }

        /// <summary>
        /// Empty constructor, only for creation from file via ResourceManager
        /// </summary>
        public Shader()
        {
        }

        /// <summary>
        /// Loads a shader from files.
        /// </summary>
        /// <param name="description">Description object describing of which files the shader consists.</param>
        internal override void Load(Shader.LoadDescription description)
        {
            System.Diagnostics.Debug.Assert(program == -1 && fragmentShader == -1 && vertexShader == -1, "Shader was already loaded.");

            string vertexShaderCode, fragmentShaderCode;
            if (description.type == LoadDescription.LoadType.FILE)
            {
                try
                {
                    vertexShaderCode = File.ReadAllText(description.vertexShader);
                }
                catch (Exception e)
                {
                    throw new ResourceException(e.GetType() == typeof(FileNotFoundException) ? ResourceException.Type.NOT_FOUND : ResourceException.Type.LOAD_ERROR,
                                    "Couldn't load vertex shader code (\"" + description.vertexShader + "\")", e);
                }
                try
                {
                    fragmentShaderCode = File.ReadAllText(description.fragmentShader);
                }
                catch (Exception e)
                {
                    throw new ResourceException(e.GetType() == typeof(FileNotFoundException) ? ResourceException.Type.NOT_FOUND : ResourceException.Type.LOAD_ERROR,
                                    "Couldn't load fragment shader code (\"" + description.fragmentShader + "\")", e);
                }
            }
            else
            {
                vertexShaderCode = description.vertexShader;
                fragmentShaderCode = description.fragmentShader;
            }
            
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderCode);
            GL.CompileShader(vertexShader);
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            if (infoLog.Length > 0)
                Console.WriteLine("Compiling vertex shader \"" + description.vertexShader + "\", Log: \"" + infoLog + "\"");

            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderCode);
            GL.CompileShader(fragmentShader);
            infoLog = GL.GetShaderInfoLog(fragmentShader);
            if (infoLog.Length > 0)
                Console.WriteLine("Compiling fragment shader \"" + description.fragmentShader + "\", Log: \"" + infoLog + "\"");

            program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            infoLog = GL.GetProgramInfoLog(program);
            if (infoLog.Length > 0)
                Console.WriteLine("Linking shader program, Log: \"" + infoLog + "\"");

            Console.WriteLine();
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
                if(fragmentShader != -1)
                {
                    GL.DeleteShader(fragmentShader);
                    fragmentShader = -1;
                }
                if(vertexShader != -1)
                {
                    GL.DeleteShader(vertexShader);
                    vertexShader = -1;
                }
                if(program != -1)
                {
                    GL.DeleteProgram(program);
                    program = -1;
                }
            }

            GC.SuppressFinalize(this); // Avoid unnecessary destructor call.
            disposed = true;
        }

        ~Shader()
        {
            Dispose();
        }

        #endregion
    }
}
