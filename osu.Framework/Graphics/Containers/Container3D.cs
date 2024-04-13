namespace osu.Framework.Graphics
{
    public partial class Container3D : Container3D<Drawable3D>
    {
    }

    public partial class Container3D<T> : Drawable3D
        where T : Drawable3D
    {
    }
}
