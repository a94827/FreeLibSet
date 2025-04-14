// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Russian
{
  #region Перечисления

  #region Род

  /// <summary>
  /// Род имени существительного
  /// </summary>
  public enum RusGender
  {
    /// <summary>
    /// Мужской род
    /// </summary>
    Masculine = 0,

    /// <summary>
    /// Женский род
    /// </summary>
    Feminine = 1,

    /// <summary>
    /// Средний род
    /// </summary>
    Newter = 2,

    /// <summary>
    /// Род не определен
    /// </summary>
    Undefined = -1
  }

  #endregion

  #region Падеж

  /// <summary>
  /// Падеж имени существительного
  /// </summary>
  public enum RusCase
  {
    /// <summary>
    /// именительный падеж
    /// </summary>
    Nominative = 0,

    /// <summary>
    /// родительный падеж
    /// </summary>
    Genitive = 1,

    /// <summary>
    /// дательный падеж
    /// </summary>
    Dative = 2,

    /// <summary>
    /// винительный падеж
    /// </summary>
    Accusative = 3,

    /// <summary>
    /// творительный падеж
    /// </summary>
    Instrumental = 4,

    /// <summary>
    /// предложный падеж
    /// </summary>
    Locative = 5,

    /// <summary>
    /// Падеж не определен
    /// </summary>
    Undefined = -1
  }

  #endregion

  #region Число

  /// <summary>
  /// Число имени сущесвтительного
  /// </summary>
  public enum RusNumber
  {
    /// <summary>
    /// Единственное число
    /// </summary>
    Singular = 0,

    /// <summary>
    /// Множественное число
    /// </summary>
    Plural = 1,

    /// <summary>
    /// Не определено
    /// </summary>
    Undefined = -1
  }

  #endregion

  #region Склонение имени существительного

  /// <summary>
  /// Склонение существительного
  /// https://ru.wikipedia.org/wiki/%D0%A1%D0%BA%D0%BB%D0%BE%D0%BD%D0%B5%D0%BD%D0%B8%D0%B5_(%D0%BB%D0%B8%D0%BD%D0%B3%D0%B2%D0%B8%D1%81%D1%82%D0%B8%D0%BA%D0%B0)#%D0%A1%D0%BA%D0%BB%D0%BE%D0%BD%D0%B5%D0%BD%D0%B8%D0%B5_%D0%B8%D0%BC%D1%91%D0%BD_%D1%81%D1%83%D1%89%D0%B5%D1%81%D1%82%D0%B2%D0%B8%D1%82%D0%B5%D0%BB%D1%8C%D0%BD%D1%8B%D1%85_%D0%B2_%D1%80%D1%83%D1%81%D1%81%D0%BA%D0%BE%D0%BC_%D1%8F%D0%B7%D1%8B%D0%BA%D0%B5
  /// </summary>
  public enum RusDeclension
  {
    // TODO: Добавить адъективное склонение, несклоняемые, разносклоняемые существительные. 
    // См. статью в Википедии

    /// <summary>
    /// I склонение — имена существительные женского, мужского рода, 
    /// имеющие в именительном падеже единственного числа окончание -а (-я).
    /// </summary>
    First = 1,

    /// <summary>
    /// II склонение — имена существительные мужского рода с нулевым окончанием или окончанием -о (-е) и 
    /// имена существительные среднего рода с окончанием -о (-е) в именительном падеже единственного числа.
    /// </summary>
    Second = 2,

    /// <summary>
    /// III склонение — имена существительные женского рода с окончанием -ь в именительном падеже единственного числа, 
    /// с основой на мягкий знак.
    /// </summary>
    Third = 3,

    /// <summary>
    /// Не определено или несклоняемое
    /// </summary>
    Undefined = -1
  }

  #endregion

  #region Перечисление RusFormArrayGetCasesOptions

  /// <summary>
  /// Опции для метода RusFormArray.GetCases()
  /// </summary>
  [FlagsAttribute]
  public enum RusFormArrayGetCasesOptions
  {
    /// <summary>
    /// Режим по умолчанию
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Cклонять слова на "ова"/"ева"
    /// </summary>
    Which = 0x1,

    /// <summary>
    /// не склонять слова на "ы"
    /// </summary>
    NoPlural = 0x2,

    /// <summary>
    /// Задается для имен, чтобы правильно склонялись имена типа "Дмитрий", "Василий"
    /// </summary>
    Name = 0x4,

    /// <summary>
    /// Одушевленность.
    /// Определяет винительный падеж во втором склонении:
    /// Есть чебурек - вижу чебурек.
    /// Есть человек - вижу человека.
    /// </summary>
    Animacy = 0x8,
  }

  #endregion

  #endregion

  /// <summary>
  /// Массив для хранения 12 форм имени существительного (6 падежей для единственного и множественного чисел).
  /// Используйте метод <see cref="Fill(string, RusGender, RusFormArrayGetCasesOptions)"/> для заполнения массива. 
  /// Допускается изменение элементов после заполнения.
  /// </summary>
  public class RusNounFormArray
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект
    /// </summary>
    public RusNounFormArray()
    {
      _Items = new string[12];
    }

    /// <summary>
    /// Создает объект. Все элементы получают одинаковые значения.
    /// Аналогично вызову метода <see cref="Fill(string)"/>.
    /// </summary>
    /// <param name="mainForm">Используемое значение</param>
    public RusNounFormArray(string mainForm)
    {
      _Items = new string[12];
      for (int i = 0; i < 12; i++)
        _Items[i] = mainForm;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Чтение или запись одного значения
    /// </summary>
    /// <param name="theCase">Падеж</param>
    /// <param name="theNumber">Число (единственное или множественное)</param>
    /// <returns>Значение</returns>
    public string this[RusCase theCase, RusNumber theNumber]
    {
      get
      {
        return _Items[GetIndex(theCase, theNumber)];
      }
      set
      {
        _Items[GetIndex(theCase, theNumber)] = value;
      }
    }

    private static int GetIndex(RusCase theCase, RusNumber theNumber)
    {
      int n1 = (int)theCase;
      if (n1 < 0 || n1 > 5)
        throw ExceptionFactory.ArgUnknownValue("theCase", theCase);
      int n2 = (int)theNumber;
      if (n2 < 0 || n2 > 1)
        throw ExceptionFactory.ArgUnknownValue("theNumber", theNumber);

      return n1 * 6 + n2;
    }

    /// <summary>
    /// Чтение или запись массива значений.
    /// </summary>
    public string[] Items
    {
      get
      {
        return _Items;
      }
      set
      {
        if (value.Length != 12)
          throw new ArgumentException("Должен быть массив из 12 элементов", "value");
        _Items = (string[])(value.Clone());
      }
    }
    private string[] _Items;

    #endregion

    #region Методы

    /// <summary>
    /// Заполняет все элементы массива одинаковым значением
    /// </summary>
    /// <param name="baseForm">Значение</param>
    public void Fill(string baseForm)
    {
      for (int i = 0; i < 12; i++)
        _Items[i] = baseForm;
    }

    /// <summary>
    /// Основной метод.
    /// Заполняет все элементы массива на основании исходной формы.
    /// Первый элемент массива получает значение <paramref name="baseForm"/>. 
    /// Остальные элементы получают элементы с измененными окончаниями в соответствии с правилами русского языка.
    /// </summary>
    /// <param name="baseForm">Базовая форма слова (именительный падеж единственного числа)</param>
    /// <param name="gender">Род слова</param>
    /// <param name="options">Параметры</param>
    /// <returns>True, если склонение слова выполнено успешно</returns>
    public bool Fill(string baseForm, RusGender gender, RusFormArrayGetCasesOptions options)
    {
      // Разбиваем на части
      List<String> aParts;
      List<Boolean> aFlags;


      DoBreakWord(baseForm, out aParts, out aFlags);
      int i, j;
      bool lRes = true;

      // Собираем обратно
      Fill("");
      RusNounFormArray forms2 = new RusNounFormArray();
      for (i = 0; i < aParts.Count; i++)
      {
        if (aFlags[i])
        {
          // преобразование части
          if (!forms2.GetCases1(aParts[i], gender, options))
            lRes = false;
          for (j = 0; j < 12; j++)
            _Items[j] += forms2._Items[j];
        }
        else
        {
          // добавление части без преобразования
          for (j = 0; j < 12; j++)
            _Items[j] += aParts[i];
        }
      }
      return lRes;
    }

    private void DoBreakWord(string baseForm, out List<string> aParts, out List<bool> aFlags)
    {
      aParts = new List<string>();
      aFlags = new List<bool>();

      if (String.IsNullOrEmpty(baseForm))
        return;

      string upperForm = baseForm.ToUpperInvariant();

      int startPos = -1;
      bool prevFlag = false; // иначе предупреждение

      for (int thisPos = 0; thisPos < baseForm.Length; thisPos++)
      {
        char c = upperForm[thisPos];

        // Является ли очередной символ русской буквой
        //bool ThisFlag = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ".IndexOf(c) >= 0;
        bool thisFlag = RussianTools.IsUpperRussianChar(c); // 18.06.2020

        if (thisPos == 0)
        {
          startPos = 0;
          prevFlag = thisFlag;
        }
        else
        {
          if (thisFlag != prevFlag)
          {
            // Смена части
            aParts.Add(baseForm.Substring(startPos, thisPos - startPos));
            aFlags.Add(prevFlag);
            startPos = thisPos;
            prevFlag = thisFlag;
          }
        }
      }

      // Добавляем последнюю часть
      aParts.Add(baseForm.Substring(startPos, baseForm.Length - startPos));
      aFlags.Add(prevFlag);
    }

    private bool GetCases1(string baseForm, RusGender gender, RusFormArrayGetCasesOptions options)
    {
      string upperForm = baseForm.ToUpperInvariant();
      bool isUpperCase = (baseForm == upperForm); // Все буквы заглавные ?
      int i;


      Fill(baseForm);

      if (String.IsNullOrEmpty(baseForm))
        return false;

      if (baseForm.Length < 2)
        return false;

      if (upperForm.IndexOfAny(new char[] { 'А', 'Е', 'Ё', 'И', 'О', 'У', 'Ы', 'Э', 'Ю', 'Я' }) < 0)
        // слово не содержит гласных букв
        return false;

      // Последний символ
      char c = upperForm[upperForm.Length - 1];

      // Склонение
      RusDeclension declen;
      if (c == 'Ы' && (options & RusFormArrayGetCasesOptions.NoPlural) == RusFormArrayGetCasesOptions.NoPlural)
        // Склонения для слов типа "ножницы" отключается
        declen = RusDeclension.Undefined;
      else
        declen = GetDeclension(baseForm, gender);

      if (declen == RusDeclension.Undefined)
        // не склоняется
        return false;

      // Разбиение слова на части
      string cBase; // Приставка+корень+суффикс
      string[] aEnds; // Окончания (12 форм)
      DoGetCases(baseForm, upperForm, declen, options, out cBase, out aEnds);


      if (aEnds != null)
      {
        // Добавляем окончания
        for (i = 0; i < 12; i++)
          _Items[i] = cBase + aEnds[i];
        if (isUpperCase)
        {
          for (i = 0; i < 12; i++)
            _Items[i] = _Items[i].ToUpperInvariant();
        }
      }
      return true;
    }

    private static void DoGetCases(string baseForm, string upperForm, RusDeclension declension,
      RusFormArrayGetCasesOptions options,
      out string cBase, out string[] aEnds)
    {
      cBase = null;
      aEnds = null;

      switch (declension)
      {
        #region Первое склонение

        case RusDeclension.First:
          if (upperForm.EndsWith("ЬЯ", StringComparison.Ordinal)) // свинья
          {
            // в отличие от слов на "я" без мягкого знака, в родительном
            // и дательном падежах множественного числа мягкий знак отбрасывается
            // свинья -> свиньи -> свиней
            // фигня  -> фигни  -> фигней
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ья"
            aEnds = new string[12]{"ья","ьи" ,"ье" , "ью", "ьей"  ,"ье",
                                   "ьи","ей" ,"ьям", "ей", "ьями" ,"ьях"};
            return;
          }
          if (upperForm.EndsWith("АЯ", StringComparison.Ordinal))
          {
            // прилагательное (красная)
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ая"
            aEnds = new string[12]{"ая", "ой" ,"ой" ,"ую","ой" ,"ой",
                                   "ые","ых","ым","ых","ыми","ых"};
            return;
          }

          if (upperForm.EndsWith("ИЯ", StringComparison.Ordinal)) // имена ("Юлия")
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ия"
            aEnds = new string[12]{"ия","ии" ,"ии" ,"ию","ией" ,"ии",
                                   "ии","ией","иям","ии","иями","иях"};
            return;
          }

          if (upperForm.EndsWith("ЯЯ", StringComparison.Ordinal)) // прилагательное (синяя)
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "яя"
            aEnds = new string[12]{"яя","ей" ,"ей" ,"юю","ей" ,"ей",
                                   "ие","их","им","их","ими","их"};
            return;
          }
          if (upperForm.EndsWith("Я", StringComparison.Ordinal))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "я"
            aEnds = new string[12]{"я","и" ,"е" ,"ю","ей" ,"е",
                                   "и","ей","ям","ей","ями","ях"};
            return;

          }
          if ((upperForm.EndsWith("ОВА", StringComparison.Ordinal) ||
            upperForm.EndsWith("ЕВА", StringComparison.Ordinal) ||
            upperForm.EndsWith("ИНА", StringComparison.Ordinal) ||
            upperForm.EndsWith("ЫНА", StringComparison.Ordinal)) &&
            ((options & RusFormArrayGetCasesOptions.Which) == RusFormArrayGetCasesOptions.Which))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "а"
            aEnds = new string[12]{"а","ой" ,"ой" ,"у","ой" ,"ой",
                                   "ы","ых","ым","ых","ыми","ых"};
            return;
          }
          if (upperForm.EndsWith("ЙКА", StringComparison.Ordinal))  // гайка, чайка
          {
            // В родительном и винительном падежах множественного числа
            // исчезает "Й"
            cBase = baseForm.Substring(0, baseForm.Length - 3);// Отбросили "йка"
            aEnds = new string[12]{"йка","йки" ,"йке" ,"йку","йкой" ,"йке",
                                   "йки","ек","йкам","ек","йками","йках"};
            return;
          }
          if (upperForm.EndsWith("ЧКА", StringComparison.Ordinal))  // тачка, булочка
          {
            // В родительном и винительном падежах множественного числа
            // появоятся буква "е"
            cBase = baseForm.Substring(0, baseForm.Length - 3);// Отбросили "чка"
            //(можно было и 2 буквы отбросить)
            aEnds = new string[12]{"чка","чки" ,"чке" ,"чку","чкой" ,"чке",
                    "чки","чек","чкам","чек","чками","чках"};
            return;
          }
          if (baseForm.EndsWith("ВКА", StringComparison.Ordinal))  // морковка
          {
            // В родительном и винительном падежах множественного числа
            // добавляется буква "о": морковка->морковок, а не морковк
            cBase = baseForm.Substring(0, baseForm.Length - 3);// Отбросили "вка"
            //(можно было и 2 буквы отбросить)
            aEnds = new string[12]{"вка","вки" ,"вке" ,"вку","вкой" ,"вке",
                                   "вки","вок","вкам","вки","вками","вках"};
            return;
          }

