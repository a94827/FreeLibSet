﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Базовый класс "выражения", которое может быть обычным полем <see cref="DBxColumn"/>, 
  /// ссылочным полем (получаемым через JOIN), 
  /// константой <see cref="DBxConst"/>, 
  /// функцией <see cref="DBxFunction"/> или математическим выражением,
  /// агрегатной функцией <see cref="DBxAggregateFunction"/>.
  /// Выражения должны сериализоваться и быть классами "однократной записи".
  /// Должен быть переопределен метод <see cref="Object.Equals(object)"/>.
  /// </summary>
  [Serializable]
  public abstract class DBxExpression
  {
    /// <summary>
    /// Получить список используемых полей
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public abstract void GetColumnNames(DBxColumnList list);

    /// <summary>
    /// Получить значение выражения из произвольного источника данных
    /// </summary>
    /// <param name="rowValues">Источник данных</param>
    /// <returns>Значение</returns>
    public abstract object GetValue(INamedValuesAccess rowValues);

    /// <summary>
    /// Сравнение двух выражений на равенство.
    /// Возвращает true, если выражения одинаковы.
    /// </summary>
    /// <param name="a">Первое сравниваемое выражение</param>
    /// <param name="b">Второе сравниваемое выражение</param>
    /// <returns>Результат сравнени</returns>
    public static bool operator ==(DBxExpression a, DBxExpression b)
    {
      if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null))
        return true;
      if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
        return false;
      return a.Equals(b);
    }

    /// <summary>
    /// Сравнение двух выражений на равенство.
    /// Возвращает true, если выражения разные.
    /// </summary>
    /// <param name="a">Первое сравниваемое выражение</param>
    /// <param name="b">Второе сравниваемое выражение</param>
    /// <returns>Результат сравнени</returns>
    public static bool operator !=(DBxExpression a, DBxExpression b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Этот метод должен быть обязательно переопределен.
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public abstract override bool Equals(object obj);

    /// <summary>
    /// Этот метод должен быть обязательно переопределен в производном классе.
    /// </summary>
    /// <returns>Числовое значение для размещения в коллекции выражений</returns>
    public abstract override int GetHashCode();

    /// <summary>
    /// Возвращает константу, если выражение является константой.
    /// Для обычных выражений возвращается null.
    /// </summary>
    /// <returns>Константа или null</returns>
    public abstract DBxConst GetConst();

    /// <summary>
    /// Добавляет префикс ко всем полям, которые входят в выражение.
    /// Если метод вызывается для создания ссылочных полей, не забудьте добавить точку в конце префикса.
    /// </summary>
    /// <param name="prefix">Добавляемый префикс</param>
    /// <returns>Новое выражение. Для константы возвращается текущий объект</returns>
    public abstract DBxExpression SetColumnNamePrefix(string prefix);

    /// <summary>
    /// Добавляет в список все выражения в дереве.
    /// Для <see cref="DBxColumn"/> и <see cref="DBxConst"/> добавляет в список текущий объект.
    /// Для <see cref="DBxFunction"/> и <see cref="DBxAggregateFunction"/> также добавляет в список аргументы
    /// </summary>
    /// <param name="list">Заполняемый список</param>
    public virtual void GetAllExpressions(IList<DBxExpression> list)
    {
      list.Add(this);
    }
  }

  /// <summary>
  /// Выражение - поле
  /// </summary>
  [Serializable]
  public sealed class DBxColumn : DBxExpression
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для простого или составного поля.
    /// Не проверяется корректность имени, так как объект используется и для <see cref="System.Data.DataView"/>, где ограничений на имя практически нет.
    /// Единственное ограничение - не допускается запятая в имени.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public DBxColumn(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
      if (columnName.IndexOf(',') >= 0)
        throw ExceptionFactory.ArgInvalidChar("columnName", columnName, ",");

      _ColumnName = columnName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает имя поля
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private readonly string _ColumnName;
    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает имя поля
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return ColumnName;
    }

    /// <summary>
    /// Сравнение с другим объектом DBxColumn.
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      DBxColumn obj2 = obj as DBxColumn;
      if (obj2 == null)
        return false;
      else
        return this._ColumnName == obj2.ColumnName;
    }

    /// <summary>
    /// Хэш-значение.
    /// </summary>
    /// <returns>Числовое значение для размещения в коллекции выражений</returns>
    public override int GetHashCode()
    {
      return _ColumnName.GetHashCode();
    }

    /// <summary>
    /// Получить список используемых полей.
    /// Добавляет в список имя поля <see cref="ColumnName"/>.
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      list.Add(ColumnName);
    }

    /// <summary>
    /// Получить значение выражения из произвольного источника данных.
    /// Извлекает значение <see cref="ColumnName"/> из <paramref name="rowValues"/>.
    /// </summary>
    /// <param name="rowValues">Источник данных</param>
    /// <returns>Значение</returns>
    public override object GetValue(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);
      if (v is DBNull)
        return null;
      else
        return v;
    }

    /// <summary>
    /// Возвращает null
    /// </summary>
    /// <returns>null</returns>
    public override DBxConst GetConst()
    {
      return null;
    }

    /// <summary>
    /// Добавляет префикс к имени поля.
    /// </summary>
    /// <param name="prefix">Добавляемый префикс</param>
    /// <returns>Новое выражение DBxColumn</returns>
    public override DBxExpression SetColumnNamePrefix(string prefix)
    {
      return new DBxColumn(prefix + ColumnName);
    }

    #endregion
  }

  /// <summary>
  /// Константное выражение
  /// </summary>
  [Serializable]
  public sealed class DBxConst : DBxExpression
  {
    #region Конструктор

    /// <summary>
    /// Создает константу с заданным значением.
    /// Нельзя использовать значение null или <see cref="DBNull"/>, используйте перегрузку с аргументом <see cref="DBxColumnType"/>.
    /// Тип данных определяется из константы.
    /// </summary>
    /// <param name="value">Значение</param>
    public DBxConst(object value)
      : this(value, DBxColumnType.Unknown)
    {
    }


    /// <summary>
    /// Создает константу с заданным значением и типом данных.
    /// Значение DBNull заменяется на null.
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="columnType">Тип данных. Должен быть обязательно задан, если <paramref name="value"/>=null или DBNull</param>
    public DBxConst(object value, DBxColumnType columnType)
    {
      if (columnType == DBxColumnType.Unknown)
      {
        if (value == null || value is DBNull)
          throw new ArgumentNullException("value", Res.DBxConst_Arg_UseDBxColumnType);
        _Value = value;

        _ColumnType = DBxTools.ValueToColumnTypeRequired(value);
      }
      else
      {
        if (value is DBNull)
          _Value = null; // DBNull заменяем на null
        else
          _Value = value;
        _ColumnType = columnType;
      }
      CheckValueType(_Value);
    }

    private static void CheckValueType(object value)
    {
      if (value == null)
        return;

      if (DataTools.IsNumericType(value.GetType()))
        return;
      if (value is String)
        return;
      if (value is Boolean)
        return;
      if (value is DateTime)
        return;
      if (value is TimeSpan)
        return;
      if (value is Guid)
        return;

      throw ExceptionFactory.ArgUnknownType("value", value);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение константы.
    /// Значение DBNull заменено на null.
    /// </summary>
    public object Value { get { return _Value; } }
    private readonly object _Value;

    /// <summary>
    /// Тип данных для константы.
    /// Нужен при использовании в функции ISNULL.
    /// Всегда определен, не может быть Unknown.
    /// </summary>
    public DBxColumnType ColumnType { get { return _ColumnType; } }
    private readonly DBxColumnType _ColumnType;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Текстовое представление в виде "Значение (ТипПоля)" (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s;
      if (_Value == null)
        s = "NULL";
      else
      {
        DBxSqlBuffer buffer = new DBxSqlBuffer();
        buffer.FormatExpression(this, new DBxFormatExpressionInfo());
        s = buffer.SB.ToString();
      }

      return s + " (" + ColumnType.ToString() + ")";
    }

    /// <summary>
    /// Сравнение с другим объектом DBxConst.
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      DBxConst obj2 = obj as DBxConst;
      if (obj2 == null)
        return false;
      else
        return Object.Equals(this._Value, obj2._Value) && this._ColumnType == obj2._ColumnType;
    }

    /// <summary>
    /// Хэш-значение.
    /// </summary>
    /// <returns>Числовое значение для размещения в коллекции выражений</returns>
    public override int GetHashCode()
    {
      if (_Value == null)
        return 0;
      else
        return _Value.GetHashCode();
    }

    /// <summary>
    /// Получить список используемых полей
    /// Ничего не добавляется
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public override void GetColumnNames(DBxColumnList list)
    {
    }

    /// <summary>
    /// Возвращает Value
    /// </summary>
    /// <param name="rowValues">Игнорируется</param>
    /// <returns>Значение</returns>
    public override object GetValue(INamedValuesAccess rowValues)
    {
      return Value;
    }

    /// <summary>
    /// Возвращает текущий объект
    /// </summary>
    /// <returns>Константа</returns>
    public override DBxConst GetConst()
    {
      return this;
    }

    /// <summary>
    /// Возвращает текущий объект без изменений
    /// </summary>
    /// <param name="prefix">Игнорируется</param>
    /// <returns>Текущий объект</returns>
    public override DBxExpression SetColumnNamePrefix(string prefix)
    {
      return this; // не требуется изменений
    }

    #endregion
  }

  /// <summary>
  /// Список функций и операторов
  /// </summary>
  [Serializable]
  public enum DBxFunctionKind
  {
    #region Псевдофункции - математические операции

    /// <summary>
    /// Операция "+"
    /// </summary>
    Add,

    /// <summary>
    /// Операция "-"
    /// </summary>
    Substract,

    /// <summary>
    /// Операция "*"
    /// </summary>
    Multiply,

    /// <summary>
    /// Операция "/"
    /// </summary>
    Divide,

    /// <summary>
    /// Унарный минус
    /// </summary>
    Neg,

    #endregion

    #region Псевдофункции - операции сравнения

    /// <summary>
    /// "=" - сравнение на равенство
    /// </summary>
    Equal,

    /// <summary>
    /// "Меньше"
    /// </summary>
    LessThan,

    /// <summary>
    /// "Меньше или равно"
    /// </summary>
    LessOrEqualThan,

    /// <summary>
    /// "Больше"
    /// </summary>
    GreaterThan,

    /// <summary>
    /// "Больше или равно"
    /// </summary>
    GreaterOrEqualThan,

    /// <summary>
    /// "Не равно"
    /// </summary>
    NotEqual,

    #endregion

    #region Функции

    /// <summary>
    /// Модуль числа
    /// </summary>
    Abs,

    /// <summary>
    /// Замена NULL.
    /// Для некоторых баз данных, в том числе MS SQL Server, используется функция ISNULL
    /// </summary>
    Coalesce,

    /// <summary>
    /// Тернарный оператор сравнения, функция IIF
    /// </summary>
    IIf,

    /// <summary>
    /// Возвращает длину строки
    /// </summary>
    Length,

    /// <summary>
    /// Перевод строки в верхний регистр.
    /// Не реализовано для <see cref="System.Data.DataView"/>.
    /// </summary>
    Upper,

    /// <summary>
    /// Перевод строки в нижний регистр.
    /// Не реализовано для <see cref="System.Data.DataView"/>.
    /// </summary>
    Lower,

    /// <summary>
    /// Возвращает подстроку.
    /// Первый аргумент - исходное строковое выражение.
    /// Второй аргумент - начальная позиция подстроки. Нумерация с единицы, а не с нуля!
    /// Третий аргумент - длина строки в символах.
    /// В отличие от метода <see cref="System.String.Substring(int, int)"/>, поддерживается выход аргументов за длину строки.
    /// </summary>
    Substring,

    #endregion
  }

  /// <summary>
  /// Функция (кроме агрегатной) или арифметическая операция
  /// </summary>
  [Serializable]
  public sealed class DBxFunction : DBxExpression
  {
    #region Конструктор

    /// <summary>
    /// Создает объект функции, принимающей список аргументов <see cref="DBxExpression"/>.
    /// </summary>
    /// <param name="function">Функция</param>
    /// <param name="args">Аргументы функции - другие выражения</param>
    public DBxFunction(DBxFunctionKind function, params DBxExpression[] args)
    {
      if (args == null)
        throw new ArgumentNullException("args");
      int minArgCount, maxArgCount;
      GetArgCount(function, out minArgCount, out maxArgCount);
      if (args.Length < minArgCount || args.Length > maxArgCount)
        throw new ArgumentException(String.Format(Res.DBxFunction_Arg_WrongArgCount,
          args.Length, function.ToString(), minArgCount, maxArgCount));
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i] == null)
          throw ExceptionFactory.ArgInvalidListItem("args", args, i);
      }

      _Function = function;
      _Arguments = args;
    }


    /// <summary>
    /// Создает объект функции, принимающей список аргументов - имен полей.
    /// Удобно для простых функций вида "UPPER()"
    /// </summary>
    /// <param name="function">Функция</param>
    /// <param name="columnNames">Аргументы функции - имена полей, для которых создаются DBxColumn</param>
    public DBxFunction(DBxFunctionKind function, params string[] columnNames)
      : this(function, DBxTools.GetColumnNameExpressions(columnNames))
    {
    }

    #endregion

    #region Проверка аргументов

    /// <summary>
    /// Возвращает количество аргументов, которое может иметь функция
    /// </summary>
    /// <param name="function">Функция</param>
    /// <param name="minArgCount">Результат - минимальное количество аргументов</param>
    /// <param name="maxArgCount">Результат - максимальное количество аргументов</param>
    public static void GetArgCount(DBxFunctionKind function, out int minArgCount, out int maxArgCount)
    {
      switch (function)
      {
        #region Псевдофункции - операции

        case DBxFunctionKind.Add:
        case DBxFunctionKind.Substract:
        case DBxFunctionKind.Multiply:
        case DBxFunctionKind.Divide:
        case DBxFunctionKind.Equal:
        case DBxFunctionKind.LessThan:
        case DBxFunctionKind.LessOrEqualThan:
        case DBxFunctionKind.GreaterThan:
        case DBxFunctionKind.GreaterOrEqualThan:
        case DBxFunctionKind.NotEqual:
          minArgCount = 2;
          maxArgCount = 2;
          break;

        case DBxFunctionKind.Neg:
          minArgCount = 1;
          maxArgCount = 1;
          break;

        #endregion

        #region Обычные функции

        case DBxFunctionKind.Abs:
          minArgCount = 1;
          maxArgCount = 1;
          break;

        case DBxFunctionKind.Coalesce:
          minArgCount = 2;
          maxArgCount = int.MaxValue;
          break;

        case DBxFunctionKind.IIf:
          minArgCount = 3;
          maxArgCount = 3;
          break;

        case DBxFunctionKind.Length:
        case DBxFunctionKind.Lower:
        case DBxFunctionKind.Upper:
          minArgCount = 1;
          maxArgCount = 1;
          break;
        case DBxFunctionKind.Substring:
          minArgCount = 3;
          maxArgCount = 3;
          break;

        #endregion

        default:
          throw ExceptionFactory.ArgUnknownValue("function", function);
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вид функции
    /// </summary>
    public DBxFunctionKind Function { get { return _Function; } }
    private readonly DBxFunctionKind _Function;

    /// <summary>
    /// Аргументы функции - список выражений
    /// </summary>
    public DBxExpression[] Arguments { get { return _Arguments; } }
    private readonly DBxExpression[] _Arguments;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_Function.ToString());
      sb.Append("(");
      for (int i = 0; i < _Arguments.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");
        sb.Append(_Arguments[i].ToString());
      }
      sb.Append(")");
      return sb.ToString();
    }

    /// <summary>
    /// Сравнение с другим объектом DBxFunction.
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      DBxFunction obj2 = obj as DBxFunction;
      if (obj2 == null)
        return false;
      if (this._Function != obj2._Function)
        return false;
      if (this._Arguments.Length != obj2._Arguments.Length)
        return false;
      for (int i = 0; i < _Arguments.Length; i++)
      {
        if (this._Arguments[i] != obj2._Arguments[i])
          return false;
      }
      return true;
    }

    /// <summary>
    /// Хэш-значение.
    /// </summary>
    /// <returns>Числовое значение для размещения в коллекции выражений</returns>
    public override int GetHashCode()
    {
      return (int)_Function;
    }

    /// <summary>
    /// Получить список используемых полей.
    /// Рекурсивно вызывает метод для всех аргументов
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      for (int i = 0; i < _Arguments.Length; i++)
        _Arguments[i].GetColumnNames(list);
    }

    /// <summary>
    /// Получить значение выражения из произвольного источника данных.
    /// Выполняет рекурсивный вызов метода для всех аргументов. Затем выполняется вычисление функции.
    /// </summary>
    /// <param name="rowValues">Источник данных</param>
    /// <returns>Значение</returns>
    public override object GetValue(INamedValuesAccess rowValues)
    {
      object[] a = new object[_Arguments.Length];
      for (int i = 0; i < a.Length; i++)
        a[i] = _Arguments[i].GetValue(rowValues);

      return DoGetValue(a);
    }

    private object DoGetValue(object[] a)
    {
      if (_Function != DBxFunctionKind.Coalesce)
      {
        for (int i = 0; i < a.Length; i++)
        {
          if (a[i] == null)
            return null;
        }
      }

      switch (_Function)
      {
        case DBxFunctionKind.Add:
          return DataTools.SumValues(a[0], a[1]);
        case DBxFunctionKind.Substract:
          return DataTools.SubstractValues(a[0], a[1]);
        case DBxFunctionKind.Multiply:
          return DataTools.MultiplyValues(a[0], a[1]);
        case DBxFunctionKind.Divide:
          return DataTools.DivideValues(a[0], a[1]);
        case DBxFunctionKind.Equal:
          return CompareFilter.TestFilter(a[0], a[1], CompareKind.Equal);
        case DBxFunctionKind.LessThan:
          return CompareFilter.TestFilter(a[0], a[1], CompareKind.LessThan);
        case DBxFunctionKind.LessOrEqualThan:
          return CompareFilter.TestFilter(a[0], a[1], CompareKind.LessOrEqualThan);
        case DBxFunctionKind.GreaterThan:
          return CompareFilter.TestFilter(a[0], a[1], CompareKind.GreaterThan);
        case DBxFunctionKind.GreaterOrEqualThan:
          return CompareFilter.TestFilter(a[0], a[1], CompareKind.GreaterOrEqualThan);
        case DBxFunctionKind.NotEqual:
          return CompareFilter.TestFilter(a[0], a[1], CompareKind.NotEqual);

        case DBxFunctionKind.Neg:
          return DataTools.NegValue(a[0]);
        case DBxFunctionKind.Abs:
          return DataTools.AbsValue(a[0]);
        case DBxFunctionKind.Coalesce:
          for (int i = 0; i < a.Length; i++)
          {
            if (a[i] != null)
              return a[i];
          }
          return null;

        case DBxFunctionKind.IIf:
          if (DataTools.GetBool(a[0]))
            return a[1];
          else
            return a[2];

        case DBxFunctionKind.Length:
          return DataTools.GetString(a[0]).Length;
        case DBxFunctionKind.Lower:
          return DataTools.GetString(a[0]).ToLowerInvariant();
        case DBxFunctionKind.Upper:
          return DataTools.GetString(a[0]).ToUpperInvariant();
        case DBxFunctionKind.Substring:
          //return DataTools.GetString(a[0]).Substring(DataTools.GetInt(a[1]) - 1, DataTools.GetInt(a[2]));
          return DataTools.Substring(DataTools.GetString(a[0]), DataTools.GetInt(a[1]) - 1, DataTools.GetInt(a[2])); // 12.05.2023
        default:
          throw new BugException("Unknown function " + _Function.ToString());
      }
    }

    /// <summary>
    /// Если все аргументы функции являются константами, возвращает вычисленное константное выражение.
    /// Иначе возвращается null.
    /// </summary>
    /// <returns>Константное выражение или, обычно, null</returns>
    public override DBxConst GetConst()
    {
      object[] a = new object[_Arguments.Length];
      DBxColumnType columnType = DBxColumnType.Unknown;
      for (int i = 0; i < a.Length; i++)
      {
        DBxConst constArg = _Arguments[i].GetConst();
        if (constArg == null)
          return null;
        a[i] = constArg.Value;
        if (i == 0)
          columnType = constArg.ColumnType;
      }

      object resValue = DoGetValue(a);
      if (resValue != null)
        columnType = DBxColumnType.Unknown;

      return new DBxConst(resValue, columnType);
    }

    /// <summary>
    /// Добавляет префикс ко всем полям, которые входят в выражение.
    /// Рекурсивно вызывает метод для всех аргументов.
    /// Затем создается новый объект DBxFunction.
    /// </summary>
    /// <param name="prefix">Добавляемый префикс</param>
    /// <returns>Новое выражение</returns>
    public override DBxExpression SetColumnNamePrefix(string prefix)
    {
      bool hasDiff = false;
      DBxExpression[] args2 = new DBxExpression[Arguments.Length];
      for (int i = 0; i < Arguments.Length; i++)
      {
        args2[i] = Arguments[i].SetColumnNamePrefix(prefix);
        if (!object.ReferenceEquals(args2[i], Arguments[i]))
          hasDiff = true;
      }

      if (hasDiff)
        return new DBxFunction(Function, args2);
      else
        return this; // реально никогда не будет
    }

    /// <summary>
    /// Рекурсивно добавляет аргументы в список
    /// </summary>
    /// <param name="list">Заполняемый список</param>
    public override void GetAllExpressions(IList<DBxExpression> list)
    {
      base.GetAllExpressions(list);
      for (int i = 0; i < _Arguments.Length; i++)
        _Arguments[i].GetAllExpressions(list);
    }

    #endregion
  }

  /// <summary>
  /// Список функций и операторов
  /// </summary>
  [Serializable]
  public enum DBxAggregateFunctionKind
  {
    #region Агрегатные функции

    /// <summary>
    /// Минимальное значение
    /// </summary>
    Min,

    /// <summary>
    /// Максимальное значение
    /// </summary>
    Max,

    /// <summary>
    /// Среднее значение
    /// </summary>
    Avg,

    /// <summary>
    /// Количество строк
    /// </summary>
    Count,

    /// <summary>
    /// Суммарное значение
    /// </summary>
    Sum,

    #endregion
  }

  /// <summary>
  /// Агрегатная функция 
  /// </summary>
  [Serializable]
  public sealed class DBxAggregateFunction : DBxExpression
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект функции, принимающей один аргумент DBxExpression или null (только для COUNT(*) )
    /// </summary>
    /// <param name="function">Функция</param>
    /// <param name="arg">Аргумент функции - другое выражение, обычно, имя поля. Может быть null для COUNT</param>
    public DBxAggregateFunction(DBxAggregateFunctionKind function, DBxExpression arg)
    {
      if (arg == null && function != DBxAggregateFunctionKind.Count)
        throw new ArgumentNullException("arg", Res.DBxAggregateFunction_Arg_NoArg);

      _Function = function;
      _Argument = arg;
    }

    /// <summary>
    /// Создает объект функции, принимающей список аргументов - имен полей.
    /// Удобно для простых функций вида "UPPER()"
    /// </summary>
    /// <param name="function">Функция</param>
    /// <param name="columnName">Аргументы функции - имена полей, для которых создаются DBxColumn</param>
    public DBxAggregateFunction(DBxAggregateFunctionKind function, string columnName)
      : this(function, String.IsNullOrEmpty(columnName) ? (DBxExpression)null : (DBxExpression)(new DBxColumn(columnName)))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вид функции
    /// </summary>
    public DBxAggregateFunctionKind Function { get { return _Function; } }
    private readonly DBxAggregateFunctionKind _Function;

    /// <summary>
    /// Аргументы функции - список выражений
    /// </summary>
    public DBxExpression Argument { get { return _Argument; } }
    private readonly DBxExpression _Argument;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(_Function.ToString());
      sb.Append("(");
      if (_Argument == null)
        sb.Append("*");
      else
        sb.Append(_Argument.ToString());
      sb.Append(")");
      return sb.ToString();
    }

    /// <summary>
    /// Сравнение с другим объектом DBxFunction.
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      DBxAggregateFunction obj2 = obj as DBxAggregateFunction;
      if (obj2 == null)
        return false;
      if (this._Function != obj2._Function)
        return false;
      if (this._Argument != obj2._Argument)
        return false;
      return true;
    }

    /// <summary>
    /// Хэш-значение.
    /// </summary>
    /// <returns>Числовое значение для размещения в коллекции выражений</returns>
    public override int GetHashCode()
    {
      return (int)_Function;
    }

    /// <summary>
    /// Получить список используемых полей.
    /// Рекурсивно вызывает метод для всех аргументов
    /// </summary>
    /// <param name="list">Заполняемый список. Не может быть null</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      if (_Argument != null)
        _Argument.GetColumnNames(list);
    }

    /// <summary>
    /// Выбрасывает исключение
    /// </summary>
    /// <param name="rowValues">Игнорируется</param>
    /// <returns>Значение</returns>
    public override object GetValue(INamedValuesAccess rowValues)
    {
      throw new NotSupportedException(Res.DBxAggregateFunction_Err_GetValueNotSupported);
    }

    /// <summary>
    /// В текущей реализации возвращает null.
    /// Вычисление составных констант не реализовано
    /// </summary>
    /// <returns>null</returns>
    public override DBxConst GetConst()
    {
      return null;
    }

    /// <summary>
    /// Добавляет префикс ко всем полям, которые входят в выражение.
    /// Рекурсивно вызывает метод для всех аргументов.
    /// Затем создается новый объект DBxFunction.
    /// </summary>
    /// <param name="prefix">Добавляемый префикс</param>
    /// <returns>Новое выражение</returns>
    public override DBxExpression SetColumnNamePrefix(string prefix)
    {
      if (_Argument == null)
        return this; // функция COUNT(*)

      DBxExpression arg2 = _Argument.SetColumnNamePrefix(prefix);
      if (Object.ReferenceEquals(arg2, _Argument))
        return this; // реально никогда не будет
      else
        return new DBxAggregateFunction(Function, arg2);
    }

    /// <summary>
    /// Рекурсивно добавляет в список аргумент функции, если он есть
    /// </summary>
    /// <param name="list">Заполняемый список</param>
    public override void GetAllExpressions(IList<DBxExpression> list)
    {
      base.GetAllExpressions(list);

      if (_Argument != null)
        _Argument.GetAllExpressions(list);
    }

    /// <summary>
    /// Функция "COUNT(*)"
    /// </summary>
    public static readonly DBxAggregateFunction Count = new DBxAggregateFunction(DBxAggregateFunctionKind.Count, (DBxExpression)null);

    #endregion
  }

