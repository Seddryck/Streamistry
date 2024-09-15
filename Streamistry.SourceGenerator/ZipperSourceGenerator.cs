using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Resources;
using Scriban;
using System.Reflection;

namespace Streamistry.SourceGenerator;

[Generator]
public class ZipperSourceGenerator : BaseSourceGenerator
{
    public override string GenerateClasses()
    {
        var sb = new StringBuilder();
        sb.Append(ReadTemplate("Header.scriban"))
            .Remove(sb.Length-3, 3)
            .AppendLine(".Pipes.Combinators;")
            .AppendLine();

        for (int i = MIN_CARDINALITY; i <= MAX_CARDINALITY; i++)
        {
            sb.Append(GenerateClass(Classname, i));
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
