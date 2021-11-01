using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;

using FreeLibSet.Config;
using FreeLibSet.Remoting;

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

/*
 * ������������� ������ �� ���� EFPDataGridView
 */

#if XXX
namespace FreeLibSet.Forms
{

  /// <summary>
  /// �������� ������ ��� ���������� �������������� ������.
  /// ������ ���� ����������� �������� - �������� ������ ��� ������, � ����� ������ ����� CreateDataView().
  /// ������ �������� "�����������", �� ����, ��������� ����� CreateDataView() �� �����������.
  /// ��� ������ � ��������� ���������� EFPDataGridView ������������� ������������ EFPDataGridViewHieHandler
  /// </summary>
  public class EFPHieView : IReadOnlyObject
  {
#region �����������

    /// <summary>
    /// ����������� �����������
    /// </summary>
    public EFPHieView()
    {
      FHideExtraSumRows = false;
      FTotalSumRowCount = 1;
      FSumColumnNames = DataTools.EmptyStrings;
    }

#endregion

#region �������� - �������� ������ ��� ������

    /// <summary>
    /// �������� ������� ������ ��� ������
    /// </summary>
    public DataTable SourceTable
    {
      get { return FSourceTable; }
      set
      {
        CheckNotReadOnly();

        if (value == null)
          throw new ArgumentNullException();

        FSourceTable = value;
      }
    }
    private DataTable FSourceTable;

    /// <summary>
    /// ������ ��������. ������� ��������� ������� ���� ����� ���������� �������,
    /// �� ���� ������ �������� ������, ��������� - ����� ������� �������.
    /// �������� ������ ���� ����������� �� ������������ ���������.
    /// ������ ������ ���������, ��� �������, ���� ������� ��������.
    /// </summary>
    public EFPHieViewLevel[] Levels
    {
      get { return FLevels; }
      set
      {
        CheckNotReadOnly();

        if (value == null)
          throw new ArgumentNullException();
        if (value.Length < 1)
          throw new ArgumentException("������ ������ ��������� �� ����� ������ ��������");
        for (int i = 0; i < value.Length; i++)
        {
          if (value[i] == null)
            throw new ArgumentNullException();

          for (int j = 0; j < i; j++)
          {
            if (value[j].Name == value[i].Name)
            {
              if (object.ReferenceEquals(value[j], value[i]))
                throw new InvalidOperationException("��������� ������������� ������ " + value[i] + "�� �����������");
              else
                throw new ArgumentException("������� � ������ " + value[i].Name + " ������ � ������ ������");
            }
          }
        }
        FLevels = value;
      }
    }
    private EFPHieViewLevel[] FLevels;

    /// <summary>
    /// ����� ��������, ������� ������ �������������
    /// </summary>
    public string[] SumColumnNames
    {
      get { return FSumColumnNames; }
      set
      {
        CheckNotReadOnly();
        if (value == null)
          value = DataTools.EmptyStrings;

        FSumColumnNames = value;
      }
    }
    private string[] FSumColumnNames;

    /// <summary>
    /// ���� True, �� ����� ������ ������ ����, ��� ������� ����
    /// ������ ���� ������ ����������� ������
    /// �� ��������� (false) �������� ������ ������������ ������
    /// </summary>
    public bool HideExtraSumRows
    {
      get { return FHideExtraSumRows; }
      set
      {
        CheckNotReadOnly();

        FHideExtraSumRows = value;
      }
    }
    private bool FHideExtraSumRows;

    /// <summary>
    /// ���������� �������� ����� ����� ���������. �� ��������� - ���� ������.
    /// ����� �������� �������������� ������, ��������, ��� ���������� ������
    /// </summary>
    public int TotalSumRowCount
    {
      get { return FTotalSumRowCount; }
      set
      {
        CheckNotReadOnly();
        if (value < 1)
          throw new ArgumentOutOfRangeException("TotalSumRowCount", value, "����� �������� ����� �� ����� ���� ������ 1");
        FTotalSumRowCount = value;
      }
    }
    private int FTotalSumRowCount;

#endregion

#region �������� �������������� ���������

#region ���������� ���� � ������, ������������ ��� ���������� ������

    private class KeyInfo
    {
#region ����

      public string Key;
      public string Text;
      public int Order;

#endregion

      public override string ToString()
      {
        return Key.ToString() + " = " + Text;
      }
    }

    /// <summary>
    /// ���������� ������ ��� ������ ������
    /// </summary>
    private class InternalLevelData
    {
#region �����������

      public InternalLevelData(EFPHieViewLevel Level)
      {
        FLevel = Level;
      }

