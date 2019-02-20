using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Monry.XsvUtility
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class XsvReaderTest
    {
        public struct SampleColumns
        {
            [XsvColumn(0), XsvKey] public int id;
            [XsvColumn(1)] public string name;
            [XsvColumn(2)] public string place;
        }
        TextAsset rowsAsset = new TextAsset(
            "id,name,place\n" +
            "10,Bob,State of Connecticut\n" +
            "100,Michael,\"Manhattan Borough\nNew York County\"");
        [Test]
        public void Case1_List_ListString()
        {
            //Debug.Log(string.Format("TextAsset:\n{0}", rowsAsset.text.Replace("\n", "\\n")));
            // case 1 Return List/List
            var rows = XsvReader.GetList(XsvParser.Delimiter.Comma, rowsAsset);
            //Debug.Log(string.Format("Case1 GetList List/List; rows[0][0]:{0}, rows[1][0]:{1}", rows[0][0], rows[1][0]));
            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual(3, rows[0].Count);
            Assert.AreEqual(3, rows[1].Count);
            Assert.AreEqual("10", rows[0][0]);
            Assert.AreEqual("100", rows[1][0]);
        }
        [Test]
        public void Case2_List_ListString_NoneHeader()
        {
            // case 2 Return List/List NoneHeader
            var rows = XsvReader.GetList(XsvParser.Delimiter.Comma, rowsAsset, false);
            //Debug.Log(string.Format("Case2 GetList List/List NoneHeader; rows[0][0]:{0}, rows[1][0]:{1}", rows[0][0], rows[1][0]));
            Assert.AreEqual(3, rows.Count);
            Assert.AreEqual(3, rows[0].Count);
            Assert.AreEqual(3, rows[1].Count);
            Assert.AreEqual("id", rows[0][0]);
            Assert.AreEqual("10", rows[1][0]);
        }
        [Test]
        public void Case3_List_DictionaryString()
        {
            // case 3 Return List/Dictionary
            var rows = XsvReader.GetListWithHeader(XsvParser.Delimiter.Comma, rowsAsset);
            //Debug.Log(string.Format("Case3 GetListWithHeader List/Dictionary; " +
            //    "rows[0][\"place\"]:{0}, rows[1][\"place\"]:{1}", rows[0]["place"], rows[1]["place"]));
            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual(3, rows[0].Count);
            Assert.AreEqual(3, rows[1].Count);
            Assert.AreEqual("State of Connecticut", rows[0]["place"]);
            Assert.AreEqual("Manhattan Borough\nNew York County", rows[1]["place"]);
        }
        [Test]
        public void Case4_List_TValue()
        {
            // case 4 Return List/T
            var rows = XsvReader.GetList<SampleColumns>(XsvParser.Delimiter.Comma, rowsAsset);
            //Debug.Log(string.Format("Case4 GetList<T> List/T; " +
            //    "rows[0].name:{0}, rows[1].name:{1}", rows[0].name, rows[1].name));
            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual("Bob", rows[0].name);
            Assert.AreEqual("Michael", rows[1].name);
        }
        [Test]
        public void Case5_List_TValue_NoneHeader()
        {
            // case 5 Return List/T NoneHeader
            var rows = XsvReader.GetList<SampleColumns>(XsvParser.Delimiter.Comma, rowsAsset, false);
            //Debug.Log(string.Format("Case5 GetList<T> NoneHeader List/T; " +
            //    "rows[0].name:{0}, rows[1].name:{1}", rows[0].name, rows[1].name));
            Assert.AreEqual(3, rows.Count);
            Assert.AreEqual("name", rows[0].name);
            Assert.AreEqual("Bob", rows[1].name);
        }
        [Test]
        public void Case6_DictionaryString_TValue()
        {
            // case 6 Return Dictionary/T (Auto:TKey=string)
            var dic = XsvReader.GetDictionary<SampleColumns>(XsvParser.Delimiter.Comma, rowsAsset);
            //Debug.Log(string.Format("Case6 GetDictionary<T> (AutoKey:String) Dictionary/T; " +
            //    "Dic[\"10\"].id:{0}, Dic[\"100\"].id:{1}", dic["10"].id, dic["100"].id));
            Assert.AreEqual(2, dic.Count);
            Assert.AreEqual(10, dic["10"].id);
            Assert.AreEqual(100, dic["100"].id);
        }
        [Test]
        public void Case7_DictionaryTKey_TValue()
        {
            // case 7 Return Dictionary<TKey>/T
            var dic = XsvReader.GetDictionary<int, SampleColumns>(XsvParser.Delimiter.Comma, rowsAsset);
            //Debug.Log(string.Format("Case7 GetDictionary<int, T> Dictionary/T; " +
            //    "Dic[10].id:{0}, Dic[100].id:{1}", dic[10].id, dic[100].id));
            Assert.AreEqual(10, dic[10].id);
            Assert.AreEqual(100, dic[100].id);
        }
        [Test]
        public void Case8_UseClass_List_TValue()
        {
            // case 8 Use XsvReader Return List/T
            XsvReader m_XsvData = new XsvReader(XsvParser.Delimiter.Comma, rowsAsset, true);
            //Debug.Log("Set XsvReader m_XsvData from Serialize Field, \n" +
            //    "Current Example Case; m_Delimiter:Comma, m_XsvAsset = rowsAsset, m_HeaderEnable = true");
            var rows = m_XsvData.GetList<SampleColumns>();
            //Debug.Log(string.Format("Case8 m_XsvData.GetList<T> List/T; " +
            //    "rows[0].name:{0}, rows[1].name:{1}", rows[0].name, rows[1].name));
            Assert.AreEqual("Bob", rows[0].name);
            Assert.AreEqual("Michael", rows[1].name);
        }
        [Test]
        public void Case9_UseClass_DictionaryTKey_TValue()
        {
            // case 9 Use XsvReader Return Dictionary<TKey>/T
            XsvReader m_XsvData = new XsvReader(XsvParser.Delimiter.Comma, rowsAsset, true);
            var dic = m_XsvData.GetDictionary<int, SampleColumns>();
            //Debug.Log(string.Format("Case9 m_XsvData.GetDictionary<int, T> Dictionary/T; " +
            //    "Dic[10].place:{0}, Dic[100].place:{1}", dic[10].place, dic[100].place));
            Assert.AreEqual("State of Connecticut", dic[10].place);
            Assert.AreEqual("Manhattan Borough\nNew York County", dic[100].place);
        }
    }
}