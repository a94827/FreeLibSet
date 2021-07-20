﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.DependedValues;

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

namespace AgeyevAV.ExtForms.Docs
{
  /// <summary>
  /// Класс формы для редактора документов
  /// </summary>
  internal partial class DocEditForm : Form
  {
    #region Конструктор и Dispose

    internal DocEditForm(DocumentEditor editor)
    {
      InitializeComponent();

      _MaxPageSize = new Size(336, 48);

      _FormProvider = new EFPFormProvider(this);

      MainTabControl.ImageList = EFPApp.MainImages;
      MainTabControl.ShowToolTips = EFPApp.ShowToolTips;
      _TabControlProvider = new EFPTabControl(_FormProvider, MainTabControl);

      _OKButtonProvider = new EFPButton(_FormProvider, btnOK);

      _CancelButtonProvider = new EFPButton(_FormProvider, btnCancel);

      btnApply.ImageList = EFPApp.MainImages;
      btnApply.ImageKey = "Apply";
      _ApplyButtonProvider = new EFPButton(_FormProvider, btnApply);

      btnMore.ImageList = EFPApp.MainImages;
      btnMore.ImageKey = "MenuButton";
      _MoreButtonProvider = new EFPButtonWithMenu(_FormProvider, btnMore);

      _DisposeFormList = new List<Form>();

      _DocEditItems = new DocEditItemList();

      _ChangeInfoList = new DepChangeInfoList();
      _ChangeInfoList.DisplayName = "Форма редактора";
      _FormProvider.ChangeInfo = _ChangeInfoList;

      _Pages = new DocEditPages(this);

      _Editor = editor;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }

      if (_DisposeFormList != null)
      {
        for (int i = 0; i < _DisposeFormList.Count; i++)
        {
          try
          {
            _DisposeFormList[i].Dispose();
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, "Ошибка удаления формы из DisposeFormList");
          }
        }
        _DisposeFormList = null;
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Провайдеры элементов формы

    /// <summary>
    /// Провайдер обработки ошибок для формы в-целом
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    /// <summary>
    /// Провайдер элемента с закладками
    /// </summary>
    public EFPTabControl TabControlProvider { get { return _TabControlProvider; } }
    private EFPTabControl _TabControlProvider;

    /// <summary>
    /// Провайдер кнопки "ОК"
    /// </summary>
    public EFPButton OKButtonProvider { get { return _OKButtonProvider; } }
    private EFPButton _OKButtonProvider;

    /// <summary>
    /// Провайдер кнопки "Отмена"
    /// </summary>
    public EFPButton CancelButtonProvider { get { return _CancelButtonProvider; } }
    private EFPButton _CancelButtonProvider;

    /// <summary>
    /// Провайдер кнопки "Запись"
    /// </summary>
    public EFPButton ApplyButtonProvider { get { return _ApplyButtonProvider; } }
    private EFPButton _ApplyButtonProvider;

    /// <summary>
    /// Провайдер кнопки "Еще"
    /// </summary>
    public EFPButtonWithMenu MoreButtonProvider { get { return _MoreButtonProvider; } }
    private EFPButtonWithMenu _MoreButtonProvider;

    private void btnOK_Click(object sender, EventArgs args)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void btnCancel_Click(object sender, EventArgs args)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }


    #endregion

    #region Список синхронно удаляемых форм

    /// <summary>
    /// Список форм, подлежащих разрушению вместе с данной формой
    /// </summary>
    public List<Form> DisposeFormList { get { return _DisposeFormList; } }
    private List<Form> _DisposeFormList;

    #endregion

    #region Другие свойства

    public DocumentEditor Editor { get { return _Editor; } }
    private DocumentEditor _Editor;

    public DocEditPages Pages { get { return _Pages; } }
    private DocEditPages _Pages;

    public DocEditItemList DocEditItems { get { return _DocEditItems; } }
    private DocEditItemList _DocEditItems;

    public DepChangeInfoList ChangeInfoList { get { return _ChangeInfoList; } }
    private DepChangeInfoList _ChangeInfoList;

    #endregion

    #region Корректировка размера формы

    private Size _MaxPageSize;

    internal void CorrectSize()
    {
      if (MainTabControl.TabCount == 0)
        return;
      //      Size sz=Size;
      //      Size=new Size(sz.Width+MyMaxPageSize.Width-336, 
      //        sz.Height+MyMaxPageSize.Height-48);

      PerformLayout();
      
      // Размеры заголовка и рамок формы и панели с кнопками
      Size sz2 = new Size();
      sz2.Width = Size.Width - ClientSize.Width;
      sz2.Height = Size.Height - ClientSize.Height + ButtonsPanel.Size.Height;

      // Размеры области закладок
      Size sz3 = new Size();
      //sz3.Width=10;
      //sz3.Height=36;
      sz3.Width = MainTabControl.Width - MainTabControl.DisplayRectangle.Width;
      sz3.Height = MainTabControl.Height - MainTabControl.DisplayRectangle.Height;
      Size = new Size(_MaxPageSize.Width + sz2.Width + sz3.Width,
                    _MaxPageSize.Height + sz2.Height + sz3.Height);
    }

    internal void RegPageSize(Size sz)
    {
      _MaxPageSize.Width = Math.Max(_MaxPageSize.Width, sz.Width);
      _MaxPageSize.Height = Math.Max(_MaxPageSize.Height, sz.Height);
    }

    #endregion
  }
}