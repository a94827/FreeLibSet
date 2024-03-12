// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#if !XXX

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.DBF
{
  /// <summary>
  /// Предварительно известная информация о типе данных поля DBF.
  /// Используется в <see cref="DbfFieldTypeDetector"/>.
  /// </summary>
  public sealed class DbfFieldTypePreliminaryInfo : ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает объект со значениями по умолчанию, <see cref="Type"/>=' '
    /// </summary>
    public DbfFieldTypePreliminaryInfo()
    {
      _Type = ' ';
    }

    /// <summary>
    /// Создает объект на основании описания столбца.
    /// Если <paramref name="fi"/>.IsEmpty=true, то создается пустой описатель
    /// </summary>
    /// <param name="fi">Описатель столбца</param>
    public DbfFieldTypePreliminaryInfo(DbfFieldInfo fi)
      :this()
    {
      if (!fi.IsEmpty)
      {
        _Type = fi.Type;
        _Length = fi.Length;
        _Precision = fi.Precision;
        _LengthIsDefined = true;
        _PrecisionIsDefined = true;
      }
    }

    #endregion

    #region Устанавливаемые свойства

    /// <summary>
    /// Тип поля DBF (один из символов 'C', 'N', 'F', 'L', 'D' или 'M') или пробел (' '), если тип поля не определен
    /// </summary>
    public char Type 
    { 
      get { return _Type; } 
      set 
      {
        if (" CNFLDM".IndexOf(value) < 0)
          throw new ArgumentException();

        _Type = value; 
      } 
    }
    private char _Type;

    /// <summary>
    /// Длина текстового или числового поля
    /// </summary>
    public int Length
    {
      get { return _Length; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _Length = value;
      }
    }
    private int _Length;

    /// <summary>
    /// Число десятичных знаков после запятой для числового поля
    /// </summary>
    public int Precision
    {
      get { return _Precision; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _Precision = value;
      }
    }
    private int _Precision;

    /// <summary>
    /// True, если длина поля определена
    /// </summary>
    public bool LengthIsDefined { get { return _LengthIsDefined; } set { _LengthIsDefined = value; } }
    private bool _LengthIsDefined;

    /// <summary>
    /// True, если точность числового поля определена (в том числе, должно быть true, если поле целочисленное)
    /// </summary>
    public bool PrecisionIsDefined { get { return _PrecisionIsDefined; } set { _PrecisionIsDefined = value; } }
    private bool _PrecisionIsDefined;

    #endregion

    #region ICloneable

    public DbfFieldTypePreliminaryInfo Clone()
    {
      DbfFieldTypePreliminaryInfo res = new DbfFieldTypePreliminaryInfo();
      res.Type = Type;
      res.Length = Length;
      res.Precision = Precision;
      res.LengthIsDefined = LengthIsDefined;
      res.PrecisionIsDefined = PrecisionIsDefined;
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Для вывода текста на экран.
    /// Если тип <see cref="Type"/> не задан, возвращает пустую строку
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (Type == ' ')
        return String.Empty;

      StringBuilder sb = new StringBuilder();
      sb.Append(Type);
      if ("CNF".IndexOf(Type) >= 0)
      {
        if (Length == 0)
          sb.Append('1');
        else
          sb.Append(Length);
        if (!LengthIsDefined)
          sb.Append('+');
        if (Type != 'C')
        {
          sb.Append(',');
          sb.Append(Precision);
          if (!PrecisionIsDefined)
            sb.Append('+');
        }
      }
      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Определение типа и размера поля будущего DBF-файла.
  /// </summary>
  public sealed class DbfFieldTypeDetector : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Инициализация информации о поле.
    /// </summary>
    public DbfFieldTypeDetector()
    {
      _ColumnName = "UNKNOWN";
      _PreliminaryInfo = new DbfFieldTypePreliminaryInfo();
      _Encoding = DbfFile.DefaultEncoding;
      _Format = DbfFileFormat.dBase3;
      _UseMemo = true;

      _Length = -1; // Маркер для вызова PrepareFirst()
    }

    #endregion

    #region Устанавливаемые свойства

    /// <summary>
    /// Имя поля.
    /// Используется только для создания свойства <see cref="Result"/>.
    /// </summary>
    public string ColumnName
    {
      get { return _ColumnName; }
      set
      {
        if (!DbfFieldInfo.IsValidFieldName(value))
          throw new ArgumentException("Неправильное имя поля");
        _ColumnName = value;
      }
    }
    private string _ColumnName;

    /// <summary>
    /// Предварительно известная информация о поле.
    /// 
    /// </summary>
    public DbfFieldTypePreliminaryInfo PreliminaryInfo
    {
      get { return _PreliminaryInfo; }
      set
      {
        if (value == null)
          _PreliminaryInfo = new DbfFieldTypePreliminaryInfo();
        else
          _PreliminaryInfo = value;
        _Length = -1;
      }
    }
    private DbfFieldTypePreliminaryInfo _PreliminaryInfo;

    /// <summary>
    /// Используемая кодировка.
    /// По умолчанию возвращается <see cref="DbfFile.DefaultEncoding"/>.
    /// </summary>
    public Encoding Encoding
    {
      get { return _Encoding; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Encoding = value;
        _Length = -1;
      }
    }
    private Encoding _Encoding;

    /// <summary>
    /// Формат файла.
    /// По умолчанию - <see cref="DbfFileFormat.dBase3"/>
    /// </summary>
    public DbfFileFormat Format
    {
      get { return _Format; }
      set
      {
        if (value == DbfFileFormat.Undefined)
          _Format = DbfFileFormat.dBase3;
        else
          _Format = value;
        if (_Format == DbfFileFormat.dBase2)
          UseMemo = false;
        _Length = -1;
      }
    }
    private DbfFileFormat _Format;

    /// <summary>
    /// Можно ли использовать Memo-поля вместо текстовых полей.
    /// По умолчанию - true
    /// </summary>
    public bool UseMemo
    {
      get
      {
        if (Format == DbfFileFormat.dBase2)
          return false;
        else
          return _UseMemo;
      }
      set
      {
        _UseMemo = value;
        _Length = -1;
      }
    }
    private bool _UseMemo;

    #endregion

    #region Определение

    /// <summary>
    /// Текущий тип поля
    /// </summary>
    private char _Type;

    /// <summary>
    /// Текущая длина строки или числового поля
    /// </summary>
    private int _Length;

    /// <summary>
    /// Число знаков после запятой для дробного числа
    /// </summary>
    private int _Precision;

    /// <summary>
    /// Максимальная длина текстового поля
    /// </summary>
    private int MaxLengthC { get { return _Format == DbfFileFormat.dBase2 ? 255 : 65535; } }
    private const int MaxLengthN = 255;

    /// <summary>
    /// Начальная инициализация внутренних переменных при первом обращении
    /// </summary>
    private void PrepareFirst()
    {
      if (_Length >= 0)
        return; // повторный вызов

      _Type = _PreliminaryInfo.Type;
      _Length = _PreliminaryInfo.Length;
      _Precision = _PreliminaryInfo.Precision;

      switch (_Type)
      {
        case 'C':
          if (_Length == 0)
            _Length = 1;
          else
          {
            _Length = Math.Min(_Length, MaxLengthC);
            _IsCompleted = _PreliminaryInfo.LengthIsDefined;
          }
          break;
        case 'N':
        case 'F':
          if (_Length == 0)
          {
            if (_Precision > 0)
              _Length = _Precision + 2;
            else
              _Length = 1;
          }
          _IsCompleted = _PreliminaryInfo.LengthIsDefined && _PreliminaryInfo.PrecisionIsDefined;
          break;
        case 'L':
        case 'D':
          _IsCompleted = true;
          break;
        case 'M':
          if (!UseMemo)
          {
            _Type = 'C';
            _Length = 1;
          }
          else
            _IsCompleted = true;
          break;
        default:
          _Type = ' ';
          _Length = 0;
          _Precision = 0;
          break;
      }
    }


    // Определение типа
    // Возвращает true, если тип и размер поля определен однозначно и (дальнейший) перебор строк не нужен
    public bool IsCompleted
    {
      get
      {
        PrepareFirst();
        return _IsCompleted;
      }
    }
    private bool _IsCompleted;

    // Возвращает непустой результат, независимо от того, определен тип данных или нет
    public DbfFieldInfo Result
    {
      get
      {
        PrepareFirst();
        switch (_Type)
        {
          case 'C':
            return DbfFieldInfo.CreateString(ColumnName, Math.Min(_Length, MaxLengthC));
          case 'N':
          case 'F':
            int len = Math.Min(_Length, MaxLengthN);
            int prec = _Precision;
            if (prec > len - 2)
              prec = Math.Max(len - 2, 0);
            return new DbfFieldInfo(ColumnName, _Type, len, prec);
          case 'D':
            return DbfFieldInfo.CreateDate(ColumnName);
          case 'L':
            return DbfFieldInfo.CreateBool(ColumnName);
          case 'M':
            return DbfFieldInfo.CreateMemo(ColumnName);
          case ' ':
            return DbfFieldInfo.CreateString(ColumnName, 1);
          default:
            throw new BugException();
        }
      }
    }

    private string _NumberMask;

    /// <summary>
    /// Пытается применить значение для увеличения размера поля
    /// </summary>
    /// <param name="value"></param>
    public void ApplyValue(object value)
    {
      PrepareFirst();

      if (value == null)
        return;
      if (value is DBNull)
        return;

      if (value is Boolean)
        ApplyBoolean();
      else if (value is DateTime)
        ApplyDateTime((DateTime)value);
      else if (DataTools.IsIntegerType(value.GetType()))
        ApplyNumeric(((IFormattable)value).ToString("0", StdConvert.NumberFormat));
      else if (DataTools.IsFloatType(value.GetType()))
      {
        if (_PreliminaryInfo.PrecisionIsDefined)
        {
          if (_NumberMask == null)
            _NumberMask = Formatting.FormatStringTools.DecimalPlacesToNumberFormat(_PreliminaryInfo.Precision);
          ApplyNumeric(((IFormattable)value).ToString(_NumberMask, StdConvert.NumberFormat));
        }
        else
          ApplyNumeric(((IFormattable)value).ToString("f", StdConvert.NumberFormat));
      }
      else
        ApplyString(value.ToString());
    }

    private void ApplyString(string s)
    {
      if (s.Length == 0)
        s = "0";
      int len = Encoding.GetByteCount(s);
      if (len > MaxLengthC)
      {
        if (UseMemo)
        {
          _Type = 'M';
          _IsCompleted = true;
          return;
        }
        else
          len = MaxLengthC;
      }

      switch (_Type)
      {
        case ' ':
          _Type = 'C';
          _Length = len;
          break;
        case 'C':
          _Length = Math.Max(_Length, len);
          break;
        case 'M':
          _IsCompleted = true;
          break;
        default:
          ConvertToC();
          _Length = Math.Max(_Length, len);
          break;
      }
    }

    private void ApplyNumeric(string s)
    {
      int len, prec;
      switch (_Type)
      {
        case ' ':
          GetLengthAndPrecision(s, out len, out prec);
          _Type = 'N';
          _Length = len;
          _Precision = prec;
          break;
        case 'N':
        case 'F':
          GetLengthAndPrecision(s, out len, out prec);
          if (prec > _Precision)
          {
            if (_Precision == 0)
            {
              _Length += prec + 1;
              _Precision = prec;
            }
            else
            {
              _Length += prec;
              _Precision += prec;
            }
          }
          _Length = Math.Max(_Length, len);
          break;
        case 'C':
          _Length = Math.Max(_Length, Encoding.GetByteCount(s));
          break;
        case 'L':
        case 'D':
          ConvertToC();
          _Type = 'C';
          _Length = Math.Max(_Length, Encoding.GetByteCount(s));
          break;
        case 'M':
          break;
        default:
          throw new BugException();
      }
    }

    private void ConvertToC()
    {
      switch (_Type)
      {
        case 'L':
          _Length = Encoding.GetByteCount("T");
          break;
        case 'D':
          _Length = GetDateTimeLength(false);
          break;
        case 'N':
        case 'F':
          string s = new string('0', _Length);
          _Length = Encoding.GetByteCount(s);
          break;
        default:
          throw new BugException();
      }
      _Type = 'C';
    }

    private void ApplyDateTime(DateTime value)
    {
      bool hasTime = value.TimeOfDay != TimeSpan.Zero;
      switch (_Type)
      {
        case ' ':
          if (hasTime)
          {
            _Type = 'C';
            _Length = GetDateTimeLength(hasTime);
            _IsCompleted = true;
          }
          else
          {
            _Type = 'D';
            _IsCompleted = true;
          }
          break;
        case 'D':
          _IsCompleted = true;
          break;
        case 'C':
          _Length = Math.Max(_Length, GetDateTimeLength(hasTime));
          break;
        case 'N':
        case 'F':
        case 'L':
          ConvertToC();
          _Type = 'C';
          _Length = Math.Max(_Length, GetDateTimeLength(hasTime));
          break;
        case 'M':
          break;
        default:
          throw new BugException();
      }
    }

    private void ApplyBoolean()
    {
      switch (_Type)
      {
        case ' ':
          _Type = 'L';
          _IsCompleted = true;
          break;
        case 'L':
          _IsCompleted = true;
          break;
        case 'D':
        case 'N':
        case 'F':
          ConvertToC();
          _Length = Math.Max(_Length, Encoding.GetByteCount("T"));
          break;
        default:
          break; // не меняется
      }
    }
    #endregion

    #region Вспомогательные измерения


    private int _DateLength;
    private int _DateTimeLength;
    private int GetDateTimeLength(bool hasTime)
    {
      if (_DateLength == 0)
      {
        // первый вызов
        _DateLength = Encoding.GetByteCount("00000000");
        _DateTimeLength = Encoding.GetByteCount("00000000-000000");
      }
      return hasTime ? _DateTimeLength : _DateLength;
    }

    private static void GetLengthAndPrecision(string s, out int len, out int prec)
    {
      len = s.Length;
      int p = s.IndexOf('.');
      if (p >= 0)
        prec = s.Length - p - 1;
      else
        prec = 0;
    }


    #endregion

    #region Прочее

    string IObjectWithCode.Code { get { return _ColumnName; } }

    /// <summary>
    /// Для отладки. Возвращает <see cref="ColumnName"/>.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _ColumnName;
    }

    #endregion
  }
}

#endif