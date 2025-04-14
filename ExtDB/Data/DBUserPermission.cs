// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{

  /// <summary>
  /// Базовый класс для <see cref="WholeDBPermission"/> и <see cref="TablePermission"/>.
  /// </summary>
  public abstract class DBUserPermission : UserPermission
  {
    #region Конструктор

    /// <summary>
    /// Создает разрешение с заданным кодом.
    /// Свойство <see cref="Mode"/> получает значение Full (полный доступ).
    /// </summary>
    /// <param name="classCode">Класс разрешения</param>
    protected DBUserPermission(string classCode)
      : base(classCode)
    {
    }

    /// <summary>
    /// Создает разрешение с заданным кодом
    /// </summary>
    /// <param name="classCode">Класс разрешения</param>
    /// <param name="mode">Режим доступа</param>
    protected DBUserPermission(string classCode, DBxAccessMode mode)
      : base(classCode)
    {
      _Mode = mode;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Режим доступа к объекту базы данных
    /// </summary>
    public DBxAccessMode Mode
    {
      get { return _Mode; }
      set
      {
        CheckNotReadOnly();
        _Mode = value;
      }
    }

    private DBxAccessMode _Mode;

    /// <summary>
    /// Текстовое представление для разрешения
    /// </summary>
    public override string ValueText
    {
      get { return DBxPermissions.GetAccessModeText(Mode); }
    }

    #endregion

    #region XML

    /// <summary>
    /// Записывает в секцию конфигурации параметр "Mode"
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void Write(CfgPart cfg)
    {
      cfg.SetString("Mode", Mode.ToString());
    }

    /// <summary>
    /// Читает из секции конфигурации параметр "Mode"
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void Read(CfgPart cfg)
    {
      string s = cfg.GetString("Mode");
      switch (s)
      {
        case "":
        case "Full": Mode = DBxAccessMode.Full; break;
        case "ReadOnly": Mode = DBxAccessMode.ReadOnly; break;
        case "None": Mode = DBxAccessMode.None; break;
        default:
          throw ExceptionFactory.Inconvertible(s, typeof(DBxAccessMode));
      }
    }

#if XXX
    public static DBxAccessMode ReadMode(CfgPart Part)
    {
      string s = Part.GetString("Mode");
      switch (s)
      {
        case "":
        case "Full": return DBxAccessMode.Full;
        case "ReadOnly": return DBxAccessMode.ReadOnly;
        case "None": return DBxAccessMode.None;
        default:
          throw new InvalidOperationException("Неизвестное значение Mode=\"" + s + "\"");
      }
    }
#endif

    #endregion

    #region Статические методы для DBxAccessMode

    /// <summary>
    /// Коды, соответствующие перечислению <see cref="DBxAccessMode"/>
    /// </summary>
    public static readonly string[] ValueCodes = new string[] { "FULL", "READONLY", "NONE" };

    /// <summary>
    /// Отображаемые значения, соответствующие перечислению <see cref="DBxAccessMode"/>
    /// </summary>
    public static readonly string[] ValueNames = new string[] {
      Res.DBxUserPermission_Msg_AccessModeFull,
      Res.DBxUserPermission_Msg_AccessModeReadOnly,
      Res.DBxUserPermission_Msg_AccessModeNone };


    /// <summary>
    /// Получить текстовое представление, соответствующее коду
    /// </summary>
    /// <param name="code">Код из списка <see cref="ValueCodes"/></param>
    /// <returns>текстовое представление</returns>
    public static string GetValueName(string code)
    {
      int p = Array.IndexOf<string>(ValueCodes, code);
      if (p >= 0)
        return ValueNames[p];
      else
        return "?? \"" + code + "\"";
    }

    /// <summary>
    /// Получить текстовое представление для перечисления <see cref="DBxAccessMode"/>.
    /// </summary>
    /// <param name="mode">Режим <see cref="DBxAccessMode"/></param>
    /// <returns>Текстовое представление</returns>
    public static string GetValueName(DBxAccessMode mode)
    {
      if ((int)mode >= 0 && (int)mode <= ValueNames.Length)
        return ValueNames[(int)mode];
      else
        return "?? " + mode.ToString();
    }

    /// <summary>
    /// Получить код из списка <see cref="ValueCodes"/> для перечисления <see cref="DBxAccessMode"/>.
    /// </summary>
    /// <param name="mode">Режим <see cref="DBxAccessMode"/></param>
    /// <returns>"Код</returns>
    public static string GetValueCode(DBxAccessMode mode)
    {
      if ((int)mode >= 0 && (int)mode <= ValueCodes.Length)
        return ValueCodes[(int)mode];
      else
        throw ExceptionFactory.ArgUnknownValue("mode", mode);
    }

    /// <summary>
    /// Получить элемент перечисления <see cref="DBxAccessMode"/>, соответствующий коду из списка <see cref="ValueCodes"/>.
    /// Если задан недопустимый код, выбрасывается исключение.
    /// </summary>
    /// <param name="code">Код</param>
    /// <returns>Перечисление <see cref="DBxAccessMode"/></returns>
    public static DBxAccessMode GetAccessMode(string code)
    {
      int p = Array.IndexOf<string>(ValueCodes, code);
      if (p >= 0)
        return (DBxAccessMode)p;
      else
        throw ExceptionFactory.ArgUnknownValue("code", code);
    }

    /// <summary>
    /// Возвращает true, если указанный код есть в списке <see cref="ValueCodes"/>
    /// </summary>
    /// <param name="code">Код</param>
    /// <returns>Наличие в списке</returns>
    public static bool IsValidAccessMode(string code)
    {
      int p = Array.IndexOf<string>(ValueCodes, code);
      return p >= 0;
    }

    #endregion
  }

  /// <summary>
  /// Разрешение на доступ к базе данных в-целом.
  /// Код класса "DB".
  /// </summary>
  public class WholeDBPermission : DBUserPermission
  {
    #region Creator

    /// <summary>
    /// Реализация интерфейса <see cref="IUserPermissionCreator"/>
    /// </summary>
    public sealed class Creator : IUserPermissionCreator
    {
      #region IUserPermissionCreator Members

      /// <summary>
      /// Возвращает "DB"
      /// </summary>
      public string Code { get { return "DB"; } }

      /// <summary>
      /// Создает <see cref="WholeDBPermission"/>.
      /// </summary>
      /// <returns></returns>
      public UserPermission CreateUserPermission()
      {
        return new WholeDBPermission();
      }

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает разрешение в режиме "Full".
    /// </summary>
    public WholeDBPermission()
      : base("DB")
    {
    }

    /// <summary>
    /// Создает разрешение
    /// </summary>
    /// <param name="mode">Режим</param>
    public WholeDBPermission(DBxAccessMode mode)
      : this()
    {
      this.Mode = mode;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Устанавливает свойство <see cref="DBxPermissions.DBMode"/>.
    /// </summary>
    /// <param name="dbPermissions">Заполняемый объект разрешений базы данных</param>
    public override void ApplyDbPermissions(DBxPermissions dbPermissions)
    {
      dbPermissions.DBMode = Mode;
    }

    /// <summary>
    /// Возвращает "Доступ к базе данных"
    /// </summary>
    public override string ObjectText { get { return Res.WholeDBPermission_Name_ObjectText; } }

    #endregion
  }

  /// <summary>
  /// Разрешение на доступ к одной или нескольким таблицам
  /// Код класса "Table"
  /// </summary>
  public class TablePermission : DBUserPermission
  {
    #region Creator

    /// <summary>
    /// Реализация интерфейса <see cref="IUserPermissionCreator"/>
    /// </summary>
    public sealed class Creator : IUserPermissionCreator
    {
      #region IUserPermissionCreator Members

      /// <summary>
      /// Возвращает "Table"
      /// </summary>
      public string Code { get { return "Table"; } }

      /// <summary>
      /// Создает <see cref="TablePermission"/>
      /// </summary>
      /// <returns></returns>
      public UserPermission CreateUserPermission()
      {
        return new TablePermission();
      }

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает разрешение.
    /// Свойства <see cref="TableNames"/> и <see cref="DBUserPermission.Mode"/> должны быть установлены явно.
    /// Свойство <see cref="DBUserPermission.Mode"/> получает значение <see cref="DBxAccessMode.Full"/>.
    /// </summary>
    public TablePermission()
      :base("Table")
    { 
    }

    /// <summary>
    /// Создает разрешение для заданных таблиц
    /// </summary>
    /// <param name="tableNames">Имена таблиц в базе данных</param>
    /// <param name="mode">Режим доступа</param>
    public TablePermission(string[] tableNames, DBxAccessMode mode)
      :this()
    {
      this.TableNames = tableNames;
      this.Mode = mode;
    }

    /// <summary>
    /// Создает разрешение для одной таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы в базе данных</param>
    /// <param name="mode">Режим доступа</param>
    public TablePermission(string tableName, DBxAccessMode mode)
      : this()
    {
      this.TableNames = new string[] { tableName };
      this.Mode = mode;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имена таблиц в базе данных.
    /// </summary>
    public string[] TableNames
    {
      get { return _TableNames; }
      set
      {
        CheckNotReadOnly();
        _TableNames = value;
      }
    }
    private string[] _TableNames;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Записать разрешения в объект <see cref="DBxPermissions"/> 
    /// </summary>
    /// <param name="dbPermissions">Разрешения на доступ к объектам базы данных</param>
    public override void ApplyDbPermissions(DBxPermissions dbPermissions)
    {
      for (int i = 0; i < TableNames.Length; i++)
        dbPermissions.TableModes[TableNames[i]] = Mode;
    }

    /// <summary>
    /// текстовое представление для списка таблиц
    /// </summary>
    public override string ObjectText
    {
      get
      {
        if (TableNames == null)
          return Res.TablePermission_Name_ObjectText0;

        if (TableNames.Length == 1)
          return String.Format(Res.TablePermission_Name_ObjectText1, TableNames[0]);
        else
          return String.Format(Res.TablePermission_Name_ObjectTextN,
            UICore.UITools.ValueListToString(TableNames));
      }
    }

    /// <summary>
    /// Запись разрешения в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации для разрешения</param>
    public override void Write(CfgPart cfg)
    {
      base.Write(cfg);
      string s = String.Join(",", this.TableNames);
      cfg.SetString("Tables", s);
    }

    /// <summary>
    /// Чтение разрешения из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации для разрешения</param>
    public override void Read(CfgPart cfg)
    {
      base.Read(cfg);

      string s = cfg.GetString("Tables");
      this.TableNames = s.Split(',');
    }

    #endregion

    #region Поиск разрешения

    /// <summary>
    /// Возвращает разрешение на таблицу документов.
    /// В списке пользовательских разрешений <paramref name="permissions"/> выполняется поиск подходящего разрешения TablePermission или WholeDBPermission.
    /// Поиск выполняется от конца к началу списка.
    /// Если разрешение не найдено, возвращается <see cref="DBxAccessMode.Full"/>
    /// </summary>
    /// <param name="permissions">Пользовательские разрешения</param>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Разрешение</returns>
    public static DBxAccessMode GetAccessMode(UserPermissions permissions, string tableName)
    {
      if (permissions == null)
        throw new ArgumentNullException("permissions");
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");
      for (int i = permissions.Count - 1; i >= 0; i--)
      {
        TablePermission tp = permissions[i] as TablePermission;
        if (tp != null)
        {
          if (Array.IndexOf<string>(tp.TableNames, tableName) >= 0)
            return tp.Mode;

          continue;
        }

        WholeDBPermission dbp = permissions[i] as WholeDBPermission;
        if (dbp != null)
          return dbp.Mode;
      }

      return DBxAccessMode.Full;
    }

    #endregion
  }
}
