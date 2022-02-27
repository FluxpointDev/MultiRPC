using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Uno.RoslynHelpers;

namespace MultiRPC.SourceGen.Generators;

/// <summary>
/// This takes our en-gb.json language and makes LanguageText for us!
/// </summary>
[Generator]
public class TextEnumGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var langFile = context.AdditionalFiles.First(at => at.Path.EndsWith("en-gb.json")).GetText();
        if (langFile == null)
        {
            return;
        }

        var builder = new IndentedStringBuilder();
        using (builder.BlockInvariant("namespace MultiRPC"))
        {
            using (builder.BlockInvariant("public enum LanguageText"))
            {
                foreach (var line in langFile.Lines)
                {
                    var firstLine = false;
                    var startIndex = 0;
                    var endIndex = 0;
                    var s = line.ToString();
                        
                    //If it contains this then the line is a comment, skip it
                    if (s.Contains("/*"))
                    {
                        continue;
                    }
                        
                    for (int i = 0; i < s.Length; i++)
                    {
                        if (s[i] != '\"')
                        {
                            continue;
                        }
                        if (firstLine)
                        {
                            endIndex = i;
                            break;
                        }
                        
                        firstLine = true;
                        startIndex = i + 1;
                    }

                    if (startIndex > 0 && endIndex > 0)
                    {
                        var li = s.Substring(startIndex, endIndex - startIndex);
                        builder.AppendLineInvariant(li + ",");
                    }
                }                    
            }
        }

        // inject the created source into the users compilation
        context.AddSource("MultiRPC.LanguageText.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }
}