using System;
using System.Collections.Generic;

namespace Msmc.Patterns.Tree
{
    public interface ITreeNode<TBranch, TLeaf> : 
    IReadOnlyDictionary<TBranch, ITreeNode<TBranch, TLeaf>>
    {          
        /// <summary>
        /// Adds a branch to the node.
        /// </summary>
        /// <param name="branch">The branch of the node to add.</param>
        /// <exception cref="ArgumentNullException">`branch` is null.</exception>
        /// <exception cref="ArgumentException">The branch already exists in the node.</exception>
        public void Add(TBranch branch);
      
        /// <summary>
        /// Adds a leaf to the node.
        /// </summary>
        /// <param name="branch">The branch of the node to add.</param>
        /// <param name="leaf">The leaf of the branch to add.</param>
        /// <exception cref="ArgumentNullException">`branch` is null.</exception>
        /// <exception cref="ArgumentException">The branch already exists in the node.</exception>
        public void Add(TBranch branch , TLeaf leaf);
          
        /// <summary>
        /// Removes the specified branch.
        /// </summary>
        /// <param name="branch">The branch of the node to remove.</param>
        /// <returns>
        /// `true` if the branch is sucessfully removed; otherwise, `false`.
        /// This method also returns `false` if branch was not found in the original ITreeNode<TBranch,TLeaf>.
        /// </returns>
        /// <exception cref="ArgumentNullException">`branch` is null.</exception>
        bool Remove(TBranch branch);     

        /// <summary>
        /// Removes all branches from the ITreeNode<TBranch, TLeaf>. 
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets or sets the leaf with the specified branch hierarchy.
        /// </summary>
        /// <value>The leaf with the specified branch hierarchy.</value>
        /// <returns>
        /// The leaf with the specified branch hierarchy.
        /// </returns>
        /// <exception cref="ArgumentNullException">`branches` is null.</exception>
        /// <exception cref="KeyNotFoundException">The leaf is not found with the specified branch hierarchy.</exception> 
        TLeaf this[TBranch[] hierachy]  { get; set; }

        /// <summary>
        /// Try to get leaf value.
        /// </summary>
        /// <param name="value">The leaf value.</param>
        /// <returns>true when the node is leaf. false when the node is branch.</returns>
        bool TryGetLeafValue(out TLeaf value);
    }
}
