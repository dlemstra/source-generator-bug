using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator;

[Generator]
internal sealed class ColorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context => AddAttributeSource<ColorGeneratorAttribute>(context));

        var valuesProvider = context.SyntaxProvider.ForAttributeWithMetadataName(typeof(ColorGeneratorAttribute).FullName, (_, _) => true, (_, _) => true);
        context.RegisterSourceOutput(valuesProvider, GenerateCode);
    }

    private void AddAttributeSource<TAttribute>(IncrementalGeneratorPostInitializationContext context)
    {
        var type = typeof(TAttribute);

        using var stream = type.Assembly.GetManifestResourceStream(type.Name) ?? throw new InvalidOperationException();

        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var sourceCode = streamReader.ReadToEnd();

        context.AddSource(
             $"{type.Name}.g.cs",
             SourceText.From(sourceCode, Encoding.UTF8));
    }

    private void GenerateCode(SourceProductionContext context, bool source)
    {
        var codeBuilder = new StringBuilder();
        codeBuilder.AppendLine("using System;");
        codeBuilder.AppendLine("internal sealed partial class GeneratedCode {");
        codeBuilder.AppendLine("  public static void PrintColor() {");
        codeBuilder.AppendLine("    Console.ForegroundColor = ConsoleColor.Magenta;");

        if (HasColorWithName("Red"))
        {
            codeBuilder.AppendLine(@"    Console.WriteLine(""ColorGenerator.HasColorWithName returns true for Red"");");
        }

        if (HasColorWithName("RebeccaPurple"))
        {
            codeBuilder.AppendLine(@"    Console.WriteLine(""ColorGenerator.HasColorWithName returns true for RebeccaPurple"");");
        }

        codeBuilder.AppendLine("    Console.ResetColor();");
        codeBuilder.AppendLine("  }");
        codeBuilder.AppendLine("}");

        context.AddSource("Colors.g.cs", SourceText.From(codeBuilder.ToString(), Encoding.UTF8));
    }

    private static bool HasColorWithName(string name)
    {
        var type = typeof(Color);
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Any(property => property.PropertyType == type && property.Name == name);
    }
}
