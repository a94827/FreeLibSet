using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

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
  /// <summary>
  /// Получение рублей и копеек из суммы
  /// </summary>
  public struct RoublesAndCopecks
  {                                         
    #region Конструктор

    /// <summary>
    /// Инициализирует структуру заданным значением в рублях
    /// </summary>
    /// <param name="value">Числовое значение в рублях</param>
    public RoublesAndCopecks(decimal value)
    {
      // См. свойство AsDecimal
      _Negative = (value < 0);
      value = Math.Abs(value);
      value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
      // Выделяем рубли и копейки
      _Roubles = (ulong)(Math.Truncate(value));
      _Copecks = (uint)((value - _Roubles) * 100);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// True, если значение меньше нуля
    /// </summary>
    public bool Negative { get { return _Negative; } }
    private bool _Negative;

    /// <summary>
    /// Рубли (целое число без знака)
    /// </summary>
    public ulong Roubles { get { return _Roubles; } }
    private ulong _Roubles;

    /// <summary>
    /// Копейки (от 0 до 99)
    /// </summary>
    public uint Copecks { get { return _Copecks; } }
    private uint _Copecks;

    #endregion

    #region Преобразование в число

    /// <summary>
    /// Возвращает значение в рублях
    /// </summary>
    public decimal AsDecimal
    {
      get
      {
        decimal res = ((decimal)Roubles) + ((decimal)Copecks) / 100;
        if (Negative)
          res = -res;
        return res;
      }
    }

    #endregion

    #region Преобразования в текст

    /// <summary>
    /// Возвращает рубли со знаком (-5.65  -> "-5")
    /// </summary>
    public string RoublesStr
    {
      get
      {
        string res = Roubles.ToString();
        if (Negative)
          res = "-" + res;
        return res;
      }
    }

    /// <summary>
    /// Возвращает копейки
    /// </summary>
    public string CopecksStr
    {
      get
      {
        return Copecks.ToString("00");
      }
    }

    /// <summary>
    /// Значение в рублях и копейках
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return RoublesStr + "." + CopecksStr;
    }

    //public override bool Equals(object obj)
    //{
    //  if (!(obj is RoublesAndCopecks))
    //    return false;

    //  return Negative == ((RoublesAndCopecks)obj).Negative &&
    //    Roubles == ((RoublesAndCopecks)obj).Roubles &&
    //    Copecks == ((RoublesAndCopecks)obj).Copecks;
    //}

    #endregion
  }

  /// <summary>
  /// Преобразование чисел в строки прописью
  /// </summary>
  public sealed class RusNumberConvert
  {
    #region Перечисления

    /// <summary>
    /// Способ преобразования копеек в MoneyStr()
    /// </summary>
    public enum CopecksFormat
    {
      /// <summary>
      /// Цифрами ("пять рублей 15 копеек")
      /// </summary>
      Digits,

      /// <summary>
      /// Прописью ("пять рублей пятнадцать копеек")
      /// </summary>
      String,

      /// <summary>
      /// Цифрами, если есть, и пусто, если 0 копеек 
      /// ("пять рублей 15 копеек", но "пять рублей")
      /// </summary>
      NoneIfZero,

      /// <summary>
      /// Отбросить копейки, сумма в целых рублях
      /// </summary>
      None,
    }

    const string NonBreakSpace = "\xA0";

    #endregion

    #region Денежная сумма

    /// <summary>
    /// Символ "мягкого" переноса
    /// </summary>
    private const char SoftHyphenChar = '\u00AD';

    /// <summary>
    /// Символ неразрывного пробела
    /// </summary>
    private const char NonBreakSpaceChar = '\u00A0';


    /// <summary>
    /// Получение денежной суммы прописью в именительном падеже.
    /// Копейки выводятся как две цифры.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="value">Денежная сумма</param>
    /// <returns>Текстовое представление суммы</returns>
    public static string MoneyStr(Decimal value)
    {
      return MoneyStr(value, CopecksFormat.Digits, RusCase.Nominative);
    }

    /// <summary>
    /// Получение денежной суммы прописью в именительном падеже.
    /// Копейки выводятся как две цифры.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="value">Денежная сумма (в рублях)</param>
    public static void MoneyStr(StringBuilder sb, Decimal value)
    {
      MoneyStr(sb, value, CopecksFormat.Digits, RusCase.Nominative);
    }

    /// <summary>
    /// Получение денежной суммы прописью в именительном падеже.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="value">Денежная сумма</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <returns>Текстовое представление суммы</returns>
    public static string MoneyStr(Decimal value, CopecksFormat copecksFormat)
    {
      return MoneyStr(value, copecksFormat, RusCase.Nominative);
    }

    /// <summary>
    /// Получение денежной суммы прописью в именительном падеже.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="value">Денежная сумма (в рублях)</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    public static void MoneyStr(StringBuilder sb, Decimal value, CopecksFormat copecksFormat)
    {
      MoneyStr(sb, value, copecksFormat, RusCase.Nominative);
    }

    /// <summary>
    /// Получение денежной суммы прописью.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="value">Денежная сумма</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <param name="theCase">В каком падеже выводить</param>
    /// <returns>Текстовое представление суммы</returns>
    public static string MoneyStr(Decimal value, CopecksFormat copecksFormat, RusCase theCase)
    {
      return MoneyStr(value, copecksFormat, theCase, false);
    }

    /// <summary>
    /// Получение денежной суммы прописью.
    /// </summary>
    /// <param name="value">Денежная сумма (в рублях)</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <param name="theCase">В каком падеже выводить</param>
    /// <param name="specChars">Если true, то будут использованы специальные символы - "мягкий" перенос и неразрывный пробел.
    /// Если false, то символов переноса не будет в тексте, и будет использован обычный пробел</param>
    /// <returns>Текстовое представление суммы</returns>
    public static string MoneyStr(Decimal value, CopecksFormat copecksFormat, RusCase theCase, bool specChars)
    {
      RoublesAndCopecks value2 = new RoublesAndCopecks(value);
      return MoneyStr(value2, copecksFormat, theCase, specChars);
    }


    /// <summary>
    /// Получение денежной суммы прописью.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="value">Денежная сумма (в рублях)</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <param name="theCase">В каком падеже выводить</param>
    public static void MoneyStr(StringBuilder sb, Decimal value, CopecksFormat copecksFormat, RusCase theCase)
    {
      MoneyStr(sb, value, copecksFormat, theCase, false);
    }

    /// <summary>
    /// Получение денежной суммы прописью.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="value">Денежная сумма (в рублях)</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <param name="theCase">В каком падеже выводить</param>
    /// <param name="specChars">Если true, то будут использованы специальные символы - "мягкий" перенос и неразрывный пробел.
    /// Если false, то символов переноса не будет в тексте, и будет использован обычный пробел</param>
    public static void MoneyStr(StringBuilder sb, Decimal value, CopecksFormat copecksFormat, RusCase theCase, bool specChars)
    {
      RoublesAndCopecks value2 = new RoublesAndCopecks(value);
      MoneyStr(sb, value2, copecksFormat, theCase, specChars);
    }

    /// <summary>
    /// Получение денежной суммы прописью.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="value">Денежная сумма</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <param name="theCase">В каком падеже выводить</param>
    /// <returns>Текстовое представление суммы</returns>
    public static string MoneyStr(RoublesAndCopecks value, CopecksFormat copecksFormat, RusCase theCase)
    {
      return MoneyStr(value, copecksFormat, theCase, false);
    }

    /// <summary>
    /// Получение денежной суммы прописью.
    /// </summary>
    /// <param name="value">Денежная сумма</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <param name="theCase">В каком падеже выводить</param>
    /// <param name="specChars">Если true, то будут использованы специальные символы - "мягкий" перенос и неразрывный пробел.
    /// Если false, то символов переноса не будет в тексте, и будет использован обычный пробел</param>
    /// <returns>Текстовое представление суммы</returns>
    public static string MoneyStr(RoublesAndCopecks value, CopecksFormat copecksFormat, RusCase theCase, bool specChars)
    {
      StringBuilder sb = new StringBuilder();
      MoneyStr(sb, value, copecksFormat, theCase, specChars);
      return sb.ToString();
    }

    /// <summary>
    /// Получение денежной суммы прописью.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="value">Денежная сумма</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <param name="theCase">В каком падеже выводить</param>
    public static void MoneyStr(StringBuilder sb, RoublesAndCopecks value, CopecksFormat copecksFormat, RusCase theCase)
    {
      MoneyStr(sb, value, copecksFormat, theCase, false);
    }

    /// <summary>
    /// Получение денежной суммы прописью.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="value">Денежная сумма</param>
    /// <param name="copecksFormat">Формат отображения копеек</param>
    /// <param name="theCase">В каком падеже выводить</param>
    /// <param name="specChars">Если true, то будут использованы специальные символы - "мягкий" перенос и неразрывный пробел.
    /// Если false, то символов переноса не будет в тексте, и будет использован обычный пробел</param>
    public static void MoneyStr(StringBuilder sb, RoublesAndCopecks value, CopecksFormat copecksFormat, RusCase theCase, bool specChars)
    {
      // Число рублей прописью

      if (value.Negative)
        SpecAppend(sb, "ми-нус ", specChars);
      int StartPos = sb.Length;
      ToString(sb, value.Roubles, RusGender.Masculine, theCase, specChars);
      sb.Append(specChars ? NonBreakSpaceChar : ' ');
      SpecAppend(sb, _RoublesForms[GetCF12(value.Roubles, theCase)], specChars);

      if (copecksFormat == CopecksFormat.NoneIfZero)
      {
        if (value.Copecks == 0)
          copecksFormat = CopecksFormat.None;
        else
          copecksFormat = CopecksFormat.Digits;
      }

      switch (copecksFormat)
      {
        case CopecksFormat.Digits:
          sb.Append(" ");
          sb.Append(value.Copecks.ToString("00"));
          sb.Append(NonBreakSpace);
          SpecAppend(sb, _CopecksForms[GetCF12(value.Copecks, theCase)], specChars);
          break;
        case CopecksFormat.String:
          sb.Append(" ");
          ToString(sb, value.Copecks, RusGender.Feminine, theCase);
          sb.Append(NonBreakSpace);
          SpecAppend(sb, _CopecksForms[GetCF12(value.Copecks, theCase)], specChars);
          break;
      }

      // В именительном падеже преобразуем к верхнему регистру
      if (theCase == RusCase.Nominative)
      {
        sb[StartPos] = sb[StartPos].ToString().ToUpperInvariant()[0];
      }
    }

    private static string[] _RoublesForms = new string[]{
      "рубль","руб-ля", "руб-лю","рубль","руб-лем","руб-ле",
      "руб-ли","руб-лей","руб-лям","руб-ли","руб-ля-ми","руб-лях"};

    private static string[] _CopecksForms = new string[]{
      "ко-пей-ка", "ко-пей-ки", "ко-пей-ке", "ко-пей-ку", "ко-пей-кой", "ко-пей-ке",
      "ко-пей-ки", "ко-пе-ек", "ко-пей-кам", "ко-пей-ки", "ко-пей-ка-ми", "ко-пей-ках"};

    #endregion

    #region Целые числа

    /// <summary>
    /// Максимальное число, которое можно преобразовать в строку
    /// </summary>
    public const Int64 MaxInt = 999999999999;

    /// <summary>
    /// Преобразование целого числа в строку прописью.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="x">Преобразуемое значение</param>
    /// <param name="gender">Род существительного, к которому относится число для определения варианта "один", "одна" или "одно"</param>
    /// <param name="theCase">Падеж ("одна (кошка)", "одной (кошки)", "одну (кошку)" ...)</param>
    /// <returns>Текстовое представление числа</returns>
    public static string ToString(Int64 x, RusGender gender, RusCase theCase)
    {
      return ToString(x, gender, theCase, false);
    }

    /// <summary>
    /// Преобразование целого числа в строку прописью.
    /// </summary>
    /// <param name="x">Преобразуемое значение</param>
    /// <param name="gender">Род существительного, к которому относится число для определения варианта "один", "одна" или "одно"</param>
    /// <param name="theCase">Падеж ("одна (кошка)", "одной (кошки)", "одну (кошку)" ...)</param>
    /// <param name="specChars">Если true, то в тексте будут использованы символы "мягкого" переноса.
    /// Если false, то специальные символы не используются</param>
    /// <returns>Текстовое представление числа</returns>
    public static string ToString(Int64 x, RusGender gender, RusCase theCase, bool specChars)
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb, x, gender, theCase, specChars);
      return sb.ToString();
    }

    /// <summary>
    /// Преобразование целого числа в строку прописью.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое значение</param>
    /// <param name="gender">Род существительного, к которому относится число для определения варианта "один", "одна" или "одно"</param>
    /// <param name="theCase">Падеж ("одна (кошка)", "одной (кошки)", "одну (кошку)" ...)</param>
    public static void ToString(StringBuilder sb, Int64 x, RusGender gender, RusCase theCase)
    {
      ToString(sb, x, gender, theCase, false);
    }

    /// <summary>
    /// Преобразование целого числа в строку прописью.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое значение</param>
    /// <param name="gender">Род существительного, к которому относится число для определения варианта "один", "одна" или "одно"</param>
    /// <param name="theCase">Падеж ("одна (кошка)", "одной (кошки)", "одну (кошку)" ...)</param>
    /// <param name="specChars">Если true, то в тексте будут использованы символы "мягкого" переноса.
    /// Если false, то специальные символы не используются</param>
    public static void ToString(StringBuilder sb, Int64 x, RusGender gender, RusCase theCase, bool specChars)
    {
      UInt64 x2;

      // Обработка отрицательного числа
      if (x < 0)
      {
        sb.Append("минус ");
        x2 = (UInt64)(-x);
      }
      else
      {
        x2 = (UInt64)x;
      }
      ToString(sb, x2, gender, theCase, specChars);
    }

    /// <summary>
    /// Преобразование целого числа в строку прописью.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="x">Преобразуемое значение</param>
    /// <param name="gender">Род существительного, к которому относится число для определения варианта "один", "одна" или "одно"</param>
    /// <param name="theCase">Падеж ("одна (кошка)", "одной (кошки)", "одну (кошку)" ...)</param>
    /// <returns>Текстовое представление числа</returns>
    public static string ToString(UInt64 x, RusGender gender, RusCase theCase)
    {
      return ToString(x, gender, theCase, false);
    }

    /// <summary>
    /// Преобразование целого числа в строку прописью.
    /// </summary>
    /// <param name="x">Преобразуемое значение</param>
    /// <param name="gender">Род существительного, к которому относится число для определения варианта "один", "одна" или "одно"</param>
    /// <param name="theCase">Падеж ("одна (кошка)", "одной (кошки)", "одну (кошку)" ...)</param>
    /// <param name="specChars">Если true, то в тексте будут использованы символы "мягкого" переноса.
    /// Если false, то специальные символы не используются</param>
    /// <returns>Текстовое представление числа</returns>
    public static string ToString(UInt64 x, RusGender gender, RusCase theCase, bool specChars)
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb, x, gender, theCase, specChars);
      return sb.ToString();
    }

    /// <summary>
    /// Преобразование целого числа в строку прописью.
    /// Специальные символы не используются.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое значение</param>
    /// <param name="gender">Род существительного, к которому относится число для определения варианта "один", "одна" или "одно"</param>
    /// <param name="theCase">Падеж ("одна (кошка)", "одной (кошки)", "одну (кошку)" ...)</param>
    public static void ToString(StringBuilder sb, UInt64 x, RusGender gender, RusCase theCase)
    {
      ToString(sb, x, gender, theCase, false);
    }

    /// <summary>
    /// Преобразование целого числа в строку прописью.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое значение</param>
    /// <param name="gender">Род существительного, к которому относится число для определения варианта "один", "одна" или "одно"</param>
    /// <param name="theCase">Падеж ("одна (кошка)", "одной (кошки)", "одну (кошку)" ...)</param>
    /// <param name="specChars">Если true, то в тексте будут использованы символы "мягкого" переноса.
    /// Если false, то специальные символы не используются</param>
    public static void ToString(StringBuilder sb, UInt64 x, RusGender gender, RusCase theCase, bool specChars)
    {
      int[] x1000 = new int[4];
      int i;
      RusGender thisForm;

      // 0 выделен в специальный случай, поскольку иначе при x=0 вообще ничего
      // выведено не будет.
      if (x == 0)
      {
        SpecAppend(sb, _ZeroForms[(int)theCase], specChars);
        return;
      }

      if (x > MaxInt)
        throw new ArgumentException("Слишком большое число для преобразования", "x");

      // Сюда записываем числа в диапазоне от 0 до 999, соответствующие
      // каждой группе из трех цифр (единицы, тысячи, миллионы и миллиарды для
      // индексов 0,1,2 и 3 соотвественно)
      for (i = 0; i < x1000.Length; i++)
      {
        x1000[i] = (int)(x % (Int64)1000);
        x = x / 1000;
      }

      int StartLength = sb.Length; // Добавляем пробелы, если строка увеличилась

      // Группы выводятся в строку в обратном порядке (начиная с миллиардов)
      for (i = x1000.Length - 1; i >= 0; i--)
      {
        if (x1000[i] > 0) // когда группа содержит 0, то ничего не выводится
        {
          // Определяем род, к которому будет относится число данной группы:
          // - миллион и миллиард - мужского рода
          // - тысяча             - женского
          // - единицы            - относятся к тому роду, который имеет
          //                        существительное, идущще за данным числом
          //
          switch (i)
          {
            case 0:
              thisForm = gender;
              break;
            case 1:
              thisForm = RusGender.Feminine;
              break;
            default:
              thisForm = RusGender.Masculine;
              break;
          }

          // Выводим прописью число в данной группе (от 1 до 999)
          if (sb.Length > StartLength)
            sb.Append(' ');
          Dig1000(sb, x1000[i], thisForm, theCase, specChars);

          // Если это не группа единиц, то надо вывести слова "тысяча",
          // "миллион" или "миллиард" в нужном числе и падеже
          if (i > 0)
          {
            if (sb.Length > StartLength)
              sb.Append(' ');
            SpecAppend(sb, _GreatestForms[(i - 1) * 12 + GetCF12((UInt64)(x1000[i]), theCase)], specChars);
          }
        }
      }
    }

    /// <summary>
    /// Добавление строки к буферу
    /// Если SpecChars=true, то обычные символы "-" переноса заменяются на символы
    /// "мягкого" переноса. Иначе они удаляются
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="s"></param>
    /// <param name="specChars"></param>
    private static void SpecAppend(StringBuilder sb, string s, bool specChars)
    {
      if (specChars)
        sb.Append(s.Replace('-', SoftHyphenChar));
      else
        sb.Append(s.Replace("-", ""));
    }

    private static string[] _ZeroForms = new string[]{
      "ноль","но-ля","но-лю","ноль","но-лем","но-ле" };

    private static string[] _GreatestForms = new string[]{
      "ты-ся-ча", "ты-ся-чи","ты-ся-че","ты-ся-чу","ты-ся-чей","ты-ся-чи",
      "ты-ся-чи","ты-сяч","ты-ся-чам","ты-ся-чи","ты-ся-ча-ми","ты-ся-чах",
      "мил-ли-он","мил-ли-о-на","мил-ли-о-ну","мил-ли-он","мил-ли-о-ном","мил-ли-о-не",
      "мил-ли-о-ны","мил-ли-о-нов","мил-ли-о-нам","мил-ли-о-ны","мил-ли-о-на-ми","мил-ли-о-нах",
      "мил-ли-ард","мил-ли-ар-да","мил-ли-ар-ду","мил-ли-ард","мил-ли-ар-дом","мил-ли-ар-де",
      "мил-ли-ар-ды","мил-ли-ар-дов","мил-ли-ар-дам","мил-ли-ар-ды","мил-ли-ар-да-ми","мил-ли-ар-дах" };


    /// <summary>
    /// Формы счета
    /// </summary>
    private enum CountForm
    {
      /// <summary>
      /// 1 рубль
      /// </summary>
      Count1 = 0,

      /// <summary>
      /// 2 рубля
      /// </summary>
      Count2 = 1,

      /// <summary>
      /// 5 рублей
      /// </summary>
      Count0 = 2
    }

    private static int GetCF12(ulong x, RusCase theCase)
    {
      RusCase resCase;
      RusNumber resNumber;

      GetCF(out resCase, out resNumber, x, theCase);
      return (int)resNumber * 6 + (int)resCase;
    }

    /// <summary>
    /// Получить падеж и число существительного, идущего после числительного.
    /// </summary>
    /// <param name="resCase">Падеж</param>
    /// <param name="resNumber">Число</param>
    /// <param name="x">Числительное, с которым согласуется существительное</param>
    /// <param name="theCase">Падеж, в котором идет вся сборка</param>
    public static void GetCF(out RusCase resCase, out RusNumber resNumber, ulong x, RusCase theCase)
    {
      CountForm cnts;

      // Сначала предполагаем, что падеж существительного
      // будет совпадать с падежом всей фразы
      resCase = theCase;

      if (x % 1000 == 0)
      {
        // Обычно числа, оканчивающиеся на ноль, склоняют сущестительное
        // во множественном числе (кроме именительного и винительного падежей)
        // 0 - исключение. Всегда родительный падеж множественного числа:
        // "Я думаю о 10/20/100/1000 рублях", но
        // "Я думаю о 0 рублей".
        resCase = RusCase.Genitive;
        resNumber = RusNumber.Plural;
        return;
      }

      // Существует три варианта склонения существительного:
      // - для чисел, оканчивающихся на 1 (кроме 11) - rusCount1
      // - для чисел, оканчивающихся на 2,3,4 (кроме 12-14) - rusCount2
      // - для чисел, оканчивающихся на 5,6,7,8,9,0 и 11-14 - rusCount5
      //
      // По последним двум десятичным разрядам определяем, какая форма требуется
      cnts = GetC100((Int64)x % 100); // Обязательно
      // сначала нужно разделить на 100 и получить остаток, а потом округлять.

      if (cnts == CountForm.Count1)
      {
        // Первая форма самая простая. Падеж существительного совпадает с
        // падежом всей фразы.
        resNumber = RusNumber.Singular;
        return;
      }
      resNumber = RusNumber.Plural;

      if (theCase == RusCase.Nominative || theCase == RusCase.Accusative)
      {
        // Именительный и винительный падежи всей фразы дают родительный падеж
        // у существительного (если число не заканчивалось на 1 - первая форма).
        resCase = RusCase.Genitive;
        // "У меня есть/Я вижу 2/3/4 рубля", "У меня есть/Я вижу 5/6/7 рублей"
        // - для второй формы существительное склоняется в единственном числе
        if (cnts == CountForm.Count2)
          resNumber = RusNumber.Singular;
      }
    }

    /// <summary>
    /// Получить падеж и число существительного, идущего после числительного.
    /// </summary>
    /// <param name="resCase">Падеж</param>
    /// <param name="resNumber">Число</param>
    /// <param name="x">Числительное, с которым согласуется существительное</param>
    /// <param name="maxDecimals">Количество знаков после запятой, до которого нужно округлить <paramref name="x"/>,
    /// чтобы определить, является ли число целым</param>
    /// <param name="theCase">Падеж, в котором идет вся сборка</param>
    public static void GetCF(out RusCase resCase, out RusNumber resNumber, decimal x, int maxDecimals, RusCase theCase)
    {
      x = Math.Abs(x); // минус игнорируется
      x = Math.Round(x, maxDecimals, MidpointRounding.AwayFromZero);
      if (x == Math.Round(x))
        GetCF(out resCase, out resNumber, (ulong)x, theCase);
      else
      {
        // !!!! ????
        resCase = RusCase.Genitive;
        resNumber = RusNumber.Singular;
      }
    }

    /// <summary>
    /// Получить падеж и число существительного, идущего после числительного.
    /// </summary>
    /// <param name="resCase">Падеж</param>
    /// <param name="resNumber">Число</param>
    /// <param name="x">Числительное, с которым согласуется существительное</param>
    /// <param name="theCase">Падеж, в котором идет вся сборка</param>
    public static void GetCF(out RusCase resCase, out RusNumber resNumber, int x, RusCase theCase)
    {
      GetCF(out resCase, out resNumber, (ulong)x, theCase);
    }

    // Внутренняя функция преобразования числа в строку
    // Выполняет преобразование для чисел в диапазоне от 1 до 999

    private static void Dig1000(StringBuilder sb, int x, RusGender gender, RusCase theCase, bool specChars)
    {
      int off, x100, x10, x1;

      // Падежное смещение
      off = (int)theCase;

      // Разделение на цифры
      x100 = (x / 100) % 10;
      x10 = (x / 10) % 10;
      x1 = x % 10;

      int StartLength = sb.Length;

      // Сотни
      if (x100 != 0)
      {
        SpecAppend(sb, _Forms100s[6 * (x100 - 1) + off], specChars);
      }

      if (x10 > 1)
      {
        // десятки начиная с 20
        if (sb.Length > StartLength)
          sb.Append(' ');
        SpecAppend(sb, _Forms2090s[6 * (x10 - 2) + off], specChars);
      }
      else
      {
        if (x10 == 1)
        {
          // десяток и единицы для чисел от 10 до 19
          if (sb.Length > StartLength)
            sb.Append(' ');
          SpecAppend(sb, _Forms1019s[6 * (x1) + off], specChars);
          return;
        }
      }

      // Единицы
      if (x1 > 0)
      {
        if (sb.Length > StartLength)
          sb.Append(' ');

        // Только 1 и 2 имеют разделение по родам
        if (x1 == 1)
        {
          if (gender == RusGender.Feminine)
          {
            SpecAppend(sb, _Forms1Feminine[off], specChars);
            return;
          }
          if (gender == RusGender.Newter)
          {
            SpecAppend(sb, _Forms1Newter[off], specChars);
            return;
          }
        }
        if (x1 == 2 && gender == RusGender.Feminine)
        {
          SpecAppend(sb, _Forms2Feminine[off], specChars);
          return;
        }

        // обобщенная форма (мужской род)
        SpecAppend(sb, _Forms19s[6 * (x1 - 1) + off], specChars);
      }
      return;
    }

    private static string[] _Forms100s = new string[]{
      "сто","ста","ста","сто","ста","ста",
      "две-сти","двух-сот","двум-стам","две-сти","дву-мя-ста-ми","двух-стах",
      "три-ста","трех-сот","трем-стам","три-ста","тре-мя-ста-ми","трех-стах",
      "че-ты-ре-ста","че-ты-рех-сот","че-ты-рем-стам","че-ты-ре-ста","че-ты-ре-мя-ста-ми","че-ты-рех-стах",
      "пять-сот","пя-ти-сот","пя-ти-стам","пять-сот","пятью-ста-ми","пя-ти-стах",
      "шесть-сот","шес-ти-сот","шес-ти-стам","шесть-сот","шестью-ста-ми","шес-ти-стах",
      "семь-сот","се-ми-сот","се-ми-стам","семь-сот","семью-ста-ми","се-ми-стах",
      "во-семь-сот","вось-ми-сот","вось-ми-стам","во-семь-сот","во-семью-ста-ми","вось-ми-стах",
      "де-вять-сот","де-вя-ти-сот","де-вя-ти-стам","дев-ять-сот","де-вятью-стами","де-вя-ти-стах" };

    private static string[] _Forms2090s = new string[]{
      "двад-цать","двад-ца-ти","двад-ца-ти","двад-цать","двад-цатью","двад-ца-ти",
      "трид-цать","трид-ца-ти","трид-ца-ти","трид-цать","трид-цатью","трид-ца-ти",
      "со-рок","со-ро-ка","со-ро-ка","со-рок","со-ро-ка","со-ро-ка",
      "пять-де-сят","пя-ти-де-ся-ти","пя-ти-де-ся-ти","пять-де-сят","пятью-де-сятью","пя-ти-де-ся-ти",
      "шесть-де-сят","шес-ти-де-ся-ти","шес-ти-де-ся-ти","шесть-де-сят","шестью-де-сятью","шес-ти-де-ся-ти",
      "семь-де-сят","се-ми-де-ся-ти","се-ми-де-ся-ти","семь-де-сят","семью-де-сятью","се-ми-де-ся-ти",
      "во-семь-де-сят","восьми-де-ся-ти","восьми-де-ся-ти","во-семь-де-сят","во-семьюде-сятью","вось-ми-де-ся-ти",
      "де-вя-но-сто","де-вя-но-ста","де-вя-но-ста","де-вя-но-сто","де-вя-но-ста","де-вя-но-ста"};

    private static string[] _Forms1019s = new string[]{
      "де-сять","де-ся-ти","де-ся-ти","де-сять","де-сятью","де-ся-ти",
      "один-на-дцать","один-на-дца-ти","один-на-дца-ти","один-на-дцать","один-на-дцатью","один-на-дца-ти",
      "две-на-дцать","две-на-дца-ти","две-на-дца-ти","две-на-дцать","две-на-дцатью","две-на-дца-ти",
      "три-на-дцать","три-на-дца-ти","три-на-дца-ти","три-на-дцать","три-на-дцатью","три-на-дца-ти",
      "че-тыр-на-дцать","че-тыр-на-дца-ти","че-тыр-на-дца-ти","че-тыр-на-дцать","че-тыр-на-дцатью","че-тыр-на-дца-ти",
      "пят-на-дцать","пят-на-дца-ти","пят-на-дца-ти","пят-на-дцать","пят-на-дцатью","пят-на-дца-ти",
      "шест-на-дцать","шест-на-дца-ти","шест-на-дца-ти","шест-на-дцать","шест-на-дцатью","шест-на-дца-ти",
      "сем-на-дцать","сем-на-дца-ти","сем-на-дца-ти","сем-на-дцать","сем-на-дцатью","сем-на-дца-ти",
      "во-сем-на-дцать","во-сем-на-дца-ти","во-сем-на-дца-ти","во-сем-на-дцать","во-сем-на-дцатью","во-сем-на-дца-ти",
      "де-вят-на-дцать","де-вят-на-дца-ти","де-вят-на-дца-ти","де-вят-на-дцать","де-вят-на-дцатью","де-вят-на-дца-ти" };

    private static string[] _Forms1Feminine = new string[]{
      "од-на","од-ной","од-ной","од-ну","од-ной","од-ной" };

    private static string[] _Forms1Newter = new string[] {
      "од-но", "од-но-го", "од-но-му", "од-но", "од-ним", "од-ном" };

    private static string[] _Forms2Feminine = new string[]{
      "две", "двух", "двум", "две", "дву-мя", "двух" };

    private static string[] _Forms19s = new string[]{
      "один","од-но-го","од-но-му","один","од-ним","од-ном",
      "два","двух","двум","два","дву-мя","двух",
      "три","трех","трем","три","тре-мя","трех",
      "че-ты-ре","че-ты-рех","че-ты-рем","че-ты-ре","че-тырь-мя","че-ты-рех",
      "пять","пя-ти","пя-ти","пять","пятью","пя-ти",
      "шесть","шес-ти","шес-ти","шесть","шестью","шес-ти",
      "семь","се-ми","се-ми","семь","семью","се-ми",
      "во-семь","вось-ми","вось-ми","во-семь","во-семью","вось-ми",
      "де-вять","де-вя-ти","де-вя-ти","де-вять","де-вятью","де-вя-ти"};

    /// <summary>
    /// Определение правильного способа склонения имени существительного,
    /// идущего после числа по двум последним десятичным разрядам.
    /// Существует три варианта склонения существительного:
    /// - для чисел, оканчивающихся на 1 (кроме 11) - rusCount1
    /// - для чисел, оканчивающихся на 2,3,4 (кроме 12-14) - rusCount2
    /// - для чисел, оканчивающихся на 5,6,7,8,9,0 и 11-14 - rusCount5
    /// </summary>
    /// <param name="x">Число</param>
    /// <returns>Форма счета</returns>
    private static CountForm GetC100(Int64 x)
    {
      int x10;
      int x1;
      x = Math.Abs(x);
      x10 = (int)((x / 10) % 10);

      if (x10 == 1)
        return CountForm.Count0;

      x1 = (int)(x % 10);

      if (x1 == 1)
        return CountForm.Count1;

      if (x1 >= 2 && x1 <= 4)
        return CountForm.Count2;

      return CountForm.Count0;
    }

    #endregion

    #region Числа с плавающей точкой

    /// <summary>
    /// Преобразование числа с плавающей точкой в строку в именительном падеже.
    /// Количество действующих десятичных разрядов определяется автоматически.
    /// </summary>
    /// <param name="x">Преобразуемое число</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(decimal x)
    {
      return ToString(x, MyGetDecimalPlaces(x), RusGender.Masculine, RusCase.Nominative);
    }

    /// <summary>
    /// Преобразование числа в строку прописью с указанием всех параметров.
    /// Первая буква не делается прописной.
    /// Выводится оптимальное количество знаков после запятой, но не более 11.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое число</param>
    public static void ToString(StringBuilder sb, decimal x)
    {
      ToString(sb, x, MyGetDecimalPlaces(x), RusGender.Masculine, RusCase.Nominative);
    }

    /// <summary>
    /// Преобразование числа в строку прописью.
    /// Первая буква не делается прописной.
    /// Выводится оптимальное количество знаков после запятой, но не более 11.
    /// </summary>
    /// <param name="x">Преобразуемое число</param>
    /// <param name="gender">Род существительного, согласуемого с числительным</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(decimal x, RusGender gender)
    {
      return ToString(x, MyGetDecimalPlaces(x), gender, RusCase.Nominative);
    }

    /// <summary>
    /// Преобразование числа в строку прописью.
    /// Первая буква не делается прописной.
    /// Выводится оптимальное количество знаков после запятой, но не более 11.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое число</param>
    /// <param name="gender">Род существительного, согласуемого с числительным</param>
    public static void ToString(StringBuilder sb, decimal x, RusGender gender)
    {
      ToString(sb, x, MyGetDecimalPlaces(x), gender, RusCase.Nominative);
    }

    /// <summary>
    /// Получение оптимального числа максимального числа цифр после запятой
    /// </summary>
    /// <param name="x">Преобразуемое число</param>
    /// <returns>Число знаков после запятой от 0 до 11</returns>
    private static int MyGetDecimalPlaces(decimal x)
    {
      string s2 = x.ToString("f", CultureInfo.InvariantCulture);
      int p = s2.IndexOf('.');
      if (p < 0)
        return 0;
      int res = s2.Length - p - 1;
      if (res > 11)
        res = 11;
      return res;
    }

    /// <summary>
    /// Преобразование числа в строку прописью.
    /// Первая буква не делается прописной.
    /// </summary>
    /// <param name="x">Преобразуемое число</param>
    /// <param name="maxDecimalPlaces">Максимальное число знаков после запятой. Реально может быть выведено меньше знаков (включая 0)
    /// Допускаются значения от 0 до 11 включительно</param>
    /// <param name="gender">Род существительного, согласуемого с числительным</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(decimal x, int maxDecimalPlaces, RusGender gender)
    {
      return ToString(x, maxDecimalPlaces, gender, RusCase.Nominative);
    }

    /// <summary>
    /// Преобразование числа в строку прописью.
    /// Первая буква не делается прописной.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое число</param>
    /// <param name="maxDecimalPlaces">Максимальное число знаков после запятой. Реально может быть выведено меньше знаков (включая 0)
    /// Допускаются значения от 0 до 11 включительно</param>
    /// <param name="gender">Род существительного, согласуемого с числительным</param>
    public static void ToString(StringBuilder sb, decimal x, int maxDecimalPlaces, RusGender gender)
    {
      ToString(sb, x, maxDecimalPlaces, gender, RusCase.Nominative);
    }


    private static string[] _IntegerWordForm = new string[]{
      "це-лая", "це-лой" ,"це-лой" ,"це-лую","це-лой" ,"це-лой",
      "це-лые","це-лых" ,"це-лым" ,"це-лые","це-лы-ми","це-лых"};

    private static int[] _FraqFormIndices0 = new int[]
      //Р Р Д Р Т П  - реально используемые падежи
      { 0, 0, 1, 0, 2, 3 };
    private static string[,] _FraqForms0 = new string[11, 4]
        //      Р.П.                Д.П.                      Т.П.                         П.П
       {{"де-ся-тых"              ,"де-ся-тым"              ,"де-ся-ты-ми"              ,"де-ся-тых"              },
        {"со-тых"                 ,"со-тым"                 ,"со-ты-ми"                 ,"со-тых"                 },
        {"ты-сяч-ных"             ,"ты-сяч-ным"             ,"ты-сяч-ны-ми"             ,"ты-сяч-ных"             },
        {"де-ся-ти-ты-сяч-ных"    ,"де-ся-ти-ты-сяч-ным"    ,"де-ся-ти-ты-сяч-ны-ми"    ,"де-ся-ти-ты-сяч-ных"    },
        {"сто-ты-сяч-ных"         ,"сто-ты-сяч-ным"         ,"сто-ты-сяч-ны-ми"         ,"сто-ты-сяч-ных"         },
        {"мил-ли-он-ных"          ,"мил-ли-он-ным"          ,"мил-ли-он-ны-ми"          ,"мил-ли-он-ных"          },
        {"де-ся-ти-мил-ли-он-ных" ,"де-ся-ти-мил-ли-он-ным" ,"де-ся-ти-мил-ли-он-ны-ми" ,"де-ся-ти-мил-ли-он-ных" },
        {"сто-мил-ли-он-ных"      ,"сто-мил-ли-он-ным"      ,"сто-мил-ли-он-ны-ми"      ,"сто-мил-ли-он-ных"      },
        {"мил-ли-ард-ных"         ,"мил-ли-ард-ным"         ,"мил-ли-ард-ны-ми"         ,"мил-ли-ард-ных"         },
        {"де-ся-ти-мил-ли-ард-ных","де-ся-ти-мил-ли-ард-ным","де-ся-ти-мил-ли-ард-ны-ми","де-ся-ти-мил-ли-ард-ных"},
        {"сто-мил-ли-ард-ных"     ,"сто-мил-ли-ард-ным"     ,"сто-мил-ли-ард-ны-ми"     ,"сто-мил-ли-ард-ных"     } };

    private static int[] _FraqFormIndices1 = new int[]
      //И Р Р В Р Р  - реально используемые падежи
      { 0, 1, 1, 2, 1, 1 };
    private static string[,] _FraqForms1 = new string[11, 3]
       //      И.П.                  Р.П.                     В.П.     
       {{"де-ся-тая"              ,"де-ся-той"              ,"де-ся-тую"              },
        {"со-тая"                 ,"со-той"                 ,"со-тую"                 },
        {"ты-сяч-ная"             ,"ты-сяч-ной"             ,"ты-сяч-ную"             },
        {"де-ся-ти-ты-сяч-ная"    ,"де-ся-ти-ты-сяч-ной"    ,"де-ся-ти-ты-сяч-ную"    },
        {"сто-ты-сяч-ныя"         ,"сто-ты-сяч-ной"         ,"сто-ты-сяч-ную"         },
        {"мил-ли-он-ная"          ,"мил-ли-он-ной"          ,"мил-ли-он-ную"          },
        {"де-ся-ти-мил-ли-он-ная" ,"де-ся-ти-мил-ли-он-ной" ,"де-ся-ти-мил-ли-он-ную" },
        {"сто-мил-ли-он-ная"      ,"сто-мил-ли-он-ной"      ,"сто-мил-ли-он-ную"      },
        {"мил-ли-ард-ная"         ,"мил-ли-ард-ной"         ,"мил-ли-ард-ную"         },
        {"де-ся-ти-мил-ли-ард-ная","де-ся-ти-мил-ли-ард-ной","де-ся-ти-мил-ли-ард-ную"},
        {"сто-мил-ли-ард-ная"     ,"сто-мил-ли-ард-ной"     ,"сто-мил-ли-ард-ную"     } };


    /// <summary>
    /// Преобразование числа в строку прописью с указанием всех параметров.
    /// Первая буква не делается прописной
    /// </summary>
    /// <param name="x">Преобразуемое число</param>
    /// <param name="maxDecimalPlaces">Максимальное число знаков после запятой. Реально может быть выведено меньше знаков (включая 0)
    /// Допускаются значения от 0 до 11 включительно</param>
    /// <param name="gender">Род существительного, согласуемого с числительным</param>
    /// <param name="theCase">Падеж, в котором идет числительное и существительное</param>
    /// <returns>Строка, содержащая числительное прописью</returns>
    public static string ToString(decimal x, int maxDecimalPlaces, RusGender gender, RusCase theCase)
    {
      return ToString(x, maxDecimalPlaces, gender, theCase, false);
    }

    /// <summary>
    /// Преобразование числа в строку прописью с указанием всех параметров.
    /// Первая буква не делается прописной.
    /// </summary>
    /// <param name="x">Преобразуемое число</param>
    /// <param name="maxDecimalPlaces">Максимальное число знаков после запятой. Реально может быть выведено меньше знаков (включая 0)
    /// Допускаются значения от 0 до 11 включительно</param>
    /// <param name="gender">Род существительного, согласуемого с числительным</param>
    /// <param name="theCase">Падеж, в котором идет числительное и существительное</param>
    /// <param name="specChars">Если true, то будут использованы специальные символы - "мягкий" перенос и неразрывный пробел.
    /// Если false, то символов переноса не будет в тексте, и будет использован обычный пробел</param>
    /// <returns>Строка, содержащая числительное прописью</returns>
    public static string ToString(decimal x, int maxDecimalPlaces, RusGender gender, RusCase theCase, bool specChars)
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb, x, maxDecimalPlaces, gender, theCase, specChars);
      return sb.ToString();
    }

    /// <summary>
    /// Преобразование числа в строку прописью.
    /// Первая буква не делается прописной.
    /// Специальные символы не используются
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое число</param>
    /// <param name="maxDecimalPlaces">Максимальное число знаков после запятой. Реально может быть выведено меньше знаков (включая 0)
    /// Допускаются значения от 0 до 11 включительно</param>
    /// <param name="gender">Род существительного, согласуемого с числительным</param>
    /// <param name="theCase">Падеж, в котором идет числительное и существительное</param>
    /// <returns>Строка, содержащая числительное прописью</returns>
    public static void ToString(StringBuilder sb, decimal x, int maxDecimalPlaces, RusGender gender, RusCase theCase)
    {
      ToString(sb, x, maxDecimalPlaces, gender, theCase, false);
    }

    /// <summary>
    /// Преобразование числа в строку прописью с указанием всех параметров.
    /// Первая буква не делается прописной.
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="x">Преобразуемое число</param>
    /// <param name="maxDecimalPlaces">Максимальное число знаков после запятой. Реально может быть выведено меньше знаков (включая 0)
    /// Допускаются значения от 0 до 11 включительно</param>
    /// <param name="gender">Род существительного, согласуемого с числительным</param>
    /// <param name="theCase">Падеж, в котором идет числительное и существительное</param>
    /// <param name="specChars">Если true, то будут использованы специальные символы - "мягкий" перенос и неразрывный пробел.
    /// Если false, то символов переноса не будет в тексте, и будет использован обычный пробел</param>
    public static void ToString(StringBuilder sb, decimal x, int maxDecimalPlaces, RusGender gender, RusCase theCase, bool specChars)
    {
      // Проверка аргументов
      if (maxDecimalPlaces < 0 || maxDecimalPlaces > 11)
        throw new ArgumentOutOfRangeException("maxDecimalPlaces", maxDecimalPlaces, "Максимальное число цифр после запятой может быть от 0 до 11");
      if (gender == RusGender.Undefined)
        gender = RusGender.Masculine;
      if (theCase == RusCase.Undefined)
        theCase = RusCase.Nominative;

      // Преобразуем число в нужную форму в десятичном формате с точкой
      string NumberMask = "0." + new string('#', maxDecimalPlaces);
      string s2 = x.ToString(NumberMask, CultureInfo.InvariantCulture);

      // Ищем десятичную точку
      int p = s2.IndexOf('.');

      if (p < 0)
      {
        // целое число
        ToString(sb, (long)x, gender, theCase, specChars);
        return;
      }

      // Число содержит дробную часть, которую тоже надо преобразовать
      // Преобразуем целую часть
      // Согласование идет всегда по женскому роду (слово "целая" [часть])
      ToString(sb, (long)x, RusGender.Feminine, theCase, specChars);

      int FormIndex = GetCF12((ulong)(Math.Abs(x)), theCase);

      if (FormIndex == 1) // Р.П. ед.числа 'целой'->'целых'
        FormIndex = 7;

      sb.Append(' ');
      SpecAppend(sb, _IntegerWordForm[FormIndex], specChars);

      s2 = s2.Substring(p + 1); // Оставили только дробную часть

      if (s2.Length < 1 || s2.Length > 11)
        throw new Exception("Ошибка в программе. Неправильная дробная часть при преобразовании числа");

      sb.Append(' ');
      ToString(sb, long.Parse(s2), RusGender.Feminine, theCase, specChars);
      sb.Append(' ');

      // Согласование падежа для дробной части ("десятых")

      // По последним двум десятичным разрядам определяем, какая форма требуется
      int n99;
      if (s2.Length == 1)
        n99 = int.Parse(s2);
      else
        n99 = int.Parse(s2.Substring(s2.Length - 2));
      CountForm cnts = GetC100(n99);

      if (cnts != CountForm.Count1)
      {
        //И.П.: одна целая три десятых     (яблока)   Р.П.мн.ч.
        //Р.П.: одной целой трех десятых   (яблока)   Р.П.мн.ч.
        //Д.П.: одной целой трем десятым   (яблока)   Д.П.мн.ч.
        //В.П.: одну целую две десятых     (яблока)   Р.П.мн.ч.
        //Т.П.: одной целой двумя десятыми (яблока)   Т.П.мн.ч.
        //П.П.: одной целой двух десятых   (яблока)   П.П.мн.ч.

        //И.П.: две целых три десятых       (яблока)   Р.П.мн.ч.
        //Р.П.: двух целых трех десятых     (яблока)   Р.П.мн.ч.
        //Д.П.: двум целым трем десятым     (яблока)   Д.П.мн.ч.
        //В.П.: две целые две десятых       (яблока)   Р.П.мн.ч.
        //Т.П.: двумя целыми двумя десятыми (яблока)   Т.П.мн.ч.
        //П.П.: двух целых двух десятых     (яблока)   П.П.мн.ч.

        //И.П.: пять целых три десятых      (яблока)   Р.П.мн.ч.
        //Р.П.: пяти целых трех десятых     (яблока)   Р.П.мн.ч.
        //Д.П.: пяти целым трем десятым     (яблока)   Д.П.мн.ч.
        //В.П.: пять целые две десятых      (яблока)   Р.П.мн.ч.
        //Т.П.: пятью целыми двумя десятыми (яблока)   Т.П.мн.ч.
        //П.П.: пяти целых двух десятых     (яблока)   П.П.мн.ч.
        FormIndex = _FraqFormIndices0[(int)theCase];

        SpecAppend(sb, _FraqForms0[s2.Length - 1, FormIndex], specChars);
      }
      else // rusCount1
      {
        // Для форм, закачнивающихся на 1
        //И.П.: одна целая одна десятая     (яблока)   И.П.ед.ч.
        //Р.П.: одной целой одной десятой   (яблока)   Р.П.ед.ч.
        //Д.П.: одной целой одной десятой   (яблока)   Д.П.ед.ч.
        //В.П.: одну целую одну десятую     (яблока)   В.П.ед.ч.
        //Т.П.: одной целой одной десятой   (яблока)   Т.П.ед.ч.
        //П.П.: одной целой одной десятой   (яблока)   П.П.нд.ч.

        // падеж для слова "десятая", "сотая" и т.д. совпадает с формой слова
        // Однако, родительный, дательный, творительный и предложный падежы имеют
        // одинаковую форму и их можно не дублировать

        FormIndex = _FraqFormIndices1[(int)theCase];

        SpecAppend(sb, _FraqForms1[s2.Length - 1, FormIndex], specChars);
      }
    }

    #endregion

    #region Сопряжение существительного и числительного

    /// <summary>
    /// Сопряжение существительного с числительным в именительном падеже. 
    /// Число выводится цифрой.
    /// Например, вызов 
    /// string s = "У меня есть "+IntWithNoun(3, "кошка", "кошки", "кошек")
    /// вернет s = "У меня есть 3 кошки"
    /// </summary>
    /// <param name="value">Числовое значение</param>
    /// <param name="nominativeSingular">Форма именительного падежа единственного числа.
    /// Используется, если число заканчивается на 1</param>
    /// <param name="genitiveSingular">Форма родительного падежа единственного  числа. 
    /// Используется, если число заканчивается на 2, 3 или 4</param>
    /// <param name="genitivePlural">Форма родительного падежа множественного числа.
    /// Используется, если число заканчивается на 0,5,6,7,8,9</param>
    /// <returns>Строка, состоящая из числа, пробела и одной из трех форм слова</returns>
    public static string IntWithNoun(int value, string nominativeSingular,
      string genitiveSingular, string genitivePlural)
    {
      return value.ToString() + " " + NounForInt(value, nominativeSingular,
        genitiveSingular, genitivePlural);
    }

    /// <summary>
    /// Сопряжение существительного с числительным в именительном падеже. 
    /// Число выводится цифрой.
    /// Например, вызов 
    /// string s = "У меня есть "+IntWithNoun(3, "кошка", "кошки", "кошек")
    /// вернет s = "У меня есть 3 кошки"
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    /// <param name="value">Числовое значение</param>
    /// <param name="nominativeSingular">Форма именительного падежа единственного числа.
    /// Используется, если число заканчивается на 1</param>
    /// <param name="genitiveSingular">Форма родительного падежа единственного  числа. 
    /// Используется, если число заканчивается на 2, 3 или 4</param>
    /// <param name="genitivePlural">Форма родительного падежа множественного числа.
    /// Используется, если число заканчивается на 0,5,6,7,8,9</param>
    public static void IntWithNoun(StringBuilder sb, int value, string nominativeSingular,
      string genitiveSingular, string genitivePlural)
    {
      sb.Append(value);
      sb.Append(" ");
      sb.Append(NounForInt(value, nominativeSingular,
        genitiveSingular, genitivePlural));
    }

    /// <summary>
    /// Сопряжение существительного с числительным в именительном падеже. 
    /// То же, что и IntWithNoun(), но без вывода самого числа.
    /// Например, вызов 
    /// string s = NounForInt(3, "кошка", "кошки", "кошек")
    /// вернет s = "кошки"
    /// </summary>
    /// <param name="value">Числовое значение</param>
    /// <param name="nominativeSingular">Форма именительного падежа единственного числа.
    /// Используется, если число заканчивается на 1</param>
    /// <param name="genitiveSingular">Форма родительного падежа единственного  числа. 
    /// Используется, если число заканчивается на 2, 3 или 4</param>
    /// <param name="genitivePlural">Форма родительного падежа множественного числа.
    /// Используется, если число заканчивается на 0,5,6,7,8,9</param>
    /// <returns>Строка, состоящая из числа, пробела и одной из трех форм слова</returns>
    public static string NounForInt(int value, string nominativeSingular,
      string genitiveSingular, string genitivePlural)
    {
      CountForm cntform = GetC100(value);
      switch (cntform)
      {
        case CountForm.Count1:
          return nominativeSingular;
        case CountForm.Count2:
          return genitiveSingular;
        case CountForm.Count0:
          return genitivePlural;
      }
      throw new Exception("Неправильнапя форма числа");
    }

    #endregion
  }
}
