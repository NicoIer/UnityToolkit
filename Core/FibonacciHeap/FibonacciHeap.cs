using System;
using System.Collections.Generic;

namespace UnityToolkit
{
    /// <summary>
    /// Fibonacci Heap realization. Uses generic type T for data storage and TKey as a key type.
    /// </summary>
    /// <typeparam name="T">Type of the stored objects.</typeparam>
    /// <typeparam name="TKey">Type of the object key. Should implement IComparable.</typeparam>
    public class FibonacciHeap<T, TKey> where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Maximum nodes quantity in the heap.
        /// </summary>
        public static readonly double OneOverLogPhi = 1.0 / Math.Log((1.0 + Math.Sqrt(5.0)) / 2.0);


        private readonly TKey _minKeyValue;

        /// <summary>
        /// Minimum (statring) node of the heap.
        /// </summary>
        private FibonacciHeapNode<T, TKey> _minNode;

        /// <summary>
        /// The nodes quantity.
        /// </summary>
        private int _nNodes;

        /// <summary>
        /// Initializes the new instance of the Heap.
        /// </summary>
        /// <param name="minKeyValue">Minimum value of the key - to be used for comparing.</param>
        public FibonacciHeap(TKey minKeyValue)
        {
            _minKeyValue = minKeyValue;
        }

        /// <summary>
        /// Identifies whatever heap is empty.
        /// </summary>
        /// <returns>true if heap is empty - contains no elements.</returns>
        public bool IsEmpty()
        {
            return _minNode == null;
        }

        /// <summary>
        /// Removes all the elements from the heap.
        /// </summary>
        public void Clear()
        {
            _minNode = null;
            _nNodes = 0;
        }

        /// <summary>
        /// Decreses the key of a node.
        /// O(1) amortized.
        /// </summary>
        public void DecreaseKey(FibonacciHeapNode<T, TKey> x, TKey k)
        {
            if (k.CompareTo(x.Key) > 0)
            {
                throw new ArgumentException("decreaseKey() got larger key value");
            }

            x.Key = k;

            FibonacciHeapNode<T, TKey> y = x.Parent;

            if (y != null && x.Key.CompareTo(y.Key) < 0)
            {
                Cut(x, y);
                CascadingCut(y);
            }

            if (x.Key.CompareTo(_minNode.Key) < 0)
            {
                _minNode = x;
            }
        }

        /// <summary>
        /// Deletes a node from the heap.
        /// O(log n)
        /// </summary>
        public void Delete(FibonacciHeapNode<T, TKey> x)
        {
            // make newParent as small as possible
            DecreaseKey(x, _minKeyValue);

            // remove the smallest, which decreases n also
            RemoveMin();
        }

        /// <summary>
        /// Inserts a new node with its key.
        /// O(1)
        /// </summary>
        public void Insert(FibonacciHeapNode<T, TKey> node)
        {
            // concatenate node into min list
            if (_minNode != null)
            {
                node.Left = _minNode;
                node.Right = _minNode.Right;
                _minNode.Right = node;
                node.Right.Left = node;

                if (node.Key.CompareTo(_minNode.Key) < 0)
                {
                    _minNode = node;
                }
            }
            else
            {
                _minNode = node;
            }

            _nNodes++;
        }

        /// <summary>
        /// Returns the smalles node of the heap.
        /// O(1)
        /// </summary>
        /// <returns></returns>
        public FibonacciHeapNode<T, TKey> Min()
        {
            return _minNode;
        }

        /// <summary>
        /// Removes the smalles node of the heap.
        /// O(log n) amortized
        /// </summary>
        /// <returns></returns>
        public FibonacciHeapNode<T, TKey> RemoveMin()
        {
            FibonacciHeapNode<T, TKey> minNode = _minNode;

            if (minNode != null)
            {
                int numKids = minNode.Degree;
                FibonacciHeapNode<T, TKey> oldMinChild = minNode.Child;

                // for each child of minNode do...
                while (numKids > 0)
                {
                    FibonacciHeapNode<T, TKey> tempRight = oldMinChild.Right;

                    // remove oldMinChild from child list
                    oldMinChild.Left.Right = oldMinChild.Right;
                    oldMinChild.Right.Left = oldMinChild.Left;

                    // add oldMinChild to root list of heap
                    oldMinChild.Left = _minNode;
                    oldMinChild.Right = _minNode.Right;
                    _minNode.Right = oldMinChild;
                    oldMinChild.Right.Left = oldMinChild;

                    // set parent[oldMinChild] to null
                    oldMinChild.Parent = null;
                    oldMinChild = tempRight;
                    numKids--;
                }

                // remove minNode from root list of heap
                minNode.Left.Right = minNode.Right;
                minNode.Right.Left = minNode.Left;

                if (minNode == minNode.Right)
                {
                    _minNode = null;
                }
                else
                {
                    _minNode = minNode.Right;
                    Consolidate();
                }

                // decrement size of heap
                _nNodes--;
            }

            return minNode;
        }

