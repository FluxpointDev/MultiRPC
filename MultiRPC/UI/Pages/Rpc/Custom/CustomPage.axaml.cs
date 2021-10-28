using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;
using MultiRPC.UI.Controls;
using MultiRPC.UI.Pages.Rpc.Custom.Popups;

namespace MultiRPC.UI.Pages.Rpc.Custom
{
    public partial class CustomPage : RpcPage
    {
        public override string IconLocation => "Icons/Custom";
        public override string LocalizableName => "Custom";
        public override RichPresence RichPresence
        {
            get => _activeProfile;
            protected set
            {
                //Don't do anything
            }
        }

        private readonly ProfilesSettings _profilesSettings = SettingManager<ProfilesSettings>.Setting;
        private RichPresence _activeProfile = null!;
        private BaseRpcControl _rpcControl = null!;
        private Button? _activeButton;
        private IDisposable? _textBindingDis;
        
        public override void Initialize(bool loadXaml)
        {
            if (loadXaml)
            {
                ContentPadding = new Thickness(0);
            }
            InitializeComponent(loadXaml);

            var tabPage = new TabsPage();
            _rpcControl = new BaseRpcControl
            {
                ImageType = ImagesType.Custom,
                GrabID = true,
                TabName = new Language("CustomPage"),
                Margin = new Thickness(10),
                ShowHelp = true
            };
            Grid.SetRow(tabPage, 2);
            tabPage.AddTabs(_rpcControl);
            tabPage.Initialize();
            grdContent.Children.Insert(grdContent.Children.Count - 1, tabPage);
           
            wrpProfileSelector.Children.AddRange(_profilesSettings.Profiles.Select(MakeProfileSelector));
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

            BtnChangePresence(wrpProfileSelector.Children[0], null!);

            _rpcControl.RichPresence = RichPresence;
            _rpcControl.Initialize(loadXaml);

            _editLang.TextObservable.Subscribe(x => ToolTip.SetTip(imgProfileEdit, x));
            _shareLang.TextObservable.Subscribe(x => ToolTip.SetTip(imgProfileShare, x));
            _addLang.TextObservable.Subscribe(x => ToolTip.SetTip(imgProfileAdd, x));
            _deleteLang.TextObservable.Subscribe(x => ToolTip.SetTip(imgProfileDelete, x));
        }
        
        private readonly Language _editLang = new Language("ProfileEdit");
        private readonly Language _shareLang = new Language("ProfileShare");
        private readonly Language _addLang = new Language("ProfileAdd");
        private readonly Language _deleteLang = new Language("ProfileDelete");
        
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
            btn.Click += BtnChangePresence;
            var binding = new Binding
            {
                Source = presence,
                Path = nameof(presence.Name)
            };
            btn.Bind(ContentProperty, binding);
            return btn;
        }

        private void BtnChangePresence(object? sender, RoutedEventArgs e)
        {
            _activeButton?.Classes.Remove("activeProfile");
            _activeButton = (Button)sender!;
            _activeButton.Classes.Add("activeProfile");
            
            _activeProfile = (RichPresence)_activeButton.DataContext!;
            _textBindingDis?.Dispose();
            AddTextBinding();
            imgProfileDelete.IsVisible = _profilesSettings.Profiles.First() != _activeProfile;

            if (_rpcControl.IsInitialized)
            {
                _rpcControl.ChangeRichPresence(_activeProfile);
            }
            RichPresence = _activeProfile;
        }

        private void ImgProfileEdit_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var window = new MainWindow(new EditPage(_activeProfile));
            window.ShowDialog(((App)Application.Current).DesktopLifetime?.MainWindow);
        }

        private void ImgProfileShare_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var window = new MainWindow(new SharePage(_activeProfile));
            window.ShowDialog(((App)Application.Current).DesktopLifetime?.MainWindow);
        }

        private void ImgProfileAdd_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var newProfile = new RichPresence("Profile" + _profilesSettings.Profiles.Count, 0);
            _profilesSettings.Profiles.Add(newProfile);
            BtnChangePresence(wrpProfileSelector.Children[^1], e);
        }

        private void ImgProfileDelete_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var profileIndex = wrpProfileSelector.Children.IndexOf(_activeButton);
            if (profileIndex == _profilesSettings.Profiles.Count - 1)
            {
                profileIndex--;
            }
            _profilesSettings.Profiles.Remove(_activeProfile);
            BtnChangePresence(wrpProfileSelector.Children[profileIndex], e);
        }

        private void Action_PointerEnter(object? sender, PointerEventArgs e)
        {
            ((Control)sender!).Opacity = 1;
        }

        private void Action_PointerLeave(object? sender, PointerEventArgs e)
        {
            ((Control)sender!).Opacity = 0.6;
        }
    }
}