namespace IDisposableAnalyzers.Test.IDISP003DisposeBeforeReassigningTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class TestFixture
        {
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static readonly DisposeInTearDownFix Fix = new DisposeInTearDownFix();

            [Test]
            public static void AssigningFieldInSetUpCreatesTearDownAndDisposes()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private Disposable disposable;

        [SetUp]
        public void SetUp()
        {
            ↓this.disposable = new Disposable();
        }

        [Test]
        public void Test()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private Disposable disposable;

        [SetUp]
        public void SetUp()
        {
            this.disposable = new Disposable();
        }

        [TearDown]
        public void TearDown()
        {
            this.disposable?.Dispose();
        }

        [Test]
        public void Test()
        {
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
            }

            [Test]
            public static void AssigningFieldInSetUpCreatesTearDownAndDisposesExplicitDisposable()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private ExplicitDisposable disposable;

        [SetUp]
        public void SetUp()
        {
            ↓this.disposable = new ExplicitDisposable();
        }

        [Test]
        public void Test()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private ExplicitDisposable disposable;

        [SetUp]
        public void SetUp()
        {
            this.disposable = new ExplicitDisposable();
        }

        [TearDown]
        public void TearDown()
        {
            ((System.IDisposable)this.disposable)?.Dispose();
        }

        [Test]
        public void Test()
        {
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ExplicitDisposable, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ExplicitDisposable, testCode }, fixedCode);
            }

            [Test]
            public static void AssigningFieldInSetUpdDisposesInTearDown()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private Disposable disposable;

        [SetUp]
        public void SetUp()
        {
            ↓this.disposable = new Disposable();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Test()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private Disposable disposable;

        [SetUp]
        public void SetUp()
        {
            this.disposable = new Disposable();
        }

        [TearDown]
        public void TearDown()
        {
            this.disposable?.Dispose();
        }

        [Test]
        public void Test()
        {
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
            }

            [Test]
            public static void AssigningFieldInSetUpdDisposesInTearDownExplicitDisposable()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private ExplicitDisposable disposable;

        [SetUp]
        public void SetUp()
        {
            ↓this.disposable = new ExplicitDisposable();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Test()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private ExplicitDisposable disposable;

        [SetUp]
        public void SetUp()
        {
            this.disposable = new ExplicitDisposable();
        }

        [TearDown]
        public void TearDown()
        {
            ((System.IDisposable)this.disposable)?.Dispose();
        }

        [Test]
        public void Test()
        {
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { ExplicitDisposable, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { ExplicitDisposable, testCode }, fixedCode);
            }

            [Test]
            public static void AssigningFieldInOneTimeSetUp()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private Disposable disposable;

        [OneTimeSetUp]
        public void SetUp()
        {
            ↓this.disposable = new Disposable();
        }

        [Test]
        public void Test()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private Disposable disposable;

        [OneTimeSetUp]
        public void SetUp()
        {
            this.disposable = new Disposable();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.disposable?.Dispose();
        }

        [Test]
        public void Test()
        {
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
            }

            [Test]
            public static void CreateStaticTeardown()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public static class Tests
    {
        private static Disposable disposable;

        [OneTimeSetUp]
        public static void SetUp()
        {
            ↓disposable = new Disposable();
        }

        [Test]
        public static void Test()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public static class Tests
    {
        private static Disposable disposable;

        [OneTimeSetUp]
        public static void SetUp()
        {
            disposable = new Disposable();
        }

        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
            disposable?.Dispose();
        }

        [Test]
        public static void Test()
        {
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
            }

            [Test]
            public static void AssigningFieldInOneTimeSetUpWhenOneTimeTearDownExists()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private Disposable disposable;

        [OneTimeSetUp]
        public void SetUp()
        {
            ↓this.disposable = new Disposable();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Test()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using NUnit.Framework;

    public class Tests
    {
        private Disposable disposable;

        [OneTimeSetUp]
        public void SetUp()
        {
            this.disposable = new Disposable();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.disposable?.Dispose();
        }

        [Test]
        public void Test()
        {
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, new[] { Disposable, testCode }, fixedCode);
            }
        }
    }
}
