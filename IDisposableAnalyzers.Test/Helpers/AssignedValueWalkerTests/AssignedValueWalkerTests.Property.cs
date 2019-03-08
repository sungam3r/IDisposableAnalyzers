namespace IDisposableAnalyzers.Test.Helpers.AssignedValueWalkerTests
{
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public partial class AssignedValueWalkerTests
    {
        [TestCase("var temp1 = this.M;", "1")]
        [TestCase("var temp2 = this.M;", "1, 2")]
        [TestCase("var temp3 = this.M;", "1, 2")]
        public void AutoPropertyGetSetAssignedInCtor(string code, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
public sealed class C
{
    public C()
    {
        var temp1 = this.M;
        this.M = 2;
        var temp2 = this.M;
    }

    public int M { get; set; } = 1;

    public void Meh()
    {
        var temp3 = this.M;
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("var temp1 = this.M;", "1")]
        [TestCase("var temp2 = this.M;", "1, 2")]
        [TestCase("var temp3 = this.M;", "1, 2")]
        public void AutoPropertyGetOnlyAssignedInCtor(string code, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
public sealed class C
{
    public C()
    {
        var temp1 = this.M;
        this.M = 2;
        var temp2 = this.M;
    }

    public int M { get; } = 1;

    public void Meh()
    {
        var temp3 = this.M;
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("var temp1 = this.bar;", "1")]
        [TestCase("var temp2 = this.M;", "")]
        [TestCase("var temp3 = this.bar;", "1, 2")]
        [TestCase("var temp4 = this.M;", "")]
        [TestCase("var temp5 = this.bar;", "1, 2")]
        [TestCase("var temp6 = this.M;", "")]
        public void BackingFieldPrivateSetInitializedAndAssignedInCtor(string code1, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
public sealed class C
{
    private int bar = 1;

    public C()
    {
        var temp1 = this.bar;
        var temp2 = this.M;
        this.bar = 2;
        var temp3 = this.bar;
        var temp4 = this.M;
    }

    public int M
    {
        get { return this.bar; }
        private set { this.bar = value; }
    }

    public void Meh()
    {
        var temp5 = this.bar;
        var temp6 = this.M;
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code1).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("var temp1 = this.bar;", "1")]
        [TestCase("var temp2 = this.M;", "")]
        [TestCase("var temp3 = this.bar;", "1, 2")]
        [TestCase("var temp4 = this.M;", "")]
        [TestCase("var temp5 = this.bar;", "1, 2, value")]
        [TestCase("var temp6 = this.M;", "")]
        public void BackingFieldPublicSetInitializedAndAssignedInCtor(string code, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public sealed class C
    {
        private int bar = 1;

        public C()
        {
            var temp1 = this.bar;
            var temp2 = this.M;
            this.bar = 2;
            var temp3 = this.bar;
            var temp4 = this.M;
        }

        public int M
        {
            get { return this.bar; }
            set { this.bar = value; }
        }

        public void Meh()
        {
            var temp5 = this.bar;
            var temp6 = this.M;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void BackingFieldPublicSetSimple()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public sealed class C
    {
        private int bar;

        public int M
        {
            get { return this.bar; }
            set { this.bar = value; }
        }

        public void Meh()
        {
            var temp = this.bar;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause("var temp = this.bar").Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
#pragma warning disable GU0006 // Use nameof.
                Assert.AreEqual("value", actual);
#pragma warning restore GU0006 // Use nameof.
            }
        }

        [Test]
        public void BackingFieldPrivateSetSimple()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public sealed class C
    {
        private int bar;

        public int M
        {
            get { return this.bar; }
            private set { this.bar = value; }
        }

        public void Meh()
        {
            var temp = this.bar;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause("var temp = this.bar").Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(string.Empty, actual);
            }
        }

        [TestCase("var temp1 = this.bar;", "1")]
        [TestCase("var temp2 = this.M;", "")]
        [TestCase("var temp3 = this.bar;", "1, 2")]
        [TestCase("var temp4 = this.M;", "2")]
        [TestCase("var temp5 = this.bar;", "1, 2")]
        [TestCase("var temp6 = this.M;", "2")]
        public void BackingFieldPrivateSetInitializedAndPropertyAssignedInCtor(string code, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
public sealed class C
{
    private int bar = 1;

    public C()
    {
        var temp1 = this.bar;
        var temp2 = this.M;
        this.M = 2;
        var temp3 = this.bar;
        var temp4 = this.M;
    }

    public int M
    {
        get { return this.bar; }
        private set { this.bar = value; }
    }

    public void Meh()
    {
        var temp5 = this.bar;
        var temp6 = this.M;
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("var temp1 = this.bar;", "1")]
        [TestCase("var temp2 = this.M;", "")]
        [TestCase("var temp3 = this.bar;", "1, 2")]
        [TestCase("var temp4 = this.M;", "2")]
        [TestCase("var temp5 = this.bar;", "1, 2, value")]
        [TestCase("var temp6 = this.M;", "2")]
        public void BackingFieldPublicSetInitializedAndPropertyAssignedInCtor(string code, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
public sealed class C
{
    private int bar = 1;

    public C()
    {
        var temp1 = this.bar;
        var temp2 = this.M;
        this.M = 2;
        var temp3 = this.bar;
        var temp4 = this.M;
    }

    public int M
    {
        get { return this.bar; }
        set { this.bar = value; }
    }

    public void Meh()
    {
        var temp5 = this.bar;
        var temp6 = this.M;
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("var temp1 = this.bar;", "1")]
        [TestCase("var temp2 = this.M;", "")]
        [TestCase("var temp3 = this.bar;", "1, 2, value / 2, 3")]
        [TestCase("var temp4 = this.M;", "2")]
        [TestCase("var temp5 = this.bar;", "1, 2, value / 2, 3, value, value")]
        [TestCase("var temp6 = this.M;", "2")]
        public void BackingFieldPublicSetInitializedAndPropertyAssignedInCtorWeirdSetter(string code, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public sealed class C
    {
        private int bar = 1;

        public C()
        {
            var temp1 = this.bar;
            var temp2 = this.M;
            this.M = 2;
            var temp3 = this.bar;
            var temp4 = this.M;
        }

        public int M
        {
            get { return this.bar; }
            set
            {
                if (true)
                {
                    this.bar = value;
                }
                else
                {
                    this.bar = value;
                }

                this.bar = value / 2;
                this.bar = 3;
            }
        }

        public void Meh()
        {
            var temp5 = this.bar;
            var temp6 = this.M;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("var temp1 = this.M;", "")]
        [TestCase("var temp2 = this.M;", "2")]
        [TestCase("var temp3 = this.M;", "2, value")]
        public void RecursiveGetAndSet(string code, string expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public sealed class C
    {
        public C()
        {
            var temp1 = this.M;
            this.M = 2;
            var temp2 = this.M;
        }

        public int M
        {
            get { return this.M; }
            set { this.M = value; }
        }

        public void Meh()
        {
            var temp3 = this.M;
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var value = syntaxTree.FindEqualsValueClause(code).Value;
            using (var assignedValues = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Recursive()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class C
    {
        private M bar1;
        private M bar2;

        public M M1
        {
            get
            {
                return this.bar1;
            }

            set
            {
                if (Equals(value, this.bar1))
                {
                    return;
                }

                if (value != null && this.bar2 != null)
                {
                    this.M2 = null;
                }

                if (this.bar1 != null)
                {
                    this.bar1.Selected = false;
                }

                this.bar1 = value;
                if (this.bar1 != null)
                {
                    this.bar1.Selected = true;
                }
            }
        }

        public M M2
        {
            get
            {
                return this.bar2;
            }

            set
            {
                if (Equals(value, this.bar2))
                {
                    return;
                }

                if (value != null && this.bar1 != null)
                {
                    this.M1 = null;
                }

                if (this.bar2 != null)
                {
                    this.bar2.Selected = false;
                }

                this.bar2 = value;
                if (this.bar2 != null)
                {
                    this.bar2.Selected = true;
                }
            }
        }
    }

    public class M
    {
        public bool Selected { get; set; }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var fieldDeclaration = syntaxTree.FindFieldDeclaration("bar1");
            var field = semanticModel.GetDeclaredSymbolSafe(fieldDeclaration, CancellationToken.None);
            using (var assignedValues = AssignedValueWalker.Borrow(field, semanticModel, CancellationToken.None))
            {
                var actual = string.Join(", ", assignedValues);
                Assert.AreEqual("null, value", actual);
            }
        }
    }
}
