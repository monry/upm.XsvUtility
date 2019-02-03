using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monry.XsvUtility
{
    [Serializable]
    public class XsvReader
    {
        /// <summary>
        /// Most Prioritable Is Text Asset
        /// </summary>
        [SerializeField, Tooltip("CSV or TSV Asset, Most Prioritable")]
        public TextAsset m_XsvAsset;
        [SerializeField, Tooltip("CSV or TSV File Path, then XsvAsset is null")]
        public string m_XsvPathInResources;
        [SerializeField, Tooltip("Header Row Skip Flag")]
        public bool m_HeaderEnable;
        [SerializeField, Tooltip("Choose delimiter, CSV->Comma, TSV->Tab")]
        public XsvParser.Delimiter m_Delimiter;
        public struct Data<T>
        {
            [XsvRow] public IEnumerable<T> Rows { get; set; }
        }
        /// <summary>
        /// カラムを解析して主キーを特定する、XsvKeyプロパティ指定で主キーとなる
        /// Setting of Primary key from XsvKey Attribute Property
        /// </summary>
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
        /// <summary>
        /// XsvDeserializeを使ってパース開始
        /// Run XsvDeserialize with options
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        static public IEnumerable<T> ReadXsv<T>(TextAsset xsvAsset, bool headerEnable = false,
            XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma)
        {
            if (xsvAsset == null) return null;  // nullチェックは後々の処理を飛ばすために入れました
            IEnumerable<T> deserialize;
            switch (delimiter)
            {
                case XsvParser.Delimiter.Comma:
                    deserialize = CsvSerializer.Deserialize<Data<T>>(xsvAsset.text).Rows;
                    break;
                case XsvParser.Delimiter.Tab:
                    deserialize = TsvSerializer.Deserialize<Data<T>>(xsvAsset.text).Rows;
                    break;
                default:    // 例外処理はCSVとして処理します
                    deserialize = CsvSerializer.Deserialize<Data<T>>(xsvAsset.text).Rows;
                    break;
            }
            if (headerEnable) deserialize = deserialize.Skip(1);
            return deserialize;
        }
        /// <summary>
        /// 辞書型で取り出し
        /// Get Directry of Select KeyType Mode from TextAsset
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        static public Dictionary<K, T> GetDictionary<K, T>(TextAsset xsvAsset, bool headerEnable = false,
            XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma)
        {
            var read = ReadXsv<T>(xsvAsset, headerEnable, delimiter);
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
        /// ファイルパスから辞書型で取り出し
        /// Get Directry of Select KeyType Mode from ResourcesPath
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        /// <param name="xsvPathInResources">Resources file path</param>
        /// <returns></returns>
        static public Dictionary<K, T> GetDictionary<K, T>(string xsvPathInResources, bool headerEnable = false,
            XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma)
        {
            return GetDictionary<K, T>(Resources.Load(xsvPathInResources) as TextAsset, headerEnable, delimiter);
        }
        /// <summary>
        /// 文字列をキーとして辞書型で取り出し
        /// Get Directry of String KeyType Mode
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        static public Dictionary<string, T> GetDictionary<T>(TextAsset xsvAsset,
            bool headerEnable = false, XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma)
        {
            return GetDictionary<string, T>(xsvAsset, headerEnable, delimiter);
        }
        /// <summary>
        /// 文字列をキーとしてファイルパスから辞書型で取り出し
        /// Get Directry of String KeyType Mode from ResourcesPath
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        static public Dictionary<string, T> GetDictionary<T>(string xsvPathInResources,
            bool headerEnable = false, XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma)
        {
            return GetDictionary<string, T>(Resources.Load(xsvPathInResources) as TextAsset, headerEnable, delimiter);
        }
        /// <summary>
        /// リスト型で取り出し
        /// Get List from TextAsset
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        static public List<T> GetList<T>(TextAsset xsvAsset,
            bool headerEnable = false, XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma)
        {
            return ReadXsv<T>(xsvAsset, headerEnable, delimiter).ToList();
        }
        /// <summary>
        /// ファイルパスからリスト型で取り出し
        /// Get List from ResourcesPath
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        static public List<T> GetList<T>(string xsvPathInResources,
            bool headerEnable = false, XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma)
        {
            return ReadXsv<T>(Resources.Load(xsvPathInResources) as TextAsset, headerEnable).ToList();
        }
        /// <summary>
        /// メンバ変数から辞書型で取り出し
        /// Get Directry of Select KeyType Mode from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        public Dictionary<string, T> ToDictionary<T>() {
            if (m_XsvAsset == null) m_XsvAsset = Resources.Load<TextAsset>(m_XsvPathInResources);
            return GetDictionary<string, T>(m_XsvAsset, m_HeaderEnable, m_Delimiter);
        }
        /// <summary>
        /// 文字列をキーとしてメンバ変数から辞書型で取り出し
        /// Get Directry of String KeyType Mode from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="K">Key Type</typeparam>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        public Dictionary<K, T> ToDictionary<K, T>()
        {
            if (m_XsvAsset == null) m_XsvAsset = Resources.Load<TextAsset>(m_XsvPathInResources);
            return GetDictionary<K, T>(m_XsvAsset, m_HeaderEnable, m_Delimiter);
        }
        /// <summary>
        /// メンバ変数からリスト型で取り出し
        /// Get List from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="T">Columns(Struct or Class) Type</typeparam>
        public List<T> ToList<T>()
        {
            if (m_XsvAsset == null) m_XsvAsset = Resources.Load<TextAsset>(m_XsvPathInResources);
            return GetList<T>(m_XsvAsset, m_HeaderEnable, m_Delimiter);
        }
    }
}