        /// <summary>
        /// The number of nodes. O(1)
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return _nNodes;
        }

        /// <summary>
        /// Joins two heaps. O(1)
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <returns></returns>
        public static FibonacciHeap<T, TKey> Union(FibonacciHeap<T, TKey> h1, FibonacciHeap<T, TKey> h2)
        {
            var h = new FibonacciHeap<T, TKey>(h1._minKeyValue.CompareTo(h2._minKeyValue) < 0
                ? h1._minKeyValue
                : h2._minKeyValue)
            {
                _minNode = h1._minNode
            };

            if (h._minNode != null)
            {
                if (h2._minNode != null)
                {
                    h._minNode.Right.Left = h2._minNode.Left;
                    h2._minNode.Left.Right = h._minNode.Right;
                    h._minNode.Right = h2._minNode;
                    h2._minNode.Left = h._minNode;

                    if (h2._minNode.Key.CompareTo(h1._minNode.Key) < 0)
                    {
                        h._minNode = h2._minNode;
                    }
                }
            }
            else
            {
                h._minNode = h2._minNode;
            }

            h._nNodes = h1._nNodes + h2._nNodes;

            return h;
        }

        /// <summary>
        /// Performs a cascading cut operation. This cuts newChild from its parent and then
        /// does the same for its parent, and so on up the tree.
        /// </summary>
        private void CascadingCut(FibonacciHeapNode<T, TKey> y)
        {
            FibonacciHeapNode<T, TKey> z = y.Parent;

            // if there's a parent...
            while (z != null)
            {
                // if newChild is unmarked, set it marked
                if (!y.Mark)
                {
                    y.Mark = true;
                    break;
                }
                // it's marked, cut it from parent

                Cut(y, z);
                // cut its parent as well
                y = z;
                z = y.Parent;
            }
        }


        private void Consolidate()
        {
            int arraySize = (int)Math.Floor(Math.Log(_nNodes) * OneOverLogPhi) + 1;

            var array = new List<FibonacciHeapNode<T, TKey>>(arraySize);

            // Initialize degree array
            for (var i = 0; i < arraySize; i++)
            {
                array.Add(null);
            }

            // Find the number of root nodes.
            var numRoots = 0;
            FibonacciHeapNode<T, TKey> x = _minNode;

            if (x != null)
            {
                numRoots++;
                x = x.Right;

                while (x != _minNode)
                {
                    numRoots++;
                    x = x.Right;
                }
            }

            // For each node in root list do...
            while (numRoots > 0)
            {
                // Access this node's degree..
                int d = x.Degree;
                FibonacciHeapNode<T, TKey> next = x.Right;

                // ..and see if there's another of the same degree.
                for (;;)
                {
                    FibonacciHeapNode<T, TKey> y = array[d];
                    if (y == null)
                    {
                        // Nope.
                        break;
                    }

                    // There is, make one of the nodes a child of the other.
                    // Do this based on the key value.
                    if (x.Key.CompareTo(y.Key) > 0)
                    {
                        (y, x) = (x, y);
                    }

                    // FibonacciHeapNode<T> newChild disappears from root list.
                    Link(y, x);

                    // We've handled this degree, go to next one.
                    array[d] = null;
                    d++;
                }

                // Save this node for later when we might encounter another
                // of the same degree.
                array[d] = x;

                // Move forward through list.
                x = next;
                numRoots--;
            }

            // Set min to null (effectively losing the root list) and
            // reconstruct the root list from the array entries in array[].
            _minNode = null;

            for (var i = 0; i < arraySize; i++)
            {
                FibonacciHeapNode<T, TKey> y = array[i];
                if (y == null)
                {
                    continue;
                }

                // We've got a live one, add it to root list.
                if (_minNode != null)
                {
                    // First remove node from root list.
                    y.Left.Right = y.Right;
                    y.Right.Left = y.Left;

                    // Now add to root list, again.
                    y.Left = _minNode;
                    y.Right = _minNode.Right;
                    _minNode.Right = y;
                    y.Right.Left = y;

                    // Check if this is a new min.
                    if (y.Key.CompareTo(_minNode.Key) < 0)
                    {
                        _minNode = y;
                    }
                }
                else
                {
                    _minNode = y;
                }
            }
        }

        /// <summary>
        /// The reverse of the link operation: removes newParent from the child list of newChild.
        /// This method assumes that min is non-null.
        /// Running time: O(1)
        /// </summary>
        private void Cut(FibonacciHeapNode<T, TKey> x, FibonacciHeapNode<T, TKey> y)
        {
            // remove newParent from childlist of newChild and decrement degree[newChild]
            x.Left.Right = x.Right;
            x.Right.Left = x.Left;
            y.Degree--;

            // reset newChild.child if necessary
            if (y.Child == x)
            {
                y.Child = x.Right;
            }

            if (y.Degree == 0)
            {
                y.Child = null;
            }

            // add newParent to root list of heap
            x.Left = _minNode;
            x.Right = _minNode.Right;
            _minNode.Right = x;
            x.Right.Left = x;

            // set parent[newParent] to nil
            x.Parent = null;

            // set mark[newParent] to false
            x.Mark = false;
        }

        /// <summary>
        /// Makes newChild a child of Node newParent.
        /// O(1)
        /// </summary>
        private static void Link(FibonacciHeapNode<T, TKey> newChild, FibonacciHeapNode<T, TKey> newParent)
        {
            // remove newChild from root list of heap
            newChild.Left.Right = newChild.Right;
            newChild.Right.Left = newChild.Left;

            // make newChild a child of newParent
            newChild.Parent = newParent;

            if (newParent.Child == null)
            {
                newParent.Child = newChild;
                newChild.Right = newChild;
                newChild.Left = newChild;
            }
            else
            {
                newChild.Left = newParent.Child;
                newChild.Right = newParent.Child.Right;
                newParent.Child.Right = newChild;
                newChild.Right.Left = newChild;
            }

            // increase degree[newParent]
            newParent.Degree++;

            // set mark[newChild] false
            newChild.Mark = false;
        }
    }
}