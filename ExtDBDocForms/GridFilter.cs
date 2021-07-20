using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using AgeyevAV.ExtForms;
using System.Windows.Forms;
using System.Data;
using AgeyevAV;
using AgeyevAV.ExtDB;
using AgeyevAV.ExtDB.Docs;
using AgeyevAV.Config;

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

namespace AgeyevAV.ExtForms.Docs
{
  #region Интерфейс IDBxDocSelFilter

  /// <summary>
  /// Интерфейс фильтра, поддерживающего работу с выборкамии документов
  /// </summary>
  public interface IDBxDocSelectionFilter
  {
    /// <summary>
    /// Добавить документы, соответствующие фильтру, в выборку
    /// </summary>
    /// <param name="docSel">Заполняемая выборка документов</param>
    void GetDocSel(DBxDocSelection docSel);

    /// <summary>
    /// Заменить документы в фильтре документами в выборке
    /// Если выборка не содержит подходящих значений, то фильтр не изменяется и
    /// возвращается false
    /// </summary>
    /// <param name="docSel">Выборка из буфера обмена</param>
    /// <returns>trye, если удалось что-нибудь использовать</returns>
    bool ApplyDocSel(DBxDocSelection docSel);
  }

  #endregion

#if XXX
  /// <summary>
  /// Абстрактный базовый класс для одиночного фильтра, применимого к табличному просмотру.
  /// Реализация клиента
  /// </summary>
  public abstract class GridFilter : IEFPGridFilter
  {
  #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="Name">Имя фильтра</param>
    public GridFilter(string Name)
    {
      if (String.IsNullOrEmpty(Name))
        throw new ArgumentNullException("Name");
      FName = Name;
    }

  #endregion

  #region Общие свойства

    /// <summary>
    /// Имя фильтра для поиска. Задается в конструкторе
    /// Обычно совпадает с именем поля
    /// Имена фильтров в пределах коллекции должны быть уникальными
    /// При необходимости, свойство может быть установлено после создания объекта,
    /// например, если есть два различных фильтра, использующих одно поле.
    /// Свойство не может устанавливаться после присоединения фильтра к GridFilters
    /// </summary>
    public string Code
    {
      get { return FName; }
      set
      {
        if (FOwner != null)
          throw new InvalidOperationException("Свойство Name не может устанавливаться после присоединения фильтра к списку фильтров");
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException("Name", "Имя фильтра должно быть задано");
        FName = value;
      }
    }
    private string FName;

