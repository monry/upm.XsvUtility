using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monry.XsvUtility;
using System.Linq;

namespace Monry.XsvUtility
{
    public class XsvReaderTest
    {
        public struct SampleColumns
        {
            [XsvColumn(0), XsvKey] public int id;
            [XsvColumn(1)] public string name;
            [XsvColumn(2)] public string place;
        }
        [SerializeField] public XsvReader m_XsvData;
        public void main()
        {
            var rowsAsset = new TextAsset(
                "id,name,place\n" +
                "10,Bob,State of Connecticut\n" +
                "100,Michael,\"Manhattan Borough\nNew York County\"");
            Debug.Log(string.Format("TextAsset:\n{0}", rowsAsset.text.Replace("\n", "\\n")));
            {
                // case 1 Return List/List
                var list = XsvReader.GetList(XsvParser.Delimiter.Comma, rowsAsset).ToList();
                Debug.Log(string.Format("Case1 GetList List/List; List[0][0]:{0}, List[1][0]:{1}", list[0].ToList()[0], list[1].ToList()[0]));
            }
            {
                // case 2 Return List/List NoneHeader
                var list = XsvReader.GetList(XsvParser.Delimiter.Comma, rowsAsset, false).ToList();
                Debug.Log(string.Format("Case2 GetList List/List NoneHeader; List[0][0]:{0}, List[1][0]:{1}", list[0].ToList()[0], list[1].ToList()[0]));
            }
            {
                // case 3 Return List/Dictionary
                var list = XsvReader.GetListWithHeader(XsvParser.Delimiter.Comma, rowsAsset).ToList();
                Debug.Log(string.Format("Case3 GetListWithHeader List/Dictionary; " +
                    "List[0][\"place\"]:{0}, List[1][\"place\"]:{1}", list[0]["place"], list[1]["place"]));
            }
            {
                // case 4 Return List/T
                var list = XsvReader.GetList<SampleColumns>(XsvParser.Delimiter.Comma, rowsAsset).ToList();
                Debug.Log(string.Format("Case4 GetList<T> List/T; " +
                    "List[0].name:{0}, List[1].name:{1}", list[0].name, list[1].name));
            }
            {
                // case 5 Return List/T NoneHeader
                var list = XsvReader.GetList<SampleColumns>(XsvParser.Delimiter.Comma, rowsAsset, false).ToList();
                Debug.Log(string.Format("Case5 GetList<T> NoneHeader List/T; " +
                    "List[0].name:{0}, List[1].name:{1}", list[0].name, list[1].name));
            }
            {
                // case 6 Return Dictionary/T (Auto:TKey=string)
                var dic = XsvReader.GetDictionary<SampleColumns>(XsvParser.Delimiter.Comma, rowsAsset);
                Debug.Log(string.Format("Case6 GetDictionary<T> (AutoKey:String) Dictionary/T; " +
                    "Dic[\"10\"].id:{0}, Dic[\"100\"].id:{1}", dic["10"].id, dic["100"].id));
            }
            {
                // case 7 Return Dictionary<TKey>/T
                var dic = XsvReader.GetDictionary<int, SampleColumns>(XsvParser.Delimiter.Comma, rowsAsset);
                Debug.Log(string.Format("Case7 GetDictionary<int, T> Dictionary/T; " +
                    "Dic[10].id:{0}, Dic[100].id:{1}", dic[10].id, dic[100].id));
            }
            m_XsvData.Delimiter = XsvParser.Delimiter.Comma;
            m_XsvData.XsvAsset = rowsAsset;
            m_XsvData.HeaderEnable = true;
            Debug.Log("Set XsvReader m_XsvData from Serialize Field, \n" +
                "Current Example Case; m_Delimiter:Comma, m_XsvAsset = rowsAsset, m_HeaderEnable = true");
            {
                // case 8 Use XsvReader Return List/T
                var list = m_XsvData.GetList<SampleColumns>().ToList();
                Debug.Log(string.Format("Case8 m_XsvData.GetList<T> List/T; " +
                    "List[0].name:{0}, List[1].name:{1}", list[0].name, list[1].name));
            }
            {
                // case 9 Use XsvReader Return Dictionary<TKey>/T
                var dic = m_XsvData.GetDictionary<int, SampleColumns>();
                Debug.Log(string.Format("Case9 m_XsvData.GetDictionary<int, T> Dictionary/T; " +
                    "Dic[10].place:{0}, Dic[100].place:{1}", dic[10].place, dic[100].place));
            }
        }
    }
}
