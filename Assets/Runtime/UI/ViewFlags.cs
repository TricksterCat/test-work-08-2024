using System;

namespace Runtime.UI
{
    [Flags]
    public enum ViewFlags
    {
        None = 0,
        InstantShow = 1 << 0,
        InstantHide = 1 << 1,
        ShownByDefault = 1 << 2,
        UseSetActive = 1 << 3
    }
}