#pragma warning disable 0162 // Unreachable code detected

          if (upperForm.EndsWith("А", StringComparison.Ordinal))
          {
            if (false /*21.02.2024*/ /*upperForm.Length > 3 && upperForm[upperForm.Length - 3] == 'Ь' &&
                 "БВГДЖЗКЛМНПРСТФХЦЧШЩ".IndexOf(upperForm[upperForm.Length - 2]) >= 0*/)
            {
              // ??? сомнительное правило
              // в отличие от других слов на "а" изменены родительный и
              // винительный падежи единственного числа
              cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "а"
              aEnds = new string[12]{"а","и" ,"е" ,"у","ой" ,"е",
                                     "и","","ам","","ами","ах"};
            }
            else
            {
              // в родительном падеже единственного числа может идти либо
              // окончание "и" либо "ы".
              // гора -> горы
              // рука -> руки
              cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "а"
              if ("ГКХЧШЩ".IndexOf(upperForm[upperForm.Length - 2]) >= 0)
                aEnds = new string[12]{"а","и" ,"е" ,"у","ой" ,"е",
                                       "и","","ам","и","ами","ах"};
              else
                aEnds = new string[12]{"а","ы" ,"е" ,"у","ой" ,"е",
                                       "ы","","ам","ы","ами","ах"};
            }
            return;
          }

