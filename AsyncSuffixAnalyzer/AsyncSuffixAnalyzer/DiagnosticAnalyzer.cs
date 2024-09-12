using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;

namespace AsyncSuffixAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncSuffixAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Naming";

        public const string MissingSuffixDiagnosticId = "ASA001";
        private static readonly string MissingSuffixTitle = "Async method names ends with 'Async' suffix";
        private static readonly string MissingSuffixMessageFormat = "Async method name '{0}' should end with 'Async' as the method returns {1}";
        private static readonly string MissingSuffixDescription = "Async method names ends with 'Async' suffix";
        private static DiagnosticDescriptor MissingSuffixRule = new DiagnosticDescriptor(MissingSuffixDiagnosticId, MissingSuffixTitle, MissingSuffixMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: MissingSuffixDescription);

        public const string MissingTaskDiagnosticId = "ASA002";
        private static readonly string MissingTaskTitle = "Method that does not return some kind of Task should not have name that ends with 'Async' suffix";
        private static readonly string MissingTaskMessageFormat = "Method '{0}' returns {1} and so it should rather be called '{2}'";
        private static readonly string MissingTaskDescription = "Method that does not return some kind of Task should not have name that ends with 'Async' suffix";
        private static DiagnosticDescriptor MissingTaskRule = new DiagnosticDescriptor(MissingTaskDiagnosticId, MissingTaskTitle, MissingTaskMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: MissingTaskDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(MissingSuffixRule, MissingTaskRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = context.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
            {
                return;
            }

            var (doesReturnTask, returnTypeName) = DoesReturnTask(methodDeclaration);
            if (doesReturnTask == true)
            {
                // Method is tasked, async suffix is necessary
                if (!DoesEndWithAsync(methodDeclaration))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            MissingSuffixRule,
                            methodDeclaration.Identifier.GetLocation(),
                            methodDeclaration.Identifier.ValueText,
                            returnTypeName));
                }
            }
            else if (doesReturnTask == false)
            {
                // Async suffix should not be there
                if (DoesEndWithAsync(methodDeclaration))
                {
                    var methodName = methodDeclaration.Identifier.ValueText;

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            MissingTaskRule,
                            methodDeclaration.Identifier.GetLocation(),
                            methodName,
                            returnTypeName,
                            methodName.Substring(0, methodName.Length - "Async".Length)));
                }
            }
        }
        
        private static (bool? doesReturnTask, string returnTypeName) DoesReturnTask(MethodDeclarationSyntax syntax)
        {
            var returnType = syntax.ReturnType as SimpleNameSyntax;
            var returnTypeName = returnType?.Identifier.Value as string;

            if (syntax.ReturnType is PredefinedTypeSyntax)
            {
                returnTypeName = ((PredefinedTypeSyntax)syntax.ReturnType).Keyword.ValueText;
            }

            if (returnTypeName == null)
            {
                return (null, "");
            }

            return (returnTypeName == "Task" || returnTypeName == "ValueTask" || returnTypeName == "UniTask", returnTypeName);
        }

        private static bool DoesEndWithAsync(MethodDeclarationSyntax syntax)
        {
            return syntax.Identifier.ValueText.EndsWith("Async");
        }
    }
}
