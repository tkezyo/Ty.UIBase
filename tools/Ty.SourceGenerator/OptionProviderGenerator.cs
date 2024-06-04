using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ty.SourceGenerator
{
    [Generator]
    public class OptionProviderGenerator : IIncrementalGenerator
    {
        private bool Condition(SyntaxNode node, CancellationToken cancellationToken)
        {
            if (node is ClassDeclarationSyntax ids)
            {
                //判断ids是否继承了IStep
                if (ids.BaseList is null)
                {
                    return false;
                }
                return ids.BaseList.Types.Any(c =>
                {
                    if (c.Type is GenericNameSyntax ff)
                    {
                      
                        if (ff.Identifier.Text == "IOptionProvider" && ff.TypeArgumentList.Arguments.Any())
                        {
                            return true;
                        }
                    }

                    return false;
                });
                //if (ids.AttributeLists.Any(v => v.Attributes.Any(c =>
                //{
                //    if (c.Name is IdentifierNameSyntax ff && ff.Identifier.Text == "FlowStep" || (c.Name is GenericNameSyntax fc && fc.Identifier.Text == "FlowConverter"))
                //    {
                //        return true;
                //    }
                //    return false;
                //})))
                //{
                //    return true;
                //}
            }
            return false;
        }
        private SyntaxModel Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
        {
            var step = context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;

            return new SyntaxModel
            {
                Option = step
            };
        }
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.SyntaxProvider.CreateSyntaxProvider<SyntaxModel>(Condition, Transform), (c, item) =>
            {
                var attires = item.Option.GetAttributes();
            
                var optionProvider = item.Option.Interfaces.Any(c => c.Name == "IOptionProvider");

                if (optionProvider)
                {
                    var type = item.Option.AllInterfaces.FirstOrDefault(c => c.Name == "IOptionProvider");
                    if (type.TypeArguments.Any())
                    {
                        type = type.TypeArguments[0] as INamedTypeSymbol;
                    }

                    string baseStr = $@"

namespace {item.Option.ContainingNamespace};

#nullable enable

    public partial class {item.Option.MetadataName}
    {{
        public const string FullName = ""{type.ToDisplayString()}"" + "":{item.Option.ContainingNamespace}.{item.Option.MetadataName}"";
        public static string Name => typeof({item.Option.MetadataName}).FullName ?? string.Empty;

        public static string Type => ""{type.ToDisplayString()}"";
    }}
#nullable restore
";

                    c.AddSource($"{item.Option.MetadataName}.c.g.cs", SourceText.From(baseStr, Encoding.UTF8));
                }
            });

        }
    }
}
