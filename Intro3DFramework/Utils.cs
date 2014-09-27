using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Intro3DFramework
{
    static public class Utils
    {
        /// <summary>
        /// List of all available OpenGL extensions.
        /// </summary>
        static public HashSet<string> Extensions
        {
            get
            {
                if(extensions == null)
                {
                    extensions = new HashSet<string>();
                    int count = GL.GetInteger(GetPName.NumExtensions);
                    for (int i = 0; i < count; i++)
                    {
                        extensions.Add(GL.GetString(StringNameIndexed.Extensions, i));
                    }
                }
                return extensions;
            }
        }
        static private HashSet<string> extensions;

        /// <summary>
        /// Activates OpenGL debug messages.
        /// </summary>
        static public void ActivateDebugMessages(DebugSeverity minSeverity = DebugSeverity.DebugSeverityMedium)
        {
            if(!Extensions.Contains("GL_ARB_debug_output"))
            {
                Console.WriteLine("Can't activate debug messages since ARB_debug_output extension is not available!");
                return;
            }

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            unsafe
            {
                GL.Arb.DebugMessageControl((OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DontCare,
                                            (OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DontCare,
                                            (OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DebugSeverityNotification, 0, (int*)null, 
                                            minSeverity == DebugSeverity.DebugSeverityNotification);

                GL.Arb.DebugMessageControl((OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DontCare,
                                            (OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DontCare,
                                            OpenTK.Graphics.OpenGL.ArbDebugOutput.DebugSeverityLowArb, 0, (int*)null,
                                            minSeverity == DebugSeverity.DebugSeverityNotification || minSeverity == DebugSeverity.DebugSeverityLow);

                GL.Arb.DebugMessageControl((OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DontCare,
                                            (OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DontCare,
                                            OpenTK.Graphics.OpenGL.ArbDebugOutput.DebugSeverityMediumArb, 0, (int*)null,
                                            minSeverity != DebugSeverity.DebugSeverityHigh);

                GL.Arb.DebugMessageControl((OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DontCare,
                                            (OpenTK.Graphics.OpenGL.ArbDebugOutput)OpenTK.Graphics.OpenGL.All.DontCare,
                                            OpenTK.Graphics.OpenGL.ArbDebugOutput.DebugSeverityHighArb, 0, (int*)null, true);

            }
            GL.Arb.DebugMessageCallback(DebugMessageCallback, IntPtr.Zero);
        }

        /// <summary>
        /// Default callback for ActivateDebugMessages.
        /// </summary>
        static private void DebugMessageCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string messageContent = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message, length);
            string messageText = string.Format("{0}: {1}({2}) {3}: {4}", source.ToString().Replace("DebugSource", ""),
                                                                         type.ToString().Replace("DebugType", ""),
                                                                         severity.ToString().Replace("DebugSeverity", ""), id, messageContent);
            System.Diagnostics.Debug.Assert(severity != DebugSeverity.DebugSeverityHigh, messageText);
            Console.WriteLine(messageText);
        }
    }
}
