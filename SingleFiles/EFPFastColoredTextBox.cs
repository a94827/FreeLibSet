using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using FastColoredTextBoxNS;
using FreeLibSet.Forms;

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

namespace AgeyevAV.ExtForms
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