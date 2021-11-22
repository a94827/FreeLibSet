// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms.Design.Behavior;
using System.ComponentModel;

#pragma warning disable 1591 // нет документации

namespace FreeLibSet.Controls.Design
{
  public class YearMonthRangeBoxDesigner : ControlDesigner
  {
    #region Размеры

    /// <summary>
    /// Разрешено менять только горизонтальные размеры
    /// </summary>
    public override SelectionRules SelectionRules
    {
      get
      {
        SelectionRules Rules = base.SelectionRules;
        Rules = Rules & (~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
        return Rules;
      }
    }

    #endregion

    #region Snap lines

    // Добавляем "сиреневую" линию базовой линии текста для дизайнера формы
    // Линия берется из комбоблока первого месяца
    // Взято из 
    // http://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
    //

    public override IList SnapLines
    {
      get
      {
        /* Code from above */
        IList snapLines = base.SnapLines;

        // *** This will need to be modified to match your user control
        YearMonthRangeBox control = Control as YearMonthRangeBox;
        if (control == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        // This is the control in your UC that you want SnapLines for the entire UC
        IDesigner designer = TypeDescriptor.CreateDesigner(control.cbMonth1, typeof(IDesigner));
        if (designer == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        designer.Initialize(control.cbMonth1);

        using (designer)
        {
          ControlDesigner boxDesigner = designer as ControlDesigner;
          if (boxDesigner == null)
            return snapLines;

          foreach (SnapLine line in boxDesigner.SnapLines)
          {
            if (line.SnapLineType == SnapLineType.Baseline)
            {
              // *** This will need to be modified to match the item in your user control
              snapLines.Add(new SnapLine(SnapLineType.Baseline,
                 line.Offset + control.cbMonth1.Top, // обычно 0
                 line.Filter, line.Priority));
              break;
            }
          }
        }

        return snapLines;
      }
    }

    #endregion
  }
}
