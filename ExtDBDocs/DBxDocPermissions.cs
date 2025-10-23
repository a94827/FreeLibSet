// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{
  #region Перечисление DBxDocPermissionReason

  /// <summary>
  /// Режим вызова метода IDBxDocPermission.TestDocAllowed()
  /// </summary>
  public enum DBxDocPermissionReason
  {
    /// <summary>
    /// Просмотр документа. Вызывается на стороне клиента перед просмотром документа
    /// В большинстве сценариев для этого запроса запрет не должен устанавливаться, т.к. пользователь имеет
    /// доступ на просмотр таблицы
    /// </summary>
    View,

    /// <summary>
    /// Создание документа. Вызывается перед записью нового документа как на стороне клиента, так и на стороне
    /// сервера при вызове DBxDocProvider.ApplyChanges().
    /// Если метод TestDocAllowed() возвращает false, запись документа невозможна. Метод вызывается только при
    /// первой записи нового документа. При повторных вызовах вызовах выполняется обращение в режиме Edit
    /// </summary>
    ApplyNew,

    /// <summary>
    /// Вызывается перед началом редактирования документа на стороне клиента при вызове метода DBxMultiDocs.Edit()
    /// или DBxSingleDocs.Edit(). Если метод TestDocAllowed() возвращает false, то генерируется исключение.
    /// При попытке редактирования документа в табличном просмотре, документ открывается на просмотр, а не на
    /// редактирование
    /// </summary>
    BeforeEdit,

    /// <summary>
    /// Вызывается перед записью редактируемого документа. Обработчик проверяет данные до внесения изменений
    /// (OriginalValues). Если требуется проверять данные в сравнении (текущие и оригинальные значения),
    /// то следует обрабатывать ApplyEditNew и там сравнивать старые и новые значения
    /// </summary>
    ApplyEditOrg,

    /// <summary>
    /// Вызывается перед записью редактируемого документа. Обработчик проверяет данные с внесенными изменениями
    /// (Values). Если требуется проверять данные в сравнении, то следует использовать свойство OriginalValues
    /// </summary>
    ApplyEditNew,

    /// <summary>
    /// Вызывается для удаленных документов перед BeforeEdit перед началом редактирования документа на стороне клиента при вызове метода DBxMultiDocs.Edit()
    /// или DBxSingleDocs.Edit(). Если метод TestDocAllowed() возвращает false, то генерируется исключение.
    /// </summary>
    BeforeRestore,

    /// <summary>
    /// Вызывается перед записью восстанавливаемого документа перед вызовом ApplyEditOrg.
    /// Как правило, требуется такая же обработка, как и для ApplyNew
    /// </summary>
    ApplyRestore,

    /// <summary>
    /// Вызывается на стороне клиента редактором документа, перед тем как запрашивать у пользователя
    /// подтверждение на удаление.
    /// На момент вызова документ еще находится в режиме просмотра и возможен доступ к значеням полей
    /// обычным способом.
    /// </summary>
    BeforeDelete,

    /// <summary>
    /// Вызывается перед удалением документа на стороне сервера.
    /// В это время строка документа уже помечена на удаление.
    /// </summary>
    ApplyDelete,

    /// <summary>
    /// Вызывается при попытке получения доступа к истории документа с помощью GetDocHistTable().
    /// Для доступа к истории необходим также доступ на просмотр документа (View)
    /// </summary>
    ViewHistory,
  }

  #endregion

  /// <summary>
  /// Аргументы, используемые при проверке прав доступа к документу или поддокументу
  /// </summary>
  public sealed class DBxDocPermissionArgs
  {
    #region Защищенный конструктор

    internal DBxDocPermissionArgs(DBxCache dbCache)
    {
      if (dbCache == null)
        throw new ArgumentNullException("dbCache");
      _ErrorMessages = new Hashtable<string, string>();
      _DBCache = dbCache;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя проверяемого документа или поддокумента
    /// </summary>
    public string TableName
    {
      get { return _TableName; }
      internal set { _TableName = value; }
    }
    private string _TableName;

    /// <summary>
    /// Причина проверки
    /// </summary>
    public DBxDocPermissionReason Reason
    {
      get { return _Reason; }
      internal set { _Reason = value; }
    }
    private DBxDocPermissionReason _Reason;

    /// <summary>
    /// Возвращает true, если текущее действие относится к просмотру документа
    /// (Reason=View или ViewHistory)
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        switch (Reason)
        {
          case DBxDocPermissionReason.View:
          case DBxDocPermissionReason.ViewHistory:
            return true;
          default:
            return false;
        }
      }
    }

    /// <summary>
    /// Значения, которые надо проверить
    /// </summary>
    public IDBxExtValues Values
    {
      get { return _Values; }
      internal set { _Values = value; }
    }
    private IDBxExtValues _Values;

    /// <summary>
    ///  Документ
    /// </summary>
    public DBxSingleDoc Doc
    {
      get { return _Doc; }
      internal set { _Doc = value; }
    }
    private DBxSingleDoc _Doc;

    /// <summary>
    /// Поддокумент
    /// </summary>
    public DBxSubDoc SubDoc
    {
      get { return _SubDoc; }
      internal set { _SubDoc = value; }
    }
    private DBxSubDoc _SubDoc;

    /// <summary>
    /// Кэш базы данных
    /// </summary>
    public DBxCache DBCache { get { return _DBCache; } }
    private readonly DBxCache _DBCache;

    /// <summary>
    /// Провайдер для извлечения произвольных значений
    /// </summary>
    public DBxDocProvider DocProvider { get { return _Doc.DocProvider; } }

    /// <summary>
    /// Сюда должно быть помещено описание причины, по которой нельзя редактировать данные.
    /// Ключом в коллекции является некоторая категория. В качестве категории рекомендуется задавать
    /// имя поля, по которому выполняется проверка. Для одного документа могут быть несколько независимых
    /// систем разрешения, каждая из которых имеет отдельную категорию. Разрешение может как устанавливать
    /// текст разрешения по категории (запрещать действие), так и сбрасывать (разрешать), что позволяет
    /// использовать последовательные разрешения
    /// </summary>
    public Hashtable<string, string> ErrorMessages { get { return _ErrorMessages; } }
    private readonly Hashtable<string, string> _ErrorMessages;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Получить полный текст сообщений об ошибках
    /// Возвращает пустую строку, если нет сообщений (действие разрешено)
    /// </summary>
    /// <returns></returns>
    public string GetErrorText()
    {
      string res = null;
      foreach (KeyValuePair<string, string> Pair in _ErrorMessages)
      {
        if (String.IsNullOrEmpty(Pair.Value))
          continue;

        if (res == null)
          res = Pair.Value;
        else
          res = res + ". " + Pair.Value;
      }

      if (res == null)
        return String.Empty;
      else
        return res;
    }

    #endregion
  }

  #region Интерфейс IDBxDocPermission

  /// <summary>
  /// Интерфейс разрешения, устанавливаемого для документов.
  /// Позволяет запретить создание или запись документа с определенными значениями полей
  /// Интерфейс может реализовываться классами, производными от UserPermission
  /// Реализация интерфейса должна быть потокобезопасной
  /// 
  /// Чтобы разрешение проверялось, необходимо дать пользователю полный доступ на документ в DBxPermissions
  /// </summary>
  public interface IDBxDocPermission
  {
    /// <summary>
    /// Список типов документов (через запятую), для которых должна выполняться проверка
    /// </summary>
    string TableNames { get;}

    /// <summary>
    /// Проверяет, разрешено ли выполнение действия для заданного документа или поддокумента
    /// Может быть задано несколько последовательных разрешений, например, первое разрешение - общий запрет,
    /// а второе - частичное разрешение
    /// </summary>
    /// <param name="args">Содержит ссылки на проверяемые данные, а также сюда должен быть помещен результат проверки</param>
    void TestDocAllowed(DBxDocPermissionArgs args);
  }

  #endregion

  /// <summary>
  /// Коллекция индивидуальных разрешений для документов
  /// Объект создается на сервере, для DBxRealDocProvider и на клиенте.
  /// При необходимости, объекты могут создаваться и в пользовательском коде для выполнения проверок
  /// Так как основной объект DocProvider не является потокобезопасным, этот объект тоже не обязан поддерживать потокобезопасность
  /// </summary>
  public sealed class DBxDocPermissions : MarshalByRefObject
  {
    #region Конструктор

    /// <summary>
    /// Создает коллекцию разрешений
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    public DBxDocPermissions(DBxDocProvider docProvider)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
#if !NET
      if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(docProvider))
        throw new ArgumentException(Res.DBxDocPermissions_Arg_DocProviderIsProxy, "docProvider");
