// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{

  /// <summary>
  /// Доступ к отдельному документу
  /// Для доступа к объекту используется индексированное свойство DBxMultiDocs[] или метод DBxMultiDocs.FindDocId()
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DBxSingleDoc
  {
    #region Защищенный конструктор

    internal DBxSingleDoc(DBxMultiDocs multiDocs, int rowIndex)
    {
      if (rowIndex < 0)
        throw new ArgumentOutOfRangeException();

      _MultiDocs = multiDocs;
      _RowIndex = rowIndex;
    }

    #endregion

    #region Свойства

    internal DataRow Row { get { return _MultiDocs.Table.Rows[_RowIndex]; } }
    private int _RowIndex;


    // Если хранить прямую ссылку на строку DataRow, то она станет недействительной, т.к. от сервера будет
    // получен новый набор данных.
    // Поэтому храним ссылки на DBxMultiDocs (который умеет обновлять свою ссылку на таблицу) и номер строки в таблице
    //private DataRow FRow;

    /// <summary>
    /// Набор данных, к которому относится документ
    /// </summary>
    public DBxDocSet DocSet { get { return MultiDocs.DocSet; } }

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public DBxMultiDocs MultiDocs { get { return _MultiDocs; } }
    private DBxMultiDocs _MultiDocs;

    /// <summary>
    /// Описание вида документа
    /// </summary>
    public DBxDocType DocType { get { return MultiDocs.DocType; } }

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return MultiDocs.DocProvider; } }

    /// <summary>
    /// Идентификатор документа.
    /// Если документ был создан, но еще не записан в базу данных, возвращается отрицательное значение.
    /// После записи этого документа он получает реальный идентификатор.
    /// </summary>
    public Int32 DocId { get { return (Int32)(DBxDocSet.GetValue(Row, "Id")); } }

    /// <summary>
    /// Возвращает DocId, а если документ был создан, но не записан в базу данных, то 0.
    /// </summary>
    public Int32 RealDocId
    {
      get
      {
        Int32 v = DocId;
        if (DocSet.DocProvider.IsRealDocId(v))
          return v;
        else
          return 0;
      }
    }

    /// <summary>
    /// Текущее состояние документа
    /// </summary>
    public DBxDocState DocState
    {
      get
      {
        return DBxDocSet.GetDocState(Row);
      }
    }

    /// <summary>
    /// Возвращает true, если открытый документ был помечен на удаление.
    /// Если DocState=Edit, то после вызова ApplyChanges() документ будет восстановлен
    /// Если документ был помечен на удаление в текущем сеансе работы вызовом метода Delete(),
    /// то для него объект DBxSingleDoc становится недействительным
    /// </summary>
    public bool Deleted
    {
      get
      {
        if (DocProvider.DocTypes.UseDeleted)
          return DataTools.GetBool(DBxDocSet.GetValue(Row, "Deleted"));
        else
          return false;
      }
    }


    /// <summary>
    /// Коллекция видов поддокументов.
    /// Вложенные коллекции содержат только поддокументы, относящиеся к данному документу
    /// </summary>
    public DBxSingleDocSubDocs SubDocs { get { return new DBxSingleDocSubDocs(this); } }

    /// <summary>
    /// Текстовое представление для отладки.
    /// Содержит вид документа, идентификатор и состояние документа
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(DocType.SingularTitle);
      sb.Append(", DocId=");
      sb.Append(DocId.ToString());
      sb.Append(", DocState=");
      sb.Append(DocState.ToString());
      return sb.ToString();
    }

    #endregion

    #region Версия документа

    /// <summary>
    /// Возвращает текущую версию документа.
    /// До первого вызова DBxDocSet.ApplyChanges() возвращает старую версию. Для документов в режиме Insert
    /// возвращается 0. После вызова ApplyChanges() возвращает измененную версию документа. 
    /// Для режима Edit версия может не поменяться.
    /// </summary>
    public int Version
    {
      get
      {
        if (DocProvider.DocTypes.UseVersions)
          return DataTools.GetInt(DBxDocSet.GetValue(Row, "Version"));
        else
          return 0;
      }
    }

