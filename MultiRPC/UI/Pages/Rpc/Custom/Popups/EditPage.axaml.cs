using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Validation;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;

namespace MultiRPC.UI.Pages.Rpc.Custom.Popups
{
    public partial class EditPage : UserControl, ITitlePage
    {
        public EditPage()
        {
            if (!Design.IsDesignMode)
            {
                throw new Exception("Shouldn't be calling this when not in designer!");
            }
        }

        private readonly ProfilesSettings _profiles = SettingManager<ProfilesSettings>.Setting;

        private RichPresence _activeRichPresence;
        private string _newName = string.Empty;
        public EditPage(RichPresence activeRichPresence)
        {
            _activeRichPresence = activeRichPresence;
            InitializeComponent();
            
            btnDone.DataContext = new Language("Done");
            txtNewName.AddRpcControl(null, s => _newName = s,
                s =>
                {
                    var result = string.IsNullOrWhiteSpace(s)
                        ? new CheckResult(false, Language.GetText("EmptyProfileName"))
                        : _profiles.Profiles.Any(x => x != _activeRichPresence && x.Name == s) ?
                            new CheckResult(false, Language.GetText("SameProfileName")) : new CheckResult(true);

                    btnDone.IsEnabled = result.Valid;
                    return result;
                }, _activeRichPresence.Name);
        }

        public Language Title { get; } = new Language("ProfileEdit");

        private void BtnDone_OnClick(object? sender, RoutedEventArgs e)
        {
            _activeRichPresence.Name = _newName;
            this.TryClose();
        }
    }
}