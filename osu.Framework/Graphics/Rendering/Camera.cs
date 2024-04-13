using System;
using osuTK;

namespace osu.Framework.Graphics.Rendering
{
    public partial class Camera : Drawable3D
    {
        public float FovY = MathF.PI / 2;
        public float NearPlaneDistance = 0.01f;
        public float FarPlaneDistance = 1000f;

        public Matrix4 ViewMatrix => Matrix.Inverted() * Matrix4.CreateScale(1, 1, -1);

        public Matrix4 GetProjectionMatrix(float width, float height)
        {
            var fovMatrix = Matrix4.CreatePerspectiveFieldOfView(FovY, width / height, NearPlaneDistance, FarPlaneDistance);
            return ViewMatrix * fovMatrix;
        }
    }
}