      private EFPHieViewLevel FLevel;

#endregion

#region ���� ����-�����

      public SortedList<string, KeyInfo> KeyPairs;

      public void CreateKeyPairs(DataTable Table)
      {
        //if (Mode == HieViewLevelMode.UserDefined)
        //  return;
        KeyPairs = new SortedList<string, KeyInfo>();

        foreach (DataRow Row in Table.Rows)
        {
          string Key = FLevel.GetRowSortKey(Row);
          //if (!IsEmptyKey(Key))
          if (!String.IsNullOrEmpty(Key))
          {
            if (!KeyPairs.ContainsKey(Key))
            {
              KeyInfo ki = new KeyInfo();
              ki.Key = Key;
              ki.Text = FLevel.GetRowText(Row);
              KeyPairs.Add(Key, ki);
            }
          }
        }
      }

      /// <summary>
      /// ������������� ���� Order � ������� KeyPairs
      /// </summary>
      public void InitKeyPairsOrder()
      {
        SortedList<String, KeyInfo> TmpList = new SortedList<string, KeyInfo>();
        int cnt = 0;
        foreach (KeyInfo ki in KeyPairs.Values)
        {
          // � ������� KeyPairs ����� ����������� ���������� ��������
          // �������, ��������� � �������� ���������� �����
          cnt++;
          TmpList.Add(ki.Key + "_" + cnt.ToString(), ki);
        }
        for (int i = 0; i < TmpList.Count; i++)
          TmpList.Values[i].Order = i + 1;
      }

      /// <summary>
      /// �������� �������� ��� ��� ���������� ��� ������
      /// </summary>
      /// <param name="Row"></param>
      /// <returns></returns>
      public int GetRowOrder(DataRow Row)
      {
        string Key = FLevel.GetRowSortKey(Row);
        if (String.IsNullOrEmpty(Key))
          return 0;
        KeyInfo ki = KeyPairs[Key];
        if (ki == null)
          return -1;
        else
          return ki.Order;
      }

#endregion
    }

#endregion

    /// <summary>
    /// ������� ������������� ��������.
    /// ����� ������, �������� DataView �������� ��������.
    /// ������ ����������� � ������ IsReadOnly=true.
    /// ���� ����� ����� �������� ������ ���� ���.
    /// </summary>
    public void CreateDataView()
    {
      CheckNotReadOnly();

      if (SourceTable == null)
        throw new NullReferenceException("�� ������ �������� SourceTable");
      if (Levels == null)
        throw new NullReferenceException("�� ������ �������� Levels");

      // ��� ������� ��������� ������ �������� ������� ������� "�������� ����"-"�����".
      // ������� ����������� �� ������ (� �� �� ����) � �������� ���������� �� �������
      // ��� ������ ������������ ��� ���������� ������� ��������.
      InternalLevelData[] LevelData = new InternalLevelData[Levels.Length];
      for (int i = 0; i < Levels.Length; i++)
      {
        LevelData[i] = new InternalLevelData(Levels[i]);
        LevelData[i].CreateKeyPairs(SourceTable);
        LevelData[i].InitKeyPairsOrder();
      }

      // ������� ����� �������
      DataTable ResTable = CreateResTable();

      // �������� ������ �������� ������ ��������
      CopyLevel0(ResTable, LevelData);

      // ������� ��������, ��������������� ������ �������
      FDataView = new DataView(ResTable);
      string s = "";
      for (int i = Levels.Length - 1; i >= 1; i--)
        s += "Hie_Order" + i.ToString() + ",";
      s += "Hie_Order0";
      FDataView.Sort = s;

      // ��������� ��� ������ �������� ������ ������ ������ ����� ������� ��������
      AddLevels(LevelData);

      // ���������� �������� �����
      CalcSums();

      if (HideExtraSumRows)
        DoHideExtraSumRows();

      // ���������� ����������� ������ ��� ������� � ������������� ���������
      MoveSumRowsFirst();

      OnAfterCreateView();

      // ��������� �����
      int cnt = 0;
      for (int j = 0; j < FDataView.Count; j++)
      {
        DataRowView drv = FDataView[j];
        if (((int)(drv.Row["Hie_Level"])) == 0)
        {
          cnt++;
          drv.Row["Hie_RowNumber"] = cnt;
        }
      }
    }

