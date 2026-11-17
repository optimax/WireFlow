namespace WireFlow
{
    // WireFlow's own values for the DOM, which uses no WPF type. Each has the names of the WPF value it stands for, which
    // are what the .wf files hold, and the same numbers, so that a presentation can convert it by a cast.

    /// <summary>Where an element, or a text, lies across: as WPF's HorizontalAlignment.</summary>
    public enum FlowHorizontalAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Stretch = 3,
    }

    /// <summary>Where an element, or a text, lies down: as WPF's VerticalAlignment.</summary>
    public enum FlowVerticalAlignment
    {
        Top = 0,
        Center = 1,
        Bottom = 2,
        Stretch = 3,
    }

    /// <summary>How the lines of a text are aligned: as WPF's TextAlignment.</summary>
    public enum FlowTextAlignment
    {
        Left = 0,
        Right = 1,
        Center = 2,
        Justify = 3,
    }

    /// <summary>Whether the lines of a text wrap: as WPF's TextWrapping.</summary>
    public enum FlowTextWrapping
    {
        WrapWithOverflow = 0,
        NoWrap = 1,
        Wrap = 2,
    }

    /// <summary>The width of a font's letters: the OpenType usWidthClass, 1 to 9, as WPF's FontStretches name them.</summary>
    public enum FlowFontStretch
    {
        UltraCondensed = 1,
        ExtraCondensed = 2,
        Condensed = 3,
        SemiCondensed = 4,
        Normal = 5,
        SemiExpanded = 6,
        Expanded = 7,
        ExtraExpanded = 8,
        UltraExpanded = 9,
    }

    /// <summary>Whether a font's letters are upright or slanted: as WPF's FontStyles name them.</summary>
    public enum FlowFontStyle
    {
        Normal = 0,
        Oblique = 1,
        Italic = 2,
    }

    /// <summary>How an image fills its frame: as WPF's Stretch.</summary>
    public enum FlowStretch
    {
        None = 0,
        Fill = 1,
        Uniform = 2,
        UniformToFill = 3,
    }

    /// <summary>The direction of a stack panel: as WPF's Orientation.</summary>
    public enum FlowOrientation
    {
        Horizontal = 0,
        Vertical = 1,
    }

    /// <summary>
    /// Where the panel holding an element places it, as a Delphi control's Align: along one of the sides of the room the
    /// panel has left, across the room left (Fill), or where the element's own position puts it (None).
    /// </summary>
    public enum FlowAlignment
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 3,
        Right = 4,
        Fill = 5,
    }
}
