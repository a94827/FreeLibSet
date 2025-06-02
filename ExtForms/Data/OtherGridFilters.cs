// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Data;
using FreeLibSet.Config;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Data
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
      _UseManualInput = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Необязательный список строк, из которого осуществляется выбор.
    /// По умолчанию - null - выбор осуществлятся путем ручного ввода значения.
    /// Если список задан, то значение выбирается из списка.
    /// Порядок строк в списке соответствует порядку строк при просмотре, поэтому
    /// сортирока строк, если необходима, должна выполняться до установки свойства.
    /// Свойство используется только при показе диалога фильтра. Перед этим 
    /// вызывается событие <see cref="AllValuesNeeded"/>, которое может использоваться для
    /// динамической установки списка.
    /// Значения null и <see cref="DataTools.EmptyStrings"/> отличаются. Во втором случае показывается пустой диалог выбора из списка
    /// </summary>
    public string[] AllValues { get { return _AllValues; } set { _AllValues = value; } }
    private string[] _AllValues;

    /// <summary>
    /// Событие вызывается перед показом диалога фильтра в <see cref="ShowFilterDialog(EFPDialogPosition)"/>.
    /// Обработчик может установить свойство <see cref="AllValues"/>.
    /// </summary>
    public event EventHandler AllValuesNeeded;

    /// <summary>
    /// Разрешен ли ручной ввод значения. По умолчанию - true. Если при этом задан непустой список <see cref="AllValues"/>,
    /// то в диалоге выбора значения из списка добавляется позиция "Ввести вручную...".
    /// Если false, то допускается только выбор значения из списка.
    /// </summary>
    public bool UseManualInput { get { return _UseManualInput; }set { _UseManualInput=value; } }
    private bool _UseManualInput;

    #endregion

    #region Методы IEFPGridFilter

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
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
    /// <param name="dialogPosition">Передается блоку диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      if (AllValuesNeeded != null)
      {
        try
        {
          AllValuesNeeded(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, String.Format(Res.StringValueGridFilter_ErrTitle_AllValuesNeeded, DisplayName));
        }
      }

      if (AllValues != null /*&& ValueList.Length > 0*/)
      {
        int n = AllValues.Length + 1; // для "Нет фильтра"
        if (UseManualInput)
          n++;
        string[] list2 = new string[n];
        string[] imageKeys = new string[n];
        list2[0] = Res.StringValueGridFilter_Msg_NoFilter;
        imageKeys[0] = EFPGridFilterTools.NoFilterImageKey;
        for (int i = 0; i < AllValues.Length; i++)
        {
          list2[i + 1] = AllValues[i];
          imageKeys[i + 1] = "Filter";
        }
        if (UseManualInput)
        {
          list2[AllValues.Length + 1] = Res.StringValueGridFilter_Msg_Manual;
          imageKeys[AllValues.Length + 1] = "Edit";
        }

        ListSelectDialog dlg1 = new ListSelectDialog();
        dlg1.Title = DisplayName;
        dlg1.ListTitle = Res.GridFilter_Msg_ListTitle;
        dlg1.OutItemTitle = DisplayName;
        dlg1.ImageKey = "Filter";
        dlg1.MultiSelect = false;
        dlg1.Items = list2;
        dlg1.ImageKeys = imageKeys;
        dlg1.ConfigSectionName = "GridFilter." + this.Code;
        dlg1.DialogPosition = dialogPosition;

        if (String.IsNullOrEmpty(Value))
          dlg1.SelectedIndex = 0;
        else
        {
          int p = Array.IndexOf<string>(AllValues, Value);
          if (p >= 0)
            dlg1.SelectedIndex = p + 1;
          else
            dlg1.SelectedIndex = AllValues.Length + 1;
        }

        if (dlg1.ShowDialog() != DialogResult.OK)
          return false;

        if (dlg1.SelectedIndex == 0)
        {
          Value = String.Empty;
          return true;
        }

        if (dlg1.SelectedIndex <= AllValues.Length)
        {
          Value = AllValues[dlg1.SelectedIndex - 1];
          return true;
        }
        // Иначе выполняется ручной ввод
      }

      // Ручной ввод
      if (UseManualInput)
      {
        TextInputDialog dlg2 = new TextInputDialog();
        dlg2.Title = DisplayName;
        dlg2.ImageKey = "Filter";
        dlg2.Prompt = Res.GridFilter_Msg_ManualInputPromptWithEmptyIfNoFilter;
        dlg2.CanBeEmpty = true;
        dlg2.Text = Value;
        dlg2.DialogPosition = dialogPosition;

        if (dlg2.ShowDialog() != DialogResult.OK)
          return false;
        Value = dlg2.Text;

        return true;
      }
      else
      {
        EFPApp.ErrorMessageBox(Res.StringValueGridFilter_Err_NoManual);
        return false;
      }
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
  /// Фильтр табличного просмотра для <see cref="StartsWithFilter"/>
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
    /// Создает <see cref="StartsWithFilter"/>
    /// </summary>
    /// <returns>Объект фильтра</returns>
    public override DBxFilter GetSqlFilter()
    {
      return new StartsWithFilter(ColumnName, Value, IgnoreCase);
    }

    /// <summary>
    /// Проверка значения
    /// </summary>
    /// <param name="rowValues">Доступ к значениям полей</param>
    /// <returns>True, если условие фильтра выполняется</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);
      return DataTools.GetString(v).StartsWith(Value, IgnoreCase?StringComparison.OrdinalIgnoreCase: StringComparison.Ordinal);
    }

    /// <summary>
    /// Метод ничего не делает, в отличие от базового класса.
    /// </summary>
    /// <param name="docValue"></param>
    protected override void OnInitNewValue(DBxExtValue docValue)
    {
    }

    #endregion
  }

  /// <summary>
  /// Фильтр IN по строковому полю.
  /// Допускается наличие пустой строки в списке значений. В этом случае фильтр пройдут строки, содержащие значение NULL.
  /// </summary>
  public class StringValuesGridFilter : StringValuesCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    public StringValuesGridFilter(string columnName)
      : base(columnName)
    {
      _EmptyValueText = Res.GridFilter_Msg_EmptyValueText;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список строк, из которого осуществляется выбор.
    /// Порядок строк в списке соответствует порядку строк при просмотре, поэтому
    /// сортирока строк, если необходима, должна выполняться до установки свойства.
    /// Свойство используется только при показе диалога фильтра. Перед этим 
    /// вызывается событие <see cref="AllValuesNeeded"/>, которое может использоваться для
    /// динамической установки списка.
    /// Может содержать пустую строку, тогда пользователь может выполнять фильтрацию по значениям "" и NULL.
    /// </summary>
    public string[] AllValues { get { return _AllValues; } set { _AllValues = value; } }
    private string[] _AllValues;

    /// <summary>
    /// Событие вызывается перед показом диалога фильтра в <see cref="ShowFilterDialog(EFPDialogPosition)"/>.
    /// Обработчик может установить свойство <see cref="AllValues"/>.
    /// </summary>
    public event EventHandler AllValuesNeeded;

    /// <summary>
    /// Текст в диалоге выбора строк, соответствующий пустой строке.
    /// По умолчанию - "[ нет ]".
    /// Используется, если в массиве <see cref="AllValues"/> есть пустая строка.
    /// </summary>
    public string EmptyValueText
    {
      get { return _EmptyValueText; }
      set { _EmptyValueText = value; }
    }
    private string _EmptyValueText;

    #endregion

    #region IEFPGridFilter

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра.
    /// </summary>
    public string FilterText
    {
      get
      {
        if (Values == null)
          return String.Empty;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Values.Length; i++)
        {
          if (i > 0)
            sb.Append(", ");
          if (String.IsNullOrEmpty(Values[i]) && (!String.IsNullOrEmpty(EmptyValueText)))
            sb.Append(EmptyValueText); // без кавычек
          else
          {
            sb.Append('\"');
            sb.Append(Values[i]);
            sb.Append('\"');
          }
        }
        return sb.ToString();
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <param name="dialogPosition">Передается блоку диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      if (AllValuesNeeded != null)
      {
        try
        {
          AllValuesNeeded(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, String.Format(Res.StringValueGridFilter_ErrTitle_AllValuesNeeded, DisplayName));
        }
      }

      ListSelectDialog dlg = new ListSelectDialog();
      dlg.MultiSelect = true;
      dlg.Title = DisplayName;
      dlg.ListTitle = Res.GridFilter_Msg_ListTitle;
      dlg.OutItemTitle = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.DialogPosition = dialogPosition;
      if (AllValues == null)
        dlg.Items = DataTools.EmptyStrings;
      else
        dlg.Items = AllValues;
      int emptyStringPos = Array.IndexOf<string>(AllValues, String.Empty);
      if (emptyStringPos >= 0)
        dlg.Items = (string[])dlg.Items.Clone();
      if (Values!=null)
        dlg.SetSelectedItems(Values);
      if (emptyStringPos >= 0)
        dlg.Items[emptyStringPos] = EmptyValueText;

        dlg.CanBeEmpty = true;
      if (dlg.ShowDialog() != DialogResult.OK)
        return false;
      if (dlg.AreAllUnselected || dlg.AreAllSelected)
        Values = null;
      else
      {
        if (emptyStringPos >= 0)
          dlg.Items[emptyStringPos] = String.Empty;
        Values = dlg.GetSelectedItems();
      }

      return true;
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по одному или нескольким значениям числового поля, каждому из
  /// которых соответствует текстовое представление.
  /// Поддерживаются только простые перечисления со значениями 0,1,2,...
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
    private readonly string[] _TextValues;

    /// <summary>
    /// Изображения, соответствующие перечислимым значениям.
    /// Длина массива должна соответствовать <see cref="TextValues"/>.
    /// По умолчанию null - отдельные изображения не используются.
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
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, _TextValues.Length);
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
    /// Пустая строка означает отсутствие фильтра.
    /// Если выбрано несколько значений, возвращаются элементы <see cref="TextValues"/>, разделенные запятыми.
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
    /// Если в списке выбрано ровно одно значение, то возвращается значок из <see cref="ImageKeys"/>, если свойство установлено.
    /// Иначе возвращается стандартное изображение фильтра.
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
    /// Показывает блок диалога для редактирования фильтра.
    /// </summary>
    /// <param name="dialogPosition">Передается блоку диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.ListTitle = Res.GridFilter_Msg_ListTitle;
      dlg.OutItemTitle = DisplayName;
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
    /// Возвращает одно из значений в массиве <see cref="TextValues"/>
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      int value = DataTools.GetInt(columnValues[0]);
      string s;
      if (value < 0 || value >= TextValues.Length)
        s = "Value # " + value.ToString();
      else
        s = TextValues[value];
      return new string[] { s };
    }

    #endregion
  }

  /// <summary>
  /// Простой фильтр по логическому полю.
  /// В обычном режиме возможен выбор из 3 вариантов: "Нет фильтра", "Значение установлено" и "Значение сброшено".
  /// Очистив одно из свойств <see cref="BoolValueGridFilter.FilterTextTrue"/> или <see cref="BoolValueGridFilter.FilterTextFalse"/>, можно использовать фильтр в упрощенном режиме на 2 положения,
  /// что может быть полезно в отчетах.
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
      _FilterTextTrue = Res.BoolValueGridFilter_Msg_FilterTextTrue;
      _FilterTextFalse = Res.BoolValueGridFilter_Msg_FilterTextFalse;
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Текстовое описание фильтра по значению "True".
    /// По умолчанию - "Значение установлено".
    /// Установка свойства равным пустой строке переводит фильтр в упрощенный режим работы: возможен выбор из 2 вариантов "Нет фильтра" и "Значение сброшено".
    /// Одновременная очистка свойств <see cref="FilterTextTrue"/> и <see cref="FilterTextFalse"/> не допускается.
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
    /// Одновременная очистка свойств <see cref="FilterTextTrue"/> и <see cref="FilterTextFalse"/> не допускается.
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
    /// Если свойство не установлено, используется стандартный значок фильтра.
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
    /// Если свойство не установлено, используется стандартный значок фильтра.
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
    /// <param name="dialogPosition">Передается блоку диалогу</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      if (String.IsNullOrEmpty(FilterTextTrue) && String.IsNullOrEmpty(FilterTextFalse))
        throw new NullReferenceException(Res.BoolValueGridFilter_Err_NoText);

      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.GroupTitle = Res.GridFilter_Msg_ListTitle;

      if (String.IsNullOrEmpty(FilterTextTrue))
      {
        dlg.Items = new string[2] { Res.GridFilter_Msg_NoFilterMode, FilterTextFalse };
        dlg.ImageKeys = new string[] { EFPGridFilterTools.NoFilterImageKey, FilterImageKeyFalse };
        dlg.SelectedIndex = (Value ?? true) ? 0 : 1;
      }
      else if (String.IsNullOrEmpty(FilterTextFalse))
      {
        dlg.Items = new string[2] { Res.GridFilter_Msg_NoFilterMode, FilterTextTrue };
        dlg.ImageKeys = new string[] { EFPGridFilterTools.NoFilterImageKey, FilterImageKeyTrue };
        dlg.SelectedIndex = (Value ?? false) ? 1 : 0;
      }
      else
      {
        dlg.Items = new string[3] { Res.GridFilter_Msg_NoFilterMode, FilterTextTrue, FilterTextFalse };
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
    /// Пустая строка означает отсутствие фильтра.
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
              return String.Format(Res.GridFilter_Err_WrongValue, "TRUE");
            else
              return FilterTextTrue;
          }
          else
          {
            if (String.IsNullOrEmpty(FilterTextFalse))
              return String.Format(Res.GridFilter_Err_WrongValue, "FALSE");
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
    /// Возвращает <see cref="FilterTextTrue"/> или <see cref="FilterTextFalse"/>, если фильтр установлен.
    /// </summary>
    /// <param name="columnValues">Значения полей</param>
    /// <returns>Текстовые представления значений</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      bool value = DataTools.GetBool(columnValues[0]);
      return new string[] { value ? FilterTextTrue : FilterTextFalse };
    }

    #endregion
  }

  /// <summary>
  /// Простой фильтр по полю типа Integer с фильтрацией по единственному значению.
  /// Если поле может принимать фиксированный набор значений, то следует использовать
  /// фильтр <see cref="EnumGridFilter"/>.
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
    /// Чтение настройки фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <returns>Значение</returns>
    protected override int DoReadConfigValue(CfgPart cfg)
    {
      return cfg.GetInt("Value");
    }

    /// <summary>
    /// Запись настройки фильтра в секцию конфигурации
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
    /// <param name="dialogPosition">Передается блоку диалога</param>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      IntInputDialog dlg = new IntInputDialog();
      dlg.CanBeEmpty = true;
      dlg.NValue = Value;
      dlg.Title = DisplayName;
      dlg.Prompt = Res.GridFilter_Msg_ManualInputPromptWithEmptyIfNoFilter;
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
    /// Пустая строка означает отсутствие фильтра.
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
      int value = DataTools.GetInt(columnValues[0]);
      return new string[] { value.ToString() };
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
    /// Текстовое описание фильтра для значения <see cref="NullNotNullCommonFilter.Value"/>=<see cref="NullNotNullFilterValue.NotNull"/>.
    /// По умолчанию - "Значение установлено".
    /// </summary>
    public string FilterTextNotNull
    {
      get
      {
        if (String.IsNullOrEmpty(_FilterTextNotNull))
          return Res.NullNotNullGridFilter_MsgNotNull;
        else
          return _FilterTextNotNull;
      }
      set { _FilterTextNotNull = value; }
    }
    private string _FilterTextNotNull;

    /// <summary>
    /// Текстовое описание фильтра для значения <see cref="NullNotNullCommonFilter.Value"/>=<see cref="NullNotNullFilterValue.Null"/>.
    /// По умолчанию - "Значение не установлено".
    /// </summary>
    public string FilterTextNull
    {
      get
      {
        if (String.IsNullOrEmpty(_FilterTextNull))
          return Res.NullNotNullGridFilter_MsgNull;
        else
          return _FilterTextNull;
      }
      set { _FilterTextNull = value; }
    }
    private string _FilterTextNull;

    /// <summary>
    /// Значок для значения фильтра <see cref="NullNotNullCommonFilter.Value"/>=<see cref="NullNotNullFilterValue.NotNull"/>.
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
    /// Значок для значения фильтра <see cref="NullNotNullCommonFilter.Value"/>=<see cref="NullNotNullFilterValue.Null"/>.
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
    /// Пустая строка означает отсутствие фильтра.
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
      dlg.GroupTitle = Res.GridFilter_Msg_ListTitle;
      dlg.Items = new string[3] { Res.GridFilter_Msg_NoFilterMode, FilterTextNotNull, FilterTextNull };
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
      _FilterTextFalse = Res.GridFilter_Msg_EmptyValueText;
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
          throw ExceptionFactory.ArgStringIsNullOrEmpty("value");
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
    /// <param name="dialogPosition">Передается в блок диалога</param>
    /// <returns>true, если установка фильтра выполнена</returns>
    public bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = DisplayName;
      dlg.ImageKey = "Filter";
      dlg.GroupTitle = Res.GridFilter_Msg_ListTitle;
      dlg.Items = new string[] { Res.GridFilter_Msg_NoFilterMode, FilterTextFalse };
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
    /// <param name="dialogPosition">Не используется</param>
    /// <returns>false</returns>
    public bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      EFPApp.ErrorMessageBox(Res.GridFilter_Err_NotEditable, DisplayName);
      return false;
    }

    #endregion
  }
}
