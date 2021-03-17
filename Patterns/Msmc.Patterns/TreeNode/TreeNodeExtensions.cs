using System;
using System.Collections.Generic;

namespace Msmc.Patterns.Tree
{
    public static class TreeNodeExtensions
    {
        public static void Traverse<TBranch, TLeaf>(this ITreeNode<TBranch, TLeaf> tree, Action<IEnumerable<ITreeNode<TBranch, TLeaf>>> action)
        {

        }

        /// <summary>
        /// Composite the leaf and the branch to the specified type object.
        /// </summary>
        /// <param name="leaf">The function to composite the leaf.</param>
        /// <param name="branch">The function to composite the branch.</param>
        /// <typeparam name="T">the specified type to composite</typeparam>
        /// <returns>The composite object.</returns>
        public static TLeaf Composite<TBranch, TLeaf>(this ITreeNode<TBranch, TLeaf> tree, Func<TLeaf, TBranch> leaf, Func<IEnumerable<(TBranch, TLeaf)>, TLeaf> branch)
        {
            return default(TLeaf);
        }
    }
}