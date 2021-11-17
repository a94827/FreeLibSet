using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;

using FreeLibSet.Config;
using FreeLibSet.DependedValues;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;
using FreeLibSet.UICore;

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
  /// Форма для фильтра по кодам
  /// </summary>
  internal partial class CodeGridFilterForm : Form
  {
    #region Конструктор

    public CodeGridFilterForm(string[] codes, string[] names, EFPCodeValidatingEventHandler codeValidating)
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      Icon = EFPApp.MainImageIcon("Filter");

      efpMode = new EFPRadioButtons(efpForm, rbNoFilter);

      efpCodes = new EFPCsvCodesComboBox(efpForm, edCodes, codes);
      efpCodes.Names = names;
      efpCodes.ToolTipText = "Список кодов, разделенных запятыми";
      efpCodes.CanBeEmpty = true;
      efpCodes.CodeValidating += codeValidating;
      efpCodes.UnknownCodeSeverity = UIValidateState.Warning;

      efpEmpty = new EFPCheckBox(efpForm, cbEmpty);
      efpEmpty.ToolTipText = "В режиме \"Включить коды\" - включить в фильтр строки, в которых поле не установлено." + Environment.NewLine +
      "В режиме \"Исключить коды\" - убрать строки с пустым значением поля";

      efpCodes.EnabledEx = new DepExpr1<bool, int>(efpMode.SelectedIndexEx, new DepFunction1<bool, int>(CalcCodesEnabled));
      efpCodes.Validators.AddError(efpCodes.IsNotEmptyEx, "Коды должны быть выбраны", new DepNot(efpEmpty.CheckedEx));
      efpEmpty.EnabledEx = efpCodes.EnabledEx;
      efpEmpty.CheckedEx.ValueChanged += efpCodes.Validate;
    }

    private static bool CalcCodesEnabled(int arg)
    {
      return arg == 1 || arg == 2;
    }

    #endregion

    #region Поля

    public EFPRadioButtons efpMode;

    public EFPCsvCodesComboBox efpCodes;

    public EFPCheckBox efpEmpty;

    #endregion
  }

  /// <summary>
  /// Абстрактный класс для реализации фильтров по кодам в табличном просмотре.
  /// Поддерживает режим включения или исключения нескольких кодов. Возможна 
  /// поддержка пустых кодов. 
  /// Является базой для класса RefBookGridFilter для фильтрации по кодам "Код-значение"
  /// </summary>
  public abstract class CodeGridFilter : CodeCommonFilter, IEFPGridFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="canBeEmpty">true, если поддерживаются пустые коды</param>
    public CodeGridFilter(string columnName, bool canBeEmpty)
      : base(columnName, canBeEmpty)
    {
    }

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Текстовое представление фильтра в правой части таблички фильтров. 
    /// Пустая строка означает отсутствие фильтра (IsEmpty=true).
    /// </summary>
    public virtual string FilterText
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        switch (Mode)
        {
          case CodeFilterMode.Include:
            break;
          case CodeFilterMode.Exclude:
            sb.Append("Кроме: ");
            break;
          default:
            return String.Empty;
        }
        sb.Append(String.Join(", ", Codes));
        if (EmptyCode)
        {
          if (Codes.Length > 0)
            sb.Append(", ");
          sb.Append("[ Нет ]");
        }
        return sb.ToString();
      }
    }

    /// <summary>
    /// Показывает блок диалога для редактирования фильтра
    /// </summary>
    /// <returns>True, если пользователь установил фильтр</returns>
    public virtual bool ShowFilterDialog(EFPDialogPosition dialogPosition)
    {
      string[] codes, names;
      GetCodesAndNames(out codes, out names);
      CodeGridFilterForm Form = new CodeGridFilterForm(codes, names, new EFPCodeValidatingEventHandler(CodeValidating));
      Form.Text = DisplayName;
      Form.efpEmpty.Visible = CanBeEmpty;

      Form.efpMode.SelectedIndex = (int)(Mode);
      Form.efpCodes.SelectedCodes = Codes;
      Form.efpEmpty.Checked = EmptyCode;

      if (EFPApp.ShowDialog(Form, true, dialogPosition) != DialogResult.OK)
        return false;

      SetFilter((CodeFilterMode)(Form.efpMode.SelectedIndex),
        Form.efpCodes.SelectedCodes,
        Form.efpEmpty.Checked);
      return true;
    }

    private void CodeValidating(object sender, EFPCodeValidatingEventArgs args)
    { 
      string msg;
      if (!CheckCode(args.Code, out msg))
        args.SetError(msg);
    }

    #endregion

    #region Абстрактные методы

