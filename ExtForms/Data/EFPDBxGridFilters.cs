﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using FreeLibSet.Forms;
using System.Windows.Forms;
using System.Data;

using FreeLibSet.Data;
using FreeLibSet.Config;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Data
{

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
        throw ExceptionFactory.ArgStringIsNullOrEmpty("Name");
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
  /// Расширение <see cref="DBxCommonFilterSet"/> интерфейсом <see cref="IEFPGridFilterSet"/>
  /// </summary>
  public class EFPDBxGridFilterSet : DBxCommonFilterSet, IEFPGridFilterSet
  {
    #region IEnumerable<IEFPGridFilter> Members

    IEnumerator<IEFPGridFilter> IEnumerable<IEFPGridFilter>.GetEnumerator()
    {
      return new ConvertEnumerable<IEFPGridFilter>.Enumerator(base.GetEnumerator());
    }

    #endregion
  }

  /// <summary>
  /// Коллекция фильтров табличного просмотра.
  /// Также можно использовать в параметрах отчетов.
  /// Реализует интерфейс <see cref="IEFPGridFilters"/>, нужный для редактирования списка фильтров.
  /// Выполняет перехват некоторых исключений и выводит сообщения об ошибке.
  /// </summary>
  public class EFPDBxGridFilters : DBxCommonFilters, IEFPGridFilters
  {
    #region Переопределенные методы

    /// <summary>
    /// Вызывается при возникновении ошибки чтении конфигурации в <see cref="DBxCommonFilter.ReadConfig(CfgPart)"/>.
    /// Выводит сообщение с помощью <see cref="EFPApp.ShowException(Exception, string)"/> и очищает фильтр.
    /// </summary>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="filter">Фильтр, для которого возникло исключение</param>
    /// <param name="cfg">Считываемая секция конфигурации</param>
    protected override void OnReadConfigError(Exception exception, DBxCommonFilter filter, CfgPart cfg)
    {
      EFPApp.ShowException(exception, String.Format(Res.DBxGridFilters_ErrTitle_ReadConfig, filter.DisplayName));
      filter.Clear();
    }

    /// <summary>
    /// Внутренний метод для вызова события <see cref="DBxCommonFilter.Changed"/>.
    /// Перехватывает исключение, которое может возникнуть в методе базового класса, и выводит сообщение с помощью <see cref="EFPApp.ShowException(Exception, string)"/>.
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
        EFPApp.ShowException(e);
      }
    }

    #endregion

    #region Методы добавления / удаления фильтров

    /// <summary>
    /// Выполняет дополнительную проверку, что добавляемый фильтр реализует интерфейс <see cref="IEFPGridFilter"/>.
    /// </summary>
    /// <param name="item">Добавляемый фильтр</param>
    protected override void OnBeforeAdd(DBxCommonFilter item)
    {
      base.OnBeforeAdd(item); // Этот метод только выполняет проверки. Наши проверки выполняем после
      // item!=null - проверено

      if (!(item is IEFPGridFilter))
      {
        ArgumentException e = ExceptionFactory.ArgNoType("item", item, typeof(IEFPGridFilter));
        e.Data["Item.GetType()"] = item.GetType().ToString();
        e.Data["Item.DisplayName"] = item.DisplayName;
        e.Data["Item.Code"] = item.Code;
        throw e;
      }
    }

    #endregion

    #region IEFPGridFilters Members

    IEFPGridFilter IEFPGridFilters.this[string code] { get { return (IEFPGridFilter)(base[code]); } }

    /// <summary>
    /// Добавляет набор фильтров
    /// </summary>
    /// <param name="filterSet">Добавляемый набор</param>
    public void Add(IEFPGridFilterSet filterSet)
    {
      if (filterSet == null)
        throw new ArgumentNullException("filterSet");
      foreach (IEFPGridFilter item in filterSet)
        Add((DBxCommonFilter)item);
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

      public Enumerator2(EFPDBxGridFilters owner)
      {
        _Owner = owner;
        _Index = -1;
      }

      #endregion

      #region Свойства

      EFPDBxGridFilters _Owner;
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

    #region Для отчетов EFPReport

    /// <summary>
    /// Заполнение таблички фильтров окна отчета.
    /// В табличку добавляются только установленные фильтры с <see cref="DBxCommonFilter.IsEmpty"/>=false.
    /// </summary>
    /// <param name="filterInfo">Список для таблички фильтров <see cref="EFPReportParams.FilterInfo"/></param>
    public void AddFilterInfo(EFPReportFilterItems filterInfo)
    {
      if (filterInfo == null)
        throw new ArgumentNullException("filterInfo");

      for (int i = 0; i < Count; i++)
      {
        if (!this[i].IsEmpty)
        {
          EFPReportFilterItem fi = new EFPReportFilterItem(this[i].DisplayName);
          fi.Value = ((IEFPGridFilter)this[i]).FilterText;
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
}
