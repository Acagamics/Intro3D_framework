using System;
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
        /// <summary>
        /// Replacement texture for meshes without defined texture.
        /// </summary>
        private Texture2D whitePixTex;
        private Shader modelShader;
        
        private BillboardEngine billboards;
        private Shader billboardShader;
        private Texture2D billboardTexture;

        private ParticleSystem particleSystem;
        private ParticleEmitterPoint particleEmitter;
        private Texture2D particleTexture;

        
        // Uniform buffer
        [StructLayout(LayoutKind.Sequential)]
        struct UniformData
        {
            public Matrix4 viewProjection;
            public Vector3 lightPosition;
            public float padding0;
            public Vector3 lightColor;
            public float padding1;
            public Vector3 materialColor;
        }
        private UniformData globalUniformData;
        private UniformBuffer<UniformData> globalUniformDataGPUBuffer;



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

            // Model/Scene
            model = Model.GetResource("Content/Models/sibenik.obj");
            whitePixTex = Texture2D.GetResource("Content/whitepix.bmp");
            for(int i=0; i<model.Meshes.Length; ++i)
            {
                if (model.Meshes[i].texture == null)
                    model.Meshes[i].texture = whitePixTex;
            }
            modelShader = Shader.GetResource(new Shader.LoadDescription("Content/simple.vert", "Content/simple.frag"));

            // Billboards
            billboards = new BillboardEngine(5);
            billboardShader = Shader.GetResource(new Shader.LoadDescription("Content/billboard.vert", "Content/billboard.frag"));
            billboardTexture = Texture2D.GetResource("Content/glare.png");

            // Particles
            particleTexture = Texture2D.GetResource("Content/particle.png");
            particleSystem = new ParticleSystem(2048);
            particleEmitter = new ParticleEmitterPoint();
            particleEmitter.ParticlesPerSecond = 100.0f;
            particleEmitter.TexTopLeft = Vector2.One;
            particleEmitter.TexBottomRight = Vector2.Zero;
            particleEmitter.StartColor = new Vector4(0.8f, 0.8f, 0.7f, 1.0f);
            particleEmitter.StartColorVariation = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            particleEmitter.EndColor = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            particleEmitter.EndColorVariation = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            particleEmitter.StartSize = 0.8f;
            particleEmitter.StartSizeVariation = 0.2f;
            particleEmitter.EndSize = 0.0f;
            particleEmitter.EndSizeVariation = 0.0f;
            particleEmitter.LifeTime = 4.0f;
            particleEmitter.LifeTimeVariation = 1.0f;
            particleEmitter.Velocity = 0.8f;
            particleEmitter.VelocityVariation = 0.2f;

            globalUniformDataGPUBuffer = new UniformBuffer<UniformData>();

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

            // Set global light position
            Vector3 pointLightPosition = new Vector3((float)Math.Sin(totalTime * 0.8f) * 10.0f - 2.0f, -6.0f + (float)Math.Cos(totalTime * 0.8f) * 3.0f, 0.0f);

            // Update Uniforms
            globalUniformData.viewProjection = camera.ViewMatrix * camera.ProjectionMatrix;
            globalUniformData.lightPosition = pointLightPosition;
            globalUniformData.lightColor = new Vector3(25, 25, 20);
            globalUniformDataGPUBuffer.UpdateGPUData(ref globalUniformData);
            globalUniformDataGPUBuffer.BindBuffer(0);    // Set "perObject" uniform buffer to binding point 0.

            // Add a glare where the light is
            billboards.Begin(camera);
            billboards.AddBillboard(pointLightPosition, new Vector4(1, 1, 0.9f, 1.0f), 4.0f, Vector2.Zero, Vector2.One, false);
            billboards.End();

            // Add a trail of particles to the light.
            particleEmitter.Position = pointLightPosition;
            particleEmitter.Emit(particleSystem, (float)e.Time);
            particleSystem.Update((float)e.Time, camera);
        }

        private void OnRenderMesh(ref Model.Mesh mesh)
        {
            globalUniformData.materialColor = new Vector3(mesh.material.ColorDiffuse.R, mesh.material.ColorDiffuse.G, mesh.material.ColorDiffuse.B);
            globalUniformDataGPUBuffer.UpdateGPUData(ref globalUniformData);
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

            // Enable backface culling#
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            // Draw a model!
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha); // Classic alpha blending
            GL.UseProgram(modelShader.Program);              // Activate shader.
            model.Draw(OnRenderMesh);

            // Important: Disable depth write (but keep read)
            GL.DepthMask(false);

            // Draw billboards!
            GL.UseProgram(billboardShader.Program);
            GL.BindTexture(TextureTarget.Texture2D, billboardTexture.Texture);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One); // Additive blending.
            billboards.Draw();

            // Draw particles (keep last blending settings)
            GL.BindTexture(TextureTarget.Texture2D, particleTexture.Texture);
            particleSystem.Draw();

            // Disable blending and reactivate depth write.
            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);

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