using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msmc.Patterns.Tree;
using System;
using System.Collections.Generic;

namespace TreeNode.Tests
{
    [TestClass()]
    public class TreeNodeTest
    {
        [TestMethod]
        public void TestAdd()
        {
            var tree = new TreeNode<string, string>();
            Assert.ThrowsException<ArgumentNullException>(() => tree.Add(null));
            Assert.ThrowsException<ArgumentNullException>(() => tree.Add(null, "TEST"));

            tree.Add("A");
            Assert.AreEqual(1, tree.Count);
            Assert.ThrowsException<ArgumentException>(() => tree.Add("A"));
            Assert.ThrowsException<ArgumentException>(() => tree.Add("A", "TEST"));

            tree["A"].Add("B", "TEST");
            Assert.AreEqual(1, tree["A"].Count);
        }

        [TestMethod]
        public void TestHierarchyAccess()
        {
            var tree = new TreeNode<string, string>();

            Assert.ThrowsException<KeyNotFoundException>(() => tree["A.B.C".Split(".")]);

            var keys = "A.B.C".Split(".");
            tree[keys] = "TEST";
            Assert.IsTrue(tree[keys] == "TEST");
        }

        [TestMethod]
        public void TestRemove()
        {
            var tree = new TreeNode<string, string>();
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
            var tree = new TreeNode<string, string>();
            tree.Add("A", "TEST");
            tree.Add("B", "TEST");

            Assert.IsTrue(tree.Count == 2);

            tree.Clear();

            Assert.IsTrue(tree.Count == 0);
        }

        [TestMethod]
        public void TestTryGetLeaf()
        {
            var tree = new TreeNode<string, string>();

            tree.Add("A");
            tree.Add("B", "TEST");

            Assert.IsFalse(tree["A"].TryGetLeafValue(out _));

            string leaf;
            Assert.IsTrue(tree["B"].TryGetLeafValue(out leaf));
            Assert.AreEqual("TEST", leaf);
        }
    }
}