    private DataTable CreateResTable()
    {
      // ������ ����� ����������� ������ � �������, �.�. ���� �������� �������
      DataTable ResTable = SourceTable.Clone();
      ResTable.TableName = "HieTable";

      // 31.01.2009
      // ������� ����������� ��� ���� �������� �������
      foreach (DataColumn Column in ResTable.Columns)
      {
        Column.AllowDBNull = true;
        Column.MaxLength = -1;
        Column.AutoIncrement = false;
      }
      ResTable.PrimaryKey = null;

      // ��������� �������

      // ����� ����� ������� ��������� (0-����� ���������� - �������� ������)
      ResTable.Columns.Add("Hie_Level", typeof(int));
#if DEBUG
      ResTable.Columns.Add("Hie_LevelName", typeof(string)); // �������
#endif

      // True ��� ����� ������, false - ��� ���������� (��� Hie_Level > 0)
      ResTable.Columns.Add("Hie_Sum", typeof(bool));

      // ����� ����� ����� � ���������� ������� ��������
      ResTable.Columns.Add("Hie_Text", typeof(string));

      // ����� ����� ����� ������ (��������� � �������) ��� ����� � Hie_Level=0
      ResTable.Columns.Add("Hie_RowNumber", typeof(int));

      // ��� ������� ������ �������� ��������� �������� ���� ��� ����������
      for (int i = 0; i < Levels.Length; i++)
      {
        ResTable.Columns.Add("Hie_Order" + i.ToString(), typeof(int));
      }

      // ������ ��� �������� ������ �����
      ResTable.Columns.Add("Hie_Hidden", typeof(bool));

      return ResTable;
    }

    private void CopyLevel0(DataTable ResTable, InternalLevelData[] LevelData)
    {
      for (int i = 0; i < SourceTable.Rows.Count; i++)
      {
        DataRow SrcRow = SourceTable.Rows[i];
        DataRow DstRow = ResTable.NewRow();
        for (int j = 0; j < SourceTable.Columns.Count; j++)
          DstRow[j] = SrcRow[j];

        DstRow["Hie_Level"] = 0;
#if DEBUG
        DstRow["Hie_LevelName"] = Levels[0].Name;
#endif
        //        DstValues["Hie_Text"] = new string(' ', Levels.Length*2)+Levels[0].GetRowText(DstValues);
        DstRow["Hie_Text"] = Levels[0].GetRowText(DstRow);
        DstRow["Hie_Order0"] = LevelData[0].GetRowOrder(DstRow);

        ResTable.Rows.Add(DstRow);
      }
    }

    /// <summary>
    /// �������� ����� ������� ��������
    /// </summary>
    private void AddLevels(InternalLevelData[] LevelData)
    {
      int nLevels = Levels.Length; // ����� ������� ��������
      DataRowView drv;
      int i, j, k;
      for (i = 0; i < SourceTable.Rows.Count; i++)
      {
        DataRow MainRow = FDataView.Table.Rows[i];
        object[] Orders = new object[nLevels];
        for (j = 0; j < nLevels; j++)
        {
          if (j > 0)
            MainRow["Hie_Order" + j.ToString()] = LevelData[j].GetRowOrder(MainRow);
          Orders[nLevels - j - 1] = (int)(MainRow["Hie_Order" + j.ToString()]);
        }

        for (j = 0; j < (Levels.Length - 1); j++)
        {
          StringBuilder sb = new StringBuilder();
          //string spc = new string(' ', (Levels.Length - j - 1) * 2);

          // ��������� ��������� ������
          if (!Levels[j + 1].SubTotalRowFirst)
          {
            Orders[nLevels - j - 1] = -2;
            if (FDataView.Find(Orders) < 0)
            {
              drv = FDataView.AddNew();
              for (k = 0; k < nLevels; k++)
                drv.Row["Hie_Order" + k.ToString()] = Orders[nLevels - k - 1];
              drv.Row["Hie_Level"] = j + 1;
#if DEBUG
              drv.Row["Hie_LevelName"] = Levels[j + 1].Name;
#endif
              drv.Row["Hie_Sum"] = false;
              drv.Row["Hie_Text"] = /*spc + *//*"������: " + */Levels[j + 1].GetRowText(MainRow, false);
              // ����������� "��������" ����� �������
              for (k = j + 1; k < Levels.Length; k++)
              {
                string[] FieldNames = Levels[k].ColumnNameArray;
                for (int l = 0; l < FieldNames.Length; l++)
                  drv.Row[FieldNames[l]] = MainRow[FieldNames[l]];
              }
              drv.EndEdit();
            }
          }

          // ��������� �������� ������
          Orders[nLevels - j - 1] = Int32.MaxValue; // ������� ����� �������� ����� �������
          if (FDataView.Find(Orders) < 0)
          {
            drv = FDataView.AddNew();
            for (k = 0; k < nLevels; k++)
              drv.Row["Hie_Order" + k.ToString()] = Orders[nLevels - k - 1];
            drv.Row["Hie_Level"] = j + 1;
#if DEBUG
            drv.Row["Hie_LevelName"] = Levels[j + 1].Name;
#endif
            drv.Row["Hie_Sum"] = true;
            if (Levels[j + 1].SubTotalRowFirst)
              drv.Row["Hie_Text"] = Levels[j + 1].GetRowText(MainRow, false);
            else
              drv.Row["Hie_Text"] = Levels[j + 1].GetRowText(MainRow, true);
            // ����������� "��������" ����� �������
            for (k = j + 1; k < Levels.Length; k++)
            {
              string[] FieldNames = Levels[k].ColumnNameArray;
              for (int l = 0; l < FieldNames.Length; l++)
                drv.Row[FieldNames[l]] = MainRow[FieldNames[l]];
            }
            drv.EndEdit();
          }
        }
      }

      // ��������� �������� ������ (��� ��������� �����)
      FTotalSumRows = new DataRow[TotalSumRowCount];
      for (i = 0; i < TotalSumRowCount; i++)
      {
        drv = FDataView.AddNew();
        for (k = 0; k < nLevels; k++)
          drv.Row["Hie_Order" + k.ToString()] = Int32.MaxValue;
        drv.Row["Hie_Level"] = nLevels;
#if DEBUG
        drv.Row["Hie_LevelName"] = "�������� ������";
#endif
        drv.Row["Hie_Sum"] = true;
        drv.Row["Hie_Text"] = "�����";
        FTotalSumRows[i] = drv.Row;
        drv.EndEdit();
      }
    }

