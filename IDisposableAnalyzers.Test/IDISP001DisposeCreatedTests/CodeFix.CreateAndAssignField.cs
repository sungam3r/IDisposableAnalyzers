namespace IDisposableAnalyzers.Test.IDISP001DisposeCreatedTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    internal partial class CodeFix
    {
        internal class CreateAndAssignField
        {
            private static readonly IDISP001DisposeCreated Analyzer = new IDISP001DisposeCreated();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("IDISP001");
            private static readonly CreateAndAssignFieldCodeFixProvider Fix = new CreateAndAssignFieldCodeFixProvider();

            [Test]
            public void LocalExplicitTypeToFieldInCtor()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    internal sealed class Foo
    {
        internal Foo()
        {
            ↓Stream stream = File.OpenRead(string.Empty);
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    internal sealed class Foo
    {
        private readonly Stream stream;

        internal Foo()
        {
            this.stream = File.OpenRead(string.Empty);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public void LocalVarToFieldInCtor()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    public sealed class Foo
    {
        public Foo()
        {
            ↓var stream = File.OpenRead(string.Empty);
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;

    public sealed class Foo
    {
        private readonly FileStream stream;

        public Foo()
        {
            this.stream = File.OpenRead(string.Empty);
        }
    }
}";
                AnalyzerAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                AnalyzerAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }
        }
    }
}
