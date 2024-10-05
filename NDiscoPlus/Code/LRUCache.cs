using MaterialColorUtilities.Schemes;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoPlus.Code;

// Reference: https://www.baeldung.com/java-lru-cache
public class LRUCache<TKey, TValue> where TKey : IEquatable<TKey>
{
    private readonly record struct CacheEntry(TKey Key, TValue Value);

    // LinkedList.Count is an O(1) operation
    public int Count => linkedList.Count;
    public int Capacity { get; }

    private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> nodeMap;
    private readonly LinkedList<CacheEntry> linkedList;

    public LRUCache(int capacity)
    {
        Capacity = capacity;
        nodeMap = new(capacity: capacity);
        linkedList = new();
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueBuilder)
    {
        if (nodeMap.TryGetValue(key, out LinkedListNode<CacheEntry>? node))
        {
            // Node exists

            MoveNodeToFirst(node);

            return node.Value.Value;
        }
        else
        {
            // Node doesn't exist

            TValue newValue = valueBuilder(key);
            CacheEntry newEntry = new(key, newValue);
            LinkedListNode<CacheEntry> newNode = new(newEntry);
            AddNewNode(newNode);

            return newValue;
        }
    }

    private void MoveNodeToFirst(LinkedListNode<CacheEntry> node)
    {
        linkedList.Remove(node);
        linkedList.AddFirst(node);
    }

    private void AddNewNode(LinkedListNode<CacheEntry> newNode)
    {
        // Remove oldest entries until one free slot is available for the new value
        while (Count >= Capacity)
            RemoveOldestEntry();

        // add to nodeMap first so that if an exception is thrown (key exists already)
        // nodeMap and linkedList don't go out-of-sync.
        nodeMap.Add(newNode.Value.Key, newNode);
        linkedList.AddFirst(newNode);
    }

    // Remove least recently used item
    private void RemoveOldestEntry()
    {
        LinkedListNode<CacheEntry>? node = linkedList.Last ?? throw new InvalidOperationException("Linked list is empty.");
        linkedList.Remove(node);
        nodeMap.Remove(node.Value.Key);
    }
}
