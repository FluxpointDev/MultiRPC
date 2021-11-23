using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
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
                throw new DesignException();
            }
        }
        
        public Language Title { get; } = new Language("ProfileEdit");
        
        private readonly ProfilesSettings _profiles = SettingManager<ProfilesSettings>.Setting;
        private readonly RichPresence _activeRichPresence = null!;
        private string _newName = null!;

        public EditPage(RichPresence activeRichPresence)
        {
            _activeRichPresence = activeRichPresence;
            _newName = activeRichPresence.Name;
            InitializeComponent();
            
            btnDone.DataContext = new Language("Done");
            txtNewName.AddValidation(null, s => _newName = s,
                s =>
                {
                    var result = string.IsNullOrWhiteSpace(s)
                        ? new CheckResult(false, Language.GetText("EmptyProfileName"))
                        : _profiles.Profiles.Any(x => x != _activeRichPresence && x.Name == s) ?
                            new CheckResult(false, Language.GetText("SameProfileName")) : new CheckResult(true);

                    btnDone.IsEnabled = result.Valid;
                    return result;
                }, initialValue: _activeRichPresence.Name);
        }

        private void BtnDone_OnClick(object? sender, RoutedEventArgs e)
        {
            _activeRichPresence.Name = _newName;
            this.TryClose();
        }
    }
}