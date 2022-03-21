using UnityEngine;
using System;
using System.Collections.Generic;

namespace RemedyDebug
{
    /// <summary>
    /// Serializable dictionary.
    /// Allows for the serialization of a Dictionary<TKey, TVal> type.
    /// Create a private Dictionary inside your class as well as a
    /// SerializableDictionary and a custom editor for that class.  Every time an edit is done
    /// update the SerializableDictionary and the Dictionary.  This we when serialization occurs
    /// the SerializableDictionary will be stored and restored.  Use Awake() function inside your class
    /// in order to convert the SerializableDictionary into a regular Dictionary
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> where TKey : IEquatable<TKey>
    {
        [SerializeField]
        private List<TKey> m_keysList = new List<TKey>();

        [SerializeField]
        private List<TValue> m_valueList = new List<TValue>();

        public List<TKey> KeysList
        {
            get { return m_keysList; }
            set { m_keysList = value; }
        }

        public List<TValue> ValueList
        {
            get { return m_valueList; }
            set { m_valueList = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SerializableDictionary`2"/> with the specified i.
        /// </summary>
        /// <param name="i">The index.</param>
        public TValue this[TKey i]
        {
            get { return GetValue(i); }
            set { SetValue(i, value); }
        }

        /// <summary>
        /// Gets the number of keyvalue pairs inside the dictionary.
        /// </summary>
        /// <value>The number of pairs in the dictionary.</value>
        public int Count
        {
            get { return m_keysList.Count; }
        }

        /// <summary>
        /// Sets the value at the specified key.  If the key doesn't exist, then it will be created.
        /// </summary>
        /// <param name='key'>
        /// Key.
        /// </param>
        /// <param name='valueData'>
        /// Value data.
        /// </param>
        public void SetValue(TKey key, TValue valueData)
        {
            // Get the index of the key and if found, set the data
            int index = m_keysList.IndexOf(key);
            if (index != -1)
            {
                m_valueList[index] = valueData;
            }
            else
            {
                m_keysList.Add(key);
                m_valueList.Add(valueData);
            }
        }

        /// <summary>
        /// Remove the specified key.
        /// </summary>
        /// <param name='key'>
        /// Key.
        /// </param>
        public void Remove(TKey key)
        {
            // Get the index of the key and if found, remove it from both lists
            int index = m_keysList.IndexOf(key);
            if (index != -1)
            {
                m_keysList.RemoveAt(index);
                m_valueList.RemoveAt(index);
            }
        }

        /// <summary>
        /// Gets the value from the specified key.
        /// </summary>
        /// <returns>
        /// The value.
        /// </returns>
        /// <param name='key'>
        /// Key.
        /// </param>
        /// <exception cref='System.ArgumentOutOfRangeException'>
        /// Is thrown when the key cannot be found.
        /// </exception>
        public TValue GetValue(TKey key)
        {
            // Get the index of the key and if found, return the data
            int index = m_keysList.IndexOf(key);
            if (index != -1)
            {
                return m_valueList[index];
            }

            // Throw an exception
            throw new System.ArgumentOutOfRangeException("key", key, "Isn't inside this list");
        }

        IEnumerable<TKey> GetKeyEnumerable()
        {
            return m_keysList;
        }

        public Dictionary<TKey, TValue> GetDictionary()
        {
            Dictionary<TKey, TValue> dictionaryData = new Dictionary<TKey, TValue>(m_keysList.Count);

            for (int i = 0; i < m_keysList.Count; i++)
            {
                dictionaryData.Add(m_keysList[i], m_valueList[i]);
            }

            return dictionaryData;
        }

        /// <summary>
        /// Determines if the specified key exists in the dictionary.
        /// </summary>
        /// <returns><c>true</c>, if key exists, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        public bool ContainsKey(TKey key)
        {
            int keyIndex = m_keysList.IndexOf(key);
            return keyIndex != -1;
        }

        /// <summary>
        /// Gets an array of the keys.
        /// </summary>
        /// <returns>Array of keys.</returns>
        public TKey[] GetKeys()
        {
            TKey[] arrKeys = new TKey[m_keysList.Count];
            m_keysList.CopyTo(arrKeys);
            return arrKeys;
        }

        /// <summary>
        /// Gets an array of the values.
        /// </summary>
        /// <returns>Array of values.</returns>
        public TValue[] GetValues()
        {
            TValue[] arrValues = new TValue[m_valueList.Count];
            m_valueList.CopyTo(arrValues);
            return arrValues;
        }

        /// <summary>
        /// Changes the key.
        /// </summary>
        /// <param name="oldKey">Old key.</param>
        /// <param name="newKey">New key.</param>
        public void ChangeKey(TKey oldKey, TKey newKey)
        {
            int keyIndex = m_keysList.FindIndex(keyValue => keyValue.Equals(oldKey));
            m_keysList[keyIndex] = newKey;
        }

        /// <summary>
        /// Attempts to get the value of the specified key.
        /// </summary>
        /// <returns><c>true</c>, if key existed, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = m_keysList.IndexOf(key);
            if (index == -1)
            {
                value = default(TValue);
                return false;
            }

            value = m_valueList[index];

            return true;
        }
    }
}
