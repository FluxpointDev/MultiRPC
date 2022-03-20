using System;
using MultiRPC.UI.Pages;

namespace MultiRPC.Rpc.Page;

public interface IRpcPage : ISidePage
{
    public RichPresence RichPresence { get; }
    public bool PresenceValid { get; }
    public event EventHandler? PresenceChanged;
    public event EventHandler<bool> PresenceValidChanged;
}