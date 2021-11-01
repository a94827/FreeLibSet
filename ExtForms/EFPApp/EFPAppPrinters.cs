﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Printing;
using System.Windows.Forms;

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
          _PaperSizes = new PaperSize[PrinterSettings.PaperSizes.Count];
          PrinterSettings.PaperSizes.CopyTo(_PaperSizes, 0);

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
        _PrinterNames = new string[PrinterSettings.InstalledPrinters.Count];
        PrinterSettings.InstalledPrinters.CopyTo(_PrinterNames, 0);

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