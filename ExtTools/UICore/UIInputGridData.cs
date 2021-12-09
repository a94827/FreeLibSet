using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FreeLibSet.UICore
{
  #region HorizontalAlignment

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
    /// Создает объект для существующей таблицы DataTable.
    /// Таблица не должна быть частью DataSet, в противном случае она будет от него отсоединена.
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
    /// Создает объект с новой таблицей DataTable
    /// </summary>
    public UIInputGridData()
      : this(new DataTable("InputTable"))
    {
    }

    #endregion

    #region Основные свойства

    private DataSet _DS;

    /// <summary>
    /// Таблица данных
    /// </summary>
    public DataTable Table { get { return _DS.Tables[0]; } }

    private TypedStringDictionary<ColumnInfo> _Dict;

    /// <summary>
    /// Описания столбцов
    /// </summary>
    public ColumnCollection Columns { get { return new ColumnCollection(this); } }

    #endregion

    #region CanBeEmpty

    /// <summary>
    /// Может ли поле быть пустым.
    /// Значение по умолчанию - Error - поле должно быть заполнено, иначе будет выдаваться ошибка.
    /// Установка свойства применяется ко всем полям таблицы.
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
    /// Может ли поле быть пустым.
    /// Значение по умолчанию: false (поле является обязательным).
    /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
    /// При CanBeEmptyMode=Warning это свойство возвращает true.
    /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
    /// Установка свойства применяется ко всем полям таблицы.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Вложенные классы

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

      public string ColumnName { get { return _ColumnName; } }
      private string _ColumnName;

      /// <summary>
      /// Столбец таблицы данных
      /// </summary>
      public DataColumn Column { get { return _Owner.Table.Columns[_ColumnName]; } }

      // Нельзя хранить ссылку на DataColumn, т.к. этот тип не является сериализуемым
      // private DataColumn _Column;

      /// <summary>
      /// Возвращает свойство DataColumn.ColumnName
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
      /// Если свойство не установлено в явном виде, то определяется по типу данных столбца (DataColumn.DataType).
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
      /// По умолчанию - 0 - используется ширина столбца, задаваемая TextWidth.
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
      /// Значение по умолчанию совпадает с текущим значением основного свойства UIInputGridData.CanBeEmptyMode.
      /// </summary>
      public UIValidateState CanBeEmptyMode
      {
        get { return _CanBeEmptyMode; }
        set { _CanBeEmptyMode = value; }
      }
      private UIValidateState _CanBeEmptyMode;

      /// <summary>
      /// Может ли поле быть пустым.
      /// Значение по умолчанию совпадает с текущим значением основного свойства UIInputGridData.CanBeEmpty.
      /// Это свойство дублирует CanBeEmptyMode, но не позволяет установить режим предупреждения.
      /// При CanBeEmptyMode=Warning это свойство возвращает true.
      /// Установка значения true эквивалентна установке CanBeEmptyMode=Ok, а false - CanBeEmptyMode=Error.
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
      public UIValidatorList Validators
      {
        get
        {
          if (_Validators == null)
            _Validators = new UIValidatorList();
          return _Validators;
        }
      }
      private UIValidatorList _Validators;

      /// <summary>
      /// Возвращает true, если список Validators не пустой.
      /// Используется для оптимизации, вместо обращения к Validators.Count, позволяя обойтись без создания объекта списка, когда у управляющего элемента нет валидаторов.
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
      internal object ValidatingValue
      {
        get { return _ValidatingValue; }
        set
        {
          if (value is DBNull)
            value = null;

          _ValidatingValue = value;
          if (_ValidatingValueEx != null)
            _ValidatingValueEx.OwnerSetValue(value);
          if (_ValidatingIsNotEmptyEx != null)
            _ValidatingIsNotEmptyEx.OwnerSetValue(!Object.ReferenceEquals(value, null));
        }
      }
      private object _ValidatingValue;

      #endregion

      #region ValidatingValueEx

      /// <summary>
      /// Управляемое свойство, возвращающее текущий проверяемый код в списке SelectedCodes.
      /// Используется в валидаторах из списка CodeValidators.
      /// Не используйте свойство в валидаторах основного списка Validators.
      /// </summary>
      public IDepValue ValidatingValueEx
      {
        get
        {
          if (_ValidatingValueEx == null)
          {
            Type valueType = Column.DataType;
            if (valueType == null)
              valueType = typeof(object);
            _ValidatingValueEx = DepTools.CreateOutput(valueType);
            _ValidatingValueEx.OwnerSetValue(ValidatingValue);
            _ValidatingValueEx.OwnerInfo = new DepOwnerInfo(this, "ValidatingValueEx");
          }
          return _ValidatingValueEx;
        }
      }
      private IDepOutput _ValidatingValueEx;

      /// <summary>
      /// Возвращает true, если обработчик свойства ValidatingValueEx присоединен к другим объектам в качестве входа.
      /// Это свойство не предназначено для использования в пользовательском коде
      /// </summary>
      public bool InternalValidatingValueExConnected
      {
        get
        {
          if (_ValidatingValueEx == null)
            return false;
          else
            return _ValidatingValueEx.IsConnected;
        }
      }

      public DepValue<T> GetValidatingValueEx<T>()
      {
        return (DepValue<T>)ValidatingValueEx;
      }

      #endregion

      #region ValidatingIsNotEmptyEx

      /// <summary>
      /// Управляемое свойство, которое возвращает true, если введено непустое значение.
      /// Используется для валидаторов.
      /// </summary>
      public DepValue<bool> ValidatingIsNotEmptyEx
      {
        get
        {
          if (_ValidatingIsNotEmptyEx == null)
          {
            _ValidatingIsNotEmptyEx = new DepOutput<bool>(!Object.ReferenceEquals(_ValidatingValue, null));
            _ValidatingIsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
          }
          return _ValidatingIsNotEmptyEx;
        }
      }
      private DepOutput<bool> _ValidatingIsNotEmptyEx;

      #endregion

      #endregion
    }

    /// <summary>
    /// Реализация свойства Columns
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

      private UIInputGridData _Owner;

      /// <summary>
      /// Доступ к свойствам столбца по имени.
      /// На момент вызова столбец должен быть добавлен в таблицу.
      /// </summary>
      /// <param name="columnName">Имя столбца (свойство DataColumn.ColumnName)</param>
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
      /// <param name="column">Столбец DataTable</param>
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
    /// <param name="row">Проверяемая строка таблицы Table</param>
    public void InternalSetValidatingRow(DataRow row)
    {
      if (row == null)
        throw new ArgumentNullException("row");
      int n = row.Table.Columns.Count;
      for (int i = 0; i < n; i++)
      {
        ColumnInfo ci;
        if (_Dict.TryGetValue(row.Table.Columns[i].ColumnName, out ci))
          ci.ValidatingValue = row[i];
      }
    }

    #endregion
  }
}
