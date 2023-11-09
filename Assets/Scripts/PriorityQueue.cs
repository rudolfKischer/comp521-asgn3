using System;
using System.Collections.Generic;

public class PriorityQueue<T, TPriority> where TPriority : IComparable<TPriority>
{
    private List<(T item, TPriority priority)> heap = new List<(T, TPriority)>();
    private Dictionary<T, int> itemToIndex = new Dictionary<T, int>();

    public int Count => heap.Count;

    public void Enqueue(T item, TPriority priority)
    {
        heap.Add((item, priority));
        itemToIndex[item] = heap.Count - 1;
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException("The priority queue is empty");

        var (item, _) = heap[0];
        RemoveAt(0);
        return item;
    }

    public T Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("The priority queue is empty");
        
        return heap[0].item;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (heap[index].priority.CompareTo(heap[parentIndex].priority) >= 0)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            int smallest = index;
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;

            if (leftChildIndex < heap.Count && heap[leftChildIndex].priority.CompareTo(heap[smallest].priority) < 0)
            {
                smallest = leftChildIndex;
            }

            if (rightChildIndex < heap.Count && heap[rightChildIndex].priority.CompareTo(heap[smallest].priority) < 0)
            {
                smallest = rightChildIndex;
            }

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void RemoveAt(int index)
    {
        // Save the item to be removed for updating the dictionary.
        var itemToRemove = heap[index].item;

        // If not removing the last item, swap before removing.
        if (index < heap.Count - 1)
        {
            Swap(index, heap.Count - 1);
        }

        // Remove the item from both the heap and the dictionary.
        itemToIndex.Remove(itemToRemove); // Remove using the original item.
        heap.RemoveAt(heap.Count - 1); // Remove the last item which is now the item to remove.

        // If we removed an item that was not the last item, we need to heapify.
        if (index < heap.Count)
        {
            HeapifyDown(index);
            HeapifyUp(index);
        }
    }


    private void Swap(int indexA, int indexB)
    {
        (heap[indexA], heap[indexB]) = (heap[indexB], heap[indexA]);
        itemToIndex[heap[indexA].item] = indexA;
        itemToIndex[heap[indexB].item] = indexB;
    }

    public bool Contains(T item)
    {
        return itemToIndex.ContainsKey(item);
    }

    public void UpdatePriority(T item, TPriority newPriority)
    {
        if (!itemToIndex.TryGetValue(item, out var index))
            throw new InvalidOperationException("The item does not exist in the priority queue");

        heap[index] = (item, newPriority);
        HeapifyUp(index);
        HeapifyDown(index);
    }

    public void Clear()
    {
        heap.Clear();
        itemToIndex.Clear();
    }
}
