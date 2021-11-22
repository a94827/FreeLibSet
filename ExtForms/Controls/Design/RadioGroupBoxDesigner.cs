// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.Windows.Forms.Design.Behavior;

#pragma warning disable 1591


namespace FreeLibSet.Controls.Design
{

  public class RadioGroupBoxDesigner : ControlDesigner
  {
    #region Snap lines

    // Добавляем "сиреневую" линию базовой линии текста для дизайнера формы
    // Линия берется из поля ввода первой даты
    // Взято из 
    // http://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
    //

#if XXXX // Не работает. Линия не на месте

    public override IList SnapLines
    {
      get
      {
        /* Code from above */
        IList snapLines = base.SnapLines;

        // *** This will need to be modified to match your user control
        RadioGroupBox control = Control as RadioGroupBox;
        if (control == null)
          return snapLines;

        if (control.Controls[0].Controls.Count == 0)
          return snapLines; // нет ни одной кнопки

        Control FirstButton=control.Controls[0].Controls[0];

        // *** This will need to be modified to match the item in your user control
        // This is the control in your UC that you want SnapLines for the entire UC
        IDesigner designer = TypeDescriptor.CreateDesigner(FirstButton, typeof(IDesigner));
        if (designer == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        designer.Initialize(FirstButton);

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
                 line.Offset + FirstButton.Top, 
                 line.Filter, line.Priority));
              break;
            }
          }
        }

        return snapLines;
      }
    }
#endif


    #endregion
  }
}
