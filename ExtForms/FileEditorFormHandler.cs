using AgeyevAV.IO;
using System;
using System.Collections.Generic;
using System.Text;
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

#pragma warning disable 1591 // пока не утрясется

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Реализация общих функций для редактора файла
  /// Хранит имя файла и признак изменения. Формирует заголовок файла, обрабатывает команды "Сохранить" и "Сохранить как"
  /// </summary>
  public class FileEditorFormHandler
  {
    #region Конструктор

    public FileEditorFormHandler(EFPFormProvider formProvider, bool addCommands)
    {
      _FormProvider = formProvider;
      DefaultFormTitle = "Без имени";

      if (addCommands)
        formProvider.CommandItems = new FileEditorFormCommandItems(this);

      _FormProvider.FormClosing += new FormClosingEventHandler(Form_FormClosing);
    }

    #endregion

    #region Свойства и события

    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    /// <summary>
    /// Полный путь к файлу или пусто, если имя файла еще не было присвовено
    /// </summary>
    public AbsPath FileName
    {
      get { return _FileName; }
      set
      {
        if (value == _FileName)
          return;
        _FileName = value;
        OnFileNameChanged();
      }
    }

    private AbsPath _FileName;

    /// <summary>
    /// Заголовок формы, пока файл не сохранен. По умолчанию - "Без имени X"
    /// </summary>
    public string DefaultFormTitle
    {
      get { return _DefaultFormTitle; }
      set
      {
        if (value != _DefaultFormTitle)
        {
          _DefaultFormTitle = value;
          InitFormTitle();
        }
      }
    }
    private string _DefaultFormTitle;

    protected virtual void OnFileNameChanged()
    {
      InitFormTitle();
    }

    public bool Modified
    {
      get { return _Modified; }
      set
      {
        if (value == _Modified)
          return;
        _Modified = value;
        OnModifiedChanged();
      }
    }
    protected bool _Modified;

    public event EventHandler ModifiedChanged;

    private void OnModifiedChanged()
    {
      InitFormTitle();
      if (ModifiedChanged != null)
        ModifiedChanged(this, EventArgs.Empty);
    }

    private void InitFormTitle()
    {
      string s = Modified ? "(*) " : "";
      if (FileName.IsEmpty)
        s += DefaultFormTitle;
      else
        s += FileName.Path;
      _FormProvider.Form.Text = s;
    }

    #endregion

    #region Команды сохранения

    /// <summary>
    /// Строка фильтра для диалога "Сохранить как"
    /// </summary>
    public string SaveDialogFilter { get { return _SaveDialogFilter; } set { _SaveDialogFilter = value; } }
    private string _SaveDialogFilter;

    /// <summary>
    /// Расширение файла по умолчанию для диалога "Сохранить как"
    /// </summary>
    public string DefaultExt { get { return _DefaultExt; } set { _DefaultExt = value; } }
    private string _DefaultExt;

    /// <summary>
    /// Обработчик этого события должен быть установлен. На момент вызова
    /// свойство FileName задает имя сохраняемого файла
    /// </summary>
    public event EventHandler Save;

    public virtual void PerformSave(bool saveAs)
    {
      if (Save == null)
        throw new InvalidOperationException("Обработчик события Save не установлен");

      AbsPath OldFileName = FileName;
      AbsPath ThisFileName = FileName;
      if (saveAs || FileName.IsEmpty)
      {
        SaveFileDialog dlg = new SaveFileDialog();
        if (!FileName.IsEmpty)
          dlg.FileName = FileName.Path;
        dlg.Filter = SaveDialogFilter;
        dlg.DefaultExt = DefaultExt;

        if (dlg.ShowDialog() != DialogResult.OK)
          return;
        ThisFileName = new AbsPath(dlg.FileName);
      }

      // Временно заменяем имя файла
      try
      {
        _FileName = ThisFileName;
        Save(this, EventArgs.Empty);
      }
      finally
      {
        _FileName = OldFileName;
      }

      // Устанавливаем новое имя файла после успешного сохранения
      FileName = ThisFileName;
      Modified = false;
    }

    #endregion

    #region Обработка закрытия формы

    void Form_FormClosing(object sender, FormClosingEventArgs args)
    {
      if (!Modified)
        return;
      switch (EFPApp.MessageBox("Документ \"" + (FileName.IsEmpty ? DefaultFormTitle : FileName.Path) + "\" не сохранен. Сохранить изменения?",
        "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
      {
        case DialogResult.Yes:
          PerformSave(false);
          if (Modified)
            args.Cancel = true;
          break;
        case DialogResult.No:
          break;
        default:
          args.Cancel = true;
          break;
      }
    }

    #endregion
  }

  /// <summary>
  /// Команды "Сохранить" и "Сохранить как"
  /// </summary>
  public class FileEditorFormCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    public FileEditorFormCommandItems(FileEditorFormHandler formHandler)
    {
      _FormHandler = formHandler;

      ciSave = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Save);
      ciSave.Enabled = true;
      ciSave.Click += new EventHandler(ciSave_Click);
      base.Add(ciSave);

      ciSaveAs = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SaveAs);
      ciSaveAs.Enabled = true;
      ciSaveAs.Click += new EventHandler(ciSaveAs_Click);
      base.Add(ciSaveAs);
    }

    #endregion

    #region Свойства

    private FileEditorFormHandler _FormHandler;

    #endregion

    #region Команды сохранения

    private EFPCommandItem ciSave, ciSaveAs;

    private void ciSave_Click(object sender, EventArgs args)
    {
      _FormHandler.PerformSave(false);
    }

    private void ciSaveAs_Click(object sender, EventArgs args)
    {
      _FormHandler.PerformSave(true);
    }

    #endregion
  }
}