    /// <summary>
    /// ���������� �������� ����
    /// </summary>
    private void CalcSums()
    {
      int i, j, k;
      DataColumn[] Columns = new DataColumn[SumColumnNames.Length];
      for (j = 0; j < SumColumnNames.Length; j++)
      {
        if (!FDataView.Table.Columns.Contains(SumColumnNames[j]))
          throw new BugException("������������ ������� \"" + SumColumnNames[j] + "\" ��� � ������� �������������� ������");
        Columns[j] = FDataView.Table.Columns[SumColumnNames[j]];
      }

      decimal[,] aaVals = new decimal[Levels.Length + 1, Columns.Length];
      Array.Clear(aaVals, 0, aaVals.Length);


      for (i = 0; i < FDataView.Count; i++)
      {
        DataRow Row = FDataView[i].Row;
        int lvl = (int)Row["Hie_Level"];
        if (lvl == 0)
        {
          // ��� ������ ������ ��������� ����������� ��������
          for (k = 0; k <= Levels.Length; k++)
          {
            for (j = 0; j < Columns.Length; j++)
              aaVals[k, j] += DataTools.GetDecimal(Row[Columns[j]]);
          }
        }
        else
        {
          bool IsSum = (bool)Row["Hie_Sum"];
          if (IsSum)
          {
            FDataView[i].BeginEdit();
            for (j = 0; j < Columns.Length; j++)
            {
              Row[Columns[j]] = aaVals[lvl, j];
              for (k = 0; k <= lvl; k++)
                aaVals[k, j] = 0;
            }
            FDataView[i].EndEdit();
          }
        }
      }
    }

    /// <summary>
    /// �������� ������ ����� ����
    /// </summary>
    private void DoHideExtraSumRows()
    {
      // ����� ��������� ����� ��� ������� ������
      int[] Counters = new int[Levels.Length + 1];
      Array.Clear(Counters, 0, Counters.Length);

      for (int i = 0; i < FDataView.Count; i++)
      {
        DataRow Row = FDataView[i].Row;

        int lvl = (int)Row["Hie_Level"];

        bool IsSum;
        if (lvl > 0)
          IsSum = (bool)Row["Hie_Sum"];
        else
          IsSum = false;

        if (IsSum)
        {
          if ((Counters[lvl - 1] == 1) && (lvl < Levels.Length))
          {
            Row.BeginEdit();
            Row["Hie_Hidden"] = true;
            Row.EndEdit();
          }
          Counters[lvl - 1] = 0;
        }
        else
        {
          Counters[lvl]++;
        }
      }

      // ������� �������� ������
      DeleteHiddenRows(FDataView.Table);
    }


