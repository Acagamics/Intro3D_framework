using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Intro3DFramework;
using Intro3DFramework.Rendering;
using System.Runtime.InteropServices;

namespace Sample
{
    public class PacmanSample : GameWindow
    {
        /// <summary>
        /// PassedTime since program start.
        /// </summary>
        private float totalTime = 0;

        /// <summary>
        /// Global uniform buffer, updated each frame.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct PerFrameUniformData
        {
            public Matrix4 viewProjection;
            public Vector3 cameraPosition;
            public float totalTime;
        }
        private PerFrameUniformData perFrameUniformData;
        private UniformBuffer<PerFrameUniformData> perFrameUniformGPUBuffer;


        private FontOverlay globalTextOverlay;
        private Font font;

        private Map map;
        private Player player;

        public PacmanSample()
            : base(800, 600)
        {
            Keyboard.KeyDown += Keyboard_KeyDown;
        }

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        /// <param name="sender">The KeyboardDevice which generated this event.</param>
        /// <param name="e">The key that was pressed.</param>
        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Exit();

            if (e.Key == Key.F11)
            {
                if (this.WindowState == WindowState.Fullscreen)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Fullscreen;
            }
        }


        /// <summary>
        /// Setup OpenGL and load resources here.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
#if DEBUG
            // Activates OpenGL debug messages if available.
            Utils.ActivateDebugMessages();
            // Writes all debug messages sent via System.Diagnostics.Debug to the console.
            System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));
#endif

            // Activate depth test.
            GL.Enable(EnableCap.DepthTest);
            // Set standard OpenGL clear color.
            GL.ClearColor(Color.MidnightBlue);
            // Enable backface culling
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            perFrameUniformGPUBuffer = new UniformBuffer<PerFrameUniformData>();
            perFrameUniformGPUBuffer.BindBuffer(0); // Per convention always at binding point 0.

            font = new Font(FontFamily.GenericSansSerif, 15.0f);

            map = new Map();
            player = new Player(Vector2.Zero);

            // OpenTK sets the update frequency by default to 30hz while rendering as fast as possible - which is 60hz at max for most screens (with activated V-Sync).
            // Since this can be rather confusing and lead to not-so-smooth animations, we set the target update frequency to 60hz
            TargetUpdateFrequency = 60.0f;
        }

        /// <summary>
        /// Respond to resize events here.
        /// </summary>
        /// <param name="e">Contains information on the new GameWindow size.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            if (globalTextOverlay != null)
                globalTextOverlay.Dispose();
            globalTextOverlay = new FontOverlay((uint)Width, (uint)Height);
        }

        /// <summary>
        /// Add your game logic here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Update the time.
            totalTime += (float)e.Time;

            player.Update((float)e.Time, map);

            // Update per frame uniform data.
            perFrameUniformData.cameraPosition = new Vector3(0.0f, 100.0f, -100.0f);
            Matrix4 view = Matrix4.LookAt(perFrameUniformData.cameraPosition, Vector3.Zero, Vector3.UnitY);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * 0.35f, (float)Width / Height, 0.1f, 200.0f);
            perFrameUniformData.viewProjection = view * projection;
            perFrameUniformData.totalTime = totalTime;
            perFrameUniformGPUBuffer.UpdateGPUData(ref perFrameUniformData);
        }

        /// <summary>
        /// Add your game rendering code here.
        /// </summary>
        /// <param name="args">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // Clear both color and depth buffer.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            player.Render();
            map.Render(totalTime);
            
            // Swap back and front buffer (=display sth.!).
            SwapBuffers();
        }


        /// <summary>
        /// Entry point of this example.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            using (PacmanSample example = new PacmanSample())
            {
                example.Run(30.0, 0.0);
            }
        }
    }
}