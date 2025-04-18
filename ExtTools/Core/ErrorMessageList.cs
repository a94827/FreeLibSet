﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using FreeLibSet.Collections;
using System.ComponentModel;

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
  /// Одно сообщение об ошибке, хранящееся в <see cref="ErrorMessageList"/>.
  /// </summary>
  [Serializable]
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
        throw ExceptionFactory.ArgStringIsNullOrEmpty("text");

      _Kind = kind;
      _Text = text;
      if (code == null)
        _Code = String.Empty;
      else
        _Code = code;
      _Tag = tag;
    }

    /// <summary>
    /// Создание копии сообщения с другим уровнем серьезности.
    /// Сообщение не добавляется в список.
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
    /// Свойство задается в конструкторе. Для списка ошибок можно использовать метод <see cref="ErrorMessageList.SetCode(string)"/>.
    /// При использовании числовых кодов требуется выполнять преобразование
    /// в строку или обратно.
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Произвольные пользовательские данные.
    /// Поле может быть использовано для идентификации объекта, в котором
    /// произошла ошибка.
    /// При копировании сообщения об ошибке значение свойства также копируется.
    /// Свойство устанавливается в конструкторе.
    /// Для списка ошибок можно использовать метод <see cref="ErrorMessageList.SetTag(object)"/>.
    /// </summary>
    public object Tag { get { return _Tag; } }
    private readonly object _Tag;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована с помощью конструктора.
    /// Пустую структуру нельзя добавлять в <see cref="ErrorMessageList"/>.
    /// </summary>
    public bool IsEmpty { get { return Object.ReferenceEquals(_Text, null); } }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Текстовое представление сообщения, включая уровень серьезности и код, если задан
    /// </summary>
    /// <returns>Текст</returns>
    public override string ToString()
    {
      string sKind = UICore.UITools.ToString(_Kind);
      if (String.IsNullOrEmpty(_Code))
        return String.Format(Res.ErrorMessageItem_Msg_ToString, sKind, _Text);
      else
        return String.Format(Res.ErrorMessageItem_Msg_ToStringWithCode, sKind, _Text, _Code);
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
    /// Нельзя увеличивать значение свойства.
    /// </summary>
    public new int Count
    {
      get { return base.Count; }
      set
      {
        CheckNotReadOnly();

        if (value < 0 || value > base.Count)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, Count);

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
    /// Если список пуст, то возвращается уровень <see cref="ErrorMessageKind.Info"/>.
    /// </summary>
    public ErrorMessageKind Severity
    {
      get
      {
        bool hasWarning = false;
        for (int i = 0; i < base.Count; i++)
        {
          switch (base[i].Kind)
          {
            case ErrorMessageKind.Error:
              return ErrorMessageKind.Error;
            case ErrorMessageKind.Warning:
              hasWarning = true;
              break;
          }
        }
        if (hasWarning)
          return ErrorMessageKind.Warning;
        else
          return ErrorMessageKind.Info;
      }
    }


    /// <summary>
    /// Максимальный уровень серьезности сообщений.
    /// Если список пуст, то возвращается null.
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
    /// Получить количество сообщений об ошибках (<see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Error"/>).
    /// Если требуется общее количество сообщений всех видов, следует использовать
    /// свойство <see cref="Count"/>, т.к. для него не требуется перебор записей.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод <see cref="GetCounts(out int, out int, out int)"/>.
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
    /// Получить количество предупреждений (<see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Warning"/>).
    /// Если требуется общее количество сообщений всех видов, следует использовать
    /// свойство <see cref="Count"/>, т.к. для него не требуется перебор записей.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод <see cref="GetCounts(out int, out int, out int)"/>.
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
    /// Получить количество информационных сообщений об ошибках (<see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Info"/>).
    /// Если требуется общее количество сообщений всех видов, следует использовать
    /// свойство <see cref="Count"/>, т.к. для него не требуется перебор записей.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод <see cref="GetCounts(out int, out int, out int)"/>.
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
    /// Получить количество сообщений об ошибках (<see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Error"/>) для части списка.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод <see cref="GetCounts(out int, out int, out int, int)"/>.
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
    /// Получить количество предупреждений (<see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Warning"/>) для части списка.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод <see cref="GetCounts(out int, out int, out int, int)"/>.
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
    /// Получить количество информационных сообщений (<see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Info"/>) для части списка.
    /// Если требуется одновременное получение количества сообщений разных видов, используйте метод <see cref="GetCounts(out int, out int, out int, int)"/>.
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
    /// Возвращает свойства <see cref="ErrorCount"/>, <see cref="WarningCount"/> и <see cref="InfoCount"/> за один проход для части списка
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
    /// Возвращает свойства <see cref="ErrorCount"/>, <see cref="WarningCount"/> и <see cref="InfoCount"/> за один проход
    /// </summary>
    /// <param name="errorCount">Сюда помещается количество сообщений об ошибках</param>
    /// <param name="warningCount">Сюда помещается количество предупреждений</param>
    /// <param name="infoCount">Сюда помещается количество информационных сообщений</param>
    public void GetCounts(out int errorCount, out int warningCount, out int infoCount)
    {
      GetCounts(out errorCount, out warningCount, out infoCount, 0);
    }

    /// <summary>
    /// Возвращает свойства <see cref="ErrorCount"/>, <see cref="WarningCount"/> за один проход.
    /// Информационные сообщения не учитываются.
    /// </summary>
    /// <param name="errorCount">Сюда помещается количество сообщений об ошибках</param>
    /// <param name="warningCount">Сюда помещается количество предупреждений</param>
    public void GetCounts(out int errorCount, out int warningCount)
    {
      int infoCount;
      GetCounts(out errorCount, out warningCount, out infoCount);
    }

    /// <summary>
    /// Возвращает первый элемент в списке, обладающий максимальным уровнем важности.
    /// То есть, если в списке есть ошибки, то будет возвращен первый элемент с уровнем серьезности <see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Error"/>.
    /// Иначе, если в списке есть предупреждения, то будет возвращен первый элемент с уровнем серьезности <see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Warning"/>.
    /// Иначе будет возвращен первый элемент списка (с уровнем серьезности <see cref="ErrorMessageItem.Kind"/>=<see cref="ErrorMessageKind.Info"/>).
    /// Если список пуст, возвращается null.
    /// </summary>
    public ErrorMessageItem? FirstSevereItem
    {
      get
      {
        if (Count == 0)
          return null;

        int firstWarningIndex = -1;

        for (int i = 0; i < Count; i++)
        {
          switch (this[i].Kind)
          {
            case ErrorMessageKind.Error:
              return this[i];
            case ErrorMessageKind.Warning:
              if (firstWarningIndex < 0)
                firstWarningIndex = i;
              break;
          }
        }

        if (firstWarningIndex >= 0)
          return this[firstWarningIndex];
        else
          return this[0]; // все сообщения имеют уровень Info
      }
    }

    #endregion

    #region IsReadOnly

    /// <summary>
    /// Устанавливает свойство <see cref="ListWithReadOnly{ErrorMessageItem}.IsReadOnly"/> в true, что блокирует дальнейшее добавление сообщений.
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
    /// Добавить копию сообщение с уровнем серьезности "Информационное сообщение"
    /// </summary>
    /// <param name="sourceItem">Копируемое сообщение</param>
    public void AddInfo(ErrorMessageItem sourceItem)
    {
      Add(new ErrorMessageItem(ErrorMessageKind.Info, sourceItem));
    }

    /// <summary>
    /// Добавить копию сообщения (например, из другого списка) с возможностью задать
    /// дополнительный текст перед и после существующего.
    /// Дополнительные пробелы не добавляются. Они должны быть предусмотрены в
    /// конце <paramref name="prefixText"/> и в начале <paramref name="suffixText"/> для отделения от основного текста сообщения.
    /// </summary>
    /// <param name="sourceItem">Исходное сообщение</param>
    /// <param name="prefixText">Текст перед сообщением</param>
    /// <param name="suffixText">Текст после сообщения</param>
    public void Add(ErrorMessageItem sourceItem, string prefixText, string suffixText)
    {
#if DEBUG
      if (String.IsNullOrEmpty(sourceItem.Text))
        throw ExceptionFactory.ArgIsEmpty("sourceItem");
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
    /// задать одинаковый текст перед и после каждого сообщения.
    /// Дополнительные пробелы не добавляются. Они должны быть предусмотрены в
    /// конце <paramref name="prefixText"/> и в начале <paramref name="suffixText"/> для отделения от основного текста сообщения.
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

    #region SetMaxSeverity()

    /// <summary>
    /// Понижает уровень серьезности сообщений до <paramref name="maxKind"/>
    /// То есть, если <paramref name="maxKind"/>=<see cref="ErrorMessageKind.Warning"/>, то ошибки заменяются на предупреждения,
    /// а предупреждения и информационные сообщения оставляются без изменений.
    /// Если же <paramref name="maxKind"/>=<see cref="ErrorMessageKind.Info"/>, то все сообщения копируются как информационные.
    /// Если <paramref name="maxKind"/>=<see cref="ErrorMessageKind.Error"/>, то никаких действий не выполняется.
    /// </summary>
    /// <param name="maxKind">Ограничитель уровня (<see cref="ErrorMessageKind.Warning"/> или <see cref="ErrorMessageKind.Info"/>)</param>
    public void SetMaxSeverity(ErrorMessageKind maxKind)
    {
      CheckNotReadOnly();

      for (int i = 0; i < base.Count; i++)
      {
        ErrorMessageKind thisKind = this[i].Kind;
        if (Compare(thisKind, maxKind) > 0)
          thisKind = maxKind;
        this[i] = new ErrorMessageItem(thisKind, this[i]);
      }
    }


    /// <summary>
    /// Создает копию списка сообщений, в котором уровень серьезности сообщений
    /// понижен до <paramref name="maxKind"/>.
    /// То есть, если <paramref name="maxKind"/>=<see cref="ErrorMessageKind.Warning"/>, то ошибки заменяются на предупреждения,
    /// а предупреждения и информационные сообщения оставляются без изменений.
    /// Если же <paramref name="maxKind"/>=<see cref="ErrorMessageKind.Info"/>, то все сообщения копируются как информационные.
    /// Если <paramref name="maxKind"/>=<see cref="ErrorMessageKind.Error"/>, то возвращается немодифицированная копия списка, как в методе <see cref="Clone()"/>.
    /// </summary>
    /// <param name="maxKind">Ограничитель уровня (<see cref="ErrorMessageKind.Warning"/> или <see cref="ErrorMessageKind.Info"/>)</param>
    /// <returns>Копия списка</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Используйте Clone() и SetMaxSeverity()", false)]
    public ErrorMessageList LimitKind(ErrorMessageKind maxKind)
    {
      ErrorMessageList list2 = Clone();
      list2.SetMaxSeverity(maxKind);
      return list2;
    }

    #endregion

    #region SetTag()

    /// <summary>
    /// Установка свойства <see cref="ErrorMessageItem.Tag"/> для всех сообщений в списке.
    /// </summary>
    /// <param name="tag">Новое значение свойства</param>
    public void SetTag(object tag)
    {
      SetTag(tag, 0); // Исправлено 27.12.2020
    }

    /// <summary>
    /// Установка свойства <see cref="ErrorMessageItem.Tag"/> для всех сообщений в списке, начиная с заданного.
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
    /// Установка свойства <see cref="ErrorMessageItem.Code"/> для всех сообщений в списке.
    /// </summary>
    /// <param name="code">Новое значение свойства</param>
    public void SetCode(string code)
    {
      SetCode(code, 0);
    }

    /// <summary>
    /// Установка свойства <see cref="ErrorMessageItem.Code"/> для всех сообщений в списке, начиная с заданного.
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

    #endregion

    #region SetPrefix() и SetSuffix()

    /// <summary>
    /// Добавление текста перед каждым сообщением в списке.
    /// Не забудьте пробел в конце текста префикса, чтобы отделить его от
    /// существующего текста.
    /// </summary>
    /// <param name="prefix">Текст префикска</param>
    public void SetPrefix(string prefix)
    {
      SetPrefix(prefix, 0);
    }

    /// <summary>
    /// Добавление текста перед каждым сообщением в списке.
    /// Не забудьте пробел в конце текста префикса, чтобы отделить его от
    /// существующего текста.
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
    /// существующего текста.
    /// </summary>
    /// <param name="suffix">Текст суффикса</param>
    public void SetSuffix(string suffix)
    {
      SetSuffix(suffix, 0);
    }

    /// <summary>
    /// Добавление текста после каждого сообщения в списке.
    /// Не забудьте пробел перед текстом суффикса, чтобы отделить его от
    /// существующего текста.
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
    /// Информация о серьезности сообщения не возвращается.
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
    /// Строки разделяются <see cref="Environment.NewLine"/>.
    /// Информация о серьезности сообщения не возвращается.
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
        return "Empty";

      int errorCount, warningCount, infoCount;
      this.GetCounts(out errorCount, out warningCount, out infoCount);

      StringBuilder sb = new StringBuilder();
      if (errorCount > 0)
      {
        sb.Append("Errors: ");
        sb.Append(errorCount.ToString());
      }
      if (warningCount > 0)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append("Warnings: ");
        sb.Append(warningCount.ToString());
      }
      if (infoCount > 0)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append("Infos: ");
        sb.Append(infoCount.ToString());
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
    /// Максимальный уровень серьезности для двух списков.
    /// Один или оба списка могут быть null.
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
    /// Метод сравнения серьезности сообщений.
    /// Возвращает положительное значение, если первое сообщение более серьезно, чем второе.
    /// Возвращает отрицательное значение, если первое сообщение менее серьезно, чем второе.
    /// Возвращает 0, если уровень серьезности одинаковый.
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
    /// Если в списке есть сообщения об ошибках, то генерируется исключение <see cref="ErrorMessageListException"/>.
    /// Независимо от этого список переводится в режим <see cref="ListWithReadOnly{ErrorMessageItem}.IsReadOnly"/>=true.
    /// </summary>
    public void ThrowIfErrors()
    {
      SetReadOnly();

      if (Severity == ErrorMessageKind.Error)
        throw new ErrorMessageListException(this);
    }

    /// <summary>
    /// Если в списке есть сообщения об ошибках, то генерируется исключение <see cref="ErrorMessageListException"/>.
    /// Независимо от этого список переводится в режим <see cref="ListWithReadOnly{ErrorMessageItem}.IsReadOnly"/>=true.
    /// </summary>
    /// <param name="message">Текст для исключения (свойство <see cref="Exception.Message"/>)</param>
    public void ThrowIfErrors(string message)
    {
      SetReadOnly();

      if (Severity == ErrorMessageKind.Error)
        throw new ErrorMessageListException(this, message);
    }

    /// <summary>
    /// Добавляет информацию об исключении как сообщение об ошибке.
    /// Исключение <see cref="ErrorMessageListException"/> обрабатывается особым образом: добавляются все сообщения,
    /// присоединенные к исключению, а свойство <see cref="Exception.Message"/> не используется.
    /// Вложенные исключения (<see cref="Exception.InnerException"/>) не учитываются.
    /// </summary>
    /// <param name="e">Добавляемое исключение</param>
    public void Add(Exception e)
    {
      Add(e, String.Empty, String.Empty);
    }

    /// <summary>
    /// Добавляет информацию об исключении как сообщение об ошибке.
    /// Исключение <see cref="ErrorMessageListException"/> обрабатывается особым образом: добавляются все сообщения,
    /// присоединенные к исключению, а свойство <see cref="Exception.Message"/> не используется.
    /// Вложенные исключения (<see cref="Exception.InnerException"/>) не учитываются.
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
    /// Пустой список сообщений в режиме <see cref="ListWithReadOnly{ErrorMessageItem}.IsReadOnly"/>=true.
    /// </summary>
    public static readonly ErrorMessageList Empty = new ErrorMessageList(true);

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию списка сообщений.
    /// У копии списка свойство <see cref="ListWithReadOnly{ErrorMessageItem}.IsReadOnly"/>=false.
    /// </summary>
    /// <returns>Копия списка</returns>
    public ErrorMessageList Clone()
    {
      ErrorMessageList list2 = new ErrorMessageList();
      for (int i = 0; i < Count; i++)
        list2.Add(this[i]);
      return list2;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию списка сообщений, если у текущего списка установлено свойство <see cref="ListWithReadOnly{ErrorMessageItem}.IsReadOnly"/>=true.
    /// Иначе никаких действий не выполняется и возвращается ссылка на текущий список.
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
      ErrorMessageList list2 = new ErrorMessageList();
      for (int i = 0; i < Count; i++)
      {
        if (this[i].Kind == kind)
          list2.Add(this[i]);
      }
      return list2;
    }

    #endregion
  }

  #region Делегат

  /// <summary>
  /// Аргументы события для делегата <see cref="ErrorMessageItemEventHandler"/>.
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
    /// Создает объект аргументов события для выбранного сообщения.
    /// Свойство <see cref="List"/> получает значение null, a <see cref="ItemIndex"/> получает значение (-1).
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
