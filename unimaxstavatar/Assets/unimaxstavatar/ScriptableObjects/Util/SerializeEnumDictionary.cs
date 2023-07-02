using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SerializeEnumDictionary<K, V> : Dictionary<string, V> , ISerializationCallbackReceiver
{
    [SerializeField]
    [DisplayWithoutEdit()]
    private List<K> keys = new List<K>();
    [SerializeField] 
    private List<V> values = new List<V>();

    static SerializeEnumDictionary()
    {
        if (typeof(K).IsEnum == false)
        {
            throw new ArgumentException("SerializeEnumDictionary T must be of type enum");
        }
    }

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (K k in Enum.GetValues(typeof(K)))
        {
            keys.Add(k);
            if (this.TryGetValue(k.ToString(), out V v))
            {
                values.Add(v);
            }
            else
            {
                values.Add(default(V));
            }
        }
        //Debug.Log($"OnBeforeSerialize keys.Count : {keys.Count}");
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        int i = 0;
        foreach (K k in Enum.GetValues(typeof(K)))
        {
            var v = GenValue(i++);
            this.Add(k.ToString(), v);
        }
    }

    private V GenValue(int index)
    {
        try
        {
            return values[index];
        }
        catch (Exception)
        {
            return default(V);
        }
    }
}