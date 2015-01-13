using Intro3DFramework.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    class Player
    {
        private Vector2 position;
        private Vector2 viewDir = Vector2.UnitX;

        private int score = 0;

        #region Shader

        [StructLayout(LayoutKind.Sequential)]
        struct PlayerUniformData
        {
            public Matrix4 world; 
        }

        private PlayerUniformData uniformData;
        private UniformBuffer<PlayerUniformData> uniformGPUBuffer;

        private Shader shader;

        #endregion

        private Model model;

        private const float moveSpeed = 50.0f;
        private const float playerSize = 10.0f;

        public Player(Vector2 startPosition)
        {
            position = startPosition;

            shader = Shader.GetResource(new Shader.LoadDescription("Content/player.vert", "Content/default.frag"));
            
            // Need to assign block binding indices. Shader memorizes these indices.
            GL.UniformBlockBinding(shader.Program, GL.GetUniformBlockIndex(shader.Program, "PerFrame"), 0);
            GL.UniformBlockBinding(shader.Program, GL.GetUniformBlockIndex(shader.Program, "Player"), 1);

            uniformGPUBuffer = new UniformBuffer<PlayerUniformData>();

            model = Model.GetResource("Content/Models/bamboo.obj");
            model.Meshes[0].texture = Texture2D.GetResource("Content/Models/Texture/Bamboo.png");
        }

        public void Update(float timeSinceLastFrame, Map map)
        {
            Vector2 nextPosition = position;
            if (Keyboard.GetState().IsKeyDown(Key.Up) || Keyboard.GetState().IsKeyDown(Key.W))
            {
                nextPosition.Y += moveSpeed * timeSinceLastFrame;
                viewDir = Vector2.UnitY;
            }
            if (Keyboard.GetState().IsKeyDown(Key.Down) || Keyboard.GetState().IsKeyDown(Key.S))
            {
                nextPosition.Y -= moveSpeed * timeSinceLastFrame;
                viewDir = -Vector2.UnitY;
            }
            if (Keyboard.GetState().IsKeyDown(Key.Left) || Keyboard.GetState().IsKeyDown(Key.A))
            {
                nextPosition.X += moveSpeed * timeSinceLastFrame;
                viewDir = Vector2.UnitX;
            }
            if (Keyboard.GetState().IsKeyDown(Key.Right) || Keyboard.GetState().IsKeyDown(Key.D))
            {
                nextPosition.X -= moveSpeed * timeSinceLastFrame;
                viewDir = -Vector2.UnitX;
            }

            // Check if we would now touch a non walkable field
            int gatheredCoins;
            if (map.TryWalk(nextPosition - playerSize / 2 * Vector2.One, nextPosition + playerSize / 2 * Vector2.One, out gatheredCoins))
            {
                position = nextPosition;
                score += gatheredCoins;
            }

            uniformData.world = Matrix4.CreateRotationY((float)Math.Acos(Vector2.Dot(viewDir, Vector2.UnitX))) * 
                                Matrix4.CreateTranslation(position.X, 0, position.Y);
            uniformGPUBuffer.UpdateGPUData(ref uniformData);
        }

        public void Render()
        {
            GL.UseProgram(shader.Program);
            
            uniformGPUBuffer.BindBuffer(1);
            
            model.Draw();
        }
    }
}
