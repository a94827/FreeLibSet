// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Threading;
using FreeLibSet.Logging;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{
  #region Перечисление UndoAction

  /// <summary>
  /// Возможные значения поля "Action" таблицы "DocActions" в undo.mdb
  /// </summary>
  public enum UndoAction
  {
    /// <summary>
    /// Исходное состояние документа.
    /// Используется, когда история редактирования начинает вестись не с начала. Или файл истории был удален
    /// </summary>
    Base = 0,

    /// <summary>
    /// Создание документа
    /// </summary>
    Insert = 1,

    /// <summary>
    /// Изменение документа
    /// </summary>
    Edit = 2,

    /// <summary>
    /// Удаление документа
    /// </summary>
    Delete = 3,

    /// <summary>
    /// Отмена редактирования
    /// </summary>
    Undo = 4,

    /// <summary>
    /// Накат обновления
    /// </summary>
    Redo = 5,
  }

  #endregion

  /// <summary>
  /// Вспомогательный класс, используемый для обновления БД undo.mdb при пользовательском редактировании
  /// </summary>
  internal class DocUndoHelper
  {
    #region Конструктор

    public DocUndoHelper(DBxCon mainConGlobal, DBxCon undoCon, Int32 userId, Int32 sessionId, Int32 userActionId, string actionInfo, DBxDocTypes docTypes, DateTime startTime)
    {
      _MainConGlobal = mainConGlobal;
      _UndoCon = undoCon;

      _ActionInfo = actionInfo;
      if (string.IsNullOrEmpty(_ActionInfo))
        _ActionInfo = "???";
      // 28.03.2016
      if (_ActionInfo.Length > docTypes.ActionInfoMaxLength)
        _ActionInfo = _ActionInfo.Substring(0, docTypes.ActionInfoMaxLength);
      _UserId = userId;
      _SessionId = sessionId;
      _StartTime = startTime;
      _ActionTime = DateTime.Now;
      _UserActionId = userActionId;
      _DocTypes = docTypes;
    }

    #endregion

    #region Общие свойства

    public DBxCon MainConGlobal { get { return _MainConGlobal; } }
    private DBxCon _MainConGlobal;

    public DBxCon UndoCon { get { return _UndoCon; } }
    private DBxCon _UndoCon;

    /// <summary>
    /// Текстовое название действия пользователя для команды меню Undo
    /// </summary>
    public string ActionInfo { get { return _ActionInfo; } }
    private string _ActionInfo;

    /// <summary>
    /// Время начала редактирования
    /// </summary>
    public DateTime StartTime { get { return _StartTime; } }
    private DateTime _StartTime;

    /// <summary>
    /// Время выполнения пользователем действия
    /// </summary>
    public DateTime ActionTime { get { return _ActionTime; } }
    private DateTime _ActionTime;

    /// <summary>
    /// Идентификатор действия пользователя. При первой операции
    /// автоматически присваивается, если был равен 0
    /// </summary>
    public Int32 UserActionId { get { return _UserActionId; } }
    private Int32 _UserActionId;

    /// <summary>
    /// Идентификатор пользователя
    /// Используется при DBxDocTypes.UseUsers=true
    /// </summary>
    public Int32 UserId { get { return _UserId; } }
    private Int32 _UserId;

    /// <summary>
    /// Идентификатор сессии.
    /// Используется при DBxDocTypes.UseSessionId=true
    /// </summary>
    public Int32 SessionId { get { return _SessionId; } }
    private Int32 _SessionId;

    /// <summary>
    /// Описания видов документов
    /// </summary>
    public DBxDocTypes DocTypes { get { return _DocTypes; } }
    private DBxDocTypes _DocTypes;

    #endregion

    #region Добавление записей в таблицы UserActions и DocActions

    public void CheckIsRealDocId(DBxDocType docType, Int32 id)
    {
      if (docType == null)
        throw new ArgumentNullException("docType");
      if (id < 0)
        throw new BugException("Недопустимый идентификатор документа " + docType.SingularTitle + ": " + id.ToString());
    }

    /// <summary>
    /// Добавление записи в DocActions. 
    /// При первом вызове также добавляется строка в UserActions
    /// </summary>
    /// <param name="docType">Вид документа</param>
    /// <param name="docId">Идентификатор документа, для которого добавляется запись</param>
    /// <param name="action">Код действия над документом</param>
    /// <param name="docVersion">Вход и выход: Версия документа.</param>
    public Int32 AddDocAction(DBxDocType docType, Int32 docId, UndoAction action, ref Int32 docVersion)
    {
      // 31.01.2022 Перенесено наверх, до проверки UndoCon=null
      docVersion++;
      if (docVersion > Int16.MaxValue)
        throw new InvalidOperationException("Невозможно задать следующую версию документа " + docType.SingularTitle + " с DocId=" + docId.ToString() + ", т.к. она превышает максимальное значение " + Int32.MaxValue.ToString());

      if (UndoCon == null)
        //return 0;
        return -1; // 31.01.202

      CheckIsRealDocId(docType, docId);

      Int32 docActionId;

      // Уже было проверено раньше
      //Int32 UndoDocVersion = DataTools.GetInt(UndoCon.GetMaxValue("DocActions", "Version",
      //  new AndFilter(new ValueFilter("DocTableId", docType.TableId), new ValueFilter("DocId", docId))));
      // DoValidateDocVersion(DocType, DocId, UndoDocVersion, ref DocVersion);

      Hashtable FieldPairs = new Hashtable();
      FieldPairs.Add("UserActionId", UserActionId);
      FieldPairs.Add("DocTableId", docType.TableId);
      FieldPairs.Add("DocId", docId);
      FieldPairs.Add("Version", docVersion);
      FieldPairs.Add("Action", (int)action);
      docActionId = _UndoCon.AddRecordWithIdResult("DocActions", FieldPairs);

      return docActionId;
    }

    /// <summary>
    /// Признак вызова InitUserAction()
    /// </summary>
    private bool _UserActionHasBeenInit;

    /// <summary>
    /// Этот метод должен быть вызван из ApplyChanges()
    /// </summary>
    public void InitUserAction()
    {
      if (_UserActionHasBeenInit)
        return;

      _UserActionHasBeenInit = true;

      if (_UserActionId == 0)
      {
        #region Первый вызов

        Hashtable fieldPairs = new Hashtable();
        if (DocTypes.UseUsers) // 31.07.2018
          fieldPairs.Add("UserId", _UserId);
        if (DocTypes.UseSessionId)
          fieldPairs.Add("SessionId", _SessionId);
        fieldPairs.Add("StartTime", StartTime);
        fieldPairs.Add("ActionTime", ActionTime);
        fieldPairs.Add("ActionInfo", ActionInfo);
        fieldPairs.Add("ApplyChangesCount", 1);
        fieldPairs.Add("ApplyChangesTime", ActionTime);
        //FieldPairs.Add("AccDepBuild", FAccDepBuild);
        _UserActionId = UndoCon.AddRecordWithIdResult("UserActions", fieldPairs);

        #endregion
      }
      else
      {
        #region Повторный вызов

        int applyChangesCount = DataTools.GetInt(UndoCon.GetValue("UserActions", _UserActionId, "ApplyChangesCount"));
        if (applyChangesCount <= 0)
          throw new BugException("Существующее значение ApplyChangesCount=" + applyChangesCount.ToString());

        Hashtable fieldPairs = new Hashtable();
        if (DocTypes.UseUsers) // 31.08.2018
          fieldPairs.Add("UserId", _UserId);
        if (DocTypes.UseSessionId)
          fieldPairs.Add("SessionId", _SessionId);
        fieldPairs.Add("ApplyChangesCount", applyChangesCount + 1);
        fieldPairs.Add("ApplyChangesTime", ActionTime);
        UndoCon.SetValues("UserActions", _UserActionId, fieldPairs);

        #endregion
      }
    }

    #endregion

    #region Проверка таблицы DocActions

    public void ValidateDocVersion(DBxDocType docType, Int32 docId)
    {
      CheckIsRealDocId(docType, docId);

      Int32 docVersion = DataTools.GetInt(MainConGlobal.GetValue(docType.Name, docId, "Version"));

      Int32 undoDocVersion = DataTools.GetInt(UndoCon.GetMaxValue("DocActions", "Version",
        new AndFilter(new ValueFilter("DocTableId", docType.TableId), new ValueFilter("DocId", docId))));

      DoValidateDocVersion(docType, docId, undoDocVersion, docVersion);
    }

    /// <summary>
    /// Проверка текущей версии документа
    /// Сравнивает поле Version, записанное в последней строке таблицы DocActions с полем Version
    /// основной базы данных
    /// При отсутствии записей в таблице DocActions добавляет запись с Action="Base"
    /// При наличии расхождений выполняет корректировку данных
    /// На момент вызова таблица DocActions заблокирована
    /// </summary>
    /// <param name="docType"></param>
    /// <param name="docId"></param>
    /// <param name="undoDocVersion">Значение поля Version в таблице DocActions</param>
    /// <param name="mainDocVersion">Значение поля Version в основной таблице документа</param>
    private void DoValidateDocVersion(DBxDocType docType, Int32 docId, Int32 undoDocVersion, Int32 mainDocVersion)
    {
      if (mainDocVersion == 0)
        return; // Документ создается, проверка не нужна
      if (mainDocVersion == undoDocVersion)
        return; // обычная ситуация, корректировка не требуется

      Hashtable fieldPairs;

      if (undoDocVersion == 0)
      {
        // База данных undo была удалена.
        // Это не считается ошибкой
        // Добавляем запись Action=Base
        // Транзакция не нужна

        fieldPairs = new Hashtable();
        // FieldPairs.Add("UserActionId", null);
        fieldPairs.Add("DocTableId", docType.TableId);
        fieldPairs.Add("DocId", docId);
        fieldPairs.Add("Version", mainDocVersion);
        fieldPairs.Add("Action", (int)UndoAction.Base);
        UndoCon.AddRecordWithIdResult("DocActions", fieldPairs);
        return;
      }

      if (undoDocVersion < mainDocVersion)
      {
        // Ошибка может возникнуть, если база данных была повреждена и восстановлена из резервной копии, 
        // а основная база данных оказалась более новой.
        // Транзакция не нужна

        #region Добавляем псевдозапись в UserActions

        object[] aMainValues = MainConGlobal.GetValues(docType.Name, docId,
          new DBxColumns("ChangeTime,ChangeUserId,CreateTime,CreateUserId"));

        DateTime? fakeTime = DataTools.GetNullableDateTime(aMainValues[0]);
        Int32 fakeUserId = DataTools.GetInt(aMainValues[1]);
        if (fakeUserId == 0)
        {
          fakeTime = DataTools.GetNullableDateTime(aMainValues[2]);
          fakeUserId = DataTools.GetInt(aMainValues[3]);
        }

        Int32 fakeUserActionId = 0;
        if (fakeUserId != 0 && fakeTime.HasValue)
        {
          fieldPairs = new Hashtable();
          fieldPairs.Add("UserId", fakeUserId);
          fieldPairs.Add("ActionTime", fakeTime);
          fieldPairs.Add("ActionInfo", "[ Автоматическое восстановление состояния с версии " + undoDocVersion.ToString() + " ]");
          //FieldPairs.Add("AccDepBuild", FAccDepBuild);
          fakeUserActionId = _UndoCon.AddRecordWithIdResult("UserActions", fieldPairs);
        }
        else
        {
          try
          {
            Exception e = new InvalidOperationException("Для документа \"" + docType.SingularTitle + "\" с DocId=" + docId.ToString() +
              " не удалось извлечь текущие значения пользователя/времени из основной таблицы документов");
            e.Data["ChangeTime"] = aMainValues[0];
            e.Data["ChangeUserId"] = aMainValues[1];
            e.Data["CreateTime"] = aMainValues[2];
            e.Data["CreateUserId"] = aMainValues[3];
            throw e;
          }
          catch (Exception e)
          {
            LogoutTools.LogoutException(e, "Ошибка добавления фиктивной записи в UserActions");
          }
        }

        #endregion

        #region Добавляем запись Base в DocActions

        fieldPairs = new Hashtable();
        if (fakeUserActionId != 0)
          fieldPairs.Add("UserActionId", fakeUserActionId);
        fieldPairs.Add("DocTableId", docType.TableId);
        fieldPairs.Add("DocId", docId);
        fieldPairs.Add("Version", mainDocVersion);
        fieldPairs.Add("Action", (int)UndoAction.Base);
        UndoCon.AddRecordWithIdResult("DocActions", fieldPairs);

        #endregion

        #region Актуализация

        ActualizeAll(docType, docId, mainDocVersion);

        #endregion

        return;
      }
      else
      {
        // Серьезная ошибка
        // Версия в DocActions больше, чем в основной строке документа
        // Пропускаем номера версий, которые есть в DocActions

        try
        {
          Exception e = new InvalidOperationException("Для документа \"" + docType.SingularTitle + "\" с DocId=" + docId.ToString() +
            " обнаружено несоответствие версии в основной таблице документов (" + mainDocVersion.ToString() +
            ") и в таблице DocActions (" + undoDocVersion.ToString() + "). В основной таблице документов будет скорректирована версия");
          throw e;
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка добавления фиктивной записи в UserActions");
        }

        MainConGlobal.SetValue(docType.Name, docId, "Version", undoDocVersion); // транзакция не нужна
      }
    }

    #endregion

    #region Актуализация

#if XXX
    /// <summary>
    /// Исключение о расхождении версии документа в таблице DocActions
    /// </summary>
    private class UndoDocActionsException : Exception
    {
    #region Конструктор

      public UndoDocActionsException(DBxDocType docType, Int32 docId, int docVersion, int[] docActionDocVersions)
        : base("Расхождение версии документа в таблице DocActions для документа \"" + docType.SingularTitle + "\" c DocId=" + docId.ToString() +
        ". Версия документа в основной базе данных: " + docVersion.ToString() + ", последняя версия в таблице DocActions: " + DataTools.GetString(DataTools.LastItem(docActionDocVersions)) +
        ". Таблица DocActions будет очищена")
      {
        _DocTypeName = docType.Name;
        _DocTableId = docType.TableId;
        _DocId = docId;
        _DocVersion = docVersion;
        _DocActionDocVersions = docActionDocVersions;
      }

      #endregion

    #region Свойства

      public string DocTypeName { get { return _DocTypeName; } }
      private string _DocTypeName;

      public Int32 DocTableId { get { return _DocTableId; } }
      private Int32 _DocTableId;

      public Int32 DocId { get { return _DocId; } }
      private Int32 _DocId;

      public Int32 DocVersion { get { return _DocVersion; } }
      private Int32 _DocVersion;

      public Int32[] DocActionDocVersions { get { return _DocActionDocVersions; } }
      private Int32[] _DocActionDocVersions;

      #endregion
    }
#endif

    /// <summary>
    /// Актуализация основного документа и всех поддокументов
    /// </summary>
    /// <param name="docType"></param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="docVersion"></param>
    public void ActualizeAll(DBxDocType docType, Int32 docId, int docVersion)
    {
      ActualizeMainDoc(docType, docId, docVersion);
      foreach (DBxSubDocType sdt in docType.SubDocs)
      {
        IdList SubDocIds = MainConGlobal.GetIds(sdt.Name, new IdsFilter("DocId", docId)); // включая deleted
        foreach (Int32 SubDocId in SubDocIds)
          ActualizeSubDoc(sdt, SubDocId, docVersion);
      }
    }

    public void ActualizeMainDoc(DBxDocType docType, Int32 docId, int docVersion)
    {
      if (docType == null)
        throw new ArgumentNullException("docType");
      CheckIsRealDocId(docType, docId);
      if (docVersion < 1 || docVersion > short.MaxValue)
        throw new ArgumentOutOfRangeException("docVersion", docVersion, "Неправильная существующая версия документа\"" + docType.SingularTitle + "\" DocId=" + docId.ToString());

      Int32 version2 = DataTools.GetInt(MainConGlobal.GetValue(docType.Name, docId, "Version2"));
      //if (Version2 == DocVersion)
      //  return;  // актуальная версия
      if (version2 == 0)
        // Исправляем
        version2 = docVersion;

      Int32 undoId = UndoCon.FindRecord(docType.Name, new AndFilter(new ValueFilter("DocId", docId),
        new ValueFilter("Version2", version2)));
      if (undoId != 0)
        return; // уже вызывалось

      DBxColumnList colNames = new DBxColumnList();
      docType.Struct.Columns.GetColumnNames(colNames);
      object[] fieldValues = MainConGlobal.GetValues(docType.Name, docId, new DBxColumns(colNames));

      Hashtable fieldPairs = DataTools.NamesAndValuesToPairs(colNames.ToArray(), fieldValues);
      fieldPairs.Add("DocId", docId);
      fieldPairs.Add("Version2", version2);

      // Транзакция не нужна, т.к. добавляется только одна строка
      // И частично актуализированный документ не является ошибкой
      // Повторная актуализация ни к чему плохому не приведет
      UndoCon.AddRecordWithIdResult(docType.Name, fieldPairs);
    }

    /// <summary>
    /// Актуализация одного поддокумента
    /// </summary>
    /// <param name="subDocType"></param>
    /// <param name="subDocId"></param>
    /// <param name="docVersion"></param>
    public void ActualizeSubDoc(DBxSubDocType subDocType, Int32 subDocId, int docVersion)
    {
      if (subDocType == null)
        throw new ArgumentNullException("subDocType");
      if (subDocId <= 0)
        throw new ArgumentException("Фиктивный идентификатор поддокумента \"" + subDocType.SingularTitle + "\" SubDocId=" + subDocId.ToString(), "subDocId");
      if (docVersion < 1 || docVersion > short.MaxValue)
        throw new ArgumentOutOfRangeException("docVersion", docVersion, "Неправильная существующая версия документа\"" + subDocType.DocType.SingularTitle + "\"");

      Int32 version2 = DataTools.GetInt(MainConGlobal.GetValue(subDocType.Name, subDocId, "Version2"));
      //if (Version2 == DocVersion)
      //  return;  // актуальная версия
      if (version2 == 0)
        // Исправляем
        version2 = docVersion;

      Int32 undoId = UndoCon.FindRecord(subDocType.Name, new AndFilter(new ValueFilter("SubDocId", subDocId),
        new ValueFilter("Version2", version2)));
      if (undoId != 0)
        return; // уже вызывалось

      DBxColumnList colNames = new DBxColumnList();
      colNames.Add("Deleted");
      subDocType.Struct.Columns.GetColumnNames(colNames);
      object[] fieldValues = MainConGlobal.GetValues(subDocType.Name, subDocId, new DBxColumns(colNames));

      Hashtable fieldPairs = DataTools.NamesAndValuesToPairs(colNames.ToArray(), fieldValues);
      fieldPairs.Add("SubDocId", subDocId);
      fieldPairs.Add("Version2", version2);

      // Также без транзакции
      UndoCon.AddRecordWithIdResult(subDocType.Name, fieldPairs);
    }

#if XXX
    private DataTable GetDocUndoData(DBxDocType DocType, Int32 DocId)
    {
      // Здесь блокировка DocActions не нужна, т.к. в ValidateBase и так выполняется блокировка

      return FUndoCon.FillSelect("DocActions", null,
        new AndFilter(new ValueFilter("DocTableId", DocType.TableId),
        new ValueFilter("DocId", DocId)), DBxOrder.FromColumnNames("Version"));
    }
#endif

#if XXX
    /// <summary>
    /// Получение текущей версии документа из основной таблицы данных
    /// </summary>
    /// <param name="docType"></param>
    /// <param name="docId">Идентификатор Id документа</param>
    /// <returns></returns>
    private int GetCurrentDocVersion(DBxDocType docType, Int32 docId)
    {
      CheckIsRealDocId(docType, docId);

      object o = _MainConGlobal.GetValue(docType.Name, docId, "Version");
      return DataTools.GetInt(o);
    }
#endif

    #endregion
  }
}
