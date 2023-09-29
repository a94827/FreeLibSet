// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Информация о принтере.
  /// Обеспечивает альтернативный доступ к информации объекта PrinterSettings,
  /// выполняя ее буферизацию. Оригинальный объект PrinterSettings доступен
  /// через свойство PrinterSettings. Методы оригинального класса очень уж медленно работают.
  /// </summary>
  public class EFPPrinterInfo
  {
    #region Конструктор

    internal EFPPrinterInfo(string printerName)
    {
      EFPApp.BeginWait("Получение свойств принтера \"" + printerName + "\"", "Print");
      try
      {
        _PrinterName = printerName;
        _PrinterSettings = new PrinterSettings();
        if (!String.IsNullOrEmpty(printerName))
          PrinterSettings.PrinterName = printerName;
        _IsValid = PrinterSettings.IsValid;

        // Извлекаем характеристики один раз
        if (_IsValid)
        {
          //_PaperSizes = new PaperSize[PrinterSettings.PaperSizes.Count];
          //PrinterSettings.PaperSizes.CopyTo(_PaperSizes, 0);
          // 28.08.2023. CopyTo() не реализован в Mono
          List<PaperSize> lstPaperSize = new List<PaperSize>(PrinterSettings.PaperSizes.Count);
          foreach (PaperSize ps in PrinterSettings.PaperSizes)
            lstPaperSize.Add(ps);
          _PaperSizes = lstPaperSize.ToArray();

          _CanDuplex = PrinterSettings.CanDuplex;
        }
        else
        {
          _PaperSizes = new PaperSize[0];
          _CanDuplex = false;
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    #endregion

    #region Свойства, продублированные из PrinterSettings

    /// <summary>
    /// Имя принтера, к которому относится объект
    /// </summary>
    public string PrinterName { get { return _PrinterName; } }
    private string _PrinterName;

    /// <summary>
    /// Оригинальный объект
    /// </summary>
    public PrinterSettings PrinterSettings { get { return _PrinterSettings; } }
    private PrinterSettings _PrinterSettings;


    /// <summary>
    /// true, если имя принтера было задано верно
    /// </summary>
    public bool IsValid { get { return _IsValid; } }
    private bool _IsValid;

    /// <summary>
    /// Поддерживает ли принтер двустороннюю печать
    /// </summary>
    public bool CanDuplex { get { return _CanDuplex; } }
    private bool _CanDuplex;

    /// <summary>
    /// Поддерживаемые принтером размеры бумаги
    /// </summary>
    public PaperSize[] PaperSizes { get { return _PaperSizes; } }
    private PaperSize[] _PaperSizes;

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает PrinterName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return PrinterName;
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства "EFPApp.Printers"
  /// </summary>
  public class EFPAppPrinters
  {
    #region Конструктор

    internal EFPAppPrinters()
    {
      _Printers = null;
      _ListVersion = 1;
    }

    private void GetReady()
    {
      if (_Printers != null)
        return; // Все готово
      EFPApp.BeginWait("Определение списка установленных принтеров", "Print");
      try
      {
        // Список имен принтеров
        //_PrinterNames = new string[PrinterSettings.InstalledPrinters.Count];
        // PrinterSettings.InstalledPrinters.CopyTo(_PrinterNames, 0);
        // 28.08.2023
        // В Mono (версия 6.12.0, без Wine) не реализован метод CopyTo()
        List<string> lstPrinterName = new List<string>(PrinterSettings.InstalledPrinters.Count);
        foreach (string name in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
          lstPrinterName.Add(name);
        _PrinterNames = lstPrinterName.ToArray();

        // Принтер по умолчанию
        _DefaultPrinterName = String.Empty;
        PrinterSettings ps = new PrinterSettings();
        ps.PrinterName = null;
        if (ps.IsValid)
          _DefaultPrinterName = ps.PrinterName;

        // В последнюю очередь - словарь принтеров
        _Printers = new Dictionary<string, EFPPrinterInfo>();
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Сброс информации о принтерах
    /// </summary>
    public void Reset()
    {
      if (_Printers != null)
      {
        _Printers = null;
        _DefaultPrinter = null; // отдельный механизм доступа, без GetReady()
        unchecked { _ListVersion++; }
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список имен установленных принтеров
    /// </summary>
    public string[] PrinterNames
    {
      get
      {
        GetReady();
        return _PrinterNames;
      }
    }
    private string[] _PrinterNames;

    /// <summary>
    /// Имя принтера по умолчанию
    /// </summary>
    public string DefaultPrinterName
    {
      get
      {
        GetReady();
        return _DefaultPrinterName;
      }
    }
    private string _DefaultPrinterName;

    /// <summary>
    /// Получение информации об одном принтере
    /// </summary>
    /// <param name="printerName">Имя принтера или null для получения информации
    /// о принтере по умолчанию</param>
    /// <returns>Описание принтера</returns>
    public EFPPrinterInfo this[string printerName]
    {
      get
      {
        GetReady();
        if (String.IsNullOrEmpty(printerName))
          return DefaultPrinter;
        EFPPrinterInfo res;
        if (!_Printers.TryGetValue(printerName, out res))
        {
          res = new EFPPrinterInfo(printerName);
          _Printers.Add(printerName, res);
        }
        return res;
      }
    }
    private Dictionary<string, EFPPrinterInfo> _Printers;

    /// <summary>
    /// Принтер по умолчанию
    /// </summary>
    public EFPPrinterInfo DefaultPrinter
    {
      get
      {
        if (_DefaultPrinter == null)
        {
          if (String.IsNullOrEmpty(DefaultPrinterName))
            _DefaultPrinter = new EFPPrinterInfo(null);
          else
            _DefaultPrinter = this[DefaultPrinterName];
        }
        return _DefaultPrinter;
      }
    }
    private EFPPrinterInfo _DefaultPrinter;

    /// <summary>
    /// Версия списка принтеров. Если список принтеров изменяется, то номер версии
    /// увеличивается
    /// </summary>
    public int ListVersion { get { return _ListVersion; } }
    private int _ListVersion;

    #endregion
  }

  /// <summary>
  /// Заполнитель для комбоблока списка принтеров
  /// </summary>
  public class PrinterComboBoxFiller
  {
    #region Конструктор

    /// <summary>
    /// Создает объект заполнителя.
    /// К управляющему элементу комбоблока добавляется обработчик события Enter, которые добавит список принтеров.
    /// </summary>
    /// <param name="control">Комбоблок</param>
    public PrinterComboBoxFiller(ComboBox control)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
#endif

      _Control = control;
      _PrevVersion = -1;

      control.Enter += new EventHandler(Control_Enter);
    }

    #endregion

    #region Поля

    private ComboBox _Control;
    private int _PrevVersion;

    #endregion

    #region Обработчики

    private void Control_Enter(object sender, EventArgs args)
    {
      if (_PrevVersion != EFPApp.Printers.ListVersion)
      {
        _PrevVersion = EFPApp.Printers.ListVersion;
        _Control.BeginUpdate();
        try
        {
          _Control.Items.Clear();
          _Control.Items.AddRange(EFPApp.Printers.PrinterNames);
        }
        finally
        {
          _Control.EndUpdate();
        }
      }
    }

    #endregion
  }

}
