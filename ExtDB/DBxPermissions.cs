// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  #region Перечисление DBxAccessMode

  /// <summary>
  /// Режимы доступа (значения в порядке убывания разрешенных действий)
  /// </summary>
  [Serializable]
  public enum DBxAccessMode
  {
    /// <summary>
    /// Полный доступ
    /// </summary>
    Full = 0,

    /// <summary>
    /// Доступ только на просмотр данных
    /// </summary>
    ReadOnly = 1,

    /// <summary>
    /// Доступ запрещен
    /// </summary>
    None = 2
  }

  #endregion

  /// <summary>
  /// Коллекция действующих разрешений для базы данных.
  /// Класс является потокобезопасным после перевода в режим "только чтение".
  /// </summary>
  [Serializable]
  public sealed class DBxPermissions : IReadOnlyObject, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор создает набор разрешений, разрешающих любые действия
    /// </summary>
    public DBxPermissions()
    {
      _TableModes = new TableList(this);
      _ColumnModes = new ColumnList(this);

      _DBMode = DBxAccessMode.Full;
    }

    #endregion

    #region Ограничения

    #region База данных в-целом

    /// <summary>
    /// Режим доступа к базе данных в-целом.
    /// Установка свойства должна выполняться в первую очередь, так как списки разрешений на таблицы очищаются.
    /// Значение по умолчанию - Full.
    /// </summary>
    public DBxAccessMode DBMode
    {
      get { return _DBMode; }
      set
      {
        CheckNotReadOnly();
        _DBMode = value;
        _TableData = null;
      }
    }
    private DBxAccessMode _DBMode;

    #endregion

    #region Таблицы

    /// <summary>
    /// Реализация свойства <see cref="TableModes"/>
    /// </summary>
    [Serializable]
    public class TableList
    {
      internal TableList(DBxPermissions Owner)
      {
        _Owner = Owner;
      }

      private DBxPermissions _Owner;

      /// <summary>
      /// Разрешение на доступ к таблице.
      /// Если свойство не установлено в явном виде, возвращает общее разрешение для базы данных.
      /// Установка свойства должна идти до установки индивидуальных рарешений на поля таблицы.
      /// </summary>
      /// <param name="tableName">Имя таблицы</param>
      /// <returns>Разрешение</returns>
      public DBxAccessMode this[string tableName]
      {
        get
        {
          if (_Owner._TableData == null)
            return _Owner.DBMode;
          InternalTableData td;
          if (_Owner._TableData.TryGetValue(tableName, out td))
            return td.TableMode;
          else
            return _Owner.DBMode;
        }
        set
        {
          _Owner.CheckNotReadOnly();
          if (String.IsNullOrEmpty(tableName))
            throw new ArgumentNullException("TableName");

          if (_Owner._TableData == null)
            _Owner._TableData = new Dictionary<string, InternalTableData>();
          InternalTableData td;
          if (!_Owner._TableData.TryGetValue(tableName, out td))
          {
            td = new InternalTableData();
            _Owner._TableData.Add(tableName, td);
          }
          td.TableMode = value;
          td.ColumnModes = null; // 13.06.2023
        }
      }
    }

    // Нельзя реализовать свойства TableModes и ColumnModes как одноразовые структуры,
    // так как будет ошибка компиляции в прикладном коде при вызовах:
    // DBxPermissions p=new DBxPermissions();
    // p.TableModes["T1"]=DBxAccessMode.ReadOnly; // <- присвоение значения

    /// <summary>
    /// Разрешения на доступ к таблицам
    /// </summary>
    public TableList TableModes { get { return _TableModes; } }
    /// <summary>
    /// Этот список не содержит реальных данных. Данные хранятся в поле _TableDefs.
    /// </summary>
    private readonly TableList _TableModes;

    /// <summary>
    /// Метод возвращает true, если хотя бы для одной таблицы есть режим, отличающийся от <see cref="DBMode"/>.
    /// </summary>
    /// <returns>Наличие разрешений на таблицы</returns>
    public bool ContainsTableModes()
    {
      if (_TableData == null)
        return false;
      if (_TableData.Count == 0)
        return false;
      foreach (InternalTableData td in _TableData.Values)
      {
        if (td.TableMode != _DBMode)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Метод возвращает true, если есть хотя бы одна таблица, для которой задан режим <paramref name="tableMode"/>
    /// Возвращает true, если база данных в-целом находится в этом режиме
    /// </summary>
    /// <param name="tableMode">Запрашиваемый режим</param>
    /// <returns>Наличие таблицы</returns>
    public bool ContainsTableModes(DBxAccessMode tableMode)
    {
      if (_DBMode == tableMode)
        return true;

      if (_TableData == null)
        return false;
      if (_TableData.Count == 0)
        return false;
      foreach (InternalTableData td in _TableData.Values)
      {
        if (td.TableMode == tableMode)
          return true;
      }
      return false;
    }

    #endregion

    #region Столбцы

    /// <summary>
    /// Реализация свойства <see cref="DBxPermissions.ColumnModes"/>
    /// </summary>
    [Serializable]
    public class ColumnList
    {
      internal ColumnList(DBxPermissions owner)
      {
        _Owner = owner;
      }

      private DBxPermissions _Owner;

      /// <summary>
      /// Разрешение на доступ к полю таблицы.
      /// Если разрешение на отдельный столбец не установлено, возвращается разрешение на таблицу.
      /// </summary>
      /// <param name="tableName">Таблица</param>
      /// <param name="columnName">Столбец</param>
      /// <returns>Разрешение</returns>
      public DBxAccessMode this[string tableName, string columnName]
      {
        get
        {
          if (_Owner._TableData == null)
            return _Owner._DBMode;
          InternalTableData td;
          if (_Owner._TableData.TryGetValue(tableName, out td))
          {
            if (td.ColumnModes == null)
              return td.TableMode;
            DBxAccessMode ColumnMode;
            if (td.ColumnModes.TryGetValue(columnName, out ColumnMode))
              return ColumnMode;
            else
              return td.TableMode;
          }
          else
            return _Owner._DBMode;
        }
        set
        {
          _Owner.CheckNotReadOnly();
          if (String.IsNullOrEmpty(tableName))
            throw new ArgumentNullException("tableName");
          if (String.IsNullOrEmpty(columnName))
            throw new ArgumentNullException("columnName");

          if (_Owner._TableData == null)
            _Owner._TableData = new Dictionary<string, InternalTableData>();
          InternalTableData td;
          if (!_Owner._TableData.TryGetValue(tableName, out td))
          {
            td = new InternalTableData();
            td.TableMode = _Owner.DBMode;
            _Owner._TableData.Add(tableName, td);
          }

          if (td.ColumnModes == null)
            td.ColumnModes = new Dictionary<string, DBxAccessMode>();

          if (td.ColumnModes.ContainsKey(columnName))
            td.ColumnModes[columnName] = value;
          else
            td.ColumnModes.Add(columnName, value);
        }
      }
    }

    /// <summary>
    /// Индивидуальные разрешения на столбцы таблиц
    /// </summary>
    public ColumnList ColumnModes { get { return _ColumnModes; } }
    /// <summary>
    /// Этот список не содержит реальных данных. Данные хранятся в списке _TableDefs
    /// </summary>
    private readonly ColumnList _ColumnModes;

    /// <summary>
    /// Метод возвращает true, если для заданной таблицы есть хотя бы одно поле, режим которого отличается
    /// от режима таблицы
    /// </summary>
    /// <returns>Наличия разрешений на столбцы</returns>
    public bool ContainsColumnModes(string tableName)
    {
      if (_TableData == null)
        return false;

      InternalTableData td;
      if (!_TableData.TryGetValue(tableName, out td))
        return false;

      if (td.ColumnModes == null)
        return false;
      foreach (DBxAccessMode acc in td.ColumnModes.Values)
      {
        if (acc != td.TableMode)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Метод возвращает true, если для заданной таблицы есть хотя бы одно поле, режим которого 
    /// равен <paramref name="columnMode"/>.
    /// Метод возвращает true, если сама таблица находится в этом режиме.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnMode">Режим доступа к столбцу</param>
    /// <returns>Наличия таблицы или столбца</returns>
    public bool ContainsColumnModes(string tableName, DBxAccessMode columnMode)
    {
      if (TableModes[tableName] == columnMode)
        return true;

      if (_TableData == null)
        return false;

      InternalTableData td;
      if (!_TableData.TryGetValue(tableName, out td))
        return false;

      if (td.ColumnModes == null)
        return false;

      foreach (DBxAccessMode acc in td.ColumnModes.Values)
      {
        if (acc == columnMode)
          return true;
      }

      return false;
    }

    #endregion

    #region Внутренние данные

    [Serializable]
    private class InternalTableData
    {
      #region Поля

      /// <summary>
      /// Разрешение для таблицы
      /// </summary>
      public DBxAccessMode TableMode;

      /// <summary>
      /// Список разрешений для полей.
      /// Обычно содержит null, если нет отдельных разрешений на поля таблицы
      /// </summary>
      public Dictionary<string, DBxAccessMode> ColumnModes;

      #endregion

      #region ToString()

      /// <summary>
      /// Текстовое представление (для отладки)
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        if (ColumnModes == null)
          return TableMode.ToString();
        else
        {
          StringBuilder sb = new StringBuilder();
          sb.Append(TableMode.ToString());
          sb.Append(" (");
          int cnt = 0;
          foreach (KeyValuePair<string, DBxAccessMode> Pair in ColumnModes)
          {
            if (cnt > 0)
              sb.Append(", ");
            sb.Append(Pair.Key);
            sb.Append('=');
            sb.Append(Pair.Value.ToString());
            cnt++;
          }
          sb.Append(")");

          return sb.ToString();
        }
      }

      #endregion
    }

    /// <summary>
    /// Здесь хранятся реальные данные разрешений
    /// </summary>
    private Dictionary<string, InternalTableData> _TableData;

    #endregion

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если список нельзя модифицировать
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерация исключения, если <see cref="IsReadOnly"/>=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Блокировка списка от внесения изменений
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region Тестовое представление

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return "Режим=" + GetAccessModeText(DBMode) + (ContainsTableModes() ? ", разрешения на таблицы" : "");
    }

    #endregion

    #region Статические методы и свойства

    /// <summary>
    /// Текстовое представление для режима доступа
    /// </summary>
    /// <param name="mode">Режим</param>
    /// <returns>Строка</returns>
    public static string GetAccessModeText(DBxAccessMode mode)
    {
      switch (mode)
      {
        case DBxAccessMode.Full: return "Полный доступ";
        case DBxAccessMode.ReadOnly: return "Просмотр";
        case DBxAccessMode.None: return "Запрет";
        default: return "??? " + mode.ToString();
      }
    }

    /// <summary>
    /// Экземпляр разрешений "Полный доступ"
    /// </summary>
    public static readonly DBxPermissions FullAccess = CreateFullAccess();

    private static DBxPermissions CreateFullAccess()
    {
      DBxPermissions res = new DBxPermissions();
      res.SetReadOnly();
      return res;
    }

    /// <summary>
    /// Экземпляр разрешений "Только для чтения"
    /// </summary>
    public static readonly DBxPermissions ReadOnlyAccess = CreateReadOnlyAccess();

    private static DBxPermissions CreateReadOnlyAccess()
    {
      DBxPermissions res = new DBxPermissions();
      res.DBMode = DBxAccessMode.ReadOnly;
      res.SetReadOnly();
      return res;
    }

    /// <summary>
    /// Минимальное из двух разрешений.
    /// Например, если <paramref name="mode1"/>=Full, а <paramref name="mode2"/>=ReadOnly, возврашается ReadOnly
    /// </summary>
    /// <param name="mode1">Первое разрешение</param>
    /// <param name="mode2">Второе разрешение</param>
    /// <returns>Общее разрешение</returns>
    public static DBxAccessMode Min(DBxAccessMode mode1, DBxAccessMode mode2)
    {
      // числовое значение - наоборот
      return (int)mode1 > (int)mode2 ? mode1 : mode2;
    }

    /// <summary>
    /// Максимальное из двух разрешений.
    /// Например, если <paramref name="mode1"/>=Full, а <paramref name="mode2"/>=ReadOnly, возврашается Full
    /// </summary>
    /// <param name="mode1">Первое разрешение</param>
    /// <param name="mode2">Второе разрешение</param>
    /// <returns>Общее разрешение</returns>
    public static DBxAccessMode Max(DBxAccessMode mode1, DBxAccessMode mode2)
    {
      // числовое значение - наоборот
      return (int)mode1 < (int)mode2 ? mode1 : mode2;
    }

    /// <summary>
    /// Сравнение двух разрешений.
    /// Возвращает (+1), если первое разрешение больше второго.
    /// Возвращает (-1), если первое разрешение меньше второго.
    /// Возвращает (0), если разрешения совпадают.
    /// Например, если <paramref name="mode1"/>=View, а <paramref name="mode2"/>=Full, возвращается (-1)
    /// </summary>
    /// <param name="mode1">Первое разрешение</param>
    /// <param name="mode2">Второе разрешение</param>
    /// <returns>Результат сравнения</returns>
    public static int Compare(DBxAccessMode mode1, DBxAccessMode mode2)
    {
      // числовое значение - наоборот
      return Math.Sign((int)mode2 - (int)mode1);
    }

#if XXX
    /// <summary>
    /// Создание копии объекта
    /// Используется вместо сериализации
    /// </summary>
    /// <param name="Src"></param>
    /// <returns></returns>
    public static DBxPermissions SimpleClone(DBxPermissions Src)
    {
      return new DBxPermissions(Src);
    }
#endif

    #endregion

    #region Клонирование разрешений

    /// <summary>
    /// Создает копию текущего набора разрешений, которую можно менять
    /// </summary>
    /// <returns></returns>
    public DBxPermissions Clone()
    {
      DBxPermissions res = new DBxPermissions();
      res._DBMode = this._DBMode;
      if (this._TableData != null)
      {
        res._TableData = new Dictionary<string, InternalTableData>(this._TableData.Count);
        foreach (KeyValuePair<string, InternalTableData> pair in (this._TableData))
        {
          InternalTableData tdres = new InternalTableData();
          tdres.TableMode = pair.Value.TableMode;
          if (pair.Value.ColumnModes != null)
            tdres.ColumnModes = new Dictionary<string, DBxAccessMode>(pair.Value.ColumnModes); // создает копию
          res._TableData.Add(pair.Key, tdres);
        }
      }
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  /// <summary>
  /// Исключение, когда у пользователя нет прав для совершения действия
  /// </summary>
  [Serializable]
  public class DBxAccessException : ApplicationException
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект исключения с текстом "У Вас нет прав для выполнения действия"
    /// </summary>
    public DBxAccessException()
      : base("У Вас нет прав для выполнения действия")
    {
    }

    /// <summary>
    /// Создает объект исключения с заданным текстом
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public DBxAccessException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает объект исключения с заданным текстом.
    /// Используется для перевыброса исключения в блоке catch
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="innerException">Вложенное исключение</param>
    public DBxAccessException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Требуется для десериализации исключения
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected DBxAccessException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }
}
