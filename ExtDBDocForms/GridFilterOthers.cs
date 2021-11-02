using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;

using FreeLibSet.Forms;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Config;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Простой фильтр по значению текстового поля (проверка поля на равенство 
  /// определенному значению)
  /// </summary>
  public class StringValueGridFilter : StringValueCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public StringValueGridFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Необязательный список строк, из которого осуществляется выбор.
    /// По умолчанию - null - выбор осуществлятся путем ручного ввода значения.
    /// Если список задан, то значение выбирается из списка.
    /// Порядок строк в списке соответствует порядку строк при просмотре, поэтому
    /// сортирока строк, если необходима, должна выполняться до установки свойства
    /// Свойство используется только при показе диалога фильтра. Перед этим 
    /// вызывается событие ValueListNeeded, которое может использоваться для
    /// динамической установки списка
    /// </summary>
    public string[] ValueList { get { return _ValueList; } set { _ValueList = value; } }
    private string[] _ValueList;

    /// <summary>
    /// Событие вызывается перед показом диалога фильтра в ShowFilterDialog()
    /// Обработчик может установить свойство ValueNeeded
    /// </summary>
    public event EventHandler ValueListNeeded;

    #endregion

    #region Методы IEFPGridFilter

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (String.IsNullOrEmpty(Value))
          return null;
        else
          return "\"" + Value + "\"";
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      if (ValueListNeeded != null)
      {
        try
        {
          ValueListNeeded(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка получения списка строк для фильтра \"" + DisplayName + "\"");
        }
      }

      if (ValueList != null && ValueList.Length > 0)
      {
        string[] List2 = new string[ValueList.Length + 2];
        string[] ImageKeys = new string[ValueList.Length + 2];
        List2[0] = "[ Нет фильтра ]";
        ImageKeys[0] = EFPGridFilterTools.NoFilterImageKey;
        for (int i = 0; i < ValueList.Length; i++)
        {
          List2[i + 1] = ValueList[i];
          ImageKeys[i + 1] = "Filter";
        }
        List2[ValueList.Length + 1] = "Задать вручную ...";
        ImageKeys[ValueList.Length + 1] = "Edit";

        ListSelectDialog dlg1 = new ListSelectDialog();
        dlg1.Title = DisplayName;
        dlg1.ListTitle = "Значение фильтра";
        dlg1.ImageKey = "Filter";
        dlg1.MultiSelect = false;
        dlg1.Items = List2;
        dlg1.ImageKeys = ImageKeys;
        dlg1.ConfigSectionName = "GridFilter." + this.Code;
        dlg1.DialogPosition = dialogPosition;

        if (String.IsNullOrEmpty(Value))
          dlg1.SelectedIndex = 0;
        else
        {
          int p = Array.IndexOf<string>(ValueList, Value);
          if (p >= 0)
            dlg1.SelectedIndex = p + 1;
          else
            dlg1.SelectedIndex = ValueList.Length + 1;
        }

        if (dlg1.ShowDialog() != DialogResult.OK)
          return false;

        if (dlg1.SelectedIndex == 0)
        {
          Value = String.Empty;
          return true;
        }

        if (dlg1.SelectedIndex <= ValueList.Length)
        {
          Value = ValueList[dlg1.SelectedIndex - 1];
          return true;
        }
        // Иначе выполняется ручной ввод
      }

      // Ручной ввод
      TextInputDialog dlg2 = new TextInputDialog();
      dlg2.Title = DisplayName;
      dlg2.ImageKey = "Filter";
      dlg2.Prompt = "Значение фильтра (пусто-нет фильтра)";
      dlg2.CanBeEmpty = true;
      dlg2.Text = Value;

      if (dlg2.ShowDialog() != DialogResult.OK)
        return false;
      Value = dlg2.Text;

      return true;
    }


    /*
    protected override string[] GetColumnStrValues(object[] ColumnValues)
    {
      return new string[] { DataTools.GetString(ColumnValues[0]) };
    }
      */
    #endregion
  }

  /// <summary>
  /// Фильтр табличного просмотра для StartsWithFilter
  /// </summary>
  public class StartsWithGridFilter : StringValueGridFilter // проще переопределить методы базового класса
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр табличного просмотра
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public StartsWithGridFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Повтор из StartsWithCommonFilter

    /// <summary>
    /// Создает StartsWithFilter
    /// </summary>
    /// <returns></returns>
    public override DBxFilter GetSqlFilter()
    {
      return new StartsWithFilter(ColumnName, Value);
    }

    /// <summary>
    /// Проверка значения
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей</param>
    /// <returns>True, если условие фильтра выполняется</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);
      return DataTools.GetString(v).StartsWith(Value);
    }

    /// <summary>
    /// Метод ничего не делает, в отличие от базового класса.
    /// </summary>
    /// <param name="docValue"></param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
    }

    #endregion
  }



  /// <summary>
  /// Фильтр по одному или нескольким значениям числового поля, каждому из
  /// которых соответствует текстовое представление
  /// </summary>
  public class EnumGridFilter : EnumCommonFilter, IEFPGridFilterWithImageKey
  {
    #region Конструктор

    /// <summary>
    /// Конструктор фильтра
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="textValues"></param>
    public EnumGridFilter(string columnName, string[] textValues)
      : base(columnName, textValues.Length)
    {
      _TextValues = textValues;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список текстовых строк, соответствующих перечислению
    /// </summary>
    public string[] TextValues { get { return _TextValues; } }
    private string[] _TextValues;

    /// <summary>
    /// Изображения, соответствующие перечислимым значениям
    /// </summary>
    public string[] ImageKeys
    {
      get { return _ImageKeys; }
      set
      {
#if DEBUG
        if (value != null)
        {
          if (value.Length != _TextValues.Length)
            throw new ArgumentException("Неправильная длина массива для свойства ImageKeys");
        }
#endif
        _ImageKeys = value;
      }
    }
    private string[] _ImageKeys;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (FilterFlags == null)
          return String.Empty;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < TextValues.Length; i++)
        {
          if (FilterFlags[i])
          {
            if (sb.Length > 0)
              sb.Append(", ");
            sb.Append(TextValues[i]);
          }
        }
        return sb.ToString();
      }
    }

    /// <summary>
    /// Если в списке выбрано ровно одно значение, то возвращается значок из ImageKeys.
    /// Иначе возвращается стандартное изображение фильтра
    /// </summary>
    public string FilterImageKey
    {
      get
      {
        if (ImageKeys == null)
          return EFPGridFilterTools.DefaultFilterImageKey;

        if (FilterFlags == null)
          return String.Empty;

        int singleIndex = -1;
        for (int i = 0; i < FilterFlags.Length; i++)
        {
          if (FilterFlags[i])
          {
            if (singleIndex >= 0) // больше одного флага
              return EFPGridFilterTools.DefaultFilterImageKey;
            else
              singleIndex = i;
          }
        }

        if (singleIndex >= 0)
          return ImageKeys[singleIndex];
        else
          return EFPGridFilterTools.DefaultFilterImageKey;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.ListTitle = "Выбрать значения фильтра";
      dlg.Items = TextValues;
      if (_ImageKeys != null)
        dlg.ImageKeys = ImageKeys;
      dlg.MultiSelect = true;
      if (FilterFlags != null)
        dlg.Selections = FilterFlags;
      dlg.CanBeEmpty = true;
      dlg.ConfigSectionName = "GridFilter." + this.Code;
      dlg.DialogPosition = dialogPosition;

      if (dlg.ShowDialog() != DialogResult.OK)
        return false;

      if (dlg.AreAllSelected || dlg.AreAllUnselected)
        FilterFlags = null;
      else
        FilterFlags = dlg.Selections;
      return true;
    }

    /// <summary>
    /// Возвращает одно из значений в массиве TextValues
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      int Value = DataTools.GetInt(columnValues[0]);
      string s;
      if (Value < 0 || Value >= TextValues.Length)
        s = "Значение " + Value.ToString();
      else
        s = TextValues[Value];
      return new string[] { s };
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// Базовый класс для построения фильтров по значению поля
  /// Активность фильтра устанавливается с помощью шаблона Nullable
  /// </summary>
  /// <typeparam name="T">Тип значения поля</typeparam>
  public abstract class NullableValueGridFilterBase<T> : GridFilter
    where T : struct
  {
  #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="ColumnName">Имя столбца</param>
    public NullableValueGridFilterBase(string ColumnName)
      : base(ColumnName)
    {
      DisplayName = ColumnName;

      FColumnName = ColumnName;

    }

  #endregion

  #region Свойства

    /// <summary>
    /// Имя фильтруемого поля типа String
    /// </summary>
    public string ColumnName { get { return FColumnName; } }
    private string FColumnName;

    /// <summary>
    /// Текущее значение фильтра
    /// </summary>
    public Nullable<T> Value
    {
      get { return FValue; }
      set
      {
        if (DataTools.AreValuesEqual(value, FValue))
          return;
        FValue = value;
        OnChanged();
      }
    }
    private Nullable<T> FValue;

  #endregion

  #region Переопределяемые свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public override string FilterText
    {
      get
      {
        if (Value.HasValue)
          return Value.Value.ToString();
        else
          return null;
      }
    }

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      Value = null;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !Value.HasValue;
      }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений
    /// Имена полей и значений должны содержать необходимые поля (которые можно
    /// получить методом GetColumnNames()), иначе будет сгенерирована ошибка
    /// </summary>
    /// <param name="ColumnNames">Имена полей</param>
    /// <param name="ColumnValues">Значения полей. Длина массива должна соответствовать <paramref name="ColumnNames"/>.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    public override bool TestValues(string[] ColumnNames, object[] ColumnValues)
    {
      if (!Value.HasValue)
        return true;
      object v = GetTestedValue(ColumnNames, ColumnValues, ColumnName);

      if (Value.Value.Equals(v))
        return true;

      // 11.08.2014
      // Если значение Value равно значению "по умолчанию" (0, false), то в фильтр
      // должны попадать записи со значением поля NULL
      if (Value.Value.Equals(default(T)))
      {
        if (v == null)
          return true;

        if (v is DBNull)
          return true;
      }

      return false;
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new ValueFilter(ColumnName, Value.Value, typeof(T));
    }

    /// <summary>
    /// Получить список имен полей, которые необходимы для вычисления фильтра.
    /// Поля добавляются в список независимо от того, активен сейчас фильтр или нет.
    /// </summary>
    /// <param name="List">Список для добавления полей</param>
    public override void GetColumnNames(DBxColumnList List)
    {
      List.Add(ColumnName);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля ColumnName, если фильтр установлен.
    /// </summary>
    /// <param name="NewDoc">Созданный документ, в котором можно установить поля</param>
    public override void InitNewDocValues(DBxSingleDoc NewDoc)
    {
      if (IsEmpty)
        return; // фильтр не установлен
      if (NewDoc.Values.IndexOf(ColumnName) >= 0)
        NewDoc.Values[ColumnName].SetValue(Value.Value);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="Config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart Config)
    {
      //#region Совместимость с предыдущими версиями

      //CfgPart Part = Config.GetChild(ColumnName, false);
      //if (Part != null)
      //{
      //  if (Part.GetBool("HasValue"))
      //    Value = DoReadConfigValue(Part);
      //  else
      //    Value = null;
      //  return;
      //}

      //#endregion

      if (!String.IsNullOrEmpty(Config.GetString("Value")))
        Value = DoReadConfigValue(Config);
      else
        Value = null;
    }

    /// <summary>
    /// Абстрактный метод, который должен прочитать параметр "Value" из секции конфигурации.
    /// Например, с помощью вызова return Part.GetInt("Value").
    /// </summary>
    /// <param name="Part">Секция конфигурации для чтения фильтра</param>
    /// <returns>Прочитанное значение</returns>
    protected abstract T DoReadConfigValue(CfgPart Part);

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="Config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart Config)
    {
      // 11.09.2012
      // Вложенная секция больше не используется и параметр HasValue тоже
      if (Value.HasValue)
        DoWriteConfigValue(Config, Value.Value);
      else
        Config.Remove("Value");
    }

    /// <summary>
    /// Абстрактный метод, который должен записывать параметр "Value" в секцию конфигурации.
    /// Например, с помощью вызова Part.SetInt("Value", Value).
    /// </summary>
    /// <param name="Part">Секция конфигурации для записи фильтра</param>
    /// <param name="Value">Записываемое значение</param>
    protected abstract void DoWriteConfigValue(CfgPart Part, T Value);

  #endregion
  }
#endif

  /// <summary>
  /// Простой фильтр по логическому полю.
  /// В обычном режиме возможен выбор из 3 вариантов: "Нет фильтра", "Значение установлено" и "Значение сброшено".
  /// Очистив одно из свойств FilterTextTrue или FilterTextFalse, можно использовать фильтр в упрощенном режиме на 2 положения,
  /// что может бытиь полезно в отчетах
  /// </summary>
  public class BoolValueGridFilter : BoolValueCommonFilter, IEFPGridFilterWithImageKey
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public BoolValueGridFilter(string columnName)
      : base(columnName)
    {
      _FilterTextTrue = "Значение установлено";
      _FilterTextFalse = "Значение сброшено";
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Текстовое описание фильтра по значению "True".
    /// По умолчанию - "Значение установлено".
    /// Установка свойства равным пустой строке переводит фильтр в упрощенный режим работы: возможен выбор из 2 вариантов "Нет фильтра" и "Значение сброшено".
    /// Одновременная очистка свойств FilterTextTrue и FilterTextFalse не допускается.
    /// </summary>
    public string FilterTextTrue
    {
      get { return _FilterTextTrue; }
      set
      {
        if (value == null)
          _FilterTextTrue = String.Empty;
        else
          _FilterTextTrue = value;
      }
    }
    private string _FilterTextTrue;

    /// <summary>
    /// Текстовое описание фильтра по значению "False".
    /// По умолчанию - "Значение сброшено"
    /// Установка свойства равным пустой строке переводит фильтр в упрощенный режим работы: возможен выбор из 2 вариантов "Нет фильтра" и "Значение установлено".
    /// Одновременная очистка свойств FilterTextTrue и FilterTextFalse не допускается.
    /// </summary>
    public string FilterTextFalse
    {
      get { return _FilterTextFalse; }
      set
      {
        if (value == null)
          _FilterTextFalse = String.Empty;
        else
          _FilterTextFalse = value;
      }
    }
    private string _FilterTextFalse;

    /// <summary>
    /// Значок для значения фильтра "True".
    /// Если свойство не установлено, используется стандартный значок фильтра
    /// </summary>
    public string FilterImageKeyTrue
    {
      get
      {
        if (String.IsNullOrEmpty(_FilterImageKeyTrue))
          return EFPGridFilterTools.DefaultFilterImageKey;
        else
          return _FilterImageKeyTrue;
      }
      set { _FilterImageKeyTrue = value; }
    }
    private string _FilterImageKeyTrue;

    /// <summary>
    /// Значок для значения фильтра "False".
    /// Если свойство не установлено, используется стандартный значок фильтра
    /// </summary>
    public string FilterImageKeyFalse 
    { 
      get 
      {
        if (String.IsNullOrEmpty(_FilterImageKeyFalse))
          return EFPGridFilterTools.DefaultFilterImageKey;
        else
          return _FilterImageKeyFalse; 
      } 
      set { _FilterImageKeyFalse = value; } 
    }
    private string _FilterImageKeyFalse;

    #endregion

    #region Переопределяемые свойства

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      if (String.IsNullOrEmpty(FilterTextTrue) && String.IsNullOrEmpty(FilterTextFalse))
        throw new NullReferenceException("Одновременная очистка свойств FilterTextTrue и FilterTextFalse не допускается.");

      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.GroupTitle = "Установка фильтра";

      if (String.IsNullOrEmpty(FilterTextTrue))
      {
        dlg.Items = new string[2] { "Нет фильтра", FilterTextFalse };
        dlg.ImageKeys = new string[] { EFPGridFilterTools.NoFilterImageKey, FilterImageKeyFalse };
        dlg.SelectedIndex = (Value ?? true) ? 0 : 1;
      }
      else if (String.IsNullOrEmpty(FilterTextFalse))
      {
        dlg.Items = new string[2] { "Нет фильтра", FilterTextTrue };
        dlg.ImageKeys = new string[] { EFPGridFilterTools.NoFilterImageKey, FilterImageKeyTrue };
        dlg.SelectedIndex = (Value ?? false) ? 1 : 0;
      }
      else
      {
        dlg.Items = new string[3] { "Нет фильтра", FilterTextTrue, FilterTextFalse };
        dlg.ImageKeys = new string[] { EFPGridFilterTools.NoFilterImageKey, FilterImageKeyTrue, FilterImageKeyFalse };
        if (Value.HasValue)
        {
          if (Value.Value)
            dlg.SelectedIndex = 1;
          else
            dlg.SelectedIndex = 2;
        }
        else
          dlg.SelectedIndex = 0;
      }

      dlg.DialogPosition = dialogPosition;

      if (dlg.ShowDialog() != DialogResult.OK)
        return false;

      if (String.IsNullOrEmpty(FilterTextTrue))
      {
        if (dlg.SelectedIndex == 0)
          Value = null;
        else
          Value = false;
      }
      else if (String.IsNullOrEmpty(FilterTextFalse))
      {
        if (dlg.SelectedIndex == 0)
          Value = null;
        else
          Value = true;
      }
      else
      {
        switch (dlg.SelectedIndex)
        {
          case 0:
            Value = null;
            break;
          case 1:
            Value = true;
            break;
          case 2:
            Value = false;
            break;
        }
      }

      return true;
    }

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (Value.HasValue)
        {
          if (Value.Value)
          {
            if (String.IsNullOrEmpty(FilterTextTrue))
              return "Ошибка. Значение TRUE недопустимо для этого фильтра";
            else
              return FilterTextTrue;
          }
          else
          {
            if (String.IsNullOrEmpty(FilterTextFalse))
              return "Ошибка. Значение FALSE недопустимо для этого фильтра";
            else
              return FilterTextFalse;
          }
        }
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает изображение для значка фильтра
    /// </summary>
    public string FilterImageKey
    {
      get
      {
        string imageKey = null;
        if (Value.HasValue)
          imageKey = Value.Value ? FilterImageKeyTrue : FilterImageKeyFalse;
        if (String.IsNullOrEmpty(imageKey))
          return EFPGridFilterTools.DefaultFilterImageKey;
        else
          return imageKey;
      }
    }

    /// <summary>
    /// Возвращает FilterTextTrue или FilterTextFales, если фильтр установлен
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      bool Value = DataTools.GetBool(columnValues[0]);
      return new string[] { Value ? FilterTextTrue : FilterTextFalse };
    }

    #endregion
  }

  /// <summary>
  /// Простой фильтр по полю типа Integer с фильтрацией по единственному значению
  /// Если поле может принимать фиксированный набор значений, то следует использовать
  /// фильтр EnumGridFilter
  /// </summary>
  public class IntValueGridFilter : IntValueCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public IntValueGridFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region Переопределяемые свойства

    /// <summary>
    /// Вызывает CfgPart.GetInt()
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <returns>Значение</returns>
    protected override int DoReadConfigValue(CfgPart cfg)
    {
      return cfg.GetInt("Value");
    }

    /// <summary>
    /// Вызывает CfgPart.SetBool()
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="value">Значение</param>
    protected override void DoWriteConfigValue(CfgPart cfg, int value)
    {
      cfg.SetInt("Value", value);
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      IntInputDialog dlg = new IntInputDialog();
      dlg.CanBeEmpty = true;
      dlg.NValue = Value;
      dlg.Title = DisplayName;
      dlg.Prompt = "Фильтр (пусто-нет фильтра)";
      dlg.DialogPosition = dialogPosition;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        Value = dlg.NValue;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        if (Value.HasValue)
          return Value.ToString();
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает текстовое представление для числа
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      int Value = DataTools.GetInt(columnValues[0]);
      return new string[] { Value.ToString() };
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по наличию или отсутствию значения NULL/NOT NULL (обычно, для поля
  /// типа "Дата")
  /// </summary>
  public class NullNotNullGridFilter : NullNotNullCommonFilter, IEFPGridFilterWithImageKey
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="columnType">Тип данных, который хранится в поле</param>
    public NullNotNullGridFilter(string columnName, Type columnType)
      : base(columnName, columnType)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текстовое описание фильтра для значения Value=NotNull
    /// По умолчанию - "Значение задано"
    /// </summary>
    public string FilterTextNotNull
    {
      get
      {
        if (String.IsNullOrEmpty(_FilterTextNotNull))
          return "Значение установлено";
        else
          return _FilterTextNotNull;
      }
      set { _FilterTextNotNull = value; }
    }
    private string _FilterTextNotNull;

    /// <summary>
    /// Текстовое описание фильтра для значения Value=Null
    /// По умолчанию - "Значение не задано"
    /// </summary>
    public string FilterTextNull
    {
      get
      {
        if (String.IsNullOrEmpty(_FilterTextNull))
          return "Значение не установлено";
        else
          return _FilterTextNull;
      }
      set { _FilterTextNull = value; }
    }
    private string _FilterTextNull;

    /// <summary>
    /// Значок для значения фильтра Value=NotNull.
    /// Если свойство не установлено, используется стандартный значок фильтра
    /// </summary>
    public string FilterImageKeyNotNull
    {
      get
      {
        if (String.IsNullOrEmpty(_FilterImageKeyNotNull))
          return EFPGridFilterTools.DefaultFilterImageKey;
        else
          return _FilterImageKeyNotNull;
      }
      set { _FilterImageKeyNotNull = value; }
    }
    private string _FilterImageKeyNotNull;

    /// <summary>
    /// Значок для значения фильтра Value=Null.
    /// Если свойство не установлено, используется стандартный значок фильтра
    /// </summary>
    public string FilterImageKeyNull
    {
      get
      {
        if (String.IsNullOrEmpty(_FilterImageKeyNull))
          return EFPGridFilterTools.DefaultFilterImageKey;
        else
          return _FilterImageKeyNull;
      }
      set { _FilterImageKeyNull = value; }
    }
    private string _FilterImageKeyNull;

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        switch (Value)
        {
          case NullNotNullFilterValue.Null:
            return FilterTextNull;
          case NullNotNullFilterValue.NotNull:
            return FilterTextNotNull;
          default:
            return String.Empty;
        }
      }
    }

    /// <summary>
    /// Значок для значения фильтра
    /// </summary>
    public string FilterImageKey
    {
      get
      {
        switch (Value)
        { 
          case NullNotNullFilterValue.Null:
            return FilterImageKeyNull;
          case NullNotNullFilterValue.NotNull:
            return FilterImageKeyNotNull;
          default:
            return String.Empty;
        }
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.GroupTitle = "Установка фильтра";
      dlg.Items = new string[3] { "Нет фильтра", FilterTextNotNull, FilterTextNull };
      dlg.ImageKeys = new string[3] { EFPGridFilterTools.NoFilterImageKey, FilterImageKeyNotNull, FilterImageKeyNull };
      dlg.SelectedIndex = (int)(Value);
      dlg.DialogPosition = dialogPosition;

      if (dlg.ShowDialog() != DialogResult.OK)
        return false;

      Value = (NullNotNullFilterValue)(dlg.SelectedIndex);

      return true;
    }

    #endregion
  }


  /// <summary>
  /// Фильтр по виду документа
  /// Текущим значением числового поля является идентификатор таблицы документа DocType.TableId
  /// </summary>
  public class DocTableIdGridFilter : DocTableIdCommonFilter, IEFPGridFilterWithImageKey
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="ui">Интерфейс пользователя для доступа к документам</param>
    /// <param name="columnName">Столбец типа Int32, хранящий идентификатор вида документа из таблицы DocTables</param>
    public DocTableIdGridFilter(DBUI ui, string columnName)
      : base(columnName)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");

      _UI = ui;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс пользователя для доступа к документам
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    /// <summary>
    /// Список типов документов, из которых осуществляется выбор.
    /// Если свойство не было установлено явно, то можно выбрать любой существующий тип документа
    /// Установка свойства никак не влияет на текущее значение
    /// </summary>
    public DocTypeUI[] DocTypeUIs
    {
      get
      {
        if (_DocTypeUIs == null)
        {
          string[] AllNames = UI.DocProvider.DocTypes.GetDocTypeNames();
          _DocTypeUIs = new DocTypeUI[AllNames.Length];
          for (int i = 0; i < AllNames.Length; i++)
            _DocTypeUIs[i] = UI.DocTypes[AllNames[i]];
        }
        return _DocTypeUIs;
      }
      set
      {
        if (value != null)
        {
          for (int i = 0; i < value.Length; i++)
          {
            if (value[i] == null)
              throw new ArgumentException("Элемент с индексом " + i.ToString() + " имеет значение null");
            if (value[i].UI != _UI)
              throw new ArgumentException("Элемент с индексом " + i.ToString() + " относится к другому объекту DBUI");
          }
        }
        _DocTypeUIs = value;
      }
    }
    private DocTypeUI[] _DocTypeUIs;

    /// <summary>
    /// Список видов типов документов, из которых осуществляется выбор.
    /// Если свойство не было установлено явно, то можно выбрать любой существующий тип документа
    /// Установка свойства никак не влияет на текущее значение
    /// </summary>
    public string[] DocTypeNames
    {
      get
      {
        string[] a = new string[DocTypeUIs.Length];
        for (int i = 0; i < DocTypeUIs.Length; i++)
          a[i] = DocTypeUIs[i].DocType.Name;
        return a;
      }
      set
      {
        if (value == null)
          DocTypeUIs = null;
        else
        {
          DocTypeUI[] a = new DocTypeUI[value.Length];
          for (int i = 0; i < value.Length; i++)
          {
            a[i] = UI.DocTypes[value[i]];
            if (a[i] == null)
              throw new ArgumentException("Неизвестный вид документа \"" + value[i] + "\"");
          }
          DocTypeUIs = a;
        }
      }
    }

    #endregion

    #region Текущий выбор - расширенные свойства

    /// <summary>
    /// Интерфейс текущего выбранного вида документов
    /// </summary>
    public DocTypeUI CurrentDocTypeUI
    {
      get
      {
        if (CurrentTableId == 0)
          return null;
        else
          return UI.DocTypes.FindByTableId(CurrentTableId);
      }
      set
      {
        if (value == null)
          CurrentTableId = 0;
        else
          CurrentTableId = value.DocType.TableId;
      }
    }

    /// <summary>
    /// Имя вида документа
    /// Альтернативное свойство для установки фильтра.
    /// </summary>
    public string CurrentDocTypeName
    {
      get
      {
        if (CurrentTableId == 0)
          return String.Empty;
        else
          return CurrentDocTypeUI.DocType.Name;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          CurrentTableId = 0;
        else
          CurrentDocTypeUI = UI.DocTypes[value];
      }
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public string FilterText
    {
      get
      {
        if (CurrentTableId == 0)
          return string.Empty;
        else
        {
          if (CurrentDocTypeUI == null)
            return "Неизвестный тип документа TableId=" + CurrentTableId.ToString();
          else
          {
            string s = CurrentDocTypeUI.DocType.PluralTitle;
            if (UI.DebugShowIds)
              s += " (TableId=" + CurrentTableId.ToString() + ")";
            return s;
          }
        }
      }
    }

    /// <summary>
    /// Возвращает значок вида документов
    /// </summary>
    public string FilterImageKey
    {
      get
      {
        if (CurrentDocTypeUI != null)
          return CurrentDocTypeUI.ImageKey;
        else
          return String.Empty;
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.Items = new string[DocTypeUIs.Length + 1];
      dlg.Items[0] = "Нет фильтра";
      dlg.ImageKeys[0] = EFPGridFilterTools.NoFilterImageKey;
      if (CurrentTableId == 0)
        dlg.SelectedIndex = 0;
      for (int i = 0; i < DocTypeUIs.Length; i++)
      {
        dlg.Items[i + 1] = DocTypeUIs[i].DocType.PluralTitle;
        if (UI.DebugShowIds)
          dlg.Items[i + 1] += " (TableId=" + DocTypeUIs[i].DocType.TableId.ToString() + ")";
        dlg.ImageKeys[i + 1] = DocTypeUIs[i].ImageKey;
        if (CurrentTableId == DocTypeUIs[i].DocType.TableId)
          dlg.SelectedIndex = i + 1;
      }
      dlg.ConfigSectionName = "GridFilter." + this.Code;
      dlg.DialogPosition = dialogPosition;
      if (dlg.ShowDialog() != DialogResult.OK)
        return false;
      if (dlg.SelectedIndex == 0)
        CurrentTableId = 0;
      else
        CurrentTableId = DocTypeUIs[dlg.SelectedIndex - 1].DocType.TableId; // 22.12.2020
      return true;
    }

    /// <summary>
    /// Возвращает свойство DBxDocType.PluralTitle, если задан фильтр по виду документов.
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      string s;
      Int32 ThisTableId = DataTools.GetInt(columnValues[0]);
      if (ThisTableId == 0)
        s = "Нет";
      else
      {
        DBxDocType DocType = UI.DocProvider.DocTypes.FindByTableId(ThisTableId);
        if (DocType == null)
          s = "Неизвестный тип документа с TableId=" + ThisTableId.ToString();
        else
        {
          s = DocType.PluralTitle;
          if (UI.DebugShowIds)
            s += " (TableId=" + DocType.TableId.ToString() + ")";
        }
      }
      return new string[] { s };
    }

    #endregion
  }

  /// <summary>
  /// Фиктивный фильтр, который, если установлен, не пропускает ни одной строки
  /// </summary>
  public class DummyGridFilter : DummyCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="code">Условный код фильтра</param>
    public DummyGridFilter(string code)
      : base(code)
    {
      _FilterTextFalse = "Нет";
    }

    #endregion

    #region Свойства


    /// <summary>
    /// Текстовое описание фильтра по значению "False".
    /// По умолчанию - "Нет"
    /// </summary>
    public string FilterTextFalse
    {
      get { return _FilterTextFalse; }
      set
      {
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        _FilterTextFalse = value;
      }
    }
    private string _FilterTextFalse;

    #endregion

    #region IEFPGridFilter Members

    /// <summary>
    /// Текстовое представление
    /// </summary>
    public string FilterText
    {
      get
      {
        if (IsTrue)
          return string.Empty;
        else
          return FilterTextFalse;
      }
    }

    /// <summary>
    /// Диалог для установки фильтра
    /// </summary>
    /// <returns></returns>
    public bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.Items = new string[] { "Нет фильтра", FilterTextFalse };
      dlg.SelectedIndex = IsTrue ? 0 : 1;
      dlg.DialogPosition = dialogPosition;
      if (dlg.ShowDialog() != DialogResult.OK)
        return false;

      IsTrue = (dlg.SelectedIndex == 0);
      return true;
    }

    #endregion
  }


  /// <summary>
  /// Фильтр с фиксированным SQL-запросом.
  /// </summary>
  public class FixedSqlGridFilter : FixedSqlCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="code">Код для фильтра</param>
    /// <param name="filter">SQL-фильтр. Обязательно должен быть задан</param>
    public FixedSqlGridFilter(string code, DBxFilter filter)
      : base(code, filter)
    {
    }

    #endregion

    #region IEFPGridFilter Members

    /// <summary>
    /// Текстовое представление для фильтра.
    /// Если не установлено в явном виде, то возвращается SQL-запрос
    /// </summary>
    public string FilterText
    {
      get
      {
        if (String.IsNullOrEmpty(_FilterText))
          return Filter.ToString();
        else
          return _FilterText;
      }
      set { _FilterText = value; }
    }
    private string _FilterText;

    /// <summary>
    /// Выводит сообщение о невозможности редактирования
    /// </summary>
    /// <returns></returns>
    public bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      EFPApp.ErrorMessageBox("Редактирование этого фильтра невозможно", DisplayName);
      return false;
    }

    #endregion
  }

}
