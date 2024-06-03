using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ty.SourceGenerator
{
    [Generator]
    public class CustomPageGenerator : IIncrementalGenerator
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
                    if (c.Type is IdentifierNameSyntax fff)
                    {
                        if (fff.Identifier.Text == "ICustomPageViewModel")
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


                //var category = flowStep.ConstructorArguments[0].Value.ToString();
                //var name = flowStep.ConstructorArguments[1].Value.ToString();

                StringBuilder inputStringBuilder = new();

                foreach (var member in item.Option.GetMembers())
                {
                    if (member is IPropertySymbol property)
                    {
                        if (property.IsStatic)
                        {
                            continue;
                        }
                        var memberName = member.Name;

                        var propAttires = property.GetAttributes();

                        var input = propAttires.FirstOrDefault(c => c.AttributeClass.Name == "InputAttribute");

                        if (input is null)
                        {
                            continue;
                        }

                        if (input is not null)
                        {
                            inputStringBuilder.AppendLine($$"""
        {{memberName}} = ICustomPageViewModel.GetValue<{{property.Type.ToDisplayString()}}>("{{memberName}}", inputs);
""");
                        }
                    }
                }

                string baseStr = $@"using Ty;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;
using System.Threading.Tasks;
using Ty.ViewModels.CustomPages;
using Ty.Services.Configs;
using Ty.Module.Configs;

namespace {item.Option.ContainingNamespace};

#nullable enable

public partial class {item.Option.MetadataName}
{{
    public void SetCustomPageValue(List<NameValue> inputs)
    {{
{inputStringBuilder}
    }}

    public static CustomViewDefinition GetDefinition()
    {{
        return new CustomViewDefinition
        {{
            Category = Category,
            Name = Name,
            Data = ConfigManager.GetConfigModel<{item.Option.MetadataName}>(attrs =>
            {{
                if (attrs is null)
                {{
                    return false;
                }}
                return attrs.Any(attr => attr is InputAttribute);
            }})
        }};
    }}
}}
#nullable restore
";

                c.AddSource($"{item.Option.MetadataName}.s.g.cs", SourceText.From(baseStr, Encoding.UTF8));

            });

        }
    }
    public class SyntaxModel
    {
        public ITypeSymbol Option { get; set; }
    }
}
