using MultiRPC.Data;
using MultiRPC.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MultiRPC.GUI.Pages;

namespace MultiRPC.GUI
{
    /// <summary>
    /// Interaction logic for ShareWindow.xaml
    /// </summary>
    public partial class ShareWindow : Window
    {
        public ShareWindow(CustomProfile profile)
        {
            InitializeComponent();
            Profile = profile;
            Title = $"Share - {Profile.Name}";
        }
        private CustomProfile Profile;

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextImport.Text))
            {
                MessageBox.Show("You need to enter a profile string to copy", "No text", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            string Decode = "";
            try
            {
                Decode = Utils.Base64Decode(TextImport.Text);
                if (string.IsNullOrEmpty(Decode))
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Could not decode the input string", "Decode failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            CustomProfile New = null;
            try
            {
                New = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomProfile>(Decode);
                if (New == null)
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Could not load json from input string", "Invalid json", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            int Count = 1;
            string Name = New.Name;
            while (_Data.Profiles.ContainsKey(New.Name))
            {
                New.Name = $"{Name}{Count}";
                Count++;
            }
            Button btn = New.GetButton();
            btn.Click += MainPage.ProfileBtn_Click;
            _Data.Profiles.Add(New.Name, New);
            _Data.SaveProfiles();
            App.WD.MenuProfiles.Items.Add(btn);
            App.WD.CheckProfileMenuWidth();
            Close();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            string Get = Newtonsoft.Json.JsonConvert.SerializeObject(Profile);
            Clipboard.SetText(Utils.Base64Encode(Get));
            MessageBox.Show("Profile has been copied to clipboard (ctrl + v) to share with others");
        }
    }
}
