using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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

namespace AgeyevAV.FIAS
{
  /// <summary>
  /// ������������� ������������ ������� ����
  /// ���������� ��� ����� ������� ������ FiasEnumerable, ���������� ����������� �������� � ��������� ���� foreach.
  /// � �������� ��������, ����������� ���� ����� ������������ ���� � ��� �� ��������� ������ FiasAddress.
  /// ���������� ��� �� ������ ���������� ������ �� ���� ������, ��� ��� ����� ����� ���������� �� ��������� �����.
  /// ��� ���������� ����� ������ ����������� ����� FiasAddress.Clone().
  /// </summary>
  public sealed class FiasEnumerable : IEnumerable<FiasAddress>
  {
    #region �����������

    /// <summary>
    /// ������� ������ � ����������� �� ���������
    /// </summary>
    /// <param name="source">�������� ������ ����. �� ����� ���� null</param>
    public FiasEnumerable(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Source = source;
      _BaseAddress = new FiasAddress();
      _BottomLevel = FiasLevel.Region;
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ������ ����. �������� � ������������
    /// </summary>
    public IFiasSource Source { get { return _Source; } }
    private IFiasSource _Source;

    /// <summary>
    /// ������� �����, �� �������� ������ ����������� ������������
    /// </summary>
    public FiasAddress BaseAddress
    {
      get { return _BaseAddress; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _BaseAddress = value;
      }
    }
    private FiasAddress _BaseAddress;

    /// <summary>
    /// ����� ������ �������, �� �������� ����� �������� ��� ������������ �������.
    /// </summary>
    public FiasLevel BottomLevel
    {
      get { return _BottomLevel; }
      set
      {
        switch (value)
        {
          case FiasLevel.House:
          case FiasLevel.Building:
          case FiasLevel.Structure:
            value = FiasLevel.House;
            _Source.DBSettings.CheckUseHouse();
            break;
          case FiasLevel.Flat:
          case FiasLevel.Room:
            value = FiasLevel.Flat;
            _Source.DBSettings.CheckUseRoom();
            break;
          default:
            if (!FiasTools.AOLevelIndexer.Contains(value))
              throw new ArgumentException();
            break;
        }

        _BottomLevel = value;
      }
    }
    private FiasLevel _BottomLevel;

    #endregion

    #region IEnumerable<FiasAddress> Members

    /// <summary>
    /// ������� �������������
    /// </summary>
    /// <returns>�������������</returns>
    public IEnumerator<FiasAddress> GetEnumerator()
    {
      return new FiasEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new FiasEnumerator(this);
    }

    #endregion

    private class LevelInfo
    {
      #region �����������

      public LevelInfo(FiasLevel level, FiasAddress[] addresses)
      {
        _Level = level;
        _Addresses = addresses;
        Index = -1;
        ChildLevel = FiasLevel.Unknown;
      }

      #endregion

      #region �������� � ����

      public FiasLevel Level { get { return _Level; } }
      private FiasLevel _Level;

      /// <summary>
      /// ������ �������.
      /// �� ����������� ��� �������� ��������, ����� ����� ��������� ������ �� ���� ������
      /// </summary>
      public FiasAddress[] Addresses { get { return _Addresses; } }
      private FiasAddress[] _Addresses;

      /// <summary>
      /// ������� ������ � ������ DataView.
      /// ������� ����� �������� (-1). ��������� ����������� ����� ���������� ���������� ������
      /// </summary>
      public int Index;

      /// <summary>
      /// ������������ ��� �������� �������� �����
      /// ��������, ��� ������� (Level=Region) ������� ���� ��������� ������ (ChildLevel=District), � ����� - ������ (ChildLevel=City).
      /// ������� � ��� �������� � ��������� ������ �������� �������� Unknown, ����� ������� ������� ������
      /// </summary>
      public FiasLevel ChildLevel;

      #endregion

      #region ��� �������

      public override string ToString()
      {
        return "Level=" + _Level.ToString() + ", AddressCount=" + _Addresses.Length.ToString() + ", Index=" + Index.ToString();
      }

      #endregion
    }

    private class FiasEnumerator : SimpleDisposableObject, IEnumerator<FiasAddress>
    {
      // 03.01.2021
      // ����� ������������ ������� ����� ��� �����������

      #region ����������� � Dispose

      public FiasEnumerator(FiasEnumerable owner)
      {
        _Owner = owner;
        _Handler = new FiasHandler(owner._Source);
        _Handler.FillAddress(owner.BaseAddress);
        if (owner.BaseAddress.Messages.Severity == ErrorMessageKind.Error)
          throw new InvalidOperationException("������� ����� �������� ������");
        _Stack = new Stack<LevelInfo>();
        _Current = new FiasAddress(); // ���������� ������������ ������

        FiasLevel firstLevel = FiasLevel.Region;
        if (!owner._BaseAddress.IsEmpty)
          firstLevel = owner._BaseAddress.GuidBottomLevel;
        int p1 = FiasTools.AllLevelIndexer.IndexOf(firstLevel);
        if (p1 < 0)
          throw new BugException("����������� ������ �������: " + firstLevel.ToString());
        int p2 = FiasTools.AllLevelIndexer.IndexOf(owner.BottomLevel);
        if (p2 < 0)
          throw new BugException("����������� ������ �������: BottomLevel=" + owner.BottomLevel.ToString());
        List<FiasLevel> lst = new List<FiasLevel>();
        for (int p = p1; p <= p2; p++)
        {
          FiasLevel lvl = FiasTools.AllLevels[p];
          switch (lvl)
          {
            case FiasLevel.Building:
            case FiasLevel.Structure:
              // ��� ����� ����
              break;
            case FiasLevel.AutonomousArea:
            case FiasLevel.InnerCityArea:
            case FiasLevel.PlanningStructure:
            case FiasLevel.Settlement:
            case FiasLevel.LandPlot:
              // ��� ������ �� ����� ����
              break;
            default:
              lst.Add(lvl);
              break;
          }
        }
        if (lst.Count == 0)
          throw new InvalidOperationException("������� ����� \"" + owner.BaseAddress.ToString() + "\" �� ��������� � �������� ������ ������� ������������ BottomLevel=" + owner.BottomLevel.ToString());
        _EnumLevels = lst.ToArray();
        _EnumLevelIndexer = new ArrayIndexer<FiasLevel>(_EnumLevels);
        _TopChildLevel = FiasLevel.Unknown;
      }

      protected override void Dispose(bool disposing)
      {
        if (disposing)
          Reset();
        base.Dispose(disposing);
      }

      #endregion

      #region ������ �������

      /// <summary>
      /// ������, ������� ����� ������������� ������, ������� � BaseAddress.GuidBottomLevel � ���������� BottomLevel
      /// </summary>
      private FiasLevel[] _EnumLevels;

      /// <summary>
      /// ���������� ��� ������� _EnumLevels
      /// </summary>
      private ArrayIndexer<FiasLevel> _EnumLevelIndexer;

      #endregion

      #region ����

      private FiasEnumerable _Owner;

      private FiasHandler _Handler;

      private Stack<LevelInfo> _Stack;

      /// <summary>
      /// ��������, ����������� LevelInfo.ChildLevel, �� ������������ ��� ������������ ������ �������� ������
      /// </summary>
      private FiasLevel _TopChildLevel;

      private FiasAddress _Current;

      #endregion

      #region IEnumerator<FiasAddress> Members

      public FiasAddress Current { get { return _Current; } }

      object System.Collections.IEnumerator.Current { get { return _Current; } }

      public void Reset()
      {
        //while (_Stack.Count > 0)
        //{
        //  LevelInfo li = _Stack.Pop();
        //  li.Dispose();
        //}
        _Stack.Clear();
        _TopChildLevel = FiasLevel.Unknown;
      }

      /// <summary>
      /// �������� �����
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        if (_Stack.Count == 0)
        {
          // ������ ���
          _TopChildLevel = GetNextLevel(_Owner.BaseAddress.GuidBottomLevel, _Owner.BaseAddress.GuidBottomLevel);
          AddNextLevel(_Owner.BaseAddress, _TopChildLevel);
          if (_TopChildLevel == FiasLevel.Unknown)
            return false;

#if XXX

            LevelInfo li = new LevelInfo(FiasLevel.Region, page.CreateDataView());
            _Stack.Push(li);


            string colName;
            switch (li.Level)
            {
              case FiasLevel.Room: colName = "ROOMGUID"; break;
              case FiasLevel.House: colName = "HOUSEGUID"; break;
              default: colName = "AOGUID"; break;
            }
            int pCol = li.DV.Table.Columns.IndexOf(colName);
#if DEBUG
            if (pCol < 0)
              throw new BugException("�� ������ ������� " + colName);
#endif

            li.Index = -2;
            for (int i = 0; i < li.DV.Count; i++)
            {
              Guid thisG = DataTools.GetGuid(li.DV[i].Row[pCol]);
              if (thisG == g)
              {
                li.Index = i;
                break;
              }
            }
            if (li.Index == -2)
              throw new InvalidOperationException("�� ������ " + colName + "=" + g.ToString());
#endif
        }

#if DEBUG
        if (_Stack.Count == 0)
          throw new BugException("_Stack.Count==0");
#endif

        while (true)
        {
          LevelInfo currLI = _Stack.Peek();

          //if (currLI.ChildLevel == FiasLevel.Unknown)
          //{
          currLI.Index++;
          if (currLI.Index < currLI.Addresses.Length)
          {
            // ���������� ������� �����
            _Current = currLI.Addresses[currLI.Index];

            // � ��������� ��� ��������� �������� �������
            currLI.ChildLevel = GetNextLevel(currLI.Level, currLI.Level);
            if (currLI.ChildLevel == FiasLevel.Unknown)
            {
              // ��� �� ������ ��������� ������
            }
            else
            {
              AddNextLevel(_Current, currLI.ChildLevel);
            }

            return true;
          }
          else
          {
            // ������ ��� ������ �������� ������ ������ ������ � ���� ���������� ������ ������
            // 
            _Stack.Pop();
            if (_Stack.Count == 0)
            {
#if DEBUG
              if (_TopChildLevel == FiasLevel.Unknown)
                throw new BugException("_TopChildLevel==FiasLevel.Unknown");
#endif

              while (true)
              {
                //FiasLevel xxx = _TopChildLevel;
                _TopChildLevel = GetNextLevel(_TopChildLevel, _TopChildLevel);
                if (_TopChildLevel == FiasLevel.Unknown)
                  return false;
                if (FiasTools.IsInheritableLevel(_Owner._BaseAddress.GuidBottomLevel, _TopChildLevel, false))
                  break; // ������������� ������ ����� �� �����
              }
              AddNextLevel(_Owner._BaseAddress, _TopChildLevel);
              continue;
            }
            else
            {
              currLI = _Stack.Peek();
              FiasLevel orgLevel = currLI.ChildLevel;
              if (currLI.ChildLevel == FiasLevel.Unknown)
                currLI.ChildLevel = GetNextLevel(currLI.Level, currLI.Level);
              else
                currLI.ChildLevel = GetNextLevel(currLI.Level, currLI.ChildLevel);

              if (currLI.ChildLevel != FiasLevel.Unknown)
              {
                _Current = currLI.Addresses[currLI.Index];
                AddNextLevel(_Current, currLI.ChildLevel);
                continue;
              }
            }
          }

          //// �� ������� ������ ��������� ������.
          //// ��������� � �������� ������
          //_Stack.Pop();
          //if (_Stack.Count == 0)
          //  return false; // ������������ ���������
          //}
          //else
          //{ 
          //  // ����
          //}
        } // while true
      }

      /// <summary>
      /// ���������� �������, ������� �������� �������� �� ��������� � ���������.
      /// �����������, ��� ������������ �����������.
      /// ���� ���������� ������� ������� �� BottomLevel, ������������ Guid.Unknown
      /// </summary>
      /// <param name="baseLevel"></param>
      /// <param name="prevLevel"></param>
      /// <returns></returns>
      private FiasLevel GetNextLevel(FiasLevel baseLevel, FiasLevel prevLevel)
      {
        if (prevLevel == FiasLevel.Unknown)
          return FiasLevel.Region;

        int p = _EnumLevelIndexer.IndexOf(prevLevel);
        if (p < 0)
          throw new ArgumentException("������ " + prevLevel.ToString() + " ��� � ������", "prevLevel");

        for (int i = p + 1; i < _EnumLevels.Length; i++)
        {
          if (FiasTools.IsInheritableLevel(baseLevel, _EnumLevels[i], false) &&
            FiasTools.IsInheritableLevel(prevLevel, _EnumLevels[i], false))
            return _EnumLevels[i];
        }
        return FiasLevel.Unknown;
      }

      private void AddNextLevel(FiasAddress baseAddress, FiasLevel nextLevel)
      {
        DataView dv;
        switch (nextLevel)
        {
          case FiasLevel.Flat:
            FiasCachedPageRoom page3 = _Handler.GetRoomPage(baseAddress.GetGuid(FiasLevel.House));
            dv = page3.CreateDataView();
            break;
          case FiasLevel.House:
            FiasCachedPageHouse page2 = _Handler.GetHousePage(baseAddress.AOGuid);
            dv = page2.CreateDataView();
            break;
          default:
            //Guid pageGuid = _Handler.Source.GetGuidInfo(new Guid[1] { baseAddress.AOGuid }, FiasTableType.AddrOb)[baseAddress.AOGuid].ParentGuid;
            FiasCachedPageAddrOb page1 = _Handler.GetAddrObPage(nextLevel, baseAddress.AOGuid);
            dv = page1.CreateDataView();
            break;
        }
#if DEBUG
        if (dv == null)
          throw new BugException("dv==null");
#endif

        try
        {
          string recIdColName = DataTools.GetPrimaryKey(dv.Table);
          int pRecIdCol = dv.Table.Columns.IndexOf(recIdColName);
#if DEBUG
          if (pRecIdCol < 0)
            throw new BugException("�� ������� ���� \"" + recIdColName + "\"");
#endif

          FiasAddress[] a = new FiasAddress[dv.Count];
          for (int i = 0; i < a.Length; i++)
          {
            a[i] = new FiasAddress();
            Guid recId = DataTools.GetGuid(dv[i].Row[pRecIdCol]);
            a[i].SetRecId(nextLevel, recId);
          }
          _Handler.FillAddresses(a);
          _Stack.Push(new LevelInfo(nextLevel, a));
        }
        finally
        {
          dv.Dispose();
        }
      }

      #endregion
    }
  }
}
