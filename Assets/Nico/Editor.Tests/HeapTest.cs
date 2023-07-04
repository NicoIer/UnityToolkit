using NUnit.Framework;

namespace Nico.Tests
{
    [TestFixture]
    public class HeapTest
    {
        [Test]
        public void MinHeapTest()
        {
            var minHeap = new MinHeap<int>((a, b) => a - b);
            minHeap.Insert(100);
            minHeap.Insert(10);
            minHeap.Insert(20);
            minHeap.Insert(30);
            Assert.IsTrue(minHeap.Count == 4);
            Assert.IsTrue(minHeap.Peek() == 10);
            Assert.IsTrue(minHeap.Pop() == 10);
            Assert.IsTrue(minHeap.Pop() == 20);
            Assert.IsTrue(minHeap.Pop() == 30);
            Assert.IsTrue(minHeap.Pop() == 100);
        }

        [Test]
        public void MaxHeapTest()
        {
            var maxHeap = new MaxHeap<int>((a, b) => a - b);
            maxHeap.Insert(100);
            maxHeap.Insert(10);
            maxHeap.Insert(20);
            maxHeap.Insert(30);
            Assert.IsTrue(maxHeap.Count == 4);
            Assert.IsTrue(maxHeap.Peek() == 100);
            Assert.IsTrue(maxHeap.Pop() == 100);
            Assert.IsTrue(maxHeap.Pop() == 30);
            Assert.IsTrue(maxHeap.Pop() == 20);
            Assert.IsTrue(maxHeap.Pop() == 10);
            
        }
    }
}