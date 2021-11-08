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

    public CodeGridFilterForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      Icon = EFPApp.MainImageIcon("Filter");

      efpMode = new EFPRadioButtons(efpForm, rbNoFilter);

      efpCodes = new EFPUserTextComboBox(efpForm, edCodes);
      efpCodes.ToolTipText = "Список кодов, разделенных запятыми";

      efpEmpty = new EFPCheckBox(efpForm, cbEmpty);
      efpEmpty.ToolTipText = "В режиме \"Включить коды\" - включить в фильтр строки, в которых поле не установлено." +Environment.NewLine+
      "В режиме \"Исключить коды\" - убрать строки с пустым значением поля";

      efpCodes.EnabledEx = new DepExpr1<bool, int>(efpMode.SelectedIndexEx, new DepFunction1<bool, int>(CalcCodesEnabled));
      efpEmpty.EnabledEx = efpCodes.EnabledEx;

      efpCodes.CanBeEmpty = true;
      efpCodes.Validating += new UIValidatingEventHandler(efpCodes_Validating);
      edCodes.PopupClick += new EventHandler(edCodes_PopupClick);
      edCodes.ClearButton = true;
      edCodes.ClearClick += new EventHandler(edCodes_ClearClick);
      efpEmpty.CheckedEx.ValueChanged += efpCodes.Validate;
    }

    private static bool CalcCodesEnabled(int arg)
    {
      return arg == 1 || arg == 2;
    }

    #endregion

    #region Поля

    public CodeGridFilter TheFilter;

    public EFPRadioButtons efpMode;

    public EFPUserTextComboBox efpCodes;

    public EFPCheckBox efpEmpty;

    #endregion

    #region Поле ввода кодов

    void efpCodes_Validating(object sender, UIValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return; // пустое поле
      if (String.IsNullOrEmpty(efpCodes.Text))
      {
        if (!efpEmpty.Checked)
          args.SetError("Список должен быть заполнен");
        return;
      }
      string[] a = efpCodes.Text.Split(',');
      for (int i = 0; i < a.Length; i++)
      {
        string s1 = a[i].Trim();
        if (String.IsNullOrEmpty(s1))
        {
          if (efpEmpty.Visible)
            args.SetError("Ввод пустых кодов не допускается. Используйте флажок внизу");
          else
            args.SetError("Пустые коды не допускаются");
          return;
        }

        for (int j = 0; j < i; j++)
        {
          string s2 = a[j].Trim();
          if (s2 == s1)
          {
            args.SetError("Одинаковые коды \"" + s1 + "\" в позициях " + (j + 1).ToString() + " и " + (i + 1).ToString());
            return;
          }
        }

        string Msg;
        bool Res = TheFilter.CheckCode(s1, out Msg);
        if (!Res)
        {
          if (String.IsNullOrEmpty(Msg))
            Msg = "Неизвестная ошибка. Обратитесь к разработчику программы";
          args.SetError("Ошибка в позиции №" + (i + 1).ToString() + " (" + s1 + "): " + Msg);
          return;
        }
        if (!String.IsNullOrEmpty(Msg))
        {
          if (args.ValidateState == UIValidateState.Ok)
            args.SetWarning("Позиция №" + (i + 1).ToString() + " (" + s1 + "): " + Msg);
        }
      }
    }

    void edCodes_PopupClick(object sender, EventArgs args)
    {
      string[] Codes;
      if (String.IsNullOrEmpty(efpCodes.Text))
        Codes = DataTools.EmptyStrings;
      else
      {
        Codes = efpCodes.Text.Split(',');
        for (int i = 0; i < Codes.Length; i++)
          Codes[i] = Codes[i].Trim();
      }
      if (!TheFilter.ShowSelectCodesDialog(ref Codes))
        return;
      efpCodes.Text = String.Join(",", Codes).Replace(",", ", ");
    }

    void edCodes_ClearClick(object Sender, EventArgs Args)
    {
      efpCodes.Text = String.Empty;
    }

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
          case CodesFilterMode.Include:
            break;
          case CodesFilterMode.Exclude:
            sb.Append("Кроме: ");
            break;
          default:
            return String.Empty;
        }
        sb.Append(Codes.Replace(",", ", "));
        if (EmptyCode)
        {
          if (!String.IsNullOrEmpty(Codes))
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
      CodeGridFilterForm Form = new CodeGridFilterForm();
      Form.TheFilter = this;
      Form.Text = DisplayName;
      Form.efpEmpty.Visible = CanBeEmpty;

      Form.efpMode.SelectedIndex = (int)(Mode);
      Form.efpCodes.Text = Codes;
      Form.efpEmpty.Checked = EmptyCode;

      if (EFPApp.ShowDialog(Form, true, dialogPosition) != DialogResult.OK)
        return false;

      Mode = (CodesFilterMode)(Form.efpMode.SelectedIndex);
      Codes = NormCodes(Form.efpCodes.Text);
      EmptyCode = Form.efpEmpty.Checked;
      return true;
    }
 
    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Показать диалог выбора кодов из справочника
    /// </summary>
    /// <param name="codes">Вход-выход: Выбранные коды</param>
    /// <returns>true, если выбор сделан</returns>
    public virtual bool ShowSelectCodesDialog(ref string[] codes)
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