using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Msmc.Patterns.Collections;

namespace TreeNode.Tests
{
    [TestClass()]
    public class TreeNodeTest
    {
        [TestMethod]
        public void TestAdd()
        {
            var tree = new TreeNode<string, string>("");
            Assert.ThrowsException<ArgumentNullException>(() => tree.Add(null));
            Assert.ThrowsException<ArgumentNullException>(() => tree.Add(null, "TEST"));

            tree.Add("A");
            Assert.AreEqual(1, tree.Count);
            Assert.ThrowsException<ArgumentException>(() => tree.Add("A"));
            Assert.ThrowsException<ArgumentException>(() => tree.Add("A", "TEST"));

            tree["A"].Add("B", "TEST");
            Assert.AreEqual(1, tree["A"].Count);
            string value;
            Assert.IsTrue(tree["A"]["B"].TryGetLeafValue(out value));
            Assert.AreEqual("TEST", value);
            Assert.AreEqual(".A.B", string.Join(".", tree["A"]["B"].Path));
        }

        [TestMethod]
        public void TestHierarchyAccess()
        {
            var tree = new TreeNode<string, string>("");

            Assert.ThrowsException<KeyNotFoundException>(() => tree["A.B.C".Split(".")]);

            var keys = "A.B.C".Split(".");
            tree[keys] = "TEST";
            Assert.IsTrue(tree[keys] == "TEST");
        }

        [TestMethod]
        public void TestRemove()
        {
            var tree = new TreeNode<string, string>("");
            tree.Add("A", "TEST");
            tree.Add("B", "TEST");

            Assert.IsTrue(tree.Remove("A"));
            Assert.IsTrue(tree.Count == 1);

            Assert.IsTrue(tree.Remove("B"));
            Assert.IsTrue(tree.Count == 0);

            Assert.ThrowsException<ArgumentNullException>(() => tree.Remove(null));
            Assert.IsFalse(tree.Remove("A"));
        }

        [TestMethod]
        public void TestClear()
        {
            var tree = new TreeNode<string, string>("");
            tree.Add("A", "TEST");
            tree.Add("B", "TEST");

            Assert.IsTrue(tree.Count == 2);

            tree.Clear();

            Assert.IsTrue(tree.Count == 0);
        }

        [TestMethod]
        public void TestTryGetLeaf()
        {
            var tree = new TreeNode<string, string>("");

            tree.Add("A");
            tree.Add("B", "TEST");

            Assert.IsFalse(tree["A"].TryGetLeafValue(out _));

            string leaf;
            Assert.IsTrue(tree["B"].TryGetLeafValue(out leaf));
            Assert.AreEqual("TEST", leaf);
        }

        [TestMethod]
        public void TestPreOrder()
        {
            var tree = new TreeNode<string, string>("");

            tree["F.B.A".Split(".")] = "";
            tree["F.B.D.C".Split(".")] = "";
            tree["F.B.D.E".Split(".")] = "";
            tree["F.G.I.H".Split(".")] = "";
            
            Assert.AreEqual("FBADCEGIH", tree.PreOrder().Aggregate("", (x, n) => x + n.Path.Last()));
        }

        [TestMethod]
        public void TestPostOrder()
        {
            var tree = new TreeNode<string, string>("");

            tree["F.B.A".Split(".")] = "";
            tree["F.B.D.C".Split(".")] = "";
            tree["F.B.D.E".Split(".")] = "";
            tree["F.G.I.H".Split(".")] = "";
            
            Assert.AreEqual("ACEDBHIGF", tree.PostOrder().Aggregate("", (x, n) => x + n.Path.Last()));
        }

        [TestMethod]
        public void TestComposite()
        {
            var tree = new TreeNode<string, int>("=");
            int a = 1, b = 2, c = 3, d = 4, e = 5;

            tree["+.*.a".Split(".")] = a;
            tree["+.*.-.b".Split(".")] = b;
            tree["+.*.-.c".Split(".")] = c;
            tree["+.+.d".Split(".")] = d;
            tree["+.+.e".Split(".")] = e;

            var operators = new Dictionary<string, Func<IEnumerable<int>, int>> 
            {
                { "+", (x) => x.ElementAt(0) + x.ElementAt(1) },
                { "-", (x) => x.ElementAt(0) - x.ElementAt(1) },
                { "*", (x) => x.ElementAt(0) * x.ElementAt(1) },
                { "/", (x) => x.ElementAt(0) / x.ElementAt(1) },
                { "=", (x) => x.ElementAt(0) },
            };

            var result = tree.Composite( 
                (_, variable) => variable, 
                (path, operand) => operators[path.Last()](operand)
            );
            Assert.AreEqual(a * (b - c) + (d + e), result);
        }
    }
}
