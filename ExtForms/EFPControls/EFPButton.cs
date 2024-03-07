// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Обработчик для Button
  /// </summary>
  public class EFPButton : EFPTextViewControl<Button>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPButton(EFPBaseProvider baseProvider, Button control)
      : base(baseProvider, control, false)
    {
      if (!DesignMode)
        control.Click += new EventHandler(Control_Click);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если свойство установить в true, то будет предотвращено закрытие формы, пока выполняется обработчик
    /// события <see cref="EFPButton.Click"/>. Также предотвращается нажатие других кнопок, для которых это свойство
    /// установлено.
    /// По умолчанию свойство не установлено, т.к. обработчик может, в общем случае, выполнить закрытие
    /// формы.
    /// Свойство не может устанавливаться из самого обработчика события <see cref="Click"/>.
    /// Свойство не влияет на вызов стандартного обработчика <see cref="Control.Click"/>.
    /// Вложенный вызов события <see cref="EFPButton.Click"/> для той же кнопки предотвращается всегда.
    /// </summary>
    public bool PreventFormClosing
    {
      get { return _PreventFormClosing; }
      set
      {
        if (_InsideClick)
          throw new InvalidOperationException("Свойство не может устанавливаться из обработчика события Click");
        _PreventFormClosing = value;
      }
    }
    private bool _PreventFormClosing;

    #endregion

    #region SetImage

    /// <summary>
    /// Присваивает кнопке изображение из списка <see cref="EFPApp.MainImages"/>.
    /// Устанавливает свойство <see cref="ButtonBase.ImageAlign"/> для выравнивания по левому краю, если у кнопки есть текст
    /// и по центру, если кнопка содержит только значок без текста
    /// </summary>
    /// <param name="imageKey">Имя изображения из списка <see cref="EFPApp.MainImages"/></param>
    public void SetMainImageKey(string imageKey)
    {
      if (String.IsNullOrEmpty(imageKey))
        Control.Image = null;
      else
      {
        Control.Image = EFPApp.MainImages.Images[imageKey];
        if (String.IsNullOrEmpty(Control.Text))
          Control.ImageAlign = ContentAlignment.MiddleCenter;
        else
          Control.ImageAlign = ContentAlignment.MiddleLeft;
      }
    }

    #endregion

    #region Событие Click

    /// <summary>
    /// Событие вызывается при нажатии кнопки.
    /// В отличие от оригинального события <see cref="Control.Click"/>, при возникновении исключения
    /// в обработчике выводится окно сообщения об ошибке, а не происходит аварийное
    /// завершение программы.
    /// </summary>
    public event EventHandler Click;

    void Control_Click(object sender, EventArgs args)
    {
      try
      {
        DoClick();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при нажатии кнопки \"" + DisplayName + "\"");
      }
    }

    private void DoClick()
    {
      if (_InsideClick)
      {
        EFPApp.ShowTempMessage("Предыдущее нажатие кнопки \"" + DisplayName + "\" еще не обработано");
        return;
      }

      // Так нельзя, внутри обработчика может быть своя установка Enabled
      //bool OldEnabled = Enabled;
      //Enabled = false;
      _InsideClick = true;
      try
      {
        if (PreventFormClosing)
        {
          if (BaseProvider.ReentranceLocker.TryLock("Нажатие кнопки \"" + DisplayName + "\""))
          {
            try
            {
              OnClick();
            }
            finally
            {
              BaseProvider.ReentranceLocker.Unlock();
            }
          }
        }
        else // !PreventFormClosing
          OnClick();
      }
      finally
      {
        _InsideClick = false;
        //Enabled = OldEnabled;
      }
    }

    private bool _InsideClick = false;

    /// <summary>
    /// Вызывает обработчик события <see cref="Click"/>, если он присоединен.
    /// Иначе, если установлено свойство <see cref="Button.DialogResult"/>, выполняет закрытие формы.
    /// </summary>
    protected virtual void OnClick()
    {
      if (Click != null)
        Click(this, EventArgs.Empty);
      else if (Control.DialogResult != DialogResult.None && (!PreventFormClosing))
      {
        // 08.05.2022
        // Закрываем форму, если установлено свойство DialogResult.

        Form frm = Control.FindForm();
        if (frm != null)
        {
          frm.DialogResult = Control.DialogResult;
          frm.Close();
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Кнопка с выпадающим меню, открывающимся при нажатии кнопки.
  /// Для добавления команд меню можно использовать свойство CommandItems.
  /// Иначе предполагается, что инициализировано свойство <see cref="Control.ContextMenuStrip"/> или <see cref="Control.ContextMenu"/>
  /// </summary>
  public class EFPButtonWithMenu : EFPButton
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPButtonWithMenu(EFPBaseProvider baseProvider, Button control)
      : base(baseProvider, control)
    {
      //control.Image = EFPApp.MainImages.Images["MenuButton"];
      //if (String.IsNullOrEmpty(control.Text))
      //  control.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter; // 27.02.2020
      //else
      //  control.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft; // 27.02.2020
      SetMainImageKey("MenuButton"); // 05.11.2020
    }

    #endregion

    #region Нажатие кнопки

    /// <summary>
    /// Возвращает false, так как в локальное меню не нужно добавлять команды из родительского элемента.
    /// </summary>
    protected override bool BaseCommandItemsNeeded { get { return false; } }

    /// <summary>
    /// Показывает выпадаюшее меню рядом с кнопкой.
    /// Обработчик события <see cref="EFPButton.Click"/> не вызывается, если есть локальное меню, которое можно показать.
    /// </summary>
    protected override void OnClick()
    {
      Point startPos = new Point(Control.Width, Control.Height);
      if (Control.ContextMenuStrip != null)
        Control.ContextMenuStrip.Show(Control, startPos);
      else if (Control.ContextMenu != null)
        Control.ContextMenu.Show(Control, startPos);
      else
        base.OnClick();
    }

    #endregion
  }

  /// <summary>
  /// Кнопка для отображения списка ошибок <see cref="ErrorMessageList"/> с помощью <see cref="EFPApp.ShowErrorMessageListDialog(ErrorMessageList, string)"/>
  /// </summary>
  public class EFPErrorMessageListButton : EFPButton
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPErrorMessageListButton(EFPBaseProvider baseProvider, Button control)
      : base(baseProvider, control)
    {
      _AutoText = !String.IsNullOrEmpty(control.Text);
      _AutoImageKey = true;
      _AutoToolTipText = true;
      _AutoEnabled = true;
      _AutoValidate = true;

      if (String.IsNullOrEmpty(control.Text))
        DisplayName = "Сообщения";
    }

    #endregion

    #region Свойства списка сообщений

    /// <summary>
    /// Присоединенный список сообщений об ошибках.
    /// Установка свойства меняет значок и текст кнопки
    /// </summary>
    public ErrorMessageList ErrorMessages
    {
      get { return _ErrorMessages; }
      set
      {
        _ErrorMessages = value;
        if (HasBeenCreated)
          OnInitState();
        Validate();
      }
    }
    private ErrorMessageList _ErrorMessages;


    /// <summary>
    /// Задает ширину столбца "Код" в символах. См.свойство <see cref="EFPDataGridViewColumn.TextWidth"/>.
    /// Нулевое значение (по умолчанию) задает скрытый столбец.
    /// </summary>
    public int CodeWidth
    {
      get
      {
        return _CodeWidth;
      }
      set
      {
#if DEBUG
        if (value < 0 || value > 20)
          throw new ArgumentException("Недопустимая ширина колонки \"Код\": " + CodeWidth);
#endif
        _CodeWidth = value;
      }
    }
    private int _CodeWidth;

    /// <summary>
    /// Делегат обработчика редактирования строки с ошибкой.
    /// Вызывается, когда пользователь выполняет команду "Редактировать" для строки таблицы.
    /// Групповое редактирование нескольких строк не поддерживается.
    /// Обработчик получает одно сообщение из списка <see cref="ErrorMessages"/>, к которому относится текущая
    /// строка табличного просмотра.
    /// Обработчик может показать детальное сообщение об ошибке, либо перейти к объекту, к которому
    /// относится сообщение.
    /// Свойство не должно устанавливаться, если "редактирование" не имеет смысла. При этом
    /// команды редактирования не будут доступны, если не установлен обработчик события.
    /// 
    /// Если "редактирование" имеет смысл только для некоторых сообщений об ошибке (может быть 
    /// выполнен переход к ошибочному объекту), а для остальных - нет, то присоединяемый обработчик
    /// должен выдавать пользователю какое-либо сообщение, когда переход невозможен.
    /// </summary>
    public ErrorMessageItemEventHandler EditHandler
    {
      get { return _EditHandler; }
      set { _EditHandler = value; }
    }
    private ErrorMessageItemEventHandler _EditHandler;

    #endregion

    #region Свойства для управления кнопкой

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то текст кнопки
    /// устанавливается автоматически при присоединении списка. Чтобы установить
    /// свойство <see cref="Control.Text"/> вручную, сначала следует задать AutoText=false.
    /// Начальное значение свойста устанавливается в true, если <see cref="Control.Text"/> задает непустой текст.
    /// Если у кнопки нет текста, то свойство получает значение false.
    /// Свойство может устанавливаться только до вывода кнопки на экран.
    /// </summary>
    public bool AutoText
    {
      get { return _AutoText; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoText = value;
      }
    }
    private bool _AutoText;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то значок кнопки
    /// устанавливается автоматически при присоединении списка. Чтобы установить
    /// свойство <see cref="ButtonBase.ImageKey"/> вручную, сначала следует задать AutoImageKey=false.
    /// Свойство может устанавливаться только до вывода кнопки на экран.
    /// </summary>
    public bool AutoImageKey
    {
      get { return _AutoImageKey; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoImageKey = value;
      }
    }
    private bool _AutoImageKey;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то всплывающие подсказки заголовка закладки
    /// устанавливается автоматически при присоединении списка. Чтобы установить
    /// свойство <see cref="EFPControlBase.ToolTipText"/> вручную, сначала следует задать AutoToolTipText=false.
    /// Свойство может устанавливаться только до вывода кнопки на экран.
    /// </summary>
    public bool AutoToolTipText
    {
      get { return _AutoToolTipText; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoToolTipText = value;
      }
    }
    private bool _AutoToolTipText;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то кнопка будет автоматически блокироваться, 
    /// если список ошибок не присоединен или не содержит сообщений. Чтобы установливать 
    /// свойство <see cref="EFPControlBase.Enabled"/> вручную, или использовать <see cref="EFPControlBase.EnabledEx"/>, сначала следует задать AutoEnabled=false.
    /// Свойство может устанавливаться только до вывода кнопки на экран.
    /// </summary>
    public bool AutoEnabled
    {
      get { return _AutoEnabled; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoEnabled = value;
      }
    }
    private bool _AutoEnabled;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то при наличии в списке ошибок или предупреждений будет устанавливаться соответствующее значение
    /// свойства <see cref="EFPControlBase.ValidateState"/>. При наличии ошибок, например, нельзя будет нажать кнопку "ОК".
    /// Если сбросить свойство в false, то <see cref="EFPControlBase.ValidateState"/> будет оставаться равным Ok, независимо от списка ошибок.
    /// </summary>
    public bool AutoValidate
    {
      get { return _AutoValidate; }
      set
      {
        if (value == _AutoValidate)
          return;
        _AutoValidate = value;
        Validate();
      }
    }
    private bool _AutoValidate;

    #endregion

    #region Установка состояния

    /// <summary>
    /// Вызывается при выводе формы на экран
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();
      OnInitState();
    }

    private void OnInitState()
    {
      if (_AutoEnabled)
      {
        if (_ErrorMessages == null)
          Enabled = false;
        else
          Enabled = _ErrorMessages.Count > 0;
      }
      if (_AutoText)
        Text = EFPApp.GetErrorTitleText(_ErrorMessages);
      if (_AutoImageKey)
        Control.Image = EFPApp.MainImages.Images[EFPApp.GetErrorImageKey(_ErrorMessages)];
      if (_AutoToolTipText)
        ValueToolTipText = EFPApp.GetErrorToolTipText(_ErrorMessages);
    }

    #endregion

    #region Нажатие кнопки

    /// <summary>
    /// При нажатии кнопки вызывается <see cref="EFPApp.ShowErrorMessageListDialog(ErrorMessageList, string, int, ErrorMessageItemEventHandler)"/>
    /// </summary>
    protected override void OnClick()
    {
      base.OnClick();
      EFPApp.ShowErrorMessageListDialog(ErrorMessages, DisplayName, CodeWidth, EditHandler);
    }

    #endregion

    #region OnValidate()

    /// <summary>
    /// Выполняет проверку при установленном свойстве <see cref="AutoValidate"/>=true.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (!AutoValidate)
        return;

      if (_ErrorMessages == null)
        return;


      switch (_ErrorMessages.Severity)
      {
        case ErrorMessageKind.Error:
          if (_ErrorMessages.Count <= 5)
          {
            ErrorMessageList lst2 = _ErrorMessages.Clone(ErrorMessageKind.Error); // 26.10.2020
            SetError(lst2.AllText);
          }
          else
            SetError("Список содержит ошибки (" + _ErrorMessages.ErrorCount.ToString() + ")");
          break;

        case ErrorMessageKind.Warning:
          if (_ErrorMessages.Count <= 5)
          {
            ErrorMessageList lst2 = _ErrorMessages.Clone(ErrorMessageKind.Warning); // 26.10.2020
            //SetWarning(_ErrorMessages.AllText);
            SetWarning(lst2.AllText); // 05.01.2021
          }
          else
            SetWarning("Список содержит предупреждения (" + _ErrorMessages.WarningCount.ToString() + ")");
          break;
      }
    }

    #endregion
  }
}