    /// <summary>
    /// Имя фильтра, которое появляется в левой части диалога фильтра
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(FDisplayName))
          return Code;
        else
          return FDisplayName;
      }
      set { FDisplayName = value; }
    }
    private string FDisplayName;

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public GridFilters Owner { get { return FOwner; } }
    internal GridFilters FOwner;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return FTag; } set { FTag = value; } }
    private object FTag;

    /// <summary>
    /// Идентификатор базы данных
    /// Переопределено для RefDocGridFilter
    /// </summary>
    public virtual string DBIdentity { get { return String.Empty; } }

  #endregion

  #region Переопределяемые методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public abstract string FilterText { get;}

    /// <summary>
    /// Вывод окна диалога для одного фильтра
    /// </summary>
    /// <returns>True, если фильтр был изменен</returns>
    public abstract bool ShowFilterDialog();

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public abstract bool IsEmpty { get;}

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public abstract DBxFilter GetSqlFilter();

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка
    /// В реализации переопределенного метода следует использовать GetTestedValue() 
    /// для извлечения значения.
    /// </summary>
    /// <param name="ColumnNames">Имена полей</param>
    /// <param name="ColumnValues">Значения полей. Длина массива должна соответствовать <paramref name="ColumnNames"/>.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    public abstract bool TestValues(string[] ColumnNames, object[] ColumnValues);


    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="Config">Секция конфигурации</param>
    public abstract void ReadConfig(CfgPart Config);

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="Config">Секция конфигурации</param>
    public abstract void WriteConfig(CfgPart Config);

    /// <summary>
    /// Получить список имен полей, которые необходимы для вычисления фильтра.
    /// Поля добавляются в список независимо от того, активен сейчас фильтр или нет.
    /// </summary>
    /// <param name="List">Список для добавления полей</param>
    public abstract void GetColumnNames(DBxColumnList List);

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Некоторые фильтры могут попытаться установить начальные значения для 
    /// полей документа в соответствии со своим состоянием.
    /// </summary>
    /// <param name="NewDoc">Созданный документ, в котором можно установить поля</param>
    public virtual void InitNewDocValues(DBxSingleDoc NewDoc)
    {
    }

    /// <summary>
    /// Если фильтр реализует установку значения фильтра "по строке", то 
    /// переопределенный метод должен извлечь значения "своих" полей из строки и
    /// вернуть true, если установка возможна. Сама установка выполняется методом
    /// SetAsCurrRow().
    /// Непереопределенный метод возвращает false.
    /// </summary>
    /// <param name="Row">Строка, откуда берутся значения</param>
    /// <returns>Признак поддержки</returns>
    public virtual bool CanAsCurrRow(DataRow Row)
    {
      return false;
    }

    /// <summary>
    /// Установка значения фильтра "по строке".
    /// Непереопределенный метод генерирует исключение.
    /// </summary>
    /// <param name="Row">Строка, откуда берутся значения</param>
    public virtual void SetAsCurrRow(DataRow Row)
    {
      throw new InvalidOperationException("Фильтр \"" + ToString() + "\" не поддерживает установку значения по текущей строке");
    }

    /// <summary>
    /// Возвращает "DisplayName = FilterText"
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      return DisplayName + " = " + FilterText;
    }

  #endregion

  #region События

    /// <summary>
    /// Событие вызывается при изменении текущего значения фильтра
    /// Это событие вызывается до события в объекте-владельце
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// Вызов события Changed и уведомление объекта-владельца об изменении фильтра
    /// Метод может быть доопределен, если фильтр имеют собственную систему нотификации
    /// </summary>
    protected virtual void OnChanged()
    {
      if (Changed != null)
        Changed(this, EventArgs.Empty);
      if (Owner != null)
        Owner.OnChanged();
    }

  #endregion

  #region Вспомогательные методы

    /// <summary>
    /// Метод извлечения значения для TestValues
    /// </summary>
    /// <param name="ColumnNames"></param>
    /// <param name="ColumnValues"></param>
    /// <param name="ColumnName"></param>
    /// <returns></returns>
    protected object GetTestedValue(string[] ColumnNames, object[] ColumnValues, string ColumnName)
    {
      int p = Array.IndexOf<string>(ColumnNames, ColumnName);
      if (p < 0)
        throw new ArgumentException("Поле для тестирования \"" + ColumnName + "\"не найдено");
      return ColumnValues[p];
    }


  #endregion

  #region IInitNewDocValues Members

    /// <summary>
    /// Не реализовано
    /// </summary>
    /// <param name="SavingDoc"></param>
    /// <param name="ErrorMessages"></param>
    public void ValidateDocValues(DBxSingleDoc SavingDoc, ErrorMessageList ErrorMessages)
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
    /// <param name="ColumnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected virtual string[] GetColumnStrValues(object[] ColumnValues)
    {
      return null;
    }

  #endregion
  }

