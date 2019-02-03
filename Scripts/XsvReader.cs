using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monry.XsvUtility
{
    // Primary key
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class XsvKeyAttribute : Attribute
    {
    }
    [Serializable]
    public class XsvReader
    {
        /// <summary>
        /// Most Prioritable Is Text Asset
        /// </summary>
        [SerializeField]
        public TextAsset m_CsvAsset;
        [SerializeField]
        public string m_CsvPathInResources;
        [SerializeField]
        public bool m_HeaderEnable;
        public struct Data<T>
        {
            [XsvRow] public IEnumerable<T> Rows { get; set; }
        }
        public static FieldInfo GetKeyColumn(Type type) {
            var fields = type
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var keyList = fields
                    .Where(x => x.GetCustomAttribute<XsvKeyAttribute>() != null).ToList();
            if (keyList.Count == 0)
            {
                keyList = fields
                    .Where(x => x.GetCustomAttribute<XsvColumnAttribute>() != null).ToList();
            }
            return ((keyList.Count > 0) ? keyList[0] : null);
        }
        static public IEnumerable<T> ReadCsv<T>(TextAsset csvAsset = null, string csvPathInResources = "", bool headerEnable = false)
        {
            if (csvAsset == null)
            {
                csvAsset = Resources.Load(csvPathInResources) as TextAsset;
            }
            if (csvAsset == null) return null;
            var deserialize = CsvSerializer.Deserialize<Data<T>>(csvAsset.text).Rows;
            if (headerEnable) deserialize = deserialize.Skip(1);
            return deserialize;
        }
        /// <summary>
        /// Get Directry Of Select KeyType Mode
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        /// <typeparam name="K">Key Type</typeparam>
        static public Dictionary<K, T> GetDictionary<K, T>(TextAsset csvAsset = null, string csvPathInResources = "", bool headerEnable = false)
        {
            var read = ReadCsv<T>(csvAsset, csvPathInResources, headerEnable);
            if (read == null) return null;
            return read
                .GroupBy(val => {
                    var value = GetKeyColumn(typeof(T)).GetValue(val);
                    if (typeof(K) == typeof(string))
                    {
                        return (K)(object)value.ToString();
                    } else
                    {
                        return (K)value;
                    }
                }).ToDictionary(x => x.Key, x => x.Last());
        }
        /// <summary>
        /// Get Directry Of String KeyType Mode
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        static public Dictionary<string, T> GetDictionary<T>(TextAsset csvAsset = null, string csvPathInResources = "", bool headerEnable = false)
        {
            return GetDictionary<string, T>(csvAsset, csvPathInResources, headerEnable);
        }
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        static public List<T> GetList<T>(TextAsset csvAsset = null, string csvPathInResources = "", bool headerEnable = false)
        {
            return ReadCsv<T>(csvAsset, csvPathInResources, headerEnable).ToList();
        }
        public Dictionary<string, T> ToDictionary<T>() {
            return GetDictionary<string, T>(m_CsvAsset, m_CsvPathInResources, m_HeaderEnable);
        }
        public Dictionary<K, T> ToDictionary<K, T>()
        {
            return GetDictionary<K, T>(m_CsvAsset, m_CsvPathInResources, m_HeaderEnable);
        }
        public List<T> ToList<T>()
        {
            return GetList<T>(m_CsvAsset, m_CsvPathInResources, m_HeaderEnable);
        }
    }
}