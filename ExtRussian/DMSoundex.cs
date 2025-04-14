// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Russian.PhoneticAlgorithms
{
  /// <summary>
  /// Daitch–Mokotoff Soundex.
  /// Используйте метод <see cref="Calculate(string)"/> для получения кодов.
  /// </summary>
  public static class DMSoundex
  {
    private static readonly string[] _EmptyStrings = new string[0];
    private static readonly int[] _EmptyInts = new int[0];


    // Описание процесса кодирования здесь:
    // http://www.jewishgen.org/InfoFiles/soundex.html

    // Расширение для использования русского языка здесь:
    // https://github.com/iourinski/DMSoundex/blob/master/DMSoundex.pm

    #region Статические таблицы

    /// <summary>
    /// Полный список допустимых символов
    /// </summary>
    private static Dictionary<char, object> _ValidChars;

    /// <summary>
    /// Общие коды.
    /// Ключ: символ или комбинация символов, подлежащая кодированию.
    /// Значение: массив строк, как правило, из одного элемента.
    /// Элемент массива содержит строку из, как правило, одного символа, задающих коды.
    /// Если массив содержит пустую строку, последовательность не кодируется.
    /// Если массив содержит несколько элементов, происходит "ветвление", и в результате 
    /// будет несколько кодов.
    /// </summary>
    private static Dictionary<string, string[]> _Generic;

    /// <summary>
    /// Коды, когда комбинация находится в начале слова
    /// </summary>
    private static Dictionary<string, string[]> _Beginning;

    /// <summary>
    /// Коды, если следующий символ является гласной
    /// </summary>
    private static Dictionary<string, string[]> _BeforeVowel;

    /// <summary>
    /// Максимальная длина кодируемой последовательности символов в исходной строке
    /// </summary>
    private static int _MaxSeqLength;

    /// <summary>
    /// Гласные буквы.
    /// Ключ - символ буквы
    /// Значение - null (не используется)
    /// </summary>
    private static Dictionary<char, object> _Vowels;

    /// <summary>
    /// Статический конструктор инициализирует таблицы
    /// </summary>
    static DMSoundex()
    {
      FillCodes();
    }

    private static void FillCodes()
    {
      #region Допустимые символы

      string aValidChars = "abcdefghijklmnopqrstuvwxyz" + "абвгдежзийклмнопрстуфхцчшщъыьэюя";
      _ValidChars = new Dictionary<char, object>(aValidChars.Length);
      for (int i = 0; i < aValidChars.Length; i++)
        _ValidChars.Add(aValidChars[i], null);

      #endregion

      #region Кодировочная таблица

      _Beginning = new Dictionary<string, string[]>();
      _BeforeVowel = new Dictionary<string, string[]>();
      _Generic = new Dictionary<string, string[]>();

      _Codes2Dict = new Dictionary<string, string[]>();

      // Таблица взята из:
      // http://www.jewishgen.org/InfoFiles/soundex.html
      // Убраны польские и румынские буквы с закорючками

      // 23.08.2017
      // Убрал для комбинаций, начинающихся с A, E, I, O, U, J, and Y код для начала слова

      AddCodes("AI,AJ,AY", /*"0"*/null, "1", "NC");
      AddCodes("AU", /*"0"*/null, "7", "NC");
      AddCodes("A", "0", "NC", "NC");
      AddCodes("B", "7");
      AddCodes("CHS", "5", "54", "54");
      AddCodes("CH", "5/4");
      AddCodes("CK", "5/45");
      AddCodes("CZ,CS,CSZ,CZS", "4");
      AddCodes("C", "5/4");
      AddCodes("DRZ,DRS", "4");
      AddCodes("DS,DSH,DSZ", "4");
      AddCodes("DZ,DZH,DZS", "4");
      AddCodes("D,DT", "3");
      AddCodes("EI,EJ,EY", /*"0"*/null, "1", "NC");
      AddCodes("EU", /*"1"*/null, "1", "NC");
      AddCodes("E", "0", "NC", "NC");
      AddCodes("FB", "7");
      AddCodes("F", "7");
      AddCodes("G", "5");
      AddCodes("H", "5", "5", "NC");
      AddCodes("IA,IE,IO,IU", /*"1"*/null, "NC", "NC");
      AddCodes("I", "0", "NC", "NC");
      AddCodes("J", "1/4");
      AddCodes("KS", "5", "54", "54");
      AddCodes("KH", "5");
      AddCodes("K", "5");
      AddCodes("L", "8");
      AddCodes("MN", null, "66", "66");
      AddCodes("M", "6");
      AddCodes("NM", null, "66", "66");
      AddCodes("N", "6");
      AddCodes("OI,OJ,OY", /*"0"*/null, "1", "NC");
      AddCodes("O", "0", "NC", "NC");
      AddCodes("P,PF,PH", "7");
      AddCodes("Q", "5");
      AddCodes("RZ,RS", "94/4");
      AddCodes("R", "9");
      AddCodes("SCHTSCH,SCHTSH,SCHTCH", "2", "4", "4");
      AddCodes("SCH", "4");
      AddCodes("SHTCH,SHCH,SHTSH", "2", "4", "4");
      AddCodes("SHT,SCHT,SCHD", "2", "43", "43");
      AddCodes("SH", "4");
      AddCodes("STCH,STSCH,SC", "2", "4", "4");
      AddCodes("STRZ,STRS,STSH", "2", "4", "4");
      AddCodes("ST", "2", "43", "43");
      AddCodes("SZCZ,SZCS", "2", "4", "4");
      AddCodes("SZT,SHD,SZD,SD", "2", "43", "43");
      AddCodes("SZ", "4");
      AddCodes("S", "4");
      AddCodes("TCH,TTCH,TTSCH", "4");
      AddCodes("TH", "3");
      AddCodes("TRZ,TRS", "4");
      AddCodes("TSCH,TSH", "4");
      AddCodes("TS,TTS,TTSZ,TC", "4");
      AddCodes("TZ,TTZ,TZS,TSZ", "4");
      AddCodes("T", "3");
      AddCodes("UI,UJ,UY", /*"0"*/null, "1", "NC");
      AddCodes("UE", /*"0"*/null, "NC", "NC");
      AddCodes("U", "0", "NC", "NC");
      AddCodes("V", "7");
      AddCodes("W", "7");
      AddCodes("X", "5", "54", "54");
      AddCodes("Y", "1", "NC", "NC");
      AddCodes("ZDZ,ZDZH,ZHDZH", "2", "4", "4");
      AddCodes("ZD,ZHD", "2", "43", "43");
      AddCodes("ZH,ZS,ZSCH,ZSH", "4");
      AddCodes("Z", "4");

      AddCodes("ай,ау", "0", "1", "NC");
      AddCodes("а", "0", "NC", "NC");
      AddCodes("б", "7");
      AddCodes("в", "7");
      AddCodes("г", "5");
      AddCodes("дж", "4");
      AddCodes("дрз", "4");
      AddCodes("дщ,дш", "4");
      AddCodes("д,дт", "3");
      AddCodes("ей,еу", "0", "1", "NC");
      AddCodes("е", "0/1", "NC", "NC"); // может быть буква "ё"
      AddCodes("ж", "4");
      AddCodes("здз,здж,ждж", "2", "4", "4");
      AddCodes("з", "4");
      AddCodes("иа,ио,иу", "1", "NC", "NC");
      AddCodes("и", "1", "NC", "NC");
      AddCodes("й", "1", "NC", "NC");
      AddCodes("кс", "5", "54", "54");
      AddCodes("к", "5");
      AddCodes("л", "8");
      AddCodes("мн", null, "66", "66");
      AddCodes("м", "6");
      AddCodes("нм", null, "66", "66");
      AddCodes("н", "6");
      AddCodes("ой", "0", "1", "NC");
      AddCodes("о", "0", "NC", "NC");
      AddCodes("пф", "7");
      AddCodes("п", "7");
      AddCodes("рщ,рш", "4");
      AddCodes("р", "9");
      AddCodes("стж", "4");
      AddCodes("стр,стрс", "2", "4", "4"); // ?
      AddCodes("сд", "2", "43", "43"); // ?
      AddCodes("с", "4");
      AddCodes("тс,тш,тч,ттч,ттс,тц", "4");
      AddCodes("тж,тз", "4");
      AddCodes("т", "3");
      AddCodes("уй", "0", "1", "NC");
      AddCodes("у,уе", "0", "NC", "NC");
      AddCodes("фб", "7");
      AddCodes("ф", "7");
      AddCodes("х", "5");
      AddCodes("ц", "4");
      AddCodes("чт", "2", "43", "43");
      AddCodes("ч,чс", "4");
      AddCodes("шд", "2", "43", "43");
      AddCodes("шт", "2", "43", "43");
      AddCodes("шщ", "4");
      AddCodes("шч,сч", "4");
      AddCodes("ш", "4");
      AddCodes("щт", "2", "43", "43");
      AddCodes("щ", "4");
      AddCodes("ъ,ь", "NC");
      AddCodes("ы", "0", "NC", "NC");
      AddCodes("э", "1", "NC", "NC");
      AddCodes("ю", "1", "NC", "NC");
      AddCodes("я", "1", "NC", "NC");

#if DEBUG
      CheckDict(_Beginning);
      CheckDict(_BeforeVowel);
      CheckDict(_Generic);
#endif

      _Codes2Dict = null; // очищаем ненужный список

      #endregion

      #region Гласные

      string aVowels = "aouiey" + "аеиоуэюя"/*ё*/;

      _Vowels = new Dictionary<char, object>(aVowels.Length);
      for (int i = 0; i < aVowels.Length; i++)
        _Vowels.Add(aVowels[i], null);

      #endregion
    }

    private static void AddCodes(string keys, string xGeneric)
    {
      AddCodes(keys, xGeneric, xGeneric, xGeneric);
    }

    private static void AddCodes(string keys, string xBegining, string xBeforeVowel, string xGeneric)
    {
      keys = keys.ToLowerInvariant();
      string[] aKeys = keys.Split(',');
      for (int i = 0; i < aKeys.Length; i++)
      {
        string key = aKeys[i];
#if DEBUG
        for (int j = 0; j < key.Length; j++)
        {
          if (!_ValidChars.ContainsKey(key[j]))
            throw new BugException("Key \"" + key + "\" contains bad char \"" + key[j] + "\"");
        }
#endif

        AddCodes2(_Beginning, key, xBegining);
        AddCodes2(_BeforeVowel, key, xBeforeVowel);
        AddCodes2(_Generic, key, xGeneric);
        _MaxSeqLength = Math.Max(_MaxSeqLength, key.Length);
      }
    }

    /// <summary>
    /// Используется только в процессе инициализации.
    /// На нужно получить мнжество маленьких массивов, вида {"1"}.
    /// Было бы плохо использовать отдельные экземпляры для одинаковых массивов
    /// </summary>
    private static Dictionary<string, string[]> _Codes2Dict;

    /// <summary>
    /// Коды для некодируемой последовательности символов "NC"
    /// </summary>
    private static readonly string[] _NonCodedCodes = new string[1] { String.Empty };

    private static void AddCodes2(Dictionary<string, string[]> dict, string key, string xValue)
    {
      if (String.IsNullOrEmpty(xValue))
        return; // комбинация не используется. Это не тоже самое, что "не кодируется"

      string[] aValues;
      if (!_Codes2Dict.TryGetValue(xValue, out aValues))
      {
        if (xValue == "NC") // не кодируется
          aValues = _NonCodedCodes;
        else
        {
          aValues = xValue.Split('/');
#if DEBUG
          if (aValues.Length == 0)
            throw new BugException("There are no codes for key \"" + key + "\"");
          for (int i = 0; i < aValues.Length; i++)
          {
            if (String.IsNullOrEmpty(aValues[i]))
              throw new BugException("Empty value when split \"" + xValue + "\"");
            for (int j = 0; j < aValues[i].Length; j++)
            {
              char ch = aValues[i][j];
              if (ch < '0' || ch > '9')
                throw new BugException("Invalid char \"" + ch + "\" in value \"" + xValue + "\"");
            }
          }
#endif
        }
        _Codes2Dict.Add(xValue, aValues);
      }

      dict.Add(key, aValues);
    }


#if DEBUG

    private static void CheckDict(Dictionary<string, string[]> dict)
    {
      #region Проверка символов в последовательностях

      foreach (KeyValuePair<string, string[]> pair in dict)
      {
        string s = pair.Key;
        for (int i = 0; i < s.Length; i++)
        {
          if (!_ValidChars.ContainsKey(s[i]))
            throw new BugException("Secuence \"" + s + "\" contains invalid char \"" + s[i] + "\"");
        }
      }

      #endregion

      #region Проверка полноты последовательностей

      foreach (KeyValuePair<char, object> pair in _ValidChars)
      {
        string s = new string(pair.Key, 1);
        if (!dict.ContainsKey(s))
          throw new BugException("Dictionary does not contain a single char \"" + s + "\" (0x" + ((int)pair.Key).ToString("x") + ")");
      }

      #endregion
    }

#endif

    #endregion

    #region Накопление кодов

    // Можно было бы, конечно, использовать простой String или StringBuilder, но есть возможность
    // использовать числовые значения, а не выделять строки

    private struct CodeAccumulator
    {
      #region Свойства

      /// <summary>
      /// Число в диапазоне от 0 до 999999 в процессе накопления
      /// </summary>
      public int Value { get { return _Value; } }
      private int _Value;

      /// <summary>
      /// Количество накопленных значений в диапазоне от 0 до 6
      /// </summary>
      public int Count { get { return _Count; } }
      private int _Count;

      #endregion

      #region Добавление кода

      private static readonly int[] _Muls = new int[6] { 100000, 10000, 1000, 100, 10, 1 };

      /// <summary>
      /// Добавление одного кода.
      /// Добавление более, чем шестого символа, отбрпасывается
      /// </summary>
      /// <param name="curr">Текущее накопленное значение</param>
      /// <param name="ch">Символ в диапазоне от '0' до '9' </param>
      /// <returns>Новое накопленное значение</returns>
      public static CodeAccumulator operator +(CodeAccumulator curr, char ch)
      {
#if DEBUG
        if (ch < '0' || ch > '9')
          throw new ArgumentException("ch");
#endif

        int nCh = ch - '0'; // в диапазоне от 0 до 9

        if (curr._Count >= 6)
          return curr; // отбрасываем

        int v2 = _Muls[curr._Count] * nCh;

        CodeAccumulator next = new CodeAccumulator();
        next._Count = curr._Count + 1;
        next._Value = curr._Value + v2;
        return next;
      }

      /// <summary>
      /// Добавление нескольких кода.
      /// Добавление более, чем шестого символа, отбрпасывается
      /// </summary>
      /// <param name="curr">Текущее накопленное значение</param>
      /// <param name="chars">Символы в диапазоне от '0' до '9' </param>
      /// <returns>Новое накопленное значение</returns>
      public static CodeAccumulator operator +(CodeAccumulator curr, string chars)
      {
        CodeAccumulator res = curr;
        for (int i = 0; i < chars.Length; i++)
          res += chars[i];
        return res;
      }

      #endregion

      #region Текстовое представление

      public override string ToString()
      {
        return Value.ToString("000000");
      }

      #endregion

      #region Статические свойства

      public static readonly CodeAccumulator[] EmptyArray = new CodeAccumulator[0];

      #endregion
    }

    #endregion

    #region Основной методы вычисления

    /// <summary>
    /// Вычисляет функцию Daitch–Mokotoff Soundex.
    /// Возвращает список 6-символьных кодов, представляемых в текстовом формате.
    /// Обычно возвращается единственный код, но некоторые комбинации символов могут
    /// порождать несколько вариантов.
    /// </summary>
    /// <param name="s">Преобразуемая строка </param>
    /// <returns>Массив кодов DM-Soundex</returns>
    public static string[] Calculate(string s)
    {
      CodeAccumulator[] a = DoCalculate(s, null);
      if (a.Length == 0)
        return _EmptyStrings;

      string[] a2 = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = a[i].ToString();
      return a2;
    }

    /// <summary>
    /// Вычисляет функцию Daitch–Mokotoff Soundex.
    /// Возвращает список 6-символьных кодов, представляемых в текстовом формате.
    /// Обычно возвращается единственный код, но некоторые комбинации символов могут
    /// порождать несколько вариантов.
    /// Версия, выдающая отладочную информацию.
    /// </summary>
    /// <param name="s">Преобразуемая строка</param>
    /// <param name="debugInfo">Сюда помещается отладочная информация</param>
    /// <returns>Массив кодов DM-Soundex</returns>
    public static string[] Calculate(string s, out DMSoundexCodingPart[] debugInfo)
    {
      List<DMSoundexCodingPart> debugList = new List<DMSoundexCodingPart>();
      CodeAccumulator[] a = DoCalculate(s, debugList);
      debugInfo = debugList.ToArray();

      if (a.Length == 0)
        return _EmptyStrings;

      string[] a2 = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = a[i].ToString();
      return a2;
    }

    /// <summary>
    /// Вычисляет функцию Daitch–Mokotoff Soundex.
    /// Возвращает список числовых кодов в диапазоне от 0 до 999999.
    /// Обычно возвращается единственный код, но некоторые комбинации символов могут
    /// порождать несколько вариантов.
    /// </summary>
    /// <param name="s">Преобразуемая строка </param>
    /// <returns>Массив кодов DM-Soundex</returns>
    public static int[] CalculateInt(string s)
    {
      CodeAccumulator[] a = DoCalculate(s, null);
      if (a.Length == 0)
        return _EmptyInts;

      int[] a2 = new int[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = a[i].Value;
      return a2;
    }

    #endregion

    #region Реализация вычисления

    private static CodeAccumulator[] DoCalculate(string s, List<DMSoundexCodingPart> debugList)
    {
      s = PrepareString(s);

      if (s.Length == 0)
        return CodeAccumulator.EmptyArray;

      // Чаще всего бывает один код, лищь иногда происходит "ветвление"
      CodeAccumulator singleCode = new CodeAccumulator(); // единственный результат
      List<CodeAccumulator> multiCodes = null; // множественный результат

      int pos = 0;
      while (pos < s.Length)
      {
        // Какую самую длинную последовательность можно найти
        int n = Math.Min(_MaxSeqLength, s.Length - pos);

        // Пытаемся найти последовательности в порядке укорачивания
        string[] codes = null;
        string seq = null;
        for (int len = n; len > 0; len--)
        {
          seq = s.Substring(pos, len); // проверяемая последовательность
          if (pos == 0)
            _Beginning.TryGetValue(seq, out codes);
          else
          {
            bool IsVowel = false;
            if ((pos + len) < s.Length)
            {
              char NextChar = s[pos + len];
              IsVowel = _Vowels.ContainsKey(NextChar);
            }
            if (IsVowel)
              _BeforeVowel.TryGetValue(seq, out codes);
            else
              _Generic.TryGetValue(seq, out codes);
          }
          if (codes != null)
            break;
        }

        // 23.08.2017
        // Выбрасываем парные буквы
        if (seq.Length == 1 && pos > 0)
        {
          if (s[pos - 1] == seq[0])
            codes = _NonCodedCodes;
        }

#if DEBUG
        if (codes == null)
          throw new BugException("codes==null");
        if (codes.Length == 0)
          throw new BugException("Empty codes for secuence \"" + seq + "\" not found");
#endif

        if (debugList != null)
        {
          DMSoundexCodingPart di = new DMSoundexCodingPart(seq, codes);
          debugList.Add(di);
        }

        if (codes.Length > 1)
        {
          // Переходим к множественным кодам
          if (multiCodes == null)
          {
            multiCodes = new List<CodeAccumulator>();
            multiCodes.Add(singleCode);
          }
          // Выполняем ветвление
          int n2 = multiCodes.Count;
          for (int i = 0; i < n2; i++)
          {
            CodeAccumulator baseCode = multiCodes[i];
            multiCodes[i] = baseCode + codes[0];
            for (int j = 1; j < codes.Length; j++)
              multiCodes.Add(baseCode + codes[j]);
          }
        }
        else // Codes.Length=1
        {
          // Простое добавление кодов
          if (multiCodes == null)
            singleCode += codes[0];
          else
          {
            for (int i = 0; i < multiCodes.Count; i++)
              multiCodes[i] += codes[0];
          }
        }


        pos += seq.Length;
      }

      if (multiCodes == null)
        return new CodeAccumulator[1] { singleCode };
      else
      {
        // Убираем возможно повторяющиеся коды
        for (int i = multiCodes.Count - 1; i >= 1; i--)
        {
          bool remove = false;
          for (int j = 0; j < i; j++)
          {
            if (multiCodes[i].Value == multiCodes[j].Value)
            {
              remove = true;
              break;
            }
          }
          if (remove)
            multiCodes.RemoveAt(i);
        }

        return multiCodes.ToArray();
      }
    }

    #endregion

    #region Подготовка строки

    /// <summary>
    /// Удаление всех символов, кроме ValidChars.
    /// Преведение к нижнему регистру.
    /// Замена "ё"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string PrepareString(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;

      s = s.ToLowerInvariant();
      s = s.Replace('ё', 'е');

      for (int i = 0; i < s.Length; i++)
      {
        if (!_ValidChars.ContainsKey(s[i]))
        {
          // Есть плохой символ
          if (i == (s.Length - 1))
          {
            // плохим оказался последний символ
            return s.Substring(0, s.Length - 1);
          }

          StringBuilder sb = new StringBuilder(s.Length - 1);
          sb.Append(s, 0, i);
          for (int j = i + 1; j < s.Length; j++)
          {
            if (_ValidChars.ContainsKey(s[j]))
              sb.Append(s[j]);
          }
          return sb.ToString();
        }
      }

      // Все символы хорошие
      return s;
    }

    #endregion
  }

  /// <summary>
  /// Отладочная информация о разбиении строки на коды DM-Soundex.
  /// Для строки может быть получен массив таких структур.
  /// </summary>
  [Serializable]
  public struct DMSoundexCodingPart
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="sequence">Последовательность символов в исходной строке</param>
    /// <param name="codes">Примененные коды</param>
    internal DMSoundexCodingPart(string sequence, string[] codes)
    {
      _Sequence = sequence;
      _Codes = codes;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Последовательность символов в исходной строке
    /// </summary>
    public string Sequence { get { return _Sequence; } }
    private readonly string _Sequence;

    /// <summary>
    /// Примененные коды.
    /// </summary>
    public string[] Codes { get { return _Codes; } }
    private readonly string[] _Codes;

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Возвращает текстовое представление в виде "Sequence"=Codes
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      ToString(sb);
      return sb.ToString();
    }

    /// <summary>
    /// Возвращает текстовое представление в виде "Sequence"=Codes
    /// </summary>
    /// <param name="sb">Буфер для заполнения</param>
    public void ToString(StringBuilder sb)
    {
      sb.Append("\"");
      sb.Append(Sequence);
      sb.Append("\"=");
      CodesToString(sb, Codes);
    }

    private static void CodesToString(StringBuilder sb, string[] codes)
    {
      if (codes == null)
        sb.Append("null");
      if (codes.Length == 1)
      {
        if (codes[0].Length == 0)
          sb.Append("NC");
        else
          sb.Append(codes[0]);
      }
      else
      {
        sb.Append("[");
        for (int i = 0; i < codes.Length; i++)
        {
          if (i > 0)
            sb.Append(",");
          sb.Append(codes[i]);
        }
        sb.Append("]");
      }
    }

    /// <summary>
    /// Выводит части через запятую
    /// </summary>
    /// <param name="info">Массив частей</param>
    /// <returns>Текстовое представление</returns>
    public static string ToString(DMSoundexCodingPart[] info)
    {
      StringBuilder sb = new StringBuilder();
      ToString(info, sb);
      return sb.ToString();
    }

    private static void ToString(DMSoundexCodingPart[] info, StringBuilder sb)
    {
      for (int i = 0; i < info.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");
        info[i].ToString(sb);
      }
    }

    #endregion
  }
}
