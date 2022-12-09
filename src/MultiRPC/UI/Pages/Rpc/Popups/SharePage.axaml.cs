using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using TinyUpdate.Core.Logging;

namespace MultiRPC.UI.Pages.Rpc.Popups;

public partial class SharePage : Grid, ITitlePage
{
    private readonly ILogging _logging = LoggingCreator.CreateLogger(nameof(SharePage));
    public Language Title { get; } = LanguageText.ProfileShare;

    public SharePage()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
    }

    private readonly Presence _activeRichPresence = null!;
    public SharePage(Presence activeRichPresence)
    {
        _activeRichPresence = activeRichPresence;
        InitializeComponent();

        tblGuild.DataContext = (Language)LanguageText.ShareHelp;
        btnExport.DataContext = (Language)LanguageText.Export;
        btnImport.DataContext = (Language)LanguageText.Import;
        btnImport.IsEnabled = false;
    }

    private async void BtnImport_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var profile = JsonSerializer.Deserialize(txtData.Text.Base64Decode(), RichPresenceContext.Default.Presence);
            if (profile == null)
            {
                await MessageBox.Show(Language.GetText(LanguageText.SharingError));
                this.TryClose();
                return;
            }
                
            var profiles = SettingManager<ProfilesSettings>.Setting.Profiles;
            profiles.CheckName(profile);

            profiles.Add(profile);
            this.TryClose();
            return;
        }
        catch (Exception exception)
        {
            _logging.Error(exception);
        }

        await MessageBox.Show(Language.GetText(LanguageText.SharingError));
        this.TryClose();
    }

    private async void BtnExport_OnClick(object? sender, RoutedEventArgs e)
    {
        var profileBase64 = JsonSerializer.Serialize(_activeRichPresence, RichPresenceContext.Default.Presence);
        await Application.Current.Clipboard.SetTextAsync(profileBase64 = profileBase64.Base64Encode());
        txtData.Text = profileBase64;
        await MessageBox.Show(Language.GetText(LanguageText.ProfileCopyMessage));
    }
}