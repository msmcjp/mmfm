using System;
using System.Linq;
using System.Collections.Generic;

namespace Msmc.Patterns.Collections
{
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Traverse tree by Pre-order.
        /// </summary>
        /// <typeparam name="TBranch">The type of the branch.</typeparam>
        /// <typeparam name="TLeaf">The type of the leaf.</typeparam>
        /// <param name="node">The node object.</param>
        /// <returns>Pre-order tree traversal iterator.</returns>
        public static IEnumerable<ITreeNode<TBranch, TLeaf>> PreOrder<TBranch, TLeaf>(this ITreeNode<TBranch, TLeaf> node)
        {
            yield return node;

            foreach(var child in node.Values)
            {
                foreach(var cnode in child.PreOrder())
                {
                    yield return cnode;
                }
            }
        }

        /// <summary>
        /// Traverse tree by Post-order.
        /// </summary>
        /// <typeparam name="TBranch">The type of the branch.</typeparam>
        /// <typeparam name="TLeaf">The type of the leaf.</typeparam>
        /// <param name="node">The node object.</param>
        /// <returns>The node iterator.</returns>
        public static IEnumerable<ITreeNode<TBranch, TLeaf>> PostOrder<TBranch, TLeaf>(this ITreeNode<TBranch, TLeaf> node)
        {
            foreach (var child in node.Values)
            {
                foreach (var cnode in child.PostOrder())
                {
                    yield return cnode;
                }
            }

            yield return node;
        }

        /// <summary>
        /// Composite the leaf and the branch to the specified type object.
        /// </summary>
        /// <typeparam name="TBranch">The type of the branch.</typeparam>
        /// <typeparam name="TLeaf">The type of the leaf.</typeparam>
        /// <typeparam name="TComposite">The type of the composite object.</typeparam>
        /// <param name="node">The node object.</param>
        /// <param name="leaf">The composition function which argument is the path of the node and the value of the node.</param>
        /// <param name="branch">The composition function which argument is the path of the node and the values of the children nodes.</param>
        /// <returns>The composite object of `node`.</returns>
        public static TComposite Composite<TBranch, TLeaf, TComposite>(
            this ITreeNode<TBranch, TLeaf> node,
            Func<IEnumerable<TBranch>, TLeaf, TComposite> leaf,
            Func<IEnumerable<TBranch>, IEnumerable<TComposite>, TComposite> branch)
        {
            var stack = new Stack<TComposite>();
            node.PostOrder().Aggregate(stack, (composite, node) =>
            {
                TLeaf value;
                if (node.TryGetLeafValue(out value))
                {
                    stack.Push(leaf(node.Path, value));
                }
                else
                {
                    stack.Push(branch(
                        node.Path, 
                        stack.TakeLast(node.Values.Count()).Aggregate(
                            new List<TComposite>(), 
                            (list, _) =>
                            {
                                list.Insert(0, stack.Pop());
                                return list;
                            }
                        )
                    ));
                }
                return stack;
            });

            TComposite composite;
            stack.TryPeek(out composite);            
            return composite;
        }
    }
}
