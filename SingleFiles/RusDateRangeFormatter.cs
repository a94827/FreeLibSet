// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

// ����� RusDateRangeFormatter �� ����� ���� �������� � ���������� ExtRussian.dll, �.�. ��� 
// �� ��������� �� ExtTools.dll
// ���� ���������� ExtRussian.dll �� ����� ��� ������������� ����� ������

namespace FreeLibSet.Russian
{
  /// <summary>
  /// ������������� ���������� ��� �� �������� �������� �����.
  /// � ���������, ��������� ����������� ���������� ���, ��������, "01-30 ���� 2017 �.".
  /// ��� ����������� �����������, ��������� � ������ ������ ���������� �����:
  /// DateRangeFormatter.Default=new RusDateRangeFormatter()
  /// </summary>
  public class RusDateRangeFormatter : DateRangeFormatter
  {
    /// <summary>
    /// ������������� ��� ���
    /// </summary>
    public static readonly IFormatProvider FormatProvider = CultureInfo.GetCultureInfo("ru-RU");

    #region ���������������� ������

    public override string ToString(DateTime date, bool isLong)
    {
      if (isLong)
        return date.ToString(@"dd MMMM yyyy �.", FormatProvider);
      else
        return date.ToString(@"dd\.MM\.yyyy", FormatProvider);
    }

    public override string ToString(DateTime? firstDate, DateTime? lastDate, bool isLong)
    {
      #region (����)�������� ���������

      if (!(firstDate.HasValue || lastDate.HasValue))
        return "��� ����";
      if (!firstDate.HasValue)
        return "�� " + ToString(lastDate.Value, isLong);
      if (!lastDate.HasValue)
        return "� " + ToString(firstDate.Value, isLong);

      #endregion

      #region �������� ���������

      if (firstDate.Value.Year != lastDate.Value.Year)
        // ��� �����������
        return ToString(firstDate.Value, isLong) + " - " + ToString(lastDate.Value, isLong);

      if (DataTools.IsBottomOfYear(firstDate.Value) && DataTools.IsEndOfYear(lastDate.Value))
      { 
        // 31.08.2018 - ����� ���
        if (isLong)
          return lastDate.Value.ToString(@"yyyy �.", FormatProvider);
        else
          return lastDate.Value.ToString(@"yyyy", FormatProvider);
      }

      if (firstDate.Value.Month != lastDate.Value.Month)
      {
        // ������������ ������ ���
        if (isLong)
          return firstDate.Value.ToString(@"dd MMMM", FormatProvider) + " - " + lastDate.Value.ToString(@"dd MMMM yyyy �.", FormatProvider);
        else
          return firstDate.Value.ToString(@"dd\.MM", FormatProvider) + "-" + lastDate.Value.ToString(@"dd\.MM\.yyyy", FormatProvider);
      }

      if (DataTools.IsBottomOfMonth(firstDate.Value) && DataTools.IsEndOfMonth(lastDate.Value))
      { 
        // 31.08.2018 - ����� �����
        if (isLong)
          return lastDate.Value.ToString(@"MMMM yyyy �.", FormatProvider);
        else
          return lastDate.Value.ToString(@"MM\.yyyy", FormatProvider);
      }

      if (firstDate.Value.Day != lastDate.Value.Day)
      { 
        // ������������ ��� � �����
        if (isLong)
          return firstDate.Value.ToString(@"dd", FormatProvider) + " - " + lastDate.Value.ToString(@"dd MMMM yyyy �.", FormatProvider);
        else
        {
          //return FirstDate.Value.ToString(@"dd\.MM", FormatProvider) + "-" + LastDate.Value.ToString(@"dd\.MM\.yyyy", FormatProvider);
          // 06.08.2018
          return firstDate.Value.ToString(@"dd", FormatProvider) + "-" + lastDate.Value.ToString(@"dd\.MM\.yyyy", FormatProvider);
        }
      }

      // ������ ����������
      return ToString(firstDate.Value, isLong);

      #endregion
    }

    public override string ToString(YearMonth firstYM, YearMonth lastYM)
    {
      if (!firstYM.IsEmpty)
      {
        if (!lastYM.IsEmpty)
        {
          if (firstYM == lastYM)
            return firstYM.BottomOfMonth.ToString("MMMM yyyy �.");
          else if (firstYM.Month==1 && lastYM.Month==12)
            return lastYM.EndOfMonth.ToString("yyyy �."); // 31.08.2018
          else
            return firstYM.BottomOfMonth.ToString("MMMM") + "-" + lastYM.EndOfMonth.ToString("MMMM yyyy �.");
        }
        else
          return "� " + firstYM.BottomOfMonth.ToString("MMMM yyyy �.");
      }
      else
      {
        if (!lastYM.IsEmpty)
          return "�� " + lastYM.EndOfMonth.ToString("MMM yyyy �.");
        else
          return "��� ����";
      }
    }

    #endregion
  }
}