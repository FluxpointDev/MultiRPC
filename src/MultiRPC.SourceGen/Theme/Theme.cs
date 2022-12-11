using MultiRPC.Theming;

namespace MultiRPC.SourceGen.Theme;

public class Theme
{
    internal bool _hasAssets = false;

    /// <summary>
    /// What colouring is in the theme
    /// </summary>
    public Colours Colours { get; set; } = null!;

    /// <summary>
    /// Any metadata about this theme
    /// </summary>
    public Metadata Metadata { get; set; } = null!;

    /// <summary>
    /// What mode this theme is in
    /// </summary>
    public ThemeType ThemeType { get; internal set; } = ThemeType.Legacy;
}