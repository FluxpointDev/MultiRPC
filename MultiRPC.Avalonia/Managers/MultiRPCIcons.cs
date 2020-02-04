using Avalonia.Media;
using System;
using static MultiRPC.Core.Enums.MultiRPCIcons;

namespace MultiRPC.Managers
{
    /// <inheritdoc cref="Core.Managers.IMultiRPCIcons{TIcon}"/>
    public class MultiRPCIcons : Core.Managers.IMultiRPCIcons<SolidColorBrush>
    {
        public SolidColorBrush EnumToIcon(Core.Enums.MultiRPCIcons multirpcIcon)
        {
            throw new NotImplementedException();
        }

        public string EnumToString(Core.Enums.MultiRPCIcons multirpcColours) => EnumToString(multirpcColours, false);

        /// <inheritdoc cref="Core.Managers.IMultiRPCIcons{TIcon}.EnumToString(Core.Enums.MultiRPCIcons)"/>
        public string EnumToString(Core.Enums.MultiRPCIcons multirpcColours, bool selected) =>
        multirpcColours switch
        {
            Shield => $"ShieldIcon{(selected ? "Selected" : "")}DrawingImage",
            Browser => $"BrowserIcon{(selected ? "Selected" : "")}DrawingImage",
            Discord => $"DiscordIcon{(selected ? "Selected" : "")}DrawingImage",
            Custom => $"CustomIcon{(selected ? "Selected" : "")}DrawingImage",
            Add => $"AddIcon{(selected ? "Selected" : "")}DrawingImage",
            Github => $"GithubIcon{(selected ? "Selected" : "")}DrawingImage",
            Pencil => $"PencilIcon{(selected ? "Selected" : "")}DrawingImage",
            Heart => $"HeartIcon{(selected ? "Selected" : "")}DrawingImage",
            Credits => $"CreditsIcon{(selected ? "Selected" : "")}DrawingImage",
            Delete => $"DeleteIcon{(selected ? "Selected" : "")}DrawingImage",
            DiscordColour => $"CustomColourIcon{(selected ? "Selected" : "")}DrawingImage",
            Download => $"DownloadIcon{(selected ? "Selected" : "")}DrawingImage",
            Share => $"ShareIcon{(selected ? "Selected" : "")}DrawingImage",
            Help => $"HelpIcon{(selected ? "Selected" : "")}DrawingImage",
            Settings => $"SettingsIcon{(selected ? "Selected" : "")}DrawingImage",
            Logs => $"LogsIcon{(selected ? "Selected" : "")}DrawingImage",
            Debug => $"DebugIcon{(selected ? "Selected" : "")}DrawingImage",
            Theme => $"ThemeIcon{(selected ? "Selected" : "")}DrawingImage",
            Alert => $"AlertIcon{(selected ? "Selected" : "")}DrawingImage",
            Warning => $"WarningIcon{(selected ? "Selected" : "")}DrawingImage",
            Info => $"InfoIcon{(selected ? "Selected" : "")}DrawingImage",
            Programs => $"ProgramsIcon{(selected ? "Selected" : "")}DrawingImage",
            Fluxpoint => $"FluxpointIcon{(selected ? "Selected" : "")}DrawingImage",
            Unknown => $"UnknownIcon{(selected ? "Selected" : "")}DrawingImage",
            _ => $"UnknownIcon{(selected ? "Selected" : "")}DrawingImage"
        };

        public Core.Enums.MultiRPCIcons IconToEnum(SolidColorBrush multirpcIcon)
        {
            throw new NotImplementedException();
        }
    }
}
