// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  public class ExtDataGridViewCheckBoxColumn : DataGridViewCheckBoxColumn
  {
    public ExtDataGridViewCheckBoxColumn()
    {
      base.CellTemplate = new ExtDataGridViewCheckBoxCell();
    }

    // ReSharper disable once RedundantOverriddenMember
    public override object Clone()
    {
      return base.Clone();
    }
  }

  public class ExtDataGridViewCheckBoxCell : DataGridViewCheckBoxCell
  {
    // ReSharper disable once RedundantOverriddenMember
    public override object Clone()
    {
      return base.Clone();
    }

#if XXXX
    private bool MouseInside;

    protected override void OnMouseEnter(int rowIndex)
    {
      base.OnMouseEnter(rowIndex);
      MouseInside = true;
    }

    protected override void OnMouseLeave(int rowIndex)
    {
      base.OnMouseLeave(rowIndex);
      MouseInside = false;
    }
#endif

    protected override void Paint(Graphics graphics, Rectangle clipBounds,
      Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState,
      object value, object formattedValue, string errorText,
      DataGridViewCellStyle cellStyle,
      DataGridViewAdvancedBorderStyle advancedBorderStyle,
      DataGridViewPaintParts paintParts)
    {
      const DataGridViewPaintParts Parts1Mask = DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground | DataGridViewPaintParts.Border|DataGridViewPaintParts.ContentBackground ;
      const DataGridViewPaintParts Parts2Mask = DataGridViewPaintParts.ContentForeground;
      const DataGridViewPaintParts Parts3Mask = DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.Focus;

      DataGridViewPaintParts Parts1 = paintParts & Parts1Mask;
      DataGridViewPaintParts Parts2 = paintParts & Parts2Mask;
      DataGridViewPaintParts Parts3 = paintParts & Parts3Mask;

      // 1. Фон
      if (Parts1 != DataGridViewPaintParts.None)
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, Parts1);
      // 2. Содержание
      if (Parts2 != DataGridViewPaintParts.None)
      {
#if XXXX
        bool ManualPrint = false;
        if (ManualPrint)
        {
          //base.getb
          //Console.Beep(100, 50);
          bool NeedPaint = false;
          CheckState State = CheckState.Unchecked;
          if (formattedValue is bool)
          {
            NeedPaint = true;
            if ((bool)formattedValue)
              State = CheckState.Checked;
            else
              State = CheckState.Unchecked;
          }
          else if (formattedValue is CheckState)
          {
            NeedPaint = true;
            State = (CheckState)formattedValue;
          }

          if (NeedPaint)
          {
            // Мышь внутри ячейки?
            bool Pressed = false;
            if (false)
            {
              Pressed = true;//!!!
            }
            else
            {
              if (MouseInside && ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left))
              {
                MouseInside = false;
                Pressed = true;
              }
            }

            CheckBoxState PaintState;
            if (Pressed)
            {
              switch (State)
              {
                case CheckState.Checked: PaintState = CheckBoxState.CheckedPressed; break;
                case CheckState.Indeterminate: PaintState = CheckBoxState.MixedPressed; break;
                default: PaintState = CheckBoxState.UncheckedPressed; break;
              }
            }
            else if (MouseInside)
            {
              switch (State)
              {
                case CheckState.Checked: PaintState = CheckBoxState.CheckedHot; break;
                case CheckState.Indeterminate: PaintState = CheckBoxState.MixedHot; break;
                default: PaintState = CheckBoxState.UncheckedHot; break;
              }
            }
            else
            {
              switch (State)
              {
                case CheckState.Checked: PaintState = CheckBoxState.CheckedNormal; break;
                case CheckState.Indeterminate: PaintState = CheckBoxState.MixedNormal; break;
                default: PaintState = CheckBoxState.UncheckedNormal; break;
              }
            }

            Size Sz = CheckBoxRenderer.GetGlyphSize(graphics, PaintState);
            Point Pt = new Point(cellBounds.Location.X + (cellBounds.Width - Sz.Width) / 2,
              cellBounds.Location.Y + (cellBounds.Height - Sz.Height) / 2);

            //ControlPaint.DrawCheckBox(
            CheckBoxRenderer.DrawCheckBox(graphics, Pt, PaintState);
          }
          //graphics.DrawLine(Pens.Red, cellBounds.Location, new Point(cellBounds.Right, cellBounds.Bottom));
        }
        else
#endif
          base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, Parts2);
      }


      // 3. Сверху
      if (Parts3 != DataGridViewPaintParts.None)
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, Parts3);
    }
  }
}
