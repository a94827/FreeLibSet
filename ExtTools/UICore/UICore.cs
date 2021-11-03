using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Calendar;
using System.Diagnostics;

// Общие объявления пользовательского интерфейса.
// Используется классами FreeLibSet.RI.Control (удаленный пользовательский интерфейс)
// и FreeLibSet.Forms.EFPControlBase (провайдеры Windows Forms)
namespace FreeLibSet.UICore
{
  #region Перечисление UIValidateState

  /// <summary>
  /// Результат проверки ошибок
  /// </summary>
  [Serializable]
  public enum UIValidateState
  {
    // Числовые значения совпадают с UIValidateState в ExtForms.dll

    /// <summary>
    /// Ошибок не найдено
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Предупреждение
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Ошибка
    /// </summary>
    Error = 2
  }

  #endregion

  /// <summary>
  /// Описание для одного объекта валидации, присоединенного к управляющему элементу.
  /// Валидаторы могут добавляться к списку Control.Validators.
  /// 
  /// Класс однократной записи.
  /// </summary>
  [Serializable]
  public sealed class UIValidator
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="expression">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isError">true-ошибка, false-предупреждение</param>
    public UIValidator(DepValue<bool> expression, string message, bool isError)
      : this(expression, message, isError, null)
    {
    }

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    /// <param name="isError">true-ошибка, false-предупреждение</param>
    /// <param name="preconditionEx">Выражение, определяющее необходимость выполнения проверки. Может быть null, если проверка выполняется всегда</param>
    public UIValidator(DepValue<bool> expressionEx, string message, bool isError, DepValue<bool> preconditionEx)
    {
      if (expressionEx == null)
        throw new ArgumentNullException("expression");
      if (String.IsNullOrEmpty(message))
        throw new ArgumentNullException("message");

      _ExpressionEx = expressionEx;
      expressionEx.ValueChanged += DummyValueChanged; // Чтобы IsConnected возвращало true
      _Message = message;
      _IsError = isError;
      _PreconditionEx = preconditionEx;
      if (preconditionEx != null)
        preconditionEx.ValueChanged += DummyValueChanged; // Чтобы IsConnected возвращало true
    }

    static void DummyValueChanged(object sender, EventArgs args)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выражение для выполнения проверки.
    /// Вычисление должно возвращать true, если условие валидации выполнено.
    /// Если вычисленное значение равно false, то для управляющего элемента будет выдано сообщение об ошибке или предупреждение,
    /// в зависимости от свойства IsError.
    /// </summary>
    public DepValue<bool> ExpressionEx { get { return _ExpressionEx; } }
    private readonly DepValue<bool> _ExpressionEx;

    /// <summary>
    /// Сообщение, которое будет выдано, если результатом вычисления Expression является false.
    /// </summary>
    public string Message { get { return _Message; } }
    private readonly string _Message;

    /// <summary>
    /// True, если нарушение условия является ошибкой, false-если предупреждением.
    /// Если хотя бы один из объектов с IsError=true не проходит валидацию, нельзя закрыть блок диалога нажатием кнопки "ОК".
    /// Предупреждения не препятствуют закрытию диалога.
    /// </summary>
    public bool IsError { get { return _IsError; } }
    private readonly bool _IsError;

    /// <summary>
    /// Необязательное предусловие для выполнения проверки. Если выражение возвращает true, то проверка выполняется,
    /// если false - то отключается.
    /// Если свойство не установлено (обычно), то проверка выполняется.
    /// </summary>
    public DepValue<bool> PreconditionEx { get { return _PreconditionEx; } }
    private readonly DepValue<bool> _PreconditionEx;

    /// <summary>
    /// Возвращает свойство Message (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Message;
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства RIItem.Validators
  /// </summary>
  [Serializable]
  public sealed class UIValidatorList : ListWithReadOnly<UIValidator>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public UIValidatorList()
    {
    }

    #endregion

    #region Методы

