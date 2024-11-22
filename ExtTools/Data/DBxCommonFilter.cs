// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using System.Data;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Calendar;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Базовый класс для реализации устанавливаемых фильтров.
  /// Фильтры используются в табличных просмотрах и параметрах отчетов.
  /// </summary>
  public abstract class DBxCommonFilter : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр, установив свойство <see cref="UseSqlFilter"/> = true.
    /// </summary>
    public DBxCommonFilter()
    {
      _UseSqlFilter = true;
    }

    #endregion

    #region Код

    /// <summary>
    /// Код фильтра. Используется при чтении / записи фильтра как имя секции конфигурации.
    /// Свойство должно быть установлено в конструкторе производного класса или в пользовательском коде до присоединения к коллекции <see cref="DBxCommonFilters"/>.
    /// </summary>
    public string Code
    {
      get { return _Code; }
      set
      {
        // Не проверяем пустое значение, так как код требуется только когда фильтр присоединяется к коллекции

        CheckNoOwner();
        _Code = value;
      }
    }
    private string _Code;

    /// <summary>
    /// Проверяет, что фильтр еще не был присоединен к коллекции <see cref="DBxCommonFilters"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Если фильтр уже присоединен</exception>
    protected void CheckNoOwner()
    {
      if (_Owner != null)
        throw new InvalidOperationException("Выполнение действия допускается только до присоединения фильтра к коллекции");
    }

    #endregion

    #region Коллекция - владелец

    /// <summary>
    /// Коллекция - владелец.
    /// Свойство возвращает null до присоединения фильтра к коллекции.
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
    /// Вызывает событие <see cref="Changed"/> для этого объекта и событие коллекции <see cref="DBxCommonFilters.Changed"/>.
    /// До присоединения фильтра к коллекции метод не выполняет никаких действий, в том числе, не вызывает события <see cref="Changed"/>.
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
    /// Если свойство не установлено в явном виде, возвращается <see cref="Code"/>.
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
    /// Переопределено для классов в ExtDBDDocs.dll.
    /// Непереопределенный метод возвращает пустую строку.
    /// </summary>
    public virtual string DBIdentity { get { return String.Empty; } }

    /// <summary>
    /// Возвращает true, если фильтр используется при формировании SQL-запроса.
    /// По умолчанию возвращает true. Для фильтра табличного просмотра обязан возвращать true.
    /// Свойство может быть сброшено в false для фильтра отчета, если фильтрация выполняется по значению, вычисляемому вручную.
    /// В этом случае имя поля является фиктивным.
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
    /// Возвращает свойство <see cref="DisplayName"/>.
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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Имена полей и значений должны содержать необходимые поля, иначе будет сгенерирована ошибка.
    /// Вызывает виртуальный метод <see cref="OnTestValues(INamedValuesAccess)"/>, если фильтр установлен и свойство <see cref="UseSqlFilter"/> равно true.
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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Имена полей и значений должны содержать необходимые поля, иначе будет сгенерирована ошибка.
    /// Метод не вызывается, если фильтр не установлен, следовательно, дополнительная проверка свойства <see cref="IsEmpty"/> не нужна.
    /// Также метод не вызывается при <see cref="UseSqlFilter"/>=false.
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
    /// Записать параметры фильтра в секцию конфигурации.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public abstract void WriteConfig(CfgPart cfg);

    #endregion

    #region Инициализация строки документа

    /// <summary>
    /// Вызывается при создании новой строки документа из просмотра.
    /// Некоторые фильтры могут попытаться установить начальные значения для 
    /// полей в соответствии со своим состоянием.
    /// </summary>
    /// <param name="newValues">Поля, которые можно установить</param>
    public void InitNewValues(IDBxExtValues newValues)
    {
      if (IsEmpty)
        return;
      if (!UseSqlFilter)
        return;
      OnInitNewValues(newValues);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Некоторые фильтры могут попытаться установить начальные значения для 
    /// полей документа в соответствии со своим состоянием.
    /// </summary>
    /// <param name="newValues">Поля, которые можно установить</param>
    protected virtual void OnInitNewValues(IDBxExtValues newValues)
    {
    }

    /// <summary>
    /// Если фильтр реализует установку значения фильтра "по строке", то 
    /// переопределенный метод должен извлечь значения "своих" полей из строки и
    /// вернуть true, если установка возможна. Сама установка выполняется методом
    /// <see cref="SetAsCurrRow(DataRow)"/>.
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
    /// <param name="savingValues"></param>
    /// <param name="errorMessages"></param>
    public void ValidateValues(IDBxExtValues savingValues, ErrorMessageList errorMessages)
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
    /// значений для отображения в сообщении.
    /// Оригинальный метод возвращает null, что означает отказ от вывода значений.
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
  /// Набор из нескольких фильтров, которые можно вместе добавить в коллекцию <see cref="DBxCommonFilters"/>.
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
    /// Фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
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
    /// Опрашиваются фильтры в списке, вызывая метод <see cref="DBxCommonFilter.TestValues(INamedValuesAccess)"/> пока один из фильтров не вернет false.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей. В списке должны быть все поля, полученные вызовом <see cref="GetColumnNames(DBxColumnList)"/></param>
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
    /// Если предполагается проверка значений для нескольких строк, используйте объект <see cref="DBxColumnValueArray"/> и перегрузку метода, принимающую <see cref="INamedValuesAccess"/>.
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
    /// Пустые фильтры и фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
    /// </summary>
    /// <returns>Объект <see cref="DBxFilter"/>, соответствующий активным фильтрам. Если фильтров 
    /// несколько, то будет возвращен <see cref="AndFilter"/>.</returns>
    public DBxFilter GetSqlFilter()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          filters.Add(this[i].GetSqlFilter());
      }
      return AndFilter.FromList(filters);
    }


    /// <summary>
    /// Очистка всех фильтров. Вызывает <see cref="DBxCommonFilters.ClearFilter(string)"/> для каждого фильтра.
    /// Список фильтров не меняется. Вызов не зависит от свойства <see cref="NamedList{DBxCommonFilter}.IsReadOnly"/>.
    /// Не путать этот метод с <see cref="NamedList{DBxCommonFilter}.Clear()"/>, который очищает сам список фильтров.
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
    /// Возвращает true, если нет ни одного активного фильтра, для которого <see cref="DBxCommonFilter.UseSqlFilter"/>=true
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
    /// Возвращает true, если нет ни одного активного фильтра, для которого <see cref="DBxCommonFilter.UseSqlFilter"/>=false
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
  /// Коллекция фильтров <see cref="DBxCommonFilter"/>
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
    /// Если true, то все фильтры в наборе должны иметь значение свойства <see cref="DBxCommonFilter.UseSqlFilter"/>=true.
    /// По умолчанию - false, проверка не выполняется.
    /// </summary>
    public bool SqlFilterRequired
    {
      get { return _SqlFilterRequired; }
      set
      {
        if (value == _SqlFilterRequired)
          return;

        if (value)
        {
          for (int i = 0; i < Count; i++)
          {
            if (!this[i].UseSqlFilter)
              throw new InvalidOperationException("Для фильтра \"" + this[i].DisplayName + "\" свойство UseSqlFilter=false");
          }
        }
        _SqlFilterRequired = value;
      }
    }
    private bool _SqlFilterRequired;

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
      if (SqlFilterRequired && (!item.UseSqlFilter))
        throw new InvalidOperationException("Свойство DBxCommonFilter.UseSqlFilter=false, в то время как свойство DBxCommnonFilter.SqlFilterRequired=true");

      base.OnBeforeAdd(item);
    }

    /// <summary>
    /// Устанавливает свойство <see cref="DBxCommonFilter.Owner"/>.
    /// </summary>
    /// <param name="item">Добавленный фильтр</param>
    protected override void OnAfterAdd(DBxCommonFilter item)
    {
      base.OnAfterAdd(item);
      item.Owner = this;
    }

    /// <summary>
    /// Очищает свойство <see cref="DBxCommonFilter.Owner"/>
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
    /// Событие фильтра <see cref="DBxCommonFilter.Changed"/> вызывается до этого события.
    /// Если обработчик события вызывает установку какого-либо фильтра, то посылка
    /// вложенного извещения не выполняется.
    /// </summary>
    public event EventHandler Changed;

    private bool _InsideChanged = false;

    /// <summary>
    /// Внутренний метод для вызова события <see cref="Changed"/>.
    /// Предотвращает реентрантный вызов.
    /// </summary>
    /// <param name="filter">Фильтр, который вызвал событие, или null</param>
    internal protected virtual void OnChanged(DBxCommonFilter filter)
    {
      int p = IndexOf(filter);
      if (p < 0)
        //throw new ArgumentException("Фильтр не относится к коллекции");
        return; // обойдемся без выброса исключения

      if (_InsideChanged)
        return;

      _InsideChanged = true;
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
        _InsideChanged = false;
      }
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает true, если среди запрашиваемых фильтров хотя бы один установлен.
    /// Если в списке <paramref name="codes"/> присутствуют несуществующие коды фильтров, то они пропускаются,
    /// как будто для них <see cref="DBxCommonFilter.IsEmpty"/>=true, без выдачи сообщения об ошибке.
    /// Чтобы проверить наличие установки любого фильтра, используйте свойство <see cref="DBxCommonFilter.IsEmpty"/>.
    /// </summary>
    /// <param name="codes">Список кодов проверяемых </param>
    /// <returns>Наличие установленных фильтров</returns>
    public bool IsAnyNotEmpty(string codes)
    {
      if (string.IsNullOrEmpty(codes))
        return false;

      if (codes.IndexOf(',') >= 0)
      {
        string[] a = codes.Split(',');
        for (int i = 0; i < a.Length; i++)
        {
          DBxCommonFilter filter = this[a[i]];
          if (filter != null)
          {
            if (!filter.IsEmpty)
              return true;
          }
        }
        return false;
      }
      else
      {
        DBxCommonFilter filter = this[codes];
        if (filter != null)
        {
          if (!filter.IsEmpty)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Поиск фильтра по пользовательскому имени фильтра (свойству <see cref="DBxCommonFilter.DisplayName"/>).
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
    /// Фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
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
    /// Фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
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
    /// Опрашиваются фильтры в списке, вызывая метод <see cref="DBxCommonFilter.TestValues(INamedValuesAccess)"/> пока один из фильтров не вернет false.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей. В списке должны быть все поля, полученные вызовом <see cref="GetColumnNames(DBxColumnList)"/></param>
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
    /// Опрашиваются фильтры в списке, вызывая метод <see cref="DBxCommonFilter.TestValues(INamedValuesAccess)"/> пока один из фильтров не вернет false.
    /// Неактивные фильтры не добавляются.
    /// Фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей. В списке должны быть все поля, полученные вызовом <see cref="GetColumnNames(DBxColumnList)"/></param>
    /// <returns>True, если строка проходит все фильтры</returns>
    public bool TestValues(INamedValuesAccess rowValues)
    {
      DBxCommonFilter badFilter;
      return TestValues(rowValues, out badFilter);
    }

    /// <summary>
    /// Проверка попадания в фильтр.
    /// Если предполагается проверка значений для нескольких строк, используйте объект <see cref="DBxColumnValueArray"/> и перегрузку метода, принимающую <see cref="INamedValuesAccess"/>.
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
    /// Если предполагается проверка значений для нескольких строк, используйте объект <see cref="DBxColumnValueArray"/> и перегрузку метода, принимающую <see cref="INamedValuesAccess"/>.
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
    /// Пустые фильтры и фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
    /// Если нет действующих фильтров, возвращается null.
    /// </summary>
    /// <returns>Объект <see cref="DBxFilter"/>, соответствующий активным фильтрам. Если фильтров 
    /// несколько, то будет возвращаен <see cref="AndFilter"/></returns>
    public DBxFilter GetSqlFilter()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          filters.Add(this[i].GetSqlFilter());
      }
      return AndFilter.FromList(filters);
    }

    /// <summary>
    /// Получение SQL-фильтра для фильтрации набора данных <see cref="DataView"/>.
    /// Возвращает строку фильтра, которую можно присвоить свойству <see cref="DataView.RowFilter"/>.
    /// Пустые фильтры и фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
    /// Если нет действующих фильтров, возвращается пустая строка.
    /// См. метод <see cref="GetSqlFilter()"/>.
    /// </summary>
    public string DataViewRowFilter
    {
      get
      {
        DBxFilter sqlFilter = GetSqlFilter();
        if (sqlFilter == null)
          return String.Empty;
        else
          return sqlFilter.ToString();
      }
    }

    /// <summary>
    /// Выполняет очистку фильтра, если он существует.
    /// Если нет фильтра с кодом <paramref name="code"/>, никаких действий не выполняется.
    /// Удобно использовать в обработчике <see cref="OnChanged(DBxCommonFilter)"/> для реализации взаимных блокировок фильтров, когда список фильтров не является постоянным.
    /// </summary>
    /// <param name="code">Код фильтра</param>
    public void ClearFilter(string code)
    {
      DBxCommonFilter filter = this[code];
      if (filter != null)
        filter.Clear();
    }

    /// <summary>
    /// Выполняет очистку фильтров с заданными кодами.
    /// Если в списке <paramref name="codes"/> указаны фильтра с несуществующими кодами, то они пропускаются.
    /// Удобно использовать в обработчике <see cref="OnChanged(DBxCommonFilter)"/> для реализации взаимных блокировок фильтров, когда список фильтров не является постоянным.
    /// </summary>
    /// <param name="codes">Список кодов фильтров, разделенных запятыми</param>
    public void ClearFilters(string codes)
    {
      if (String.IsNullOrEmpty(codes))
        return;
      if (codes.IndexOf(',') >= 0)
      {
        string[] a = codes.Split(',');
        for (int i = 0; i < a.Length; i++)
          ClearFilter(a[i]);
      }
      else
        ClearFilter(codes);
    }

    /// <summary>
    /// Очистка всех фильтров. Вызывает <see cref="DBxCommonFilter.Clear()"/> для каждого фильтра.
    /// Список фильтров не меняется. Метод работает независимо от свойства <see cref="NamedList{DBxCommonFilter}.IsReadOnly"/>.
    /// Не путать этот метод с <see cref="NamedList{DBxCommonFilter}.Clear()"/>, который очищает сам список фильтров.
    /// </summary>
    public void ClearAllFilters()
    {
      for (int i = 0; i < Count; i++)
        this[i].Clear();
    }


    /// <summary>
    /// Идентификатор базы данных.
    /// Возвращается значение <see cref="DBxCommonFilter.DBIdentity"/> для первого фильтра в списке,
    /// вернувшего непустое значение.
    /// Если ни один из фильтров не вернул значение, возвращается пустая строка.
    /// Это означает, что в списке нет ссылочных фильтров и фильтры можно копировать/вставлять
    /// через буфер обмена между любыми программами.
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
    /// Возвращает true, если нет ни одного активного фильтра.
    /// Чтобы определить наличие установленного фильтра среди определенного подмножества, используйте <see cref="IsAnyNotEmpty(string)"/>.
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
    /// Возвращает true, если нет ни одного активного фильтра, для которого <see cref="DBxCommonFilter.UseSqlFilter"/>=true
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
    /// Возвращает true, если нет ни одного активного фильтра, для которого <see cref="DBxCommonFilter.UseSqlFilter"/>=false
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
    /// Инициализация текущих значений фильтров на основании сохраненных ранее настроек.
    /// Для каждого фильтра предусмотрена отдельная часть, которая всегда
    /// существует после записи фильтров (включая пустые).
    /// Если для какого-либо фильтра (или всех фильтров) нет части, значит пользователь
    /// еще не настраивал фильтр. В этом случае сохраняется значение по умолчанию.
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

        CfgPart cfg2 = config.GetChild(this[i].Code, false);
        if (cfg2 != null)
        {
          try
          {
            this[i].ReadConfig(cfg2);
          }
          catch (Exception e)
          {
            OnReadConfigError(e, this[i], cfg2);
          }
        }
      }
    }

    /// <summary>
    /// Вызывается при возникновении ошибки чтении конфигурации в <see cref="DBxCommonFilter.ReadConfig(CfgPart)"/>.
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
    /// Чтение свойства вызывает <see cref="WriteConfig(CfgPart)"/>, а запись - <see cref="ReadConfig(CfgPart)"/>. При этом используется <see cref="TempCfg"/> и преобразование в XML-формат.
    /// Это свойство удобно использовать в отчетах для передачи данных от клиента к серверу.
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

    #region IInitNewValues Members

    /// <summary>
    /// Вызывается из DocTypeUI.PerformEditing (ExtDBDocsForms.dll)
    /// </summary>
    /// <param name="newValues"></param>
    public void InitNewValues(IDBxExtValues newValues)
    {
      for (int i = 0; i < Count; i++)
        this[i].InitNewValues(newValues);
    }


    /// <summary>
    /// Вызывает <see cref="DBxCommonFilter.ValidateValues(IDBxExtValues, ErrorMessageList)"/> для всех фильтров в списке
    /// </summary>
    /// <param name="savingValues"></param>
    /// <param name="errorMessages"></param>
    public void ValidateValues(IDBxExtValues savingValues, ErrorMessageList errorMessages)
    {
      for (int i = 0; i < Count; i++)
        this[i].ValidateValues(savingValues, errorMessages);
    }

    #endregion
  }

  #region Фильтры для одного поля

  /// <summary>
  /// Базовый класс для фильтров по одному полю (большинство фильтров).
  /// Определяет свойство <see cref="OneColumnCommonFilter.ColumnName"/>.
  /// </summary>
  public abstract class OneColumnCommonFilter : DBxCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// Устанавливает свойства <see cref="ColumnName"/>, <see cref="DBxCommonFilter.Code"/> и <see cref="DBxCommonFilter.DisplayName"/> равными <paramref name="columnName"/>.
    /// </summary>
    /// <param name="columnName">Имя поля. Должно быть задано</param>
    public OneColumnCommonFilter(string columnName)
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
    private readonly string _ColumnName;

    /// <summary>
    /// Получить список имен полей, которые необходимы для вычисления фильтра.
    /// Поля добавляются в список независимо от того, активен сейчас фильтр или нет.
    /// Добавляет в список поле <see cref="ColumnName"/>.
    /// </summary>
    /// <param name="list">Список для добавления полей</param>
    public override /*sealed */ void GetColumnNames(DBxColumnList list)
    {
      list.Add(ColumnName);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Проверяет наличие в документе поля <see cref="ColumnName"/> и вызывает метод <see cref="DBxCommonFilter.OnInitNewValues(IDBxExtValues)"/> для значения поля.
    /// </summary>
    /// <param name="newValues">Поля новой строки</param>
    protected override /*sealed*/ void OnInitNewValues(IDBxExtValues newValues)
    {
      int p = newValues.IndexOf(ColumnName);
      if (p < 0)
        return;

      OnInitNewValue(newValues[p]);
    }

    /// <summary>
    /// Инициализация значения поля при создании нового документа.
    /// Метод вызывается, только когда фильтр установлен.
    /// </summary>
    /// <param name="newValue">Значение поля, которое можно установить</param>
    protected virtual void OnInitNewValue(DBxExtValue newValue)
    {
    }

    #endregion
  }

  #region Фильтры по строковому полю

  /// <summary>
  /// Простой фильтр по значению текстового поля (проверка поля на равенство 
  /// определенному значению).
  /// Использует SQL-фильтр <see cref="StringValueFilter"/>.
  /// Не дает возможности фильтровать по пустому значению поля, так как пустая строка считается отсутствием фильтра.
  /// </summary>
  public class StringValueCommonFilter : OneColumnCommonFilter
  {
    #region Конструкторы

    /// <summary>
    /// Создает фильтр.
    /// По умолчанию фильтр является чувствительным к регистру
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public StringValueCommonFilter(string columnName)
      : base(columnName)
    {
      _IgnoreCase = false;
      _Value = String.Empty;
    }

    #endregion

    #region Управляющее свойство

    /// <summary>
    /// Если true, то сравнение будет выполняться без учета регистра.
    /// По умолчанию false - регистр не учитывается.
    /// </summary>
    public bool IgnoreCase
    {
      get { return _IgnoreCase; }
      set
      {
        CheckNoOwner();
        _IgnoreCase = value;
      }
    }
    private bool _IgnoreCase;

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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      string v = DataTools.GetString(rowValues.GetValue(ColumnName));
      return String.Equals(v, Value, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new StringValueFilter(ColumnName, Value, IgnoreCase);
    }

    /// <summary>
    /// Инициализация значения поля при создании нового документа.
    /// Метод вызывается, только когда фильтр установлен.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewValue(DBxExtValue docValue)
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
    /// Записать параметры фильтра в секцию конфигурации
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
    /// Если фильтр не установлен (<see cref="IsEmpty"/>=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;
      return String.Equals(rowValue, Value, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по совпадению начала строки (SQL-фильтр <see cref="StartsWithFilter"/>)
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

    // При изменениях не забыть продублировать их в классе StartsWithGridFilter в ExtDbDocForms.dll

    /// <summary>
    /// Создает <see cref="StartsWithFilter"/>
    /// </summary>
    /// <returns>Новый объект SQL-фильтра</returns>
    public override DBxFilter GetSqlFilter()
    {
      return new StartsWithFilter(ColumnName, Value, IgnoreCase);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей</param>
    /// <returns>True, если условие фильтра выполняется</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);
      return DataTools.GetString(v).StartsWith(Value, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    /// <summary>
    /// Метод ничего не делает, в отличие от базового класса.
    /// </summary>
    /// <param name="docValue"></param>
    protected override void OnInitNewValue(DBxExtValue docValue)
    {
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (<see cref="DBxCommonFilter.IsEmpty"/>=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public new bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;
      return rowValue.StartsWith(Value, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    #endregion
  }

  #region Перечисление CodesFilterMode

  /// <summary>
  /// Возможные значения свойства <see cref="CodeCommonFilter.Mode"/>
  /// </summary>
  public enum CodeFilterMode
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
  /// </summary>
  public class CodeCommonFilter : OneColumnCommonFilter
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
      _Mode = CodeFilterMode.NoFilter;
      _Codes = DataTools.EmptyStrings;
      _EmptyCode = false;
    }

    #endregion

    #region Текущее состояние фильтра

    /// <summary>
    /// Текущий режим фильтра.
    /// Устанавливается методом <see cref="SetFilter(CodeFilterMode, string[], bool)"/>.
    /// </summary>
    public CodeFilterMode Mode { get { return _Mode; } }
    private CodeFilterMode _Mode;

    /// <summary>
    /// Список включаемых или исключаемых кодов.
    /// Устанавливается методом <see cref="SetFilter(CodeFilterMode, string[], bool)"/>.
    /// </summary>
    public string[] Codes { get { return _Codes; } }
    private string[] _Codes;

    /// <summary>
    /// Включить ли в фильтр строки без кода.
    /// Свойство действительно только при <see cref="CanBeEmpty"/>=true.
    /// Устанавливается методом <see cref="SetFilter(CodeFilterMode, string[], bool)"/>.
    /// </summary>
    public bool EmptyCode { get { return _EmptyCode; } }
    private bool _EmptyCode;

    /// <summary>
    /// Установка (или сброс) фильтра
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    /// <param name="codes">Список кодов</param>
    /// <param name="emptyCode">Должен ли использоваться пустой код</param>
    public void SetFilter(CodeFilterMode mode, string[] codes, bool emptyCode)
    {
      if (mode == CodeFilterMode.NoFilter)
      {
        Clear();
        return;
      }

      if (codes == null)
        codes = DataTools.EmptyStrings;

      if (codes.Length == 0 && (!emptyCode))
      {
        Clear();
        return;
      }

      _Mode = mode;
      _Codes = codes;
      _EmptyCode = emptyCode;
      OnChanged();
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// true, если поддерживаются пустые коды. Если false, то предполагается, что 
    /// поле всегда имеет установленное значение, а флажок "Код не установлен" недоступен.
    /// Свойство устанавливается в конструкторе.
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } }
    private readonly bool _CanBeEmpty;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      if (IsEmpty)
        return;
      _Mode = CodeFilterMode.NoFilter;
      _Codes = DataTools.EmptyStrings;
      _EmptyCode = false;
      OnChanged();
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty { get { return Mode == CodeFilterMode.NoFilter; } }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      if (Codes.Length > 0)
        filters.Add(new ValuesFilter(Code, Codes));
      if (EmptyCode)
        filters.Add(new ValueFilter(Code, String.Empty, typeof(string)));

      DBxFilter filter = OrFilter.FromList(filters);
      if (Mode == CodeFilterMode.Exclude)
      {
        filter = new NotFilter(filter);
        if (CanBeEmpty && (!EmptyCode))
          filter = new OrFilter(filter, new ValueFilter(Code, String.Empty, typeof(string))); // 17.11.2021
      }
      return filter;
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      string rowValue = DataTools.GetString(rowValues.GetValue(ColumnName));
      return TestValue(rowValue);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      CodeFilterMode mode = cfg.GetEnumDef<CodeFilterMode>("Mode", CodeFilterMode.NoFilter);
      string[] codes = cfg.GetString("Codes").Split(',');
      bool emptyCode = false;
      if (CanBeEmpty)
        emptyCode = cfg.GetBool("EmptyCode");
      SetFilter(mode, codes, emptyCode);
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      switch (Mode)
      {
        case CodeFilterMode.Include:
          cfg.SetString("Mode", "Include");
          break;
        case CodeFilterMode.Exclude:
          cfg.SetString("Mode", "Exclude");
          break;
        default:
          cfg.Remove("Mode");
          break;
      }

      if (Codes.Length == 0)
        cfg.Remove("Codes");
      else
        cfg.SetString("Codes", String.Join(",", Codes));

      if (CanBeEmpty)
        cfg.SetBool("EmptyCode", EmptyCode);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (<see cref="DBxCommonFilter.IsEmpty"/>=true), возвращается true
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;

      bool flag;
      if (String.IsNullOrEmpty(rowValue))
        flag = EmptyCode;
      else
        flag = Array.IndexOf<string>(Codes, rowValue) >= 0;
      if (Mode == CodeFilterMode.Exclude)
        return !flag;
      else
        return flag;
    }

    #endregion
  }

  #endregion

  #region Фильтры ValueFilter

  /// <summary>
  /// Базовый класс для построения фильтров по значению поля (SQL-фильтр <see cref="ValueFilter"/>).
  /// Активность фильтра устанавливается с помощью шаблона <see cref="Nullable{T}"/>.
  /// Поддерживается только проверка на равенство <see cref="CompareKind.Equal"/>.
  /// </summary>
  /// <typeparam name="T">Тип значения поля (должен быть структурой, а не классом)</typeparam>
  public abstract class ValueCommonFilterBase<T> : OneColumnCommonFilter
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
      _Kind = CompareKind.Equal;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее значение фильтра.
    /// Содержит null, если фильтр не установлен.
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

    /// <summary>
    /// Знак операции сравнения. По умолчанию <see cref="CompareKind.Equal"/>.
    /// Это свойство можно устанавливать только до присоединения фильтра к списку.
    /// </summary>
    public CompareKind Kind
    {
      get { return _Kind; }
      set
      {
        CheckNoOwner();
        //        if (value == _Kind)
        //          return;
        _Kind = value;
        //        OnChanged();
      }
    }
    private CompareKind _Kind;

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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);

      //if (Value.Value.Equals(v))
      //  return true;

      //// 11.08.2014
      //// Если значение Value равно значению "по умолчанию" (0, false), то в фильтр
      //// должны попадать записи со значением поля NULL
      //if (Value.Value.Equals(default(T)))
      //{
      //  if (v == null)
      //    return true;

      //  if (v is DBNull)
      //    return true;
      //}

      //return false;

      if (v == null || v is DBNull)
        v = default(T);
      return CompareFilter.TestFilter(v, Value.Value, Kind);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new ValueFilter(ColumnName, Value.Value, Kind, typeof(T));
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля <see cref="OneColumnCommonFilter.ColumnName"/>, если в фильтре выбрано единственное значение.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewValue(DBxExtValue docValue)
    {
      if (Kind == CompareKind.Equal)
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
    /// Например, с помощью вызова return cfg.GetInt("Value").
    /// </summary>
    /// <param name="cfg">Секция конфигурации для чтения фильтра</param>
    /// <returns>Прочитанное значение</returns>
    protected abstract T DoReadConfigValue(CfgPart cfg);

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
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
    /// Например, с помощью вызова cfg.SetInt("Value", Value).
    /// </summary>
    /// <param name="cfg">Секция конфигурации для записи фильтра</param>
    /// <param name="value">Записываемое значение</param>
    protected abstract void DoWriteConfigValue(CfgPart cfg, T value);

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (<see cref="DBxCommonFilter.IsEmpty"/>=true), возвращается true.
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
  /// Простой фильтр по логическому полю (SQL-фильтр <see cref="ValueFilter"/>).
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
    /// Вызывает <see cref="CfgPart.GetBool(string)"/>
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <returns>Значение</returns>
    protected override bool DoReadConfigValue(CfgPart cfg)
    {
      return cfg.GetBool("Value");
    }

    /// <summary>
    /// Вызывает <see cref="CfgPart.SetBool(string, bool)"/>
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
  /// Простой фильтр по полю типа <see cref="Int32"/> с фильтрацией по единственному значению (SQL-фильтр <see cref="ValueFilter"/>).
  /// Если поле может принимать фиксированный набор значений, то следует использовать
  /// фильтр <see cref="EnumCommonFilter"/>.
  /// </summary>
  public class IntValueCommonFilter : ValueCommonFilterBase<Int32>
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
    /// Вызывает <see cref="CfgPart.GetInt(string)"/>
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <returns>Значение</returns>
    protected override int DoReadConfigValue(CfgPart cfg)
    {
      return cfg.GetInt("Value");
    }

    /// <summary>
    /// Вызывает <see cref="CfgPart.SetBool(string, bool)"/>
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
      int value = DataTools.GetInt(columnValues[0]);
      return new string[] { value.ToString() };
    }

    #endregion
  }


  /// <summary>
  /// Фильтр по году для числового поля или поля типа "Дата" (SQL-фильтр <see cref="ValueFilter"/>)
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
    private readonly bool _IsDateColumn;

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

  #region Фильтры ValuesFilter

  /// <summary>
  /// Базовый класс для построения IN-фильтров по значению поля (SQL-фильтр <see cref="ValuesFilter"/>).
  /// </summary>
  /// <typeparam name="T">Тип значения поля </typeparam>
  public abstract class ValuesCommonFilterBase<T> : OneColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public ValuesCommonFilterBase(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущее значение фильтра.
    /// Содержит null, если фильтр не установлен.
    /// </summary>
    public T[] Values
    {
      get { return _Values; }
      set
      {
        if (value != null)
        {
          if (value.Length == 0)
            value = null;
        }
        if (object.Equals(value, _Values))
          return;
        _Values = value;
        OnChanged();
      }
    }
    private T[] _Values;


    #endregion

    #region Переопределяемые свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      Values = null;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return Values == null;
      }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);

      return ValuesFilter.TestFilter(v, Values, DBxColumnType.Unknown);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new ValuesFilter(ColumnName, Values);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля <see cref="OneColumnCommonFilter.ColumnName"/>, если в фильтре выбрано единственное значение.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewValue(DBxExtValue docValue)
    {
      if (Values != null && Values.Length == 1)
        docValue.SetValue(Values[0]);
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.Clear();
      if (Values != null)
      {
        cfg.SetInt("Count", Values.Length);
        for (int i = 0; i < Values.Length; i++)
          cfg.SetString("Value" + StdConvert.ToString(i + 1), ToCfgText(Values[i]));
      }
    }

    /// <summary>
    /// Преобразовать одно значение в текст для записи в секцию конфигурации
    /// </summary>
    /// <param name="value">Записываемое значение</param>
    /// <returns>Текст</returns>
    protected abstract string ToCfgText(T value);

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      int n = cfg.GetInt("Count");
      if (n == 0)
        Values = null;
      else
      {
        T[] a = new T[n];
        for (int i = 0; i < n; i++)
        {
          string s = cfg.GetString("Value" + StdConvert.ToString(i + 1));
          a[i] = FromCfgText(s);
        }

        Values = a;
      }
    }

    /// <summary>
    /// Преобразование текста в одно значениее
    /// </summary>
    /// <param name="s">Прочитанный текст</param>
    /// <returns>Значение</returns>
    protected abstract T FromCfgText(string s);

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (<see cref="DBxCommonFilter.IsEmpty"/>=true), возвращается true.
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(T rowValue)
    {
      if (IsEmpty)
        return true;

      return ValuesFilter.TestFilter(rowValue, Values, DBxColumnType.Unknown);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр IN по строковому полю.
  /// Допускается наличие пустой строки в списке значений. В этом случае фильтр пройдут строки, содержащие значение NULL.
  /// </summary>
  public class StringValuesCommonFilter : ValuesCommonFilterBase<string>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public StringValuesCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected override string ToCfgText(string value)
    {
      return value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    protected override string FromCfgText(string s)
    {
      return s;
    }

    #endregion
  }

  #endregion

  #region Фильтры по диапазонам значений

  /// <summary>
  /// Базовый класс фильтра для одного поля, в котором можно задавать диапазон значений.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public abstract class RangeCommonFilterBase<T> : OneColumnCommonFilter
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public RangeCommonFilterBase(string columnName)
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
      // 06.11.2024
      // Нельзя очищать значения по одному. Метод OnChanged() будет вызван после очистки первого значения.
      // Обработчик события может прочитать некомплектное значение
      //FirstValue = null;
      //LastValue = null;

      if (IsEmpty)
        return;

      _FirstValue = null;
      _LastValue = null;
      OnChanged();
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен (свойства <see cref="FirstValue"/> и <see cref="LastValue"/> вместе равны null).
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !(FirstValue.HasValue || LastValue.HasValue);
      }
    }

    /// <summary>
    /// Если <see cref="FirstValue"/> и <see cref="LastValue"/> установлены в одно и то же значение, отличное от null, то значение поля документа инициализируется выбранным значением.
    /// </summary>
    /// <param name="docValue"></param>
    protected override void OnInitNewValue(DBxExtValue docValue)
    {
      if (FirstValue.HasValue && LastValue.HasValue && Object.Equals(FirstValue, LastValue))
      {
        if (NullIsZero && FirstValue.Value.Equals(default(T)))
          docValue.SetNull();
        else
          docValue.SetValue(FirstValue.Value);
      }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для одного поля, содержащего целочисленное значение (SQL-фильтр <see cref="NumRangeFilter"/>).
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public class IntRangeCommonFilter : RangeCommonFilterBase<int>
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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
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

      return DataTools.IsInRange<Int32>(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.IsInRange<Int32>(0, FirstValue, LastValue))
        filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(int)), filter);
      return filter;
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
    /// Записать параметры фильтра в секцию конфигурации
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
  /// Фильтр табличного просмотра для одного поля, содержащего числовое значение с плавающей точкой  (SQL-фильтр <see cref="NumRangeFilter"/>).
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public class SingleRangeCommonFilter : RangeCommonFilterBase<float>
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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
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

      return DataTools.IsInRange<Single>(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.IsInRange<Single>(0, FirstValue, LastValue))
        filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(float)), filter);
      return filter;
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
    /// Записать параметры фильтра в секцию конфигурации
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
  /// Фильтр табличного просмотра для одного поля, содержащего числовое значение с плавающей точкой  (SQL-фильтр <see cref="NumRangeFilter"/>).
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public class DoubleRangeCommonFilter : RangeCommonFilterBase<double>
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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
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

      return DataTools.IsInRange<Double>(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.IsInRange<Double>(0, FirstValue, LastValue))
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
    /// Записать параметры фильтра в секцию конфигурации
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
  /// Фильтр табличного просмотра для одного поля, содержащего числовое значение с плавающей точкой (SQL-фильтр <see cref="NumRangeFilter"/>).
  /// Можно задавать диапазон значений, которые должны проходить фильтр.
  /// Допускаются полуоткрытые интервалы.
  /// </summary>
  public class DecimalRangeCommonFilter : RangeCommonFilterBase<decimal>
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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
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

      return DataTools.IsInRange<Decimal>(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.IsInRange<Decimal>(0, FirstValue, LastValue))
        filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(float)), filter);
      return filter;
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
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDecimal("FirstValue", FirstValue);
      cfg.SetNullableDecimal("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по диапазону дат для одного поля (SQL-фильтр <see cref="DateRangeFilter"/>).
  /// Поддерживаются полуоткрытые интервалы.
  /// Если фильтр установлен, то пустые значения поля не проходят фильтр.
  /// </summary>
  public class DateRangeCommonFilter : RangeCommonFilterBase<DateTime>
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

    #region Переопределяемые методы


    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      DateTime? v = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName));
      if (v.HasValue)
        return DataTools.DateInRange(v.Value, FirstValue, LastValue);
      else
        return false;
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeFilter(ColumnName, FirstValue, LastValue);

    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableDate("FirstValue");
      LastValue = cfg.GetNullableDate("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDate("FirstValue", FirstValue);
      cfg.SetNullableDate("LastValue", LastValue);
    }

    /// <summary>
    /// Использует <see cref="DateRangeFormatter.Default"/> для преобразования в строку значения поля
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      return new string[] { DateRangeFormatter.Default.ToString(DataTools.GetNullableDateTime(columnValues[0]), true) };
    }

    #endregion
  }

  #endregion

  #region Прочие фильтры

  /// <summary>
  /// Фильтр по одному или нескольким значениям числового поля, каждому из
  /// которых соответствует текстовое представление. Используется SQL-фильтр <see cref="ValuesFilter"/> (фильтр "IN ()").
  /// Предполагается, что элементы перечисления имеют последовательные значения 0,1,2,...
  /// Каждое значение проходит или не проходит фильтр, что определяется массивом флагов <see cref="EnumCommonFilter.FilterFlags"/>.
  /// Если фильтр установлен, то значения поля, выходящие за диапазон значений, считаются не проходящими фильтр.
  /// </summary>
  public class EnumCommonFilter : OneColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Конструктор фильтра
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="itemCount">Количество элементов в перечислении. 
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
    private readonly int _ItemCount;

    /// <summary>
    /// Текущее значение фильтра. Содержит массив флагов, соответствующих числовым
    /// значениям поля 0,1,2, ... (<see cref="ItemCount"/>-1).
    /// Если фильтр не установлен, то свойство содержит null.
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
    /// Альтернативная установка фильтра.
    /// Выбор единственного выбранного значения.
    /// Значение (-1) соответствует пустому фильтру (<see cref="FilterFlags"/>=null)
    /// </summary>
    public int SingleFilterItemIndex
    {
      get
      {
        if (FilterFlags == null)
          return -1;
        int index = -1;
        for (int i = 0; i < FilterFlags.Length; i++)
        {
          if (FilterFlags[i])
          {
            if (index < 0)
              index = i;
            else
              // Установлено больше одного флага
              return -1;
          }
        }
        return index;
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
    /// Возвращает true, если фильтр не установлен (<see cref="FilterFlags"/>=null).
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

      List<int> values = new List<int>();
      for (int i = 0; i < FilterFlags.Length; i++)
      {
        if (FilterFlags[i])
          values.Add(i);
      }
      DBxFilter filter = new ValuesFilter(ColumnName, values.ToArray());
      // 18.10.2019
      // Форматировщик OnFormatValuesFilter() теперь сам учитывает наличие пустого значения среди Values и дополнительная проверка на NULL не нужна
      //if (FilterFlags[0])
      //  Filter = new OrFilter(Filter, new ValueFilter(ColumnName, null, typeof(Int16)));
      return filter;
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int value = DataTools.GetInt(rowValues.GetValue(ColumnName));
      if (value < 0 || value > FilterFlags.Length)
        return false;
      else
        return FilterFlags[value];
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
    /// Записать параметры фильтра в секцию конфигурации
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
    /// Устанавливает начальное значение поля <see cref="OneColumnCommonFilter.ColumnName"/>, если в фильтре выбрано единственное значение.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить поля</param>
    protected override void OnInitNewValue(DBxExtValue docValue)
    {
      if (SingleFilterItemIndex >= 0)
        docValue.SetInteger(SingleFilterItemIndex);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (<see cref="IsEmpty"/>=true), возвращается true.
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
  /// Возможные состояния фильтра <see cref="NullNotNullCommonFilter"/>
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
  /// типа "Дата". Для ссылочных полей может использоваться RefDocCommonFilter из ExtDBDocs.dll, который, в том числе, поддерживает и проверку на NULL/NOT NULL).
  /// Создает SQL-фильтры <see cref="ValueFilter"/> или <see cref="NotNullFilter"/>.
  /// </summary>
  public class NullNotNullCommonFilter : OneColumnCommonFilter
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
    /// Требуется для создания объекта <see cref="NotNullFilter"/>
    /// </summary>
    public Type ColumnType { get { return _ColumnType; } }
    private readonly Type _ColumnType;

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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object rowValue = rowValues.GetValue(ColumnName);
      return TestValue(rowValue);
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
    /// Записать параметры фильтра в секцию конфигурации
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
    /// Если фильтр не установлен (<see cref="IsEmpty"/>=true), возвращается true.
    /// Под "NULL" понимаются значения null и <see cref="DBNull"/>. Остальные значения считаются "NOT NULL".
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(object rowValue)
    {
      bool isNull = (rowValue == null || rowValue is DBNull);
      switch (Value)
      {
        case NullNotNullFilterValue.Null:
          return isNull;
        case NullNotNullFilterValue.NotNull:
          return !isNull;
        default:
          return true;
      }
    }

    #endregion
  }

  #endregion

  #endregion

  #region Фильтры для двух полей

  /// <summary>
  /// Базовый класс для фильтров по двум полям полю.
  /// </summary>
  public abstract class TwoColumnsCommonFilter : DBxCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// Устанавливает свойство <see cref="DBxCommonFilter.Code"/> равным "ColumnName1_ColumnName2".
    /// Свойство <see cref="DBxCommonFilter.DisplayName"/> остается не инициализированным. Без дополнительной инициализации оно будет равно свойству <see cref="DBxCommonFilter.Code"/>.
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
    /// Задается в конструкторе и не может быть изменено.
    /// </summary>
    public string ColumnName1 { get { return _ColumnName1; } }
    private readonly string _ColumnName1;

    /// <summary>
    /// Имя второго фильтруемого поля.
    /// Задается в конструкторе и не может быть изменено.
    /// </summary>
    public string ColumnName2 { get { return _ColumnName2; } }
    private readonly string _ColumnName2;

    /// <summary>
    /// Получить список имен полей, которые необходимы для вычисления фильтра.
    /// Поля добавляются в список независимо от того, активен сейчас фильтр или нет.
    /// Добавляет в список поля <see cref="ColumnName1"/> и <see cref="ColumnName2"/>.
    /// </summary>
    /// <param name="list">Список для добавления полей</param>
    public override /*sealed*/ void GetColumnNames(DBxColumnList list)
    {
      list.Add(ColumnName1);
      list.Add(ColumnName2);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Проверяет наличие в документе полей <see cref="ColumnName1"/> и <see cref="ColumnName2"/> и вызывает метод <see cref="OnInitNewValues(DBxExtValue, DBxExtValue)"/> для значений полей.
    /// </summary>
    /// <param name="newValues">Созданный документ, в котором можно установить поля</param>
    protected override /*sealed*/ void OnInitNewValues(IDBxExtValues newValues)
    {
      int p1 = newValues.IndexOf(ColumnName1);
      int p2 = newValues.IndexOf(ColumnName2);
      if (p1 < 0 || p2 < 0)
        return;

      OnInitNewValues(newValues[p1], newValues[p2]);
    }

    /// <summary>
    /// Инициализация значения поля при создании нового документа.
    /// Метод вызывается, только когда фильтр установлен.
    /// </summary>
    /// <param name="docValue1">Значение первого поля, которое можно установить</param>
    /// <param name="docValue2">Значение второго поля, которое можно установить</param>
    protected virtual void OnInitNewValues(DBxExtValue docValue1, DBxExtValue docValue2)
    {
    }

    #endregion
  }

  #region Фильтры по охвату значения диапазоном

  /// <summary>
  /// Базовый класс для фильтров по двум полям, задающих диапазон числовых значений.
  /// Управляющим является свойство <see cref="RangeInclusionCommonFilterBase{T}.Value"/>. Если фильтр установлен, то проверяется, что диапазон, который образуют значения полей,
  /// охватывает <see cref="RangeInclusionCommonFilterBase{T}.Value"/>.
  /// Поддерживаются полуоткрытые интервалы.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class RangeInclusionCommonFilterBase<T> : TwoColumnsCommonFilter
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// </summary>
    /// <param name="columnName1">Имя первого поля. Должно быть задано</param>
    /// <param name="columnName2">Имя второго поля. Должно быть задано</param>
    public RangeInclusionCommonFilterBase(string columnName1, string columnName2)
      : base(columnName1, columnName2)
    {
      _Value = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение фильтра.
    /// Заданное значение должно попадать в диапазон значений, образованный полями <see cref="TwoColumnsCommonFilter"/>.
    /// Содержит null, если фильтр не установлен.
    /// </summary>
    public T? Value
    {
      get { return _Value; }
      set
      {
        if (Object.Equals(value, _Value))
          return;
        _Value = value;
        OnChanged();
      }
    }
    private T? _Value;

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Устанавливает <see cref="Value"/> = null
    /// </summary>
    public override void Clear()
    {
      Value = null;
    }

    /// <summary>
    /// Возвращает true, если <see cref="Value"/> не содержит значения
    /// </summary>
    public override bool IsEmpty
    {
      get { return !Value.HasValue; }
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, задающих диапазон числовых значений.
  /// Поддерживаются полуоткрытые интервалы.
  /// Используется SQL-фильтр <see cref="NumRangeInclusionFilter"/>.
  /// </summary>
  public class IntRangeInclusionCommonFilter : RangeInclusionCommonFilterBase<Int32>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// </summary>
    /// <param name="columnName1">Имя первого поля. Должно быть задано</param>
    /// <param name="columnName2">Имя второго поля. Должно быть задано</param>
    public IntRangeInclusionCommonFilter(string columnName1, string columnName2)
      : base(columnName1, columnName2)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int? v1 = DataTools.GetNullableInt(rowValues.GetValue(ColumnName1));
      int? v2 = DataTools.GetNullableInt(rowValues.GetValue(ColumnName1));
      return DataTools.IsInRange<Int32>(Value.Value, v1, v2);
    }

    /// <summary>
    /// Создает <see cref="NumRangeInclusionFilter"/>.
    /// </summary>
    /// <returns>Новый объект фильтра</returns>
    public override DBxFilter GetSqlFilter()
    {
      return new NumRangeInclusionFilter(ColumnName1, ColumnName2, Value.Value);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      Value = cfg.GetNullableInt("Value");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableInt("Value", Value);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, задающих диапазон числовых значений.
  /// Поддерживаются полуоткрытые интервалы.
  /// Используется SQL-фильтр <see cref="NumRangeInclusionFilter"/>.
  /// </summary>
  public class SingleRangeInclusionCommonFilter : RangeInclusionCommonFilterBase<Single>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// </summary>
    /// <param name="columnName1">Имя первого поля. Должно быть задано</param>
    /// <param name="columnName2">Имя второго поля. Должно быть задано</param>
    public SingleRangeInclusionCommonFilter(string columnName1, string columnName2)
      : base(columnName1, columnName2)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      float? v1 = DataTools.GetNullableSingle(rowValues.GetValue(ColumnName1));
      float? v2 = DataTools.GetNullableSingle(rowValues.GetValue(ColumnName1));
      return DataTools.IsInRange<Single>(Value.Value, v1, v2);
    }

    /// <summary>
    /// Создает <see cref="NumRangeInclusionFilter"/>.
    /// </summary>
    /// <returns>Новый объект фильтра</returns>
    public override DBxFilter GetSqlFilter()
    {
      return new NumRangeInclusionFilter(ColumnName1, ColumnName2, Value.Value);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      Value = cfg.GetNullableSingle("Value");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableSingle("Value", Value);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, задающих диапазон числовых значений.
  /// Поддерживаются полуоткрытые интервалы.
  /// Используется SQL-фильтр <see cref="NumRangeInclusionFilter"/>.
  /// </summary>
  public class DoubleRangeInclusionCommonFilter : RangeInclusionCommonFilterBase<Double>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// </summary>
    /// <param name="columnName1">Имя первого поля. Должно быть задано</param>
    /// <param name="columnName2">Имя второго поля. Должно быть задано</param>
    public DoubleRangeInclusionCommonFilter(string columnName1, string columnName2)
      : base(columnName1, columnName2)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      double? v1 = DataTools.GetNullableDouble(rowValues.GetValue(ColumnName1));
      double? v2 = DataTools.GetNullableDouble(rowValues.GetValue(ColumnName1));
      return DataTools.IsInRange<Double>(Value.Value, v1, v2);
    }

    /// <summary>
    /// Создает <see cref="NumRangeInclusionFilter"/>.
    /// </summary>
    /// <returns>Новый объект фильтра</returns>
    public override DBxFilter GetSqlFilter()
    {
      return new NumRangeInclusionFilter(ColumnName1, ColumnName2, Value.Value);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      Value = cfg.GetNullableDouble("Value");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDouble("Value", Value);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, задающих диапазон числовых значений.
  /// Поддерживаются полуоткрытые интервалы.
  /// Используется SQL-фильтр <see cref="NumRangeInclusionFilter"/>.
  /// </summary>
  public class DecimalRangeInclusionCommonFilter : RangeInclusionCommonFilterBase<Decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// </summary>
    /// <param name="columnName1">Имя первого поля. Должно быть задано</param>
    /// <param name="columnName2">Имя второго поля. Должно быть задано</param>
    public DecimalRangeInclusionCommonFilter(string columnName1, string columnName2)
      : base(columnName1, columnName2)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      decimal? v1 = DataTools.GetNullableDecimal(rowValues.GetValue(ColumnName1));
      decimal? v2 = DataTools.GetNullableDecimal(rowValues.GetValue(ColumnName1));
      return DataTools.IsInRange<Decimal>(Value.Value, v1, v2);
    }

    /// <summary>
    /// Создает <see cref="NumRangeInclusionFilter"/>.
    /// </summary>
    /// <returns>Новый объект фильтра</returns>
    public override DBxFilter GetSqlFilter()
    {
      return new NumRangeInclusionFilter(ColumnName1, ColumnName2, Value.Value);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      Value = cfg.GetNullableDecimal("Value");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDecimal("Value", Value);
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// Фильтр на попадание года в интервал.
  /// В таблице должно быть два числовых поля, задающих первый и последний год.
  /// Строка проходит фильтр, если заданный в фильтре год (<see cref="Value"/>) попадает в диапазон.
  /// Обрабатываются значения типа NULL, задающие открытые интервалы.
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
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Значения полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int year1 = DataTools.GetInt(rowValues.GetValue(ColumnName1));
      int year2 = DataTools.GetInt(rowValues.GetValue(ColumnName2));

      if (year1 > 0 && Value < year1)
        return false;
      if (year2 > 0 && Value > year2)
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
#endif

  /// <summary>
  /// Фильтр по интервалу дат.
  /// В таблице должно быть два поля типа даты, которые составляют интервал дат.
  /// В фильтре задается одна дата. В просмотр попадают строки, в которых интервал дат
  /// включает в себя эту дату. Обрабатываются открытые и полуоткрытые интервалы,
  /// когда одно или оба поля содержат NULL.
  /// Поддерживает специальный режим фильтра "Рабочая дата".
  /// </summary>
  public class DateRangeInclusionCommonFilter : TwoColumnsCommonFilter
  {
    // Используем отдельную реализацию, так как используется рабочая дата

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
    /// Текущее значение фильтра. Дата или null, если фильтра нет.
    /// </summary>
    public DateTime? Value
    {
      get { return _Value; }
      set
      {
        if (value == _Value)
          return;
        _Value = value;
        if (_Value.HasValue)
        {
          if (_Value.Value != WorkDate)
            _UseWorkDate = false;
        }
        else
          _UseWorkDate = false;
        OnChanged();
      }
    }
    private DateTime? _Value;

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
        if (_UseWorkDate && _Value.HasValue)
          _Value = WorkDate;
        OnChanged();
      }
    }
    private bool _UseWorkDate;

    #endregion

    #region Свойства, которые можно переопределить для использования рабочей даты

    /// <summary>
    /// Если переопределено, то может возвращать рабочую дату вместо текущей.
    /// </summary>
    public virtual DateTime WorkDate { get { return DateTime.Today; } }

    #endregion

    #region Переопределенные методы и свойства

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
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeInclusionFilter(ColumnName1, ColumnName2, Value.Value);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<DateTime> dt1 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName1));
      Nullable<DateTime> dt2 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName2));
      return DataTools.DateInRange(Value.Value, dt1, dt2);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      Value = config.GetNullableDate("Date");
      UseWorkDate = config.GetBool("UseWorkDate");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableDate("Date", Value);
      config.SetBool("UseWorkDate", UseWorkDate);
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (<see cref="IsEmpty"/>=true), возвращается true.
    /// </summary>
    /// <param name="rowValue1">Проверяемое значение первого поля</param>
    /// <param name="rowValue2">Проверяемое значение второго поля</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(DateTime? rowValue1, DateTime? rowValue2)
    {
      if (IsEmpty)
        return true;
      return DataTools.DateInRange(Value.Value, rowValue1, rowValue2);
    }

    #endregion
  }

  #endregion

  #region Фильтры по охвату значения диапазоном

  /// <summary>
  /// Базовый класс для фильтров по двум полям, задающих диапазон числовых значений.
  /// Управляющими является является пара свойств, задающих начальный и конечный интервалы.
  /// Поддерживаются полуоткрытые интервалы.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class RangeCrossCommonFilterBase<T> : TwoColumnsCommonFilter
    where T : struct
  {
    #region Конструктор

    /// <summary>
    /// Создает объект фильтра.
    /// </summary>
    /// <param name="columnName1">Имя первого поля. Должно быть задано</param>
    /// <param name="columnName2">Имя второго поля. Должно быть задано</param>
    public RangeCrossCommonFilterBase(string columnName1, string columnName2)
      : base(columnName1, columnName2)
    {
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

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      if (IsEmpty)
        return;
      _FirstValue = null;
      _LastValue = null;
      OnChanged();
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен (свойства <see cref="FirstValue"/> и <see cref="LastValue"/> вместе равны null).
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !(FirstValue.HasValue || LastValue.HasValue);
      }
    }

    /// <summary>
    /// Если <see cref="FirstValue"/> или <see cref="LastValue"/> установлены, то значения копируются в поля документа.
    /// </summary>
    /// <param name="docValue1">Первое поле</param>
    /// <param name="docValue2">Второе поле</param>
    protected override void OnInitNewValues(DBxExtValue docValue1, DBxExtValue docValue2)
    {
      docValue1.SetValue(FirstValue);
      docValue1.SetValue(LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон чисел.
  /// В фильтр входят строки, диапазон значений полей которых пересекается с заданным диапазоном.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Компоненты времени не поддерживаются.
  /// Используется SQL-фильтр <see cref="NumRangeCrossFilter"/>.
  /// </summary>
  public class IntRangeCrossCommonFilter : RangeCrossCommonFilterBase<Int32>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля , задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public IntRangeCrossCommonFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
      DisplayName = "Диапазон";
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new NumRangeCrossFilter(ColumnName1, ColumnName2, FirstValue, LastValue);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<Int32> v1 = DataTools.GetNullableInt(rowValues.GetValue(ColumnName1));
      Nullable<Int32> v2 = DataTools.GetNullableInt(rowValues.GetValue(ColumnName2));
      return DataTools.AreRangesCrossed<Int32>(FirstValue, LastValue, v1, v2);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      FirstValue = config.GetNullableInt("FirstValue");
      LastValue = config.GetNullableInt("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableInt("FirstValue", FirstValue);
      config.SetNullableInt("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон чисел.
  /// В фильтр входят строки, диапазон значений полей которых пересекается с заданным диапазоном.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Компоненты времени не поддерживаются.
  /// Используется SQL-фильтр <see cref="NumRangeCrossFilter"/>.
  /// </summary>
  public class SingleRangeCrossCommonFilter : RangeCrossCommonFilterBase<Single>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля , задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public SingleRangeCrossCommonFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
      DisplayName = "Диапазон";
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new NumRangeCrossFilter(ColumnName1, ColumnName2, FirstValue, LastValue);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<Single> v1 = DataTools.GetNullableSingle(rowValues.GetValue(ColumnName1));
      Nullable<Single> v2 = DataTools.GetNullableSingle(rowValues.GetValue(ColumnName2));
      return DataTools.AreRangesCrossed<Single>(FirstValue, LastValue, v1, v2);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      FirstValue = config.GetNullableSingle("FirstValue");
      LastValue = config.GetNullableSingle("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableSingle("FirstValue", FirstValue);
      config.SetNullableSingle("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон чисел.
  /// В фильтр входят строки, диапазон значений полей которых пересекается с заданным диапазоном.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Компоненты времени не поддерживаются.
  /// Используется SQL-фильтр <see cref="NumRangeCrossFilter"/>.
  /// </summary>
  public class DoubleRangeCrossCommonFilter : RangeCrossCommonFilterBase<Double>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля , задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public DoubleRangeCrossCommonFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
      DisplayName = "Диапазон";
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new NumRangeCrossFilter(ColumnName1, ColumnName2, FirstValue, LastValue);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<Double> v1 = DataTools.GetNullableDouble(rowValues.GetValue(ColumnName1));
      Nullable<Double> v2 = DataTools.GetNullableDouble(rowValues.GetValue(ColumnName2));
      return DataTools.AreRangesCrossed<Double>(FirstValue, LastValue, v1, v2);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      FirstValue = config.GetNullableDouble("FirstValue");
      LastValue = config.GetNullableDouble("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableDouble("FirstValue", FirstValue);
      config.SetNullableDouble("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон чисел.
  /// В фильтр входят строки, диапазон значений полей которых пересекается с заданным диапазоном.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Компоненты времени не поддерживаются.
  /// Используется SQL-фильтр <see cref="NumRangeCrossFilter"/>.
  /// </summary>
  public class DecimalRangeCrossCommonFilter : RangeCrossCommonFilterBase<Decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя числового поля , задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя числового поля, задающего конец диапазона</param>
    public DecimalRangeCrossCommonFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
      DisplayName = "Диапазон";
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new NumRangeCrossFilter(ColumnName1, ColumnName2, FirstValue, LastValue);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<Decimal> v1 = DataTools.GetNullableDecimal(rowValues.GetValue(ColumnName1));
      Nullable<Decimal> v2 = DataTools.GetNullableDecimal(rowValues.GetValue(ColumnName2));
      return DataTools.AreRangesCrossed<Decimal>(FirstValue, LastValue, v1, v2);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      FirstValue = config.GetNullableDecimal("FirstValue");
      LastValue = config.GetNullableDecimal("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableDecimal("FirstValue", FirstValue);
      config.SetNullableDecimal("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по двум полям, содержащим диапазон дат.
  /// В фильтр входят строки, диапазон значений полей которых пересекается с заданным диапазоном.
  /// Поддерживаются полуоткрытые интервалы и в базе данных, и в проверяемом интервале.
  /// Компоненты времени не поддерживаются.
  /// Используется SQL-фильтр <see cref="DateRangeCrossFilter"/>.
  /// </summary>
  public class DateRangeCrossCommonFilter : RangeCrossCommonFilterBase<DateTime>
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="firstColumnName">Имя поля типа "Дата", задающего начало диапазона</param>
    /// <param name="lastColumnName">Имя поля типа "Дата", задающего конец диапазона</param>
    public DateRangeCrossCommonFilter(string firstColumnName, string lastColumnName)
      : base(firstColumnName, lastColumnName)
    {
      DisplayName = "Период";
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeCrossFilter(ColumnName1, ColumnName2, FirstValue, LastValue);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поля <see cref="TwoColumnsCommonFilter.ColumnName1"/> и <see cref="TwoColumnsCommonFilter.ColumnName2"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Значения полей</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<DateTime> dt1 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName1));
      Nullable<DateTime> dt2 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName2));
      return DataTools.DateRangesCrossed(FirstValue, LastValue, dt1, dt2);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      FirstValue = config.GetNullableDate("FirstValue");
      LastValue = config.GetNullableDate("LastValue");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableDate("FirstValue", FirstValue);
      config.SetNullableDate("LastValue", LastValue);
    }

    #endregion
  }

  #endregion

  #endregion

  /// <summary>
  /// Фиктивный фильтр, который может ничего не делать или не пропускать ни одной строки
  /// </summary>
  public class DummyCommonFilter : DBxCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр.
    /// Свойство <see cref="IsTrue"/> имеет значение true.
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
    /// Устанавливает <see cref="IsTrue"/>=true
    /// </summary>
    public override void Clear()
    {
      IsTrue = true;
    }

    /// <summary>
    /// Возвращает <see cref="IsTrue"/>
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
    /// Возвращает <see cref="DummyFilter.AlwaysFalse"/>, если фильтр установлен
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
    /// Возвращает <see cref="IsTrue"/>
    /// </summary>
    /// <param name="RowValues">Игнорируется</param>
    /// <returns>Значение свойства <see cref="IsTrue"/></returns>
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
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      IsTrue = config.GetBool("Value");
    }

    #endregion
  }

  /// <summary>
  /// Фильтр с фиксированным SQL-запросом.
  /// С этим фильтром нельзя выполнить никаких действий, в том числе, очистить.
  /// </summary>
  public class FixedSqlCommonFilter : DBxCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр.
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
    /// Возвращает список полей из фильтра
    /// </summary>
    /// <param name="list">Игнорируется</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      _Filter.GetColumnNames(list);
    }

    /// <summary>
    /// Возвращает <see cref="Filter"/>.
    /// </summary>
    /// <returns>Фильтр</returns>
    public override DBxFilter GetSqlFilter()
    {
      return _Filter;
    }

    /// <summary>
    /// Проверяет значения на соответствие фильтру
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей</param>
    /// <returns>Прохождение фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      return _Filter.TestFilter(rowValues);
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
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
    }

    #endregion
  }
}
