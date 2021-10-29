using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Printing;
using Microsoft.Win32;
using System.Collections;
using System.Windows.Forms;
using FreeLibSet.Core;

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
  /// Реализация свойства EFPApp.Fonts
  /// </summary>
  public sealed class EFPAppFonts : DisposableObject
  {
    // Нельзя использовать SimpleDisposableObject в качестве базового класса

    #region Конструктор и Dispose

    /// <summary>
    /// Единственный экземпляр объекта создается по требованию. Он будет 
    /// реагировать на системные события и обновлять списки
    /// </summary>
    internal EFPAppFonts()
    {
      _EHInstalledFontsChanged = new EventHandler(InstalledFontChanged);
      SystemEvents.InstalledFontsChanged += _EHInstalledFontsChanged;
    }

    /// <summary>
    /// Вызывает при завершении работы приложения
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (_EHInstalledFontsChanged!=null)
      {
        SystemEvents.InstalledFontsChanged -= _EHInstalledFontsChanged;
        _EHInstalledFontsChanged = null;
      }
      base.Dispose(disposing);
    }

    /// <summary>
    /// Сброс буферизации списка шрифтов.
    /// Вызывается автоматически при получении системного сообщения.
    /// </summary>
    public void Reset()
    {
      _FontNames = null;
      _FontNamesVersion++;
    }

    #endregion

    #region Внутренняя реализация

    private EventHandler _EHInstalledFontsChanged;

    private void InstalledFontChanged(object sender, EventArgs args)
    {
      Reset();
    }

    /// <summary>
    /// Для отладки.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_FontNames == null)
        return "Список шрифтов не загружен";
      else
        return "Шрифтов: " + _FontNames.Length.ToString();
    }

    #endregion

    #region Имена шрифтов

    /// <summary>
    /// Список шрифтов, которые можно использовать в списке для выбора
    /// </summary>
    public string[] FontNames
    {
      get
      {
        if (_FontNames == null)
        {
          EFPApp.BeginWait("Загрузка списка установленных шрифтов");
          try
          {
            // 27.12.2020 Добавлен "using"
            using (System.Drawing.Text.InstalledFontCollection fc = new System.Drawing.Text.InstalledFontCollection())
            {
              _FontNames = new string[fc.Families.Length];
              for (int i = 0; i < _FontNames.Length; i++)
                _FontNames[i] = fc.Families[i].Name;
            }
          }
          finally
          {
            EFPApp.EndWait();
          }
        }
        return _FontNames;
      }
    }
    private string[] _FontNames;

    /// <summary>
    /// Номер версии, который увеличивается на 1 после вызова Reset().
    /// Используется для буферизации списков
    /// </summary>
    public int FontNamesVersion { get { return _FontNamesVersion; } }
    private int _FontNamesVersion = 0;

    #endregion
  }

  /// <summary>
  /// Заполнитель для комбоблока списка шрифтов
  /// </summary>
  public class FontComboBoxFiller
  {
    #region Конструктор

    /// <summary>
    /// Создает заполнитель
    /// </summary>
    /// <param name="control">Комбоблок</param>
    public FontComboBoxFiller(ComboBox control)
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
      // 27.12.2020 Не может быть null.
      //if (EFPApp.Fonts == null)
      //  return;
      if (_PrevVersion != EFPApp.Fonts.FontNamesVersion)
      {
        _PrevVersion = EFPApp.Fonts.FontNamesVersion;
        _Control.BeginUpdate();
        try
        {
          _Control.Items.Clear();
          _Control.Items.AddRange(EFPApp.Fonts.FontNames);
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
