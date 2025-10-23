// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
   
// Прочие провайдеры управляющих элементов

namespace FreeLibSet.Forms
{

  /// <summary>
  /// Метка со ссылкой на адрес электронной почты
  /// </summary>
  public class EFPEMailLabel : EFPControl<LinkLabel>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPEMailLabel(EFPBaseProvider baseProvider, LinkLabel control)
      : base(baseProvider, control, false)
    {
      if (!DesignMode)
        control.LinkClicked += new LinkLabelLinkClickedEventHandler(Control_LinkClicked);
    }

    void Control_LinkClicked(object sender, LinkLabelLinkClickedEventArgs args)
    {
      switch (args.Button)
      {
        case MouseButtons.Left:
          StartMailTo(Address);
          break;
        case MouseButtons.Right:
          ShowLocalMenu();
          break;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Электронный адрес.
    /// Установка значения свойства задает текст метки "e-mail адрес"
    /// </summary>
    public string Address
    {
      get { return _Address; }
      set
      {
        if (value == null)
          value = String.Empty;
        _Address = value;
        Control.Text = "e-mail " + value;
        Control.LinkArea = new LinkArea(7, value.Length);
      }
    }
    private string _Address;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает EFPEMailLabelCommandItems
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPEMailLabelCommandItems(this);
    }

    #endregion

    #region Статический метод

    /// <summary>
    /// Запускает процесс для отправки письма "mailto://Адрес".
    /// В случае отсутствия почтовой программы выдает сообщение об ошибке
    /// </summary>
    /// <param name="address">Адрес электронной почты</param>
    public static void StartMailTo(string address)
    {
      if (String.IsNullOrEmpty(address))
        EFPApp.ShowTempMessage(Res.EFPEMailLabel_Err_NoAddress);
      else
      {
        try
        {
          Process.Start("mailto://" + address);
        }
        catch
        {
          EFPApp.ErrorMessageBox(Res.EFPEMailLabel_Err_Start);
        }
      }
    }

    #endregion
  }


  /// <summary>
  /// Команды локального меню для EFPEMailLabel.
  /// Содержит единственную команду "Запустить почтовую программу"
  /// </summary>
  public class EFPEMailLabelCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPEMailLabelCommandItems(EFPEMailLabel controlProvider)
      :base(controlProvider)
    {
      ciStart = new EFPCommandItem("Service", "StartEMailApp");
      ciStart.MenuText = Res.Cmd_Menu_StartEMailApp;
      ciStart.ImageKey = "LetterClosed";
      ciStart.Click += new EventHandler(ciStart_Click);
      Add(ciStart);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Enabled = true;
      ciCopy.Click += new EventHandler(ciCopy_Click);
      Add(ciCopy);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPEMailLabel ControlProvider { get { return (EFPEMailLabel)(base.ControlProvider); } }

    #endregion

    #region Команды

    private readonly EFPCommandItem ciStart, ciCopy;

    void ciStart_Click(object sender, EventArgs args)
    {
      EFPEMailLabel.StartMailTo(ControlProvider.Address);
    }

    void ciCopy_Click(object sender, EventArgs args)
    {
      if (String.IsNullOrEmpty(ControlProvider.Address))
        EFPApp.ShowTempMessage(Res.EFPEMailLabel_Err_NoAddress);
      else
        new EFPClipboard().SetText(ControlProvider.Address);
    }

    #endregion
  }
}
