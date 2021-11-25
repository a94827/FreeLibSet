// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.RI;
using System.Threading;
using FreeLibSet.Config;
using FreeLibSet.Logging;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

// Реализация удаленного пользовательского интерфейса в Windows Forms

/*
 * Необходимо обеспечить расширяемость интерфейса, чтобы в прикладной программе можно было объявить
 * собственные элементы управления, производные от RI.Control, и обеспечить их соответствующей реализацией
 * на стороне клиента. 
 * Для создания соответствия между типами RIItem и классом управляющего элемента используется коллекция
 * EFPApp.RICreators. Она содержит коллекцию объектов, реализующих интефрейс IEFPAppRICreator. Один Creator
 * может создавать элементы интерфейса разных типов
 * После того, как от сервера получен RIDialog, перебирается коллекция RICreators, пока один из объектов не
 * обработает вызов и не создаст форму. Создатель объекта (диалога)
 * отвечает за создание дочерних элементов, при этом выполняется рекурсивное обращение к EFPApp.RICreators
 */

namespace FreeLibSet.Forms.RI
{
  /// <summary>
  /// Реализация удаленного пользовательского интерфейса.
  /// Вызовы методов интерфейса должны выполняться в основном потоке приложения EFPApp
  /// </summary>
  internal sealed class EFPAppRemoteInterface : MarshalByRefObject, IRemoteInterface
  {
    #region IRemoteInterface Members

