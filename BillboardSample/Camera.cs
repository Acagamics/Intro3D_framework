using OpenTK;

namespace BillboardSample
{
    /// <summary>
    /// Abstract class for cameras
    /// you can easily make your own camera by inheriting this class
    /// </summary>
    public abstract class Camera
    {
        // matrices
        protected Matrix4 projectionMatrix;
        protected Matrix4 viewMatrix = Matrix4.Identity;
        // vectors
        protected Vector3 viewDirection = new Vector3(0,0,1);
        protected Vector3 position = new Vector3(0, 1, 0);
        // projection properties
        protected float aspectRatio;
        protected float fov;
        protected float nearPlane;
        protected float farPlane;

        /// <summary>
        /// creates a new camera and sets a projection matrix up
        /// </summary>
        /// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height. 
        ///                          To match aspect ratio of the viewport, the property AspectRatio.</param>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="nearPlane">Distance to the near view plane.</param>
        /// <param name="farPlane">Distance to the far view plane.</param>
        public Camera(float aspectRatio, float fov, float nearPlane, float farPlane)
        { 
            this.aspectRatio = aspectRatio;
            this.fov = fov;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
            RebuildProjectionMatrix();
        }

        /// <summary>
        /// The projection matrix for this camera
        /// </summary>
        public Matrix4 ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        /// <summary>
        /// The view matrix for this camera
        /// </summary>
        public Matrix4 ViewMatrix
        {
            get { return viewMatrix; }
        }

        /// <summary>
        /// Current position of the camera
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; } 
        }

        /// <summary>
        /// Current view-direction of the camera
        /// </summary>
        public Vector3 Direction
        {
            get { return viewDirection; }
        }

        /// <summary>
        /// Aspect ratio (width / height) of this camera.
        /// </summary>
        public float AspectRatio
        {
            get { return aspectRatio; }
            set { aspectRatio = value; RebuildProjectionMatrix(); }
        }

        /// <summary>
        /// Updates the Camera.
        /// Handles user input intern and updates matrices.
        /// </summary>
        public abstract void Update(float timeSinceLastUpdate);

        /// <summary>
        /// Intern function for recreating the projection matrix.
        /// Capsuling the Matrix.Create... makes it easy to exchange the type of projection
        /// </summary>
        protected virtual void RebuildProjectionMatrix()
        {
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane);
        }
    }
}
