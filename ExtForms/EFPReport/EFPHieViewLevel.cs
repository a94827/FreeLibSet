using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Remoting;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

/*
 * Иерархические отчеты на базе EFPDataGridView
 */


#if XXX

namespace FreeLibSet.Forms
{
#region Перечисления

  /// <summary>
  /// Значения свойства HieViewLevel.Position. Определяют возможности перестановки
  /// уровней иерархии в редакторе детализации
  /// </summary>
  public enum EFPHieViewLevelPosition
  {
    /// <summary>
    /// Значение по умолчанию. 
    /// Этот уровень может переставляться с другими, имеющими значение Position=Normal
    /// </summary>
    Normal,

    /// <summary>
    /// Этот уровень не может быть переставлен. Уровни с Position=FixedTop 
    /// располагаются на "вершине" детализации (то есть самые внешние уровни).
    /// Порядок таких уровней фиксирован и соответствует исходному порядку
    /// </summary>
    FixedTop,

    /// <summary>
    /// Этот уровень не может быть переставлен. Уровни с Position=FixedBottom 
    /// располагаются "внутри" детализации.
    /// Порядок таких уровней фиксирован и соответствует исходному порядку
    /// </summary>
    FixedBottom
  }

#endregion

#region Делегаты

  /// <summary>
  /// Аргумент для события HieViewLevel.UserGetText
  /// </summary>
  public class EFPHieViewLevelTextNeededEventArgs : EventArgs
  {
#region Конструктор

    internal EFPHieViewLevelTextNeededEventArgs()
    {
    }

#endregion

#region Свойства

    /// <summary>
    /// Строка, из которой нужно извлечь знчение поля
    /// </summary>
    public DataRow Row { get { return FRow; } internal set { FRow = value; } }
    private DataRow FRow;

    /// <summary>
    /// Сюда надо записать результат - текстовое значение
    /// </summary>
    public string Text
    {
      get { return FText; }
      set
      {
        if (value == null)
          FText = String.Empty;
        else
          FText = value;
      }
    }
    private string FText;

    /// <summary>
    /// Сюда можно занести особый текст для итоговой строки. Если значение не будет
    /// установлено, то используется текст вида "Итого: "+Text
    /// </summary>
    public string TotalText
    {
      get
      {
        if (String.IsNullOrEmpty(FTotalText))
          return "Итого: " + Text;
        else
          return FTotalText;
      }
      set
      {
        FTotalText = value;
      }
    }
    private string FTotalText;

#endregion
  }

  public delegate void EFPHieViewLevelTextNeededEventHandler(object Sender,
    EFPHieViewLevelTextNeededEventArgs Args);

  public class EFPHieViewLevelSortKeyNeededEventArgs : EventArgs
  {
#region Конструктор

    internal EFPHieViewLevelSortKeyNeededEventArgs()
    {
    }

#endregion

#region Свойства

    /// <summary>
    /// Строка, из которой нужно извлечь значение поля
    /// </summary>
    public DataRow Row { get { return FRow; } internal set { FRow = value; } }
    private DataRow FRow;

    /// <summary>
    /// Сюда надо записать результат - текстовое значение, которое будет использоваться
    /// для сортировки
    /// </summary>
    public string Key { get { return FKey; } set { FKey = value; } }
    private string FKey;

#endregion
  }

  public delegate void EFPHieViewLevelSortKeyNeededEventHandler(object Sender,
    EFPHieViewLevelSortKeyNeededEventArgs Args);

  public class HieViewLevelEditRowEventArgs : EventArgs
  {
#region Конструктор

    public HieViewLevelEditRowEventArgs(DataRow Row, bool ReadOnly)
    {
      FRow = Row;
      FReadOnly = ReadOnly;
    }

#endregion

#region Свойства

    /// <summary>
    /// Строка в табличном просмотре, для которой выполняется редактирование
    /// </summary>
    public DataRow Row { get { return FRow; } }
    private DataRow FRow;

    /// <summary>
    /// Возвращает true, если предполагается просмотр, а не редактирование
    /// </summary>
    public bool ReadOnly { get { return FReadOnly; } }
    private bool FReadOnly;

