using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Lifetime;

namespace AgeyevAV
{
  /// <summary>
  /// ���������� �������� ������ �������, ������������� �� ������, � �������� ����� ������������
  /// ������� ������� ��������.
  /// ������������ � ������ "������-������" ��� ��������, ����������� ��������, ��� ������� ������
  /// ������ �������������� ����� �����
  /// </summary>
  public class MarshalByRefSponsoredObject : MarshalByRefObject
  {
    #region ���������� �������� �����

    /// <summary>
    /// ���� �������� �����������, �� ������ ��� ���������� ���������� ����� ���� �����.
    /// �������� ������� ������������ ��� "���������" ����������, ���������������� ���� �������������,
    /// ������� ���������� ��� ����� ������ �������
    /// </summary>
    public bool EternalLife { get { return _EternalLife; } set { _EternalLife = value; } }
    private bool _EternalLife;

    /// <summary>
    /// ���� �������� �����������, �� ��� ��������� ������� �������� ���������� ������� � InitializeLifetimeService(),
    /// � �������� �������������� ��������� �������.
    /// �������� ������ ��������������� ����� ����� �������� �������, �� ����, ��� �� �������� �� ������
    /// Net Remoting.
    /// �������� �� ����� ������, ���� EternalLife=true
    /// </summary>
    /// <remarks>
    /// �������������� ������� �� ����� ����������� ����������, ������� ������, ��� ���������� ISponsor
    /// ������ �������� �������� ������, ���� ����������� ������� � ������� ��� ����.
    /// ��� ������� ������������� ������� �� ������ �������� �� ������ ����������� �������. ������� ������
    /// � ������ ������� �������, ������������ � �����������. ����� ������ ��������� ������� (���������� ��������
    /// ��� �� ����-����), � ������� �������� ��������� ���������� ������. ����� ����� ������� �� ������
    /// �������� ��������
    /// </remarks>
    public System.Runtime.Remoting.Lifetime.ISponsor ExternalSponsor { get { return _ExternalSponsor; } set { _ExternalSponsor = value; } }
    private System.Runtime.Remoting.Lifetime.ISponsor _ExternalSponsor;

    /// <summary>
    /// �������������� ������ ILease, ����������� � ��� �������� ExternalSponsor.
    /// ���� ����������� �������� EternalLife, ������������ null
    /// </summary>
    /// <returns>������, ����������� ILease ��� null</returns>
    public override object InitializeLifetimeService()
    {
      if (EternalLife)
        return null;

      ILease lease = (ILease)base.InitializeLifetimeService();
      if (lease.CurrentState == LeaseState.Initial)
      {
        if (ExternalSponsor != null)
          lease.Register(ExternalSponsor);

        //        ExtLease lease2 = new ExtLease(this, lease1);

        //#if DEBUG
        //        System.Threading.Interlocked.Increment(ref _DebugExtLeaseCreateCount);
        //#endif

        //        return lease2;
      }
      //    else

      return lease;
    }


    // ��� �� ��������
    // � InitializeLifetimeService() ����� ������� ���� ������, ����������� ILease � ������� ���.
    // �� Net Framework � ���� ������ ������� ����� �������� ������ Lease � ��������� � ���� ��������.
    // � ��� ������ ���������� � �������.

#if XXX
#if DEBUG

    /// <summary>
    /// ���������� ��������� �������� �������� ��� �������.
    /// �� ������������ � ���������� ����
    /// </summary>
    public int DebugExtLeaseCreateCount { get { return _DebugExtLeaseCreateCount; } }
    private int _DebugExtLeaseCreateCount;

#endif

    /// <summary>
    /// ���������� ��� �������� ���������� ILease (������������ ���������� ������� Net Framework).
    /// ��� "��������" ������ Renew() ������������� ���������� ����������� ����� � �������� ������
    /// </summary>
    private class ExtLease : MarshalByRefObject, ILease
    {
    #region �����������

      public ExtLease(MarshalByRefSponsoredObject owner, ILease mainLease)
      {
        _Owner = owner;
        _MainLease = mainLease;
      }

    #endregion

    #region ����

      private MarshalByRefSponsoredObject _Owner;
      private ILease _MainLease;

    #endregion

    #region ILease Members

      public TimeSpan CurrentLeaseTime { get { return _MainLease.CurrentLeaseTime; } }

      public LeaseState CurrentState { get { return _MainLease.CurrentState; } }

      public TimeSpan InitialLeaseTime
      {
        get { return _MainLease.InitialLeaseTime; }
        set { _MainLease.InitialLeaseTime = value; }
      }

      public TimeSpan RenewOnCallTime
      {
        get { return _MainLease.RenewOnCallTime; }
        set { _MainLease.RenewOnCallTime = value; }
      }

      public TimeSpan SponsorshipTimeout
      {
        get { return _MainLease.SponsorshipTimeout; }
        set { _MainLease.SponsorshipTimeout = value; }
      }

      public void Register(ISponsor obj)
      {
        _MainLease.Register(obj);
      }

      public void Register(ISponsor obj, TimeSpan renewalTime)
      {
        _MainLease.Register(obj, renewalTime);
      }

      public void Unregister(ISponsor obj)
      {
        _MainLease.Unregister(obj);
      }

      public TimeSpan Renew(TimeSpan renewalTime)
      {
        TimeSpan expirationTime = _MainLease.Renew(renewalTime);
        _Owner.OnRenew(ref expirationTime);
        return expirationTime;
      }

    #endregion
    }

    /// <summary>
    /// ���� ����� ���������� ����� ������ ILease.Renew().
    /// ������ ���������� ����� ���� ������.
    /// ����������� ����� ����� �������� ��� �������� ��� ���-�� �������������, ���� �������� ����� TimeSpan.Zero.
    /// ����� �� ����������, ���� �������� EternalLife=true.
    /// </summary>
    /// <param name="expirationTime">����� ���� ������</param>
    protected virtual void OnRenew(ref TimeSpan expirationTime)
    {
    }
#endif

    #endregion
  }
}