#pragma warning restore 0162 

          if (upperForm.EndsWith("Ы", StringComparison.Ordinal)) // "ножницы"
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "ы"
            aEnds = new string[12]{"ы", "", "ам", "ы", "амм"  ,"ах",
                                   "ы", "", "ам", "ы", "ами" , "ах"};
          }
          break;

        #endregion

        #region Второе склонение

        case RusDeclension.Second:
          if (upperForm.EndsWith("Ь", StringComparison.Ordinal)) // конь, пень
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "ь"
            aEnds = new string[12]{"ь","я" ,"ю" ,"я","ем" ,"е",
                                   "и","ей","ям","ей","ями","ях"};
            return;
          }
          if (upperForm.EndsWith("ЫЙ", StringComparison.Ordinal))  // прилагательные ("главный")
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ый"
            aEnds = new string[12]{"ый","ого" ,"ому" ,"ого","ым" ,"ом",
                                   "ые","ых","ым","ых","ыми","ых"};
            return;
          }

          if (upperForm.EndsWith("ИЙ", StringComparison.Ordinal))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ий"
            if ((options & RusFormArrayGetCasesOptions.Name) == RusFormArrayGetCasesOptions.Name)
              // Имена "Василий", "Дмитрий"
              aEnds = new string[12]{"ий","ия" ,"ию" ,"ия","ием" ,"ие",
                                     "ии", "иев", "иям", "иев", "иями", "иях"};
            else
            // прилагательные ("ближний"-"ближнего")
            // , но            "близкий"-"близкого"  
            {
              char c3 = ' ';
              if (upperForm.Length > 2)
                c3 = upperForm[upperForm.Length - 3];

              if ("БВДЗЛМПРСТЦ".IndexOf(c3) >= 0) // 05.11.2024
              {
                // ниобий
                // гравий 
                // иридий
                // цезий
                // калий
                // кадмий
                // виночерпий
                // барий
                // лоуренсий
                // литий (но третий - будет ошибка)
                // морфий
                // нунций
                if ((options & RusFormArrayGetCasesOptions.Animacy) == RusFormArrayGetCasesOptions.Animacy) 
                  // такая же форма, как для имени
                  aEnds = new string[12]{"ий", "ия", "ию", "ия", "ием", "ии",
                                         "ии", "иев", "иями", "иев", "иями", "иях"};
                else
                  aEnds = new string[12]{"ий", "ия", "ию", "ий", "ием", "ии",
                                         "ии", "иев", "иями", "иев", "иями", "иях"};
              }
              else if ("ГКХ".IndexOf(c3) >= 0) // 30.10.2009
                // убогий
                // узкий
                // тихий
                aEnds = new string[12]{"ий", "ого", "ому", "ого", "им","ом",
                                       "ие", "их", "им", "их", "ими", "их"};
              else if (c3 == 'Н')
              {
                // 05.11.2024
                // Тут непонятно:
                // гений -> гения (существительное)
                // зимний -> зимнего (прилагательное)

                // Так как существительных меньше, чем прилагательных, используем вторую форму
                aEnds = new string[12]{"ий", "его", "ему", "его", "им", "ем",
                                     "ие", "их", "им", "их", "ими", "их"};
              }
              else // "ЕЖЧШЩ"
                // тонкошеий
                // рыжий
                // лежачий
                // леший
                // вещий
                aEnds = new string[12]{"ий", "его", "ему", "его", "им", "ем",
                                     "ие", "их", "им", "их", "ими", "их"};
            }
            return;
          }
          if (upperForm.EndsWith("ЕЙ", StringComparison.Ordinal))
          {
            // Отличается винительным падежом множественного числа
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ей"
            aEnds = new string[12]{"ей","ея" ,"ею" ,"ея","еем" ,"ее",
                                   "еи","еев","еям","еев","еями","еях"};
            return;
          }
          if (upperForm.EndsWith("ОЙ", StringComparison.Ordinal))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ой"
            aEnds = new string[12]{"ой","ого" ,"ому" ,"ого","ым" ,"ом",
                                   "ые","ых","ым","ых","ыми","ых"};
            return;
          }
          if (upperForm.EndsWith("АЙ", StringComparison.Ordinal))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ай"
            aEnds = new string[12]{"ай","ая" ,"аю" ,"ая","аем" ,"ае",
                                   "аи","аев","аями","аев","аями","аях"};
            return;
          }
          if (upperForm.EndsWith("Й", StringComparison.Ordinal)) // буй
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "й"
            aEnds = new string[12]{"й","я" ,"ю" ,"й","ем" ,"е",
                                   "и","ев","ям","и","ями","ях"};
            return;
          }
          if (upperForm.EndsWith("Е", StringComparison.Ordinal)) // солнце
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "е"
            // В зависимости от предпоследней буквы, в родительном (и др.)
            // падеже окончание может быть "а" или "я"
            // и: соглашение -> соглашения
            // л: поле -> поля
            // р: море -> моря
            // ц: солнце -> солнца
            if ("ИЛР".IndexOf(upperForm[upperForm.Length - 2]) >= 0)
              aEnds = new string[12]{"е","я" ,"ю" ,"е","ем" ,"е",
                                     "я","ей","ям","я","ями","ях"};
            else
              aEnds = new string[12]{"е","а" ,"у" ,"е","ем" ,"е",
                                     "а","","ам","а","ами","ах"};
            return;
          }
          if (upperForm.EndsWith("О", StringComparison.Ordinal)) // окно
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "о"
            aEnds = new string[12]{"о","а" ,"у" ,"о","ом" ,"е",
                                   "а","","ам","а","ами","ах"};
            return;
          }
          if (upperForm.EndsWith("НЕЦ", StringComparison.Ordinal))
          {
            // В таких словах ("конец") исчезает буква "е"
            cBase = baseForm.Substring(0, baseForm.Length - 3);// Отбросили "нец"
            aEnds = new string[12]{"нец","нца" ,"нцу" ,"нец","нцом" ,"нце",
                                   "нцы","нцов","нцам","нцы","нцами","нцах"};
            return;
          }
          if (upperForm.EndsWith("ОВ", StringComparison.Ordinal) ||
            upperForm.EndsWith("ЕВ", StringComparison.Ordinal))
          {
            cBase = baseForm; // ничего не отбрасываем
            aEnds = new string[12]{""  ,"а" ,"у" ,"а" ,"ым" ,"е",
                                   "ы","ых","ым","ых","ыми","ых"};
            return;
          }
          if (upperForm.EndsWith("ИН", StringComparison.Ordinal) ||
            upperForm.EndsWith("ЫН", StringComparison.Ordinal))
          {
            cBase = baseForm; // ничего не отбрасываем
            aEnds = new string[12]{""  ,"а" ,"у" ,"а" ,"ым" ,"е",
                                   "ы","ых","ым","ых","ами","ых"};
            return;
          }
          if (upperForm.EndsWith("ИЧ", StringComparison.Ordinal) ||
            upperForm.EndsWith("ЫЧ", StringComparison.Ordinal))
          {
            cBase = baseForm; // ничего не отбрасываем
            aEnds = new string[12]{""  ,"а" ,"у" ,"а" ,"ем" ,"е",
                                   "и","ей","ам","ей","ами","ах"};
            return;
          }
          if (upperForm.EndsWith("ОК", StringComparison.Ordinal)) // горшок, совок, пучок
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ок"
            aEnds = new string[12]{"ок", "ка" ,"ку" ,"ок","ком" ,"ке",
                                   "ки", "ков","кам","ки","ками","ках"};
            return;
          }

          if (upperForm.EndsWith("ВЕЛ", StringComparison.Ordinal) &&
            (options & RusFormArrayGetCasesOptions.Name) == RusFormArrayGetCasesOptions.Name)
          {
            // 04.02.2024 В имени "Павел" исчезает буква "е"
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ел"
            aEnds = new string[12]{"ел", "ла" ,"лу" ,"ла","лом" ,"ле",
                                   "лы", "лов","лам", "лов","лами","лах"};
            return;
          }

          /**************
                   CASE s2=="ЕК" .AND. AT(LEFT(s3,1), "НРЛ")>0 // конек
                      // Вообще-то записит от наличия буквы "ё" или "е"
                      // хорек -> хорька  (буква "ё")
                      // берег -> берега  (буква "е")
                      // К сожадению, такие слова отличить невозможно
                      cBase:=LEFT(cWord, LEN(cWord)-2) // Отбросили "ек"
                      aEnds:={"ек", "ька" ,"ьку" ,"ька","ьком" ,"ьке",;
                              "ьки", "ьков","ькам","ьки","ьками","ьках"}
          ***************/

          if (upperForm.EndsWith("Ы", StringComparison.Ordinal)) // "щипцы"
          {
            // ("ножницы" относятся к 1-му склонению, а "щипцы" - ко второму):
            // ножницы - ножниц
            // щипцы   - ципцов (а не щипц)
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "ы"
            aEnds = new string[12]{"ы", "ов", "ам", "ы", "амм"  ,"ах",
                                   "ы", "ов", "ам", "ы", "ами" , "ах"};
            return;
          }
          // Остальные слова второго склонения: дом, стол
          cBase = baseForm; // ничего не отбрасываем
          // ???? Винительный падеж ед.ч.
          // Дом -> Дом
          // Иван -> Ивана
          aEnds = new string[12]{""  ,"а" ,"у" ,"а" ,"ом" ,"е",
                                 "ы","ов","ам","ов","ами","ах"};
          return;

        #endregion

        #region Третье склонение

        case RusDeclension.Third:
          // дательный падеж мн.числа "ам" или "ям"
          // ночь -> ночам
          // мышь -> мышам
          // тень -> теням
          if (upperForm.EndsWith("ЧЬ", StringComparison.Ordinal) ||
            upperForm.EndsWith("ШЬ", StringComparison.Ordinal) ||
            upperForm.EndsWith("ЩЬ", StringComparison.Ordinal) ||
            upperForm.EndsWith("ЖЬ", StringComparison.Ordinal))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "ь"
            aEnds = new string[12]{"ь","и" ,"и" ,"ь","ью" ,"и",
                                   "и","ей","ам","и","ами","ах"};
            return;
          }
          cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "ь"
          aEnds = new string[12]{"ь","и" ,"и" ,"ь","ью" ,"и",
                                 "и","ей","ям","и","ями","ях"};
          return;

          #endregion
      }
    }

    private static RusDeclension GetDeclension(string baseForm, RusGender gender)
    {
      if (String.IsNullOrEmpty(baseForm) || baseForm.Length < 2)
        return RusDeclension.Undefined;
      // Последний символ
      char c = baseForm.ToUpperInvariant()[baseForm.Length - 1];

      if (gender == RusGender.Undefined)
        // Если род слова не задан
        gender = DoGetGender(c);

      // Род слова определен - определяем склонение
      switch (gender)
      {
        case RusGender.Masculine:
          if ("АЯ".IndexOf(c) >= 0)
            return RusDeclension.First;
          if ("БВГДЖЗЙКЛМНПРСТФХЦЧШЩЬЫ".IndexOf(c) >= 0)
            return RusDeclension.Second;
          break;

        case RusGender.Feminine:
          if ("АЯЫ".IndexOf(c) >= 0)
            return RusDeclension.First;
          if ("Ь".IndexOf(c) >= 0)
            return RusDeclension.Third;
          break;
        case RusGender.Newter:
          return RusDeclension.Second;
      }
      return RusDeclension.Undefined;
    }

    private static RusGender DoGetGender(char c)
    {
      if ("БВГДЖЗЙКЛМНПРСТФХЧШЩ".IndexOf(c) >= 0)
        return RusGender.Masculine;
      if ("АЯ".IndexOf(c) >= 0)
        return RusGender.Feminine;
      if ("ОЕЭЮ".IndexOf(c) >= 0)
        return RusGender.Newter;
      return RusGender.Undefined;
    }

    #endregion
  }
}
