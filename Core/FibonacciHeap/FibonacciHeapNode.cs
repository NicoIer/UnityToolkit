using System;

namespace UnityToolkit
{
    /// <summary>
    /// Represents the one node in the Fibonacci Heap.
    /// </summary>
    /// <typeparam name="T">Type of the object to be stored.</typeparam>
    /// <typeparam name="TKey">Type of the key to be used for the stored object. 
    /// Has to implement the <see cref="IComparable"/> interface.</typeparam>
    public class FibonacciHeapNode<T, TKey> where TKey : IComparable<TKey>
    {
        public FibonacciHeapNode(T data, TKey key)
        {
            Right = this;
            Left = this;
            Data = data;
            Key = key;
        }

        /// <summary>
        /// Gets or sets the node data object.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Gets or sets the reference to the first child node.
        /// </summary>
        internal FibonacciHeapNode<T, TKey> Child { get; set; }

        /// <summary>
        /// Gets or sets the reference to the left node neighbour.
        /// </summary>
        internal FibonacciHeapNode<T, TKey> Left { get; set; }

        /// <summary>
        /// Gets or sets the reference to the node parent.
        /// </summary>
        internal FibonacciHeapNode<T, TKey> Parent { get; set; }

        /// <summary>
        /// Gets or sets the reference to the right node neighbour.
        /// </summary>
        internal FibonacciHeapNode<T, TKey> Right { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whatever node is marked (visited).
        /// </summary>
        internal bool Mark { get; set; }

        /// <summary>
        /// Gets or sets the value of the node key.
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Gets or sets the value of the node degree.
        /// </summary>
        internal int Degree { get; set; }
    }
}
