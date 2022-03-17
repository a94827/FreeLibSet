// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.Collections.Specialized;
using System.Windows.Forms;
using FreeLibSet.Formatting;
using FreeLibSet.DBF;
using System.Data;
using System.Globalization;
using FreeLibSet.Collections;
using FreeLibSet.Controls;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ���������� �������� EFPGridProducer.Columns
  /// </summary>
  public class EFPGridProducerColumns : NamedList<EFPGridProducerColumn>
  {
    #region ��������

    /// <summary>
    /// ���������� ��������� ����������� �������
    /// </summary>
    public EFPGridProducerColumn LastAdded
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return this[Count - 1];
      }
    }

    #endregion

    #region ������ ���������� ��������

    #region ��������� �������

    /// <summary>
    /// ��������� ��������� �������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="textWidth">������ � ��������� ��������</param>
    /// <param name="minTextWidth">����������� ������ � ��������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddText(string columnName, string headerText, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Left;
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      item.DataType = typeof(string);
      Add(item);
      return item;
    }

    /// <summary>
    /// ��������� ����������� ��������� �������
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="textWidth">������ � ��������� ��������</param>
    /// <param name="minTextWidth">����������� ������ � ��������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddUserText(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, int textWidth, int minTextWidth)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Left;
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      item.ValueNeeded += valueNeeded;
      item.DataType = typeof(string);

      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ������� ��� ����������� ��������� ����.
    /// �������� ������ ���� ��������������.
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="textWidth">������ ������� � ��������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddInt(string columnName, string headerText, int textWidth)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = textWidth;
      item.MinTextWidth = 1;
      item.Format = "0";
      item.DataType = typeof(int);
      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ����������� ������� ��� ����������� �����.
    /// �������� ������ ���� ��������������.
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="textWidth">������ ������� � ��������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddUserInt(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, int textWidth)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;

      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = textWidth;
      item.MinTextWidth = 1;
      item.Format = "0";
      item.DataType = typeof(int);

      Add(item);
      return item;
    }


    /// <summary>
    /// �������� ������� ��� ����������� ������� (��� �����) �����.
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="textWidth">������ ������� � ��������� ��������</param>
    /// <param name="decimalPlaces">���������� ������ ����� ���������� �����. 0 - ��� ����������� ����� �����</param>
    /// <param name="sizeGroup">������ ���������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddFixedPoint(string columnName, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      if (decimalPlaces < 0)
        throw new ArgumentException("���������� ������ ����� ������� �� ����� ���� �������������", "decimalPlaces");

      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = textWidth;
      if (decimalPlaces > 0)
      {
        item.Format = "0." + new string('0', decimalPlaces);
        item.MinTextWidth = decimalPlaces + 2; // ����� ��� �����
      }
      else
      {
        item.Format = "0";
        item.MinTextWidth = 1;
      }
      item.SizeGroup = sizeGroup; // 25.12.2020
      item.DataType = typeof(double);
      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ����������� ������� ��� ����������� ������� (��� �����) �����.
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="textWidth">������ ������� � ��������� ��������</param>
    /// <param name="decimalPlaces">���������� ������ ����� ���������� �����. 0 - ��� ����������� ����� �����</param>
    /// <param name="sizeGroup">������ ���������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddUserFixedPoint(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;

      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = textWidth;
      if (decimalPlaces > 0)
      {
        item.Format = "0." + new string('0', decimalPlaces);
        item.MinTextWidth = decimalPlaces + 2; // ����� ��� �����
      }
      else
      {
        item.Format = "0";
        item.MinTextWidth = 1;
      }
      item.DataType = typeof(double);

      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ������� ��� ����������� ���� (��� ���������� �������)
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddDate(string columnName, string headerText)
    {
      return AddDateTime(columnName, headerText, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// �������� ������� ��� ����������� ���� (��� ���������� �������).
    /// ��������� ������� ����� <paramref name="columnName"/>
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddDate(string columnName)
    {
      return AddDateTime(columnName, columnName, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// �������� ����������� ������� ��� ����������� ���� (��� ���������� �������)
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddUserDate(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      return AddUserDateTime(name, sourceColumnNames, valueNeeded, headerText, EditableDateTimeFormatterKind.Date);
    }


    /// <summary>
    /// �������� ������� ��� ����������� ���� � �������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddDateTime(string columnName, string headerText)
    {
      return AddDateTime(columnName, headerText, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// �������� ������� ��� ����������� ���� �/��� �������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="kind">��� ����/�������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddDateTime(string columnName, string headerText, EditableDateTimeFormatterKind kind)
    {
      EditableDateTimeFormatter formatter = EditableDateTimeFormatters.Get(kind);

      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Center;
      item.TextWidth = formatter.TextWidth;
      item.MinTextWidth = formatter.TextWidth;
      item.SizeGroup = kind.ToString();
      item.Format = formatter.Format;
      item.FormatProvider = formatter.FormatProvider;
      item.MaskProvider = formatter.MaskProvider;
      //Item.CanIncSearch = true;
      item.DataType = typeof(DateTime);
      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ������� ��� ����������� ���� � �������.
    /// ��������� ������� ����� <paramref name="columnName"/>.
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddDateTime(string columnName)
    {
      return AddDateTime(columnName, columnName, EditableDateTimeFormatterKind.DateTime);
    }


    /// <summary>
    /// �������� ����������� ������� ��� ����������� ���� � �������
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddUserDateTime(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      return AddUserDateTime(name, sourceColumnNames, valueNeeded, headerText, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// �������� ����������� ������� ��� ����������� ���� �/��� �������
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="kind">������ ����/�������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddUserDateTime(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, EditableDateTimeFormatterKind kind)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EditableDateTimeFormatter formatter = EditableDateTimeFormatters.Get(kind);

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;

      item.TextAlign = HorizontalAlignment.Center;
      item.TextWidth = formatter.TextWidth;
      item.MinTextWidth = formatter.TextWidth;
      item.SizeGroup = kind.ToString();
      item.Format = formatter.Format;
      item.FormatProvider = formatter.FormatProvider;
      item.MaskProvider = formatter.MaskProvider;
      //Item.CanIncSearch = true;
      item.DataType = typeof(DateTime);

      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ������� ��� ����������� �������� ����
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddMoney(string columnName, string headerText)
    {
      return AddMoney(columnName, headerText, false);
    }

    /// <summary>
    /// �������� ������� ��� ����������� �������� ����
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="showPlusSign">���� true, �� ��� ������������� �������� �������� �����
    /// ������������ ���� "+". 
    /// ����� ���� ������ ��� ��������, ���������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddMoney(string columnName, string headerText, bool showPlusSign)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(columnName);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = 12;
      item.MinTextWidth = 8;
      if (showPlusSign)
        item.Format = "+0.00;-0.00;0.00";
      else
        item.Format = "0.00";
      item.SizeGroup = "Money";
      item.DataType = typeof(Decimal);
      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ������� ��� ����������� �������� ����.
    /// ��������� ������� ����� <paramref name="columnName"/>.
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddMoney(string columnName)
    {
      return AddMoney(columnName, columnName);
    }

    /// <summary>
    /// �������� ����������� ������� ��� ����������� �������� ����.
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddUserMoney(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      return AddUserMoney(name, sourceColumnNames, valueNeeded, headerText, false);
    }

    /// <summary>
    /// �������� ����������� ������� ��� ����������� �������� ����.
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="showPlusSign">���� true, �� ��� ������������� �������� �������� �����
    /// ������������ ���� "+". 
    /// ����� ���� ������ ��� ��������, ���������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddUserMoney(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText, bool showPlusSign)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerColumn item = new EFPGridProducerColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = 12;
      item.MinTextWidth = 8;
      if (showPlusSign)
        item.Format = "+0.00;-0.00;0.00";
      else
        item.Format = "0.00";
      item.SizeGroup = "Money";
      item.ValueNeeded += valueNeeded;
      item.DataType = typeof(Decimal);
      Add(item);
      return item;
    }

    /// <summary>
    /// �������� �������� �������, ���������� ������ ���� ��� ����� ������ ��������
    /// ���� decimal. ���� ��� ������� � ������ ����� �������� DBNull, �� ��������
    /// �� ���������
    /// </summary>
    /// <param name="name">�������� ��� �������</param>
    /// <param name="sourceColumnNames">����� �������� (�����������) ��������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerSumColumn AddSumMoney(string name, string sourceColumnNames,
      string headerText)
    {
      EFPGridProducerSumColumn item = new EFPGridProducerSumColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Right;
      item.TextWidth = 12;
      item.MinTextWidth = 8;
      item.Format = "0.00";
      item.SizeGroup = "Money";
      item.DataType = typeof(Decimal);
      Add(item);
      return item;
    }

    #endregion

    #region ������������ ��������

    /// <summary>
    /// ��������� ��������� ������� ��� ����������� ������������� ��������, �������
    /// ����������� �� ��������� ��������� ����.
    /// ������������ �������� ������ ���� �� ������� (0,1,2, ...).
    /// ����������� ������� ����� ��� "<paramref name="sourceColumnName"/>_Text".
    /// ��� ������ ������ "������������" ������������ ����������� AddUserText().
    /// </summary>
    /// <param name="sourceColumnName">��� �������������� �������, ����������� �������� ��������</param>
    /// <param name="textValues">������ ��������� ��������, ������� ������������ � �����������
    /// �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="textWidth">������ � ��������� ��������</param>
    /// <param name="minTextWidth">����������� ������ � ��������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerEnumColumn AddEnumText(string sourceColumnName,
      string[] textValues,
      string headerText, int textWidth, int minTextWidth)
    {
      EFPGridProducerEnumColumn item = new EFPGridProducerEnumColumn(sourceColumnName, textValues);
      item.HeaderText = headerText;
      item.TextAlign = HorizontalAlignment.Left;
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      item.DataType = typeof(string);

      Add(item);
      return item;
    }

    /// <summary>
    /// ��������� ������� ������ ��� ����������� ������������� ��������, �������
    /// ����������� �� ��������� ��������� ����.
    /// ������������ �������� ������ ���� �� ������� (0,1,2, ...).
    /// ����������� ������� ����� ��� "<paramref name="sourceColumnName"/>_Image".
    /// ��� ������ ������ "������������" ������������ ����������� AddUserImage().
    /// </summary>
    /// <param name="sourceColumnName">��� �������������� �������, ����������� �������� ��������</param>
    /// <param name="imageKeys">������ ����� � EFPApp.MainImages, ������� ������������ � �����������
    /// �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerEnumImageColumn AddEnumImage(string sourceColumnName,
      string[] imageKeys,
      string headerText)
    {
      EFPGridProducerEnumImageColumn item = new EFPGridProducerEnumImageColumn(sourceColumnName, imageKeys);
      item.HeaderText = headerText;
      item.CustomOrderSourceColumnName = sourceColumnName; // TODO: 05.07.2021. ������� ����� ��� ���������� �������� ��� AddEnumText() � AddEnumImage() �� ���������

      Add(item);
      return item;
    }
    /// <summary>
    /// ��������� ������� ������ ��� ����������� ������������� ��������, �������
    /// ����������� �� ��������� ��������� ����.
    /// ������������ �������� ������ ���� �� ������� (0,1,2, ...).
    /// ����������� ������� ����� ��� "<paramref name="sourceColumnName"/>_Image".
    /// ��� ������ ������ "������������" ������������ ����������� AddUserImage().
    /// </summary>
    /// <param name="sourceColumnName">��� �������������� �������, ����������� �������� ��������</param>
    /// <param name="imageKeys">������ ����� � EFPApp.MainImages, ������� ������������ � �����������
    /// �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerEnumImageColumn AddEnumImage(string sourceColumnName,
      string[] imageKeys)
    {
      return AddEnumImage(sourceColumnName, imageKeys, String.Empty);
    }

    #endregion

    #region ��������� ������������� ���

#if XXX
    /// <summary>
    /// ���������� ���������� ������� ��� ������ ������ � ���� � ���� ������ ("���� 2013 �.")
    /// </summary>
    /// <param name="columnPrefixName">������� ����� �������. ������� ������ ��������� ������� "����������" � "������������"</param>
    /// <param name="HeaderText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public GridProducerUserColumn AddYearMonth(string columnPrefixName, string HeaderText)
    {
      GridProducerUserColumn Col = AddUserText(columnPrefixName, columnPrefixName + "���," + columnPrefixName + "�����",
        new GridProducerUserColumnValueNeededEventHandler(YearMonthColumnValueNeeded),
        HeaderText,
        16 /* "�������� 2012 �."*/,
        12);
      Col.TextAlign = HorizontalAlignment.Center;
      Col.SizeGroup = "YearMonth";
      return Col;
    }

    private static void YearMonthColumnValueNeeded(object Sender, GridProducerUserColumnValueNeededEventArgs Args)
    {
      // TODO:
      /*
      GridProducerUserColumn Col = (GridProducerUserColumn)Sender;
      int Year = DataTools.GetInt(Args.Row, Col.FieldNames[0]);
      int Month = DataTools.GetInt(Args.Row, Col.FieldNames[1]);
      if (Year != 0)
        Args.Value = DataConv.DateLongStr(Year, Month);
       * */
    }
#endif

    /// <summary>
    /// �������� ����������� ��������� ������� ��� ����������� ��������� ��� �� ��������� ����
    /// ����� � �����.
    /// ��� ����������� ������ ������������ DateRangeFormatter.
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="firstColumnName">��� ���� � ��������� ����� ���������</param>
    /// <param name="lastColumnName">��� ���� � �������� ����� ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="longFormat">������������� �������� (true) ��� ��������� (false) ������� �����������.
    /// ��. ����� DateRangeFormatter</param>
    /// <param name="textWidth">������ � ��������� ��������</param>
    /// <param name="minTextWidth">����������� ������ � ��������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddDateRange(string name, string firstColumnName, string lastColumnName,
      string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn item = AddDateRange(name, firstColumnName, lastColumnName, headerText, longFormat);
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      return item;
    }

    /// <summary>
    /// �������� ����������� ��������� ������� ��� ����������� ��������� ��� �� ��������� ����
    /// ����� � �����.
    /// ��� ����������� ������ ������������ DateRangeFormatter.
    /// ��� ������ ��������� ������ ������� �������������.
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="firstColumnName">��� ���� � ��������� ����� ���������</param>
    /// <param name="lastColumnName">��� ���� � �������� ����� ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="longFormat">������������� �������� (true) ��� ��������� (false) ������� �����������.
    /// ��. ����� DateRangeFormatter</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddDateRange(string name, string firstColumnName, string lastColumnName,
      string headerText, bool longFormat)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(name, new string[] { firstColumnName, lastColumnName });

      item.HeaderText = headerText;
      if (longFormat)
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(DateRangeColumn_LongValueNeeded);
      else
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(DateRangeColumn_ShortValueNeeded);
      // 05.07.2019.
      // ���������� ������ �������
      if (longFormat)
        item.TextWidth = DateRangeFormatter.Default.DateRangeLongTextLength;
      else
        item.TextWidth = DateRangeFormatter.Default.DateRangeShortTextLength;
      item.MinTextWidth = item.TextWidth;
      item.EmptyValue = DateRangeFormatter.Default.ToString(null, null, longFormat);

      item.SizeGroup = longFormat ? "DateRangeLong" : "DateRangeShort";
      item.DataType = typeof(string);
      Add(item);
      return item;
    }

    internal static void DateRangeColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      DateRangeColumn_ValueNeeded(sender, args, true);
    }

    internal static void DateRangeColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      DateRangeColumn_ValueNeeded(sender, args, false);
    }
    private static void DateRangeColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
    {
      EFPGridProducerItemBase item = (EFPGridProducerItemBase)sender;
      DateTime? firstDate = args.GetNullableDateTime(item.SourceColumnNames[0]);
      DateTime? lastDate = args.GetNullableDateTime(item.SourceColumnNames[1]);
      if (firstDate.HasValue || lastDate.HasValue)
        args.Value = DateRangeFormatter.Default.ToString(firstDate, lastDate, longFormat);
    }

    /// <summary>
    /// ������� ������� ��� ����������� �������, ����������� ����� ��� � ��������� �� 1 �� 365 ��� ������ � ��� (��������� MonthDay).
    /// ��� ���������� ������������� ������������ ����� DateRangeFormatter.
    /// </summary>
    /// <param name="name">�������� ��� ������������ ������� ���������� ���������</param>
    /// <param name="sourceColumnName">��� ��������� ������� � ���� ������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="longFormat">true - ������������ ������� ������, false-������������ �������� ������</param>
    /// <param name="textWidth">������ � ��������� ��������</param>
    /// <param name="minTextWidth">����������� ������ � ��������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddMonthDay(string name, string sourceColumnName, string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn item = AddMonthDay(name, sourceColumnName, headerText, longFormat);
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      return item;
    }

    /// <summary>
    /// ������� ��������� ������� ��� ����������� ��������� ���� � ������� ��� � ��������� �� 1 �� 365 ��� ������ � ��� (��������� MonthDay).
    /// ��� ���������� ������������� ������������ ����� DateRangeFormatter.
    /// ��� ������ ��������� ������ ������� �������������.
    /// </summary>
    /// <param name="name">�������� ��� ������������ ������� ���������� ���������</param>
    /// <param name="sourceColumnName">��� ��������� ������� � ���� ������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="longFormat">true - ������������ ������� ������, false-������������ �������� ������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddMonthDay(string name, string sourceColumnName, string headerText, bool longFormat)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(name, new string[] { sourceColumnName });
      if (longFormat)
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(MonthDayColumn_LongValueNeeded);
      else
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(MonthDayColumn_ShortValueNeeded);
      item.EmptyValue = DateRangeFormatter.Default.ToString(MonthDayRange.Empty, longFormat);
      item.HeaderText = headerText;
      if (longFormat)
        item.TextWidth = DateRangeFormatter.Default.MonthDayLongTextLength;
      else
        item.TextWidth = DateRangeFormatter.Default.MonthDayShortTextLength;
      item.MinTextWidth = item.TextWidth;
      item.SizeGroup = longFormat ? "MonthDayLong" : "MonthDayShort";
      item.DataType = typeof(string);
      Add(item);
      return item;
    }

    internal static void MonthDayColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      MonthDayColumn_ValueNeeded(sender, args, true);
    }

    internal static void MonthDayColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      MonthDayColumn_ValueNeeded(sender, args, false);
    }

    private static void MonthDayColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
    {
      EFPGridProducerItemBase item = (EFPGridProducerItemBase)sender;
      int v = args.GetInt(item.SourceColumnNames[0]);
      if (v == 0)
        return;
      else if (v < 1 || v > 365)
        args.Value = "?? " + v.ToString();
      else
      {
        MonthDay md = new MonthDay(v);
        args.Value = DateRangeFormatter.Default.ToString(md, longFormat);
      }
    }

    /// <summary>
    /// ������� ������� ��� ����������� ���� �������, ���������� ����� ��� � ��������� �� 1 �� 365 ��� ��������� ���� � ���� (��������� MonthDayRange).
    /// ��� ���������� ������������� ������������ ����� DateRangeFormatter.
    /// </summary>
    /// <param name="name">�������� ��� ������������ ������� ���������� ���������</param>
    /// <param name="firstDayColumnName">��� ��������� ������� � ���� ������, ��������� ������ ���� ���������</param>
    /// <param name="lastDayColumnName">��� ��������� ������� � ���� ������, ��������� ��������� ���� ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="longFormat">true - ������������ ������� ������, false-������������ �������� ������</param>
    /// <param name="textWidth">������ � ��������� ��������</param>
    /// <param name="minTextWidth">����������� ������ � ��������� ��������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddMonthDayRange(string name, string firstDayColumnName, string lastDayColumnName, string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn item = AddMonthDayRange(name, firstDayColumnName, lastDayColumnName, headerText, longFormat);
      item.TextWidth = textWidth;
      item.MinTextWidth = minTextWidth;
      return item;
    }

    /// <summary>
    /// ������� ������� ��� ����������� ���� �������, ���������� ����� ��� � ��������� �� 1 �� 365 ��� ��������� ���� � ���� (��������� MonthDayRange).
    /// ��� ���������� ������������� ������������ ����� DateRangeFormatter.
    /// ��� ������ ��������� ������ ������� �������������.
    /// </summary>
    /// <param name="name">�������� ��� ������������ ������� ���������� ���������</param>
    /// <param name="firstDayColumnName">��� ��������� ������� � ���� ������, ��������� ������ ���� ���������</param>
    /// <param name="lastDayColumnName">��� ��������� ������� � ���� ������, ��������� ��������� ���� ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <param name="longFormat">true - ������������ ������� ������, false-������������ �������� ������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerColumn AddMonthDayRange(string name, string firstDayColumnName, string lastDayColumnName, string headerText, bool longFormat)
    {
      EFPGridProducerColumn item = new EFPGridProducerColumn(name, new string[] { firstDayColumnName, lastDayColumnName });
      if (longFormat)
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(MonthDayRangeColumn_LongValueNeeded);
      else
        item.ValueNeeded += new EFPGridProducerValueNeededEventHandler(MonthDayRangeColumn_ShortValueNeeded);
      item.HeaderText = headerText;
      if (longFormat)
        item.TextWidth = DateRangeFormatter.Default.MonthDayRangeLongTextLength;
      else
        item.TextWidth = DateRangeFormatter.Default.MonthDayRangeShortTextLength;
      item.MinTextWidth = item.TextWidth;
      item.SizeGroup = longFormat ? "MonthDayRangeLong" : "MonthDayRangeShort";
      item.DataType = typeof(string);
      Add(item);
      return item;
    }

    internal static void MonthDayRangeColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      MonthDayRangeColumn_ValueNeeded(sender, args, true);
    }

    internal static void MonthDayRangeColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      MonthDayRangeColumn_ValueNeeded(sender, args, false);
    }

    private static void MonthDayRangeColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
    {
      EFPGridProducerItemBase item = (EFPGridProducerItemBase)sender;
      int v1 = args.GetInt(item.SourceColumnNames[0]);
      int v2 = args.GetInt(item.SourceColumnNames[1]);
      if (v1 == 0 && v2 == 0)
        return;
      else if (v1 < 1 || v1 > 365 || v2 < 1 || v2 > 365)
        args.Value = "?? " + v1.ToString() + "-" + v2.ToString();
      else
      {
        MonthDayRange r = new MonthDayRange(new MonthDay(v1), new MonthDay(v2));
        args.Value = DateRangeFormatter.Default.ToString(r, longFormat);
      }
    }

    #endregion

    #region CheckBox

    /// <summary>
    /// �������� �������-������ ��� ����������� ����
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerCheckBoxColumn AddBool(string columnName, string headerText)
    {
      EFPGridProducerCheckBoxColumn item = new EFPGridProducerCheckBoxColumn(columnName);
      item.HeaderText = headerText;
      item.DataType = typeof(bool);
      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ����������� �������-������ ��� ����������� ����
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerCheckBoxColumn AddUserBool(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerCheckBoxColumn item = new EFPGridProducerCheckBoxColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;
      item.DataType = typeof(bool);

      Add(item);
      return item;
    }

    #endregion

    #region ������

    /// <summary>
    /// �������� ����������� ������� � ���������
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <param name="headerText">��������� �������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerImageColumn AddUserImage(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded,
      string headerText)
    {
      if (valueNeeded == null)
        throw new ArgumentNullException("valueNeeded");

      EFPGridProducerImageColumn item = new EFPGridProducerImageColumn(name, sourceColumnNames.Split(','));
      item.HeaderText = headerText;
      item.ValueNeeded += valueNeeded;

      Add(item);
      return item;
    }

    /// <summary>
    /// �������� ����������� ������� � ���������
    /// </summary>
    /// <param name="name">�������� ��� ������������ �������</param>
    /// <param name="sourceColumnNames">������ ���� �����, �� ��������� ������� ����������� �������� �����</param>
    /// <param name="valueNeeded">���������������� ����������, ����������� ������ ��������.
    /// ���������� ��� ���������� ������ ������ ���������</param>
    /// <returns>�������� �������</returns>
    public EFPGridProducerImageColumn AddUserImage(string name, string sourceColumnNames,
      EFPGridProducerValueNeededEventHandler valueNeeded)
    {
      return AddUserImage(name, sourceColumnNames, valueNeeded, string.Empty);
    }

    #endregion

    #endregion

    #region ������ ������

    /// <summary>
    /// ������� ������ �������� (��� �������)
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      string s = "";
      for (int i = 0; i < Count; i++)
      {
        if (i > 0)
          s += ", ";
        s += this[i].Name;
      }
      return "{" + s + "}";
    }

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region IGridProducerColumns Members
#if XXX
    public int IndexOf(IEFPGridProducerColumn ColumnProducer)
    {
      for (int i = 0; i < Count; i++)
      {
        if (this[i] == ColumnProducer)
          return i;
      }
      return -1;
    }
