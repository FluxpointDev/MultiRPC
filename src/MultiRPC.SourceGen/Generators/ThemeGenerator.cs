using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Uno.RoslynHelpers;
using MultiRPC.Converters;
using System.Text.Json;
using MultiRPC.Theming;
using System.IO.Compression;
using SemVersion;
using Metadata = MultiRPC.Theming.Metadata;

namespace MultiRPC.SourceGen.Generators;

[Generator]
public class ThemeGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var themeFiles = context.AdditionalFiles.Where(at =>
        {
            var ext = Path.GetExtension(at.Path);
            return ext is ".multitheme" or ".multirpctheme";
        });

        var themeComplied = new List<string>();
        var themeListing = new List<string>();
        string? defaultName = null;
        var themesBuilder = new IndentedStringBuilder();
        //usings and namespaces
        themesBuilder.AppendLineInvariant("using System.Collections.Generic;");
        themesBuilder.AppendLineInvariant("using System.Collections.ObjectModel;");
        themesBuilder.AppendLineInvariant("using Avalonia.Media;");

        themesBuilder.AppendLineInvariant("namespace MultiRPC.Theming;");

        var classDisposable = themesBuilder.BlockInvariant("public static class Themes");
        foreach (var themeFile in themeFiles)
        {
            var theme = Load(themeFile.Path);
            if (theme == null)
            {
                continue;
            }

            var themeBuilder = new IndentedStringBuilder();
            var themeName = theme.Metadata.Name.Replace(' ', '_');
            //TODO: Make this not hard coded
            defaultName = "Dark";
            
            //Make theme props
            using (themeBuilder.BlockInvariant($"public static readonly Theme {themeName} = new Theme"))
            {
                //Colour
                using (themeBuilder.BlockInvariant("Colours = new Colours"))
                {
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.ThemeAccentColor, "ThemeAccentColor"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.ThemeAccentColor2, "ThemeAccentColor2"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.ThemeAccentColor2Hover, "ThemeAccentColor2Hover"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.ThemeAccentColor3, "ThemeAccentColor3"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.ThemeAccentColor4, "ThemeAccentColor4"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.ThemeAccentColor5, "ThemeAccentColor5"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.TextColour, "TextColour"));
                    
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.ThemeAccentDisabledColor, "ThemeAccentDisabledColor"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.ThemeAccentDisabledTextColor, "ThemeAccentDisabledTextColor"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.NavButtonSelectedColor, "NavButtonSelectedColor"));
                    themeBuilder.AppendLineInvariant(MakeColourLine(theme.Colours.NavButtonSelectedIconColor, "NavButtonSelectedIconColor"));
                }
                themeBuilder.Append(",");
                
                //Metadata
                themeBuilder.AppendLineInvariant(string.Format("Metadata = new Metadata(\"{0}\", \"{1}\")", theme.Metadata.Name, theme.Metadata.Version));
                themeBuilder.AppendLineInvariant(string.Format("{{{{ Mode = ThemeMode.{0} }}}},", theme.Metadata.Mode.ToString()));

                //Other props
                themeBuilder.AppendLineInvariant(string.Format("Location = \"#{0}\",", theme.Metadata.Name));
                themeBuilder.AppendLineInvariant(string.Format("ThemeType = ThemeType.{0},", theme.ThemeType.ToString()));

                //If the theme contains assets then we also want to tell the theming that it should load them up
                if (theme._hasAssets)
                {
                    themeBuilder.AppendLineInvariant("_hasAssets = true,");
                    themeBuilder.AppendLineInvariant(string.Format("_filepath = \"{0}\",", Path.Combine("Assets", "Themes", Path.GetFileName(themeFile.Path)).Replace(@"\", @"\\")));
                }
            }
            
            //Add to our listing
            themeListing.Add(string.Format("{{{{ {0}.Location, {0} }}}},", themeName));
            themeComplied.Add(themeBuilder.ToString().TrimEnd() + ";");
        }

        themesBuilder.AppendLineInvariant("public static readonly IReadOnlyDictionary<string, Theme> ThemeIndexes;");
        using (themesBuilder.BlockInvariant("static Themes()"))
        {
            //Build index
            using (themesBuilder.BlockInvariant("ThemeIndexes = new ReadOnlyDictionary<string, Theme>(new Dictionary<string, Theme>"))
            {
                themeListing.ForEach(x => themesBuilder.AppendLineInvariant(x));
            }
            themesBuilder.Append(");");
        }
        
        //Write themes
        themeComplied.ForEach(x =>
        {
            foreach (var line in x.Split(new []{ '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    themesBuilder.AppendLineInvariant(line
                        .Replace("{", "{{")
                        .Replace("}", "}}"));
                }
            }
        });

        //We have to add it here or this will be null
        themesBuilder.AppendLineInvariant($"public static readonly Theme Default = {defaultName};");

        //Add to context
        classDisposable.Dispose();
        context.AddSource("Themes.gen.cs", themesBuilder.ToString());
    }

    private static readonly SemanticVersion ModernVersion = new SemanticVersion(7, 0, null);
    /// <summary>
    /// Load's in the theme for being used
    /// </summary>
    /// <param name="filepath">Where the file is</param>
    /// <returns>The theme if we successfully loaded it in</returns>
    public static Theme.Theme? Load(string? filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath))
        {
            return null;
        }

        if (!File.Exists(filepath))
        {
            return null;
        }

        using var fileStream = File.Open(filepath, FileMode.Open);
        ZipArchive archive;
        try
        {
            archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        }
        catch
        {
            return null;
        }
        var coloursEntry = archive.GetEntry("colours.json");
        var metadataEntry = archive.GetEntry("metadataEntry.json");
        if (coloursEntry == null || metadataEntry == null)
        {
            return null;
        }

        var theme = new Theme.Theme
        {
            _hasAssets = archive.Entries.Any(x => x.FullName.StartsWith("Assets/"))
        };
        
        try
        {
            theme.Colours = JsonSerializer.Deserialize<Colours>(coloursEntry.Open(), 
                new JsonSerializerOptions(JsonSerializerDefaults.General)
                { Converters = { new ColourJsonConverter() } })!;
        }
        catch
        {
            archive.Dispose();
            return null;
        }

        try
        {
            theme.Metadata = JsonSerializer.Deserialize<Metadata>(metadataEntry.Open(), 
                new JsonSerializerOptions(JsonSerializerDefaults.General)
                { Converters = { new VersionJsonConverter() } })!;

            if (theme.Metadata.Version >= ModernVersion)
            {
                theme.ThemeType = ThemeType.Modern;
            }
        }
        catch (Exception)
        {
            archive.Dispose();
            return null;
        }

        archive.Dispose();
        return theme;
    }
    
    private static string MakeColourLine(Color color, string name)
    {
        return string.Format("{3} = Color.FromRgb({0}, {1}, {2}),", color.R, color.G, color.B, name);
    }
}