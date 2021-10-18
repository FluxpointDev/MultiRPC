﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MultiRPC.UI.Pages
{
    public partial class MissingPage : SidePage
    {
        public MissingPage() { }

        public override string IconLocation => "Icons/Discord";
        public override string LocalizableName => "Missing";
        
        public override void Initialize(bool loadXaml) => InitializeComponent(loadXaml);
    }
}