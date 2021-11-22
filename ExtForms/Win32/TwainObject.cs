// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Взято из http://www.codeproject.com/Articles/1376/NET-TWAIN-image-scanner
// Автор: NETMaster
// licensed under A Public Domain dedication
// http://creativecommons.org/licenses/publicdomain/

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using FreeLibSet.Core;

/*
 * Класс Twain сделан наследником DisposableObject и объявлен internal
 * 
 * Для реальной работы используется класс EFPTwainHandler
 */

#pragma warning disable 1591

namespace FreeLibSet.Win32.Twain
{
  #region Перечисление TwainCommand

  /// <summary>
  /// Результат вызова PassMessage().
  /// Эти константы не имеют отношения к TWAIN
  /// </summary>
  public enum TwainCommand
  {
    Not = -1,
    Null = 0,
    TransferReady = 1,
    CloseRequest = 2,
    CloseOk = 3,
    DeviceEvent = 4
  }

  #endregion

  public class TwainException : Exception
  {
    #region Конструктор

    public TwainException(string message, TwRC rc)
      : base("TWAIN: " + message + " (" + GetRCText(rc) + ")")
    {
      _RC = rc;
    }

    private static string GetRCText(TwRC rc)
    {
      switch (rc)
      {
        case TwRC.Failure: return "Ошибка";
        case TwRC.CheckStatus: return "Операция выполнена частично";
        case TwRC.Cancel: return "Операция отменена пользователем";
        case TwRC.XferDone: return "Все данные переданы";
        case TwRC.EndOfList: return "Больше источников не найдено";
        case TwRC.InfoNotSupported: return "Получение информации недоступно";
        case TwRC.DataNotAvailable: return "Данные недоступны";
        //case TwRC.DSEvent:
        //case TwRC.NotDSEvent:
        default:
          return "Код ошибки " + ((int)rc).ToString();
      }
    }

    #endregion

    #region Свойства

    public TwRC RC { get { return _RC; } }
    private TwRC _RC;

    #endregion
  }

  internal class TwainObject : DisposableObject
  {
    #region Конструктор и Dispose

    private const short CountryUSA = 1;
    private const short LanguageUSA = 13;

    public TwainObject()
    {
      appid = new TwIdentity();
      appid.Id = IntPtr.Zero;

      #region Версия

      Assembly asm = EnvironmentTools.EntryAssembly;
      if (asm != null)
      {
        string ProgName = asm.FullName;
        AssemblyName an = new AssemblyName(ProgName);

        AssemblyDescriptionAttribute attrDescr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));
        if (attrDescr != null)
          appid.Version.Info = attrDescr.Description;
        else
          appid.Version.Info = an.Name;

        appid.Version.MajorNum = (short)(an.Version.Major);
        appid.Version.MinorNum = (short)(an.Version.Minor);
      }
      else
        appid.Version.Info = EnvironmentTools.ApplicationName; // 03.04.2015

      appid.Version.Language = LanguageUSA; // !!!
      appid.Version.Country = CountryUSA; // !!!

      #endregion

      appid.ProtocolMajor = TwProtocol.Major;
      appid.ProtocolMinor = TwProtocol.Minor;

