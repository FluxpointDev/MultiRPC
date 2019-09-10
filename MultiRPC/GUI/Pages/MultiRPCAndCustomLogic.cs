using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DiscordRPC;
using MultiRPC.Functions;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    internal class MultiRPCAndCustomLogic
    {
        private static bool OnCustomPage =>
            MainPage._MainPage.frmContent.Content is MasterCustomPage && RPC.Type == RPC.RPCType.Custom;

        public static Task UpdateTimestamps(CheckBox checkBox)
        {
            RPC.Presence.Timestamps = checkBox.IsChecked.Value ? new Timestamps() : null;

            return Task.CompletedTask;
        }

        public static Task<string> CheckImageText(TextBox textBox)
        {
            var text = textBox.Text;
            if (!text.UnderAmountOfBytes(128))
            {
                textBox.Undo();
            }

            return Task.FromResult(textBox.Text);
        }

        public static async Task<bool> CanRunRPC(TextBox tbText1, TextBox tbText2, TextBox tbSmallText,
            TextBox tbLargeText, TextBox tbClientID = null, bool tokenTextChanged = false)
        {
            var isEnabled = true;
            if (tbText2.Text.Length == 1)
            {
                tbText2.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbText2.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbText2.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbText2.ToolTip = null;
            }

            if (tbText1.Text.Length == 1)
            {
                tbText1.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbText1.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbText1.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbText1.ToolTip = null;
            }

            if (tbSmallText.Text.Length == 1)
            {
                tbSmallText.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbSmallText.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbSmallText.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbSmallText.ToolTip = null;
            }

            if (tbLargeText.Text.Length == 1)
            {
                tbLargeText.SetResourceReference(Control.BorderBrushProperty, "Red");
                tbLargeText.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbLargeText.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                tbLargeText.ToolTip = null;
            }

            var profile = MasterCustomPage.Profiles != null && MasterCustomPage.CurrentButton != null
                ? MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()]
                : null;
            if (OnCustomPage && profile != null)
            {
                if (!RPC.IsRPCRunning)
                {
                    MainPage._MainPage.btnUpdate.IsEnabled = false;
                    MainPage._MainPage.btnStart.IsEnabled = false;
                }

                var isValidCode =
                    ulong.TryParse(tbClientID.Text, NumberStyles.Any, new NumberFormatInfo(), out var ID);

                if (App.Config.CheckToken && tokenTextChanged)
                {
                    if (ID.ToString().Length != tbClientID.MaxLength || !isValidCode)
                    {
                        RPC.IDToUse = 0;
                        tbClientID.SetResourceReference(Control.BorderBrushProperty, "Red");
                        tbClientID.ToolTip = !isValidCode
                            ? new ToolTip(App.Text.ClientIDIsNotValid)
                            : new ToolTip(App.Text.ClientIDMustBe18CharactersLong);
                        isEnabled = false;
                    }
                    else
                    {
                        tbClientID.SetResourceReference(Control.BorderBrushProperty, "Orange");
                        tbClientID.ToolTip = new ToolTip(App.Text.CheckingClientID);
                        await Task.Delay(1000);
                        HttpResponseMessage T = null;
                        try
                        {
                            var Client = new HttpClient();
                            T = await Client.PostAsync("https://discordapp.com/api/oauth2/token/rpc",
                                new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                    {"client_id", ID.ToString()}
                                }));
                        }
                        catch
                        {
                            if (MainPage._MainPage.frmContent.Content is MasterCustomPage && RPC.Type == RPC.RPCType.Custom)
                            {
                                App.Logging.Error("API", App.Text.DiscordAPIDown);
                                tbClientID.ToolTip = new ToolTip($"{App.Text.NetworkIssue}!");
                                tbClientID.SetResourceReference(Control.BorderBrushProperty, "Red");
                                isEnabled = false;
                            }
                        }

                        if (MainPage._MainPage.frmContent.Content is MasterCustomPage && RPC.Type == RPC.RPCType.Custom &&
                            T != null && profile.ClientID ==
                            MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()].ClientID)
                        {
                            if (T.StatusCode == HttpStatusCode.BadRequest)
                            {
                                App.Logging.Error("API", App.Text.ClientIDIsNotValid);
                                tbClientID.ToolTip = new ToolTip(App.Text.ClientIDIsNotValid);
                                tbClientID.SetResourceReference(Control.BorderBrushProperty, "Red");
                                isEnabled = false;
                            }
                            else if (T.StatusCode != HttpStatusCode.Unauthorized)
                            {
                                var response = T.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                App.Logging.Error("API", $"{App.Text.APIError} {response}");
                                tbClientID.ToolTip = new ToolTip($"{App.Text.APIIssue}!");
                                tbClientID.SetResourceReference(Control.BorderBrushProperty, "Red");
                                isEnabled = false;
                            }
                            else
                            {
                                if (MainPage._MainPage.frmContent.Content is MasterCustomPage)
                                {
                                    RPC.IDToUse = ID;
                                }

                                tbClientID.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                                tbClientID.ToolTip = null;
                            }
                        }
                    }
                }
                else if (MainPage._MainPage.frmContent.Content is MasterCustomPage)
                {
                    if (tokenTextChanged)
                    {
                        RPC.IDToUse = ID;
                        tbClientID.SetResourceReference(Control.BorderBrushProperty, "AccentColour4SCBrush");
                        tbClientID.ToolTip = null;
                    }
                    else if (tbClientID.BorderBrush == Application.Current.Resources["Red"])
                    {
                        isEnabled = false;
                    }
                }
            }

            if (MainPage._MainPage.frmContent.Content is MultiRPCPage && RPC.Type == RPC.RPCType.MultiRPC ||
                OnCustomPage && MasterCustomPage.Profiles[MasterCustomPage.CurrentButton.Content.ToString()] == profile && !RPC.AFK)
            {
                if (MainPage._MainPage.btnStart.Content.ToString() == App.Text.Shutdown)
                {
                    MainPage._MainPage.btnUpdate.IsEnabled = isEnabled;
                    MainPage._MainPage.btnStart.IsEnabled = true;
                }
                else
                {
                    MainPage._MainPage.btnUpdate.IsEnabled = false;
                    MainPage._MainPage.btnStart.IsEnabled = isEnabled;
                }
            }

            return isEnabled;
        }
    }
}