using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class HeadDataExtension
{
    public static bool IsEyeMorphAvailable(this IHeadDataGetter headData, uint eyeIndex)
    {
        return headData.AvailableMorphs is { Eye: {} eye } && IsMorphAvailable(eye, eyeIndex);
    }

    public static bool IsLipMorphAvailable(this IHeadDataGetter headData, uint lipIndex)
    {
        return headData.AvailableMorphs is { Lip: {} lip } && IsMorphAvailable(lip, lipIndex);
    }

    public static bool IsNoseMorphAvailable(this IHeadDataGetter headData, uint noseIndex)
    {
        return headData.AvailableMorphs is { Nose: {} nose } && IsMorphAvailable(nose, noseIndex);
    }

    public static bool IsMorphAvailable(this IMorphGetter morph, uint index)
    {
        var div = (int)index / 8;
        var mod = (byte)(index % 8);
        var data = morph.Data[div];

        return (data & (1 << mod)) != 0;
    }
}
