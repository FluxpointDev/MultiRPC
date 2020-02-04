using Avalonia.Controls;
using MultiRPC.Core.Enums;

namespace MultiRPC.Avalonia
{
    /// <summary>
    /// Allows pages to define a icon for the side bar
    /// </summary>
    public class PageWithIcon : UserControl
    {
        /// <summary>
        /// The name of the icon, all icons that the side bar needs should be added to <see cref="App.Resources"/>
        /// </summary>
        public virtual MultiRPCIcons IconName { get; } = MultiRPCIcons.Unknown;

        /// <summary>
        /// Text to show from it's name that it has in the json language file
        /// </summary>
        public virtual string JsonContent { get; } = "";

        public override void BeginInit()
        {
            //this.SetResourceReference(MaxHeightProperty, "PagesMaxHeight");
            base.BeginInit();
        }
    }
}
