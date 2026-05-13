using System;
using System.Collections.Generic;
using UnityEngine;

public class ContentManager : MonoBehaviour
{
    [SerializeField] private Entry entryPrefab;
    [SerializeField] private Transform container;

    [Serializable]
    public class EntryInfos
    {
        [TextArea(3, 8)] public string Text;
    }

    [SerializeField] private List<EntryInfos> entries = new List<EntryInfos>();

    // Dictionary to track dynamically-added entries by a string key,
    // so they can be removed or updated later.
    private readonly Dictionary<string, Entry> dynamicEntries = new Dictionary<string, Entry>();

    private void Start()
    {
        CreateEntries();
    }

    private void CreateEntries()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        dynamicEntries.Clear();

        foreach (var entry in entries)
        {
            var newEntry = Instantiate(entryPrefab, container);
            newEntry.Setup(entry.Text);
        }
    }

    /// <summary>
    /// Adds a dynamic entry at the end of the list, identified by a key so it
    /// can be updated or removed later. If an entry with the same key already
    /// exists, its text is updated instead of creating a new one.
    /// </summary>
    public void AddOrUpdateEntry(string key, string text)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (dynamicEntries.TryGetValue(key, out Entry existing) && existing != null)
        {
            existing.Setup(text);
            return;
        }

        var newEntry = Instantiate(entryPrefab, container);
        newEntry.Setup(text);
        dynamicEntries[key] = newEntry;
    }

    /// <summary>
    /// Removes a dynamic entry previously added with AddOrUpdateEntry.
    /// </summary>
    public void RemoveEntry(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (dynamicEntries.TryGetValue(key, out Entry entry))
        {
            if (entry != null)
                Destroy(entry.gameObject);
            dynamicEntries.Remove(key);
        }
    }

    public bool HasEntry(string key)
    {
        return dynamicEntries.ContainsKey(key);
    }
}