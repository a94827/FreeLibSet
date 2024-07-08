using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Reporting
{
  #region Перечисления

  /// <summary>
  /// Возможность переноса в данной позиции.
  /// Возвращаемое значение для индексированного свойства <see cref="BRWordWrapper"/>
  /// </summary>
  public enum BRWordWrapperPosType
  {
    /// <summary>
    /// В этой позиции перенос не допустим
    /// </summary>
    Disabled,

    /// <summary>
    /// В этой позиции находится пробел. В случае переноса данная позиция отбрасывается.
    /// </summary>
    Space,

    /// <summary>
    /// По этой позиции возможен перенос. В случае переноса этот символ должен 
    /// остаться в конце строки, а следующей - на новой
    /// </summary>
    Enabled,

    /// <summary>
    /// По этой позиции возможен перенос. В случае переноса после этого символа
    /// должен быть добавлен знак "-"
    /// </summary>
    Hyphen
  }

  #endregion

  /// <summary>
  /// Поддержка переноса по словам
  /// </summary>
  public class BRWordWrapper
  {
    #region Свойства

    /// <summary>
    /// Основное свойство должно быть установлено до вызова методов и получения других свойств. 
    /// Определяет текст, в котором определяются позиции, по которым возможен перенос по словам.
    /// </summary>
    public string Text
    {
      get { return _Text; }
      set
      {
        if (value == null)
          value = string.Empty;
        _Text = value;
        InitPositions();
      }
    }

    private string _Text;

    /// <summary>
    /// Возвращает количество символов в строке <see cref="Text"/>.
    /// </summary>
    public int Count { get { return _Items.Length; } }

    /// <summary>
    /// Возможность переноса текста в данной позиции
    /// </summary>
    /// <param name="index">Индекс символа в строке <see cref="Text"/></param>
    /// <returns>Признак возможности переноса</returns>
    public BRWordWrapperPosType this[int index] { get { return _Items[index]; } }
    private BRWordWrapperPosType[] _Items;

    #endregion

    #region Инициализация позиций переноса

    /// <summary>
    /// Инициализация свойства this
    /// Не доделано
    /// </summary>
    private void InitPositions()
    {
      _Items = new BRWordWrapperPosType[Text.Length];
      for (int i = 0; i < _Text.Length; i++)
      {
        switch (_Text[i])
        {
          case ' ':
            _Items[i] = BRWordWrapperPosType.Space;
            break;
          case '-':
            // По знаку "минус" перенос можно делать, только если слева и справа
            // от него буквы и не меньше, чем по две
            _Items[i] = BRWordWrapperPosType.Disabled;
            if (i > 1 && i < (_Text.Length - 2))
            {
              if (Char.IsLetter(_Text[i - 1]) &&
                Char.IsLetter(_Text[i - 2]) &&
                Char.IsLetter(_Text[i + 1]) &&
                Char.IsLetter(_Text[i + 2]))

                _Items[i] = BRWordWrapperPosType.Enabled;
            }
            break;
          case DataTools.SoftHyphenChar:
            _Items[i] = BRWordWrapperPosType.Hyphen;
            break;
          default:
            _Items[i] = BRWordWrapperPosType.Disabled;
            break;
        }
      }
    }

    #endregion

    #region Методы поиска

    /// <summary>
    /// Найти ближайший индекс позиции переноса в указанном интервале.
    /// Если <paramref name="forward"/>=true, то отыскивается ближайшая позиция справа от <paramref name="firstIndex"/>.
    /// Иначе отыскивается ближайшая позиция слева от <paramref name="lastIndex"/>.
    /// Если в интервале (включительно) нет ни одной позиции переноса, то возвращается (-1)
    /// </summary>
    /// <param name="firstIndex">Начальный индекс в строке <see cref="Text"/></param>
    /// <param name="lastIndex">Конечный индекс в строке <see cref="Text"/></param>
    /// <param name="forward">Направление поиска</param>
    /// <returns>Индекс позиции переноса или -1</returns>
    public int FindPos(int firstIndex, int lastIndex, bool forward)
    {
#if DEBUG
      CheckIndexRange(firstIndex, lastIndex);
#endif

      if (forward)
      {
        for (int i = firstIndex; i <= lastIndex; i++)
        {
          if (_Items[i] != BRWordWrapperPosType.Disabled)
            return i;
        }
      }
      else
      {
        for (int i = lastIndex; i >= firstIndex; i--)
        {
          if (_Items[i] != BRWordWrapperPosType.Disabled)
            return i;
        }
      }
      return -1;
    }

    #endregion

    #region Поиск подходящей позиции при измерении ширины

    /*
     * Задача.
     * Есть строка текста и нужно определить оптимальную позицию переноса, 
     * проводя измерение ширины строки (символы могут иметь переменную ширину).
     * Желательно выполнять как можно меньше измерений, поэтому оптимальным
     * является метод деления строки пополам.
     * Сначала берется вся строка и отыскивается позиция переноса, ближайшая к
     * середине. Вычисляется ширина от начала строки до позиции (с учетом формата
     * переноса). Если длина больше требуемой, то отыскивается середина в левой
     * половине строки, иначе - в правой. Древовидный поиск выполняется до тех пор,
     * пока в очередной части строки не будет ни одной строки переноса
     * Алгоритм поиска
     * 
     *   WordWrapper wrp=new WordWrapper();
     *   wrp.Text="Здравствуй, мир!";
     *   wrp.WrapStart(); // Подготовка к началу поиска
     *   while(wrp.WrapHasLine()) // Пока есть еще строки
     *   {
     *     while(wrp.WrapNeedsMeasure()) // Пока требуется выполнять измерения
     *     {
     *       int w=Graphics.MeasureString(wrp.WrapString, ...); // Измеряем
     *       wrp.WrapRes = w<=MaxW; // Сообщаем результат: влезла ли часть строки
     *     }
     *     // Можно вывести часть строки
     *     Graphics.DrawString(wrp.WrapString, ...);
     *   }
     * 
     * WrapHasLine() и WrapNeedsMeasure() не могут быть свойствами, т.к. их вызов
     * меняет состояние объекта
     */

      /// <summary>
      /// Начать поиск позиций для переноса
      /// </summary>
    public void WrapStart()
    {
      _WrapNextPosition = 0;
      _WrapString = String.Empty;
    }

    /// <summary>
    /// Возвращает true, если есть еще строка для разбора
    /// </summary>
    /// <returns>Наличие строки</returns>
    public bool WrapHasLine()
    {
      _WrapFirstIndex = -1;
      _WrapLastIndex = -1;
      _WrapNeedsMeasureCount = -1;
      _WrapFoundPosition = -1;
      _WrapSecondPhase = false;
      return _WrapNextPosition < _Text.Length;
    }

    /// <summary>
    /// Счетчик вызовов функции WrapNeedsMeasure
    /// </summary>
    private int _WrapNeedsMeasureCount;

    /// <summary>
    /// false-первая фаза поиска (с учетом позиций переноса),
    /// true-вторая фаза поиска, если в первой фазе позиции переноса найдены не были
    /// </summary>
    private bool _WrapSecondPhase;

    /// <summary>
    /// Если в процессе измерений была найдена позиция переноса, которая поместилась,
    /// то индекс найденной позиции переноса. Иначе - (-1)
    /// </summary>
    private int _WrapFoundPosition;

    /// <summary>
    /// Возвращает true, если требуется провести измеренение размера строки <see cref="WrapString"/>.
    /// </summary>
    /// <returns>Потребность в измерении строки</returns>
    public bool WrapNeedsMeasure()
    {
      _WrapNeedsMeasureCount++;

      switch (_WrapNeedsMeasureCount)
      {
        case 0:
          // Первый вызов
          if (_WrapNextPosition == _Text.Length - 1)
          {
            // Остался последний символ строки. Измерять ничего не надо
            _WrapString = _Text.Substring(_WrapNextPosition);
            _WrapNextPosition = _Text.Length;
            return false;
          }
          _WrapFirstIndex = _WrapNextPosition;
          _WrapLastIndex = _Text.Length - 1;
          WrapPrepareString(_WrapFirstIndex, _WrapLastIndex);
          return true;
        case 1:
          if (_WrapRes)
          {
            // Если первое измерение вернуло true (вся строка поместилась)
            // то больше ничего делать не надоа
            _WrapNextPosition = _Text.Length;
            return false;
          }
          break;
        default:
          // До этого выполнялось измерение. Надо проанализировать WrapRes 
          if (_WrapRes)
          {
            _WrapFoundPosition = _WrapMiddleIndex;
            if (_WrapMiddleIndex == _WrapFirstIndex)
            {
              _WrapNextPosition = WrapPrepareString(_WrapNextPosition, _WrapMiddleIndex);
              return false;
            }
            _WrapFirstIndex = _WrapMiddleIndex;
          }
          else
          {
            if (_WrapMiddleIndex == _WrapLastIndex)
            {
              _WrapNextPosition = WrapPrepareString(_WrapNextPosition, _WrapMiddleIndex);
              return false;
            }
            _WrapLastIndex = _WrapMiddleIndex;
          }
          break;
      }

      _WrapMiddleIndex = (_WrapFirstIndex + _WrapLastIndex) / 2;
      if (!_WrapSecondPhase)
      {
        // Во второй фазе уже ничего не ищем
        _WrapMiddleIndex = FindPos(_WrapFirstIndex, _WrapMiddleIndex, false);
        if (_WrapMiddleIndex <= _WrapFirstIndex)
        {
          // Сразу сдаваться нельзя. Может быть, есть еще позиции.
          _WrapMiddleIndex = (_WrapFirstIndex + _WrapLastIndex) / 2;
          _WrapMiddleIndex = FindPos(_WrapMiddleIndex, _WrapLastIndex, true);
          if (_WrapMiddleIndex < 0 || _WrapMiddleIndex >= _WrapLastIndex)
          {
            // Больше нечего измерять. Диапазон исчерпан
            if (_WrapFoundPosition >= 0)
            {
              // До этого была найдена подходящая позиция переноса
              if (_WrapFoundPosition == _WrapNextPosition)
                _WrapFoundPosition++;
              _WrapNextPosition = WrapPrepareString(_WrapNextPosition, _WrapFoundPosition);
              return false;
            }
            if (_WrapLastIndex == _WrapFirstIndex)
            {
              _WrapNextPosition = WrapPrepareString(_WrapNextPosition, _WrapLastIndex);
              return false;
            }
            // Иначе ищем середину, игнорируя переносы
            _WrapMiddleIndex = (_WrapFirstIndex + _WrapLastIndex) / 2;
            _WrapSecondPhase = true;
            // измерения начинаем заново
            _WrapFirstIndex = _WrapNextPosition;
            _WrapLastIndex = _Text.Length - 1;
          }
        }
      } // первая фаза
      // Подготавливаем строку для измерения
      WrapPrepareString(_WrapNextPosition, _WrapMiddleIndex);
      return true;
    }

    /// <summary>
    /// Подготовить строку WrapString. Возвращает следующую позицию строки
    /// </summary>
    /// <param name="firstPos"></param>
    /// <param name="lastPos"></param>
    /// <returns></returns>
    private int WrapPrepareString(int firstPos, int lastPos)
    {
#if DEBUG
      CheckIndexRange(firstPos, lastPos);
#endif

      int w = lastPos - firstPos + 1;
      switch (_Items[lastPos])
      {
        case BRWordWrapperPosType.Space:
          // Перенос с удалением текущей позиции
          _WrapString = DataTools.RemoveSoftHyphens(_Text.Substring(firstPos, w - 1));
          return lastPos + 1;
        case BRWordWrapperPosType.Hyphen:
          // Добавление знака "-"
          _WrapString = DataTools.RemoveSoftHyphens(_Text.Substring(firstPos, w)) + "-";
          return lastPos + 1;
        default:
          // Перенос "по месту" без добавления / удаления символов
          _WrapString = DataTools.RemoveSoftHyphens(_Text.Substring(firstPos, w));
          return lastPos + 1;
      }

    }

    /// <summary>
    /// Индексы предыдущих измерений 
    /// </summary>
    private int _WrapFirstIndex, _WrapLastIndex, _WrapMiddleIndex;

    /// <summary>
    /// Строка, которую требуется измерить
    /// </summary>
    public string WrapString { get { return _WrapString; } }
    private string _WrapString;

    /// <summary>
    /// Сюда должен быть помещен результат измерения строки <see cref="WrapString"/>.
    /// </summary>
    public bool WrapRes { get { return _WrapRes; } set { _WrapRes = value; } }
    private bool _WrapRes;

    /// <summary>
    /// ??
    /// </summary>
    public int WrapNextPosition { get { return _WrapNextPosition; } }
    private int _WrapNextPosition;

    #endregion

    #region Вспомогательные методы

#if DEBUG
    private void CheckIndexRange(int firstIndex, int lastIndex)
    {
      if (_Items.Length == 0)
        throw new InvalidOperationException("Никакой диапазон символов не может быть правильным, т.к. строка пустая");
      if (firstIndex < 0 || firstIndex >= _Items.Length)
        throw new ArgumentOutOfRangeException("firstIndex", firstIndex,
          "Начальный индекс должен быть в диапазоне от 0 до " + (_Items.Length - 1).ToString());
      if (lastIndex < firstIndex || lastIndex >= _Items.Length)
        throw new ArgumentOutOfRangeException("lastIndex", lastIndex,
          "Конечный индекс должен быть в диапазоне от " + firstIndex.ToString() + " до " + (_Items.Length - 1).ToString());
    }

#endif

    #endregion
  }
}
