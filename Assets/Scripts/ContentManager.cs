using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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

    private void Start()
    {
        CreateEntries();
    }

    private void CreateEntries()
    {
        foreach(Transform child in container)
        {
            Destroy(child.gameObject);
        }
        foreach(var entry in entries)
        {
            var newEntry = Instantiate(entryPrefab, container);
            newEntry.Setup(entry.Text);
        }
    }
}
