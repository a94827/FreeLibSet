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
  /// Режимы доступа (значения в порядке убывани)
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
  /// Коллекция действующих разрешений для базы данных
  /// Класс является потокобезопасным после перевода в режим "только чтение"
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
    /// Установка свойства должна выполняться в первую очередь, так как списки разрешений на таблицы очищаются
    /// </summary>
    public DBxAccessMode DBMode
    {
      get { return _DBMode; }
      set
      {
        CheckNotReadOnly();
        _DBMode = value;
        _TableDefs = null;
      }
    }
    private DBxAccessMode _DBMode;

    #endregion

    #region Таблицы

    /// <summary>
    /// Реализация свойства TableModes
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
      /// Установка свойства должна идти до установки индивидуальных рарешений на поля таблицы
      /// </summary>
      /// <param name="TableName">Имя таблицы</param>
      /// <returns>Разрешение</returns>
      public DBxAccessMode this[string TableName]
      {
        get
        {
          if (_Owner._TableDefs == null)
            return _Owner.DBMode;
          TableDef td;
          if (_Owner._TableDefs.TryGetValue(TableName, out td))
            return td.TableMode;
          else
            return _Owner.DBMode;
        }
        set
        {
          _Owner.CheckNotReadOnly();
          if (String.IsNullOrEmpty(TableName))
            throw new ArgumentNullException("TableName");

          if (_Owner._TableDefs == null)
            _Owner._TableDefs = new Dictionary<string, TableDef>();
          TableDef td;
          if (!_Owner._TableDefs.TryGetValue(TableName, out td))
          {
            td = new TableDef();
            _Owner._TableDefs.Add(TableName, td);
          }
          td.TableMode = value;
        }
      }
    }

    /// <summary>
    /// Разрешения на доступ к таблицам
    /// </summary>
    public TableList TableModes { get { return _TableModes; } }
    /// <summary>
    /// Этот список не содержит реальных данных. Данные хранятся в поле _TableDefs
    /// </summary>
    private TableList _TableModes;

    /// <summary>
    /// Метод возвращает true, если хотя бы для одной таблицы есть режим, отличающийся от DBMode
    /// </summary>
    /// <returns>Наличие разрешений на таблицы</returns>
    public bool ContainsTableModes()
    {
      if (_TableDefs == null)
        return false;
      if (_TableDefs.Count == 0)
        return false;
      foreach (TableDef td in _TableDefs.Values)
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

      if (_TableDefs == null)
        return false;
      if (_TableDefs.Count == 0)
        return false;
      foreach (TableDef td in _TableDefs.Values)
      {
        if (td.TableMode == tableMode)
          return true;
      }
      return false;
    }

    #endregion

    #region Столбцы

    /// <summary>
    /// Реализация свойства ColumnModes
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
          if (_Owner._TableDefs == null)
            return _Owner._DBMode;
          TableDef td;
          if (_Owner._TableDefs.TryGetValue(tableName, out td))
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

          if (_Owner._TableDefs == null)
            _Owner._TableDefs = new Dictionary<string, TableDef>();
          TableDef td;
          if (!_Owner._TableDefs.TryGetValue(tableName, out td))
          {
            td = new TableDef();
            td.TableMode = _Owner.DBMode;
            _Owner._TableDefs.Add(tableName, td);
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
    /// Этот список не содержит реальных данных. Данные хранятся в списке FTableDefs
    /// </summary>
    private ColumnList _ColumnModes;

    /// <summary>
    /// Метод возвращает true, если для заданной таблицы есть хотя бы одно поле, режим которого отличается
    /// от режима таблицы
    /// </summary>
    /// <returns>Наличия разрешений на столбцы</returns>
    public bool ContainsColumnModes(string tableName)
    {
      if (_TableDefs == null)
        return false;

      TableDef td;
      if (!_TableDefs.TryGetValue(tableName, out td))
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
    /// равен ColumnMode
    /// Метод возвращает true, если сама таблица находится в этом режиме
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnMode">Режим доступа к столбцу</param>
    /// <returns>Наличия таблицы или столбца</returns>
    public bool ContainsColumnModes(string tableName, DBxAccessMode columnMode)
    {
      if (TableModes[tableName] == columnMode)
        return true;

      if (_TableDefs == null)
        return false;

      TableDef td;
      if (!_TableDefs.TryGetValue(tableName, out td))
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
    private class TableDef
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
    private Dictionary<string, TableDef> _TableDefs;

    #endregion

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если список нельзя модифицировать
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерация исключения, если IsReadOnly=true
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
    /// <param name="Mode">Режим</param>
    /// <returns>Строка</returns>
    public static string GetAccessModeText(DBxAccessMode Mode)
    {
      switch (Mode)
      {
        case DBxAccessMode.Full: return "Полный доступ";
        case DBxAccessMode.ReadOnly: return "Просмотр";
        case DBxAccessMode.None: return "Запрет";
        default: return "??? " + Mode.ToString();
      }
    }

    /// <summary>
    /// Экземпляр разрешений "Полный доступ"
    /// </summary>
    public static readonly DBxPermissions FullAccess = CreateFullAccess();

    private static DBxPermissions CreateFullAccess()
    {
      DBxPermissions Res = new DBxPermissions();
      Res.SetReadOnly();
      return Res;
    }

    /// <summary>
    /// Экземпляр разрешений "Только для чтения"
    /// </summary>
    public static readonly DBxPermissions ReadOnlyAccess = CreateReadOnlyAccess();

    private static DBxPermissions CreateReadOnlyAccess()
    {
      DBxPermissions Res = new DBxPermissions();
      Res.DBMode = DBxAccessMode.ReadOnly;
      Res.SetReadOnly();
      return Res;
    }

    /// <summary>
    /// Минимальное из двух разрешений.
    /// Например, если <paramref name="mode1"/>=Full, а <paramref name="mode2"/>=ReadOnly, вовзврашается ReadOnly
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
    /// Например, если <paramref name="mode1"/>=Full, а <paramref name="mode2"/>=ReadOnly, вовзврашается Full
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
      if (this._TableDefs != null)
      {
        res._TableDefs = new Dictionary<string, TableDef>(this._TableDefs.Count);
        foreach (KeyValuePair<string, TableDef> pair in (this._TableDefs))
        {
          TableDef tdres = new TableDef();
          tdres.TableMode = pair.Value.TableMode;
          if (pair.Value.ColumnModes != null)
            tdres.ColumnModes = new Dictionary<string, DBxAccessMode>(pair.Value.ColumnModes); // создает копию
          res._TableDefs.Add(pair.Key, tdres);
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
