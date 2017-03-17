using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace AsyncSuffixAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncSuffixAnalyzerCodeFixProvider)), Shared]
    public class AsyncSuffixAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string titleForAddition = "Add 'Async' to method name";
        private const string titleForRemoval = "Remove 'Async' from method name";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AsyncSuffixAnalyzer.MissingSuffixDiagnosticId, AsyncSuffixAnalyzer.MissingTaskDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent as MethodDeclarationSyntax;

            if (diagnostic.Id == "ASA001")
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: titleForAddition,
                        createChangedSolution: c => AddAsync(context.Document, declaration, c),
                        equivalenceKey: titleForAddition),
                    diagnostic);
            }
            else if (diagnostic.Id == "ASA002")
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: titleForRemoval,
                        createChangedSolution: c => RemoveAsync(context.Document, declaration, c),
                        equivalenceKey: titleForRemoval),
                    diagnostic);
            }
        }

        private Task<Solution> AddAsync(Document document, MethodDeclarationSyntax syntax, CancellationToken cancellationToken)
            => GiveNewName(
                syntax.Identifier.ValueText + "Async",
                document, syntax, cancellationToken);

        private Task<Solution> RemoveAsync(Document document, MethodDeclarationSyntax syntax, CancellationToken cancellationToken)
            => GiveNewName(
                syntax.Identifier.ValueText.Substring(0, syntax.Identifier.ValueText.Length - "Async".Length),
                document, syntax, cancellationToken);

        private async Task<Solution> GiveNewName(string newName, Document document, MethodDeclarationSyntax syntax, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var semantics = semanticModel.GetDeclaredSymbol(syntax, cancellationToken);
            var solution = await Renamer.RenameSymbolAsync(document.Project.Solution, semantics, newName, document.Project.Solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
            return solution;
        }
    }
}