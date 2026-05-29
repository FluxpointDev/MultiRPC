using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Helpers;

//This is based off Wpf MessageBox
namespace MultiRPC.UI;

public enum MessageBoxResult
{
    None = 0,
    Ok = 1,
    Cancel = 2,
    Yes = 6,
    No = 7,
}

public enum MessageBoxButton
{
    Ok = 0,
    OkCancel = 1,
    YesNoCancel = 3,
    YesNo = 4,
}
    
public enum MessageBoxImage
{
    None = 0,
    Error = 16,
    Question = 32,
    Warning = 48,
    Information = 64,
}

public partial class MessageBox : Grid
{
    public MessageBox()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
    }

    private MessageBox(string messageBoxText, string messageBoxTitle,
        MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
    {
        InitializeComponent();
            
        btnNo.Content = Language.GetText(LanguageText.No);
        btnYes.Content = Language.GetText(LanguageText.Yes);
        btnOk.Content = Language.GetText(LanguageText.Ok);
        btnCancel.Content = Language.GetText(LanguageText.Cancel);
        tblText.Text = messageBoxText;

        if (messageBoxButton == MessageBoxButton.Ok)
        {
            btnOk.IsVisible = true;
        }
        if (messageBoxButton == MessageBoxButton.OkCancel)
        {
            btnOk.IsVisible = true;
            btnCancel.IsVisible = true;
        }
        if (messageBoxButton == MessageBoxButton.YesNo)
        {
            btnYes.IsVisible = true;
            btnNo.IsVisible = true;
        }
        if (messageBoxButton == MessageBoxButton.YesNoCancel)
        {
            btnYes.IsVisible = true;
            btnNo.IsVisible = true;
            btnCancel.IsVisible = true;
        }

        var imgInt = (int)messageBoxImage;
        imgStatus.IsVisible = imgInt != 0;
        imgStatus.Source = imgInt switch
        {
            64 => SvgImageHelper.LoadImage("Icons/Info.svg"),
            48 => SvgImageHelper.LoadImage("Icons/Warning.svg"),
            16 => SvgImageHelper.LoadImage("Icons/Alert.svg"),
            32 => SvgImageHelper.LoadImage("Icons/Help.svg"),
            _ => imgStatus.Source
        };
    }

    private void ButOk_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TryClose(MessageBoxResult.Ok);
    }

    private void ButYes_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TryClose(MessageBoxResult.Yes);
    }

    private void ButNo_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TryClose(MessageBoxResult.No);
    }

    private void ButCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TryClose(MessageBoxResult.Cancel);
    }
        
    private static MessageBoxResult DefaultReturn(MessageBoxButton button, MessageBoxResult defaultMessageBoxResult)
    {
        switch (button)
        {
            case MessageBoxButton.OkCancel:
                return MessageBoxResult.Cancel;
            case MessageBoxButton.Ok:
                return MessageBoxResult.Ok;
            case MessageBoxButton.YesNoCancel:
                return MessageBoxResult.Cancel;
            case MessageBoxButton.YesNo:
                return MessageBoxResult.Yes;
            default:
                return defaultMessageBoxResult;
        }
    }
        
    private static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
        MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage,
        MessageBoxResult messageBoxResult = MessageBoxResult.None, Window? ownerWindow = null)
    {
        var page = new MessageBox(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage);
        switch (messageBoxButton)
        {
            case MessageBoxButton.OkCancel:
                page.btnOk.IsDefault = true;
                break;
            case MessageBoxButton.Ok:
                page.btnOk.IsDefault = true;
                break;
            case MessageBoxButton.YesNoCancel:
                page.btnYes.IsDefault = true;
                break;
            case MessageBoxButton.YesNo:
                page.btnYes.IsDefault = true;
                break;
        }

        var window = new MainWindow(page)
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            MinHeight = page.MinHeight + 30,
            MaxWidth = 410,
            Title = messageBoxTitle,
            ShowInTaskbar = false,
            DisableMinimiseButton = true
        };

        ownerWindow ??= App.MainWindow;
        return await window.ShowDialog<MessageBoxResult?>(ownerWindow) ?? DefaultReturn(messageBoxButton, messageBoxResult);
    }
        
    public static async Task<MessageBoxResult> Show(string messageBoxText, Window? window = null)
    {
        return await Show(messageBoxText, Language.GetText(LanguageText.MultiRPC), MessageBoxButton.Ok, MessageBoxImage.None,
            ownerWindow: window);
    }

    public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
        Window? window = null)
    {
        return await Show(messageBoxText, messageBoxTitle, MessageBoxButton.Ok, MessageBoxImage.None,
            ownerWindow: window);
    }

    public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
        MessageBoxButton messageBoxButton, Window? window = null)
    {
        return await Show(messageBoxText, messageBoxTitle, messageBoxButton, MessageBoxImage.None,
            ownerWindow: window);
    }

    public static async Task<MessageBoxResult> Show(string messageBoxText, string messageBoxTitle,
        MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage, Window? window = null)
    {
        return await Show(messageBoxText, messageBoxTitle, messageBoxButton, messageBoxImage, ownerWindow: window);
    }
}