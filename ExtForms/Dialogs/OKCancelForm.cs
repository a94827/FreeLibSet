﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Core;

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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Простая форма с кнопками "ОК" и "Отмена".
  /// Используйте свойство MainPanel для добавления управляющих элементов.
  /// Также есть две дополнительные панели TopPanel и BottomPanel. Эти панели автоматически подбирают свои размеры под содержимое.
  /// Эти панели изначально пусты и скрыты. В них можно добавлять свои элементы, установив свойство Visible=true.
  /// Форма содержит FormProvider.
  /// При необходимости использовать форму в немодальном режиме, добавляйте обработчик
  /// к efpOk.Click или Form.Closed, проверяя DialogResult
  /// </summary>
  public partial class OKCancelForm : Form
  {
    /// <summary>
    /// Создает форму с кнопками "ОК" и "Отмена".
    /// Устанавливается рамка формы Fixed3D, кнопки минимизации и максимизации отключаются
    /// </summary>
    public OKCancelForm()
      : this(true)
    {
    }

    /// <summary>
    /// Создает форму с кнопками "ОК" и "Отмена".
    /// </summary>
    /// <param name="initBorder">Если true, то устанавливается рамка формы Fixed3D, кнопки минимизации и максимизации отключаются.
    /// Если false, используется рамка формы по умолчанию с возможностью установки размеров формы</param>
    public OKCancelForm(bool initBorder)
    {
      InitializeComponent();
      if (initBorder)
      {
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      }

      EFPApp.InitFormImages(this);

      _FormProvider = new EFPFormProvider(this);
      _OKButtonProvider = new EFPButton(FormProvider, btnOk);
      _CancelButtonProvider = new EFPButton(FormProvider, btnCancel);
      _NoButtonProvider = new EFPButton(FormProvider, btnNo);
      FormProvider.AddFormCheck(new EFPValidatingEventHandler(DoFormValidating));

      //BottomPanel.BackColor = Color.Red;
      //this.panel1.BackColor = Color.Yellow;

      // 31.03.2020
      this.TopPanel.Layout += new LayoutEventHandler(TopOrBottomPanel_Layout);
      this.TopPanel.Resize += new EventHandler(TopOrBottomPanel_Layout);
      this.BottomPanel.Layout += new LayoutEventHandler(TopOrBottomPanel_Layout);
      this.BottomPanel.Resize += new EventHandler(TopOrBottomPanel_Layout);
    }

    #region Провайдеры формы

    /// <summary>
    /// Провайдер формы
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

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
    /// Провайдер кнопки "Нет".
    /// По умолчанию эта кнопка невидима
    /// </summary>
    public EFPButton NoButtonProvider { get { return _NoButtonProvider; } }
    private EFPButton _NoButtonProvider;

    #endregion

    #region События

    /// <summary>
    /// Вызывается при нажатии кнопки ОК для проверки корректности значений
    /// В отличие от обработчиков, присоединяемых efpForm.AddFormCheck(), 
    /// здесь параметром Object будет объект OKCancelForm
    /// </summary>
    public event EFPValidatingEventHandler FormValidating;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Минимальные размеры по умолчанию - чтобы панель с кнопками была видна
    /// </summary>
    protected override Size DefaultMinimumSize
    {
      get
      {
        if (ButtonsPanel == null)
          return base.DefaultMinimumSize;

        int LeftPos = int.MaxValue;
        int RightPos = 0;
        bool Flag = false;
        foreach (Control btn in ButtonsPanel.Controls)
        {
          if (btn.Visible)
          {
            Flag = true;
            LeftPos = Math.Min(LeftPos, btn.Left);
            RightPos = Math.Max(RightPos, btn.Right);
          }
        }

        if (!Flag) // нет ни одной кнопки
          return base.DefaultMinimumSize;


        int W = RightPos + LeftPos; // отступ в 8 пикселей от правого края кнопки

        int H = base.Height - base.ClientSize.Height + ButtonsPanel.Height;
        if (TopPanel.Visible)
          H += TopPanel.Height;
        if (BottomPanel.Visible)
          H += BottomPanel.Height;

        return new Size(W + base.Width - ButtonsPanel.Width,
          H + 8);
      }
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Создает и добавляет информационное сообщение в верхней или нижней части формы.
    /// Объект InfoLabel присоединяется к панели TopPanel или BottomPanel, в зависимости от значения <paramref name="dock"/>.
    /// Для добавления элементов других типов используйте верхнюю и нижние панели самостоятельно.
    /// </summary>
    /// <param name="dock">Расположение метки. Допустимые значения: Top и Bottom</param>
    /// <returns></returns>
    public InfoLabel AddInfoLabel(DockStyle dock)
    {
      Panel parent;
      switch (dock)
      {
        case DockStyle.Top:
          parent = TopPanel;
          break;
        case DockStyle.Bottom:
          parent = BottomPanel;
          break;
        default:
          throw new ArgumentException("dock");
      }

      InfoLabel lbl = new InfoLabel();
      lbl.Dock = dock;
      lbl.AutoSize = true;
      parent.Controls.Add(lbl);
      parent.Visible = true;
      return lbl;
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Обработчик изменения размеров панелей TopPanel и BottomPanel, а также изменения дочерних элементов.
    /// Можно использовать один обработчик для событий Panel.Resize и Panel.Layot, хотя аргументы имеют разный тип
    /// </summary>
    /// <param name="sender">Панель TopPanel или BottomPanel</param>
    /// <param name="args">Не используется</param>
    void TopOrBottomPanel_Layout(object sender, EventArgs args)
    {
      // 31.03.2020
      // Больше не используем свойства Panel.AutoSize и AutoSizeMode для управления размерами панели,
      // т.к. они неправильно работают с InfoLabel (это может быть и бяка в InfoLabel)
      // Вместо этого, устанавливаем размеры самостоятельно

      Panel TBPanel = (Panel)sender;
      TBPanel.Visible = TBPanel.Controls.Count > 0;
      if (TBPanel.Controls.Count == 0)
        return;

      MinMaxInt mm = new MinMaxInt();
      // Дочерние элементы могут располагаться в любом месте панели, и сверху и снизу.
      // Они получат нормальные координаты только после изменения размеров, а не сейчас
      foreach (Control child in TBPanel.Controls)
      {
        mm += child.Top;
        mm += child.Bottom;
      }
      TBPanel.Height = mm.MaxValue - mm.MinValue;
    }

    private void btn_Click(object sender, EventArgs args)
    {
      Button button = (Button)sender;
      DialogResult = button.DialogResult;
      Close();
    }

    private void DoFormValidating(object sender, EFPValidatingEventArgs args)
    {
      if (FormValidating == null)
        return;
      FormValidating(this, args);
    }

    #endregion
  }


  /// <summary>
  /// Простая форма, содержащая единственный управляющий элемент и панель инстументов.
  /// Также есть две дополнительные панели TopPanel и BottomPanel. Эти панели автоматически подбирают свои размеры под содержимое.
  /// Эти панели изначально пусты и скрыты. В них можно добавлять свои элементы, установив свойство Visible=true.
  /// Для управляющего элемента требуется отдельно создать подходящий провайдер,
  /// производный от EFPControlBase.
  /// </summary>
  /// <typeparam name="T">Тип управляющего элемента</typeparam>
  public class SimpleForm<T> : Form, IEFPControlWithToolBar<T>
    where T : Control, new()
  {
    #region Конструктор

    /// <summary>
    /// Создает форму с управляющим элементом и панелью инструментов
    /// </summary>
    public SimpleForm()
    {
      _FormProvider = new EFPFormProvider(this);

      _MainPanel = new Panel();
      _MainPanel.Dock = DockStyle.Fill;
      _MainPanel.TabIndex = 1;
      Controls.Add(_MainPanel);

      _TopPanel = new Panel();
      _TopPanel.Dock = DockStyle.Top;
      //_TopPanel.AutoSize = true;
      //_TopPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      _TopPanel.Visible = false;
      _TopPanel.TabIndex = 0;
      Controls.Add(_TopPanel);

      _BottomPanel = new Panel();
      _BottomPanel.Dock = DockStyle.Bottom;
      //_BottomPanel.AutoSize = true;
      //_BottomPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      _BottomPanel.Visible = false;
      Controls.Add(_BottomPanel);

      _ControlWithToolBar = new EFPControlWithToolBar<T>(_FormProvider, _MainPanel);
      _ControlWithToolBar.Control.Name = "MainControl"; // 27.06.2019 Требуется, чтобы табличный просмотр и другие элементы могли сохранять свои параметры для композиции рабочего стола.

      EFPApp.SetFormSize(this, 50, 50);

      // 31.03.2020
      _TopPanel.Layout += new LayoutEventHandler(TopOrBottomPanel_Layout);
      _TopPanel.Resize += new EventHandler(TopOrBottomPanel_Layout);
      _BottomPanel.Layout += new LayoutEventHandler(TopOrBottomPanel_Layout);
      _BottomPanel.Resize += new EventHandler(TopOrBottomPanel_Layout);

    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основная панель - занимает всю форму. На ней располагается основной управляющий
    /// элемент Control и панель кнопок, если есть
    /// </summary>
    public Panel MainPanel { get { return _MainPanel; } }
    private Panel _MainPanel;

    /// <summary>
    /// Дополнительная  панель в верхней части окна. По умолчанию она пуста и скрыта.
    /// </summary>
    public Panel TopPanel { get { return _TopPanel; } }
    private Panel _TopPanel;

    /// <summary>
    /// Дополнительная  панель в нижней части окна. По умолчанию она пуста и скрыта.
    /// </summary>
    public Panel BottomPanel { get { return _BottomPanel; } }
    private Panel _BottomPanel;

    /// <summary>
    /// Провайдер формы
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    /// <summary>
    /// Управляющий элемент и панель инструментов
    /// </summary>
    public EFPControlWithToolBar<T> ControlWithToolBar { get { return _ControlWithToolBar; } }
    private EFPControlWithToolBar<T> _ControlWithToolBar;

    #endregion

    #region IEFPControlWithToolBar<T> Members

    EFPBaseProvider IEFPControlWithToolBar.BaseProvider
    {
      get { return _ControlWithToolBar.BaseProvider; }
    }

    /// <summary>
    /// Управляющий элемент
    /// </summary>
    public T Control
    {
      get { return _ControlWithToolBar.Control; }
    }

    Control IEFPControlWithToolBar.Control { get { return _ControlWithToolBar.Control; } }

    /// <summary>
    /// Панель для размещения кнопок
    /// </summary>
    Panel IEFPControlWithToolBar.ToolBarPanel
    {
      get { return _ControlWithToolBar.ToolBarPanel; }
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Обработчик изменения размеров панелей TopPanel и BottomPanel, а также изменения дочерних элементов.
    /// Можно использовать один обработчик для событий Panel.Resize и Panel.Layot, хотя аргументы имеют разный тип
    /// </summary>
    /// <param name="sender">Панель TopPanel или BottomPanel</param>
    /// <param name="args">Не используется</param>
    void TopOrBottomPanel_Layout(object sender, EventArgs args)
    {
      // 31.03.2020
      // Больше не используем свойства Panel.AutoSize и AutoSizeMode для управления размерами панели,
      // т.к. они неправильно работают с InfoLabel (это может быть и бяка в InfoLabel)
      // Вместо этого, устанавливаем размеры самостоятельно

      Panel TBPanel = (Panel)sender;
      TBPanel.Visible = TBPanel.Controls.Count > 0;
      if (TBPanel.Controls.Count == 0)
        return;

      MinMaxInt mm = new MinMaxInt();
      // Дочерние элементы могут располагаться в любом месте панели, и сверху и снизу.
      // Они получат нормальные координаты только после изменения размеров, а не сейчас
      foreach (Control child in TBPanel.Controls)
      {
        mm += child.Top;
        mm += child.Bottom;
      }
      TBPanel.Height = mm.MaxValue - mm.MinValue;
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Создает и добавляет информационное сообщение в верхней или нижней части формы.
    /// Объект InfoLabel присоединяется к панели TopPanel или BottomPanel, в зависимости от значения <paramref name="dock"/>.
    /// Для добавления элементов других типов используйте верхнюю и нижние панели самостоятельно.
    /// </summary>
    /// <param name="dock">Расположение метки. Допустимые значения: Top и Bottom</param>
    /// <returns></returns>
    public InfoLabel AddInfoLabel(DockStyle dock)
    {
      Panel parent;
      switch (dock)
      {
        case DockStyle.Top:
          parent = TopPanel;
          break;
        case DockStyle.Bottom:
          parent = BottomPanel;
          break;
        default:
          throw new ArgumentException("dock");
      }

      InfoLabel lbl = new InfoLabel();
      lbl.Dock = dock;
      lbl.AutoSize = true;
      parent.Controls.Add(lbl);
      parent.Visible = true;
      return lbl;
    }

    #endregion
  }

  /// <summary>
  /// Простая форма, содержащая единственный управляющий элемент, панель инстументов
  /// и кнопки "ОК" и "Отмена".
  /// Для управляющего элемента обычно требуется отдельно создать подходящий провайдер,
  /// производный от EFPControlBase.
  /// </summary>
  /// <typeparam name="T">Тип управляющего элемента</typeparam>
  public class OKCancelSimpleForm<T> : OKCancelForm, IEFPControlWithToolBar<T>
    where T : Control, new()
  {
    #region Конструктор

    /// <summary>
    /// Создает форму, управляющий элемент, панель инструментов и кнопки.
    /// Для управляющего элемента требуется отдельно создать подходящий провайдер,
    /// производный от EFPControlBase.
    /// Эта версия конструктора не создает дополнительную рамку
    /// </summary>
    public OKCancelSimpleForm()
      :this(false)
    {
    }

    /// <summary>
    /// Создает форму, управляющий элемент, панель инструментов и кнопки.
    /// Для управляющего элемента требуется отдельно создать подходящий провайдер,
    /// производный от EFPControlBase.
    /// <param name="useGroupBox">Если true, то будет создана дополнительная рамка вокруг элемента</param>
    /// </summary>
    public OKCancelSimpleForm(bool useGroupBox)
    {
      // Возвращаем стандартные значения
      FormBorderStyle = FormBorderStyle.Sizable;
      MaximizeBox = true;
      MinimizeBox = true;

      Control parent = MainPanel;
      if (useGroupBox)
      {
        _GroupBox = new GroupBox();
        _GroupBox.Dock = DockStyle.Fill;
        MainPanel.Controls.Add(_GroupBox);
        parent = _GroupBox;
      }

      _ControlWithToolBar = new EFPControlWithToolBar<T>(base.FormProvider, parent);
      _ControlWithToolBar.Control.Name = "MainControl"; // 27.06.2019 Требуется, чтобы табличный просмотр и другие элементы могли сохранять свои параметры для композиции рабочего стола.

      EFPApp.SetFormSize(this, 50, 50);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Рамка вокруг управляющего элемента и панели инструментов.
    /// Если использован конструктор, не создающий рамку, то свойство возвращает null.
    /// </summary>
    public GroupBox GroupBox { get { return _GroupBox; } }
    private GroupBox _GroupBox;

    /// <summary>
    /// Управляющий элемент и панель инструментов
    /// </summary>
    public EFPControlWithToolBar<T> ControlWithToolBar { get { return _ControlWithToolBar; } }
    private EFPControlWithToolBar<T> _ControlWithToolBar;

    #endregion

    #region IEFPControlWithToolBar<T> Members

    EFPBaseProvider IEFPControlWithToolBar.BaseProvider
    {
      get { return _ControlWithToolBar.BaseProvider; }
    }

    /// <summary>
    /// Управляющий элемент
    /// </summary>
    public T Control
    {
      get { return _ControlWithToolBar.Control; }
    }

    Control IEFPControlWithToolBar.Control
    {
      get { return _ControlWithToolBar.Control; }
    }

    /// <summary>
    /// Панель для размещения кнопок
    /// </summary>
    Panel IEFPControlWithToolBar.ToolBarPanel
    {
      get { return _ControlWithToolBar.ToolBarPanel; }
    }

    #endregion
  }


  /// <summary>
  /// Форма со списком ListView, кнопками "ОК" и "Отмена"
  /// Объект ListView имеет стиль списка и единственную колонку без заголовка,
  /// ширина которой синхронизируется с размерами окна
  /// </summary>
  public class ListViewOKCancelForm : OKCancelForm
  {
    #region Конструктор

    /// <summary>
    /// Создает форму
    /// </summary>
    public ListViewOKCancelForm()
    {
      TheGroupBox = new GroupBox();
      TheGroupBox.Dock = DockStyle.Fill;
      MainPanel.Controls.Add(TheGroupBox);

      _TheListView = new ListView();
      _TheListView.Dock = DockStyle.Fill;
      _TheListView.View = View.Details;
      _TheListView.HeaderStyle = ColumnHeaderStyle.None;
      _TheListView.Columns.Add(String.Empty);
      _TheListView.SmallImageList = EFPApp.MainImages;
      TheGroupBox.Controls.Add(_TheListView);
      this.Resize += new EventHandler(Form_Resize);
      Form_Resize(null, null);
    }

    void Form_Resize(object sender, EventArgs args)
    {
      _TheListView.Columns[0].Width = -2;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Управляющий элемент списка
    /// </summary>
    public ListView TheListView { get { return _TheListView; } }
    private ListView _TheListView;

    private GroupBox TheGroupBox;

    /// <summary>
    /// Заголовок на рамке, окружающей список
    /// </summary>
    public string GroupTitle
    {
      get { return TheGroupBox.Text; }
      set { TheGroupBox.Text = value; }
    }

    #endregion
  }
}