using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Controls;

namespace MultiRPC.UI.Pages.Rpc
{
    //TODO: Add edit/add/remove/share popup's
    public partial class CustomPage : RpcPage
    {
        public CustomPage()
        {
            ContentPadding = new Thickness(0);
        }

        public override string IconLocation => "Icons/Custom";
        public override string LocalizableName => "Custom";
        public override RichPresence RichPresence { get; protected set; } = new RichPresence("", 0);

        private readonly ProfilesSettings _profilesSettings = SettingManager<ProfilesSettings>.Setting;
        private RichPresence _activeProfile;
        private IDisposable _textBindingDis;
        private BaseRpcControl rpcControl;

        public override void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            brdContent.Background = (IBrush)Application.Current.Resources["ThemeAccentBrush"];

            var tabPage = new TabsPage();
            rpcControl = new BaseRpcControl
            {
                ImageType = ImagesType.Custom,
                GrabID = true,
                TabName = new Language("CustomPage")
            };
            Grid.SetRow(tabPage, 2);
            tabPage.AddTabs(rpcControl);
            tabPage.Initialize();
            grdContent.Children.Insert(grdContent.Children.Count - 1, tabPage);
            
            _activeProfile = _profilesSettings.Profiles.First();
            AddTextBinding();
           
            foreach (var profile in _profilesSettings.Profiles)
            {
                wrpProfileSelector.Children.Add(MakeProfileSelector(profile));
            }
            _profilesSettings.Profiles.CollectionChanged += (sender, args) =>
            {
                foreach (RichPresence profile in args.OldItems ?? Array.Empty<object>())
                {
                    wrpProfileSelector.Children.Remove(wrpProfileSelector.Children.First(x => x.DataContext == profile));
                }
                foreach (RichPresence profile in args.NewItems ?? Array.Empty<object>())
                {
                    wrpProfileSelector.Children.Add(MakeProfileSelector(profile));
                }
            };

            rpcControl.RichPresence = RichPresence;
            rpcControl.Initialize(loadXaml);
        }
        
        private void AddTextBinding()
        {
            var textBinding = new Binding
            {
                Source = _activeProfile, 
                Mode = BindingMode.OneWay,
                Path = nameof(_activeProfile.Name)
            };
            _textBindingDis = tblProfileName.Bind(TextBlock.TextProperty, textBinding);
        }

        private Control MakeProfileSelector(RichPresence presence)
        {
            var btn = new Button
            {
                DataContext = presence,
                Margin = new Thickness(0, 0, 5, 0)
            };
            btn.Click += (sender, args) =>
            {
                _activeProfile = presence;
                _textBindingDis.Dispose();
                AddTextBinding();
                rpcControl.ChangeRichPresence(_activeProfile);
            };
            var binding = new Binding
            {
                Source = presence,
                Path = nameof(presence.Name)
            };
            btn.Bind(ContentProperty, binding);
            return btn;
        }
    }
}