#if XXX // Нужных полей нет в DBxDocProvider.GetTemplate() (16.10.2018)
    /// <summary>
    /// Возвращает идентификатор пользователя, создавшего документ.
    /// Если документ находится в режиме создания и еще не был записан, возвращает 0.
    /// Если DocProvider.DocTypes.UseUsers=false, возвращает 0
    /// </summary>
    public Int32 CreateUserId
    {
      get
      {
        if (DocProvider.DocTypes.UseUsers)
          return DataTools.GetInt(DBxDocSet.GetValue(Row, "CreateUserId"));
        else
          return 0;
      }
    }

    /// <summary>
    /// Возвращает идентификатор пользователя, записавшего документ.
    /// Если документ находится в режиме создания и еще не был записан, возвращает 0.
    /// Если документ был только один раз сохранен, возвращает 0. 
    /// Если DocProvider.DocTypes.UseUsers=false, возвращает 0.
    /// </summary>
    public Int32 ChangeUserId
    {
      get
      {
        if (DocProvider.DocTypes.UseUsers)
          return DataTools.GetInt(DBxDocSet.GetValue(Row, "ChangeUserId"));
        else
          return 0;
      }
    }


    /// <summary>
    /// Возвращает идентификатор пользователя, последним записавшего документ.
    /// Возвращает ChangeUserId или CreateUserId
    /// Если документ находится в режиме создания и еще не был записан, возвращает 0.
    /// Если DocProvider.DocTypes.UseUsers=false, возвращает 0.
    /// </summary>
    public Int32 LastWriteUserId
    {
      get
      {
        if (DocProvider.DocTypes.UseUsers)
        {
          Int32 UserId = DataTools.GetInt(DBxDocSet.GetValue(Row, "ChangeUserId"));
          if (UserId != 0)
            return UserId;
          else
            return DataTools.GetInt(DBxDocSet.GetValue(Row, "CreateUserId"));
        }
        else
          return 0;
      }
    }


    /// <summary>
    /// Возвращает время создания документа.
    /// Если документ находится в режиме создания и еще не был записан, возвращает null.
    /// Если DocProvider.DocTypes.UseTime=false, возвращает null
    /// </summary>
    public DateTime? CreateTime
    {
      get
      {
        if (DocProvider.DocTypes.UseTime)
          return DataTools.GetNullableDateTime(DBxDocSet.GetValue(Row, "CreateTime"));
        else
          return null;
      }
    }


    /// <summary>
    /// Возвращает время изменения документа.
    /// Если документ находится в режиме создания и еще не был записан, возвращает null.
    /// Возвращает ChangeTime или CreateTime
    /// Если DocProvider.DocTypes.UseTime=false, возвращает null
    /// </summary>
    public DateTime? ChangeTime
    {
      get
      {
        if (DocProvider.DocTypes.UseTime)
        {
          DateTime? tm = DataTools.GetNullableDateTime(DBxDocSet.GetValue(Row, "ChangeTime"));
          if (tm.HasValue)
            return tm;
          else
            return DataTools.GetNullableDateTime(DBxDocSet.GetValue(Row, "CreateTime"));
        }
        else
          return null;
      }
    }


    /// <summary>
    /// Возвращает время последней документа.
    /// Если документ находится в режиме создания и еще не был записан, возвращает null.
    /// Если DocProvider.DocTypes.UseTime=false, возвращает null
    /// </summary>
    public DateTime? LastWriteTime
    {
      get
      {
        if (DocProvider.DocTypes.UseTime)
          return DataTools.GetNullableDateTime(DBxDocSet.GetValue(Row, "ChangeTime"));
        else
          return null;
      }
    } 