#endif
      if (docProvider.UserPermissions == null)
        throw new NullReferenceException(Res.DBxDocPermissions_Err_DocProviderWithoutUserPermissions);

      _DocProvider = docProvider;

      _Args = new DBxDocPermissionArgs(docProvider.DBCache);

      _Items = new Dictionary<string, List<IDBxDocPermission>>();

      docProvider.UserPermissions.SetReadOnly();
      for (int i = 0; i < docProvider.UserPermissions.Count; i++)
      {
        IDBxDocPermission item = docProvider.UserPermissions[i] as IDBxDocPermission;
        if (item == null)
          continue;

        string tableNames = item.TableNames;
        if (String.IsNullOrEmpty(tableNames))
          throw ExceptionFactory.ObjectPropertyNotSet(item, "TableNames");
        if (tableNames.IndexOf(',') >= 0)
        {
          string[] a = tableNames.Split(',');
          for (int j = 0; j < a.Length; j++)
            AddItem(a[j], item);
        }
        else
          AddItem(tableNames, item);
      }
    }

    private void AddItem(string tableName, IDBxDocPermission item)
    {
      List<IDBxDocPermission> list;
      if (!_Items.TryGetValue(tableName, out list))
      {
        list = new List<IDBxDocPermission>();
        _Items.Add(tableName, list);
      }
      list.Add(item);
    }

#endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер документов, для документов которого будут проверяться документы
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private readonly DBxDocProvider _DocProvider;

    #endregion

    #region Коллекция объектов UserPermission

    private readonly Dictionary<string, List<IDBxDocPermission>> _Items;

    #endregion

    #region Вызов проверяющих методов

    /// <summary>
    /// Чтобы не создавать объект при каждом вызове
    /// </summary>
    private readonly DBxDocPermissionArgs _Args;

    /// <summary>
    /// Выполняет проверку прав доступа к документу.
    /// При отсутствии прав доступа в заданном режиме возвращается false.
    /// </summary>
    /// <param name="doc">Проверяемый документ</param>
    /// <param name="reason">Режим доступа, права для которого проверяются</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке, если прав нет</param>
    /// <returns>Наличие прав доступа</returns>
    public bool TestDocument(DBxSingleDoc doc, DBxDocPermissionReason reason, out string errorText)
    {
      List<IDBxDocPermission> list;
      if (!_Items.TryGetValue(doc.DocType.Name, out list))
      {
        errorText = null;
        return true;
      }

      _Args.TableName = doc.DocType.Name;
      _Args.Doc = doc;
      _Args.SubDoc = new DBxSubDoc(); // заглушка
      _Args.Reason = reason;

      /*
      switch (reason)
      {
        case DBxDocPermissionReason.ApplyEditOrg:
          _Args.Values = _Args.Doc.OriginalValues;
          break;
        case DBxDocPermissionReason.ApplyDelete:
          _Args.Values = _Args.Doc.OriginalValues;
          break;
        default:
          _Args.Values = _Args.Doc.Values;
          break;
      }
       * */
      // 31.01.2022 Теперь не надо использовать OriginalValues, т.к. документ перечитывается из базы данных
      _Args.Values = _Args.Doc.Values;


#if DEBUG
      if (_Args.Values == null)
        throw new BugException("Args.Values=null");
      Int32 dummyDocId = _Args.Values["Id"].AsInt32; // пытаемся прочитать
#endif

      _Args.ErrorMessages.Clear();
      // Всегда проверяем все разрешения, а не до первого запрета, т.к. последнее разрешение может разрешить
      // то, что было запрещено до этого
      for (int i = 0; i < list.Count; i++)
        list[i].TestDocAllowed(_Args);

      errorText = _Args.GetErrorText();
      return String.IsNullOrEmpty(errorText);
    }

    /// <summary>
    /// Выполняет проверку прав доступа к документу.
    /// При отсутствии прав доступа в заданном режиме генерируется исключение DBxAccessException.
    /// </summary>
    /// <param name="doc">Проверяемый документ</param>
    /// <param name="reason">Режим доступа, права для которого проверяются</param>
    [DebuggerStepThrough]
    public void TestDocument(DBxSingleDoc doc, DBxDocPermissionReason reason)
    {
      string errorText;
      if (!TestDocument(doc, reason, out errorText))
        // throw new DBxAccessException("Нет права " + GetActionName(Reason) + " документа \"" + Doc.DocType.SingularTitle + "\" с Id="+Doc.DocId.ToString()+". " + ErrorText);
        throw new DBxAccessException(String.Format(Res.DBxDocPermissions_Err_AccessDenied,
          GetActionName(reason), doc.DocType.SingularTitle, doc.TextValue, errorText));
    }

    private static string GetActionName(DBxDocPermissionReason reason)
    {
      switch (reason)
      {
        case DBxDocPermissionReason.View: return Res.DBxDocPermissions_Err_AccessDeniedView;
        case DBxDocPermissionReason.ViewHistory: return Res.DBxDocPermissions_Err_AccessDeniedViewHistory;
        case DBxDocPermissionReason.ApplyNew: return Res.DBxDocPermissions_Err_AccessDeniedNew;
        case DBxDocPermissionReason.BeforeDelete: 
        case DBxDocPermissionReason.ApplyDelete: return Res.DBxDocPermissions_Err_AccessDeniedDelete;
        case DBxDocPermissionReason.BeforeRestore: 
        case DBxDocPermissionReason.ApplyRestore: return Res.DBxDocPermissions_Err_AccessDeniedRestore;
        default: return Res.DBxDocPermissions_Err_AccessDeniedEdit;
      }
    }

    // TODO: Проверка для поддокументов

    /// <summary>
    /// Проверка всех документов в наборе.
    /// Проверка прекращается, если нет доступа хотя бы к одному из документов
    /// </summary>
    /// <param name="docSet">Набор документов</param>
    /// <param name="reason">Действие</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке при отсутствии прав доступа</param>
    /// <returns>Наличие прав доступа</returns>
    public bool TestDocuments(DBxDocSet docSet, DBxDocPermissionReason reason, out string errorText)
    {
      foreach (DBxMultiDocs multiDocs in docSet)
      {
        foreach (DBxSingleDoc doc in multiDocs)
        {
          if (!TestDocument(doc, reason, out errorText))
            return false;
        }
      }

      errorText = null;
      return true;
    }

    /// <summary>
    /// Проверка всех документов в наборе.
    /// Проверка прекращается, если нет доступа хотя бы к одному из документов.
    /// При этом выбрасывается исключение.
    /// </summary>
    /// <param name="docSet">Набор документов</param>
    /// <param name="reason">Действие</param>
    [DebuggerStepThrough]
    public void TestDocuments(DBxDocSet docSet, DBxDocPermissionReason reason)
    {
      string errorText;
      if (!TestDocuments(docSet, reason, out errorText))
        // throw new DBxAccessException("Нет права " + GetActionName(Reason) + " документа \"" + Doc.DocType.SingularTitle + "\" с Id="+Doc.DocId.ToString()+". " + ErrorText);
        throw new DBxAccessException(String.Format(Res.DBxDocPermissions_Err_DocSetAccessDenied, 
          GetActionName(reason), errorText));
    }

    internal static DBxAccessException GetException(DBxDocType docType, DBxDocPermissionReason reason)
    {
      return new DBxAccessException(String.Format(Res.DBxDocPermissions_Err_DocTypeAccessDenied,
        GetActionName(reason), docType.PluralTitle));
    }

    #endregion
  }
}