    /// <summary>
    /// Создает объект Validator с IsError=true и добавляет его в список
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    public UIValidator AddError(DepValue<bool> expressionEx, string message)
    {
      UIValidator item = new UIValidator(expressionEx, message, true);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=true и добавляет его в список
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    /// <param name="preconditionEx">Выражение предусловия. Может быть null, если проверка выполняется всегда</param>
    public UIValidator AddError(DepValue<bool> expressionEx, string message, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(expressionEx, message, true, preconditionEx);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=false и добавляет его в список
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    public UIValidator AddWarning(DepValue<bool> expressionEx, string message)
    {
      UIValidator item = new UIValidator(expressionEx, message, false);
      Add(item);
      return item;
    }

    /// <summary>
    /// Создает объект Validator с IsError=false и добавляет его в список
    /// </summary>
    /// <param name="expressionEx">Выражение валидации</param>
    /// <param name="message">Сообщение</param>
    /// <param name="preconditionEx">Выражение предусловия. Может быть null, если проверка выполняется всегда</param>
    public UIValidator AddWarning(DepValue<bool> expressionEx, string message, DepValue<bool> preconditionEx)
    {
      UIValidator item = new UIValidator(expressionEx, message, false, preconditionEx);
      Add(item);
      return item;
    }

    /// <summary>
    /// Переводит список в режим "Только чтение".
    /// Метод не должен использоваться в прикладном коде
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region Статический список

    /// <summary>
    /// Пустой список валидаторов.
    /// Свойство списка IsReadOnly=true.
    /// </summary>
    public static readonly UIValidatorList Empty = CreateEmpty();

    private static UIValidatorList CreateEmpty()
    {
      UIValidatorList obj = new UIValidatorList();
      obj.SetReadOnly();
      return obj;
    }

    #endregion
  }

  #region Интерфейсы управляющих элементов

#if XXX
  /// <summary>
  /// Интерфейс управляющего элемента
  /// </summary>
  public interface IUIControl
  {
    /// <summary>
    /// Доступность элемента
    /// </summary>
    bool Enabled { get; set;}

    /// <summary>
    /// Управляемое свойство для Enabled
    /// </summary>
    DepValue<bool> EnabledEx { get;set;}

    /// <summary>
    /// Не предназначено для использования в прикладном коде
    /// </summary>
    bool EnabledExConnected { get;}

    /// <summary>
    /// Проверяющие объекты
    /// </summary>
    UIValidatorList Validators { get;}
  }

  /// <summary>
  /// Управляющий элемент, поддерживающий проверку наличия значения
  /// </summary>
  public interface IUIControlCanBeEmpty : IUIControl
  {
    /// <summary>
    /// True, если элемент может содержать пустое значение.
    /// По умолчанию - false.
    /// </summary>
    bool CanBeEmpty { get;set;}

    /// <summary>
    /// Расширение для свойства CanBeEmpty, поддерживающее выдачу предупреждений
    /// </summary>
    UIValidateState CanBeEmptyMode { get;set;}
  }

  /// <summary>
  /// Управляющий элемент, поддерживающий просмотр значений (свойство ReadOnly)
  /// </summary>
  public interface IUIControlWithReadOnly : IUIControl
  {
    /// <summary>
    /// True, если элемент предназначен только для просмотра значений
    /// </summary>
    bool ReadOnly { get;set;}

    /// <summary>
    /// Управляемое свойство для ReadOnly
    /// </summary>
    DepValue<bool> ReadOnlyEx { get;set;}
  }
#endif
  #endregion

  /// <summary>
  /// Статические методы для реализации пользовательского интерфейса
  /// </summary>
  public static class UITools
  {
    #region Text <-> Lines

    /// <summary>
    /// Преобразование свойства Text в Lines для многострочного текста
    /// </summary>
    /// <param name="text">Строка</param>
    /// <returns>Массив строк</returns>
    public static string[] TextToLines(string text)
    {
      if (String.IsNullOrEmpty(text))
        return DataTools.EmptyStrings;
      else
        return text.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
    }

    /// <summary>
    /// Преобразование свойства Lines в Text для многострочного текста
    /// </summary>
    /// <param name="lines">Массив строк</param>
    /// <returns>Строка</returns>
    public static string LinesToText(string[] lines)
    {
      if (lines == null)
        return String.Empty;
      if (lines.Length == 0)
        return String.Empty;
      else
        return String.Join(Environment.NewLine, lines);
    }

    #endregion

    #region Сдвиг интервала дат

    #region ShiftDateRange

    /// <summary>
    /// Сдвиг интервала дат вперед или назад.
    /// Если текущий период представляет собой целое число месяцев, то сдвиг
    /// выполняется на число месяцев, содержащихся в периоде. Иначе сдвиг выполняется на число дней в периоде.
    /// Если сдвиг нельзя выполнить, то ссылочные значения не изменяются и возвращается false.
    /// 
    /// Эта версия позволяет сдвигать полузакрытые интервалы. Если задана только начальная дата, то можно
    /// выполнить "сдвиг назад", с <paramref name="forward"/>=false. Если задана только конечная дата,
    /// то можно выполнить "сдвиг вперед", с <paramref name="forward"/>=true. 
    /// 
    /// Используется в элементах пользовательского интерфейса, предназначенных для работы с интервалами дат.
    /// См. также операторы сдвига вправо/влева для типа DateRange.
    /// </summary>
    /// <param name="firstDate">Начальная дата (по ссылке)</param>
    /// <param name="lastDate">Конечная дата (по ссылке)</param>
    /// <param name="forward">true - для сдвига вперед, false - назад</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRange(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRange(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return Res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRange(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      if (firstDate.HasValue && lastDate.HasValue)
      {
        // Обычный сдвиг
        DateRange dtr = new DateRange(firstDate.Value, lastDate.Value);

        dtr = dtr >> (forward ? +1 : -1);

        firstDate = dtr.FirstDate;
        lastDate = dtr.LastDate;
        return true;
      }

      if (firstDate.HasValue)
      {
        if (!forward)
        {
          if (firstDate.Value == DateTime.MinValue)
            return false; // 02.09.2020
          lastDate = firstDate.Value.AddDays(-1);
          firstDate = null;
          return true;
        }
      }
      if (lastDate.HasValue)
      {
        if (forward)
        {
          if (lastDate.Value == DateTime.MaxValue.Date)
            return false; // 02.09.2020
          firstDate = lastDate.Value.AddDays(1);
          lastDate = null;
          return true;
        }
      }
      return false;
    }

    #endregion

    #region ShiftDateRangeYear

    /// <summary>
    /// Сдвиг интервала дат вперед или назад на один год.
    /// Если текущий период представляет собой целое число месяцев, то сдвиг
    /// выполняется на 12 месяцев. Иначе сдвиг выполняется по дням. Эта тонкость имеет значение только для високосных
    /// годов, если период заканчивается в феврале.
    /// Например, период {01.10.2018-28.02.2019} сдвигается вперед до {01.10.2019-29.02.2020},
    /// а {02.10.2018-28.02.2019} - до {02.10.2019-28.02.2020}.
    /// Если сдвиг нельзя выполнить, то ссылочные значения не изменяются и возвращается false.
    /// Используется в элементах пользовательского интерфейса, предназначенных для работы с интервалами дат.
    /// </summary>
    /// <param name="firstDate">Начальная дата (по ссылке)</param>
    /// <param name="lastDate">Конечная дата (по ссылке)</param>
    /// <param name="forward">true для сдвига вперед, false - назад</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRangeYear(ref DateTime firstDate, ref DateTime lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRangeYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1.Value;
        lastDate = dt2.Value;
      }
      return Res;
    }

    /// <summary>
    /// Сдвиг интервала дат вперед или назад на один год.
    /// Если текущий период представляет собой целое число месяцев, то сдвиг
    /// выполняется на 12 месяцев. Иначе сдвиг выполняется по дням. Эта тонкость имеет значение только для високосных
    /// годов, если период заканчивается в феврале.
    /// Например, период {01.10.2018-28.02.2019} сдвигается вперед до {01.10.2019-29.02.2020},
    /// а {02.10.2018-28.02.2019} - до {02.10.2019-28.02.2020}.
    /// Если сдвиг нельзя выполнить, то ссылочные значения не изменяются и возвращается false.
    /// Эта версия позволяет сдвигать полузакрытые интервалы.
    /// Используется в элементах пользовательского интерфейса, предназначенных для работы с интервалами дат.
    /// </summary>
    /// <param name="firstDate">Начальная дата (по ссылке)</param>
    /// <param name="lastDate">Конечная дата (по ссылке)</param>
    /// <param name="forward">true для сдвига вперед, false - назад</param>
    [DebuggerStepThrough]
    public static bool ShiftDateRangeYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      DateTime? dt1 = firstDate;
      DateTime? dt2 = lastDate;
      bool Res;
      try
      {
        Res = DoShiftDateRangeYear(ref dt1, ref dt2, forward);
      }
      catch
      {
        Res = false;
      }
      if (Res)
      {
        firstDate = dt1;
        lastDate = dt2;
      }
      return Res;
    }

    [DebuggerStepThrough]
    private static bool DoShiftDateRangeYear(ref DateTime? firstDate, ref DateTime? lastDate, bool forward)
    {
      bool Res = false;
      bool IsWholeMonth = false;
      if (firstDate.HasValue && lastDate.HasValue)
      {
        if (DataTools.IsBottomOfMonth(firstDate.Value) && DataTools.IsEndOfMonth(lastDate.Value))
          IsWholeMonth = true;
      }

      if (firstDate.HasValue)
      {
        firstDate = DataTools.CreateDateTime(firstDate.Value.Year + (forward ? +1 : -1), firstDate.Value.Month, firstDate.Value.Day);
        Res = true;
      }

      if (lastDate.HasValue)
      {
        lastDate = DataTools.CreateDateTime(lastDate.Value.Year + (forward ? +1 : -1), lastDate.Value.Month,
          IsWholeMonth ? 31 : lastDate.Value.Day);
        Res = true;
      }
      return Res;
    }

    #endregion

    #endregion

    #region Получение инкрементных значений


    /// <summary>
    /// Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
    /// Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
    /// округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=5,
    /// то будут возвращаться значения 0, 5, 10, 15, ... Но, для значения 1 возвращается 5, а не 2.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент (положительное значение) или декремент (отрицательное)</param>
    /// <returns>Новое значение</returns>
    public static int GetIncrementedValue(int currValue, int increment)
    {
      return currValue + increment;
    }

    /// <summary>
    /// Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
    /// Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
    /// округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=0.2,
    /// то будут возвращаться значения 0, 0.2, 0.4, 0.6, ... Но, для значения 0.1 возвращается 0.2, а не 0.3.
    /// Если <paramref name="increment"/> не является дробью вида 1/n, где n-целое число, то выполняется
    /// обычное сложение.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент (положительное значение) или декремент (отрицательное)</param>
    /// <returns>Новое значение</returns>
    public static float GetIncrementedValue(float currValue, float increment)
    {
      // TODO: Не реализовано

      return currValue + increment;
    }

    /// <summary>
    /// Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
    /// Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
    /// округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=0.2,
    /// то будут возвращаться значения 0, 0.2, 0.4, 0.6, ... Но, для значения 0.1 возвращается 0.2, а не 0.3.
    /// Если <paramref name="increment"/> не является дробью вида 1/n, где n-целое число, то выполняется
    /// обычное сложение.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент (положительное значение) или декремент (отрицательное)</param>
    /// <returns>Новое значение</returns>
    public static double GetIncrementedValue(double currValue, double increment)
    {
      // TODO: Не реализовано

      return currValue + increment;

      /*
      if (increment == 0.0)
        return currValue;

      double incMod = Math.Abs(increment);
      if (incMod >= 1.0)
      {
        if (Math.Truncate(incMod) != incMod)
          return currValue + increment;

        if (increment>0)
          currValue=
      }
      else
      { 
      }
       * */
    }

    /// <summary>
    /// Получение инкрементного значения для редактора числового поля со стрелочками прокрутки.
    /// Обычно возвращает <paramref name="currValue"/>+<paramref name="increment"/>, но сначала выполняет
    /// округление исходного значения в нужную сторону. Например, если <paramref name="increment"/>=0.2,
    /// то будут возвращаться значения 0, 0.2, 0.4, 0.6, ... Но, для значения 0.1 возвращается 0.2, а не 0.3.
    /// Если <paramref name="increment"/> не является дробью вида 1/n, где n-целое число, то выполняется
    /// обычное сложение.
    /// </summary>
    /// <param name="currValue">Текущее значение</param>
    /// <param name="increment">Инкремент (положительное значение) или декремент (отрицательное)</param>
    /// <returns>Новое значение</returns>
    public static decimal GetIncrementedValue(decimal currValue, decimal increment)
    {
      // TODO: Не реализовано

      return currValue + increment;
    }

    #endregion
  }
}
