// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Lifetime;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Расширение базового класса объекта, передаваемого по ссылке, к которому можно присоединить
  /// внешний спонсор лицензии.
  /// Используется в модели "клиент-сервер" для объектов, создаваемых сервером, для которых сервер
  /// должен контролировать время жизни
  /// </summary>
  public class MarshalByRefSponsoredObject : MarshalByRefObject
  {
    #region Управление временем жизни

    /// <summary>
    /// Если свойство установлено, то объект для удаленного интерфейса будет жить вечно.
    /// Свойство следует использовать для "гостевого" провайдера, предоставляемого всем пользователям,
    /// который существует все время работы сервера
    /// </summary>
    public bool EternalLife { get { return _EternalLife; } set { _EternalLife = value; } }
    private bool _EternalLife;

    /// <summary>
    /// Если свойство установлено, то при созданиии объекта лицензии удаленного доступа в InitializeLifetimeService(),
    /// к лицензии присоединяется указанный спонсор.
    /// Свойство должно устанавливаться сразу после создания объекта, до того, как он окажется во власти
    /// Net Remoting.
    /// Свойство не имеет смысла, если EternalLife=true
    /// </summary>
    /// <remarks>
    /// Присоединенный спонсор не имеет возможности отключения, поэтому сервер, при реализации ISponsor
    /// должен продлять лицензию только, если подключение клиента к серверу еще живо.
    /// Для сервера рекомендуется создать по одному спонсору на каждое подключение клиента. Спонсор связан
    /// с некими данными сервера, относящегося к подключению. Когда сервер отключает клиента (нормальным способом
    /// или по тайм-ауту), в объекте спонсора очищается внутренняя ссылка. После этого спонсор не должен
    /// продлять лицензию
    /// </remarks>
    public System.Runtime.Remoting.Lifetime.ISponsor ExternalSponsor { get { return _ExternalSponsor; } set { _ExternalSponsor = value; } }
    private System.Runtime.Remoting.Lifetime.ISponsor _ExternalSponsor;

    /// <summary>
    /// Инициализирует объект ILease, регистрируя в нем спонсора ExternalSponsor.
    /// Если установлено свойство EternalLife, возвращается null
    /// </summary>
    /// <returns>Объект, реализующий ILease или null</returns>
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


    // Так не работает
    // В InitializeLifetimeService() можно создать свой объект, реализующий ILease и вернуть его.
    // Но Net Framework в этом случае создаст новый закрытый объект Lease и скопирует в него свойства.
    // А наш объект отправится в мусорку.

#if XXX
#if DEBUG

    /// <summary>
    /// Количество созданных объектов лицензий для отладки.
    /// Не использовать в прикладном коде
    /// </summary>
    public int DebugExtLeaseCreateCount { get { return _DebugExtLeaseCreateCount; } }
    private int _DebugExtLeaseCreateCount;

#endif

    /// <summary>
    /// Переходник для внешнего интерфейса ILease (реализуемого внутренним классом Net Framework).
    /// Для "главного" метода Renew() дополнительно вызывается виртуальный метод в основном классе
    /// </summary>
    private class ExtLease : MarshalByRefObject, ILease
    {
    #region Конструктор

      public ExtLease(MarshalByRefSponsoredObject owner, ILease mainLease)
      {
        _Owner = owner;
        _MainLease = mainLease;
      }

    #endregion

    #region Поля

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
    /// Этот метод вызывается после вызова ILease.Renew().
    /// Методу передается новый срок аренды.
    /// Производный класс может изменить это значение или как-то отреагировать, если значение равно TimeSpan.Zero.
    /// Метод не вызывается, если свойство EternalLife=true.
    /// </summary>
    /// <param name="expirationTime">Новый срок аренды</param>
    protected virtual void OnRenew(ref TimeSpan expirationTime)
    {
    }
#endif

    #endregion
  }
}