    /// <summary>
    /// ���������� ������ ��������� ��� ������� � ������������� ���������
    /// SubTotalRowFirst � ������ ������ �����
    /// </summary>
    private void MoveSumRowsFirst()
    {
      bool HasMovement = false;
      for (int i = 0; i < Levels.Length; i++)
      {
        if (Levels[i].SubTotalRowFirst)
          HasMovement = true;
      }
      if (!HasMovement)
        return;

      // ���������� �������, � �� DataView, �.� ������� ����� ��������
      foreach (DataRow Row in FDataView.Table.Rows)
      {
        //if (DataTools.GetInt(Row, "Hie_Level")==0)
        //  continue;
        if (!DataTools.GetBool(Row, "Hie_Sum"))
          continue;
        //for (int i = 0; i < Levels.Length; i++)
        //{
        //  if (Levels[i].SubTotalRowFirst)
        //  {
        //    string ColName = "Hie_Order" + (Levels.Length - i - 1).ToString();
        //    if (DataTools.GetInt(Row[ColName]) == int.MaxValue)
        //      Row[ColName.ToString()] = -2;
        //  }
        //}
        int Level = DataTools.GetInt(Row, "Hie_Level");
        if (Level < 1 || Level >= Levels.Length)
          continue;
        if (Levels[Level].SubTotalRowFirst)
        {
          string ColName = "Hie_Order" + (Level - 1).ToString();
          int CurrOrder = DataTools.GetInt(Row[ColName]);
          if (CurrOrder == int.MaxValue)
            Row[ColName] = -2;
        }

      }
    }


    /// <summary>
    /// �������� �����, ���������� � ���� "Hie_Hidden"
    /// </summary>
    /// <param name="Table">������� ������ ���������� ���������</param>
    private void DeleteHiddenRows(DataTable Table)
    {
      for (int i = Table.Rows.Count - 1; i >= 0; i--)
      {
        if (DataTools.GetBool(Table.Rows[i], "Hie_Hidden"))
          Table.Rows[i].Delete();
      }
      Table.AcceptChanges();
    }

#endregion

#region �������� � �������, ��������������� ������� CreateDataView()

    /// <summary>
    /// ��������� ������ �������������� ���������.
    /// �������� �������� �������� ����� ������ CreateDataView()
    /// </summary>
    public DataView DataView { get { return FDataView; } }
    private DataView FDataView;

    /// <summary>
    /// ������ �������� ����� ���������. (������ ���� ������). �������� �����������
    /// ����� ������������ ��������� � �������� ���������� �����, ������ TotalSumsRowCount
    /// </summary>
    public DataRow[] TotalSumRows { get { return FTotalSumRows; } }
    private DataRow[] FTotalSumRows;

    /// <summary>
    /// ������� ���������� ����� ����������� ������ CreateDataView, ����� ���� � ��������
    /// ���������, �� ��������� ����� ��� �� ���������
    /// ���������� ����� ���������������� TotalSumRows, ������� �������� ������
    /// </summary>
    public event EventHandler AfterCreateView;

    /// <summary>
    /// �������� ������� AfterCreateView
    /// </summary>
    protected virtual void OnAfterCreateView()
    {
      if (AfterCreateView != null)
        AfterCreateView(this, EventArgs.Empty);
    }

#if DEBUG

    private void CheckIfCreated()
    {
      if (FDataView == null)
        throw new InvalidOperationException("�������� �� ��� ������. �� ���� ������ CreateDataView()");
    }

    /// <summary>
    /// �������� ������, ��� ��� ��������� � �������������� �������
    /// </summary>
    /// <param name="Row"></param>
    private void CheckRow(DataRow Row)
    {
      CheckIfCreated();
      if (Row == null)
        throw new ArgumentNullException("Row");
      if (Row.Table != FDataView.Table)
        throw new ArgumentException("������ ��������� � ������ �������", "Row");
    }

#endif

#endregion

#region ������, ������� ����� �������������� ����� �������� ���������

    /// <summary>
    /// �������� ��� ������ ��� �������� ������ (�������� HieViewLevel.NamePart)
    /// ��� �������� ������ ������������ ""
    /// </summary>
    /// <param name="Row">������ �� ���������, ���������� CreateView</param>
    /// <returns>��� ������</returns>
    public string GetLevelName(DataRow Row)
    {
      if (Row == null)
        return String.Empty;

#if DEBUG
      CheckRow(Row);
#endif

      int Level = DataTools.GetInt(Row, "Hie_Level");
      if (Level >= Levels.Length)
        return String.Empty;
      return Levels[Level].Name;
    }

    /// <summary>
    /// �������� ������ ������ ��� �������� ������ (0 .. Levels.Length-1)
    /// ��� �������� ������ ������������ (-1)
    /// </summary>
    /// <param name="Row">������ �� ���������, ���������� CreateView</param>
    /// <returns>��� ������</returns>
    public int GetLevelIndex(DataRow Row)
    {
      if (Row == null)
        return -1;

#if DEBUG
      CheckRow(Row);
#endif

      int Level = DataTools.GetInt(Row, "Hie_Level");
      if (Level >= Levels.Length)
        return -1;
      return Level;
    }

