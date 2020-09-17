namespace MultiRPC.Core
{
    public class CreditsList
    {
        public CreditsList(string[] admins, string[] patreon, string[] paypal)
        {
            Admins = admins;
            Patreon = patreon;
            Paypal = paypal;
        }

        /// <summary>
        /// All the admins in the discord server
        /// </summary>
        public string[] Admins { get; }

        /// <summary>
        /// All the people who pledge to Fluxpoint Development's Patreon
        /// </summary>
        public string[] Patreon { get; }

        /// <summary>
        /// All the people who have given money to Builder through paypal
        /// </summary>
        public string[] Paypal { get; }
    }
}