using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using AsyncSuffixAnalyzer;

namespace AsyncSuffixAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptySourceTriggerNoDiagnostic()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }
        
        [TestMethod]
        public void MethodReturningGenericTaskWithoutTheSuffixProducesDiagnosticAndFix()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public Task<int> DoStuff()
            {
                return Task.FromResult(5);
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "ASA001",
                Message = "Async method name 'DoStuff' should end with 'Async' as the method returns Task",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 30)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public Task<int> DoStuffAsync()
            {
                return Task.FromResult(5);
            }
        }
    }";
            // VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void MethodNotReturningTaskWithTheSuffixProducesDiagnosticAndFix()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public Int32 DoStuffAsync()
            {
                return Task.FromResult(5);
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "ASA002",
                Message = "Method 'DoStuffAsync' returns Int32 and so it should rather be called 'DoStuff'",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 26)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public Task<int> DoStuffAsync()
            {
                return Task.FromResult(5);
            }
        }
    }";
            // VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AsyncSuffixAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AsyncSuffixAnalyzer();
        }
    }
}