// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  internal partial class EFPDataViewCopyFormatsForm : Form
  {
    #region Конструктор формы

    public EFPDataViewCopyFormatsForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["CopySettings"];

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpText = new EFPCheckBox(efpForm, cbText);
      efpCsv = new EFPCheckBox(efpForm, cbCsv);
      efpHtml = new EFPCheckBox(efpForm, cbHtml);

      efpForm.FormChecks.Add(ValidateForm);
    }

    #endregion

    #region Флажки

    private EFPCheckBox efpText, efpCsv, efpHtml;

    private void ValidateForm(object sender, UIValidatingEventArgs args)
    {
      if (efpText.Editable && efpText.Checked)
        return;
      if (efpCsv.Editable && efpCsv.Checked)
        return;
      if (efpHtml.Editable && efpHtml.Checked)
        return;
      args.SetError("Должен быть выбран хотя бы один формат");
    }

    #endregion

    #region Статический метод

    /// <summary>
    /// Форматы, выбранные пользователем.
    /// </summary>
    public static EFPDataViewCopyFormats UserSelectedFormats { get { return _UserSelectedFormats; } }
    private static EFPDataViewCopyFormats _UserSelectedFormats = EFPDataViewCopyFormats.All;

    /// <summary>
    /// Показ диалога настроек
    /// </summary>
    /// <param name="copyFormats">Форматы, которые поддерживаются управляющим элементом</param>
    public static void ShowDialog(EFPDataViewCopyFormats copyFormats)
    {
      EFPDataViewCopyFormatsForm form = new EFPDataViewCopyFormatsForm();
      InitCheckBox(form.efpText, copyFormats, EFPDataViewCopyFormats.Text);
      InitCheckBox(form.efpCsv, copyFormats, EFPDataViewCopyFormats.CSV );
      InitCheckBox(form.efpHtml, copyFormats, EFPDataViewCopyFormats.HTML);
      if (EFPApp.ShowDialog(form, true) != DialogResult.OK)
        return;

      ReadCheckBox(form.efpText, copyFormats, EFPDataViewCopyFormats.Text);
      ReadCheckBox(form.efpCsv, copyFormats, EFPDataViewCopyFormats.CSV);
      ReadCheckBox(form.efpHtml, copyFormats, EFPDataViewCopyFormats.HTML);
    }

    private static void InitCheckBox(EFPCheckBox efp, EFPDataViewCopyFormats copyFormats, EFPDataViewCopyFormats format)
    {
      if ((copyFormats & format) != 0)
        efp.Checked = (_UserSelectedFormats & format) != 0;
      else
        efp.Enabled = false;
    }

    private static void ReadCheckBox(EFPCheckBox efp, EFPDataViewCopyFormats copyFormats, EFPDataViewCopyFormats format)
    {
      if ((copyFormats & format) != 0)
      {
        if (efp.Checked)
          _UserSelectedFormats |= format;
        else
          _UserSelectedFormats &= (~format);
      }
    }

    internal static void AddCommandItem(IEFPDataViewClipboardCommandItems commandItems)
    {
      EFPCommandItem ci1 = new EFPCommandItem("Edit", "CopyFormats");
      ci1.MenuText = "Настройка форматов копирования ...";
      ci1.ImageKey = "CopySettings";
      ci1.Tag = commandItems;
      ci1.Usage = EFPCommandItemUsage.Menu;
      ci1.Click += CopyFormats_Click;
      ci1.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBarAux;

      ((EFPCommandItems)commandItems).Add(ci1);
      /*
      EFPCommandItem toolBarSubMenu = new EFPCommandItem("Edit", "CopySubMenu");
      toolBarSubMenu.MenuText = "Дополнительные команды копирования";
      toolBarSubMenu.Usage = EFPCommandItemUsage.ToolBarDropDown;
      ((EFPCommandItems)commandItems).Add(toolBarSubMenu);

      EFPCommandItem ci2 = new EFPCommandItem("Edit", "CopyFormats2");
      ci2.Parent = toolBarSubMenu;
      ci2.MenuText = "Настройка форматов копирования";
      ci2.Tag = commandItems;
      ci2.Usage = EFPCommandItemUsage.Menu;
      ci2.Click += CopyFormats_Click;
      ((EFPCommandItems)commandItems).Add(ci2);

      return toolBarSubMenu;
      */
    }

    private static void CopyFormats_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      IEFPDataViewClipboardCommandItems commandItems = (IEFPDataViewClipboardCommandItems)(ci.Tag);
      ShowDialog(commandItems.CopyFormats);
    }

    #endregion
  }
}