      appid.SupportedGroups = (int)(TwDG.Image | TwDG.Control);
      AssemblyCompanyAttribute attrCompany = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute));
      if (attrCompany != null)
        appid.Manufacturer = attrCompany.Company;
      else
        appid.Manufacturer = "";

      AssemblyProductAttribute attrProduct = (AssemblyProductAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyProductAttribute));
      if (attrProduct != null)
        appid.ProductName = attrProduct.Product;
      else
        appid.ProductName = "";

      srcds = new TwIdentity();
      srcds.Id = IntPtr.Zero;

      evtmsg.EventPtr = Marshal.AllocHGlobal(Marshal.SizeOf(winmsg));
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (evtmsg.EventPtr != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(evtmsg.EventPtr);
          evtmsg.EventPtr = IntPtr.Zero;
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Основные методы

    private void CheckResult(TwRC rc, string message)
    {
      if (rc == TwRC.Success)
        return;

      if (rc == TwRC.Cancel)
        throw new UserCancelException();

      throw new TwainException(message, rc);
    }

    public void Init(IntPtr hwndp)
    {
      Finish();
      if (hwndp == IntPtr.Zero)
        return;

      TwRC rc = DSMparent(appid, IntPtr.Zero, TwDG.Control, TwDAT.Parent, TwMSG.OpenDSM, ref hwndp);
      CheckResult(rc, "Не удалось инициализировать TWAIN");

      rc = DSMident(appid, IntPtr.Zero, TwDG.Control, TwDAT.Identity, TwMSG.GetDefault, srcds);
      if (rc != TwRC.Success)
      {
        DSMparent(appid, IntPtr.Zero, TwDG.Control, TwDAT.Parent, TwMSG.CloseDSM, ref hwndp);
        CheckResult(rc, "Сканер не найден");
      }
      hwnd = hwndp;
    }

    public void Select()
    {
      TwRC rc;
      CloseSrc();
      if (appid.Id == IntPtr.Zero)
      {
        Init(hwnd);
        if (appid.Id == IntPtr.Zero)
          return;
      }
      rc = DSMident(appid, IntPtr.Zero, TwDG.Control, TwDAT.Identity, TwMSG.UserSelect, srcds);
    }


    public void Acquire()
    {
      TwRC rc;
      CloseSrc();
      if (appid.Id == IntPtr.Zero)
      {
        Init(hwnd);
        if (appid.Id == IntPtr.Zero)
          return;
      }
      rc = DSMident(appid, IntPtr.Zero, TwDG.Control, TwDAT.Identity, TwMSG.OpenDS, srcds);
      CheckResult(rc, "Не удалось открыть источник");

      TwCapability cap = new TwCapability(TwCap.XferCount, 1);
      rc = DScap(appid, srcds, TwDG.Control, TwDAT.Capability, TwMSG.Set, cap);

      if (rc != TwRC.Success)
      {
        CloseSrc();
        CheckResult(rc, "Не удалось получить возможности устройства TWAIN");
      }

      TwUserInterface guif = new TwUserInterface();
      guif.ShowUI = 1;
      guif.ModalUI = 1;
      guif.ParentHand = hwnd;
      rc = DSuserif(appid, srcds, TwDG.Control, TwDAT.UserInterface, TwMSG.EnableDS, guif);
      if (rc != TwRC.Success)
      {
        CloseSrc();
        CheckResult(rc, "Не удалось запустить пользовательский интерфейс TWAIN");
      }
    }


    public List<IntPtr> TransferPictures()
    {
      List<IntPtr> pics = new List<IntPtr>();
      if (srcds.Id == IntPtr.Zero)
        return pics;

      TwRC rc;
      IntPtr hbitmap = IntPtr.Zero;
      TwPendingXfers pxfr = new TwPendingXfers();

      do
      {
        pxfr.Count = 0;
        hbitmap = IntPtr.Zero;

        // Запрашиваем информацию об очередном изображении
        TwImageInfo iinf = new TwImageInfo();
        rc = DSiinf(appid, srcds, TwDG.Image, TwDAT.ImageInfo, TwMSG.Get, iinf);
        if (rc != TwRC.Success)
        {
          CloseSrc();
          return pics;
        }

        rc = DSixfer(appid, srcds, TwDG.Image, TwDAT.ImageNativeXfer, TwMSG.Get, ref hbitmap);
        if (rc != TwRC.XferDone)
        {
          CloseSrc();
          return pics;
        }

        // в pfxr изменится поле Count, по которому мы выходим из цикла
        rc = DSpxfer(appid, srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.EndXfer, pxfr);
        if (rc != TwRC.Success)
        {
          CloseSrc();
          return pics;
        }

        pics.Add(hbitmap);
      }
      while (pxfr.Count != 0);

      rc = DSpxfer(appid, srcds, TwDG.Control, TwDAT.PendingXfers, TwMSG.Reset, pxfr);
      return pics;
    }


    public TwainCommand PassMessage(ref Message m)
    {
      if (srcds.Id == IntPtr.Zero)
        return TwainCommand.Not;

      int pos = GetMessagePos();

      winmsg.hwnd = m.HWnd;
      winmsg.message = m.Msg;
      winmsg.wParam = m.WParam;
      winmsg.lParam = m.LParam;
      winmsg.time = GetMessageTime();
      winmsg.x = (short)pos;
      winmsg.y = (short)(pos >> 16);

      Marshal.StructureToPtr(winmsg, evtmsg.EventPtr, false);
      evtmsg.Message = 0;
      TwRC rc = DSevent(appid, srcds, TwDG.Control, TwDAT.Event, TwMSG.ProcessEvent, ref evtmsg);
      if (rc == TwRC.NotDSEvent)
        return TwainCommand.Not;
      if (evtmsg.Message == (short)TwMSG.XFerReady)
        return TwainCommand.TransferReady;
      if (evtmsg.Message == (short)TwMSG.CloseDSReq)
        return TwainCommand.CloseRequest;
      if (evtmsg.Message == (short)TwMSG.CloseDSOK)
        return TwainCommand.CloseOk;
      if (evtmsg.Message == (short)TwMSG.DeviceEvent)
        return TwainCommand.DeviceEvent;

      return TwainCommand.Null;
    }

    public void CloseSrc()
    {
      TwRC rc;
      if (srcds.Id != IntPtr.Zero)
      {
        TwUserInterface guif = new TwUserInterface();
        rc = DSuserif(appid, srcds, TwDG.Control, TwDAT.UserInterface, TwMSG.DisableDS, guif);
        rc = DSMident(appid, IntPtr.Zero, TwDG.Control, TwDAT.Identity, TwMSG.CloseDS, srcds);
      }
    }

    public void Finish()
    {
      TwRC rc;
      CloseSrc();
      if (appid.Id != IntPtr.Zero)
        rc = DSMparent(appid, IntPtr.Zero, TwDG.Control, TwDAT.Parent, TwMSG.CloseDSM, ref hwnd);
      appid.Id = IntPtr.Zero;
    }

    #endregion

    #region Поля

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct WINMSG
    {
      public IntPtr hwnd;
      public int message;
      public IntPtr wParam;
      public IntPtr lParam;
      public int time;
      public int x;
      public int y;
    }

    private IntPtr hwnd;
    private TwIdentity appid;
    private TwIdentity srcds;
    private TwEvent evtmsg;
    private WINMSG winmsg;

    #endregion

    #region DllImport

    // ------ DSM entry point DAT_ variants:
    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSMparent([In, Out] TwIdentity origin, IntPtr zeroptr, TwDG dg, TwDAT dat, TwMSG msg, ref IntPtr refptr);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSMident([In, Out] TwIdentity origin, IntPtr zeroptr, TwDG dg, TwDAT dat, TwMSG msg, [In, Out] TwIdentity idds);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSMstatus([In, Out] TwIdentity origin, IntPtr zeroptr, TwDG dg, TwDAT dat, TwMSG msg, [In, Out] TwStatus dsmstat);


    // ------ DSM entry point DAT_ variants to DS:
    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSuserif([In, Out] TwIdentity origin, [In, Out] TwIdentity dest, TwDG dg, TwDAT dat, TwMSG msg, TwUserInterface guif);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSevent([In, Out] TwIdentity origin, [In, Out] TwIdentity dest, TwDG dg, TwDAT dat, TwMSG msg, ref TwEvent evt);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSstatus([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwDAT dat, TwMSG msg, [In, Out] TwStatus dsmstat);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DScap([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwDAT dat, TwMSG msg, [In, Out] TwCapability capa);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSiinf([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwDAT dat, TwMSG msg, [In, Out] TwImageInfo imginf);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSixfer([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwDAT dat, TwMSG msg, ref IntPtr hbitmap);

    [DllImport("twain_32.dll", EntryPoint = "#1")]
    private static extern TwRC DSpxfer([In, Out] TwIdentity origin, [In] TwIdentity dest, TwDG dg, TwDAT dat, TwMSG msg, [In, Out] TwPendingXfers pxfr);


    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern IntPtr GlobalAlloc(int flags, int size);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern IntPtr GlobalLock(IntPtr handle);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern bool GlobalUnlock(IntPtr handle);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    internal static extern IntPtr GlobalFree(IntPtr handle);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int GetMessagePos();
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int GetMessageTime();


    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CreateDC(string szdriver, string szdevice, string szoutput, IntPtr devmode);

    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern bool DeleteDC(IntPtr hdc);

    #endregion

#if XXX

    public static int ScreenBitDepth
    {
      get
      {
        IntPtr screenDC = CreateDC("DISPLAY", null, null, IntPtr.Zero);
        int bitDepth = GetDeviceCaps(screenDC, 12);
        bitDepth *= GetDeviceCaps(screenDC, 14);
        DeleteDC(screenDC);
        return bitDepth;
      }
    }

#endif

  } // class Twain
}
