using System;
using System.Text;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using MultiRPC.Extensions;
using MultiRPC.Rpc;

namespace MultiRPC.UI.Pages.Rpc
{
    public partial class MultiRpcPage : RpcPage
    {
        public MultiRpcPage() { }

        public override string IconLocation => "Icons/Discord";
        public override string LocalizableName => "MultiRPC";
        public override void Initialize(bool loadXaml)
        {
            InitializeComponent(loadXaml);

            rpcControl.RichPresence = RichPresence;
            rpcControl.Initialize(loadXaml);
        }

        public override RichPresence RichPresence { get; protected set; } = new RichPresence("MultiRPC", Constants.MultiRPCID);
    }
}