    /// <summary>
    /// Значение должно быть установлено в true, если редактирование было выполнено
    /// и данные, возможно, изменились
    /// </summary>
    public bool Modified
    {
      get { return FModified; }
      set { FModified = value; }
    }
    private bool FModified;

#endregion
  }

  public delegate void EFPHieViewLevelEditRowEventHandler(object Sender,
    HieViewLevelEditRowEventArgs Args);

#endregion

  /// <summary>
  /// Один уровень иерархии для отчета.
  /// Массив объектов присоединяется к свойству EFPHieView.Levels.
  /// Объект EFPHieViewLevel, в отличие от EFPHieView, является "многоразовым".
  /// </summary>
  public class EFPHieViewLevel : IReadOnlyObject
  {
#region Конструктор

    public EFPHieViewLevel(string Name)
    {
#if DEBUG
      if (String.IsNullOrEmpty(Name))
        throw new ArgumentNullException("Name", "Не задано имя уровня иерархии");
      if (Name.IndexOf(',') >= 0)
        throw new ArgumentException("Имя уровня иерархии \"" + Name + "\" содержит запятую", "Name");
#endif
      FName = Name;
      FColumnNameArray = new string[] { Name };
      FVisible = true;
    }

#if XXX
    /// <summary>
    /// Создание копии для использования в другом отчете
    /// </summary>
    /// <param name="OrgLevel"></param>
    public EFPHieViewLevel(EFPHieViewLevel OrgLevel)
      : this(OrgLevel.Name)
    {
      Mode = OrgLevel.Mode;
      FieldNameArray = OrgLevel.FieldNameArray;
      TextPrefix = OrgLevel.TextPrefix;
      RefTable = OrgLevel.RefTable;
      RefFieldName = OrgLevel.RefFieldName;
      EmptyText = OrgLevel.EmptyText;
      NotFoundText = OrgLevel.NotFoundText;
      if (OrgLevel.UserGetText != null)
        UserGetText = (EFPHieViewLevelGetTextEventHandler)(OrgLevel.UserGetText.Clone());

      FVisible = true;
      if (OrgLevel.FExtValues != null)
        FExtValues = (NamedValues)(OrgLevel.ExtValues.Clone());

      FParamEditor = OrgLevel.ParamEditor;
    }
#endif

#endregion

#region Основные свойства

    /// <summary>
    /// Название уровня иерархии.
    /// </summary>
    public string Name { get { return FName; } }
    private string FName;

    /// <summary>
    /// Если установить в True, то строка подытогов для данного уровня будет выводится
    /// перед вложенными уровнями, а строка заголовка выводиться не будет
    /// Игнорируется для строк нулевого уровня
    /// </summary>
    public bool SubTotalRowFirst
    {
      get { return FSubTotalRowFirst; }
      set
      {
        CheckNotReadOnly();
        FSubTotalRowFirst = value;
      }
    }
    private bool FSubTotalRowFirst;

#if XXX
    /// <summary>
    /// Произвольные пользовательские данные, например, для обработчика UserGetText
    /// </summary>
    public NamedValues ExtValues
    {
      get
      {
        if (FExtValues == null)
          FExtValues = new NamedValues();
        return FExtValues;
      }
    }
    private NamedValues FExtValues;

    internal bool HasExtValues { get { return FExtValues != null; } }
#endif

    /// <summary>
    /// Имя поля, из которого берется значение.
    /// Может быть задано несколько имен полей, разделенных запятыми
    /// Для доступа к полям как к массиву, используйте FieldNameArray
    /// </summary>
    public string ColumnName
    {
      get { return String.Join(",", FColumnNameArray); }
      set
      {
#if DEBUG
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException("FieldName");
#endif
        ColumnNameArray = value.Split(',');
      }
    }

