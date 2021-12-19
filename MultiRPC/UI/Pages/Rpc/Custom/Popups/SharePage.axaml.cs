using System;
using System.Linq;
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

namespace MultiRPC.UI.Pages.Rpc.Custom.Popups;

public partial class SharePage : UserControl, ITitlePage
{
    private readonly ILogging _logging = LoggingCreator.CreateLogger(nameof(SharePage));
    public Language Title { get; } = Language.GetLanguage(LanguageText.ProfileShare);

    public SharePage()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
    }

    private readonly RichPresence _activeRichPresence = null!;
    public SharePage(RichPresence activeRichPresence)
    {
        _activeRichPresence = activeRichPresence;
        InitializeComponent();

        tblGuild.DataContext = Language.GetLanguage(LanguageText.ShareHelp);
        btnExport.DataContext = Language.GetLanguage(LanguageText.Export);
        btnImport.DataContext = Language.GetLanguage(LanguageText.Import);
        btnImport.IsEnabled = false;
    }

    private async void BtnImport_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var profile = JsonSerializer.Deserialize<RichPresence>(txtData.Text.Base64Decode());
            if (profile == null)
            {
                await MessageBox.Show(Language.GetText(LanguageText.SharingError));
                this.TryClose();
                return;
            }
                
            var profiles = SettingManager<ProfilesSettings>.Setting.Profiles;
            if (profiles.Any(x => profile.Name == x.Name))
            {
                var count = 0;
                while (profiles.Any(x => profile.Name + $" {count}" == x.Name))
                {
                    count++;
                }
                profile.Name += $" {count}";
            }
                
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
        var profileBase64 = JsonSerializer.Serialize(_activeRichPresence);
        await Application.Current.Clipboard.SetTextAsync(profileBase64 = profileBase64.Base64Encode());
        txtData.Text = profileBase64;
        await MessageBox.Show(Language.GetText(LanguageText.ProfileCopyMessage));
    }
}