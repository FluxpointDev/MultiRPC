﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Svg;
using MultiRPC.UI.Pages;

namespace MultiRPC.UI
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            Button? btnToTrigger = null;
            SidePage? pageToTrigger = null;
            foreach (var page in PageManager.CurrentPages)
            {
                var btn = AddSidePage(page);
                btnToTrigger ??= btn;
                pageToTrigger ??= page;
            }
            PageManager.PageAdded += (sender, page) => AddSidePage(page);

            //TODO: Add autostart
            btnToTrigger?.Classes.Add("selected");
            cclContent.Content = pageToTrigger;
            selectedBtn = btnToTrigger;
        }

        private Button? selectedBtn;
        private Button AddSidePage(SidePage page)
        {
            var btn = new Button
            {
                Content = new Image
                {
                    Margin = new Thickness(4.5),
                    Source = new SvgImage
                    {
                        Source = SvgSource.Load("avares://MultiRPC/Assets/" + page.IconLocation + ".svg", null)
                    }
                }
            };

            btn.Click += delegate
            {
                selectedBtn?.Classes.Remove("selected");

                btn.Classes.Add("selected");
                selectedBtn = btn;
                cclContent.Content = page;
            };

            btn.Classes.Add("nav");
            splPages.Children.Add(btn);
            return btn;
        }
    }
}