    /// <summary>
    /// Имена полей для уровня в виде массива.
    /// Массив содержит хотя бы одно поле
    /// </summary>
    public string[] ColumnNameArray
    {
      get { return FColumnNameArray; }
      set
      {
        CheckNotReadOnly();
        if (value == null || value.Length == 0)
          throw new ArgumentNullException("ColumnName");
        for (int i = 0; i < value.Length; i++)
        {
          if (value[i].IndexOf(',') >= 0)
            throw new ArgumentException("Имена полей в массиве не могут содержать запятых", "FieldNameArray");
        }
        FColumnNameArray = value;
      }
    }
    private string[] FColumnNameArray;

#endregion

#region Получение текста строки

    /// <summary>
    /// Получение текста в режиме UserDefined
    /// </summary>
    public event EFPHieViewLevelTextNeededEventHandler TextNeeded;

    private EFPHieViewLevelTextNeededEventArgs TextNeededArgs;

    /// <summary>
    /// Главный метод. Получение текстового названия для строки.
    /// Должен вернуть строку. Формировать отступы не должен
    /// </summary>
    /// <param name="Row">Строка данных</param>
    /// <returns></returns>
    public string GetRowText(DataRow Row)
    {
      return GetRowText(Row, false);
    }

    public virtual string GetRowText(DataRow Row, bool TotalRow)
    {
      if (Row == null)
        throw new ArgumentNullException("Row");

      if (TextNeeded == null)
      {
        // Возвращаем значение поля
        object v = Row[ColumnName];
        if (v is DateTime)
          return ((DateTime)v).ToString("d");
        string s = v.ToString();
        return s;
      }
      else
      {
        // Используем пользовательский обработчик

        if (TextNeededArgs == null)
          TextNeededArgs = new EFPHieViewLevelTextNeededEventArgs();
        TextNeededArgs.Row = Row;
        TextNeededArgs.Text = "???";
        TextNeeded(this, TextNeededArgs);
        if (TotalRow)
          return TextNeededArgs.TotalText;
        else
          return TextNeededArgs.Text;
      }
    }

#endregion

#region Ключ для сортировки записей

    /// <summary>
    /// Событие вызывается при построении отчета для определения порядка сортировки
    /// строк одного уровня. Если обработчик не задан, то строки сортируются по
    /// текстовому значению плюс текстовые представления имен полей
    /// Обработчик может реализовать альтернативную сортировку
    /// по другим полям исходной таблицы, записав значения строки-ключа для сортировки
    /// </summary>
    public event EFPHieViewLevelSortKeyNeededEventHandler SortKeyNeeded;

    private EFPHieViewLevelSortKeyNeededEventArgs SortKeyNeededArgs;

    /// <summary>
    /// Получить текст для сортировки строк одного уровня
    /// </summary>
    /// <param name="Row"></param>
    /// <returns></returns>
    public virtual string GetRowSortKey(DataRow Row)
    {
      if (Row == null)
        throw new ArgumentNullException("Row");

      if (SortKeyNeeded == null)
      {
        if (ColumnNameArray.Length == 1)
          return GetColumnKeyValue(Row, ColumnNameArray[0]);
        else
        {
          string[] a = new string[ColumnNameArray.Length];
          for (int i = 0; i < ColumnNameArray.Length; i++)
            a[i] = GetColumnKeyValue(Row, ColumnNameArray[i]);
          return String.Join("|", a);
        }
      }
      else
      {
        if (SortKeyNeededArgs == null)
          SortKeyNeededArgs = new EFPHieViewLevelSortKeyNeededEventArgs();

        SortKeyNeededArgs.Row = Row;
        SortKeyNeededArgs.Key = String.Empty;
        SortKeyNeeded(this, SortKeyNeededArgs);
        return SortKeyNeededArgs.Key;
      }
    }

    private static string GetColumnKeyValue(DataRow Row, string ColumnName)
    {
      object v = Row[ColumnName];
      if (v is DBNull)
        return String.Empty;

      Type t = Row.Table.Columns[ColumnName].DataType;
      if (t == typeof(string))
        return ((string)v).ToUpperInvariant();
      if (t == typeof(DateTime))
        return ((DateTime)v).ToString("s");
      if (t == typeof(Int32))
        return BitConverter.ToString(BitConverter.GetBytes((Int32)v));
      // !!! Остальные типы данных
      throw new NotSupportedException("Сортировка для типа поля " + t.ToString() + " не реализована");
    }

#endregion

#region Свойства, используемые редактором детализации

