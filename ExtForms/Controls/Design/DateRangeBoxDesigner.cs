﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.Windows.Forms.Design.Behavior;
using System.ComponentModel;

#pragma warning disable 1591

namespace FreeLibSet.Controls.Design
{

  public class DateRangeBoxDesigner : ControlDesigner
  {
    #region Размеры

    /// <summary>
    /// Разрешено менять только горизонтальные размеры
    /// </summary>
    public override SelectionRules SelectionRules
    {
      get
      {
        SelectionRules rules = base.SelectionRules;
        rules = rules & (~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
        return rules;
      }
    }

    #endregion

    #region Snap lines

    // Добавляем "сиреневую" линию базовой линии текста для дизайнера формы
    // Линия берется из поля ввода первой даты
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
        DateRangeBox control = Control as DateRangeBox;
        if (control == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        // This is the control in your UC that you want SnapLines for the entire UC
        IDesigner designer = TypeDescriptor.CreateDesigner(control.First, typeof(IDesigner));
        if (designer == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        designer.Initialize(control.First);

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
                 line.Offset + control.First.Top, // всегда 0
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
