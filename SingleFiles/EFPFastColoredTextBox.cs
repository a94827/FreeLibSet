// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using FastColoredTextBoxNS;

/*
 * Провайдер элемента FastColoredTextBox
 * (с)Pavel Torgashov, 2011-2015
 * http://www.codeproject.com/Articles/161871/Fast-Colored-TextBox-for-syntax-highlighting
 * 
 * Этот модуль не входит в ExtForms.dll, т.к. основная сборка FastColoredTextBox.dll должна подключаться 
 * к проекту отдельно
 * 
 * Файл EFPFastColoredTextBox.cs должен быть включен в состав Вашего приложения отдельно
 */

namespace FreeLibSet.Forms
{
  public class EFPFastColoredTextBox : EFPTextBoxControlWithReadOnly<FastColoredTextBox>, IEFPTextBoxWithStatusBar
  {
    #region Конструкторы

    public EFPFastColoredTextBox(EFPBaseProvider BaseProvider, FastColoredTextBox Control)
      : base(BaseProvider, Control)
    {
      Init();
    }

    public EFPFastColoredTextBox(IEFPControlWithToolBar<FastColoredTextBox> ControlWithToolBar)
      : base(ControlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      //Control.AcceptsReturn = false;
      Control.AcceptsTab = false;
    }

    #endregion

    #region Реализация для TextBox

    protected override bool ControlReadOnly
    {
      get
      {
        return Control.ReadOnly;
      }
      set
      {
        Control.ReadOnly = value;
      }
    }

    protected override int ControlMaxLength
    {
      get
      {
        return Int32.MaxValue;
      }
      set
      {
      }
    }

    public override int SelectionStart
    {
      get
      {
        return Control.SelectionStart;
      }
      set
      {
        Control.SelectionStart = value;
      }
    }

    public override int SelectionLength
    {
      get
      {
        return Control.SelectionLength;
      }
      set
      {
        Control.SelectionLength = value;
      }
    }

    public override string SelectedText
    {
      get
      {
        return Control.SelectedText;
      }
      set
      {
        Control.SelectedText = value;
      }
    }

    public override void SelectAll()
    {
      Control.SelectAll();
    }

    public override void Select(int start, int length)
    {
      Control.SelectionStart = start;
      Control.SelectionLength = length;
    }

    #endregion

    #region IEFPTextBoxWithStatusBar Members

    public override bool IsMultiLine
    {
      get { return Control.Multiline; }
    }

    public void GetCurrentRC(out int Row, out int Column)
    {
      Range r = Control.Selection;
      Row = r.Start.iLine + 1;
      Column = r.Start.iChar + 1;
    }

    #endregion

    #region Локальное меню

    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPFastColoredTextBoxCommandItems(this);
    }

    public new EFPTextBoxCommandItems CommandItems
    {
      get { return (EFPFastColoredTextBoxCommandItems)(base.CommandItems); }
      set { base.CommandItems = value; }
    }

    #endregion
  }

  public class EFPFastColoredTextBoxCommandItems : EFPTextBoxCommandItems
  {
    #region Конструктор

    public EFPFastColoredTextBoxCommandItems(EFPFastColoredTextBox Owner)
      : base(Owner, true)
    {
    }

    #endregion

    #region Свойства

    public new EFPFastColoredTextBox Owner { get { return (EFPFastColoredTextBox)(base.Owner); } }

    #endregion
  }

}
