using System;
using System.Collections;
using System.Data;
using System.Reflection;
using AgeyevAV;
using System.Threading;
using AgeyevAV.Logging;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtDB.Docs
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
      if (UndoCon == null)
        return 0;

      CheckIsRealDocId(docType, docId);

      Int32 DocActionId;

      // Уже было проверено раньше
      //Int32 UndoDocVersion = DataTools.GetInt(UndoCon.GetMaxValue("DocActions", "Version",
      //  new AndFilter(new ValueFilter("DocTableId", docType.TableId), new ValueFilter("DocId", docId))));
      // DoValidateDocVersion(DocType, DocId, UndoDocVersion, ref DocVersion);

      docVersion++;
      if (docVersion > Int16.MaxValue)
        throw new InvalidOperationException("Невозможно задать следующую версию документа " + docType.SingularTitle + " с DocId=" + docId.ToString() + ", т.к. она превышает максимальное значение " + Int32.MaxValue.ToString());

      Hashtable FieldPairs = new Hashtable();
      FieldPairs.Add("UserActionId", UserActionId);
      FieldPairs.Add("DocTableId", docType.TableId);
      FieldPairs.Add("DocId", docId);
      FieldPairs.Add("Version", docVersion);
      FieldPairs.Add("Action", (int)action);
      DocActionId = _UndoCon.AddRecordWithIdResult("DocActions", FieldPairs);

      return DocActionId;
    }

    /// <summary>
    /// Признак вызова InitUserAction()
    /// </summary>
    private bool _UserActionHasBeenInit;

    /// <summary>
    /// Этот метод должен быть вызван из ApplyChanges
    /// </summary>
    public void InitUserAction()
    {
      if (_UserActionHasBeenInit)
        return;

      _UserActionHasBeenInit = true;

      if (_UserActionId == 0)
      {
        #region Первый вызов

        Hashtable FieldPairs = new Hashtable();
        if (DocTypes.UseUsers) // 31.07.2018
          FieldPairs.Add("UserId", _UserId);
        if (DocTypes.UseSessionId)
          FieldPairs.Add("SessionId", _SessionId);
        FieldPairs.Add("StartTime", StartTime);
        FieldPairs.Add("ActionTime", ActionTime);
        FieldPairs.Add("ActionInfo", ActionInfo);
        FieldPairs.Add("ApplyChangesCount", 1);
        FieldPairs.Add("ApplyChangesTime", ActionTime);
        //FieldPairs.Add("AccDepBuild", FAccDepBuild);
        _UserActionId = UndoCon.AddRecordWithIdResult("UserActions", FieldPairs);

        #endregion
      }
      else
      {
        #region Повторный вызов

        int ApplyChangesCount = DataTools.GetInt(UndoCon.GetValue("UserActions", _UserActionId, "ApplyChangesCount"));
        if (ApplyChangesCount <= 0)
          throw new BugException("Существующее значение ApplyChangesCount=" + ApplyChangesCount.ToString());

        Hashtable FieldPairs = new Hashtable();
        if (DocTypes.UseUsers) // 31.08.2018
          FieldPairs.Add("UserId", _UserId);
        if (DocTypes.UseSessionId)
          FieldPairs.Add("SessionId", _SessionId);
        FieldPairs.Add("ApplyChangesCount", ApplyChangesCount + 1);
        FieldPairs.Add("ApplyChangesTime", ActionTime);
        UndoCon.SetValues("UserActions", _UserActionId, FieldPairs);

        #endregion
      }
    }

    #endregion

    #region Проверка таблицы DocActions

    public void ValidateDocVersion(DBxDocType docType, Int32 docId)
    {
      CheckIsRealDocId(docType, docId);

      Int32 DocVersion = DataTools.GetInt(MainConGlobal.GetValue(docType.Name, docId, "Version"));

      Int32 UndoDocVersion = DataTools.GetInt(UndoCon.GetMaxValue("DocActions", "Version",
        new AndFilter(new ValueFilter("DocTableId", docType.TableId), new ValueFilter("DocId", docId))));

      DoValidateDocVersion(docType, docId, UndoDocVersion, DocVersion);
    }

    /// <summary>
    /// Проверка текущей версии документа
    /// Сравнивает поле Version, записанное в последней строке таблицы DocActions с полем Version
    /// основной базы данных
    /// При отсутстивии записей в таблице DocActions добавляет запись с Action="Base"
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

      Hashtable FieldPairs;

      if (undoDocVersion == 0)
      {
        // База данных undo была удалена.
        // Это не считается ошибкой
        // Добавляем запись Action=Base
        // Транзакция не нужна

        FieldPairs = new Hashtable();
        // FieldPairs.Add("UserActionId", null);
        FieldPairs.Add("DocTableId", docType.TableId);
        FieldPairs.Add("DocId", docId);
        FieldPairs.Add("Version", mainDocVersion);
        FieldPairs.Add("Action", (int)UndoAction.Base);
        UndoCon.AddRecordWithIdResult("DocActions", FieldPairs);
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

        DateTime? FakeTime = DataTools.GetNullableDateTime(aMainValues[0]);
        Int32 FakeUserId = DataTools.GetInt(aMainValues[1]);
        if (FakeUserId == 0)
        {
          FakeTime = DataTools.GetNullableDateTime(aMainValues[2]);
          FakeUserId = DataTools.GetInt(aMainValues[3]);
        }

        Int32 FakeUserActionId = 0;
        if (FakeUserId != 0 && FakeTime.HasValue)
        {
          FieldPairs = new Hashtable();
          FieldPairs.Add("UserId", FakeUserId);
          FieldPairs.Add("ActionTime", FakeTime);
          FieldPairs.Add("ActionInfo", "[ Автоматическое восстановление состояния с версии " + undoDocVersion.ToString() + " ]");
          //FieldPairs.Add("AccDepBuild", FAccDepBuild);
          FakeUserActionId = _UndoCon.AddRecordWithIdResult("UserActions", FieldPairs);
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

        FieldPairs = new Hashtable();
        if (FakeUserActionId != 0)
          FieldPairs.Add("UserActionId", FakeUserActionId);
        FieldPairs.Add("DocTableId", docType.TableId);
        FieldPairs.Add("DocId", docId);
        FieldPairs.Add("Version", mainDocVersion);
        FieldPairs.Add("Action", (int)UndoAction.Base);
        UndoCon.AddRecordWithIdResult("DocActions", FieldPairs);

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

      Int32 Version2 = DataTools.GetInt(MainConGlobal.GetValue(docType.Name, docId, "Version2"));
      //if (Version2 == DocVersion)
      //  return;  // актуальная версия
      if (Version2 == 0)
        // Исправляем
        Version2 = docVersion;

      Int32 UndoId = UndoCon.FindRecord(docType.Name, new AndFilter(new ValueFilter("DocId", docId),
        new ValueFilter("Version2", Version2)));
      if (UndoId != 0)
        return; // уже вызывалось

      DBxColumnList ColNames = new DBxColumnList();
      docType.Struct.Columns.GetColumnNames(ColNames);
      object[] FieldValues = MainConGlobal.GetValues(docType.Name, docId, new DBxColumns(ColNames));

      Hashtable FieldPairs = DataTools.NamesAndValuesToPairs(ColNames.ToArray(), FieldValues);
      FieldPairs.Add("DocId", docId);
      FieldPairs.Add("Version2", Version2);

      // Транзакция не нужна, т.к. добавляется только одна строка
      // И частично актуализированный документ не является ошибкой
      // Повторная актуализация ни к чему плохому не приведет
      UndoCon.AddRecordWithIdResult(docType.Name, FieldPairs);
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

      Int32 Version2 = DataTools.GetInt(MainConGlobal.GetValue(subDocType.Name, subDocId, "Version2"));
      //if (Version2 == DocVersion)
      //  return;  // актуальная версия
      if (Version2 == 0)
        // Исправляем
        Version2 = docVersion;

      Int32 UndoId = UndoCon.FindRecord(subDocType.Name, new AndFilter(new ValueFilter("SubDocId", subDocId),
        new ValueFilter("Version2", Version2)));
      if (UndoId != 0)
        return; // уже вызывалось

      DBxColumnList ColNames = new DBxColumnList();
      ColNames.Add("Deleted");
      subDocType.Struct.Columns.GetColumnNames(ColNames);
      object[] FieldValues = MainConGlobal.GetValues(subDocType.Name, subDocId, new DBxColumns(ColNames));

      Hashtable FieldPairs = DataTools.NamesAndValuesToPairs(ColNames.ToArray(), FieldValues);
      FieldPairs.Add("SubDocId", subDocId);
      FieldPairs.Add("Version2", Version2);

      // Также без транзакции
      UndoCon.AddRecordWithIdResult(subDocType.Name, FieldPairs);
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