    /// <summary>
    /// �������� �� �������� ������ ������ ��� ��������� ?
    /// </summary>
    /// <param name="Row"></param>
    /// <returns></returns>
    public bool IsSumRow(DataRow Row)
    {
      if (Row == null)
        return false;

#if DEBUG
      CheckRow(Row);
#endif


      int Level = DataTools.GetInt(Row, "Hie_Level");
      if (Level > 0)
        return DataTools.GetBool(Row, "Hie_Sum");
      else
        return false;
    }

    /// <summary>
    /// �������� �� �������� ������ ������������� ?
    /// </summary>
    /// <param name="Row"></param>
    /// <returns></returns>
    public bool IsHeaderRow(DataRow Row)
    {
      if (Row == null)
        return false;

#if DEBUG
      CheckRow(Row);
#endif

      int Level = DataTools.GetInt(Row, "Hie_Level");
      if (Level > 0)
        return !DataTools.GetBool(Row, "Hie_Sum");
      else
        return false;
    }

    /// <summary>
    /// ��������� �������� ��� ���� <param name="ColumnName"></param>. �������� ������������,
    /// ���� ��� ��������� ������ ������� ����������� �� ��������� ��������.
    /// �������� �� ������������, ���� �������� ���� �� �������
    /// ���� ������� �������� �������� ��������� �����, �� FieldName ������ ��������
    /// ��� ����� �����, ����������� ��������, ���, ��� ��� ��������� � ������.
    /// � ���� ������ ������������ ������ ��������
    /// </summary>
    /// <param name="Row">������ � ����������� ������������� ���������</param>
    /// <param name="ColumnName">��� ��������� ����</param>
    /// <param name="Value">���� ���������� ������������ �������� ��� null, ���� �������� �� ������������</param>
    /// <returns>true, ���� �������� �������������</returns>
    public bool GetLevelValue(DataRow Row, string ColumnName, out object Value)
    {
      Value = null;

      if (Row == null)
        return false;

#if DEBUG
      CheckRow(Row);
#endif

      int Level = -1;
      for (int i = 0; i < Levels.Length; i++)
      {
        if (Levels[i].ColumnName == ColumnName)
        {
          Level = i;
          break;
        }
      }
      if (Level < 0)
        return false; // ������� �������� �� ������
      int ThisLevel = DataTools.GetInt(Row, "Hie_Level");
      if (ThisLevel > Level)
        return false;
      if (ColumnName.IndexOf(',') >= 0)
      {
        object[] a = new object[Levels[Level].ColumnNameArray.Length];
        for (int i = 0; i < Levels[Level].ColumnNameArray.Length; i++)
          a[i] = Row[Levels[Level].ColumnNameArray[i]];
        Value = a;
      }
      else
        Value = Row[ColumnName];
      return true;
    }

#if XXXXX
#region ��������� �������������� ��������

    public string GetLevelAsString(DataRow Row, string FieldName)
    { 
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetString(Value);
    }

    public bool GetLevelAsBool(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetBool(Value);
    }

    public DateTime?GetLevelAsNullableDateTime(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetNullableDateTime(Value);
    }

    public int GetLevelAsInt(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetInt(Value);
    }

    public float GetLevelAsSingle(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetSingle(Value);
    }

    public double GetLevelAsDouble(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetDouble(Value);
    }

    public decimal GetLevelAsDeciumal(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetDecimal(Value);
    }

#endregion
#endif
    /// <summary>
    /// �������� ������������ ������ ��� ������ ������ Row ��������� ������ Level
    /// ���� ��� ������ Row ������� Level �� ��������� (�� ���� ������ Row ���������
    /// � ����� �������� ������, ��� Level), �� ������������ null
    /// </summary>
    /// <param name="Row">������ �� ��������� dv</param>
    /// <param name="Level">��������� �������</param>
    /// <returns>������, ���������� ���������� ��� ������ Level ��� null</returns>
    public DataRow GetHeaderRow(DataRow Row, int Level)
    {
#if DEBUG
      CheckRow(Row);
#endif

      object[] Keys = new object[Levels.Length];
      //int RowLevel=DataTools.GetInt(Row, "Hie_Level");
      for (int i = 0; i < Levels.Length; i++)
      {
        object k;
        //if (i == Level)
        //  k = -2;
        //else
        //{
        //  if (i < Level)
        //    k = int.MaxValue;
        //  else
        //    k = Row["Hie_Order" + i.ToString()];
        //}
        // ���������� 06.01.2011
        if (i >= Level)
          k = Row["Hie_Order" + i.ToString()];
        else if (i == (Level - 1))
          k = -2;
        else
          k = int.MaxValue; // 16.12.2014
        Keys[Levels.Length - i - 1] = k;
      }
      int p = FDataView.Find(Keys);
      if (p < 0)
        return null;
      else
        return FDataView[p].Row;
    }

