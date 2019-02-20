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
        private XsvParser.Delimiter delimiter = XsvParser.Delimiter.Comma;
        [SerializeField, Tooltip("CSV or TSV Asset, Most Prioritable")]
        private TextAsset xsvAsset;
        [SerializeField, Tooltip("CSV or TSV File Path, then XsvAsset is null")]
        private string xsvPathInResources;
        [SerializeField, Tooltip("Header Row Skip Flag")]
        private bool headerEnable = true;

        public XsvParser.Delimiter Delimiter
        {
            get => delimiter;
            set => delimiter = value;
        }

        public TextAsset XsvAsset
        {
            get => xsvAsset;
            set => xsvAsset = value;
        }

        public string XsvPathInResources
        {
            get => xsvPathInResources;
            set => xsvPathInResources = value;
        }

        public bool HeaderEnable
        {
            get => headerEnable;
            set => headerEnable = value;
        }

        public struct Data<TValue>
        {
            [XsvRow] public IEnumerable<TValue> Rows { get; set; }
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
            return keyList.Count > 0 ? keyList[0] : null;
        }
        /// <summary>
        /// XsvDeserializeを使ってTValueの型でパース開始
        /// Run XsvDeserialize with options
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
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
        /// TValueの型で行を辞書型にして取り出し
        /// Get Rows of Dictionary, Columns of TValue, style and Select KeyType Mode from TextAsset
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
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
                    }

                    return (TKey)value;
                }).ToDictionary(x => x.Key, x => x.Last());
        }

        /// <summary>
        /// TValueの型でファイルパスから行を辞書型にして取り出し
        /// Get Rows of Dictionary, Columns of TValue, style and Select KeyType Mode from ResourcesPath
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources,
            bool headerEnable = true)
        {
            return GetDictionary<TKey, TValue>(
                delimiter, Resources.Load(xsvPathInResources) as TextAsset, headerEnable);
        }
        /// <summary>
        /// TValueの型で文字列をキーとして行を辞書型にして取り出し
        /// Get Rows of Dictionary, Columns of TValue, style and String KeyType Mode
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public static Dictionary<string, TValue> GetDictionary<TValue>(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            return GetDictionary<string, TValue>(delimiter, xsvAsset, headerEnable);
        }
        /// <summary>
        /// TValueの型で文字列をキーとしてファイルパスから行を辞書型にして取り出し
        /// Get Rows of Dictionary, Columns of TValue, style and String KeyType Mode from ResourcesPath
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public static Dictionary<string, TValue> GetDictionary<TValue>(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources,
            bool headerEnable = true)
        {
            return GetDictionary<string, TValue>(
                delimiter, Resources.Load(xsvPathInResources) as TextAsset, headerEnable);
        }
        /// <summary>
        /// TValueの型で行をリスト型にして取り出し
        /// Get Rows of List style from TextAsset of TValue, Columns of TValue
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public static List<TValue> GetList<TValue>(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            return ReadXsv<TValue>(delimiter, xsvAsset, headerEnable).ToList();
        }
        /// <summary>
        /// TValueの型でファイルパスから行をリスト型にして取り出し
        /// Get Rows of List style from ResourcesPath, Columns of TValue
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public static List<TValue> GetList<TValue>(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources,
            bool headerEnable = true)
        {
            return ReadXsv<TValue>(delimiter, Resources.Load(xsvPathInResources) as TextAsset, headerEnable).ToList();
        }

        /// <summary>
        /// 列をリスト型、行もリスト型にして、アセットから取り出し
        /// Get Rows of List, Columns of List, style from TextAsset
        /// </summary>
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
        /// 列をリスト型、行もリスト型にして、ファイルパスから取り出し
        /// Get Rows of List, Columns of List, style from TextAsset
        /// </summary>
        public static List<List<string>> GetList(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources,
            bool headerEnable = true)
        {
            return GetList(delimiter, Resources.Load(xsvPathInResources) as TextAsset, headerEnable);
        }
        /// <summary>
        /// 列を辞書型、行をリスト型にして、アセットから取り出し
        /// Get Rows of List, Columns of Dictionary, style from TextAsset
        /// </summary>
        public static List<Dictionary<string, string>> GetListWithHeader(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset)
        {
            return XsvParser.ParseWithHeader(delimiter, xsvAsset.text)
                .Select(x => x.ToDictionary(a => a.Key, a => a.Value)).ToList();
        }
        /// <summary>
        /// 列を辞書型、行をリスト型にして、ファイルパスから取り出し
        /// Get Rows of List, Columns of Dictionary, style from TextAsset
        /// </summary>
        public static List<Dictionary<string, string>> GetListWithHeader(
            XsvParser.Delimiter delimiter,
            string xsvPathInResources)
        {
            return GetListWithHeader(delimiter, Resources.Load(xsvPathInResources) as TextAsset);
        }

        /// <summary>
        /// TValueの型でメンバ変数から行を辞書型にして取り出し
        /// Get Directry, Columns of TValue, of Select KeyType Mode from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>()
        {
            if (XsvAsset == null)
                return GetDictionary<TKey, TValue>(Delimiter, XsvPathInResources, HeaderEnable);
            return GetDictionary<TKey, TValue>(Delimiter, XsvAsset, HeaderEnable);
        }
        /// <summary>
        /// TValueの型で文字列をキーとしてメンバ変数から行を辞書型にして取り出し
        /// Get Rows of Dictionary, Columns of TValue, style and String KeyType Mode from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public Dictionary<string, TValue> GetDictionary<TValue>() {
            if (XsvAsset == null)
                return GetDictionary<string, TValue>(Delimiter, XsvPathInResources, HeaderEnable);
            else
                return GetDictionary<string, TValue>(Delimiter, XsvAsset, HeaderEnable);
        }
        /// <summary>
        /// TValueの型でメンバ変数から行をリスト型にして取り出し
        /// Get Rows of List, Columns of TValue, style from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public List<TValue> GetList<TValue>()
        {
            if (XsvAsset == null)
                return GetList<TValue>(Delimiter, XsvPathInResources, HeaderEnable);
            return GetList<TValue>(Delimiter, XsvAsset, HeaderEnable);
        }
        /// <summary>
        /// メンバ変数から行をリスト型にして取り出し
        /// Get Rows of List, Columns of List, style from Member of TextAsset or Resources Filepath
        /// </summary>
        public List<List<string>> GetList()
        {
            if (XsvAsset == null)
                return GetList(Delimiter, XsvPathInResources, HeaderEnable);
            return GetList(Delimiter, XsvAsset, HeaderEnable);
        }
    }
}