    /// <summary>
    /// Имя уровня для отображения в редакторе иерархии. Если свойство не
    /// установлено явно, используется значение свойства NamePart
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(FDisplayName))
          return Name;
        else
          return FDisplayName;
      }
      set
      {
        CheckNotReadOnly();
        FDisplayName = value;
      }
    }
    private string FDisplayName;

    /// <summary>
    /// Изображение для уровня, используемое редактором детализации
    /// </summary>
    public string ImageKey
    {
      get
      {
        if (String.IsNullOrEmpty(FImageKey))
          return "Item";
        else
          return FImageKey;
      }
      set
      {
        CheckNotReadOnly();
        FImageKey = value;
      }
    }
    private string FImageKey;

    /// <summary>
    /// Используется редактором детализации. Если свойство установлено в false,
    /// то этот уровень иерархии не показывается в таблице и не может быть выбран.
    /// Значение по умолчанию - true. Свойство можно использовать, когда наличие
    /// уровней зависит от других параметров отчета. 
    /// После изменения свойства таблица редактора детализации должна быть обновлена
    /// </summary>
    public bool Visible
    {
      get { return FVisible; }
      set
      {
        FVisible = value;
        if (!value)
          Requred = false;
      }
    }
    private bool FVisible;

    /// <summary>
    /// Используется редактором детализации. Если свойство установлено в true,
    /// то уровень является обязательным (флажок включен и не может быть снят)
    /// После изменения свойства таблица редактора детализации должна быть обновлена
    /// </summary>
    public bool Requred
    {
      get { return FRequred; }
      set
      {
        CheckNotReadOnly();
        FRequred = value;
        if (value)
          Visible = true;
      }
    }
    private bool FRequred;

    /// <summary>
    /// Возможность перестановки уровней иерарархии в редакторе детализации
    /// После изменения свойства таблица редактора детализации должна быть обновлена
    /// </summary>
    public EFPHieViewLevelPosition Position { get { return FPosition; } set { FPosition = value; } }
    private EFPHieViewLevelPosition FPosition;

    /// <summary>
    /// Признак выбранности по умолчанию для данного уровня
    /// Значение по умолчанию - false (уровень не отмечен)
    /// Свойство игнорируется, ecли Required=true или Visible=false
    /// </summary>
    public bool DefaultSelected { get { return FDefaultSelected; } set { FDefaultSelected = value; } }
    private bool FDefaultSelected;

    /// <summary>
    /// Редактор произвольного параметра (параметров) настройки уровня
    /// Если свойство установлено, то в таблице редактора доступна кнопка "Редактировать",
    /// а рядом с названием уровня DisplayName выводится дополнительный текст
    /// </summary>
    public EFPHieViewLevelParamEditor ParamEditor { get { return FParamEditor; } set { FParamEditor = value; } }
    private EFPHieViewLevelParamEditor FParamEditor;

#endregion

#region Свойства и методы, используемые при редактировании в отчете

    /// <summary>
    /// Событие вызывается методом HieViewHandler.EditReportRow() для обработки редактирования
    /// строки отчета, при наличии вызова в прикладном модуле
    /// </summary>
    public event EFPHieViewLevelEditRowEventHandler EditRow;

    public bool OnEditRow(DataRow Row, bool ReadOnly)
    {
      if (EditRow == null)
      {
        EFPApp.ShowTempMessage("Редактирование для уровня \"" + DisplayName + "\" не предусмотрено");
        return false;
      }

      HieViewLevelEditRowEventArgs Args = new HieViewLevelEditRowEventArgs(Row, ReadOnly);
      EditRow(this, Args);
      return Args.Modified;
    }

#endregion

#region Прочие методы

    public override string ToString()
    {
      return Name;
    }

#endregion

#region IReadOnlyObject Members

    public bool IsReadOnly { get { return FIsReadOnly; } }
    private bool FIsReadOnly;

    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    public void SetReadOnly()
    {
      FIsReadOnly = true;
    }

#endregion
  }


}
#endif

