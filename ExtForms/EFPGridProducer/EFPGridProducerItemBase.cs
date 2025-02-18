// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{

  /// <summary>
  /// Структура, содержащая информацию о строке <see cref="EFPDataGridView"/> или <see cref="EFPDataTreeView"/>,
  /// которая используется для вызова событий объектов <see cref="EFPGridProducer"/>.
  /// Для получения заполненной структуры используйте методы <see cref="EFPDataGridView.GetRowInfo(int)"/> и
  /// <see cref="EFPDataTreeView.GetRowInfo(FreeLibSet.Controls.TreeNodeAdv)"/>.
  /// Структура однократной записи.
  /// </summary>
  public struct EFPDataViewRowInfo
  {
    #region Конструктор

    /// <summary>
    /// Заполняет структуру
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    /// <param name="dataBoundItem">Ссылка на строку данных</param>
    /// <param name="values">Интерфейс доступа к значениям</param>
    /// <param name="rowIndex">Индекс строки</param>
    public EFPDataViewRowInfo(IEFPDataView controlProvider, object dataBoundItem, INamedValuesAccess values, int rowIndex)
    {
      _ControlProvider = controlProvider;
      _DataBoundItem = dataBoundItem;
      _Values = values;
      _RowIndex = rowIndex;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного или иерархического просмотра
    /// </summary>
    public IEFPDataView ControlProvider { get { return _ControlProvider; } }
    private readonly IEFPDataView _ControlProvider;


    /// <summary>
    /// Объект, связанный с текущей строкой просмотра.
    /// Обычно, это строка данных <see cref="System.Data.DataRow"/>.
    /// Используйте свойство <see cref="Values"/> или типизированные методы для извлечения данных.
    /// </summary>
    public object DataBoundItem { get { return _DataBoundItem; } }
    private readonly object _DataBoundItem;

    /// <summary>
    /// Интерфейс для извлечения значений полей из строки данных
    /// </summary>
    public INamedValuesAccess Values { get { return _Values; } }
    private readonly INamedValuesAccess _Values;

    /// <summary>
    /// Индекс строки табличного просмотра <see cref="System.Windows.Forms.DataGridViewBand.Index"/>.
    /// Для иерархического просмотра возвращает свойство <see cref="FreeLibSet.Controls.TreeNodeAdv.Row"/>.
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private readonly int _RowIndex;

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает "RowIndex=XXX".
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "RowIndex=" + _RowIndex.ToString();
    }

    #endregion
  }

  #region Делегат

  /// <summary>
  /// Базовый класс для аргументов событий, связанных с <see cref="EFPGridProducer"/>.
  /// </summary>
  public class EFPGridProducerBaseEventArgs : EventArgs
  {
    #region Защищенный конструктор

    internal EFPGridProducerBaseEventArgs(EFPGridProducerItemBase owner)
    {
      _Owner = owner;
      if (owner.SourceColumnNames == null)
        _SourceColumnNames = new string[1] { owner.Name };
      else
        _SourceColumnNames = owner.SourceColumnNames;
    }

    #endregion

    #region Свойства

    private readonly EFPGridProducerItemBase _Owner;

    /// <summary>
    /// Список имен исходных столбцов.
    /// Если столбец не является вычисляемым, то возвращает массив из одного элемента, содержащего условное имя столбца или всплывающей подсказки.
    /// </summary>
    public string[] SourceColumnNames { get { return _SourceColumnNames; } }
    private readonly string[] _SourceColumnNames;

    internal EFPDataViewRowInfo RowInfo { get { return _RowInfo; } set { _RowInfo = value; } }
    private EFPDataViewRowInfo _RowInfo;

    /// <summary>
    /// Провайдер табличного или иерархического просмотра
    /// </summary>
    public IEFPDataView ControlProvider { get { return _RowInfo.ControlProvider; } }

    /// <summary>
    /// Объект, связанный с текущей строкой просмотра.
    /// Обычно, это строка данных <see cref="System.Data.DataRow"/>.
    /// Используйте свойство <see cref="Values"/> или типизированные методы для извлечения данных.
    /// </summary>
    public object DataBoundItem { get { return _RowInfo.DataBoundItem; } }

    /// <summary>
    /// Интерфейс для извлечения значений полей из строки данных
    /// </summary>
    public INamedValuesAccess Values { get { return _RowInfo.Values; } }

    /// <summary>
    /// Индекс строки табличного просмотра.
    /// Для иерархического просмотра возвращает свойство <see cref="FreeLibSet.Controls.TreeNodeAdv.Row"/>.
    /// </summary>
    public int RowIndex { get { return _RowInfo.RowIndex; } }

    /// <summary>
    /// Произвольные пользовательские данные, присоединенные к объекту
    /// </summary>
    public object Tag { get { return _Owner.Tag; } } // нельзя использовать локальную копию

    #endregion

    #region Методы форматированного доступа к полям строки по имени

    /// <summary>
    /// Получить строковое значение поля.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public string GetString(string columnName)
    {
      return DataTools.GetString(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public int GetInt(string columnName)
    {
      return DataTools.GetInt(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public long GetInt64(string columnName)
    {
      return DataTools.GetInt64(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public float GetSingle(string columnName)
    {
      return DataTools.GetSingle(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public double GetDouble(string columnName)
    {
      return DataTools.GetDouble(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public decimal GetDecimal(string columnName)
    {
      return DataTools.GetDecimal(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить значение логического поля.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public bool GetBool(string columnName)
    {
      return DataTools.GetBool(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить значение поля, содержащего дату и/или время.
    /// Если поле содержит <see cref="DBNull"/>, возвращается неинициализированная дата.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public DateTime GetDateTime(string columnName)
    {
      return DataTools.GetDateTime(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить значение поля, содержащего дату и/или время.
    /// Если поле содержит <see cref="DBNull"/>, возвращается null.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public DateTime? GetNullableDateTime(string columnName)
    {
      return DataTools.GetNullableDateTime(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить значение поля, содержащего интервал времени.
    /// Если поле содержит <see cref="DBNull"/>, возвращается <see cref="TimeSpan.Zero"/>.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public TimeSpan GetTimeSpan(string columnName)
    {
      return DataTools.GetTimeSpan(Values.GetValue(columnName));
    }

    /// <summary>
    /// Получить значение поля типа <see cref="Guid"/>.
    /// Если поле содержит <see cref="DBNull"/>, возвращается <see cref="Guid.Empty"/>.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public Guid GetGuid(string columnName)
    {
      return DataTools.GetGuid(Values.GetValue(columnName));
    }

    /// <summary>
    /// Возвращает true, если поле содержит значение null или <see cref="DBNull"/>.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Признак пустого значения</returns>
    public bool IsNull(string columnName)
    {
      object v = Values.GetValue(columnName);
      return (v == null) || (v is DBNull);
    }

    /// <summary>
    /// Получить значение перечислимого типа.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public T GetEnum<T>(string columnName)
      where T : struct
    {
      return DataTools.GetEnum<T>(Values.GetValue(columnName));
    }

    #endregion

    #region Методы форматированного доступа по индексу исходного столбца

    /// <summary>
    /// Получить строковое значение поля.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public string GetString(int sourceColumnIndex)
    {
      return DataTools.GetString(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public int GetInt(int sourceColumnIndex)
    {
      return DataTools.GetInt(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public long GetInt64(int sourceColumnIndex)
    {
      return DataTools.GetInt64(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public float GetSingle(int sourceColumnIndex)
    {
      return DataTools.GetSingle(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public double GetDouble(int sourceColumnIndex)
    {
      return DataTools.GetDouble(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить числовое значение поля.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public decimal GetDecimal(int sourceColumnIndex)
    {
      return DataTools.GetDecimal(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить значение логического поля.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public bool GetBool(int sourceColumnIndex)
    {
      return DataTools.GetBool(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить значение поля, содержащего дату и/или время.
    /// Если поле содержит <see cref="DBNull"/>, возвращается неинициализированная дата.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public DateTime GetDateTime(int sourceColumnIndex)
    {
      return DataTools.GetDateTime(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить значение поля, содержащего дату и/или время.
    /// Если поле содержит <see cref="DBNull"/>, возвращается null.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public DateTime? GetNullableDateTime(int sourceColumnIndex)
    {
      return DataTools.GetNullableDateTime(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить значение поля, содержащего интервал времени.
    /// Если поле содержит <see cref="DBNull"/>, возвращается <see cref="TimeSpan.Zero"/>.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public TimeSpan GetTimeSpan(int sourceColumnIndex)
    {
      return DataTools.GetTimeSpan(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Получить значение поля типа <see cref="Guid"/>.
    /// Если поле содержит <see cref="DBNull"/>, возвращается <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public Guid GetGuid(int sourceColumnIndex)
    {
      return DataTools.GetGuid(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// Возвращает true, если поле содержит значение null или <see cref="DBNull"/>.
    /// Имя поля должно быть в списке исходных столбцов <see cref="SourceColumnNames"/>, заданных при объявлении вычисляемого столбца.
    /// </summary>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Признак пустого значения</returns>
    public bool IsNull(int sourceColumnIndex)
    {
      object v = Values.GetValue(SourceColumnNames[sourceColumnIndex]);
      return (v == null) || (v is DBNull);
    }

    /// <summary>
    /// Получить значение перечислимого типа.
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="sourceColumnIndex">Индекс исходного столбца в массиве <see cref="SourceColumnNames"/>.
    /// Если столбец не является вычисляемым, то допускается только значение 0, чтобы получить значение поля</param>
    /// <returns>Значение</returns>
    public T GetEnum<T>(int sourceColumnIndex)
      where T : struct
    {
      return DataTools.GetEnum<T>(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    #endregion
  }

  #region Перечисление EFPGridProducerValueReason

  /// <summary>
  /// Причина вызова события <see cref="EFPGridProducerItemBase.ValueNeeded"/>
  /// </summary>
  public enum EFPGridProducerValueReason
  {
    /// <summary>
    /// Требуется значение ячейки
    /// </summary>
    Value,

    /// <summary>
    /// Требуется всплывающая подсказка
    /// </summary>
    ToolTipText
  }

  #endregion

  /// <summary>
  /// Аргументы события <see cref="EFPGridProducerItemBase.ValueNeeded"/>
  /// </summary>
  public class EFPGridProducerValueNeededEventArgs : EFPGridProducerBaseEventArgs
  {
    #region Защищенный конструктор

    internal EFPGridProducerValueNeededEventArgs(EFPGridProducerItemBase owner)
      : base(owner)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Причина вызова события <see cref="EFPGridProducerItemBase.ValueNeeded"/>.
    /// Для события в <see cref="EFPGridProducerToolTip"/> всегда возвращает <see cref="EFPGridProducerValueReason.ToolTipText"/>.
    /// </summary>
    public EFPGridProducerValueReason Reason
    {
      get { return _Reason; }
      internal set { _Reason = value; }
    }
    private EFPGridProducerValueReason _Reason;

    /// <summary>
    /// Сюда должно быть записано вычисленное значение
    /// </summary>
    public object Value
    {
      get { return _Value; }
      set { _Value = value; }
    }
    private object _Value;

    /// <summary>
    /// Текст всплывающей подсказки
    /// </summary>
    public string ToolTipText
    {
      get { return _ToolTipText; }
      set { _ToolTipText = value; }
    }
    private string _ToolTipText;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPGridProducerItemBase.ValueNeeded"/>
  /// </summary>
  /// <param name="sender">Описатель столбца <see cref="EFPGridProducerColumn"/> или всплывающей подсказки <see cref="EFPGridProducerToolTip"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPGridProducerValueNeededEventHandler(object sender,
    EFPGridProducerValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// Базовый класс для <see cref="EFPGridProducerColumn"/> и <see cref="EFPGridProducerToolTip"/>
  /// </summary>
  public abstract class EFPGridProducerItemBase : IObjectWithCode
  {
    #region Защищенный конструктор

    internal EFPGridProducerItemBase(string name, string[] sourceColumnNames)
    {
#if DEBUG
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");
      if (name[0] >= '0' && name[0] <= '9')
        throw new ArgumentException(Res.EFPGridProducer_Arg_NameStartsWithDigit, "name");

      EFPApp.CheckMainThread();
#endif

      _Name = name;

      if (sourceColumnNames != null)
      {
        // может быть пустой массив
        for (int i = 0; i < sourceColumnNames.Length; i++)
        {
          if (String.IsNullOrEmpty(sourceColumnNames[i]))
            throw new ArgumentException(Res.EFPGridProducer_Arg_EmptySourceColumn, "sourceColumnNames");
          if (sourceColumnNames[i].IndexOf(',') >= 0)
            throw new ArgumentException(Res.EFPGridProducer_Arg_SourceColumnWithComma, "sourceColumnNames");

          // 21.05.2021
          if (String.Equals(name, sourceColumnNames[i], StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(String.Format(Res.EFPGridProducer_Arg_SourceColumnNameIsName, name), "sourceColumnNames");

          for (int j = 0; j < i; j++)
          {
            if (String.Equals(sourceColumnNames[i], sourceColumnNames[j], StringComparison.OrdinalIgnoreCase))
              throw new ArgumentException(String.Format(Res.EFPGridProducer_Arg_SourceColumnNameTwice, sourceColumnNames[i]), "sourceColumnNames");
          }
        }

        _SourceColumnNames = sourceColumnNames;
        _ValueNeededArgs = new EFPGridProducerValueNeededEventArgs(this);
      }
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Условное имя.
    /// Для простого, невычисляемого столбца/подсказки равно имени поля.
    /// Задается в конструкторе.
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    string IObjectWithCode.Code { get { return _Name; } }

    /// <summary>
    /// Возвращает свойство <see cref="Name"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Name;
    }

    /// <summary>
    /// Имя для отображения в настройке просмотра. Если не задано в явном
    /// виде, то возвращает свойство <see cref="Name"/>.
    /// Также свойство используется для отображения всплывающей подсказки при 
    /// наведении курсора на заголовок столбца.
    /// </summary>
    public virtual string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return GetDefaultDisplayName();
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Переопределяется для <see cref="EFPGridProducerColumn"/>
    /// </summary>
    /// <returns>Свойство <see cref="Name"/></returns>
    protected virtual string GetDefaultDisplayName()
    {
      return _Name;
    }

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region SourceColumnNames

    /// <summary>
    /// Возвращает true, если столбец/подсказка является вычисляемым, а не извлекает данные из источника данных (таблицы)
    /// </summary>
    public bool IsCalculated { get { return _SourceColumnNames != null; } }

    /// <summary>
    /// Исходные столбцы для вычисляемого столбца.
    /// Если столбец извлекает данные из источника, то свойство равно null.
    /// Если столбец не нуждается в данных исходных столбцов, то свойство равно пустому массиву строк.
    /// </summary>
    public string[] SourceColumnNames { get { return _SourceColumnNames; } }
    private readonly string[] _SourceColumnNames;

    /// <summary>
    /// Получить имена полей, которые должны быть в наборе данных.
    /// Если столбец является вычисляемым, в список добавляются имена исходных столбцов <see cref="SourceColumnNames"/>.
    /// Иначе добавляется имя <see cref="Name"/> для невычисляемого столбца/подсказки.
    /// Если столбец является вычисляемым, но не использует исходные столбцы, то ничего не добавляется в список.
    /// </summary>
    /// <param name="columns">Список для добавления имен полей</param>
    public virtual void GetColumnNames(IList<string> columns)
    {
#if DEBUG
      if (columns == null)
        throw new ArgumentNullException("columns");
#endif

      if (_SourceColumnNames == null)
        columns.Add(Name);
      else
      {
        for (int i = 0; i < _SourceColumnNames.Length; i++)
          columns.Add(_SourceColumnNames[i]);
      }
    }

    #endregion

    #region Событие ValueNeeded

    /// <summary>
    /// Обработчик вызывается для вычисляемого столбца каждый раз, когда требуется получить значение.
    /// Для невычисляемого столбца не вызывается.
    /// </summary>
    public event EFPGridProducerValueNeededEventHandler ValueNeeded;

    /// <summary>
    /// Получение значения вычисляемого столбца/подсказки.
    /// Если столбец/подсказка не является вычисляемым, метод не вызывается.
    /// Непереопределенный метод вызывает обработчик события <see cref="ValueNeeded"/>, если он установлен.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      if (ValueNeeded != null)
        ValueNeeded(this, args);
    }

    /// <summary>
    /// Чтобы каждый раз не создавать новый объект аргументов
    /// </summary>
    private readonly EFPGridProducerValueNeededEventArgs _ValueNeededArgs;


    internal void DoGetValue(EFPGridProducerValueReason reason, EFPDataViewRowInfo rowInfo, out object value, out string toolTipText)
    {
#if DEBUG
      EFPApp.CheckMainThread();
#endif


      if (IsCalculated)
      {
        _ValueNeededArgs.RowInfo = rowInfo;
        _ValueNeededArgs.Reason = reason;
        _ValueNeededArgs.Value = null;
        _ValueNeededArgs.ToolTipText = null;
        OnValueNeeded(_ValueNeededArgs);
        _ValueNeededArgs.RowInfo = new EFPDataViewRowInfo(); // освобождаем память
        value = _ValueNeededArgs.Value;
        toolTipText = _ValueNeededArgs.ToolTipText;
      }
      else
      {
        value = rowInfo.Values.GetValue(Name);
        toolTipText = null;
      }

      if (value == null || value is DBNull)
        value = EmptyValue;
    }

    /// <summary>
    /// Пустое значение.
    /// Используется при получении значения, когда значение поля или значение, вычисленное обработчиком события <see cref="ValueNeeded"/>,
    /// равно null или <see cref="DBNull"/>.
    /// По умолчанию - null.
    /// </summary>
    public object EmptyValue
    {
      get { return _EmptyValue; }
      set { _EmptyValue = value; }
    }
    private object _EmptyValue;

    #endregion
  }
}
