using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class P3FloatExtension
{
    public static P3Float Cross(this P3Float a, P3Float b)
    {
        return new P3Float(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);
    }

    public static float Dot(this P3Float a, P3Float b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
}