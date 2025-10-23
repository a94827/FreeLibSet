using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.Remoting;

namespace FreeLibSet.Parsing
{

  /// <summary>
  /// Буферизация при создании выражений.
  /// Класс является потокобезопасным, так все поля являются привязанными к потоку [ThreadStatic].
  /// </summary>
  public static class ParsingShareTools
  {
    #region Свойство CurrentBuffer

    /// <summary>
    /// Начать буферизацию.
    /// Используйте этот метод перед началом массового создания вычисляемых выражений.
    /// В блоке try/finally обязательно должен быть парный вызов метода EndBuffering().
    /// Допускаются вложенные вызовы.
    /// Метод неявно вызывается в ParserList.CreateExpression(), поэтому при одиночном создании выражений нет смысла вызывать BeginBuffering() из прикладного кода.
    /// </summary>
    public static void BeginBuffering()
    {
      _BufferingLevel++;
    }

    /// <summary>
    /// Парный вызов, соответствующий BeginBuffering()
    /// </summary>
    public static void EndBuffering()
    {
      if (_BufferingLevel == 0)
        throw new InvalidOperationException();
      _BufferingLevel--;
      if (_BufferingLevel == 0)
        _CurrentBuffer = null;
    }

    [ThreadStatic]
    private static int _BufferingLevel;

    /// <summary>
    /// Возвращает текущий буфер, используемый при создании выражений.
    /// Возвращаемое значение относится к текущему потоку выполнения.
    /// Если в настоящий момент не выполняется создание выражений, возвращает null.
    /// </summary>
    public static NamedValues CurrentBuffer 
    { 
      get 
      {
        if (Object.ReferenceEquals(_CurrentBuffer, null) && _BufferingLevel > 0)
          _CurrentBuffer = new NamedValues();
        return _CurrentBuffer; 
      } 
    }

    [ThreadStatic]
    private static NamedValues _CurrentBuffer;

    #endregion

    #region ShareValue

    private interface ISharedCollection
    {
      object this[object value] { get; }
      int Count { get; } // для отладки
    }

    private class SharedCollection<T>:ISharedCollection
    {
      #region Конструктор

      public SharedCollection()
      {
        _List = new System.Collections.ArrayList();
        _Positions = new Dictionary<T, int>();
      }

      #endregion

      #region Свойства

      private readonly System.Collections.ArrayList _List;
      private readonly Dictionary<T, int> _Positions;

      public object this[T value]
      {
        get
        {
          int pos;
          if (_Positions.TryGetValue(value, out pos))
            return _List[pos];

          _List.Add((object)value);
          _Positions.Add(value, _List.Count - 1);
          return value;
        }
      }

      public int Count { get { return _List.Count; } }

      #endregion

      #region ISharedCollection Members

      object ISharedCollection.this[object value]
      {
        get { return this[(T)value]; }
      }

      #endregion
    }

    /// <summary>
    /// Проверяет, было ли такое значение уже использовано в текущем сеансе буферизации.
    /// Если да, то возвращается ранее использованное значение. Иначе возвращается <paramref name="value"/>,
    /// которое сохраняется для дальнейшего использования.
    /// Для значения null возвращается null.
    /// Значение может быть ссылочного или значимого типа и должно поддерживать эффективный механизм сравнения.
    /// </summary>
    /// <param name="value">Входящее значение</param>
    /// <returns>Ранее сохраненное или переданное значение</returns>
    public static object ShareValue(object value)
    {
      if (value == null)
        return null;

      Type tDict = typeof(SharedCollection<>).MakeGenericType(new Type[1]{value.GetType()});
      ISharedCollection dict = (ISharedCollection)(CurrentBuffer.GetOrCreate(tDict, value.GetType().Name));
      return dict[value];
    }

    #endregion

    #region ShareConstExpression()

    /// <summary>
    /// Создает или возвращает существующий объект <see cref="ConstExpression"/>, используя буферизацию.
    /// Метод используется внутри IParser.GetExpression().
    /// </summary>
    /// <param name="value">Константное выражение</param>
    /// <returns>Объект выражениея</returns>
    public static ConstExpression ShareConstExpression(object value)
    {
      //string sValue = value as string;
      //if (sValue != null)
      //  value = ShareString(sValue);
      value = ShareValue(value);

      string typeName;
      object key;
      if (!Object.ReferenceEquals(value, null))
      {
        typeName = value.GetType().Name;
        key = value;
      }
      else
      {
        typeName = "Null";
        key = String.Empty;
      }

      Dictionary<object, ConstExpression> dict = CurrentBuffer.GetOrCreate<Dictionary<object, ConstExpression>>("ConstExpression_" + typeName);
      ConstExpression expr;
      if (!dict.TryGetValue(key, out expr))
      {
        expr = new ConstExpression(value);
        dict.Add(key, expr);
      }
      return expr;
    }

    #endregion
  }
}
