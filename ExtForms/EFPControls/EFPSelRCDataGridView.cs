// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  #region Делегат EFPSelRCValidatingEventHandler

  /// <summary>
  /// Аргументы события EFPSelRCColumn.Validating
  /// </summary>
  public class EFPSelRCValidatingEventArgs : EventArgs, IUIValidableObject
  {
    #region Инициализация

    /// <summary>
    /// Инициализация.
    /// Так как событие Validating обычно вызывается много раз подряд, не выгоднго создавать объект
    /// EFPSelRCValidatingEventArgs каждый раз. Вместо этого объект создается однократно, а затем вызывается
    /// метод InitSourceData() для каждого значения, после чего вызывается событие
    /// </summary>
    /// <param name="sourceData"></param>
    public void InitSourceData(string sourceData)
    {
      _SourceData = sourceData;
      _ResultValue = sourceData;
      _ValidateState = UIValidateState.Ok;
      _ErrorText = null;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Исходные данные из буфера обмена (строка), которые нужно проверить и преобразовать в значение.
    /// Может быть пустая строка
    /// </summary>
    public string SourceData { get { return _SourceData; } }
    private string _SourceData;

    /// <summary>
    /// Результат преобразования. По умолчанию, значение совпадает со строкой SourceData
    /// Обработчик может установить собственное значение, не обязательно строковое
    /// </summary>
    public object ResultValue { get { return _ResultValue; } set { _ResultValue = value; } }
    private object _ResultValue;

    #endregion

    #region Результаты проверки

    /// <summary>
    /// Результат вызова события Validating
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidateState; } }
    private UIValidateState _ValidateState;

    /// <summary>
    /// Сообщение об ошибке или предупреждении
    /// </summary>
    public string ErrorText { get { return _ErrorText; } }
    private string _ErrorText;

    /// <summary>
    /// Установить признак ошибочного значения
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
    /// Задать предупреждение
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
    /// Вспомогательный метод, вызывающий SetError() или SetWarning()
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
  /// Делегат события EFPSelRCColumn.Validating
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void EFPSelRCValidatingEventHandler(object sender, EFPSelRCValidatingEventArgs args);

  #endregion

  /// <summary>
  /// Описание для одного столбца при вставке текста из буфера обмена
  /// </summary>
  public class EFPSelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public EFPSelRCColumn(string code)
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
    public EFPSelRCColumn(string code, string displayName)
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
    public EFPSelRCColumn(string code, string displayName, EFPSelRCValidatingEventHandler validateHandler)
      : this(code, displayName)
    {
      if (validateHandler != null)
        Validating += validateHandler;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Код столбца
    /// Обязательное свойство, устанавливается в конструкторе
    /// </summary>
    public string Code { get { return _Code; } }
    private string _Code;

    /// <summary>
    /// Имя, отображаемое в выпадающем списке выбора назначения столбца.
    /// Если свойство не задано в явном виде, возвращает значение свойства Code
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
    /// не устанавливается
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Вспомогательный метод.
    /// Устанавливает CanBeEmpty=false и возвращает текущий объект.
    /// Удобно использовать при инициализации массива описаний
    /// </summary>
    /// <returns>Указатель this</returns>
    public EFPSelRCColumn SetRequired()
    {
      CanBeEmpty = false;
      return this;
    }

    /// <summary>
    /// Событие вызывается для проверки корректности строкового значения и, возможно, для преобразования значения
    /// Если обработчик события не установлен, столбец допускает любые строковые значения, включая пустую строку ""
    /// Обработчик вызывается в том числе и для пустых строк, если свойство CanBeEmpty=true.
    /// </summary>
    public event EFPSelRCValidatingEventHandler Validating;

    /// <summary>
    /// Проверка корректности данных.
    /// Непереопределенный метод, после проверки наличия данных вызывает обработчик события Validating.
    /// </summary>
    /// <param name="args"></param>
    public virtual void PerformValidating(EFPSelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceData) && (!CanBeEmpty))
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
  /// Столбец для вставки значения типа "Дата"
  /// Значение ResultValue имеет тип Nullable DateTime 
  /// </summary>
  public class EFPSelRCDateColumn : EFPSelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public EFPSelRCDateColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public EFPSelRCDateColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод ValidateValue()
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение ValidateState, и не выполнять проверку, если есть ошибок</param>
    public EFPSelRCDateColumn(string code, string displayName, EFPSelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в дату.
    /// В случае успеха вызывается метод ValidateValue(), а затем - метод базового класса для вызова пользовательского обработчика события Validating
    /// </summary>
    /// <param name="args"></param>
    public override void PerformValidating(EFPSelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceData))
        args.ResultValue = null;
      else
      {
        DateTime value;
        string s = args.SourceData;
        //if (!DataConv.TryDateFromStr10(s, out dt))
        //{
        if (!DateTime.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceData + "\" нельзя преобразовать в дату");
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
    /// Вызывается из PerformValidating() после успешного преобразования строки в дату.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(EFPSelRCValidatingEventArgs args, DateTime value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки числового значения
  /// Значение ResultValue имеет тип Int32 
  /// </summary>
  public class EFPSelRCIntColumn : EFPSelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public EFPSelRCIntColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public EFPSelRCIntColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод ValidateValue()
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение ValidateState, и не выполнять проверку, если есть ошибок</param>
    public EFPSelRCIntColumn(string code, string displayName, EFPSelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в число.
    /// В случае успеха вызывается метод ValidateValue(), а затем - метод базового класса для вызова пользовательского обработчика события Validating.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    public override void PerformValidating(EFPSelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceData))
        args.ResultValue = 0m;
      else
      {
        int value;
        string s = args.SourceData;
        WinFormsTools.CorrectNumberString(ref s);
        if (!Int32.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceData + "\" нельзя преобразовать в целое число");
          return;
        }
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из PerformValidating() после успешного преобразования строки в число.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(EFPSelRCValidatingEventArgs args, int value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки числового значения
  /// Значение ResultValue имеет тип Single 
  /// </summary>
  public class EFPSelRCSingleColumn : EFPSelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public EFPSelRCSingleColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public EFPSelRCSingleColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод ValidateValue()
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение ValidateState, и не выполнять проверку, если есть ошибок</param>
    public EFPSelRCSingleColumn(string code, string displayName, EFPSelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в число.
    /// В случае успеха вызывается метод ValidateValue(), а затем - метод базового класса для вызова пользовательского обработчика события Validating.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    public override void PerformValidating(EFPSelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceData))
        args.ResultValue = 0m;
      else
      {
        float value;
        string s = args.SourceData;
        WinFormsTools.CorrectNumberString(ref s);
        if (!Single.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceData + "\" нельзя преобразовать в числовое значение");
          return;
        }
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из PerformValidating() после успешного преобразования строки в число.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(EFPSelRCValidatingEventArgs args, float value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки числового значения
  /// Значение ResultValue имеет тип Double
  /// </summary>
  public class EFPSelRCDoubleColumn : EFPSelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public EFPSelRCDoubleColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public EFPSelRCDoubleColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод ValidateValue()
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение ValidateState, и не выполнять проверку, если есть ошибок</param>
    public EFPSelRCDoubleColumn(string code, string displayName, EFPSelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в число.
    /// В случае успеха вызывается метод ValidateValue(), а затем - метод базового класса для вызова пользовательского обработчика события Validating.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    public override void PerformValidating(EFPSelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceData))
        args.ResultValue = 0m;
      else
      {
        double value;
        string s = args.SourceData;
        WinFormsTools.CorrectNumberString(ref s);
        if (!Double.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceData + "\" нельзя преобразовать в числовое значение");
          return;
        }
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из PerformValidating() после успешного преобразования строки в число.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(EFPSelRCValidatingEventArgs args, double value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки числового значения
  /// Значение ResultValue имеет тип decimal 
  /// </summary>
  public class EFPSelRCDecimalColumn : EFPSelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным кодом
    /// </summary>
    /// <param name="code">Код столбца</param>
    public EFPSelRCDecimalColumn(string code)
      : base(code)
    {
    }

    /// <summary>
    /// Создает столбец с заданным кодом и отображаемым именем
    /// </summary>
    /// <param name="code">Код столбца</param>
    /// <param name="displayName">Название, отображаемое при выборе столбца из выпадающего списка</param>
    public EFPSelRCDecimalColumn(string code, string displayName)
      : base(code, displayName)
    {
    }

    /// <summary>
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод ValidateValue()
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение ValidateState, и не выполнять проверку, если есть ошибок</param>
    public EFPSelRCDecimalColumn(string code, string displayName, EFPSelRCValidatingEventHandler validateHandler)
      : base(code, displayName, validateHandler)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в число.
    /// В случае успеха вызывается метод ValidateValue(), а затем - метод базового класса для вызова пользовательского обработчика события Validating.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    public override void PerformValidating(EFPSelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceData))
        args.ResultValue = 0m;
      else
      {
        decimal value;
        string s = args.SourceData;
        WinFormsTools.CorrectNumberString(ref s);
        if (!decimal.TryParse(s, out value))
        {
          args.SetError("Строку \"" + args.SourceData + "\" нельзя преобразовать в числовое значение");
          return;
        }
        args.ResultValue = value;
        ValidateValue(args, value);
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из PerformValidating() после успешного преобразования строки в число.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(EFPSelRCValidatingEventArgs args, decimal value)
    {
    }

    #endregion
  }

  /// <summary>
  /// Столбец для вставки значения перечислимого типа <typeparamref name="T"/>
  /// Значение ResultValue имеет перечислимый тип.
  /// Использует список строковых значений, соответствующих элементам перечисления.
  /// </summary>
  /// <typeparam name="T">Тип перечисления. Поддерживаются только простые перечисления,
  /// имеющие значения 0,1,2, ...</typeparam>
  public class EFPSelRCEnumColumn<T> : EFPSelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным списком значений
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="textValues">Список текстовых значений</param>
    public EFPSelRCEnumColumn(string code, string displayName, string[] textValues)
      : this(code, displayName, textValues, null)
    {
    }

    /// <summary>
    /// Создает столбец с заданным списком значений.
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Альтернативно, можно реализовать производный класс и переопределить метод ValidateValue()
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="textValues">Список текстовых значений</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение ValidateState, и не выполнять проверку, если есть ошибок</param>
    public EFPSelRCEnumColumn(string code, string displayName, string[] textValues, EFPSelRCValidatingEventHandler validateHandler)
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
    private string[] _TextValues;

    private StringArrayIndexer _TextValueIndexer;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет преобразование строки в перечисление.
    /// В случае успеха вызывается метод ValidateValue(), а затем - метод базового класса для вызова пользовательского обработчика события Validating.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    public override void PerformValidating(EFPSelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceData))
        args.ResultValue = null;
      else
      {
        int p = _TextValueIndexer.IndexOf(args.SourceData);
        if (p < 0)
        {
          args.SetError("Неизвестная строка \"" + args.SourceData + "\"");
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
    /// Вызывается из PerformValidating() после успешного преобразования строки в перечислимое значение.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(EFPSelRCValidatingEventArgs args, T value)
    {
    }

    #endregion
  }


  /// <summary>
  /// Столбец для вставки значения перечислимого типа <typeparamref name="T"/>
  /// Значение ResultValue имеет перечислимый тип.
  /// Использует словарь строковых значений, соответствующих элементам перечисления.
  /// В словаре несколько строк могут соответствовать одному значению.
  /// </summary>
  /// <typeparam name="T">Тип перечисления</typeparam>
  public class EFPSelRCEnumColumnWithDict<T> : EFPSelRCColumn
  {
    #region Конструкторы

    /// <summary>
    /// Создает столбец с заданным словарем значений.
    /// Учет регистра символов определяется коллекцией TextValues.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="textValues">Список текстовых значений и соответствующих им элементов перечисления</param>
    public EFPSelRCEnumColumnWithDict(string code, string displayName, TypedStringDictionary<T> textValues)
      : this(code, displayName, textValues, null)
    {
    }

    /// <summary>
    /// Создает столбец с заданным словарем значений.
    /// Эта версия конструктора позволяет установить дополнительную проверку значения.
    /// Учет регистра символов определяется коллекцией TextValues.
    /// </summary>
    /// <param name="code">Код</param>
    /// <param name="displayName">Отображаемое наименование</param>
    /// <param name="textValues">Список текстовых значений и соответствующих им элементов перечисления</param>
    /// <param name="validateHandler">Дополнительный обработчик проверки. 
    /// Обработчик должен сначала проверить текущее значение ValidateState, и не выполнять проверку, если есть ошибок</param>
    public EFPSelRCEnumColumnWithDict(string code, string displayName, TypedStringDictionary<T> textValues, EFPSelRCValidatingEventHandler validateHandler)
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
    /// В случае успеха вызывается метод ValidateValue(), а затем - метод базового класса для вызова пользовательского обработчика события Validating.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    public override void PerformValidating(EFPSelRCValidatingEventArgs args)
    {
      if (String.IsNullOrEmpty(args.SourceData))
        args.ResultValue = null;
      else
      {
        T value;
        if (TextValues.TryGetValue(args.SourceData, out value))
        {
          args.ResultValue = value;
          ValidateValue(args, value);
        }
        else
        {
          args.SetError("Неизвестная строка \"" + args.SourceData + "\"");
          args.ResultValue = null;
        }
      }
      base.PerformValidating(args);
    }

    /// <summary>
    /// Проверка преобразованного значения.
    /// Вызывается из PerformValidating() после успешного преобразования строки в перечислимое значение.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    /// <param name="args">Аргументы события Validating</param>
    /// <param name="value">Проверяемое значение после преобразования из строки</param>
    protected virtual void ValidateValue(EFPSelRCValidatingEventArgs args, T value)
    {
    }

    #endregion
  }

  #endregion


  /// <summary>
  /// Табличный просмотр для выбора строк и назначения столбцов
  /// Используется в диалогах или мастерах вставки текстовых значений из буфера обмена
  /// </summary>
  public class EFPSelRCDataGridView : EFPDataGridView
  {
    #region Константы

    private const string NoneColText = "[ Нет ]";

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает провайдер табличного просмотра.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Табличный просмотр</param>
    public EFPSelRCDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер табличного просмотра.
    /// </summary>
    /// <param name="controlWithToolBar">Табличный просмотр и панель инструментов</param>
    public EFPSelRCDataGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      _ValidatingArgs = new EFPSelRCValidatingEventArgs();

      base.UseRowImages = true;
      Control.VirtualMode = true;
      if (!DesignMode)
      {
        Control.CellValueNeeded += new DataGridViewCellValueEventHandler(Control_CellValueNeeded);
        Control.CellValuePushed += new DataGridViewCellValueEventHandler(Control_CellValuePushed);
      }

      base.ReadOnly = true; // 11.01.2017
      base.CanView = false; // 11.01.2017

      base.UseRowImages = true; // 12.01.2022

      // 12.01.2022
      Columns.AddBool("RowFlag", false, String.Empty); // будет удален при установке SourceData
      base.MarkRowsColumnIndex = 0;
    }

    private EFPSelRCValidatingEventArgs _ValidatingArgs;

    #endregion

    #region SourceData

    /// <summary>
    /// Двумерный массив строк исходных данных.
    /// Свойство должно быть установлено до вывода управляющего элемента на экран
    /// </summary>
    public string[,] SourceData
    {
      get { return _SourceData; }
      set
      {
        _SourceData = value;
        InitSourceData();
      }
    }
    private string[,] _SourceData;

    #endregion

    #region AllColumns

    /// <summary>
    /// Полный список описаний возможных столбцов, из которых можно осуществлять выбор
    /// </summary>
    public EFPSelRCColumn[] AllColumns
    {
      get { return _AllColumns; }
      set
      {
        _AllColumns = value;
        InitSourceData();
      }
    }
    private EFPSelRCColumn[] _AllColumns;

    private void InitSourceData()
    {
      if (_AllColumns == null || _SourceData == null)
      {
        _SelColumns = null;
        _SelRows = null;
        return;
      }
      _SelColumns = new EFPSelRCColumn[SourceData.GetLength(1)];
      _SelRows = new bool[SourceData.GetLength(0)];

      // 12.12.2017
      // Ставим флажки только для непустых строк
      // DataTools.FillArray<bool>(FSelRows, true);
      for (int i = 0; i < _SelRows.Length; i++)
      {
        for (int j = 0; j < _SelColumns.Length; j++)
        {
          if (!String.IsNullOrEmpty(_SourceData[i, j]))
          {
            _SelRows[i] = true;
            break;
          }
        }
      }

      Columns.Clear();
      Columns.AddBool("RowFlag", false, String.Empty);
      Columns.LastAdded.GridColumn.ToolTipText = "Использование строки";
      Columns.LastAdded.GridColumn.DividerWidth = 1; // 12.01.2022

      string[] displayNames = new string[AllColumns.Length + 1];
      displayNames[0] = NoneColText;
      for (int i = 0; i < AllColumns.Length; i++)
        displayNames[i + 1] = AllColumns[i].DisplayName;
      int dropDownWidth = CalcDropDownWidth(displayNames);

      for (int i = 0; i < SelColumns.Length; i++)
      {
        DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
        col.Name = "Col" + (i + 1).ToString(); // 12.01.2022
        col.HeaderText = (i + 1).ToString();
        //Col.DropDownWidth = Math.Max(DropDownWidth, Col.Width);
        col.DropDownWidth = dropDownWidth; // расширение до столбца выполняется автоматически
        col.Items.AddRange(displayNames);
        Control.Columns.Add(col);

        int maxLen = 7; // нужно место для стрелочки выпадающего списка
        for (int j = 0; j < SelRows.Length; j++)
        {
          string s = DataTools.GetString(SourceData[j, i]);
          maxLen = Math.Max(s.Length, maxLen);
        }
        maxLen = Math.Min(maxLen, 30);

        Columns[col].TextWidth = maxLen;
      }

      Control.RowCount = SelRows.Length + 1;

      base.FrozenColumns = 1;
      Control.Rows[0].Frozen = true;
      Control.Rows[0].DividerHeight = 1; // 12.01.2022
      base.MarkRowsColumnIndex = 0;

      for (int i = 0; i < SelRows.Length; i++)
      {
        Control.Rows[i + 1].HeaderCell.Value = i + 1;
        for (int j = 0; j < SelColumns.Length; j++)
        {
          DataGridViewComboBoxCell Cell = (DataGridViewComboBoxCell)(base.Control[j + 1, i + 1]);

          Cell.Items.Add(SourceData[i, j]);
          Cell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
          Cell.Value = SourceData[i, j];
        }
      }

      InitDefaultColumns();
    }

    /// <summary>
    /// Начальная инициализация столбцов подходящими значениями
    /// </summary>
    private void InitDefaultColumns()
    {
      List<EFPSelRCColumn> cols2 = new List<EFPSelRCColumn>(AllColumns);

      EFPSelRCValidatingEventArgs args = new EFPSelRCValidatingEventArgs();

      for (int i = 0; i < SelColumns.Length; i++)
      {
        for (int j = 0; j < cols2.Count; j++)
        {
          bool IsOK = true;
          for (int k = 0; k < SelRows.Length; k++)
          {
            args.InitSourceData(SourceData[k, i]);
            cols2[j].PerformValidating(args);
            if (args.ValidateState != UIValidateState.Ok)
            {
              IsOK = false;
              break;
            }
          }

          if (IsOK)
          {
            // Столбец подходит
            SelColumns[i] = cols2[j];
            cols2.RemoveAt(j);
            break;
          }
        }
      }
    }

    #endregion

    #region SelColumns и SelRows

    /// <summary>
    /// Выбранные столбцы из списка AllColumns.
    /// Длина массива соответствует числу столбцов в SourceData.
    /// Для ненужных столбцов содержится значение null
    /// Свойство доступно только после установки строк AllColumns и SourceData
    /// </summary>
    public EFPSelRCColumn[] SelColumns { get { return _SelColumns; } }
    private EFPSelRCColumn[] _SelColumns;

    /// <summary>
    /// Флажки для выбранных строк.
    /// Длина массива соответствует числу строк в массиве SourceData
    /// Свойство доступно только после установки строк AllColumns и SourceData
    /// </summary>
    public bool[] SelRows { get { return _SelRows; } }
    private bool[] _SelRows;

    /// <summary>
    /// Возвращает индекс выбранного столбца с заданным кодом
    /// Если данный столбец не выбран, возвращается (-1)
    /// </summary>
    /// <param name="code">Код столбца из массива AllColumns</param>
    /// <returns>Индекс выбранного столбца в SelColumns</returns>
    public int IndexOfSelColumn(string code)
    {
      for (int i = 0; i < SelColumns.Length; i++)
      {
        if (SelColumns[i] != null)
        {
          if (SelColumns[i].Code == code)
            return i;
        }
      }

      return -1;
    }

    /// <summary>
    /// Возвращает true, если заданный столбец выбран
    /// </summary>
    /// <param name="code">Код столбца из массива AllColumns</param>
    /// <returns>Наличие выбранного столбца</returns>
    public bool ContainsSelColumn(string code)
    {
      return IndexOfSelColumn(code) >= 0;
    }

    /// <summary>
    /// Возвращает true, если хотя бы один из указанных столбцов выбран
    /// </summary>
    /// <param name="codes">Список кодов столбцов из массива AllColumns, разделенных запятыми</param>
    /// <returns>Наличие выбранных столбцов</returns>
    public bool ContainsAnySelColumn(string codes)
    {
      if (String.IsNullOrEmpty(codes))
        return false;
      string[] a = codes.Split(',');
      for (int i = 0; i < a.Length; i++)
      {
        if (ContainsSelColumn(a[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если все указанные столбцы выбраны
    /// </summary>
    /// <param name="codes">Список кодов столбцов из массива AllColumns, разделенных запятыми</param>
    /// <returns>Наличие выбранных столбцов</returns>
    public bool ContainsAllSelColumns(string codes)
    {
      if (String.IsNullOrEmpty(codes))
        return true;
      string[] a = codes.Split(',');
      for (int i = 0; i < a.Length; i++)
      {
        if (!ContainsSelColumn(a[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает количество строк, отмеченных галочками
    /// </summary>
    public int SelRowCount
    {
      get
      {
        if (_SelRows == null)
          return 0;
        int cnt = 0;
        for (int i = 0; i < _SelRows.Length; i++)
        {
          if (_SelRows[i])
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Программная установка выбранного столбца
    /// </summary>
    /// <param name="columnIndex">Индекс столбца в массве SelColumns</param>
    /// <param name="value">Выбранный столбец для привязки. Может быть null</param>
    public void SetSelColumn(int columnIndex, EFPSelRCColumn value)
    {
      if (_SelColumns == null)
        throw new NullReferenceException("Свойство SelColumns не было инициализировано");
      if (value == _SelColumns[columnIndex])
        return;

      _SelColumns[columnIndex] = value;
      Control.InvalidateColumn(columnIndex + 1);
    }

    #endregion

    #region Обработчики таблицы

    private int CalcDropDownWidth(string[] displayNames)
    {
      int maxW = 0;
      Graphics gr = Control.CreateGraphics();
      try
      {
        for (int i = 0; i < displayNames.Length; i++)
        {
          int w = (int)(gr.MeasureString(displayNames[i], Control.Font).Width);
          maxW = Math.Max(maxW, w);
        }
      }
      finally
      {
        gr.Dispose();
      }
      maxW += SystemInformation.VerticalScrollBarWidth;
      maxW += 4; // для рамочек
      Screen scr = Screen.FromControl(Control);
      maxW = Math.Min(maxW, scr.WorkingArea.Width);
      return maxW;
    }

    void Control_CellValueNeeded(object sender, DataGridViewCellValueEventArgs args)
    {
      try
      {
        if (args.ColumnIndex == 0)
        {
          if (args.RowIndex > 0)
            args.Value = SelRows[args.RowIndex - 1];
        }
        else
        {
          //DataGridViewComboBoxCell Cell = (DataGridViewComboBoxCell)(Control[args.ColumnIndex, args.RowIndex]);
          if (args.RowIndex > 0)
            args.Value = SourceData[args.RowIndex - 1, args.ColumnIndex - 1];
          else
          {
            EFPSelRCColumn Col = SelColumns[args.ColumnIndex - 1];
            if (Col == null)
              args.Value = NoneColText;
            else
              args.Value = Col.DisplayName;
          }
        }
      }
      catch
      {
      }
    }

    void Control_CellValuePushed(object sender, DataGridViewCellValueEventArgs args)
    {
      try
      {
        if (args.ColumnIndex == 0)
        {
          if (args.RowIndex == 0)
            return;
          SelRows[args.RowIndex - 1] = DataTools.GetBool(args.Value);
          Control.InvalidateRow(args.RowIndex);
          base.Validate();
        }

        if (args.ColumnIndex > 0 && args.RowIndex == 0)
        {
          string displayName = DataTools.GetString(args.Value);
          if (displayName == NoneColText)
            SelColumns[args.ColumnIndex - 1] = null;
          else
          {
            bool found = false;
            for (int i = 0; i < AllColumns.Length; i++)
            {
              if (AllColumns[i].DisplayName == displayName)
              {
                SelColumns[args.ColumnIndex - 1] = AllColumns[i];
                found = true;
                break;
              }
            }
            if (!found)
              throw new InvalidOperationException("Не найден столбец с названием \"" + displayName + "\"");
          }

          Control.InvalidateColumn(args.ColumnIndex);
          base.Validate();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "CellValuePushed");
      }
    }

    /// <summary>
    /// Получение списка ошибок и предупреждений для строки, если есть ячейки с неправильным форматированием
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnGetRowAttributes(EFPDataGridViewRowAttributesEventArgs args)
    {
      if (args.RowIndex >= 1 && SelRows[args.RowIndex - 1]) // серые ячейки делаем поштучно
      {
        for (int i = 0; i < SelColumns.Length; i++)
        {
          if (SelColumns[i] != null)
          {
            _ValidatingArgs.InitSourceData(SourceData[args.RowIndex - 1, i]);
            SelColumns[i].PerformValidating(_ValidatingArgs);
            switch (_ValidatingArgs.ValidateState)
            {
              case UIValidateState.Error:
                args.AddRowError(_ValidatingArgs.ErrorText, Columns[i + 1].Name);
                break;
              case UIValidateState.Warning:
                args.AddRowWarning(_ValidatingArgs.ErrorText, Columns[i + 1].Name);
                break;
            }
          }
        }
      }
      base.OnGetRowAttributes(args);
    }

    /// <summary>
    /// Раскраска серых ячеек
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnGetCellAttributes(EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.RowIndex == 0)
      {
        // Строка выбора типа столбца
        if (args.ColumnIndex == 0)
        {
          // Пустая ячейка
          args.ContentVisible = false;
          args.ReadOnly = true;
          args.ReadOnlyMessage = "Пустая ячейка";
          return;
        }
      }
      else
      {
        // Строка таблицы
        if (args.ColumnIndex >= 1)
        {
          // Столбец данных
          if (SelRows[args.RowIndex - 1])
          {
            if (SelColumns[args.ColumnIndex - 1] == null)
              args.Grayed = true;
          }
          else
            args.Grayed = true;
          args.ReadOnly = true;
          args.ReadOnlyMessage = "Нельзя редактировать значения";
        }
        // Ячейка с флажком никогда не делается серой
      }

      base.OnGetCellAttributes(args);
    }

    #endregion

    #region Validate

    /// <summary>
    /// Проверка корректности введенных данных управляющего элемента
    /// Проверка значений импортируемых ячеек не выполняется, т.к. она может быть сравнительно длительной
    /// и вызвать "подвисание" компьютера при большом размере таблицы SourceData
    /// </summary>
    protected override void OnValidate()
    {
      if (SelRows == null)
      {
        base.SetError("Свойства AllColumns и SourceData должны быть установлены");
        return;
      }
      bool hasCol = false;
      for (int i = 0; i < SelColumns.Length; i++)
      {
        if (SelColumns[i] != null)
        {
          hasCol = true;
          for (int j = 0; j < i; j++)
          {
            if (SelColumns[j] == SelColumns[i])
            {
              Control.CurrentCell = Control[j + 1, 0];
              SetError("Столбец \"" + SelColumns[j].DisplayName + "\" выбран дважды");
              return;
            }
          }
        }
      }
      if (!hasCol)
      {
        base.SetError("Не выбрано ни одного столбца");
        return;
      }

      // Проверяем наличие выбранных строк и корректность данных
      bool hasRows = false;
      for (int i = 0; i < SelRows.Length; i++)
      {
        if (SelRows[i])
        {
          hasRows = true;

          for (int j = 0; j < SelColumns.Length; j++)
          {
            if (SelColumns[j] != null)
            {
              _ValidatingArgs.InitSourceData(SourceData[i, j]);
              SelColumns[j].PerformValidating(_ValidatingArgs);
              if (_ValidatingArgs.ValidateState == UIValidateState.Error)
              {
                // 12.01.2022
                // Переход на ошибочную ячейку не выполняем.
                // Например, пусть выбрана строка с ошибкой и установленным флажком.
                // Пользователь выключает флажок.
                // Фокус прыгает на другую строку с ошибкой.
                // Теперь можно использовать Ctrl+] и Ctrl+[ для поиска ошибок.
                //Control.CurrentCell = Control[j + 1, i + 1];

                base.SetError("Строка "+ (i+1).ToString()+", столбец "+(j+1).ToString()+" ("+SelColumns[j].DisplayName+"). "+ 
                  _ValidatingArgs.ErrorText);
                return;
              }
            }
          }
        }
      }

      if (!hasRows)
      {
        this.CurrentColumnIndex = 0;
        //EFPApp.ShowTempMessage("Нет ни одной выбранной строки данных");
        base.SetError("Нет ни одной выбранной строки данных"); // 12.01.2022
        return;
      }

      base.OnValidate();
    }

    #endregion

    #region Получение значения

    /// <summary>
    /// Получить результирующее значение ячейки после преобразования
    /// Выбрасывает исключение NullReferenceException, если для столбца с индексом <paramref name="columnIndex"/> не выбрано значение
    /// (SelCoumns[ColumnIndex]=null)
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnIndex">Индекс столбца в массиве SelColumns</param>
    /// <returns>Преобразованное значение</returns>
    public object GetResultValue(int rowIndex, int columnIndex)
    {
      if (SelColumns[columnIndex] == null)
        throw new NullReferenceException("Для столбца с индексом " + columnIndex.ToString() + " не выбрано значение в массиве SelColumns");
      _ValidatingArgs.InitSourceData(SourceData[rowIndex, columnIndex]);
      SelColumns[columnIndex].PerformValidating(_ValidatingArgs);
      return _ValidatingArgs.ResultValue;
    }

    /// <summary>
    /// Получить результирующее значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается null
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnCode">Код столбца в массиве AllColumns</param>
    /// <returns>Значение ячйки</returns>
    public object GetResultValue(int rowIndex, string columnCode)
    {
      int columnIndex = IndexOfSelColumn(columnCode);
      if (columnIndex < 0)
        return null;
      else
        return GetResultValue(rowIndex, columnIndex);
    }

    #region Получение типизированных значений

    /// <summary>
    /// Получить результирующее строковое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается пустая строка
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnCode">Код столбца в массиве AllColumns</param>
    /// <returns>Типизированное значение ячйки</returns>
    public string GetString(int rowIndex, string columnCode)
    {
      return DataTools.GetString(GetResultValue(rowIndex, columnCode));
    }

    /// <summary>
    /// Получить результирующее числовое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается 0
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnCode">Код столбца в массиве AllColumns</param>
    /// <returns>Типизированное значение ячйки</returns>
    public int GetInt(int rowIndex, string columnCode)
    {
      return DataTools.GetInt(GetResultValue(rowIndex, columnCode));
    }

    /// <summary>
    /// Получить результирующее числовое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается 0
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnCode">Код столбца в массиве AllColumns</param>
    /// <returns>Типизированное значение ячйки</returns>
    public float GetSingle(int rowIndex, string columnCode)
    {
      return DataTools.GetSingle(GetResultValue(rowIndex, columnCode));
    }

    /// <summary>
    /// Получить результирующее числовое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается 0
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnCode">Код столбца в массиве AllColumns</param>
    /// <returns>Типизированное значение ячйки</returns>
    public double GetDouble(int rowIndex, string columnCode)
    {
      return DataTools.GetDouble(GetResultValue(rowIndex, columnCode));
    }

    /// <summary>
    /// Получить результирующее числовое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается 0
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnCode">Код столбца в массиве AllColumns</param>
    /// <returns>Типизированное значение ячйки</returns>
    public decimal GetDecimal(int rowIndex, string columnCode)
    {
      return DataTools.GetDecimal(GetResultValue(rowIndex, columnCode));
    }

    /// <summary>
    /// Получить результирующее значение дата/время ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается null
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnCode">Код столбца в массиве AllColumns</param>
    /// <returns>Типизированное значение ячйки</returns>
    public DateTime? GetNullableDateTime(int rowIndex, string columnCode)
    {
      return DataTools.GetNullableDateTime(GetResultValue(rowIndex, columnCode));
    }

    /// <summary>
    /// Получить результирующее логическое значение ячейки для столбца с заданным кодом
    /// Если столбец с заданным кодом не выбран, возвращается false
    /// </summary>
    /// <param name="rowIndex">Индекс строки в массиве SourceData</param>
    /// <param name="columnCode">Код столбца в массиве AllColumns</param>
    /// <returns>Типизированное значение ячйки</returns>
    public bool GetBool(int rowIndex, string columnCode)
    {
      return DataTools.GetBool(GetResultValue(rowIndex, columnCode));
    }

    #endregion

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Возвращает список команд локального меню
    /// </summary>
    public new EFPSelRCDataGridViewCommandItems CommandItems
    {
      get { return (EFPSelRCDataGridViewCommandItems)(base.CommandItems); }
    }

    /// <summary>
    /// Создает список команд локального меню
    /// </summary>
    /// <returns>Новый список команд</returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPSelRCDataGridViewCommandItems(this);
    }

    #endregion
  }

  /// <summary>
  /// Добавляет команду "Очистить привязку столбцов" для табличного просмотра выбора строк и столбцов
  /// </summary>
  public class EFPSelRCDataGridViewCommandItems : EFPDataGridViewCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="owner">Провайдер табличного просмотра</param>
    public EFPSelRCDataGridViewCommandItems(EFPSelRCDataGridView owner)
      : base(owner)
    {
      ciClearColumns = new EFPCommandItem("Edit", "CleraColumnRefs");
      ciClearColumns.MenuText = "Очистить привязку столбцов";
      ciClearColumns.ImageKey = "No";
      ciClearColumns.ShortCut = Keys.F8;
      ciClearColumns.GroupBegin = true;
      ciClearColumns.GroupEnd = true;
      ciClearColumns.Click += ciClearColumns_Click;
      base.Add(ciClearColumns);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPSelRCDataGridView ControlProvider { get { return (EFPSelRCDataGridView)(base.ControlProvider); } }

    #endregion

    #region Команда очистки

    EFPCommandItem ciClearColumns;

    void ciClearColumns_Click(object sender, EventArgs args)
    {
      for (int i = 0; i < ControlProvider.SelColumns.Length; i++)
        ControlProvider.SetSelColumn(i, null);
    }

    #endregion
  }

  /// <summary>
  /// Шаг мастера для привязки строк и столбцов
  /// Используется в мастерах вставки в справочники из буфера обмена
  /// </summary>
  public class WizardStepSelRC : WizardStepWithDataGridView
  {
    #region Конструктор

    /// <summary>
    /// Создает шаг мастера
    /// </summary>
    public WizardStepSelRC()
      : base(false)
    {
      DoInit(new EFPSelRCDataGridView(base.BaseProvider, new DataGridView()));
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPSelRCDataGridView TheControlProvider
    {
      get { return (EFPSelRCDataGridView)(base.TheControlProvider); }
    }

    #endregion
  }
}
