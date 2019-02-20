using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
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
            get
            {
                if (xsvAsset == null && !string.IsNullOrEmpty(XsvPathInResources))
                {
                    xsvAsset = Resources.Load<TextAsset>(XsvPathInResources);
                }

                return xsvAsset;
            }
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
            [XsvRow] public IEnumerable<TValue> Rows { get; [UsedImplicitly] set; }

            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
            public static void AOTWorkaround()
            {
                new Data<TValue>();
                new List<Data<TValue>>();
            }
        }

        /// <summary>
        /// カラムを解析して主キーを特定する、XsvKeyプロパティ指定で主キーとなる
        /// Setting of Primary key from XsvKey Attribute Property
        /// </summary>
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        private static object GetKeyColumn<T>(T instance)
        {
            var type = typeof(T);
            var keyFieldList = type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<XsvKeyAttribute>() != null)
                .ToList();
            if (keyFieldList.Any())
            {
                return keyFieldList.First().GetValue(instance);
            }

            var keyPropertyList = type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<XsvKeyAttribute>() != null)
                .ToList();
            if (keyPropertyList.Any())
            {
                return keyPropertyList.First().GetValue(instance);
            }

            return null;
        }

        /// <summary>
        /// XsvDeserializeを使ってTValueの型でパース開始
        /// Run XsvDeserialize with options
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        private static IEnumerable<TValue> ReadXsv<TValue>(
            XsvParser.Delimiter delimiter,
            TextAsset xsvAsset,
            bool headerEnable = true)
        {
            if (xsvAsset == null)
            {
                return null;
            }

            return
                (
                    headerEnable
                        ? InternalSerializer.DeserializeWithHeader<Data<TValue>>(delimiter, xsvAsset.text)
                        : InternalSerializer.Deserialize<Data<TValue>>(delimiter, xsvAsset.text)
                )
                .Rows;
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
            return ReadXsv<TValue>(delimiter, xsvAsset, headerEnable)?
                .GroupBy(
                    val =>
                    {
                        var value = GetKeyColumn(val);
                        if (typeof(TKey) == typeof(string))
                        {
                            return (TKey) (object) value.ToString();
                        }

                        return (TKey) value;
                    }
                )
                .ToDictionary(x => x.Key, x => x.Last());
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
            if (headerEnable)
            {
                list.RemoveAt(0);
            }

            return list;
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
        /// TValueの型でメンバ変数から行を辞書型にして取り出し
        /// Get Directry, Columns of TValue, of Select KeyType Mode from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>()
        {
            return GetDictionary<TKey, TValue>(Delimiter, XsvAsset, HeaderEnable);
        }

        /// <summary>
        /// TValueの型で文字列をキーとしてメンバ変数から行を辞書型にして取り出し
        /// Get Rows of Dictionary, Columns of TValue, style and String KeyType Mode from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public Dictionary<string, TValue> GetDictionary<TValue>()
        {
            return GetDictionary<string, TValue>(Delimiter, XsvAsset, HeaderEnable);
        }

        /// <summary>
        /// TValueの型でメンバ変数から行をリスト型にして取り出し
        /// Get Rows of List, Columns of TValue, style from Member of TextAsset or Resources Filepath
        /// </summary>
        /// <typeparam name="TValue">Columns(Struct or Class) Type</typeparam>
        public List<TValue> GetList<TValue>()
        {
            return GetList<TValue>(Delimiter, XsvAsset, HeaderEnable);
        }

        /// <summary>
        /// メンバ変数から行をリスト型にして取り出し
        /// Get Rows of List, Columns of List, style from Member of TextAsset or Resources Filepath
        /// </summary>
        public List<List<string>> GetList()
        {
            return GetList(Delimiter, XsvAsset, HeaderEnable);
        }
    }
}
