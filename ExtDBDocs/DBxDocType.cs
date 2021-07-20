﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using AgeyevAV.Config;
using System.Collections;
using System.Threading;

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
  #region Делегаты

  #region ServerDocTypeDocEventArgs

  /// <summary>
  /// Базовый класс
  /// </summary>
  public class ServerDocTypeDocEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Не должен использоваться в пользовательском коде
    /// </summary>
    /// <param name="doc">Обрабатываемый документ</param>
    public ServerDocTypeDocEventArgs(DBxSingleDoc doc)
    {
      _Doc = doc;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Обрабатываемый документ
    /// </summary>
    public DBxSingleDoc Doc { get { return _Doc; } }
    private DBxSingleDoc _Doc;

    /// <summary>
    /// Доступ к кэшированным данным других документов.
    /// Это свойство не следует использовать для извлечения данных из обрабатываемых документов,
    /// т.к. в кэше содержатся неактуальные данные
    /// </summary>
    public DBxCache DBCache { get { return _Doc.DocProvider.DBCache; } }

    ///// <summary>
    ///// Процедура, выполняющая вызов события
    ///// </summary>
    //public AccDepServerExec Caller { get { return FDocsChanges.Caller; } }

    ///// <summary>
    ///// Провайдер документов
    ///// </summary>
    //public DocProvider DocProvider { get { return Caller.DocProvider; } }

    ///// <summary>
    ///// Идентификатор пользователя, выполняющего запись
    ///// </summary>
    //public int UserId { get { return FDocsChanges.Owner.UserId; } }

    //public SingleDocChangesInfo SingleChanges { get { return FDocsChanges.Docs[DocIndex]; } }

    #endregion

    #region Доступ к поддокументам

    ///// <summary>
    ///// Получить массив строк.
    ///// Удаленные строки не включаются
    ///// Можно вносить изменения в поля
    ///// </summary>
    ///// <param name="SubDocTypeName"></param>
    ///// <returns></returns>
    //public DataRow[] GetSubDocRows(string SubDocTypeName)
    //{
    //  return FDocsChanges.SubDocs[SubDocTypeName].GetRowsForDocId(DocId, false);
    //}

    #endregion
  }

  #endregion

  #region ServerDocTypeBeforeInsertEventHandler

  /// <summary>
  /// Аргументы события DBxDocType.BeforeInsert
  /// </summary>
  public class ServerDocTypeBeforeInsertEventArgs : ServerDocTypeDocEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Аргументы создаются в объекте DBxDocType
    /// </summary>
    /// <param name="doc">Обрабатываемый документ</param>
    /// <param name="restoreDeleted">true, если событие вызвано при восстановлении удаленного документа</param>
    public ServerDocTypeBeforeInsertEventArgs(DBxSingleDoc doc, bool restoreDeleted)
      : base(doc)
    {
      _RestoreDeleted = restoreDeleted;
    }


    #endregion

    #region Свойства

    /// <summary>
    /// Свойство возвращает true, если событие вызвано при восстановлении удаленного документа,
    /// и false, если создается новый документ
    /// </summary>
    public bool RestoreDeleted { get { return _RestoreDeleted; } }
    private bool _RestoreDeleted;

    #endregion
  }

  /// <summary>
  /// Делегат события DBxDocType.BeforeInsert
  /// </summary>
  /// <param name="sender">Объект DBxDocType</param>
  /// <param name="args">Аргументы события</param>
  public delegate void ServerDocTypeBeforeInsertEventHandler(object sender,
    ServerDocTypeBeforeInsertEventArgs args);

  #endregion

  #region ServerDocTypeBeforeWriteEventHandler

  /// <summary>
  /// Аргументы события DBxDocType.BeforeWrite
  /// </summary>
  public class ServerDocTypeBeforeWriteEventArgs : ServerDocTypeDocEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Аргументы создаются в DBxDocType, а не в пользовательском коде
    /// </summary>
    /// <param name="doc">Обрабатываемый документ</param>
    /// <param name="append">True, если вызов идет перед первым добавлением документа</param>
    /// <param name="recalcColumnsOnly">True, если вызвано из команды пересчета вычисляемых полей</param>
    public ServerDocTypeBeforeWriteEventArgs(DBxSingleDoc doc, bool append, bool recalcColumnsOnly)
      : base(doc)
    {
      _Append = append;
      _RecalcColumnsOnly = recalcColumnsOnly;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// True, если вызов идет перед первым добавлением документа, false - перед записью
    /// </summary>
    public bool Append { get { return _Append; } }
    private bool _Append;

    /// <summary>
    /// Возвращает true, если событие вызвано из команды пересчета вычисляемых полей.
    /// Если обработчик выполняет проверку допустимости выполнения записи документа, то
    /// такая проверка не должна выполняться
    /// </summary>
    public bool RecalcColumnsOnly { get { return _RecalcColumnsOnly; } }
    private bool _RecalcColumnsOnly;

    #endregion
  }

  /// <summary>
  /// Делегат события DBxDocType.BeforeWrite
  /// </summary>
  /// <param name="sender">Объект DBxDocType</param>
  /// <param name="args">Аргументы события</param>
  public delegate void ServerDocTypeBeforeWriteEventHandler(object sender,
    ServerDocTypeBeforeWriteEventArgs args);

  #endregion

  #region ServerDocTypeAfterWriteEventHandler

  /// <summary>
  /// Аргументы события DBxDocType.AfterWrite
  /// </summary>
  public class ServerDocTypeAfterWriteEventArgs : ServerDocTypeDocEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Аргументы создаются в DBxDocType, а не в пользовательском коде
    /// </summary>
    /// <param name="doc">Обрабатываемый документ</param>
    public ServerDocTypeAfterWriteEventArgs(DBxSingleDoc doc)
      : base(doc)
    {
    }

    #endregion

    // пока никаких полей
  }

  /// <summary>
  /// Делегат события DBxDocType.AfterWrite
  /// </summary>
  /// <param name="sender">Объект DBxDocType</param>
  /// <param name="args">Аргументы события</param>
  public delegate void ServerDocTypeAfterWriteEventHandler(object sender,
    ServerDocTypeAfterWriteEventArgs args);

  #endregion

  #region ServerDocTypeBeforeDeleteEventHandler

  /// <summary>
  /// Аргументы события DBxDocType.BeforeDelete
  /// </summary>
  public class ServerDocTypeBeforeDeleteEventArgs : ServerDocTypeDocEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Аргументы создаются в DBxDocType, а не в пользовательском коде
    /// </summary>
    /// <param name="doc">Обрабатываемый документ</param>
    public ServerDocTypeBeforeDeleteEventArgs(DBxSingleDoc doc)
      : base(doc)
    {
    }

    #endregion
  }

  /// <summary>
  /// Делегат события DBxDocType.BeforeDelete
  /// </summary>
  /// <param name="sender">Объект DBxDocType</param>
  /// <param name="args">Аргументы события</param>
  public delegate void ServerDocTypeBeforeDeleteEventHandler(object sender,
    ServerDocTypeBeforeDeleteEventArgs args);

  #endregion

