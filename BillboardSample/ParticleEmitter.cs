using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillboardSample
{
    abstract class ParticleEmitter
    {
        public float ParticlesPerSecond;

        public Vector3 Position;

        #region Particle Properties

        public Vector2 TexTopLeft;
        public Vector2 TexBottomRight;
       
        public Vector4 StartColor;
        public Vector4 StartColorVariation;

        public Vector4 EndColor;
        public Vector4 EndColorVariation;

        public float StartSize;
        public float StartSizeVariation;

        public float EndSize;
        public float EndSizeVariation;

        public float LifeTime;
        public float LifeTimeVariation;

        #endregion


        private Random randomNumberGen = new Random();
        protected float leftOverTime = 0.0f;

        public abstract void Emit(ParticleSystem particleSystem, float timeSinceLastFrame);

        /// <summary>
        /// Returns random numter between -value, value
        /// </summary>
        protected float Random(float value)
        {
            return ((float)randomNumberGen.NextDouble() * 2.0f - 1.0f) * value;
        }

        protected Vector3 RandomDirection()
        {
            double phi = randomNumberGen.NextDouble() * Math.PI * 2.0;
            double theta = randomNumberGen.NextDouble() * Math.PI * 2.0;
            return new Vector3((float)(System.Math.Cos(phi) * System.Math.Sin(theta)),
                                        (float)(System.Math.Cos(theta)),
                                        (float)(System.Math.Sin(phi) * System.Math.Sin(theta)));
        }
    }

    class ParticleEmitterPoint : ParticleEmitter
    {
        public float Velocity;
        public float VelocityVariation;

        public override void Emit(ParticleSystem particleSystem, float timeSinceLastFrame)
        {
            float timeSinceLastParticle = timeSinceLastFrame + leftOverTime;
            int numParticles = (int)(timeSinceLastParticle * ParticlesPerSecond);

            ParticleSystem.Particle particle;
            particle.Position = Position;
            particle.TexTopLeft = TexTopLeft;
            particle.TexBottomRight = TexBottomRight;

            for(int i=0; i<numParticles; ++i)
            {
                particle.StartColor = StartColor + new Vector4(Random(StartColorVariation.X), Random(StartColorVariation.Y),Random(StartColorVariation.Z),Random(StartColorVariation.W));
                particle.EndColor = EndColor + new Vector4(Random(EndColorVariation.X), Random(EndColorVariation.Y), Random(EndColorVariation.Z), Random(EndColorVariation.W));
                particle.StartSize = StartSize + Random(StartSizeVariation);
                particle.EndSize = EndSize + Random(EndSizeVariation);
                particle.TotalLifeTime = particle.RemainingLifeTime = LifeTime + Random(LifeTimeVariation);
                particle.Velocity = RandomDirection() * (Velocity + Random(VelocityVariation));

                particleSystem.AddParticle(ref particle);
            }
            leftOverTime = timeSinceLastParticle - numParticles / ParticlesPerSecond;
        }
    }
}
