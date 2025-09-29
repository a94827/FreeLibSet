using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Collections
{
  /// <summary>
  /// Компаратор для строк, содержащих числовые последовательности.
  /// Может использоваться, например, для сортировки IP-адресов, когда "192.168.0.9" меньше, чем "192.168.0.10".
  /// Предполагается, что сравниваемые строки содержат последовательности цифр, которые надо интерпретировать как целые числа без знака.
  /// Под цифрами понимаются символы '0' - '9'.
  /// Все остальные символы сравниваются "как есть", с возможностью игнорирования регистра.
  /// Для числовых частей ведущие нули имеют значение, "A01B" меньше, чем "A1B", но больше, чем "A001B" (так как числовое значение везде равно 1, используется сравнение строк "01", "1" и "001" соответственно).
  /// </summary>
  public sealed class NumericStringComparer : IComparer<string>
  {
    #region Конструктор

    /// <summary>
    /// Создает компаратор
    /// </summary>
    /// <param name="baseComparer">Базовый компаратор, используемый для сравнения нечисловых фрагментов.
    /// Если null, то используется стандартный компаратор <see cref="Comparer{String}"/></param>
    public NumericStringComparer(IComparer<string> baseComparer)
    {
      if (baseComparer == null)
        _BaseComparer = Comparer<string>.Default;
      else
        _BaseComparer = baseComparer;
    }

    /// <summary>
    /// Создает компаратор, используя <see cref="Comparer{String}"/> в качестве базового
    /// </summary>
    public NumericStringComparer()
      : this(Comparer<string>.Default)
    {
    }

    private readonly IComparer<string> _BaseComparer;

    #endregion

    #region IComparer<string> Members

    /// <summary>
    /// Основной метод - сравнение двух строк в целях сортировки
    /// </summary>
    /// <param name="x">Первая сравниваемая строка</param>
    /// <param name="y">Вторая сравниваемая строка</param>
    /// <returns>Результат сравнения</returns>
    public int Compare(string x, string y)
    {
      if (x == null)
        x = String.Empty;
      if (y == null)
        y = String.Empty;

      int posX = 0;
      int posY = 0;
      while (true)
      {
        string partX = GetNext(x, ref posX);
        string partY = GetNext(y, ref posY);
        if (partX.Length == 0)
        {
          if (partY.Length == 0)
            return 0;
          else
            return -1;
        }
        else if (partY.Length == 0)
          return +1;

        #region Числовое сравнение

        if (partX[0] >= '0' && partX[0] <= '9' && partY[0] >= '0' && partY[0] <= '9')
        {
          ulong vx, vy;
          UInt64.TryParse(partX, out vx);
          UInt64.TryParse(partY, out vy);

          if (vx != vy)
            return vx > vy ? 1 : -1;
        }

        #endregion

        #region Строковое сравнение

        int res2 = _BaseComparer.Compare(partX, partY);
        if (res2 != 0)
          return res2;

        #endregion
      }
    }

    /// <summary>
    /// Возвращает очередную порцию кода, цифровую или не-цифровую.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <param name="pos">Позиция, начиная с которой нужно просматривать <paramref name="s"/>.
    /// В процессе просмотра кода позиция увеличивается, чтобы ее можно было применить при следующем вызове метода.</param>
    /// <returns>Очередная порция строки или пустая строка</returns>
    private static string GetNext(string s, ref int pos)
    {
      if (pos >= s.Length)
        return String.Empty;

      int startPos = pos;
      bool isDigitPart = s[pos] >= '0' && s[pos] <= '9';
      pos++;
      while (pos < s.Length)
      {
        bool thisIsDigit = s[pos] >= '0' && s[pos] <= '9';
        if (thisIsDigit != isDigitPart)
          break;
        pos++;
      }

      if (startPos == 0 && pos == s.Length)
        return s; // Если весь код является числовым или символьным, он возвращается как единственная часть.
      else
        return s.Substring(startPos, pos - startPos);
    }

    #endregion

    #region Статический экземпляры

    /// <summary>
    /// Выполняет сравнение нечисловых последовательностей по кодам символов.
    /// Подходит, например, для сравнения адресов IPv4.
    /// </summary>
    public static readonly NumericStringComparer Ordinal = new NumericStringComparer(StringComparer.Ordinal);

    /// <summary>
    /// Выполняет сравнение нечисловых последовательностей по кодам символов без учета регистра
    /// </summary>
    public static readonly NumericStringComparer OrdinalIgnoreCase = new NumericStringComparer(StringComparer.OrdinalIgnoreCase);

    #endregion
  }
}