    /// <summary>
    /// �������� ��������� �������� ��� ��������� ������ Level ��� ������ Row
    /// ���� ��� ������ Row ������� Level �� ��������� (�� ���� ������ Row ���������
    /// � ����� �������� ������, ��� Level), �� ������������ ������ ������
    /// ����������� ����� GetHeaderRow() � ������������ �������� ���� "Hie_Text"
    /// </summary>
    /// <param name="Row">������ �� ��������� dv</param>
    /// <param name="Level">��������� �������</param>
    /// <returns>��������� �������� ��� null</returns>
    public string GetHeaderText(DataRow Row, int Level)
    {
      DataRow HeaderRow = GetHeaderRow(Row, Level);
      if (HeaderRow == null)
        return String.Empty;
      else
        return DataTools.GetString(HeaderRow, "Hie_Text");
    }

#if XXX
    public DataRow GetFirstZeroLevelRow(DataRow Row)
    {
#if DEBUG
      CheckRow(Row);
#endif

      object[] Keys = new object[Levels.Length];
      int Level = DataTools.GetInt(Row, "Hie_Level");
      for (int i = 0; i < Levels.Length; i++)
      {
        object k;
        if (i < Level)
          k = 1;
        else
          k = Row["Hie_Order" + i.ToString()];
        Keys[Levels.Length - i - 1] = k;
      }
      int p = FDataView.Find(Keys);
      if (p < 0)
      {
        // 01.02.2010
        // � ��������� ������� ������ ������ � ������ ����� ����� �������� � ����
        // Hie_OrderXXX �� 1 �, ��������, 2. � ���� ������ ������ ����� ����� 
        // ����������
        if (Level > 0)
        {
          // ������� ��������� ��������
          DataView dv2 = new DataView(FDataView.Table, dv.RowFilter, dv.Sort, dv.RowStateFilter);
          // � ���������� �� ���� ���. �������
          DataFilter[] Filters = new DataFilter[Levels.Length - Level];
          for (int i = 0; i < Filters.Length; i++)
            Filters[i] = new ValueFilter("Hie_Order" + (Levels.Length - i - 1).ToString(), Keys[i]);
          DataFilter Filter2 = AndFilter.FromArray(Filters);
          DataFilter.AddToDataViewAnd(dv2, Filter2);

          // ������ ������� ���������� ������, ��������� ������ ���������� � �����.
          // ������ ���������� ������ - ��, ��� ��� �����
          for (int i = 0; i < dv2.Count; i++)
          {
            DataRow Row2 = dv2[i].Row;
            bool Found = true;
            for (int j = 0; j < Level; j++)
            {
              int ord = DataTools.GetInt(Row2, "Hie_Order" + j.ToString());
              if (ord <= 0 || ord == int.MaxValue)
              {
                Found = false;
                break;
              }
            }
            if (Found)
              return Row2;
          }
        }
        return null;
      }
      else
        return dv[p].Row;
    }

#endif

#endregion

#region IReadOnlyObject Members

    /// <summary>
    /// ���������� true ����� ������ CreateDataView()
    /// </summary>
    public bool IsReadOnly
    {
      get { return FDataView != null; }
    }

    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

#endregion
  }

  /// <summary>
  /// �������� ��������������� ��������� ��� ������ �������� (����������� ������� �����)
  /// �������������� � �������� HieViewLevel.ParamEditor.
  /// ��������� ��������������, ������ � ������ ������������� ��������� (��� ����������),
  /// ����������� � ������� ������
  /// ��� �������� �������� ��������� ������������ HieViewLevel.ExtBalues, �������
  /// ���� ������ ����� �������������� ��� ���������� HieViewLevel
  /// </summary>
  public abstract class EFPHieViewLevelParamEditor
  {
#region ����������� ������

    public abstract bool PerformEdit(EFPHieViewLevel Level);

    public abstract void WriteConfig(EFPHieViewLevel Level, CfgPart Part);

    public abstract void ReadConfig(EFPHieViewLevel Level, CfgPart Part);

