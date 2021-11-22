// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using FreeLibSet.Core;

namespace FreeLibSet.Formatting
{
  /// <summary>
  /// Интерфейс провайдера ввода текста по маске
  /// </summary>
  public interface IMaskProvider
  {
    /// <summary>
    /// Выполняет проверку текстовой строке на соответствие маски.
    /// В случае соответствия возвращает true. В случае несоответствия возвращает false, а <paramref name="errorText"/> содержит сообщение об ошибке
    /// </summary>
    /// <param name="text">Проверяемый текст</param>
    /// <param name="errorText">Текст сообщения об ошибке</param>
    /// <returns>true, если <paramref name="text"/> соответствует маске</returns>
    bool Test(string text, out string errorText);

    /// <summary>
    /// Свойство возвращает маску, пригодую для использования в поле ввода MaskedTextBox или MaskedTextProvider.
    /// Если объект не поддерживает маски, совместимые с MaskedTextProvider, свойство возвращает пустую строку
    /// </summary>
    string EditMask { get;}

    /// <summary>
    /// Настройки локализации для символов-разделителей.
    /// Если системные разделители не используются, свойство должно возвращать CultureInfo.InvariantCultrure
    /// </summary>
    CultureInfo Culture { get; }
  }

  /// <summary>
  /// Переходник для стандартного MaskedTextProvider.
  /// Этот класс является потокобезопасным. При работе с MaskedTextProvider выполняется блокировка объекта
  /// </summary>
  /// <remarks>
  /// Если маска содержит только обязательные числовые позиции и символы-разделители, рекомендуется
  /// использовать класс SimpleDigitalMaskProvider, обладающий лучшим быстродействием.
  /// </remarks>
  public class StdMaskProvider : IMaskProvider
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый объект, используя созданный ранее MaskedTextProvider.
    /// Для обеспечения потокобезопасности не следует использовать переданный <paramref name="provider"/>
    /// для других целей.
    /// </summary>
    /// <param name="provider">Основной объект. Не может быть null</param>
    public StdMaskProvider(MaskedTextProvider provider)
    {
      if (provider == null)
        throw new ArgumentNullException("provider");

      _Provider = provider;
    }

    /// <summary>
    /// Создает новые объекты MaskedTextProvider и StdMaskProvider на основании заданной маски.
    /// </summary>
    /// <param name="mask">Маска для MaskedTextProvider</param>
    public StdMaskProvider(string mask)
      : this(new MaskedTextProvider(mask))
    {
    }

    /// <summary>
    /// Создает новые объекты MaskedTextProvider и StdMaskProvider на основании заданной маски и культуры.
    /// </summary>
    /// <param name="mask">Маска для MaskedTextProvider</param>
    /// <param name="culture">Задает локализацию для используемых разделителей</param>
    public StdMaskProvider(string mask, CultureInfo culture)
      : this(new MaskedTextProvider(mask, culture))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной объект.
    /// При работе с MaskedTextProvider выполняется Monitor.Enter()/Leave() для обеспечения потокобезопасности
    /// </summary>
    public MaskedTextProvider Provider { get { return _Provider; } }
    private MaskedTextProvider _Provider;

    /// <summary>
    /// Возвращает свойство MaskedTextProvider.Mask
    /// </summary>
    /// <returns>Текстовое представление объекта</returns>
    public override string ToString()
    {
      return _Provider.Mask;
    }

    #endregion

    #region IMaskProvider Members

    /// <summary>
    /// Проверить соответствие строки <paramref name="text"/> на соответствие маске.
    /// </summary>
    /// <param name="text">Проверяемая строка</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке, если строка не соответствует маске</param>
    /// <returns>true, если строка соответствует маске</returns>
    public bool Test(string text, out string errorText)
    {
      if (String.IsNullOrEmpty(text))
      {
        errorText = "Значение не задано";
        return false;
      }

      lock (_Provider)
      {
        return DoTest(text, out errorText);
      }
    }

    private bool DoTest(string text, out string errorText)
    {
      int TestPos;
      MaskedTextResultHint TestHint;
      if (_Provider.Set(text, out TestPos, out TestHint))
      {
        if (_Provider.MaskCompleted)
        {
          errorText = null;
          return true;
        }
        else
        {
          errorText = "Требуются дополнительные символы";
          return false;
        }
      }

      if (TestPos >= 0 && TestPos < text.Length)
        errorText = "Недопустимый символ \"" + text[TestPos] + "\" в позиции " + (TestPos + 1).ToString();
      else
        errorText = "Значение не соответствует маске";
      return false;
    }

    /// <summary>
    /// Проверить соответствие строки <paramref name="text"/> на соответствие маске.
    /// </summary>
    /// <param name="text">Проверяемая строка</param>
    /// <returns>true, если строка соответствует маске</returns>
    public bool Test(string text)
    {
      string ErrorText;
      return Test(text, out ErrorText);
    }


