using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;

namespace BillboardSample
{
    class FreeCamera : Camera
    {
        Vector3 move;

        /// <summary>
        /// creates a new camera and sets a projection matrix up
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height. 
        ///                          To match aspect ratio of the viewport, the property AspectRatio.</param>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="nearPlane">Distance to the near view plane.</param>
        /// <param name="farPlane">Distance to the far view plane.</param>
        public FreeCamera(float aspectRatio, float fov = 1.309f, float nearPlane = 0.1f, float farPlane = 5000.0f) :
            base(aspectRatio, fov, nearPlane, farPlane)
        {
        }

        /// <summary>
        /// Speed in forward and backwards direction - pressing arrow up or w
        /// </summary>
        public float ForwardSpeed
        {
            get { return forwardSpeed; }
            set { forwardSpeed = value; }
        }


        /// <summary>
        /// Speed for side movements - pressing left/right arrow or a/d
        /// </summary>
        public float SideSpeed
        {
            get { return sideSpeed; }
            set { sideSpeed = value; }
        }

        /// <summary>
        /// Speed of the camera rotation - using the mouse
        /// </summary>
        public float RotationSpeed
        {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }

        // movement factors variables
        protected float rotationSpeed = 0.005f;
        protected float forwardSpeed = 0.5f;
        protected float verticalSpeed = 0.5f;
        protected float sideSpeed = 0.5f;

        // some intern controlling variables
        protected float phi = 0.0f;
        protected float theta = 0.0f;
        protected int lastMouseX = 0; // last x position of the mouse
        protected int lastMouseY = 0; // last y position of the mouse

        /// <summary>
        /// Updates the Camera 
        /// </summary>
        public override void Update(float timeSinceLastUpdate)
        {

            // mouse movement
            UpdateThetaPhiFromMouse();
            //Mouse.SetPosition(250, 250);

            // resulting view direction
            viewDirection = new Vector3((float)(System.Math.Cos(phi) * System.Math.Sin(theta)),
                                        (float)(System.Math.Cos(theta)),
                                        (float)(System.Math.Sin(phi) * System.Math.Sin(theta)));
            // up vector - by rotation 90°
            float theta2 = theta + (float)System.Math.PI / 2.0f;
            Vector3 upVec = new Vector3((float)(System.Math.Cos(phi) * System.Math.Sin(theta2)),
                                        (float)(System.Math.Cos(theta2)),
                                        (float)(System.Math.Sin(phi) * System.Math.Sin(theta2)));
            // compute side
            Vector3 sideVec = Vector3.Cross(upVec, viewDirection);

            // forward movement
            float forward = (Keyboard.GetState().IsKeyDown(Key.W) ? 1.0f : 0.0f) + (Keyboard.GetState().IsKeyDown(Key.Up) ? 1.0f : 0.0f) -
                            (Keyboard.GetState().IsKeyDown(Key.S) ? 1.0f : 0.0f) - (Keyboard.GetState().IsKeyDown(Key.Down) ? 1.0f : 0.0f);
            //Position += forward * forwardSpeed * viewDirection;

            // side movement
            float side = (Keyboard.GetState().IsKeyDown(Key.A) ? 1.0f : 0.0f) + (Keyboard.GetState().IsKeyDown(Key.Right) ? 1.0f : 0.0f) -
                         (Keyboard.GetState().IsKeyDown(Key.D) ? 1.0f : 0.0f) - (Keyboard.GetState().IsKeyDown(Key.Left) ? 1.0f : 0.0f);

            float vertical = (Keyboard.GetState().IsKeyDown(Key.Q) ? 1.0f : 0.0f) -
                             (Keyboard.GetState().IsKeyDown(Key.E) ? 1.0f : 0.0f);

            move = side * sideSpeed * sideVec + forward * forwardSpeed * viewDirection + vertical * upVec * verticalSpeed;

            Position += move;

            // compute view matrix
            viewMatrix = Matrix4.LookAt(Position, Position + viewDirection, upVec);
        }

        /// <summary>
        /// intern helper to update view angles by mouse
        /// </summary>
        protected void UpdateThetaPhiFromMouse()
        {
            float deltaX = Mouse.GetState().X - lastMouseX;
            float deltaY = Mouse.GetState().Y - lastMouseY;
            phi += deltaX * rotationSpeed;
            theta -= deltaY * rotationSpeed;
            lastMouseX = Mouse.GetState().X;
            lastMouseY = Mouse.GetState().Y;
        }
    }
}
