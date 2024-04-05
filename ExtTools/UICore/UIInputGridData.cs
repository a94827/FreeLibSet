using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeLibSet.UICore
{
  #region UIHorizontalAlignment

  /// <summary>
  /// Specifies how an object or text in a control is horizontally aligned relative
  /// to an element of the control.
  /// </summary>
  [Serializable]
  public enum UIHorizontalAlignment
  {
    /// <summary>
    /// The object or text is aligned on the left of the control element.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The object or text is aligned on the right of the control element.
    /// </summary>
    Right = 1,

    /// <summary>
    /// The object or text is aligned in the center of the control element.
    /// </summary>
    Center = 2,
  }

  #endregion

  /// <summary>
  /// Табличные данные, форматирование столбцов и валидаторы, используемые просмотром для табличного ввода данных.
  /// </summary>
  [Serializable]
  public sealed class UIInputGridData
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект для существующей таблицы <see cref="DataTable"/>, созданной в пользовательском коде.
    /// Таблица не должна быть частью <see cref="DataSet"/>, в противном случае она будет от него отсоединена.
    /// </summary>
    /// <param name="table">Внешняя таблица данных. Не может быть null</param>
    public UIInputGridData(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");

      _DS = new DataSet();
      DataTools.AddTableToDataSet(_DS, table);

      _Dict = new TypedStringDictionary<ColumnInfo>(table.Columns.Count, true);
      _CanBeEmptyMode = UIValidateState.Error;
    }

    /// <summary>
    /// Создает объект с новой пустой таблицей <see cref="DataTable"/>.
    /// Прикладной код должен будет добавить столбцы в коллекцию <see cref="ColumnCollection"/>, используя свойство <see cref="Table"/>.
    /// </summary>
    public UIInputGridData()
      : this(new DataTable("InputTable"))
    {
    }

    #endregion

    #region Основные свойства

    private readonly DataSet _DS;

    /// <summary>
    /// Таблица данных. Задается в конструкторе.
    /// Если использовался конструктор без аргументов, то прикладной код должен добавить столбцы в таблицу.
    /// </summary>
    public DataTable Table { get { return _DS.Tables[0]; } }

    private readonly TypedStringDictionary<ColumnInfo> _Dict;

    /// <summary>
    /// Дополнительные описания столбцов таблицы.
    /// Позволяют задавать форматирование значений и правила проверки.
    /// </summary>
    public ColumnCollection Columns { get { return new ColumnCollection(this); } }

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Могут ли поля быть пустыми.
    /// Значение по умолчанию - <see cref="UIValidateState.Error"/> - поля должны быть заполнены, иначе будет выдаваться ошибка.
    /// Установка свойства применяется ко всем полям таблицы.
    /// Для задания индивидуальных ограничений для столбцов используйте свойство <see cref="UIInputGridData.ColumnInfo.CanBeEmptyMode"/>.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        _CanBeEmptyMode = value;
        foreach (KeyValuePair<string, ColumnInfo> pair in _Dict)
          pair.Value.CanBeEmptyMode = value;
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Могут ли поля быть пустыми.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует <see cref="CanBeEmptyMode"/>, но не позволяет установить режим предупреждения.
    /// При <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/> это свойство возвращает true.
    /// Установка значения true эквивалентна установке <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Ok"/>, 
    /// а false - <see cref="UIValidateState.Error"/>.
    /// Установка свойства применяется ко всем полям таблицы. Для задания проверки для отдельныз столбцов используйте свойство <see cref="ColumnInfo.CanBeEmpty"/>.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Вложенные классы

    /// <summary>
    /// Расширяет список свойствами с текущим проверяемым значением
    /// </summary>
    [Serializable]
    public sealed class ColumnValidators : UIValidatorList
    {
      #region Защищенный конструктор

      internal ColumnValidators(ColumnInfo columnInfo)
      {
        _CI = columnInfo;
      }

      private ColumnInfo _CI;

      #endregion

      #region ValueEx

      /// <summary>
      /// Управляемое свойство, возвращающее значение проверяемой строки.
      /// </summary>
      public IDepValue ValueEx
      {
        get
        {
          if (_ValueEx == null)
          {
            Type valueType = _CI.Column.DataType;
            if (valueType == null)
              valueType = typeof(object);
            _ValueEx = DepTools.CreateOutput(valueType);
            _ValueEx.OwnerSetValue(_CI.CurrentValue);
            _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
          }
          return _ValueEx;
        }
      }
      private IDepOutput _ValueEx;

      internal bool ValueExConnected
      {
        get
        {
          if (_ValueEx == null)
            return false;
          else
            return _ValueEx.IsConnected;
        }
      }

      #endregion

      #region IsNotEmptyEx

      /// <summary>
      /// Управляемое свойство, которое возвращает true, если в поле введено непустое значение.
      /// </summary>
      public DepValue<bool> IsNotEmptyEx
      {
        get
        {
          if (_IsNotEmptyEx == null)
          {
            _IsNotEmptyEx = new DepOutput<bool>(!Object.ReferenceEquals(_CI.CurrentValue, null));
            _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
          }
          return _IsNotEmptyEx;
        }
      }
      private DepOutput<bool> _IsNotEmptyEx;

      #endregion

      #region Установка значения

      internal void InitValue()
      {
        if (_ValueEx != null)
          _ValueEx.OwnerSetValue(_CI.CurrentValue);
        if (_IsNotEmptyEx != null)
          _IsNotEmptyEx.OwnerSetValue(!Object.ReferenceEquals(_CI.CurrentValue, null));
      }

      #endregion

      #region Форматированный доступ

      /// <summary>
      /// Вызывает <see cref="DepTools.ToTypeEx{T}(IDepValue)"/> для свойства <see cref="ValueEx"/>.
      /// </summary>
      /// <typeparam name="T">Тип данных, к которому нужно преобразовать текущее проверяемое значение</typeparam>
      /// <returns>Вычисляемый объект</returns>
      public DepValue<T> ToTypeEx<T>()
      {
        return DepTools.ToTypeEx<T>(ValueEx);
      }

      /// <summary>
      /// Возвращает текущее проверяемое значение как строку.
      /// </summary>
      public DepValue<string> AsStringEx { get { return ToTypeEx<string>(); } }

      /// <summary>
      /// Возвращает текущее проверяемое значение как число.
      /// </summary>
      public DepValue<int> AsIntEx { get { return ToTypeEx<int>(); } }

      /// <summary>
      /// Возвращает текущее проверяемое значение как число.
      /// </summary>
      public DepValue<long> AsInt64Ex { get { return ToTypeEx<long>(); } }

      /// <summary>
      /// Возвращает текущее проверяемое значение как число.
      /// </summary>
      public DepValue<float> AsSingleEx { get { return ToTypeEx<float>(); } }

      /// <summary>
      /// Возвращает текущее проверяемое значение как число.
      /// </summary>
      public DepValue<double> AsDoubleEx { get { return ToTypeEx<double>(); } }

      /// <summary>
      /// Возвращает текущее проверяемое значение как число.
      /// </summary>
      public DepValue<decimal> AsDecimalEx { get { return ToTypeEx<decimal>(); } }

      /// <summary>
      /// Возвращает текущее проверяемое значение как логическое значение.
      /// </summary>
      public DepValue<bool> AsBoolEx { get { return ToTypeEx<bool>(); } }

      /// <summary>
      /// Возвращает текущее проверяемое значение как дату/время.
      /// Пустое значение возвращается как <see cref="DateTime.MinValue"/>.
      /// </summary>
      public DepValue<DateTime> AsDateTimeEx { get { return ToTypeEx<DateTime>(); } }

      /// <summary>
      /// Возвращает текущее проверяемое nullable-значение как дату/время.
      /// </summary>
      public DepValue<DateTime?> AsNulableDateTimeEx { get { return ToTypeEx<DateTime?>(); } }

      // для GUID'jd и прочих типов вряд ли нужно делать

      #endregion
    }

    /// <summary>
    /// Форматирование и валидаторы для одного столбца данных
    /// </summary>
    [Serializable]
    public sealed class ColumnInfo
    {
      #region Защищенный конструктор

      internal ColumnInfo(UIInputGridData owner, string columnName)
      {
        _Owner = owner;
        _ColumnName = columnName;

        Type t = Column.DataType;

        if (t == typeof(DateTime) || t == typeof(bool))
          _Align = UIHorizontalAlignment.Center;
        else if (DataTools.IsNumericType(t))
          _Align = UIHorizontalAlignment.Right;
        else
          _Align = UIHorizontalAlignment.Left;

        _Format = String.Empty;
        _CanBeEmptyMode = _Owner.CanBeEmptyMode;
      }

      #endregion

      #region Основные свойства

      private UIInputGridData _Owner;

      /// <summary>
      /// Имя столбца <see cref="DataColumn.ColumnName"/>.
      /// </summary>
      public string ColumnName { get { return _ColumnName; } }
      private string _ColumnName;

      /// <summary>
      /// Столбец таблицы данных.
      /// Свойство используется для доступа к основным свойствам столбца <see cref="DataColumn"/>.
      /// </summary>
      public DataColumn Column { get { return _Owner.Table.Columns[_ColumnName]; } }

      // Нельзя хранить ссылку на DataColumn, т.к. этот тип не является сериализуемым
      // private DataColumn _Column;

      /// <summary>
      /// Возвращает свойство <see cref="DataColumn.ColumnName"/>
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return ColumnName;
      }

      #endregion

      #region Форматирование столбца

      /// <summary>
      /// Горизонтальное выравнивание.
      /// Если свойство не установлено в явном виде, то определяется по типу данных столбца (<see cref="DataColumn.DataType"/>).
      /// Для числовых типов используется выравнивание по правому краю, для строк - по левому, для даты/времени и логического типа - по центру.
      /// </summary>
      public UIHorizontalAlignment Align
      {
        get { return _Align; }
        set { _Align = value; }
      }
      private UIHorizontalAlignment _Align;

      /// <summary>
      /// Формат для числового столбца или столбца даты/времени.
      /// </summary>
      public string Format
      {
        get { return _Format; }
        set
        {
          if (value == null)
            _Format = String.Empty;
          else
            _Format = value;
        }
      }
      private string _Format;

      /// <summary>
      /// Ширина столбца как количество символов.
      /// </summary>
      public int TextWidth
      {
        get { return _TextWidth; }
        set { _TextWidth = value; }
      }
      private int _TextWidth;

      /// <summary>
      /// Минимальная ширина столбца как количество символов.
      /// </summary>
      public int MinTextWidth
      {
        get { return _MinTextWidth; }
        set { _MinTextWidth = value; }
      }
      private int _MinTextWidth;

      /// <summary>
      /// Весовой коэффициент для столбца, который должен заполнять таблицу по ширине.
      /// По умолчанию - 0 - используется ширина столбца, задаваемая <see cref="TextWidth"/>, а заполнение не используется.
      /// </summary>
      public int FillWeight
      {
        get { return _FillWeight; }
        set { _FillWeight = value; }
      }
      private int _FillWeight;

      #endregion

      #region Проверка значения

      #region CanBeEmpty

      /// <summary>
      /// Может ли поле быть пустым.
      /// Значение по умолчанию совпадает с текущим значением основного свойства <see cref="UIInputGridData.CanBeEmptyMode"/>.
      /// </summary>
      public UIValidateState CanBeEmptyMode
      {
        get { return _CanBeEmptyMode; }
        set { _CanBeEmptyMode = value; }
      }
      private UIValidateState _CanBeEmptyMode;

      /// <summary>
      /// Может ли поле быть пустым.
      /// Значение по умолчанию совпадает с текущим значением основного свойства <see cref="UIInputGridData.CanBeEmpty"/>.
      /// Это свойство дублирует <see cref="CanBeEmptyMode"/>, но не позволяет установить режим предупреждения.
      /// При <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Warning"/> это свойство возвращает true.
      /// Установка значения true эквивалентна установке <see cref="CanBeEmptyMode"/>=<see cref="UIValidateState.Ok"/>, 
      /// а false - <see cref="UIValidateState.Error"/>.
      /// </summary>
      public bool CanBeEmpty
      {
        get { return CanBeEmptyMode != UIValidateState.Error; }
        set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
      }

      #endregion

      #region Validators

      /// <summary>
      /// Список валидаторов для столбца
      /// </summary>
      public ColumnValidators Validators
      {
        get
        {
          if (_Validators == null)
            _Validators = new ColumnValidators(this);
          return _Validators;
        }
      }
      private ColumnValidators _Validators;

      /// <summary>
      /// Возвращает true, если список <see cref="Validators"/> не пустой.
      /// Используется для оптимизации, вместо обращения к Validators.Count, 
      /// позволяя обойтись без создания объекта списка, когда у столбца нет валидаторов.
      /// </summary>
      public bool HasValidators
      {
        get
        {
          if (_Validators == null)
            return false;
          else
            return _Validators.Count > 0;
        }
      }

      #endregion

      #region Текущее значение

      /// <summary>
      /// Текущее проверяемое значение.
      /// Вместо DBNull используется null.
      /// </summary>
      internal object CurrentValue
      {
        get { return _CurrentValue; }
        set
        {
          if (value is DBNull)
            value = null;

          _CurrentValue = value;
          if (_Validators != null)
            _Validators.InitValue();
        }
      }
      private object _CurrentValue;

      #endregion

      #endregion
    }

    /// <summary>
    /// Реализация свойства <see cref="Columns"/>
    /// </summary>
    public struct ColumnCollection
    {
      #region Конструктор

      internal ColumnCollection(UIInputGridData owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private readonly UIInputGridData _Owner;

      /// <summary>
      /// Доступ к свойствам столбца по имени.
      /// На момент вызова столбец должен быть добавлен в таблицу.
      /// </summary>
      /// <param name="columnName">Имя столбца (свойство <see cref="DataColumn.ColumnName"/>)</param>
      /// <returns>Свойства столбца табличного просмотра</returns>
      public ColumnInfo this[string columnName]
      {
        get
        {
          ColumnInfo info;
          if (!_Owner._Dict.TryGetValue(columnName, out info))
          {
            DataColumn column = _Owner.Table.Columns[columnName];
            if (column == null)
            {
              if (String.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");
              else
                throw new ArgumentException("В таблице " + _Owner.Table.ToString() + " нет столбца с именем \"" + columnName + "\"");
            }
            info = new ColumnInfo(_Owner, columnName);
            _Owner._Dict.Add(columnName, info);
          }
          return info;
        }
      }

      /// <summary>
      /// Доступ к свойствам столбца.
      /// На момент вызова столбец должен быть добавлен в таблицу.
      /// </summary>
      /// <param name="column">Столбец таблицы <see cref="UIInputGridData.Table"/></param>
      /// <returns>Свойства столбца табличного просмотра</returns>
      public ColumnInfo this[DataColumn column]
      {
        get
        {
          if (column == null)
            throw new ArgumentNullException("column");
          return this[column.ColumnName];
        }
      }

      /// <summary>
      /// Доступ к свойствам столбца.
      /// </summary>
      /// <param name="columnIndex">Индекс столбца</param>
      /// <returns>Свойства столбца табличного просмотра</returns>
      public ColumnInfo this[int columnIndex]
      {
        get
        {
          return this[_Owner.Table.Columns[columnIndex].ColumnName];
        }
      }

      /// <summary>
      /// Доступ к свойствам последнего столбца, который был добавлен в таблицу.
      /// </summary>
      public ColumnInfo LastAdded
      {
        get
        {
          if (_Owner.Table.Columns.Count == 0)
            return null;
          else
            return this[_Owner.Table.Columns[_Owner.Table.Columns.Count - 1]];
        }
      }

      #endregion
    }

    #endregion

    #region Выполнение проверки

    /// <summary>
    /// Установка значений для проверки строки.
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="row">Проверяемая строка таблицы <see cref="UIInputGridData.Table"/></param>
    public void InternalSetValidatingRow(DataRow row)
    {
      if (row == null)
        throw new ArgumentNullException("row");
      int n = row.Table.Columns.Count;
      for (int i = 0; i < n; i++)
      {
        ColumnInfo ci;
        if (_Dict.TryGetValue(row.Table.Columns[i].ColumnName, out ci))
          ci.CurrentValue = row[i];
      }
    }

    #endregion
  }
}
