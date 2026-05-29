using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MultiRPC.Theming;
using Avalonia.Svg;
using AvaloniaGif;

namespace MultiRPC.UI.Controls;

public class AssetControl : UserControl
{
    private Theme? _theme;
    public AssetControl()
    {
        InitializeComponent("Icons/Discord.svg");
    }
    
    public AssetControl(string key, Theme? theme = null)
    {
        _theme = theme;
        InitializeComponent(key);
    }

    //TODO: Update on theme change (Only change if the asset would of changed)
    public void SetTheme(Theme theme)
    {
        _theme = theme;
    }
    
    private void InitializeComponent(string key)
    {
        Content = GetControlBasedOnAssetType(key);
    }
    
    private IControl GetControlBasedOnAssetType(string key)
    {
        var asset = string.IsNullOrWhiteSpace(Path.GetExtension(key)) 
            ? _theme?.GetEntries(key)?.FirstOrDefault()?.FullName ?? AssetManager.GetAssetLocation(key) 
            : key;

        var extension = Path.GetExtension(asset);
        if (AssetManager.SupportedAnimatedAsset.Any(x => x.Extensions.Any(y => y == extension)))
        {
            return GetAnimatedControl(key, extension);
        }
        
        IImage? image = null;
        if (extension == AssetManager.SvgFormat.Extensions[0])
        {
            image = new SvgImage
            {
                Source = AssetManager.LoadSvgImage(key, _theme)
            };
        }

        image ??= new Bitmap(AssetManager.GetAsset(key, _theme));
        return new Image()
        {
            Source = image
        };
    }

    private IControl GetAnimatedControl(string key, string extension)
    {
        return new GifImage() { SourceStream = AssetManager.GetSeekableStream(key, _theme) };
    }
}