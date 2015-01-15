using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillboardSample
{
    /// <summary>
    /// Simple particle system.
    /// </summary>
    class ParticleSystem
    {
        /// <summary>
        /// Struct for a particle.
        /// 
        /// Note that it is much more efficient to use multiple arrays that are updated in multiple loops.
        /// Consequently you should have multiple vertex buffers etc.
        /// </summary>
        public struct Particle
        {
            public float TotalLifeTime;
            public float RemainingLifeTime;

            public Vector3 Position;
            public Vector3 Velocity;

            public Vector2 TexTopLeft;
            public Vector2 TexBottomRight;
       
            public Vector4 StartColor;
            public Vector4 EndColor;

            public float StartSize;
            public float EndSize;
        }

        public int MaxNumParticles { get { return particles.Length; } }
        private int numActiveParticles = 0;
        private Particle[] particles;

        private BillboardEngine billboardEngine;

        private Random randomGenerator = new Random();

        public ParticleSystem(int maxNumParticles)
        {
            particles = new Particle[maxNumParticles];
            billboardEngine = new BillboardEngine(maxNumParticles);
        }

        public void AddParticle(ref Particle newParticle)
        {
            int newIndex = numActiveParticles;

            // full? use random place
            if (particles.Length == numActiveParticles)
                newIndex = randomGenerator.Next(particles.Length);
            else
                ++numActiveParticles;

            // add particle
            particles[newIndex] = newParticle;
        }

        public void Update(float timeSinceLastFrame, Camera camera)
        {
            billboardEngine.Begin(camera);

            for (int i = 0; i < numActiveParticles; ++i)
            {
                particles[i].RemainingLifeTime -= timeSinceLastFrame;

                // particle dead?
                if (particles[i].RemainingLifeTime < 0.0f)
                {
                    // "delete" by switching with last active particle.
                    --numActiveParticles;
                    particles[i] = particles[numActiveParticles];
                    --i;
                    continue;
                }

                particles[i].Position += particles[i].Velocity * timeSinceLastFrame;

                float fraction = particles[i].RemainingLifeTime / particles[i].TotalLifeTime;
                billboardEngine.AddBillboard(particles[i].Position,
                                            Vector4.Lerp(particles[i].EndColor, particles[i].StartColor, fraction),
                                            particles[i].EndSize * (1.0f - fraction) + particles[i].StartSize * fraction,
                                            particles[i].TexTopLeft, particles[i].TexBottomRight);
            }

            billboardEngine.End();
        }

        public void Draw()
        {
            billboardEngine.Draw();
        }
    }
}
