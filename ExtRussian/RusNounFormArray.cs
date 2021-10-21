using System;
using System.Collections.Generic;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
  }

  #endregion

  #endregion

  /// <summary>
  /// Массив для хранения 12 форм имени существительного
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
    /// Создает объект. Все элементы получают одинаковые значения
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
        throw new ArgumentOutOfRangeException("theCase", theCase, "Неправильный род");
      int n2 = (int)theNumber;
      if (n2 < 0 || n2 > 1)
        throw new ArgumentOutOfRangeException("theNumber", theNumber, "Неправильное число");

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
    public bool GetCases(string baseForm, RusGender gender, RusFormArrayGetCasesOptions options)
    {
      // Разбиваем на части
      List<String> aParts;
      List<Boolean> aFlags;


      MyBreakWord(baseForm, out aParts, out aFlags);
      int i, j;
      bool lRes = true;

      // Собираем обратно
      Fill("");
      RusNounFormArray Forms2 = new RusNounFormArray();
      for (i = 0; i < aParts.Count; i++)
      {
        if (aFlags[i])
        {
          // преобразование части
          if (!Forms2.GetCases1(aParts[i], gender, options))
            lRes = false;
          for (j = 0; j < 12; j++)
            _Items[j] += Forms2._Items[j];
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

    private void MyBreakWord(string baseForm, out List<string> aParts, out List<bool> aFlags)
    {
      aParts = new List<string>();
      aFlags = new List<bool>();

      if (String.IsNullOrEmpty(baseForm))
        return;

      string UpperForm = baseForm.ToUpperInvariant();

      int StartPos = -1;
      bool PrevFlag = false; // иначе предупреждение

      for (int ThisPos = 0; ThisPos < baseForm.Length; ThisPos++)
      {
        char c = UpperForm[ThisPos];

        // Является ли очередной символ русской буквой
        //bool ThisFlag = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ".IndexOf(c) >= 0;
        bool ThisFlag = RussianTools.IsUpperRussianChar(c); // 18.06.2020

        if (ThisPos == 0)
        {
          StartPos = 0;
          PrevFlag = ThisFlag;
        }
        else
        {
          if (ThisFlag != PrevFlag)
          {
            // Смена части
            aParts.Add(baseForm.Substring(StartPos, ThisPos - StartPos));
            aFlags.Add(PrevFlag);
            StartPos = ThisPos;
            PrevFlag = ThisFlag;
          }
        }
      }

      // Добавляем последнюю часть
      aParts.Add(baseForm.Substring(StartPos, baseForm.Length - StartPos));
      aFlags.Add(PrevFlag);
    }

    private bool GetCases1(string baseForm, RusGender gender, RusFormArrayGetCasesOptions options)
    {
      string UpperForm = baseForm.ToUpperInvariant();
      bool IsUpperCase = (baseForm == UpperForm); // Все буквы заглавные ?
      int i;


      Fill(baseForm);

      if (String.IsNullOrEmpty(baseForm))
        return false;

      if (baseForm.Length < 2)
        return false;

      if (UpperForm.IndexOfAny(new char[] { 'А', 'Е', 'Ё', 'И', 'О', 'У', 'Ы', 'Э', 'Ю', 'Я' }) < 0)
        // слово не содержит гласных букв
        return false;

      // Последний символ
      char c = UpperForm[UpperForm.Length - 1];

      // Склонение
      RusDeclension Scl;
      if (c == 'Ы' && (options & RusFormArrayGetCasesOptions.NoPlural) == RusFormArrayGetCasesOptions.NoPlural)
        // Склонения для слов типа "ножницы" отключается
        Scl = RusDeclension.Undefined;
      else
        Scl = GetDeclension(baseForm, gender);

      if (Scl == RusDeclension.Undefined)
        // не склоняется
        return false;

      // Разбиение слова на части
      string cBase; // Приставка+корень+суффикс
      string[] aEnds; // Окончания (12 форм)
      MyGetCases(baseForm, UpperForm, Scl, options, out cBase, out aEnds);


      if (aEnds != null)
      {
        // Добавляем окончания
        for (i = 0; i < 12; i++)
          _Items[i] = cBase + aEnds[i];
        if (IsUpperCase)
        {
          for (i = 0; i < 12; i++)
            _Items[i] = _Items[i].ToUpperInvariant();
        }
      }
      return true;
    }

    private static void MyGetCases(string baseForm, string upperForm, RusDeclension declension,
      RusFormArrayGetCasesOptions options,
      out string cBase, out string[] aEnds)
    {
      cBase = null;
      aEnds = null;

      switch (declension)
      {
        #region Первое склонение

        case RusDeclension.First:
          if (upperForm.EndsWith("ЬЯ")) // свинья
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
          if (upperForm.EndsWith("АЯ"))
          {
            // прилагательное (красная)
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ая"
            aEnds = new string[12]{"ая", "ой" ,"ой" ,"ую","ой" ,"ой",
                                   "ые","ых","ым","ых","ыми","ых"};
            return;
          }

          if (upperForm.EndsWith("ИЯ")) // имена ("Юлия")
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ия"
            aEnds = new string[12]{"ия","ии" ,"ии" ,"ию","ией" ,"ии",
                                   "ии","ией","иям","ии","иями","иях"};
            return;
          }

          if (upperForm.EndsWith("ЯЯ")) // прилагательное (синяя)
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "яя"
            aEnds = new string[12]{"яя","ей" ,"ей" ,"юю","ей" ,"ей",
                                   "ие","их","им","их","ими","их"};
            return;
          }
          if (upperForm.EndsWith("Я"))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "я"
            aEnds = new string[12]{"я","и" ,"е" ,"ю","ей" ,"е",
                                   "и","ей","ям","ей","ями","ях"};
            return;

          }
          if ((upperForm.EndsWith("ОВА") || upperForm.EndsWith("ЕВА") ||
            upperForm.EndsWith("ИНА") || upperForm.EndsWith("ЫНА")) &&
            ((options & RusFormArrayGetCasesOptions.Which) == RusFormArrayGetCasesOptions.Which))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "а"
            aEnds = new string[12]{"а","ой" ,"ой" ,"у","ой" ,"ой",
                                   "ы","ых","ым","ых","ыми","ых"};
            return;
          }
          if (upperForm.EndsWith("ЙКА"))  // гайка, чайка
          {
            // В родительном и винительном падежах множественного числа
            // исчезает "Й"
            cBase = baseForm.Substring(0, baseForm.Length - 3);// Отбросили "йка"
            aEnds = new string[12]{"йка","йки" ,"йке" ,"йку","йкой" ,"йке",
                                   "йки","ек","йкам","ек","йками","йках"};
            return;
          }
          if (upperForm.EndsWith("ЧКА"))  // тачка, булочка
          {
            // В родительном и винительном падежах множественного числа
            // появоятся буква "е"
            cBase = baseForm.Substring(0, baseForm.Length - 3);// Отбросили "чка"
            //(можно было и 2 буквы отбросить)
            aEnds = new string[12]{"чка","чки" ,"чке" ,"чку","чкой" ,"чке",
                    "чки","чек","чкам","чек","чками","чках"};
            return;
          }
          if (baseForm.EndsWith("ВКА"))  // морковка
          {
            // В родительном и винительном падежах множественного числа
            // добавляется буква "о": морковка->морковок, а не морковк
            cBase = baseForm.Substring(0, baseForm.Length - 3);// Отбросили "вка"
            //(можно было и 2 буквы отбросить)
            aEnds = new string[12]{"вка","вки" ,"вке" ,"вку","вкой" ,"вке",
                                   "вки","вок","вкам","вки","вками","вках"};
            return;
          }
          if (upperForm.EndsWith("А"))
          {
            if (upperForm.Length > 3 && upperForm[upperForm.Length - 3] == 'Ь' &&
                 "БВГДЖЗКЛМНПРСТФХЦЧШЩ".IndexOf(upperForm[upperForm.Length - 2]) >= 0)
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
          if (upperForm.EndsWith("Ы")) // "ножницы"
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "ы"
            aEnds = new string[12]{"ы", "", "ам", "ы", "амм"  ,"ах",
                                   "ы", "", "ам", "ы", "ами" , "ах"};
          }
          break;

        #endregion

        #region Второе склонение

        case RusDeclension.Second:
          if (upperForm.EndsWith("Ь")) // конь, пень
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "ь"
            aEnds = new string[12]{"ь","я" ,"ю" ,"я","ем" ,"е",
                                   "и","ей","ям","ей","ями","ях"};
            return;
          }
          if (upperForm.EndsWith("ЫЙ"))  // прилагательные ("главный")
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ый"
            aEnds = new string[12]{"ый","ого" ,"ому" ,"ого","ым" ,"ом",           
                                   "ые","ых","ым","ых","ыми","ых"};
            return;
          }

          if (upperForm.EndsWith("ИЙ"))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ий"
            if ((options & RusFormArrayGetCasesOptions.Name) == RusFormArrayGetCasesOptions.Name)
              // Имена "Василий", "Дмитрий"
              aEnds = new string[12]{"ий","ия" ,"ию" ,"ия","ием" ,"ие",           
                                     "ии","иев","иям","иев","иями","иях"};
            else
            // прилагательные ("ближний"-"ближнего")
            // , но            "близкий"-"близкого"  
            {
              char c3 = ' ';
              if (upperForm.Length > 2)
                c3 = upperForm[upperForm.Length - 3];
              if ("БВГКМПРСТФХ".IndexOf(c3) >= 0) // 30.10.2009 ??? Где бы найти правило
                aEnds = new string[12]{"ий","ого" ,"ому" ,"ого","им" ,"ом",           
                                     "ие","их","им","их","ими","их"};
              else
                aEnds = new string[12]{"ий","его" ,"ему" ,"его","им" ,"ем",           
                                     "ие","их","им","их","ими","их"};
            }
            return;
          }
          if (upperForm.EndsWith("ЕЙ"))
          {
            // Отличается винительным падежом множественного числа
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ей"
            aEnds = new string[12]{"ей","ея" ,"ею" ,"ея","еем" ,"ее",
                                   "еи","еев","еям","еев","еями","еях"};
            return;
          }
          if (upperForm.EndsWith("ОЙ"))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ой"
            aEnds = new string[12]{"ой","ого" ,"ому" ,"ого","ым" ,"ом",
                                   "ые","ых","ым","ых","ыми","ых"};
            return;
          }
          if (upperForm.EndsWith("АЙ"))
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ай"
            aEnds = new string[12]{"ай","ая" ,"аю" ,"ая","аем" ,"ае",
                                   "аи","аев","аями","аев","аями","аях"};
            return;
          }
          if (upperForm.EndsWith("Й")) // буй
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "й"
            aEnds = new string[12]{"й","я" ,"ю" ,"й","ем" ,"е",
                                   "и","ев","ям","и","ями","ях"};
            return;
          }
          if (upperForm.EndsWith("Е")) // солнце
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
          if (upperForm.EndsWith("О")) // окно
          {
            cBase = baseForm.Substring(0, baseForm.Length - 1);// Отбросили "о"
            aEnds = new string[12]{"о","а" ,"у" ,"о","ом" ,"е",
                                   "а","","ам","а","ами","ах"};
            return;
          }
          if (upperForm.EndsWith("НЕЦ"))
          {
            // В таких словах ("конец") исчезает буква "е"
            cBase = baseForm.Substring(0, baseForm.Length - 3);// Отбросили "нец"
            aEnds = new string[12]{"нец","нца" ,"нцу" ,"нец","нцом" ,"нце",
                                   "нцы","нцов","нцам","нцы","нцами","нцах"};
            return;
          }
          if (upperForm.EndsWith("ОВ") || upperForm.EndsWith("ЕВ"))
          {
            cBase = baseForm; // ничего не отбрасываем
            aEnds = new string[12]{""  ,"а" ,"у" ,"а" ,"ым" ,"е",
                                   "ы","ых","ым","ых","ыми","ых"};
            return;
          }
          if (upperForm.EndsWith("ИН") || upperForm.EndsWith("ЫН"))
          {
            cBase = baseForm; // ничего не отбрасываем
            aEnds = new string[12]{""  ,"а" ,"у" ,"а" ,"ым" ,"е",
                                   "ы","ых","ым","ых","ами","ых"};
            return;
          }
          if (upperForm.EndsWith("ИЧ") || upperForm.EndsWith("ЫЧ"))
          {
            cBase = baseForm; // ничего не отбрасываем
            aEnds = new string[12]{""  ,"а" ,"у" ,"а" ,"ем" ,"е",
                                   "и","ей","ам","ей","ами","ах"};
            return;
          }
          if (upperForm.EndsWith("ОК")) // горшок, совок, пучок
          {
            cBase = baseForm.Substring(0, baseForm.Length - 2);// Отбросили "ок"
            aEnds = new string[12]{"ок", "ка" ,"ку" ,"ок","ком" ,"ке",
                                   "ки", "ков","кам","ки","ками","ках"};
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

          if (upperForm.EndsWith("Ы")) // "щипцы"
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
          if (upperForm.EndsWith("ЧЬ") || upperForm.EndsWith("ШЬ") ||
            upperForm.EndsWith("ЩЬ") || upperForm.EndsWith("ЖЬ"))
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
        gender = MyGetGender(c);

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

    private static RusGender MyGetGender(char c)
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
