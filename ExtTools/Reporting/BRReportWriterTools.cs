using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Reporting;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Вспомогательные методы, используемые при экспорте отчетов
  /// </summary>
  internal static class BRReportWriterTools
  {
    public static void InitCellStyleKey(StringBuilder sb, BRSelector sel)
    {
      sb.Length = 0;
      sb.Append(sel.CellStyle.Format);
      sb.Append("|");
      sb.Append(sel.CellStyle.HAlign.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.VAlign.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.WrapMode.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.IndentLevel.ToString());
      sb.Append("|");

      sb.Append(sel.CellStyle.FontName);
      sb.Append("|");
      sb.Append(sel.CellStyle.FontHeightTwip.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.LineHeightTwip.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.FontWidthPercent.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.FontWidthPt.ToString());
      sb.Append("|");

      sb.Append(sel.CellStyle.BackColor.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.ForeColor.ToString());
      sb.Append("|");
      sb.Append(sel.CellStyle.Bold);
      sb.Append("|");
      sb.Append(sel.CellStyle.Italic);
      sb.Append("|");
      sb.Append(sel.CellStyle.Underline);
      sb.Append("|");
      sb.Append(sel.CellStyle.Strikeout);
      sb.Append("|");

      sb.Append(sel.CellStyle.LeftBorder.Style.ToString());
      sb.Append(sel.CellStyle.LeftBorder.Color.ToString());
      sb.Append(sel.CellStyle.TopBorder.Style.ToString());
      sb.Append(sel.CellStyle.TopBorder.Color.ToString());
      sb.Append(sel.CellStyle.RightBorder.Style.ToString());
      sb.Append(sel.CellStyle.RightBorder.Color.ToString());
      sb.Append(sel.CellStyle.BottomBorder.Style.ToString());
      sb.Append(sel.CellStyle.BottomBorder.Color.ToString());
      sb.Append(sel.CellStyle.DiagonalUp.Style.ToString());
      sb.Append(sel.CellStyle.DiagonalUp.Color.ToString());
      sb.Append(sel.CellStyle.DiagonalDown.Style.ToString());
      sb.Append(sel.CellStyle.DiagonalDown.Color.ToString());
    }

    #region Плохие символы

    /// <summary>
    /// Список плохих символов, которые не могут использоваться в значении
    /// </summary>
    internal static readonly string BadValueChars = CreateBadValueChars();

    private static string CreateBadValueChars()
    {
      StringBuilder sb = new StringBuilder(32);
      for (int i = 0; i < 32; i++)
      {
        char ch = (char)i;
        if (ch == '\n')
          continue;
        sb.Append(ch);
      }

      return sb.ToString();
    }

    #endregion
  }
}
