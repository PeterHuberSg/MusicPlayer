using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


namespace MusicPlayer {

  /// <summary>
  /// A Dictionary that can store several TValues in a HashSet for each TKey.
  /// </summary>
  public class DictionaryHashSets<TKey, TValue>: IDictionary<TKey, HashSet<TValue>> where TKey: notnull{


    readonly Dictionary<TKey, HashSet<TValue>> dictionary = new Dictionary<TKey, HashSet<TValue>>();


    public HashSet<TValue> this[TKey key] { 
      get => dictionary[key]; 
      set => dictionary[key] = value; }


    public bool this[TKey key, TValue valueKey] {
      get => dictionary[key].Contains(valueKey);
      set {
        if (!dictionary.TryGetValue(key, out var hashSet)) {
          hashSet = new HashSet<TValue>();
        }
        if (value) {
          hashSet.Add(valueKey);
        } else {
          hashSet.Remove(valueKey);
        }
      }
    }


    public ICollection<TKey> Keys => dictionary.Keys;


    public ICollection<HashSet<TValue>> Values => dictionary.Values;


    public int Count => dictionary.Count;


    public bool IsReadOnly => false;


    public void Add(TKey key) {
      dictionary.Add(key, new HashSet<TValue>());
    }


    public void Add(TKey key, HashSet<TValue> hashSet) {
      dictionary.Add(key, hashSet);
    }


    public void Add(TKey key, TValue value) {
      if (!dictionary.TryGetValue(key, out var hashSet)) {
        hashSet = new();
      }
      hashSet.Add(value);
    }


    public void Add(KeyValuePair<TKey, HashSet<TValue>> item) {
      ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>)dictionary).Add(item);
    }


    public void Clear() {
      dictionary.Clear();
    }


    public bool ContainsKey(TKey key) {
      return dictionary.ContainsKey(key);
    }


    public bool Contains(TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out var hashSet)) {
        return hashSet.Contains(value);
      }
      return false;
    }


    public bool Contains(KeyValuePair<TKey, HashSet<TValue>> item) {
      return ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>)dictionary).Contains(item);
    }


    public bool Remove(TKey key) {
      return dictionary.Remove(key);
    }


    public bool Remove(TKey key, TValue value) {
      return dictionary.TryGetValue(key, out var hashSet) && hashSet.Remove(value);
    }


    public bool Remove(KeyValuePair<TKey, HashSet<TValue>> item) {
      return ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>)dictionary).Remove(item);
    }


    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out HashSet<TValue> value) {
      return dictionary.TryGetValue(key, out value);
    }


    public void CopyTo(KeyValuePair<TKey, HashSet<TValue>>[] array, int arrayIndex) {
      ((ICollection<KeyValuePair<TKey, HashSet<TValue>>>)dictionary).CopyTo(array, arrayIndex);
    }


    public IEnumerator<KeyValuePair<TKey, HashSet<TValue>>> GetEnumerator() {
      return ((IEnumerable<KeyValuePair<TKey, HashSet<TValue>>>)dictionary).GetEnumerator();
    }


    IEnumerator IEnumerable.GetEnumerator() {
      return ((IEnumerable)dictionary).GetEnumerator();
    }
  }
}
