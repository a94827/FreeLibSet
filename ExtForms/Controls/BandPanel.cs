// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

namespace FreeLibSet.Controls
{
  /// <summary>                                                
  /// Контейнер для размещения управляющих элементов один под другим
  /// </summary>
  internal class BandPanel : TableLayoutPanel
  {                                           
    #region Конструктор

    public BandPanel()
    {
      _Stripes = new StripeCollection(this);
      //MainPanel.ColumnCount = 2;
      //MainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
      //MainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

      base.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
      base.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
      //MainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

      //MainPanel.Dock = DockStyle.Top;
      base.AutoScroll = false;
      base.AutoSize = true;
      base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      base.Padding = new Padding(6);
      base.Margin = new Padding(0);
    }

    #endregion

    #region Список полос

    public sealed class StripeCollection : IList<BandPanelStripe>
    {
      #region Конструктор

      internal StripeCollection(BandPanel owner)
      {
        _Owner = owner;
        _Items = new List<BandPanelStripe>();
      }

      #endregion

      #region Список

      private readonly List<BandPanelStripe> _Items;

      public void Add(BandPanelStripe item)
      {
        if (item == null)
          throw new ArgumentNullException("item");

        if (item.BandPanel != null)
          throw ExceptionFactory.CannotAddItemAgain(item);

        if (item.Label != null)
          InitTabIndex(item.Label);
        InitTabIndex(item.Control);

        _Items.Add(item);
        _Owner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        if (item.Label == null)
        {
          // Один элемент по всей ширине
          item.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
          _Owner.Controls.Add(item.Control, 0, _Items.Count - 1);
          _Owner.SetColumnSpan(item.Control, 2);
        }
        else
        {
          // Элемент с меткой
          item.Label.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
          _Owner.Controls.Add(item.Label, 0, _Items.Count - 1);

          item.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
          _Owner.Controls.Add(item.Control, 1, _Items.Count - 1);
        }
      }

      /// <summary>
      /// Инициализация свойства TabIndex для нового элемента или метки
      /// </summary>
      /// <param name="control">Еще не присоединенный к полосеэлемент</param>
      private void InitTabIndex(Control control)
      {
        if (_Owner.Controls.Count > 0)
        {
          Control LastControl = _Owner.Controls[_Owner.Controls.Count - 1];
          control.TabIndex = LastControl.TabIndex + 1;
        }
        else
          control.TabIndex = 0;
      }

      public int IndexOf(BandPanelStripe item)
      {
        return _Items.IndexOf(item);
      }

      public void Insert(int index, BandPanelStripe item)
      {
        throw new ObjectReadOnlyException();
      }

      public void RemoveAt(int index)
      {
        throw new ObjectReadOnlyException();
      }

      public BandPanelStripe this[int index]
      {
        get
        {
          return _Items[index];
        }
        set
        {
          throw new ObjectReadOnlyException();
        }
      }

      public void Clear()
      {
        throw new ObjectReadOnlyException();
      }

      public bool Contains(BandPanelStripe item)
      {
        return _Items.Contains(item);
      }

      public void CopyTo(BandPanelStripe[] array, int arrayIndex)
      {
        _Items.CopyTo(array, arrayIndex);
      }

      public int Count
      {
        get { return _Items.Count; }
      }

      public bool IsReadOnly
      {
        get { return false; }
      }

      public bool Remove(BandPanelStripe item)
      {
        throw new ObjectReadOnlyException();
      }

      public IEnumerator<BandPanelStripe> GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      #endregion

      #region Прочие свойства

      private readonly BandPanel _Owner;

      #endregion
    }

    /// <summary>
    /// Список полос
    /// </summary>
    public StripeCollection Stripes { get { return _Stripes; } }
    private readonly StripeCollection _Stripes;

    #endregion

    public void FinishLoad()
    {
      base.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100);
    }

    //protected override void OnSizeChanged(EventArgs args)
    //{
    //  base.OnSizeChanged(args);
    //  if (IsHandleCreated)
    //  {
    //    int h1=DataTools.SumInt32(base.GetRowHeights());
    //    int h2 = base.ClientSize.Height;
    //    FindForm().Text = "h1=" + h1.ToString() + ", h2=" + h2.ToString();
    //  }
    //}
  }

  internal class BandPanelStripe
  {
    #region Конструкторы

    public BandPanelStripe(string labelText, Control control)
      : this(CreateLabel(labelText), control)
    {
    }

    private static Control CreateLabel(string labelText)
    {
      if (String.IsNullOrEmpty(labelText))
        return null;

      Label lbl = new Label();
      lbl.Text = labelText;
      lbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      lbl.AutoSize = false;
      lbl.Width = lbl.PreferredWidth; // 07.11.2017
      lbl.UseMnemonic = true;
      return lbl;
    }

    public BandPanelStripe(Control label, Control control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      _Label = label;
      _Control = control;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Элемент - метка. Обычно объект Label
    /// Может быть null
    /// </summary>
    public Control Label { get { return _Label; } }
    private readonly Control _Label;

    /// <summary>
    /// Основной управляющий элемент. Не может быть null
    /// </summary>
    public Control Control { get { return _Control; } }
    private readonly Control _Control;

    /// <summary>
    /// Объект-владелец, после добавления полосы в управляющий  элемент
    /// </summary>
    public BandPanel BandPanel
    {
      get { return _BandPanel; }
      internal set { _BandPanel = value; }
    }
    private BandPanel _BandPanel;

    #endregion
  }
}
