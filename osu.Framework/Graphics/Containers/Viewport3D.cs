using osu.Framework.Graphics.Containers;

namespace osu.Framework.Graphics
{
    public partial class Viewport3D : BufferedContainer<Drawable3D>
    {
    }

    public partial class Viewport3D<T> : BufferedContainer<T>
        where T : Drawable3D
    {
    }
}