    /// <summary>
    /// Возвращает свойство MaskedTextProvider
    /// </summary>
    public string EditMask
    {
      get { return _Provider.Mask; }
    }

    /// <summary>
    /// Возвращает свойство MaskedTextProvider.Culture
    /// </summary>
    public CultureInfo Culture { get { return _Provider.Culture; } }

    #endregion
  }

  /// <summary>
  /// Простейшая реализация IMaskProvider. Провайдер поддерживает в маске только обязательные цифры (символ "0")
  /// и символы-разделители ".", "-", ":", " ". Необязательные цифры не поддерживаются.
  /// Класс является потокобезопасным.
  /// </summary>
  public class SimpleDigitalMaskProvider : IMaskProvider
  {
    #region Конструкторы

    /// <summary>
    /// Создать новый объект с заданной маской.
    /// </summary>
    /// <param name="mask">Маска, содержащая "0" для числовых позиций, и символы-разделители "-.: "</param>
    public SimpleDigitalMaskProvider(string mask)
    {
      if (String.IsNullOrEmpty(mask))
        throw new ArgumentNullException("mask");
      int p = DataTools.IndexOfAnyOther(mask, ValidMaskChars);
      if (p >= 0)
        throw new ArgumentException("Маска содержит недопустимый символ \"" + mask[p] + "\"", "mask");
      if (mask.IndexOf('0') < 0)
        throw new ArgumentException("Маска не содержит ни одной числовой позиции", "mask");
      _Mask = mask;
    }

    /// <summary>
    /// Создать новый объект с заданным числом цифр без символов-разделителей
    /// </summary>
    /// <param name="digits">Количество цифр</param>
    public SimpleDigitalMaskProvider(int digits)
    {
      if (digits < 1)
        throw new ArgumentOutOfRangeException("digits", digits, "Количество цифр должно быть больше 0");
      _Mask = new string('0', digits);
    }

    #endregion

    #region Свойства

    const string ValidMaskChars = "0 .:-";

    /// <summary>
    /// Маска
    /// </summary>
    public string Mask { get { return _Mask; } }
    private string _Mask;

    /// <summary>
    /// Возвращает свойство Mask
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Mask;
    }

    #endregion

    #region IMaskProvider Members

    /// <summary>
    /// Тестирование строки.
    /// </summary>
    /// <param name="text">Проверяемая строка</param>
    /// <param name="errorText">Сообщение об ошибке</param>
    /// <returns>true, если строка соответствует маске</returns>
    public bool Test(string text, out string errorText)
    {
      if (String.IsNullOrEmpty(text))
      {
        errorText = "Пустая строка";
        return false;
      }

      if (text.Length != _Mask.Length)
      {
        errorText = "Неправильная длина строки (" + text.Length + "). Должно быть " + _Mask.Length.ToString() + " символов";
        return false;
      }

      for (int i = 0; i < text.Length; i++)
      {
        if (_Mask[i] == '0')
        {
          if (text[i] < '0' || text[i] > '9') // исправлено 27.12.2020
          {
            errorText = "Неправильный символ \"" + text[i] + "\" в позиции " + (i + 1).ToString() + ". Ожидалась цифра";
            return false;
          }
        }
        else
        {
          if (text[i] != _Mask[i])
          {
            errorText = "Неправильный символ \"" + text[i] + "\" в позиции " + (i + 1).ToString() + ". Ожидался символ \"" + _Mask[i] + "\"";
            return false;
          }
        }
      }

      errorText = null;
      return true;
    }

    /// <summary>
    /// Проверить соответствие строки <paramref name="text"/> на соответствие маске.
    /// </summary>
    /// <param name="text">Проверяемая строка</param>
    /// <returns>true, если строка соответствует маске</returns>
    public bool Test(string text)
    {
      string ErrorText;
      return Test(text, out ErrorText);
    }

    /// <summary>
    /// Возвращает маску для редактирования, совместимую с MaskedTextProvider.
    /// Возвращает Mask, в котором символы-разделители дополнены оброатной косой чертой
    /// </summary>
    public string EditMask
    {
      get
      {
        StringBuilder sb = null;
        for (int i = 0; i < _Mask.Length; i++)
        {
          switch (Mask[i])
          {
            case '.':
            case ':':
              // Требуется экранирование
              if (sb == null)
              {
                sb = new StringBuilder();
                if (i > 0)
                  sb.Append(_Mask, 0, i - 1); // добавили предыдущие символы
              }
              sb.Append('\\');
              sb.Append(_Mask[i]);
              break;
            default:
              if (sb != null)
                sb.Append(_Mask[i]);
              break;
          }
        }
        if (sb == null)
          return _Mask;
        else
          return sb.ToString();
      }
    }

    /// <summary>
    /// Возвращает CultureInfo.InvariantCulture
    /// </summary>
    public CultureInfo Culture { get { return CultureInfo.InvariantCulture; } }

    #endregion
  }
}
