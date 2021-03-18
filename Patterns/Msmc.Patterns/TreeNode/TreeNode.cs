using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Linq;

namespace Msmc.Patterns.Collections
{
    public class TreeNode<TBranch, TLeaf> : ITreeNode<TBranch, TLeaf>
    {
        private IDictionary<TBranch, ITreeNode<TBranch, TLeaf>> storage;

        private bool isTerminal;

        private TLeaf leaf;

        private TreeNode()
        {
        }

        public TreeNode(TBranch root) : this(new TBranch[] { root })
        {
        }

        public TreeNode(IEnumerable<TBranch> path)
        {
            if(path == null || path.Count() == 0)
            {
                throw new ArgumentException();
            }
            Path = path;
            storage = new Dictionary<TBranch, ITreeNode<TBranch, TLeaf>>();
        }

        public TreeNode(IEnumerable<TBranch> path, TLeaf leaf) : this(path)
        {
            storage = new ReadOnlyDictionary<TBranch, ITreeNode<TBranch, TLeaf>>(storage);
            isTerminal = true;
            this.leaf = leaf;
        }

        public IEnumerable<TBranch> Path
        {
            get;
            private set; 
        }

        public TLeaf this[TBranch[] hierachy]
        { 
            get
            {
                if(hierachy == null)
                {
                    throw new ArgumentNullException();
                }
                TLeaf leaf;
                hierachy.Aggregate((ITreeNode<TBranch, TLeaf>)this, (node, branch) => {
                    if(node.ContainsKey(branch) == false){
                        throw new KeyNotFoundException();                        
                    }
                    return node[branch];
                }).TryGetLeafValue(out leaf);
                return leaf;
            }
            set
            {
                if(hierachy == null)
                {
                    throw new ArgumentNullException();
                }

                if(hierachy.Length == 0)
                {
                    throw new ArgumentException();
                }
                
                var node = hierachy.Take(hierachy.Length - 1).Aggregate((ITreeNode<TBranch, TLeaf>)this, (node, branch) => {
                    if(node.ContainsKey(branch) == false){
                        node.Add(branch);
                    }
                    return node[branch];
                });
                node.Add(hierachy.TakeLast(1).Single(), value);
            }
        }

        public ITreeNode<TBranch, TLeaf> this[TBranch key] => storage[key];

        public IEnumerable<TBranch> Keys => storage.Keys;

        public IEnumerable<ITreeNode<TBranch, TLeaf>> Values => storage.Values;

        public int Count => storage.Count;

        public void Add(TBranch branch)
        {
            var path = Path.Concat(new TBranch[] { branch });
            storage.Add(branch, new TreeNode<TBranch, TLeaf>(path));
        }

        public void Add(TBranch branch, TLeaf leaf)
        {
            var path = Path.Concat(new TBranch[] { branch });
            storage.Add(branch, new TreeNode<TBranch, TLeaf>(path, leaf));
        }

        public void Clear()
        {
            storage.Clear();
        }

        public bool ContainsKey(TBranch key)
        {
            return storage.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TBranch, ITreeNode<TBranch, TLeaf>>> GetEnumerator()
        {
            return storage.GetEnumerator();
        }

        public bool Remove(TBranch branch)
        {
            return storage.Remove(branch);
        }

        public bool TryGetValue(TBranch key, [MaybeNullWhen(false)] out ITreeNode<TBranch, TLeaf> value)
        {
            return storage.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return storage.GetEnumerator();
        }

        public bool TryGetLeafValue(out TLeaf value)
        {
            value = isTerminal ? leaf : default(TLeaf);
            return isTerminal;
        }
    }
}