#endif

  /// <summary>
  /// Расширение DBxCommonFilterSet интерфейсом IEFPGridFilterSet
  /// </summary>
  public class DBxClientFilterSet : DBxCommonFilterSet, IEFPGridFilterSet
  {
    #region IEnumerable<IEFPGridFilter> Members

    IEnumerator<IEFPGridFilter> IEnumerable<IEFPGridFilter>.GetEnumerator()
    {
      return new ConvertEnumerator<IEFPGridFilter>(base.GetEnumerator());
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для DBxClientFilters и GridFilters.
  /// Реализует интерфейс IEFPGridFilters, нужный для редактирования списка фильтров.
  /// Выполняет перехват некоторых исключений и выводит сообщения об ошибке.
  /// </summary>
  public abstract class DBxClientFiltersBase : DBxCommonFilters, IEFPGridFilters
  {
    #region Переопределенные методы

    /// <summary>
    /// Вызывается при возникновении ошибки чтении конфигурации в DBxCommonFilter.ReadConfig().
    /// Выводит сообщение с помощью EFPApp.ShowException() и очищает фильтр
    /// </summary>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="filter">Фильтр, для которого возникло исключение</param>
    /// <param name="cfg">Считываемая секция конфигурации</param>
    protected override void OnReadConfigError(Exception exception, DBxCommonFilter filter, CfgPart cfg)
    {
      EFPApp.ShowException(exception, "Ошибка загрузки состояния фильтра \"" + filter.DisplayName + "\"");
      filter.Clear();
    }

    /// <summary>
    /// Внутренний метод для вызова события Changed.
    /// Перехватывает исключение, которое может возникнуть в методе базового класса, и выводит сообщение с помощью EFPApp.ShowException().
    /// </summary>
    /// <param name="filter">Фильтр, который вызвал событие</param>
    protected override void OnChanged(DBxCommonFilter filter)
    {
      try
      {
        base.OnChanged(filter);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки события DBxClientFilters.Changed");
      }
    }

    #endregion

    #region Методы добавления / удаления фильтров

    /// <summary>
    /// Выполняет дополнительную проверку, что добавляемый фильтр реализует интерфейс IEFPGridFilter и в фильтре установлено свойство UseSqlFilter.
    /// </summary>
    /// <param name="item">Добавляемый фильтр</param>
    protected override void OnBeforeAdd(DBxCommonFilter item)
    {
      base.OnBeforeAdd(item);

      if (!(item is IEFPGridFilter))
      {
        Exception e = new ArgumentException("Фильтр должен реализовывать интерфейс IEFPGridFilter", "item");
        if (item != null)
        {
          e.Data["Item.GetType()"] = item.GetType().ToString();
          e.Data["Item.DisplayName"] = item.DisplayName;
          e.Data["Item.Code"] = item.Code;
        }
        throw e;
      }
    }

    #endregion

    #region IEFPGridFilters Members

    IEFPGridFilter IEFPGridFilters.this[string Name] { get { return (IEFPGridFilter)(base[Name]); } }

    /// <summary>
    /// Добавляет набор фильтров
    /// </summary>
    /// <param name="filterSet">Добавляемый набор</param>
    public void Add(IEFPGridFilterSet filterSet)
    {
      if (filterSet == null)
        throw new ArgumentNullException("filterSet");
      foreach (IEFPGridFilter Item in filterSet)
        Add((DBxCommonFilter)Item);
    }

    #endregion

    #region IList<IEFPGridFilter> Members

    int IList<IEFPGridFilter>.IndexOf(IEFPGridFilter item)
    {
      return IndexOf((DBxCommonFilter)item);
    }

    void IList<IEFPGridFilter>.Insert(int index, IEFPGridFilter item)
    {
      Insert(index, (DBxCommonFilter)item);
    }

    IEFPGridFilter IList<IEFPGridFilter>.this[int index]
    {
      get
      {
        return (IEFPGridFilter)(this[index]);
      }
      set
      {
        this[index] = (DBxCommonFilter)value;
      }
    }

    #endregion

    #region ICollection<IEFPGridFilter> Members

    void ICollection<IEFPGridFilter>.Add(IEFPGridFilter item)
    {
      Add((DBxCommonFilter)item);
    }

    bool ICollection<IEFPGridFilter>.Contains(IEFPGridFilter item)
    {
      DBxCommonFilter item2 = item as DBxCommonFilter;
      if (item2 == null)
        return false;
      else
        return Contains(item2);
    }

    void ICollection<IEFPGridFilter>.CopyTo(IEFPGridFilter[] array, int arrayIndex)
    {
      for (int i = 0; i < Count; i++)
        array[arrayIndex + i] = (IEFPGridFilter)(this[i]);
    }

    bool ICollection<IEFPGridFilter>.Remove(IEFPGridFilter item)
    {
      DBxCommonFilter item2 = item as DBxCommonFilter;
      if (item2 == null)
        return false;
      else
        return Remove(item2);
    }

    #endregion

    #region IEnumerable<IEFPGridFilter> Members


    private class Enumerator2 : IEnumerator<IEFPGridFilter>
    {
      #region Конструктор

      public Enumerator2(DBxClientFiltersBase owner)
      {
        _Owner = owner;
        _Index = -1;
      }

      #endregion

      #region Свойства

      DBxClientFiltersBase _Owner;
      int _Index;

      #endregion

      #region IEnumerator<IEFPGridFilter> Members

      public IEFPGridFilter Current
      {
        get { return (IEFPGridFilter)(_Owner[_Index]); }
      }

      public void Dispose()
      {
      }

      object IEnumerator.Current
      {
        get { return _Owner[_Index]; }
      }

      public bool MoveNext()
      {
        _Index++;
        return _Index < _Owner.Count;
      }

      public void Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    IEnumerator<IEFPGridFilter> IEnumerable<IEFPGridFilter>.GetEnumerator()
    {
      return new Enumerator2(this);
    }


    #endregion
  }

  /// <summary>
  /// Коллекция фильтров, которую можно использовать в параметрах отчетов
  /// </summary>
  public class DBxClientFilters : DBxClientFiltersBase
  {
    #region Прочие методы

    /// <summary>
    /// Заполнение таблички фильтров окна отчета.
    /// В табличку добавляются только установленные фильтры с DBxCommonFilter.IsEmpty=false.
    /// </summary>
    /// <param name="filterInfo">Список для таблички фильтров EFPReportParams.FilterInfo</param>
    public void AddFilterInfo(EFPReportFilterItems filterInfo)
    {
      if (filterInfo == null)
        throw new ArgumentNullException("filterInfo");

      for (int i = 0; i < Count; i++)
      {
        if (!this[i].IsEmpty)
        {
          EFPReportFilterItem fi = new EFPReportFilterItem(this[i].DisplayName, ((IEFPGridFilter)this[i]).FilterText);
          if (EFPApp.ShowListImages)
          {
            string imageKey = null;
            IEFPGridFilterWithImageKey filter2 = this[i] as IEFPGridFilterWithImageKey;
            if (filter2 != null)
              imageKey = filter2.FilterImageKey;
            if (!String.IsNullOrEmpty(imageKey))
              fi.ImageKey = imageKey;
          }
          filterInfo.Add(fi);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Коллекция фильтров табличного просмотра.
  /// В отличие от DBxClientFilters, требует, чтобы все элементы создавали фильтры для Sql-запросов, то есть, чтобы DBxCommonFilter.UseSqlFilter=true.
  /// </summary>
  public class GridFilters : DBxClientFiltersBase
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список фильтров
    /// </summary>
    public GridFilters()
    {
    }

    #endregion

    #region Методы добавления / удаления фильтров

    /// <summary>
    /// Выполняет дополнительную проверку, что добавляемый фильтр реализует интерфейс IEFPGridFilter и в фильтре установлено свойство UseSqlFilter.
    /// </summary>
    /// <param name="item">Добавляемый фильтр</param>
    protected override void OnBeforeAdd(DBxCommonFilter item)
    {
      base.OnBeforeAdd(item);

      //if (!(Item is IEFPGridFilter))
      //  throw new ArgumentException("Фильтр должен реализовывать интерфейс IEFPGridFilter");
      if (!item.UseSqlFilter)
        throw new InvalidOperationException("Свойство DBxCommonFilter.UseSqlFilter должно быть установлено");
    }

    #endregion

    #region Прочие методы и свойства



#if XXXXX
    /// <summary>
    /// Информация о фильтрах, необходимая для передачи на сервер
    /// Не используется в прикладной части программы
    /// </summary>
    /// <returns></returns>
    public object GetFilterValues()
    {
      ArrayList res = new ArrayList();
      for (int i = 0; i < Count; i++)
      {
        if (!this[i].IsEmpty)
        {
          string ClassName = this[i].GetType().FullName;
          res.Add(ClassName);
          res.Add(this[i].CurrentState);
        }
      }

      if (res.Count == 0)
        return null;

      object[] res2 = new object[res.Count];
      res.CopyTo(res2);
      return res2;
    }
#endif



    #endregion
  }
}
