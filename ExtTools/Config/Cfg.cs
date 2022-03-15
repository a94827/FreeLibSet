// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Xml;
using System.IO;
using FreeLibSet.IO;
using System.Diagnostics;
using FreeLibSet.Win32;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Config
{
  #region Базовые классы

  /// <summary>
  /// Конвертер значений в/из текста для хранения настроечных параметров числовых типов и даты.
  /// </summary>
  public class CfgConverter
  {
    #region Методы преобразования

    #region Int32

    /// <summary>
    /// Преобразование значения в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Текстовое представление, используемое для записи значения</returns>
    public virtual string ToString(int value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Попытка преобразование строки в значение
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="value">Сюда помещается значение, если преобразование успешно выполнено</param>
    /// <returns>true, если преобразование выполнено, false, если строку нельзя преобразовать в значение</returns>
    public virtual bool TryParse(string s, out int value)
    {
      return int.TryParse(s, NumberStyles.Integer, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразование строки в значение.
    /// Если строка имеет неподходящий формат, выбрасывается исключение.
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <returns>Преобразованное значение</returns>
    public int ParseInt(string s)
    {
      int value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException("Строку \"" + s + "\" нельзя преобразовать в целое число");
    }

    #endregion

    #region Int64

    /// <summary>
    /// Преобразование значения в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Текстовое представление, используемое для записи значения</returns>
    public virtual string ToString(Int64 value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Попытка преобразование строки в значение
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="value">Сюда помещается значение, если преобразование успешно выполнено</param>
    /// <returns>true, если преобразование выполнено, false, если строку нельзя преобразовать в значение</returns>
    public virtual bool TryParse(string s, out Int64 value)
    {
      return Int64.TryParse(s, NumberStyles.Integer, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразование строки в значение.
    /// Если строка имеет неподходящий формат, выбрасывается исключение.
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <returns>Преобразованное значение</returns>
    public Int64 ParseInt64(string s)
    {
      Int64 value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException("Строку \"" + s + "\" нельзя преобразовать в целое число Int64");
    }

    #endregion

    #region Single

    /// <summary>
    /// Преобразование значения в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Текстовое представление, используемое для записи значения</returns>
    public virtual string ToString(float value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Попытка преобразование строки в значение
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="value">Сюда помещается значение, если преобразование успешно выполнено</param>
    /// <returns>true, если преобразование выполнено, false, если строку нельзя преобразовать в значение</returns>
    public virtual bool TryParse(string s, out float value)
    {
      return float.TryParse(s, NumberStyles.Float, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразование строки в значение.
    /// Если строка имеет неподходящий формат, выбрасывается исключение.
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <returns>Преобразованное значение</returns>
    public float ParseSingle(string s)
    {
      float value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException("Строку \"" + s + "\" нельзя преобразовать в число Single");
    }

    #endregion

    #region Double

    /// <summary>
    /// Преобразование значения в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Текстовое представление, используемое для записи значения</returns>
    public virtual string ToString(double value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Попытка преобразование строки в значение
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="value">Сюда помещается значение, если преобразование успешно выполнено</param>
    /// <returns>true, если преобразование выполнено, false, если строку нельзя преобразовать в значение</returns>
    public virtual bool TryParse(string s, out double value)
    {
      return double.TryParse(s, NumberStyles.Float, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразование строки в значение.
    /// Если строка имеет неподходящий формат, выбрасывается исключение.
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <returns>Преобразованное значение</returns>
    public double ParseDouble(string s)
    {
      double value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException("Строку \"" + s + "\" нельзя преобразовать в число Double");
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Преобразование значения в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Текстовое представление, используемое для записи значения</returns>
    public virtual string ToString(decimal value)
    {
      return value.ToString(NumberFormat);
    }

    /// <summary>
    /// Попытка преобразование строки в значение
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="value">Сюда помещается значение, если преобразование успешно выполнено</param>
    /// <returns>true, если преобразование выполнено, false, если строку нельзя преобразовать в значение</returns>
    public virtual bool TryParse(string s, out decimal value)
    {
      return decimal.TryParse(s, NumberStyles.Float, NumberFormat, out value);
    }

    /// <summary>
    /// Преобразование строки в значение.
    /// Если строка имеет неподходящий формат, выбрасывается исключение.
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <returns>Преобразованное значение</returns>
    public decimal ParseDecimal(string s)
    {
      decimal value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException("Строку \"" + s + "\" нельзя преобразовать в число Decimal");
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Преобразование значения в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <param name="useTime">true - использовать компонент времени</param>
    /// <returns>Текстовое представление, используемое для записи значения</returns>
    public virtual string ToString(DateTime value, bool useTime)
    {
      if (useTime)
        return value.ToString("s", DateTimeFormat);
      else
        return value.ToString("yyyy\\-MM\\-dd", DateTimeFormat);
    }

    /// <summary>
    /// Попытка преобразование строки в значение
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="value">Сюда помещается значение, если преобразование успешно выполнено</param>
    /// <param name="useTime">true - использовать компонент времени</param>
    /// <returns>true, если преобразование выполнено, false, если строку нельзя преобразовать в значение</returns>
    public virtual bool TryParse(string s, out DateTime value, bool useTime)
    {
      if (DateTime.TryParseExact(s,
        new string[] { "s", "yyyy\\-MM\\-dd" },
        DateTimeFormat, DateTimeStyles.None, out value))
      {
        if (!useTime)
          value = value.Date;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Преобразование строки в значение.
    /// Если строка имеет неподходящий формат, выбрасывается исключение.
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="useTime">true - использовать компонент времени</param>
    /// <returns>Преобразованное значение</returns>
    public DateTime ParseDateTime(string s, bool useTime)
    {
      DateTime value;
      if (TryParse(s, out value, useTime))
        return value;
      else
        throw new FormatException("Строку \"" + s + "\" нельзя преобразовать в DateTime");
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Преобразование значения в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Текстовое представление, используемое для записи значения</returns>
    public virtual string ToString(TimeSpan value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Попытка преобразование строки в значение
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="value">Сюда помещается значение, если преобразование успешно выполнено</param>
    /// <returns>true, если преобразование выполнено, false, если строку нельзя преобразовать в значение</returns>
    public virtual bool TryParse(string s, out TimeSpan value)
    {
      return TimeSpan.TryParse(s, out value);
    }

    /// <summary>
    /// Преобразование строки в значение.
    /// Если строка имеет неподходящий формат, выбрасывается исключение.
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <returns>Преобразованное значение</returns>
    public TimeSpan ParseTimeSpan(string s)
    {
      TimeSpan value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException("Строку \"" + s + "\" нельзя преобразовать в TimeSpan");
    }

    #endregion

    #region Guid

    /// <summary>
    /// Преобразование значения в строку
    /// </summary>
    /// <param name="value">Преобразуемое значение</param>
    /// <returns>Текстовое представление, используемое для записи значения</returns>
    public virtual string ToString(Guid value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Попытка преобразование строки в значение Guid
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <param name="value">Сюда помещается значение, если преобразование успешно выполнено</param>
    /// <returns>true, если преобразование выполнено, false, если строку нельзя преобразовать в значение</returns>
    [DebuggerStepThrough]
    public virtual bool TryParse(string s, out Guid value)
    {
      try
      {
        value = new Guid(s);
        return true;
      }
      catch
      {
        value = Guid.Empty;
        return false;
      }
    }

    /// <summary>
    /// Преобразование строки в значение.
    /// Если строка имеет неподходящий формат, выбрасывается исключение.
    /// </summary>
    /// <param name="s">Текстовое представление, используемое для хранения значения</param>
    /// <returns>Преобразованное значение</returns>
    public Guid ParseGuid(string s)
    {
      // Можно было бы просто использовать конструктор Guid(s), но тогда не будет вызван виртуальный метод TryParse(), который, теоретически,
      // может быть переопределен

      Guid value;
      if (TryParse(s, out value))
        return value;
      else
        throw new FormatException("Строку \"" + s + "\" нельзя преобразовать в GUID");
    }

    #endregion

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс System.Runtime.InteropServices.IFormatProvider для преобразования числовых значений
    /// </summary>
    public virtual IFormatProvider NumberFormat { get { return StdConvert.NumberFormat; } }

    /// <summary>
    /// Интерфейс System.Runtime.InteropServices.IFormatProvider для преобразования даты и времени
    /// </summary>
    public virtual IFormatProvider DateTimeFormat { get { return StdConvert.DateTimeFormat; } }

    /// <summary>
    /// Разделитель для списочных значений.
    /// Непереопределенный метод возвращает запятую.
    /// </summary>
    public virtual string ListSeparator { get { return ","; } }

    #endregion

    #region Статический экземпляр объекта

    /// <summary>
    /// Экземпляр конвертера по умолчанию
    /// </summary>
    public static CfgConverter Default
    {
      get { return _Default; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Default = value;
      }
    }
    private static CfgConverter _Default = new CfgConverter();

    #endregion
  }

  /// <summary>
  /// Базовый класс для доступа к настроечным параметрам
  /// </summary>
  public abstract class CfgPart
  {
    #region Доступ к значениям

    #region String

    /// <summary>
    /// Получить строку с заданным именем
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Значение</returns>
    public string GetString(string name)
    {
      ValidateName(name);
      return DoGetString(name);
    }

    /// <summary>
    /// Получить строку с заданным именем
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Значение</returns>
    protected abstract string DoGetString(string name);

    /// <summary>
    /// Записать строку с заданным именем.
    /// Строка записывается, даже если <paramref name="Value"/> задает пустую строку.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="Value">Значение</param>
    public void SetString(string name, string Value)
    {
      SetString(name, Value, false);
    }

    /// <summary>
    /// Записать строку с заданным именем.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает пустую строку, то запись будет удалена из хранилища</param>
    public void SetString(string name, string value, bool removeEmpty)
    {
      ValidateName(name);
      DoSetString(name, value, removeEmpty);
    }

    /// <summary>
    /// Записать строку с заданным именем.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает пустую строку, то запись будет удалена из хранилища</param>
    protected abstract void DoSetString(string name, string value, bool removeEmpty);

    #endregion

    #region Int32

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 0
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Значение</returns>
    public int GetInt(string name)
    {
      string s = GetString(name);

      int value;

      if (Converter.TryParse(s, out value))
        return value;
      else
        return 0;
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, сохраняется существующее 
    /// значение <paramref name="value"/>.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetInt(string name, ref int value)
    {
      string s = GetString(name);
      int x2;
      if (Converter.TryParse(s, out x2))
      {
        value = x2;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public int GetIntDef(string name, int defValue)
    {
      GetInt(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// Строка записывается, даже если <paramref name="value"/> задает нулевое значение.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Записываемое значение</param>
    public void SetInt(string name, int value)
    {
      SetInt(name, value, false);
    }

    /// <summary>
    /// Записать значение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает нулевое значение, то запись удаляется из хранилища.</param>
    public void SetInt(string name, int value, bool removeEmpty)
    {
      if (removeEmpty && value == 0)
      {
        Remove(name);
        return;
      }

      SetString(name, Converter.ToString(value));
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 0
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Значение</returns>
    public long GetInt64(string name)
    {
      string s = GetString(name);

      long value;

      if (Converter.TryParse(s, out value))
        return value;
      else
        return 0L;
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, сохраняется существующее 
    /// значение <paramref name="value"/>.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetInt64(string name, ref long value)
    {
      string s = GetString(name);
      long x2;
      if (Converter.TryParse(s, out x2))
      {
        value = x2;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public long GetInt64Def(string name, long defValue)
    {
      GetInt64(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// Строка записывается, даже если <paramref name="value"/> задает нулевое значение.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Записываемое значение</param>
    public void SetInt64(string name, long value)
    {
      SetInt64(name, value, false);
    }

    /// <summary>
    /// Записать значение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает нулевое значение, то запись удаляется из хранилища.</param>
    public void SetInt64(string name, long value, bool removeEmpty)
    {
      if (removeEmpty && value == 0L)
      {
        Remove(name);
        return;
      }

      SetString(name, Converter.ToString(value));
    }

    #endregion

    #region Boolean

    /// <summary>
    /// Получить логическое значение
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается false.
    /// Нулевое значение соответствует false, ненулевое - true.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public bool GetBool(string name)
    {
      return GetInt(name) != 0;
    }


    /// <summary>
    /// Получить логическое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, сохраняется существующее 
    /// значение <paramref name="value"/>.
    /// Нулевое значение соответствует false, ненулевое - true.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetBool(string name, ref bool value)
    {
      int x = 0;
      if (!GetInt(name, ref x))
        return false;
      value = (x != 0);
      return true;
    }

    /// <summary>
    /// Получить логическое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// Нулевое значение соответствует false, ненулевое - true.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public bool GetBoolDef(string name, bool defValue)
    {
      GetBool(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// Для значения <paramref name="value"/>=true записывается "1", а для false - "0".
    /// Строка записывается, даже если <paramref name="value"/> задает false.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Записываемое значение</param>
    public void SetBool(string name, bool value)
    {
      SetBool(name, value, false);
    }

    /// <summary>
    /// Записать значение.
    /// Для значения <paramref name="value"/>=true записывается "1", а для false - "0".
    /// </summary>                                                                   
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает значение false, то запись удаляется из хранилища.</param>
    public void SetBool(string name, bool value, bool removeEmpty)
    {
      if (removeEmpty && (!value))
      {
        Remove(name);
        return;
      }

      SetString(name, value ? "1" : "0");
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 0
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public float GetSingle(string name)
    {
      return GetSingleDef(name, 0f);
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, сохраняется существующее 
    /// значение <paramref name="value"/>.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetSingle(string name, ref float value)
    {
      string s = GetString(name);
      float x2;
      if (Converter.TryParse(s, out x2))
      {
        value = x2;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public float GetSingleDef(string name, float defValue)
    {
      GetSingle(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// Строка записывается, даже если <paramref name="value"/> задает нулевое значение.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Записываемое значение</param>
    public void SetSingle(string name, float value)
    {
      SetSingle(name, value, false);
    }

    /// <summary>
    /// Записать значение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает нулевое значение, то запись удаляется из хранилища.</param>
    public void SetSingle(string name, float value, bool removeEmpty)
    {
      if (removeEmpty && value == 0f)
      {
        Remove(name);
        return;
      }

      SetString(name, Converter.ToString(value));
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 0
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public double GetDouble(string name)
    {
      return GetDoubleDef(name, 0.0);
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, сохраняется существующее 
    /// значение <paramref name="value"/>.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetDouble(string name, ref double value)
    {
      string s = GetString(name);
      double x2;
      if (Converter.TryParse(s, out x2))
      {
        value = x2;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public double GetDoubleDef(string name, double defValue)
    {
      GetDouble(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// Строка записывается, даже если <paramref name="value"/> задает нулевое значение.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Записываемое значение</param>
    public void SetDouble(string name, double value)
    {
      SetDouble(name, value, false);
    }

    /// <summary>
    /// Записать значение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает нулевое значение, то запись удаляется из хранилища.</param>
    public void SetDouble(string name, double value, bool removeEmpty)
    {
      if (removeEmpty && value == 0.0)
      {
        Remove(name);
        return;
      }

      SetString(name, Converter.ToString(value));
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 0
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public decimal GetDecimal(string name)
    {
      return GetDecimalDef(name, 0m);
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, сохраняется существующее 
    /// значение <paramref name="value"/>.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetDecimal(string name, ref decimal value)
    {
      string s = GetString(name);
      decimal x2;
      if (Converter.TryParse(s, out x2))
      {
        value = x2;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Получить числовое значение.
    /// Если в хранилище нет такой строки или там записано нечисловое значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public decimal GetDecimalDef(string name, decimal defValue)
    {
      GetDecimal(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// Строка записывается, даже если <paramref name="value"/> задает нулевое значение.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Записываемое значение</param>
    public void SetDecimal(string name, decimal value)
    {
      SetDecimal(name, value, false);
    }

    /// <summary>
    /// Записать значение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает нулевое значение, то запись удаляется из хранилища.</param>
    public void SetDecimal(string name, decimal value, bool removeEmpty)
    {
      if (removeEmpty && value == 0m)
      {
        Remove(name);
        return;
      }

      SetString(name, Converter.ToString(value));
    }

    #endregion

    #region DateTime

    #region Только дата

    /// <summary>
    /// Получить значение типа даты. Если дата не записана или имеет неправильный
    /// формат, возвращается default(DateTime) = DateTime.MinValue.
    /// Компонент времени не устанавливается.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Прочитанная дата</returns>
    public DateTime GetDate(string name)
    {
      string s = GetString(name);
      DateTime value;
      Converter.TryParse(s, out value, false);
      return value;
    }

    /// <summary>
    /// Попытаться получить значение типа даты.
    /// Если в хранилище нет такой строки или там записано неправильное значение, сохраняется существующее 
    /// значение <paramref name="value"/>.
    /// Компонент времени не устанавливается.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetDate(string name, ref DateTime value)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return false;

      DateTime x2;
      if (Converter.TryParse(s, out x2, false))
      {
        value = x2;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Получить значение даты.
    /// Если в хранилище нет такой строки или там записано неправильное значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// Компонент времени не устанавливается.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public DateTime GetDateDef(string name, DateTime defValue)
    {
      GetDate(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// Компонент времени не записывается.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Записываемое значение</param>
    public void SetDate(string name, DateTime value)
    {
      SetString(name, Converter.ToString(value, false));
    }

    // Нет перегрузки SetDate() с аргументом RemoveEmpty, т.к. нет пустого значения

    #endregion

    #region Дата и время

    /// <summary>
    /// Получить значение типа дата/время. Если дата не записана или имеет неправильный
    /// формат, возвращается default(DateTime)
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Прочитанная дата</returns>
    public DateTime GetDateTime(string name)
    {
      string s = GetString(name);
      DateTime value;
      Converter.TryParse(s, out value, true);
      return value;
    }

    /// <summary>
    /// Попытаться получить значение типа дата/время
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Записываемое значение по ссылке</param>
    /// <returns>True, если время было установлено</returns>
    public bool GetDateTime(string name, ref DateTime value)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return false;
      DateTime x2;
      if (Converter.TryParse(s, out x2, true))
      {
        value = x2;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Получить значение даты и времени.
    /// Если в хранилище нет такой строки или там записано неправильное значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public DateTime GetDateTimeDef(string name, DateTime defValue)
    {
      GetDateTime(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Записываемое значение</param>
    public void SetDateTime(string name, DateTime value)
    {
      SetString(name, Converter.ToString(value, true));
    }

    // Нет перегрузки SetDateTime() с аргументом RemoveEmpty, т.к. нет пустого значения

    #endregion

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить значение типа TimeSpan.
    /// Если в хранилище нет такой строки, возвращается пустой Guid
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public TimeSpan GetTimeSpan(string name)
    {
      return GetTimeSpanDef(name, TimeSpan.Zero);
    }

    /// <summary>
    /// Получить значение типа TimeSpan.
    /// Если в хранилище нет такой строки, сохраняется существующее значение <paramref name="value"/>.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetTimeSpan(string name, ref TimeSpan value)
    {
      string s = GetString(name);
      TimeSpan x2;
      if (Converter.TryParse(s, out x2))
      {
        value = x2;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Получить значение типа TimeSpan.
    /// Если в хранилище нет такой строки, возвращается значение <paramref name="defValue"/>.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public TimeSpan GetTimeSpanDef(string name, TimeSpan defValue)
    {
      GetTimeSpan(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение типа TimeSpan.
    /// Строка записывается, даже если <paramref name="value"/> задает пустое значение Guid.Empty.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Записываемое значение</param>
    public void SetTimeSpan(string name, TimeSpan value)
    {
      SetTimeSpan(name, value, false);
    }

    /// <summary>
    /// Записать значение типа TimeSpan.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает пустое значение Guid.Empty, то запись удаляется из хранилища.</param>
    public void SetTimeSpan(string name, TimeSpan value, bool removeEmpty)
    {
      if (removeEmpty && value == TimeSpan.Zero)
      {
        Remove(name);
        return;
      }

      SetString(name, Converter.ToString(value));
    }

    #endregion

    #region Enum

    /// <summary>
    /// Получить значение перечислимого типа
    /// Предполагается, что значение хранится как строка.
    /// Если нет сохраненного значения с заданным именем <paramref name="name"/>, то будет возвращено 
    /// перечислимое значение, соответствующее числовому значению 0 (даже если в перечислении нет такого элемента)
    /// Если препочтительнее хранить числовые значения, используйте GetInt()
    /// </summary>
    /// <typeparam name="T">Тип перечисления. Если T не является перечислением, возникнет ошибка времени выполнения</typeparam>
    /// <param name="name">Имя</param>
    /// <returns>Перечислимое значение</returns>
    public T GetEnum<T>(string name)
      where T : struct
    {
      T value = default(T);
      GetEnum<T>(name, ref value);
      return value;
    }

    /// <summary>
    /// Получить значение перечислимого типа
    /// Предполагается, что значение хранится как строка.
    /// Если нет сохраненного значения с заданным именем <paramref name="name"/>, то текущее значение,
    /// переданное по ссылке не изменяется
    /// Если препочтительнее хранить числовые значения, используйте GetInt()
    /// </summary>
    /// <typeparam name="T">Тип перечисления. Если T не является перечислением, возникнет ошибка времени выполнения</typeparam>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Текущее значение. Если существует строка с именем <paramref name="name"/>, то сюда помещается преобразованное значение</param>
    /// <returns>true, если значение было успешно прочитано</returns>
    [DebuggerStepThrough]
    public bool GetEnum<T>(string name, ref T value)
      where T : struct
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return false;
      try
      {
        // К сожалению, в Net Framework 2 нет метода TryParse()
        value = (T)Enum.Parse(typeof(T), s, true);
      }
      catch
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Получить значение перечислимого типа.
    /// Если в хранилище нет такой строки или там записано неправильное значение, возвращается 
    /// значение <paramref name="defValue"/>.
    /// </summary>
    /// <typeparam name="T">Тип перечисления. Если T не является перечислением, возникнет ошибка времени выполнения</typeparam>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public T GetEnumDef<T>(string name, T defValue)
      where T : struct
    {
      GetEnum<T>(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение.
    /// Записывается строкое значение <paramref name="value"/>.ToString(), а не числовой идентификатор.
    /// </summary>
    /// <typeparam name="T">Тип перечисления. Если T не является перечислением, возникнет ошибка времени выполнения</typeparam>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Записываемое значение</param>
    public void SetEnum<T>(string name, T value)
      where T : struct
    {
      SetString(name, value.ToString());
    }

    #endregion

    #region Guid

    /// <summary>
    /// Получить значение типа Guid.
    /// Если в хранилище нет такой строки, возвращается пустой Guid
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public Guid GetGuid(string name)
    {
      return GetGuidDef(name, Guid.Empty);
    }

    /// <summary>
    /// Получить значение типа Guid.
    /// Если в хранилище нет такой строки, сохраняется существующее значение <paramref name="value"/>.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Сюда будет помещено прочитанное значение в случае успеха</param>
    /// <returns>true, если значение было прочитано, false в случае неудачи</returns>
    public bool GetGuid(string name, ref Guid value)
    {
      string s = GetString(name);
      Guid x2;
      if (Converter.TryParse(s, out x2))
      {
        value = x2;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Получить значение типа Guid.
    /// Если в хранилище нет такой строки, возвращается значение <paramref name="defValue"/>.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="defValue">Значение по умолчанию</param>
    /// <returns>Прочитанное значение или значение по умолчанию</returns>
    public Guid GetGuidDef(string name, Guid defValue)
    {
      GetGuid(name, ref defValue);
      return defValue;
    }

    /// <summary>
    /// Записать значение типа Guid.
    /// Строка записывается, даже если <paramref name="value"/> задает пустое значение Guid.Empty.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Записываемое значение</param>
    public void SetGuid(string name, Guid value)
    {
      SetGuid(name, value, false);
    }

    /// <summary>
    /// Записать значение типа Guid.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает пустое значение Guid.Empty, то запись удаляется из хранилища.</param>
    public void SetGuid(string name, Guid value, bool removeEmpty)
    {
      if (removeEmpty && value == Guid.Empty)
      {
        Remove(name);
        return;
      }

      SetString(name, Converter.ToString(value));
    }

    #endregion

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает дочернюю группу параметров
    /// Если <paramref name="create"/> равно true, то, при необходимости, создается новая группа
    /// Если <paramref name="create"/> равно false, то, при отсутствии секции с указанным именем,
    /// возвращается null
    /// </summary>
    /// <param name="name">Имя дочерней секции. Не может быть пустой строкой</param>
    /// <param name="create">Необходимость создания секции</param>
    /// <returns>Дочерняя секция или null</returns>
    public CfgPart GetChild(string name, bool create)
    {
      ValidateName(name);
      return DoGetChild(name, create);
    }

    /// <summary>
    /// Возвращает дочернюю группу параметров
    /// Если <paramref name="create"/> равно true, то, при необходимости, создается новая группа
    /// Если <paramref name="create"/> равно false, то, при отсутствии секции с указанным именем,
    /// возвращается null
    /// </summary>
    /// <param name="name">Имя дочерней секции. Не может быть пустой строкой</param>
    /// <param name="create">Необходимость создания секции</param>
    /// <returns>Дочерняя секция или null</returns>
    protected abstract CfgPart DoGetChild(string name, bool create);

    /// <summary>
    /// Получить список дочерних секций
    /// </summary>
    /// <returns></returns>
    public abstract string[] GetChildNames();

    /// <summary>
    /// Получить список хранящихся значений
    /// </summary>
    /// <returns></returns>
    public abstract string[] GetValueNames();

    /// <summary>
    /// Получить список имен дочерних узлов и хранящихся значений
    /// Возвращает объединенение списков, возвращаемых методами GetChildNames() и GteValueNames
    /// </summary>
    /// <returns></returns>
    public virtual string[] GetChildAndValueNames()
    {
      string[] a1 = GetChildNames();
      string[] a2 = GetValueNames();
      if (a1.Length == 0)
        return a2;
      if (a2.Length == 0)
        return a1;
      return DataTools.MergeArraysOnce<string>(a1, a2);
    }

    /// <summary>
    /// Удаление вложенной секции и/или параметра с заданным именем
    /// </summary>
    /// <param name="name">Имя секции или параметра. Не может быть пустой строкой</param>
    public void Remove(string name)
    {
      ValidateName(name);
      DoRemove(name);
    }

    /// <summary>
    /// Удаление вложенной секции и/или параметра с заданным именем
    /// </summary>
    /// <param name="name">Имя секции или параметра. Не может быть пустой строкой</param>
    protected abstract void DoRemove(string name);

    /// <summary>
    /// Очистка всех значений и вложенных секций
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Возвращает true, если есть дочерняя секция с таким именем.
    /// </summary>
    /// <param name="name">Имя дочерней секции. Если задана пустая строка, метод возвращает false</param>
    /// <returns>Наличие дочерней секции</returns>
    public bool HasChild(string name)
    {
      if (String.IsNullOrEmpty(name))
        return false; // 25.07.2019

      ValidateName(name);
      return DoHasChild(name);
    }

    /// <summary>
    /// Возвращает true, если есть дочерняя секция с таким именем.
    /// </summary>
    /// <param name="name">Имя дочерней секции. Не может быть пустой строкой</param>
    /// <returns>Наличие дочерней секции</returns>
    protected abstract bool DoHasChild(string name);

    /// <summary>
    /// Возвращает true, если есть параметр с указанным именем
    /// </summary>
    /// <param name="name">Имя параметра. Если задана пустая строка, метод возвращает false</param>
    /// <returns>Наличие записанного значения</returns>
    public bool HasValue(string name)
    {
      if (String.IsNullOrEmpty(name))
        return false; // 25.07.2019

      ValidateName(name);
      return DoHasValue(name);
    }

    /// <summary>
    /// Возвращает true, если есть параметр с указанным именем
    /// </summary>
    /// <param name="name">Имя параметра. Не моет быть пустой строкой</param>
    /// <returns>Наличие записанного значения</returns>
    protected abstract bool DoHasValue(string name);

    /// <summary>
    /// Возвращает true, если ли есть дочерняя секция или параметр с заданным именем.
    /// Комбинация HasChild() и HasValue()
    /// </summary>
    /// <param name="name">Имя дочерней секции или параметра. Если задана пустая строка, метод возвращает false</param>
    /// <returns>Наличие дочерней секции или записанного значения</returns>
    public virtual bool HasChildOrValue(string name)
    {
      return HasChild(name) || HasValue(name);
    }

    /// <summary>
    /// Вычисление контрольной суммы для секции, включая дочерние секции
    /// </summary>
    /// <returns>Сумма</returns>
    public virtual string MD5Sum()
    {
      // Только XmlCfgPart умеет действительно вычислять контрольную сумму
      TempCfg cfg2 = new TempCfg();
      CopyTo(cfg2);
      return cfg2.MD5Sum();
    }

    /// <summary>
    /// Возвращает данные в виде текста XML
    /// </summary>
    /// <returns>Текстовая строка, содержащая представление XML</returns>
    public virtual string GetXmlText()
    {
      TempCfg cfg2 = new TempCfg();
      CopyTo(cfg2);
      return cfg2.GetXmlText();
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Конвертер, используемый для преобразования числовых значений и дат в хранимый строковый формат и обратно.
    /// </summary>
    public abstract CfgConverter Converter { get; }

    /// <summary>
    /// Возвращает true, если секция не содержит ни значений, ни вложенных секций
    /// </summary>
    public abstract bool IsEmpty { get; }

    #endregion

    #region Доступ к Nullable-значениям

    /// <summary>
    /// Получить nullable-значение.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<int> GetNullableInt(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        int res;
        if (Converter.TryParse(s, out res))
          return res;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Нулевое значение записывается обычным образом.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableInt(string name, Nullable<int> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value));
      else
        Remove(name);
    }


    /// <summary>
    /// Получить nullable-значение.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<long> GetNullableInt64(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        long res;
        if (Converter.TryParse(s, out res))
          return res;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Нулевое значение записывается обычным образом.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableInt64(string name, Nullable<long> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value));
      else
        Remove(name);
    }


    /// <summary>
    /// Получить nullable-значение.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<bool> GetNullableBool(string name)
    {
      Nullable<int> x = GetNullableInt(name);
      if (x.HasValue)
        return x.Value != 0;
      else
        return null;
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Нулевое значение записывается обычным образом.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableBool(string name, Nullable<bool> value)
    {
      if (value.HasValue)
        SetString(name, value.Value ? "1" : "0");
      else
        Remove(name);
    }


    /// <summary>
    /// Получить nullable-значение.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<float> GetNullableSingle(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        float res;
        if (Converter.TryParse(s, out res))
          return res;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Нулевое значение записывается обычным образом.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableSingle(string name, Nullable<float> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value));
      else
        Remove(name);
    }


    /// <summary>
    /// Получить nullable-значение.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<double> GetNullableDouble(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        double res;
        if (Converter.TryParse(s, out res))
          return res;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Нулевое значение записывается обычным образом.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableDouble(string name, Nullable<double> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value));
      else
        Remove(name);
    }


    /// <summary>
    /// Получить nullable-значение.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<decimal> GetNullableDecimal(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        decimal res;
        if (Converter.TryParse(s, out res))
          return res;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Нулевое значение записывается обычным образом.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableDecimal(string name, Nullable<decimal> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value));
      else
        Remove(name);
    }


    /// <summary>
    /// Получить nullable-значение даты.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// Компонент времени не считывается.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<DateTime> GetNullableDate(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        DateTime value;
        if (Converter.TryParse(s, out value, false))
          return value;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Компонент времени не записывается.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableDate(string name, Nullable<DateTime> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value, false));
      else
        Remove(name);
    }


    /// <summary>
    /// Получить nullable-значение даты и времени.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<DateTime> GetNullableDateTime(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        DateTime value;
        if (Converter.TryParse(s, out value, true))
          return value;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение даты и времени, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableDateTime(string name, Nullable<DateTime> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value, true));
      else
        Remove(name);
    }

    /// <summary>
    /// Получить nullable-значение.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<TimeSpan> GetNullableTimeSpan(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        TimeSpan value;
        if (Converter.TryParse(s, out value))
          return value;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Нулевое значение записывается обычным образом.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableTimeSpan(string name, Nullable<TimeSpan> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value));
      else
        Remove(name);
    }


    /// <summary>
    /// Получить nullable-значение.
    /// Если в хранилище нет строки с заданным именем, или строкое значение нельзя преобразовать,
    /// возвращается null.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Nullable-значение</returns>
    public Nullable<Guid> GetNullableGuid(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      else
      {
        Guid res;
        if (Converter.TryParse(s, out res))
          return res;
        else
          return null;
      }
    }

    /// <summary>
    /// Записать значение, если оно задано.
    /// Удаляет запись из хранилища, если <paramref name="value"/> содержит null.
    /// Значение Guid.Empty записывается обычным образом.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Nullable-значение</param>
    public void SetNullableGuid(string name, Nullable<Guid> value)
    {
      if (value.HasValue)
        SetString(name, Converter.ToString(value.Value));
      else
        Remove(name);
    }

    #endregion

    #region Доступ к массивам значений

    /// <summary>
    /// Записывает массив целых чисел в виде одной строки, разделенной запятыми.
    /// Если задано значение null, то записывается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="values">Массив записываемых значений</param>
    public void SetIntCommaString(string name, int[] values)
    {
      // Нельзя использовать DataTools.CommaStringFromIds(), т.к. он не учитывает объект Converter
      //if (Values == null)
      //  Remove(Name);
      //else
      //  SetString(Name, DataTools.CommaStringFromIds(Values, false));

      if (values == null)
        Remove(name);
      else
      {
        string[] a = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
          a[i] = Converter.ToString(values[i]);
        SetString(name, String.Join(Converter.ListSeparator, a));
      }
    }

    /// <summary>
    /// Считывает строку как массив целых чисел.
    /// Если строка пустая, возвращается null, а не пустой массив.
    /// Если строка имеет неправильный формат, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Массив прочитанных значений или null</returns>
    public int[] GetIntCommaString(string name)
    {
      // Нельзя использовать DataTools.CommaStringToIds(), т.к. он не учитывает объект Converter
      // return DataTools.CommaStringToIds(GetString(Name));

      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      string[] a = s.Split(new string[] { Converter.ListSeparator }, StringSplitOptions.None);
      int[] values = new int[a.Length];
      for (int i = 0; i < values.Length; i++)
        values[i] = Converter.ParseInt(a[i].Trim());

      return values;
    }


    /// <summary>
    /// Записывает массив целых чисел в виде одной строки, разделенной запятыми.
    /// Если задано значение null, то записывается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="values">Массив записываемых значений</param>
    public void SetInt64CommaString(string name, Int64[] values)
    {
      if (values == null)
        Remove(name);
      else
      {
        string[] a = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
          a[i] = Converter.ToString(values[i]);
        SetString(name, String.Join(Converter.ListSeparator, a));
      }
    }

    /// <summary>
    /// Считывает строку как массив целых чисел.
    /// Если строка пустая, возвращается null, а не пустой массив.
    /// Если строка имеет неправильный формат, генерируется исключение
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Массив прочитанных значений или null</returns>
    public Int64[] GetInt64CommaString(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      string[] a = s.Split(new string[] { Converter.ListSeparator }, StringSplitOptions.None);
      Int64[] values = new Int64[a.Length];
      for (int i = 0; i < values.Length; i++)
        values[i] = Converter.ParseInt64(a[i].Trim());

      return values;
    }


    /// <summary>
    /// Записывает массив чисел в виде одной строки, разделенной запятыми.
    /// Если задано значение null, то записывается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="values">Массив записываемых значений</param>
    public void SetSingleCommaString(string name, float[] values)
    {
      if (values == null)
        Remove(name);
      else
      {
        string[] a = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
          a[i] = Converter.ToString(values[i]);
        SetString(name, String.Join(Converter.ListSeparator, a));
      }
    }

    /// <summary>
    /// Считывает строку как массив чисел.
    /// Если строка пустая, возвращается null, а не пустой массив.
    /// Если строка имеет неправильный формат, генерируется исключение
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Массив прочитанных значений или null</returns>
    public float[] GetSingleCommaString(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      string[] a = s.Split(new string[] { Converter.ListSeparator }, StringSplitOptions.None);
      float[] values = new float[a.Length];
      for (int i = 0; i < values.Length; i++)
        values[i] = Converter.ParseSingle(a[i].Trim());

      return values;
    }


    /// <summary>
    /// Записывает массив чисел в виде одной строки, разделенной запятыми.
    /// Если задано значение null, то записывается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="values">Массив записываемых значений</param>
    public void SetDoubleCommaString(string name, double[] values)
    {
      if (values == null)
        Remove(name);
      else
      {
        string[] a = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
          a[i] = Converter.ToString(values[i]);
        SetString(name, String.Join(Converter.ListSeparator, a));
      }
    }

    /// <summary>
    /// Считывает строку как массив чисел.
    /// Если строка пустая, возвращается null, а не пустой массив.
    /// Если строка имеет неправильный формат, генерируется исключение
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Массив прочитанных значений или null</returns>
    public double[] GetDoubleCommaString(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      string[] a = s.Split(new string[] { Converter.ListSeparator }, StringSplitOptions.None);
      double[] values = new double[a.Length];
      for (int i = 0; i < values.Length; i++)
        values[i] = Converter.ParseDouble(a[i].Trim());

      return values;
    }


    /// <summary>
    /// Записывает массив чисел в виде одной строки, разделенной запятыми.
    /// Если задано значение null, то записывается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="values">Массив записываемых значений</param>
    public void SetDecimalCommaString(string name, decimal[] values)
    {
      if (values == null)
        Remove(name);
      else
      {
        string[] a = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
          a[i] = Converter.ToString(values[i]);
        SetString(name, String.Join(Converter.ListSeparator, a));
      }
    }

    /// <summary>
    /// Считывает строку как массив чисел.
    /// Если строка пустая, возвращается null, а не пустой массив.
    /// Если строка имеет неправильный формат, генерируется исключение
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Массив прочитанных значений или null</returns>
    public decimal[] GetDecimalCommaString(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      string[] a = s.Split(new string[] { Converter.ListSeparator }, StringSplitOptions.None);
      decimal[] values = new decimal[a.Length];
      for (int i = 0; i < values.Length; i++)
        values[i] = Converter.ParseDecimal(a[i].Trim());

      return values;
    }


    /// <summary>
    /// Записывает массив дат в виде одной строки, разделенной запятыми.
    /// Если задано значение null, то записывается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="values">Массив записываемых значений</param>
    /// <param name="useTime">true - использовать компонент времени</param>
    public void SetDateTimeCommaString(string name, DateTime[] values, bool useTime)
    {
      if (values == null)
        Remove(name);
      else
      {
        string[] a = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
          a[i] = Converter.ToString(values[i], useTime);
        SetString(name, String.Join(Converter.ListSeparator, a));
      }
    }

    /// <summary>
    /// Считывает строку как массив дат.
    /// Если строка пустая, возвращается null, а не пустой массив.
    /// Если строка имеет неправильный формат, генерируется исключение
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="useTime">true - использовать компонент времени</param>
    /// <returns>Массив прочитанных значений или null</returns>
    public DateTime[] GetDateTimeCommaString(string name, bool useTime)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      string[] a = s.Split(new string[] { Converter.ListSeparator }, StringSplitOptions.None);
      DateTime[] values = new DateTime[a.Length];
      for (int i = 0; i < values.Length; i++)
        values[i] = Converter.ParseDateTime(a[i].Trim(), useTime);

      return values;
    }


    /// <summary>
    /// Записывает массив значений TimeSpan в виде одной строки, разделенной запятыми.
    /// Если задано значение null, то записывается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="values">Массив записываемых значений</param>
    public void SetTimeSpanCommaString(string name, TimeSpan[] values)
    {
      if (values == null)
        Remove(name);
      else
      {
        string[] a = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
          a[i] = Converter.ToString(values[i]);
        SetString(name, String.Join(Converter.ListSeparator, a));
      }
    }

    /// <summary>
    /// Считывает строку как массив значений.
    /// Если строка пустая, возвращается null, а не пустой массив.
    /// Если строка имеет неправильный формат, генерируется исключение
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Массив прочитанных значений или null</returns>
    public TimeSpan[] GetTimeSpanCommaString(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      string[] a = s.Split(new string[] { Converter.ListSeparator }, StringSplitOptions.None);
      TimeSpan[] values = new TimeSpan[a.Length];
      for (int i = 0; i < values.Length; i++)
        values[i] = Converter.ParseTimeSpan(a[i].Trim());

      return values;
    }


    /// <summary>
    /// Записывает массив Guid'ов в виде одной строки, разделенной запятыми.
    /// Если задано значение null, то записывается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="values">Массив записываемых значений</param>
    public void SetGuidCommaString(string name, Guid[] values)
    {
      if (values == null)
        Remove(name);
      else
      {
        string[] a = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
          a[i] = Converter.ToString(values[i]);
        SetString(name, String.Join(Converter.ListSeparator, a));
      }
    }

    /// <summary>
    /// Считывает строку как массив Guid'ов.
    /// Если строка пустая, возвращается null, а не пустой массив.
    /// Если строка имеет неправильный формат, генерируется исключение
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Массив прочитанных значений или null</returns>
    public Guid[] GetGuidCommaString(string name)
    {
      string s = GetString(name);
      if (String.IsNullOrEmpty(s))
        return null;
      string[] a = s.Split(new string[] { Converter.ListSeparator }, StringSplitOptions.None);
      Guid[] values = new Guid[a.Length];
      for (int i = 0; i < values.Length; i++)
        values[i] = Converter.ParseGuid(a[i].Trim());

      return values;
    }

    #endregion

    #region Хранение списка истории

    /*
     * Хранение строки с историей изменений
     * Имя NamePart задает имя текущего значения, поэтому функции Get/SetHist совместимы
     * с GetString().
     * Для истории используется дополнительная секция с именем "Name_Hist"
     * Внутри секции хранятся строки с именами "H1", "H2", ...
     * Например, если NamePart="ИмяФайла", то в XML-файле будут следующие узлы:
     *    ИмяФайла
     *    ИмяФайла_Hist
     *       H1
     *       H2
     *       H3
     */

    /// <summary>
    /// Записать список истории.
    /// Для первого списка истории будет выполнен обычный вызов SetString().
    /// Также создается дочернеяя секция конфигурации с именем "Name_Hist", в которую 
    /// добавляются узлы "H1", "H2", ..., куда помещаются строки списка.
    /// Если список <paramref name="histList"/> пустой, то из хранилища удаляется запись "Name" и
    /// секция "Name_Hist".
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="histList">Список истории</param>
    public void SetHist(string name, HistoryList histList)
    {
      if (histList.IsEmpty)
      {
        // Выполняем очистку
        Remove(name);
        Remove(name + "_Hist");
      }
      else
      {
        SetString(name, histList[0]);
        if (histList.Count > 1)
        {
          CfgPart Part = GetChild(name + "_Hist", true);
          Part.Clear();
          for (int i = 1; i < histList.Count; i++)
            Part.SetString("H" + i.ToString(), histList[i]);
        }
        else
          // Один элемент в списке - история стирается
          Remove(name + "_Hist");
      }
    }

    /// <summary>
    /// Прочитать список истории, записанный SetHist(). Также может быть прочитано значение,
    /// записанное простым вызовом SetString() (список из одной строки).
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Список</returns>
    public HistoryList GetHist(string name)
    {
      string mainValue = GetString(name);
      CfgPart part = GetChild(name + "_Hist", false);
      if (String.IsNullOrEmpty(mainValue) && part == null)
        return new HistoryList();
      else
      {
        //List<string> lst = new List<string>();
        SingleScopeList<string> lst = new SingleScopeList<string>(); // 20.06.2018
        lst.Add(mainValue);
        if (part != null)
        {
          int cnt = 0;
          while (true)
          {
            cnt++;
            string s = part.GetString("H" + cnt.ToString());
            if (String.IsNullOrEmpty(s))
              break;
            lst.Add(s);
          }
        }
        return new HistoryList(lst);
      }
    }

    /// <summary>
    /// Добавить запись к списку истории.
    /// Эта перегрузка использует HistoryList.DefaultMaxHistLength для ограничения максимального размера списка истории.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Добавляемое значение истории</param>
    public void AddToHist(string name, string value)
    {
      AddToHist(name, value, HistoryList.DefaultMaxHistLength);
    }

    /// <summary>
    /// Добавить запись к списку истории.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="value">Добавляемое значение истории</param>
    /// <param name="maxHistLength">Максимальная длина списка. Если список превысит этот предел, то самые послежние записи будут удалены из списка</param>
    public void AddToHist(string name, string value, int maxHistLength)
    {
      HistoryList hist = GetHist(name);
      hist = hist.Add(value, maxHistLength);
      SetHist(name, hist);
    }

    #endregion

    #region Копирование

    /// <summary>
    /// Копирует все значения и вложенные секции конфигурации (рекурсивно) в указанную секцию.
    /// Если заполняемая секция <paramref name="dest"/> уже содержит записи, то, при совпадении имен
    /// значения переписываются. Если в  <paramref name="dest"/> есть имена, отсутствующие в текущей
    /// секции, то они сохраняются.
    /// Таким образом, секция <paramref name="dest"/> будет солержать объединение старых и новых записей
    /// и дочерних секций.
    /// </summary>
    /// <param name="dest">Заполняемая секция</param>
    public virtual void CopyTo(CfgPart dest)
    {
      if (dest == null)
        throw new ArgumentNullException("dest");

      string[] valueNames = GetValueNames();
      for (int i = 0; i < valueNames.Length; i++)
      {
        string s = GetString(valueNames[i]);
        dest.SetString(valueNames[i], s);
      }

      string[] childNames = GetChildNames();
      for (int i = 0; i < childNames.Length; i++)
      {
        CfgPart srcChild = GetChild(childNames[i], false);
        CfgPart destChild = dest.GetChild(childNames[i], true);
        // Рекурсивный вызов
        srcChild.CopyTo(destChild);
      }
    }

    #endregion

    #region Свойство Empty

    /// <summary>
    /// Фиктивная пустая секция конфигурации, предназначенная только для чтения
    /// </summary>
    private class EmptyCfgPart : CfgPart
    {
      #region Свойства и методы-заглушки

      protected override string DoGetString(string name)
      {
        return String.Empty;
      }

      protected override void DoSetString(string name, string value, bool removeEmpty)
      {
        throw new ObjectReadOnlyException();
      }

      protected override CfgPart DoGetChild(string name, bool create)
      {
        if (create)
          throw new ObjectReadOnlyException();
        else
          return null;
      }

      public override string[] GetChildNames()
      {
        return DataTools.EmptyStrings;
      }

      public override string[] GetValueNames()
      {
        return DataTools.EmptyStrings;
      }

      protected override void DoRemove(string name)
      {
        throw new ObjectReadOnlyException();
      }

      public override void Clear()
      {
        throw new ObjectReadOnlyException();
      }

      protected override bool DoHasChild(string name)
      {
        return false;
      }

      protected override bool DoHasValue(string name)
      {
        return false;
      }

      public override CfgConverter Converter
      {
        get { return CfgConverter.Default; }
      }

      public override bool IsEmpty { get { return true; } }

      #endregion
    }

    /// <summary>
    /// Пустая секция конфигурации с возможностью доступа только для чтения.
    /// Все записывающие методы вызывают ObjectReadOnlyException
    /// </summary>
    public static readonly CfgPart Empty = new EmptyCfgPart();

    #endregion

    #region Проверка имени

    /// <summary>
    /// Символы, которые можно использовать в именах, кроме букв и цифр
    /// </summary>
    private static CharArrayIndexer _ValidNameCharIndexer = new CharArrayIndexer("_.-");

    /// <summary>
    /// Проверка корректности имени.
    /// Возвращает true, если имя корректное.
    /// </summary>
    /// <param name="name">Проверяемое имя</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <returns>Корректность имени</returns>
    public static bool IsValidName(string name, out string errorText)
    {
      if (String.IsNullOrEmpty(name))
      {
        errorText = "Имя не задано";
        return false;
      }

      if (!(Char.IsLetter(name[0]) || name[0] == '_'))
      {
        errorText = "Имя должно начинаться с буквы или знака подчеркивания";
        return false;
      }

      for (int i = 1; i < name.Length; i++)
      {
        if (Char.IsLetterOrDigit(name[i]))
          continue;
        if (_ValidNameCharIndexer.Contains(name[i]))
          continue;

        errorText = "Недопустимый символ \"" + name[i] + "\" в позиции " + (i + 1).ToString() + ". Допускаются только буквы, цифры и символы \"-\", \"_\" и \".\"";
        return false;
      }

      errorText = null;
      return true;
    }

    /// <summary>
    /// Проверка корректности имени.
    /// Возвращает true, если имя корректное.
    /// </summary>
    /// <param name="name">Проверяемое имя</param>
    /// <returns>Корректность имени</returns>
    public static bool IsValidName(string name)
    {
      string errorText;
      return IsValidName(name, out errorText);
    }

    /// <summary>
    /// Проверяет аргумент <paramref name="name"/>. Если имя некорректное, выбрасывается исключение ArgumentException.
    /// </summary>
    /// <param name="name">Проверяемый аргумент</param>
    public static void ValidateName(string name)
    {
      string errorText;
      if (!IsValidName(name, out errorText))
      {
        if (String.IsNullOrEmpty(name))
          throw new ArgumentNullException("name");
        else
          throw new ArgumentException(errorText, "name");
      }
    }

    #endregion
  }

  #endregion

  #region Хранение настроек в XML-файле

  /// <summary>
  /// Секция, хранящаяся в XML-формате.
  /// Для доступа к секциям используйте XmlCfgFile или TempCfg.
  /// </summary>
  public class XmlCfgPart : CfgPart
  {
    #region Защищенный конструктор

    /// <summary>
    /// Защищенный конструктор, которому передается существующий тег в качесте корня
    /// </summary>
    /// <param name="converter">Конвертер для преобразование чисел и дат в строковый формат для хранения</param>
    /// <param name="document">Объект XmlDocument</param>
    /// <param name="rootNode">Корневой узел для заданной секции</param>
    /// <param name="parent">Родительская секция</param>
    internal protected XmlCfgPart(CfgConverter converter, XmlDocument document, XmlNode rootNode, XmlCfgPart parent)
    {
      _Converter = converter;
      _Document = document;
      _RootNode = rootNode;
      _Parent = parent;
    }

    /// <summary>
    /// Защищенный конструктор, создающий XmlDocument и корневой тег "Config" 
    /// </summary>
    /// <param name="converter">Конвертер для преобразование чисел и дат в строковый формат для хранения</param>
    internal protected XmlCfgPart(CfgConverter converter)
    {
      _Converter = converter;
      _Document = new XmlDocument();

      _RootNode = _Document.CreateElement("Config");
      _Document.AppendChild(_RootNode);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Объект XmlDocument, к которому относится секция
    /// </summary>
    public XmlDocument Document { get { return _Document; } }
    private readonly XmlDocument _Document;

    /// <summary>
    /// Корневой узел для заданной секции.
    /// </summary>
    public XmlNode RootNode
    {
      get { return _RootNode; }
      protected set { _RootNode = value; }
    }
    private XmlNode _RootNode;

    /// <summary>
    /// Родительская секция
    /// </summary>
    public XmlCfgPart Parent { get { return _Parent; } }
    private readonly XmlCfgPart _Parent;

    /// <summary>
    /// Возвращает true, если секция не содержит ни одного значения или дочернего узла
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !RootNode.HasChildNodes;
      }
    }

    /// <summary>
    /// Конвертер для преобразование чисел и дат в строковый формат для хранения
    /// </summary>
    public sealed override CfgConverter Converter { get { return _Converter; } }
    private readonly CfgConverter _Converter;

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получить строкое значение.
    /// Возвращает InnerText для элемента с заданным именем или пустую строку, если нет тега с таким именем.
    /// </summary>
    /// <param name="name">Имя тега. Не может быть пустой строкой</param>
    /// <returns>Значение</returns>
    protected override string DoGetString(string name)
    {
      XmlNode node = RootNode.SelectSingleNode(name);
      if (node == null)
        return String.Empty;
      return node.InnerText;
    }

    /// <summary>
    /// Записать значение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Удалить тег, если задано пустое значение</param>
    protected override void DoSetString(string name, string value, bool removeEmpty)
    {
      if (removeEmpty && String.IsNullOrEmpty(value))
      {
        Remove(name);
        return;
      }

      XmlNode node = RootNode.SelectSingleNode(name);
      if (node == null)
      {
        node = Document.CreateElement(name);
        RootNode.AppendChild(node);
      }
      XmlText xt = Document.CreateTextNode(value);
      node.RemoveAll();
      node.AppendChild(xt);

      PerformChange();
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Очистка всех значений и вложенных секций
    /// </summary>
    public override void Clear()
    {
      RootNode.RemoveAll();
      PerformChange();
    }

    /// <summary>
    /// Доступ к дочерней секции
    /// </summary>
    /// <param name="name">Имя дочерней секции. Не может быть пустой строкой</param>
    /// <param name="create">True, если требуется создать секцию, если она не существует.
    /// Если false и дочерней секции не существует, возвращается null</param>
    /// <returns>Дочерняя секция или null</returns>
    protected override CfgPart DoGetChild(string name, bool create)
    {
      XmlNode node = RootNode.SelectSingleNode(name);
      if (node != null)
      {
        if (!(node is XmlElement))
        {
          RootNode.RemoveChild(node);
          node = null;
        }
      }
      if (node == null)
      {
        if (!create)
          return null;
        node = Document.CreateElement(name);
        RootNode.AppendChild(node);
        PerformChange();
      }
      return new XmlCfgPart(Converter, Document, node, this);
    }

    /// <summary>
    /// Получить список имен дочерних секций (не значений)
    /// </summary>
    /// <returns>Имена секций</returns>
    public override string[] GetChildNames()
    {
      XmlNodeList list = RootNode.ChildNodes;
      List<string> Names = new List<string>(list.Count);
      for (int i = 0; i < list.Count; i++)
      {
        if (HasChild(list[i].Name))
          Names.Add(list[i].Name);
      }
      return Names.ToArray();
    }

    /// <summary>
    /// Получить список имен значений
    /// </summary>
    /// <returns>Имена значений</returns>
    public override string[] GetValueNames()
    {
      XmlNodeList list = RootNode.ChildNodes;
      List<string> names = new List<string>(list.Count);
      foreach (XmlNode node in list)
      {
        if (HasValue(node.Name))
          names.Add(node.Name);
      }
      return names.ToArray();
    }

    /// <summary>
    /// Получить список имен дочерних секций (не значений)
    /// </summary>
    /// <returns>Имена дочерних секций и имен</returns>
    public override string[] GetChildAndValueNames()
    {
      XmlNodeList list = RootNode.ChildNodes;
      string[] names = new string[list.Count];
      for (int i = 0; i < list.Count; i++)
        names[i] = list[i].Name;
      return names;
    }


    /// <summary>
    /// Удаление значения или дочернего узла
    /// </summary>
    /// <param name="name">Имя поля или дочерней секции. Не может быть пустой строкой</param>
    protected override void DoRemove(string name)
    {
      XmlNode node = RootNode.SelectSingleNode(name);
      if (node != null)
      {
        RootNode.RemoveChild(node);
        PerformChange();
      }
    }

    /// <summary>
    /// Возвращает true, если есть дочерний тег с указанным именем и он содержит вложенные теги
    /// </summary>
    /// <param name="name">Имя дочерней секции. Если задана пустая строка, метод возвращает false</param>
    /// <returns>Наличие дочерней секции</returns>
    protected override bool DoHasChild(string name)
    {
      XmlElement el = RootNode.SelectSingleNode(name) as XmlElement;
      if (el != null)
      {
        if (el.FirstChild is XmlElement)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращае тrue, если есть записанное значение с указанным именем
    /// </summary>
    /// <param name="name">Имя параметра. Если задана пустая строка, метод возвращает false</param>
    /// <returns>Наличие записанного значения</returns>
    protected override bool DoHasValue(string name)
    {
      XmlElement el = RootNode.SelectSingleNode(name) as XmlElement;
      if (el != null)
      {
        if (el.FirstChild != null)
        {
          return (el.FirstChild is XmlText);
        }
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если ли есть дочерняя секция или параметр с заданным именем.
    /// Комбинация HasChild() и HasValue()
    /// </summary>
    /// <param name="name">Имя дочерней секции или параметра. Если задана пустая строка, метод возвращает false</param>
    /// <returns>Наличие дочерней секции или записанного значения</returns>
    public override bool HasChildOrValue(string name)
    {
      if (String.IsNullOrEmpty(name))
        return false; // 25.07.2019

      XmlNode node = RootNode.SelectSingleNode(name);
      return node != null;
    }

    /// <summary>
    /// Копирование части в другую часть.
    /// Все существующие данные в записываемой части удаляются
    /// Метод может быть применен как к отдельной части, так и к секции в целом
    /// </summary>
    /// <param name="dest">Записываемая часть</param>
    public void CopyTo(XmlCfgPart dest)
    {
#if DEBUG
      if (dest == null)
        throw new ArgumentNullException("dest");
#endif

      dest.PartAsXmlText = PartAsXmlText;
    }

    /// <summary>
    /// Копирование одного значения части или дочерней секции (рекурсивное) в другую часть
    /// Если в Dest существует поле или дочерняя часть с именем Name, то она удаляется
    /// Если исходная часть не содержит поля или дочерней части, то просто
    /// выполняется удаление
    /// </summary>
    /// <param name="dest">Записываемая часть или секция</param>
    /// <param name="name">Имя копируемого узла. Не может быть пустой строкой</param>
    public void CopyTo(XmlCfgPart dest, string name)
    {
#if DEBUG
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
#endif

      dest.Remove(name);
      XmlNode SrcNode = RootNode.SelectSingleNode(name);
      if (SrcNode != null)
      {
        XmlNode resNode = dest.Document.CreateElement(name);
        resNode.InnerText = SrcNode.InnerText;
        dest.RootNode.AppendChild(resNode);
        dest.PerformChange();
      }
    }

    /*
    public override string ToString()
    {
      return RootNode.ParentNode;
    }*/

    #endregion

    #region Преобразование в XML

    /// <summary>
    /// Преобразование всего XML-документа в строку или из строки
    /// </summary>
    public string PartAsXmlText
    {
      get
      {
        return RootNode.InnerXml;
      }
      set
      {
        Clear();
        if (String.IsNullOrEmpty(value))
          return;
        RootNode.InnerXml = value;
        PerformChange();
      }
    }

    /// <summary>
    /// Возвращает PartAsXmlText 
    /// </summary>
    /// <returns>Текстовое представление секции конфигурации</returns>
    public override string GetXmlText()
    {
      return PartAsXmlText;
    }

    #endregion

    #region Вычисление контрольной суммы

    /// <summary>
    /// Вычисляет контрольную сумму MD5 для заданной секции.
    /// Использует свойство XmlElement.OuterXml для получения текста секции (без форматирования),
    /// преобразует текст в массив байт в кодировке UTF-16, затем вычисляет MD5.
    /// </summary>
    /// <returns>MD5</returns>
    public override string MD5Sum()
    {
      byte[] bytes = Encoding.Unicode.GetBytes(RootNode.OuterXml);
      return DataTools.MD5Sum(bytes);
    }

    #endregion

    #region Отслеживание изменений

    /// <summary>
    /// Событие вызывается, если изменения вносятся в текущую секцию или в любую из дочерних секций
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// Вызывает событие OnChanged(), и, если есть родительская секция, вызывает Parent.OnChanged().
    /// </summary>
    protected virtual void OnChanged()
    {
      if (Changed != null)
        Changed(this, EventArgs.Empty);

      if (_Parent != null)
        _Parent.OnChanged();
    }

    /// <summary>
    /// Генерация события Changed
    /// </summary>
    public void PerformChange()
    {
      OnChanged();
    }

    #endregion
  }

  /// <summary>
  /// Хранение настроек в XML-файле
  /// </summary>
  public class XmlCfgFile : XmlCfgPart
  {
    #region Конструктор

    /// <summary>
    /// Конструктор, использующий конвертер по умолчанию
    /// </summary>
    /// <param name="filePath">Путь к XML-файлу</param>
    public XmlCfgFile(AbsPath filePath)
      : this(filePath, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Конструктор с возможность указания конвертера
    /// </summary>
    /// <param name="filePath">Путь к XML-файлу</param>
    /// <param name="converter">Конвертер, используемый для преобразования значений</param>
    public XmlCfgFile(AbsPath filePath, CfgConverter converter)
      : base(converter)
    {
      if (filePath.IsEmpty)
        throw new ArgumentNullException("filePath");
      if (converter == null)
        throw new ArgumentNullException("converter");

      _FilePath = filePath;
      _Encoding = DefaultEncoding;

      if (File.Exists(filePath.Path))
      {
        Document.Load(filePath.Path);
        base.RootNode = Document.DocumentElement;
        if (base.RootNode == null)
        {
          base.RootNode = Document.CreateElement("Config");
          Document.AppendChild(base.RootNode);
        }
        if (!DataTools.GetXmlEncoding(Document, out _Encoding))
          _Encoding = DefaultEncoding;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к XML-файлу. Задается в конструкторе.
    /// </summary>
    public AbsPath FilePath { get { return _FilePath; } }
    private readonly AbsPath _FilePath;

    /// <summary>
    /// Возвращает FileName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return FilePath.Path;
    }

    #endregion

    #region Кодировка XML-файла

    /// <summary>
    /// Кодировка XML-файла, используемая по умолчанию.
    /// Свойство используется только в конструкторе класса 
    /// Если свойство не установлено в явном виде, возвращает utf-8.
    /// </summary>
    public static Encoding DefaultEncoding
    {
      get { return _DefaultEncoding; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _DefaultEncoding = value;
      }
    }
    private static Encoding _DefaultEncoding = Encoding.UTF8;

    /// <summary>
    /// Текущая кодировка XML-файла.
    /// Она будет использована при вызове метода Save().
    /// В конструкторе свойство устанавливается, исходя из текущей кодировки файла.
    /// Если файл отсутствует (новый файл), или кодировку определить не удалось, свойство устанавливается равным DefaultEncoding.
    /// </summary>
    public Encoding Encoding
    {
      get { return _Encoding; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Encoding = value;
      }
    }
    private Encoding _Encoding;

    #endregion

    #region Методы

    /// <summary>
    /// Записывает XML-файл в кодировке, задаваемой свойством Enciding
    /// </summary>
    public void Save()
    {
      if (Document.FirstChild is XmlDeclaration)
        Document.RemoveChild(Document.FirstChild);

      XmlDeclaration xmldecl = Document.CreateXmlDeclaration("1.0", Encoding.WebName, null);
      Document.InsertBefore(xmldecl, Document.DocumentElement);

      FileTools.WriteXmlDocument(FilePath, Document);
    }

    #endregion

    #region Преобразование в XML

    /// <summary>
    /// Преобразование всего XML-документа в строку или из строки
    /// Корневым узлом является "Config"
    /// </summary>
    public string AsXmlText
    {
      get
      {
        if (IsEmpty)
          return String.Empty;
        StringWriter wr1 = new StringWriter();
        XmlTextWriter wr2 = new XmlTextWriter(wr1);
        Document.WriteTo(wr2);
        return wr1.ToString();
      }
      set
      {
        if (String.IsNullOrEmpty(value))
        {
          Clear();
          return;
        }
        StringReader rd1 = new StringReader(value);
        XmlTextReader rd2 = new XmlTextReader(rd1);
        Document.Load(rd2);
        XmlNode node2 = Document.SelectSingleNode("Config");
        if (node2 != null)
          base.RootNode = node2;
      }
    }

    /// <summary>
    /// Возвращает AxXmlText
    /// </summary>
    /// <returns></returns>
    public override string GetXmlText()
    {
      return AsXmlText;
    }

    #endregion
  }

  #endregion

  #region Хранение настроек в Реестре Windows

  #region Использование стандартного объекта RegistryKey

  /// <summary>
  /// Секция конфигурации, хранящаяся в ветви реестра.
  /// Внешний объект RegistryTree используется для доступа к данным реестра.
  /// В пользовательском коде используйте класс RegistryCfg для доступа к реестру.
  /// </summary>
  public class RegistryCfgPart : CfgPart
  {
    #region Конструктор

    /// <summary>
    /// Создает секцию, используя конвертер по умолчанию CfgConverter.Default.
    /// </summary>
    /// <param name="tree">Объект для доступа к реестру</param>
    /// <param name="keyName">Путь к ветви реестра</param>
    public RegistryCfgPart(RegistryTree tree, string keyName)
      : this(tree, keyName, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Создает секцию, используя заданный конвертер.
    /// </summary>
    /// <param name="tree">Объект для доступа к реестру</param>
    /// <param name="keyName">Путь к ветви реестра</param>
    /// <param name="converter">Конвертер для преобразование чисел и дат в строки, хранящиеся в реестре</param>
    public RegistryCfgPart(RegistryTree tree, string keyName, CfgConverter converter)
    {
      if (tree == null)
        throw new ArgumentNullException("tree");
      tree.CheckNotDisposed();
      if (String.IsNullOrEmpty(keyName))
        throw new ArgumentNullException("keyName");
      if (converter == null)
        throw new ArgumentNullException("converter");

      _Tree = tree;
      _KeyName = keyName;
      _Converter = converter;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Хранилище открытых узлов реестра
    /// Задается в конструкторе.
    /// </summary>
    public RegistryTree Tree { get { return _Tree; } }
    private readonly RegistryTree _Tree;

    /// <summary>
    /// Путь к ветви реестра.
    /// Задается в конструкторе.
    /// </summary>
    public string KeyName { get { return _KeyName; } }
    private readonly string _KeyName;

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получить строковое значение.
    /// Если в реестре нет ветви KeyName, возвращается пустая строка.
    /// Если в ветви нет значения с именем <paramref name="name"/>, также возвращается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Значение</returns>
    protected override string DoGetString(string name)
    {
      if (Tree[KeyName] == null)
        return String.Empty;
      return DataTools.GetString(Tree[KeyName].GetValue(name, String.Empty));
    }

    /// <summary>
    /// Записать строкое значение.
    /// Если свойство RegistryTree.IsReadOnly=true, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если задано true и <paramref name="value"/> - пустая строка, то значение удаляется из реестра</param>
    protected override void DoSetString(string name, string value, bool removeEmpty)
    {
      if (removeEmpty && String.IsNullOrEmpty(value))
      {
        Remove(name);
        return;
      }

      Tree.CheckNotReadOnly();
      Tree[KeyName].SetValue(name, value);
    }

    #endregion

    #region Другие методы и свойства

    /// <summary>
    /// Возвращает KeyName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return KeyName;
    }

    /// <summary>
    /// Конвертер для преобразования чисел и дат.
    /// Задается в конструкторе.
    /// </summary>
    public override CfgConverter Converter { get { return _Converter; } }
    private readonly CfgConverter _Converter;

    /// <summary>
    /// Удаляет из реестра значение с заданным именем или вложенную ветвь.
    /// Если свойство RegistryTree.IsReadOnly=true, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя значения или вложенной ветви. Не может быть пустой строкой</param>
    protected override void DoRemove(string name)
    {
      Tree.CheckNotReadOnly();

      Tree[KeyName].DeleteValue(name, false);
      if (Tree.Exists(name))
      {
        Tree[KeyName].DeleteSubKeyTree(name);
        Tree.Close(); // иначе будут недействительные узлы
      }
    }

    /// <summary>
    /// Получить дочернюю секцию с заданным именем
    /// </summary>
    /// <param name="name">Имя дочерней ветви реестра. Не может быть пустой строкой</param>
    /// <param name="create">Надо ли создать ветвь, если ее нет</param>
    /// <returns>Дочерняя секция или null, если секция не существует и не создана</returns>
    protected override CfgPart DoGetChild(string name, bool create)
    {
      //if (name.IndexOf('\\') >= 0)
      //  throw new ArgumentException("Дочерний узел не может содержать символа \'\\\'");
      string childKeyName = KeyName + "\\" + name;

      if (!create)
      {
        if (!Tree.Exists(childKeyName))
          return null;
      }
      return new RegistryCfgPart(Tree, KeyName + "\\" + name, Converter);
    }

    /// <summary>
    /// Возвращает массив имен вложенных ветвей реестра.
    /// </summary>
    /// <returns>Массив имен</returns>
    public override string[] GetChildNames()
    {
      if (Tree.Exists(KeyName))
        return Tree[KeyName].GetSubKeyNames();
      else
        return DataTools.EmptyStrings;
    }

    /// <summary>
    /// Возвращает массив значений в ветви реестра.
    /// Имя ""  не возвращается.
    /// </summary>
    /// <returns>Массив имен</returns>
    public override string[] GetValueNames()
    {
      if (Tree.Exists(KeyName))
        return Tree[KeyName].GetValueNames();
      else
        return DataTools.EmptyStrings;
    }

    /// <summary>
    /// Очистить все значения ветви реестра и вложенные секции.
    /// Следует применять с осторожностью.
    /// </summary>
    public override void Clear()
    {
      if (!Tree.Exists(KeyName))
        return;

      // Сначала удаляем все значения
      string[] valueNames = Tree[KeyName].GetValueNames();
      for (int i = 0; i < valueNames.Length; i++)
        Tree[KeyName].DeleteValue(valueNames[i]);

      // Удаляем дочерние узлы
      string[] subNames = Tree[KeyName].GetSubKeyNames();
      if (subNames.Length > 0)
        Tree.Close(); // закрываем все узлы, т.к. удалять открытые узлы нельзя
      for (int i = 0; i < subNames.Length; i++)
        Tree[KeyName].DeleteSubKeyTree(subNames[i]);
    }

    /// <summary>
    /// Возвращает true, если в ветви реестра нет ни одного значения и вложенных секций.
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        if (!Tree.Exists(KeyName))
          return true;

        return Tree[KeyName].ValueCount == 0 && Tree[KeyName].SubKeyCount == 0;
      }
    }

    /// <summary>
    /// Возвращает true, если есть дочерняя ветвь реестра с указанным именем.
    /// </summary>
    /// <param name="name">Имя дочерней секции. </param>
    /// <returns>Наличие дочерней секции</returns>
    protected override bool DoHasChild(string name)
    {
      //if (name.IndexOf('\\') >= 0)
      //  throw new ArgumentException("Дочерний узел не может содержать символа \'\\\'");
      string childKeyName = KeyName + "\\" + name;

      return Tree.Exists(childKeyName);
    }

    /// <summary>
    /// Возвращает true, если в ветви реестра есть значение с указанным именем.
    /// </summary>
    /// <param name="name">Имя параметра</param>
    /// <returns>Наличие записанного значения</returns>
    protected override bool DoHasValue(string name)
    {
      if (!Tree.Exists(KeyName))
        return false;

      return Tree[KeyName].GetValue(name) != null;
    }

    #endregion
  }

  /// <summary>
  /// Объединение объектов RegistryTree и RegistryCfgPart.
  /// Обычно этот класс используется в прикладном коде.
  /// Обычно следует создавать объект для корневой ветви реестра, в которой хранится конфигурация приложения,
  /// например, "HKEY_CURRENT_USER\МояКомпания\МоеПриложение", а для обращения к вложенным ветвям использовать GetChild().
  /// </summary>
  public class RegistryCfg : RegistryCfgPart, IDisposable
  {
    #region Конструкторы и Dispose

    /// <summary>
    /// Начать работу с реестром в режиме чтения и записи, используя конвертер по умолчанию CfgConverter.Default
    /// </summary>
    /// <param name="keyName">Путь к корневой ветви реестра</param>
    public RegistryCfg(string keyName)
      : this(keyName, false, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Начать работу с реестром, используя конвертер по умолчанию CfgConverter.Default
    /// </summary>
    /// <param name="keyName">Путь к корневой ветви реестра</param>
    /// <param name="isReadOnly">true - режим только чтения</param>
    public RegistryCfg(string keyName, bool isReadOnly)
      : this(keyName, isReadOnly, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Начать работу с реестром, используя заданный конвертер
    /// </summary>
    /// <param name="keyName">Путь к корневой ветви реестра</param>
    /// <param name="isReadOnly">true - режим только чтения</param>
    /// <param name="converter">Конвертер для чисел и дат</param>
    public RegistryCfg(string keyName, bool isReadOnly, CfgConverter converter)
      : base(new RegistryTree(isReadOnly), keyName, converter)
    {
#if DEBUG
      DisposableObject.RegisterDisposableObject(this);
#endif
    }

    /// <summary>
    /// Деструктор
    /// </summary>
    ~RegistryCfg()
    {
      if (!IsDisposed)
        Dispose(false);
    }

    /// <summary>
    /// Этот метод должен быть вызван по окончании работы с реестром.
    /// Закрывает открытые ключи реестра.
    /// </summary>
    public void Dispose()
    {
      if (IsDisposed)
        return;
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Вызывается для закрытия ключей реестра
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose()</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
#if DEBUG
        DisposableObject.UnregisterDisposableObject(this, disposing);
#endif
        _IsDisposed = true;

        if (disposing)
          Tree.Dispose();
      }
    }

    /// <summary>
    /// Возвращает true, если был вызван метод Dispose()
    /// </summary>
    public bool IsDisposed { get { return _IsDisposed; } }
    private bool _IsDisposed;

    #endregion
  }

  #endregion

  #region Использование объекта RegistryKey2

  /// <summary>
  /// Секция конфигурации, хранящаяся в ветви реестра.
  /// Внешний объект RegistryTree2 используется для доступа к данным реестра.
  /// В пользовательском коде используйте класс RegistryCfg2 для доступа к реестру.
  /// В отличие от RegistryCfgPart, позволяет управлять виртуализацией реестра в Windows-64 bit.
  /// Режим виртуализации задается в RegistryTree2.
  /// </summary>
  public class RegistryCfgPart2 : CfgPart
  {
    #region Конструктор

    /// <summary>
    /// Создает секцию, используя конвертер по умолчанию CfgConverter.Default.
    /// </summary>
    /// <param name="tree">Объект для доступа к реестру</param>
    /// <param name="keyName">Путь к ветви реестра</param>
    public RegistryCfgPart2(RegistryTree2 tree, string keyName)
      : this(tree, keyName, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Создает секцию, используя заданный конвертер.
    /// </summary>
    /// <param name="tree">Объект для доступа к реестру</param>
    /// <param name="keyName">Путь к ветви реестра</param>
    /// <param name="converter">Конвертер для преобразование чисел и дат в строки, хранящиеся в реестре</param>
    public RegistryCfgPart2(RegistryTree2 tree, string keyName, CfgConverter converter)
    {
      if (tree == null)
        throw new ArgumentNullException("tree");
      tree.CheckNotDisposed();
      if (String.IsNullOrEmpty(keyName))
        throw new ArgumentNullException("keyName");
      if (converter == null)
        throw new ArgumentNullException("converter");

      _Tree = tree;
      _KeyName = keyName;
      _Converter = converter;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Хранилище открытых узлов реестра.
    /// Задается в конструкторе.
    /// Этот объект определяет также режим виртуализации в Windows-64 bit
    /// </summary>
    public RegistryTree2 Tree { get { return _Tree; } }
    private readonly RegistryTree2 _Tree;

    /// <summary>
    /// Путь к ветви реестра.
    /// Задается в конструкторе.
    /// </summary>
    public string KeyName { get { return _KeyName; } }
    private readonly string _KeyName;

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получить строковое значение.
    /// Если в реестре нет ветви KeyName, возвращается пустая строка.
    /// Если в ветви нет значения с именем <paramref name="name"/>, также возвращается пустая строка.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Значение</returns>
    protected override string DoGetString(string name)
    {
      if (Tree[KeyName] == null)
        return String.Empty;
      return DataTools.GetString(Tree[KeyName].GetValue(name, String.Empty));
    }

    /// <summary>
    /// Записать строкое значение.
    /// Если свойство RegistryTree.IsReadOnly=true, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если задано true и <paramref name="value"/> - пустая строка, то значение удаляется из реестра</param>
    protected override void DoSetString(string name, string value, bool removeEmpty)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      if (removeEmpty && String.IsNullOrEmpty(value))
      {
        Remove(name);
        return;
      }

      Tree.CheckNotReadOnly();
      Tree[KeyName].SetValue(name, value);
    }

    #endregion

    #region Другие методы и свойства

    /// <summary>
    /// Возвращает KeyName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return KeyName;
    }

    /// <summary>
    /// Конвертер для преобразования чисел и дат.
    /// Задается в конструкторе.
    /// </summary>
    public override CfgConverter Converter { get { return _Converter; } }
    private readonly CfgConverter _Converter;

    /// <summary>
    /// Удаляет из реестра значение с заданным именем или вложенную ветвь.
    /// Если свойство RegistryTree.IsReadOnly=true, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя значения или вложенной ветви. Не может быть пустой строкой</param>
    protected override void DoRemove(string name)
    {
      Tree.CheckNotReadOnly();

      Tree[KeyName].DeleteValue(name, false);
      if (Tree.Exists(name))
      {
        Tree[KeyName].DeleteSubKeyTree(name);
        Tree.Close(); // иначе будут недействительные узлы
      }
    }

    /// <summary>
    /// Получить дочернюю секцию с заданным именем
    /// </summary>
    /// <param name="name">Имя дочерней ветви реестра. Не может быть пустой строкой</param>
    /// <param name="create">Надо ли создать ветвь, если ее нет</param>
    /// <returns>Дочерняя секция или null, если секция не существует и не создана</returns>
    protected override CfgPart DoGetChild(string name, bool create)
    {
      //if (name.IndexOf('\\') >= 0)
      //  throw new ArgumentException("Дочерний узел не может содержать символа \'\\\'");
      string childKeyName = KeyName + "\\" + name;

      if (!create)
      {
        if (!Tree.Exists(childKeyName))
          return null;
      }
      return new RegistryCfgPart2(Tree, KeyName + "\\" + name, Converter);
    }

    /// <summary>
    /// Возвращает массив имен вложенных ветвей реестра.
    /// </summary>
    /// <returns>Массив имен</returns>
    public override string[] GetChildNames()
    {
      if (Tree.Exists(KeyName))
        return Tree[KeyName].GetSubKeyNames();
      else
        return DataTools.EmptyStrings;
    }

    /// <summary>
    /// Возвращает массив значений в ветви реестра.
    /// Имя ""  не возвращается.
    /// </summary>
    /// <returns>Массив имен</returns>
    public override string[] GetValueNames()
    {
      if (Tree.Exists(KeyName))
        return Tree[KeyName].GetValueNames();
      else
        return DataTools.EmptyStrings;
    }

    /// <summary>
    /// Очистить все значения ветви реестра и вложенные секции.
    /// Следует применять с осторожностью.
    /// </summary>
    public override void Clear()
    {
      if (!Tree.Exists(KeyName))
        return;

      // Сначала удаляем все значения
      string[] valueNames = Tree[KeyName].GetValueNames();
      for (int i = 0; i < valueNames.Length; i++)
        Tree[KeyName].DeleteValue(valueNames[i]);

      // Удаляем дочерние узлы
      string[] subNames = Tree[KeyName].GetSubKeyNames();
      if (subNames.Length > 0)
        Tree.Close(); // закрываем все узлы, т.к. удалять открытые узлы нельзя
      for (int i = 0; i < subNames.Length; i++)
        Tree[KeyName].DeleteSubKeyTree(subNames[i]);
    }

    /// <summary>
    /// Возвращает true, если в ветви реестра нет ни одного значения и вложенных секций.
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        if (!Tree.Exists(KeyName))
          return true;

        return Tree[KeyName].ValueCount == 0 && Tree[KeyName].SubKeyCount == 0;
      }
    }

    /// <summary>
    /// Возвращает true, если есть дочерняя ветвь реестра с указанным именем.
    /// </summary>
    /// <param name="name">Имя дочерней секции. </param>
    /// <returns>Наличие дочерней секции</returns>
    protected override bool DoHasChild(string name)
    {
      //if (name.IndexOf('\\') >= 0)
      //  throw new ArgumentException("Дочерний узел не может содержать символа \'\\\'");
      string childKeyName = KeyName + "\\" + name;

      return Tree.Exists(childKeyName);
    }

    /// <summary>
    /// Возвращает true, если в ветви реестра есть значение с указанным именем.
    /// </summary>
    /// <param name="name">Имя параметра. Если задана пустая строка, метод возвращает false</param>
    /// <returns>Наличие записанного значения</returns>
    protected override bool DoHasValue(string name)
    {
      if (!Tree.Exists(KeyName))
        return false;

      return Tree[KeyName].GetValue(name) != null;
    }

    #endregion
  }

  /// <summary>
  /// Объединение объектов RegistryTree2 и RegistryCfgPart2.
  /// Обычно этот класс используется в прикладном коде.
  /// Обычно следует создавать объект для корневой ветви реестра, в которой хранится конфигурация приложения,
  /// например, "HKEY_CURRENT_USER\МояКомпания\МоеПриложение", а для обращения к вложенным ветвям использовать GetChild().
  /// В отличие от RegistryCfg, позволяет управлять виртуализацией реестра в Windows-64 bit.
  /// Режим виртуализации задается в RegistryTree2.
  /// </summary>
  public class RegistryCfg2 : RegistryCfgPart2, IDisposable
  {
    #region Конструкторы и Dispose

    /// <summary>
    /// Начать работу с реестром в режиме чтения и записи, используя конвертер по умолчанию CfgConverter.Default
    /// Будет использован режим виртуализации View=Registry64 или Registry32 в зависимости от
    /// разрядности операционной системы, а не от разрядности приложения.
    /// </summary>
    /// <param name="keyName">Путь к корневой ветви реестра</param>
    public RegistryCfg2(string keyName)
      : this(keyName, false, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Начать работу с реестром, используя конвертер по умолчанию CfgConverter.Default
    /// Будет использован режим виртуализации View=Registry64 или Registry32 в зависимости от
    /// разрядности операционной системы, а не от разрядности приложения.
    /// </summary>
    /// <param name="keyName">Путь к корневой ветви реестра</param>
    /// <param name="isReadOnly">true - режим только чтения</param>
    public RegistryCfg2(string keyName, bool isReadOnly)
      : this(keyName, isReadOnly, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Начать работу с реестром, используя заданный конвертер
    /// Будет использован режим виртуализации View=Registry64 или Registry32 в зависимости от
    /// разрядности операционной системы, а не от разрядности приложения.
    /// </summary>
    /// <param name="keyName">Путь к корневой ветви реестра</param>
    /// <param name="isReadOnly">true - режим только чтения</param>
    /// <param name="converter">Конвертер для чисел и дат</param>
    public RegistryCfg2(string keyName, bool isReadOnly, CfgConverter converter)
      : base(new RegistryTree2(isReadOnly), keyName, converter)
    {
    }

    /// <summary>
    /// Начать работу с реестром, используя конвертер по умолчанию CfgConverter.Default.
    /// Эта версия позволяет задать режим виртуализации.
    /// </summary>
    /// <param name="keyName">Путь к корневой ветви реестра</param>
    /// <param name="isReadOnly">true - режим только чтения</param>
    /// <param name="view">Способ виртуализации</param>
    public RegistryCfg2(string keyName, bool isReadOnly, RegistryView2 view)
      : this(keyName, isReadOnly, view, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Начать работу с реестром, используя заданный конвертер
    /// Будет использован режим виртуализации View=Registry64 или Registry32 в зависимости от
    /// разрядности операционной системы, а не от разрядности приложения.
    /// Эта версия позволяет задать режим виртуализации.
    /// </summary>
    /// <param name="keyName">Путь к корневой ветви реестра</param>
    /// <param name="isReadOnly">true - режим только чтения</param>
    /// <param name="view">Способ виртуализации</param>
    /// <param name="converter">Конвертер для чисел и дат</param>
    public RegistryCfg2(string keyName, bool isReadOnly, RegistryView2 view, CfgConverter converter)
      : base(new RegistryTree2(isReadOnly, view), keyName, converter)
    {
#if DEBUG
      DisposableObject.RegisterDisposableObject(this);
#endif
    }

    /// <summary>
    /// Деструктор
    /// </summary>
    ~RegistryCfg2()
    {
      if (!IsDisposed)
        Dispose(false);
    }

    /// <summary>
    /// Этот метод должен быть вызван по окончании работы с реестром.
    /// Закрывает открытые ключи реестра.
    /// </summary>
    public void Dispose()
    {
      if (IsDisposed)
        return;
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Вызывается для закрытия ключей реестра
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose()</param>
    protected virtual void Dispose(bool disposing)
    {

      if (!IsDisposed)
      {
#if DEBUG
        DisposableObject.UnregisterDisposableObject(this, disposing);
#endif
        _IsDisposed = true;

        if (disposing)
          Tree.Dispose();
      }
    }

    /// <summary>
    /// Возвращает true, если был вызван метод Dispose()
    /// </summary>
    public bool IsDisposed { get { return _IsDisposed; } }
    private bool _IsDisposed;

    #endregion
  }

  #endregion

  #endregion

  #region Временное хранилище в памяти

  /// <summary>
  /// Временное хранилише настроек в памяти без возможности сохранения
  /// </summary>
  public class TempCfg : XmlCfgPart
  {
    #region Конструктор

    /// <summary>
    /// Создает пустое хранилище в памяти
    /// </summary>
    public TempCfg()
      : base(CfgConverter.Default)
    {
    }

    #endregion

    #region Преобразование в XML

    /// <summary>
    /// Преобразование всего XML-документа в строку или из строки
    /// Корневым узлом является "Config"
    /// </summary>
    public string AsXmlText
    {
      get
      {
        if (IsEmpty)
          return String.Empty;
        StringWriter wr1 = new StringWriter();
        XmlTextWriter wr2 = new XmlTextWriter(wr1);
        Document.WriteTo(wr2);
        return wr1.ToString();
      }
      set
      {
        if (String.IsNullOrEmpty(value))
        {
          Clear();
          return;
        }
        StringReader rd1 = new StringReader(value);
        XmlTextReader rd2 = new XmlTextReader(rd1);
        Document.Load(rd2);
        XmlNode node2 = Document.SelectSingleNode("Config");
        if (node2 != null)
          base.RootNode = node2;
      }
    }

    #endregion
  }

  #endregion

  #region Хранение настроек в INI-файле

  /// <summary>
  /// Секция конфигурации в INI-файле.
  /// Использование ini-файлов для хранения настроек приложения не рекомендуется.
  /// В отличие от xml-файлов и реестра, в ini-файле нет вложенных секций.
  /// Для их эмуляции используются имена секций вида "Родитель\Дочерняя\..\Дочерняя".
  /// </summary>
  public class IniCfgPart : CfgPart
  {
    #region Конструктор

    /// <summary>
    /// Создать объект доступа к секции, испрользуя конвертер по умолчанию CfgConverter.Default.
    /// </summary>
    /// <param name="file">Объект доступа к ini-файлу</param>
    /// <param name="sectionName">Имя секции конфигурации</param>
    public IniCfgPart(IIniFile file, string sectionName)
      : this(file, sectionName, CfgConverter.Default)
    {
    }

    /// <summary>
    /// Создать объект доступа к секции, испрользуя заданный конвертер.
    /// </summary>
    /// <param name="file">Объект доступа к ini-файлу</param>
    /// <param name="sectionName">Имя секции конфигурации</param>
    /// <param name="converter">Конвертер чисел и дат</param>
    public IniCfgPart(IIniFile file, string sectionName, CfgConverter converter)
    {
      if (file == null)
        throw new ArgumentNullException("file");
      if (String.IsNullOrEmpty(sectionName))
        throw new ArgumentNullException("sectionName");
      if (converter == null)
        throw new ArgumentNullException("converter");

      _File = file;
      _SectionName = sectionName;
      _Converter = converter;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект доступа к ini-файлу.
    /// Задается в конструкторе.
    /// </summary>
    public IIniFile File { get { return _File; } }
    private readonly IIniFile _File;

    /// <summary>
    /// Имя секции конфигурации.
    /// Задается в конструкторе.
    /// </summary>
    public string SectionName { get { return _SectionName; } }
    private readonly string _SectionName;

    /// <summary>
    /// Конвертер для преобразования чисел и дат.
    /// Задается в конструкторе.
    /// </summary>
    public override CfgConverter Converter { get { return _Converter; } }
    private readonly CfgConverter _Converter;

    #endregion

    #region Доступ к данным

    /// <summary>
    /// Прочитать строку
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <returns>Значение</returns>
    protected override string DoGetString(string name)
    {
      return File.GetString(SectionName, name, String.Empty);
    }

    /// <summary>
    /// Записать строку
    /// </summary>
    /// <param name="name">Имя параметра. Не может быть пустой строкой</param>
    /// <param name="value">Значение</param>
    /// <param name="removeEmpty">Если true и <paramref name="value"/> задает пустую строку, строка удаляется из ini-файла, если она там была</param>
    protected override void DoSetString(string name, string value, bool removeEmpty)
    {
      if (removeEmpty && String.IsNullOrEmpty(value))
      {
        //Remove(Name);
        File.DeleteKey(SectionName, name); // 09.10.2018
        return;
      }

      File[SectionName, name] = value;
    }

    /// <summary>
    /// Удаляет из текущей секции строку с именем <paramref name="name"/>, а из файла - секцию "SectionName\Name"
    /// </summary>
    /// <param name="name">Имя строки или дочерней секции. Не может быть пустой строкой</param>
    protected override void DoRemove(string name)
    {
      File.DeleteKey(SectionName, name);
      File.DeleteSection(SectionName + '\\' + name); // 09.10.2018
    }

    #endregion

    /// <summary>
    /// INI-файлы не поддерживают вложенные секции конфигурации, поэтому вложенность эмулируется.
    /// Возвращает "дочернюю" секцию конфигурации с именем "SectionName\Name".
    /// Этот метод ничего не делает, так как секция создается в момент записи первого значения. 
    /// </summary>
    /// <param name="name">Имя дочерней секции. Не может быть пустой строкой</param>
    /// <param name="create">Надо ли создавать секцию, если ее нет. Если false, а секции нет,
    /// возвращается false</param>
    /// <returns>Объект для доступа к дочерней секции</returns>
    protected override CfgPart DoGetChild(string name, bool create)
    {
      string subName = SectionName + "\\" + name;

      if (!create)
      {
        // Проверяем наличие секции
        string[] allNames = File.GetSectionNames();
        if (Array.LastIndexOf<string>(allNames, name) < 0)
          return null;
      }

      return new IniCfgPart(File, subName);
    }

    /// <summary>
    /// Возвращает имена "дочерних" секций.
    /// Так как ini-файл не поддерживает вложение секций, используется эмуляция.
    /// Возвращаются имена, начинающиеся с "SectionName\"
    /// </summary>
    /// <returns>Список имен</returns>
    public override string[] GetChildNames()
    {
      string[] allNames = File.GetSectionNames();
      List<string> subNames = null;
      string Name2 = SectionName + "\\";
      for (int i = 0; i < allNames.Length; i++)
      {
        if (allNames[i].StartsWith(Name2))
        {
          if (subNames == null)
            subNames = new List<string>();
          subNames.Add(allNames[i].Substring(Name2.Length));
        }
      }

      if (subNames == null)
        return DataTools.EmptyStrings;
      else
      {
        subNames.Sort();
        return subNames.ToArray();
      }
    }

    /// <summary>
    /// Получить список имен строк для секции
    /// </summary>
    /// <returns>Список имен</returns>
    public override string[] GetValueNames()
    {
      return File.GetKeyNames(SectionName);
    }

    /// <summary>
    /// Возвращает true, если в секции нет ни одной строки и нет ни одной "дочерней" секции
    /// </summary>
    public override bool IsEmpty
    {
      get { return GetValueNames().Length == 0 && GetChildNames().Length == 0; }
    }

    /// <summary>
    /// Удаляет текущую секцию и "дочерние" секции
    /// </summary>
    public override void Clear()
    {
      // Очищаем значения
      File.DeleteSection(SectionName);
      // Удаляем дочерние значения
      string[] Children = GetChildNames();
      for (int i = 0; i < Children.Length; i++)
        File.DeleteSection(SectionName + "\\" + Children[i]);
    }

    /// <summary>
    /// Возвращает true, если есть "дочерняя" секция с заданным именем.
    /// Дочернии секции эмулируются.
    /// Проверяется наличие в ini-файле секции с именем "SectionName\Name".
    /// </summary>
    /// <param name="name">Имя дочерней секции</param>
    /// <returns>Наличие дочерней секции</returns>
    protected override bool DoHasChild(string name)
    {
      //string SubName = SectionName + "\\" + name;

      // Проверяем наличие секции
      string[] allNames = File.GetSectionNames();
      return Array.LastIndexOf<string>(allNames, name) >= 0;
    }

    /// <summary>
    /// Возвращает true, если в текущей секции есть строка с заданным именем.
    /// </summary>
    /// <param name="name">Имя параметра.</param>
    /// <returns>Наличие записанного значения</returns>
    protected override bool DoHasValue(string name)
    {
      string[] allNames = File.GetKeyNames(SectionName);
      return Array.IndexOf<string>(allNames, name) >= 0;
    }
  }

  #endregion
}