#if XXX // Пока не используется
  /// <summary>
  /// Список выражений.
  /// Класс не используется в прикладном коде.
  /// </summary>
  [Serializable]
  public sealed class DBxExpressions : IList<DBxExpression>
  {
    #region Конструкторы

    /// <summary>
    /// Создает список выражений из массива
    /// </summary>
    /// <param name="items">Массив выражений.</param>
    public DBxExpressions(DBxExpression[] items)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      for (int i = 0; i < items.Length; i++)
      {
        if (items[i] == null)
          throw new ArgumentNullException("items[" + i.ToString() + "]");
        _Items = items;
      }
    }

    /// <summary>
    /// Создает массив выражений DBxColumn и DBxRefColumn из списка столбцов.
    /// Ссылочные поля определяются по наличию разделителя - точки
    /// </summary>
    /// <param name="columns">Имена столбцов</param>
    public DBxExpressions(DBxColumns columns)
    {
      _Items = new DBxExpression[columns.Count];
      for (int i = 0; i < _Items.Length; i++)
        _Items[i] = new DBxColumn(columns[i]);
    }

    /// <summary>
    /// Создает массив выражений DBxColumn и DBxRefColumn из списка столбцов.
    /// Ссылочные поля определяются по наличию разделителя - точки
    /// </summary>
    /// <param name="columns">Имена столбцов</param>
    public DBxExpressions(DBxColumnList columns)
    {
      _Items = new DBxExpression[columns.Count];
      for (int i = 0; i < _Items.Length; i++)
        _Items[i] = new DBxColumn(columns[i]);
    }

    /// <summary>
    /// Создает список выражений DBxColumn из списка столбцов.
    /// Ссылочные поля определяются по наличию разделителя - точки.
    /// Если <paramref name="columns"/>=null, возвращается null.
    /// </summary>
    /// <param name="columns">Список имен полей или null</param>
    /// <returns>Объект DBxExpressions или null</returns>
    public static DBxExpressions FromColumns(DBxColumns columns)
    {
      if (columns == null)
        return null;
      else
        return new DBxExpressions(columns);
    }

    /// <summary>
    /// Создает список выражений DBxColumn из списка столбцов.
    /// Ссылочные поля определяются по наличию разделителя - точки.
    /// Если <paramref name="columns"/>=null, возвращается null.
    /// </summary>
    /// <param name="columns">Список имен полей или null</param>
    /// <returns>Объект DBxExpressions или null</returns>
    public static DBxExpressions FromColumns(DBxColumnList columns)
    {
      if (columns == null)
        return null;
      else
        return new DBxExpressions(columns);
    }

    #endregion

    #region Доступ к отдельным выражениям

    private DBxExpression[] _Items;

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return _Items.Length; } }

    /// <summary>
    /// Возвращает выражение по индексу
    /// </summary>
    /// <param name="index">Индекс выражения</param>
    /// <returns>Выражение</returns>
    public DBxExpression this[int index]
    {
      get { return _Items[index]; }
    }

    /// <summary>
    /// Возвращает перечислитель выражений.
    /// 
    /// Тип возвращаемого значения (ArrayEnumerator) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns></returns>
    public ArrayEnumerable<DBxExpression>.Enumerator GetEnumerator()
    {
      return new ArrayEnumerable<DBxExpression>.Enumerator(_Items);
    }

    IEnumerator<DBxExpression> IEnumerable<DBxExpression>.GetEnumerator()
    {
      return new ArrayEnumerable<DBxExpression>.Enumerator(_Items);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerable<DBxExpression>.Enumerator(_Items);
    }

    #endregion

    #region IList<DBxExpression> Members

    int IList<DBxExpression>.IndexOf(DBxExpression item)
    {
      return Array.IndexOf<DBxExpression>(_Items, item);
    }


    void IList<DBxExpression>.Insert(int index, DBxExpression item)
    {
      throw new NotSupportedException();
    }

    void IList<DBxExpression>.RemoveAt(int index)
    {
      throw new NotSupportedException();
    }

    DBxExpression IList<DBxExpression>.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    #endregion

    #region ICollection<DBxExpression> Members

    void ICollection<DBxExpression>.Add(DBxExpression item)
    {
      throw new NotSupportedException();
    }

    void ICollection<DBxExpression>.Clear()
    {
      throw new NotSupportedException();
    }

    bool ICollection<DBxExpression>.Contains(DBxExpression item)
    {
      return Array.IndexOf<DBxExpression>(_Items, item) >= 0;
    }

    void ICollection<DBxExpression>.CopyTo(DBxExpression[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    bool ICollection<DBxExpression>.IsReadOnly
    {
      get { return true; }
    }

    bool ICollection<DBxExpression>.Remove(DBxExpression item)
    {
      throw new NotSupportedException();
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Добавляет в список <paramref name="list"/> все имена полей, входящие в выражения
    /// </summary>
    /// <param name="list">Заполняемый список</param>
    public void GetColumnNames(DBxColumnList list)
    {
      for (int i = 0; i < _Items.Length; i++)
        _Items[i].GetColumnNames(list);
    }

    #endregion
  }

#endif
}
