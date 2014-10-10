using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Intro3DFramework;
using Intro3DFramework.Rendering;
using System.Runtime.InteropServices;

namespace Examples.Tutorial
{
    public class SimpleWindow : GameWindow
    {
        private float totalTime = 0;

        private Model model;
        private Shader shader;
        private bool isNormalSkinSet = true;

        [StructLayout(LayoutKind.Sequential)]
        struct PerObjectUniformData
        {
            public OpenTK.Matrix4 worldViewProjection;
        }
        private PerObjectUniformData perObjectUniformData;
        private UniformBuffer<PerObjectUniformData> perObjectUniformGPUBuffer;

        private FontOverlay globalTextOverlay;
        private Font font;
        
        public SimpleWindow()
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
            // If Enter is pressed, change the skin
            if (e.Key == Key.Enter && !e.IsRepeat)
            {
                if (isNormalSkinSet == true)
                    model.Meshes[0].texture = Texture2D.GetResource("Content/Models/Skins/Panda_pink.png");
                else
                    model.Meshes[0].texture = Texture2D.GetResource("Content/Models/Skins/Panda_normal.png");

                // Switch boolean
                isNormalSkinSet = !isNormalSkinSet;
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

            // Load Resources
            model = Model.GetResource("Content/Models/Panda_oneMesh.FBX");
            shader = Shader.GetResource(new Shader.LoadDescription("Content/simple.vert", "Content/simple.frag"));
            perObjectUniformGPUBuffer = new UniformBuffer<PerObjectUniformData>();

            font = new Font(FontFamily.GenericSansSerif, 15.0f);



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

            // The transformation of all world objects (currently, the Panda only).
            Matrix4 world = Matrix4.Translation(Vector3.Zero);

            // "The camera" - position, look at position (point the camera is focused on) and the up-direction.
            Matrix4 view = Matrix4.LookAt(new Vector3(0.0f, 10.0f, -20.0f), new Vector3(0.0f, 10.0f, 0.0f), Vector3.UnitY);

            // "The lens" - defines the opening angle of the camera.
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * 0.5f, (float)Width / Height, 0.1f, 200.0f);

            // Everything together.
            perObjectUniformData.worldViewProjection = world * view * projection;
                                                       
        }

        /// <summary>
        /// Add your game rendering code here.
        /// </summary>
        /// <param name="args">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // Draw to the text overlay.
            globalTextOverlay.Clear();
            globalTextOverlay.AddText("This is some test text.", new OpenTK.Vector2(5.0f), font, Brushes.Gray);
            globalTextOverlay.AddText("And some more test text here.", new OpenTK.Vector2(Width - 500, Height - 40), font, Brushes.Purple);
            globalTextOverlay.AddText("Everybody is curious about the Framerate...", new OpenTK.Vector2(5.0f, 30.0f), font, Brushes.White);
            globalTextOverlay.AddText(String.Format("Here it is FPS: {0:0.0} ({1:0.00}ms)", 1.0f / args.Time, args.Time * 1000.0f), new OpenTK.Vector2(5.0f, 60.0f), font, Brushes.White);

            // Update uniform buffer data.
            perObjectUniformGPUBuffer.UpdateGPUData(ref perObjectUniformData);



            // Clear both color and depth buffer.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Draw a model!
            GL.UseProgram(shader.Program);              // Activate shader.
            perObjectUniformGPUBuffer.BindBuffer(0);    // Set "perObject" uniform buffer to binding point 0.
            model.Draw();       // Actual drawing! (also does some stuff internally upfront ;))

            // Draw text overlay.
            globalTextOverlay.Draw();

            // Swap back and front buffer (=display sth.!).
            SwapBuffers();
        }


        /// <summary>
        /// Entry point of this example.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            using (SimpleWindow example = new SimpleWindow())
            {
                example.Run(30.0, 0.0);
            }
        }
    }
}