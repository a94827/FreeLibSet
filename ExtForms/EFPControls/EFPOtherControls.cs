using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
    protected override EFPControlCommandItems GetCommandItems()
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
      try
      {
        if (String.IsNullOrEmpty(address))
          EFPApp.ShowTempMessage("Адрес электронной почты не задан");
        else
          Process.Start("mailto://" + address);
      }
      catch
      {
        EFPApp.ErrorMessageBox("Почтовая программа не установлена. Если Вы отправляете электронную почту не через почтовую программу, а со страницы в Интернете, то скопируйте адрес в буфер обмена и вставьте его в нужное поле ввода на странице");
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
    {
      _ControlProvider = controlProvider;

      ciStart = new EFPCommandItem("Service", "SendMail");
      ciStart.MenuText = "Запустить почтовую программу";
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
    public EFPEMailLabel Owner { get { return _ControlProvider; } }
    private EFPEMailLabel _ControlProvider;

    #endregion

    #region Команды

    EFPCommandItem ciStart, ciCopy;

    void ciStart_Click(object sender, EventArgs args)
    {
      EFPEMailLabel.StartMailTo(Owner.Address);
    }

    void ciCopy_Click(object sender, EventArgs args)
    {
      if (String.IsNullOrEmpty(Owner.Address))
        EFPApp.ShowTempMessage("Адрес электронной почты не задан");
      else
        EFPApp.Clipboard.SetText(Owner.Address);
    }

    #endregion
  }

  /// <summary>
  /// Провайдер элемента предварительного просмотра
  /// </summary>
  public class EFPExtPrintPreviewControl : EFPControl<ExtPrintPreviewControl>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPExtPrintPreviewControl(EFPBaseProvider baseProvider, ExtPrintPreviewControl control)
      : base(baseProvider, control, true)
    {
    }

    #endregion
  }
}
