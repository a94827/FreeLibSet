using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using System.Reflection;
using FreeLibSet.Remoting;

namespace ExtTools_tests.Collections
{
  [TestFixture]
  public class SingleScopeStringListTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_IgnoreCase([Values(true, false)] bool ignoreCase)
    {
      SingleScopeStringList sut = new SingleScopeStringList(ignoreCase);
      Assert.AreEqual(ignoreCase, sut.IgnoreCase, "IgnoreCase");
      Assert.AreEqual(0, sut.Count, "Count");
      Assert.IsFalse(sut.IsReadOnly, "IsReadOnly");
      Assert.AreSame(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal, sut.Comparer, "Comparer");
    }

    [TestCase("Ordinal", false)]
    [TestCase("OrdinalIgnoreCase", true)]
    [TestCase("InvariantCulture", false)]
    [TestCase("InvariantCultureIgnoreCase", true)]
    [TestCase("CurrentCulture", false)]
    [TestCase("CurrentCultureIgnoreCase", true)]
    public void Constructor_Comparer(string comparerName, bool wantedIgnoreCase)
    {
      PropertyInfo pi = typeof(StringComparer).GetProperty(comparerName);
      StringComparer comparer = pi.GetValue(null, null) as StringComparer;
      if (comparer == null)
        throw new NullReferenceException("StringComparer not found");

      SingleScopeStringList sut = new SingleScopeStringList(comparer);
      Assert.AreSame(comparer, sut.Comparer, "Comparer");
      Assert.AreEqual(wantedIgnoreCase, sut.IgnoreCase, "IgnoreCase");
    }

    public void Constructor_Comparer_Null()
    {
      Assert.Catch<ArgumentException>(delegate() { new SingleScopeStringList((StringComparer)null); });
    }


    [Test]
    public void Constructor_Collection([Values(true, false)] bool ignoreCase)
    {
      string[] source = new string[] { "AaA", "bbb", "aAa", "bbb" };
      string[] wantedRes = ignoreCase ? new string[] { "AaA", "bbb" } : new string[] { "AaA", "bbb", "aAa" };

      ICollection<string> source1 = new List<String>(source);
      SingleScopeStringList sut1 = new SingleScopeStringList(source1, ignoreCase);
      Assert.AreEqual(wantedRes, sut1.ToArray(), "#1");

      IEnumerable<string> source2 = new ArrayEnumerable<string>(source);
      SingleScopeStringList sut2 = new SingleScopeStringList(source2, ignoreCase);
      Assert.AreEqual(wantedRes, sut2.ToArray(), "#2");
    }

    [Test]
    public void Constructor_Collection_Empty([Values(true, false)] bool ignoreCase)
    {
      ICollection<string> source1 = new List<String>(EmptyArray<string>.Empty);
      SingleScopeStringList sut1 = new SingleScopeStringList(source1, ignoreCase);
      Assert.AreEqual(0, sut1.Count, "#1");

      IEnumerable<string> source2 = new ArrayEnumerable<string>(EmptyArray<string>.Empty);
      SingleScopeStringList sut2 = new SingleScopeStringList(source2, ignoreCase);
      Assert.AreEqual(0, sut2.Count, "#2");
    }

    #endregion

    #region Item()