    public abstract string GetText(EFPHieViewLevel Level);

#endregion
  }

#if XXX
  /// <summary>
  /// ������������� ���������� ��������� ������ ��������, ����� ����� ��������
  /// ������������� ��������� ��������.
  /// ��� �������� �������� ������������ ��� � HieViewLevel.ExtValues["�����"]
  /// ��� ������ � ������ ������������ ������������ ��������� �������� "�����"
  /// </summary>
  public class EFPHieViewLevelEnumParamEditor : EFPHieViewLevelParamEditor
  {
#region ������������

    public EFPHieViewLevelEnumParamEditor(string[] Codes, string[] Names)
    {
#if DEBUG
      if (Codes == null)
        throw new ArgumentNullException("Codes");
      if (Names == null)
        throw new ArgumentNullException("Names");
      if (Names.Length != Codes.Length)
        throw new ArgumentException("����� ������� ���� �� ��������� � ������ ������� �����", "Names");
#endif
      FCodes = Codes;
      FNames = Names;
    }

    public EFPHieViewLevelEnumParamEditor(string[] Names)
      : this(GetCodes(Names), Names)
    {
    }

    private static string[] GetCodes(string[] Names)
    {
      string[] Codes = new string[Names.Length];
      for (int i = 0; i < Codes.Length; i++)
        Codes[i] = i.ToString();
      return Codes;
    }

#endregion

#region ��������

    /// <summary>
    /// ���� ������������ ��� �������� �������� �������� ���������
    /// �������� � ������������
    /// </summary>
    public string[] Codes { get { return FCodes; } }
    private string[] FCodes;

    /// <summary>
    /// ����� ������������ ��� ����������� �� ������
    /// �������� � ������������
    /// </summary>
    public string[] Names { get { return FNames; } }
    private string[] FNames;

#endregion

#region ��������� ��������

    public string GetSelectedCode(EFPHieViewLevel Level)
    {
      string Code = Level.ExtValues.GetString("�����");
      int p = Array.IndexOf<string>(FCodes, Code);
      if (p < 0)
        return FCodes[0];
      else
        return FCodes[p];
    }

    public void SetSelectedCode(EFPHieViewLevel Level, string Code)
    {
      Level.ExtValues["�����"] = Code;
    }

    public int GetSelectedIndex(EFPHieViewLevel Level)
    {
      string Code = Level.ExtValues.GetString("�����");
      int p = Array.IndexOf<string>(FCodes, Code);
      if (p < 0)
        return 0;
      else
        return p;
    }

    public void SetSelectedIndex(EFPHieViewLevel Level, int Index)
    {
      Level.ExtValues["�����"] = FCodes[Index];
    }

#endregion

#region ���������������� ������

    public override bool PerformEdit(EFPHieViewLevel Level)
    {
      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.ImageKey = Level.ImageKey;
      dlg.Title = Level.DisplayName;
      dlg.GroupTitle = "�����";
      dlg.Items = Names;
      dlg.SelectedIndex = GetSelectedIndex(Level);

      if (dlg.ShowDialog() != DialogResult.OK)
        return false;

      SetSelectedIndex(Level, dlg.SelectedIndex);
      return true;
    }

    public override void WriteConfig(EFPHieViewLevel Level, CfgPart Part)
    {
      Part.SetString("�����", GetSelectedCode(Level));
    }

    public override void ReadConfig(EFPHieViewLevel Level, CfgPart Part)
    {
      string Code = Part.GetString("�����");
      int p = Array.IndexOf<string>(FCodes, Code);
      if (p < 0)
        SetSelectedIndex(Level, 0);
      else
        SetSelectedIndex(Level, p);
    }

    public override string GetText(EFPHieViewLevel Level)
    {
      int p = GetSelectedIndex(Level);
      return Names[p];
    }

#endregion
  }
#endif

#if XXX
  /// <summary>
  /// ���������� ��������� ������ �������� ���� "����" � ������������ ������
  /// ������ ��������� ����-�����-�������-���
  /// </summary>
  public class EFPHieViewLevelDateGeneralizationParamEditor : EFPHieViewLevelEnumParamEditor
  {
#region �����������

    private static readonly string[] DateTypeNames=new string[]{"����", "�����", "�������", "���"};

    public EFPHieViewLevelDateGeneralizationParamEditor()
      : base(DateTypeNames, DateTypeNames)
    { 
    }

#endregion

#region �������������� ������

    public AccDepDateGeneralization GetSelectedMode(EFPHieViewLevel Level)
    {
      return (AccDepDateGeneralization)(base.GetSelectedIndex(Level));
    }

    public void SetSelectedMode(EFPHieViewLevel Level, AccDepDateGeneralization Mode)
    {
      base.SetSelectedIndex(Level, (int)Mode);
    }

#endregion
  }
#endif

}
#endif
