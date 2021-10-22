using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MultiRPC.Extensions;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Validation;

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

        private RichPresence _activeRichPresence;
        public EditPage(RichPresence activeRichPresence)
        {
            _activeRichPresence = activeRichPresence;
            InitializeComponent();
            
            btnDone.DataContext = new Language("Done");
            txtNewName.AddRpcControl(null, s => _activeRichPresence.Name = s,
                s =>
                {
                    var result = string.IsNullOrWhiteSpace(s)
                        ? new CheckResult(false, Language.GetText("EmptyProfileName"))
                        : new CheckResult(true);

                    btnDone.IsEnabled = result.Valid;
                    return result;
                }, _activeRichPresence.Name);
        }

        public Language Title { get; } = new Language("ProfileEdit");

        private void BtnDone_OnClick(object? sender, RoutedEventArgs e)
        {
            this.TryClose();
        }
    }
}