#if XXX
  #region ServerDocTypeSearchDocEventHandler

  public class ServerDocTypeSearchDocEventArgs : EventArgs
  {
  #region Конструктор

    public ServerDocTypeSearchDocEventArgs(ServerDocType DocType,
      AccDepServerExec Caller, DataFields FieldNames, object[] FieldValues)
    {
      FDocType = DocType;
      FCaller = Caller;
      FFieldNames = FieldNames;
      FFieldValues = FieldValues;
      Id = 0;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Описание вида основного документа
    /// </summary>
    public ServerDocType DocType { get { return FDocType; } }
    private ServerDocType FDocType;

    /// <summary>
    /// Вызывающая процедура для доступа к базе данных
    /// </summary>
    public AccDepServerExec Caller { get { return FCaller; } }
    private AccDepServerExec FCaller;

    /// <summary>
    /// Доступ к БД для удобства
    /// </summary>
    public Database MainDB { get { return FCaller.MainDB; } }

    /// <summary>
    /// Имена полей, в которых можно выполнять поиск
    /// </summary>
    public DataFields FieldNames { get { return FFieldNames; } }
    private DataFields FFieldNames;

    /// <summary>
    /// Значения полей, в которых можно выполнять поиск
    /// </summary>
    public object[] FieldValues { get { return FFieldValues; } }
    private object[] FFieldValues;

    /// <summary>
    /// Если false, то поиск выполняется среди существующих документов, если
    /// true - то среди удаленных (второй проход, на первом - не нашли).
    /// </summary>
    public bool SearchInDeleted { get { return FSearchInDeleted; } }
    internal bool FSearchInDeleted;

    /// <summary>
    /// Список полей для поиска, заданный в свойстве DocType/SubDocType.SearchFieldNames
    /// </summary>
    public virtual DataFields SearchFieldNames { get { return DocType.SearchFieldNames; } }

    /// <summary>
    /// Идентификатор документа, если он будет найден или 0
    /// </summary>
    public int Id;

  #endregion

  #region Значения отдельных полей

    public object GetFieldAsObject(string FieldName)
    {
      int p = FieldNames.IndexOf(FieldName);
      if (p < 0)
        return null;
      return FieldValues[p];
    }

    /// <summary>
    /// Получить имя поля. Если данного поля нет в списке SearchFieldNames, возвращается
    /// пустая строка
    /// </summary>
    /// <param name="FieldName"></param>
    /// <returns></returns>
    public string GetFieldAsString(string FieldName)
    {
      int p = FieldNames.IndexOf(FieldName);
      if (p < 0)
        return string.Empty;
      return DataTools.GetString(FieldValues[p]);
    }

    public int GetFieldAsInt(string FieldName)
    {
      int p = FieldNames.IndexOf(FieldName);
      if (p < 0)
        return 0;
      return DataTools.GetInt(FieldValues[p]);
    }

    public decimal GetFieldAsDecimal(string FieldName)
    {
      int p = FieldNames.IndexOf(FieldName);
      if (p < 0)
        return 0m;
      return DataTools.GetDecimal(FieldValues[p]);
    }

    public bool GetFieldAsBool(string FieldName)
    {
      int p = FieldNames.IndexOf(FieldName);
      if (p < 0)
        return false;
      return DataTools.GetBool(FieldValues[p]);
    }

    public DateTime? GetFieldAsNullableDateTime(string FieldName)
    {
      int p = FieldNames.IndexOf(FieldName);
      if (p < 0)
        return null;
      return DataTools.GetNullableDateTime(FieldValues[p]);
    }

  #endregion

  #region Методы поиска в БД

    /// <summary>
    /// Выполнить поиск, используя поля
    /// Результат записывается в свойство Id (0-не найден)
    /// </summary>
    /// <param name="SearchFieldNames">Имена полей, по которым выполнить поиск</param>
    public void PerformSearch(DataFields SearchFieldNames)
    {
      //Id = DocType.DoPerformSearch(MainDB, FieldNames, FieldValues, SearchFieldNames, SearchInDeleted);
      DoPerformSearch(FieldNames, FieldValues, SearchFieldNames);
    }

    /// <summary>
    /// Выполнить стандартный поиск, как если бы обработчика SearchDoc не было
    /// Результат записывается в свойство Id (0-не найден)
    /// </summary>
    public void PerformSearch()
    {
      PerformSearch(FieldNames);
    }

    /// <summary>
    /// Выполнить поиск, используя собственный список полей и значений для поиска
    /// Результат записывается в свойство Id (0-не найден)
    /// </summary>
    /// <param name="SearchFieldNames">Список имен полей</param>
    /// <param name="SearchValues">Список значений для поиска</param>
    public void PerformSearch(DataFields SearchFieldNames, object[] SearchValues)
    {
      DoPerformSearch(SearchFieldNames, SearchValues, SearchFieldNames);
    }

    /// <summary>
    /// Выполнить стандартный поиск, используя поля SearchFieldNames и
    /// значения подстановки для недостающих полей
    /// У документа/поддокумента должно быть установлено свойство SearchFieldNames
    /// Результат записывается в свойство Id (0-не найден)
    /// </summary>
    public void PerformSearchSubst(DataFields SubstNames, object[] SubstValues)
    {
#if DEBUG
      if (SubstNames == null)
        throw new ArgumentNullException("SubstNames");
      if (SubstValues == null)
        throw new ArgumentNullException("SubstValues");
      if (SubstValues.Length != SubstNames.Count)
        throw new ArgumentException("Длина списка значений не совпадает со списком имен полей", "SubstValues");
      if (SearchFieldNames == null)
        throw new InvalidOperationException("Свойство SearchFieldNames не установлено. Поиск с подстановкой значения невозможен");
      if (!SearchFieldNames.Contains(SubstNames))
        throw new ArgumentException("Свойство SearchFieldNames " + SearchFieldNames.ToString() + " не содержит некоторых подстановочных полей " + SubstNames.ToString(), "SubstNames");
#endif

      object[] Values2 = new object[SearchFieldNames.Count];
      for (int i = 0; i < SearchFieldNames.Count; i++)
      {
        int p;
        if (FieldNames == null)
          p = -1; // Есть только ссылочные поля и все ссылки на новые документы
        else
          p = FieldNames.IndexOf(SearchFieldNames[i]);
        if (p >= 0)
          // Искомое значение
          Values2[i] = FieldValues[p];
        else
        {
          // Подстановочное значение
          p = SubstNames.IndexOf(SearchFieldNames[i]);
          if (p < 0)
          {
            /*
            throw new InvalidOperationException("Поле \"" + SearchFieldNames[i] +
              "\" отсутствует в списке искомых полей " + SearchFieldNames.ToString() +
              " и в списке значений для подстановки " + SubstNames.ToString());
             */
            Id = 0;
            return;
          }
          Values2[i] = SubstValues[p];
        }
      }

      DoPerformSearch(SearchFieldNames, Values2, SearchFieldNames);
    }
    /// <summary>
    /// Реальный поиск. Метод переопределяется для ServerSubDocType
    /// </summary>
    /// <param name="SearchFieldNames"></param>
    /// <param name="SearchFieldValues"></param>
    /// <param name="SearchFieldNames"></param>
    protected virtual void DoPerformSearch(DataFields FieldNames, object[] FieldValues, DataFields SearchFieldNames)
    {
      Id = DocType.DoPerformSearch(MainDB, FieldNames, FieldValues, SearchFieldNames, SearchInDeleted);
    }


    /// <summary>
    /// Выполнить поиск, используя поля, но найти все подходящие строки. 
    /// Свойство Id не устанавливается
    /// </summary>
    /// <param name="SearchFieldNames">Имена полей, по которым выполнить поиск</param>
    /// <returns></returns>
    public Int32[] PerformSearchAll(DataFields SearchFieldNames)
    {
      return DoPerformSearchAll(FieldNames, FieldValues, SearchFieldNames);
    }

    /// <summary>
    /// Выполнить поиск, используя поля, но найти все подходящие строки. 
    /// Свойство Id не устанавливается
    /// </summary>
    /// <param name="SearchFieldNames">Имена полей, по которым выполнить поиск</param>
    /// <param name="SearchValues">Значения для поиска. Исходные значения искомого документа не учитываются</param>
    /// <returns>Массив идентификаторов найденных документов</returns>
    public Int32[] PerformSearchAll(DataFields SearchFieldNames, object[] SearchValues)
    {
      return DoPerformSearchAll(SearchFieldNames, SearchValues, SearchFieldNames);
    }

    /// <summary>
    /// Метод переопределяется для SubDocType
    /// </summary>
    /// <param name="SearchFieldNames"></param>
    /// <param name="SearchFieldValues"></param>
    /// <param name="SearchFieldNames"></param>
    /// <returns></returns>
    protected virtual Int32[] DoPerformSearchAll(DataFields FieldNames, object[] FieldValues, DataFields SearchFieldNames)
    {
      return DocType.DoPerformSearchAll(MainDB, FieldNames, FieldValues, SearchFieldNames, SearchInDeleted);
    }

    /// <summary>
    /// Выполняет поиск всех строк, удовлетворяющих заданному условию
    /// Устанавливает свойство Id, если найдена ровно одна строка, в противном 
    /// случае Id=0
    /// </summary>
    /// <param name="SearchFieldNames">Имена полей, по которым выполнить поиск</param>
    public void PerformSearchSingle(DataFields SearchFieldNames)
    {
      Int32[] Ids = PerformSearchAll(SearchFieldNames);
      if (Ids.Length == 1)
        Id = Ids[0];
      else
        Id = 0;
    }

    /// <summary>
    /// Выполняет поиск всех строк, удовлетворяющих заданному условию
    /// Устанавливает свойство Id, если найдена ровно одна строка, в противном 
    /// случае Id=0
    /// </summary>
    /// <param name="SearchFieldNames">Имена полей, по которым выполнить поиск</param>
    /// <param name="SearchValues">Значения для поиска. Исходные значения искомого документа не учитываются</param>
    public void PerformSearchSingle(DataFields SearchFieldNames, object[] SearchValues)
    {
      Int32[] Ids = PerformSearchAll(SearchFieldNames, SearchValues);
      if (Ids.Length == 1)
        Id = Ids[0];
      else
        Id = 0;
    }

  #endregion

  }

  public delegate void ServerDocTypeSearchDocEventHandler(object Sender,
    ServerDocTypeSearchDocEventArgs Args);

  #endregion

  #region ServerSubDocTypeSearchSubDocEventHandler

  public class ServerSubDocTypeSearchSubDocEventArgs : ServerDocTypeSearchDocEventArgs
  {
  #region Конструктор

    public ServerSubDocTypeSearchSubDocEventArgs(ServerSubDocType SubDocType,
      AccDepServerExec Caller, DataFields FieldNames, object[] FieldValues)
      : base(SubDocType.Parent, Caller, FieldNames, FieldValues)
    {
      FSubDocType = SubDocType;
#if DEBUG
      if (!FieldNames.Contains("DocId"))
        throw new BugException("Поле DocId отсутствует в списке полей для поиска");
      if (GetFieldAsInt("DocId") == 0)
        throw new BugException("Попытка поиска поддокумента без заданного идентификатора документа");
#endif
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Тип поддокумента
    /// </summary>
    public ServerSubDocType SubDocType { get { return FSubDocType; } }
    private ServerSubDocType FSubDocType;

    /// <summary>
    /// Идентификатор документа, в котором выполняется поиск
    /// Не может быть 0
    /// </summary>
    public Int32 DocId
    {
      get { return GetFieldAsInt("DocId"); }
    }

  #endregion

  #region Методы

    /// <summary>
    /// Получить просмотр для всех существующих поддокументов в текущем
    /// документе, чтобы можно было найти вручную
    /// </summary>
    /// <returns></returns>
    public DataView CreateSubDocsView()
    {
      DataOrder OrderBy;
      if (String.IsNullOrEmpty(FSubDocType.DefaultOrder))
        OrderBy = DataOrder.ValueId;
      else
        OrderBy = DataOrder.FromFieldNames(FSubDocType.DefaultOrder);
      DataTable Table = MainDB.FillSelect(FSubDocType.Name, null, new AndFilter(new ValueFilter("DocId", DocId),
        ValueFilter.DeletedFalse), OrderBy).Tables[0];
      return Table.DefaultView;
    }

  #endregion

  #region Переопределенные методы и свойства

    protected override void DoPerformSearch(DataFields FieldNames, object[] FieldValues, DataFields SearchFieldNames)
    {
      Id = SubDocType.DoPerformSearch(MainDB, FieldNames, FieldValues, SearchFieldNames);
    }

    protected override Int32[] DoPerformSearchAll(DataFields FieldNames, object[] FieldValues, DataFields SearchFieldNames)
    {
      return SubDocType.DoPerformSearchAll(MainDB, FieldNames, FieldValues, SearchFieldNames);
    }

    public override DataFields SearchFieldNames { get { return FSubDocType.SearchFieldNames; } }

  #endregion
  }

  public delegate void ServerSubDocTypeSearchSubDocEventHandler(object Sender,
    ServerSubDocTypeSearchSubDocEventArgs Args);

  #endregion

  #region ServerDocTypeReplaceFieldsEventHandler

  /// <summary>
  /// Причина вызова события ReplaceFields
  /// </summary>
  public enum ServerDocTypeReplaceFieldsReason
  {
    /// <summary>
    /// Импорт данных
    /// </summary>
    Import,

    /// <summary>
    /// Объединение документов
    /// </summary>
    Merge
  }

  /// <summary>
  /// Аргументы для события ServerDocType.ReplaceFields
  /// </summary>
  public class ServerDocTypeReplaceFieldsEventArgs : EventArgs
  {
  #region Конструктор

    public ServerDocTypeReplaceFieldsEventArgs(ServerDocType DocType, AccDepServerExec Caller, IDocValues SrcValues, IDocValues ResValues,
      ServerDocTypeReplaceFieldsReason Reason)
    {
      FDocType = DocType;
      FCaller = Caller;
      FSrcValues = SrcValues;
      FResValues = ResValues;
      FReason = Reason;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Описание вида основного документа
    /// </summary>
    public ServerDocType DocType { get { return FDocType; } }
    private ServerDocType FDocType;

    /// <summary>
    /// Вызывающая процедура для доступа к базе данных
    /// </summary>
    public AccDepServerExec Caller { get { return FCaller; } }
    private AccDepServerExec FCaller;

    /// <summary>
    /// Исходные значения из импортируемого документа или из удаляемого документа при
    /// объединении. Может содержать не все поля (в случае импорта) Обработчик должен
    /// проверять наличие каждого поля. 
    /// Значения доступны только для просмотра
    /// </summary>
    public IDocValues SrcValues { get { return FSrcValues; } }
    private IDocValues FSrcValues;

    /// <summary>
    /// Значения в найденном существующем документе при импорте или в остающемся
    /// документе при объединении.
    /// Обработчик может заменить значения некоторых полей в этом наборе.
    /// В отличие от SrcValues, этот набор полей всегда является полным.
    /// При вызове обработчика набор содержит исходные значения полей документа
    /// </summary>
    public IDocValues ResValues { get { return FResValues; } }
    private IDocValues FResValues;

    /// <summary>
    /// Причина вызова события: Импорт данных или объединение документов
    /// </summary>
    public ServerDocTypeReplaceFieldsReason Reason { get { return FReason; } }
    private ServerDocTypeReplaceFieldsReason FReason;

    /// <summary>
    /// Это свойство следует установить в true, если данные были изменены
    /// Методы ReplaceXxx выполняют установку этого свойства автоматически
    /// </summary>
    public bool Modified;

  #endregion

  #region Методы

    private bool GetFieldPos(string FieldName, out int pSrc, out int pRes)
    {
      pSrc = SrcValues.IndexOf(FieldName);
      pRes = ResValues.IndexOf(FieldName);
      return pSrc >= 0 && pRes >= 0;
    }

    /// <summary>
    /// Заменить значение поля FieldNames в наборе ResValues значением из SrcValues.
    /// Замена выполнеятся если выполнены все условия: 
    /// 1. Поле существует в наборе SrcValues.
    /// 2. Значение в SrcValues отлично от NULL
    /// 3. Значение в ResValues равно NULL
    /// </summary>
    /// <param name="FieldName">Имя заменямого поля</param>
    /// <returns>true, если замена выполнена</returns>
    public bool ReplaceNull(string FieldName)
    {
      int pSrc, pRes;
      if (!GetFieldPos(FieldName, out pSrc, out pRes))
        return false;

      object v = SrcValues.GetValue(pSrc);
      if (DataTools.IsEmptyValue(v))
        return false;
      if (!ResValues.IsNull(pRes))
        return false;
      ResValues.SetValue(pRes, v);
      Modified = true;
      return true;
    }

    /// <summary>
    /// Заменить значение поля FieldNames в наборе ResValues значением из SrcValues.
    /// Замена выполнеятся если поле существует в наборе SrcValues.
    /// Проверка значения не выполняется, поэтому существующее значение может быть
    /// затерто
    /// </summary>
    /// <param name="FieldName">Имя заменямого поля</param>
    /// <returns>true, если замена выполнена</returns>
    public bool ReplaceAnyway(string FieldName)
    {
      int pSrc, pRes;
      if (!GetFieldPos(FieldName, out pSrc, out pRes))
        return false;

      object v1 = SrcValues.GetValue(pSrc);
      object v2 = ResValues.GetValue(pRes);
      if (DataTools.IsEqualValues(v1, v2))
        return false;
      ResValues.SetValue(pRes, v1);
      Modified = true;
      return true;
    }

    public void IncInt(string FieldName)
    {
      int pSrc, pRes;
      if (GetFieldPos(FieldName, out pSrc, out pRes))
      {
        int s = SrcValues[pSrc].AsInteger + ResValues[pRes].AsInteger;
        if (s == 0)
          ResValues[pRes].SetNull();
        else
          ResValues[pRes].AsInteger = s;
      }
    }

    public void IncSingle(string FieldName)
    {
      int pSrc, pRes;
      if (GetFieldPos(FieldName, out pSrc, out pRes))
      {
        float s = SrcValues[pSrc].AsSingle + ResValues[pRes].AsSingle;
        if (s == 0f)
          ResValues[pRes].SetNull();
        else
          ResValues[pRes].AsSingle = s;
      }
    }

    public void IncDouble(string FieldName)
    {
      int pSrc, pRes;
      if (GetFieldPos(FieldName, out pSrc, out pRes))
      {
        double s = SrcValues[pSrc].AsDouble + ResValues[pRes].AsDouble;
        if (s == 0.0)
          ResValues[pRes].SetNull();
        else
          ResValues[pRes].AsDouble = s;
      }
    }

    public void IncDecimal(string FieldName)
    {
      int pSrc, pRes;
      if (GetFieldPos(FieldName, out pSrc, out pRes))
      {
        decimal s = SrcValues[pSrc].AsDecimal + ResValues[pRes].AsDecimal;
        if (s == 0m)
          ResValues[pRes].SetNull();
        else
          ResValues[pRes].AsDecimal = s;
      }
    }

  #endregion
  }

  public delegate void ServerDocTypeReplaceFieldsEventHandler(object Sender,
    ServerDocTypeReplaceFieldsEventArgs Args);

  #endregion

  #region ServerSubDocTypeReplaceFieldsEventHandler

  public class ServerSubDocTypeReplaceFieldsEventArgs : ServerDocTypeReplaceFieldsEventArgs
  {
  #region Конструктор

    public ServerSubDocTypeReplaceFieldsEventArgs(ServerSubDocType SubDocType, AccDepServerExec Caller, IDocValues SrcValues, IDocValues ResValues,
      ServerDocTypeReplaceFieldsReason Reason)
      : base(SubDocType.Parent, Caller, SrcValues, ResValues, Reason)
    {
      FSubDocType = SubDocType;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Описание вида поддокумента
    /// </summary>
    public ServerSubDocType SubDocType { get { return FSubDocType; } }
    private ServerSubDocType FSubDocType;

  #endregion
  }

  public delegate void ServerSubDocTypeReplaceFieldsEventHandler(object Sender,
    ServerSubDocTypeReplaceFieldsEventArgs Args);

  #endregion
#endif

  #region ServerDocTypeAutoCorrectEventHandler
#if XXX
  public class ServerDocTypeAutoCorrectEventArgs : EventArgs
  {
  #region Конструктор

    public ServerDocTypeAutoCorrectEventArgs(AccDepServerExec Caller, DataRow Row)
    {
      FCaller = Caller;
      FRow = Row;
    }

  #endregion

  #region Свойства

    public AccDepServerExec Caller { get { return FCaller; } }
    private AccDepServerExec FCaller;

    public DataRow Row { get { return FRow; } }
    private DataRow FRow;

    public IDocValues Values
    {
      get
      {
        if (FValues == null)
          FValues = new FixedDataRowDocValues(FRow);
        return FValues;
      }
    }
    private IDocValues FValues;

    /// <summary>
    /// Это свойство должно быть установлено в true, если автокоррекция была выполнена
    /// </summary>
    public bool Changed;

  #endregion
  }

  public delegate void ServerDocTypeAutoCorrectEventHandler(object Sender,
     ServerDocTypeAutoCorrectEventArgs Args);
#endif
  #endregion

  #region ServerDocTypeCheckUniqueEventHandler
#if XXX
  public class ServerDocTypeCheckUniqueEventArgs : EventArgs
  {
  #region Конструктор

    public ServerDocTypeCheckUniqueEventArgs(AccDepServerExec Caller)
    {
      FCaller = Caller;
    }

    public void Init(Int32 CurrId, IDocValues CurrValues, Int32 OtherId, IDocValues OtherValues)
    {
      FCurrId = CurrId;
      FCurrValues = CurrValues;
      FOtherId = OtherId;
      FOtherValues = OtherValues;
      Cancel = false;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Вызывающий объект. Может использоваться для получения других полей
    /// </summary>
    public AccDepServerExec Caller { get { return FCaller; } }
    private AccDepServerExec FCaller;

    /// <summary>
    /// Идентификатор текущего (проверяемого) документа
    /// </summary>
    public Int32 CurrId { get { return FCurrId; } }
    private Int32 FCurrId;

    /// <summary>
    /// Значения полей текущего (проверяемого) документа
    /// </summary>
    public IDocValues CurrValues { get { return FCurrValues; } }
    private IDocValues FCurrValues;

    /// <summary>
    /// Идентификатор текущего другого документа (с которым выполняется сравнение)
    /// </summary>
    public Int32 OtherId { get { return FOtherId; } }
    private Int32 FOtherId;

    /// <summary>
    /// Значения полей другого документа (с которым выполняется сравнение)
    /// </summary>
    public IDocValues OtherValues { get { return FOtherValues; } }
    private IDocValues FOtherValues;

    /// <summary>
    /// Если установить это свойство в true, то сообщение об ошибке выдаваться не будет
    /// </summary>
    public bool Cancel;

    /// <summary>
    /// Сюда можно поместить текст сообщения об ошибке, вместо стандартного
    /// (при Cancel=false)
    /// </summary>
    public string ErrorMessage;

  #endregion
  }

  public delegate void ServerDocTypeCheckUniqueEventHandler(object Sender,
    ServerDocTypeCheckUniqueEventArgs Args);

#endif
  #endregion

  #endregion


  /*
   * Объект DBxDocTypes создается и заполняется на сервере при запуске.
   * Вызываются методы DBxDocTypes.GetMainDBStruct() и GetUndoDBStruct() для получения структур основной базы
   * данных и базы данных undo. В этот момент описания переводятся в режим ReadOnly
   * Вызываются методы DBx.UpdateStruct() для обновления реальной структуры баз данных
   * 
   * Передача списка определений клиенту
   * -----------------------------------
   * Использование DBxDocTypes предполагает полное программное описание структуры баз данных, поэтому проблемы
   * отложенного извлечения описаний из базы данных нет. Следовательно, объекты DBxDocType (вместе с DBxTableStruct)
   * и DBxDocTypes являются сериализуемыми, а не Marshal-by-reference, и могут передаваться клиенту за один раз
   * Так как DBxStruct не является сериализуемым объектом, DBxDocTypes не содержит поля DBStruct. Полная 
   * структура базы данных может быть передана клиенту, если необходимо, отдельно. Или она может быть получена
   * на стороне клиента вызовом GetMainDBStruct()
   */

  /// <summary>
  /// Описание способов буферизации полей таблицы документа / поддокумента.
  /// Представляет собой набор флажков для полей из списка DBxDocTypeBase.Struct.
  /// На основании полей этого класса создается DBxTableCacheInfo.
  /// </summary>
  [Serializable]
  public sealed class DBxDocTypeIndividualCacheColumns
  {
    #region Конструктор

    internal DBxDocTypeIndividualCacheColumns(DBxDocTypeBase owner)
    {
      _Owner = owner;
      _Items = new Dictionary<string, bool>();
    }

    #endregion

    #region Свойства

    private DBxDocTypeBase _Owner;

    /// <summary>
    /// Чтение / запись флажка для столбца
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Флажок индивидуального кэширования</returns>
    public bool this[string columnName]
    {
      get
      {
        bool Res;
        if (_Items.TryGetValue(columnName, out Res))
          return Res;
        else
        {
          DBxColumnStruct Col = _Owner.Struct.Columns[columnName];
          if (Col == null)
          {
            if (String.IsNullOrEmpty(columnName))
              throw new ArgumentNullException("columnName");
            else
              throw new ArgumentException("Структура таблицы не содержит объявления поля \"" + columnName + "\"", "columnName");
          }
          return DBxTableCacheInfo.IsIndividualByDefault(Col);
        }
      }
      set
      {
        _Owner.CheckNotReadOnly();

        if (_Items.ContainsKey(columnName))
          _Items[columnName] = value;
        else
          _Items.Add(columnName, value); // можно было бы и проверить на значение по умолчанию
      }
    }

    private Dictionary<string, bool> _Items;

    /// <summary>
    /// Возвращает true, если не было установлено вручную ни одного флажка
    /// </summary>
    public bool AreAllDefaults { get { return _Items.Count == 0; } }

    #endregion
  }


  /// <summary>
  /// Базовый класс для DBxDocType и DBxSubDocType
  /// </summary>
  [Serializable]
  public abstract class DBxDocTypeBase : IObjectWithCode, IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает описание
    /// </summary>
    /// <param name="name">Имя документа или поддокумента</param>
    public DBxDocTypeBase(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      _Struct = new DBxTableStruct(name);
      _FileRefs = new DBxDocTypeFileRefs(_Struct);
      _BinDataRefs = new DBxDocTypeBinDataRefs(_Struct);
      _VTRefs = new DBxVTReferenceList(this);
      _CalculatedColumns = new DBxColumnList();
      _IndividualCacheColumns = new DBxDocTypeIndividualCacheColumns(this);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Имя вида документа.
    /// Совпадает с именем таблицы в базе данных
    /// </summary>
    public string Name { get { return Struct.TableName; } }

    /// <summary>
    /// Название одного документа ("Человек") для вывода пользователю
    /// </summary>
    public string SingularTitle
    {
      get
      {
        if (_SingularTitle == null)
          return "Объект \"" + PluralTitle + "\"";
        return _SingularTitle;
      }
      set
      {
        CheckNotReadOnly();
        _SingularTitle = value;
      }
    }
    private string _SingularTitle;

    /// <summary>
    /// Название нескольких документов ("Люди") для вывода пользователю
    /// </summary>
    public string PluralTitle
    {
      get
      {
        if (_PluralTitle == null)
          return Name;
        return _PluralTitle;
      }
      set
      {
        CheckNotReadOnly();
        _PluralTitle = value;
      }
    }
    private string _PluralTitle;

    /// <summary>
    /// Возвращает свойство Name
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Name;
    }

    /// <summary>
    /// True, если текущий объект DBxSubDocType.
    /// False, если DBxDocType
    /// </summary>
    public abstract bool IsSubDoc { get; }

    #endregion

    #region Древовидный просмотр

    /// <summary>
    /// Имя столбца (обычно, "ParentId"), используемое для организации древовидной структуры документов и поддокументов
    /// Если свойство не установлено (по умолчанию), древовидный просмотр не предусмотрен
    /// </summary>
    public string TreeParentColumnName
    {
      get { return _TreeParentColumnName; }
      set
      {
        CheckNotReadOnly();
        _TreeParentColumnName = value;
      }
    }
    private string _TreeParentColumnName;

    // Для текстового поля свойство не задается.
    // Вместо этого используется обычное текстовое представление документа/поддокумента

    #endregion

    #region Структура таблицы

    /// <summary>
    /// Структура таблицы документа
    /// Структура является неполной, т.к. не содержит объявлений служебных полей "Id", "Deleted" и прочих.
    /// В структуре есть поля, создаваемые при добавлении записей в коллекции VTRefs, FileRefs и BinDataRefs.
    /// Эти поля добавляются автоматически
    /// </summary>
    public DBxTableStruct Struct { get { return _Struct; } }
    private DBxTableStruct _Struct;

    /// <summary>
    /// Объявления переменных ссылок
    /// </summary>
    public DBxVTReferenceList VTRefs { get { return _VTRefs; } }
    private DBxVTReferenceList _VTRefs;

    /// <summary>
    /// Объявления полей, ссылающиеся на файлы, хранящиеся в базе данных
    /// </summary>
    public DBxDocTypeFileRefs FileRefs { get { return _FileRefs; } }
    private DBxDocTypeFileRefs _FileRefs;

    /// <summary>
    /// Объявляения полей, ссылающихся на двоичные файлы, хранящиеся в отдельной таблице
    /// </summary>
    public DBxDocTypeBinDataRefs BinDataRefs { get { return _BinDataRefs; } }
    private DBxDocTypeBinDataRefs _BinDataRefs;

    /// <summary>
    /// Порядок сортировки документов по умолчанию
    /// Если свойство не установлено, возвращается порядок сортировки по идентификатору Id
    /// </summary>
    public DBxOrder DefaultOrder
    {
      get
      {
        if (_DefaultOrder == null)
          return DBSDocType.OrderById;
        else
          return _DefaultOrder;
      }
      set
      {
        CheckNotReadOnly();
        _DefaultOrder = value;
      }
    }
    private DBxOrder _DefaultOrder;

    /// <summary>
    /// Список вычисляемых полей.
    /// В этот список поля должны добавляться вручную, после того, как они добавлены в список Struct
    /// </summary>
    public DBxColumnList CalculatedColumns { get { return _CalculatedColumns; } }
    private DBxColumnList _CalculatedColumns;

    /// <summary>
    /// Признаки индивидуальной буферизации значений полей.
    /// В этот список поля должны добавляться вручную, после того, как они добавлены в список Struct
    /// </summary>
    public DBxDocTypeIndividualCacheColumns IndividualCacheColumns { get { return _IndividualCacheColumns; } }
    private DBxDocTypeIndividualCacheColumns _IndividualCacheColumns;

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code { get { return Struct.TableName; } }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если описание находится в режиме "только чтение".
    /// Не имеет отношения к правам доступа к документам.
    /// </summary>
    public bool IsReadOnly { get { return Struct.IsReadOnly; } }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      Struct.CheckNotReadOnly();
    }

    internal void SetReadOnly()
    {
      _Struct.SetReadOnly();
      _FileRefs.SetReadOnly();
      _CalculatedColumns.SetReadOnly();
    }

    #endregion

    #region Проверка структуры

    internal void CheckStructAux(DBxTableStruct RealStruct)
    {
      // Проверяем структуру объявления таблицы
      // Вызывается в DBxDocTypes Struct.CheckStruct();
      string PK = RealStruct.CheckTablePrimaryKeyInt32();
      if (PK != "Id")
        throw new DBxDocTypeStructException("Описание таблицы \"" + RealStruct.TableName + "\" имеет неправильный первичный ключ по полю \"" + PK + "\", а не \"Id\"");

      // Проверяем список CalculatedColumns
      for (int i = 0; i < CalculatedColumns.Count; i++)
      {
        if (!RealStruct.Columns.Contains(CalculatedColumns[i]))
          throw new DBxDocTypeStructException("Описание таблицы \"" + RealStruct.TableName + "\" не содержит описания вычисляемого поля \"" + CalculatedColumns[i] + "\"");
      }
    }

    #endregion
  }

  /// <summary>
  /// Описание одного вида документов, хранящихся в базе данных
  /// </summary>
  [Serializable]
  public class DBxDocType : DBxDocTypeBase
  {
    #region Конструктор

    /// <summary>
    /// Создает описание документа
    /// </summary>
    /// <param name="name">Имя таблицы документа</param>
    public DBxDocType(string name)
      : base(name)
    {
      _SubDocs = new DBxSubDocTypes(this);
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Идентификатор таблицы документа. Используется для выполнения
    /// нечетких связей между таблицами, когда одна ссылка может выполняться
    /// на разные таблицы. Используется вместо имени таблицы для уменьшения
    /// размера базы данных
    /// Свойство доступно только после инициализации базы данных
    /// </summary>
    public Int32 TableId { get { return _TableId; } internal set { _TableId = value; } }
    private Int32 _TableId;

    /// <summary>
    /// Коллекция видов поддокументов для данного вида документов
    /// </summary>
    public DBxSubDocTypes SubDocs { get { return _SubDocs; } }
    private DBxSubDocTypes _SubDocs;

    /// <summary>
    /// Возвращает false
    /// </summary>
    public override bool IsSubDoc { get { return false; } }

    /// <summary>
    /// Возвращает true, если есть вычисляемые поля в документе или в поддокументах
    /// </summary>
    public bool HasCalculatedColumns
    {
      get
      {
        if (CalculatedColumns.Count > 0)
          return true;

        for (int i = 0; i < SubDocs.Count; i++)
        {
          if (SubDocs[i].CalculatedColumns.Count > 0)
            return true;
        }

        return false;
      }
    }

    /// <summary>
    /// Организация группировки документов.
    /// Свойство задает имя столбца (обычно, "GroupId"), который ссылается
    /// на отдельный вид документов, задающий дерево групп.
    /// По умолчанию свойство не установлено, так как группировка не используется
    /// </summary>
    public string GroupRefColumnName
    {
      get { return _GroupRefColumnName; }
      set
      {
        CheckNotReadOnly();
        _GroupRefColumnName = value;
      }
    }
    private string _GroupRefColumnName;

    #endregion

    #region SetReadOnly

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
      _SubDocs.SetReadOnly();
    }

    #endregion

    #region События сервера

#if XXX
    /// <summary>
    /// Члены класса event не могут быть помечены, как несериализуемые
    /// Храним серверные события как вложенный объект
    /// </summary>
    private class ServerEvents
    {
      public event ServerDocTypeBeforeInsertEventHandler BeforeInsert;
      public bool HasBeforeInsert { get { return BeforeInsert != null; } }
      public void OnBeforeInsert(object Sender, ServerDocTypeBeforeInsertEventArgs Args)
      {
        BeforeInsert(Sender, Args);
      }

      public event ServerDocTypeBeforeWriteEventHandler BeforeWrite;
      public bool HasBeforeWrite { get { return BeforeWrite != null; } }
      public void OnBefor

      public event ServerDocTypeAfterWriteEventHandler AfterWrite;
   }
#endif

    #region BeforeInsert

    /// <summary>
    /// Событие вызывается перед добавлением документа на стороне сервера и
    /// при воссановлении документа из удаленных.
    /// После этого события вызывается основное событие BeforeWrite.
    /// </summary>
    public event ServerDocTypeBeforeInsertEventHandler BeforeInsert
    {
      add
      {
        CheckNotReadOnly();
        _BeforeInsert += value;
      }
      remove
      {
        CheckNotReadOnly();
        _BeforeInsert -= value;
      }
    }
    [field: NonSerialized]
    private event ServerDocTypeBeforeInsertEventHandler _BeforeInsert;

    internal void PerformBeforeInsert(DBxSingleDoc doc, bool restoreDeleted)
    {
      if (_BeforeInsert == null)
        return;

      ServerDocTypeBeforeInsertEventArgs Args = new ServerDocTypeBeforeInsertEventArgs(doc, restoreDeleted);
      _BeforeInsert(this, Args);
    }

    #endregion

    #region BeforeWrite

    /// <summary>
    /// Вызывается непосредственно перед записью документа в базу данных
    /// Обработчик может, например, сгенерировать номер документа, или заполнить
    /// вычисляемые поля. Для этого следует изменить текущие значения, которые
    /// доступны через Args.Doc.Values
    /// Не вызывается перед удалением документа.
    /// </summary>
    public event ServerDocTypeBeforeWriteEventHandler BeforeWrite
    {
      add
      {
        CheckNotReadOnly();
        _BeforeWrite += value;
      }
      remove
      {
        CheckNotReadOnly();
        _BeforeWrite -= value;
      }
    }
    [field: NonSerialized]
    private event ServerDocTypeBeforeWriteEventHandler _BeforeWrite;

    internal void PerformBeforeWrite(DBxSingleDoc doc, bool append, bool recalcColumnsOnly)
    {
      if (_BeforeWrite == null)
        return;

      try
      {
        ServerDocTypeBeforeWriteEventArgs Args = new ServerDocTypeBeforeWriteEventArgs(doc, append, recalcColumnsOnly);
        _BeforeWrite(this, Args);
      }
      catch (Exception e)
      {
        e.Data["DocType"] = this.Name;
        e.Data["DocId"] = doc.DocId;
        e.Data["DocState"] = doc.DocState;
        e.Data["PerformBeforeWrite.Append"] = append;
        e.Data["PerformBeforeWrite.RecalcColumnsOnly"] = recalcColumnsOnly;
        throw;
      }
    }

    #endregion

    #region AfterWrite

    /// <summary>
    /// Вызывается, после того как документ был сохранен.
    /// Событие может использоваться для проверки связанных документов. При необходимости
    /// может быть выполнена установка обратных связей.
    /// В обработчике нельзя изменять значения полей этого документа
    /// 08.12.2014
    /// Событие вызывается после выхода из транзакции и снятия блокировки записи. Возможен асинхронный вызов события из разных процедур
    /// </summary>
    public event ServerDocTypeAfterWriteEventHandler AfterWrite
    {
      add
      {
        CheckNotReadOnly();
        _AfterWrite += value;
      }
      remove
      {
        CheckNotReadOnly();
        _AfterWrite -= value;
      }
    }
    [field: NonSerialized]
    private event ServerDocTypeAfterWriteEventHandler _AfterWrite;

    internal void PerformAfterWrite(DBxSingleDoc doc)
    {
      if (_AfterWrite == null)
        return;

      ServerDocTypeAfterWriteEventArgs Args = new ServerDocTypeAfterWriteEventArgs(doc);
      _AfterWrite(this, Args);
    }

    #endregion

#if XXX
    /// <summary>
    /// Вызывается для автоматического исправления ошибок в основной строке 
    /// DataRow документа
    /// Не имеет доступа к поддокументам
    /// </summary>
    public event ServerDocTypeAutoCorrectEventHandler AutoCorrect;

    public bool PerformAutoCorrect(AccDepServerExec Caller, DataRow Row)
    {
      if (AutoCorrect == null)
        return false;
      ServerDocTypeAutoCorrectEventArgs Args = new ServerDocTypeAutoCorrectEventArgs(Caller, Row);
      Args.Changed = false;
      AutoCorrect(this, Args);
      return Args.Changed;
    }

    /// <summary>
    /// Вызывается перед удалением документа. Может выполнить проверку допустимости
    /// удаления и выкинуть исключение, если удалять нельзя
    /// </summary>
    public event ServerDocTypeBeforeDeleteEventHandler BeforeDelete;

    internal void PerformBeforeDelete(MultiDocsChangesInfo DocsChanges, int DocIndex)
    {
      if (BeforeDelete == null)
        return;

      ServerDocTypeBeforeDeleteEventArgs Args = new ServerDocTypeBeforeDeleteEventArgs(DocsChanges, DocIndex);
      BeforeDelete(this, Args);
    }
#endif

    #region BeforeDelete

    /// <summary>
    /// Событие вызывается перед удалением документа на стороне сервера
    /// </summary>
    public event ServerDocTypeBeforeDeleteEventHandler BeforeDelete
    {
      add
      {
        CheckNotReadOnly();
        _BeforeDelete += value;
      }
      remove
      {
        CheckNotReadOnly();
        _BeforeDelete -= value;
      }
    }
    [field: NonSerialized]
    private event ServerDocTypeBeforeDeleteEventHandler _BeforeDelete;

    internal void PerformBeforeDelete(DBxSingleDoc doc)
    {
      if (_BeforeDelete == null)
        return;

      ServerDocTypeBeforeDeleteEventArgs Args = new ServerDocTypeBeforeDeleteEventArgs(doc);
      _BeforeDelete(this, Args);
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Коллекция описаний видов документов
  /// </summary>
  [Serializable]
  public class DBxDocTypes : NamedList<DBxDocType>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список описаний
    /// </summary>
    public DBxDocTypes()
    {
      _UsersTableName = "Users";
      _ActionInfoMaxLength = 60;
      _UseDeleted = true;
      _UseVersions = true;
      _UseTime = true;
    }

    #endregion

    #region Специальные имена и размеры полей

    /// <summary>
    /// Имя таблицы "Users".
    /// По умолчанию задает имя таблицы "Users".
    /// Если свойство установлено равным пустой строке, то в документах не будет служебных полей
    /// "CreateUserId", и "ChangeUserId". При этом, однако, может вестить история изменения документов
    /// </summary>
    public string UsersTableName
    {
      get { return _UsersTableName; }
      set
      {
        //if (String.IsNullOrEmpty(value))
        //  throw new ArgumentNullException();  // 06.12.2017 - может быть
        CheckNotReadOnly();
        if (value == null)
          _UsersTableName = String.Empty;
        else
          _UsersTableName = value;
      }
    }
    private string _UsersTableName;

    /// <summary>
    /// Возвращает true, если используются идентификаторы пользователей
    /// </summary>
    public bool UseUsers { get { return _UsersTableName.Length > 0; } }



    /// <summary>
    /// Длина поля "ActionInfo" в таблице "UserActions".
    /// По умолчанию - 60 символов
    /// </summary>
    public int ActionInfoMaxLength
    {
      get { return _ActionInfoMaxLength; }
      set
      {
        CheckNotReadOnly();
        if (value < 30 || value > 255)
          throw new ArgumentOutOfRangeException();
        _ActionInfoMaxLength = value;
      }
    }
    private int _ActionInfoMaxLength;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то при удалении документа он не удаляется из базы
    /// данных, а только помечается установкой служебного поля "Deleted".
    /// Если свойство сброшено в false, то служебного поля нет, а документ удаляется из базы данных
    /// командой DELETE FROM без возможности восстановления. 
    /// Этот режим несовместим с ведением истории.
    /// </summary>
    public bool UseDeleted
    {
      get { return _UseDeleted; }
      set
      {
        CheckNotReadOnly();
        _UseDeleted = value;
      }
    }
    private bool _UseDeleted;

    /// <summary>
    /// Если true (по умолчанию), то в документах присутствует служебное поле Version, которое
    /// имеет значение 1 при создании документа, а затем увеличивается при изменениях.
    /// Если свойство сброшено в false, то служебного поля нет.
    /// Этот режим несовместим с ведением истории.
    /// </summary>
    public bool UseVersions
    {
      get { return _UseVersions; }
      set
      {
        CheckNotReadOnly();
        _UseVersions = value;
      }
    }
    private bool _UseVersions;

    /// <summary>
    /// Если true (по умолчанию), то в документах присутствуют служебные поля CreateTime и ChangeTime, 
    /// которые хранят время создания и последнего изменения документв.
    /// Если свойство сброшено в false, то служебных полей нет.
    /// Этот режим несовместим с ведением истории.
    /// </summary>
    public bool UseTime
    {
      get { return _UseTime; }
      set
      {
        CheckNotReadOnly();
        _UseTime = value;
      }
    }
    private bool _UseTime;

    /// <summary>
    /// Если свойство установить в true, то в таблице UserActions будет добавлено числовое поле "SessionId".
    /// Это поле можно использовать при просмотре истории документа вместе или вместо поля "UserId"
    /// для учета сеансов работы клиента.
    /// В отличие от "UserId", для поля не "SessionId" в ExtDBDocs не обеспечивается ссылочная целостность.
    /// Ведение таблице сеансов клиентов возлагается на программу.
    /// Также, сеанс работы не записывается в основую базу данных, а есть только в UserActions,
    /// поэтому установка свойства допускается, если ведется база данных истории, то есть при
    /// UseDeleted=true, UseVersions=true и UseTime=true.
    /// По умолчанию свойство UseSessionId=false.
    /// </summary>
    public bool UseSessionId
    {
      get { return _UseSessionId; /* 25.12.2020 */}
      set
      {
        CheckNotReadOnly();
        _UseSessionId = value;
      }
    }
    private bool _UseSessionId;

    #endregion

    #region Группы документов

    /// <summary>
    /// Возвращает массив описаний видов документов-деревьев групп, на которые есть ссылки в DBxDocType.GroupRefColumnName.
    /// Массив каждый раз собирается заново.
    /// В случае несогласованности объявлений возникает исключение.
    /// </summary>
    /// <returns>Массив описаний документов</returns>
    public DBxDocType[] GetGroupDocTypes()
    {
      SingleScopeList<DBxDocType> List = new SingleScopeList<DBxDocType>();
      foreach (DBxDocType dt in this)
      {
        DBxDocType dt2 = GetGroupDocType(dt);
        if (dt2 != null)
          List.Add(dt2);
      }

      return List.ToArray();
    }

    /// <summary>
    /// Возвращает массив имен видов документов-деревьев групп, на которые есть ссылки в DBxDocType.GroupRefColumnName.
    /// Массив каждый раз собирается заново.
    /// В случае несогласованности объявлений возникает исключение.
    /// </summary>
    /// <returns>Массив имен видов документов</returns>
    public string[] GetGroupDocTypeNames()
    {
      SingleScopeList<string> List = new SingleScopeList<string>();
      foreach (DBxDocType dt in this)
      {
        DBxDocType dt2 = GetGroupDocType(dt);
        if (dt2 != null)
          List.Add(dt2.Name);
      }

      return List.ToArray();
    }

    /// <summary>
    /// Возвращает описание вида документов-дерева групп для заданного основного вида документов.
    /// Если вид документа не использует группы, возвращается null
    /// </summary>
    /// <param name="docType">Основной вид документо</param>
    /// <returns>Вид документов дерева групп</returns>
    public DBxDocType GetGroupDocType(DBxDocType docType)
    {
      if (docType == null)
        return null;
      if (String.IsNullOrEmpty(docType.GroupRefColumnName))
        return null;

      DBxColumnStruct ColDef = docType.Struct.Columns[docType.GroupRefColumnName];
      if (ColDef == null)
        throw new BugException("Неизвестное имя столбца \"" + docType.GroupRefColumnName + "\" в документе \"" + docType.Name + "\", которое задается свойством GroupRefColumnName");
      if (String.IsNullOrEmpty(ColDef.MasterTableName))
        throw new BugException("Столбец \"" + docType.GroupRefColumnName + "\" документа \"" + docType.Name + " не является ссылочным");
      DBxDocType dt2 = this[ColDef.MasterTableName];
      if (dt2 == null)
        throw new BugException("Неизвестный вид документов \"" + ColDef.MasterTableName + "\" в документе \"" + docType.Name + "\" для группировки");
      return dt2;
    }


    #endregion

    #region Получение структуры базы данных

    /// <summary>
    /// Получение структуры основной базы данных.
    /// Устанавливает IsReadOnly=true.
    /// </summary>
    /// <returns>Структура базы данных, включая служебные таблицы и поля</returns>
    public DBxStruct GetMainDBStruct()
    {
      SetReadOnly(); // также добавляет объявления системных таблиц

      DBxStruct dbs = new DBxStruct();

      // Не-документные таблицы
      DBxTableStruct ts = new DBxTableStruct("DocTables");
      ts.Columns.AddId();
      ts.Columns.AddString("DocTableName", 64, false);
      ts.Indices.Add("DocTableName");
      dbs.Tables.Add(ts);

      foreach (DBxDocType dt in this)
      {
        #region Документ

        ts = new DBxTableStruct(dt.Name);

        // Обязательные столбцы
        ts.Columns.AddId(DBSDocType.Id);
        if (UseVersions)
        {
          ts.Columns.AddInt(DBSDocType.Version, 0, short.MaxValue);
          ts.Columns.AddInt(DBSDocType.Version2, 0, short.MaxValue);
        }
        if (UseDeleted)
          ts.Columns.AddBoolean(DBSDocType.Deleted);
        if (UseTime)
          ts.Columns.AddDateTime(DBSDocType.CreateTime, true);
        if (UseUsers)
          ts.Columns.AddReference(DBSDocType.CreateUserId, UsersTableName, true, DBxRefType.Disallow);
        if (UseTime)
          ts.Columns.AddDateTime(DBSDocType.ChangeTime, true);
        if (UseUsers)
          ts.Columns.AddReference(DBSDocType.ChangeUserId, UsersTableName, true, DBxRefType.Disallow);

        // Копируем прикладные столбцы
        foreach (DBxColumnStruct cs in dt.Struct.Columns)
          ts.Columns.Add(cs.Clone());

        // Копируем индексы
        foreach (DBxIndexStruct index in dt.Struct.Indices)
          ts.Indices.Add(index.Clone()); // 20.08.2020

        dbs.Tables.Add(ts);

        #endregion

        #region Поддокументы

        foreach (DBxSubDocType sdt in dt.SubDocs)
        {
          ts = new DBxTableStruct(sdt.Name);

          // Обязательные столбцы
          ts.Columns.AddId(DBSSubDocType.Id);
          ts.Columns.AddReference(DBSSubDocType.DocId, dt.Name, false);
          if (UseDeleted)
            ts.Columns.AddBoolean(DBSSubDocType.Deleted);
          if (UseVersions)
          {
            ts.Columns.AddInt(DBSSubDocType.StartVersion, 0, short.MaxValue);
            ts.Columns.AddInt(DBSSubDocType.Version2, 0, short.MaxValue);
          }

          // Копируем прикладные столбцы
          foreach (DBxColumnStruct cs in sdt.Struct.Columns)
            ts.Columns.Add(cs.Clone());

          // Копируем индексы
          foreach (DBxIndexStruct index in sdt.Struct.Indices)
            ts.Indices.Add(index.Clone()); // 20.08.2020

          dbs.Tables.Add(ts);
        }

        #endregion
      }

      return dbs;
    }

    /// <summary>
    /// Получение структуры базы данных Undo.
    /// Устанавливает IsReadOnly=true.
    /// Если какое-либо из свойств UseDeleted, UserVersions или UseTime,
    /// метод возвращает null, т.к. ведение истории невозможно
    /// </summary>
    /// <returns>Структура базы данных, включая служебные таблицы и поля, или null</returns>
    public DBxStruct GetUndoDBStruct()
    {
      SetReadOnly(); // также добавляет объявления системных таблиц

      if (UseDeleted && UseVersions && UseTime)
      {
        DBxStruct dbs = new DBxStruct();

        // Не-документные таблицы
        DBxTableStruct ts = new DBxTableStruct("UserActions");
        ts.Columns.AddId();
        if (UseUsers)
          ts.Columns.AddInt("UserId"); // псевдоссылочное поле на таблицу "Пользователи"
        if (UseSessionId)
          ts.Columns.AddInt("SessionId"); // псевдоссылочное поле "в никуда". Таблица сессий может реализовываться в пользовательском коде
        ts.Columns.AddDateTime("StartTime", true); // 22.11.2018 Время начала редактирования
        ts.Columns.AddDateTime("ActionTime", false); // Время первого вызова ApplyChanges()
        ts.Columns.AddString("ActionInfo", ActionInfoMaxLength, false);
        ts.Columns.AddInt("ApplyChangesCount", 0, short.MaxValue); // 22.12.2016 Количество вызовов ApplyChanges() (1,2,3 ...)
        ts.Columns.AddDateTime("ApplyChangesTime", true); // 22.12.2016 Время последнего вызова ApplyChanges()
        ts.Indices.Add(new DBxColumns("ActionTime")); // 15.01.2017
        if (UseUsers)
          ts.Indices.Add(new DBxColumns("UserId,ActionTime")); // 15.04.2019
        dbs.Tables.Add(ts);

        ts = new DBxTableStruct("DocActions");
        ts.Columns.AddId();
        ts.Columns.AddReference("UserActionId", "UserActions", true, DBxRefType.Delete);
        ts.Columns.AddInt("DocTableId"); // псевдоссылочное поле на таблицу "DocTables"
        ts.Columns.AddInt("DocId"); // псевдоссылочное поле на прикладную таблицу в db.mdb
        ts.Columns.AddInt("Version", 0, short.MaxValue);
        ts.Columns.AddInt("Action", 0, 127);
        ts.Indices.Add("DocTableId,DocId,UserActionId");
        dbs.Tables.Add(ts);

        foreach (DBxDocType dt in this)
        {
          #region Документ

          ts = new DBxTableStruct(dt.Name);

          // Обязательные столбцы
          ts.Columns.AddId();
          ts.Columns.AddInt("DocId"); // псевдоссылочное поле на прикладную таблицу в db.mdb
          ts.Columns.AddInt("Version2", 0, short.MaxValue);

          // Копируем прикладные столбцы
          foreach (DBxColumnStruct cs in dt.Struct.Columns)
            ts.Columns.Add(CloneWithoutRef(cs));
          ts.Indices.Add("DocId,Version2");
          // Пользовательские индексы не нужны

          dbs.Tables.Add(ts);

          #endregion

          #region Поддокументы

          foreach (DBxSubDocType sdt in dt.SubDocs)
          {
            ts = new DBxTableStruct(sdt.Name);

            // Обязательные столбцы
            ts.Columns.AddId();
            ts.Columns.AddInt("SubDocId"); // псевдоссылочное поле на прикладную таблицу в db.mdb
            ts.Columns.AddInt("Version2", 0, short.MaxValue);
            ts.Columns.AddBoolean("Deleted");

            // Копируем прикладные столбцы
            foreach (DBxColumnStruct cs in sdt.Struct.Columns)
              ts.Columns.Add(CloneWithoutRef(cs));
            ts.Indices.Add("SubDocId,Version2");
            // Пользовательские индексы не нужны

            dbs.Tables.Add(ts);
          }

          #endregion
        }

        return dbs;
      }
      else
        return null;
    }

    /// <summary>
    /// Клонирование столбца для undo
    /// Убираем ссылки
    /// </summary>
    /// <param name="cs"></param>
    /// <returns></returns>
    private static DBxColumnStruct CloneWithoutRef(DBxColumnStruct cs)
    {
      DBxColumnStruct Res = cs.Clone();
      Res.MasterTableName = String.Empty;
      return Res;
    }

    private void GetReadySystemDocTypes()
    {
      if (!String.IsNullOrEmpty(UsersTableName))
      {
        if (!base.Contains(UsersTableName))
        {
          DBxDocType dtUsers = new DBxDocType(UsersTableName);
          dtUsers.Struct.Columns.AddString("Name", 20, false);
          base.Insert(0, dtUsers);
        }
      }
    }

    #endregion

    #region SetReadOnly

    /// <summary>
    /// Переводит список описаний в режим "только чтение".
    /// Этот режим не имеет отношения к правам доступа к документам.
    /// Метод нет необходимости вызывать в явном виде из пользовательского кода.
    /// Он вызывается автоматически в конструкторе DBxRealDocProviderGlobal, а также
    /// в методах DBxDocTypes.GetMainDBStruct() и GetUndoDBStruct().
    /// </summary>
    public new void SetReadOnly()
    {
      if (IsReadOnly)
        return;

      GetReadySystemDocTypes();

      foreach (DBxDocType dt in this)
        dt.SetReadOnly();

      base.SetReadOnly();
    }

    #endregion

    #region Дополнительные методы и свойства

    #region FindByTableId

    /// <summary>
    /// Поиск описания документа по идентификатору в таблице "DocTypes".
    /// В текущей реализации, только описания документов, но не поддокументов, имеют идентификаторы.
    /// Если задан недействительный идентфикатор, возвращается null.
    /// </summary>
    /// <param name="tableId">Идентификатор в таблице "DocTypes"</param>
    /// <returns>Описание документа или null</returns>
    public DBxDocType FindByTableId(Int32 tableId)
    {
      if (tableId == 0)
        return null;

      #region С использованием буферизации

      if (IsReadOnly)
      {
        Dictionary<Int32, DBxDocType> Dict = GetTableIdDict();
        DBxDocType DocType;
        if (Dict.TryGetValue(tableId, out DocType))
          return DocType;
        else
          return null;
      }

      #endregion

      #region Без использования буферизации

      for (int i = 0; i < Count; i++)
      {
        if (this[i].TableId == 0)
          throw new InvalidOperationException("Поиск по идентифиатору TableId возможен только после обновления структуры базы данных. До этого момента свойство DBxDocType.TableId не определено");
        if (this[i].TableId == tableId)
          return this[i];
      }

      return null;

      #endregion
    }

    /// <summary>
    /// Используем буферизацию после того, как свойство IsReadOnly установлено в true.
    /// До этого применяется перебор документов и поддокументов.
    /// Для ицициализации "по требованию" используется "метод двойной проверки" ("double checking")
    /// </summary>
    /// <remarks>
    /// Можно либо использовать volatile, либо устанавливать значение с помощью Interlocked.Exchange().
    /// 
    /// См.: Джеффри Рихтер. CLR via C#. Программирование на платформе Microsoft .NET Framework 4.5 на языке C#. — 4-е изд. — СПб.: Питер, 2013. — 896 с. — ISBN 978-5-496-00433-6
    /// гл.29, раздел "Запирание с двойной проверкой"
    /// </remarks>
    [NonSerialized]
    private volatile Dictionary<Int32, DBxDocType> _TableIdDict;

    private Dictionary<Int32, DBxDocType> GetTableIdDict()
    {
      if (_TableIdDict != null)
        return _TableIdDict; // без блокировки

      lock (_SyncRoot)
      {
        if (_TableIdDict == null)
        {
          // Присвоим в самом конце
          Dictionary<Int32, DBxDocType> Dict = new Dictionary<Int32, DBxDocType>();

          for (int i = 0; i < Count; i++)
            Dict.Add(this[i].TableId, this[i]);

          //Interlocked.Exchange(ref _TableIdDict, Dict);
          _TableIdDict = Dict; // 05.01.2021
        }
      }

      return _TableIdDict;
    }

    #endregion

    #region FindByTableName

    /// <summary>
    /// Поиск документа или поддокумента по имени таблицы.
    /// Эта версия метода возвращает описание и документа и поддокумента. Если предполагается использовать
    /// обобщенный класс DBxDocTypeBase, используйте другие перегрузки метода.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="docType">Сюда помещается описание документа, если описание найдено.
    /// Если таблица соответствует поддокумеенту, то описание документа-владельца</param>
    /// <param name="subDocType">Сюда помещается описание документа, если описание найдено, и
    /// таблица соответствует поддокументу. Если таблица соответствует документу, то сюда записывается null</param>
    /// <returns>true, если описание найдено</returns>
    public bool FindByTableName(string tableName, out DBxDocType docType, out DBxSubDocType subDocType)
    {
      docType = null;
      subDocType = null;

      #region С использованием буферизации

      if (IsReadOnly)
      {
        Dictionary<string, DBxDocTypeBase> Dict = GetTableNameDict();
        DBxDocTypeBase dtb;
        if (Dict.TryGetValue(tableName, out dtb))
        {
          if (dtb.IsSubDoc)
          {
            subDocType = (DBxSubDocType)dtb;
            docType = subDocType.DocType;
#if DEBUG
            if (docType == null)
              throw new BugException("Потеряли DocType");
#endif
          }
          else
            docType = (DBxDocType)dtb;
        }
        else
          return false;
      }

      #endregion

      #region Без использования буферизации

      for (int i = 0; i < Count; i++)
      {
        if (this[i].Name == tableName)
        {
          docType = this[i];
          return true;
        }

        for (int j = 0; j < this[i].SubDocs.Count; j++)
        {
          if (this[i].SubDocs[j].Name == tableName)
          {
            docType = this[i];
            subDocType = this[i].SubDocs[j];
            return true;
          }
        }
      }

      return false;

      #endregion
    }

    /// <summary>
    /// Используем буферизацию после того, как свойство IsReadOnly установлено в true.
    /// До этого применяется перебор документов и поддокументов.
    /// Для ицициализации "по требованию" используется "метод двойной проверки" ("double checking")
    /// </summary>
    /// <remarks>
    /// См. примечание к TableIdDict
    /// </remarks>
    [NonSerialized]
    private volatile Dictionary<string, DBxDocTypeBase> _TableNameDict;

    /// <summary>
    /// Используется для реализации "double checking"
    /// </summary>
    private static readonly object _SyncRoot = new object();

    private Dictionary<string, DBxDocTypeBase> GetTableNameDict()
    {
      if (_TableNameDict != null)
        return _TableNameDict; // без блокировки

      lock (_SyncRoot)
      {
        if (_TableNameDict == null)
        {
          // Присвоим в самом конце
          Dictionary<string, DBxDocTypeBase> Dict = new Dictionary<string, DBxDocTypeBase>();

          for (int i = 0; i < Count; i++)
          {
            Dict.Add(this[i].Name, this[i]);

            for (int j = 0; j < this[i].SubDocs.Count; j++)
              Dict.Add(this[i].SubDocs[j].Name, this[i].SubDocs[j]);
          }

          //Interlocked.Exchange(ref _TableNameDict, Dict);
          _TableNameDict = Dict; // 05.01.2021
        }
      }

      return _TableNameDict;
    }

    /// <summary>
    /// Поиск документа или поддокумента по имени таблицы.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="docTypeBase">Сюда записывается описание документа или поддокумента</param>
    /// <returns>true, если описание найдено</returns>
    public bool FindByTableName(string tableName, out DBxDocTypeBase docTypeBase)
    {
      #region С использованием буферизации

      if (IsReadOnly)
      {
        Dictionary<string, DBxDocTypeBase> Dict = GetTableNameDict();
        return Dict.TryGetValue(tableName, out docTypeBase);
      }

      #endregion

      #region Без использования буферизации

      DBxDocType DocType;
      DBxSubDocType SubDocType;
      if (FindByTableName(tableName, out DocType, out SubDocType))
      {
        if (SubDocType == null)
          docTypeBase = DocType;
        else
          docTypeBase = SubDocType;
        return true;
      }
      else
      {
        docTypeBase = null;
        return false;
      }

      #endregion
    }

    /// <summary>
    /// Поиск документа или поддокумента по имени таблицы.
    /// Если имя таблицы не соответствует ни одному описанию, возвращается null
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Описание или null</returns>
    public DBxDocTypeBase FindByTableName(string tableName)
    {
      DBxDocTypeBase DocTypeBase;
      if (FindByTableName(tableName, out DocTypeBase))
        return DocTypeBase;
      else
        return null;
    }

    #endregion

    /// <summary>
    /// Возвращает имена всех объявленных документов в виде массива.
    /// Имена документов совпадают с именами таблицы в базе данных.
    /// Имена поддокументов не возвращаются.
    /// При каждом вызове создается новая копия массива
    /// </summary>
    /// <returns>Массив имен таблиц</returns>
    public string[] GetDocTypeNames()
    {
      return base.GetCodes();
    }

    /// <summary>
    /// Возвращает имя таблицы документа по идентификатору таблицы.
    /// Возвращает пустую строку, если идентификатор таблицы не найден.
    /// Этот метод может быть удобнее, чем FindByTableId(), если дальше идет выбор действия, в зависимости
    /// от вида документа
    /// </summary>
    /// <param name="tableId">Идентификатор таблицы документа</param>
    /// <returns>Имя таблицы или пустая строка</returns>
    public string GetTableNameById(Int32 tableId)
    {
      DBxDocType dt = FindByTableId(tableId);
      if (dt == null)
        return String.Empty;
      else
        return dt.Name;
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Count=");
      sb.Append(Count.ToString());
      if (IsReadOnly)
        sb.Append(" (ReadOnly)");
      return sb.ToString();
    }

    /// <summary>
    /// Возвращает true, если хотя бы для одного документа или поддокумента есть ссылочное поле
    /// в BinDataRefs.
    /// </summary>
    /// <remarks>Получение значения свойства выполняет перебор всех документов</remarks>
    public bool HasBinDataRefs
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].BinDataRefs.Count > 0)
            return true;
          DBxSubDocTypes sdts = this[i].SubDocs;
          for (int j = 0; j < sdts.Count; j++)
          {
            if (sdts[j].BinDataRefs.Count > 0)
              return true;
          }
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает true, если хотя бы для одного документа или поддокумента есть ссылочное поле
    /// в FileRefs.
    /// </summary>
    /// <remarks>Получение значения свойства выполняет перебор всех документов</remarks>
    public bool HasFileRefs
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].FileRefs.Count > 0)
            return true;
          DBxSubDocTypes sdts = this[i].SubDocs;
          for (int j = 0; j < sdts.Count; j++)
          {
            if (sdts[j].FileRefs.Count > 0)
              return true;
          }
        }
        return false;
      }
    }

    #endregion

    #region InitDocTableIds

    /// <summary>
    /// Инициализация значений DBxDocType.TableIds.
    /// Обычно этот метод вызывается из класса DBxConnectionHelper.
    /// </summary>
    /// <param name="mainEntry">Точка входя в основную базу данных</param>
    public void InitDocTableIds(DBxEntry mainEntry)
    {
#if DEBUG
      if (mainEntry == null)
        throw new ArgumentNullException("mainEntry");
#endif

      using (DBxCon Con = new DBxCon(mainEntry))
      {
        #region 1. Загружаем существующие номера

        DataTable SrcTable = Con.FillSelect("DocTables");

        foreach (DataRow SrcRow in SrcTable.Rows)
        {
          // Int32 Id = (int)SrcRow["Id"];
          Int32 Id = DataTools.GetInt(SrcRow, "Id"); // 06.12.2017
          string TableName = DataTools.GetString(SrcRow, "DocTableName");
          if (this.Contains(TableName))
          {
            DBxDocType dt = this[TableName];
            if (dt.TableId != 0)
              throw new BugException("Ошибка в таблице DocTables. Повторное вхождение документа \"" + TableName + "\"");
            dt.TableId = Id;
          }
        }

        #endregion

        #region 2. Определяем документы, у которых нет номеров. Для них добавляем строки

        foreach (DBxDocType dt in this)
        {
          if (dt.TableId == 0)
          {
            dt.TableId = Con.AddRecordWithIdResult("DocTables", "DocTableName", dt.Name);
          }
        }

        #endregion

        #region VTReferences
        // TODO:
        /*
// Инициализация ссылочных полей на переменные таблицы. Добавляем массивы
// идентификаторов таблиц для каждого имени master-таблицы
foreach (ServerDocType dt in dts)
{
InitTableVTRefs(dt.TableDef, dts);
foreach (ServerSubDocType sdt in dt.SubDocs)
  InitTableVTRefs(sdt.TableDef, dts);
}
          */
        #endregion
      }
    }

    #endregion

    #region Таблицы ссылок

    /// <summary>
    /// Возвращает массив описаний ссылок на заданный вид документов и его поддокументы
    /// </summary>
    /// <param name="docTypeName">Имя вида документов</param>
    /// <returns>Массив описаний ссылок</returns>
    public DBxDocTypeRefInfo[] GetToDocTypeRefs(string docTypeName)
    {
      if (IsReadOnly)
      {
        // Используем буферизацию
        DBxDocTypeRefInfo[] a;
        lock (_SyncRoot)
        {
          if (_BufToDocTypeRefs == null)
            _BufToDocTypeRefs = new Dictionary<string, DBxDocTypeRefInfo[]>();
          if (!_BufToDocTypeRefs.TryGetValue(docTypeName, out a))
          {
            a = DBxDocTypeRefInfo.GetToDocTypeRefs(this, docTypeName);
            _BufToDocTypeRefs.Add(docTypeName, a);
          }
        }
        return a;
      }
      else
        // Пока IsReadOnly не установлено, каждый раз создаем новый массив
        return DBxDocTypeRefInfo.GetToDocTypeRefs(this, docTypeName);
    }

    /// <summary>
    /// Буферизация значений, возвращаемых GetToDocTypeRefs()
    /// </summary>
    [NonSerialized]
    private Dictionary<string, DBxDocTypeRefInfo[]> _BufToDocTypeRefs;

    #endregion

    #region Проверка структруры

    /// <summary>
    /// Проверка корректности описаний документов и поддокументов.
    /// При обнаружении ошибки генерируется исключение DBxDocTypeStructException, DBxStructException или DBxPrimaryException
    /// Пользовательскому коду нет необходимости вызывать этот метод напрямую,
    /// так как проверка выполняется в конструкторе DBxRealDocProviderGlobal.
    /// </summary>
    public void CheckStruct()
    {
      // Собираем пседвоструктуру базы данных для проверки
      DBxStruct dbs = this.GetMainDBStruct();
      dbs.CheckStruct();

      foreach (DBxDocType dt in this)
      {
        dt.CheckStructAux(dbs.Tables[dt.Name]);
        foreach (DBxSubDocType sdt in dt.SubDocs)
          sdt.CheckStructAux(dbs.Tables[sdt.Name]);
      }

      if (!String.IsNullOrEmpty(UsersTableName))
      {
        if (this[UsersTableName] == null)
          throw new DBxDocTypeStructException("Свойство \"UsersTableName\" ссылается на несуществующую таблицу документов \"" + UsersTableName + "\"");
      }
    }

    #endregion
  }

  /// <summary>
  /// Описание одного вида поддокументов, хранящихся в базе данных
  /// </summary>
  [Serializable]
  public class DBxSubDocType : DBxDocTypeBase
  {
    #region Конструктор

    /// <summary>
    /// Создает описание поддокумента
    /// </summary>
    /// <param name="name">Имя таблицы поддокумента</param>
    public DBxSubDocType(string name)
      : base(name)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вид документа, к которому относится поддокумент
    /// Свойство становится доступным только после перевода списка описаний документов в режим просмотра
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } internal set { _DocType = value; } }
    private DBxDocType _DocType;

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool IsSubDoc { get { return true; } }

    #endregion
  }

  /// <summary>
  /// Список поддокументов
  /// Реализация свойства DBxDocType.SubDocs
  /// </summary>
  [Serializable]
  public class DBxSubDocTypes : NamedList<DBxSubDocType>
  {
    #region Защищенный конструктор

    internal DBxSubDocTypes(DBxDocType owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    private DBxDocType _Owner;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("DocType=");
      sb.Append(_Owner.Name);
      sb.Append("Count=");
      sb.Append(Count.ToString());
      if (IsReadOnly)
        sb.Append(" (ReadOnly)");
      return sb.ToString();
    }

    #endregion

    #region SetReadOnly

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
      for (int i = 0; i < Count; i++)
      {
        this[i].DocType = _Owner;
        this[i].SetReadOnly();
      }
    }

    #endregion
  }

  /// <summary>
  /// Это исключение выбрасывается при вызове DBxDocTypes.CheckStruct()
  /// </summary>
  [Serializable]
  public class DBxDocTypeStructException : Exception
  {
    #region Конструкторы

    /// <summary>
    /// Создает исключение
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    public DBxDocTypeStructException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Создает исключение
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    /// <param name="innerException">Вложенное исключение</param>
    public DBxDocTypeStructException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DBxDocTypeStructException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Имена полей и фильтры, используемых в таблицах документов
  /// </summary>
  public static class DBSDocType
  {
    #region Имена полей

    /// <summary>
    /// Имя поля идентификатора документа
    /// </summary>
    public const string Id = "Id";

    /// <summary>
    /// Имя поля версии документа
    /// </summary>
    public const string Version = "Version";

    /// <summary>
    /// Используется для внутренних целей
    /// </summary>
    public const string Version2 = "Version2";

    /// <summary>
    /// Имя логического поля для пометки удаленных документов
    /// </summary>
    public const string Deleted = "Deleted";

    /// <summary>
    /// Имя поля времени создания документа
    /// </summary>
    public const string CreateTime = "CreateTime";

    /// <summary>
    /// Имя ссылочного поля, содержащего идентфикатор пользователя, создавшего документ
    /// </summary>
    public const string CreateUserId = "CreateUserId";

    /// <summary>
    /// Имя поля времени последнего изменения документа.
    /// Если документ был только создан, но ни разу не изменялся, поле содержит NULL
    /// </summary>
    public const string ChangeTime = "ChangeTime";

    /// <summary>
    /// Имя ссылочного поля, содержащего идентфикатор пользователя, последним изменивший документ.
    /// Если документ был только создан, но ни разу не изменялся, поле содержит NULL.
    /// </summary>
    public const string ChangeUserId = "ChangeUserId";

    #endregion

    #region Объекты DBxColumns

    /// <summary>
    /// Список из одного столбца "Id"
    /// </summary>
    public static DBxColumns IdColumns { get { return DBxColumns.Id; } }

    #endregion

    #region Фильтры

    /// <summary>
    /// Фильтр по полю "Deleted" для отмены загрузки удаленных документов
    /// </summary>
    public static readonly ValueFilter DeletedFalseFilter = new ValueFilter("Deleted", false);

    #endregion

    #region Порядок сортировки

    /// <summary>
    /// Порядок сортировки по полю "Id"
    /// </summary>
    public static DBxOrder OrderById { get { return DBxOrder.ById; } }

    #endregion
  }

  /// <summary>
  /// Имена полей и фильтры, используемых в таблицах документов
  /// </summary>
  public static class DBSSubDocType
  {
    #region Имена полей

    /// <summary>
    /// Имя поля идентификатора поддокумента
    /// </summary>
    public const string Id = "Id";

    /// <summary>
    /// Имя поля идентификатора документа
    /// </summary>
    public const string DocId = "DocId";

    /// <summary>
    /// Имя логического поля для пометки удаленных поддокументов
    /// </summary>
    public const string Deleted = "Deleted";

    /// <summary>
    /// Используется для внутренних целей
    /// </summary>
    public const string StartVersion = "StartVersion";

    /// <summary>
    /// Используется для внутренних целей
    /// </summary>
    public const string Version2 = "Version2";

    #endregion

    #region Объекты DBxColumns

    /// <summary>
    /// Список из одного столбца "Id"
    /// </summary>
    public static DBxColumns IdColumns { get { return DBxColumns.Id; } }

    /// <summary>
    /// Список из одного столбца "DocId"
    /// </summary>
    public static readonly DBxColumns DocIdColumns = new DBxColumns("DocId");

    #endregion

    #region Фильтры

    /// <summary>
    /// Фильтр по полю "Deleted" для отмены загрузки удаленных поддокументов
    /// </summary>
    public static readonly ValueFilter DeletedFalseFilter = new ValueFilter("Deleted", false);

    /// <summary>
    /// Фильтр по полю "DocId.Deleted" для отмены загрузки поддокументов для удаленных документов
    /// </summary>
    public static readonly ValueFilter DocIdDeletedFalseFilter = new ValueFilter("DocId.Deleted", false);

    #endregion

    #region Порядок сортировки

    /// <summary>
    /// Порядок сортировки по полю "Id"
    /// </summary>
    public static DBxOrder OrderById { get { return DBxOrder.ById; } }

    #endregion
  }
}
