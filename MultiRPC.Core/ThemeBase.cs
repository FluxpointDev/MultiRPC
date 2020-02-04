using JetBrains.Annotations;
using MultiRPC.Core.Enums;

namespace MultiRPC.Core
{
    //ToDo: Wait for themes to be added, edit this to be useable
    public class ThemeBase
    {
        public MultiRPCIcons Icons { get; }
 
        public MultiRPCColours Colours { get; }

        [CanBeNull]
        public string ThemeName { get; }
    }
}
