// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Переходник между <see cref="DBxStruct"/> и <see cref="DBx"/> для извлечения описаний таблиц
  /// </summary>
  public sealed class DBxRealStructSource : MarshalByRefObject, IDBxStructSource
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник
    /// </summary>
    /// <param name="entry">Точка подключения. Не может быть null</param>
    public DBxRealStructSource(DBxEntry entry)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");

      _Entry = entry;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Точка подключения. 
    /// При вызове методов для нее создается соединение с базой данных.
    /// Методы соединения используются для извлечения информации.
    /// Задается в конструкторе.
    /// </summary>
    public DBxEntry Entry { get { return _Entry; } }
    private readonly DBxEntry _Entry;

    bool IDBxStructSource.IsRealStruct { get { return true; } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Получить полный список имен таблиц в виде массива (без структуры)
    /// </summary>
    /// <returns>Массив имен таблиц</returns>
    public string[] GetAllTableNames()
    {
      string[] a;
      using (DBxConBase con = Entry.CreateCon())
      {
        a = con.GetAllTableNamesFromSchema();
      }
      return a;
    }

    /// <summary>
    /// Получить описание структуры одной таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненное описание структуры таблицы</returns>
    public DBxTableStruct GetTableStruct(string tableName)
    {
      DBxTableStruct obj;

      using (DBxConBase con = Entry.CreateCon())
      {
        obj = con.GetRealTableStructFromSchema(tableName);
      }

      return obj;
    }

    #endregion
  }
}
