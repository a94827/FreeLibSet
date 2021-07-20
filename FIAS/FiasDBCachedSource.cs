using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Remoting;
using System.Diagnostics;

namespace AgeyevAV.FIAS
{

  /// <summary>
  /// Значения параметра "Action" при вызове IFiasSource.StartDistributedCall()
  /// </summary>
  internal enum FiasExecProcAction
  {
    GetGuidInfo,
    GetRecIdInfo,
    GetAddrObPages,
    GetSpecialAddrObPage,
    GetHousePages,
    GetRoomPages,
    GetAOTypes,
    FindAddresses,
    GetTableForGuid,
  }

  /// <summary>
  /// Реализация свойства FiasDB.Source
  /// </summary>
  internal sealed class FiasDBCachedSource : FiasCachedSource
  {
    #region Конструктор

    internal FiasDBCachedSource(FiasSourceProxy proxy, FiasDB fiasDB)
      : base(proxy)
    {
      _FiasDB = fiasDB;
    }

    private FiasDB _FiasDB;

    #endregion

    #region "Разделенное" выполнение

    /// <summary>
    /// Процедура, выполняемая асинхронно
    /// </summary>
    private class FiasExecProc : ExecProc
    {
      #region Конструктор и Dispose()

      internal FiasExecProc(FiasDBCachedSource owner, object userData)
      {
        _Owner = owner;
        _EventArgs = new FiasExecProcEventArgs(this, userData);

        _Owner._FiasDB.OnExecProcCreated(_EventArgs);
      }

      protected override void Dispose(bool disposing)
      {
        _Owner._FiasDB.OnExecProcDisposed(_EventArgs);
        base.Dispose(disposing);
      }

      #endregion

      #region Свойства

      private FiasDBCachedSource _Owner;

      /// <summary>
      /// Чтобы не создавать каждый раз новый объект
      /// </summary>
      private FiasExecProcEventArgs _EventArgs;

      #endregion

      #region Выполнение

      protected override void OnBeforeExecute(NamedValues args)
      {
        base.OnBeforeExecute(args);
        _Owner._FiasDB.OnExecProcBeforeExecute(_EventArgs);
      }

      protected override void OnAfterExecute(NamedValues args, NamedValues results, Exception exception)
      {
        _Owner._FiasDB.OnExecProcAfterExecute(_EventArgs);
        base.OnAfterExecute(args, results, exception);
      }

      protected override NamedValues OnExecute(NamedValues args)
      {
        NamedValues res = new NamedValues();
        FiasExecProcAction action = (FiasExecProcAction)(args["Action"]);

        //BeginSplash("!!!Action=" + action.ToString());
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasExecProc.OnExecute() started. Action="+action.ToString());

        switch (action)
        {
          case FiasExecProcAction.GetGuidInfo:
            res["Res"] = _Owner.GetGuidInfo((Guid[])(args["Guids"]), (FiasTableType)(args["TableType"]));
            break;

          case FiasExecProcAction.GetRecIdInfo:
            res["Res"] = _Owner.GetRecIdInfo((Guid[])(args["RecIds"]), (FiasTableType)(args["TableType"]));
            break;

          case FiasExecProcAction.GetAddrObPages:
            res["Res"] = _Owner.GetAddrObPages((FiasLevel)(args["Level"]), (Guid[])(args["PageAOGuids"]));
            break;

          case FiasExecProcAction.GetSpecialAddrObPage:
            res["Res"] = _Owner.GetSpecialAddrObPage((FiasSpecialPageType)(args["PageType"]), (Guid)(args["PageAOGuid"]));
            break;

          case FiasExecProcAction.GetHousePages:
            res["Res"] = _Owner.GetHousePages((Guid[])(args["PageAOGuids"]));
            break;

          case FiasExecProcAction.GetRoomPages:
            res["Res"] = _Owner.GetRoomPages((Guid[])(args["PageHouseGuids"]));
            break;

          case FiasExecProcAction.GetAOTypes:
            res["Res"] = _Owner.GetAOTypes();
            break;

          case FiasExecProcAction.FindAddresses:
            res["Res"] = _Owner.FindAddresses((FiasAddressSearchParams)(args["SearchParams"]));
            break;

          case FiasExecProcAction.GetTableForGuid:
            res["Res"] = _Owner.GetTableForGuid((Guid)(args["Guid"]), (FiasTableType)(args["TableType"]));
            break;

          default:
            throw new BugException("Неизвестное действие: " + action.ToString());
        }

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasExecProc.OnExecute() finished.");

        return res;
      }

      #endregion
    }

    public override DistributedCallData StartDistributedCall(NamedValues args, object userData)
    {
      FiasExecProc proc = new FiasExecProc(this, userData);
      proc.SetContext(NamedValues.Empty);
      return proc.StartDistributedCall(args);
    }

    #endregion
  }
}
