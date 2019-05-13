using System.Net;
using DiscordRPC;
using System.Net.Http;
using MultiRPC.Functions;
using System.Globalization;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using ToolTip = MultiRPC.GUI.Controls.ToolTip;

namespace MultiRPC.GUI.Pages
{
    class MultiRPCAndCustomLogic
    {
        public static async Task UpdateTimestamps(CheckBox checkBox)
        {
            RPC.Presence.Timestamps = checkBox.IsChecked.Value ? new Timestamps() : null;
        }

        public static async Task<string> CheckImageText(TextBox textBox)
        {
            string text = textBox.Text;
            if (!Checks.UnderAmountOfBytes(text, 128))
                textBox.Undo();
            return textBox.Text;
        }

        public static async Task<bool> CanRunRPC(TextBox tbText1, TextBox tbText2, TextBox tbSmallText, TextBox tbLargeText, TextBox tbClientID = null, bool TokenTextChanged = false)
        {
            bool isEnabled = true;
            if (tbText2.Text.Length == 1)
            {
                tbText2.SetResourceReference(TextBox.BorderBrushProperty, "Red");
                tbText2.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbText2.SetResourceReference(TextBox.BorderBrushProperty, "AccentColour4SCBrush");
                tbText2.ToolTip = null;
            }
            if (tbText1.Text.Length == 1)
            {
                tbText1.SetResourceReference(TextBox.BorderBrushProperty, "Red");
                tbText1.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbText1.SetResourceReference(TextBox.BorderBrushProperty, "AccentColour4SCBrush");
                tbText1.ToolTip = null;
            }

            if (tbSmallText.Text.Length == 1)
            {
                tbSmallText.SetResourceReference(TextBox.BorderBrushProperty, "Red");
                tbSmallText.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbSmallText.SetResourceReference(TextBox.BorderBrushProperty, "AccentColour4SCBrush");
                tbSmallText.ToolTip = null;
            }
            if (tbLargeText.Text.Length == 1)
            {
                tbLargeText.SetResourceReference(TextBox.BorderBrushProperty, "Red");
                tbLargeText.ToolTip = new ToolTip(App.Text.LengthMustBeAtLeast2CharactersLong);
                isEnabled = false;
            }
            else
            {
                tbLargeText.SetResourceReference(TextBox.BorderBrushProperty, "AccentColour4SCBrush");
                tbLargeText.ToolTip = null;
            }

            bool isCustomPage = MainPage._MainPage.ContentFrame.Content is CustomPage && RPC.Type == RPC.RPCType.Custom;
            var profile = CustomPage.Profiles != null && CustomPage.CurrentButton != null ? CustomPage.Profiles[CustomPage.CurrentButton.Content.ToString()] : null;
            if (isCustomPage && profile != null)
            {
                MainPage._MainPage.btnUpdate.IsEnabled = false;
                MainPage._MainPage.btnStart.IsEnabled = false;
                ulong ID = 0;

                if (App.Config.CheckToken && TokenTextChanged)
                {
                    var isValidCode =
                        ulong.TryParse(tbClientID.Text, NumberStyles.Any, new NumberFormatInfo(), out ID);

                    if ((ID.ToString().Length != tbClientID.MaxLength || !isValidCode))
                    {
                        RPC.IDToUse = 0;
                        tbClientID.SetResourceReference(TextBox.BorderBrushProperty, "Red");
                        tbClientID.ToolTip = !isValidCode
                            ? new ToolTip(App.Text.ClientIDIsNotValid)
                            : new ToolTip(App.Text.ClientIDMustBe18CharactersLong);
                        isEnabled = false;
                    }
                    else
                    {
                        tbClientID.SetResourceReference(TextBox.BorderBrushProperty, "Orange");
                        tbClientID.ToolTip = new ToolTip(App.Text.CheckingClientID);
                        await Task.Delay(1000);
                        HttpResponseMessage T = null;
                        try
                        {
                            HttpClient Client = new HttpClient();
                            T = await Client.PostAsync("https://discordapp.com/api/oauth2/token/rpc",
                                new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                {"client_id", ID.ToString()}
                                }));
                        }
                        catch
                        {
                            if (MainPage._MainPage.ContentFrame.Content is CustomPage && RPC.Type == RPC.RPCType.Custom)
                            {
                                App.Logging.Error("API", App.Text.DiscordAPIDown);
                                tbClientID.ToolTip = new ToolTip($"{App.Text.NetworkIssue}!");
                                tbClientID.SetResourceReference(TextBox.BorderBrushProperty, "Red");
                                isEnabled = false;
                            }
                        }

                        if (MainPage._MainPage.ContentFrame.Content is CustomPage && RPC.Type == RPC.RPCType.Custom && T != null && profile.ClientID == CustomPage.Profiles[CustomPage.CurrentButton.Content.ToString()].ClientID)
                        {
                            if (T.StatusCode == HttpStatusCode.BadRequest)
                            {
                                App.Logging.Error("API", App.Text.ClientIDIsNotValid);
                                tbClientID.ToolTip = new ToolTip(App.Text.ClientIDIsNotValid);
                                tbClientID.SetResourceReference(TextBox.BorderBrushProperty, "Red");
                                isEnabled = false;
                            }
                            else if (T.StatusCode != HttpStatusCode.InternalServerError)
                            {
                                string response = T.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                App.Logging.Error("API", $"{App.Text.APIError} {response}");
                                tbClientID.ToolTip = new ToolTip($"{App.Text.APIIssue}!");
                                tbClientID.SetResourceReference(TextBox.BorderBrushProperty, "Red");
                                isEnabled = false;
                            }
                            else
                            {
                                if (MainPage._MainPage.ContentFrame.Content is CustomPage)
                                    RPC.IDToUse = ID;
                                tbClientID.SetResourceReference(TextBox.BorderBrushProperty, "AccentColour4SCBrush");
                                tbClientID.ToolTip = null;
                            }
                        }
                    }
                }
                else
                {
                    if (tbClientID.BorderBrush == (SolidColorBrush)App.Current.Resources["Red"])
                        isEnabled = false;
                }
            }

            if (MainPage._MainPage.ContentFrame.Content is MultiRPCPage && RPC.Type == RPC.RPCType.MultiRPC || isCustomPage && CustomPage.Profiles[CustomPage.CurrentButton.Content.ToString()] == profile)
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