#endif

    #endregion

    #region Доступ к значениям полей документа

    /// <summary>
    /// Значения полей одиночного документа
    /// </summary>
    public IDBxDocValues Values
    {
      get
      {
        // Объект-владелец хранит коллекцию объектов
        return _MultiDocs.GetSingleDocValues(_RowIndex);
      }
    }

    /// <summary>
    /// Доступ к оригинальным значениям в режиме Edit
    /// </summary>
    public IDBxDocValues OriginalValues
    {
      get
      {
        if (DocState == DBxDocState.Edit || DocState == DBxDocState.Delete)
          return _MultiDocs.GetSingleDocOriginalValues(_RowIndex);
        else
          return null;
      }
    }

    #endregion

    #region Изменение состояния документов

    /// <summary>
    /// Переводит документ в режим редактирования.
    /// Документ должен находиться в состоянии View
    /// </summary>
    public void Edit()
    {
      MultiDocs.CheckCanModify();

      switch (DocState)
      {
        case DBxDocState.Edit:
          break; // Ничего не делаем
        case DBxDocState.View:
          Row.SetModified();
          break;
        default:
          throw new InvalidOperationException("Нельзя перевести документ " + ToString() + " в режим редактирования из текущего состояния");
      }
    }

    /// <summary>
    /// Переводит документ в режим просмотра из режима редактирования.
    /// Документ должен находиться в состоянии Edit
    /// </summary>
    public void View()
    {
      MultiDocs.CheckCanModify();

      switch (DocState)
      {
        case DBxDocState.View:
          break; // Ничего не делаем
        case DBxDocState.Edit:
          Row.RejectChanges();
          break;
        default:
          throw new InvalidOperationException("Нельзя перевести документ " + ToString() + " в режим просмотра из текущего состояния");
      }
    }

    /// <summary>
    /// Перемещает документ в список на удаление и удаляет его из списка открытых документов.
    /// Документ должен находиться в состоянии View
    /// </summary>
    public void Delete()
    {
      MultiDocs.CheckCanModify();

      Row.Delete();
    }

    /// <summary>
    /// Удаляет документ из списка открытых документов без сохранения изменений.
    /// Документ может находиться в любом состоянии (View, Edit или Insert)
    /// После вызова метода, дальнейшее использованиет объекта DBxSingleDoc невозможно
    /// </summary>
    public void RemoveFromList()
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Проверка наличия изменений в документе

    /// <summary>
    /// Свойство возвращает true, если для документа в состоянии Edit есть измененные значения
    /// Также возвращается true, если есть какие-либо изменения в поддокументах
    /// </summary>
    public bool IsDataModified
    {
      get
      {
        switch (DocState)
        {
          case DBxDocState.Insert:
          case DBxDocState.Delete:
            return true;
          case DBxDocState.Edit:
            if (DBxDocSet.GetIsModified(Row))
              return true;
            for (int i = 0; i < SubDocs.Count; i++)
            {
              foreach (DBxSubDoc SubDoc in SubDocs[i])
              {
                if (SubDoc.IsDataModified)
                  return true;
              }
            }
            return false;

          default:
            return false;
        }
      }
    }

    /// <summary>
    /// Свойство возвращает true, если для документа в состоянии Edit есть измененные значения
    /// Изменения в поддокументах не учитываются
    /// </summary>
    public bool IsMainDocModified
    {
      get
      {
        switch (DocState)
        {
          case DBxDocState.Insert:
          case DBxDocState.Delete:
            return true;
          case DBxDocState.Edit:
            return DBxDocSet.GetIsModified(Row);
          default:
            return false;
        }
      }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращается "выборка" из одного документа.
    /// Если новый документ еще не записан в базу данных, возвращается пустая выборка
    /// </summary>
    /// <returns>Выборка</returns>
    public DBxDocSelection GetDocSelection()
    {
      if (DocSet.DocProvider.IsRealDocId(DocId))
        return new DBxDocSelection(DocSet.DocProvider.DBIdentity, DocType.Name, DocId);
      else
        return new DBxDocSelection(DocSet.DocProvider.DBIdentity);
    }

    /// <summary>
    /// Возвращает true, если текущий и сравниваемый документ относятся к одному и тому же 
    /// документу базы данных.
    /// Документы могут относится к разным DBxDocProvider
    /// Возвращает false, если текущий документ не был сохранен в базе данных
    /// </summary>
    /// <param name="otherDoc">Сравниваемый документ</param>
    /// <returns>true, если один документ</returns>
    public bool IsSameDoc(DBxSingleDoc otherDoc)
    {
      if (DocId <= 0)
        return false;
      if (DocId != otherDoc.DocId)
        return false;

      if (DocType.Name != otherDoc.DocType.Name)
        return false;
      if (DocSet.DocProvider.DBIdentity != otherDoc.DocSet.DocProvider.DBIdentity)
        return false;
      return true;
    }

    /// <summary>
    /// Выполняет проверку прав доступа к документу.
    /// При отсутствии прав доступа в заданном режиме возвращается false.
    /// Вызывает метод DBxDocPermissions.TestDocument().
    /// </summary>
    /// <param name="reason">Режим доступа, права для которого проверяются</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке, если прав нет</param>
    /// <returns>Наличие прав доступа</returns>
    public bool TestDocument(DBxDocPermissionReason reason, out string errorText)
    {
      return DocProvider.DocPermissions.TestDocument(this, reason, out errorText);
    }

    ///// <summary>
    ///// Выполняет проверку прав доступа к документу.
    ///// При отсутствии прав доступа в заданном режиме генерируется исключение DBxAccessException.
    ///// Вызывает метод DBxDocPermissions.TestDocument().
    ///// </summary>
    ///// <param name="doc">Проверяемый документ</param>
    ///// <param name="reason">Режим доступа, права для которого проверяются</param>
    //[DebuggerStepThrough]
    //public void TestDocument(DBxSingleDoc doc, DBxDocPermissionReason reason)
    //{
    //  DocProvider.TestDocument(this, reason);
    //}

    #endregion

    #region Копирование

    /// <summary>
    /// Копирует значения всех полей из текущего документа в другой документ. 
    /// Копирование поддокументов (с созданием и удалением ненужных поддокументов).
    /// Документы должны быть одного типа, но могут относится к разным DocProvider с разными правами доступа
    /// </summary>
    /// <param name="resDoc">Заполняемый документ</param>
    public void CopyTo(DBxSingleDoc resDoc)
    {
      DBxDocValue.CopyValues(Values, resDoc.Values);
      for (int i = 0; i < SubDocs.Count; i++)
      {
        DBxSingleSubDocs sds1 = SubDocs[i];
        if (DocSet.DocProvider.DBPermissions.TableModes[sds1.SubDocs.SubDocType.Name] == DBxAccessMode.None)
          continue;
        if (resDoc.DocSet.DocProvider.DBPermissions.TableModes[sds1.SubDocs.SubDocType.Name] == DBxAccessMode.None)
          continue;

        DBxSingleSubDocs sds2 = resDoc.SubDocs[sds1.SubDocs.SubDocType.Name];
        if (IsSameDoc(resDoc))
        {
          // Один и тот же документ.
          // Выполняем поштучную обработку поддокументов
          foreach (DBxSubDoc sd1 in sds1)
          {
            DBxSubDoc sd2;
            switch (sd1.SubDocState)
            {
              case DBxDocState.Insert:
                sd2 = sds2.Insert();
                DBxDocValue.CopyValues(sd1.Values, sd2.Values);
                break;
              case DBxDocState.Edit:
                sd2 = sds2.GetSubDocById(sd1.SubDocId);
                DBxDocValue.CopyValues(sd1.Values, sd2.Values);
                break;
              case DBxDocState.Delete:
                sd2 = sds2.GetSubDocById(sd1.SubDocId);
                sd2.Delete();
                break;
            }
          }
        }
        else
        {
          // Разные документы.
          // Удаляем все поддокументы в записываем документе и заменяем их новыми
          sds2.Delete();

          foreach (DBxSubDoc sd1 in sds1)
          {
            DBxSubDoc sd2 = sds2.Insert();
            DBxDocValue.CopyValues(sd1.Values, sd2.Values);
          }
        }
      }
    }

    #endregion

    #region Текстовое представление документа

    /// <summary>
    /// Возвращает текстовое представление документа
    /// </summary>
    public string TextValue
    {
      get
      {
        // Если тупо передать весь набор данных в DocProvider, то может быть передача большого объема данных через сериализацию
        return DocSet.DocProvider.InternalGetTextValue(this.Row);
      }
    }

    #endregion
  }
}
