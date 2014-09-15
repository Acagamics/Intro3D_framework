using System;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Intro3DFramework.Rendering
{
    /// <summary>
    /// Simplistic abstraction for uniform buffer.
    /// </summary>
    public class UniformBuffer<DataBlock> : IDisposable
        where DataBlock : struct
    {
        public int SizeInBytes { get; private set; }

        private int uniformBuffer = -1;

        /// <summary>
        /// Empty constructor, only for creation from file via ResourceManager.
        /// TODO: Version with initial data?
        /// </summary>
        public UniformBuffer()
        {
            SizeInBytes = Marshal.SizeOf(typeof(DataBlock));
            System.Diagnostics.Debug.Assert(SizeInBytes % sizeof(float) * 4 == 0, "All uniform buffer data blocks must have a size of a multiple of 16 bytes (float4). This is not the case for \"" + typeof(DataBlock).Name + "\"");

            uniformBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.UniformBuffer, uniformBuffer); // Bind the buffer for writing
            GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)SizeInBytes, (IntPtr)null, BufferUsageHint.DynamicDraw);
        }

        /// <summary>
        /// Updates the data of the uniform buffer.
        /// </summary>
        public void UpdateGPUData(ref DataBlock newData, int offsetInBytes = 0, int sizeToReplaceInBytes = -1)
        {
            if (sizeToReplaceInBytes == -1)
                sizeToReplaceInBytes = SizeInBytes;

            System.Diagnostics.Debug.Assert(offsetInBytes < SizeInBytes, "Invalid offset, needs to be smaller than the size of the uniform buffer.");
            System.Diagnostics.Debug.Assert(sizeToReplaceInBytes + offsetInBytes <= SizeInBytes, "Invalid data update size. Offset + updated size need to be smaller or equal than the buffer size!");

            GL.BindBuffer(BufferTarget.UniformBuffer, uniformBuffer);
            GL.BufferSubData<DataBlock>(BufferTarget.UniformBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeToReplaceInBytes, ref newData);
        }

        /// <summary>
        /// Binds the uniform buffer to the given binding point.
        /// </summary>
        /// <remarks>Does NOT take care about redundant binding operations!</remarks>
        public void BindBuffer(int bindingPointIndex)
        {
            GL.BindBufferRange(BufferRangeTarget.UniformBuffer, bindingPointIndex, uniformBuffer, (IntPtr)0, (IntPtr)SizeInBytes);
        }

        #region Disposing

        /// <summary>
        /// Has dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Destroys the uniform buffer.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
                return;

            // Only delete buffer if there is still a context (may be already deleted on shutdown)
            if (OpenTK.Graphics.GraphicsContext.CurrentContext != null)
            {
                GL.DeleteBuffer(uniformBuffer);
                uniformBuffer = -1;
            }

            GC.SuppressFinalize(this); // Avoid unnecessary destructor call.
            disposed = true;
        }

        ~UniformBuffer()
        {
            Dispose();
        }

        #endregion
    }
}
