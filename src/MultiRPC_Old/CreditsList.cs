using System.Text.Json.Serialization;

namespace MultiRPC;

public class CreditsList
{
    public CreditsList(string[] admins, string[] patreon, string[] paypal)
    {
        Admins = admins;
        Patreon = patreon;
        Paypal = paypal;
    }

    public string[] Admins { get; }
    public string[] Patreon { get; }
    public string[] Paypal { get; }
}