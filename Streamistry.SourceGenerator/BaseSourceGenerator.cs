using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace Streamistry.SourceGenerator;
public abstract class BaseSourceGenerator : ISourceGenerator
{
    public const int MIN_CARDINALITY = 2;
    public const int MAX_CARDINALITY = 7;

    public string Classname
    {
        get
        {
            var classname = GetType().Name;
            return classname.Substring(0, classname.Length - 15);
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var source = GenerateClasses();
        context.AddSource($"{Classname}.gen.cs", SourceText.From(source, Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context)
    { }

    public abstract string GenerateClasses();

    public string ReadTemplate(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string fullResourceName = $"Streamistry.SourceGenerator.{resourceName}";

        using (var stream = assembly.GetManifestResourceStream(fullResourceName))
        {
            if (stream is null)
                throw new FileNotFoundException("Embedded resource not found.", fullResourceName);

            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }
    }

    public string GenerateClass(string classname, int numGenerics)
    {
        var templateText = ReadTemplate($"{classname}.scriban");
        var template = Template.Parse(templateText);

        var generics = new List<string>();
        var indexes = new List<int>();
        for (int i = 1; i <= numGenerics; i++)
        {
            generics.Add($"T{i}");
            indexes.Add(i);
        }

        var model = new
        {
            generics = generics.ToArray(),
            indexes = indexes.ToArray()
        };

        string result = template.Render(model);
        return result;
    }
}
