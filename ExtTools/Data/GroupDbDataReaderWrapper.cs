// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Обертка над <see cref="DbDataReader"/>, которая группирует строки по повторяющимся значениям полей.
  /// За один такт чтения загружаются строки с одинаковыми значениями полей, пока не будет достигнута строка с другими значениями.
  /// Полученные строки собираются в <see cref="DataTable"/> и обрабатываются прикладным кодом.
  /// В процессе работы не следует вызывать <see cref="DbDataReader.Read()"/> из прикладного кода.
  /// <para>
  /// Порядок работы.
  /// </para>
  /// <para>
  /// 1. Получить <see cref="DbDataReader"/>, выполнив запрос. Запрос должен использовать сортировку "ORDER BY", чтобы группируемые поля шли по порядку.
  /// В отличие от <see cref="DataTools.GroupRows(DataTable, string, bool)"/>, нет возможности сначала собрать все ключи, а потом выполнять группировку,
  /// так как <see cref="DbDataReader"/> является "одноразовым".
  /// </para>
  /// <para>
  /// 2. Создать <see cref="GroupDbDataReaderWrapper"/>
  /// </para>
  /// <para>
  /// 3. Вызывать в цикле метод <see cref="GroupDbDataReaderWrapper.Read()"/>
  /// </para>
  /// <para>
  /// 4. На каждом такте использовать свойство <see cref="GroupDbDataReaderWrapper.Table"/> для доступа к очередной группе строк
  /// </para>
  /// </summary>
  public sealed class GroupDbDataReaderWrapper
  {
    #region Конструктор

    /// <summary>
    /// Инициализация обертки
    /// </summary>
    /// <param name="reader">Готовый к употреблению объект для чтения результатов запроса</param>
    /// <param name="keyColumnNames">Имена полей, разделенные запятыми, по которым выполняется группировка.
    /// Должно быть задано, как минимум, одно поле</param>
    public GroupDbDataReaderWrapper(DbDataReader reader, string keyColumnNames)
    {
#if DEBUG
      if (reader == null)
        throw new ArgumentNullException("reader");
#endif
      _Reader = reader;

      if (String.IsNullOrEmpty(keyColumnNames))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("keyColumnNames");

      string[] a = keyColumnNames.Split(',');
      _KeyColumnIndexes = new int[a.Length];
      for (int i = 0; i < a.Length; i++)
      {
        _KeyColumnIndexes[i] = reader.GetOrdinal(a[i]);
        if (_KeyColumnIndexes[i] < 0)
          throw ExceptionFactory.ArgUnknownColumnName("keyColumnNames", reader, a[i]);
      }
      _CurrentKeys = new object[a.Length];

      _Table = new DataTable();
      for (int i = 0; i < reader.FieldCount; i++)
        _Table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));

      _Phase = Phase.Init;
      _DBNullAsZero = false;
    }

    #endregion

    #region Свойства

    private readonly DbDataReader _Reader;

    private readonly int[] _KeyColumnIndexes;

    /// <summary>
    /// Таблица со строками с одинаковыми значениями ключевых полей.
    /// Таблица очищается и заполняется строками при каждом вызове <see cref="Read()"/>.
    /// Используется единственный экземпляр таблицы. Если требуется сохранять таблицу между тактами, используйте <see cref="DataTable.Copy()"/>.
    /// Создается в конструкторе.
    /// В прикладном коде можно использовать <see cref="DataTableValues"/> для доступа к значениям полей.
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private readonly DataTable _Table;

    /// <summary>
    /// Если true, то значения <see cref="DBNull"/> в полях группировки будут трактоваться как нулевые значения соответствующего типа.
    /// Если false (по умолчанию), то нулевые значения и <see cref="DBNull"/> различаются.
    /// Свойство используется только в процессе группировки строк и не влияет на значения полей в <see cref="Table"/>. В таблице
    /// всегда содержаться значения как они получены из <see cref="DbDataReader"/>.
    /// </summary>
    public bool DBNullAsZero
    {
      get { return _DBNullAsZero; }
      set
      {
        if (_Phase != Phase.Init)
          throw ExceptionFactory.MethodAlreadyCalled(this, "Read()");
        _DBNullAsZero = value;
      }
    }
    private bool _DBNullAsZero;


    #endregion

    #region Read()

    /*
     * Обертка находится в одном из следующих состояний:
     * 1. Начало. Метод DbDataReader.Read() еще не вызывался.
     * 2. После чтения очередной группы строк. В Table имеется, как минимум, одна строка. DbDataReader.Read() вернул true, но
     *    текущая запись в нем содержит отличающиеся значения, поэтому она не попала в Table. На следуюшем такте эта запись попадет в Table.
     * 3. Чтение закончено. DbDataReader.Read() вернул false, но GroupDbDataReaderWrapper.Read() вернул true, так как в таблице были строки
     * 4. Завершение. GroupDbDataReaderWrapper.Read() вернул false, так как больше строк нет
     */

    private enum Phase { Init, Progress, Tail, End }
    private Phase _Phase;

    /// <summary>
    /// Чтение группы строк.
    /// Вызывает один или несколько раз метод <see cref="DbDataReader.Read()"/> пока не будут прочитаны все строки с одинаковыми значениями полей группировки.
    /// </summary>
    /// <returns>True, если есть очередная группа строк.
    /// False, если нет больше строк</returns>
    public bool Read()
    {
      _Table.Rows.Clear();
      switch (_Phase)
      {
        case Phase.End:
          return false;
        case Phase.Tail:
          return false;
        case Phase.Progress:
          SaveKeys();
          AddRow();
          break;
      }

      while (_Reader.Read())
      {
        if (_Table.Rows.Count==0)
          SaveKeys();
        if (AreKeysEqual())
          AddRow();
        else
        {
#if DEBUG
          if (_Table.Rows.Count == 0)
            throw new BugException("No rows in the group");
#endif
          _Phase = Phase.Progress;
          _Table.AcceptChanges();
          return true;
        }
      }

      if (Table.Rows.Count == 0)
      {
        _Phase = Phase.End;
        return false;
      }
      else
      {
        _Phase = Phase.Tail;
        _Table.AcceptChanges();
        return true;
      }
    }


    private void AddRow()
    {
      DataRow row = _Table.NewRow();
      for (int i = 0; i < _Table.Columns.Count; i++)
      {
        if (!_Reader.IsDBNull(i))
          row[i] = _Reader.GetValue(i);
      }
      _Table.Rows.Add(row);
    }

    #endregion

    #region Значения ключей для текущей строки

    private readonly object[] _CurrentKeys;

    private void SaveKeys()
    {
      for (int i = 0; i < _KeyColumnIndexes.Length; i++)
        _CurrentKeys[i] = GetCurrenkKey(i);
    }

    private bool AreKeysEqual()
    {
      for (int i = 0; i < _KeyColumnIndexes.Length; i++)
      {
        if (!Object.Equals(_CurrentKeys[i], GetCurrenkKey(i)))
          return false;
      }
      return true;
    }

    private object GetCurrenkKey(int keyIndex)
    {
      if (_Reader.IsDBNull(_KeyColumnIndexes[keyIndex]))
      {
        if (_DBNullAsZero)
          return DataTools.GetEmptyValue(_Reader.GetFieldType(_KeyColumnIndexes[keyIndex]));
        else
          return DBNull.Value;
      }
      else
        return _Reader.GetValue(_KeyColumnIndexes[keyIndex]);
    }

    #endregion
  }
}
