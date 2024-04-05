using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.UICore
{
  #region Делегат EFPSelRCValidatingEventHandler

  /// <summary>
  /// Аргументы события <see cref="UISelRCColumn.Validating"/>
  /// </summary>
  public class UISelRCValidatingEventArgs : EventArgs, IUIValidableObject
  {
    #region Защищенный конструктор

    internal UISelRCValidatingEventArgs(UISelRCGridData data)
    {
      _Data = data;
    }

    #endregion

    #region Инициализация

    /// <summary>
    /// Инициализация.
    /// Так как событие Validating обычно вызывается много раз подряд, не выгодно создавать объект
    /// UISelRCValidatingEventArgs каждый раз. Вместо этого объект создается однократно, а затем вызывается
    /// метод InitSource() для каждого значения, после чего вызывается событие Validating.
    /// </summary>
    internal void InitSource(int rowIndex, int columnIndex)
    {
      _RowIndex = rowIndex;
      _ColumnIndex = columnIndex;
      _ResultValue = _Data.SourceData[rowIndex, columnIndex];
      _ValidateState = UIValidateState.Ok;
      _ErrorText = null;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Набор данных, для которых выполняется проверка.
    /// </summary>
    public UISelRCGridData Data { get { return _Data; } }
    private UISelRCGridData _Data;

    /// <summary>
    /// Строка матрицы <see cref="UISelRCGridData.SourceData"/>, для которой выполняется проверка
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private int _RowIndex;

    /// <summary>
    /// Столбец матрицы <see cref="UISelRCGridData.SourceData"/>, для которого выполняется проверка
    /// </summary>
    public int ColumnIndex { get { return _ColumnIndex; } }
    private int _ColumnIndex;

    /// <summary>
    /// Исходные данные из буфера обмена (строка), которые нужно проверить и преобразовать в значение.
    /// Может быть пустая строка.
    /// </summary>
    public string SourceText { get { return _Data.SourceData[_RowIndex, _ColumnIndex]; } }

    /// <summary>
    /// Результат преобразования. По умолчанию, значение совпадает со строкой <see cref="SourceText"/>.
    /// Обработчик может установить собственное значение, не обязательно строковое.
    /// </summary>
    public object ResultValue { get { return _ResultValue; } set { _ResultValue = value; } }
    private object _ResultValue;

    #endregion

    #region Результаты проверки

    /// <summary>
    /// Результат вызова события <see cref="UISelRCColumn.Validating"/>.
    /// По умолчанию - <see cref="UIValidateState.Ok"/>. Для установки признака ошибки или прдупреждения используйте методы
    /// <see cref="SetError(string)"/> и <see cref="SetWarning(string)"/> соответственно
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidateState; } }
    private UIValidateState _ValidateState;

    /// <summary>
    /// Сообщение об ошибке или предупреждении, установленное вызовом <see cref="SetError(string)"/> и <see cref="SetWarning(string)"/>.
    /// </summary>
    public string ErrorText { get { return _ErrorText; } }
    private string _ErrorText;

    /// <summary>
    /// Установить признак ошибочного значения.
    /// Если уже было установлено состояние ошибки, то никаких действий не выполняется.
    /// Если было установлено предупреждение, то установка выполняется.
    /// </summary>
    /// <param name="message">Текст сообщения об ошибке</param>
    public void SetError(string message)
    {
      if (ValidateState != UIValidateState.Error)
      {
        _ValidateState = UIValidateState.Error;
        _ErrorText = message;
      }
    }

    /// <summary>
    /// Задать предупреждение.
    /// Если уже было установлено состояние ошибки или предупреждения, то никаких действий не выполняется.
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public void SetWarning(string message)
    {
      if (ValidateState == UIValidateState.Ok)
      {
        _ValidateState = UIValidateState.Warning;
        _ErrorText = message;
      }
    }

    /// <summary>
    /// Вспомогательный метод, вызывающий <see cref="SetError(string)"/> или <see cref="SetWarning(string)"/>.
    /// Вызов с <paramref name="state"/>==<see cref="UIValidateState.Ok"/> не выполняет никаких действий,
    /// текущее значение <see cref="ValidateState"/> не меняется.
    /// </summary>
    /// <param name="state">Состояние</param>
    /// <param name="message">Сообщение</param>
    public void SetState(UIValidateState state, string message)
    {
      if (ValidateState == UIValidateState.Error)
        return;

      switch (state)
      {
        case UIValidateState.Error:
          SetError(message);
          break;
        case UIValidateState.Warning:
          if (ValidateState == UIValidateState.Ok)
            SetWarning(message);
          break;
      }
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="UISelRCColumn.Validating"/>.
  /// </summary>
  /// <param name="sender">Ссылка на <see cref="UISelRCColumn"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void UISelRCValidatingEventHandler(object sender, UISelRCValidatingEventArgs args);

  #endregion

  /// <summary>
  /// Описание для одного столбца для модели <see cref="UISelRCGridData"/>.
  /// Этот класс является базовым и используется для ввода текстовых значений.
  /// Для ввода типизированных значений удобнее использовать производные классы.
  /// </summary>
  public class UISelRCColumn: IObjectWithCode
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public UISelRCColumn(string code)
    {
      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");

      _Code = code;
      _CanBeEmpty = true;
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public UISelRCColumn(string code, string displayName)
      : this(code)
    {
      this.DisplayName = displayName;
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    /// <param name="validateHandler">Делегат для проверки корректности данных</param>
    public UISelRCColumn(string code, string displayName, UISelRCValidatingEventHandler validateHandler)
      : this(code, displayName)
    {
      if (validateHandler != null)
        Validating += validateHandler;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Код столбца, является чувствительным к региструю
    /// Обязательное свойство, устанавливается в конструкторе.
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Имя, отображаемое в выпадающем списке выбора назначения столбца.
    /// Если свойство не задано в явном виде, возвращает значение свойства <see cref="Code"/>.
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return _Code;
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
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region Получение и проверка значения

    /// <summary>
    /// Если true (по умолчанию), то ячейки столбца могут содержать пустые значения.
    /// Если свойство равно false, то для пустой ячейки устанавливается ошибка, а пользовательский обработчик
    /// не устанавливается.
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Вспомогательный метод.
    /// Устанавливает <see cref="CanBeEmpty"/>=false и возвращает текущий объект.
    /// Удобно использовать при инициализации массива описаний.
    /// </summary>
    /// <returns>Указатель this</returns>
    public UISelRCColumn SetRequired()
    {
      CanBeEmpty = false;
      return this;
    }

    /// <summary>
    /// Событие вызывается для проверки корректности строкового значения и, возможно, для преобразования значения.
    /// Если обработчик события не установлен, столбец допускает любые строковые значения, включая пустую строку ""
    /// Обработчик вызывается в том числе и для пустых строк, если свойство <see cref="CanBeEmpty"/>=true.
    /// </summary>
    public event UISelRCValidatingEventHandler Validating;

    /// <summary>
    /// Проверка корректности данных.
    /// Непереопределенный метод, после проверки наличия данных вызывает обработчик события <see cref="Validating"/>.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public virtual void PerformValidating(UISelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceText) && (!CanBeEmpty))
      {
        args.SetError("Пустые значения не допускаются");
        return;
      }

      if (Validating != null)
        Validating(this, args);
    }

    #endregion
  }

  #region Расширенные реализации столбцов

  /// <summary>
  /// Столбец для вставки значения типа "Дата".
  /// Значение <see cref="UISelRCValidatingEventArgs.ResultValue"/> имеет тип <see cref="Nullable{DateTime}"/>.
  /// Пустая строка трактуется как null (при <see cref="UISelRCColumn.CanBeEmpty"/>=true).
  /// </summary>
  public class UISelRCDateColumn : UISelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public UISelRCDateColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public UISelRCDateColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод <see cref="ValidateValue(UISelRCValidatingEventArgs, DateTime)"/>.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение <see cref="UISelRCValidatingEventArgs.ValidateState"/>, и не выполнять проверку, если есть ошибки</param>
    public UISelRCDateColumn(string code, string displayName, UISelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в дату.
    /// В случае успеха вызывается метод <see cref="ValidateValue(UISelRCValidatingEventArgs, DateTime)"/>, а затем - метод базового класса для вызова пользовательского обработчика события <see cref="UISelRCColumn.Validating"/>.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public override void PerformValidating(UISelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceText))
        args.ResultValue = null;
      else
      {
        DateTime value;
        string s = args.SourceText;
        //if (!DataConv.TryDateFromStr10(s, out dt))
        //{
        if (!DateTime.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceText + "\" нельзя преобразовать в дату");
          return;
        }
        //}
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из <see cref="PerformValidating(UISelRCValidatingEventArgs)"/> после успешного преобразования строки в дату.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(UISelRCValidatingEventArgs args, DateTime value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки числового значения.
  /// Значение <see cref="UISelRCValidatingEventArgs.ResultValue"/> имеет тип <see cref="Int32"/>.
  /// Пустая строка трактуется как 0 (при <see cref="UISelRCColumn.CanBeEmpty"/>=true).
  /// </summary>
  public class UISelRCIntColumn : UISelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public UISelRCIntColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public UISelRCIntColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод <see cref="ValidateValue(UISelRCValidatingEventArgs, int)"/>.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение <see cref="UISelRCValidatingEventArgs.ValidateState"/>, и не выполнять проверку, если есть ошибок</param>
    public UISelRCIntColumn(string code, string displayName, UISelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в число.
    /// В случае успеха вызывается метод <see cref="ValidateValue(UISelRCValidatingEventArgs, int)"/>, а затем - метод базового класса для вызова пользовательского обработчика события <see cref="UISelRCColumn.Validating"/>.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    public override void PerformValidating(UISelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceText))
        args.ResultValue = 0;
      else
      {
        int value;
        string s = args.SourceText;
        UITools.CorrectNumberString(ref s);
        if (!Int32.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceText + "\" нельзя преобразовать в целое число");
          return;
        }
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из <see cref="PerformValidating(UISelRCValidatingEventArgs)"/> после успешного преобразования строки в число.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(UISelRCValidatingEventArgs args, int value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки числового значения.
  /// Значение <see cref="UISelRCValidatingEventArgs.ResultValue"/> имеет тип <see cref="Single"/>.
  /// Пустая строка трактуется как 0 (при <see cref="UISelRCColumn.CanBeEmpty"/>=true).
  /// </summary>
  public class UISelRCSingleColumn : UISelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public UISelRCSingleColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public UISelRCSingleColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод <see cref="ValidateValue(UISelRCValidatingEventArgs, float)"/>.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение <see cref="UISelRCValidatingEventArgs.ValidateState"/>, и не выполнять проверку, если есть ошибок</param>
    public UISelRCSingleColumn(string code, string displayName, UISelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в число.
    /// В случае успеха вызывается метод <see cref="ValidateValue(UISelRCValidatingEventArgs, float)"/>, а затем - метод базового класса для вызова пользовательского обработчика события <see cref="UISelRCColumn.Validating"/>.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    public override void PerformValidating(UISelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceText))
        args.ResultValue = 0m;
      else
      {
        float value;
        string s = args.SourceText;
        UITools.CorrectNumberString(ref s);
        if (!Single.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceText + "\" нельзя преобразовать в числовое значение");
          return;
        }
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из <see cref="PerformValidating(UISelRCValidatingEventArgs)"/> после успешного преобразования строки в число.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(UISelRCValidatingEventArgs args, float value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки числового значения.
  /// Значение <see cref="UISelRCValidatingEventArgs.ResultValue"/> имеет тип <see cref="Double"/>.
  /// Пустая строка трактуется как 0 (при <see cref="UISelRCColumn.CanBeEmpty"/>=true).
  /// </summary>
  public class UISelRCDoubleColumn : UISelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public UISelRCDoubleColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public UISelRCDoubleColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод <see cref="ValidateValue(UISelRCValidatingEventArgs, double)"/>.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение <see cref="UISelRCValidatingEventArgs.ValidateState"/>, и не выполнять проверку, если есть ошибок</param>
    public UISelRCDoubleColumn(string code, string displayName, UISelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в число.
    /// В случае успеха вызывается метод <see cref="ValidateValue(UISelRCValidatingEventArgs, double)"/>, а затем - метод базового класса для вызова пользовательского обработчика события <see cref="UISelRCColumn.Validating"/>.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    public override void PerformValidating(UISelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceText))
        args.ResultValue = 0m;
      else
      {
        double value;
        string s = args.SourceText;
        UITools.CorrectNumberString(ref s);
        if (!Double.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceText + "\" нельзя преобразовать в числовое значение");
          return;
        }
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из <see cref="PerformValidating(UISelRCValidatingEventArgs)"/> после успешного преобразования строки в число.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(UISelRCValidatingEventArgs args, double value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки числового значения.
  /// Значение <see cref="UISelRCValidatingEventArgs.ResultValue"/> имеет тип <see cref="Decimal"/>.
  /// Пустая строка трактуется как 0 (при <see cref="UISelRCColumn.CanBeEmpty"/>=true).
  /// </summary>
  public class UISelRCDecimalColumn : UISelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public UISelRCDecimalColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public UISelRCDecimalColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод <see cref="ValidateValue(UISelRCValidatingEventArgs, decimal)"/>.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение <see cref="UISelRCValidatingEventArgs.ValidateState"/>, и не выполнять проверку, если есть ошибок</param>
    public UISelRCDecimalColumn(string code, string displayName, UISelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в число.
    /// В случае успеха вызывается метод <see cref="ValidateValue(UISelRCValidatingEventArgs, decimal)"/>, а затем - метод базового класса для вызова пользовательского обработчика события <see cref="UISelRCColumn.Validating"/>.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    public override void PerformValidating(UISelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceText))
        args.ResultValue = 0m;
      else
      {
        decimal value;
        string s = args.SourceText;
        UITools.CorrectNumberString(ref s);
        if (!decimal.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceText + "\" нельзя преобразовать в числовое значение");
          return;
        }
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из <see cref="PerformValidating(UISelRCValidatingEventArgs)"/> после успешного преобразования строки в число.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(UISelRCValidatingEventArgs args, decimal value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки значения перечислимого типа <typeparamref name="T"/>
  /// Значение <see cref="UISelRCValidatingEventArgs.ResultValue"/> имеет перечислимый тип.
  /// Использует список строковых значений, соответствующих элементам перечисления.
  /// Пустая строка трактуется как null (при <see cref="UISelRCColumn.CanBeEmpty"/>=true).
  /// </summary>
  /// <typeparam name="T">Тип перечисления. Поддерживаются только простые перечисления,
  /// имеющие значения 0,1,2, ... Для нерегулярных перечислений используйте класс <see cref="UISelRCEnumColumnWithDict{T}"/>.</typeparam>
  public class UISelRCEnumColumn<T> : UISelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным списком значений
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="textValues">Список текстовых значений</param>
    public UISelRCEnumColumn(string code, string displayName, string[] textValues)
      : this(code, displayName, textValues, null)
    {
    }

    /// <summary>
    /// Создает столбец с заданным списком значений.
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод <see cref="ValidateValue(UISelRCValidatingEventArgs, T)"/>.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="textValues">Список текстовых значений</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение <see cref="UISelRCValidatingEventArgs.ValidateState"/>, и не выполнять проверку, если есть ошибки</param>
    public UISelRCEnumColumn(string code, string displayName, string[] textValues, UISelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
      if (textValues == null)
        throw new ArgumentNullException("textValues");
      if (textValues.Length == 0)
        throw new ArgumentException("TextValues.Length=0", "textValues");
      _TextValues = textValues;
      _TextValueIndexer = new StringArrayIndexer(textValues, true);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список текстовых значений, соответствующих элементам перечисления со значениями 0,1,2, ...
    /// Регистр символов не учитывается.
    /// Список создается в конструкторе.
    /// </summary>
    public string[] TextValues { get { return _TextValues; } }
    private readonly string[] _TextValues;

    private readonly StringArrayIndexer _TextValueIndexer;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в перечисление.
    /// В случае успеха вызывается метод <see cref="ValidateValue(UISelRCValidatingEventArgs, T)"/>, а затем - метод базового класса для вызова пользовательского обработчика события <see cref="UISelRCColumn.Validating"/>.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    public override void PerformValidating(UISelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceText))
      {
        args.ResultValue = null;
      }
      else
      {
        int p = _TextValueIndexer.IndexOf(args.SourceText);
        if (p < 0)
        {
          args.SetError("Неизвестная строка \"" + args.SourceText + "\"");
          args.ResultValue = null;
        }
        else
        {
          T value = (T)(Enum.ToObject(typeof(T), p));
          args.ResultValue = value;
          ValidateValue(args, value);
        }
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из <see cref="PerformValidating(UISelRCValidatingEventArgs)"/> после успешного преобразования строки в перечислимое значение.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(UISelRCValidatingEventArgs args, T value)
    {
    }

    #endregion
  }


  /// <summary>
  /// Столбец для вставки значения перечислимого типа <typeparamref name="T"/>
  /// Значение ResultValue имеет перечислимый тип.
  /// Использует словарь строковых значений, соответствующих элементам перечисления.
  /// В словаре несколько строк могут соответствовать одному значению.
  /// Пустая строка трактуется как null (при CanBeEmpty=true).
  /// Для простых перечислений, у которых используются числовые значения 0,1,2,... удобнее использовать <see cref="UISelRCEnumColumn{T}"/>.
  /// </summary>
  /// <typeparam name="T">Тип перечисления</typeparam>
  public class UISelRCEnumColumnWithDict<T> : UISelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным словарем значений.
    /// Учет регистра символов определяется коллекцией <paramref name="textValues"/>.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="textValues">Список текстовых значений и соответствующих им элементов перечисления</param>
    public UISelRCEnumColumnWithDict(string code, string displayName, TypedStringDictionary<T> textValues)
      : this(code, displayName, textValues, null)
    {
    }

    /// <summary>
    /// Создает столбец с заданным словарем значений.
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Учет регистра символов определяется коллекцией <paramref name="textValues"/>.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="textValues">Список текстовых значений и соответствующих им элементов перечисления</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение <see cref="UISelRCValidatingEventArgs.ValidateState"/>, и не выполнять проверку, если есть ошибки</param>
    public UISelRCEnumColumnWithDict(string code, string displayName, TypedStringDictionary<T> textValues, UISelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
      if (textValues == null)
        throw new ArgumentNullException("textValues");
      if (textValues.Count == 0)
        throw new ArgumentException("textValues.Count=0", "textValues");
      _TextValues = textValues;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список текстовых значений и соответствующих им элементов перечисления.
    /// Список задается в конструкторе.
    /// </summary>
    public TypedStringDictionary<T> TextValues { get { return _TextValues; } }
    private TypedStringDictionary<T> _TextValues;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в перечисление.
    /// В случае успеха вызывается метод <see cref="ValidateValue(UISelRCValidatingEventArgs, T)"/>, а затем - метод базового класса для вызова пользовательского обработчика события <see cref="UISelRCColumn.Validating"/>.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    public override void PerformValidating(UISelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceText))
        args.ResultValue = null;
      else
      {
        T value;
        if (TextValues.TryGetValue(args.SourceText, out value))
        {
          args.ResultValue = value;
          ValidateValue(args, value);
        }
        else
        {
          args.SetError("Неизвестная строка \"" + args.SourceText + "\"");
          args.ResultValue = null;
        }
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из <see cref="PerformValidating(UISelRCValidatingEventArgs)"/> после успешного преобразования строки в перечислимое значение.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события <see cref="UISelRCColumn.Validating"/></param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(UISelRCValidatingEventArgs args, T value)
    {
    }

    #endregion
  }

  #endregion

  /// <summary>
  /// Данные для табличного просмотра текстовой матрицы (свойство EFPSelRCDataGridView.Data).
  /// Содержит исходный двумерный массив строк и список столбцов, из которых можно делать выбор.
  /// Свойства-коллекции <see cref="UISelRCGridData.SelRows"/> и <see cref="UISelRCGridData.SelColumns"/> содержат текущие результаты выбора, сделанные пользователем.
  /// Класс не является потокобезопасным.
  /// 
  /// Имеется неоднозначность с термином "столбец". Он означает как столбец в матрице исходных данных, так и описание
  /// столбца.
  /// </summary>
  public sealed class UISelRCGridData
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует набор данных.
    /// </summary>
    /// <param name="sourceData">Матрица текстовых строк (обычно вставляемая из буфера обмена). Не может быть null.</param>
    /// <param name="availableColumns">Список описателей столбцов, доступных для выбора пользователем. Не может быть null или содержать элементы null.
    /// Все столбцы должны иметь разные значения свойства <see cref="UISelRCColumn.Code"/>.</param>
    public UISelRCGridData(string[,] sourceData, IEnumerable<UISelRCColumn> availableColumns)
    {
      if (sourceData == null)
        throw new ArgumentNullException("sourceData");
      if (availableColumns == null)
        throw new ArgumentNullException("availableColumns");

      _SourceData = sourceData;
      _AvailableColumns = new NamedList<UISelRCColumn>(availableColumns, false, true);

      _SelRows = new SelRowCollection(this);
      _SelColumns = new SelColumnCollection(this);
      _ValidatingArgs = new UISelRCValidatingEventArgs(this);
    }

    #endregion

    #region SourceData

    /// <summary>
    /// Двумерный массив строк исходных данных.
    /// Задается в конструкторе.
    /// </summary>
    public string[,] SourceData { get { return _SourceData; } }
    private readonly string[,] _SourceData;

    /// <summary>
    /// Возвращает количество исходных строк в <see cref="SourceData"/>.
    /// </summary>
    public int RowCount { get { return _SourceData.GetLength(0); } }

    /// <summary>
    /// Возвращает количество исходных столбцов в <see cref="SourceData"/>.
    /// </summary>
    public int ColumnCount { get { return _SourceData.GetLength(1); } }

    #endregion

    #region AvailableColumns

    /// <summary>
    /// Полный список описаний возможных столбцов, из которых можно осуществлять выбор.
    /// Список доступен только для просмотра.
    /// Задается в конструкторе.
    /// </summary>
    public NamedList<UISelRCColumn> AvailableColumns { get { return _AvailableColumns; } }
    private readonly NamedList<UISelRCColumn> _AvailableColumns;

    #endregion

    #region SelRows и SelColumns

    /// <summary>
    /// Реализация свойства <see cref="SelRows"/>
    /// </summary>
    public sealed class SelRowCollection
    {
      // Нельзя делать одноразовой структурой.
      // Компилятор будет ругаться на присвоение свойства this

      #region Конструктор

      internal SelRowCollection(UISelRCGridData data)
      {
        _Data = data;
        _Items = new bool[data.RowCount];
      }

      #endregion

      #region Свойства

      private UISelRCGridData _Data;

      /// <summary>
      /// Возвращает или устанавливает отметку для строки
      /// </summary>
      /// <param name="index">Индекс строки</param>
      /// <returns>Наличие отметки</returns>
      public bool this[int index]
      {
        get { return _Items[index]; }
        set
        {
          if (value == _Items[index])
            return;
          _Items[index] = value;
          _Data.CallChanged();
        }
      }
      private bool[] _Items;


      /// <summary>
      /// Возвращает true, если ни одна из строк не выбрана
      /// </summary>
      public bool IsEmpty
      {
        get
        {
          return Array.IndexOf<bool>(_Items, true) < 0;
        }
      }

      /// <summary>
      /// Возвращает true, если выбраны все строки
      /// </summary>
      public bool IsFull
      {
        get
        {
          return Array.IndexOf<bool>(_Items, false) < 0;
        }
      }

      /// <summary>
      /// Флажки для выбранных строк в виде массива
      /// </summary>
      public bool[] AsArray
      {
        get
        {
          bool[] a = new bool[_Items.Length];
          _Items.CopyTo(a, 0);
          return a;
        }
        set
        {
          if (value == null)
            Clear();
          else
          {
            int n = Math.Min(value.Length, _Items.Length);
            DataTools.FillArray<bool>(_Items, false);
            Array.Copy(value, _Items, n);
            _Data.CallChanged();
          }
        }
      }

      /// <summary>
      /// Возвращает количество выбранных строк
      /// </summary>
      public int SelectionCount
      {
        get
        {
          int n = 0;
          for (int i = 0; i < _Items.Length; i++)
          {
            if (_Items[i])
              n++;
          }
          return n;
        }
      }

      #endregion

      #region Методы

      /// <summary>
      /// Очищает список выбранных строк
      /// </summary>
      public void Clear()
      {
        if (IsEmpty)
          return;
        DataTools.FillArray<bool>(_Items, false);
        _Data.CallChanged();
      }

      /// <summary>
      /// Устанавливает отметки для всех непустых строк.
      /// </summary>
      public void Init()
      {
        // 12.12.2017
        // Ставим флажки только для непустых строк
        // DataTools.FillArray<bool>(FSelRows, true);

        int columnCount = _Data.ColumnCount;
        for (int i = 0; i < _Items.Length; i++)
        {
          for (int j = 0; j < columnCount; j++)
          {
            if (!String.IsNullOrEmpty(_Data._SourceData[i, j]))
            {
              _Items[i] = true;
              break;
            }
          }
        }
        _Data.CallChanged();
      }

      #endregion
    }

    /// <summary>
    /// Флажки для выбранных строк.
    /// </summary>
    public SelRowCollection SelRows { get { return _SelRows; } }
    private readonly SelRowCollection _SelRows;

    /// <summary>
    /// Реализация свойства <see cref="SelColumns"/>
    /// </summary>
    public sealed class SelColumnCollection
    {
      // Нельзя делать одноразовой структурой.
      // Компилятор будет ругаться на присвоение свойства this

      #region Конструктор

      internal SelColumnCollection(UISelRCGridData data)
      {
        _Data = data;
        _Items = new UISelRCColumn[data.ColumnCount];
      }

      #endregion

      #region Свойства

      private UISelRCGridData _Data;

      /// <summary>
      /// Возвращает или назначает выбранное описание для столбца.
      /// Свойство индексируется по столбцам матрицы исходных данных.
      /// </summary>
      /// <param name="index">Индекс столбца в матрице <see cref="SourceData"/></param>
      /// <returns>Один из элементов в <see cref="AvailableColumns"/> или null, если столбец не используется</returns>
      public UISelRCColumn this[int index]
      {
        get { return _Items[index]; }
        set
        {
          if (value == _Items[index])
            return;
          _Items[index] = value;
          ResetPositions();
          _Data.CallChanged();
        }
      }
      private UISelRCColumn[] _Items;

      /// <summary>
      /// Возвращает true, если нет ни одного назначенного описания столбца,
      /// то есть все элементы возвращают значения null.
      /// </summary>
      public bool IsEmpty
      {
        get
        {
          PreparePositions();
          return _IsEmpty;
        }
      }

      /// <summary>
      /// Возвращает true, если всем столбцам назначены описания, 
      /// то есть все элементы возвращают значения, не равные null.
      /// Корректность не проверяется, то есть одно описание может быть назначено несколько раз.
      /// </summary>
      public bool IsFull 
      {
        get
        {
          PreparePositions();
          return _IsFull;
        }
      }


      /// <summary>
      /// Коды описаний выбранных столбцов. 
      /// Свойство возвращает массив, длина которого равна количеству столбцов исходной матрицы <see cref="ColumnCount"/>.
      /// Для столбцов с назначенным описанием возвращается свойство <see cref="UISelRCColumn.Code"/>.
      /// Для невыбранных столбцов элементы возвращаемого массива содержат пустую строку.
      /// При установке свойства для ненайденных кодов, как и для пустых строк/null, устанавливается значение соответствующего элемента SelColumns равным null.
      /// Присваиваемый массив может быть короче или длиннее массива <see cref="SelColumns"/>. Лишние элементы игнорируются,
      /// недостающие принимают значение null.
      /// Так как одно и то же описание может быть назначено сразу нескольким столбцам (что является ошибкой
      /// и не позволит закрыть диалог с табличным просмотром), коды в массиве могут повторяться.
      /// </summary>
      public string[] Codes
      {
        get
        {
          string[] a = new string[_Items.Length];
          for (int i = 0; i < _Items.Length; i++)
          {
            if (_Items[i] == null)
              a[i] = String.Empty;
            else
              a[i] = _Items[i].Code;
          }
          return a;
        }
        set
        {
          if (value == null)
            value = DataTools.EmptyStrings;

          _Data.BeginUpdate();
          try
          {
            for (int i = 0; i < _Items.Length; i++)
            {
              if (i < value.Length)
              {
                if (String.IsNullOrEmpty(value[i]))
                  this[i] = null;
                else
                {
                  int p = _Data._AvailableColumns.IndexOf(value[i]);
                  if (p < 0)
                    this[i] = null;
                  else
                    this[i] = _Data._AvailableColumns[p];
                }
              }
              else // короткий массив value
                this[i] = null;
            }
          }
          finally
          {
            _Data.EndUpdate();
          }
        }
      }

      /// <summary>
      /// Получение и установка списка кодов выбранных столбцов в виде строки CSV.
      /// Количество запятых в возвращаемой строке равно (<see cref="ColumnCount"/>-1).
      /// Дублирует свойство <see cref="Codes"/> и может использоваться для хранения настроек просмотра.
      /// Как и для <see cref="Codes"/>, в строке могут быть повторяющиеся элементы.
      /// При установке свойства, количество элементов может не совпадать с <see cref="ColumnCount"/>.
      /// </summary>
      public string AsString
      {
        get
        {
          CsvTextConvert conv = new CsvTextConvert();
          return conv.ToString(Codes);
        }
        set
        {
          CsvTextConvert conv = new CsvTextConvert();
          Codes = conv.ToArray(value);
        }
      }

      /// <summary>
      /// Возвращает количество столбцов, для которых заданы соответствия
      /// </summary>
      public int SelectionCount
      {
        get
        {
          int n = 0;
          for (int i = 0; i < _Items.Length; i++)
          {
            if (_Items[i] != null)
              n++;
          }
          return n;
        }
      }

      /// <summary>
      /// Возвращает массив кодов описаний столбцов, которым не назначены описания
      /// </summary>
      public string[] UnassignedCodes
      {
        get
        {
          if (IsEmpty)
            return _Data.AvailableColumns.GetCodes();
          else
          {
            List<string> lst = null;
            for (int i = 0; i < _Data.AvailableColumns.Count; i++)
            {
              string code = _Data.AvailableColumns[i].Code;
              if (!Contains(code))
              {
                if (lst == null)
                  lst = new List<string>();
                lst.Add(code);
              }
            }
            if (lst == null)
              return DataTools.EmptyStrings;
            else
              return lst.ToArray();
          }
        }
      }

      #endregion

      #region Буферизация позиций столбцов и повторов

      /// <summary>
      /// Позиции выбранных столбцов.
      /// Ключом является код описания столбца, значением - индекс в SelColumns.
      /// Используется в методе IndexOf().
      /// Повторные (ошибочные) назначения отбрасываются.
      /// Если есть столбцы без назначенного описания, коллекция содержит ключ "".
      /// При изменениях в SelColumns обнуляется. Инициализируется методом PreparePositions().
      /// </summary>
      [NonSerialized]
      private Dictionary<string, int> _CodePositions;

      /// <summary>
      /// Содержит (ошибочные) повторы в выбранных описания столбцов.
      /// Ключом является индекс ошибочного столбца (от 0 до ColumnCount).
      /// Значение не используется.
      /// Если нет повторов, поле равно null. В противном случае коллекция содержит, как минимум, два элемента.
      /// Инициализируется методом PreparePositions().
      /// </summary>
      [NonSerialized]
      private Dictionary<int, object> _Repeats;

      /// <summary>
      /// true, если нет назначенных описаний столбцов
      /// </summary>
      [NonSerialized]
      private bool _IsEmpty;

      /// <summary>
      /// true, если всем столбцам назначено описание
      /// </summary>
      [NonSerialized]
      private bool _IsFull;

      private void PreparePositions()
      {
        if (_CodePositions == null)
          DoPreparePositions();
      }

      private void DoPreparePositions()
      {
        _CodePositions = new Dictionary<string, int>();
        _Repeats = null;
        _IsEmpty = true;
        _IsFull = true;
        for (int i = 0; i < _Items.Length; i++)
        {
          if (_Items[i] == null)
          {
            if (!_CodePositions.ContainsKey(String.Empty))
              _CodePositions.Add(String.Empty, i);
            _IsFull = false;
          }
          else
          {
            _IsEmpty = false;
            int prevColIndex;
            if (_CodePositions.TryGetValue(_Items[i].Code, out prevColIndex))
            {
              // Обнаружен повтор
              if (_Repeats == null)
                _Repeats = new Dictionary<int, object>();
              _Repeats[prevColIndex] = null; // нельзя использовать Add(), т.к. может быть и тройное назначение, а не только повторное
              _Repeats.Add(i, null);
            }
            else
              _CodePositions.Add(_Items[i].Code, i);
          }
        }
      }

      private void ResetPositions()
      {
        _CodePositions = null;
      }

      #endregion

      #region Методы

      /// <summary>
      /// Возвращает индекс столбца в матрице, для которого выбрано указанное описание.
      /// Если данное описание столбца не выбрано, возвращается (-1).
      /// Если описание (ошибочно) назначено больше, чем для одного столбца, возвращается индекс первого столбца.
      /// </summary>
      /// <param name="column">Столбец из массива <see cref="AvailableColumns"/>. 
      /// Для null возвращает индекс первого столбца, для которого не назначено описание.</param>
      /// <returns>Индекс выбранного столбца</returns>
      public int IndexOf(UISelRCColumn column)
      {
        PreparePositions();
        int pos;
        if (column == null)
        {
          if (_CodePositions.TryGetValue(String.Empty, out pos))
            return pos;
          else
            return -1;
        }
        else
        {
          if (_CodePositions.TryGetValue(column.Code, out pos))
          {
            if (Object.ReferenceEquals(column, this[pos]))
              return pos;
            else
              return -1; // можно было бы выбросить исключение
          }
          else
            return -1;
        }
      }

      /// <summary>
      /// Возвращает индекс столбца, для которого выбрано описание с заданным кодом.
      /// Если описание не выбрано, возвращается (-1).
      /// </summary>
      /// <param name="code">Код столбца из массива <see cref="AvailableColumns"/>. 
      /// Для пустой строки возвращает индекс первого столбца, для которого не назначено описание.</param>
      /// <returns>Индекс выбранного столбца</returns>
      public int IndexOf(string code)
      {
        PreparePositions();
        if (code == null)
          code = String.Empty;

        int pos;
        if (_CodePositions.TryGetValue(code, out pos))
          return pos;
        else
          return -1;
      }

      /// <summary>
      /// Возвращает индекс первого выбранного столбца с любым из заданных кодов описаний.
      /// Не может использоваться для поиска невыбранных столбцов.
      /// </summary>
      /// <param name="codes">Список кодов описаний из массива <see cref="AvailableColumns"/>, разделенных запятыми</param>
      /// <returns>Индекс </returns>
      public int IndexOfAny(string codes)
      {
        if (String.IsNullOrEmpty(codes))
          return -1;

        string[] a = codes.Split(',');
        int pRes = -1;
        for (int i = 0; i < a.Length; i++)
        {
          int p = IndexOf(a[i]);
          if (p < 0)
            continue;
          if (pRes < 0)
            pRes = p;
          else
            pRes = Math.Min(pRes, p);
        }
        return pRes;
      }

      /// <summary>
      /// Возвращает true, если в <see cref="SelColumns"/> выбрано описание с заданным кодом.
      /// </summary>
      /// <param name="code">Код описания столбца из массива <see cref="AvailableColumns"/>.
      /// Если null или пустая строка, то определяет наличие столбцов, у которых не назначено описание.</param>
      /// <returns>Наличие выбранного столбца</returns>
      public bool Contains(string code)
      {
        return IndexOf(code) >= 0;
      }

      /// <summary>
      /// Возвращает true, если столбцам назначено хотя бы одно из описаний с заданными кодами.
      /// Не может использоваться для поиска невыбранных столбцов.
      /// </summary>
      /// <param name="codes">Список кодов описаний из массива <see cref="AvailableColumns"/>, разделенных запятыми</param>
      /// <returns>Наличие выбранных описаний</returns>
      public bool ContainsAny(string codes)
      {
        if (String.IsNullOrEmpty(codes))
          return false;
        string[] a = codes.Split(',');
        for (int i = 0; i < a.Length; i++)
        {
          if (Contains(a[i]))
            return true;
        }
        return false;
      }

      /// <summary>
      /// Возвращает true, если столбцам назначены все описания с заданными кодами.
      /// Не может использоваться для поиска невыбранных столбцов.
      /// </summary>
      /// <param name="codes">Список кодов описаний из массива <see cref="AvailableColumns"/>, разделенных запятыми</param>
      /// <returns>Наличие выбранных описаний</returns>
      public bool ContainsAll(string codes)
      {
        if (String.IsNullOrEmpty(codes))
          return true;
        string[] a = codes.Split(',');
        for (int i = 0; i < a.Length; i++)
        {
          if (!Contains(a[i]))
            return false;
        }
        return true;
      }

      /// <summary>
      /// Определяет количество указанных описаний столбцов из списка, которые назначены столбцам.
      /// Если одно описание (ошибочно) назначено нескольким столбцам, то описание учитывается только один раз
      /// </summary>
      /// <param name="codes">Список кодов описаний из массива <see cref="AvailableColumns"/>, разделенных запятыми</param>
      /// <returns>Количество назначенных описаний в диапазоне от 0 до количества кодов в строке <paramref name="codes"/> включительно.</returns>
      public int ContainCount(string codes)
      {
        if (String.IsNullOrEmpty(codes))
          return 0;

        string[] a = codes.Split(',');
        int cnt = 0;
        for (int i = 0; i < a.Length; i++)
        {
          if (Contains(a[i]))
            cnt++;
        }
        return cnt;
      }

      /// <summary>
      /// Снимает назначение описаний для всех столбцов.
      /// </summary>
      public void Clear()
      {
        if (IsEmpty)
          return;
        DataTools.FillArray<UISelRCColumn>(_Items, null);
        ResetPositions();
        _Data.CallChanged();
      }

      /// <summary>
      /// Начальная инициализация назначений столбцов подходящими описаниями из списка <see cref="AvailableColumns"/>.
      /// Уже назначенные столбцы сохраняются.
      /// Также должны быть отмечены строки <see cref="SelRows"/>, иначе подходящие столбцы не могут быть назначены.
      /// При назначении проверяются условия:
      /// - столбцу в <see cref="SourceData"/> еще не назначено описание;
      /// - в <see cref="SourceData"/> для выбранных строк <see cref="SelRows"/> есть непустые текстовые значения; пустые столбцы пропускаются;
      /// - кандидат из <see cref="AvailableColumns"/> еще не назначен другому столбцу;
      /// - при проверке значения для всех отмеченных строк не возникает ошибки или предупреждения.
      /// </summary>
      public void Init()
      {
        // Столбцы, которые можно назначать
        List<UISelRCColumn> cols2 = new List<UISelRCColumn>(_Data.AvailableColumns);
        for (int i = 0; i < _Items.Length; i++)
        {
          if (_Items[i] != null) // уже использован в прикладном коде
            cols2.Remove(_Items[i]);
        }

        _Data.BeginUpdate();
        try
        {
          int rowCount = _Data.RowCount;

          for (int i = 0; i < _Items.Length; i++)
          {
            if (_Items[i] != null)
              continue; // уже назначен в прикладном коде

            bool columnIsEmpty = true;
            for (int k = 0; k < rowCount; k++)
            {
              if (!_Data.SelRows[k])
                continue; // 17.06.2022. Пустые строки не учитываются для распределения столбцов
              if (!String.IsNullOrEmpty(_Data._SourceData[k, i]))
              {
                columnIsEmpty = false;
                break;
              }
            }
            if (columnIsEmpty)
              continue; // 17.06.2022. Полностью пустому столбцу данных не назначаем столбец по умолчанию.

            for (int j = 0; j < cols2.Count; j++)
            {
              bool isOK = true;
              for (int k = 0; k < rowCount; k++)
              {
                if (!_Data.SelRows[k])
                  continue; // 17.06.2022. Пустые строки не учитываются для распределения столбцов

                _Data._ValidatingArgs.InitSource(k, i);
                cols2[j].PerformValidating(_Data._ValidatingArgs);
                if (_Data._ValidatingArgs.ValidateState != UIValidateState.Ok)
                {
                  isOK = false;
                  break;
                }
              }

              if (isOK)
              {
                // Столбец подходит
                this[i] = cols2[j];
                cols2.RemoveAt(j);
                break;
              }
            }
          }
        }
        finally
        {
          _Data.EndUpdate();
        }
      }

      /// <summary>
      /// Возвращает массив индексов столбцов в <see cref="SourceData"/>, которым назначено описание с заданным кодом.
      /// Обычно возвращается массив из одного элемента. 
      /// Если описание не назначено, возвращается пустой элемент.
      /// В случае ошибки может быть возвращен массив из нескольких индексов.
      /// 
      /// Если <paramref name="code"/> - пустая строка или null, возвращаются индексы всех столбцов, у которых
      /// описания не назначены.
      /// 
      /// Возвращаемый массив отсортирован по возрастанию индексов.
      /// </summary>
      /// <param name="code">Код описания столбца</param>
      /// <returns>Массив индексов столбцов</returns>
      public int[] GetIndexes(string code)
      {
        List<int> lst = null;

        if (String.IsNullOrEmpty(code))
        {
          for (int i = 0; i < _Items.Length; i++)
          {
            if (_Items[i] == null)
            {
              if (lst == null)
                lst = new List<int>();
              lst.Add(i);
            }
          }
          if (lst == null)
            return DataTools.EmptyIds;
          else
            return lst.ToArray();
        }
        else
        {
          PreparePositions();
          int pos;
          if (_CodePositions.TryGetValue(code, out pos))
          {
            if (_Repeats == null)
              // обычно так и бывает, можно обойтись без списка
              return new int[1] { pos };

            if (!_Repeats.ContainsKey(pos))
              // повторы относятся к другим описаниям
              return new int[1] { pos };

            // Есть повторы для нашего кода
            lst = new List<int>();
            foreach (KeyValuePair<int, object> pair in _Repeats) // это все равно быстрее, чем перебирать все столбцы матрицы
            {
              UISelRCColumn colDef = _Items[pair.Key];
              if (colDef.Code == code)
                lst.Add(pair.Key);
            }

#if DEBUG
            if (lst.Count < 2)
              throw new BugException("Найдено меньше двух индексов в списке повторов");
#endif
            lst.Sort(); // при переборе коллекции _Repeats порядок не гарантирован

            return lst.ToArray();
          }
          else
            return DataTools.EmptyInts;
        }
      }

      #endregion

      #region Наличие повторов

      /// <summary>
      /// Свойство возвращает true, если есть повторно назначенные описания столбцов
      /// </summary>
      public bool HasRepeats
      {
        get
        {
          PreparePositions();
          return _Repeats != null;
        }
      }

      /// <summary>
      /// Возвращает true, если для столбца с указанным индексом назначено описание, которое используется и для другого столбца.
      /// </summary>
      /// <param name="index">Индекс столбца в матрице SourceData</param>
      /// <returns>true, если есть повтор</returns>
      public bool IsRepeated(int index)
      {
        PreparePositions();
        if (_Repeats == null)
          return false;
        return _Repeats.ContainsKey(index);
      }

      /// <summary>
      /// Возвращает массив кодов описаний столбцов, для которых ошибочно выполнено больше одного назначения.
      /// Если ошибок нет, возвращает пустой массив.
      /// </summary>
      public string[] RepeatedCodes
      {
        get
        {
          PreparePositions();
          if (_Repeats == null)
            return DataTools.EmptyStrings;

          SingleScopeStringList lst = new SingleScopeStringList(false);
          foreach (KeyValuePair<int, object> pair in _Repeats)
          {
            UISelRCColumn col = _Items[pair.Key];
            lst.Add(col.Code);
          }

          lst.Sort(ColumnCodeComparer); // по порядку вхождения кодов
          return lst.ToArray();
        }
      }

      private int ColumnCodeComparer(string code1, string code2)
      {
        int p1 = _CodePositions[code1];
        int p2 = _CodePositions[code2];
        return p1 - p2;
      }

      /// <summary>
      /// Возвращает индекc первого повторяющегося столбца, если есть повторы.
      /// Если ошибок нет, возвращается (-1).
      /// </summary>
      public int FirstRepeatedColumnIndex
      {
        get
        {
          PreparePositions();
          if (_Repeats == null)
            return -1;

          int res = int.MaxValue;
          foreach (KeyValuePair<int, object> pair in _Repeats)
          {
            res = Math.Min(res, pair.Key);
          }
          return res;
        }
      }

      #endregion
    }

    /// <summary>
    /// Выбранные описания столбцов из списка <see cref="AvailableColumns"/>.
    /// Длина массива равна числу столбцов в <see cref="SourceData"/> (свойство <see cref="ColumnCount"/>).
    /// Для столбцов, у которых описание не назначено, содержится значение null.
    /// </summary>
    public SelColumnCollection SelColumns { get { return _SelColumns; } }
    private readonly SelColumnCollection _SelColumns;

    #endregion

    #region Событие Changed

    /// <summary>
    /// Событие вызывается при изменениях в выбранных строках или столбцах (свойства <see cref="SelRows"/> и <see cref="SelColumns"/>).
    /// </summary>
    public event EventHandler Changed;

    private void CallChanged()
    {
      if (_UpdateCount == 0)
      {
        if (Changed != null)
          Changed(this, EventArgs.Empty);
      }
      else
        _DelayedChanged = true;
    }

    private bool _DelayedChanged;

    private int _UpdateCount;

    /// <summary>
    /// Приостанавливает отправку события <see cref="Changed"/>.
    /// </summary>
    public void BeginUpdate()
    {
      if (_UpdateCount == 0)
        _DelayedChanged = false;
      _UpdateCount++;
    }

    /// <summary>
    /// Восстанавливает отправку события <see cref="Changed"/>.
    /// </summary>
    public void EndUpdate()
    {
      if (_UpdateCount <= 0)
        throw new InvalidOperationException();
      _UpdateCount--;
      if (_UpdateCount == 0)
      {
        if (_DelayedChanged)
        {
          _DelayedChanged = false;
          CallChanged();
        }
      }
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Единственный экземпляр аргументов события
    /// </summary>
    private UISelRCValidatingEventArgs _ValidatingArgs;

    /// <summary>
    /// Выполнить проверку значения.
    /// Если указанная строка не отмечена (SelRows[<paramref name="rowIndex"/>]=false) или столбцу не назначено описание
    /// (SelColumns[<paramref name="columnIndex"/>]=null), событие <see cref="UISelRCColumn.Validating "/> не вызывается и возвращается Ok.
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnIndex">Индекс столбца в массиве <see cref="SourceData"/></param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке или предупреждение</param>
    /// <returns>Результат проверки</returns>
    public UIValidateState Validate(int rowIndex, int columnIndex, out string errorText)
    {
      if (SelColumns[columnIndex] == null ||
        (!SelRows[rowIndex]))
      {
        errorText = null;
        return UIValidateState.Ok;
      }

      try
      {
        _ValidatingArgs.InitSource(rowIndex, columnIndex);
        SelColumns[columnIndex].PerformValidating(_ValidatingArgs);
        errorText = _ValidatingArgs.ErrorText;
        return _ValidatingArgs.ValidateState;
      }
      catch (Exception e)
      {
        errorText = "Ошибка проверки значения. " + e.Message;
        return UIValidateState.Error;
      }
    }

    #endregion

    #region Результирующие значения

    #region Получение нетипизированного значения

    /// <summary>
    /// Получить результирующее значение ячейки после преобразования.
    /// Если указанная строка или столбец не выбраны, событие <see cref="UISelRCColumn.Validating"/> не вызывается, а возвращается значение null.
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnIndex">Индекс столбца в массиве <see cref="SelColumns"/></param>
    /// <returns>Преобразованное значение</returns>
    public object this[int rowIndex, int columnIndex]
    {
      get
      {
        if (_SelColumns[columnIndex] == null ||
          (!SelRows[rowIndex]))

          return null;

        try
        {
          _ValidatingArgs.InitSource(rowIndex, columnIndex);
          SelColumns[columnIndex].PerformValidating(_ValidatingArgs);
          return _ValidatingArgs.ResultValue;
        }
        catch
        {
          return null;
        }
      }
    }

    /// <summary>
    /// Получить результирующее значение ячейки для столбца с назначенным описанием с заданным кодом.
    /// Если описание с заданным кодом не назначено, возвращается null.
    /// Если для нескольких столбцов матрицы ошибочно выбран один и тот же столбец, 
    /// возвращается значение для первого по счету столбца.
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnCode">Код описания столбца в массиве <see cref="AvailableColumns"/></param>
    /// <returns>Значение ячейки</returns>
    public object this[int rowIndex, string columnCode]
    {
      get
      {
        int columnIndex = SelColumns.IndexOf(columnCode);
        if (columnIndex < 0)
          return null;
        else
          return this[rowIndex, columnIndex];
      }
    }

    #endregion

    #region Получение типизированных значений

    /// <summary>
    /// Получить результирующее строковое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается пустая строка
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnCode">Код описания столбца в массиве <see cref="AvailableColumns"/></param>
    /// <returns>Типизированное значение ячейки</returns>
    public string GetString(int rowIndex, string columnCode)
    {
      return DataTools.GetString(this[rowIndex, columnCode]);
    }

    /// <summary>
    /// Получить результирующее числовое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается 0
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnCode">Код описания столбца в массиве <see cref="AvailableColumns"/></param>
    /// <returns>Типизированное значение ячейки</returns>
    public int GetInt(int rowIndex, string columnCode)
    {
      return DataTools.GetInt(this[rowIndex, columnCode]);
    }

    /// <summary>
    /// Получить результирующее числовое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается 0
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnCode">Код описания столбца в массиве <see cref="AvailableColumns"/></param>
    /// <returns>Типизированное значение ячейки</returns>
    public float GetSingle(int rowIndex, string columnCode)
    {
      return DataTools.GetSingle(this[rowIndex, columnCode]);
    }

    /// <summary>
    /// Получить результирующее числовое значение ячейки для столбца с заданным кодом.
    /// Если столбец с заданным кодом не выбран, возвращается 0.
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnCode">Код описания столбца в массиве <see cref="AvailableColumns"/></param>
    /// <returns>Типизированное значение ячейки</returns>
    public double GetDouble(int rowIndex, string columnCode)
    {
      return DataTools.GetDouble(this[rowIndex, columnCode]);
    }

    /// <summary>
    /// Получить результирующее числовое значение ячейки для столбца с заданным кодом.
    /// Если столбец с заданным кодом не выбран, возвращается 0.
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnCode">Код описания столбца в массиве <see cref="AvailableColumns"/></param>
    public decimal GetDecimal(int rowIndex, string columnCode)
    {
      return DataTools.GetDecimal(this[rowIndex, columnCode]);
    }

    /// <summary>
    /// Получить результирующее значение дата/время ячейки для столбца с заданным кодом.
    /// Если столбец с заданным кодом не выбран, возвращается null.
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnCode">Код описания столбца в массиве <see cref="AvailableColumns"/></param>
    /// <returns>Типизированное значение ячейки</returns>
    public DateTime? GetNullableDateTime(int rowIndex, string columnCode)
    {
      return DataTools.GetNullableDateTime(this[rowIndex, columnCode]);
    }

    /// <summary>
    /// Получить результирующее логическое значение ячейки для столбца с заданным кодом.
    /// Если столбец с заданным кодом не выбран, возвращается false.
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве <see cref="SourceData"/></param>
    /// <param name="columnCode">Код описания столбца в массиве <see cref="AvailableColumns"/></param>
    /// <returns>Типизированное значение ячейки</returns>
    public bool GetBool(int rowIndex, string columnCode)
    {
      return DataTools.GetBool(this[rowIndex, columnCode]);
    }

    #endregion

    #endregion
  }
}
