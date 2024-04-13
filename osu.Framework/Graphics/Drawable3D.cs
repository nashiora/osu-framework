using System;
using System.Runtime.CompilerServices;
using osu.Framework.Caching;
using osuTK;

namespace osu.Framework.Graphics
{
    // NOTE(local): Drawable3D : Drawable is because Viewport3D : BufferedContainer<Drawable3D>
    // In English, since the Viewport3D class wants to reuse the internal framebuffer
    // already present in the framework, anything it contains must be a Drawable.
    // This does not have to be true if instead the Viewport3D contained the BufferedContainer,
    // rather than inheriting it, but then we're required to wrap and re-expose a lot of
    // its functionality (probably) and osu!framework is already a giant inheritance tree,
    // so this wouldn't look strange; at least until Component and Drawable have their inheritance
    // switched, in which case maybe a container contains Components? who knows. Too much
    // foresight for now.
    public partial class Drawable3D : Drawable, IDrawable3D
    {
        private Container3D? parent;
        public new Container3D? Parent
        {
            get => parent;
            //[Friend(typeof(Container3D))]
            internal set
            {
                parent = value;
                matrix.Invalidate();
            }
        }

        private Vector3 origin;
        public override Anchor Origin
        {
            get => throw new NotImplementedException("3D drawables cannot have an origin anchor");
            set => throw new NotImplementedException("3D drawables cannot have an origin anchor");
        }

        public new virtual Vector3 OriginPosition
        {
            get => origin;
            set => trySet(ref origin, ref value);
        }

        private Vector3 position;
        public new virtual Vector3 Position
        {
            get => position;
            set => trySet(ref position, ref value);
        }

        private Vector3 scale = Vector3.One;
        public new virtual Vector3 Scale
        {
            get => scale;
            set => trySet(ref scale, ref value);
        }

        private Quaternion rotation = Quaternion.Identity;
        public new virtual Quaternion Rotation
        {
            get => rotation;
            set => trySet(ref rotation, ref value);
        }

        private Cached<Matrix4> localMatrix = new();
        private Cached<Matrix4> matrix = new();

        public Matrix4 LocalMatrix
        {
            get
            {
                if (!localMatrix.IsValid)
                { // TODO combine into one operation (or just dont do the multiplication on known 0-cells)
                    var newMatrix = Matrix4.CreateTranslation(-origin);

                    Matrix4 temp;
                    if (scale != Vector3.One)
                    {
                        temp = Matrix4.CreateScale(scale);
                        Matrix4.Mult(ref newMatrix, ref temp, out newMatrix);
                    }
                    if (rotation != Quaternion.Identity)
                    {
                        temp = Matrix4.CreateFromQuaternion(rotation);
                        Matrix4.Mult(ref newMatrix, ref temp, out newMatrix);
                    }
                    if (position != Vector3.Zero)
                    {
                        temp = Matrix4.CreateTranslation(position);
                        Matrix4.Mult(ref newMatrix, ref temp, out newMatrix);
                    }

                    localMatrix.Value = newMatrix;
                }

                return localMatrix.Value;
            }
        }

        public Matrix4 Matrix
        {
            get
            {
                if (!matrix.IsValid)
                    matrix.Value = parent != null ? LocalMatrix * parent.Matrix : LocalMatrix;

                return matrix.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void trySet(ref float field, ref float value)
        {
            if (field == value)
                return;

            field = value;
            TryInvalidateMatrix();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void trySet(ref Vector3 field, ref Vector3 value)
        {
            if (field == value)
                return;

            field = value;
            TryInvalidateMatrix();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void trySet(ref Quaternion field, ref Quaternion value)
        {
            if (field == value)
                return;

            field = value;
            TryInvalidateMatrix();
        }

        public void TryInvalidateMatrix()
        {
            if (localMatrix.Invalidate())
                InvalidateMatrix();
        }

        protected virtual void InvalidateMatrix()
        {
            matrix.Invalidate();
            Invalidate(Invalidation.DrawNode | Invalidation.DrawInfo);
        }
    }
}
