using System;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using TinyUpdate.Core.Logging;

namespace MultiRPC.UI.Pages.Rpc.Custom.Popups
{
    public partial class SharePage : UserControl
    {
        private ILogging _logging = LoggingCreator.CreateLogger(nameof(SharePage));

        public SharePage()
        {
            if (!Design.IsDesignMode)
            {
                throw new Exception("Shouldn't be calling this when not in designer!");
            }
        }

        private RichPresence _activeRichPresence;
        public SharePage(RichPresence activeRichPresence)
        {
            _activeRichPresence = activeRichPresence;
            InitializeComponent();

            tblGuild.DataContext = new Language("ShareHelp");
            btnExport.DataContext = new Language("Export");
            btnImport.DataContext = new Language("Import");
        }

        private void BtnImport_OnClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                var profile = JsonSerializer.Deserialize<RichPresence>(txtData.Text.Base64Decode());
                if (profile != null)
                {
                    SettingManager<ProfilesSettings>.Setting.Profiles.Add(profile);
                    this.TryClose();
                    return;
                }
            }
            catch (Exception exception)
            {
                _logging.Error(exception);
            }

            //TODO: Show failed popup if we get here
            this.TryClose();
        }

        private async void BtnExport_OnClick(object? sender, RoutedEventArgs e)
        {
            var profileBase64 = JsonSerializer.Serialize(_activeRichPresence);
            await Application.Current.Clipboard.SetTextAsync(profileBase64 = profileBase64.Base64Encode());
            txtData.Text = profileBase64;
            //TODO: Show export popup
        }
    }
}