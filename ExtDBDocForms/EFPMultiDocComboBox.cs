using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

/*
 * The BSD License
 * 
 * Copyright (c) 2018, Ageyev A.V.
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

namespace FreeLibSet.Forms.Docs
{
  // ���������� ������ ���������� ����������

  #region ��������

  /// <summary>
  /// ��������� ������� EFPMultiDocComboBoxBase.TextValueNeeded
  /// </summary>
  public class EFPMultiDocComboBoxTextValueNeededEventArgs : EFPComboBoxTextValueNeededEventArgs
  {
    #region �����������

    /// <summary>
    /// ��������� �����������
    /// </summary>
    /// <param name="owner">������-��������</param>
    public EFPMultiDocComboBoxTextValueNeededEventArgs(EFPMultiDocComboBoxBase owner)
    {
      _Owner = owner;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ��������������
    /// </summary>
    public Int32[] Ids { get { return _Owner.Ids; } }

    private EFPMultiDocComboBoxBase _Owner;

    #endregion
  }

  /// <summary>
  /// ������� ������� EFPMultiDocComboBoxBase.TextValueNeeded
  /// </summary>
  /// <param name="sender">���������</param>
  /// <param name="args">��������� �������</param>
  public delegate void EFPMultiDocComboBoxTextValueNeededEventHandler(object sender,
    EFPMultiDocComboBoxTextValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// ������� ����� ���������� ���������� ������ ���������� ����������.
  /// ��������� �������� Ids ��� ������ ���������������.
  /// ������ ������������ IdList, �.�. ������� ������������� �����
  /// </summary>
  public abstract class EFPMultiDocComboBoxBase : EFPAnyDocComboBoxBase, IDepSyncObject
  {
    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="ui">���������������� ��������� ��� ������� � ����������</param>
    public EFPMultiDocComboBoxBase(EFPBaseProvider baseProvider, UserSelComboBox control, DBUI ui)
      : base(baseProvider, control, ui)
    {
      _TextValueNeededArgs = new EFPMultiDocComboBoxTextValueNeededEventArgs(this);
      _Ids = DataTools.EmptyIds;

      _MaxTextItemCount = 1;
    }

    #endregion

    #region �������� Ids - �������������� �����

    /// <summary>
    /// ��������������� �� �����
    /// </summary>
    private bool _InsideSetIds;

    /// <summary>
    /// ������ ��������� ���������������.
    /// ���� �������� �� ������, �������� �������� ������ ������.
    /// � �������-����������� �������� �����������������.
    /// </summary>
    internal protected virtual Int32[] Ids
    {
      get { return _Ids; }
      set
      {
        if (_InsideSetIds)
          return;

        if (value == null)
          value = DataTools.EmptyIds;
        if (Object.ReferenceEquals(value, _Ids))
          return;

        for (int i = 0; i < value.Length; i++)
        {
          if (value[i] == 0)
            throw new ArgumentException("������ ������������� �� ����� ��������� �������� 0");
        }

        _InsideSetIds = true;
        try
        {
          _Ids = value;

          if (_IdsEx != null)
            _IdsEx.Value = value;
          if (_SingleIdEx != null)
            _SingleIdEx.Value = SingleId;
          if (_DeletedEx != null)
            _DeletedEx.SetDelayed();
          InitTextAndImage();
          ClearButtonEnabled = (_Ids.Length > 0);
          //if (IdValueChangedBeforeValidate != null)
          //  IdValueChangedBeforeValidate(this, EventArgs.Empty);
          Validate();
          DoSyncValueChanged();

          if (CommandItems is EFPAnyDocComboBoxBaseControlItems)
            ((EFPAnyDocComboBoxBaseControlItems)CommandItems).InitEnabled();
        }
        finally
        {
          _InsideSetIds = false;
        }
      }
    }
    private Int32[] _Ids;

    /// <summary>
    /// ������������� ����� ���������� ���������. 
    /// � �������-����������� �������� �����������������.
    /// </summary>
    internal protected DepValue<Int32[]> IdsEx
    {
      get
      {
        InitIdsEx();
        return _IdsEx;
      }
      set
      {
        InitIdsEx();
        _IdsEx.Source = value;
      }
    }

    private void InitIdsEx()
    {
      if (_IdsEx == null)
      {
        _IdsEx = new DepInput<Int32[]>(Ids, IdsEx_ValueChanged);
        _IdsEx.OwnerInfo = new DepOwnerInfo(this, "IdsEx");
      }
    }
    private DepInput<Int32[]> _IdsEx;

    ///// <summary>
    ///// ��� ������� ���������� ��� ��������� �������� �������� ��������������, ��
    ///// �� ������ ������ Validate(). 
    ///// </summary>
    //internal event EventHandler IdsValueChangedBeforeValidate;

    private void IdsEx_ValueChanged(object sender, EventArgs args)
    {
      Ids = _IdsEx.Value;
    }


    /// <summary>
    /// ���������� true ��� Id �� ������ 0.
    /// </summary>
    public override bool IsNotEmpty { get { return Ids.Length > 0; } }

    /// <summary>
    /// ������-������� ���������� true, ���� ���� ���� �� ���� ��������� �������������.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, Int32[]>(IdsEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(Int32[] ids)
    {
      return ids.Length > 0;
    }

    #endregion

    #region �������� SingleId

    /// <summary>
    /// ������������� ������������� ���������� ���������. 
    /// � �������-����������� �������� �����������������.
    /// </summary>
    protected Int32 SingleId
    {
      get
      {
        if (Ids.Length == 1)
          return Ids[0];
        else
          return 0;
      }
      set
      {
        if (_InsideSetIds)
          return;

        if (value == 0)
          Ids = DataTools.EmptyIds;
        else
          Ids = new Int32[] { value };
      }
    }


    /// <summary>
    /// ������������� ������������� ���������� ���������. 
    /// � �������-����������� �������� �����������������.
    /// </summary>
    protected DepValue<Int32> SingleIdEx
    {
      get
      {
        InitSingleIdEx();
        return _SingleIdEx;
      }
      set
      {
        InitSingleIdEx();
        _SingleIdEx.Source = value;
      }
    }

    private void InitSingleIdEx()
    {
      if (_SingleIdEx == null)
      {
        _SingleIdEx = new DepInput<Int32>(SingleId,SingleIdEx_ValueChanged);
        _SingleIdEx.OwnerInfo = new DepOwnerInfo(this, "SingleIdEx");
      }
    }
    private DepInput<Int32> _SingleIdEx;

    private void SingleIdEx_ValueChanged(object sender, EventArgs args)
    {
      SingleId = _SingleIdEx.Value;
    }

    #endregion

    #region ������� TextValueNeeded

    /// <summary>
    /// ��� ������� ���������� ����� ������ �������� �� ������ ��� ��������� ��������
    /// Id � ��������� �������������� ����� � ����������, ����� ������������� ���������
    /// � �����������. ������� ���������� � ��� ����� � ��� Id=0
    /// ����� ���������� ��� ��������� � �������� TextValue
    /// </summary>
    public event EFPMultiDocComboBoxTextValueNeededEventHandler TextValueNeeded
    {
      // 18.03.2016
      // ����� ��������� ����������� ��������� �������� �����, �.�. ���������� ����� �������� ����� ��� Id=0
      // ��� �������� Id ����� ���� ��� ����������� �� ������������� �����������
      // ������������:
      // ������� InitTextAndImage() �� OnShown(), �� ����� �������� TextValue � ������ ����� �����
      // ������������ �������� �� ������ �� �����
      add
      {
        _TextValueNeeded += value;
        InitTextAndImage();
      }
      remove
      {
        _TextValueNeeded -= value;
        InitTextAndImage();
      }
    }
    private EFPMultiDocComboBoxTextValueNeededEventHandler _TextValueNeeded;

    #endregion

    #region InitTextAndImage

    /// <summary>
    /// ����� �� ��������� ������ ������ ���, ������� ��� � ������������.
    /// ����� ���������� ��� �������� ����������� ����� ������� InitText() �
    /// ��� ������� � ����������
    /// </summary>
    private EFPMultiDocComboBoxTextValueNeededEventArgs _TextValueNeededArgs;

    /// <summary>
    /// ��������� ������ ��������
    /// EFPDocComboBox ������������ ����� ��� ��������� ����������� ������ Edit
    /// </summary>
    protected override void InitTextAndImage()
    {
      try
      {
        _TextValueNeededArgs.Clear();
        // ����������� �������� ������, ��������� � �����������
        if (Ids.Length == 0)
        {
          _TextValueNeededArgs.TextValue = EmptyText;
          _TextValueNeededArgs.ImageKey = EmptyImageKey;
        }
        else
        {
          _TextValueNeededArgs.TextValue = DoGetText();
          if (EFPApp.ShowListImages)
          {
            _TextValueNeededArgs.ImageKey = DoGetImageKey();

            EFPDataGridViewColorType ColorType;
            bool Grayed;
            DoGetValueColor(out ColorType, out Grayed);
            _TextValueNeededArgs.Grayed = Grayed;
          }
          else
            _TextValueNeededArgs.ImageKey = String.Empty;
          if (EFPApp.ShowToolTips)
            _TextValueNeededArgs.ToolTipText = DoGetValueToolTipText();
          else
            _TextValueNeededArgs.ToolTipText = String.Empty;
        }

        // ���������������� ����������
        if (_TextValueNeeded != null)
          _TextValueNeeded(this, _TextValueNeededArgs);

        // ������������� ��������. ����������� ������������ ��������
        Control.Text = _TextValueNeededArgs.TextValue;
        if (EFPApp.ShowListImages)
        {
          if (String.IsNullOrEmpty(_TextValueNeededArgs.ImageKey))
            Control.Image = null;
          else
            Control.Image = EFPApp.MainImages.Images[_TextValueNeededArgs.ImageKey];
        }
        if (EFPApp.ShowToolTips)
          ValueToolTipText = _TextValueNeededArgs.ToolTipText;
      }
      catch (Exception e)
      {
        Control.Text = "!!! ������ !!! " + e.Message;
        if (EFPApp.ShowListImages)
          Control.Image = EFPApp.MainImages.Images["Error"];
        EFPApp.ShowTempMessage("������ ��� ��������� ������: " + e.Message);
      }
      if (UI.DebugShowIds)
        Control.Text = "Id=" + DataTools.CommaStringFromIds(Ids, false) + " " + Control.Text;
    }

    /// <summary>
    /// ��������� ������ ��� �������� ��������, ���� Id!=0
    /// </summary>
    /// <returns>��������� �������������</returns>
    protected abstract string DoGetText();

    /// <summary>
    /// ��������� ����������� ��� �������� ��������, ���� Id!=0
    /// </summary>
    /// <returns>��� ����������� � EFPApp.MainImages</returns>
    protected virtual string DoGetImageKey()
    {
      return String.Empty;
    }

    /// <summary>
    /// ��������� ��������� ��� ������ ��������� / ������������
    /// </summary>
    /// <param name="colorType">���� ���������� ���� ������.
    /// ��� �������� �� ������������</param>
    /// <param name="grayed">���� ������ ���� �������� true, ���� �������� ������ ���� ������� ����� ������</param>
    protected virtual void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      colorType = EFPDataGridViewColorType.Normal;
      grayed = false;
    }

    /// <summary>
    /// ��������� ��������� ��� �������� ��������, ���� Id!=0
    /// </summary>
    /// <returns>��������� �� ��������</returns>
    protected virtual string DoGetValueToolTipText()
    {
      return String.Empty;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������������ ���������� ���������������, ������� ����� ���� �������, ��� �������
    /// ������������ �������� ���� ��������� ����� �������.
    /// ����� ������� ������ ���������, �� ���������� ��������� � �������.
    /// �� ��������� ����� 1.
    /// </summary>
    public int MaxTextItemCount
    {
      get { return _MaxTextItemCount; }
      set
      {
        if (value == _MaxTextItemCount)
          return;
        _MaxTextItemCount = value;
        InitTextAndImage();
      }
    }
    private int _MaxTextItemCount;

    #endregion

    #region �������� Deleted

    /// <summary>
    /// ���������� true, ���� ��������� �������� ��� ����������� ������
    /// ���� �������� �� ������, �� ������������ false
    /// </summary>
    public bool Deleted
    {
      get
      {
        for (int i = 0; i < Ids.Length; i++)
        {
          string Message;
          if (GetDeletedValue(Ids[i], out Message))
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��������� �������� ��� ����������� ������
    /// ���� �������� �� ������, �� ������������ false.
    /// ����������� �������� ��� Deleted
    /// </summary>
    public DepValue<bool> DeletedEx
    {
      get
      {
        if (_DeletedEx == null)
        {
          _DeletedEx = new DepValueDelayed<bool>();
          _DeletedEx.OwnerInfo = new DepOwnerInfo(this, "DeletedEx");
          _DeletedEx.ValueNeeded += new DepValueNeededEventHandler<bool>(Deleted_ValueNeeded);
        }
        return _DeletedEx;
      }
    }
    private DepValueDelayed<bool> _DeletedEx;

    void Deleted_ValueNeeded(object sender, DepValueNeededEventArgs<bool> args)
    {
      args.Value = Deleted;
    }

    /// <summary>
    /// ����������, ��� ��������� �������� ������
    /// </summary>
    /// <returns></returns>
    protected abstract bool GetDeletedValue(Int32 id, out string message);

    /// <summary>
    /// ��������, ��� �������� Deleted, ��������, ����������
    /// </summary>
    protected void SetDeletedChanged()
    {
      if (_DeletedEx != null)
        _DeletedEx.SetDelayed();
    }


    #endregion

    #region ���������������� ������

    /// <summary>
    /// �������� ������������ ��������.
    /// �������������� ����������� � ����������� �� ������� CanBeEmpty � 
    /// CanBeDeleted (� ��������������� ������������ ������ GetDeletedValue())
    /// </summary>
    protected override void OnValidate()
    {
      if (Ids.Length == 0)
      {
        if (CanBeEmpty)
        {
          if (WarningIfEmpty)
            SetWarning("�������� \"" + DisplayName + "\", ��������, ������ ���� ������� �� ������");
        }
        else
        {
          SetError("�������� \"" + DisplayName + "\" ������ ���� ������� �� ������");
          return;
        }
      }
      else if (!CanBeDeleted)
      {
        for (int i = 0; i < Ids.Length; i++)
        {
          string Message;
          if (GetDeletedValue(Ids[i], out Message))
          {
            if (WarningIfDeleted)
              SetWarning(Message);
            else
              SetError(Message);
          }
        }
      }
    }

    /// <summary>
    /// ���������� ��� ������������� ������������������ �������� �������� Id ��� ���������� ���������� IDepSyncObject.
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return Ids;
      }
      set
      {
        Ids = (Int32[])value;
      }
    }

    /// <summary>
    /// ������������� Id=0
    /// </summary>
    public override void Clear()
    {
      Ids = DataTools.EmptyIds;
    }

    #endregion

    //#region �������� ����� ������ ��������� ��� ������������

    ///// <summary>
    ///// �������� �������� ����, ��������������� ���������� ��������� ��� ������������
    ///// </summary>
    ///// <param name="ColumnName">��� ����</param>
    ///// <returns>�������������� �������� ���� ��� null, ���� �������� �� �������</returns>
    //public abstract object GetColumnValue(string ColumnName);

    //#endregion
  }

  /// <summary>
  /// ������� ����� ��� EFPMultiDocComboBox.
  /// ��������� � EFPMultiDocComboBoxBase ��������� ��������
  /// </summary>
  public abstract class EFPMultiDocComboBoxBaseWithFilters : EFPMultiDocComboBoxBase
  {
    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="ui">���������������� ��������� ��� ������� � ����������</param>
    public EFPMultiDocComboBoxBaseWithFilters(EFPBaseProvider baseProvider, UserSelComboBox control, DBUI ui)
      : base(baseProvider, control, ui)
    {
      _ClearByFilter = true;
    }

    #endregion

    #region �������� Filters

    /// <summary>
    /// �������������� ������� ��� ������ ���������� �� �����������
    /// </summary>
    public GridFilters Filters
    {
      get
      {
        if (_Filters == null)
        {
          _Filters = new GridFilters();
          _Filters.Changed += new EventHandler(HandleFiltersChanged);
        }
        return _Filters;
      }
    }
    private GridFilters _Filters;

    /// <summary>
    /// ���������� ����� �������� (�������� DocFilters.Count, ������� ������ �������)
    /// </summary>
    public int FilterCount
    {
      get
      {
        if (_Filters == null)
          return 0;
        else
          return _Filters.Count;
      }
    }


    /// <summary>
    /// ���������� ��� ��������� ��������
    /// </summary>
    private void HandleFiltersChanged(object sender, EventArgs args)
    {
      OnFiltersChanged();
    }

    /// <summary>
    /// ���������� ��� ��������� ��������
    /// </summary>
    protected virtual void OnFiltersChanged()
    {
      FilterPassed = TestFilter();
      if (ClearByFilter)
      {
        if (!FilterPassed)
          Ids = DataTools.EmptyIds;
      }
      else
        Validate();
    }

    /// <summary>
    /// �������� ������������ ���� ��������� ���������� ������� DocFilters
    /// </summary>
    /// <returns>True, ���� ��� ��������� ������������� ������� ��� ��� ���������� ��� DocFilters ���������</returns>
    public bool TestFilter()
    {
      Int32 BadId;
      DBxCommonFilter BadFilter;
      return TestFilter(out BadId, out BadFilter);
    }

    /// <summary>
    /// �������� ������������ ��������� Id ������� DocFilters
    /// </summary>
    /// <param name="badId">���� ������������ ������������� ���������, �� ���������� ������</param>
    /// <param name="badFilter">���� ������������ ������ �� ������ ������ � ������ Filters, ������� �� ������������� ���������� ���������</param>
    /// <returns>True, ���� �������� �������������, ��� DocId=0 ��� DocFilters ���������</returns>
    public bool TestFilter(out Int32 badId, out DBxCommonFilter badFilter)
    {
      badFilter = null;
      badId = 0;

      if (FilterCount == 0)
        return true;
      if (_Filters.IsEmpty)
        return true;

      for (int i = 0; i < Ids.Length; i++)
      {
        if (!DoTestFilter(Ids[i], out badFilter))
        {
          badId = Ids[i];
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// �������� ��������� � ������.
    /// ����� ���������, ��� ������ � ��������������� <paramref name="id"/> ��������
    /// ������� ��������.
    /// </summary>
    /// <param name="id">������������� ��������� ��� ������������</param>
    /// <param name="badFilter">���� �����-���� ������ �� ��������, �� ������������
    /// ������ ������ � ������ Filters, ������� "�� ����������" ������.
    /// ���� ������ �������� ��� �������, ���� ���������� null</param>
    /// <returns>True, ���� ������ �������� ��� �������</returns>
    protected abstract bool DoTestFilter(Int32 id, out DBxCommonFilter badFilter);

    /// <summary>
    /// �������� ������������ ��������.
    /// ������������� � �������� ������, ����������� ��������� ��������� ������ � ������
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Error)
        return;

      try
      {
        Int32 BadId;
        DBxCommonFilter BadFilter;
        if (!TestFilter(out BadId, out BadFilter))
          SetError("�������� �������� �� �������� ������ \"" + BadFilter.DisplayName + "\" (" + ((IEFPGridFilter)(BadFilter)).FilterText + ")");
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ �������� ������������ �������� \"" + DisplayName + "\" �������");
        SetError(e.Message);
      }
    }

    #endregion

    #region �������� ClearByFilter

    /// <summary>
    /// ��������� �� ������� �������� �������� ��� ����� �������, ���� �������� ��
    /// �������� ������� ��� ������ �������
    /// �������� �� ��������� - true (�������)
    /// ���� ��������� � false, �� "������������" �������� ��������� ���������, ��
    /// ��� �������� Validate() ����� ���������� ������
    /// </summary>
    public bool ClearByFilter
    {
      get { return _ClearByFilter; }
      set
      {
        if (value == _ClearByFilter)
          return;
        _ClearByFilter = value;
        if (_ClearByFilterEx != null)
          _ClearByFilterEx.Value = value;

        // ��� ������������ ������� ���������� - ����, ��� � ��� ��������� �������
        OnFiltersChanged();
      }
    }
    private bool _ClearByFilter;

    /// <summary>
    /// ��������, ����������� ��� ��������� �������, ����� ������� �������� DocId
    /// �� ������������� ������ �������
    /// True (�� ���������) - �������� DocId � 0
    /// False - �������� ������� ��������, �� �������� ������.
    /// �������� �������� �� ������ �� ��������, ����������� ��� ��������� �������������
    /// �������� DocId. � ���� ������ ������ ������������ ������
    /// </summary>
    public DepValue<Boolean> ClearByFilterEx
    {
      get
      {
        InitClearByFilterEx();
        return _ClearByFilterEx;
      }
      set
      {
        InitClearByFilterEx();
        _ClearByFilterEx.Source = value;
      }
    }

    private void InitClearByFilterEx()
    {
      if (_ClearByFilterEx == null)
      {
        _ClearByFilterEx = new DepInput<bool>(ClearByFilter, ClearByFilterEx_ValueChanged);
        _ClearByFilterEx.OwnerInfo = new DepOwnerInfo(this, "ClearByFilterEx");
      }
    }
    private DepInput<Boolean> _ClearByFilterEx;

    private void ClearByFilterEx_ValueChanged(object sender, EventArgs args)
    {
      ClearByFilter = _ClearByFilterEx.Value;
    }

    #endregion

    #region �������� FilterPassed

    /// <summary>
    /// �������� ���������� true, ���� ������� ��������� �������� (Id) �������� ������� �������.
    /// ���� �������� ClearByFilter ����� �������� true (�� ���������), �� FilterPassed ������
    /// ���������� true, �.�. ������������ �������� ���������� �������������
    /// </summary>
    public bool FilterPassed
    {
      get { return _FilterPassed; }
      private set
      {
        _FilterPassed = value;
        if (_FilterPassedEx != null)
          _FilterPassedEx.OwnerSetValue(value);
      }
    }
    private bool _FilterPassed;

    /// <summary>
    /// ������ �������� FilterPassed, � ������� ������� ����� ��������� ���������� ����������
    /// </summary>
    public DepValue<bool> FilterPassedEx
    {
      get
      {
        if (_FilterPassedEx == null)
        {
          _FilterPassedEx = new DepOutput<bool>(_FilterPassed);
          _FilterPassedEx.OwnerInfo = new DepOwnerInfo(this, "FilterPassedEx");
        }
        return _FilterPassedEx;
      }
    }
    private DepOutput<bool> _FilterPassedEx;

    #endregion
  }

  /// <summary>
  /// ��������� ������ ���������� ����������
  /// </summary>
  public class EFPMultiDocComboBox : EFPMultiDocComboBoxBaseWithFilters
  {
    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="docTypeUI">���������������� ��������� ��� ������� � ����������</param>
    public EFPMultiDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, DocTypeUI docTypeUI)
      : base(baseProvider, control, docTypeUI.UI)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
      this.DocType = docTypeUI.DocType;

      control.EditButton = true;
      control.EditClick += new EventHandler(Control_EditClick);
      if (EFPApp.ShowToolTips) // 15.03.2018
      {
        control.PopupButtonToolTipText = "�������: "+DocType.PluralTitle;
        control.ClearButtonToolTipText = "�������� ���� ������";
      }
    }

    /// <summary>
    /// ������ ����������������� ���������� ��� ���� ����������.
    /// �� ����� ���� null.
    /// �������� DocType, DocTypeUI � DocTypeName ����������������.
    /// </summary>
    public DocTypeUI DocTypeUI
    {
      get { return UI.DocTypes[DocType.Name]; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        DocType = value.DocType;
      }
    }

    #endregion

    #region ������� ��� ���������

    #region DocType

    /// <summary>
    /// �������� ���� ���������, �� �������� ����� ���������� ���������.
    /// �� ����� ���� null.
    /// �������� DocType, DocTypeUI, DocTypeName � DocTableId ����������������.
    /// </summary>
    public DBxDocType DocType
    {
      get { return _DocType; }
      set
      {
        if (value == null)
          throw new ArgumentException();
        if (value == _DocType)
          return;
        _DocType = value;
        DocIds = DataTools.EmptyIds;
        if (_DocTableIdEx != null)
          _DocTableIdEx.Value = DocTableId;
        if (_DocTypeNameEx != null)
          _DocTypeNameEx.Value = DocTypeName;

        InitTextAndImage();
        Validate();
      }
    }

    private DBxDocType _DocType;

    #endregion

    #region DocTypeName

    /// <summary>
    /// ��� ������� ����������, �� �������� ����� ���������� ���������.
    /// �� ����� ���� null ��� ������ �������.
    /// �������� DocType, DocTypeUI, DocTypeName � DocTableId ����������������.
    /// </summary>
    public string DocTypeName
    {
      get
      {
        return DocType.Name;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        DocType = UI.DocTypes[value].DocType;
      }
    }

    /// <summary>
    /// ����������� �������� ��� DocTypeName.
    /// </summary>
    public DepValue<string> DocTypeNameEx
    {
      get
      {
        InitDocTypeNameEx();
        return _DocTypeNameEx;
      }
      set
      {
        InitDocTypeNameEx();
        _DocTypeNameEx.Source = value;
      }
    }

    private void InitDocTypeNameEx()
    {
      if (_DocTypeNameEx == null)
      {
        _DocTypeNameEx = new DepInput<string>(DocTypeName,DocTypeNameEx_ValueChanged);
        _DocTypeNameEx.OwnerInfo = new DepOwnerInfo(this, "DocTypeNameEx");
      }
    }

    private DepInput<string> _DocTypeNameEx;

    private void DocTypeNameEx_ValueChanged(object sender, EventArgs args)
    {
      DocTypeName = _DocTypeNameEx.Value;
    }

    #endregion

    #region TableId

    /// <summary>
    /// ������������� ������� ���� ���������� (�������� DBxDocType.TableId).
    /// �������� DocType, DocTypeUI, DocTypeName � DocTableId ����������������.
    /// </summary>
    public Int32 DocTableId
    {
      get
      {
        return _DocType.TableId;
      }
      set
      {
        DBxDocType NewDocType = UI.DocProvider.DocTypes.FindByTableId(value);
        if (NewDocType == null)
        {
          if (value == 0)
            throw new ArgumentException("������������� ������� ���������� �� ����� ���� ����� 0");
          else
            throw new ArgumentException("����������� ������������� ������� ���������� " + value.ToString());
        }
        DocType = NewDocType;
      }
    }

    /// <summary>
    /// ������������� ������� ���� ���������� (�������� DBxDocType.TableId).
    /// ����������� �������� ��� DocTableId
    /// </summary>
    public DepValue<Int32> DocTableIdEx
    {
      get
      {
        InitDocTableIdEx();
        return _DocTableIdEx;
      }
      set
      {
        InitDocTableIdEx();
        _DocTableIdEx.Source = value;
      }
    }

    private void InitDocTableIdEx()
    {
      if (_DocTableIdEx == null)
      {
        _DocTableIdEx = new DepInput<Int32>(DocTableId,DocTableIdEx_ValueChanged);
        _DocTableIdEx.OwnerInfo = new DepOwnerInfo(this, "DocTableIdEx");
      }
    }

    private DepInput<Int32> _DocTableIdEx;

    private void DocTableIdEx_ValueChanged(object sender, EventArgs args)
    {
      DocTableId = _DocTableIdEx.Value;
    }

    #endregion

    #endregion

    #region ��������� �������������� ����������

    /// <summary>
    /// �������������� ��������� ����������
    /// </summary>
    public Int32[] DocIds
    {
      get { return base.Ids; }
      set { base.Ids = value; }
    }

    /// <summary>
    /// �������������� ��������� ����������.
    /// ����������� ��������
    /// </summary>
    public DepValue<Int32[]> DocIdsEx
    {
      get { return base.IdsEx; }
      set { base.IdsEx = value; }
    }

    #endregion

    #region ��������������� �������� ��� �������������� ���������

    /// <summary>
    /// ������������� ������������� ���������� ���������.
    /// ���� ��� ��������� ���������� ��� ������� ������ ������ ���������, �������� ���������� 0.
    /// ��������� �������� � 0 ������� ������ ��������� ���������� DocIds.
    /// ��������� ���������� �������� ������ ��������� ������������ ��������
    /// </summary>
    public Int32 SingleDocId
    {
      get { return base.SingleId; }
      set { base.SingleId = value; }
    }

    /// <summary>
    /// ������������� ������������� ���������� ���������.
    /// ����������� �������� ��� SingleDocId
    /// </summary>
    public DepValue<Int32> SingleDocIdEx
    {
      get { return base.SingleIdEx; }
      set { base.SingleIdEx = value; }
    }

    #endregion

    #region EditorCaller

    /// <summary>
    /// ����������� �������� ��������� �������� ��� �������� ��������� � ���������� ������
    /// ���� �������� �� ����������� (�� ���������), �� ��������� �������� ������������
    /// ��������� (��������� GridFilters, ���� ������, ��� �������� ��������������
    /// ��������� � ��������� ��������� �����������)
    /// </summary>
    public DocumentViewHandler EditorCaller
    {
      get { return _EditorCaller; }
      set { _EditorCaller = value; }
    }
    private DocumentViewHandler _EditorCaller;

    #endregion

    #region �������������� ��������� ��������

    /// <summary>
    /// ���� ����������� � true, �� ��� ��������� �������� ����������� ����� ����������
    /// �������. ���� ����� ���� ������ �������� ������� �������, �� ���������������
    /// �������� DocId.
    /// �� ��������� - false
    /// </summary>
    public bool AutoSelectByFilter
    {
      get { return _AutoSelectByFilter; }
      set
      {
        _AutoSelectByFilter = value;
        if (value)
          SelectByFilter();
      }
    }
    private bool _AutoSelectByFilter;

    /// <summary>
    /// ���������� ��� ��������� ��������.
    /// ��������� ����� ������, ���� ����������� �������� AutoSelectByFilter
    /// </summary>
    protected override void OnFiltersChanged()
    {
      if (AutoSelectByFilter)
      {
        if (SelectByFilter())
          return;
      }

      base.OnFiltersChanged();
    }

    /// <summary>
    /// ���������� �������� DocIds, ���������� ������� �������, ���� ������� ������ ����
    /// ����� ��������.
    /// </summary>
    /// <returns>true, ���� ���� ������������ ��������, false, ���� ���������� ������
    /// ������ ����������� ��������� ��� �� ������� �� ������ �����������</returns>
    public bool SelectByFilter()
    {
      DBxFilter Filter = Filters.GetSqlFilter();
      if (UI.DocProvider.DocTypes.UseDeleted) // 23.05.2021
        Filter &= DBSDocType.DeletedFalseFilter;
      Int32 NewId = UI.DocProvider.FindRecord(DocTypeName, Filter, true);
      if (NewId == 0)
        return false;

      DocIds = new Int32[] { NewId };
      return true;
    }

    #endregion

    #region ����� � ������

    /// <summary>
    /// ���������� ��������� ������������� ��� ��������� ����������.
    /// � ����������� �� ���������� ��������� ���������� � �������� MaxTextItemCount,
    /// ������������ ���� ��������� ������������� ���������� ����� �������, ���� ����� ����������
    /// ��������� ����������
    /// </summary>
    /// <returns>����� ��� ����������</returns>
    protected override string DoGetText()
    {
      if (DocIds.Length == 1)
        return UI.TextHandlers.GetTextValue(DocType.Name, DocIds[0]);
      else if (DocIds.Length <= MaxTextItemCount)
      {
        string[] a = new string[DocIds.Length];
        for (int i = 0; i < DocIds.Length; i++)
          a[i] = UI.TextHandlers.GetTextValue(DocType.Name, DocIds[i]);
        return String.Join(", ", a);
      }
      else
        return DocType.PluralTitle + " (" + DocIds.Length.ToString() + ")";
    }

    /// <summary>
    /// ���������� ��� ������ �� EFPApp.MainImages ��� ���������� ���������.
    /// ���� ������� ��������� ���������� � ��� ��� ���������� ������������ ������,
    /// �� ������������ ������ "DBxDocSelection".
    /// ����� �� ����������, ���� ��� ��������� ����������.
    /// </summary>
    /// <returns>��� �����������</returns>
    protected override string DoGetImageKey()
    {
      if (DocIds.Length < 1)
        return "UnknownState"; // ������
      string ImageKey = UI.DocTypes[DocTypeName].GetImageKey(DocIds[0]);
      for (int i = 1; i < DocIds.Length; i++)
      {
        string ImageKey2 = UI.DocTypes[DocTypeName].GetImageKey(DocIds[i]);
        if (ImageKey2 != ImageKey)
          return "DBxDocSelection";
      }
      return ImageKey;
    }

    /// <summary>
    /// ���������� ���� � ������� ��������� ����� ������, ���� ������ ����� ���� ��������.
    /// ���� ������� ��������� ����������, ������������
    /// ������� ��������� ����������.
    /// ����� �� ����������, ���� ��� ��������� ����������.
    /// </summary>
    /// <param name="colorType">���� ���������� ���� ��� ������ ���������</param>
    /// <param name="grayed">���� ������������ true, ���� �������� ���������� ����� ������</param>
    protected override void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      if (DocIds.Length == 1)
        UI.DocTypes[DocTypeName].GetRowColor(DocIds[0], out colorType, out grayed);
      else
        base.DoGetValueColor(out colorType, out grayed);
    }

    /// <summary>
    /// ���������� ����������� ��������� ��� ���������� ���������, ���� �� ����.
    /// ����� ������������ ����� "������� ����������: X".
    /// ����� �� ����������, ���� ��� ��������� ����������.
    /// </summary>
    /// <returns>����� ��� ����������� ���������</returns>
    protected override string DoGetValueToolTipText()
    {
      if (DocIds.Length == 1)
        return UI.DocTypes[DocTypeName].GetToolTipText(DocIds[0]);
      else
        return "������� ����������: " + DocIds.Length.ToString();
    }

    /// <summary>
    /// ������������ ����������� ������ "�������������"
    /// </summary>
    protected override void InitTextAndImage()
    {
      base.InitTextAndImage();

      if (DocType == null || DocIds.Length != 1)
      {
        Control.EditButtonEnabled = false;
        if (Selectable)
          Control.EditButtonKind = UserComboBoxEditButtonKind.Edit;
        else
          Control.EditButtonKind = UserComboBoxEditButtonKind.View;
        Control.EditButtonToolTipText = "������ �������������, �.�. ��� ���������� ���������";
        return;
      }

      switch (UI.DocProvider.DBPermissions.TableModes[DocTypeName])
      {
        case DBxAccessMode.Full:
          Control.EditButtonEnabled = true;
          if (Selectable)
          {
            Control.EditButtonKind = UserComboBoxEditButtonKind.Edit;
            Control.EditButtonToolTipText = "������������� ��������� �������� \"" + DocType.SingularTitle + "\"";
          }
          else
          {
            // �� ����� �������� DocType.TestEditable(), �.�. ����� ��������
            Control.EditButtonKind = UserComboBoxEditButtonKind.View;
            Control.EditButtonToolTipText = "����������� ��������� �������� \"" + DocType.SingularTitle + "\"";
          }
          break;

        case DBxAccessMode.ReadOnly:
          Control.EditButtonEnabled = true;
          Control.EditButtonKind = UserComboBoxEditButtonKind.View;
          Control.EditButtonToolTipText = "����������� ��������� �������� \"" + DocType.SingularTitle + "\"";
          if (Selectable)
            Control.EditButtonToolTipText += ". � ��� ��� ���� ��� �������������� ����������";
          break;

        case DBxAccessMode.None:
          Control.EditButtonEnabled = false;
          Control.EditButtonKind = UserComboBoxEditButtonKind.View;
          Control.EditButtonToolTipText += "� ��� ��� ���� ��� ��������� ���������� \"" + DocType.PluralTitle + "\"";
          break;
      }
    }

    #endregion

    #region ���������� ������

    /// <summary>
    /// ���������� ���� ������� ��� ������ ���������� ����������.
    /// ������������ ����� DocTypeUI.SelectDocs().
    /// ����� ��������������� �������� DocIds.
    /// </summary>
    protected override void DoPopup()
    {
      if (_DocType == null)
      {
        EFPApp.ShowTempMessage("��� ��������� �� �����");
        return;
      }

      DocSelectDialog dlg = new DocSelectDialog(DocTypeUI);
      dlg.SelectionMode = DocSelectionMode.MultiList;
      if (!String.IsNullOrEmpty(DisplayName))
        dlg.Title = DisplayName;
      dlg.CanBeEmpty = CanBeEmpty;
      if (Filters.Count > 0)
        dlg.Filters = Filters; // ����� ����� ��������� ����������� �������
      dlg.DocIds = DocIds;
      dlg.EditorCaller = EditorCaller;
      dlg.DialogPosition.PopupOwnerControl = Control;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      DocIds = dlg.DocIds;
    }

    ///// <summary>
    ///// �������� �������� ���� ��� ���������� ���������. ���������� null, ����
    ///// DocIdValue=0
    ///// </summary>
    ///// <param name="ColumnName">��� ����, �������� �������� ����� ��������</param>
    ///// <returns>�������� ����</returns>
    //public override object GetColumnValue(string ColumnName)
    //{
    //  if (DocType == null || DocId == 0)
    //    return null;
    //  return UI.TextHandlers.DBCache[DocType.Name].GetValue(DocId, ColumnName);
    //}

    /// <summary>
    /// �������� �� �������������� �������� ���������� ���������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_EditClick(object sender, EventArgs args)
    {
      try
      {
        switch (DocIds.Length)
        {
          case 0:
            EFPApp.ShowTempMessage("�������� �� ������");
            break;
          case 1:
            UI.DocTypes[DocType.Name].PerformEditing(DocIds[0], Control.EditButtonKind == UserComboBoxEditButtonKind.View);
            InitTextAndImage();
            SetDeletedChanged();
            Validate();
            DocIdsEx.OnValueChanged();
            break;
          default:
            EFPApp.ShowTempMessage("������� ��������� ����������");
            break;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ �������������� ���������");
      }
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ���������� DBxDocType.PluralTitle ������ "��� ��������"
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return DocTypeUI.DocType.PluralTitle; }
    }

    /// <summary>
    /// ���������� true, ���� �������� � ��������������� <paramref name="id"/> ������� �� ��������
    /// </summary>
    /// <param name="id">������������� ������������ ���������</param>
    /// <param name="message">���� ������������ ��������� "��������� �������� ������"</param>
    /// <returns>������� �� ��������</returns>
    protected override bool GetDeletedValue(Int32 id, out string message)
    {
      if (!UI.DocProvider.DocTypes.UseDeleted)
      {
        message = null;
        return false;
      }

      if (DataTools.GetBool(UI.TextHandlers.DBCache[DocType.Name].GetBool(id, "Deleted")))
      {
        message = "��������� �������� \"" + DocType.SingularTitle + "\" ������";
        return true;
      }
      else
      {
        message = null;
        return false;
      }
    }

    /// <summary>
    /// ���������� true, ���� �������� �������� ��� ������� � ������ Filters
    /// </summary>
    /// <param name="id">������������� ������������ ���������</param>
    /// <param name="badFilter">���� ������������ ������ �� ������ ������, ������� �� ���������� ��������</param>
    /// <returns>����������� ��������</returns>
    protected override bool DoTestFilter(Int32 id, out DBxCommonFilter badFilter)
    {
      badFilter = null;
      if (DocType == null)
        return true;

      // �������� ������ ��� ����������
      DBxColumnList ColList = new DBxColumnList();
      Filters.GetColumnNames(ColList);
      DBxColumns ColumnNames = new DBxColumns(ColList);

      object[] Values = UI.TextHandlers.DBCache[DocTypeName].GetValues(id, ColumnNames);
      return Filters.TestValues(ColumnNames, Values, out badFilter);
    }

    #endregion

    #region ������� ����������

    /// <summary>
    /// ���������� true
    /// </summary>
    public override bool GetDocSelSupported { get { return true; } }

    /// <summary>
    /// ���������� true
    /// </summary>
    public override bool SetDocSelSupported { get { return true; } }

    /// <summary>
    /// ��������� ������� ����������
    /// </summary>
    /// <param name="reason">������� ���������</param>
    /// <returns>�������</returns>
    protected override DBxDocSelection OnGetDocSel(EFPDBxGridViewDocSelReason reason)
    {
      DBxDocSelection DocSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      if (DocType != null && DocIds.Length > 0)
        UI.DocTypes[DocType.Name].PerformGetDocSel(DocSel, DocIds, reason);
      return DocSel;
    }

    /// <summary>
    /// ��������� ������� ����������.
    /// ���� � ������� <paramref name="docSel"/> ���� ��������� DocTypeName, ��
    /// ��� ��������������� � �������� ��������� (�������� DocIds), ��������� ������� ��������� �����.
    /// ����� �������� ��������� �� ������ � ������� ����� �� ��������.
    /// </summary>                                
    /// <param name="docSel">������� ����������</param>
    protected override void OnSetDocSel(DBxDocSelection docSel)
    {
      Int32[] NewIds = docSel[DocTypeName];
      if (NewIds.Length == 0)
        EFPApp.ShowTempMessage("� ������ ������ ��� ������ �� �������� \"" + DocType.SingularTitle + "\"");
      else
        DocIds = NewIds;
    }

    /// <summary>
    /// ���������� true
    /// </summary>
    public override bool DocInfoSupported { get { return true; } }

    #endregion

    #region �������� �������������� ���������

    /// <summary>
    /// ��������� ����������� ���������� ��������� �������������� ��������� <paramref name="docId"/> ��� �������� ��������� �������� DocIds.
    /// ���������� false, ���� �������� ������, � �������� CanBeDeleted=false � WarningIfDeleted=false
    /// (�������� �� ���������). ����� ���������� false, ���� �������� �� �������� ������� ������-���� ������� � ������ Filters.
    /// </summary>
    /// <param name="docId">������������� ������������ ���������</param>
    /// <returns>����������� ���������� ��������������</returns>
    public bool TestDocId(Int32 docId)
    {
      string Message;
      return TestDocId(docId, out Message);
    }

    /// <summary>
    /// ��������� ����������� ���������� ��������� �������������� ��������� <paramref name="docId"/> ��� �������� ��������� �������� DocIds.
    /// ���������� false, ���� �������� ������, � �������� CanBeDeleted=false � WarningIfDeleted=false
    /// (�������� �� ���������). ����� ���������� false, ���� �������� �� �������� ������� ������-���� ������� � ������ Filters.
    /// </summary>
    /// <param name="docId">������������� ������������ ���������</param>
    /// <param name="message">���� ������������ ��������� �� ������, ���� ���������� ����������</param>
    /// <returns>����������� ���������� ��������������</returns>
    public bool TestDocId(Int32 docId, out string message)
    {
      if (docId == 0)
      {
        if (CanBeEmpty)
        {
          message = null;
          return true;
        }

        message = "�� ����� ������������� ���������";
        return false;
      }
      UI.DocProvider.CheckIsRealDocId(docId);


      if (UI.DocProvider.DocTypes.UseDeleted)
      {
        if (DataTools.GetBool(UI.TextHandlers.DBCache[DocType.Name].GetBool(docId, "Deleted")))
        {
          if (!(CanBeDeleted))
          {
            message = "�������� \"" + DocType.SingularTitle + "\" ������";
            return false;
          }
        }
      }

      DBxCommonFilter BadFilter;
      if (!DoTestFilter(docId, out BadFilter))
      {
        message = "�������� �� �������� ������ \"" + BadFilter.DisplayName + "\"";
        return false;
      }

      message = null;
      return true;
    }


    #endregion
  }


  /// <summary>
  /// ���������� ��� ����������, ���������������� ��� ������ ���������� ������������� �� ������ ���������
  /// ������������ ������ ���� ��������� � ���� ������, � �� ���������� � �������� �������������� ���������
  /// </summary>
  public class EFPMultiSubDocComboBox : EFPMultiDocComboBoxBase
  {
    #region ������������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="subDocTypeUI">���������������� ��������� ��� ������� � �������������</param>
    public EFPMultiSubDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control , SubDocTypeUI subDocTypeUI)
      : base(baseProvider, control, subDocTypeUI.UI)
    {
      //if (subDocTypeUI == null)
      //  throw new ArgumentNullException("SubDocTypeUI");

      this._SubDocTypeUI = subDocTypeUI;

      control.PopupButtonToolTipText = "�������: " + subDocTypeUI.SubDocType.PluralTitle; // 13.06.2021
      control.ClearButtonToolTipText = "�������� ���� ������";

      _DocId = 0;
      _DocIdWasSet = false;
    }

    /// <summary>
    /// ������� ���������, ��������� � ��� ����������� ����������� ���������� ������ ���������.
    /// ����� ������� �������������, ������������� ����� ��� �������� DocIdEx
    /// </summary>
    /// <param name="docComboBoxProvider">��������� ���������� ������ ���������</param>
    /// <param name="control">����������� �������</param>
    /// <param name="subDocTypeName">��� ���� ������������� � <paramref name="docComboBoxProvider"/>.DocType.SubDocs.</param>
    public EFPMultiSubDocComboBox(EFPDocComboBox docComboBoxProvider, UserSelComboBox control, string subDocTypeName)
      : this(docComboBoxProvider.BaseProvider, control, GetSubDocTypeUI(docComboBoxProvider, subDocTypeName))
    {
      this.DocIdEx = docComboBoxProvider.DocIdEx;
    }

    private static SubDocTypeUI GetSubDocTypeUI(EFPDocComboBox docComboBoxProvider, string subDocTypeName)
    {
      if (String.IsNullOrEmpty(subDocTypeName))
        throw new ArgumentNullException("subDocTypeName");
      return docComboBoxProvider.DocTypeUI.SubDocTypes[subDocTypeName];
    }

    #endregion

    #region ��������������� ��������

    /// <summary>
    /// ������ ����������������� ���������� ��� ���� �������������.
    /// �� ����� ���� null.
    /// �������� SubDocTypeUI � SubDocTypeName ����������������.
    /// </summary>
    public SubDocTypeUI SubDocTypeUI
    {
      get { return _SubDocTypeUI; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (value == _SubDocTypeUI)
          return;
        _SubDocTypeUI = value;
        SubDocIds = DataTools.EmptyIds;
        InitTextAndImage();
      }
    }
    private SubDocTypeUI _SubDocTypeUI;

    /// <summary>
    /// ��� ������� �������������.
    /// �������� SubDocType, SubDocTypeUI � SubDocTypeName ����������������.
    /// </summary>
    public string SubDocTypeName
    {
      get
      {
        if (_SubDocTypeUI == null)
          return "";
        else
          return SubDocTypeUI.SubDocType.Name;
      }
    }

    /// <summary>
    /// �������� ���� ���������, � �������� ��������� ������������
    /// </summary>
    public DBxDocType DocType { get { return _SubDocTypeUI.DocType; } }

    /// <summary>
    /// �������� ���� �������������
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocTypeUI.SubDocType; } }

    #endregion

    #region �������� DocId

    /// <summary>
    /// ������������� ���������, �� �������� ���������� ������������.
    /// ���� �������� ����������� � ����� ����, �� ������� ������� "�����" ������������
    /// �������� � ��������� ������.
    /// ���� �������� �� ����������� � ����� ����, �� ����������� ��������� ������������� ��������
    /// �������� SubDocIds (��� �������, ��� ��� ������������ ��������� � ������ ���������), 
    /// ��� ���� ������������ �������� ����� ��������
    /// </summary>
    public Int32 DocId
    {
      get { return _DocId; }
      set
      {
        if (value != DocId || (!_DocIdWasSet))
        {
          _DocId = value;
          _DocIdWasSet = true;

          if (_OutDocIdEx != null)
            _OutDocIdEx.OwnerSetValue(value);
          InitSubDocIdsOnDocId();
        }
        else
        {
          // �������� DocId ����� �������� ��������������� �� ���������� EFPDocComboBox.DocId
          // ����� �������������� ��������� �������, ������������ �������������
          // ��������, �������� �����������, �� �������� ����� �������; 
          // ��������, ��������� ���� �����������
          if (SubDocIds.Length == 0)
            InitSubDocIdsOnDocId();
        }

        Validate();
      }
    }
    /// <summary>
    /// ������� �������� ��������
    /// </summary>
    private Int32 _DocId;

    /// <summary>
    /// true, ���� �������� ���� ����������� � ����� ���� �������, false, ���� �������� ����������� �� SubDocId
    /// </summary>
    private bool _DocIdWasSet;


    /// <summary>
    /// ���������� ����� ��������� �������� DocId.
    /// �� ������������ ��� ��������� �� ����������������� ����
    /// </summary>
    /// <param name="value">������������� ���������</param>
    protected void InternalSetDocId(Int32 value)
    {
      if (_DocIdWasSet)
      {
        if (_DocId != 0)
          return; // ��������� �������� �������� �� �����������
      }
      _DocId = value;
      _DocIdWasSet = false;

      if (_OutDocIdEx != null)
        _OutDocIdEx.OwnerSetValue(value);
    }

    private void InitSubDocIdsOnDocId()
    {
      if (_InsideSetSubDocIds)
        return;
      SubDocIds = DataTools.EmptyIds;
      if ((DocId != 0) && (SubDocTypeUI != null))
      {
        if (AutoSetAll)
        {
          SubDocIds = SubDocTypeUI.GetSubDocIds(DocId);
          return;
        }

        if (InitDefSubDocs != null)
          InitDefSubDocs(this, EventArgs.Empty);
      }
    }

    /// <summary>
    /// ����������� �������� ��� DocId
    /// </summary>
    public DepValue<Int32> DocIdEx
    {
      get
      {
        InitOutDocIdEx();
        return _OutDocIdEx;
      }
      set
      {
        InitInDocIdEx();
        _InDocIdEx.Source = value;
      }
    }
    private DepInput<Int32> _InDocIdEx;

    private void InitInDocIdEx()
    {
      if (_InDocIdEx == null)
      {
        _InDocIdEx = new DepInput<int>(0, InDocIdEx_ValueChanged);
        _InDocIdEx.OwnerInfo = new DepOwnerInfo(this, "InDocIdEx");
      }
    }


    void InDocIdEx_ValueChanged(object sender, EventArgs args)
    {
      DocId = _InDocIdEx.Value; // ������� ���������
    }

    private void InitOutDocIdEx()
    {
      if (_OutDocIdEx == null)
      {
        _OutDocIdEx = new DepOutput<Int32>(DocId);
        _OutDocIdEx.OwnerInfo = new DepOwnerInfo(this, "OutDocIdEx");
      }
    }
    private DepOutput<Int32> _OutDocIdEx;

    /// <summary>
    /// �������� ���������� ��������, ����������� ������� �������� ���������� 
    /// (�� ���� ��������, ������� ���� ��������� �������� DocIdEx)
    /// ��� null, ���� �������� ���������� ���
    /// 
    /// �������� ��������. ��������, ����� ����������� ���� ��������� ���� 
    /// ����������� ��������� ���� �����������
    /// </summary>
    public DepValue<Int32> DocIdExSource
    {
      get
      {
        if (_InDocIdEx == null)
          return null;
        else
          return _InDocIdEx.Source;
      }
    }

    /// <summary>
    /// ���������� ����� ��������� ���������� �������� ��������
    /// DocId (��� ���� SubDocIds ������������). ���������� �����
    /// ���������� �������� SubDocIds. 
    /// ������� �� ����������, ���� ����������� �������� AutoSetAll.
    /// </summary>
    public event EventHandler InitDefSubDocs;

    #endregion

    #region �������� SubDocIds

    /// <summary>
    /// ������� ��������� ������������.
    /// ���� ��� �� ������ ���������� ������������, ������������ ������ ������
    /// </summary>
    public Int32[] SubDocIds
    {
      // ����� ����������� ������������ ������� �������� Id, �.�. � ���� ��������� ��������� �������� IdEx
      get { return Ids; }
      set { Ids = value; }
    }


    /// <summary>
    /// ������ ��������� ��������������� �������������.
    /// ��� ��������� �������� ���������� InternalSetDocId(), ��� ��� ��������� �������� ��� ���� �����������
    /// </summary>
    protected internal override Int32[] Ids
    {
      get { return base.Ids; }
      set
      {
        if (_InsideSetSubDocIds)
          return;

        if (value == null)
          value = DataTools.EmptyIds;

        if (DataTools.AreArraysEqual<Int32>(value, base.Ids))
          return;

        _InsideSetSubDocIds = true;
        try
        {
          base.Ids = value;
          if (value.Length > 0)
            InternalSetDocId(SubDocTypeUI.TableCache.GetInt(value[0], "DocId"));
          else
            InternalSetDocId(0);
        }
        finally
        {
          _InsideSetSubDocIds = false;
        }

        Validate();
      }
    }

    private bool _InsideSetSubDocIds = false;

    /// <summary>
    /// �������������� ��������� �������������.
    /// ����������� �������� ��� SubDocIds
    /// </summary>
    public DepValue<Int32[]> SubDocIdsEx
    {
      get { return base.IdsEx; }
      set { base.IdsEx = value; }
    }

    #endregion

    #region �������� AutoSetAll

    /// <summary>
    /// ���� ���������� � true, �� ��� ��������� �������� DocId ����� �������� ������ ���� ������������� 
    /// (����� ���������).
    /// ������� InitDefSubSoc �� ����������.
    /// �������� �� ��������� - false
    /// </summary>
    public bool AutoSetAll
    {
      get { return _AutoSetAll; }
      set
      {
        if (value == _AutoSetAll)
          return;
        _AutoSetAll = value;

        if (_AutoSetAllEx != null)
          _AutoSetAllEx.Value = value;

        if (value && (DocId != 0) && (SubDocIds.Length == 0) &&
          (SubDocTypeUI != null))
        {
          SubDocIds = SubDocTypeUI.GetSubDocIds(DocId);
        }

        Validate(); // 29.08.2016
      }
    }
    private bool _AutoSetAll;

    /// <summary>
    /// ���� ���������� � true, �� ��� ��������� �������� DocId ����� �������� ������ ���� ������������� 
    /// (����� ���������).
    /// ������� InitDefSubSoc �� ����������.
    /// ����������� �������� ��� AutoSetAll.
    /// </summary>
    public DepValue<Boolean> AutoSetAllEx
    {
      get
      {
        InitAutoSetAllEx();
        return _AutoSetAllEx;
      }
      set
      {
        InitAutoSetAllEx();
        _AutoSetAllEx.Source = value;
      }
    }

    private void InitAutoSetAllEx()
    {
      if (_AutoSetAllEx == null)
      {
        _AutoSetAllEx = new DepInput<bool>(AutoSetAll,AutoSetAllEx_ValueChanged);
        _AutoSetAllEx.OwnerInfo = new DepOwnerInfo(this, "AutoSetAllEx");
      }
    }
    private DepInput<Boolean> _AutoSetAllEx;

    void AutoSetAllEx_ValueChanged(object sender, EventArgs args)
    {
      AutoSetAll = _AutoSetAllEx.Value;
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ������������ ��������� �������������.
    /// ����� �������� �������� ������, ������������, ��� ��� ������������ � ������ ���������
    /// � ��������� � ��������������� DocId.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == UIValidateState.Error)
        return;

      // ���������, ��� ����������� ��������� � ���������� ���������
      Int32 DummyDocId = 0;
      for (int i = 0; i < SubDocIds.Length; i++)
      {
        Int32 DocId2 = SubDocTypeUI.TableCache.GetInt(SubDocIds[i], "DocId");
        if (DocId != 0)
        {
          if (DocId2 != DocId)
          {
            SetError("��������� ����������� \"" + SubDocTypeUI.SubDocType.SingularTitle + "\" " +
              SubDocTypeUI.GetTextValue(SubDocIds[i]) + " ��������� � ��������� \"" +
              SubDocTypeUI.DocTypeUI.GetTextValue(DocId2) + "\", � �� \"" + SubDocTypeUI.DocTypeUI.GetTextValue(DocId) + "\"");
            return;
          }
        }
        else
        {
          if (i == 0)
            DummyDocId = DocId2;
          else
          {
            if (DocId2 != DummyDocId)
            {
              SetError("��������� ������������ ��������� � ������ ����������");
              return;
            }
          }
        }
      }
    }

    #endregion

    #region ����� � ������

    /// <summary>
    /// ��������� ������������� ��� ��������� �������������.
    /// ���� ������� ������������� �� �����, ��� ���������� ��������� MaxTextItemCount,
    /// �� ������������ ��������� �������������, ����������� ��������.
    /// ���� ������� ������ �������������, �� ������������ ������ ���������� �������������.
    /// ����� �� ����������, ���� ��� ��������� �������������.
    /// </summary>
    /// <returns>������ ��� ����������</returns>
    protected override string DoGetText()
    {
      if (SubDocIds.Length == 1)
        return SubDocTypeUI.GetTextValue(SubDocIds[0]);
      else if (SubDocIds.Length <= MaxTextItemCount)
      {
        string[] a = new string[SubDocIds.Length];
        for (int i = 0; i < SubDocIds.Length; i++)
          a[i] = SubDocTypeUI.GetTextValue(SubDocIds[i]);
        return String.Join(", ", a);
      }
      else
        return SubDocType.PluralTitle + " (" + SubDocIds.Length.ToString() + ")";
    }

    /// <summary>
    /// ���������� ��� ����������� �� ������ EFPApp.MainImages.
    /// ���� ������ ���� ����������� ��� ��� ��������� ������������� ����� ���������� ������,
    /// �� �� ������������.
    /// ����� ������������ "DBxDocSelection".
    /// ����� �� ����������, ���� ��� ��������� �������������.
    /// </summary>
    /// <returns>������ ��� ����������</returns>
    protected override string DoGetImageKey()
    {
      if (SubDocIds.Length < 1)
        return "UnknownState"; // ������
      string ImageKey = SubDocTypeUI.GetImageKey(SubDocIds[0]);
      for (int i = 1; i < SubDocIds.Length; i++)
      {
        string ImageKey2 = SubDocTypeUI.GetImageKey(SubDocIds[i]);
        if (ImageKey2 != ImageKey)
          return "DBxDocSelection";
      }
      return ImageKey;
    }

    /// <summary>
    /// �������� �������� ���������� ��� ����������.
    /// ���� ������� ������ ������ ������������, �� ������������ ����������� ����������
    /// </summary>
    /// <param name="colorType">���� ������������ ����, ������������ ��� ���������</param>
    /// <param name="grayed">�������� �������� True, ���� ����������� ���������� ����� ������</param>
    protected override void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      if (SubDocIds.Length == 1)
        SubDocTypeUI.GetRowColor(SubDocIds[0], out colorType, out grayed);
      else
        base.DoGetValueColor(out colorType, out grayed);
    }

    /// <summary>
    /// ���������� ����� ����������� ��������� ��� ���������� ������������.
    /// ���� ������� ��������� �������������, ���������� "������� �������������: X".
    /// </summary>
    /// <returns>����� ����������� ���������</returns>
    protected override string DoGetValueToolTipText()
    {
      if (SubDocIds.Length == 1)
        return SubDocTypeUI.GetToolTipText(SubDocIds[0]);
      else
        return "������� �������������: " + SubDocIds.Length.ToString();
    }

    #endregion

    #region ���������� ������

    /// <summary>
    /// ���������� ������ ������ ������ ��� ���������� ������������� ��� ��������� ��������� 
    /// c ������� SubDocTypeUI.SelectSubDocs().
    /// ����� ��������������� �������� SubDocIds.
    /// </summary>
    protected override void DoPopup()
    {
      if (DocId == 0)
      {
        EFPApp.ShowTempMessage("�� ����� �������� \"" + DocType.SingularTitle + "\", �� �������� ����� ��������");
        return;
      }

      Int32[] ThisSubDocIds = SubDocIds;

      DBxDocSet DocSet = new DBxDocSet(UI.DocProvider);
      DBxSingleDoc Doc = DocSet[DocType.Name].View(DocId);

      SubDocSelectDialog dlg = new SubDocSelectDialog(SubDocTypeUI, Doc.SubDocs[SubDocTypeName].SubDocs);
      dlg.SelectionMode = DocSelectionMode.MultiSelect;
      if (!String.IsNullOrEmpty(DisplayName))
        dlg.Title = DisplayName;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.SubDocIds = SubDocIds;
      dlg.DialogPosition.PopupOwnerControl = Control;

      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      if (DataTools.AreArraysEqual<Int32>(dlg.SubDocIds, SubDocIds))
        InitTextAndImage();
      else
        SubDocIds = dlg.SubDocIds;
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ���������� DBxSubDocType.PluralTitle ������ "��� ��������"
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return SubDocTypeUI.SubDocType.PluralTitle; }
    }

    /// <summary>
    /// ���������� true, ���� ����������� � �������� ��������������� ��� ��������,
    /// � �������� �� ���������, �������� �� ��������
    /// </summary>
    /// <param name="subDocId">������������� ������������</param>
    /// <param name="message">���� ������������ ���������, ���� ����������� ������</param>
    /// <returns>True, ���� ����������� ������</returns>
    protected override bool GetDeletedValue(Int32 subDocId, out string message)
    {
      if (!UI.DocProvider.DocTypes.UseDeleted)
      {
        message = null;
        return false;
      }

      object[] a = SubDocTypeUI.GetValues(subDocId, "Deleted,DocId");
      if (DataTools.GetBool(a[0]))
      {
        message = "��������� ����������� \"" + SubDocType.SingularTitle + "\" ������";
        return true; // ������ �����������
      }
      Int32 DocId = DataTools.GetInt(a[1]);
      if (DataTools.GetBool(UI.TextHandlers.DBCache[DocType.Name].GetBool(DocId, "Deleted")))
      {
        string DocText;
        try
        {
          DocText = UI.DocTypes[DocType.Name].GetTextValue(DocId) + " (DocId=" + DocId.ToString() + ")";
        }
        catch (Exception e)
        {
          DocText = "Id=" + DocId.ToString() + ". ������ ��������� ������: " + e.Message;
        }
        message = "�������� \"" + DocType.SingularTitle + "\" (" + DocText + "), � �������� ��������� ��������� �����������, ������";
        return true;
      }
      else
      {
        message = null;
        return false;
      }
    }

    #endregion

    #region ������� ����������

    /// <summary>
    /// ���������� �������� �������� SubDocTypeUI.HasGetDocSel, ��� ��� �� � ���� �����
    /// ������������� ���� ��������� ���� �� ���������, � ���� ������������ �� �������� �������.
    /// 
    /// ������� ������� �� ������ ������ �� �������������
    /// </summary>
    public override bool GetDocSelSupported { get { return SubDocTypeUI.HasGetDocSel; } }

    /// <summary>
    /// �������� SubDocTypeUI.PerformGetDocSel() ��� ���� ��������� �������������
    /// </summary>
    /// <param name="reason">������� �������� �������</param>
    /// <returns>������� ����������</returns>
    protected override DBxDocSelection OnGetDocSel(EFPDBxGridViewDocSelReason reason)
    {
      DBxDocSelection DocSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      for (int i = 0; i < SubDocIds.Length; i++)
      {
        SubDocTypeUI.PerformGetDocSel(DocSel, SubDocIds[i], reason);
      }

      DocSel.Add(DocType.Name, DocId);
      return DocSel;
    }

    #endregion
  }
}