#endif

    #endregion
  }

  #region ��������

  /// <summary>
  /// ��������� ������� EFPGridProducerColumn.CellClick
  /// </summary>
  public class EFPGridProducerCellClickEventArgs : EFPGridProducerBaseEventArgs
  {
    #region ���������� �����������

    internal EFPGridProducerCellClickEventArgs(EFPGridProducerItemBase owner)
      : base(owner)
    {
    }

    #endregion


    // ��� ����� �������
  }

  /// <summary>
  /// ������� ������� EFPGridProducerColumn.CellClick
  /// </summary>
  /// <param name="sender">������ EFPGridProducerColumn</param>
  /// <param name="args"></param>
  public delegate void EFPGridProducerCellClickEventHandler(object sender,
    EFPGridProducerCellClickEventArgs args);


  /// <summary>
  /// ��������� ������� EFPGridProducerColumn.CellEdit
  /// </summary>
  public class EFPGridProducerCellEditEventArgs : EFPGridProducerBaseEventArgs
  {
    #region ���������� �����������

    internal EFPGridProducerCellEditEventArgs(EFPGridProducerItemBase owner)
      : base(owner)
    {
    }

    #endregion


    /// <summary>
    /// �������� ������ ���� ����������� � true, ���� �������������� ��������� � ���������� �������� �� ���������
    /// </summary>
    public bool Handled
    {
      get { return _Handled; }
      set { _Handled = value; }
    }
    private bool _Handled;
  }

  /// <summary>
  /// ������� ������� EFPGridProducerColumn.CellEdit
  /// </summary>
  /// <param name="sender">������ EFPGridProducerColumn</param>
  /// <param name="args"></param>
  public delegate void EFPGridProducerCellEditEventHandler(object sender,
    EFPGridProducerCellEditEventArgs args);

  #endregion

  /// <summary>
  /// �������� ������ ���������� �������.
  /// ������ ����������� �� ��������� ������ ��� ������� ����� ���� ������������.
  /// </summary>
  public class EFPGridProducerColumn : EFPGridProducerItemBase, IEFPGridProducerColumn
  {
    #region ������������

    /// <summary>
    /// ������� �������� ������� � �������� ������.
    /// ������ ����� ����������� �� ��������� ������ 
    /// </summary>
    /// <param name="columnName">��� �������</param>
    public EFPGridProducerColumn(string columnName)
      : this(columnName, null)
    {
    }

    /// <summary>
    /// �������� ������������ �������.
    /// ���� <paramref name="sourceColumnNames"/> ������, �� ������� ����� �����������.
    /// ���� ������� ��������� ������ �� �� ��������� ������, �� ������� ������ ������ ������ <paramref name="sourceColumnNames"/>=DataTools.EmptyStrings.
    /// </summary>
    /// <param name="name">�������� ��� ����� �������</param>
    /// <param name="sourceColumnNames">����� �������, �� ��������� ������� ������������ ����������.
    /// ���� null, �� ������� �������� �������, � �������������.
    /// ��� �������� ������������ �������, �� ������������� ������ ������ �������� (�������� ��� ��������� �����),
    /// ������� ������ ������ DataTools.EmptyStrings</param>
    public EFPGridProducerColumn(string name, string[] sourceColumnNames)
      : base(name, sourceColumnNames)
    {
      TextWidth = 5;
      MinTextWidth = 5;
      CanIncSearch = false;
      TextAlign = HorizontalAlignment.Left;
      if (sourceColumnNames == null)
        _CustomOrderSourceColumnName = name;
      else
        _CustomOrderSourceColumnName = String.Empty;
    }

    #endregion

    #region ����� �������� �������

    /// <summary>
    /// ��������� ������� � ��������� ���������
    /// </summary>
    public string HeaderText
    {
      get
      {
        if (_HeaderText == null)
          return Name;
        else
          return _HeaderText;
      }
      set
      {
        _HeaderText = value;
      }
    }
    private string _HeaderText;

    /// <summary>
    /// ������������, ����� �������� DisplayName �� ������ � ����� ����
    /// </summary>
    /// <returns></returns>
    protected override string GetDefaultDisplayName()
    {
      if (String.IsNullOrEmpty(HeaderText))
        return base.GetDefaultDisplayName();
      else
        return DataTools.RemoveDoubleChars(DataTools.ReplaceAny(HeaderText, "\r\n", ' '), ' ');
    }

    /// <summary>
    /// ����������� ��������� ��� ��������� ������� �� ������� �������.
    /// ���� �������� �� ����������� � ����� ����, ���������� DisplayName
    /// </summary>
    public string HeaderToolTipText
    {
      get
      {
        if (String.IsNullOrEmpty(_HeaderToolTipText))
          return DisplayName;
        else
          return _HeaderToolTipText;
      }
      set
      {
        _HeaderToolTipText = value;
      }
    }
    private string _HeaderToolTipText;

    /// <summary>
    /// ������ ������� � ��������
    /// </summary>
    public int TextWidth
    {
      get { return _TextWidth; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", value, "������ ������� �� ����� ���� ������ 1 �������");
        _TextWidth = value;
      }
    }
    private int _TextWidth;

    /// <summary>
    /// ����������� ������ ������� � ��������
    /// </summary>
    public int MinTextWidth
    {
      get { return _MinTextWidth; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", value, "����������� ������ ������� �� ����� ���� ������ 1 �������");
        _MinTextWidth = value;
      }
    }
    private int _MinTextWidth;

    private const string NonResizableSizeGroup = "-";

    /// <summary>
    /// ��� ������ ��� ����������� ��������� ��������. ��. EFPDataGridViewColumn.SizeGroup
    /// ��� �������� ������������ �� ������� �������� Resizable=false
    /// </summary>
    public string SizeGroup
    {
      get
      {
        if (_SizeGroup == NonResizableSizeGroup)
          return String.Empty;
        else
          return _SizeGroup;
      }
      set { _SizeGroup = value; }
    }
    private string _SizeGroup;

    /// <summary>
    /// ���� true (�� ���������), �� ������������ ����� ������ ������ �������.
    /// </summary>
    public bool Resizable
    {
      get { return _SizeGroup != NonResizableSizeGroup; }
      set
      {
        if (value)
        {
          if (_SizeGroup == NonResizableSizeGroup)
            _SizeGroup = String.Empty;
        }
        else
          _SizeGroup = NonResizableSizeGroup;
      }
    }

    /// <summary>
    /// �������������� ������������ ������ � �������
    /// </summary>
    public HorizontalAlignment TextAlign { get { return _TextAlign; } set { _TextAlign = value; } }
    private HorizontalAlignment _TextAlign;

    /// <summary>
    /// ����� ����� ������ ��� ����������� � ������.
    /// �������� �� ���������: 1. ���� ������ ��������, ������� 1, �� ��� ������� 
    /// ��������������� �������� DataGridViewCellStyle.WrapMode=True, � ������
    /// ���� ����� � ��������� ����� ���������
    /// </summary>
    public int TextRowHeight { get { return _TextRowHeight; } set { _TextRowHeight = value; } }
    private int _TextRowHeight;

    /// <summary>
    /// �������������� ������ ��� �����������.
    /// ������������ ��� ��������� �������� DataGridViewCellStyle.Format.
    /// </summary>
    public string Format { get { return _Format; } set { _Format = value; } }
    private string _Format;

    /// <summary>
    /// ������������ ��� �������.
    /// ������������ ��� ��������� �������� DataGridViewCellStyle.FormatProvider.
    /// </summary>
    public IFormatProvider FormatProvider { get { return _FormatProvider; } set { _FormatProvider = value; } }
    private IFormatProvider _FormatProvider;

    /// <summary>
    /// ��������� � true ������������� �������������� ������� "�� �����"
    /// </summary>
    public bool ReadOnly { get { return _ReadOnly; } set { _ReadOnly = value; } }
    private bool _ReadOnly;

    /// <summary>
    /// ��������� � true ��������� ������� ����� �� ������ ������ ��� ����� �������
    /// </summary>
    public bool CanIncSearch { get { return _CanIncSearch; } set { _CanIncSearch = value; } }
    private bool _CanIncSearch;

    /// <summary>
    /// ������������ ��� ������ �� ������ ������
    /// </summary>
    public IMaskProvider MaskProvider { get { return _MaskProvider; } set { _MaskProvider = value; } }
    private IMaskProvider _MaskProvider;

    /// <summary>
    /// �������� ���������� ��� �������
    /// </summary>
    public EFPDataGridViewColorType ColorType { get { return _ColorType; } set { _ColorType = value; } }
    private EFPDataGridViewColorType _ColorType;

    /// <summary>
    /// ������� ������ ������� �������
    /// </summary>
    public bool Grayed { get { return _Grayed; } set { _Grayed = value; } }
    private bool _Grayed;

    /// <summary>
    /// ������ ������� ��� ���������� ��������� � DBF-�������
    /// </summary>
    public DbfFieldInfo DbfInfo { get { return _DbfInfo; } set { _DbfInfo = value; } }
    private DbfFieldInfo _DbfInfo;


    /// <summary>
    /// ��� �������, ������������� ��� ������������ ����������.
    /// �� ��������� ��� ������������� �������� ��������������� ������ Name.
    /// ��� ����������� �������� - ������ ������
    /// </summary>
    public string CustomOrderSourceColumnName { get { return _CustomOrderSourceColumnName; } set { _CustomOrderSourceColumnName = value; } }
    private string _CustomOrderSourceColumnName;

    /// <summary>
    /// ��� ������ ��� �������. ������������ � �������-����������� EFPGridProducerDataTableRepeater
    /// </summary>
    public Type DataType { get { return _DataType; } set { _DataType = value; } }
    private Type _DataType;

    #endregion

    #region �������� ������� ��� ���������� ��������� EFPDataGridView

    /// <summary>
    /// ������� ������ ������� ��� ���������� ��������� Windows Forms.
    /// ������� �� ����������� � ��������.
    /// </summary>
    /// <returns>������ �������, ����������� �� DataGridViewColumn</returns>
    public virtual DataGridViewColumn CreateColumn()
    {
      DataGridViewTextBoxColumn gridColumn = new DataGridViewTextBoxColumn();
      InitColumn(gridColumn);
      return gridColumn;
    }

    /// <summary>
    /// �������������� �������� DataGridViewColumn.
    /// ������������ � ������� CreateColumn()
    /// </summary>
    /// <param name="column"></param>
    protected void InitColumn(DataGridViewColumn column)
    {
      column.Name = Name;
      if (!IsCalculated)
        column.DataPropertyName = Name;
      column.HeaderText = HeaderText;
      column.ToolTipText = HeaderToolTipText;
      switch (TextAlign)
      {
        case HorizontalAlignment.Left:
          column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
          break;
        case HorizontalAlignment.Center:
          column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
          break;
        case HorizontalAlignment.Right:
          column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
          break;
      }

      if (TextRowHeight > 1)
        column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

      column.DefaultCellStyle.Format = Format;
      if (FormatProvider != null)
        column.DefaultCellStyle.FormatProvider = FormatProvider;
    }

    /// <summary>
    /// ��������� ����������� ������������ ���������� ��������� � ������� Windows Forms.
    /// ������������� �������� DataGridViewColumn.AutoSizeMode, Width, FillWeight �, ��������, ������. 
    /// </summary>
    /// <param name="column">������� Windows Forms, </param>
    /// <param name="config">������������ �������</param>
    /// <param name="controlProvider">��������� ���������� ���������</param>
    public virtual void ApplyConfig(DataGridViewColumn column, EFPDataGridViewConfigColumn config, EFPDataGridView controlProvider)
    {
      bool IsImgColumn = column is DataGridViewImageColumn || column is DataGridViewCheckBoxColumn;
      if (config.FillMode)
      {
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        column.FillWeight = config.FillWeight;
      }
      else
      {
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        if (config.Width == 0)
          column.Width = GetWidth(controlProvider.Measures);
        else
          column.Width = config.Width;
        column.FillWeight = 1; // 08.02.2017
      }
      if (IsImgColumn)
        column.MinimumWidth = column.Width;
      else
        column.MinimumWidth = controlProvider.Measures.GetTextColumnWidth(MinTextWidth);

      if (!Resizable)
        column.Resizable = DataGridViewTriState.False;

      if (ReadOnly)
        column.ReadOnly = true;
    }

    #endregion

    #region �������� ������� �������������� ��������� EFPDataTreeView

    /// <summary>
    /// ������� ������� ��� �������������� ��������� TreeViewAdv
    /// </summary>
    /// <param name="config">������������ �������</param>
    /// <returns>������� TreeViewAdv</returns>
    public virtual TreeColumn CreateTreeColumn(EFPDataGridViewConfigColumn config)
    {
      TreeColumn column = new TreeColumn(DisplayName, TextWidth * 10);
      /*
      if (Config.FillMode)
      {
        Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Column.FillWeight = Config.FillWeight;
      } */
      column.TextAlign = this.TextAlign;
      column.TooltipText = HeaderToolTipText;

      return column;
    }

    /// <summary>
    /// ������� �������������� ������� ��� ������� TreeViewAdv.
    /// ������������������ ����� ���������� NodeTextBox
    /// </summary>
    /// <returns>�������������� �������</returns>
    public virtual BindableControl CreateNodeControl()
    {
      NodeTextBox tb = new NodeTextBox();
      tb.EditEnabled = !ReadOnly;
      tb.TextAlign = TextAlign; // 15.03.2019
      return tb;
    }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    /// <param name="nodeControl"></param>
    /// <param name="config"></param>
    /// <param name="controlProvider"></param>
    public virtual void ApplyConfig(BindableControl nodeControl, EFPDataGridViewConfigColumn config, EFPDataTreeView controlProvider)
    {
      /*
      bool IsImgColumn = Column is DataGridViewImageColumn || Column is DataGridViewCheckBoxColumn;
      if (Config.FillMode)
      {
        Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Column.FillWeight = Config.FillWeight;
      }
      else
      {
        Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        if (Config.Width == 0)
        {
          if (IsImgColumn)
          {
            if (Column is DataGridViewImageColumn)
              Column.Width = ControlProvider.Measures.ImageColumnWidth;
            else
              Column.Width = ControlProvider.Measures.CheckBoxColumnWidth;
          }
          else
            Column.Width = ControlProvider.Measures.GetTextColumnWidth(TextWidth);
        }
        else
          Column.Width = Config.Width;
      }
      if (IsImgColumn)
        Column.MinimumWidth = Column.Width;
      else
        Column.MinimumWidth = ControlProvider.Measures.GetTextColumnWidth(MinTextWidth);
        */

    }

    #endregion

    #region ��������� ��������

    /// <summary>
    /// ��������� �������� ������������ ����. �������� ����������� ����� OnValueNeeded().
    /// ���� �������/��������� �� �������� �����������, ������������ �������� <paramref name="rowInfo"/>.Values.GetValue(Name).
    /// </summary>
    /// <param name="rowInfo">���������� � ������</param>
    /// <returns>����������� ��������</returns>
    public object GetValue(EFPDataViewRowInfo rowInfo)
    {
      object value;
      string toolTipText;
      base.DoGetValue(EFPGridProducerValueReason.Value, rowInfo, out value, out toolTipText);
      return value;
    }


    /// <summary>
    /// ���������� ��� ������������� ��������� ����������� ��������� ��� ������ ������
    /// ��� ��������� �������. ���� ������������ ������ ������, �� ��� �������������� ��������� ��� ������
    /// </summary>
    /// <param name="rowInfo">���������� � ������</param>
    /// <param name="columnName">������������. ������� ��� ��������� � ���������� �������������</param>
    /// <returns>����� ���������</returns>
    public string GetCellToolTipText(EFPDataViewRowInfo rowInfo, string columnName)
    {
      object value;
      string toolTipText;
      base.DoGetValue(EFPGridProducerValueReason.ToolTipText, rowInfo, out value, out toolTipText);

      if (toolTipText == null)
        return String.Empty;
      else
        return toolTipText;
    }

    #endregion

    #region ������� GetCellAttributes

    /// <summary>
    /// ������� ���������� ��� �������������� ������ � ������������ ������� 
    /// �������� ��������� � ������������������ ��������
    /// </summary>
    public event EFPDataGridViewCellAttributesEventHandler GetCellAttributes;

    ///// <summary>
    ///// ���������� true, ���� ��� ������� ������ ������������� �����������
    ///// </summary>
    //public bool HasGetCellAttributes
    //{
    //  get { return GetCellAttributes != null; }
    //}

    /// <summary>
    /// ����� ������� GetCellAttributes �� DocGridHandler
    /// </summary>
    /// <param name="args">���������, ������������ �����������</param>
    internal void OnGetCellAttributes(EFPDataGridViewCellAttributesEventArgs args)
    {
      if (GetCellAttributes != null)
        GetCellAttributes(this, args);
    }

    #endregion

    #region ������� CellClick

    /// <summary>
    /// ������� ���������� ��� ��������� ������ ����� ������ ���� �� ������ �������.
    /// ������������ ��� �������� CheckBox.
    /// </summary>
    public event EFPGridProducerCellClickEventHandler CellClick;

    /// <summary>
    /// ���������� ��� ��������� ������ ����� ������ ���� �� ������ �������.
    /// �������� ���������� ������� CellClick, ���� �� ����������
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected virtual void OnCellClick(EFPGridProducerCellClickEventArgs args)
    {
      if (CellClick != null)
        CellClick(this, args);
    }

    /// <summary>
    /// ���������� ��� ��������� ������ ����� ������ ���� �� ������ �������.
    /// ������������ ��� �������� CheckBox.
    /// �������� ����������� ����� OnCellClick()
    /// </summary>
    /// <param name="rowInfo">���������� � ������</param>
    /// <param name="columnName">������������, �.�. EFPGridProducerColumn � ������ ��������� � ������������� ������� ���������</param>
    public void PerformCellClick(EFPDataViewRowInfo rowInfo, string columnName)
    {
      EFPGridProducerCellClickEventArgs args = new EFPGridProducerCellClickEventArgs(this);
      args.RowInfo = rowInfo;
      OnCellClick(args);
    }

    #endregion

    #region ������� CellEdit

    /// <summary>
    /// ������� ���������� ��� ������� �������������� ������, ��������� �� ��������.
    /// ����� ���������� �� ����������� ���������, ��������, ������� ��������� EFPDataGridView.Editdata
    /// </summary>
    public event EFPGridProducerCellEditEventHandler CellEdit;

    /// <summary>
    /// �������� ���������� ������� CellEdit, ���� �� ����������
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnCellEdit(EFPGridProducerCellEditEventArgs args)
    {
      if (CellEdit != null)
        CellEdit(this, args);
    }

    /// <summary>
    /// ���������� ��� ������� �������������� ������, ��������� �� ��������.
    /// ����� ���������� �� ����������� ���������, ��������, ������� ��������� EFPDataGridView.Editdata
    /// </summary>
    /// <param name="rowInfo">���������� � ������ ���������</param>
    /// <param name="columnName">������������, �.�. EFPGridProducerColumn � ������ ��������� � ������������� ������� ���������</param>
    /// <returns>true, ���� �������������� ��������� � ���������� �������������� �� ������ �����������</returns>
    public bool PerformCellEdit(EFPDataViewRowInfo rowInfo, string columnName)
    {
      EFPGridProducerCellEditEventArgs args = new EFPGridProducerCellEditEventArgs(this);
      args.RowInfo = rowInfo;
      OnCellEdit(args);
      return args.Handled;
    }

    #endregion

    #region �������� ��� ������

    /// <summary>
    /// ������������� ��������� ��� ������ �������
    /// </summary>
    public string[] PrintHeaders { get { return _PrintHeaders; } set { _PrintHeaders = value; } }
    private string[] _PrintHeaders;

    /// <summary>
    /// ������������� ��������� ��� ������ ������� (�������� PrintHeaders)
    /// ������ ��� ��������� � ���� ����� ������ � ������� ��������:
    /// "|" - ����������� �������������� ���������
    /// "^" - ������ �������
    /// "_" - ����������� ������
    /// </summary>
    public string PrintHeadersSpec
    {
      get { return DataTools.StrFromSpecCharsArray(PrintHeaders); }
      set { PrintHeaders = DataTools.StrToSpecCharsArray(value); }
    }

    #endregion

    #region ��������������� ������

    /// <summary>
    /// �������� ����� �����, ������� ������ ���� � ������ ������.
    /// ���� ������� �������� �����������, � ������ ����������� ����� �������� �������� SourceColumnNames.
    /// ����� ����������� ��� Name ��� �������������� �������/���������/.
    /// ����� � ������ ����������� CustomOrderSourceColumnName.
    /// </summary>
    /// <param name="columns">������ ��� ���������� ���� �����</param>
    public override void GetColumnNames(IList<string> columns)
    {
      base.GetColumnNames(columns);
      if (!String.IsNullOrEmpty(CustomOrderSourceColumnName))
        columns.Add(CustomOrderSourceColumnName);
    }

    /// <summary>
    /// ���������� �������� ������ ������� � ��������.
    /// ������������������ ����� ��������� ������ ������ �� �������� TextWidth.
    /// ���������������� ��� �������� c CheckBox � �������
    /// </summary>
    /// <param name="measures">������ ��� ���������� ��������, �������������� � ���������� ���������.</param>
    /// <returns>������ � ��������</returns>
    public virtual int GetWidth(IEFPGridControlMeasures measures)
    {
      int tw = this.TextWidth;
      if (tw < 1)
        tw = 1; // ���� �� �����������
      return measures.GetTextColumnWidth(/*TextWidth*/tw /*25.12.2020*/);
    }

    ///// <summary>
    ///// ���������� ��� ���������� ���������� ���������. ���� ������� ����������
    ///// �����������, �� ��� ������ ���� ��������
    ///// </summary>
    //public virtual void Refresh()
    //{
    //}

    #endregion
  }

  /// <summary>
  /// ����������� ����� ������� ���� CheckBox
  /// </summary>
  public class EFPGridProducerCheckBoxColumn : EFPGridProducerColumn
  {
    #region ������������

    /// <summary>
    /// ������� �������, ������������ �������� ����
    /// </summary>
    /// <param name="columnName">��� �������</param>
    public EFPGridProducerCheckBoxColumn(string columnName)
      : this(columnName, null)
    {
    }


    /// <summary>
    /// ������� ����������� �������
    /// </summary>
    /// <param name="name">�������� ��� ����� �������</param>
    /// <param name="sourceColumnNames">����� �������, �� ��������� ������� ������������ ����������.
    /// ���� null, �� ������� �������� �������, � �� �����������.</param>
    public EFPGridProducerCheckBoxColumn(string name, string[] sourceColumnNames)
      : base(name, sourceColumnNames)
    {
      TextWidth = 3;
      MinTextWidth = 3;
      SizeGroup = "CheckBox";
      TextAlign = HorizontalAlignment.Center;
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������� ������ ������� ��� ���������� ��������� Windows Forms.
    /// ������� �� ����������� � ��������.
    /// </summary>
    /// <returns>������ �������</returns>
    public override DataGridViewColumn CreateColumn()
    {
      ExtDataGridViewCheckBoxColumn column = new ExtDataGridViewCheckBoxColumn();
      InitColumn(column);
      return column;
    }

    /// <summary>
    /// ���������� �������� ������ ������� � ��������.
    /// </summary>
    /// <param name="measures">������ ��� ���������� ��������, �������������� � ���������� ���������.</param>
    /// <returns>������ � ��������</returns>
    public override int GetWidth(IEFPGridControlMeasures measures)
    {
      return measures.CheckBoxColumnWidth;
    }

    #endregion
  }


  /// <summary>
  /// ����������� ������� � ������������
  /// </summary>
  public class EFPGridProducerImageColumn : EFPGridProducerColumn
  {
    #region �����������

    /// <summary>
    /// ������� ��������� �������
    /// </summary>
    /// <param name="columnName">��� ������������ ������� �����������</param>
    /// <param name="sourceColumnNames">����� �������� �������. ����� ����������� ��������</param>
    public EFPGridProducerImageColumn(string columnName, string[] sourceColumnNames)
      : base(columnName, sourceColumnNames)
    {
      TextWidth = 3;
      MinTextWidth = 3;
      SizeGroup = "Image";
      TextAlign = HorizontalAlignment.Center;
      CustomOrderSourceColumnName = String.Empty;
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������� ������ ������� ��� ���������� ��������� Windows Forms.
    /// ������� �� ����������� � ��������.
    /// </summary>
    /// <returns>������ �������</returns>
    public override DataGridViewColumn CreateColumn()
    {
      DataGridViewImageColumn col = new DataGridViewImageColumn();
      InitColumn(col);
      return col;
    }

    /// <summary>
    /// ���������� �������� ������ ������� � ��������.
    /// </summary>
    /// <param name="measures">������ ��� ���������� ��������, �������������� � ���������� ���������.</param>
    /// <returns>������ � ��������</returns>
    public override int GetWidth(IEFPGridControlMeasures measures)
    {
      return measures.ImageColumnWidth;
    }

    #endregion
  }


  /// <summary>
  /// ������� ������, ������������ ����������� �������� ��
  /// ��������� ������ �������������� ������� ������, ����������� ������������ �������� 0,1,2,...
  /// </summary>
  public class EFPGridProducerEnumColumn : EFPGridProducerColumn
  {
    #region �����������

    /// <summary>
    /// �������� ����������������� �������.
    /// ������� �������� ��� "<paramref name="sourceColumnName"/>_Text".
    /// </summary>
    /// <param name="sourceColumnName">��� ��������� ������� � ������� ������</param>
    /// <param name="textValues">������ ��������� ��������</param>
    public EFPGridProducerEnumColumn(string sourceColumnName, string[] textValues)
      : this(sourceColumnName + "_Text", new string [] { sourceColumnName }, textValues)
    {
    }


    /// <summary>
    /// ��� ������ ������������� ��� �������� �������-�����������. ������������ � ExtDBDocForms.dll.
    /// </summary>
    /// <param name="name">�������� ��� ����� �������</param>
    /// <param name="sourceColumnNames">�������� �������</param>
    /// <param name="textValues">������ ��������� ��������</param>
    protected EFPGridProducerEnumColumn(string name, string[] sourceColumnNames, string[] textValues)
      : base(name, sourceColumnNames)
    {
      if (textValues == null)
        throw new ArgumentNullException("textValues");
      _TextValues = textValues;
      _NullIsZero = true;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� �������� ��� ������������
    /// </summary>
    public string[] TextValues { get { return _TextValues; } }
    private string[] _TextValues;

    /// <summary>
    /// ���� true (�� ���������), �� �������� ���� NULL ���������������� ��� 0.
    /// ���� false, �� ��� �������� NULL ����� ���������� ������ ��������
    /// </summary>
    public bool NullIsZero { get { return _NullIsZero; } set { _NullIsZero = value; } }
    private bool _NullIsZero;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ��������� ��������
    /// </summary>
    /// <param name="args"></param>
    protected override void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      object val = GetSourceValue(args);
      if (val is DBNull && (!NullIsZero))
        args.Value = String.Empty;
      else
      {
        int srcVal = DataTools.GetInt(val);
        if (srcVal < 0 || srcVal >= TextValues.Length)
          args.Value = "?? " + srcVal.ToString();
        else
          args.Value = TextValues[srcVal];
      }

      base.OnValueNeeded(args);
    }

    /// <summary>
    /// ���������� �������� ��������� ����.
    /// ������������������ ����� ���������� �������� ��������� ����.
    /// </summary>
    /// <param name="args">��������� ������� ValueNeeded</param>
    /// <returns>�������� ����</returns>
    protected virtual object GetSourceValue(EFPGridProducerValueNeededEventArgs args)
    {
      return args.Values.GetValue(SourceColumnNames[0]);
    }

    #endregion
  }

  /// <summary>
  /// ������� ������ � ������������� �� ������ EFPApp.ImageKeys.
  /// ����������� ������� �� ��������� ������ �������������� ������� ������, ����������� ������������ ��������.
  /// </summary>
  public class EFPGridProducerEnumImageColumn : EFPGridProducerImageColumn
  {
    #region �����������

    /// <summary>
    /// �������� ����������������� �������.
    /// ������� �������� ��� "<paramref name="sourceColumnName"/>_Image".
    /// </summary>
    /// <param name="sourceColumnName">��� ��������� �������, ����������� ������������ ��������</param>
    /// <param name="imageKeys">������ ����� ����������� � EFPApp.ImageKeys</param>
    public EFPGridProducerEnumImageColumn(string sourceColumnName, string[] imageKeys)
      : this(sourceColumnName + "_Image", new string [] { sourceColumnName }, imageKeys)
    {
    }

    /// <summary>
    /// ��� ������ ������������� ��� �������� �������-�����������. ������������ � ExtDBDocForms.dll.
    /// </summary>
    /// <param name="name">�������� ��� ����� �������</param>
    /// <param name="sourceColumnNames">�������� �������</param>
    /// <param name="imageKeys">������ ����� ����������� � EFPApp.ImageKeys</param>
    protected EFPGridProducerEnumImageColumn(string name, string[] sourceColumnNames, string[] imageKeys)
      : base(name, sourceColumnNames)
    {
      if (imageKeys == null)
        throw new ArgumentNullException("imageKeys");
      _ImageKeys = imageKeys;
      _NullIsZero = true;
      _ErrorImageKey = "Error";
      _EmptyToolTipText = String.Empty;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ ����� ����������� � EFPApp.ImageKeys ��� ������������.
    /// �������� � ������������
    /// </summary>
    public string[] ImageKeys { get { return _ImageKeys; } }
    private string[] _ImageKeys;

    /// <summary>
    /// ���� true (�� ���������), �� �������� ���� NULL ���������������� ��� 0.
    /// ���� false, �� ��� �������� NULL ����� ���������� ������ �����������
    /// </summary>
    public bool NullIsZero { get { return _NullIsZero; } set { _NullIsZero = value; } }
    private bool _NullIsZero;

    /// <summary>
    /// �����������, ������������, ���� �������� ���� ������� �� ������� ������ ImageKeys.
    /// �� ��������� - "Error".
    /// </summary>
    public string ErrorImageKey { get { return _ErrorImageKey; } set { _ErrorImageKey = value; } }
    private string _ErrorImageKey;

    /// <summary>
    /// ������ ����������� ���������, ��������������� ������� ImageKeys.
    /// </summary>
    public string[] ToolTipTexts
    {
      get { return _ToolTipTexts; }
      set
      {
        if (value != null)
        {
          if (value.Length != _ImageKeys.Length)
            throw new ArgumentException("������������ ����� ������� ���������");
          _ToolTipTexts = value;
        }
      }
    }
    private string[] _ToolTipTexts;

    /// <summary>
    /// ���������� ���������, ��������������� �������� NULL, ��� NullIsZero=false.
    /// �� ��������� - ������ ������
    /// </summary>
    public string EmptyToolTipText
    {
      get { return _EmptyToolTipText; }
      set { _EmptyToolTipText = value; }
    }
    private string _EmptyToolTipText;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ��������� ��������
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected override void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      object val = GetSourceValue(args);
      string imageKey;
      if (val is DBNull && (!NullIsZero))
      {
        imageKey = "EmptyImage";
        args.ToolTipText = EmptyToolTipText;
      }
      else
      {
        int srcVal = DataTools.GetInt(val);
        if (srcVal < 0 || srcVal >= ImageKeys.Length)
        {
          imageKey = ErrorImageKey;
          args.ToolTipText = "������������ ��������: " + srcVal.ToString();
        }
        else
        {
          imageKey = ImageKeys[srcVal];
          if (ToolTipTexts != null)
            args.ToolTipText = ToolTipTexts[srcVal];
        }
      }
      args.Value = EFPApp.MainImages.Images[imageKey];

      base.OnValueNeeded(args);
    }

    /// <summary>
    /// ���������� �������� ��������� ����.
    /// ������������������ ����� ���������� �������� ��������� ����.
    /// </summary>
    /// <param name="args">��������� ������� ValueNeeded</param>
    /// <returns>�������� ����</returns>
    protected virtual object GetSourceValue(EFPGridProducerValueNeededEventArgs args)
    {
      return args.Values.GetValue(SourceColumnNames[0]);
    }

    #endregion
  }

  /// <summary>
  /// �������, ���������� ������ ����� � ��������� ��������� (1,2, ...)
  /// </summary>
  public class EFPGridProducerRowOrderColumn : EFPGridProducerColumn
  {
    #region ������������

    /// <summary>
    /// ������� ����������� ������� c �������� ������ � ������������ �������� ���������
    /// </summary>
    public EFPGridProducerRowOrderColumn(string name, string filterColumnName, int filterValue)
      : base(name, GetSourceColumnNames(filterColumnName))
    {
      HeaderText = "� �/�";
      TextAlign = HorizontalAlignment.Right;
      _FilterColumnName = filterColumnName;
      _FilterValue = filterValue;
    }

    private static string[] GetSourceColumnNames(string filterColumnName)
    {
      if (String.IsNullOrEmpty(filterColumnName))
        return DataTools.EmptyStrings;
      else
        return new string[] { filterColumnName };
    }

    /// <summary>
    /// ������� ����������� ������� c �������� ������. ���������� ��� ������
    /// </summary>
    public EFPGridProducerRowOrderColumn(string name)
      : this( name, String.Empty, 0)
    {
    }

    /// <summary>
    /// ������� ����������� ������� � ������ "RowOrder"
    /// </summary>
    public EFPGridProducerRowOrderColumn()
      : this("RowOrder")
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ���� ��� ����������� �����, ���������� ���������
    /// ���� �������� �� ������, ���������� ��� ������
    /// ����� ��������� ��������� ������ ��� �����, ���������� ������.
    /// ������ ������ �������� ������ ����. ��������� �� ����������, �������
    /// ����� ������������ ������ ��� �������� �������� ������.
    /// �������� � ������������
    /// </summary>
    public string FilterColumnName { get { return _FilterColumnName; } }
    private string _FilterColumnName;

    /// <summary>
    /// �������� ���� ��� �������, ��������� FilterColumnName
    /// �������� � ������������
    /// </summary>
    public int FilterValue { get { return _FilterValue; } }
    private int _FilterValue;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ��������� ��������
    /// </summary>
    /// <param name="args"></param>
    protected override void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      args.Value = args.RowIndex + 1;
      if (!String.IsNullOrEmpty(FilterColumnName))
      {
        int Value = args.GetInt(FilterColumnName);
        if (Value != FilterValue)
          args.Value = null;
      }
      base.OnValueNeeded(args);
    }

    #endregion
  }

#if XXX

  #region ������������ GridProducerDBFileInfoColumnKind

  /// <summary>
  /// ��� ������������ ������ ��� ������� GridProducerDBFileInfoColumn
  /// </summary>
  public enum GridProducerDBFileInfoColumnKind
  {
    /// <summary>
    /// ��� �����
    /// </summary>
    Name,

    /// <summary>
    /// ������ ����� � ������
    /// </summary>
    Length,

    /// <summary>
    /// ����� �������� �����
    /// </summary>
    CreationTime,

    /// <summary>
    /// ����� ��������� ������ �����
    /// </summary>
    LastWriteTime
  }

  #endregion

  /// <summary>
  /// ������� ��� ������ ��������� �����, ����������� � ���� ������
  /// �������� ����������� � ������� ������ AccDepClientExec.GetDBFileInfo()
  /// </summary>
  public class GridProducerDBFileInfoColumn : GridProducerColumn
  {
  #region ������������

    /// <summary>
    /// �������� �������.
    /// ���������� ������ ������ ���� �������� � �������� ��������� � �������
    /// GridProducerColumns.Add()
    /// ������ � ������������ ������� ����� ����
    /// </summary>
    /// <param name="Kind">��� ������� (��� �����, ������, �����)</param>
    /// <param name="SourceTableName">��� ������� � ���� ������, ������� ������ ������ �� ����</param>
    /// <param name="SourceColumnName">��� ���� � ������� ���� ������, ������� �������� ������ �� ����</param>
    /// <param name="ColumnName">��� ������������ ������� (��������, "��������"). �� ������ ��������� � ������ ���� � ���� ������.
    /// ���� �� ������, �� ��� ��������� ������������� �� ������ ����� ��������� ������� � ����</param>
    public GridProducerDBFileInfoColumn(GridProducerDBFileInfoColumnKind Kind, string SourceTableName, string SourceColumnName, string ColumnName)
      : base(GetColumnName(ColumnName, SourceColumnName, Kind))
    {
#if DEBUG
      if (String.IsNullOrEmpty(SourceTableName))
        throw new ArgumentNullException("SourceTableName");
      if (String.IsNullOrEmpty(SourceColumnName))
        throw new ArgumentNullException("SourceColumnName");
      if (ColumnName == SourceColumnName)
        throw new ArgumentException("��� ������� �� ������ ��������� � ������ ��������� ������� \"" +
          SourceColumnName + "\"", "SourceColumnName");
#endif
      FSourceTableName = SourceTableName;
      FSourceColumnName = SourceColumnName;
      FKind = Kind;
      switch (Kind)
      {
        case GridProducerDBFileInfoColumnKind.Name:
          HeaderText = "��� �����";
          Align = HorizontalAlignment.Left;
          break;

        case GridProducerDBFileInfoColumnKind.Length:
          HeaderText = "������, ����";
          Align = HorizontalAlignment.Right;
          SizeGroup = "FileLength";
          TextWidth = 11;
          MinTextWidth = 7;
          Format = "# ### ### ##0";
          FormatProvider = DataConv.DotNumberConvWithGroups;
          break;

        case GridProducerDBFileInfoColumnKind.CreationTime:
          HeaderText = "����� ��������";
          Align = HorizontalAlignment.Center;
          TextWidth = 19;
          MinTextWidth = 19;
          SizeGroup = "DateTime";
          Format = "dd/MM/yyyy HH:mm:ss";
          FormatProvider = DataConv.DotDateTimeConv;
          break;

        case GridProducerDBFileInfoColumnKind.LastWriteTime:
          HeaderText = "����� ������";
          Align = HorizontalAlignment.Center;
          TextWidth = 19;
          MinTextWidth = 19;
          SizeGroup = "DateTime";
          Format = "dd/MM/yyyy HH:mm:ss";
          FormatProvider = DataConv.DotDateTimeConv;
          break;

        default:
          throw new ArgumentException("����������� ��� �������: " + Kind.ToString(), "Kind");
      }
    }

    private static string GetColumnName(string ColumnName, string SourceColumnName, GridProducerDBFileInfoColumnKind Kind)
    {
#if DEBUG
      if (String.IsNullOrEmpty(SourceColumnName))
        throw new ArgumentNullException("SourceColumnName");
#endif
      if (String.IsNullOrEmpty(ColumnName))
      {
        string Suffix;
        switch (Kind)
        {
          case GridProducerDBFileInfoColumnKind.Name:
            Suffix = "���"; break;
          case GridProducerDBFileInfoColumnKind.Length:
            Suffix = "������"; break;
          case GridProducerDBFileInfoColumnKind.CreationTime:
            Suffix = "�������������"; break;
          case GridProducerDBFileInfoColumnKind.LastWriteTime:
            Suffix = "�����������"; break;
          default:
            throw new ArgumentException("����������� Kind");
        }
        return SourceColumnName + Suffix;
      }
      else
        return ColumnName;
    }

    /// <summary>
    /// �������� �������.
    /// ���������� ������ ������ ���� �������� � �������� ��������� � �������
    /// GridProducerColumns.Add()
    /// ������ ��� ��������������� ����� ����
    /// </summary>
    /// <param name="Kind">��� ������� (��� �����, ������, �����)</param>
    /// <param name="SourceTableName">��� ������� � ���� ������, ������� ������ ������ �� ����</param>
    /// <param name="SourceColumnName">��� ���� � ������� ���� ������, ������� �������� ������ �� ����</param>
    public GridProducerDBFileInfoColumn(GridProducerDBFileInfoColumnKind Kind, string SourceTableName, string SourceColumnName)
      : this(Kind, SourceTableName, SourceColumnName, null)
    {
    }

  #endregion

  #region ��������

    public string SourceTableName { get { return FSourceTableName; } }
    private string FSourceTableName;

    public string SourceColumnName { get { return FSourceColumnName; } }
    private string FSourceColumnName;

    public GridProducerDBFileInfoColumnKind Kind { get { return FKind; } }
    private GridProducerDBFileInfoColumnKind FKind;

  #endregion

  #region ���������������� ������

    /// <summary>
    /// ��������� ����
    /// </summary>
    public override string[] FieldNames { get { return new string[1] { SourceColumnName }; } }

    public override object GetValue(DataGridView Grid, int RowIndex, DataRow Row)
    {
      Int32 FileId = DataTools.GetInt(Row, FSourceColumnName);
      if (FileId == 0)
        return DBNull.Value;
      else
      {
        AccDepFileInfo FileInfo = AccDepClientExec.GetDBFileInfo(FSourceTableName, FSourceColumnName, FileId);
        return GetValue(FileInfo, FKind);
      }
    }

    /// <summary>
    /// ����������� ����� ���������� �������� �� ��������� ��������� �����
    /// ����� ���� ����������� �������� �� ���� ����������� ������ ��� ����������
    /// ��������� ���������� ��� GridProducer
    /// </summary>
    /// <param name="fi">��������� ��������� �����</param>
    /// <param name="Kind">��� ������������ ��������</param>
    /// <returns>��������</returns>
    public static object GetValue(AccDepFileInfo FileInfo, GridProducerDBFileInfoColumnKind Kind)
    {
      switch (Kind)
      {
        case GridProducerDBFileInfoColumnKind.Name:
          return FileInfo.Name;
        case GridProducerDBFileInfoColumnKind.Length:
          return FileInfo.Length;
        case GridProducerDBFileInfoColumnKind.CreationTime:
          return FileInfo.CreationTime;
        case GridProducerDBFileInfoColumnKind.LastWriteTime:
          return FileInfo.LastWriteTime;
        default:
          return DBNull.Value;
      }
    }

  #endregion
  }
#endif

  #region ������������ EFPGridProducerSumColumnMode

  /// <summary>
  /// ����������� ���������� ������� ��� �������, �������� � ������� �������� ����������� �� ������� ��������
  /// </summary>
  public enum EFPGridProducerSumColumnMode
  {
    /// <summary>
    /// �����
    /// </summary>
    Sum,

    /// <summary>
    /// ����������� ��������
    /// </summary>
    Min,

    /// <summary>
    /// ������������ ��������
    /// </summary>
    Max,

    /// <summary>
    /// ������� ��������
    /// </summary>
    Average
  }

  #endregion

  /// <summary>
  /// �������, ���������� ����� ���������� ��������.
  /// ����� ����� ��������� �����������, ������������ ��� ������� ��������
  /// ���� ��� ������� �������� DBNull, �� ������������ �������� ����� ���� DBNull
  /// </summary>
  public class EFPGridProducerSumColumn : EFPGridProducerColumn
  {
    #region �����������

    /// <summary>
    /// �������� �������, ����������� ����� ������ �������
    /// </summary>
    /// <param name="name">�������� ��� ����� �������</param>
    /// <param name="sourceColumnNames">����� ����������� �������</param>
    public EFPGridProducerSumColumn(string name, string[] sourceColumnNames)
      : base(name, sourceColumnNames)
    {
      _Mode = EFPGridProducerSumColumnMode.Sum;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����������� �������. �� ��������� - Sum
    /// </summary>
    public EFPGridProducerSumColumnMode Mode { get { return _Mode; } set { _Mode = value; } }
    private EFPGridProducerSumColumnMode _Mode;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ��������� ��������
    /// </summary>
    /// <param name="args"></param>
    protected override void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      object[] a = new object[SourceColumnNames.Length];
      for (int i = 0; i < SourceColumnNames.Length; i++)
        a[i] = args.Values.GetValue(SourceColumnNames[i]);

      switch (Mode)
      {
        case EFPGridProducerSumColumnMode.Sum:
          args.Value = DataTools.SumValue(a);
          break;
        case EFPGridProducerSumColumnMode.Min:
          args.Value = DataTools.MinValue(a);
          break;
        case EFPGridProducerSumColumnMode.Max:
          args.Value = DataTools.MaxValue(a);
          break;
        case EFPGridProducerSumColumnMode.Average:
          args.Value = DataTools.AverageValue(a);
          break;
      }

      base.OnValueNeeded(args);
    }

    #endregion
  }
}