    /// <summary>
    /// Вывести сообщение
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public void MessageBox(string text)
    {
      MessageBox(text, String.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Вывести сообщение
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна</param>
    public void MessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Вывести сообщение
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна</param>
    /// <param name="buttons">Кнопки</param>
    /// <returns>Выбранный пользователем ответ</returns>
    public DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons)
    {
      return MessageBox(text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Вывести сообщение
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна</param>
    /// <param name="buttons">Кнопки</param>
    /// <param name="icon">Значок</param>
    /// <returns>Выбранный пользователем ответ</returns>
    public DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
      return MessageBox(text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
    }

    /// <summary>
    /// Вывести сообщение
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок окна</param>
    /// <param name="buttons">Кнопки</param>
    /// <param name="icon">Значок</param>
    /// <param name="defaultButton">Выбранная кнопка по умолчанию</param>
    /// <returns>Выбранный пользователем ответ</returns>
    public DialogResult MessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
    {
#if DEBUG
      EFPApp.CheckMainThread();
#endif
      return (DialogResult)(int)EFPApp.MessageBox(text, caption,
        (System.Windows.Forms.MessageBoxButtons)(int)buttons,
        (System.Windows.Forms.MessageBoxIcon)(int)icon,
        (System.Windows.Forms.MessageBoxDefaultButton)(int)defaultButton);
    }

    /// <summary>
    /// Вывести сообщение об ошибке
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public void ErrorMessageBox(string text)
    {
      MessageBox(text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Вывести сообщение об ошибке
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок</param>
    public void ErrorMessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Вывести предупреждение
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public void WarningMessageBox(string text)
    {
      MessageBox(text, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Вывести предупреждение
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="caption">Заголовок</param>
    public void WarningMessageBox(string text, string caption)
    {
      MessageBox(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Показать произвольный диалог
    /// </summary>
    /// <param name="dialog">Собранный блок диалога</param>
    /// <returns>Выбранный пользователем результат (OK или Cancel)</returns>
    public DialogResult ShowDialog(Dialog dialog)
    {
      if (dialog == null)
        throw new ArgumentNullException("dialog");

      IEFPAppRIFormItem RIItem = EFPApp.RICreators.Create(dialog, null) as IEFPAppRIFormItem;
      RIItem.WriteValues();
      DialogResult Res = (DialogResult)(int)EFPApp.ShowDialog(RIItem.FormProvider.Form, true);
      RIItem.ReadValues();
      return Res;
    }

    /// <summary>
    /// Показать стандартный диалог
    /// </summary>
    /// <param name="dialog">Блок диалога с установленными свойствами</param>
    /// <returns>Выбранный пользователем результат (OK или Cancel)</returns>
    public DialogResult ShowDialog(StandardDialog dialog)
    {
      if (dialog == null)
        throw new ArgumentNullException("dialog");

      IEFPAppRIStandardDialogItem RIItem = EFPApp.RICreators.Create(dialog, null) as IEFPAppRIStandardDialogItem;
      RIItem.WriteValues();
      DialogResult Res = RIItem.ShowDialog();
      RIItem.ReadValues();
      return Res;
    }

    #endregion
  }

  #region Интерфейсы

  /// <summary>
  /// Интерфейс взаимодействия RIItem и "настоящего" управляющего элемента
  /// </summary>
  public interface IEFPAppRIItem
  {
    /// <summary>
    /// Запись значений из RIItem в управляющий элемент
    /// Отвечает за вызов метода дочерних элементов
    /// </summary>
    void WriteValues();

    /// <summary>
    /// Чтение значений из управляющего элемента в RIItem при нажатии кнопки "ОК"
    /// Отвечает за вызов метода дочерних элементов
    /// </summary>
    void ReadValues();
  }

  /// <summary>
  /// Расширяет IEFPAppRIItem возможностью проверки значений
  /// </summary>
  public interface IEFPAppRIControlItem : IEFPAppRIItem, IEFPControl
  {

  }

  /// <summary>
  /// Интерфейс формы
  /// </summary>
  public interface IEFPAppRIFormItem : IEFPAppRIItem
  {
    /// <summary>
    /// Провайдер формы
    /// </summary>
    EFPFormProvider FormProvider { get; }
  }

  /// <summary>
  /// Интерфейс стандартного блока диалога
  /// </summary>
  public interface IEFPAppRIStandardDialogItem : IEFPAppRIItem
  {
    /// <summary>
    /// Вывести стандартный блок диалога
    /// </summary>
    /// <returns>Выбранный пользователем ответ (Ok или Cancel)</returns>
    DialogResult ShowDialog();
  }

  /// <summary>
  /// Интерфейс создателя управляющего элемента
  /// Объекты, поддерживающие интерфейс, хранятся в коллекции EFPApp.RICreators
  /// </summary>
  public interface IEFPAppRICreator
  {
    /// <summary>
    /// Создать управляющий элемент 
    /// </summary>
    /// <param name="item">Сериализуемый объект, полученный с сервера</param>
    /// <param name="baseProvider">Базовый провайдер для создания EFPControl</param>
    /// <returns>Созданный переходник, если данный объект</returns>
    IEFPAppRIItem Create(RIItem item, EFPBaseProvider baseProvider);
  }

  #endregion

  /// <summary>
  /// Аргумент события EFPAppRICreators.BeforeCreate
  /// </summary>
  public sealed class EFPAppRIBeforeCreateEventArgs : EventArgs
  {
    #region Конструктор

    internal EFPAppRIBeforeCreateEventArgs(RIItem riItem, EFPBaseProvider baseProvider)
    {
      _RIItem = riItem;
      _BaseProvider = baseProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Описатель элемента удаленного пользовательского интерфейса, для которого нужно создать элемент.
    /// Не может быть null.
    /// </summary>
    public RIItem RIItem { get { return _RIItem; } }
    private RIItem _RIItem;

    /// <summary>
    /// Базовый провайдер, если создается управляющий элемент.
    /// Null, если создается блок диалога.
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private EFPBaseProvider _BaseProvider;

    /// <summary>
    /// Сюда может быть помещена ссылка на созданный объект. В этом случае не будут вызываться методы IEFPAppRICreator.Create().
    /// </summary>
    public IEFPAppRIItem Result { get { return _Result; } set { _Result = value; } }
    private IEFPAppRIItem _Result;

    #endregion
  }

  /// <summary>
  /// Делегат события EFPAppRICreators.BeforeCreate
  /// </summary>
  /// <param name="sender">Ссылка на объект EFPAppRICreators</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPAppRIBeforeCreateEventHandler(object sender, EFPAppRIBeforeCreateEventArgs args);

  /// <summary>
  /// Реализация свойства EFPApp.RICreators
  /// </summary>
  public sealed class EFPAppRICreators : List<IEFPAppRICreator>
  {
    #region Метод Create

    /// <summary>
    /// Создание IEFPAppRIItem для выбранной RIItem.
    /// Если в списке нет создателя для данного типа класса, генерируется исключение
    /// </summary>
    /// <param name="item">Описатель объект удаленного интерфейса</param>
    /// <param name="baseProvider">Провайдер для подключения</param>
    /// <returns>Интерфейс управляющего элемента</returns>
    public IEFPAppRIItem Create(RIItem item, EFPBaseProvider baseProvider)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      if (BeforeCreate != null)
      {
        EFPAppRIBeforeCreateEventArgs args = new EFPAppRIBeforeCreateEventArgs(item, baseProvider);
        BeforeCreate(this, args);
        if (args.Result != null)
          return args.Result;
      }

      for (int i = 0; i < Count; i++)
      {
        IEFPAppRIItem res = this[i].Create(item, baseProvider);
        if (res != null)
        {
          InitValidators(item, res);
          return res;
        }
      }

      throw new ArgumentException("В списке EFPApp.RICreators не один из объектов не смог создать управляющий элемент для элемента удаленного интерфейса типа " + item.GetType().ToString());
    }

    /// <summary>
    /// Событие вызывается при создании любого элемента удаленного пользовательского интерфейса.
    /// Пользовательский обработчик может, например, проверить аргумент RIItem и, при необходимости, добавить
    /// IEFPAppRICreator в список, если используется отложенная загрузка библиотек.
    /// Также, обработчик может создать элемент в обход списка IEFPAppRICreator.
    /// </summary>
    public event EFPAppRIBeforeCreateEventHandler BeforeCreate;

    #endregion

    #region Проверка корректности

    internal static void InitValidators(FreeLibSet.RI.RIItem riItem, IEFPAppRIItem efpItem)
    {
      FreeLibSet.RI.Control riControl = riItem as FreeLibSet.RI.Control;
      if (riControl == null)
        return;
      if (!riControl.HasValidators)
        return;

      IEFPAppRIControlItem efpControl = efpItem as IEFPAppRIControlItem;
      if (efpControl == null)
        throw new InvalidOperationException("Класс " + efpItem.GetType().ToString() + " не реализует интерфейс IEFPAppRIItemWithValidating для проверки корректности введенных значений. Нельзя использовать списки валидаторов " + riItem.GetType().ToString() + ".Validators");

      efpControl.Validators.AddRange(riControl.Validators);
    }

    #endregion
  }

  /// <summary>
  /// Генератор блока диалога, полосы и управляющего элемента с меткой
  /// </summary>
  internal class RIDialogCreator : IEFPAppRICreator
  {
    #region IEFPAppRICreator Members

    public IEFPAppRIItem Create(RIItem item, EFPBaseProvider baseProvider)
    {
      if (item is Dialog)
      {
        if (baseProvider != null)
          throw new ArgumentException("Для диалога BaseProvider не задается");
        return new DialogItem((Dialog)item);
      }
      if (item is Band)
        return new BandPanelItem((Band)item, baseProvider);
      if (item is ControlWithLabel)
        return new ControlWithLabelItem((ControlWithLabel)item, baseProvider);

      return null;
    }

    #endregion

    #region Блок диалога

    private class DialogItem : OKCancelForm, IEFPAppRIFormItem
    {
      #region Конструктор

      public DialogItem(Dialog dialog)
      {

        base.Text = dialog.Text;
        base.ShowIcon = false;
        base.Icon = WinFormsTools.AppIcon;
        base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        base.MaximizeBox = true;

        System.Windows.Forms.GroupBox grpBox = new System.Windows.Forms.GroupBox();
        grpBox.Text = String.Empty;
        grpBox.Dock = System.Windows.Forms.DockStyle.Fill;
        base.MainPanel.Controls.Add(grpBox);

        _BandPanel = new BandPanelItem(dialog.Controls, FormProvider);
        // 07.11.2017 Пока пусть расширяется как надо
        //BandPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        grpBox.Controls.Add(_BandPanel);
        //BandPanel.AutoSize = true;
        //BandPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

        //base.Size = new System.Drawing.Size(WinFormsTools.Scale(640, base.CurrentAutoScaleDimensions.Width/6F),
        //  WinFormsTools.Scale(480, base.CurrentAutoScaleDimensions.Height/13F)); // 07.11.2017
      }

      protected override void OnLoad(EventArgs args)
      {
        base.OnLoad(args);

        /*
        // 07.11.2017
        // Делаем форму нужного размера
        base.Width = Math.Max(BandPanel.Width + (base.Width - base.ClientSize.Width), 
          base.MinimumSize.Width);
        base.Height = BandPanel.Height + (base.Height - MainPanel.Height);
         * */

        // 10.11.2017
        // Сначала подбираем ширину, потом - высоту

        // Максимальный размер формы
        System.Drawing.Size MaxSize = System.Windows.Forms.Screen.FromControl(this).WorkingArea.Size;

        int ExtWidth = base.Width - MainPanel.ClientSize.Width - MainPanel.Margin.Size.Width;
        base.Width = Math.Min(Math.Max(_BandPanel.Width + _BandPanel.Padding.Size.Width + ExtWidth,
          base.MinimumSize.Width),
          MaxSize.Width);
        _BandPanel.Dock = System.Windows.Forms.DockStyle.Top;

        //BandPanel.PerformLayout();
        _BandPanel.FinishLoad();
        //BandPanel.PerformLayout();

        int ExtHeight = base.Height - MainPanel.ClientSize.Height + MainPanel.Margin.Size.Height;
        base.Height = Math.Min(_BandPanel.Height + _BandPanel.Padding.Size.Height + ExtHeight,
          MaxSize.Height);
        _BandPanel.Dock = System.Windows.Forms.DockStyle.Fill;

        _BandPanel.AutoScroll = true;

        WinFormsTools.PlaceFormInScreenCenter(this, true);
        // чтобы было ровно посередине и не выходило за пределы экрана
      }

      #endregion

      #region Свойства

      private BandPanelItem _BandPanel;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _BandPanel.WriteValues();
      }

      public void ReadValues()
      {
        _BandPanel.ReadValues();
      }

      EFPFormProvider IEFPAppRIFormItem.FormProvider { get { return base.FormProvider; } }

      #endregion
    }

    #endregion

    #region Полоса элементов

    private class BandPanelItem : BandPanel, IEFPAppRIItem
    {
      #region Конструктор

      public BandPanelItem(Band band, EFPBaseProvider baseProvider)
      {
        Children = new List<IEFPAppRIItem>();

        foreach (Control ctrl in band)
          Add(ctrl, baseProvider);
      }

      #endregion

      #region Список элементов

      private List<IEFPAppRIItem> Children;

      internal void Add(Control ctrl, EFPBaseProvider baseProvider)
      {
        IEFPAppRIItem Item = EFPApp.RICreators.Create(ctrl, baseProvider);
        Children.Add(Item);
        ControlWithLabelItem cwli = Item as ControlWithLabelItem;
        if (cwli != null)
        {
          EFPControlBase CP = cwli.ControlItem as EFPControlBase;
          BandPanelStripe Stripe = new BandPanelStripe(cwli.LabelText, CP.Control);
          base.Stripes.Add(Stripe);
          if (CP.LabelNeeded)
            CP.Label = Stripe.Label;
          return;
        }
        if (Item is EFPControlBase)
        {
          EFPControlBase CP = (EFPControlBase)Item;
          BandPanelStripe Stripe = new BandPanelStripe(String.Empty, CP.Control);
          base.Stripes.Add(Stripe);
          if (CP.LabelNeeded && base.Stripes.Count >= 2)
          {
            // Проверяем, что в предыдущей строке не сидит одна метка
            BandPanelStripe PrevStripe = base.Stripes[base.Stripes.Count - 2];
            if (PrevStripe.Label == null && PrevStripe.Control is System.Windows.Forms.Label)
              CP.Label = PrevStripe.Control;
          }
          return;
        }

        throw new InvalidOperationException("Элемент \"" + Item.ToString() + "\" нельзя добавить к полосе управляющих элементов");
      }

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        for (int i = 0; i < Children.Count; i++)
        {
          try
          {
            Children[i].WriteValues();
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, "Ошибка записи значения");
          }
        }
      }

      public void ReadValues()
      {
        for (int i = 0; i < Children.Count; i++)
          Children[i].ReadValues();
      }

      #endregion
    }

    #endregion

    #region Управляющий элемент с меткой

    private class ControlWithLabelItem : IEFPAppRIItem
    {
      #region Конструктор

      public ControlWithLabelItem(ControlWithLabel cwl, EFPBaseProvider baseProvider)
      {
        _LabelText = cwl.Label.Text;
        _ControlItem = EFPApp.RICreators.Create(cwl.MainControl, baseProvider);
      }

      #endregion

      #region Свойства

      public string LabelText { get { return _LabelText; } }
      private string _LabelText;

      public IEFPAppRIItem ControlItem { get { return _ControlItem; } }
      private IEFPAppRIItem _ControlItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        ControlItem.WriteValues();
      }

      public void ReadValues()
      {
        ControlItem.ReadValues();
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Статические методы, используемые для реализации элементов удаленного пользовательского интерфейса
  /// </summary>
  public static class EFPAppRITools
  {
    #region InitControlItem()

    private class ControlAdapter
    {
      #region Конструктор

      public ControlAdapter(EFPControlBase controlProvider, Control riItem)
      {
        _ControlProvider = controlProvider;
        _RIItem = riItem;
        // Не надо. Диалог показывается только один раз. _ControlProvider.Attached += new EventHandler(ControlProvider_Attached);
        _ControlProvider.Validating += new UIValidatingEventHandler(ControlProvider_Validating);
        controlProvider.Enabled = riItem.Enabled; // 25.11.2021
        if (riItem.EnabledExConnected)
        {
          if (riItem.EnabledEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            controlProvider.EnabledEx = riItem.EnabledEx;
          else
          {
            controlProvider.Enabled = riItem.Enabled; // обязательное присвоение, иначе свойство обнулится
            riItem.EnabledEx = controlProvider.EnabledEx;
          }
        }
      }

      #endregion

      #region Свойства

      EFPControlBase _ControlProvider;

      Control _RIItem;

      #endregion

      #region Проверка элемента

      //void ControlProvider_Attached(object sender, EventArgs args)
      //{
      //  _ValueChangedFlag = false;
      //}

      /// <summary>
      /// Если в процессе редактирования значение изменилось, флаг устанавливается в true для
      /// отключения вывода сообщения об ошибке
      /// </summary>
      private bool _ValueChangedFlag;

      void ControlProvider_Validating(object sender, UIValidatingEventArgs args)
      {
        if (args.ValidateState == UIValidateState.Error)
          return;

        // Не препятствуем закрытию окна, если проверка вызвана при закрытии формы
        if (_ControlProvider.BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
          return;

        if (String.IsNullOrEmpty(_RIItem.ErrorMessage))
          return;

        if (_ValueChangedFlag) // пользователь поменял текущее значение
          return;

        IEFPAppRIItem efpItem = _ControlProvider as IEFPAppRIItem;
        if (efpItem != null)
        {
          efpItem.ReadValues();
          if (_RIItem.HasChanges)
          {
            _ValueChangedFlag = true;
            return;
          }
        }

        args.SetError(_RIItem.ErrorMessage);
      }

      #endregion
    }

    /// <summary>
    /// Этот метод должен вызываться в конструкторе класса, реализующего интерфейс IEFPAppRIItem для провайдера управляющего элемента.
    /// Если используется иерархия из нескольких классов, то метод должен вызываться только один раз.
    /// 
    /// Реализует удаленную проверку состояния формы, обеспечивает свойство Control.EnabledEx.
    /// </summary>
    /// <param name="controlProvider">Расширение провайдера управляющего элемента, в котором реализуется интерфейс IEFPAppRIItem</param>
    /// <param name="riItem">Связанный объект удаленного интерфейса</param>
    public static void InitControlItem(EFPControlBase controlProvider, Control riItem)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");

      if (!(controlProvider is IEFPAppRIItem))
        throw new ArgumentException("Класс " + controlProvider.GetType().ToString() + " не реализует интерфейс IEFPAppRIItem", "controlProvider");

      if (riItem == null)
        throw new ArgumentNullException("riItem");

      if (riItem.HasValidators)
        controlProvider.Validators.AddRange(riItem.Validators);

      new ControlAdapter(controlProvider, riItem);
    }

    #endregion
  }

  internal class RIControlCreator : IEFPAppRICreator
  {
    #region IEFPAppRICreator Members

    public IEFPAppRIItem Create(RIItem riItem, EFPBaseProvider baseProvider)
    {
      if (riItem is FreeLibSet.RI.Label)
        return new LabelItem((FreeLibSet.RI.Label)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.PasswordBox)
        return new PasswordBoxItem((FreeLibSet.RI.PasswordBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.TextBox)
        return new TextBoxItem((FreeLibSet.RI.TextBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.IntEditBox)
        return new IntEditBoxItem((FreeLibSet.RI.IntEditBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.SingleEditBox)
        return new SingleEditBoxItem((FreeLibSet.RI.SingleEditBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.DoubleEditBox)
        return new DoubleEditBoxItem((FreeLibSet.RI.DoubleEditBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.DecimalEditBox)
        return new DecimalEditBoxItem((FreeLibSet.RI.DecimalEditBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.DateTimeBox)
        return new DateTimeBoxItem((FreeLibSet.RI.DateTimeBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.DateRangeBox)
        return new DateRangeBoxItem((FreeLibSet.RI.DateRangeBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.DateOrRangeBox)
        return new DateOrRangeBoxItem((FreeLibSet.RI.DateOrRangeBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.YearMonthBox)
        return new YearMonthBoxItem((FreeLibSet.RI.YearMonthBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.YearMonthRangeBox)
        return new YearMonthRangeBoxItem((FreeLibSet.RI.YearMonthRangeBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.CheckBox)
        return new CheckBoxItem((FreeLibSet.RI.CheckBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.RadioGroup)
        return new RadioGroupItem((FreeLibSet.RI.RadioGroup)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.InfoLabel)
        return new InfoLabelItem((FreeLibSet.RI.InfoLabel)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.ListComboBox)
        return new ListComboBoxItem((FreeLibSet.RI.ListComboBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.TextComboBox)
        return new TextComboBoxItem((FreeLibSet.RI.TextComboBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.CsvCodesComboBox)
        return new CsvCodesComboBoxItem((FreeLibSet.RI.CsvCodesComboBox)riItem, baseProvider);

      if (riItem is FreeLibSet.RI.FolderBrowserTextBox)
        return new FolderBrowserTextBoxItem((FreeLibSet.RI.FolderBrowserTextBox)riItem, baseProvider);
      if (riItem is FreeLibSet.RI.OpenFileTextBox)
        return new FileTextBoxItem((FreeLibSet.RI.OpenFileTextBox)riItem, baseProvider, FreeLibSet.Forms.FileDialogMode.Read);
      if (riItem is FreeLibSet.RI.SaveFileTextBox)
        return new FileTextBoxItem((FreeLibSet.RI.SaveFileTextBox)riItem, baseProvider, FreeLibSet.Forms.FileDialogMode.Write);

      return null;
    }

    #endregion

    #region Label

    private class LabelItem : FreeLibSet.Forms.EFPLabel, IEFPAppRIItem
    {
      #region Конструктор

      public LabelItem(FreeLibSet.RI.Label riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.Label())
      {
        base.Text = riItem.Text;
        //base.Control.AutoSize = true;
        base.Control.AutoSize = false;
        base.Control.Width = base.Control.PreferredWidth; // 07.11.2017
      }

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
      }

      public void ReadValues()
      {
      }

      #endregion
    }

    #endregion

    #region TextBox

    private class TextBoxItem: FreeLibSet.Forms.EFPTextBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public TextBoxItem(FreeLibSet.RI.TextBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.TextBox())
      {
        _RIItem = riItem;
        base.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.MaxLength = riItem.MaxLength;

        EFPAppRITools.InitControlItem(this, riItem);

        base.Text = riItem.Text; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalTextExConnected)
        {
          if (riItem.TextEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.TextEx = riItem.TextEx;
          else
            riItem.TextEx = base.TextEx;
        }

        base.ReadOnly = riItem.ReadOnly; // 25.11.2021
        if (riItem.InternalReadOnlyExConnected)
        {
          if (riItem.ReadOnlyEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.ReadOnlyEx = riItem.ReadOnlyEx;
          else
            riItem.ReadOnlyEx = base.ReadOnlyEx;
        }
      }

      FreeLibSet.RI.TextBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Text = _RIItem.Text;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Text = base.Text;
      }

      #endregion
    }

    private class PasswordBoxItem: FreeLibSet.Forms.EFPTextBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public PasswordBoxItem(FreeLibSet.RI.PasswordBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.TextBox())
      {
        Control.UseSystemPasswordChar = true;
        _RIItem = riItem;
        base.CanBeEmptyMode = riItem.CanBeEmptyMode;

        EFPAppRITools.InitControlItem(this, riItem);

        base.Text = riItem.Text; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalTextExConnected)
        {
          if (riItem.TextEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.TextEx = riItem.TextEx;
          else
            riItem.TextEx = base.TextEx;
        }
      }

      FreeLibSet.RI.PasswordBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Text = _RIItem.Text;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Text = base.Text;
      }

      #endregion
    }

    /// <summary>
    /// Поле ввода целого числа.
    /// Версия без стрелочек
    /// </summary>
    private class IntEditBoxItem : FreeLibSet.Forms.EFPIntEditBox, IEFPAppRIItem
    {
      #region Конструктор

      public IntEditBoxItem(FreeLibSet.RI.IntEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.IntEditBox())
      {
        _RIItem = riItem;
        InitEFPNumEditBox<Int32>(this, riItem);
      }

      FreeLibSet.RI.IntEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.NValue = _RIItem.NValue;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.NValue = base.NValue;
      }

      #endregion
    }

    private static void InitEFPNumEditBox<T>(EFPNumEditBoxBase<T> controlProvider, FreeLibSet.RI.BaseNumEditBox<T> riItem)
      where T : struct, IFormattable, IComparable<T>
    {
      controlProvider.CanBeEmptyMode = riItem.CanBeEmptyMode;
      controlProvider.Minimum = riItem.Minimum;
      controlProvider.Maximum = riItem.Maximum;
      controlProvider.Control.Format = riItem.Format;
      controlProvider.Control.UpDownHandler = riItem.UpDownHandler;
      EFPAppRITools.InitControlItem(controlProvider, riItem);

      controlProvider.NValue = riItem.NValue; // обязательное присвоение, иначе свойство обнулится
      if (riItem.InternalNValueExConnected)
      {
        if (riItem.NValueEx.HasSource)
          // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
          controlProvider.NValueEx = riItem.NValueEx;
        else
          riItem.NValueEx = controlProvider.NValueEx;
      }
      if (riItem.InternalValueExConnected)
      {
        if (riItem.ValueEx.HasSource)
          // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
          controlProvider.ValueEx = riItem.ValueEx;
        else
          riItem.ValueEx = controlProvider.ValueEx;
      }

      controlProvider.ReadOnly = riItem.ReadOnly;
      if (riItem.InternalReadOnlyExConnected)
      {
        if (riItem.ReadOnlyEx.HasSource)
          // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
          controlProvider.ReadOnlyEx = riItem.ReadOnlyEx;
        else
          riItem.ReadOnlyEx = controlProvider.ReadOnlyEx;
      }
    }

    private class SingleEditBoxItem : FreeLibSet.Forms.EFPSingleEditBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public SingleEditBoxItem(FreeLibSet.RI.SingleEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.SingleEditBox())
      {
        _RIItem = riItem;
        InitEFPNumEditBox<Single>(this, riItem);
      }

      FreeLibSet.RI.SingleEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.NValue = _RIItem.NValue;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.NValue = base.NValue;
      }

      #endregion
    }

    private class DoubleEditBoxItem : FreeLibSet.Forms.EFPDoubleEditBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public DoubleEditBoxItem(FreeLibSet.RI.DoubleEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.DoubleEditBox())
      {
        _RIItem = riItem;
        InitEFPNumEditBox<Double>(this, riItem);
      }

      FreeLibSet.RI.DoubleEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.NValue = _RIItem.NValue;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.NValue = base.NValue;
      }

      #endregion
    }

    private class DecimalEditBoxItem : FreeLibSet.Forms.EFPDecimalEditBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public DecimalEditBoxItem(FreeLibSet.RI.DecimalEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.DecimalEditBox())
      {
        _RIItem = riItem;
        InitEFPNumEditBox<Decimal>(this, riItem);
      }

      FreeLibSet.RI.DecimalEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.NValue = _RIItem.NValue;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.NValue = base.NValue;
      }

      #endregion
    }

    #endregion

    #region CheckBox

    private class CheckBoxItem : FreeLibSet.Forms.EFPCheckBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public CheckBoxItem(FreeLibSet.RI.CheckBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.CheckBox())
      {
        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        base.Control.Text = riItem.Text;
        base.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.NChecked = riItem.NChecked; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalNCheckedExConnected)
        {
          if (riItem.NCheckedEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            this.NCheckedEx = riItem.NCheckedEx;
          else
            riItem.NCheckedEx = this.NCheckedEx;
        }
        if (riItem.InternalCheckedExConnected)
        {
          if (riItem.CheckedEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.CheckedEx = riItem.CheckedEx;
          else
            riItem.CheckedEx = base.CheckedEx;
        }
      }

      FreeLibSet.RI.CheckBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.NChecked = _RIItem.NChecked;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.NChecked = base.NChecked;
      }

      #endregion
    }

    #endregion

    #region RadioGroup

    private class RadioGroupItem : FreeLibSet.Forms.EFPRadioButtons, IEFPAppRIControlItem
    {
      #region Конструктор

      public RadioGroupItem(FreeLibSet.RI.RadioGroup riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.RadioGroupBox(riItem.Items))
      {
        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        base.Codes = riItem.Codes;
        //base.UnselectedCode = RIItem.UnselectedCode;
        base.SelectedIndex = riItem.SelectedIndex; // испр. 23.11.2021
        if (riItem.InternalSelectedIndexExConnected)
        {
          if (riItem.SelectedIndexEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedIndexEx = riItem.SelectedIndexEx;
          else
            base.SelectedIndex = riItem.SelectedIndex; // обязательное присвоение, иначе свойство обнулится
        }

        if (riItem.InternalSelectedCodeExConnected)
        {
          if (riItem.SelectedCodeEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedCodeEx = riItem.SelectedCodeEx;
          else
            riItem.SelectedCodeEx = base.SelectedCodeEx;
        }
      }

      FreeLibSet.RI.RadioGroup _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.SelectedIndex = _RIItem.SelectedIndex;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.SelectedIndex = base.SelectedIndex;
      }

      #endregion
    }

    #endregion

    #region DateBox

    private class DateTimeBoxItem : FreeLibSet.Forms.EFPDateTimeBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public DateTimeBoxItem(FreeLibSet.RI.DateTimeBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.DateTimeBox())
      {
        _RIItem = riItem;

        base.Control.Kind = riItem.Kind;

        base.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.Minimum = riItem.Minimum;
        base.Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);

        base.NValue = riItem.NValue; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalNValueExConnected)
        {
          if (riItem.NValueEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.NValueEx = riItem.NValueEx;
          else
            riItem.NValueEx = base.NValueEx;
        }
        if (riItem.InternalValueExConnected)
        {
          if (riItem.ValueEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.ValueEx = riItem.ValueEx;
          else
            riItem.ValueEx = base.ValueEx;
        }
        if (riItem.InternalNTimeExConnected)
        {
          if (riItem.NTimeEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.NTimeEx = riItem.NTimeEx;
          else
            riItem.NTimeEx = base.NTimeEx;
        }
        if (riItem.InternalTimeExConnected)
        {
          if (riItem.TimeEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.TimeEx = riItem.TimeEx;
          else
            riItem.TimeEx = base.TimeEx;
        }

        base.ReadOnly = riItem.ReadOnly;
        if (riItem.InternalReadOnlyExConnected)
        {
          if (riItem.ReadOnlyEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.ReadOnlyEx = riItem.ReadOnlyEx;
          else
            riItem.ReadOnlyEx = base.ReadOnlyEx;
        }
      }

      FreeLibSet.RI.DateTimeBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.NValue = _RIItem.NValue;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.NValue = base.NValue;
      }

      #endregion
    }

    #endregion

    #region DateRangeBox

    private class DateRangeBoxItem : FreeLibSet.Forms.EFPDateRangeBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public DateRangeBoxItem(FreeLibSet.RI.DateRangeBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.DateRangeBox())
      {
        _RIItem = riItem;
        base.First.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.Last.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.First.Minimum = riItem.MinimumFirstDate;
        base.First.Maximum = riItem.MaximumFirstDate;
        base.Last.Minimum = riItem.MinimumLastDate;
        base.Last.Maximum = riItem.MaximumLastDate;
        EFPAppRITools.InitControlItem(this, riItem);

        base.First.NValue = riItem.NFirstDate; // обязательное присвоение, иначе свойство обнулится
        base.Last.NValue = riItem.NLastDate; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalNFirstDateExConnected)
        {
          if (riItem.NFirstDateEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.First.NValueEx = riItem.NFirstDateEx;
          else
            riItem.NFirstDateEx = base.First.NValueEx;
        }
        if (riItem.InternalFirstDateExConnected)
        {
          if (riItem.FirstDateEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.First.ValueEx = riItem.FirstDateEx;
          else
            riItem.FirstDateEx = base.First.ValueEx;
        }

        if (riItem.InternalNLastDateExConnected)
        {
          if (riItem.NLastDateEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.Last.NValueEx = riItem.NLastDateEx;
          else
            riItem.NLastDateEx = base.Last.NValueEx;
        }
        if (riItem.InternalLastDateExConnected)
        {
          if (riItem.LastDateEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.Last.ValueEx = riItem.LastDateEx;
          else
            riItem.LastDateEx = base.Last.ValueEx;
        }
      }

      FreeLibSet.RI.DateRangeBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.First.NValue = _RIItem.NFirstDate;
        base.Last.NValue = _RIItem.NLastDate;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.NFirstDate = base.First.NValue;
        _RIItem.NLastDate = base.Last.NValue;
      }

      #endregion
    }

    #endregion

    #region DateOrRangeBox

    private class DateOrRangeBoxItem : FreeLibSet.Forms.EFPDateOrRangeBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public DateOrRangeBoxItem(FreeLibSet.RI.DateOrRangeBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.UserMaskedComboBox())
      {
        _RIItem = riItem;

        base.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.Minimum = riItem.Minimum;
        base.Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);

        base.DateRange = riItem.DateRange; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalFirstDateExConnected)
        {
          if (riItem.FirstDateEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.FirstDateEx = riItem.FirstDateEx;
          else
            riItem.FirstDateEx = base.FirstDateEx;
        }
        if (riItem.InternalLastDateExConnected)
        {
          if (riItem.LastDateEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.LastDateEx = riItem.LastDateEx;
          else
            riItem.LastDateEx = base.LastDateEx;
        }
        if (riItem.InternalNFirstDateExConnected)
        {
          if (riItem.NFirstDateEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.NFirstDateEx = riItem.NFirstDateEx;
          else
            riItem.NFirstDateEx = base.NFirstDateEx;
        }
        if (riItem.InternalNLastDateExConnected)
        {
          if (riItem.NLastDateEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.NLastDateEx = riItem.NLastDateEx;
          else
            riItem.NLastDateEx = base.NLastDateEx;
        }
      }

      FreeLibSet.RI.DateOrRangeBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.DateRange = _RIItem.DateRange;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.DateRange = base.DateRange;
      }

      #endregion
    }

    #endregion

    #region YearMonthBox

    private class YearMonthBoxItem : FreeLibSet.Forms.EFPYearMonthBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public YearMonthBoxItem(FreeLibSet.RI.YearMonthBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.YearMonthBox())
      {
        _RIItem = riItem;
        Minimum = riItem.Minimum;
        Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);

        base.Year = riItem.Year; // обязательное присвоение, иначе свойство обнулится
        base.Month = riItem.Month; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalYearExConnected)
        {
          if (riItem.YearEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.YearEx = riItem.YearEx;
          else
            riItem.YearEx = base.YearEx;
        }

        if (riItem.InternalMonthExConnected)
        {
          if (riItem.MonthEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.MonthEx = riItem.MonthEx;
          else
            riItem.MonthEx = base.MonthEx;
        }

        if (riItem.InternalYMExConnected)
        {
          if (riItem.YMEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.YMEx = riItem.YMEx;
          else
            riItem.YMEx = base.YMEx;
        }
      }

      FreeLibSet.RI.YearMonthBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Year = _RIItem.Year;
        base.Month = _RIItem.Month;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Year = base.Year;
        _RIItem.Month = base.Month;
      }

      #endregion
    }

    #endregion

    #region YearMonthRangeBox

    private class YearMonthRangeBoxItem : FreeLibSet.Forms.EFPYearMonthRangeBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public YearMonthRangeBoxItem(FreeLibSet.RI.YearMonthRangeBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.YearMonthRangeBox())
      {
        _RIItem = riItem;
        Minimum = riItem.Minimum;
        Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);

        base.Year = riItem.Year; // обязательное присвоение, иначе свойство обнулится
        base.FirstMonth = riItem.FirstMonth; // обязательное присвоение, иначе свойство обнулится
        base.LastMonth = riItem.LastMonth; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalYearExConnected)
        {
          if (riItem.YearEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.YearEx = riItem.YearEx;
          else
            riItem.YearEx = base.YearEx;
        }

        if (riItem.InternalFirstMonthExConnected)
        {
          if (riItem.FirstMonthEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.FirstMonthEx = riItem.FirstMonthEx;
          else
            riItem.FirstMonthEx = base.FirstMonthEx;
        }

        if (riItem.InternalLastMonthExConnected)
        {
          if (riItem.LastMonthEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.LastMonthEx = riItem.LastMonthEx;
          else
            riItem.LastMonthEx = base.LastMonthEx;
        }

        if (riItem.InternalFirstYMExConnected)
        {
          if (riItem.FirstYMEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.FirstYMEx = riItem.FirstYMEx;
          else
            riItem.FirstYMEx = base.FirstYMEx;
        }

        if (riItem.InternalLastYMExConnected)
        {
          if (riItem.LastYMEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.LastYMEx = riItem.LastYMEx;
          else
            riItem.LastYMEx = base.LastYMEx;
        }
      }

      FreeLibSet.RI.YearMonthRangeBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Year = _RIItem.Year;
        base.FirstMonth = _RIItem.FirstMonth;
        base.LastMonth = _RIItem.LastMonth;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Year = base.Year;
        _RIItem.FirstMonth = base.FirstMonth;
        _RIItem.LastMonth = base.LastMonth;
      }

      #endregion
    }

    #endregion

    #region InfoLabel

    private class InfoLabelItem : FreeLibSet.Forms.EFPInfoLabel, IEFPAppRIItem
    {
      #region Конструктор

      public InfoLabelItem(FreeLibSet.RI.InfoLabel riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.InfoLabel())
      {
        base.Text = riItem.Text;
        base.Control.Icon = (System.Windows.Forms.MessageBoxIcon)(int)(riItem.Icon);
        base.Control.AutoSize = true; // 26.03.2016

        switch (riItem.ColorType)
        {
          case InfoLabelColorType.Info:
            break;
          case InfoLabelColorType.Simple:
            base.Control.BackColor = System.Drawing.Color.Transparent;
            base.Control.ForeColor = System.Drawing.SystemColors.ControlText;
            break;
          default:
            throw new BugException("Неизвестное значение свойства ColorType=" + riItem.ColorType.ToString());
        }
      }

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
      }

      public void ReadValues()
      {
      }

      #endregion
    }

    #endregion

    #region Комбоблоки

    private class ListComboBoxItem : FreeLibSet.Forms.EFPListComboBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public ListComboBoxItem(FreeLibSet.RI.ListComboBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.ComboBox())
      {
        base.Control.Items.AddRange(riItem.Items);
        base.CanBeEmpty = false;
        WinFormsTools.SetComboBoxWidth(base.Control);

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        base.Codes = riItem.Codes;
        //base.UnselectedCode = RIItem.UnselectedCode;
        base.SelectedIndex = riItem.SelectedIndex; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalSelectedIndexExConnected)
        {
          if (riItem.SelectedIndexEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedIndexEx = riItem.SelectedIndexEx;
          else
            riItem.SelectedIndexEx = base.SelectedIndexEx;
        }

        if (riItem.InternalSelectedCodeExConnected)
        {
          if (riItem.SelectedCodeEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedCodeEx = riItem.SelectedCodeEx;
          else
            riItem.SelectedCodeEx = base.SelectedCodeEx;
        }
      }

      FreeLibSet.RI.ListComboBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.SelectedIndex = _RIItem.SelectedIndex;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.SelectedIndex = base.SelectedIndex;
      }

      #endregion
    }

    private class TextComboBoxItem : FreeLibSet.Forms.EFPTextComboBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public TextComboBoxItem(FreeLibSet.RI.TextComboBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.ComboBox())
      {
        base.Control.Items.AddRange(riItem.Items);
        WinFormsTools.SetComboBoxWidth(base.Control);

        base.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.MaxLength = riItem.MaxLength;

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        base.Text = riItem.Text; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalTextExConnected)
        {
          if (riItem.TextEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.TextEx = riItem.TextEx;
          else
            riItem.TextEx = base.TextEx;
        }
      }

      FreeLibSet.RI.TextComboBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Text = _RIItem.Text;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Text = base.Text;
      }

      #endregion
    }

    private class CsvCodesComboBoxItem : FreeLibSet.Forms.EFPCsvCodesComboBox, IEFPAppRIControlItem
    {
      #region Конструктор

      public CsvCodesComboBoxItem(FreeLibSet.RI.CsvCodesComboBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new FreeLibSet.Controls.UserTextComboBox(), riItem.Codes)
      {
        base.CanBeEmptyMode = riItem.CanBeEmptyMode;
        base.Names = riItem.Names;
        base.UnknownCodeSeverity = riItem.UnknownCodeSeverity;

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        base.SelectedCodes = riItem.SelectedCodes; // обязательное присвоение, иначе свойство обнулится
        if (riItem.InternalSelectedCodesExConnected)
        {
          if (riItem.SelectedCodesEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedCodesEx = riItem.SelectedCodesEx;
          else
            riItem.SelectedCodesEx = base.SelectedCodesEx;
        }

        base.ReadOnly = riItem.ReadOnly; 
        if (riItem.InternalReadOnlyExConnected)
        {
          if (riItem.ReadOnlyEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.ReadOnlyEx = riItem.ReadOnlyEx;
          else
            riItem.ReadOnlyEx = base.ReadOnlyEx;
        }

        if (riItem.HasCodeValidators)
          base.CodeValidators.AddRange(riItem.CodeValidators);

        if (riItem.InternalValidatingCodeExConnected)
          riItem.InternalSetValidatingCodeEx(base.ValidatingCodeEx);
      }

      FreeLibSet.RI.CsvCodesComboBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.SelectedCodes = _RIItem.SelectedCodes;
      }

      public void ReadValues()
      {
        _RIItem.SelectedCodes = base.SelectedCodes;
      }

      #endregion
    }

    #endregion

    #region Поля ввода имени файла / каталога

    private class TextBoxWithButton : System.Windows.Forms.UserControl
    {
      #region Конструктор

      public TextBoxWithButton()
      {
        //base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        //base.Size = new System.Drawing.Size(360, 24);

        TheTextBox = new System.Windows.Forms.TextBox();
        TheTextBox.Location = new System.Drawing.Point(0, 0);
        TheTextBox.Size = new System.Drawing.Size(Size.Width - 88 - 8, TheTextBox.Height);
        TheTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        TheTextBox.TabIndex = 0;
        base.Controls.Add(TheTextBox);

        TheButton = new System.Windows.Forms.Button();
        TheButton.Location = new System.Drawing.Point(Size.Width - 88, 0);
        TheButton.Size = new System.Drawing.Size(88, Math.Max(24, TheTextBox.Height));
        TheButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        TheButton.TabIndex = 1;
        TheButton.Image = EFPApp.MainImages.Images["Open"];
        TheButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        TheButton.Text = "Обзор";
        base.Controls.Add(TheButton);
        System.Drawing.Size ps = new System.Drawing.Size(360, 24);
        // ReSharper disable once VirtualMemberCallInConstructor
        base.Size = WinFormsTools.Max(GetPreferredSize(ps), ps);

      }

      #endregion

      #region Свойства

      public readonly System.Windows.Forms.TextBox TheTextBox;
      public readonly System.Windows.Forms.Button TheButton;

      #endregion

      #region Переопределенные методы

      public override System.Drawing.Size GetPreferredSize(System.Drawing.Size proposedSize)
      {
        if (TheTextBox == null)
          return base.GetPreferredSize(proposedSize);

        System.Drawing.Size Size1 = TheTextBox.GetPreferredSize(proposedSize);
        System.Drawing.Size Size2 = TheButton.GetPreferredSize(proposedSize);
        return new System.Drawing.Size(Size1.Width + 8 + Size2.Width, Math.Max(Size1.Height, Size2.Height));
      }

      public override System.Drawing.Size MinimumSize
      {
        get
        {
          if (TheTextBox == null)
            return base.MinimumSize;

          return new System.Drawing.Size(2 * TheButton.Width + 8, Math.Max(TheTextBox.Height, TheButton.Height));
        }
        set { }
      }
      public override System.Drawing.Size MaximumSize
      {
        get
        {
          if (TheTextBox == null)
            return base.MaximumSize;

          return new System.Drawing.Size(Int32.MaxValue, Math.Max(TheTextBox.Height, TheButton.Height));
        }
        set { }
      }

      #endregion
    }

    private class EFPTextBoxWithButton : EFPControl<TextBoxWithButton>
    {
      #region Конструктор

      public EFPTextBoxWithButton(EFPBaseProvider baseProvider)
        : base(baseProvider, new TextBoxWithButton(), true)
      {
        TheTextBox = new EFPTextBox(baseProvider, Control.TheTextBox);
      }

      #endregion

      #region Свойства

      public readonly EFPTextBox TheTextBox;

      public override System.Windows.Forms.Control Label
      {
        get
        {
          return base.Label;
        }
        set
        {
          base.Label = value;
          TheTextBox.Label = value;
        }
      }

      #endregion
    }


    private class FolderBrowserTextBoxItem : EFPTextBoxWithButton, IEFPAppRIControlItem
    {
      #region Конструктор

      public FolderBrowserTextBoxItem(FreeLibSet.RI.FolderBrowserTextBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider)
      {
        base.TheTextBox.CanBeEmptyMode = riItem.CanBeEmptyMode;

        base.TheTextBox.ReadOnly = riItem.ReadOnly; 
        if (riItem.InternalReadOnlyExConnected)
        {
          if (riItem.ReadOnlyEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.TheTextBox.ReadOnlyEx = riItem.ReadOnlyEx;
          else
            riItem.ReadOnlyEx = base.TheTextBox.ReadOnlyEx;
        }

        TheButton = new EFPFolderBrowserButton(TheTextBox, Control.TheButton);
        TheButton.Description = riItem.Description;
        TheButton.ShowNewFolderButton = riItem.ShowNewFolderButton;
        TheButton.PathValidateMode = riItem.PathValidateMode;
        TheButton.Path = riItem.Path;
        if (riItem.InternalPathExConnected)
        {
          if (riItem.PathEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            TheButton.PathEx = riItem.PathEx;
          else
            riItem.PathEx = TheButton.PathEx;
        }

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        TheButton.EnabledEx = TheTextBox.EditableEx; // 25.11.2021
      }

      EFPFolderBrowserButton TheButton;

      FreeLibSet.RI.FolderBrowserTextBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.TheTextBox.Text = _RIItem.Path.SlashedPath;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Path = new FreeLibSet.IO.AbsPath(base.TheTextBox.Text);
      }

      #endregion
    }

    private class FileTextBoxItem : EFPTextBoxWithButton, IEFPAppRIControlItem
    {
      #region Конструктор

      public FileTextBoxItem(FreeLibSet.RI.FileTextBox riItem, EFPBaseProvider baseProvider, FreeLibSet.Forms.FileDialogMode mode)
        : base(baseProvider)
      {
        base.TheTextBox.CanBeEmptyMode = riItem.CanBeEmptyMode;

        TheButton = new EFPFileDialogButton(TheTextBox, Control.TheButton);
        TheButton.Mode = mode;
        TheButton.Filter = riItem.Filter;
        TheButton.PathValidateMode = riItem.PathValidateMode;
        TheButton.Path = riItem.Path;
        if (riItem.InternalPathExConnected)
        {
          if (riItem.PathEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            TheButton.PathEx = riItem.PathEx;
          else
            riItem.PathEx = TheButton.PathEx;
        }

        base.TheTextBox.ReadOnly = riItem.ReadOnly;
        if (riItem.InternalReadOnlyExConnected)
        {
          if (riItem.ReadOnlyEx.HasSource)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.TheTextBox.ReadOnlyEx = riItem.ReadOnlyEx;
          else
            riItem.ReadOnlyEx = base.TheTextBox.ReadOnlyEx;
        }

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        TheButton.EnabledEx = TheTextBox.EditableEx; // 25.11.2021
      }

      EFPFileDialogButton TheButton;

      FreeLibSet.RI.FileTextBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.TheTextBox.Text = _RIItem.Path.Path;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Path = new FreeLibSet.IO.AbsPath(base.TheTextBox.Text);
      }

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Генератор стандартных блоков диалога
  /// </summary>
  internal class RIStandardDialogCreator : IEFPAppRICreator
  {
    #region IEFPAppRICreator Members

    public IEFPAppRIItem Create(RIItem riItem, EFPBaseProvider baseProvider)
    {
      #region Файлы и папки

      if (riItem is FolderBrowserDialog)
        return new FolderBrowserDialogItem((FolderBrowserDialog)riItem);
      if (riItem is OpenFileDialog)
        return new FileDialogItem((OpenFileDialog)riItem, false);
      if (riItem is SaveFileDialog)
        return new FileDialogItem((SaveFileDialog)riItem, true);

      #endregion

      #region Ввод значений

      if (riItem is FreeLibSet.RI.PasswordInputDialog)
        return new PasswordInputDialogItem((FreeLibSet.RI.PasswordInputDialog)riItem);
      if (riItem is FreeLibSet.RI.TextInputDialog)
        return new TextInputDialogItem((FreeLibSet.RI.TextInputDialog)riItem);
      if (riItem is FreeLibSet.RI.TextComboInputDialog)
        return new TextComboInputDialogItem((FreeLibSet.RI.TextComboInputDialog)riItem);
      if (riItem is FreeLibSet.RI.IntInputDialog)
        return new IntInputDialogItem((FreeLibSet.RI.IntInputDialog)riItem);
      if (riItem is FreeLibSet.RI.SingleInputDialog)
        return new SingleInputDialogItem((FreeLibSet.RI.SingleInputDialog)riItem);
      if (riItem is FreeLibSet.RI.DoubleInputDialog)
        return new DoubleInputDialogItem((FreeLibSet.RI.DoubleInputDialog)riItem);
      if (riItem is FreeLibSet.RI.DecimalInputDialog)
        return new DecimalInputDialogItem((FreeLibSet.RI.DecimalInputDialog)riItem);
      if (riItem is FreeLibSet.RI.DateTimeInputDialog)
        return new DateTimeInputDialogItem((FreeLibSet.RI.DateTimeInputDialog)riItem);
      if (riItem is FreeLibSet.RI.MultiLineTextInputDialog)
        return new MultiLineTextInputDialogItem((FreeLibSet.RI.MultiLineTextInputDialog)riItem);

      #endregion

      #region Диапазон значений

      if (riItem is FreeLibSet.RI.IntRangeDialog)
        return new IntRangeDialogItem((FreeLibSet.RI.IntRangeDialog)riItem);
      if (riItem is FreeLibSet.RI.SingleRangeDialog)
        return new SingleRangeDialogItem((FreeLibSet.RI.SingleRangeDialog)riItem);
      if (riItem is FreeLibSet.RI.DoubleRangeDialog)
        return new DoubleRangeDialogItem((FreeLibSet.RI.DoubleRangeDialog)riItem);
      if (riItem is FreeLibSet.RI.DecimalRangeDialog)
        return new DecimalRangeDialogItem((FreeLibSet.RI.DecimalRangeDialog)riItem);
      if (riItem is FreeLibSet.RI.DateRangeDialog)
        return new DateRangeDialogItem((FreeLibSet.RI.DateRangeDialog)riItem);

      #endregion

      #region Диалоги выбора из списка

      if (riItem is FreeLibSet.RI.ListSelectDialog)
        return new ListSelectDialogItem((FreeLibSet.RI.ListSelectDialog)riItem);
      if (riItem is FreeLibSet.RI.RadioSelectDialog)
        return new RadioSelectDialogItem((FreeLibSet.RI.RadioSelectDialog)riItem);

      #endregion

      #region Ввод табличных данных

      if (riItem is FreeLibSet.RI.InputDataGridDialog)
        return new InputGridDataDialogItem((FreeLibSet.RI.InputDataGridDialog)riItem);

      #endregion

      return null;
    }

    #endregion

    #region FolderBrowserDialog

    private class FolderBrowserDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public FolderBrowserDialogItem(FolderBrowserDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new System.Windows.Forms.FolderBrowserDialog();
        _WinDlg.ShowNewFolderButton = riDialog.ShowNewFolderButton;
        _WinDlg.Description = riDialog.Title;
      }

      #endregion

      #region Свойства

      private FolderBrowserDialog _RIDialog;
      private System.Windows.Forms.FolderBrowserDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.SelectedPath = _RIDialog.Path.Path;
      }

      public void ReadValues()
      {
        _RIDialog.Path = new FreeLibSet.IO.AbsPath(_WinDlg.SelectedPath);
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog(EFPApp.DialogOwnerWindow);
      }

      #endregion
    }

    #endregion

    #region FileDialog

    private class FileDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public FileDialogItem(FileDialog riDialog, bool isSave)
      {
        _RIDialog = riDialog;
        if (isSave)
          _WinDlg = new System.Windows.Forms.SaveFileDialog();
        else
          _WinDlg = new System.Windows.Forms.OpenFileDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Filter = riDialog.Filter;
        _WinDlg.DefaultExt = riDialog.DefaultExt;
      }

      #endregion

      #region Свойства

      private FileDialog _RIDialog;
      private System.Windows.Forms.FileDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.FileName = _RIDialog.Path.Path;
      }

      public void ReadValues()
      {
        _RIDialog.Path = new FreeLibSet.IO.AbsPath(_WinDlg.FileName);
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog(EFPApp.DialogOwnerWindow);
      }

      #endregion
    }

    #endregion

    #region Диалоги ввода значений

    private class TextInputDialogItem: IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public TextInputDialogItem(FreeLibSet.RI.TextInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.TextInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;

        _WinDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
        _WinDlg.MaxLength = riDialog.MaxLength;
        //_WinDlg.IsPassword = riDialog.IsPassword;

        _WinDlg.Text = riDialog.Text; // обязательное присвоение, иначе свойство обнулится
        if (riDialog.InternalTextExConnected)
          riDialog.TextEx = _WinDlg.TextEx;


        if (riDialog.HasValidators)
          _WinDlg.Validators.AddRange(riDialog.Validators);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.TextInputDialog _RIDialog;
      private FreeLibSet.Forms.TextInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Text = _RIDialog.Text;
      }

      public void ReadValues()
      {
        _RIDialog.Text = _WinDlg.Text;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class PasswordInputDialogItem: IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public PasswordInputDialogItem(FreeLibSet.RI.PasswordInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.TextInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.IsPassword = true;

        _WinDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
        //_WinDlg.IsPassword = riDialog.IsPassword;

        _WinDlg.Text = riDialog.Text; // обязательное присвоение, иначе свойство обнулится
        if (riDialog.InternalTextExConnected)
          riDialog.TextEx = _WinDlg.TextEx;


        if (riDialog.HasValidators)
          _WinDlg.Validators.AddRange(riDialog.Validators);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.PasswordInputDialog _RIDialog;
      private FreeLibSet.Forms.TextInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Text = _RIDialog.Text;
      }

      public void ReadValues()
      {
        _RIDialog.Text = _WinDlg.Text;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class TextComboInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public TextComboInputDialogItem(FreeLibSet.RI.TextComboInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.TextComboInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.Items = riDialog.Items;
        _WinDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
        _WinDlg.MaxLength = riDialog.MaxLength;
        _WinDlg.Text = riDialog.Text; // обязательное присвоение, иначе свойство обнулится
        if (riDialog.InternalTextExConnected)
          riDialog.TextEx = _WinDlg.TextEx;

        if (riDialog.HasValidators)
          _WinDlg.Validators.AddRange(riDialog.Validators);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.TextComboInputDialog _RIDialog;
      private FreeLibSet.Forms.TextComboInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Text = _RIDialog.Text;
      }

      public void ReadValues()
      {
        _RIDialog.Text = _WinDlg.Text;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class IntInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public IntInputDialogItem(FreeLibSet.RI.IntInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.IntInputDialog();
        InitNumInputDialog<Int32>(_WinDlg, riDialog);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.IntInputDialog _RIDialog;
      private FreeLibSet.Forms.IntInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NValue = _RIDialog.NValue;
      }

      public void ReadValues()
      {
        _RIDialog.NValue = _WinDlg.NValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private static void InitNumInputDialog<T>(FreeLibSet.Forms.BaseNumInputDialog<T> winDlg, FreeLibSet.RI.BaseNumInputDialog<T> riDialog)
        where T : struct, IFormattable, IComparable<T>
    {
      winDlg.Title = riDialog.Title;
      winDlg.Prompt = riDialog.Prompt;
      winDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
      winDlg.Minimum = riDialog.Minimum;
      winDlg.Maximum = riDialog.Maximum;
      winDlg.Format = riDialog.Format;
      winDlg.UpDownHandler = riDialog.UpDownHandler;
      winDlg.NValue = riDialog.NValue; // обязательное присвоение, иначе свойство обнулится
      if (riDialog.InternalNValueExConnected)
        riDialog.NValueEx = winDlg.NValueEx;
      if (riDialog.InternalValueExConnected)
        riDialog.ValueEx = winDlg.ValueEx;

      if (riDialog.HasValidators)
        winDlg.Validators.AddRange(riDialog.Validators);
    }

    private class SingleInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public SingleInputDialogItem(FreeLibSet.RI.SingleInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.SingleInputDialog();
        InitNumInputDialog<Single>(_WinDlg, riDialog);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.SingleInputDialog _RIDialog;
      private FreeLibSet.Forms.SingleInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NValue = _RIDialog.NValue;
      }

      public void ReadValues()
      {
        _RIDialog.NValue = _WinDlg.NValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class DoubleInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public DoubleInputDialogItem(FreeLibSet.RI.DoubleInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.DoubleInputDialog();
        InitNumInputDialog<Double>(_WinDlg, riDialog);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.DoubleInputDialog _RIDialog;
      private FreeLibSet.Forms.DoubleInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NValue = _RIDialog.NValue;
      }

      public void ReadValues()
      {
        _RIDialog.NValue = _WinDlg.NValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class DecimalInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public DecimalInputDialogItem(FreeLibSet.RI.DecimalInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.DecimalInputDialog();
        InitNumInputDialog<Decimal>(_WinDlg, riDialog);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.DecimalInputDialog _RIDialog;
      private FreeLibSet.Forms.DecimalInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NValue = _RIDialog.NValue;
      }

      public void ReadValues()
      {
        _RIDialog.NValue = _WinDlg.NValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class DateTimeInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public DateTimeInputDialogItem(FreeLibSet.RI.DateTimeInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.DateTimeInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.Kind = riDialog.Kind;
        _WinDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
        _WinDlg.Minimum = riDialog.Minimum;
        _WinDlg.Maximum = riDialog.Maximum;
        _WinDlg.UseCalendar = riDialog.UseCalendar;
        _WinDlg.NValue = riDialog.NValue; // обязательное присвоение, иначе свойство обнулится
        if (riDialog.InternalNValueExConnected)
          riDialog.NValueEx = _WinDlg.NValueEx;
        if (riDialog.InternalValueExConnected)
          riDialog.ValueEx = _WinDlg.ValueEx;
        if (riDialog.InternalNTimeExConnected)
          riDialog.NTimeEx = _WinDlg.NTimeEx;
        if (riDialog.InternalTimeExConnected)
          riDialog.TimeEx = _WinDlg.TimeEx;

        if (riDialog.HasValidators)
          _WinDlg.Validators.AddRange(riDialog.Validators);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.DateTimeInputDialog _RIDialog;
      private FreeLibSet.Forms.DateTimeInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NValue = _RIDialog.NValue;
      }

      public void ReadValues()
      {
        _RIDialog.NValue = _WinDlg.NValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class MultiLineTextInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public MultiLineTextInputDialogItem(FreeLibSet.RI.MultiLineTextInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.MultiLineTextInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.ReadOnly = riDialog.ReadOnly;
        _WinDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
        _WinDlg.Maximized = riDialog.Maximized;
        _WinDlg.Lines = riDialog.Lines; // обязательное присвоение, иначе свойство обнулится
        if (riDialog.InternalLinesExConnected)
          riDialog.LinesEx = _WinDlg.LinesEx;
        if (riDialog.InternalTextExConnected)
          riDialog.TextEx = _WinDlg.TextEx;

        if (riDialog.HasValidators)
          _WinDlg.Validators.AddRange(riDialog.Validators);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.MultiLineTextInputDialog _RIDialog;
      private FreeLibSet.Forms.MultiLineTextInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Text = _RIDialog.Text;
      }

      public void ReadValues()
      {
        _RIDialog.Text = _WinDlg.Text;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    #region Диалоги ввода диапазона значений

    private class IntRangeDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public IntRangeDialogItem(FreeLibSet.RI.IntRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.IntRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.Minimum = riDialog.Minimum;
        _WinDlg.Maximum = riDialog.Maximum;
        _WinDlg.Format = riDialog.Format;
        _WinDlg.UpDownHandler = riDialog.UpDownHandler;
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.IntRangeDialog _RIDialog;
      private FreeLibSet.Forms.IntRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NFirstValue = _RIDialog.NFirstValue;
        _WinDlg.NLastValue = _RIDialog.NLastValue;
      }

      public void ReadValues()
      {
        _RIDialog.NFirstValue = _WinDlg.NFirstValue;
        _RIDialog.NLastValue = _WinDlg.NLastValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private static void InitNumRangeDialog<T>(FreeLibSet.Forms.BaseNumRangeDialog<T> winDlg, FreeLibSet.RI.BaseNumRangeDialog<T> riDialog)
    where T : struct, IFormattable, IComparable<T>
    {
      winDlg.Title = riDialog.Title;
      winDlg.Prompt = riDialog.Prompt;
      winDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
      winDlg.Minimum = riDialog.Minimum;
      winDlg.Maximum = riDialog.Maximum;
      winDlg.Format = riDialog.Format;
      winDlg.UpDownHandler = riDialog.UpDownHandler;
      winDlg.NFirstValue = riDialog.NFirstValue; // обязательное присвоение, иначе свойство обнулится
      if (riDialog.InternalNFirstValueExConnected)
        riDialog.NFirstValueEx = winDlg.NFirstValueEx;
      if (riDialog.InternalFirstValueExConnected)
        riDialog.FirstValueEx = winDlg.FirstValueEx;
      winDlg.NLastValue = riDialog.NLastValue; // обязательное присвоение, иначе свойство обнулится
      if (riDialog.InternalNLastValueExConnected)
        riDialog.NLastValueEx = winDlg.NLastValueEx;
      if (riDialog.InternalLastValueExConnected)
        riDialog.LastValueEx = winDlg.LastValueEx;

      if (riDialog.HasValidators)
        winDlg.Validators.AddRange(riDialog.Validators);
    }



    private class SingleRangeDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public SingleRangeDialogItem(FreeLibSet.RI.SingleRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.SingleRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
        _WinDlg.Minimum = riDialog.Minimum;
        _WinDlg.Maximum = riDialog.Maximum;
        _WinDlg.Format = riDialog.Format;
        _WinDlg.UpDownHandler = riDialog.UpDownHandler;
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.SingleRangeDialog _RIDialog;
      private FreeLibSet.Forms.SingleRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NFirstValue = _RIDialog.NFirstValue;
        _WinDlg.NLastValue = _RIDialog.NLastValue;
      }

      public void ReadValues()
      {
        _RIDialog.NFirstValue = _WinDlg.NFirstValue;
        _RIDialog.NLastValue = _WinDlg.NLastValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class DoubleRangeDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public DoubleRangeDialogItem(FreeLibSet.RI.DoubleRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.DoubleRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
        _WinDlg.Minimum = riDialog.Minimum;
        _WinDlg.Maximum = riDialog.Maximum;
        _WinDlg.Format = riDialog.Format;
        _WinDlg.UpDownHandler = riDialog.UpDownHandler;
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.DoubleRangeDialog _RIDialog;
      private FreeLibSet.Forms.DoubleRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NFirstValue = _RIDialog.NFirstValue;
        _WinDlg.NLastValue = _RIDialog.NLastValue;
      }

      public void ReadValues()
      {
        _RIDialog.NFirstValue = _WinDlg.NFirstValue;
        _RIDialog.NLastValue = _WinDlg.NLastValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class DecimalRangeDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public DecimalRangeDialogItem(FreeLibSet.RI.DecimalRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.DecimalRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
        _WinDlg.Minimum = riDialog.Minimum;
        _WinDlg.Maximum = riDialog.Maximum;
        _WinDlg.Format = riDialog.Format;
        _WinDlg.UpDownHandler = riDialog.UpDownHandler;
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.DecimalRangeDialog _RIDialog;
      private FreeLibSet.Forms.DecimalRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NFirstValue = _RIDialog.NFirstValue;
        _WinDlg.NLastValue = _RIDialog.NLastValue;
      }

      public void ReadValues()
      {
        _RIDialog.NFirstValue = _WinDlg.NFirstValue;
        _RIDialog.NLastValue = _WinDlg.NLastValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class DateRangeDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public DateRangeDialogItem(FreeLibSet.RI.DateRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.DateRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmptyMode = riDialog.CanBeEmptyMode;
        _WinDlg.Minimum = riDialog.Minimum;
        _WinDlg.Maximum = riDialog.Maximum;

        _WinDlg.NFirstDate = riDialog.NFirstDate; // обязательное присвоение, иначе свойство обнулится
        if (riDialog.InternalNFirstDateExConnected)
          riDialog.NFirstDateEx = _WinDlg.NFirstDateEx;
        if (riDialog.InternalFirstDateExConnected)
          riDialog.FirstDateEx = _WinDlg.FirstDateEx;
        _WinDlg.NLastDate = riDialog.NLastDate; // обязательное присвоение, иначе свойство обнулится
        if (riDialog.InternalNLastDateExConnected)
          riDialog.NLastDateEx = _WinDlg.NLastDateEx;
        if (riDialog.InternalLastDateExConnected)
          riDialog.LastDateEx = _WinDlg.LastDateEx;

        if (riDialog.HasValidators)
          _WinDlg.Validators.AddRange(riDialog.Validators);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.DateRangeDialog _RIDialog;
      private FreeLibSet.Forms.DateRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NFirstDate = _RIDialog.NFirstDate;
        _WinDlg.NLastDate = _RIDialog.NLastDate;
      }

      public void ReadValues()
      {
        _RIDialog.NFirstDate = _WinDlg.NFirstDate;
        _RIDialog.NLastDate = _WinDlg.NLastDate;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    #endregion

    #endregion

    #region Диалоги выбора из списка

    private class ListSelectDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public ListSelectDialogItem(FreeLibSet.RI.ListSelectDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.ListSelectDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.MultiSelect = riDialog.MultiSelect;
        _WinDlg.Items = riDialog.Items;
        _WinDlg.ListTitle = riDialog.ListTitle;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.SubItems = riDialog.SubItems;
        _WinDlg.ClipboardMode = (ListSelectDialogClipboardMode)(int)(riDialog.ClipboardMode);
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.ListSelectDialog _RIDialog;
      private FreeLibSet.Forms.ListSelectDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        if (_RIDialog.MultiSelect)
          _WinDlg.Selections = _RIDialog.Selections;
        _WinDlg.SelectedIndex = _RIDialog.SelectedIndex;
      }

      public void ReadValues()
      {
        if (_RIDialog.MultiSelect)
          _RIDialog.Selections = _WinDlg.Selections;
        _RIDialog.SelectedIndex = _WinDlg.SelectedIndex;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class RadioSelectDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public RadioSelectDialogItem(FreeLibSet.RI.RadioSelectDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.RadioSelectDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Items = riDialog.Items;
        _WinDlg.GroupTitle = riDialog.GroupTitle;
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.RadioSelectDialog _RIDialog;
      private FreeLibSet.Forms.RadioSelectDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.SelectedIndex = _RIDialog.SelectedIndex;
      }

      public void ReadValues()
      {
        _RIDialog.SelectedIndex = _WinDlg.SelectedIndex;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    #endregion

    #region Ввод табличных данных

    private class InputGridDataDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public InputGridDataDialogItem(FreeLibSet.RI.InputDataGridDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new FreeLibSet.Forms.InputDataGridDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.FixedRows = riDialog.FixedRows;
        _WinDlg.ReadOnly = riDialog.ReadOnly;
        _WinDlg.InfoText = riDialog.InfoText;
      }

      #endregion

      #region Свойства

      private FreeLibSet.RI.InputDataGridDialog _RIDialog;
      private FreeLibSet.Forms.InputDataGridDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Table = _RIDialog.Table;
      }

      public void ReadValues()
      {
        //_RIDialog.DataSource = _WinDlg.DataSource;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    #endregion
  }
}
