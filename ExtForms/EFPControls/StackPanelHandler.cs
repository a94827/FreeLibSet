﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

#pragma warning disable 1591

namespace FreeLibSet.Forms
{
  /*
   * Реализация сборки панелей блока диалога с вертикальным размещением элементов.
   * Для панели блока диалога или еще чего-нибудь, создается объект StackPanelHandler,
   * которому передается родительский управляющий элемент и EFPBaseProvider
   * 
   * Затем могут создаваться элементы, производные от StackPanelItem, которые
   * размещаются сверху вниз в родительском элементе. Элементы занимают всю ширину
   * панели
   */


  /// <summary>
  /// Панель с вертикальным размещением элементов.
  /// Используется в удаленном интерфейсе
  /// </summary>
  public class StackPanelHandler
  {
    #region Конструктор

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentControl"></param>
    /// <param name="baseProvider"></param>
    public StackPanelHandler(Control parentControl, EFPBaseProvider baseProvider)
    {
#if DEBUG
      if (parentControl == null)
        throw new ArgumentNullException("parentControl");
      if (baseProvider == null)
        throw new ArgumentNullException("baseProvider");
#endif
      _ParentControl = parentControl;
      _BaseProvider = baseProvider;
    }

    #endregion

    #region Свойства

    public Control ParentControl { get { return _ParentControl; } }
    private readonly Control _ParentControl;

    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private readonly EFPBaseProvider _BaseProvider;

    #endregion
  }

  /// <summary>
  /// Базовый класс для реализации части панели с вертикальным размещением элементов
  /// </summary>
  public class StackPanelItem
  {
    #region Конструктор

    public StackPanelItem(StackPanelHandler handler, Control itemControl)
    {
      itemControl.Dock = DockStyle.Top;
      handler.ParentControl.Controls.Add(itemControl);
      handler.ParentControl.Controls.SetChildIndex(itemControl, 0);
      _ItemControl = itemControl;
    }

    #endregion

    #region Свойства

    protected Control ItemControl { get { return _ItemControl; } }
    protected readonly Control _ItemControl;

    #endregion
  }

  /// <summary>
  /// Часть панели с заголовком. Заголовок центрируется и выделен жирным шрифтом
  /// </summary>
  public class StackPanelHeader : StackPanelItem
  {
    #region Конструктор

    public StackPanelHeader(StackPanelHandler handler)
      :base(handler, new Label())
    {
      ItemControl.Height = 16;
      ((Label)ItemControl).TextAlign = ContentAlignment.MiddleCenter;
      ((Label)ItemControl).Font = new Font(((Label)ItemControl).Font, FontStyle.Bold);
    }

    public StackPanelHeader(StackPanelHandler handler, string text)
      :this(handler)
    {
      this.Text = text;
    }

    #endregion

    #region Свойства

    public string Text
    {
      get { return ItemControl.Text; }
      set { ItemControl.Text = value; }
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс части панели, содержащей GroupBox
  /// </summary>
  public class StackPanelGroupBox : StackPanelItem
  {
    #region Конструктор

    public StackPanelGroupBox(StackPanelHandler handler)
      :base(handler, new GroupBox())
    { 
      _TheGroupBox=new EFPGroupBox(handler.BaseProvider, (GroupBox)ItemControl);
      _TheGroupBox.Control.AutoSize = true;
      _TheGroupBox.Control.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    }

    #endregion

    #region Свойства

    public EFPGroupBox TheGroupBox { get { return _TheGroupBox; } }
    private EFPGroupBox _TheGroupBox;

    #endregion
  }

  public class StackPanelRadioButtons : StackPanelGroupBox
  {
    #region Конструктор

    public StackPanelRadioButtons(StackPanelHandler handler, string[] items)
      :base(handler)
    {
      RadioButton[] buttons=new RadioButton[items.Length];
      for (int i = 0; i < items.Length; i++)
      {
        buttons[i] = new RadioButton();
        buttons[i].AutoSize = true;
        buttons[i].Text = items[i];
        buttons[i].Location = new Point(3,
          16 + buttons[i].Height * i);
        TheGroupBox.Control.Controls.Add(buttons[i]);
      }

      _TheButtons = new EFPRadioButtons(handler.BaseProvider, buttons);
    }

    #endregion

    #region Свойства

    public EFPRadioButtons TheButtons { get { return _TheButtons; } }
    private EFPRadioButtons _TheButtons;

    #endregion
  }


  public class StackPanelControlWithLabel : StackPanelItem
  {
    #region Конструктор

    public StackPanelControlWithLabel(StackPanelHandler handler, EFPControlBase controlProvider)
      : base(handler, new TableLayoutPanel())
    {
      _ControlProvider = controlProvider;

      TableLayoutPanel panel = (TableLayoutPanel)(base.ItemControl);
      panel.AutoSize = true;
      panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

      panel.RowCount = 1;
      panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

      panel.ColumnCount = 2;
      panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
      panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

      _TheLabel = new Label();
      _TheLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
      _TheLabel.TextAlign = ContentAlignment.MiddleLeft;
      panel.Controls.Add(_TheLabel, 0, 0);

      _ControlProvider.Label = _TheLabel;
      _ControlProvider.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top;
      panel.Controls.Add(_ControlProvider.Control, 1, 0);

    }

    #endregion

    #region Свойства

    public Label TheLabel { get { return _TheLabel; } }
    private Label _TheLabel;

    public EFPControlBase ControlProvider { get { return _ControlProvider; } }
    private EFPControlBase _ControlProvider;

    #endregion
  }


  /*

  public class StackPanelControlWithLabel<T> : StackPanelItem
    where T : Control, new()
  {
    #region Конструктор

    public StackPanelControlWithLabel(StackPanelHandler Handler)
      :base(Handler, new TableLayoutPanel())
    {
      TableLayoutPanel Panel = (TableLayoutPanel)(base.ItemControl);
      Panel.AutoSize = true;
      Panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

      Panel.RowCount = 1;
      Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

      Panel.ColumnCount = 2;
      Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
      Panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

      FTheLabel = new Label();
      FTheLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
      FTheLabel.TextAlign = ContentAlignment.MiddleLeft;
      Panel.Controls.Add(FTheLabel, 0, 0);

      FTheControl = new T();
      FTheControl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
      Panel.Controls.Add(FTheControl, 1, 0);

    }

    #endregion

    #region Свойства

    public Label TheLabel { get { return FTheLabel; } }
    private Label FTheLabel;

    public T TheControl { get { return FTheControl; } }
    private T FTheControl;

    #endregion
  }
                                          
  public class StackPanelDateBox : StackPanelControlWithLabel<DateBox>
  {
    #region Конструктор

    public StackPanelDateBox(StackPanelHandler Handler)
      : base(Handler)
    {
      FControlProvider = new EFPDateBox(Handler.BaseProvider, TheControl);
    }

    #endregion

    #region Свойства

    public EFPDateBox ControlProvider { get { return FControlProvider; } }
    private EFPDateBox FControlProvider;

    #endregion
  }
   * */

}
