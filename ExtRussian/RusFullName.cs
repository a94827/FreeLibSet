﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Russian
{
  #region Перечисление RusFullNamePart

  /// <summary>
  /// Один из трех компонентов Ф.И.О.
  /// </summary>
  public enum RusFullNamePart
  {
    /// <summary>
    /// Фамилия
    /// </summary>
    Surname,

    /// <summary>
    /// Имя
    /// </summary>
    Name,

    /// <summary>
    /// Отчество
    /// </summary>
    Patronymic
  }

  #endregion

  #region Перечисление SexOfPerson

  /// <summary>
  /// Пол человека
  /// </summary>
  public enum SexOfPerson
  {
    /*
     * По поводу различия между "sex" и "gender" в английском языке, см. статью в Википедии
     * "Sex and gender distinction"
     * http://en.wikipedia.org/wiki/Sex_and_gender_distinction
     */

    /// <summary>
    /// Мужской
    /// </summary>
    Male = 0,

    /// <summary>
    /// Женский                                 
    /// </summary>
    Female = 1,

    /// <summary>
    /// Неизвестный
    /// </summary>
    Undefined = -1
  }

  #endregion

  #region Перечисление RusFullNameValidateOptions

  /// <summary>
  /// Опции проверки корректности Ф.И.О.
  /// </summary>
  [Flags]
  public enum RusFullNameValidateOptions
  {
    /// <summary>
    /// Режим проверки по умолчанию.
    /// Имя и отчество должны быть заданы полностью, а не инициалами.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Если флаг задан, то имя и отчество могут быть заданы одной буквой, но могут быть заданы и полностью
    /// </summary>
    InitialsAllowed = 1,

    /// <summary>
    /// Допускаются только инициалы вместо имени и отчества
    /// </summary>
    InitialsOnly = 2,

    /// <summary>
    /// Если флаг установлен, то будет игнорироваться регистр при проверке символов
    /// </summary>
    IgnoreCases = 4,
  }

  #endregion

  /// <summary>
  /// Фамилия, Имя и Отчество человека.
  /// Обычно хранит полное Ф.И.О., но может использоваться и для хранения фамилии и инициалов (по одному символу).
  /// Поддерживаются компоненты с дефисом, а также с окончаниями "ОГЛЫ", "КЫЗЫ" через пробел.
  /// Позволяет определить пол по отчеству (и по имени, если есть окончание "ОГЛЫ", "КЫЗЫ").
  /// Умеет склонять Ф.И.О. по падежам.
  /// Структура однократной записи.
  /// </summary>
  [Serializable]
  public struct RusFullName : IEquatable<RusFullName>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует структуры заданными Ф.И.О.
    /// Компоненты не проверяются и не модифицируются.
    /// </summary>
    /// <param name="surname">Фамилия</param>
    /// <param name="name">Имя</param>
    /// <param name="patronymic">Отчество</param>
    public RusFullName(string surname, string name, string patronymic)
    {
      if (surname == null)
        _Surname = String.Empty;
      else
        _Surname = surname;

      if (name == null)
        _Name = String.Empty;
      else
        _Name = name;

      if (patronymic == null)
        _Patronymic = String.Empty;
      else
        _Patronymic = patronymic;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Фамилия
    /// </summary>
    public string Surname { get { return _Surname; } }
    private readonly string _Surname;

    /// <summary>
    /// Имя
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    /// <summary>
    /// Отчество
    /// </summary>
    public string Patronymic { get { return _Patronymic; } }
    private readonly string _Patronymic;

    #endregion

    #region Получение текста

    /// <summary>
    /// Полное имя в виде "Иванов Иван Иванович"
    /// </summary>
    public string FullName
    {
      get
      {
        if (String.IsNullOrEmpty(Surname))
          return String.Empty;

        string s = Surname;

        if (String.IsNullOrEmpty(Name))
          return s;

        s += " " + Name;

        if (String.IsNullOrEmpty(Patronymic))
          return s;

        s += " " + Patronymic;
        return s;
      }
    }

    // 26.04.2021 Подчеркивания больше не поддерживаются.
    //private static string MyStr(string s)
    //{
    //  if (String.IsNullOrEmpty(s))
    //    return String.Empty;
    //  if (s[0] == '_')
    //    return s.Substring(1);
    //  else
    //    return s;
    //}

    /// <summary>
    /// Строка в виде "Иванов И.И."
    /// Фамилия отделяется от иницалов неразрывным пробелом.
    /// </summary>
    public string NameWithInitials
    {
      get
      {
        if (String.IsNullOrEmpty(Surname))
          return String.Empty;

        StringBuilder sb = new StringBuilder();
        sb.Append(Surname);

        if (Name.Length > 0)
        {
          //sb.Append(" ");
          sb.Append(DataTools.NonBreakSpaceChar); // 06.06.2024
          sb.Append(Name, 0, 1);
          sb.Append(".");

          if (Patronymic.Length > 0)
          {
            sb.Append(Patronymic, 0, 1);
            sb.Append(".");
          }
        }
        return sb.ToString();
      }
    }

    /// <summary>
    /// Строка в обратном порядке "И.И. Иванов".
    /// Инициалы от фамилии отделяются неразрывным пробелом.
    /// </summary>
    public string InvNameWithInitials
    {
      get
      {
        if (String.IsNullOrEmpty(Surname))
          return String.Empty;

        StringBuilder sb = new StringBuilder();
        if (!String.IsNullOrEmpty(Name))
        {
          sb.Append(Name, 0, 1);
          sb.Append(".");

          if (!String.IsNullOrEmpty(Patronymic))
          {
            sb.Append(Patronymic, 0, 1);
            sb.Append(".");
          }
          sb.Append(DataTools.NonBreakSpaceChar); // 06.06.2024
        }
        sb.Append(Surname);
        return sb.ToString();
      }
    }

    /// <summary>
    /// Возвращает свойство <see cref="FullName"/> или <see cref="NameWithInitials"/>.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (String.IsNullOrEmpty(Surname))
        return String.Empty;
      if (Name.Length == 1)
        return NameWithInitials;
      else
        return FullName;
    }

    #endregion

    #region Другие свойства

    /// <summary>
    /// Пустой объект
    /// </summary>
    public static readonly RusFullName Empty = new RusFullName(String.Empty, String.Empty, String.Empty);

    /// <summary>
    /// Возвращает true, если фамилия не заполнена
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(Surname); } }

    /// <summary>
    /// Получить структуру Ф.И.О. с сокращенным именем и отчеством.
    /// То есть, если текущее Ф.И.О. - "Иванов Иван Иванович", то будет
    /// возвращена структура "Иванов И И"
    /// </summary>
    public RusFullName NameWithInitalsObj
    {
      get
      {
        string n = String.IsNullOrEmpty(Name) ? String.Empty : Name.Substring(0, 1);
        string p = String.IsNullOrEmpty(Patronymic) ? String.Empty : Patronymic.Substring(0, 1);
        return new RusFullName(Surname, n, p);
      }
    }

    /// <summary>
    /// Возвращает true, если заданы только инициалы
    /// </summary>
    public bool IsInitials
    {
      get
      {
        if (!GetIsInitials(Name))
          return false;
        if (!String.IsNullOrEmpty(Patronymic))
        {
          if (!GetIsInitials(Patronymic))
            return false;
        }
        return true;
      }
    }

    private static bool GetIsInitials(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      if (s.Length > 1)
        return false;
      return RussianTools.RussianCharIndexer.Contains(s[0]);
    }

    /// <summary>
    /// Возвращает требуемый компонент Ф.И.О.
    /// </summary>
    /// <param name="part">Требуемый компонент</param>
    /// <returns>Значение свойства</returns>
    public string this[RusFullNamePart part]
    {
      get
      {
        switch (part)
        {
          case RusFullNamePart.Surname: return Surname;
          case RusFullNamePart.Name: return Name;
          case RusFullNamePart.Patronymic: return Patronymic;
          default:
            throw ExceptionFactory.ArgUnknownValue("part", part);
        }
      }
    }

    #endregion

    #region Парсинг

    /// <summary>
    /// Извлечение компонентов Ф.И.О. из строки в максимально возможной степени.
    /// Если <paramref name="s"/> - пустая строка, возвращается <see cref="RusFullName.Empty"/>.
    /// </summary>
    /// <param name="s">Строка для преобразования</param>
    /// <returns>Заполненная структура <see cref="RusFullName"/></returns>
    /// <exception cref="InvalidCastException">Если не удалось распознать Ф.И.О.</exception>
    public static RusFullName Parse(string s)
    {
      return Parse(s, true);
    }

    /// <summary>
    /// Извлечение компонентов Ф.И.О. из строки.
    /// Если <paramref name="s"/> -пустая строка, возвращается <see cref="RusFullName.Empty"/>.
    /// </summary>
    /// <param name="s">Строка для преобразования</param>
    /// <param name="autoCorrect">Если true, то будет выполнена попытка автокоррекции строки</param>
    /// <returns>Заполненная структура <see cref="RusFullName"/></returns>
    /// <exception cref="InvalidCastException">Если не удалось распознать Ф.И.О.</exception>
    public static RusFullName Parse(string s, bool autoCorrect)
    {
      RusFullName res;
      string errorText;
      if (TryParse(s, out res, out errorText, autoCorrect))
        return res;
      else
        throw new InvalidCastException("Строку \"" + s + "\" нельзя преобразовать в Ф.И.О. " + errorText);
    }

    /// <summary>
    /// Выполняет попытку извлечения компонентов Ф.И.О. из строки.
    /// Если <paramref name="s"/> -пустая строка, возвращается <paramref name="res"/>=<see cref="RusFullName.Empty"/> и значение true.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="res">Сюда записывается преобразованное значение</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <param name="autoCorrect">Если true, то будет выполнена попытка автокоррекции строки</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out RusFullName res, out string errorText, bool autoCorrect)
    {
      errorText = null;
      if (String.IsNullOrEmpty(s))
      {
        res = Empty;
        return true;
      }

      if (autoCorrect)
        s = CorrectText(s);

      string part1 = GetNextPart(ref s);
      string part2 = GetNextPart(ref s);
      string part3 = GetNextPart(ref s);

      // 10.06.2024
      // Если строка задана в формате "И И Иванов" или "И Иванов", требуется переставить части местами

      if (part1.Length == 1 && part2.Length == 1 && part3.Length > 1) // "И И Иванов"
        res = new RusFullName(part3, part1, part2);
      else if (part1.Length == 1 && part2.Length > 1 && part3.Length == 0) // "И Иванов"
        res = new RusFullName(part2, part1, String.Empty);
      else
        res = new RusFullName(part1, part2, part3); // "Иванов Иван Иванович", "Иванов Иван", "Иванов И И" или "Иванов И"
      if (autoCorrect)
        res = res.Normalized;

      if (s.Length > 0)
      {
        errorText = "Остаток строки \"" + s + "\" не может быть добавлен к Ф.И.О.";
        return false;
      }

      if (autoCorrect)
      {
        if (!res.IsValid(RusFullNameValidateOptions.InitialsAllowed | RusFullNameValidateOptions.IgnoreCases, out errorText))
          return false;
      }

      return true;
    }

    private static string GetNextPart(ref string s)
    {
      string s1;
      if (s.Length > 1)
      {
        if (s[1] == '.')
        {
          s1 = s.Substring(0, 1);
          s = s.Substring(2);
          return s1;
        }
      }

      int p1 = s.IndexOf(' ');
      if (p1 < 0)
      {
        // Больше пробелов нет
        s1 = s;
        s = String.Empty;
        return s1;
      }

      int p2 = s.IndexOf(' ', p1 + 1);
      string s2;
      if (p2 < 0)
      {
        s2 = s.Substring(p1 + 1);
        p2 = s.Length;
      }
      else
        s2 = s.Substring(p1 + 1, p2 - p1 - 1);

      s1 = s.Substring(0, p1);
      if (_SpacedSuffixMaleIndexer.Contains(s2) ||
        _SpacedSuffixFemaleIndexer.Contains(s2)) // испр. 18.06.2021
      {
        if (p2 >= s.Length)
          s = String.Empty;
        else
          s = s.Substring(p2 + 1);
        return s1 + ' ' + s2;
      }
      else
      {
        s = s.Substring(p1 + 1);
        return s1;
      }
    }

    /// <summary>
    /// Выполняет попытку извлечения компонентов Ф.И.О. из строки.
    /// Выполняется попытка автокоррекции строки.
    /// Если <paramref name="s"/> -пустая строка, возвращается <paramref name="res"/>=<see cref="RusFullName.Empty"/> и значение true.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="res">Сюда записывается преобразованное значение</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out RusFullName res, out string errorText)
    {
      return TryParse(s, out res, out errorText, true);
    }

    /// <summary>
    /// Выполняет попытку извлечения компонентов Ф.И.О. из строки.
    /// Выполняется попытка автокоррекции строки.
    /// Если <paramref name="s"/> -пустая строка, возвращается <paramref name="res"/>=<see cref="RusFullName.Empty"/> и значение true.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="res">Сюда записывается преобразованное значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public static bool TryParse(string s, out RusFullName res)
    {
      string errorText;
      return TryParse(s, out res, out errorText, true);
    }

    #endregion

    #region Нормализация

    /// <summary>
    /// Преобразование регистров Ф.И.О., удаление точек и лишних символов.
    /// Буква "Ё" сохраняется.
    /// Пробелы заменяются на "-", кроме окончаний "Кызы", "Гызы" и "Оглы"
    /// </summary>
    public RusFullName Normalized
    {
      get
      {
        string s = GetNormailizedPart(Surname, true);
        s = s.Replace(' ', '-');
        string n = GetNormailizedPart(Name, true);
        n = n.Replace(' ', '-');
        string p = GetNormailizedPart(Patronymic, true);
        return new RusFullName(s, n, p);
      }
    }

    /// <summary>
    /// Преобразование регистров Ф.И.О без преобразования символов
    /// ("ИВАНОВ ИВАН ИВАНОВИЧ" -> "Иванов Иван Иванович")
    /// </summary>
    public RusFullName NormalCased
    {
      get
      {
        return new RusFullName(
          GetNormailizedPart(Surname, false),
          GetNormailizedPart(Name, false),
          GetNormailizedPart(Patronymic, false));
      }
    }

    private static string GetNormailizedPart(string s, bool replace)
    {
      if (string.IsNullOrEmpty(s))
        return String.Empty;

      if (replace)
      {
        s = CorrectText(s);

        // 21.02.2024
        s = s.Replace(' ', '-');
        if (s.IndexOf('-') >= 0)
        {
          if (s.EndsWith("-КЫЗЫ", StringComparison.OrdinalIgnoreCase) ||
            s.EndsWith("-ГЫЗЫ", StringComparison.OrdinalIgnoreCase) ||
            s.EndsWith("-ОГЛЫ", StringComparison.OrdinalIgnoreCase))
          {
            s = s.Substring(0, s.Length - 5) + ' ' + s.Substring(s.Length - 4);
          }
        }
      }
      // Исправляем регистр
      return DataTools.ToUpperWordsInvariant(s);
    }

    /// <summary>
    /// Убирает концевые и двойные пробелы, точки, неправильные сочетания "-".
    /// Регистр символов не меняется
    /// </summary>
    /// <param name="s">Корректируемая строка</param>
    /// <returns>Исправленная строка</returns>
    private static string CorrectText(string s)
    {
      // Заменяем неразрывные пробелы (10.06.2024)
      s = s.Replace(DataTools.NonBreakSpaceChar, ' ');
      // Убираем точки
      s = s.Replace(".", " ");
      // Убираем пробелы вокруг дефиса
      s = s.Replace(" -", "-");
      s = s.Replace("- ", "-");
      s = DataTools.RemoveDoubleChars(s, '-');
      // Убираем двойные и концевые пробелы
      s = DataTools.RemoveDoubleChars(s, ' ');
      s = s.Trim();
      return s;
    }

    /// <summary>
    /// Преобразование к верхнему регистру
    /// </summary>
    public RusFullName UpperCased
    {
      get
      {
        return new RusFullName(Surname.ToUpperInvariant(), Name.ToUpperInvariant(), Patronymic.ToUpperInvariant());
      }
    }

    /// <summary>
    /// Замена букв "Ё" на "Е"
    /// </summary>
    public RusFullName YoReplaced
    {
      get
      {
        return new RusFullName(ReplaceYo(Surname), ReplaceYo(Name), ReplaceYo(Patronymic));
      }
    }

    private static string ReplaceYo(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      s = s.Replace('Ё', 'Е');
      s = s.Replace('ё', 'е');
      return s;
    }

    #endregion

    #region Замена частей

    /// <summary>
    /// Возвращает новый объект Ф.И.О. с замененной фамилией
    /// </summary>
    /// <param name="newSurname">Новая фамилия</param>
    /// <returns>Копия объекта с заменой</returns>
    public RusFullName SetSurname(string newSurname)
    {
      return new RusFullName(newSurname, this.Name, this.Patronymic);
    }

    /// <summary>
    /// Возвращает новый объект Ф.И.О. с замененным именем
    /// </summary>
    /// <param name="newName">Новое имя</param>
    /// <returns>Копия объекта с заменой</returns>
    public RusFullName SetName(string newName)
    {
      return new RusFullName(this.Surname, newName, this.Patronymic);
    }

    /// <summary>
    /// Возвращает новый объект Ф.И.О. с замененным отчеством
    /// </summary>
    /// <param name="newPatronymic">Новое отчество</param>
    /// <returns>Копия объекта с заменой</returns>
    public RusFullName SetPatronymic(string newPatronymic)
    {
      return new RusFullName(this.Surname, this.Name, newPatronymic);
    }

    #endregion

    #region Склонение Ф.И.О.

    /// <summary>
    /// Получение 12 склонений
    /// </summary>
    /// <param name="sex">Пол человека. Если Undefined, то определяется автоматически исходя из отчества</param>
    /// <returns>Массив из 12 элементов</returns>
    public RusFullName[] GetCases(SexOfPerson sex)
    {
      RusFullName[] forms;
      GetCases(sex, out forms);
      return forms;
    }

    /// <summary>
    /// Получение 12 склонений.
    /// </summary>
    /// <param name="sex">Пол человека. Если Undefined, то определяется автоматически исходя из отчества</param>
    /// <param name="forms">По ссылке возвращается массив из 12 элементов</param>
    /// <returns>В текущей реализации всегда возвращается true</returns>
    public bool GetCases(SexOfPerson sex, out RusFullName[] forms)
    {
      if (sex == SexOfPerson.Undefined)
        // Пытаемся определить пол
        sex = this.Sex;

      RusGender gender;
      switch (sex)
      {
        case SexOfPerson.Male:
          gender = RusGender.Masculine;
          break;
        case SexOfPerson.Female:
          gender = RusGender.Feminine;
          break;
        default:
          gender = RusGender.Undefined;
          break;
      }

      forms = new RusFullName[12];

      RusNounFormArray surnames = new RusNounFormArray();
      RusNounFormArray names = new RusNounFormArray();
      RusNounFormArray patronymics = new RusNounFormArray();

      bool noCasesSurname = false;
      if (String.IsNullOrEmpty(Surname))
        noCasesSurname = true;
      else
      {
        string surname1 = Surname.ToUpperInvariant();
        if (surname1.EndsWith("О", StringComparison.Ordinal) ||
          surname1.EndsWith("ИХ", StringComparison.Ordinal) ||
          surname1.EndsWith("ЫХ", StringComparison.Ordinal))
          // Фамилии на "О" не склоняются
          noCasesSurname = true;

        // 20.09.2011
        // Женские фамилии, не имеющие суффиксов, не склоняются
        if (gender == RusGender.Feminine)
        {
          if ("БВГДЕЖЗИЙКЛМНПРСТУФХЦШЫЬЭЮ".IndexOf(surname1[surname1.Length - 1]) >= 0)
            noCasesSurname = true;
        }
      }
      if (noCasesSurname)
        surnames.Fill(Surname);
      else
        surnames.Fill(Surname, gender, RusFormArrayGetCasesOptions.NoPlural | RusFormArrayGetCasesOptions.Which| RusFormArrayGetCasesOptions.Animacy);

      names.Fill(Name, gender, RusFormArrayGetCasesOptions.NoPlural | RusFormArrayGetCasesOptions.Name | RusFormArrayGetCasesOptions.Animacy);
      patronymics.Fill(Patronymic, gender, RusFormArrayGetCasesOptions.NoPlural | RusFormArrayGetCasesOptions.Animacy);

      string[] surnameItems = surnames.Items;
      string[] nameItems = names.Items;
      string[] patronymicItems = patronymics.Items;
      for (int i = 0; i < 12; i++)
        forms[i] = new RusFullName(surnameItems[i], nameItems[i], patronymicItems[i]);
      return true;
    }

    #endregion

    #region Пол

    /// <summary>
    /// Определение пола человека по Ф.И.О.
    /// </summary>
    public SexOfPerson Sex
    {
      get
      {
        // Шаг 1. - по отчеству
        SexOfPerson res = GetSexFromPatronymic(Patronymic);
        if (res != SexOfPerson.Undefined)
          return res;

        // 19.07.2018
        // Определение пола по фамилии отключено.
        // Например, фамилия "Шин" не обязательно относится к мужскому полу.
        // Можно было бы проверять проверять наличие гласной перед окончанием,
        // но это не дает гарантии для иностранных фамилий, например, китайских.
#if XXX
        // Шаг 2. - по фамилии
        if (!String.IsNullOrEmpty(Surname))
        {
          s = Surname.ToUpperInvariant();
          if (s.EndsWith("ОВ") || s.EndsWith("ЕВ") || s.EndsWith("ИН") ||
            s.EndsWith("ЫЙ") || s.EndsWith("ИЙ"))
          {
            if (HasVowelInBase(s, 2)) // 19.07.2018
              return SexOfPerson.Male;
          }
          else if (s.EndsWith("ОВА") || s.EndsWith("ЕВА") ||
            s.EndsWith("ИНА"))
          {
            if (HasVowelInBase(s, 3)) // 19.07.2018
              return SexOfPerson.Female;
          }
          else if (s.EndsWith("АЯ")) // 19.07.2018
          {
            if (HasVowelInBase(s, 2))
              return SexOfPerson.Female;
          }
        }
#endif

        // 27.04.2021
        // В имени тоже может быть суффикс "КЫЗЫ" или "ОГЛЫ"
        res = GetSexFromSuffix(Name);

        return res;
        // Неизвестно
        //return SexOfPerson.Undefined;
      }
    }

    /// <summary>
    /// Возвращает пол на основании отчества
    /// </summary>
    /// <param name="patronymic">Отчество</param>
    /// <returns>Пол</returns>
    public static SexOfPerson GetSexFromPatronymic(string patronymic)
    {
      if (String.IsNullOrEmpty(patronymic))
        return SexOfPerson.Undefined;

      if (patronymic.EndsWith("ИЧ", StringComparison.OrdinalIgnoreCase))
        return SexOfPerson.Male;
      if (patronymic.EndsWith("ВНА", StringComparison.OrdinalIgnoreCase) ||
        patronymic.EndsWith("ЧНА", StringComparison.OrdinalIgnoreCase))
        return SexOfPerson.Female;

      return GetSexFromSuffix(patronymic);
    }

    private static SexOfPerson GetSexFromSuffix(string s)
    {
      if (String.IsNullOrEmpty(s))
        return SexOfPerson.Undefined;

      int p = Math.Max(s.LastIndexOf(' '), s.LastIndexOf('-'));
      if (p < 0)
        return SexOfPerson.Undefined;

      string suffix = s.Substring(p + 1);
      if (_SpacedSuffixMaleIndexer.Contains(suffix))
        return SexOfPerson.Male;
      if (_SpacedSuffixFemaleIndexer.Contains(suffix))
        return SexOfPerson.Female;

      return SexOfPerson.Undefined;
    }

    #endregion

    #region Сравнение

    /// <summary>
    /// Сравнивает с другим Ф.И.О.
    /// Регистр символов игнорируется.
    /// Считаются одинаковыми Ф.И.О., если в одном объекте имя и отчество заданы полностью, а в другом - только сокращение.
    /// Например, "Иванов Иван Иванович"=="Иванов И.И."
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(RusFullName obj)
    {
      return this == obj;
    }

    /// <summary>
    /// Сравнивает с другим Ф.И.О.
    /// Регистр символов игнорируется.
    /// Считаются одинаковыми Ф.И.О., если в одном объекте имя и отчество заданы полностью, а в другом - только сокращение.
    /// Например, "Иванов Иван Иванович"=="Иванов И.И."
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      if (!(obj is RusFullName))
        return false;
      return this == (RusFullName)obj;
    }

    /// <summary>
    /// Сравнивает два Ф.И.О.
    /// Регистр символов игнорируется.
    /// Буквы "Е" и "Ё" считаются одинаковыми.
    /// Считаются одинаковыми Ф.И.О., если в одном объекте имя и отчество заданы полностью, а в другом - только сокращение.
    /// Например, "Иванов Иван Иванович"=="Иванов И.И."
    /// </summary>
    /// <param name="obj1">Первый сравниваемый объект</param>
    /// <param name="obj2">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(RusFullName obj1, RusFullName obj2)
    {
      if (!CompareParts(obj1.Surname, obj2.Surname, 1))
        return false;
      if (!CompareParts(obj1.Name, obj2.Name, 2))
        return false;
      return CompareParts(obj1.Patronymic, obj2.Patronymic, 3);
    }

    private static bool CompareParts(string s1, string s2, int nPart)
    {
      if (s1 == null)
        s1 = String.Empty;
      if (s2 == null)
        s2 = String.Empty;
      s1 = s1.Replace('Ё', 'Е').Replace('ё', 'е').Replace(' ', '-'); // 20.02.2024. Замена ё на е в нижнем регистре.
      s2 = s2.Replace('Ё', 'Е').Replace('ё', 'е').Replace(' ', '-');

      // Точное сравнение
      if (String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase))
        return true;

      // Имя и отчество может быть одно задано, а другое - нет
      if (nPart >= 2)
      {
        if (s1.Length == 0 && s2.Length > 0)
          return true;
        if (s1.Length > 0 && s2.Length == 0)
          return true;

        if (s1.Length > 1 && s2.Length == 1)
          return s1[0] == s2[0];
        if (s1.Length == 1 && s2.Length > 1)
          return s1[0] == s2[0];
      }

      return false;
    }

    /// <summary>
    /// Сравнивает два Ф.И.О.
    /// Регистр символов игнорируется.
    /// Считаются одинаковыми Ф.И.О., если в одном объекте имя и отчество заданы полностью, а в другом - только сокращение.
    /// Например, "Иванов Иван Иванович"=="Иванов И.И."
    /// </summary>
    /// <param name="obj1">Первый сравниваемый объект</param>
    /// <param name="obj2">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(RusFullName obj1, RusFullName obj2)
    {
      return !(obj1 == obj2);
    }

    /// <summary>
    /// Возвращает хэш-код из фамилии.
    /// Требуется для работы коллекций
    /// </summary>
    /// <returns>Хэш код</returns>
    public override int GetHashCode()
    {
      if (String.IsNullOrEmpty(Surname))
        return 0;
      else
      {
        string s = Surname.ToUpperInvariant().Replace('Ё', 'Е');
        return s.GetHashCode();
      }
    }

    #endregion

    #region Проверка корректности

    #region Проверка Ф.И.О. в-целом

    /// <summary>
    /// Выполнить проверку фамилии, имени и отчества.
    /// Требуется, чтобы фамилия и имя были заданы. Пустой объект (<see cref="IsEmpty"/>=true) считается неправильным.
    /// </summary>
    /// <param name="options">Опции проверки</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true, если все компоненты корректны</returns>
    public bool IsValid(RusFullNameValidateOptions options, out string errorText)
    {
      if (!DoIsValidSurname(Surname, options, out errorText, 0))
        return false;
      if (!DoIsValidName(Name, options, out errorText, Surname.Length + 1))
        return false;
      if (!DoIsValidPatronymic(Patronymic, options, out errorText, Surname.Length + Name.Length + 2))
        return false;

      if (!IsValidComplex(options, out errorText))
        return false;

      return true;
    }

    /// <summary>
    /// Выполнить проверку фамилии, имени и отчества.
    /// Требуется, чтобы фамилия и имя были заданы. Пустой объект (<see cref="IsEmpty"/>=true) считается неправильным.
    /// </summary>
    /// <param name="options">Опции проверки</param>
    /// <returns>true, если все компоненты корректны</returns>
    public bool IsValid(RusFullNameValidateOptions options)
    {
      string errorText;
      return IsValid(options, out errorText);
    }

    private bool IsValidComplex(RusFullNameValidateOptions options, out string errorText)
    {
      if ((options & RusFullNameValidateOptions.InitialsAllowed) == RusFullNameValidateOptions.InitialsAllowed &&
        (!String.IsNullOrEmpty(Patronymic)))
      {
        bool nameIsInitial = Name.Length == 1;
        bool patronymicIsInitial = Patronymic.Length == 1;
        if (nameIsInitial != patronymicIsInitial)
        {
          errorText = "Имя и отчество либо должны быть заданы полностью, либо оба инициалами";
          return false;
        }
      }

      errorText = null;
      return true;
    }

    /// <summary>
    /// Проверка корректности строки, содержащей фамилию, имени и отчество, или содержащей фамилию с инициалами
    /// (в зависимости от параметра <paramref name="options"/>).
    /// Требуется, чтобы фамилия и имя были заданы. Пустая строка считается неправильной.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="options">Режимы проверки</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>True, если проверка выполнена успешно</returns>
    public static bool IsValid(string s, RusFullNameValidateOptions options, out string errorText)
    {
      if (!IsValidGeneral(s, out errorText))
        return false;

      RusFullName rfn;
      if (!TryParse(s, out rfn, out errorText, false))
        return false;

      if (!DoIsValidSurname(rfn.Surname, options, out errorText, 0))
        return false;
      if (!DoIsValidName(rfn.Name, options, out errorText, rfn.Surname.Length + 1))
        return false;
      if (!DoIsValidPatronymic(rfn.Patronymic, options, out errorText, rfn.Surname.Length + rfn.Name.Length + 2))
        return false;
      if (!rfn.IsValidComplex(options, out errorText))
        return false;
      return true;
    }

    /// <summary>
    /// Проверка корректности строки, содержащей фамилию, имени и отчество, или содержащей фамилию с инициалами
    /// (в зависимости от параметра <paramref name="options"/>).
    /// Требуется, чтобы фамилия и имя были заданы. Пустая строка считается неправильной.
    /// </summary>
    /// <param name="s">Проверяемая строка</param>
    /// <param name="options">Режимы проверки</param>
    /// <returns>True, если проверка выполнена успешно</returns>
    public static bool IsValid(string s, RusFullNameValidateOptions options)
    {
      string errorText;
      return IsValid(s, options, out errorText);
    }

    #endregion

    #region Общая проверка строки

    private static readonly CharArrayIndexer _ValidCharIndexer = new CharArrayIndexer(RussianTools.UpperRussianChars + RussianTools.LowerRussianChars + ". -");

    private static readonly StringArrayIndexer _BadPairIndexer = new StringArrayIndexer(new string[]{
      "ЪЪ", "ЬЬ", "ЪЬ", "ЬЪ",
      "АЬ", "ЕЬ", "ЁЬ", "ИЬ", "ЙЬ", "ОЬ", "УЬ", "ЫЬ", "ЭЬ", "ЮЬ", "ЯЬ",
      // Убрано 04.12.2020
      // Есть фамилия "Муъминов". См. например: https://www.rusprofile.ru/ip/317057100007161
      // "АЪ", "ЕЪ", "ЁЪ", "ИЪ", "ЙЪ", "ОЪ", "УЪ", "ЫЪ", "ЭЪ", "ЮЪ", "ЯЪ",

      // может быть еще есть

      "--", "- ", " -"
    });


    private static bool IsValidGeneral(string s, out string errorText)
    {
      errorText = null;
      if (String.IsNullOrEmpty(s))
        return true;

      for (int i = 0; i < s.Length; i++)
      {
        if (!_ValidCharIndexer.Contains(s[i]))
        {
          errorText = "Недопустимый символ \"" + s[i] + "\" в позиции " + (i + 1).ToString();
          return false;
        }
      }

      if (s[0] == ' ' || s[s.Length - 1] == ' ')
      {
        errorText = "Ф.И.О. не может начинаться или заканчиваться пробелом";
        return false;
      }
      int p = s.IndexOf("  ", StringComparison.Ordinal);
      if (p >= 0)
      {
        errorText = "Два пробела подряд в позиции " + (p + 1).ToString();
        return false;
      }

      // 16.11.2020
      // Проверяем три одинаковые буквы подряд
      string sUpper = s.ToUpperInvariant();
      for (int i = 2; i < sUpper.Length; i++)
      {
        if (sUpper[i] == sUpper[i - 1] && sUpper[i] == sUpper[i - 2])
        {
          errorText = "Три символа \"" + s.Substring(i - 2, 3) + "\" подряд в позиции " + (p + 1).ToString();
          return false;
        }
      }

      // 16.11.2020
      // Проверяем недопустимые сочетания символов.
      // В DataTools, к сожалению, нет метода IndexOfAny() для StringArrayIndexer.
      // Зато мы знаем, что BadPairs содержит только двусимвольные строки
      //int p = DataTools.IndexOfAny(sUpper, BadPairs);
      for (int i = 1; i < sUpper.Length; i++)
      {
        string s2 = sUpper.Substring(i - 1, 2);
        if (_BadPairIndexer.IndexOf(s2) >= 0)
        {
          errorText = "Недопустимое сочетание символов \"" + s.Substring(i - 1, 2) + "\" в позиции " + (p + 1).ToString();
          return false;
        }
      }

      return true;
    }

    #endregion

    #region Проверка отдельных компонентов

    /// <summary>
    /// Суффиксы, которые могут идти после пробела
    /// </summary>
    private static readonly StringArrayIndexer _SpacedSuffixMaleIndexer = new StringArrayIndexer(new string[] { "ОГЛЫ" }, true);
    private static readonly StringArrayIndexer _SpacedSuffixFemaleIndexer = new StringArrayIndexer(new string[] { "КЫЗЫ", "ГЫЗЫ" }, true);

    /// <summary>
    /// Выполнить проверку фамилии
    /// </summary>
    /// <param name="surname">Проверяемое значение</param>
    /// <param name="options">Опции проверки</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true, если значение корректно</returns>
    public static bool IsValidSurname(string surname, RusFullNameValidateOptions options, out string errorText)
    {
      if (!IsValidGeneral(surname, out errorText))
        return false;
      return DoIsValidSurname(surname, options, out errorText, 0);
    }

    /// <summary>
    /// Выполнить проверку фамилии
    /// </summary>
    /// <param name="surname">Проверяемое значение</param>
    /// <param name="options">Опции проверки</param>
    /// <returns>true, если значение корректно</returns>
    public static bool IsValidSurname(string surname, RusFullNameValidateOptions options)
    {
      string errorText;
      return IsValidSurname(surname, options, out errorText);
    }

    private static bool DoIsValidSurname(string surname, RusFullNameValidateOptions options, out string errorText, int charOff)
    {
      if (String.IsNullOrEmpty(surname))
      {
        errorText = "Фамилия должна быть задана";
        return false;
      }

      return TestPart(surname, options, out errorText, RusFullNamePart.Surname, charOff);
    }

    /// <summary>
    /// Выполнить проверку имени
    /// </summary>
    /// <param name="name">Проверяемое значение</param>
    /// <param name="options">Опции проверки</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true, если значение корректно</returns>
    public static bool IsValidName(string name, RusFullNameValidateOptions options, out string errorText)
    {
      if (!IsValidGeneral(name, out errorText))
        return false;
      return DoIsValidName(name, options, out errorText, 0);
    }

    /// <summary>
    /// Выполнить проверку имени
    /// </summary>
    /// <param name="name">Проверяемое значение</param>
    /// <param name="options">Опции проверки</param>
    /// <returns>true, если значение корректно</returns>
    public static bool IsValidName(string name, RusFullNameValidateOptions options)
    {
      string errorText;
      return IsValidName(name, options, out errorText);
    }

    private static bool DoIsValidName(string name, RusFullNameValidateOptions options, out string errorText, int charOff)
    {
      if (String.IsNullOrEmpty(name))
      {
        errorText = "Имя должно быть задано";
        return false;
      }

      if (!TestPart(name, options, out errorText, RusFullNamePart.Name, charOff))
        return false;


      switch (options & (RusFullNameValidateOptions.InitialsAllowed | RusFullNameValidateOptions.InitialsOnly))
      {
        case RusFullNameValidateOptions.Default:
          if (name.Length == 1)
          {
            errorText = "Имя должно быть задано полностью, а не инициалами";
            return false;
          }
          break;
        case RusFullNameValidateOptions.InitialsOnly:
          if (name.Length > 1)
          {
            errorText = "Имя должно быть задано инициалами, а не полностью";
            return false;
          }
          break;
      }
      return true;
    }

    /// <summary>
    /// Выполнить проверку отчества.
    /// Пустое отчество считается корректным значением
    /// </summary>
    /// <param name="patronymic">Проверяемое значение</param>
    /// <param name="options">Опции проверки</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true, если значение корректно</returns>
    public static bool IsValidPatronymic(string patronymic, RusFullNameValidateOptions options, out string errorText)
    {
      if (!IsValidGeneral(patronymic, out errorText))
        return false;
      return DoIsValidPatronymic(patronymic, options, out errorText, 0);
    }

    /// <summary>
    /// Выполнить проверку отчества.
    /// Пустое отчество считается корректным значением
    /// </summary>
    /// <param name="patronymic">Проверяемое значение</param>
    /// <param name="options">Опции проверки</param>
    /// <returns>true, если значение корректно</returns>
    public static bool IsValidPatronymic(string patronymic, RusFullNameValidateOptions options)
    {
      string errorText;
      return IsValidPatronymic(patronymic, options, out errorText);
    }

    private static bool DoIsValidPatronymic(string patronymic, RusFullNameValidateOptions options, out string errorText, int charOff)
    {
      if (String.IsNullOrEmpty(patronymic))
      {
        errorText = null;
        return true;
      }
      if (!TestPart(patronymic, options, out errorText, RusFullNamePart.Patronymic, 0))
        return false;


      switch (options & (RusFullNameValidateOptions.InitialsAllowed | RusFullNameValidateOptions.InitialsOnly))
      {
        case RusFullNameValidateOptions.Default:
          if (patronymic.Length == 1)
          {
            errorText = "Отчество должно быть задано полностью, а не инициалами";
            return false;
          }
          break;
        case RusFullNameValidateOptions.InitialsOnly:
          if (patronymic.Length > 1)
          {
            errorText = "Отчество должно быть задано инициалами, а не полностью";
            return false;
          }
          break;
      }
      return true;

    }

    private static bool TestPart(string s, RusFullNameValidateOptions options, out string errorText, RusFullNamePart part, int charOff)
    {
      bool wantsUpper = true;
      bool wantsLower = false;
      bool hasSpace = false;
      for (int i = 0; i < s.Length; i++)
      {
        bool thisIsUpper;
        if (RussianTools.UpperRussianCharIndexer.Contains(s[i]))
          thisIsUpper = true;
        else if (RussianTools.LowerRussianCharIndexer.Contains(s[i]))
          thisIsUpper = false;
        else
        {
          switch (s[i])
          {
            case '-':
              // Убрано 09.12.2020
              // Дефис может быть везде.
              // Например: Гилагаев Хас-Магомед Саид-Магомедович
              // https://ru.wikipedia.org/wiki/Гилагаев,_Хас-Магомед_Саид-Магомедович

              //if (codeType != "Ф" && codeType != "О")
              //{
              //  errorText = "Символы \"-\" допускается только в фамилии или отчестве";
              //  return false;
              //}
              if (i == 0 || i == s.Length - 1)
              {
                errorText = "Символ \"-\" не может быть в начале или в конце компонента Ф.И.О.";
                return false;
              }
              if (s[i - 1] == '-')
              {
                errorText = "Два символа \"--\" подряд в позиции " + (i + charOff + 1).ToString();
                return false;
              }
              wantsUpper = true;
              wantsLower = true;
              break;

            case ' ':
              // Убрано 15.04.2021
              //if (codeType != "О")
              //{
              //  errorText = "Пробелы допускаются только в отчестве";
              //  return false;
              //}
              if (i == 0 || i == s.Length - 1)
              {
                errorText = "Пробел не может быть в начале или в конце отчества";
                return false;
              }
              if (hasSpace)
              {
                errorText = "Допускается только один пробел";
                return false;
              }
              hasSpace = true;
              wantsUpper = true;
              wantsLower = true;
              break;

            default:
              errorText = "Недопустимый символ \"" + s[i] + "\" в позиции " + (i + charOff + 1).ToString();
              return false;
          }
          continue;
        } // не буква

        if (wantsUpper)
        {
          if ("ЪЬ".IndexOf(s[i]) >= 0) // 16.11.2020
          {
            errorText = "Слово не может начинаться с твердого или мягкого знака. Позиция " + (i + charOff + 1).ToString();
            return false;
          }
        }

        if (thisIsUpper)
        {
          if ((!wantsUpper) && (options & RusFullNameValidateOptions.IgnoreCases) == 0)
          {
            errorText = "Ожидалась буква в нижнем регистре в позиции " + (i + charOff + 1).ToString();
            return false;
          }
          wantsUpper = false;
          wantsLower = true;
        }
        else
        {
          if ((!wantsLower) && (options & RusFullNameValidateOptions.IgnoreCases) == 0)
          {
            errorText = "Ожидалась буква в верхнем регистре в позиции " + (i + charOff + 1).ToString();
            return false;
          }
        }
      }


      // 15.04.2021
      // Окончания могут быть и в фамилии и в имени
      //if (codeType == "О")
      //{
      // Убрано 09.12.2020. См. выше.
      // string s1 = s.Replace('-', ' ');
      if (s.IndexOf(' ') >= 0)
      {
        if (!(s.EndsWith(" КЫЗЫ", StringComparison.OrdinalIgnoreCase) ||
          s.EndsWith(" ГЫЗЫ", StringComparison.OrdinalIgnoreCase) ||  // добавлено 15.04.2021
          s.EndsWith(" ОГЛЫ", StringComparison.OrdinalIgnoreCase)))
        {
          errorText = "Текст с пробелом может заканчиваться только на \"Кызы\", \"Гызы\" или \"Оглы\"";
          return false;
        }
      }
      //}

      switch (part)
      {
        case RusFullNamePart.Surname:
          if (s.Length < 2)
          {
            errorText = "Длина фамилии не может быть меньше двух символов";
            return false;
          }
          break;

        // Имя может быть любой длины, например, "Ян".

        case RusFullNamePart.Patronymic:
          if (s.Length > 1 && s.Length < 4)
          {
            errorText = "Длина отчества не может быть меньше 4 символов";
            return false;
          }
          break;
      }

      errorText = null;
      return true;
    }

    private static string GetErrorPrefix(RusFullNamePart part, int i)
    {
      string partName = GetPartDisplayName(part);

      return partName + ", позиция " + (i + 1).ToString() + ". ";
    }

    private static string GetPartDisplayName(RusFullNamePart part)
    {
      switch (part)
      {
        case RusFullNamePart.Surname: return "Фамилия";
        case RusFullNamePart.Name: return "Имя";
        case RusFullNamePart.Patronymic: return "Отчество";
        default: throw ExceptionFactory.ArgUnknownValue("part", part);
      }
    }

    #endregion

    #endregion
  }
}