#if XXX
    /// <summary>
    /// Показать диалог выбора кодов из справочника
    /// </summary>
    /// <param name="codes">Вход-выход: Выбранные коды</param>
    /// <param name="dialogPosition">Позиция для выпадающего списка</param>
    /// <returns>true, если выбор сделан</returns>
    public virtual bool ShowSelectCodesDialog(ref string[] codes, EFPDialogPosition dialogPosition)
    {
      string[] aCodes;
      string[] aNames;

      GetCodesAndNames(out aCodes, out aNames);
#if DEBUG
      if (aNames != null)
      {
        if (aNames.Length != aCodes.Length)
          throw new InvalidOperationException("Не совпадает длина массивов кодов и значений");
      }
#endif
      if (/*aCodes == null || */ aCodes.Length == 0)
      {
        EFPApp.MessageBox("Нет ни одного кода в справочнике", DisplayName,
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }

      ListSelectDialog dlg = new ListSelectDialog();
      dlg.DialogPosition = dialogPosition;
      dlg.Title = DisplayName;
      dlg.ImageKey = ImageKey;
      dlg.Items = new string[aCodes.Length];
      for (int i = 0; i < aCodes.Length; i++)
      {
        if (aNames == null)
          dlg.Items[i] = aCodes[i];
        else
          dlg.Items[i] = aCodes[i] + " " + aNames[i];
      }
      dlg.ListTitle = "Выбранные коды";
      dlg.MultiSelect = true;
      for (int i = 0; i < codes.Length; i++)
      {
        int p = Array.IndexOf<string>(aCodes, codes[i]);
        if (p >= 0)
          dlg.Selections[p] = true;
      }
      dlg.ConfigSectionName = "GridFilter." + this.Code;

      if (dlg.ShowDialog() != DialogResult.OK)
        return false;
      List<string> lst = new List<string>();
      for (int i = 0; i < aCodes.Length; i++)
      {
        if (dlg.Selections[i])
          lst.Add(aCodes[i]);
      }
      codes = lst.ToArray();
      return true;
    }
#endif


    /// <summary>
    /// Получить массив доступных кодов и значений.
    /// Метод должен возвращать массивы <paramref name="codes"/> и <paramref name="names"/> одинаковой длины.
    /// Вместо <paramref name="names"/> может возвращаться null, если есть коды без названий
    /// </summary>
    /// <param name="codes">Сюда помещаются коды</param>
    /// <param name="names">Сюда помещаются названия</param>
    public abstract void GetCodesAndNames(out string[] codes, out string[] names);

    /// <summary>
    /// Значок для отображения в списке кодов
    /// </summary>
    public virtual string ImageKey { get { return "Item"; } }

    /// <summary>
    /// Проверить код. Метод должен вернуть true, если формат кода является допустимым.
    /// Иначе в Message должно быть описание ошибки. Если метод возвращает true,
    /// но при этом есть описание ошибки, то будет выдаваться предупреждение
    /// </summary>
    /// <param name="code">Проверяемый код</param>
    /// <param name="message">Сообщение об ошибке или предупреждении</param>
    /// <returns>true, если нет ошибки (но возможно наличие предупреждения)</returns>
    public abstract bool CheckCode(string code, out string message);

    #endregion
  }
}