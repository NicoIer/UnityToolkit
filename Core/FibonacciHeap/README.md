# Fibonacci Heap

A [Fibonacci Heap](https://en.wikipedia.org/wiki/Fibonacci_heap) is a data structure modelling a priority queue. The implemenation is generic like System.Collection.Generic types.

## Example

```csharp
// a heap with objects for data and int as sorting key.
var heap = new FibonacciHeap<object, int>(0);

var nodeToInsert = new FibonacciHeapNoe<object, int>(new object(), -42);

heap.Insert(nodeToInsert);
```