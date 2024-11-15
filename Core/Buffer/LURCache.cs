using System.Collections.Generic;

namespace UnityToolkit
{
    public class LURCache<TKey, TValue>
    {
        private class Node
        {
            public TKey key;
            public TValue value;
            public Node next;
            public Node previous;

            public Node(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
                this.next = null;
                this.previous = null;
            }
        }

        private readonly Dictionary<TKey, Node> _dictionary;
        public int Capacity { get; private set; }
        public int Count { get; private set; }

        private Node _head;
        private Node _tail;

        public LURCache(int capacity)
        {
            _dictionary = new Dictionary<TKey, Node>(capacity);
            this.Capacity = capacity;
            this.Count = 0;

            _head = new Node(default(TKey), default(TValue));
            _tail = new Node(default(TKey), default(TValue));
            _head.next = _tail;
            _tail.previous = _head;
        }


        private void RemoveNode(Node node)
        {
            node.next.previous = node.previous;
            node.previous.next = node.next;
        }

        private void MoveToHead(Node node)
        {
            RemoveNode(node);
            AddToHead(node);
        }

        private void AddToHead(Node node)
        {
            node.next = _head.next;
            node.previous = _head;

            _head.next.previous = node;
            _head.next = node;
        }

        private Node RemoveTail()
        {
            Node node = _tail.previous;
            RemoveNode(node);
            return node;
        }

        public void Put(TKey key, TValue value)
        {
            if(_dictionary.ContainsKey(key))
            {
                var node = _dictionary[key];
                MoveToHead(node);
                node.value = value;
            }
            else
            {
                var node = new Node(key, value);
                _dictionary.Add(key, node);
                AddToHead(node);
                Count++;

                if(Count > Capacity)
                {
                    var tail = RemoveTail();
                    _dictionary.Remove(tail.key);
                    Count--;
                }
            }
        }

        public bool Get(TKey key, out TValue value)
        {
            if (_dictionary.ContainsKey(key))
            {
                var node = _dictionary[key];
                MoveToHead(node);
                value = node.value;
                return true;
            }

            value = default;
            return false;
        }
    }
}