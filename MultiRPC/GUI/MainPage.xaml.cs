using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using DiscordRPC;
using MultiRPC.Core;
using MultiRPC.Core.Notification;
using MultiRPC.Core.Rpc;
using MultiRPC.GUI.CorePages;
using MultiRPC.GUI.MessageBox;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Threading;
using System.Net.NetworkInformation;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        NotificationToast NetworkNotificationToast = null;

        //ToDo: Add Connection Lost, allow the user to stop the program
        public MainPage()
        {
            InitializeComponent();
            NotificationCenter.NewNotificationToast += NewNotificationToast;
            NetworkNotificationToast = NotificationToast.Create(null, 0, showNotification: false, giveID: true, dismissive: false);
                
            rUsername.Text = Settings.Current.LastUser;
            tblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Loaded += (_, __) =>
            {
                var loggingPage = new LoggingPage();
                NotificationCenter.AddLoggerSinks(loggingPage);
                App.Manager.MainPageManager.AddMainPage(new MultiRPCPage());
                App.Manager.MainPageManager.AddMainPage(new CustomPage());
                App.Manager.MainPageManager.AddMainPage(new SettingsPage());
                App.Manager.MainPageManager.AddMainPage(loggingPage);
                App.Manager.MainPageManager.AddMainPage(new CreditsPage());
#if DEBUG
                if (Settings.Current.Debug)
                { 
                    App.Manager.MainPageManager.AddMainPage(new ProgramPage());
                }
#endif
                UpdateText();
                (spMainPages.Children[0] as Button)?.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            };
            Rpc.ConnectionOpened += Rpc_ConnectionOpened;
            Rpc.ConnectionClosed += Rpc_ConnectionClosed;
            Rpc.ConnectionLost += (sender, message) => RPCPreview.CurrentView = Views.RPCPreview.ViewType.Error;
            Rpc.ConnectionStarted += Rpc_ConnectionStarted;
            Settings.Current.LanguageChanged += (_, __) => UpdateText();
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        private void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            //ToDo: Make a group edit funtion...
            var hasInternet = Utils.NetworkIsAvailable();
            var jsonContent = hasInternet ?
                "InternetBack" : 
                "InternetLost";
            var level = hasInternet ? 
                Serilog.Events.LogEventLevel.Information :
                Serilog.Events.LogEventLevel.Error;

                NetworkNotificationToast.JsonContent = jsonContent;
                NetworkNotificationToast.Level = level;
                NetworkNotificationToast.TimeOut = hasInternet ?
                    TimeSpan.FromSeconds(3) : 
                    TimeSpan.Zero;
            if (!hasInternet && NetworkNotificationToast.Closed)
            {
                NetworkNotificationToast.OpenToast();
            }
        }

        private async void UpdateText() => await Dispatcher.InvokeAsync(() =>
        {
            switch (RPCPreview.CurrentView)
            {
                case Views.RPCPreview.ViewType.Default:
                    rCon.Text = LanguagePicker.GetLineFromLanguageFile("Disconnected");
                break;
                case Views.RPCPreview.ViewType.Loading:
                    rCon.Text = LanguagePicker.GetLineFromLanguageFile("Loading");
                break;
                case Views.RPCPreview.ViewType.RichPresence:
                    rCon.Text = LanguagePicker.GetLineFromLanguageFile("Connected");
                break;
            }

            btnUpdate.Content = LanguagePicker.GetLineFromLanguageFile("UpdatePresence");
            rStatus.Text = LanguagePicker.GetLineFromLanguageFile("Status") + ": ";
            rUser.Text = LanguagePicker.GetLineFromLanguageFile("User") + ": ";
            btnAuto.Content = LanguagePicker.GetLineFromLanguageFile("Auto");
            btnAfk.Content = LanguagePicker.GetLineFromLanguageFile("Afk");
            tblAfkText.Text = LanguagePicker.GetLineFromLanguageFile("AfkText") + ": ";

            if (Rpc.HasConnection)
            {
                App.Current.Resources["StartButtonText"] = LanguagePicker.GetLineFromLanguageFile("Shutdown");
            }
            //btnDisableDiscordCheck.Content = LanguagePicker.GetLineFromLanguageFile("TempDisableDiscordCheck");
        });

        //ToDo: Redo this, it's a complete mess
        private void NewNotificationToast(object sender, NotificationToast e)
        {
            var spContainer = new StackPanel
            {
                Name = "sp" + DateTime.Now.Ticks,
                Background = GetColourFromLevel(e.Level),
                Visibility = e.Closed ? Visibility.Collapsed : Visibility.Visible
            };
            RegisterName(spContainer.Name, spContainer);

            var gridContainer = new Grid 
            {
                Margin = new Thickness(5)
            };
            gridContainer.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });
            gridContainer.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0, GridUnitType.Auto)
            });
            gridContainer.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(0, GridUnitType.Auto)
            });
            gridContainer.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(0, GridUnitType.Auto)
            });
            spContainer.Children.Add(gridContainer);

            var txtContent = new TextBlock 
            {
                Text = LanguagePicker.GetLineFromLanguageFile(e.JsonContent),
                TextWrapping = TextWrapping.Wrap
            };
            Settings.Current.LanguageChanged += (_,__) => txtContent.Text = LanguagePicker.GetLineFromLanguageFile(e.JsonContent);
            gridContainer.Children.Add(txtContent);

            var closeButtonImg = new Image();
            closeButtonImg.SetResourceReference(Image.SourceProperty, "CrossIcon");

            var closeButton = new Button
            {
                Content = closeButtonImg,
                Height = 21,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0),
                Visibility = e.Dismissive ? Visibility.Visible : Visibility.Collapsed
            };
            closeButton.Click += (_, __) => e.DismissToast();
            Grid.SetColumn(closeButton, 1);
            gridContainer.Children.Add(closeButton);

            var wrapPanel = new WrapPanel 
            {
                Margin = new Thickness(0, 5, 0, 0)
            };
            Grid.SetRow(wrapPanel, 1);
            Grid.SetColumnSpan(wrapPanel, 2);

            UpdateNotificationButtons(e, wrapPanel);

            gridContainer.Children.Add(wrapPanel);
            spNotificationCenter.Children.Add(spContainer);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            spContainer.Tag = cancellationTokenSource;
            Task.Run(async () =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(1);
                }
                e.DismissToast();
            });
            e.PropertyChanged += (sender, e) => EditNotificationToast(txtContent, spContainer, closeButton, wrapPanel, sender as NotificationToast);
            //ToDo: Make this not get fired every time it gets dismissed
            e.ToastDismissed += async (_, __) => await Dispatcher.Invoke(async () =>
            {
                await spContainer.DoubleAnimation(0, spContainer.ActualHeight, propertyDependency: HeightProperty);
                spContainer.Height = 0;
                if (e.ID == 0)
                {
                    spNotificationCenter.Children.Remove(spContainer);
                }
            });
        }

        private void UpdateNotificationButtons(NotificationToast notifications, WrapPanel wrapPanel)
        {
            wrapPanel.Children.Clear();

            for (int i = 0; i < (notifications.Buttons?.Length ?? 0); i++)
            {
                var btnAction = new Button
                {
                    Content = LanguagePicker.GetLineFromLanguageFile(notifications.Buttons[i].JsonContent)
                };
                Settings.Current.LanguageChanged += (_, __) =>
                {
                    btnAction.Content = LanguagePicker.GetLineFromLanguageFile(notifications.Buttons[i].JsonContent);
                };
                var k = i;
                btnAction.Click += (_, __) => notifications.Buttons[k].Action.Invoke(notifications);
                wrapPanel.Children.Add(btnAction);
            }
        }

        private SolidColorBrush GetColourFromLevel(Serilog.Events.LogEventLevel logEvent) => logEvent switch
        {
            Serilog.Events.LogEventLevel.Verbose => new SolidColorBrush(Colors.Green),
            Serilog.Events.LogEventLevel.Debug => new SolidColorBrush(Colors.LightGray),
            Serilog.Events.LogEventLevel.Information => new SolidColorBrush(Colors.Green),
            Serilog.Events.LogEventLevel.Warning => new SolidColorBrush(Colors.Orange),
            Serilog.Events.LogEventLevel.Error => new SolidColorBrush(Colors.Red),
            Serilog.Events.LogEventLevel.Fatal => new SolidColorBrush(Colors.DarkRed),
            _ => null
	    };

        private void EditNotificationToast(TextBlock txtContent, StackPanel spContainer, Button closeButton, WrapPanel wrapPanel, NotificationToast notification) => Dispatcher.Invoke(() => 
        {
            NotificationCenter.Logger.Debug(notification.Closed.ToString());
            if (notification.Closed)
            {
                notification.DismissToast();
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                spContainer.Tag = cancellationTokenSource;
                Task.Run(async () =>
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        await Task.Delay(1);
                    }
                    notification.DismissToast();
                });
                return;
            }
            else
            {
                spContainer.Visibility = Visibility.Visible;
                spContainer.Height = double.NaN;
                NotificationCenter.Logger.Debug(spContainer.Height.ToString());
                NotificationCenter.Logger.Debug(nameof(spContainer.Visibility));
            }
            spContainer.Background = GetColourFromLevel(notification.Level);
            txtContent.Text = LanguagePicker.GetLineFromLanguageFile(notification.JsonContent);
            closeButton.Visibility = notification.Dismissive ? Visibility.Visible : Visibility.Collapsed;
            UpdateNotificationButtons(notification, wrapPanel);

            var token = spContainer.Tag as CancellationTokenSource;
            if (notification.TimeOut > TimeSpan.Zero) 
            {
                token.CancelAfter(notification.TimeOut);
            }
            else
            {
                token.CancelAfter(-1);
            }
        });

        private async void Rpc_ConnectionClosed(object sender, EventArgs e) => await Dispatcher.InvokeAsync(() =>
        {
            tbAfkReason.IsEnabled = true;
            RPCPreview.CurrentView = Views.RPCPreview.ViewType.Default;
            btnUpdate.IsEnabled = false;
            btnStart.SetResourceReference(StyleProperty, "ButtonGreen");
            btnStart.SetResourceReference(Button.IsEnabledProperty, "CanRunRpc");
            rCon.Text = LanguagePicker.GetLineFromLanguageFile("Disconnected");
            App.Current.Resources["StartButtonText"] = App.Current.Resources["LastRpcStartButtonText"];
            CheckAfkStatus();
        });

        private async void Rpc_ConnectionOpened(object sender, DiscordRPC.Message.ReadyMessage e)
        {
            RPCPreview.CurrentView = Views.RPCPreview.ViewType.RichPresence;
            await Dispatcher.InvokeAsync(() =>
            {
                rUsername.Text = Settings.Current.LastUser;
                App.Current.Resources["StartButtonText"] = LanguagePicker.GetLineFromLanguageFile("Shutdown");
                btnStart.SetResourceReference(Button.StyleProperty, "ButtonRed");
                btnStart.IsEnabled = true;
                btnUpdate.SetResourceReference(Button.IsEnabledProperty, "CanRunRpc");
                rCon.Text = LanguagePicker.GetLineFromLanguageFile("Connected");
            });
        }

        private void Rpc_ConnectionStarted(object sender, EventArgs e) => Dispatcher.Invoke(() =>
        {
            tbAfkReason.IsEnabled = false;
            btnAfk.IsEnabled = false;
            rCon.Text = LanguagePicker.GetLineFromLanguageFile("Loading");
            RPCPreview.CurrentView = Views.RPCPreview.ViewType.Loading;
            btnStart.IsEnabled = false;
        });

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Rpc.HasConnection)
            {
                if (sender.Equals(btnAfk))
                {
                    Rpc.StartRpc(Constants.AfkID);
                }
                else
                {
                    Rpc.StartRpc();
                }
            }
            else
            {
                Rpc.StopRpc();
            }
        }

        private void BtnUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            if (Rpc.RichPresence.Timestamps != null &&
                !Rpc.RichPresence.Timestamps.Start.HasValue)
            {
                Rpc.RichPresence.Timestamps.Start = Rpc.RpcStartTime;
            }
            Rpc.Client?.SetPresence(Rpc.RichPresence);
        }

        private void TbAfkReason_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckAfkStatus();
        }

        private async void BtnAfk_OnClick(object sender, RoutedEventArgs e)
        {
            if (tbAfkReason.Text.Length == 0)
            {
                await CustomMessageBox.Show(LanguagePicker.GetLineFromLanguageFile("NeedAfkReason"));
                return;
            }

            Rpc.RichPresence = new RichPresence
            {
                Details = tbAfkReason.Text,
                Assets = new Assets
                {
                    LargeImageKey = "cat",
                    LargeImageText = LanguagePicker.GetLineFromLanguageFile("SleepyCat")
                },
                Timestamps = Settings.Current.AFKTime ? new Timestamps() : null
            };
            tbAfkReason.Clear();
            BtnStart_OnClick(sender, e);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var height = 65d * (double)spMainPages.Children.Count;
            App.Current.Resources["PagesMaxHeight"] = (height > 320d) ?
                height : 320d;
        }

        private void CheckAfkStatus() 
        {
            if (tbAfkReason.Text.Length == 1)
            {
                tbAfkReason.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbAfkReason.ToolTip = new ToolTip(LanguagePicker.GetLineFromLanguageFile("LengthMustBeAtLeast2CharactersLong"));
                btnAfk.IsEnabled = false;
            }
            else
            {
                tbAfkReason.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbAfkReason.ToolTip = null;
                btnAfk.IsEnabled = !Rpc.HasConnection;
            }
        }
    }
}
