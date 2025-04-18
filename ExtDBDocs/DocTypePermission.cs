﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Core;

/*
 * Пользовательские разрешения в ExtDBDocs
 * 1. Определены несколько дополнительных классов, производных от UserPermission, для выдачи разрешений на документы
 * 2. Разрешения пользователя, задаваемые на стороне сервера, хранятся в объекте DBxRealDocProviderSource.
 *    Доступ к ним может быть получен также из DBxRealDocProvider
 * 3. В момент установки свойства DBxRealDocProviderSource.UserPermissions происходит инициализация списка
 *    DBxDocPermissions, который доступен через свойство DBxRealDocProviderSource.DocPermissons и
 *    DBxRealDocProvider.DocPermissions
 * 4. Есть метод DBxDocProvider.ReadUserPermissions(), который по цепочке добирается до DBxRealDocProvider и
 *    возвращает пользовательские разрешения в XML-формате в виде строки текста, чтобы преодолеть границы
 *    домена / приложения
 * 5. Для объекта DBxChainDocProvider (и производных от него) может быть установлено свойство UserPermissionCreators.
 *    Если свойство установлено, то свойства DBxChainDocProvider.UserPermissions и DocPermissions создают и
 *    возвращают собственные копии объектов разрешений. 
 *    Если свойство UserPermissionCreators не установлено, возвращаются свойства базового объекта, однако 
 *    при переходе границы возникнет исключение, т.к. UserPermission и DBxDocPermissions не являются сериализуемыми
 */

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Разрешение на просмотр/редактирование для одного или несколько видов документов.
  /// Код класса: "DocType"
  /// Если список DocTypeNames не задан, разрешение не действует.
  /// </summary>
  public class DocTypePermission : DBUserPermission
  {
    #region Creator

    /// <summary>
    /// Генератор для разрешений <see cref="DocTypePermission"/>
    /// </summary>
    public sealed class Creator : IUserPermissionCreator, IUserPermissionCreatorWithDbInitialization
    {
      #region Конструктор

      /// <summary>
      /// Создает генератор
      /// </summary>
      /// <param name="allDocTypes">Коллекция для доступа к видам документов</param>
      public Creator(DBxDocTypes allDocTypes)
      {
        if (allDocTypes == null)
          throw new ArgumentNullException("allDocTypes");
        _AllDocTypes = allDocTypes;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Коллекция для доступа к видам документов
      /// </summary>
      public DBxDocTypes AllDocTypes { get { return _AllDocTypes; } }
      private readonly DBxDocTypes _AllDocTypes;

      #endregion

      #region IUserPermissionCreator Members

      /// <summary>
      /// Возвращает "DocType"
      /// </summary>
      public string Code { get { return "DocType"; } }

      /// <summary>
      /// Создает DocTypePermission
      /// </summary>
      /// <returns>Новый объект</returns>
      public UserPermission CreateUserPermission()
      {
        return new DocTypePermission(AllDocTypes);
      }

      #endregion

      #region IUserPermissionCreatorWithDbInitialization Members 

      // 28.04.2021

      /// <summary>
      /// Устанавливает для видов документов-деревьев групп запрета на просмотр таблиц.
      /// </summary>
      /// <param name="userPermissions"></param>
      /// <param name="dbPermissions">Доступ к таблицам</param>
      /// <returns>null</returns>
      public object BeforeApplyDbPermissions(UserPermissions userPermissions, DBxPermissions dbPermissions)
      {
        // Пока не делаем.
        // Может быть выдан полный доступ к базе данных, а затем запреты на некоторые виды документов.
        // Для оставшихся видов может не быть разрешения DocTypePermission, нельзя запрещать все подряд

        //DBxDocType[] a = _AllDocTypes.GetGroupDocTypes();
        //for (int i = 0; i < a.Length; i++)
        //  dbPermissions.TableModes[a[i].Name] = DBxAccessMode.None; 

        return null; 
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      /// <param name="userPermissions"></param>
      /// <param name="dbPermissions"></param>
      /// <param name="userData"></param>
      public void AfterApplyDbPermissions(UserPermissions userPermissions, DBxPermissions dbPermissions, object userData)
      {
        // Ничего делать не надо
      }

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает разрешение класса "DocType".
    /// Свойства <see cref="DocTypeNames"/> и <see cref="DBUserPermission.Mode"/> должны быть установлены в явном виде
    /// </summary>
    /// <param name="allDocTypes">Полный список возможных видов документов</param>
    public DocTypePermission(DBxDocTypes allDocTypes)
      : base("DocType")
    {
      if (allDocTypes == null)
        throw new ArgumentNullException("allDocTypes");
      _AllDocTypes = allDocTypes;
    }

    /// <summary>
    /// Создает разрешение класса "DocType" для одного вида документов.
    /// </summary>
    /// <param name="allDocTypes">Полный список возможных видов документов</param>
    /// <param name="docTypeName">Имя вида документов</param>
    /// <param name="mode">Режим доступа</param>
    public DocTypePermission(DBxDocTypes allDocTypes, string docTypeName, DBxAccessMode mode)
      : this(allDocTypes)
    {
      if (String.IsNullOrEmpty(docTypeName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("docTypeName");
      DocTypeNames = new string[] { docTypeName };
      base.Mode = mode;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Коллекция для доступа к видам документов
    /// </summary>
    public DBxDocTypes AllDocTypes { get { return _AllDocTypes; } }
    private readonly DBxDocTypes _AllDocTypes;

    /// <summary>
    /// Список видов документов.
    /// Список не может быть пустым
    /// </summary>
    public string[] DocTypeNames
    {
      get { return _DocTypeNames; }
      set
      {
        CheckNotReadOnly();
        if (value != null)
        {
          if (value.Length < 1)
            throw new ArgumentException(Res.DocTypePermission_Arg_NoDocTypes);
          for (int i = 0; i < value.Length; i++)
          {
            if (String.IsNullOrEmpty(value[i]))
              throw ExceptionFactory.ArgInvalidEnumerableItem("value", value, value[i]);
          }
        }
        _DocTypeNames = value;
      }
    }
    private string[] _DocTypeNames;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Записывает в секцию конфигурации параметры "DocType" и "Mode"
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public override void Write(CfgPart part)
    {
      base.Write(part);
      WriteDocTypes(part, DocTypeNames);
    }

    internal static void WriteDocTypes(CfgPart part, string[] docTypeNames)
    {
      if (docTypeNames == null)
        part.SetString("DocType", String.Empty);
      else
        part.SetString("DocType", String.Join(",", docTypeNames));
    }

    /// <summary>
    /// Читает из секции конфигурации параметр "DocType" и "Mode"
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public override void Read(CfgPart part)
    {
      base.Read(part);
      DocTypeNames = ReadDocTypes(part);
    }

    internal static string[] ReadDocTypes(CfgPart part)
    {
      string s = part.GetString("DocType");
      if (String.IsNullOrEmpty(s))
        return null;
      else
        return s.Split(',');
    }


    /// <summary>
    /// Инициализация правил доступа к объектам базы данных.
    /// Устанавливает права доступа на таблицы документов в списке DocTypeNames и всех их поддокументов
    /// </summary>
    /// <param name="result">Правила доступа к объектам базы данных</param>
    public override void ApplyDbPermissions(DBxPermissions result)
    {
      if (DocTypeNames == null)
        return;
      for (int i = 0; i < DocTypeNames.Length; i++)
      {
        DBxDocType docType = AllDocTypes[DocTypeNames[i]];
        if (docType == null)
          continue;

        result.TableModes[docType.Name] = Mode;
        foreach (DBxSubDocType sdt in docType.SubDocs)
          result.TableModes[sdt.Name] = Mode;

        // 28.04.2021
        DBxDocType grpType = _AllDocTypes.GetGroupDocType(docType);
        if (grpType != null)
          result.TableModes[grpType.Name] = DBxPermissions.Max(result.TableModes[grpType.Name], Mode);
      }
    }

    /// <summary>
    /// Возвращает список видов документов
    /// </summary>
    public override string ObjectText
    {
      get
      {
        return GetDocTypeObjectText(AllDocTypes, DocTypeNames);
      }
    }

    internal static string GetDocTypeObjectText(DBxDocTypes allDocTypes, string[] docTypeNames)
    {
      if (docTypeNames == null)
        return Res.DocTypePermission_Msg_ObjectTextNoDocTypes;

      string[] titles = new string[docTypeNames.Length];
      for (int i = 0; i < docTypeNames.Length; i++)
      {
        DBxDocType docType = allDocTypes[docTypeNames[i]];
        if (docType == null)
          titles[i] = "?? " + docTypeNames[i];
        else
          titles[i] = "\"" + docType.PluralTitle + "\"";
      }
      return String.Format(Res.DocTypePermission_Msg_ObjectText, String.Join(", ", titles));
    }

    #endregion

    #region Поиск разрешений

    /// <summary>
    /// Просматривает разрешения на просмотр заданного вида документов.
    /// Возвращает действующее разрешение <see cref="DocTypePermission"/>, относящееся к заданному виду документа,
    /// или null, если разрешения нет в списке.
    /// </summary>
    /// <param name="permissions">Список пользовательских разрешений</param>
    /// <param name="docTypeName">Имя типа документов. Если задана пустая строка, то возвращается разрешение "вообще", то есть то, у которого не заданы типы документов</param>
    /// <returns>Найденное разрешение или null</returns>
    public static DocTypePermission FindPermission(UserPermissions permissions, string docTypeName)
    {
      if (permissions == null)
        throw new ArgumentNullException("permissions");

      for (int i = permissions.Count - 1; i >= 0; i--)
      {
        DocTypePermission p = permissions[i] as DocTypePermission;
        if (p == null)
          continue;
        if (p.DocTypeNames == null)
          return p;

        if (!String.IsNullOrEmpty(docTypeName))
        {
          if (DataTools.IndexOf(p.DocTypeNames, docTypeName, StringComparison.OrdinalIgnoreCase) >= 0)
            return p;
        }
      }

      return null;
    }

    /// <summary>
    /// Возвращает режим доступа для вида документов.
    /// Вызывает <see cref="FindPermission(UserPermissions, string)"/>. Если разрешение найдено, то возвращается заданное в нем значение.
    /// Иначе извлекается режим доступа для таблицы документов или для базы данных в целом. 
    /// Для этого используется метод <see cref="TablePermission.GetAccessMode(UserPermissions, string)"/>.
    /// </summary>
    /// <param name="permissions">Список пользовательских разрешений</param>
    /// <param name="docTypeName">Имя типа документов. Если задана пустая строка, то возвращается разрешение "вообще", то есть то, у которого не заданы типы документов</param>
    /// <returns>Режим доступа</returns>
    public static DBxAccessMode GetAccessMode(UserPermissions permissions, string docTypeName)
    {
      DocTypePermission p = FindPermission(permissions, docTypeName);
      if (p != null)
        return p.Mode;
      else
        return TablePermission.GetAccessMode(permissions, docTypeName);
    }

    #endregion
  }

  /// <summary>
  /// Разрешение на просмотр истории для одного или нескольких видов документов.
  /// Список может быть пустым. В этом случае выполняется полная блокировка истории
  /// Код класса: "History"
  /// </summary>
  public class DocTypeViewHistoryPermission : UserPermission
  {
    #region Creator

    /// <summary>
    /// Генератор для разрешений <see cref="DocTypeViewHistoryPermission"/>
    /// </summary>
    public sealed class Creator : IUserPermissionCreator
    {
      #region Конструктор

      /// <summary>
      /// Создает генератор
      /// </summary>
      /// <param name="allDocTypes">Коллекция описаний видов документов</param>
      public Creator(DBxDocTypes allDocTypes)
      {
        if (allDocTypes == null)
          throw new ArgumentNullException("allDocTypes");
        _AllDocTypes = allDocTypes;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Коллекция описаний видов документов
      /// </summary>
      public DBxDocTypes AllDocTypes { get { return _AllDocTypes; } }
      private readonly DBxDocTypes _AllDocTypes;

      #endregion

      #region IUserPermissionCreator Members

      /// <summary>
      /// Возвращает "History"
      /// </summary>
      public string Code { get { return "History"; } }

      /// <summary>
      /// Создает новый объект DocTypeViewHistoryPermission
      /// </summary>
      /// <returns>разрешение</returns>
      public UserPermission CreateUserPermission()
      {
        return new DocTypeViewHistoryPermission(AllDocTypes);
      }

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает разрешение.
    /// Свойства DocTypeNames и Allowed должны быть установлены дополнительно
    /// </summary>
    /// <param name="allDocTypes">Коллекция для доступа ко всем видам документов</param>
    public DocTypeViewHistoryPermission(DBxDocTypes allDocTypes)
      : base("History")
    {
      if (allDocTypes == null)
        throw new ArgumentNullException("allDocTypes");
      _AllDocTypes = allDocTypes;
      _Allowed = true;
    }


    /// <summary>
    /// Создает разрешение для одного вида документов или всех видов документов.
    /// </summary>
    /// <param name="allDocTypes">Коллекция для доступа ко всем видам документов</param>
    /// <param name="docTypeName">Имя вида документов. Пустая строка, чтобы задать разрешение для всех документов</param>
    /// <param name="allowed">True, если есть право на просмотр истории. false - запрет</param>
    public DocTypeViewHistoryPermission(DBxDocTypes allDocTypes, string docTypeName, bool allowed)
      : this(allDocTypes)
    {
      if (String.IsNullOrEmpty(docTypeName))
        this.DocTypeNames = null;
      else
        this.DocTypeNames = new string[] { docTypeName };
      this.Allowed = allowed;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Коллекция для доступа к видам документов
    /// </summary>
    public DBxDocTypes AllDocTypes { get { return _AllDocTypes; } }
    private readonly DBxDocTypes _AllDocTypes;

    /// <summary>
    /// Список видов документов.
    /// Список не может быть пустым
    /// </summary>
    public string[] DocTypeNames
    {
      get { return _DocTypeNames; }
      set
      {
        CheckNotReadOnly();
        if (value != null)
        {
          if (value.Length < 1)
            throw new ArgumentException(Res.DocTypePermission_Arg_NoDocTypes);
          for (int i = 0; i < value.Length; i++)
          {
            if (String.IsNullOrEmpty(value[i]))
              throw ExceptionFactory.ArgInvalidEnumerableItem("value", value, value[i]);
          }
        }
        _DocTypeNames = value;
      }
    }
    private string[] _DocTypeNames;


    /// <summary>
    /// true, если доступ к истории разрешен
    /// </summary>
    public bool Allowed
    {
      get { return _Allowed; }
      set
      {
        CheckNotReadOnly();
        _Allowed = value;
      }
    }
    private bool _Allowed;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Записывает в секцию конфигурацию параметры "DocType" и "Allowed"
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public override void Write(CfgPart part)
    {
      part.SetBool("Allowed", Allowed);
      DocTypePermission.WriteDocTypes(part, DocTypeNames);
    }

    /// <summary>
    /// Читает из секции конфигурацию параметры "DocType" и "Allowed"
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public override void Read(CfgPart part)
    {
      Allowed = part.GetBool("Allowed");
      DocTypeNames = DocTypePermission.ReadDocTypes(part);
    }

    /// <summary>
    /// Текстовое представление для разрешения
    /// </summary>
    public override string ObjectText
    {
      get
      {
        string s;
        if (DocTypeNames == null)
          s = Res.DocTypePermission_Msg_AllDocTypes;
        else
          s = DocTypePermission.GetDocTypeObjectText(AllDocTypes, DocTypeNames);
        return String.Format(Res.DocTypeViewHistoryPermission_Msg_ObjectText, s);
      }
    }

    /// <summary>
    /// Возвращает "Разрешен" или "Запрещен"
    /// </summary>
    public override string ValueText
    {
      get { return Allowed ? Res.Allowed_Msg_True : Res.Allowed_Msg_False; }
    }

    #endregion

    #region Поиск разрешений

    /// <summary>
    /// Просматривает разрешения на просмотр истории для заданного вида документов.
    /// Возвращает действующее разрешение <see cref="DocTypeViewHistoryPermission"/>, относящееся к заданному виду документа,
    /// или null, если разрешения нет в списке.
    /// </summary>
    /// <param name="permissions">Список пользовательских разрешений</param>
    /// <param name="docTypeName">Имя типа документов. Если задана пустая строка, то возвращается разрешение "вообще", то есть то, у которого не заданы типы документов</param>
    /// <returns>Найденное разрешение или null</returns>
    public static DocTypeViewHistoryPermission FindPermission(UserPermissions permissions, string docTypeName)
    {
      if (permissions == null)
        throw new ArgumentNullException("permissions");

      for (int i = permissions.Count - 1; i >= 0; i--)
      {
        DocTypeViewHistoryPermission p = permissions[i] as DocTypeViewHistoryPermission;
        if (p == null)
          continue;
        if (p.DocTypeNames == null)
          return p;

        if (!String.IsNullOrEmpty(docTypeName))
        {
          if (DataTools.IndexOf(p.DocTypeNames, docTypeName, StringComparison.OrdinalIgnoreCase) >= 0)
            return p;
        }
      }

      return null;
    }

    /// <summary>
    /// Просматривает разрешения на просмотр истории для заданного вида документов.
    /// Если разрешения не были установлены в явном виде, возвращается true (доступ разрешен).
    /// </summary>
    /// <param name="permissions">Список пользовательских разрешений</param>
    /// <param name="docTypeName">Имя типа документов. Если задана пустая строка, то возвращается разрешение "вообще", то есть то, у которого не заданы типы документов</param>
    /// <returns>true, если доступ разрешен</returns>
    public static bool GetAllowed(UserPermissions permissions, string docTypeName)
    {
      if (permissions == null)
        throw new ArgumentNullException("permissions");

      for (int i = permissions.Count - 1; i >= 0; i--)
      {
        DocTypeViewHistoryPermission p = permissions[i] as DocTypeViewHistoryPermission;
        if (p == null)
          continue;
        if (p.DocTypeNames == null)
          return p.Allowed;

        if (!String.IsNullOrEmpty(docTypeName))
        {
          if (DataTools.IndexOf(p.DocTypeNames, docTypeName, StringComparison.OrdinalIgnoreCase) >= 0)
            return p.Allowed;
        }
      }

      return true;
    }

    /// <summary>
    /// Просматривает разрешения на просмотр истории.
    /// Возвращает true, если разрешен просмотр истории хотя бы для одного типа документов.
    /// Если разрешения не были установлены в явном виде, возвращается true (доступ разрешен).
    /// </summary>
    /// <param name="permissions">Список пользовательских разрешений</param>
    /// <returns>true, если доступ разрешен</returns>
    public static bool GetAllowedAny(UserPermissions permissions)
    {
      if (permissions == null)
        throw new ArgumentNullException("permissions");

      for (int i = permissions.Count - 1; i >= 0; i--)
      {
        DocTypeViewHistoryPermission p = permissions[i] as DocTypeViewHistoryPermission;
        if (p == null)
          continue;
        if (p.DocTypeNames == null)
          return p.Allowed;

        if (p.Allowed)
          return true;
      }

      return true;
    }

    #endregion
  }

  #region Перечисление RecalcColumnsPermissionMode

  /// <summary>
  /// Доступные режимы для разрешения <see cref="RecalcColumnsPermission"/>
  /// </summary>
  public enum RecalcColumnsPermissionMode
  {
    /// <summary>
    /// Пересчет запрещен
    /// </summary>
    Disabled,

    /// <summary>
    /// Разрешено пересчитывать только выбранные документы или те, которые видны в просмотре
    /// </summary>
    Selected,

    /// <summary>
    /// Доступна команда "Все существующие"
    /// </summary>
    All,
  }

  #endregion

  /// <summary>
  /// Пользовательское разрешение на пересчет вычисляемых полей.
  /// Доступны три уровня: "Запрещено", "Выбранные" или "Все существующие".
  /// В табличном просмотре документов команда доступна, если у пользователя есть
  /// разрешение на редактирование таблицы документа, в документе есть вычисляемые поля 
  /// и просмотр открыт на редактирование.
  /// Если нет явно заданного разрешения, пересчет разрешен (Mode=All)
  /// </summary>
  public class RecalcColumnsPermission : UserPermission
  {
    #region Creator

    /// <summary>
    /// Генератор разрешений RecalcColumnsPermission
    /// </summary>
    public sealed class Creator : IUserPermissionCreator
    {
      #region IUserPermissionCreator Members

      /// <summary>
      /// Возвращает "RecalcColumns"
      /// </summary>
      public string Code { get { return "RecalcColumns"; } }

      /// <summary>
      /// Создает новый объект <see cref="RecalcColumnsPermission"/>
      /// </summary>
      /// <returns>Разрешение</returns>
      public UserPermission CreateUserPermission()
      {
        return new RecalcColumnsPermission();
      }

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает разрешение с режимом "All"
    /// </summary>
    public RecalcColumnsPermission()
      : this(RecalcColumnsPermissionMode.All)
    {
    }

    /// <summary>
    /// Создает разрешение с заданным режимом
    /// </summary>
    /// <param name="mode">Разрешение</param>
    public RecalcColumnsPermission(RecalcColumnsPermissionMode mode)
      : base("RecalcColumns")
    {
      _Mode = mode;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Разрешение
    /// </summary>
    public RecalcColumnsPermissionMode Mode
    {
      get { return _Mode; }
      set
      {
        CheckNotReadOnly();
        _Mode = value;
      }
    }
    private RecalcColumnsPermissionMode _Mode;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Записывает в секцию конфигурации параметр "Mode"
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public override void Write(CfgPart part)
    {
      part.SetEnum<RecalcColumnsPermissionMode>("Mode", Mode);
    }

    /// <summary>
    /// Читает из секции конфигурации параметр "Mode"
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public override void Read(CfgPart part)
    {
      Mode = part.GetEnum<RecalcColumnsPermissionMode>("Mode");
    }

    /// <summary>
    /// Возвращает "Пересчет вычисляемых полей"
    /// </summary>
    public override string ObjectText
    {
      get { return Res.RecalcColumnsPermission_Msg_ObjectText; }
    }

    /// <summary>
    /// Текстовые представления, соответствующие перечислению <see cref="RecalcColumnsPermissionMode"/>
    /// </summary>
    public static string[] ValueNames = new string[] {
      Res.RecalcColumnsPermissionMode_Msg_Disabled,
      Res.RecalcColumnsPermissionMode_Msg_Disabled,
      Res.RecalcColumnsPermissionMode_Msg_All };

    /// <summary>
    /// Возвращает текстовое значение для перечисления <see cref="RecalcColumnsPermissionMode"/> 
    /// </summary>
    /// <param name="mode">Перечисление</param>
    /// <returns>Текстовое представление</returns>
    public static string GetValueName(RecalcColumnsPermissionMode mode)
    {
      if ((int)mode < 0 || (int)mode >= ValueNames.Length)
        return "?? " + mode.ToString();
      else
        return ValueNames[(int)mode];
    }

    /// <summary>
    /// Возвращает текстовое значение для <see cref="Mode"/>
    /// </summary>
    public override string ValueText
    {
      get { return GetValueName(Mode); }
    }

    #endregion

    #region Статические методы получения разрешения

    /// <summary>
    /// Получить действующий режим разрешения
    /// </summary>
    /// <param name="userPermissions">Список пользовательских разрешений</param>
    /// <returns>Действующий режим</returns>
    public static RecalcColumnsPermissionMode GetMode(UserPermissions userPermissions)
    {
      RecalcColumnsPermission recalcColumnsPerm = userPermissions.GetLast<RecalcColumnsPermission>();
      if (recalcColumnsPerm != null)
        return recalcColumnsPerm.Mode;
      else
        return RecalcColumnsPermissionMode.All;
    }

    #endregion
  }

  /// <summary>
  /// Пользовательское разрешение на просмотр действий других пользователей.
  /// Класс разрешения: "ViewOtherUsersAction"
  /// </summary>
  public class ViewOtherUsersActionPermission : UserPermission
  {
    #region Creator

    /// <summary>
    /// Генератор разрешений <see cref="ViewOtherUsersActionPermission"/>
    /// </summary>
    public sealed class Creator : IUserPermissionCreator
    {
      #region IUserPermissionCreator Members

      /// <summary>
      /// Возвращает "ViewOtherUsersAction"
      /// </summary>
      public string Code { get { return "ViewOtherUsersAction"; } }

      /// <summary>
      /// Создает новое разрешение <see cref="ViewOtherUsersActionPermission"/>
      /// </summary>
      /// <returns>Разрешение</returns>
      public UserPermission CreateUserPermission()
      {
        return new ViewOtherUsersActionPermission(true);
      }

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает разрешение
    /// </summary>
    /// <param name="allowed">True, если доступ разрешен</param>
    public ViewOtherUsersActionPermission(bool allowed)
      : base("ViewOtherUsersAction")
    {
      _Allowed = allowed;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// True, если разрешен просмотр действий других пользователей.
    /// </summary>
    public bool Allowed
    {
      get { return _Allowed; }
      set
      {
        CheckNotReadOnly();
        _Allowed = value;
      }
    }
    private bool _Allowed;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Записывает в секцию конфигурации параметр "Allowed"
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public override void Write(CfgPart part)
    {
      part.SetBool("Allowed", Allowed);
    }

    /// <summary>
    /// Читает из секции конфигурации параметр "Allowed"
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public override void Read(CfgPart part)
    {
      Allowed = part.GetBool("Allowed");
    }

    /// <summary>
    /// Возвращает "Просмотр действий других пользователей"
    /// </summary>
    public override string ObjectText
    {
      get { return Res.ViewOtherUsersActionPermission_Msg_ObjectText; }
    }

    /// <summary>
    /// Возвращает "Разрешен" или "Запрещен"
    /// </summary>
    public override string ValueText
    {
      get { return Allowed ? Res.Allowed_Msg_True : Res.Allowed_Msg_False; }
    }

    #endregion
  }
}
