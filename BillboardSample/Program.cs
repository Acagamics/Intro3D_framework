﻿using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Intro3DFramework;
using Intro3DFramework.Rendering;
using System.Runtime.InteropServices;

namespace BillboardSample
{
    public class SimpleWindow : GameWindow
    {
        private float totalTime = 0;

        private Camera camera;

        private Model model;
        private Shader shader;
        
        private BillboardEngine billboards;

        
        // Uniform buffer
        [StructLayout(LayoutKind.Sequential)]
        struct PerObjectUniformData
        {
            public Matrix4 worldViewProjection;
        }
        private PerObjectUniformData perObjectUniformData;
        private UniformBuffer<PerObjectUniformData> perObjectUniformGPUBuffer;



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
            model = Model.GetResource("Content/Models/sibenik.obj");
            shader = Shader.GetResource(new Shader.LoadDescription("Content/simple.vert", "Content/simple.frag"));

            billboards = new BillboardEngine(5);
            perObjectUniformGPUBuffer = new UniformBuffer<PerObjectUniformData>();


            camera = new FreeCamera((float)Width / Height);
            camera.Position = new Vector3(0, 0, 0);


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
            camera.AspectRatio = (float)Width / Height;
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

            // Update camera.
            camera.Update((float)e.Time);

            // Add billboards.
            billboards.Begin(camera.ViewMatrix);
            billboards.AddBillboard(new Vector3(-50, 0, (float)Math.Sin(totalTime * 0.3) * 20), new Vector4((float)Math.Sin(totalTime), (float)Math.Sin(totalTime + 1), 1, 1), 5, Vector2.Zero, Vector2.One);
            billboards.AddBillboard(new Vector3(-50, 0, (float)Math.Sin(totalTime * 0.3 + 2) * 20), new Vector4((float)Math.Sin(totalTime), (float)Math.Sin(totalTime + 1), 1, 1), 5, Vector2.Zero, Vector2.One);
            billboards.End();
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

            // Enable backface culling
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            // Draw a model!
            GL.UseProgram(shader.Program);              // Activate shader.
            perObjectUniformData.worldViewProjection = camera.ViewMatrix * camera.ProjectionMatrix;
            perObjectUniformGPUBuffer.UpdateGPUData(ref perObjectUniformData);
            perObjectUniformGPUBuffer.BindBuffer(0);    // Set "perObject" uniform buffer to binding point 0.
            model.Draw();

            // Draw billboards!
            billboards.Draw();

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