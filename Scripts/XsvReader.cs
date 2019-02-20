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
        [SerializeField, Tooltip("Choose delimiter, CSV->Comma, TSV->Tab")]
        protected XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma;
        public XsvParser.Delimiter Delimiter => delimiter;
        /// <summary>
        /// Most Prioritable Is Text Asset
        /// </summary>
        [SerializeField, Tooltip("CSV or TSV Asset, Most Prioritable")]
        protected TextAsset xsvAsset = default;
        public TextAsset XsvAsset => this.xsvAsset;
        [SerializeField, Tooltip("CSV or TSV File Path, then XsvAsset is null")]
        protected string xsvPathInResources = default;
        public string XsvPathInResources => this.xsvPathInResources;
        [SerializeField, Tooltip("Header Row Skip Flag")]
        protected bool headerEnable = true;
        public bool HeaderEnable => this.headerEnable;
        public struct Data<TValue>
        {
            [XsvRow] public IEnumerable<TValue> Rows { get; set; }
        }
        /// <summary>
        /// Create XsvReader with Member
        /// </summary>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvAsset">TextAsset Data</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        public XsvReader(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            this.delimiter = delimiter;
            this.xsvAsset = xsvAsset;
            this.headerEnable = headerEnable;
        }
        /// <summary>
        /// Create XsvReader with Member
        /// </summary>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvPathInResources">Resources file path</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        public XsvReader(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources,
            bool headerEnable = true)
        {
            this.delimiter = delimiter;
            this.xsvPathInResources = xsvPathInResources;
            this.headerEnable = headerEnable;
        }
        /// <summary>
        /// Setting is Primary key from XsvKey Attribute Property
        /// </summary>
        /// <param name="type">Type Name</param>
        /// <returns>Primary key Field</returns>
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
        /// Run XsvDeserialize with options
        /// </summary>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvAsset">TextAsset Data</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        /// <returns>IEnumerable of TValue</returns>
        public static IEnumerable<TValue> ReadXsv<TValue>(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            if (xsvAsset == null) return null;
            IEnumerable<TValue> deserialize;
            switch (delimiter)
            {
                case XsvParser.Delimiter.Comma:
                    deserialize = CsvSerializer.Deserialize<Data<TValue>>(xsvAsset.text).Rows;
                    break;
                case XsvParser.Delimiter.Tab:
                    deserialize = TsvSerializer.Deserialize<Data<TValue>>(xsvAsset.text).Rows;
                    break;
                default:    // 例外処理はCSVとして処理します
                    deserialize = CsvSerializer.Deserialize<Data<TValue>>(xsvAsset.text).Rows;
                    break;
            }
            if (headerEnable) deserialize = deserialize.Skip(1);
            return deserialize;
        }
        /// <summary>
        /// Get Rows is Dictionary(Key:TKey), Columns is TValue
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvAsset">TextAsset Data</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        /// <returns>Dictionary(Key:TKey) of TValue</returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            var read = ReadXsv<TValue>(delimiter, xsvAsset, headerEnable);
            if (read == null) return null;
            return read
                .GroupBy(val => {
                    var value = GetKeyColumn(typeof(TValue)).GetValue(val);
                    if (typeof(TKey) == typeof(string))
                    {
                        return (TKey)(object)value.ToString();
                    } else
                    {
                        return (TKey)value;
                    }
                }).ToDictionary(x => x.Key, x => x.Last());
        }
        /// <summary>
        /// Get Rows is Dictionary(Key:TKey), Columns is TValue
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <param name="xsvPathInResources">Resources file path</param>
        /// <returns>Dictionary(Key:TKey) of TValue</returns>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources,
            bool headerEnable = true)
        {
            return GetDictionary<TKey, TValue>(
                delimiter, Resources.Load(xsvPathInResources) as TextAsset, headerEnable);
        }
        /// <summary>
        /// Get Rows is Dictionary(Key:string), Columns is TValue
        /// </summary>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvAsset">TextAsset Data</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        /// <returns>Dictionary(Key:string) of TValue</returns>
        public static Dictionary<string, TValue> GetDictionary<TValue>(
            XsvParser.Delimiter delimiter, 
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            return GetDictionary<string, TValue>(delimiter, xsvAsset, headerEnable);
        }
        /// <summary>
        /// Get Rows is Dictionary(Key:TKey), Columns is TValue
        /// </summary>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvPathInResources">TextPath in Resources</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        /// <returns>Dictionary(Key:TKey) of TValue</returns>
        public static Dictionary<string, TValue> GetDictionary<TValue>(
            XsvParser.Delimiter delimiter, 
            string xsvPathInResources,
            bool headerEnable = true)
        {
            return GetDictionary<string, TValue>(
                delimiter, Resources.Load(xsvPathInResources) as TextAsset, headerEnable);
        }
        /// <summary>
        /// Get Rows is List, Columns is TValue
        /// </summary>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvAsset">TextAsset Data</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        /// <returns>List of TValue</returns>
        public static List<TValue> GetList<TValue>(
            XsvParser.Delimiter delimiter, 
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            return ReadXsv<TValue>(delimiter, xsvAsset, headerEnable).ToList();
        }
        /// <summary>
        /// Get Rows is List, Columns is TValue
        /// </summary>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvPathInResources">TextPath in Resources</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        /// <returns>List of TValue</returns>
        public static List<TValue> GetList<TValue>(
            XsvParser.Delimiter delimiter, 
            string xsvPathInResources,
            bool headerEnable = true)
        {
            return ReadXsv<TValue>(delimiter, Resources.Load(xsvPathInResources) as TextAsset, headerEnable).ToList();
        }
        /// <summary>
        /// Get Rows is List, Columns is List(string)
        /// </summary>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvAsset">TextAsset Data</param>
        /// <param name="headerEnable">true:Skip First Rows, default:true</param>
        /// <returns>List of List(string)</returns>
        public static List<List<string>> GetList(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            var parse = XsvParser.Parse(delimiter, xsvAsset.text);
            var list = parse.Select(x => x.ToList()).ToList();
            if (headerEnable) list.RemoveAt(0);
            return list;
        }
        /// <summary>
        /// Get Rows is List, Columns is List(string)
        /// </summary>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvPathInResources">TextPath in Resources</param>
        /// <param name="headerEnable">default: Skip First rows</param>
        /// <returns>List of List(string)</returns>
        public static List<List<string>> GetList(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources,
            bool headerEnable = true)
        {
            return GetList(delimiter, Resources.Load(xsvPathInResources) as TextAsset, headerEnable);
        }
        /// <summary>
        /// The same as GetWithHeader,
        /// Rows is List, Columns is Dictionary(Key:string)
        /// </summary>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvAsset">TextAsset Data</param>
        /// <returns>List of Dictionary(Key:string)</returns>
        public static List<Dictionary<string, string>> GetListWithHeader(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset)
        {
            return XsvParser.ParseWithHeader(delimiter, xsvAsset.text)
                .Select(x => x.ToDictionary(a => a.Key, a => a.Value)).ToList(); ;
        }
        /// <summary>
        /// The same as GetWithHeader,
        /// Rows is List, Columns is Dictionary(Key:string)
        /// </summary>
        /// <param name="delimiter">Delimiter Type</param>
        /// <param name="xsvPathInResources">TextPath in Resources</param>
        /// <returns>List of Dictionary(Key:string)</returns>
        public static List<Dictionary<string, string>> GetListWithHeader(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources)
        {
            return GetListWithHeader(delimiter, Resources.Load(xsvPathInResources) as TextAsset);
        }

        /// <summary>
        /// Row is Dictionary(Key:TKey), Columns is TValue
        /// TextAsset or TextPath in Resources Read from Member Values
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <returns>Dictionary(Key:TKey) of TValue</returns>
        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>()
        {
            if (this.xsvAsset == null)
                return GetDictionary<TKey, TValue>(delimiter, this.xsvPathInResources, this.headerEnable);
            else
                return GetDictionary<TKey, TValue>(delimiter, this.xsvAsset, this.headerEnable);
        }
        /// <summary>
        /// Rows is Dictionary(Key:String), Columns is TValue,
        /// TextAsset or TextPath in Resources Read from Member Values
        /// </summary>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <returns>Dictionary(Key:string) of TValue</returns>
        public Dictionary<string, TValue> GetDictionary<TValue>() {
            if (this.xsvAsset == null)
                return GetDictionary<string, TValue>(delimiter, this.xsvPathInResources, this.headerEnable);
            else
                return GetDictionary<string, TValue>(delimiter, this.xsvAsset, this.headerEnable);
        }
        /// <summary>
        /// Rows is List, Columns is TValue,
        /// TextAsset or TextPath in Resources Read from Member Values
        /// </summary>
        /// <typeparam name="TValue">Columns Type</typeparam>
        /// <returns>List of TValue</returns>
        public List<TValue> GetList<TValue>()
        {
            if (this.xsvAsset == null)
                return GetList<TValue>(delimiter, this.xsvPathInResources, this.headerEnable);
            else
                return GetList<TValue>(delimiter, this.xsvAsset, this.headerEnable);
        }
        /// <summary>
        /// Rows is List, Columns is List(string),
        /// TextAsset or TextPath in Resources Read from Member Values
        /// </summary>
        /// <returns>List of List(string)</returns>
        public List<List<string>> GetList()
        {
            if (this.xsvAsset == null)
                return GetList(delimiter, this.xsvPathInResources, this.headerEnable);
            else
                return GetList(delimiter, this.xsvAsset, this.headerEnable);
        }
    }
}
