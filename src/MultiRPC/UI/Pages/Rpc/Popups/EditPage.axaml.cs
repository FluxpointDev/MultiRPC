using Avalonia.Controls;
using Avalonia.Interactivity;
using MultiRPC.Commands;
using MultiRPC.Exceptions;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;

namespace MultiRPC.UI.Pages.Rpc.Popups;

public partial class EditPage : StackPanel, ITitlePage
{
    private readonly ProfilesSettings _profiles = SettingManager<ProfilesSettings>.Setting;
    private readonly Presence _activeRichPresence = null!;
    private string _newName = null!;
    public EditPage()
    {
        if (!Design.IsDesignMode)
        {
            throw new DesignException();
        }
    }

    public Language Title { get; } = LanguageText.ProfileEdit;

    public EditPage(Presence activeRichPresence)
    {
        _activeRichPresence = activeRichPresence;
        _newName = activeRichPresence.Name;
        InitializeComponent();
            
        btnDone.DataContext = (Language)LanguageText.Done;
        btnDone.Command = new ActionCommand((ob) =>
        {
            if (btnDone.IsEnabled)
            {
                BtnDone_OnClick(ob, null!);
            }
        });
        txtNewName.AddValidation(null, s => _newName = s,
            s =>
            {
                var result = string.IsNullOrWhiteSpace(s)
                    ? new CheckResult(false, Language.GetText(LanguageText.EmptyProfileName))
                    : _profiles.Profiles.Any(x => x != _activeRichPresence && x.Name == s) ?
                        new CheckResult(false, Language.GetText(LanguageText.SameProfileName)) : new CheckResult(true);

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