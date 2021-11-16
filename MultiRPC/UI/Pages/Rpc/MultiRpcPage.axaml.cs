using System;
using Avalonia;
using Avalonia.Media;
using MultiRPC.Rpc;
using MultiRPC.Rpc.Page;
using MultiRPC.Setting;
using MultiRPC.Setting.Settings;

namespace MultiRPC.UI.Pages.Rpc
{
    public partial class MultiRpcPage : RpcPage
    {
        public override string IconLocation => "Icons/Discord";
        public override string LocalizableName => "MultiRPC";
        public override RichPresence RichPresence { get; protected set; } = SettingManager<MultiRPCSettings>.Setting.Presence;
        public override event EventHandler? PresenceChanged;
        public override bool PresenceValid => rpcControl.RpcValid;
        public override event EventHandler<bool> PresenceValidChanged;

        public override void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            tblLookLike.DataContext = new Language("WhatItWillLookLike");
            rpcView.RpcProfile = RichPresence;
            rpcView.UpdateBackground((IBrush)Application.Current.Resources["PurpleBrush"]!);

            rpcControl.PresenceValidChanged += (sender, b) => PresenceValidChanged?.Invoke(sender, b);
            rpcControl.ProfileChanged += (sender, args) => PresenceChanged?.Invoke(sender, args);
            rpcControl.RichPresence = RichPresence;
            rpcControl.Initialize(loadXaml);
        }
    }
}