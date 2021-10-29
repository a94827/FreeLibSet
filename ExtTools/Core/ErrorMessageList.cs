using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace FreeLibSet.Core
{
  #region Перечисление ErrorMessageKind

  /// <summary>
  /// Тип сообщения об ошибке
  /// </summary>
  public enum ErrorMessageKind
  {
    /// <summary>
    /// Ошибка
    /// </summary>
    Error,

    /// <summary>
    /// Предупреждение
    /// </summary>
    Warning,

    /// <summary>
    /// Информационное сообщение
    /// </summary>
    Info
  }

  #endregion

  /// <summary>
  /// Одно сообщение об ошибке, хранящееся в ErrorMessageList
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct ErrorMessageItem
  {
    #region Конструкторы

    /// <summary>
    /// Создает новое сообщение
    /// </summary>
    /// <param name="kind">Серьезность</param>
    /// <param name="text">Текст сообщения</param>
    public ErrorMessageItem(ErrorMessageKind kind, string text)
      : this(kind, text, null, null)
    {
    }

    /// <summary>
    /// Создает новое сообщение с возможностью указать код
    /// </summary>
    /// <param name="kind">Серьезность</param>
    /// <param name="text">Текст сообщения</param>
    /// <param name="code">Код ошибки</param>
    public ErrorMessageItem(ErrorMessageKind kind, string text, string code)
      : this(kind, text, code, null)
    {
    }

    /// <summary>
    /// Создает новое сообщение с возможностью указать код и присоединить пользовательские данные
    /// </summary>
    /// <param name="kind">Серьезность</param>
    /// <param name="text">Текст сообщения</param>
    /// <param name="code">Код ошибки</param>
    /// <param name="tag">Пользовательские данные</param>
    public ErrorMessageItem(ErrorMessageKind kind, string text, string code, object tag)
    {
      if (String.IsNullOrEmpty(text))
        throw new ArgumentNullException("text");

      _Kind = kind;
      _Text = text;
      _Code = code;
      _Tag = tag;
    }

    /// <summary>
    /// Создание копии сообщения с другим уровнем серьезности.
    /// Сообщение не добавляется в список
    /// </summary>
    /// <param name="kind">Серьензноть</param>
    /// <param name="sourceItem">Копируемое сообщение</param>
    public ErrorMessageItem(ErrorMessageKind kind, ErrorMessageItem sourceItem)
    {
      _Kind = kind;
      _Text = sourceItem.Text;
      _Code = sourceItem.Code;
      _Tag = sourceItem.Tag;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Тип сообщения
    /// Свойство устанавливается в конструкторе
    /// </summary>
    public ErrorMessageKind Kind { get { return _Kind; } }
    private readonly ErrorMessageKind _Kind;

    /// <summary>
    /// Текст сообщения
    /// Свойство устанавливается в конструкторе
    /// </summary>
    public string Text { get { return _Text; } }
    private readonly string _Text;

    /// <summary>
    /// Код ошибки
    /// Свойство задается в конструкторе. Для списка ошибок можно использовать метод ErrorMessageList.SetCode()
    /// При использовании числовых кодов требуется выполнять преобразование
    /// в строку или обратно
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Произвольные пользовательские данные.
    /// Поле может быть использовано для идентификации объекта, в котором
    /// произошла ошибка.
    /// При копировании сообщения об ошибке значение свойства также копируется
    /// Свойство устанавливается в конструкторе или после создания объекта.
    /// Для списка ошибок можно использовать метод ErrorMessageList.SetTag()
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована с помощью конструктора
    /// </summary>
    public bool IsEmpty { get { return _Text == null; } }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Текстовое представление сообщения, включая уровень серьезности и код, если задан
    /// </summary>
    /// <returns>Текст</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      switch (_Kind)
      {
        case ErrorMessageKind.Error:
          sb.Append("Ошибка. ");
          break;
        case ErrorMessageKind.Warning:
          sb.Append("Предупреждение. ");
          break;
        case ErrorMessageKind.Info:
          sb.Append("Инфо. ");
          break;
        default:
          sb.Append("Неизвестно что. ");
          break;
      }
      sb.Append(_Text);
      if (!String.IsNullOrEmpty(_Code))
      {
        sb.Append(" (Код");
        sb.Append(_Code);
        sb.Append(")");
      }
      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Список сообщений об ошибках, предупреждениях и информационных сообщениях
  /// </summary>
  [Serializable]
  public class ErrorMessageList : ListWithReadOnly<ErrorMessageItem>, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создать пустой список сообщений
    /// </summary>
    public ErrorMessageList()
    {
    }

    /// <summary>
    /// Конструктор для списка Empty
    /// </summary>
    /// <param name="isReadOnly"></param>
    private ErrorMessageList(bool isReadOnly)
    {
      if (isReadOnly)
        SetReadOnly();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Общее количество сообщений.
    /// Допускается установка свойства, чтобы уменьшить количество сообщений.
    /// Нельзя увеличивать значение свойства
    /// </summary>
    public new int Count
    {
      get { return base.Count; }
      set
      {
        CheckNotReadOnly();

        if (value < 0 || value > base.Count)
          throw new ArgumentOutOfRangeException("value", value, "Нельзя увеличивать число сообщений в списке");

        if (value == 0)
          Clear();
        else
        {
          while (value > base.Count)
            base.RemoveAt(base.Count - 1);
        }
      }
    }

    /// <summary>
    /// Максимальный уровень серьезности сообщений.
    /// Если список пуст, то возвращается уровень "Info"
    /// </summary>
    public ErrorMessageKind Severity
    {
      get
      {
        bool HasWarning = false;
        for (int i = 0; i < base.Count; i++)
        {
          switch (base[i].Kind)
          {
            case ErrorMessageKind.Error:
              return ErrorMessageKind.Error;
            case ErrorMessageKind.Warning:
              HasWarning = true;
              break;
          }
        }
        if (HasWarning)
          return ErrorMessageKind.Warning;
        else
          return ErrorMessageKind.Info;
      }
    }


    /// <summary>
    /// Максимальный уровень серьезности сообщений.
    /// Если список пуст, то возвращается null
    /// </summary>
    public ErrorMessageKind? NullableSeverity
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return Severity;
      }
    }

    /// <summary>
    /// Получить количество сообщений об ошибках (Kind=ErrorMessageKind.Error)
    /// Если требуется общее количество сообщений всех видов, следует использовать
    /// свойство Count, т.к. для него не требуется перебор записей.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод GetCounts()
    /// </summary>
    public int ErrorCount
    {
      get
      {
        int n = 0;
        for (int i = 0; i < base.Count; i++)
        {
          if (base[i].Kind == ErrorMessageKind.Error)
            n++;
        }
        return n;
      }
    }

    /// <summary>
    /// Получить количество предупреждений (Kind=ErrorMessageKind.Warning)
    /// Если требуется общее количество сообщений всех видов, следует использовать
    /// свойство Count, т.к. для него не требуется перебор записей
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод GetCounts()
    /// </summary>
    public int WarningCount
    {
      get
      {
        int n = 0;
        for (int i = 0; i < base.Count; i++)
        {
          if (base[i].Kind == ErrorMessageKind.Warning)
            n++;
        }
        return n;
      }
    }

    /// <summary>
    /// Получить количество информационных сообщений об ошибках (Kind=ErrorMessageKind.Info)
    /// Если требуется общее количество сообщений всех видов, следует использовать
    /// свойство Count, т.к. для него не требуется перебор записей
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод GetCounts()
    /// </summary>
    public int InfoCount
    {
      get
      {
        int n = 0;
        for (int i = 0; i < base.Count; i++)
        {
          if (base[i].Kind == ErrorMessageKind.Info)
            n++;
        }
        return n;
      }
    }


    /// <summary>
    /// Получить количество сообщений об ошибках (Kind=ErrorMessageKind.Error) для части списка.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод GetCounts()
    /// <param name="startIndex">Начальная позиция в списке, с которой нужно начать подсчет</param>
    /// </summary>
    /// <returns>Количество ошибок от <paramref name="startIndex"/> до конца списка</returns>
    public int GetErrorCount(int startIndex)
    {
      int n = 0;
      for (int i = startIndex; i < base.Count; i++)
      {
        if (base[i].Kind == ErrorMessageKind.Error)
          n++;
      }
      return n;
    }

    /// <summary>
    /// Получить количество предупреждений (Kind=ErrorMessageKind.Warning) для части списка.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод GetCounts()
    /// <param name="startIndex">Начальная позиция в списке, с которой нужно начать подсчет</param>
    /// </summary>
    /// <returns>Количество ошибок от <paramref name="startIndex"/> до конца списка</returns>
    public int GetWarningCount(int startIndex)
    {
      int n = 0;
      for (int i = startIndex; i < base.Count; i++)
      {
        if (base[i].Kind == ErrorMessageKind.Warning)
          n++;
      }
      return n;
    }

    /// <summary>
    /// Получить количество информационных сообщений об ошибках (Kind=ErrorMessageKind.Info) для части списка.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод GetCounts()
    /// <param name="startIndex">Начальная позиция в списке, с которой нужно начать подсчет</param>
    /// </summary>
    /// <returns>Количество ошибок от <paramref name="startIndex"/> до конца списка</returns>
    public int GetInfoCount(int startIndex)
    {
      int n = 0;
      for (int i = startIndex; i < base.Count; i++)
      {
        if (base[i].Kind == ErrorMessageKind.Info)
          n++;
      }
      return n;
    }


    /// <summary>
    /// Возвращает свойства ErrorCount, WarningCount и InfoCount за один проход для части списка
    /// </summary>
    /// <param name="errorCount">Сюда помещается количество сообщений об ошибках</param>
    /// <param name="warningCount">Сюда помещается количество предупреждений</param>
    /// <param name="infoCount">Сюда помещается количество информационных сообщений</param>
    /// <param name="startIndex">Начальная позиция списка, откуда следует начать подсчет</param>
    public void GetCounts(out int errorCount, out int warningCount, out int infoCount, int startIndex)
    {
      errorCount = 0;
      warningCount = 0;
      infoCount = 0;
      for (int i = startIndex; i < base.Count; i++)
      {
        switch (base[i].Kind)
        {
          case ErrorMessageKind.Error: errorCount++; break;
          case ErrorMessageKind.Warning: warningCount++; break;
          case ErrorMessageKind.Info: infoCount++; break;
        }
      }
    }

    /// <summary>
    /// Возвращает свойства ErrorCount, WarningCount и InfoCount за один проход
    /// </summary>
    /// <param name="errorCount">Сюда помещается количество сообщений об ошибках</param>
    /// <param name="warningCount">Сюда помещается количество предупреждений</param>
    /// <param name="infoCount">Сюда помещается количество информационных сообщений</param>
    public void GetCounts(out int errorCount, out int warningCount, out int infoCount)
    {
      GetCounts(out errorCount, out warningCount, out infoCount, 0);
    }

    /// <summary>
    /// Возвращает свойства ErrorCount и WarningCount за один проход
    /// </summary>
    /// <param name="errorCount">Сюда помещается количество сообщений об ошибках</param>
    /// <param name="warningCount">Сюда помещается количество предупреждений</param>
    public void GetCounts(out int errorCount, out int warningCount)
    {
      int InfoCount;
      GetCounts(out errorCount, out warningCount, out InfoCount);
    }

    /// <summary>
    /// Возвращает первый элемент в списке, обладающий максимальным уровнем важности.
    /// То есть, если в списке есть ошибки, то будет возвращен первый элемент с уровнем серьезности Error.
    /// Иначе, если в списке есть предупреждения, то будет возвращен первый элемент с уровнем серьезности Warning.
    /// Иначе будет возвращен первый элемент списка (с уровнем серьезности Info).
    /// Если список пуст, возвращается null
    /// </summary>
    public ErrorMessageItem? FirstSevereItem
    {
      get
      {
        if (Count == 0)
          return null;

        int FirstWarningIndex = -1;

        for (int i = 0; i < Count; i++)
        {
          switch (this[i].Kind)
          {
            case ErrorMessageKind.Error:
              return this[i];
            case ErrorMessageKind.Warning:
              if (FirstWarningIndex < 0)
                FirstWarningIndex = i;
              break;
          }
        }

        if (FirstWarningIndex >= 0)
          return this[FirstWarningIndex];
        else
          return this[0]; // все сообщения имеют уровень Info
      }
    }

    #endregion

    #region ReadOnly

    /// <summary>
    /// Устанавливает свойство IsReadOnly в true, что блокирует дальнейшее добавление сообщений
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region Методы добавления сообщений

    /// <summary>
    /// Добавить сообщение об ошибке
    /// </summary>
    /// <param name="text">Сообщение</param>
    public void AddError(string text)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Error, text));
    }

    /// <summary>
    /// Добавить сообщение об ошибке с возможностью задать код
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код ошибки</param>
    public void AddError(string text, string code)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Error, text, code));
    }

    /// <summary>
    /// Добавить сообщение об ошибке с возможностью задать код
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код ошибки</param>
    public void AddError(string text, int code)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Error, text, code.ToString()));
    }

    /// <summary>
    /// Добавить сообщение об ошибке с возможностью задать код и пользовательские данные
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код ошибки</param>
    /// <param name="tag">Пользовательские данные</param>
    public void AddError(string text, string code, object tag)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Error, text, code, tag));
    }

    /// <summary>
    /// Добавить сообщение об ошибке с возможностью задать код и пользовательские данные
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код ошибки</param>
    /// <param name="tag">Пользовательские данные</param>
    public void AddError(string text, int code, object tag)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Error, text, code.ToString(), tag));
    }

    /// <summary>
    /// Добавить копию сообщение с уровнем серьезности "Ошибка"
    /// </summary>
    /// <param name="sourceItem">Копируемое сообщение</param>
    public void AddError(ErrorMessageItem sourceItem)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Error, sourceItem));
    }

    /// <summary>
    /// Добавить предупреждение
    /// </summary>
    /// <param name="text">Сообщение</param>
    public void AddWarning(string text)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Warning, text));
    }

    /// <summary>
    /// Добавить предупреждение с возможностью задать код
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код предупреждения</param>
    public void AddWarning(string text, string code)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Warning, text, code));
    }

    /// <summary>
    /// Добавить предупреждение с возможностью задать код
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код предупреждения</param>
    public void AddWarning(string text, int code)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Warning, text, code.ToString()));
    }

    /// <summary>
    /// Добавить предупреждение с возможностью задать код и пользовательские данные
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код предупреждения</param>
    /// <param name="tag">Пользовательские данные</param>
    public void AddWarning(string text, string code, object tag)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Warning, text, code, tag));
    }

    /// <summary>
    /// Добавить предупреждение с возможностью задать код и пользовательские данные
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код предупреждения</param>
    /// <param name="tag">Пользовательские данные</param>
    public void AddWarning(string text, int code, object tag)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Warning, text, code.ToString(), tag));
    }

    /// <summary>
    /// Добавить копию сообщение с уровнем серьезности "Предупреждение"
    /// </summary>
    /// <param name="sourceItem">Копируемое сообщение</param>
    public void AddWarning(ErrorMessageItem sourceItem)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Warning, sourceItem));
    }

    /// <summary>
    /// Добавить информационное сообщение
    /// </summary>
    /// <param name="text">Сообщение</param>
    public void AddInfo(string text)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Info, text));
    }

    /// <summary>
    /// Добавить информационное сообщение с возможностью задать код 
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код сообщения</param>
    public void AddInfo(string text, string code)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Info, text, code));
    }

    /// <summary>
    /// Добавить информационное сообщение с возможностью задать код 
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код сообщения</param>
    public void AddInfo(string text, int code)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Info, text, code.ToString()));
    }

    /// <summary>
    /// Добавить информационное сообщение с возможностью задать код и пользовательские данные
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код сообщения</param>
    /// <param name="tag">Пользовательские данные</param>
    public void AddInfo(string text, string code, object tag)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Info, text, code, tag));
    }

    /// <summary>
    /// Добавить информационное сообщение с возможностью задать код и пользовательские данные
    /// </summary>
    /// <param name="text">Сообщение</param>
    /// <param name="code">Код сообщения</param>
    /// <param name="tag">Пользовательские данные</param>
    public void AddInfo(string text, int code, object tag)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Info, text, code.ToString(), tag));
    }

    /// <summary>
    /// Добавить копию сообщение с уровнем серьезности "Информационное сообщение"
    /// </summary>
    /// <param name="sourceItem">Копируемое сообщение</param>
    public void AddInfo(ErrorMessageItem sourceItem)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Info, sourceItem));
    }

    /// <summary>
    /// Добавить копию сообщения (например, из другого списка) с возможностью задать
    /// дополнительный текст перед и после существующего
    /// Дополнительные пробелы не добавляются. Они должны быть предусмотрены в
    /// конце PrefixText и в начале PostfixText для отделения от основного текста
    /// сообщения
    /// </summary>
    /// <param name="sourceItem">Исходное сообщение</param>
    /// <param name="prefixText">Текст перед сообщением</param>
    /// <param name="suffixText">Текст после сообщения</param>
    public void Add(ErrorMessageItem sourceItem, string prefixText, string suffixText)
    {
#if DEBUG
      if (String.IsNullOrEmpty(sourceItem.Text))
        throw new ArgumentException("Неинициализированное сообщение", "sourceItem");
#endif

      if (String.IsNullOrEmpty(prefixText) && String.IsNullOrEmpty(suffixText))
        Add(sourceItem);
      else
      {
        StringBuilder sb = new StringBuilder();
        if (!String.IsNullOrEmpty(prefixText))
          sb.Append(prefixText);
        sb.Append(sourceItem.Text);
        if (!String.IsNullOrEmpty(suffixText))
          sb.Append(suffixText);
        Add(new ErrorMessageItem(sourceItem.Kind, sb.ToString(), sourceItem.Code, sourceItem.Tag));
      }
    }

    /// <summary>
    /// Добавить копии существующих сообщений из другого списка с возможностью
    /// задать одинаковый текст перед и после каждого сообщения
    /// Дополнительные пробелы не добавляются. Они должны быть предусмотрены в
    /// конце PrefixText и в начале PostfixText для отделения от основного текста
    /// сообщения
    /// </summary>
    /// <param name="source">Исходный список сообщений</param>
    /// <param name="prefixText">Текст перед каждым сообщением</param>
    /// <param name="suffixText">Текст после каждого сообщения</param>
    public void Add(ErrorMessageList source, string prefixText, string suffixText)
    {
      for (int i = 0; i < source.Count; i++)
        Add(source[i], prefixText, suffixText);
    }

    /// <summary>
    /// Добавить копии сушествующих сообщений из другого списка
    /// </summary>
    /// <param name="source">Исходный список сообщений</param>
    public void Add(ErrorMessageList source)
    {
      for (int i = 0; i < source.Count; i++)
        Add(source[i]);
    }

    #endregion

    #region Методы модификации списка

    #region LimitKind()

    /// <summary>
    /// Создает копию списка сообщений, в котором уровень серьезности сообщений
    /// понижен до MaxKind.
    /// То есть, если MaxKind=Warning, то ошибки заменяются на предупреждения,
    /// а предупреждения и информационные сообщения оставляются без изменений
    /// Если же MaxKind=Info, то все сообщения копируются как информационные
    /// </summary>
    /// <param name="maxKind">Ограничитель уровня (Warning или Info)</param>
    /// <returns></returns>
    public ErrorMessageList LimitKind(ErrorMessageKind maxKind)
    {
      ErrorMessageList List2 = new ErrorMessageList();
      for (int i = 0; i < Count; i++)
      {
        ErrorMessageKind Kind = this[i].Kind;
        if (Kind > maxKind)
          Kind = maxKind;
        List2.Add(new ErrorMessageItem(Kind, this[i]));
      }
      return List2;
    }

    #endregion

    #region SetTag()

    /// <summary>
    /// Установка свойства Tag для всех сообщений в списке
    /// </summary>
    /// <param name="tag">Новое значение свойства</param>
    public void SetTag(object tag)
    {
      SetTag(tag, 0); // Исправлено 27.12.2020
    }

    /// <summary>
    /// Установка свойства Tag для всех сообщений в списке, начиная с заданного
    /// </summary>
    /// <param name="tag">Новое значение свойства</param>
    /// <param name="startIndex">Индекс сообщения в списке, с которого должна выполняться установка</param>
    public void SetTag(object tag, int startIndex)
    {
      CheckNotReadOnly();

      for (int i = startIndex; i < base.Count; i++)
      {
        ErrorMessageItem src = base[i];
        base[i] = new ErrorMessageItem(src.Kind, src.Text, src.Code, tag);
      }
    }


    #endregion

    #region SetCode()

    /// <summary>
    /// Установка свойства Code для всех сообщений в списке
    /// </summary>
    /// <param name="code">Новое значение свойства</param>
    public void SetCode(string code)
    {
      SetCode(code, 0);
    }

    /// <summary>
    /// Установка свойства Code для всех сообщений в списке
    /// </summary>
    /// <param name="code">Новое значение свойства</param>
    /// <param name="startIndex">Индекс сообщения в списке, с которого должна выполняться установка</param>
    public void SetCode(string code, int startIndex)
    {
      CheckNotReadOnly();

      for (int i = startIndex; i < base.Count; i++)
      {
        ErrorMessageItem src = base[i];
        base[i] = new ErrorMessageItem(src.Kind, src.Text, code, src.Tag);
      }
    }

    /// <summary>
    /// Установка свойства Code для всех сообщений в списке
    /// </summary>
    /// <param name="code">Новое значение свойства</param>
    public void SetCode(int code)
    {
      SetCode(code, 0);
    }

    /// <summary>
    /// Установка свойства Code для всех сообщений в списке
    /// </summary>
    /// <param name="code">Новое значение свойства</param>
    /// <param name="startIndex">Индекс сообщения в списке, с которого должна выполняться установка</param>
    public void SetCode(int code, int startIndex)
    {
      CheckNotReadOnly();

      for (int i = startIndex; i < base.Count; i++)
      {
        ErrorMessageItem src = base[i];
        base[i] = new ErrorMessageItem(src.Kind, src.Text, code.ToString(), src.Tag);
      }
    }

    #endregion

    #region SetPrefix() и SetSuffix()

    /// <summary>
    /// Добавление текста перед каждым сообщением в списке.
    /// Не забудьте пробел в конце текста префикса, чтобы отделить его от
    /// существующего текста
    /// </summary>
    /// <param name="prefix">Текст префикска</param>
    public void SetPrefix(string prefix)
    {
      SetPrefix(prefix, 0);
    }

    /// <summary>
    /// Добавление текста перед каждым сообщением в списке.
    /// Не забудьте пробел в конце текста префикса, чтобы отделить его от
    /// существующего текста
    /// </summary>
    /// <param name="prefix">Текст префикска</param>
    /// <param name="startIndex">Индекс сообщения в списке, с которого должна выполняться установка</param>
    public void SetPrefix(string prefix, int startIndex)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(prefix))
        return;

      for (int i = startIndex; i < base.Count; i++)
      {
        ErrorMessageItem src = base[i];
        base[i] = new ErrorMessageItem(src.Kind, prefix + src.Text, src.Code, src.Tag);
      }
    }

    /// <summary>
    /// Добавление текста после каждого сообщения в списке.
    /// Не забудьте пробел перед текстом суффикса, чтобы отделить его от
    /// существующего текста
    /// </summary>
    /// <param name="suffix">Текст суффикса</param>
    public void SetSuffix(string suffix)
    {
      SetSuffix(suffix, 0);
    }

    /// <summary>
    /// Добавление текста после каждого сообщения в списке.
    /// Не забудьте пробел перед текстом суффикса, чтобы отделить его от
    /// существующего текста
    /// </summary>
    /// <param name="suffix">Текст суффикса</param>
    /// <param name="startIndex">Индекс сообщения в списке, с которого должна выполняться установка</param>
    public void SetSuffix(string suffix, int startIndex)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(suffix))
        return;

      for (int i = startIndex; i < base.Count; i++)
      {
        ErrorMessageItem src = base[i];
        base[i] = new ErrorMessageItem(src.Kind, src.Text + suffix, src.Code, src.Tag);
      }
    }

    #endregion

    #endregion

    #region Получение текста

    /// <summary>
    /// Получить список сообщений в виде массива строк.
    /// Информация о серьезности сообщения не возвращается
    /// </summary>
    public string[] AllLines
    {
      get
      {
        string[] a = new string[Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = this[i].Text;
        return a;
      }
    }

    /// <summary>
    /// Получить список сообщений в виде текста.
    /// Строки разделяются Environment.NewLine
    /// Информация о серьезности сообщения не возвращается
    /// </summary>
    public string AllText
    {
      get
      {
        string[] a = AllLines;
        return String.Join(Environment.NewLine, a);
      }
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Текстовое представление, содержащее количество сообщений в списке
    /// </summary>
    /// <returns>Текст</returns>
    public override string ToString()
    {
      if (Count == 0)
        return "Нет сообщений";

      int ErrorCount, WarningCount, InfoCount;
      this.GetCounts(out ErrorCount, out WarningCount, out InfoCount);

      StringBuilder sb = new StringBuilder();
      if (ErrorCount > 0)
      {
        sb.Append("Ошибок: ");
        sb.Append(ErrorCount.ToString());
      }
      if (WarningCount > 0)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append("Предупреждений: ");
        sb.Append(WarningCount.ToString());
      }
      if (InfoCount > 0)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append("Информационных сообщений: ");
        sb.Append(InfoCount.ToString());
      }

      return sb.ToString();
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Максимальный из двух уроовней серьезности сообщений
    /// </summary>
    /// <param name="kind1">Уровень серьезности №1</param>
    /// <param name="kind2">Уровень серьезности №2</param>
    /// <returns>Максимальный из двух</returns>
    public static ErrorMessageKind MaxSeverity(ErrorMessageKind kind1, ErrorMessageKind kind2)
    {
      if (kind1 == ErrorMessageKind.Error || kind2 == ErrorMessageKind.Error)
        return ErrorMessageKind.Error;
      if (kind1 == ErrorMessageKind.Warning || kind2 == ErrorMessageKind.Warning)
        return ErrorMessageKind.Warning;
      return ErrorMessageKind.Info;
    }

    /// <summary>
    /// Максимальный уровень серьезности для двух списков
    /// Один или оба списка могут быть null
    /// </summary>
    /// <param name="list1">Первый список</param>
    /// <param name="list2">Второй список</param>
    /// <returns>Максимальный из двух</returns>
    public static ErrorMessageKind MaxSeverity(ErrorMessageList list1, ErrorMessageList list2)
    {
      if (list1 == null)
      {
        if (list2 == null)
          return ErrorMessageKind.Info;
        else
          return list2.Severity;
      }
      else
      {
        if (list2 == null)
          return list1.Severity;
        else
          return MaxSeverity(list1.Severity, list2.Severity);
      }
    }

    /// <summary>
    /// Максимальный уровень серьезности для уже определенного уровня и списка
    /// </summary>
    /// <param name="kind1">Ранее вычисленный уровень серьезности</param>
    /// <param name="list2">Список ошибок</param>
    /// <returns>Максимальный из двух</returns>
    public static ErrorMessageKind MaxSeverity(ErrorMessageKind kind1, ErrorMessageList list2)
    {
      if (list2 == null)
        return kind1;
      else
        return ErrorMessageList.MaxSeverity(kind1, list2.Severity);
    }

    /// <summary>
    /// Метод сравнения серьезности сообщений
    /// Возвращает (-1), если первое сообщение менее серьезно, чем второе
    /// Возвращает (+1), если первое сообщение более серьезно, чем второе
    /// Возвращает 0, если уровень серьезности одинаковый
    /// </summary>
    /// <param name="kind1">Первое сравниваемое сообщение</param>
    /// <param name="kind2">Второе сравниваемое сообщение</param>
    /// <returns>Результат сравнения серьезности</returns>
    public static int Compare(ErrorMessageKind kind1, ErrorMessageKind kind2)
    {
      return -(((int)kind1).CompareTo((int)kind2));
    }

    #endregion

    #region Исключение ErrorMessageListException

    /// <summary>
    /// Если в списке есть сообщения об ошибках, то генерируется исключение ErrorMessageListExcepion.
    /// Независимо от этого список переводится в режим IsReadOnly=true
    /// </summary>
    public void ThrowIfErrors()
    {
      SetReadOnly();

      if (Severity == ErrorMessageKind.Error)
        throw new ErrorMessageListException(this);
    }

    /// <summary>
    /// Если в списке есть сообщения об ошибках, то генерируется исключение ErrorMessageListExcepion.
    /// Независимо от этого список переводится в режим IsReadOnly=true
    /// </summary>
    /// <param name="message">Текст для исключения (свойство Exception.Message)</param>
    public void ThrowIfErrors(string message)
    {
      SetReadOnly();

      if (Severity == ErrorMessageKind.Error)
        throw new ErrorMessageListException(this, message);
    }

    /// <summary>
    /// Добавляет информацию об исключении как сообщение об ошибке.
    /// Исключение ErrorMessageListException обрабатывается особым образом: добавляются все сообщения,
    /// присоединенные к исключению, а свойство Exception.Message не используется.
    /// Вложенные исключения (Exception.InnerException) не учитываются
    /// </summary>
    /// <param name="e">Добавляемое исключение</param>
    public void Add(Exception e)
    {
      Add(e, String.Empty, String.Empty);
    }

    /// <summary>
    /// Добавляет информацию об исключении как сообщение об ошибке.
    /// Исключение ErrorMessageListException обрабатывается особым образом: добавляются все сообщения,
    /// присоединенные к исключению, а свойство Exception.Message не используется.
    /// Вложенные исключения (Exception.InnerException) не учитываются
    /// </summary>
    /// <param name="e">Добавляемое исключение</param>
    /// <param name="prefixText">Текст перед каждым сообщением</param>
    /// <param name="suffixText">Текст после каждого сообщения</param>
    public void Add(Exception e, string prefixText, string suffixText)
    {
      if (e == null)
        return;

      ErrorMessageListException e2 = e as ErrorMessageListException;
      if (e2 == null)
        Add(new ErrorMessageItem(ErrorMessageKind.Error, e.Message), prefixText, suffixText);
      else
        Add(e2.Errors, prefixText, suffixText);
    }

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Пустой список сообщений в режиме IsReadOnly=true
    /// </summary>
    public static readonly ErrorMessageList Empty = new ErrorMessageList(true);

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию списка сообщений.
    /// У копии списка свойство IsReadOnly=false
    /// </summary>
    /// <returns>Копия списка</returns>
    public ErrorMessageList Clone()
    {
      ErrorMessageList List2 = new ErrorMessageList();
      for (int i = 0; i < Count; i++)
        List2.Add(this[i]);
      return List2;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию списка сообщений, если у текущего списка установлено свойство IsReadOnly=true.
    /// Иначе никаких действий не выполняется и возвращается ссылка на текущий список
    /// </summary>
    /// <returns>Копия списка</returns>
    public ErrorMessageList CloneIfReadOnly()
    {
      if (IsReadOnly)
        return Clone();
      else
        return this;
    }

    /// <summary>
    /// Создает копию списка, содержащую только сообщения заданной серьезности.
    /// </summary>
    /// <param name="kind">Серьезность сообщений</param>
    /// <returns>Копия списка</returns>
    public ErrorMessageList Clone(ErrorMessageKind kind)
    {
      ErrorMessageList lst2 = new ErrorMessageList();
      for (int i = 0; i < Count; i++)
      {
        if (this[i].Kind == kind)
          lst2.Add(this[i]);
      }
      return lst2;
    }

    #endregion
  }

  #region Делегат

  /// <summary>
  /// Аргументы события для делегата ErrorMessageItemEventHandler
  /// </summary>
  public class ErrorMessageItemEventArgs : EventArgs
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект аргументов события для выбранного сообщения
    /// </summary>
    /// <param name="list">Список сообщений</param>
    /// <param name="itemIndex">Номер выбранного сообщения</param>
    public ErrorMessageItemEventArgs(ErrorMessageList list, int itemIndex)
    {
      _List = list;
      _ItemIndex = itemIndex;
      _Item = list[itemIndex];
    }

    /// <summary>
    /// Создает объект аргументов события lдля выбранного сообщения.
    /// Свойство List получает значение null, a ItemIndex получает значение (-1)
    /// </summary>
    /// <param name="item">Сообщение</param>
    public ErrorMessageItemEventArgs(ErrorMessageItem item)
    {
      _List = null;
      _ItemIndex = -1;
      _Item = item;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список сообщений
    /// </summary>
    public ErrorMessageList List { get { return _List; } }
    private readonly ErrorMessageList _List;

    /// <summary>
    /// Номер сообщения, к которому относится событие
    /// </summary>
    public int ItemIndex { get { return _ItemIndex; } }
    private readonly int _ItemIndex;

    /// <summary>
    /// Сообщение
    /// </summary>
    public ErrorMessageItem Item { get { return _Item; } }
    private readonly ErrorMessageItem _Item;

    #endregion
  }

  /// <summary>
  /// Делегат события, связанного с сообщением
  /// </summary>
  /// <param name="sender">Истгочник события</param>
  /// <param name="args">Аргументы</param>
  public delegate void ErrorMessageItemEventHandler(object sender,
    ErrorMessageItemEventArgs args);

  #endregion
}
