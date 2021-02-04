#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnoHKUtils.Collections
{
    /// <summary>
    /// Represents a tree of nodes, each containing a value of type <typeparamref name="T"/> and optionally holding references to parent and/or children <see cref="TreeNode{T}"/> objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeNode<T> : IEnumerable<TreeNode<T>>
    {
        private TreeNode<T>? _parent;

        public T Value { get; set; }

        public TreeNode<T>? Parent
        {
            get => _parent;
            set
            {
                if (_parent is not null && _parent != value)
                    _parent.RemoveChild(this);

                _parent = value;
            }
        }

        public IList<TreeNode<T>> Children { get; }

        public TreeNode(T value, TreeNode<T>? parent = null, IEnumerable<TreeNode<T>>? children = null)
        {
            Value = value;
            Parent = parent;
            Children = new List<TreeNode<T>>(children ?? Enumerable.Empty<TreeNode<T>>());
        }

        public TreeNode<T> AddChild(T value)
        {
            TreeNode<T> child = new(value, this);
            Children.Add(child);
            return child;
        }

        public IList<TreeNode<T>> AddChildren(params T[] values)
        {
            List<TreeNode<T>> toReturn = new(values.Length);
            foreach (var value in values)
                toReturn.Add(AddChild(value));

            return toReturn;
        }

        public bool RemoveChild(TreeNode<T> value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (value.Parent == this)
                value.Parent = null;

            return Children.Remove(value);
        }

        public void Traverse(Action<TreeNode<T>, int> action)
        {
            Traverse(action, 0);
        }

        public void Traverse(Action<TreeNode<T>, int> action, int depth)
        {
            action(this, depth);
            foreach (var child in Children)
                action(child, depth + 1);
        }

        public IEnumerable<T> Flatten()
        {
            return EnumerableUtil.Solo(Value).Concat(Children.SelectMany(c => c.Flatten()));
        }

        public IEnumerator<TreeNode<T>> GetEnumerator()
        {
            foreach (var node in Enumerate())
                yield return node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<TreeNode<T>> Enumerate()
        {
            return EnumerableUtil.Solo(this).Concat(Children.SelectMany(c => c.Enumerate()));
        }
    }
}