    [TestCase("Aaa,Bbb", 0, false, "Aaa")]
    [TestCase("Aaa,Bbb", 0, true, "Aaa")]
    public void Item_get(string sOrgItems, int index, bool ignoreCase, string wanted)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);

      string s = sut[index];
      Assert.AreEqual(wanted, s);
    }

    [Test]
    public void Item_get_OutOfRange()
    {
      SingleScopeStringList sut = new SingleScopeStringList(new string[] { "Aaa", "Bbb" }, true);
      string dummy;
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[-1]; }, "#1");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { dummy = sut[2]; }, "#2");
    }

    [TestCase("Aaa,Bbb", 0, "Ccc", false, "Ccc,Bbb")]
    [TestCase("Aaa,Bbb", 0, "Ccc", true, "Ccc,Bbb")]
    [TestCase("Aaa,Bbb", 1, "BBB", false, "Aaa,BBB")]
    [TestCase("Aaa,Bbb", 1, "BBB", true, "Aaa,BBB")]
    [TestCase("Aaa,Bbb", 0, "AAA", true, "AAA,Bbb")]
    public void Item_set(string sOrgItems, int index, string item, bool ignoreCase, string sWanted)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      string[] wanted = ToStringArray(sWanted);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);

      sut[index] = item;
      Assert.AreEqual(wanted, sut.ToArray());
    }

    [Test]
    public void Item_set_OutOfRange()
    {
      SingleScopeStringList sut = new SingleScopeStringList(new string[] { "Aaa", "Bbb" }, true);

      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut[-1] = "CCC"; }, "#1");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut[2] = "CCC"; }, "#2");
    }

    public void Item_set_SameValueException()
    {
      SingleScopeStringList sut = new SingleScopeStringList(new string[] { "Aaa", "Bbb" }, true);
      Assert.Catch(delegate() { sut[0] = "BBB"; });
    }


    #endregion

    #region Contains/IndexOf()

    [TestCase("Aaa,Bbb", "BBB", false, -1)]
    [TestCase("Aaa,Bbb", "BBB", true, 1)]
    [TestCase("Aaa,Bbb", "bbb", true, 1)]
    [TestCase("Aaa,Bbb", "Aaa", false, 0)]
    [TestCase("Aaa,Bbb", "Aaa", true, 0)]
    [TestCase("", "BBB", true, -1)]
    public void IndexOf_Contains(string sOrgItems, string item, bool ignoreCase, int wanted)
    {
      string[] orgItems = ToStringArray(sOrgItems);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);

      int res1 = sut.IndexOf(item);
      Assert.AreEqual(wanted, res1, "IndexOf()");

      bool res2 = sut.Contains(item);
      Assert.AreEqual(wanted >= 0, res2, "Contains()");
    }

    [TestCase("Bbb", false, -1)]
    [TestCase("Bbb", true, 1)]
    [TestCase("666", false, -1)]
    [TestCase("666", true, -1)]
    public void IndexOf_Contains_LongList(string item, bool ignoreCase, int wanted)
    {
      // Если в списке больше 20 элементов, используется дополнительная проверка в IndexOf()
      // См. реализацию метода

      string[] orgItems = new string[26]; // "aaa", "bbb", ..., "zzz"
      for (int i = 0; i < 26; i++)
      {
        char c =  Convert.ToChar(Convert.ToInt32('A') + i);
        orgItems[i] = new string(c, 3);
      }

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);

      int res1 = sut.IndexOf(item);
      Assert.AreEqual(wanted, res1, "IndexOf()");

      bool res2 = sut.Contains(item);
      Assert.AreEqual(wanted >= 0, res2, "Contains()");
    }

    #endregion

    #region Add/Remove()

    [TestCase("Aaa,Bbb", "AAA", false, "Aaa,Bbb,AAA")]
    [TestCase("Aaa,Bbb", "AAA", true, "Aaa,Bbb")]
    [TestCase("Aaa,Bbb", "aaa", false, "Aaa,Bbb,aaa")]
    [TestCase("Aaa,Bbb", "aaa", true, "Aaa,Bbb")]
    [TestCase("Aaa,Bbb", "Aaa", false, "Aaa,Bbb")]
    [TestCase("Aaa,Bbb", "Aaa", true, "Aaa,Bbb")]
    [TestCase("", "Aaa", false, "Aaa")]
    [TestCase("", "Aaa", true, "Aaa")]
    public void Add(string sOrgItems, string added, bool ignoreCase, string sWanted)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      string[] wanted = ToStringArray(sWanted);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);
      sut.Add(added);
      Assert.AreEqual(wanted, sut.ToArray());
    }

    [TestCase("Aaa,Bbb,Ccc", "ddd,EEE", false, "Aaa,Bbb,Ccc,ddd,EEE")]
    [TestCase("Aaa,Bbb,Ccc", "ddd,EEE", true, "Aaa,Bbb,Ccc,ddd,EEE")]
    [TestCase("Aaa,Bbb,Ccc", "CCC,EEE", false, "Aaa,Bbb,Ccc,CCC,EEE")]
    [TestCase("Aaa,Bbb,Ccc", "CCC,EEE", true, "Aaa,Bbb,Ccc,EEE")]
    [TestCase("Aaa,Bbb,Ccc", "Ccc,EEE", false, "Aaa,Bbb,Ccc,EEE")]
    [TestCase("Aaa,Bbb,Ccc", "Ccc,EEE", true, "Aaa,Bbb,Ccc,EEE")]
    [TestCase("Aaa,Bbb,Ccc", "BBB,bbb", false, "Aaa,Bbb,Ccc,BBB,bbb")]
    [TestCase("Aaa,Bbb,Ccc", "BBB,bbb", true, "Aaa,Bbb,Ccc")]
    [TestCase("Aaa,Bbb,Ccc", "", false, "Aaa,Bbb,Ccc")]
    [TestCase("", "Aaa,Bbb,Ccc", false, "Aaa,Bbb,Ccc")]
    [TestCase("", "", false, "")]
    public void AddRange(string sOrgItems, string sAdded, bool ignoreCase, string sWanted)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      string[] added = ToStringArray(sAdded);
      string[] wanted = ToStringArray(sWanted);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);
      sut.AddRange(added);
      Assert.AreEqual(wanted, sut.ToArray());
    }

    [TestCase("Aaa,Bbb", 0, "AAA", false, "AAA,Aaa,Bbb")]
    [TestCase("Aaa,Bbb", 0, "AAA", true, "Aaa,Bbb")]
    [TestCase("Aaa,Bbb", 1, "AAA", false, "Aaa,AAA,Bbb")]
    [TestCase("Aaa,Bbb", 1, "AAA", true, "Aaa,Bbb")]
    [TestCase("Aaa,Bbb", 2, "AAA", false, "Aaa,Bbb,AAA")]
    [TestCase("Aaa,Bbb", 2, "AAA", true, "Aaa,Bbb")]
    public void Insert(string sOrgItems, int index, string added, bool ignoreCase, string sWanted)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      string[] wanted = ToStringArray(sWanted);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);
      sut.Insert(index, added);
      Assert.AreEqual(wanted, sut.ToArray());
    }

    [TestCase("Aaa,Bbb", "Aaa", false, true, "Bbb")]
    [TestCase("Aaa,Bbb", "Aaa", true, true, "Bbb")]
    [TestCase("Aaa,Bbb", "BBB", false, false, "Aaa,Bbb")]
    [TestCase("Aaa,Bbb", "BBB", true, true, "Aaa")]
    [TestCase("Aaa,Bbb", "CCC", false, false, "Aaa,Bbb")]
    [TestCase("Aaa,Bbb", "CCC", true, false, "Aaa,Bbb")]
    public void Remove(string sOrgItems, string item, bool ignoreCase, bool wantedRes, string sWantedArray)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      string[] wantedArray = ToStringArray(sWantedArray);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);
      bool res = sut.Remove(item);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(wantedArray, sut.ToArray(), "ToArray()");
    }

    [TestCase("Aaa,Bbb", 0, false, "Bbb")]
    [TestCase("Aaa,Bbb", 0, true, "Bbb")]
    [TestCase("Aaa,Bbb", 1, true, "Aaa")]
    public void RemoveAt(string sOrgItems, int index, bool ignoreCase, string sWantedArray)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      string[] wantedArray = ToStringArray(sWantedArray);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);

      sut.RemoveAt(index);
      Assert.AreEqual(wantedArray, sut.ToArray(), "ToArray()");
    }

    [Test]
    public void RemoveAt_OutOfRange()
    {
      SingleScopeStringList sut = new SingleScopeStringList(new string[] { "Aaa", "Bbb" }, true);

      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut.RemoveAt(-1); }, "#1");
      Assert.Catch<ArgumentOutOfRangeException>(delegate() { sut.RemoveAt(2); }, "#2");
    }

    #endregion

    #region SetReadOnly()

    private class SingleScopeStringList_RO : SingleScopeStringList
    {
      public SingleScopeStringList_RO(ICollection<string>source, bool ignoreCase)
        :base(source, ignoreCase)
      { 
      }

      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    [Test]
    public void SetReadOnly()
    {
      SingleScopeStringList_RO sut = new SingleScopeStringList_RO(new string[] { "Aaa", "Bbb" }, true);

      sut.SetReadOnly();

      Assert.IsTrue(sut.IsReadOnly, "IsReadOnly");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.CheckNotReadOnly(); }, "CheckNotReadOnly()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut[1]="SSS"; }, "Item_Set()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Add("SSS"); }, "Add()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.AddRange(new string[]{"SSS"}); }, "Add()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Insert(1, "SSS"); }, "Insert()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Remove("SSS"); }, "Remove()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.RemoveAt(1); }, "RemoveAt()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Clear(); }, "Clear()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Clear(); }, "Clear()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Reverse(); }, "Reverse()");
      Assert.Catch<ObjectReadOnlyException>(delegate() { sut.Sort(); }, "Sort()");
    }

    #endregion

    #region Перечислитель

    [TestCase("Aaa,Bbb", false)]
    [TestCase("Aaa,Bbb", true)]
    [TestCase("", true)]
    public void GetEnumerator(string sOrgItems, bool ignoreCase)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);

      List<string> lst = new List<string>();
      foreach (String item in sut)
        lst.Add(item);

      Assert.AreEqual(orgItems, lst.ToArray());
    }

    #endregion

    #region Прочие методы

    [Test]
    public void Clear([Values(true, false)] bool ignoreCase)
    {
      SingleScopeStringList sut = new SingleScopeStringList(new string[] { "Aaa", "Bbb" }, ignoreCase);

      sut.Clear();

      Assert.AreEqual(0, sut.Count, "Count");
      Assert.AreEqual(-1, sut.IndexOf("Aaa"), "IndexOf()");
      Assert.IsFalse(sut.Contains("Aaa"), "IndexOf()");
    }


    [TestCase("Aaa,Bbb", false, "Bbb,Aaa")]
    [TestCase("Aaa,Bbb", true, "Bbb,Aaa")]
    [TestCase("Aaa,Bbb,Ccc,Ddd,Eee", true, "Eee,Ddd,Ccc,Bbb,Aaa")]
    public void Reverse(string sOrgItems, bool ignoreCase, string sWantedArray)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      string[] wantedArray = ToStringArray(sWantedArray);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);

      sut.Reverse();
      Assert.AreEqual(wantedArray, sut.ToArray(), "ToArray()");
    }

    [TestCase("CCC,aaa,BBB", false, "BBB,CCC,aaa")]
    [TestCase("CCC,aaa,BBB", true, "aaa,BBB,CCC")]
    [TestCase("", true, "")]
    public void Sort(string sOrgItems, bool ignoreCase, string sWantedArray)
    {
      string[] orgItems = ToStringArray(sOrgItems);
      string[] wantedArray = ToStringArray(sWantedArray);

      SingleScopeStringList sut = new SingleScopeStringList(orgItems, ignoreCase);

      sut.Sort();
      Assert.AreEqual(wantedArray, sut.ToArray(), "ToArray()");
    }

    [Test]
    public void CopyTo([Values(true, false)] bool ignoreCase)
    {
      SingleScopeStringList sut = new SingleScopeStringList(new string[] { "Aaa", "Bbb" }, ignoreCase);

      string[] res1 = new string[2];
      sut.CopyTo(res1);
      Assert.AreEqual(new string[]{"Aaa","Bbb"}, res1, "#1");

      string[] res2 = new string[5];
      sut.CopyTo(res2, 2);
      Assert.AreEqual(new string[] { null, null, "Aaa", "Bbb", null }, res2, "#2");
    }


    [Test]
    public void ToArray([Values(true, false)] bool ignoreCase)
    {
      SingleScopeStringList sut = new SingleScopeStringList(new string[] { "Aaa", "Bbb" }, ignoreCase);

      string[] res = sut.ToArray();
      Assert.AreEqual(new string[] { "Aaa", "Bbb" }, res);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization([Values(true, false)] bool ignoreCase)
    {
      SingleScopeStringList sut1 = new SingleScopeStringList(new string[] { "Aaa", "Bbb" }, ignoreCase);
      sut1.Add("Ccc");
      sut1.RemoveAt(0);
      sut1.Contains("Bbb"); // в зависимости от реализации, может иметь значение вызов метода для восстановления коллекции

      byte[] bytes = SerializationTools.SerializeBinary(sut1);

      SingleScopeStringList sut2 = (SingleScopeStringList)(SerializationTools.DeserializeBinary(bytes));
      Assert.AreEqual(sut1.ToArray(), sut2.ToArray(), "ToArray()");
      Assert.AreEqual(sut1.IgnoreCase, sut2.IgnoreCase, "IgnoreCase");
    }

    #endregion

    #region Последовательности действий

    [Test]
    public void Add_and_Remove([Values(true, false)] bool ignoreCase)
    {
      SingleScopeStringList sut = new SingleScopeStringList(ignoreCase);
      sut.Add("AAA");
      sut.Add("BBB");
      sut.Remove("AAA");
      sut.Add("AAA");
      Assert.AreEqual(new string[] { "BBB", "AAA" }, sut.ToArray(), "#1");

      sut.RemoveAt(1);
      sut.Add("BBB");
      Assert.AreEqual(new string[] { "BBB"}, sut.ToArray(), "#2");

      sut.Add("CCC");
      Assert.AreEqual(new string[] { "BBB","CCC" }, sut.ToArray(), "#3");

      sut.Remove("CCC");
      Assert.AreEqual(new string[] { "BBB" }, sut.ToArray(), "#4");
    }

    #endregion

    #region Вспомогательные методы

    private static string[] ToStringArray(string s)
    {
      if (s == null)
        return null;
      if (s.Length == 0)
        return EmptyArray<string>.Empty;

      return s.Split(',');
    }

    #endregion
  }
}
