using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using NUnit.Framework;

namespace ExtTools_tests.IO
{
  // TODO: Сделан только метод GetVersionFromStr()

  [TestFixture]
  public class FileToolsTests
  {
    [TestCase("", "")] // пустая строка
    [TestCase("1", "1.0")] // не хватает одного разряда
    [TestCase("1.2", "1.2")]
    [TestCase("1.2.3", "1.2.3")]
    [TestCase("1.2.3.4", "1.2.3.4")]
    [TestCase("1.2.3.4.5", "1.2.3.4")] // лишний разряд
    [TestCase("1a.2b.3c.4d", "1.2.3.4")] // лишние символы
    [TestCase("1.abc.3.4", "1.0.3.4")] // буквенная часть
    [TestCase("111111111111", "")] // слишком длинное число
    [TestCase("1.222222222222", "1.0")]
    [TestCase("1.2.333333333333", "1.2.0")]
    [TestCase("1.2.3.444444444444", "1.2.3.0")]
    [TestCase("1.-2", "1.0")] // отрицательное число
    public void GetVersionFromStr(string s, string sWantedRes)
    {
      Version res = FileTools.GetVersionFromStr(s);
      string sRes;
      if (res == null)
        sRes = "";
      else                                                                              
        sRes = res.ToString();

      Assert.AreEqual(sWantedRes, sRes);
    }
  }
}
