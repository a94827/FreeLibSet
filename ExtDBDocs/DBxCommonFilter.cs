using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using System.Data;
using FreeLibSet.Models.Tree;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Calendar;

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

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Базовый класс для реализации устанавливаемых фильтров.
  /// Фильтры используются в табличных просмотрах и параметрах отчетов
  /// </summary>
  public abstract class DBxCommonFilter : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр, установив свойство UseSqlFilter = true.
    /// </summary>
    public DBxCommonFilter()
    {
      _UseSqlFilter = true;
    }

    #endregion

    #region Код

    /// <summary>
    /// Код фильтра. Используется при чтении / записи фильтра как имя секции конфигурации.
    /// Свойство должно быть установлено в конструкторе производного класса или в пользовательском коде до присоединения к коллекции DBxCommonFilters
    /// </summary>
    public string Code
    {
      get { return _Code; }
      set
      {
        // Не проверяем пустое значение, так как код трьбуется только когда фильтр присоединяется к коллекции

        if (_Owner != null)
          throw new InvalidOperationException("Установка свойства Code допускается только до присоединения фильтра к коллекции");
        _Code = value;
      }
    }
    private string _Code;

    #endregion

    #region Коллекция - владелец

    /// <summary>
    /// Коллекция - владелец.
    /// Свойство возвращает null до присоединения фильтра к коллекции
    /// </summary>
    public DBxCommonFilters Owner
    {
      get { return _Owner; }
      internal set
      {
        if (value != null)
        {
          if (_Owner != null)
            throw new InvalidOperationException("Повторная установка свойства Owner не допускается");

          if (String.IsNullOrEmpty(_Code))
            throw new NullReferenceException("Свойство Code должно быть установлено до свойства Owner");
        }
        _Owner = value;
      }
    }
    private DBxCommonFilters _Owner;

    #endregion

    #region Событие Changed

    /// <summary>
    /// Событие вызывается при изменении фильтра в процессе редактирования таблички фильтров.
    /// Событие не вызывается до присоединения фильтра к коллекции.
    /// Не вызывается при изменении других фильтров в коллекции.
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// Этот метод вызывается классами-наследниками при изменении значения фильтра.
    /// Вызывает событие Changed для этого объекта и событие Changed коллекции DBxCommonFilters.
    /// До присоединения фильтра к коллекции метод не выполняет никаких действий, в том числе, не вызывает события Changed.
    /// </summary>
    protected virtual void OnChanged()
    {
      if (_Owner == null)
        return;

      if (Changed != null)
        Changed(this, EventArgs.Empty);

      _Owner.OnChanged(this);
    }

    #endregion

    #region Прочие методы и свойства

    /// <summary>
    /// Имя фильтра, которое появляется в левой части диалога фильтра.
    /// Если свойство не установлено в явном виде, возвращается Code.
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (!String.IsNullOrEmpty(_DisplayName))
          return _DisplayName;
        else if (!String.IsNullOrEmpty(Code))
          return Code;
        else
          return "[Без имени и кода]";
      }
      set { _DisplayName = value; }
    }
    private string _DisplayName;


    /// <summary>
    /// Идентификатор базы данных.
    /// Переопределено для RefDocGridFilter.
    /// Непереопределенный метод возвращает пустую строку.
    /// </summary>
    public virtual string DBIdentity { get { return String.Empty; } }

    /// <summary>
    /// Возвращает true, если фильтр используется при формировании SQL-запроса.
    /// По умолчанию возвращает true. Для фильтра табличного просмотра обязан возвращать true.
    /// Свойство может быть сброшено в false для фильтра отчета, если фильтрация выполняется по значению, вычисляемому вручную.
    /// В этом случае имя поля явуляется фиктивным
    /// </summary>
    public bool UseSqlFilter
    {
      get { return _UseSqlFilter; }
      set
      {
        if (_Owner != null)
          throw new InvalidOperationException("Установка свойства UseSqlFilter допускается только до присоединения фильтра к коллекции");
        _UseSqlFilter = value;
      }
    }
    private bool _UseSqlFilter;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Возвращает свойство DisplayName.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region Очистка фильтра

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public abstract bool IsEmpty { get; }

    #endregion

    #region Столбцы, SQL-фильтр и тестирование значения

    /// <summary>
    /// Получить список имен полей, которые необходимы для вычисления фильтра.
    /// Поля добавляются в список независимо от того, активен сейчас фильтр или нет.
    /// </summary>
    /// <param name="list">Список для добавления полей</param>
    public abstract void GetColumnNames(DBxColumnList list);

    /// <summary>
    /// Получение SQL-фильтра для фильтрации строк таблицы данных
    /// </summary>
    public abstract DBxFilter GetSqlFilter();

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля, иначе будет сгенерирована ошибка.
    /// Вызывает виртуальный метод OnTestValues(), если фильтр установлен и свойство UseSqlFilter равно true
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    public bool TestValues(INamedValuesAccess rowValues)
    {
      if (!UseSqlFilter)
        return true;
      if (IsEmpty)
        return true;
      return OnTestValues(rowValues);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля, иначе будет сгенерирована ошибка.
    /// Метод не вызывается, если фильтр не установлен, следовательно, проверка IsEmpty не нужна.
    /// Также метод не вызывается при UseSqlFilter=false
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected abstract bool OnTestValues(INamedValuesAccess rowValues);

    #endregion

    #region Чтение и запись секции конфигурации

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public abstract void ReadConfig(CfgPart cfg);

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public abstract void WriteConfig(CfgPart cfg);

    #endregion

    #region Инициализация строки документа

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Некоторые фильтры могут попытаться установить начальные значения для 
    /// полей документа в соответствии со своим состоянием.
    /// </summary>
    /// <param name="newDoc">Созданный документ, в котором можно установить поля</param>
    public void InitNewDocValues(DBxSingleDoc newDoc)
    {
      if (IsEmpty)
        return;
      if (!UseSqlFilter)
        return;
      OnInitNewDocValues(newDoc);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Некоторые фильтры могут попытаться установить начальные значения для 
    /// полей документа в соответствии со своим состоянием.
    /// </summary>
    /// <param name="newDoc">Созданный документ, в котором можно установить поля</param>
    protected virtual void OnInitNewDocValues(DBxSingleDoc newDoc)
    {
    }

    /// <summary>
    /// Если фильтр реализует установку значения фильтра "по строке", то 
    /// переопределенный метод должен извлечь значения "своих" полей из строки и
    /// вернуть true, если установка возможна. Сама установка выполняется методом
    /// SetAsCurrRow().
    /// Непереопределенный метод возвращает false.
    /// </summary>
    /// <param name="row">Строка, откуда берутся значения</param>
    /// <returns>Признак поддержки</returns>
    public virtual bool CanAsCurrRow(DataRow row)
    {
      return false;
    }

    /// <summary>
    /// Установка значения фильтра "по строке".
    /// Непереопределенный метод генерирует исключение.
    /// </summary>
    /// <param name="row">Строка, откуда берутся значения</param>
    public virtual void SetAsCurrRow(DataRow row)
    {
      throw new InvalidOperationException("Фильтр \"" + ToString() + "\" не поддерживает установку значения по текущей строке");
    }

    #endregion

    #region IInitNewDocValues Members

    /// <summary>
    /// Не реализовано
    /// </summary>
    /// <param name="savingDoc"></param>
    /// <param name="errorMessages"></param>
    public void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
    {
      // TODO:
#if XXX
      if (IsEmpty)
        return;
      DBxColumnList ColumnNames = new DBxColumnList();
      GetColumnNames(ColumnNames);

      object[] Values = new object[ColumnNames.Count];
      for (int i = 0; i < ColumnNames.Count; i++)
      {
        int p = SavingDoc.Values.IndexOf(ColumnNames[i]);
        if (p < 0)
          return;
        Values[i] = SavingDoc.Values[p].Value;
      }

      if (!TestValues(ColumnNames.AsArray, Values))
      {
        string[] StrValues = GetColumnStrValues(Values);
        StringBuilder sb = new StringBuilder();
        if (ColumnNames.Count == 1)
        {
          // Одно поле
          sb.Append("Значение поля \"");
          sb.Append(ColumnNames[0]);
          sb.Append("\" ");
          if (StrValues != null)
          {
            sb.Append("(");
            sb.Append(StrValues[0]);
            sb.Append(") ");
          }
          sb.Append("не соответствует");
        }
        else
        {
          sb.Append("Значения полей ");
          for (int i = 0; i < ColumnNames.Count; i++)
          {
            sb.Append("\"");
            sb.Append(ColumnNames[i]);
            sb.Append("\" ");
            if (StrValues != null)
            {
              sb.Append(" (");
              sb.Append(StrValues[i]);
              sb.Append(") ");
            }
            sb.Append("не соответствуют");
          }
        }
        sb.Append(" установленному фильтру \"");
        sb.Append(DisplayName);
        sb.Append("\" (");
        sb.Append(FilterText);
        sb.Append(")");
        ErrorMessages.AddWarning(sb.ToString());
      }
#endif
    }

    /// <summary>
    /// Переопределенный метод может вернуть текстовые представления тестируемых
    /// значений для отображения в сообщении
    /// Оригинальный метод возвращает null, что означает отказ от вывода значений
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected virtual string[] GetColumnStrValues(object[] columnValues)
    {
      return null;
    }

    #endregion
  }

  /// <summary>
  /// Набор из нескольких фильтров, которые можно вместе добавить в коллекцию DBxCommonFiltes
  /// </summary>
  public class DBxCommonFilterSet : NamedList<DBxCommonFilter>
  {
    #region Коллекция - владелец

    /// <summary>
    /// Коллекция - владелец.
    /// Свойство возвращает null до присоединения фильтра к коллекции
    /// </summary>
    internal DBxCommonFilters Owner
    {
      get { return _Owner; }
      set
      {
        if (value != null)
        {
          if (_Owner != null)
            throw new InvalidOperationException("Повторная установка свойства Owner не допускается");
        }
        _Owner = value;
        SetReadOnly();
      }
    }
    private DBxCommonFilters _Owner;

    #endregion

    #region Методы и свойства

    /// <summary>
    /// Получение списка имен полей, которые необходимы для выполнения фильтрации.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством UseSqlFilter пропускаются.
    /// Каждое поле в список входит один раз.
    /// </summary>
    /// <param name="list">Список для добавления полей</param>
    public void GetColumnNames(DBxColumnList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          this[i].GetColumnNames(list);
      }
    }
    /// <summary>
    /// Тестирование фильтра.
    /// Опрашиваются фильтры в списке, вызывая метод DBxCommonFilter.TestrValues() пока один из фильтров не вернет false.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством UseSqlFilter пропускаются.
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей. В списке должны быть все поля, полученные вызовом GetColumnNames</param>
    /// <param name="badFilter">Сюда помещается ссылка на фильтр, который вернул false. Если строка проходит условия всех фильтров, сюда записывается null.</param>
    /// <returns>True, если строка проходит все фильтры</returns>
    public bool TestValues(INamedValuesAccess rowValues, out DBxCommonFilter badFilter)
    {
      if (rowValues == null)
        throw new ArgumentNullException("rowValues");

      badFilter = null;
      for (int i = 0; i < Count; i++)
      {
        if (this[i].IsEmpty)
          continue;
        if (!this[i].UseSqlFilter)
          continue;

        if (!this[i].TestValues(rowValues))
        {
          badFilter = this[i];
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Проверка попадания в фильтр.
    /// Если предполагается проверка значений для нескольких строк, используйте объект DBxColumnValueArray и перегрузку метода, принимающую INamedValuesAccess.
    /// </summary>
    /// <param name="columnNames">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <param name="badFilter">Фильтр, который вызывал ошибку</param>
    /// <returns>True, если все активные фильтры пропускают значения</returns>
    public bool TestValues(DBxColumns columnNames, object[] values, out DBxCommonFilter badFilter)
    {
      DBxColumnValueArray cva = new DBxColumnValueArray(columnNames, values);
      return TestValues(cva, out badFilter);
    }

    ///// <summary>
    ///// Проверка попадания в фильтр
    ///// </summary>
    ///// <param name="Columns">Имена полей</param>
    ///// <param name="Row">Строка, содержащая значения всех полей</param>
    ///// <param name="BadFilter">Фильтр, который вызывал ошибку</param>
    ///// <returns>True, если все активные фильтры пропускают значения</returns>
    //public bool TestValues(DBxColumns Columns, DataRow Row, out GridFilter BadFilter)
    //{
    //  object[] Values = Columns.GetRowValues(Row);
    //  return TestValues(Columns.AsArray, Values, out BadFilter);
    //}

    /// <summary>
    /// Получение SQL-фильтра для фильтрации набора данных.
    /// Пустые фильтры и фильтры со сброшенным свойством UseSqlFilter пропускаются.
    /// </summary>
    /// <returns>Объект DatatFilter, соответствующий активным фильтрам. Если фильтров 
    /// несколько, то будет возвращаен AndDBxFilter</returns>
    public DBxFilter GetSqlFilter()
    {
      List<DBxFilter> Filters = new List<DBxFilter>();
      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          Filters.Add(this[i].GetSqlFilter());
      }
      return AndFilter.FromArray(Filters.ToArray());
    }


    /// <summary>
    /// Очистка всех фильтров. Вызывает DBxCommonFilters.Clear() для каждого фильтра.
    /// Список фильтров не меняется. Метод работает независимо от свойства IsReadOnly.
    /// Не путать этот метод с Clear(), который очищает сам список фильтров.
    /// </summary>
    public void ClearAllFilters()
    {
      for (int i = 0; i < Count; i++)
        this[i].Clear();
    }

    /// <summary>
    /// Возвращает true, если ни один из фильтров в наборе не установлен
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].IsEmpty)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если нет ни одного активного фильтра, для которого UseSqlFilter=true
    /// </summary>
    public bool IsSqlEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].UseSqlFilter)
          {
            if (!this[i].IsEmpty)
              return false;
          }
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если нет ни одного активного фильтра, для которого UseSqlFilter=false
    /// </summary>
    public bool IsNonSqlEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].UseSqlFilter)
          {
            if (!this[i].IsEmpty)
              return false;
          }
        }
        return true;
      }
    }

    #endregion
  }

  /// <summary>
  /// Коллекция фильтров
  /// </summary>
  public class DBxCommonFilters : NamedListWithNotifications<DBxCommonFilter>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    public DBxCommonFilters()
    {
    }

    #endregion

    #region Добавление / удаление фильтров

    /// <summary>
    /// Проверяет, что у добавляемого фильтра есть код и он еще не был присоединен к списку.
    /// </summary>
    /// <param name="item">Добавляемый фильтр</param>
    protected override void OnBeforeAdd(DBxCommonFilter item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (item.Owner != null)
        throw new InvalidOperationException("Повторное добавление элемента не допускается");
      if (String.IsNullOrEmpty(item.Code))
        throw new NullReferenceException("Свойство DBxCommonFilter.Code должно быть установлено перед добавлением элемента");

      base.OnBeforeAdd(item);
    }

    /// <summary>
    /// Устанавливает свойство DBxCommonFilter.Owner
    /// </summary>
    /// <param name="item">Добавленный фильтр</param>
    protected override void OnAfterAdd(DBxCommonFilter item)
    {
      base.OnAfterAdd(item);
      item.Owner = this;
    }

    /// <summary>
    /// Очищает свойство DBxCommonFilter.Owner
    /// </summary>
    /// <param name="item">Удаленный фильтр</param>
    protected override void OnAfterRemove(DBxCommonFilter item)
    {
      base.OnAfterRemove(item);
      item.Owner = null;
    }

    #endregion

    #region Добавление набора фильтров

    /// <summary>
    /// Добавление набора фильтров
    /// </summary>
    /// <param name="filterSet">Набор фильтров</param>
    public void Add(DBxCommonFilterSet filterSet)
    {
      filterSet.Owner = this;
      AddRange(filterSet);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Переводит список в режим "Только чтение".
    /// При этом значения самих фильтров можно устанавливать.
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region Извещение об изменении фильтра

    /// <summary>
    /// Вызывается, когда изменяется текущая установка в одном из фильтров.
    /// Событие фильтра DBxCommonFilter.Changed вызывается до этого события.
    /// Если обработчик события вызывает установку какого-либо фильтра, то посылка
    /// вложенного извещения не выполняется
    /// </summary>
    public event EventHandler Changed;

    private bool InsideChanged = false;

    /// <summary>
    /// Внутренний метод для вызова события Changed
    /// </summary>
    /// <param name="filter">Фильтр, который вызвал событие, или null</param>
    internal protected virtual void OnChanged(DBxCommonFilter filter)
    {
      int p = IndexOf(filter);
      if (p < 0)
        //throw new ArgumentException("Фильтр не относится к коллекции");
        return; // обойдемся без выброса исключения

      if (InsideChanged)
        return;

      InsideChanged = true;
      try
      {
        if (!base.IsUpdating)
        {
          if (Changed != null)
          {
            //try
            //{
            Changed(this, EventArgs.Empty);
            //}
            //catch (Exception e)
            //{
            //  EFPApp.ShowException(e, "Ошибка обработки события GridFilters.Changed");
            //}
          }
        }

        base.NotifyItemChanged(p);
      }
      finally
      {
        InsideChanged = false;
      }
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Поиск фильтра по пользовательскому имени фильтра (свойству GridFilter.DisplayName)
    /// </summary>
    /// <param name="displayName">Имя фильтра для поиска</param>
    /// <returns>Найденный фильтр или null, если фильтр не найден</returns>
    public DBxCommonFilter FindByDisplayName(string displayName)
    {
      if (String.IsNullOrEmpty(displayName))
        return null;
      for (int i = 0; i < Count; i++)
      {
        if (this[i].DisplayName == displayName)
          return this[i];
      }
      return null;
    }

    /// <summary>
    /// Получение списка имен полей, которые необходимы для выполнения фильтрации.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством UseSqlFilter пропускаются.
    /// Каждое поле в список входит один раз.
    /// </summary>
    /// <param name="list">Список для добавления полей</param>
    public void GetColumnNames(DBxColumnList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          this[i].GetColumnNames(list);
      }
    }

    /// <summary>
    /// Получение списка имен полей, которые необходимы для выполнения фильтрации.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством UseSqlFilter пропускаются.
    /// Если нет установленных фильтров, возвращается null.
    /// </summary>
    public DBxColumns GetColumnNames()
    {
      DBxColumnList list = new DBxColumnList();
      GetColumnNames(list);
      if (list.Count > 0)
        return new DBxColumns(list);
      else
        return null;
    }

    /// <summary>
    /// Тестирование фильтра.
    /// Опрашиваются фильтры в списке, вызывая метод DBxCommonFilter.TestrValues() пока один из фильтров не вернет false.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством UseSqlFilter пропускаются.
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей. В списке должны быть все поля, полученные вызовом GetColumnNames</param>
    /// <param name="badFilter">Сюда помещается ссылка на фильтр, который вернул false. Если строка проходит условия всех фильтров, сюда записывается null.</param>
    /// <returns>True, если строка проходит все фильтры</returns>
    public bool TestValues(INamedValuesAccess rowValues, out DBxCommonFilter badFilter)
    {
      badFilter = null;
      for (int i = 0; i < Count; i++)
      {
        if (this[i].IsEmpty)
          continue;
        if (!this[i].UseSqlFilter)
          continue;

        if (!this[i].TestValues(rowValues))
        {
          badFilter = this[i];
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Тестирование фильтра.
    /// Опрашиваются фильтры в списке, вызывая метод DBxCommonFilter.TestrValues() пока один из фильтров не вернет false.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством UseSqlFilter пропускаются.
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей. В списке должны быть все поля, полученные вызовом GetColumnNames</param>
    /// <returns>True, если строка проходит все фильтры</returns>
    public bool TestValues(INamedValuesAccess rowValues)
    {
      DBxCommonFilter badFilter;
      return TestValues(rowValues, out badFilter);
    }

    /// <summary>
    /// Проверка попадания в фильтр.
    /// Если предполагается проверка значений для нескольких строк, используйте объект DBxColumnValueArray и перегрузку метода, принимающую INamedValuesAccess.
    /// </summary>
    /// <param name="columns">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <param name="badFilter">Фильтр, который вызывал ошибку</param>
    /// <returns>True, если все активные фильтры пропускают значения</returns>
    public bool TestValues(DBxColumns columns, object[] values, out DBxCommonFilter badFilter)
    {
      DBxColumnValueArray cva = new DBxColumnValueArray(columns, values);
      return TestValues(cva, out badFilter);
    }


    /// <summary>
    /// Проверка попадания в фильтр.
    /// Если предполагается проверка значений для нескольких строк, используйте объект DBxColumnValueArray и перегрузку метода, принимающую INamedValuesAccess.
    /// </summary>
    /// <param name="columns">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <returns>True, если все активные фильтры пропускают значения</returns>
    public bool TestValues(DBxColumns columns, object[] values)
    {
      DBxCommonFilter badFilter;
      return TestValues(columns, values, out badFilter);
    }

    ///// <summary>
    ///// Проверка попадания в фильтр
    ///// </summary>
    ///// <param name="Columns">Имена полей</param>
    ///// <param name="Row">Строка, содержащая значения всех полей</param>
    ///// <param name="BadFilter">Фильтр, который вызывал ошибку</param>
    ///// <returns>True, если все активные фильтры пропускают значения</returns>
    //public bool TestValues(DBxColumns Columns, DataRow Row, out GridFilter BadFilter)
    //{
    //  object[] Values = Columns.GetRowValues(Row);
    //  return TestValues(Columns.AsArray, Values, out BadFilter);
    //}

    /// <summary>
    /// Получение SQL-фильтра для фильтрации набора данных.
    /// Пустые фильтры и фильтры со сброшенным свойством UseSqlFilter пропускаются.
    /// </summary>
    /// <returns>Объект DatatFilter, соответствующий активным фильтрам. Если фильтров 
    /// несколько, то будет возвращаен AndDBxFilter</returns>
    public DBxFilter GetSqlFilter()
    {
      List<DBxFilter> Filters = new List<DBxFilter>();
      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          Filters.Add(this[i].GetSqlFilter());
      }
      return AndFilter.FromArray(Filters.ToArray());
    }


    /// <summary>
    /// Очистка всех фильтров. Вызывает DBxCommonFilters.Clear() для каждого фильтра.
    /// Список фильтров не меняется. Метод работает независимо от свойства IsReadOnly.
    /// Не путать этот метод с Clear(), который очищает сам список фильтров.
    /// </summary>
    public void ClearAllFilters()
    {
      for (int i = 0; i < Count; i++)
        this[i].Clear();
    }


    /// <summary>
    /// Идентификатор базы данных.
    /// Возвращается значение GridFilter.DBIdentity для первого фильтра в списке,
    /// вернувшего непустое значение.
    /// Если ни один из фильтров не вернул значение, возвращается пустая строка.
    /// Это означает, что в списке нет ссылочных фильтров и фильтры можно копировать/вставлять
    /// через буфер обмена между любыми программами
    /// </summary>
    public string DBIdentity
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          string s = this[i].DBIdentity;
          if (!String.IsNullOrEmpty(s))
            return s;
        }

        return String.Empty;
      }
    }


    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = "Count=" + Count.ToString();
      if (IsReadOnly)
        s += " (ReadOnly)";
      return s;
    }

    /// <summary>
    /// Возвращает true, если нет ни одного активного фильтра
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].IsEmpty)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если нет ни одного активного фильтра, для которого UseSqlFilter=true
    /// </summary>
    public bool IsSqlEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].UseSqlFilter)
          {
            if (!this[i].IsEmpty)
              return false;
          }
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если нет ни одного активного фильтра, для которого UseSqlFilter=false
    /// </summary>
    public bool IsNonSqlEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].UseSqlFilter)
          {
            if (!this[i].IsEmpty)
              return false;
          }
        }
        return true;
      }
    }

    #endregion

    #region Чтение / запись секций конфигурации

    /// <summary>
    /// Инициализация текущих значений фильтров на основании сохраненных ранее настроек
    /// Для каждого фильтра предусмотрена отдельная часть, которая всегда
    /// существует после записи фильтров (включая пустые).
    /// Если для какого-либо фильтра (или всех фильтров) нет части, значит пользователь
    /// еще не настраивал фильтр. В этом случае сохраняется значение по умолчанию
    /// </summary>
    /// <param name="config">Секция конфигурации, откуда будут прочитаны настройки.
    /// Если null, то чтение не будет выполнено и текущие настройки останутся без изменений</param>
    public void ReadConfig(CfgPart config)
    {
      if (config == null)
        return;

      for (int i = 0; i < Count; i++)
      {
        //try
        //{
        //  CfgPart Part2 = Config.GetChild(this[i].Code, false);
        //  if (Part2 != null)
        //    this[i].ReadConfig(Part2);
        //}
        //catch (Exception e)
        //{
        //  EFPApp.ShowException(e, "Ошибка загрузки состояния фильтра \"" + this[i].DisplayName + "\"");
        //  this[i].Clear();
        //}

        CfgPart Part2 = config.GetChild(this[i].Code, false);
        if (Part2 != null)
        {
          try
          {
            this[i].ReadConfig(Part2);
          }
          catch (Exception e)
          {
            OnReadConfigError(e, this[i], Part2);
          }
        }
      }
    }

    /// <summary>
    /// Вызывается при возникновении ошибки чтении конфигурации в DBxCommonFilter.ReadConfig().
    /// Непереопределенный метод повторно выбрасывает исключение.
    /// </summary>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="filter">Фильтр, для которого возникло исключение</param>
    /// <param name="cfg">Считываемая секция конфигурации</param>
    protected virtual void OnReadConfigError(Exception exception, DBxCommonFilter filter, CfgPart cfg)
    {
      //Filter.Clear();
      throw exception;
    }

    /// <summary>
    /// Сохранение текущих значений фильтров в секции конфигурации.
    /// </summary>
    /// <param name="cfg">Секция конфигурации для сохранения настроек. Ссылка должна быть задана, иначе будет сгенерировано исключение</param>
    public void WriteConfig(CfgPart cfg)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg", "Раздел для записи конфигурации фильтров должен быть задан");

      cfg.Clear();

      for (int i = 0; i < Count; i++)
      {
        CfgPart cfg2 = cfg.GetChild(this[i].Code, true); // обязательно создаем даже для неустановленных фильтров
        cfg2.Clear(); // 11.09.2012
        if (!this[i].IsEmpty)
          this[i].WriteConfig(cfg2);
      }
    }

    /// <summary>
    /// Текущие значения фильтров в виде строки текста.
    /// Чтение свойства вызывает WriteConfig(), а запись - ReadConfig(). При этом используется TempCfg и преобразование в XML-формат.
    /// Это свойство удобно использовать в отчетах для передачи данных от клиента к серверу
    /// </summary>
    public string ConfigAsXmlText
    {
      get
      {
        TempCfg Cfg = new TempCfg();
        WriteConfig(Cfg);
        return Cfg.AsXmlText;
      }
      set
      {
        TempCfg Cfg = new TempCfg();
        Cfg.AsXmlText = value;
        ReadConfig(Cfg);
      }
    }

    #endregion

    #region IInitNewDocValues Members

    /// <summary>
    /// Вызывается из ClientDocType.PerformEditing
    /// </summary>
    /// <param name="newDoc"></param>
    public void InitNewDocValues(DBxSingleDoc newDoc)
    {
      for (int i = 0; i < Count; i++)
        this[i].InitNewDocValues(newDoc);
    }


    /// <summary>
    /// Вызывает GridFilter.ValidateDocValues для всех фильтров в списке
    /// </summary>
    /// <param name="savingDoc"></param>
    /// <param name="errorMessages"></param>
    public void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
    {
      for (int i = 0; i < Count; i++)
        this[i].ValidateDocValues(savingDoc, errorMessages);
    }

    #endregion
  }

  #region Фильтры для одного поля

  /// <summary>
  /// Базовый класс для фильтров по одному полю.
  /// Определяет свойство ColumnName
  /// </summary>
  public abstract class SingleColumnCommonFilter : DBxCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// Устанавливает свойства DBxCommonFilter.Code и DisplayName равными <paramref name="columnName"/>.
    /// </summary>
    /// <param name="columnName">Имя поля. Должно быть задано</param>
    public SingleColumnCommonFilter(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      _ColumnName = columnName;

      base.Code = columnName; // может быть изменено далее в пользовательском коде
      base.DisplayName = columnName; // может быть изменено далее в пользовательском коде
    }

    #endregion

    #region Свойства и методы

    /// <summary>
    /// Имя фильтруемого поля.
    /// Задается в конструкторе и не может быть изменено
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Получить список имен полей, которые необходимы для вычисления фильтра.
    /// Поля добавляются в список независимо от того, активен сейчас фильтр или нет.
    /// Добавляет в список поле ColumnName.
    /// </summary>
    /// <param name="list">Список для добавления полей</param>
    public override /*sealed */ void GetColumnNames(DBxColumnList list)
    {
      list.Add(ColumnName);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Проверяет наличие в документе поля ColumnName и вызывает метод OnInitNewDocValue() для значения поля
    /// </summary>
    /// <param name="newDoc">Созданный документ, в котором можно установить поля</param>
    protected override /*sealed*/ void OnInitNewDocValues(DBxSingleDoc newDoc)
    {
      int p = newDoc.Values.IndexOf(ColumnName);
      if (p < 0)
        return;

      OnInitNewDocValue(newDoc.Values[p]);
    }

    /// <summary>
    /// Инициализация значения поля при создании нового документа.
    /// Метод вызывается, только когда фильтр установлен
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected virtual void OnInitNewDocValue(DBxDocValue docValue)
    {
    }

    #endregion
  }

  #region Фильтры по строковому полю

  /// <summary>
  /// Простой фильтр по значению текстового поля (проверка поля на равенство 
  /// определенному значению)
  /// </summary>
  public class StringValueCommonFilter : SingleColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public StringValueCommonFilter(string columnName)
      : base(columnName)
    {
      _Value = String.Empty;
    }

    #endregion

    #region Текущее значение

    /// <summary>
    /// Текущее значение фильтра. Пустая строка означает отсутствие фильтра
    /// </summary>
    public String Value
    {
      get { return _Value; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (value == _Value)
          return;
        _Value = value;
        OnChanged();
      }
    }
    private string _Value;

    #endregion

    #region Переопределяемые методы свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      Value = String.Empty;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return String.IsNullOrEmpty(Value);
      }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      return DataTools.GetString(rowValues.GetValue(ColumnName)) == Value;
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new ValueFilter(ColumnName, Value);
    }

    /// <summary>
    /// Инициализация значения поля при создании нового документа.
    /// Метод вызывается, только когда фильтр установлен
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      docValue.SetString(Value);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      Value = cfg.GetString("Value");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetString("Value", Value);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;
      return rowValue == Value;
    }

    #endregion
  }

  /// <summary>
  /// Фильтр для StartsWithFilter
  /// </summary>
  public class StartsWithCommonFilter : StringValueCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра
    /// </summary>
    /// <param name="columnName">Имя строкового поля</param>
    public StartsWithCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределенные методы

    // При изменениях не забыть продублировать их в классе StartsWuthGridFilter в ExtDbDocForms.dll

    /// <summary>
    /// Создает StartsWithFilter
    /// </summary>
    /// <returns></returns>
    public override DBxFilter GetSqlFilter()
    {
      return new StartsWithFilter(ColumnName, Value);
    }

    /// <summary>
    /// Проверка значения
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей</param>
    /// <returns>True, если условие фильтра выполняется</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);
      return DataTools.GetString(v).StartsWith(Value);
    }

    /// <summary>
    /// Метод ничего не делает, в отличие от базового класса.
    /// </summary>
    /// <param name="docValue"></param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public new bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;
      return rowValue.StartsWith(Value);
    }

    #endregion
  }

  #region Перечисление CodesFilterMode

  /// <summary>
  /// Возможные значения свойства CodeGridFilter.Mode
  /// </summary>
  public enum CodesFilterMode
  {
    /// <summary>
    /// Нет фильтра
    /// </summary>
    NoFilter,

    /// <summary>
    /// Включить коды
    /// </summary>
    Include,

    /// <summary>
    /// Исключить коды
    /// </summary>
    Exclude
  }

  #endregion

  /// <summary>
  /// Класс для реализации фильтров по кодам.
  /// Поддерживает режим включения или исключения нескольких кодов. Возможна 
  /// поддержка пустых кодов. 
  /// Является базой для класса RefBookGridFilter для фильтрации по кодам "Код-значение"
  /// </summary>
  public class CodeCommonFilter : SingleColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="canBeEmpty">true, если поддерживаются пустые коды</param>
    public CodeCommonFilter(string columnName, bool canBeEmpty)
      : base(columnName)
    {
      _CanBeEmpty = canBeEmpty;
      _Mode = CodesFilterMode.NoFilter;
      _Codes = String.Empty;
      _EmptyCode = false;
    }

    #endregion

    #region Текущее состояние фильтра

    /// <summary>
    /// Текущий режим фильтра
    /// </summary>
    public CodesFilterMode Mode
    {
      get { return _Mode; }
      set
      {
        if (value == _Mode)
          return;
        _Mode = value;
        OnChanged();
      }
    }
    private CodesFilterMode _Mode;

    /// <summary>
    /// Список включаемых или исключаемых кодов, разделенных запятыми
    /// </summary>
    public string Codes
    {
      get { return _Codes; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (value == _Codes)
          return;
        _Codes = value;
        OnChanged();
      }
    }
    private string _Codes;

    /// <summary>
    /// Включить ли в фильтр строки без кода.
    /// Свойство действительно только при CanBeEmpty=true
    /// </summary>
    public bool EmptyCode
    {
      get { return _EmptyCode; }
      set
      {
        if (value == _EmptyCode)
          return;
        _EmptyCode = value;
        OnChanged();
      }
    }
    private bool _EmptyCode;

    #endregion

    #region Прочие свойства

    /// <summary>
    /// true, если поддерживаются пустые коды. Если false, то предполагается, что 
    /// поле всегда имеет установленное значение, а флажок "Код не установлен" недоступен
    /// Свойство устанавливается в конструкторе
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } }
    private bool _CanBeEmpty;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Нормализация списка кодов. Удаляет пробелы вокруг запятых
    /// </summary>
    /// <param name="s">Строка, введенная пользователем</param>
    /// <returns>Исправленная строка</returns>
    protected static string NormCodes(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      string[] a = s.Split(',');
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < a.Length; i++)
      {
        string s1 = a[i].Trim();
        if (String.IsNullOrEmpty(s1))
          continue;
        if (sb.Length > 0)
          sb.Append(',');
        sb.Append(s1);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      Mode = CodesFilterMode.NoFilter;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty { get { return Mode == CodesFilterMode.NoFilter; } }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      List<DBxFilter> Filters = new List<DBxFilter>();
      if (!String.IsNullOrEmpty(Codes))
      {
        string[] a = Codes.Split(',');
        Filters.Add(new ValuesFilter(Code, a));
      }
      if (EmptyCode)
        Filters.Add(new ValueFilter(Code, String.Empty, typeof(string)));

      DBxFilter Filter = OrFilter.FromArray(Filters.ToArray());
      if (Mode == CodesFilterMode.Exclude)
        Filter = new NotFilter(Filter);
      return Filter;
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      string RowValue = DataTools.GetString(rowValues.GetValue(ColumnName));
      return TestValue(RowValue);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      switch (cfg.GetString("Mode"))
      {
        case "Include":
          Mode = CodesFilterMode.Include;
          break;
        case "Exclude":
          Mode = CodesFilterMode.Exclude;
          break;
        default:
          Mode = CodesFilterMode.NoFilter;
          break;
      }
      Codes = cfg.GetString("Codes");
      if (CanBeEmpty)
        EmptyCode = cfg.GetBool("EmptyCode");
      else
        EmptyCode = false;
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      switch (Mode)
      {
        case CodesFilterMode.Include:
          cfg.SetString("Mode", "Include");
          break;
        case CodesFilterMode.Exclude:
          cfg.SetString("Mode", "Exclude");
          break;
        default:
          cfg.Remove("Mode");
          break;
      }

      if (String.IsNullOrEmpty(Codes))
        cfg.Remove("Codes");
      else
        cfg.SetString("Codes", Codes);

      if (CanBeEmpty)
        cfg.SetBool("EmptyCode", EmptyCode);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;

      bool Flag;
      if (String.IsNullOrEmpty(rowValue))
        Flag = EmptyCode;
      else
      {
        if (String.IsNullOrEmpty(Codes))
          Flag = false;
        else
          Flag = Codes.Contains(rowValue);
      }
      if (Mode == CodesFilterMode.Exclude)
        return !Flag;
      else
        return Flag;
    }

    #endregion

  }

  #endregion

  #region Фильтры ValueFilter

  /// <summary>
  /// Базовый класс для построения фильтров по значению поля
  /// Активность фильтра устанавливается с помощью шаблона Nullable
  /// </summary>
  /// <typeparam name="T">Тип значения поля</typeparam>
  public abstract class ValueCommonFilterBase<T> : SingleColumnCommonFilter
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public ValueCommonFilterBase(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее значение фильтра
    /// </summary>
    public Nullable<T> Value
    {
      get { return _Value; }
      set
      {
        if (DataTools.AreValuesEqual(value, _Value))
          return;
        _Value = value;
        OnChanged();
      }
    }
    private Nullable<T> _Value;

    #endregion

    #region Переопределяемые свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      Value = null;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !Value.HasValue;
      }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);

      if (Value.Value.Equals(v))
        return true;

      // 11.08.2014
      // Если значение Value равно значению "по умолчанию" (0, false), то в фильтр
      // должны попадать записи со значением поля NULL
      if (Value.Value.Equals(default(T)))
      {
        if (v == null)
          return true;

        if (v is DBNull)
          return true;
      }

      return false;
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new ValueFilter(ColumnName, Value.Value, typeof(T));
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля ColumnName, если в фильтре выбрано единственное значение.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      docValue.SetValue(Value.Value);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      if (!String.IsNullOrEmpty(cfg.GetString("Value")))
        Value = DoReadConfigValue(cfg);
      else
        Value = null;
    }

    /// <summary>
    /// Абстрактный метод, который должен прочитать параметр "Value" из секции конфигурации.
    /// Например, с помощью вызова return Part.GetInt("Value").
    /// </summary>
    /// <param name="cfg">Секция конфигурации для чтения фильтра</param>
    /// <returns>Прочитанное значение</returns>
    protected abstract T DoReadConfigValue(CfgPart cfg);

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      if (Value.HasValue)
        DoWriteConfigValue(cfg, Value.Value);
      else
        cfg.Remove("Value");
    }

    /// <summary>
    /// Абстрактный метод, который должен записывать параметр "Value" в секцию конфигурации.
    /// Например, с помощью вызова Part.SetInt("Value", Value).
    /// </summary>
    /// <param name="cfg">Секция конфигурации для записи фильтра</param>
    /// <param name="value">Записываемое значение</param>
    protected abstract void DoWriteConfigValue(CfgPart cfg, T value);

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(T rowValue)
    {
      if (IsEmpty)
        return true;

      if (Value.Value.Equals(rowValue))
        return true;

      return false;
    }

    #endregion
  }

  /// <summary>
  /// Простой фильтр по логическому полю
  /// </summary>
  public class BoolValueCommonFilter : ValueCommonFilterBase<bool>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public BoolValueCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Вызывает CfgPart.GetBool()
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <returns>Значение</returns>
    protected override bool DoReadConfigValue(CfgPart cfg)
    {
      return cfg.GetBool("Value");
    }

    /// <summary>
    /// Вызывает CfgPart.SetBool()
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="value">Значение</param>
    protected override void DoWriteConfigValue(CfgPart cfg, bool value)
    {
      cfg.SetBool("Value", value);
    }

    ///// <summary>
    ///// Возвращает FilterTextTrue или FilterTextFales, если фильтр установлен
    ///// </summary>
    ///// <param name="ColumnValues">Значения полей</param>
    ///// <returns>Текстовые представления значений</returns>
    //protected override string[] GetColumnStrValues(object[] ColumnValues)
    //{
    //  bool Value = DataTools.GetBool(ColumnValues[0]);
    //  return new string[] { Value ? FilterTextTrue : FilterTextFalse };
    //}

    #endregion
  }

  /// <summary>
  /// Простой фильтр по полю типа Integer с фильтрацией по единственному значению
  /// Если поле может принимать фиксированный набор значений, то следует использовать
  /// фильтр EnumGridFilter
  /// </summary>
  public class IntValueCommonFilter : ValueCommonFilterBase<int>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public IntValueCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределяемые свойства

    /// <summary>
    /// Вызывает CfgPart.GetInt()
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <returns>Значение</returns>
    protected override int DoReadConfigValue(CfgPart cfg)
    {
      return cfg.GetInt("Value");
    }

    /// <summary>
    /// Вызывает CfgPart.SetBool()
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="value">Значение</param>
    protected override void DoWriteConfigValue(CfgPart cfg, int value)
    {
      cfg.SetInt("Value", value);
    }

    /// <summary>
    /// Возвращает текстовое представление для числа
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      int Value = DataTools.GetInt(columnValues[0]);
      return new string[] { Value.ToString() };
    }

    #endregion
  }


  /// <summary>
  /// Фильтр по году для числового поля или поля типа "Дата"
  /// </summary>
  public class YearCommonFilter : IntValueCommonFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор для числового поля
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public YearCommonFilter(string columnName)
      : this(columnName, false)
    {
    }

    /// <summary>
    /// Конструктор для поля типа "Дата" или числового поля
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="isDateColumn">Если true, то поле имеет тип "Дата".
    /// Если false, то поле является числовым</param>
    public YearCommonFilter(string columnName, bool isDateColumn)
      : base(columnName)
    {
      _IsDateColumn = isDateColumn;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если true, то поле имеет тип "Дата".
    /// Если false, то поле является числовым.
    /// </summary>
    public bool IsDateColumn { get { return _IsDateColumn; } }
    private bool _IsDateColumn;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (!Value.HasValue)
        return null;
      if (IsDateColumn)
        return new DateRangeFilter(ColumnName, Value.Value);
      else
        return new ValueFilter(ColumnName, Value.Value);
    }

    #endregion
  }

  #endregion

  #region Фильтры по диапазонам значений

  /// <summary>
  /// Фильтр по диапазону дат для одного поля
  /// </summary>
  public class DateRangeCommonFilter : SingleColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца типа "Дата"</param>
    public DateRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее значение фильтра - начальная дата или null
    /// </summary>
    public DateTime? FirstDate
    {
      get { return _FirstDate; }
      set
      {
        if (value == _FirstDate)
          return;
        _FirstDate = value;
        OnChanged();
      }
    }
    private DateTime? _FirstDate;

    /// <summary>
    /// Текущее значение фильтра - конечная дата или null
    /// </summary>
    public DateTime? LastDate
    {
      get { return _LastDate; }
      set
      {
        if (value == _LastDate)
          return;
        _LastDate = value;
        OnChanged();
      }
    }
    private DateTime? _LastDate;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      FirstDate = null;
      LastDate = null;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !(FirstDate.HasValue || LastDate.HasValue);
      }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);
      if (v == null)
        return false;
      if (v is DBNull)
        return false;

      return DataTools.DateInRange((DateTime)v, FirstDate, LastDate);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeFilter(ColumnName, FirstDate, LastDate);

    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstDate = cfg.GetNullableDate("FirstDate");
      LastDate = cfg.GetNullableDate("LastDate");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDate("FirstDate", FirstDate);
      cfg.SetNullableDate("LastDate", LastDate);
    }

    /// <summary>
    /// Использует DateRangeFormatter для преобразования в строку значения поля
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      return new string[] { DateRangeFormatter.Default.ToString(DataTools.GetNullableDateTime(columnValues[0]), true) };
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего целочисленное значение.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// Базовый класс для IntRangeCommonFilter, SingleRangeCommonFilter, DoubleRangeCommonFilter и DecimalRangeCommonFilter
  /// </summary>
  public abstract class NumRangeCommonFilter<T> : SingleColumnCommonFilter
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public NumRangeCommonFilter(string columnName)
      : base(columnName)
    {
      _NullIsZero = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее значение фильтра - начальное значение диапазона или null
    /// </summary>
    public T? FirstValue
    {
      get { return _FirstValue; }
      set
      {
        if (Object.Equals(value, _FirstValue))
          return;
        _FirstValue = value;
        OnChanged();
      }
    }
    private T? _FirstValue;

    /// <summary>
    /// Текущее значение фильтра - конечное значение диапазона или null
    /// </summary>
    public T? LastValue
    {
      get { return _LastValue; }
      set
      {
        if (Object.Equals(value, _LastValue))
          return;
        _LastValue = value;
        OnChanged();
      }
    }
    private T? _LastValue;

    /// <summary>
    /// Если true (по умолчанию, то значения NULL трактуются как 0)
    /// </summary>
    public bool NullIsZero { get { return _NullIsZero; } set { _NullIsZero = value; } }
    private bool _NullIsZero;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      FirstValue = null;
      LastValue = null;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !(FirstValue.HasValue || LastValue.HasValue);
      }
    }

    /// <summary>
    /// Если FirstValue и LastValue установлены в одно и то же значение, отличное от null, то значение поля документа инициализируется выбранным значением.
    /// </summary>
    /// <param name="DocValue"></param>
    protected override void OnInitNewDocValue(DBxDocValue DocValue)
    {
      if (FirstValue.HasValue && LastValue.HasValue && Object.Equals(FirstValue, LastValue))
      {
        if (NullIsZero && FirstValue.Value.Equals(default(T)))
          DocValue.SetNull();
        else
          DocValue.SetValue(FirstValue.Value);
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего целочисленное значение.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public class IntRangeCommonFilter : NumRangeCommonFilter<int>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public IntRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v1 = rowValues.GetValue(ColumnName);
      int v2;
      if (v1 == null || (v1 is DBNull))
      {
        if (NullIsZero)
          v2 = 0;
        else
          return false;
      }
      else
        v2 = DataTools.GetInt(v1);

      return DataTools.IntInRange(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.IntInRange(0, FirstValue, LastValue))
        Filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(int)), Filter);
      return Filter;
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableInt("FirstValue");
      LastValue = cfg.GetNullableInt("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableInt("FirstValue", FirstValue);
      cfg.SetNullableInt("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего числовое значение с плавающей точкой.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public class SingleRangeCommonFilter : NumRangeCommonFilter<float>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public SingleRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v1 = rowValues.GetValue(ColumnName);
      float v2;
      if (v1 == null || (v1 is DBNull))
      {
        if (NullIsZero)
          v2 = 0f;
        else
          return false;
      }
      else
        v2 = DataTools.GetSingle(v1);

      return DataTools.SingleInRange(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.SingleInRange(0, FirstValue, LastValue))
        Filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(float)), Filter);
      return Filter;
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableSingle("FirstValue");
      LastValue = cfg.GetNullableSingle("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableSingle("FirstValue", FirstValue);
      cfg.SetNullableSingle("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего числовое значение с плавающей точкой.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public class DoubleRangeCommonFilter : NumRangeCommonFilter<double>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public DoubleRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v1 = rowValues.GetValue(ColumnName);
      double v2;
      if (v1 == null || (v1 is DBNull))
      {
        if (NullIsZero)
          v2 = 0.0;
        else
          return false;
      }
      else
        v2 = DataTools.GetDouble(v1);

      return DataTools.DoubleInRange(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.DoubleInRange(0, FirstValue, LastValue))
        Filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(double)), Filter);
      return Filter;
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableDouble("FirstValue");
      LastValue = cfg.GetNullableDouble("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDouble("FirstValue", FirstValue);
      cfg.SetNullableDouble("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего числовое значение с плавающей точкой.
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public class DecimalRangeCommonFilter : NumRangeCommonFilter<decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public DecimalRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v1 = rowValues.GetValue(ColumnName);
      decimal v2;
      if (v1 == null || (v1 is DBNull))
      {
        if (NullIsZero)
          v2 = 0m;
        else
          return false;
      }
      else
        v2 = DataTools.GetDecimal(v1);

      return DataTools.DecimalInRange(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.DecimalInRange(0, FirstValue, LastValue))
        Filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(float)), Filter);
      return Filter;
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableDecimal("FirstValue");
      LastValue = cfg.GetNullableDecimal("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDecimal("FirstValue", FirstValue);
      cfg.SetNullableDecimal("LastValue", LastValue);
    }

    #endregion
  }

  #endregion

  #region Прочие фильтры

  /// <summary>
  /// Фильтр по одному или нескольким значениям числового поля, каждому из
  /// которых соответствует текстовое представление
  /// </summary>
  public class EnumCommonFilter : SingleColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Конструктор фильтра
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="itemCount">Количество элементов в перечислении. Поле может принимать значения от 0 до (ItemCount-1). 
    /// Должно быть не меньше 1</param>
    public EnumCommonFilter(string columnName, int itemCount)
      : base(columnName)
    {
      if (itemCount < 1)
        throw new ArgumentOutOfRangeException("itemCount", itemCount, "Количество элементов перечисления должно быть не меньше 1");
      _ItemCount = itemCount;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Количество элементов в перечислилении.
    /// Задается в конструкторе.
    /// </summary>
    public int ItemCount { get { return _ItemCount; } }
    private int _ItemCount;

    /// <summary>
    /// Текущее значение фильтра. Содержит массив флагов, соответствующих числовым
    /// значениям поля 0,1,2,...,(TextValues.Lenght-1).
    /// Если фильтр не установлен, то свойство содержит null
    /// </summary>
    public bool[] FilterFlags
    {
      get { return _FilterFlags; }
      set
      {
        _FilterFlags = value;
        OnChanged();
      }
    }
    private bool[] _FilterFlags;

    /// <summary>
    /// Альтернативная установка фильтра
    /// Выбор единственного выбранного значения.
    /// Значение (-1) соответствует пестому фильтру (FilterFlags=null)
    /// </summary>
    public int SingleFilterItemIndex
    {
      get
      {
        if (FilterFlags == null)
          return -1;
        int Index = -1;
        for (int i = 0; i < FilterFlags.Length; i++)
        {
          if (FilterFlags[i])
          {
            if (Index < 0)
              Index = i;
            else
              // Установлено больше одного флага
              return -1;
          }
        }
        return Index;
      }
      set
      {
        if (value < 0)
          FilterFlags = null;
        else
        {
          bool[] a = new bool[ItemCount];
          DataTools.FillArray<bool>(a, false);
          a[value] = true;
          FilterFlags = a;
        }
      }
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      FilterFlags = null;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get { return FilterFlags == null; }
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (FilterFlags == null)
        return null;

      List<int> Values = new List<int>();
      for (int i = 0; i < FilterFlags.Length; i++)
      {
        if (FilterFlags[i])
          Values.Add(i);
      }
      DBxFilter Filter = new ValuesFilter(ColumnName, Values.ToArray());
      // 18.10.2019
      // Форматировщик OnFormatValuesFilter() теперь сам учитывает наличие пустого значения среди Values и дополнительная проверка на NULL не нужна
      //if (FilterFlags[0])
      //  Filter = new OrFilter(Filter, new ValueFilter(ColumnName, null, typeof(Int16)));
      return Filter;
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int Value = DataTools.GetInt(rowValues.GetValue(ColumnName));
      if (Value < 0 || Value > FilterFlags.Length)
        return false;
      else
        return FilterFlags[Value];
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      int[] a = StdConvert.ToInt32Array(config.GetString("Flags"));
      if (a.Length == 0)
        FilterFlags = null;
      else
      {
        FilterFlags = new bool[ItemCount];
        for (int i = 0; i < a.Length; i++)
        {
          if (a[i] >= 0 && a[i] < FilterFlags.Length)
            FilterFlags[a[i]] = true;
        }
      }
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      int[] a = null;
      if (FilterFlags != null)
      {
        List<int> lst = new List<int>();
        for (int i = 0; i < FilterFlags.Length; i++)
        {
          if (FilterFlags[i])
            lst.Add(i);
        }
        a = lst.ToArray();
      }
      config.SetString("Flags", StdConvert.ToString(a));
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля ColumnName, если в фильтре выбрано единственное значение.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить поля</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      if (SingleFilterItemIndex >= 0)
        docValue.SetInteger(SingleFilterItemIndex);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(int rowValue)
    {
      if (IsEmpty)
        return true;

      if (rowValue < 0 || rowValue >= FilterFlags.Length)
        return false;

      return FilterFlags[rowValue];
    }

    #endregion
  }

  #region Перечисление NullNotNullGridFilterValue

  /// <summary>
  /// Возможные состояния фильтра NullNotNullCommonFilter
  /// </summary>
  public enum NullNotNullFilterValue
  {
    /// <summary>
    /// Фильтр не установлен
    /// </summary>
    NoFilter,

    /// <summary>
    /// Поле имеет значение, отличное от NULL
    /// </summary>
    NotNull,

    /// <summary>
    /// Поле имеет значение NULL
    /// </summary>
    Null,
  }

  #endregion

  /// <summary>
  /// Фильтр по наличию или отсутствию значения NULL/NOT NULL (обычно, для поля
  /// типа "Дата")
  /// </summary>
  public class NullNotNullCommonFilter : SingleColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="columnType">Тип данных, который хранится в поле</param>
    public NullNotNullCommonFilter(string columnName, Type columnType)
      : base(columnName)
    {
#if DEBUG
      if (columnType == null)
        throw new ArgumentNullException("columnType");
#endif
      _ColumnType = columnType;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее состояние фильтра: не установлен, NULL или NOT NULL
    /// </summary>
    public NullNotNullFilterValue Value
    {
      get { return _Value; }
      set
      {
        if (value == _Value)
          return;
        _Value = value;
        OnChanged();
      }
    }
    private NullNotNullFilterValue _Value;

    /// <summary>
    /// Тип значения, хранящегося в поле.
    /// Свойство задается в конструкторе фильтра.  
    /// Требуется для создания объекта NotNullFilter
    /// </summary>
    public Type ColumnType { get { return _ColumnType; } }
    private Type _ColumnType;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      Value = NullNotNullFilterValue.NoFilter;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get { return Value == NullNotNullFilterValue.NoFilter; }
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      switch (Value)
      {
        case NullNotNullFilterValue.NotNull:
          return new NotNullFilter(ColumnName, ColumnType);
        case NullNotNullFilterValue.Null:
          return new ValueFilter(ColumnName, null, ColumnType);
        default:
          return null;
      }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object RowValue = rowValues.GetValue(ColumnName);
      return TestValue(RowValue);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      string s = config.GetString("Mode");
      switch (s)
      {
        case "Null":
          Value = NullNotNullFilterValue.Null;
          break;
        case "NotNull":
          Value = NullNotNullFilterValue.NotNull;
          break;
        default:
          Value = NullNotNullFilterValue.NoFilter;
          break;
      }
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      string s;
      switch (Value)
      {
        case NullNotNullFilterValue.Null: s = "Null"; break;
        case NullNotNullFilterValue.NotNull: s = "NotNull"; break;
        default: s = String.Empty; break;
      }
      config.SetString("Mode", s);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(object rowValue)
    {
      bool IsNull = (rowValue == null || rowValue is DBNull);
      switch (Value)
      {
        case NullNotNullFilterValue.Null:
          return IsNull;
        case NullNotNullFilterValue.NotNull:
          return !IsNull;
        default:
          return true;
      }
    }

    #endregion
  }

  #endregion

  #region Перечисление RefDocFilterMode

  /// <summary>
  /// Режимы работы RefDocGridFilter.
  /// Поддерживается режим фильтрации по выбранным записям и режим исключения выбранных записей
  /// </summary>
  public enum RefDocFilterMode
  {
    /// <summary>
    /// Фильтр не установлен
    /// </summary>
    NoFilter,

    /// <summary>
    /// Режим фильтрации по выбранным записям
    /// </summary>
    Include,

    /// <summary>
    /// Режим исключения выбранных записей
    /// </summary>
    Exclude,

    /// <summary>
    /// Фильтр по любой ссылке.
    /// Должно использоваться только для ссылочных полей, поддерживающих значение NULL.
    /// </summary>
    NotNull,

    /// <summary>
    /// Фильтр по значению Null.
    /// Должно использоваться только для ссылочных полей, поддерживающих значение NULL.
    /// </summary>
    Null
  }

  #endregion

  /// <summary>
  /// Фильтр по значению ссылочного поля на документ
  /// Возможен фильтр по нескольким идентификаторам и режим "Исключить"
  /// Фильтр по пустому значению поля невозможен
  /// </summary>
  public class RefDocCommonFilter : SingleColumnCommonFilter
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docType">Описание вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocCommonFilter(DBxDocProvider docProvider, DBxDocType docType, string columnName)
      : base(columnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (docType == null)
        throw new ArgumentNullException("docType");

      _DocProvider = docProvider;
      _DocType = docType;
      _Mode = RefDocFilterMode.NoFilter;
    }

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docTypeName">Имя вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocCommonFilter(DBxDocProvider docProvider, string docTypeName, string columnName)
      : this(docProvider, docProvider.DocTypes[docTypeName], columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private DBxDocProvider _DocProvider;

    /// <summary>
    /// Описание вида документа, из которого осуществляется выбор.
    /// Задается в конструкторе объекта фильтра.
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } }
    private DBxDocType _DocType;

    /// <summary>
    /// Имя таблицы документа
    /// </summary>
    public string DocTypeName { get { return _DocType.Name; } }

    /// <summary>
    /// Режим фильтра
    /// </summary>
    public RefDocFilterMode Mode { get { return _Mode; } }
    private RefDocFilterMode _Mode;

    /// <summary>
    /// Идентификаторы документов, если фильтр установлен
    /// </summary>
    public IdList DocIds { get { return _DocIds; } }
    private IdList _DocIds;

    /// <summary>
    /// Установить или очистить фильтр
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    /// <param name="docIds">Массив идентификаторов выбранных документов, если применимо к выбранному режиму.
    /// Если неприменимо, аргумент игнорируется</param>
    public void SetFilter(RefDocFilterMode mode, Int32[] docIds)
    {
      IdList DocIds2 = null;
      if (docIds != null)
        DocIds2 = new IdList(docIds);
      SetFilter(mode, DocIds2);
    }

    /// <summary>
    /// Установить или очистить фильтр.
    /// Эта перегрузка предназначена только для режимов <paramref name="mode"/>=NoFilter, Null и NotNull.
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    public void SetFilter(RefDocFilterMode mode)
    {
      SetFilter(mode, (IdList)null);
    }

    /// <summary>
    /// Установить или очистить фильтр
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    /// <param name="docIds">Список идентификаторов выбранных документов, если фильтр устанавливается</param>
    public void SetFilter(RefDocFilterMode mode, IdList docIds)
    {
      switch (mode)
      {
        case RefDocFilterMode.NoFilter:
          Clear();
          return;
        case RefDocFilterMode.Include:
        case RefDocFilterMode.Exclude:
          if (docIds == null)
            throw new ArgumentNullException("docIds");
          // 24.07.2019 - разрешаем пустой список идентификаторов
          //if (DocIds.Count == 0)
          //  throw new ArgumentException("Не задан список идентификаторов для фильтра", "DocIds");
          _Mode = mode;
          _DocIds = docIds;
          _DocIds.SetReadOnly();
          OnChanged();
          break;
        case RefDocFilterMode.NotNull:
        case RefDocFilterMode.Null:
          if (mode == _Mode)
            return;
          _Mode = mode;
          _DocIds = null;
          OnChanged();
          break;
        default:
          throw new ArgumentException("Неизвестный режим " + mode.ToString(), "mode");
      }
    }

    /// <summary>
    /// Упрощенная установка фильтра по единственному значению. 
    /// Свойство возвращает идентификатор документа или поддокумента для ссылочного поля, 
    /// если установлен режим "включить" и задан один идентификатор в списке. Возвращает 0,
    /// если а) фильтр не установлен или б) фильтр в режиме "кроме", или в) вывбрано 
    /// несколько идентификаторов в списке.
    /// Установка значения свойства в ненулевое значение устанавливает фильтр по
    /// одному документу, а в нулевое значение - очищает фильтр
    /// </summary>
    public Int32 SingleDocId
    {
      get
      {
        if (_Mode == RefDocFilterMode.Include && _DocIds.Count == 1)
          return _DocIds.SingleId;
        else
          return 0;
      }
      set
      {
        if (value == SingleDocId)
          return;
        if (value == 0)
          Clear();
        else
          SetFilter(RefDocFilterMode.Include, new Int32[1] { value });
      }
    }

    /// <summary>
    /// Идентификатор базы данных.
    /// Используется в операциях с буфером обмена
    /// </summary>
    public override string DBIdentity
    {
      get { return DocProvider.DBIdentity; }
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Очистить фильтр
    /// </summary>
    public override void Clear()
    {
      if (IsEmpty)
        return;
      _Mode = RefDocFilterMode.NoFilter;
      _DocIds = null;
      OnChanged();
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get { return Mode == RefDocFilterMode.NoFilter; }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Int32 Id = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return TestValue(Id);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      switch (Mode)
      {
        case RefDocFilterMode.NoFilter:
          return null;
        case RefDocFilterMode.Include:
          if (_DocIds.Count == 0)
            return DummyFilter.AlwaysFalse; // 24.07.2019
          else
            return new IdsFilter(ColumnName, _DocIds);
        case RefDocFilterMode.Exclude:
          if (_DocIds.Count == 0)
            return null; // 24.07.2019
          else
            return new NotFilter(new IdsFilter(ColumnName, _DocIds));
        case RefDocFilterMode.NotNull:
          return new NotNullFilter(ColumnName, typeof(Int32));
        case RefDocFilterMode.Null:
          return new ValueFilter(ColumnName, null, typeof(Int32));
        default:
          throw new BugException("Неизвестный режим " + Mode.ToString());
      }
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      RefDocFilterMode NewMode = config.GetEnum<RefDocFilterMode>("Mode");
      Int32[] NewDocIds = StdConvert.ToInt32Array(config.GetString("Ids"));
      SetFilter(NewMode, NewDocIds);
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetEnum("Mode", Mode);
      if (DocIds != null)
        config.SetString("Ids", StdConvert.ToString(_DocIds.ToArray()));
      else
        config.Remove("Ids");
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля ColumnName, если в фильтре выбрано единственное значение.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      switch (Mode)
      {
        case RefDocFilterMode.Include:
          Int32 Id = SingleDocId;
          if (Id != 0)
            docValue.SetInteger(Id);
          break;
        case RefDocFilterMode.Null:
          docValue.SetNull();
          break;
      }
    }

#pragma warning disable 1591

    public override bool CanAsCurrRow(DataRow row)
    {
      Int32 ThisId = DataTools.GetInt(row, ColumnName);
      if (ThisId == 0 || ThisId == SingleDocId)
        return false;
      return true;
    }

    public override void SetAsCurrRow(DataRow row)
    {
      Int32 ThisId = DataTools.GetInt(row, ColumnName);
      SingleDocId = ThisId;
    }

#pragma warning restore 1591

    ///// <summary>
    ///// Вызывает DBxDocTextHandlers.GetTextValue() для получения текстового представления
    ///// </summary>
    ///// <param name="ColumnValues">Значения полей</param>
    ///// <returns>Текстовые представления значений</returns>
    //protected override string[] GetColumnStrValues(object[] ColumnValues)
    //{
    //  return new string[] { UI.TextHandlers.GetTextValue(DocTypeName, DataTools.GetInt(ColumnValues[0])) };
    //}

    #endregion

    #region Другие методы

    /// <summary>
    /// Проверить соответствие идентификатора фильтру
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>true, если идентификатор проходит условие фильтра</returns>
    public bool TestValue(Int32 id)
    {
      switch (Mode)
      {
        case RefDocFilterMode.NoFilter:
          return true;
        case RefDocFilterMode.Include:
          return DocIds.Contains(id);
        case RefDocFilterMode.Exclude:
          return !DocIds.Contains(id);
        case RefDocFilterMode.NotNull:
          return id != 0;
        case RefDocFilterMode.Null:
          return id == 0;
        default:
          throw new BugException("Неизвестный режим " + Mode.ToString());
      }
    }

    #endregion
  }

  /// <summary>
  /// Набор из фильтра по группе и фильтра по документам
  /// </summary>
  public class RefDocCommonFilterSet : DBxCommonFilterSet
  {
    #region Конструкторы

    /// <summary>
    /// Создает набор из одного или двух фильтров
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docType">Описание вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocCommonFilterSet(DBxDocProvider docProvider, DBxDocType docType, string columnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (docType == null)
        throw new ArgumentNullException("docType");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      if (!String.IsNullOrEmpty(docType.GroupRefColumnName))
      {
        DBxColumnStruct GroupIdCol = docType.Struct.Columns[docType.GroupRefColumnName];
        if (GroupIdCol == null)
          throw new ArgumentException("Неправильное описание вида документа \"" + docType.Name + "\". Нет поля \"" + docType.GroupRefColumnName + "\"", "docType");
        if (String.IsNullOrEmpty(GroupIdCol.MasterTableName))
          throw new ArgumentException("Неправильное описание вида документа \"" + docType.Name + "\". Поле \"" + docType.GroupRefColumnName + "\" нея является", "docType");
        DBxDocType GroupDocType = docProvider.DocTypes[GroupIdCol.MasterTableName];
        if (GroupDocType == null)
          throw new NullReferenceException("Не найдены документы для мастер-таблицы \"" + GroupIdCol.MasterTableName + "\"");
        _GroupFilter = new RefDocCommonFilter(docProvider, GroupDocType, columnName + "." + docType.GroupRefColumnName);
        _GroupFilter.DisplayName = GroupDocType.SingularTitle;
        Add(_GroupFilter);
      }

      _DocFilter = new RefDocCommonFilter(docProvider, docType, columnName);
      Add(_DocFilter);
    }

    /// <summary>
    /// Создает набор из одного или двух фильтров
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docTypeName">Имя вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocCommonFilterSet(DBxDocProvider docProvider, string docTypeName, string columnName)
      : this(docProvider, docProvider.DocTypes[docTypeName], columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Фильтр по группе документов.
    /// Если вид документов, на которое ссылается поле, не использует группы, свойство возвращает null
    /// </summary>
    public RefDocCommonFilter GroupFilter { get { return _GroupFilter; } }
    private RefDocCommonFilter _GroupFilter;

    /// <summary>
    /// Основной фильтр
    /// </summary>
    public RefDocCommonFilter DocFilter { get { return _DocFilter; } }
    private RefDocCommonFilter _DocFilter;

    #endregion
  }

  /// <summary>
  /// Фильтр по полю GroupId
  /// </summary>
  public class RefGroupDocCommonFilter : SingleColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="groupDocType">Описание вида документа групп</param>
    /// <param name="groupRefColumnName">Имя ссылочного поля, определяющего группу</param>
    public RefGroupDocCommonFilter(DBxDocProvider docProvider, DBxDocType groupDocType, string groupRefColumnName)
      : base(groupRefColumnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (groupDocType == null)
        throw new ArgumentNullException("groupDocType");
      if (String.IsNullOrEmpty(groupDocType.TreeParentColumnName))
        throw new ArgumentException("Для документов \"" + groupDocType.Name + "\" не установлено свойство TreeParentColumnName. Следовательно, эти документы не могут быть деревом групп");

      _DocProvider = docProvider;
      _GroupDocType = groupDocType;

      base.DisplayName = groupDocType.SingularTitle; // лучше, чем "GroupId"

      _GroupId = 0;
      _IncludeNestedGroups = true;
    }

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="groupDocTypeName">Имя вида документов дерева групп</param>
    /// <param name="groupRefColumnName">Имя ссылочного поля, определяющего группу</param>
    public RefGroupDocCommonFilter(DBxDocProvider docProvider, string groupDocTypeName, string groupRefColumnName)
      : this(docProvider, docProvider.DocTypes[groupDocTypeName], groupRefColumnName)
    {
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private DBxDocProvider _DocProvider;

    /// <summary>
    /// Описание вида документа групп.
    /// Задается в конструкторе объекта фильтра.
    /// </summary>
    public DBxDocType GroupDocType { get { return _GroupDocType; } }
    private DBxDocType _GroupDocType;

    /// <summary>
    /// Имя таблицы документа групп.
    /// </summary>
    public string GroupDocTypeName { get { return _GroupDocType.Name; } }

    #endregion

    #region Текущие установки фильтра

    /// <summary>
    /// Идентификатор выбранной группы
    /// </summary>
    public Int32 GroupId
    {
      get { return _GroupId; }
      set
      {
        if (value == _GroupId)
          return;
        _GroupId = value;
        OnChanged();
      }
    }
    private Int32 _GroupId;

    /// <summary>
    /// Если true, то включаются также вложенные группы
    /// </summary>
    public bool IncludeNestedGroups
    {
      get { return _IncludeNestedGroups; }
      set
      {
        if (value == _IncludeNestedGroups)
          return;
        _IncludeNestedGroups = value;
        OnChanged();
      }
    }
    private bool _IncludeNestedGroups;


    /// <summary>
    /// Идентификатор базы данных.
    /// Используется в операциях с буфером обмена
    /// </summary>
    public override string DBIdentity
    {
      get { return DocProvider.DBIdentity; }
    }

    #endregion

    #region Свойство AuxFilterGroupIds

    /// <summary>
    /// Возвращает массив идентификаторов отфильтрованных групп документов.
    /// Используется для фильтрации документов в основном просмотре.
    /// Если выбраны "Все документы", возвращает null.
    /// Если выбраны "Документы без групп", возвращает массив нулевой длины.
    /// Если есть выбранная группа, возвращает массив из одного или нескольких элементов,
    /// в зависимости от IncludeNested
    /// </summary>
    public IdList AuxFilterGroupIdList
    {
      get
      {
        if (!_AuxFilterGroupIdsReady)
        {
          _AuxFilterGroupIdList = GetAuxFilterGroupIdList();
          _AuxFilterGroupIdsReady = true;
        }
        return _AuxFilterGroupIdList;
      }
    }

    private IdList _AuxFilterGroupIdList;

    /// <summary>
    /// Флажок устанавливается в true, если FAuxFilterGroupIdList содержит корректное значение
    /// </summary>
    private bool _AuxFilterGroupIdsReady;

    private IdList GetAuxFilterGroupIdList()
    {
      if (GroupId == 0)
      {
        if (IncludeNestedGroups)
          return null;
        else
          return IdList.Empty;
      }
      else
      {
        if (IncludeNestedGroups)
        {
          DBxDocTreeModel Model = new DBxDocTreeModel(DocProvider,
            GroupDocType,
            new DBxColumns(new string[] { "Id", GroupDocType.TreeParentColumnName }));

          return new IdList(Model.GetIdWithChildren(GroupId));
        }
        else
          return IdList.FromId(GroupId);
      }
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Этот метод вызывается при изменении значения фильтра.
    /// </summary>
    protected override void OnChanged()
    {
      _AuxFilterGroupIdsReady = false;
      base.OnChanged();
    }

    /// <summary>
    /// Очистить фильтр
    /// </summary>
    public override void Clear()
    {
      if (IsEmpty)
        return;
      _GroupId = 0;
      _IncludeNestedGroups = true;
      OnChanged();
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен:
    /// GroupId=0 и IncludeNestedGroups=true.
    /// </summary>
    public override bool IsEmpty
    {
      get { return GroupId == 0 && IncludeNestedGroups; }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Int32 Id = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return TestValue(Id);
    }

    /// <summary>
    /// Проверяет попадание значения поля в фильтр
    /// </summary>
    /// <param name="id">Проверяемое значение ссылочного поля</param>
    /// <returns>Результат проверки</returns>
    public bool TestValue(Int32 id)
    {
      if (GroupId == 0)
      {
        if (IncludeNestedGroups)
          return true;
        else
          return id == 0;
      }
      else
        return AuxFilterGroupIdList.Contains(id);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (IncludeNestedGroups)
      {
        if (GroupId == 0)
          return null;
        else
          return new IdsFilter(ColumnName, AuxFilterGroupIdList);
      }
      else
        return new ValueFilter(ColumnName, GroupId);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      GroupId = config.GetInt("GroupId");
      IncludeNestedGroups = config.GetBoolDef("IncludeNestedGroups", true);
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetInt("GroupId", GroupId);
      config.SetBool("IncludeNestedGroups", IncludeNestedGroups);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля ColumnName, если в фильтре выбрано единственное значение.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      if (IncludeNestedGroups)
        return;

      docValue.SetInteger(GroupId);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по виду документа
  /// Текущим значением числового поля является идентификатор таблицы документа DocType.TableId
  /// </summary>
  public class DocTableIdCommonFilter : SingleColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Столбец типа Int32, хранящий идентификатор вида документа из таблицы DocTables</param>
    public DocTableIdCommonFilter(string columnName)
      : base(columnName)
    {
      _CurrentTableId = 0;
    }

    #endregion

    #region Текущее состояние

    /// <summary>
    /// Выбранный тип документа (значение DocType.TableId) или 0, если фильтр не установлен.
    /// Свойства CurrentDocTypeName, CurrentDocTypeUI и CurrentTableId синхронизированы.
    /// </summary>
    public Int32 CurrentTableId
    {
      get { return _CurrentTableId; }
      set
      {
        if (value == _CurrentTableId)
          return;
        _CurrentTableId = value;
        OnChanged();
      }
    }
    private Int32 _CurrentTableId;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      CurrentTableId = 0;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return CurrentTableId == 0;
      }
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (CurrentTableId == 0)
        return null;
      return new ValueFilter(ColumnName, CurrentTableId);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int v = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return v == CurrentTableId;
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      CurrentTableId = config.GetInt("TableId");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetInt("TableId", CurrentTableId);
    }

    ///// <summary>
    ///// Возвращает свойство DBxDocType.PluralTitle, если задан фильтр по виду документов.
    ///// </summary>
    ///// <param name="ColumnValues">Значения полей</param>
    ///// <returns>Текстовые представления значений</returns>
    //protected override string[] GetColumnStrValues(object[] ColumnValues)
    //{
    //  string s;
    //  Int32 ThisTableId = DataTools.GetInt(ColumnValues[0]);
    //  if (ThisTableId == 0)
    //    s = "Нет";
    //  else
    //  {
    //    DBxDocType DocType = UI.DocProvider.DocTypes.FindByTableId(ThisTableId);
    //    if (DocType == null)
    //      s = "Неизвестный тип документа с TableId=" + ThisTableId.ToString();
    //    else
    //    {
    //      s = DocType.PluralTitle;
    //      if (UI.DebugShowIds)
    //        s += " (TableId=" + DocType.TableId.ToString() + ")";
    //    }
    //  }
    //  return new string[] { s };
    //}

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(Int32 rowValue)
    {
      if (IsEmpty)
        return true;
      return rowValue == CurrentTableId;
    }

    #endregion
  }

  #endregion

  #region Фильтры для двух полей

  /// <summary>
  /// Базовый класс для фильтров по одному полю.
  /// Определяет свойство ColumnName
  /// </summary>
  public abstract class TwoColumnsCommonFilter : DBxCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// Устанавливает свойства DBxCommonFilter.Code равным "ColumnName1_ColumnName2".
    /// Свойство DisplayName остается не инициализированным. Без дополнительной инициализации оно будет равно свойству Code.
    /// </summary>
    /// <param name="columnName1">Имя первого поля. Должно быть задано</param>
    /// <param name="columnName2">Имя второго поля. Должно быть задано</param>
    public TwoColumnsCommonFilter(string columnName1, string columnName2)
    {
      if (String.IsNullOrEmpty(columnName1))
        throw new ArgumentNullException("columnName1");
      if (String.IsNullOrEmpty(columnName2))
        throw new ArgumentNullException("columnName2");
      if (columnName1 == columnName2)
        throw new ArgumentException("Имена полей совпадают", "columnName2");

      _ColumnName1 = columnName1;
      _ColumnName2 = columnName2;

      base.Code = columnName1 + "_" + columnName2; // может быть изменено далее в пользовательском коде
    }

    #endregion

    #region Свойства и методы

    /// <summary>
    /// Имя первого фильтруемого поля.
    /// Задается в конструкторе и не может быть изменено
    /// </summary>
    public string ColumnName1 { get { return _ColumnName1; } }
    private string _ColumnName1;

    /// <summary>
    /// Имя второго фильтруемого поля.
    /// Задается в конструкторе и не может быть изменено
    /// </summary>
    public string ColumnName2 { get { return _ColumnName2; } }
    private string _ColumnName2;

    /// <summary>
    /// Получить список имен полей, которые необходимы для вычисления фильтра.
    /// Поля добавляются в список независимо от того, активен сейчас фильтр или нет.
    /// Добавляет в список поле ColumnName.
    /// </summary>
    /// <param name="list">Список для добавления полей</param>
    public override /*sealed*/ void GetColumnNames(DBxColumnList list)
    {
      list.Add(ColumnName1);
      list.Add(ColumnName2);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Проверяет наличие в документе поля ColumnName и вызывает метод OnInitNewDocValue() для значения поля
    /// </summary>
    /// <param name="newDoc">Созданный документ, в котором можно установить поля</param>
    protected override /*sealed*/ void OnInitNewDocValues(DBxSingleDoc newDoc)
    {
      int p1 = newDoc.Values.IndexOf(ColumnName1);
      int p2 = newDoc.Values.IndexOf(ColumnName2);
      if (p1 < 0 || p2 < 0)
        return;

      OnInitNewDocValues(newDoc.Values[p1], newDoc.Values[p2]);
    }

    /// <summary>
    /// Инициализация значения поля при создании нового документа.
    /// Метод вызывается, только когда фильтр установлен
    /// </summary>
    /// <param name="docValue1">Значение первого поля, которое можно установить</param>
    /// <param name="docValue2">Значение второго поля, которое можно установить</param>
    protected virtual void OnInitNewDocValues(DBxDocValue docValue1, DBxDocValue docValue2)
    {
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по интервалу дат
  /// В таблице должно быть два поля типа даты, которые составляют интервал дат.
  /// В фильтре задается дата. В просмотр попадают строки, в которых интервал дат
  /// включает в себя эту дату. Обрабатываются открытые и полуоткрытые интервалы,
  /// когда одно или оба поля содержат NULL
  /// Поддерживает специальный режим фильтра "Рабочая дата".
  /// </summary>
  public class DateRangeInclusionCommonFilter : TwoColumnsCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstDateColumnName">Имя поля типа "Дата", задающего начало диапазона</param>
    /// <param name="lastDateColumnName">Имя поля типа "Дата", задающего конец диапазона</param>
    public DateRangeInclusionCommonFilter(string firstDateColumnName, string lastDateColumnName)
      : base(firstDateColumnName, lastDateColumnName)
    {
      DisplayName = "Период";
    }

    #endregion

    #region Текущее состояние

    /// <summary>
    /// Текущее значение фильтра. Дата или null, если фильтра нет
    /// </summary>
    public DateTime? Date
    {
      get { return _Date; }
      set
      {
        if (value == _Date)
          return;
        _Date = value;
        if (_Date.HasValue)
        {
          if (_Date.Value != WorkDate)
            _UseWorkDate = false;
        }
        else
          _UseWorkDate = false;
        OnChanged();
      }
    }
    private DateTime? _Date;

    /// <summary>
    /// Использовать ли рабочую дату?
    /// </summary>
    public bool UseWorkDate
    {
      get { return _UseWorkDate; }
      set
      {
        if (value == _UseWorkDate)
          return;
        _UseWorkDate = value;
        if (_UseWorkDate && _Date.HasValue)
          _Date = WorkDate;
        OnChanged();
      }
    }
    private bool _UseWorkDate;

    #endregion

    #region Свойства, которые можно переопределить для использования рабочей даты

    /// <summary>
    /// Если переопределено, то может возвращать рабочую дату вместо текущей
    /// </summary>
    public virtual DateTime WorkDate { get { return DateTime.Today; } }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      Date = null;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !Date.HasValue;
      }
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeInclusionFilter(ColumnName1, ColumnName2, Date.Value);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<DateTime> dt1 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName1));
      Nullable<DateTime> dt2 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName2));
      return DataTools.DateInRange(Date.Value, dt1, dt2);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      Date = config.GetNullableDate("Date");
      UseWorkDate = config.GetBool("UseWorkDate");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableDate("Date", Date);
      config.SetBool("UseWorkDate", UseWorkDate);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue1">Проверяемое значение первого поля</param>
    /// <param name="rowValue2">Проверяемое значение второго поля</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(DateTime? rowValue1, DateTime? rowValue2)
    {
      if (IsEmpty)
        return true;
      return DataTools.DateInRange(Date.Value, rowValue1, rowValue2);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон дат.
  /// В фильтр входят строки, в диапазон дат которых попадает любая из дат в указанном диапазоне.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Компоненты времени не поддерживаются.
  /// </summary>
  public class DateRangeCrossCommonFilter : TwoColumnsCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstDateColumnName">Имя поля типа "Дата", задающего начало диапазона</param>
    /// <param name="lastDateColumnName">Имя поля типа "Дата", задающего конец диапазона</param>
    public DateRangeCrossCommonFilter(string firstDateColumnName, string lastDateColumnName)
      : base(firstDateColumnName, lastDateColumnName)
    {
      DisplayName = "Период";
    }

    #endregion

    #region Текущее состояние

    /// <summary>
    /// Текущее значение фильтра. Начальная дата диапазона или null, если фильтр не установлен или задан полуоткрытый интервал
    /// </summary>
    public DateTime? FirstDate
    {
      get { return _FirstDate; }
      set
      {
        if (value == _FirstDate)
          return;
        _FirstDate = value;
        OnChanged();
      }
    }
    private DateTime? _FirstDate;

    /// <summary>
    /// Текущее значение фильтра. Конечная дата диапазона или null, если фильтр не установлен или задан полуоткрытый интервал
    /// </summary>
    public DateTime? LastDate
    {
      get { return _LastDate; }
      set
      {
        if (value == _LastDate)
          return;
        _LastDate = value;
        OnChanged();
      }
    }
    private DateTime? _LastDate;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      FirstDate = null;
      LastDate = null;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !(_FirstDate.HasValue || _LastDate.HasValue);
      }
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeCrossFilter(ColumnName1, ColumnName2, FirstDate, LastDate);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<DateTime> dt1 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName1));
      Nullable<DateTime> dt2 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName2));
      return DataTools.DateRangeCrossed(FirstDate, LastDate, dt1, dt2);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      FirstDate = config.GetNullableDate("FirstDate");
      LastDate = config.GetNullableDate("LastDate");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableDate("FirstDate", FirstDate);
      config.SetNullableDate("LastDate", LastDate);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue1">Проверяемое значение первого поля</param>
    /// <param name="rowValue2">Проверяемое значение второго поля</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(DateTime? rowValue1, DateTime? rowValue2)
    {
      return DataTools.DateRangeCrossed(FirstDate, LastDate, rowValue1, rowValue2);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр на попадание года в интервал.
  /// В таблице должно быть два числовых поля, задающих первый и последний год.
  /// Строка проходит фильтр, если заданный в фильтре год (Value) попадает в диапазон.
  /// Обрабатываются значения типа NULL, задающие открытые интервалы
  /// </summary>
  public class YearRangeInclusionCommonFilter : TwoColumnsCommonFilter
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор для числовых полей
    /// </summary>
    /// <param name="firstYearFieldName">Имя числового поля, содержащего начальный год диапазона</param>
    /// <param name="lastYearFieldName">Имя числового поля, содержащего конечный год диапазона</param>
    public YearRangeInclusionCommonFilter(string firstYearFieldName, string lastYearFieldName)
      : base(firstYearFieldName, lastYearFieldName)
    {
      DisplayName = "Период";
    }

    #endregion

    #region Текущее состояние

    /// <summary>
    /// Выбранный год, если фильтр установлен. 0, если фильтр не задан
    /// </summary>
    public int Value
    {
      get { return _Value; }
      set
      {
        if (value == _Value)
          return;
        _Value = value;
        OnChanged();
      }
    }
    private int _Value;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      Value = 0;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return Value == 0;
      }
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (Value == 0)
        return null;

      throw new NotImplementedException();
      // TODO: return DBxFilter.CreateRangeOverYearFilter(FirstYearFieldName, LastYearFieldName, Value);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// </summary>
    /// <param name="rowValues">Значения полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int Year1 = DataTools.GetInt(rowValues.GetValue(ColumnName1));
      int Year2 = DataTools.GetInt(rowValues.GetValue(ColumnName2));

      if (Year1 > 0 && Value < Year1)
        return false;
      if (Year2 > 0 && Value > Year2)
        return false;
      return true;
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      Value = config.GetInt("Value");
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetInt("Value", Value);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (IsEmpty=true), возвращается true
    /// </summary>
    /// <param name="rowValue1">Проверяемое значение первого поля</param>
    /// <param name="rowValue2">Проверяемое значение второго поля</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(int rowValue1, int rowValue2)
    {
      if (IsEmpty)
        return true;
      if (rowValue1 > 0 && Value < rowValue1)
        return false;
      if (rowValue2 > 0 && Value > rowValue2)
        return false;
      return true;
    }

    #endregion
  }

  #endregion

  /// <summary>
  /// Фиктивный фильтр, который может ничего не делать или не пропускать ни одной строки
  /// </summary>
  public class DummyCommonFilter : DBxCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр.
    /// Свойство IsTrue имеет значение true
    /// </summary>
    /// <param name="code">Код для фильтра</param>
    public DummyCommonFilter(string code)
    {
      base.Code = code;
      _IsTrue = true;
    }

    #endregion

    #region Текущее значение

    /// <summary>
    /// Если true, то фильтр не установлен.
    /// Если false, то фильтр не пропускает ни одной строки.
    /// По умолчанию - true.
    /// </summary>
    public bool IsTrue
    {
      get { return _IsTrue; }
      set
      {
        if (value == _IsTrue)
          return;
        _IsTrue = value;
        OnChanged();
      }
    }
    private bool _IsTrue;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Устанавливает IsTrue=true
    /// </summary>
    public override void Clear()
    {
      IsTrue = true;
    }

    /// <summary>
    /// Возвращает IsTrue
    /// </summary>
    public override bool IsEmpty
    {
      get { return IsTrue; }
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="list">Игнорируется</param>
    public override void GetColumnNames(DBxColumnList list)
    {
    }

    /// <summary>
    /// Возвращает DummyFilter.AlwaysFalse, если фильтр установлен
    /// </summary>
    /// <returns>Фиктивный фильтр или null</returns>
    public override DBxFilter GetSqlFilter()
    {
      if (IsTrue)
        return null;
      else
        return DummyFilter.AlwaysFalse;
    }

    /// <summary>
    /// Возвращает IsTrue
    /// </summary>
    /// <param name="RowValues">Игнорируется</param>
    /// <returns>Значение свойства IsTrue</returns>
    protected override bool OnTestValues(INamedValuesAccess RowValues)
    {
      return IsTrue;
    }

    /// <summary>
    /// Запись значения
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetBool("Value", IsTrue);
    }

    /// <summary>
    /// Чтение значения
    /// </summary>
    /// <param name="Config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart Config)
    {
      IsTrue = Config.GetBool("Value");
    }

    #endregion
  }

  /// <summary>
  /// Фильтр с фиксированным SQL-запросом.
  /// С этим фильтром нельзя выполнить никаких действий, в том числе, очистить
  /// </summary>
  public class FixedSqlCommonFilter : DBxCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр.
    /// Свойство IsTrue имеет значение true
    /// </summary>
    /// <param name="code">Код для фильтра</param>
    /// <param name="filter">SQL-фильтр. Обязательно должен быть задан</param>
    public FixedSqlCommonFilter(string code, DBxFilter filter)
    {
      base.Code = code;

      if (filter == null)
        throw new ArgumentNullException("filter");

      _Filter = filter;
    }

    #endregion

    #region SQL-фильтр

    /// <summary>
    /// SQL-фильтр. Задается в конструкторе
    /// </summary>
    public DBxFilter Filter { get { return _Filter; } }
    private DBxFilter _Filter;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public override void Clear()
    {
    }

    /// <summary>
    /// Возвращает false
    /// </summary>
    public override bool IsEmpty { get { return false; } }

    /// <summary>
    /// Вовзращает список полей из фильтра
    /// </summary>
    /// <param name="list">Игнорируется</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      _Filter.GetColumnNames(list);
    }

    /// <summary>
    /// Возвращает Filter.
    /// </summary>
    /// <returns>Фиктивный фильтр или null</returns>
    public override DBxFilter GetSqlFilter()
    {
      return _Filter;
    }

    /// <summary>
    /// Проверяет значения на соответствие фильтру
    /// </summary>
    /// <param name="RowValues">Доступ к значениям полей</param>
    /// <returns>Прохождение фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess RowValues)
    {
      return _Filter.TestFilter(RowValues);
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="Config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart Config)
    {
    }

    #endregion
  }
}
