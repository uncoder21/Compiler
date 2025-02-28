using System;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Compiler.Generators;

[Generator(LanguageNames.CSharp)]
public class BoundNodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource(
           "BoundPatternAttribute.Generated.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        var classDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            "BoundPatternAttribute",
            IsSyntaxTargetForGeneration,
            GetSemanticTargetForGeneration)
            .Collect();

        context.RegisterSourceOutput(classDeclarations, Execute);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode, CancellationToken ct)
    {
        return syntaxNode is ClassDeclarationSyntax;
    }

    private static ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        return (ClassDeclarationSyntax)context.TargetNode;
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax> classDeclarations)
    {
        var sourceBuilder = new StringBuilder();

        sourceBuilder.AppendLine("namespace Compiler.Core.Sharp;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("internal partial class BoundVisitor");
        sourceBuilder.AppendLine("{");

        foreach (var classDeclaration in classDeclarations)
        {
            sourceBuilder.AppendLine(GetBoundNodeVisitorMethod(classDeclaration));
        }

        sourceBuilder.AppendLine("}");

        context.AddSource("BoundNode.Generated.cs", sourceBuilder.ToString());
    }

    private static string GetBoundNodeVisitorMethod(ClassDeclarationSyntax classDeclaration)
    {
        var className = classDeclaration.Identifier.Text;

        var methodName = $"Visit{className}";

        return $@"
    public virtual BoundNode {methodName}({className} node)
    {{
        return DefaultVisit(node);
    }}";
    }
}

public static class SourceGenerationHelper
{
    public const string Attribute = @"
[AttributeUsage(AttributeTargets.Class)]
public sealed class BoundPatternAttribute : Attribute
{

}";
}
