using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using NUnit.Framework;

namespace ExtDB.tests
{
  /// <summary>
  /// Вспомогательные методы для тестирования баз данных
  /// </summary>
  public static class DBxTestHelpers
  {
    /// <summary>
    /// Сравнение требуемой структуры с реальной
    /// </summary>
    /// <param name="wanted">Объявленная структура</param>
    /// <param name="real">Реальная структура</param>
    /// <param name="prefixText">Текст выводимый перед всеми сообщениями</param>
    public static void ValidateStruct(DBxStruct wanted, DBxStruct real, string prefixText)
    {
      Assert.IsNotNull(wanted, prefixText + "Wanted DBxStruct");
      Assert.IsNotNull(real, prefixText + "Real DBxStruct");
      Assert.AreNotSame(wanted, real, prefixText + "DBxStruct are same");

      foreach (DBxTableStruct ts1 in wanted.Tables)
      {
        DBxTableStruct ts2 = real.Tables[ts1.TableName];
        if (ts2 == null)
        {
          string txt = prefixText + "There is no table <" + ts1.TableName +
            ">. Available tables (" + real.AllTableNames.Length.ToString() + "): " + String.Join(", ", real.AllTableNames);
          Assert.Fail(txt);
        }

        foreach (DBxColumnStruct cs1 in ts1.Columns)
        {
          DBxColumnStruct cs2 = ts2.Columns[cs1.ColumnName];
          if (cs2 == null)
          {
            string txt = prefixText + "There is no column <" + cs1.ColumnName + "> in table <" + ts2.TableName +
              ">. Available columns (" + ts2.Columns.Count.ToString() + "): " + String.Join(", ", ts2.Columns.GetCodes());
            Assert.Fail(txt);
          }
        }
      }
    }

  }
}
