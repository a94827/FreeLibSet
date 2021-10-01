using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.RI;
using System.Threading;
using AgeyevAV.Config;
using AgeyevAV.Logging;
using AgeyevAV.DependedValues;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace AgeyevAV.ExtForms.RI
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
  /// Интерфейс формы
  /// </summary>
  public interface IEFPAppRIFormItem : IEFPAppRIItem
  {
    /// <summary>
    /// Провайдер формы
    /// </summary>
    EFPFormProvider FormProvider { get;}
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
  /// Реализация свойства EFPApp.RICreators
  /// </summary>
  public sealed class EFPAppRICreators : List<IEFPAppRICreator>
  {
    #region Методы

    /// <summary>
    /// Создание IEFPAppRIItem для выбранной RIItem.
    /// Если в списке нет создателя для данного типа класса, генерируется исключение
    /// </summary>
    /// <param name="item">Описатель объектп удаленного интерфейса</param>
    /// <param name="baseProvider">Провайдер для подключения</param>
    /// <returns>Интерфейс управляющего элемента</returns>
    public IEFPAppRIItem Create(RIItem item, EFPBaseProvider baseProvider)
    {
      for (int i = 0; i < Count; i++)
      {
        IEFPAppRIItem res = this[i].Create(item, baseProvider);
        if (res != null)
          return res;
      }

      throw new ArgumentException("В списке EFPApp.RICreators не один из объектов не смог создать управляющий элемент для элемента удаленного интерфейса типа " + item.GetType().ToString());
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
        _ControlProvider.Validating += new EFPValidatingEventHandler(ControlProvider_Validating);
        if (riItem.HasEnabledExProperty)
        {
          if (riItem.EnabledEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            controlProvider.EnabledEx = riItem.EnabledEx;
          else
          {
            controlProvider.Enabled = _RIItem.Enabled; // обязательное присвоение, иначе свойство обнулится
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

      void ControlProvider_Validating(object sender, EFPValidatingEventArgs args)
      {
        if (args.ValidateState == EFPValidateState.Error)
          return;

        // Не препятствуем закрытию окна, если проверка вызвана при закрытии формы
        if (_ControlProvider.BaseProvider.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
          return;

        if (!String.IsNullOrEmpty(_RIItem.ErrorMessage))
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

      new ControlAdapter(controlProvider, riItem);
    }

    #endregion
  }

  internal class RIControlCreator : IEFPAppRICreator
  {
    #region IEFPAppRICreator Members

    public IEFPAppRIItem Create(RIItem riItem, EFPBaseProvider baseProvider)
    {
      if (riItem is AgeyevAV.RI.Label)
        return new LabelItem((AgeyevAV.RI.Label)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.TextBox)
        return new TextBoxItem((AgeyevAV.RI.TextBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.IntEditBox)
      {
        if (((AgeyevAV.RI.IntEditBox)riItem).ShowUpDown)
          return new IntEditBoxItem2((AgeyevAV.RI.IntEditBox)riItem, baseProvider);
        else
          return new IntEditBoxItem1((AgeyevAV.RI.IntEditBox)riItem, baseProvider);
      }
      if (riItem is AgeyevAV.RI.SingleEditBox)
        return new SingleEditBoxItem((AgeyevAV.RI.SingleEditBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.DoubleEditBox)
        return new DoubleEditBoxItem((AgeyevAV.RI.DoubleEditBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.DecimalEditBox)
        return new DecimalEditBoxItem((AgeyevAV.RI.DecimalEditBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.DateBox)
        return new DateBoxItem((AgeyevAV.RI.DateBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.DateRangeBox)
        return new DateRangeBoxItem((AgeyevAV.RI.DateRangeBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.DateOrRangeBox)
        return new DateOrRangeBoxItem((AgeyevAV.RI.DateOrRangeBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.YearMonthBox)
        return new YearMonthBoxItem((AgeyevAV.RI.YearMonthBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.YearMonthRangeBox)
        return new YearMonthRangeBoxItem((AgeyevAV.RI.YearMonthRangeBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.CheckBox)
        return new CheckBoxItem((AgeyevAV.RI.CheckBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.RadioGroup)
        return new RadioGroupItem((AgeyevAV.RI.RadioGroup)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.InfoLabel)
        return new InfoLabelItem((AgeyevAV.RI.InfoLabel)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.ListComboBox)
        return new ListComboBoxItem((AgeyevAV.RI.ListComboBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.TextComboBox)
        return new TextComboBoxItem((AgeyevAV.RI.TextComboBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.CsvCodesComboBox)
        return new CsvCodesComboBoxItem((AgeyevAV.RI.CsvCodesComboBox)riItem, baseProvider);

      if (riItem is AgeyevAV.RI.FolderBrowserTextBox)
        return new FolderBrowserTextBoxItem((AgeyevAV.RI.FolderBrowserTextBox)riItem, baseProvider);
      if (riItem is AgeyevAV.RI.OpenFileTextBox)
        return new FileTextBoxItem((AgeyevAV.RI.OpenFileTextBox)riItem, baseProvider, AgeyevAV.ExtForms.FileDialogMode.Read);
      if (riItem is AgeyevAV.RI.SaveFileTextBox)
        return new FileTextBoxItem((AgeyevAV.RI.SaveFileTextBox)riItem, baseProvider, AgeyevAV.ExtForms.FileDialogMode.Write);

      return null;
    }

    #endregion

    #region Label

    private class LabelItem : AgeyevAV.ExtForms.EFPLabel, IEFPAppRIItem
    {
      #region Конструктор

      public LabelItem(AgeyevAV.RI.Label riItem, EFPBaseProvider baseProvider)
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

    private class TextBoxItem : AgeyevAV.ExtForms.EFPTextBox, IEFPAppRIItem
    {
      #region Конструктор

      public TextBoxItem(AgeyevAV.RI.TextBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.TextBox())
      {
        _RIItem = riItem;
        switch (riItem.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: base.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: base.CanBeEmpty = true; base.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: base.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riItem.CanBeEmptyMode.ToString());
        }

        base.MaxLength = riItem.MaxLength;

        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasTextExProperty)
        {
          if (riItem.TextEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.TextEx = riItem.TextEx;
          else
          {
            base.Text = _RIItem.Text; // обязательное присвоение, иначе свойство обнулится
            riItem.TextEx = base.TextEx;
          }
        }

        if (riItem.HasReadOnlyExProperty)
        {
          if (riItem.ReadOnlyEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.ReadOnlyEx = riItem.ReadOnlyEx;
          else
          {
            base.ReadOnly = _RIItem.ReadOnly; // обязательное присвоение, иначе свойство обнулится
            riItem.ReadOnlyEx = base.ReadOnlyEx;
          }
        }

        base.ErrorRegExPattern = riItem.ErrorRegExPattern;
        base.ErrorRegExMessage = riItem.ErrorRegExMessage;
        base.WarningRegExPattern = riItem.WarningRegExPattern;
        base.WarningRegExMessage = riItem.WarningRegExMessage;
      }

      AgeyevAV.RI.TextBox _RIItem;

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
    private class IntEditBoxItem1 : AgeyevAV.ExtForms.EFPNumEditBox, IEFPAppRIItem
    {
      #region Конструктор

      public IntEditBoxItem1(AgeyevAV.RI.IntEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.NumEditBox())
      {
        _RIItem = riItem;
        if (riItem.Minimum.HasValue)
          base.Minimum = riItem.Minimum.Value;
        if (riItem.Maximum.HasValue)
          base.Maximum = riItem.Maximum.Value;
        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasValueExProperty)
        {
          if (riItem.ValueEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.IntValueEx = riItem.ValueEx;
          else
          {
            base.IntValue = _RIItem.Value; // обязательное присвоение, иначе свойство обнулится
            riItem.ValueEx = base.IntValueEx;
          }
        }
      }

      AgeyevAV.RI.IntEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.IntValue = _RIItem.Value;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Value = base.IntValue;
      }

      #endregion
    }

    /// <summary>
    /// Поле ввода целого числа.
    /// Версия со стрелочками
    /// </summary>
    private class IntEditBoxItem2 : AgeyevAV.ExtForms.EFPExtNumericUpDown, IEFPAppRIItem
    {
      #region Конструктор

      public IntEditBoxItem2(AgeyevAV.RI.IntEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.ExtNumericUpDown())
      {
        base.Control.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Right;
        base.Control.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        _RIItem = riItem;
        if (riItem.Minimum.HasValue)
          base.Minimum = riItem.Minimum.Value;
        else
          base.Minimum = int.MinValue;
        if (riItem.Maximum.HasValue)
          base.Maximum = riItem.Maximum.Value;
        else
          base.Maximum = int.MaxValue;
        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasValueExProperty)
        {
          if (riItem.ValueEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.IntValueEx = riItem.ValueEx;
          else
          {
            base.IntValue = _RIItem.Value; // обязательное присвоение, иначе свойство обнулится
            riItem.ValueEx = base.IntValueEx;
          }
        }
      }

      AgeyevAV.RI.IntEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.IntValue = _RIItem.Value;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Value = base.IntValue;
      }

      #endregion
    }

    private class SingleEditBoxItem : AgeyevAV.ExtForms.EFPNumEditBox, IEFPAppRIItem
    {
      #region Конструктор

      public SingleEditBoxItem(AgeyevAV.RI.SingleEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.NumEditBox())
      {
        _RIItem = riItem;
        base.Control.DecimalPlaces = riItem.DecimalPlaces;
        if (riItem.Minimum.HasValue)
          base.Minimum = (decimal)(riItem.Minimum.Value);
        if (riItem.Maximum.HasValue)
          base.Maximum = (decimal)(riItem.Maximum.Value);
        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasValueExProperty)
        {
          if (riItem.ValueEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SingleValueEx = riItem.ValueEx;
          else
          {
            base.SingleValue = _RIItem.Value; // обязательное присвоение, иначе свойство обнулится
            riItem.ValueEx = base.SingleValueEx;
          }
        }
      }

      AgeyevAV.RI.SingleEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.SingleValue = _RIItem.Value;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Value = base.SingleValue;
      }

      #endregion
    }

    private class DoubleEditBoxItem : AgeyevAV.ExtForms.EFPNumEditBox, IEFPAppRIItem
    {
      #region Конструктор

      public DoubleEditBoxItem(AgeyevAV.RI.DoubleEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.NumEditBox())
      {
        _RIItem = riItem;
        base.Control.DecimalPlaces = riItem.DecimalPlaces;
        if (riItem.Minimum.HasValue)
          base.Minimum = (decimal)(riItem.Minimum.Value);
        if (riItem.Maximum.HasValue)
          base.Maximum = (decimal)(riItem.Maximum.Value);
        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasValueExProperty)
        {
          if (riItem.ValueEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.DoubleValueEx = riItem.ValueEx;
          else
          {
            base.DoubleValue = _RIItem.Value; // обязательное присвоение, иначе свойство обнулится
            riItem.ValueEx = base.DoubleValueEx;
          }
        }
      }

      AgeyevAV.RI.DoubleEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.DoubleValue = _RIItem.Value;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Value = base.DoubleValue;
      }

      #endregion
    }

    private class DecimalEditBoxItem : AgeyevAV.ExtForms.EFPNumEditBox, IEFPAppRIItem
    {
      #region Конструктор

      public DecimalEditBoxItem(AgeyevAV.RI.DecimalEditBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.NumEditBox())
      {
        _RIItem = riItem;
        base.Control.DecimalPlaces = riItem.DecimalPlaces;
        base.Minimum = riItem.Minimum;
        base.Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasValueExProperty)
        {
          if (riItem.ValueEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.DecimalValueEx = riItem.ValueEx;
          else
          {
            base.DecimalValue = _RIItem.Value; // обязательное присвоение, иначе свойство обнулится
            riItem.ValueEx = base.DecimalValueEx;
          }
        }
      }

      AgeyevAV.RI.DecimalEditBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.DecimalValue = _RIItem.Value;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Value = base.DecimalValue;
      }

      #endregion
    }

    #endregion

    #region CheckBox

    private class CheckBoxItem : AgeyevAV.ExtForms.EFPCheckBox, IEFPAppRIItem
    {
      #region Конструктор

      public CheckBoxItem(AgeyevAV.RI.CheckBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.CheckBox())
      {
        Control.ThreeState = riItem.ThreeState;
        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasCheckedExProperty)
        {
          if (riItem.CheckedEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.CheckedEx = riItem.CheckedEx;
          else
          {
            base.Checked = _RIItem.Checked; // обязательное присвоение, иначе свойство обнулится
            riItem.CheckedEx = base.CheckedEx;
          }
        }

        if (riItem.HasCheckStateExProperty)
        {
          if (riItem.CheckStateEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            this.CheckStateEx2 = riItem.CheckStateEx;
          else
          {
            base.CheckState = (System.Windows.Forms.CheckState)(int)(_RIItem.CheckState); // обязательное присвоение, иначе свойство обнулится
            riItem.CheckStateEx = this.CheckStateEx2;
          }
        }
      }

      AgeyevAV.RI.CheckBox _RIItem;

      #endregion

      #region Переопределенное свойство CheckStateEx

      // Перечисления Syatem.Windows.Forms.CheckState и AgeyevAV.RI.CheckState совпадают,
      // но это разные перечисления. Нужен переходник

      public DepValue<AgeyevAV.RI.CheckState> CheckStateEx2
      {
        get
        {
          InitCheckStateEx2();
          return FCheckStateEx2;
        }
        set
        {
          InitCheckStateEx2();
          FCheckStateEx2.Source = value;
        }
      }
      private DepInput<AgeyevAV.RI.CheckState> FCheckStateEx2;

      private void InitCheckStateEx2()
      {
        if (FCheckStateEx2 == null)
        {
          FCheckStateEx2 = new DepInput<AgeyevAV.RI.CheckState>();
          FCheckStateEx2.OwnerInfo = new DepOwnerInfo(this, "CheckStateEx2");
          FCheckStateEx2.Value = (AgeyevAV.RI.CheckState)(int)CheckState;
          FCheckStateEx2.ValueChanged += new EventHandler(FCheckStateEx2_ValueChanged);

          base.CheckStateEx.ValueChanged += new EventHandler(CheckStateEx_ValueChanged);
        }
      }

      void CheckStateEx_ValueChanged(object Sender, EventArgs Args)
      {
        FCheckStateEx2.Value = (AgeyevAV.RI.CheckState)(int)(base.CheckState);
      }

      private void FCheckStateEx2_ValueChanged(object Sender, EventArgs Args)
      {
        base.CheckState = (System.Windows.Forms.CheckState)(int)FCheckStateEx2.Value;
      }

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Control.Text = _RIItem.Text;
        base.CheckState = (System.Windows.Forms.CheckState)(int)_RIItem.CheckState;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.CheckState = (CheckState)(int)base.CheckState;
      }

      #endregion
    }

    #endregion

    #region RadioGroup

    private class RadioGroupItem : AgeyevAV.ExtForms.EFPRadioButtons, IEFPAppRIItem
    {
      #region Конструктор

      public RadioGroupItem(AgeyevAV.RI.RadioGroup riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.RadioGroupBox(riItem.Items))
      {
        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        base.Codes = riItem.Codes;
        //base.UnselectedCode = RIItem.UnselectedCode;
        if (riItem.HasSelectedIndexExProperty)
        {
          if (riItem.SelectedIndexEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedIndexEx = riItem.SelectedIndexEx;
          else
          {
            base.SelectedIndex = _RIItem.SelectedIndex; // обязательное присвоение, иначе свойство обнулится
            riItem.SelectedIndexEx = base.SelectedIndexEx;
          }
        }

        if (riItem.HasSelectedCodeExProperty)
        {
          if (riItem.SelectedCodeEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedCodeEx = riItem.SelectedCodeEx;
          else
          {
            base.SelectedCode = _RIItem.SelectedCode; // обязательное присвоение, иначе свойство обнулится
            riItem.SelectedCodeEx = base.SelectedCodeEx;
          }
        }
      }

      AgeyevAV.RI.RadioGroup _RIItem;

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

    private class DateBoxItem : AgeyevAV.ExtForms.EFPDateBox, IEFPAppRIItem
    {
      #region Конструктор

      public DateBoxItem(AgeyevAV.RI.DateBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.DateBox())
      {
        _RIItem = riItem;

        switch (riItem.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: base.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: base.CanBeEmpty = true; base.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: base.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riItem.CanBeEmptyMode.ToString());
        }

        base.Minimum = riItem.Minimum;
        base.Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasValueExProperty)
        {
          if (riItem.ValueEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.ValueEx = riItem.ValueEx;
          else
          {
            base.Value = _RIItem.Value; // обязательное присвоение, иначе свойство обнулится
            riItem.ValueEx = base.ValueEx;
          }
        }
      }

      AgeyevAV.RI.DateBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Value = _RIItem.Value;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Value = base.Value;
      }

      #endregion
    }

    #endregion

    #region DateRangeBox

    private class DateRangeBoxItem : AgeyevAV.ExtForms.EFPDateRangeBox, IEFPAppRIItem
    {
      #region Конструктор

      public DateRangeBoxItem(AgeyevAV.RI.DateRangeBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.DateRangeBox())
      {
        _RIItem = riItem;
        switch (riItem.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: base.FirstDate.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: base.FirstDate.CanBeEmpty = true; base.FirstDate.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: base.FirstDate.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riItem.CanBeEmptyMode.ToString());
        }
        base.LastDate.CanBeEmpty = base.FirstDate.CanBeEmpty;
        base.LastDate.WarningIfEmpty = base.FirstDate.CanBeEmpty;
        
        base.FirstDate.Minimum = riItem.MinimumFirstDate;
        base.FirstDate.Maximum = riItem.MaximumFirstDate;
        base.LastDate.Minimum = riItem.MinimumLastDate;
        base.LastDate.Maximum = riItem.MaximumLastDate;
        EFPAppRITools.InitControlItem(this, riItem);
      }

      AgeyevAV.RI.DateRangeBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.FirstDate.Value = _RIItem.FirstDate;
        base.LastDate.Value = _RIItem.LastDate;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.FirstDate = base.FirstDate.Value;
        _RIItem.LastDate = base.LastDate.Value;
      }

      #endregion
    }

    #endregion

    #region DateOrRangeBox

    private class DateOrRangeBoxItem : AgeyevAV.ExtForms.EFPDateOrRangeBox, IEFPAppRIItem
    {
      #region Конструктор

      public DateOrRangeBoxItem(AgeyevAV.RI.DateOrRangeBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.UserMaskedComboBox())
      {
        _RIItem = riItem;

        switch (riItem.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: base.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: base.CanBeEmpty = true; base.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: base.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riItem.CanBeEmptyMode.ToString());
        }

        base.Minimum = riItem.Minimum;
        base.Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);
      }

      AgeyevAV.RI.DateOrRangeBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.Value = _RIItem.DateRange;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.DateRange = base.Value;
      }

      #endregion
    }

    #endregion

    #region YearMonthBox

    private class YearMonthBoxItem : AgeyevAV.ExtForms.EFPYearMonthBox, IEFPAppRIItem
    {
      #region Конструктор

      public YearMonthBoxItem(AgeyevAV.RI.YearMonthBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.YearMonthBox())
      {
        _RIItem = riItem;
        Minimum = riItem.Minimum;
        Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);
      }

      AgeyevAV.RI.YearMonthBox _RIItem;

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

    private class YearMonthRangeBoxItem : AgeyevAV.ExtForms.EFPYearMonthRangeBox, IEFPAppRIItem
    {
      #region Конструктор

      public YearMonthRangeBoxItem(AgeyevAV.RI.YearMonthRangeBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.YearMonthRangeBox())
      {
        _RIItem = riItem;
        Minimum = riItem.Minimum;
        Maximum = riItem.Maximum;
        EFPAppRITools.InitControlItem(this, riItem);
      }

      AgeyevAV.RI.YearMonthRangeBox _RIItem;

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

    private class InfoLabelItem : AgeyevAV.ExtForms.EFPInfoLabel, IEFPAppRIItem
    {
      #region Конструктор

      public InfoLabelItem(AgeyevAV.RI.InfoLabel riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.InfoLabel())
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

    private class ListComboBoxItem : AgeyevAV.ExtForms.EFPListComboBox, IEFPAppRIItem
    {
      #region Конструктор

      public ListComboBoxItem(AgeyevAV.RI.ListComboBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.ComboBox())
      {
        base.Control.Items.AddRange(riItem.Items);
        base.CanBeEmpty = false;
        WinFormsTools.SetComboBoxWidth(base.Control);

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        base.Codes = riItem.Codes;
        //base.UnselectedCode = RIItem.UnselectedCode;
        if (riItem.HasSelectedIndexExProperty)
        {
          if (riItem.SelectedIndexEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedIndexEx = riItem.SelectedIndexEx;
          else
          {
            base.SelectedIndex = _RIItem.SelectedIndex; // обязательное присвоение, иначе свойство обнулится
            riItem.SelectedIndexEx = base.SelectedIndexEx;
          }
        }

        if (riItem.HasSelectedCodeExProperty)
        {
          if (riItem.SelectedCodeEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.SelectedCodeEx = riItem.SelectedCodeEx;
          else
          {
            base.SelectedCode = _RIItem.SelectedCode; // обязательное присвоение, иначе свойство обнулится
            riItem.SelectedCodeEx = base.SelectedCodeEx;
          }
        }
      }

      AgeyevAV.RI.ListComboBox _RIItem;

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

    private class TextComboBoxItem : AgeyevAV.ExtForms.EFPTextComboBox, IEFPAppRIItem
    {
      #region Конструктор

      public TextComboBoxItem(AgeyevAV.RI.TextComboBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new System.Windows.Forms.ComboBox())
      {
        base.Control.Items.AddRange(riItem.Items);
        WinFormsTools.SetComboBoxWidth(base.Control);

        switch (riItem.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: base.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: base.CanBeEmpty = true; base.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: base.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riItem.CanBeEmptyMode.ToString());
        }

        base.MaxLength = riItem.MaxLength;

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);

        if (riItem.HasTextExProperty)
        {
          if (riItem.TextEx.Source != null)
            // Анализируем свойство "Source", а присвоение выполняем для самого свойства, т.к. там есть дополнительная обработка
            base.TextEx = riItem.TextEx;
          else
          {
            base.Text = _RIItem.Text; // обязательное присвоение, иначе свойство обнулится
            riItem.TextEx = base.TextEx;
          }
        }

        base.ErrorRegExPattern = riItem.ErrorRegExPattern;
        base.ErrorRegExMessage = riItem.ErrorRegExMessage;
        base.WarningRegExPattern = riItem.WarningRegExPattern;
        base.WarningRegExMessage = riItem.WarningRegExMessage;
      }

      AgeyevAV.RI.TextComboBox _RIItem;

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

    private class CsvCodesComboBoxItem : AgeyevAV.ExtForms.EFPCsvCodesComboBox, IEFPAppRIItem
    {
      #region Конструктор

      public CsvCodesComboBoxItem(AgeyevAV.RI.CsvCodesComboBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider, new AgeyevAV.ExtForms.UserTextComboBox(), riItem.Codes)
      {
        switch (riItem.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: base.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: base.CanBeEmpty = true; base.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: base.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riItem.CanBeEmptyMode.ToString());
        }

        base.Names = riItem.Names;

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);
      }

      AgeyevAV.RI.CsvCodesComboBox _RIItem;

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


    private class FolderBrowserTextBoxItem : EFPTextBoxWithButton, IEFPAppRIItem
    {
      #region Конструктор

      public FolderBrowserTextBoxItem(AgeyevAV.RI.FolderBrowserTextBox riItem, EFPBaseProvider baseProvider)
        : base(baseProvider)
      {
        switch (riItem.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: base.TheTextBox.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: base.TheTextBox.CanBeEmpty = true; base.TheTextBox.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: base.TheTextBox.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riItem.CanBeEmptyMode.ToString());
        }

        TheButton = new EFPFolderBrowserButton(TheTextBox, Control.TheButton);
        TheButton.Description = riItem.Description;
        TheButton.ShowNewFolderButton = riItem.ShowNewFolderButton;
        TheButton.PathValidateMode = riItem.PathValidateMode;

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);
      }

      EFPFolderBrowserButton TheButton;

      AgeyevAV.RI.FolderBrowserTextBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.TheTextBox.Text = _RIItem.Path.SlashedPath;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Path = new AgeyevAV.IO.AbsPath(base.TheTextBox.Text);
      }

      #endregion
    }

    private class FileTextBoxItem : EFPTextBoxWithButton, IEFPAppRIItem
    {
      #region Конструктор

      public FileTextBoxItem(AgeyevAV.RI.FileTextBox riItem, EFPBaseProvider baseProvider, AgeyevAV.ExtForms.FileDialogMode mode)
        : base(baseProvider)
      {
        switch (riItem.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: base.TheTextBox.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: base.TheTextBox.CanBeEmpty = true; base.TheTextBox.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: base.TheTextBox.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riItem.CanBeEmptyMode.ToString());
        }

        TheButton = new EFPFileDialogButton(TheTextBox, Control.TheButton);
        TheButton.Mode = mode;
        TheButton.Filter = riItem.Filter;
        TheButton.PathValidateMode = riItem.PathValidateMode;

        _RIItem = riItem;
        EFPAppRITools.InitControlItem(this, riItem);
      }

      EFPFileDialogButton TheButton;

      AgeyevAV.RI.FileTextBox _RIItem;

      #endregion

      #region IEFPAppRIItem Members

      public void WriteValues()
      {
        base.TheTextBox.Text = _RIItem.Path.Path;
        base.Validate();
      }

      public void ReadValues()
      {
        _RIItem.Path = new AgeyevAV.IO.AbsPath(base.TheTextBox.Text);
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

      if (riItem is AgeyevAV.RI.TextInputDialog)
        return new TextInputDialogItem((AgeyevAV.RI.TextInputDialog)riItem);
      if (riItem is AgeyevAV.RI.ComboTextInputDialog)
        return new ComboTextInputDialogItem((AgeyevAV.RI.ComboTextInputDialog)riItem);
      if (riItem is AgeyevAV.RI.IntInputDialog)
        return new IntInputDialogItem((AgeyevAV.RI.IntInputDialog)riItem);
      if (riItem is AgeyevAV.RI.SingleInputDialog)
        return new SingleInputDialogItem((AgeyevAV.RI.SingleInputDialog)riItem);
      if (riItem is AgeyevAV.RI.DoubleInputDialog)
        return new DoubleInputDialogItem((AgeyevAV.RI.DoubleInputDialog)riItem);
      if (riItem is AgeyevAV.RI.DecimalInputDialog)
        return new DecimalInputDialogItem((AgeyevAV.RI.DecimalInputDialog)riItem);
      if (riItem is AgeyevAV.RI.DateInputDialog)
        return new DateInputDialogItem((AgeyevAV.RI.DateInputDialog)riItem);
      if (riItem is AgeyevAV.RI.MultiLineTextInputDialog)
        return new MultiLineTextInputDialogItem((AgeyevAV.RI.MultiLineTextInputDialog)riItem);

      #endregion

      #region Диапазон значений

      if (riItem is AgeyevAV.RI.IntRangeDialog)
        return new IntRangeDialogItem((AgeyevAV.RI.IntRangeDialog)riItem);
      if (riItem is AgeyevAV.RI.SingleRangeDialog)
        return new SingleRangeDialogItem((AgeyevAV.RI.SingleRangeDialog)riItem);
      if (riItem is AgeyevAV.RI.DoubleRangeDialog)
        return new DoubleRangeDialogItem((AgeyevAV.RI.DoubleRangeDialog)riItem);
      if (riItem is AgeyevAV.RI.DecimalRangeDialog)
        return new DecimalRangeDialogItem((AgeyevAV.RI.DecimalRangeDialog)riItem);
      if (riItem is AgeyevAV.RI.DateRangeDialog)
        return new DateRangeDialogItem((AgeyevAV.RI.DateRangeDialog)riItem);

      #endregion

      #region Диалоги выбора из списка

      if (riItem is AgeyevAV.RI.ListSelectDialog)
        return new ListSelectDialogItem((AgeyevAV.RI.ListSelectDialog)riItem);
      if (riItem is AgeyevAV.RI.RadioSelectDialog)
        return new RadioSelectDialogItem((AgeyevAV.RI.RadioSelectDialog)riItem);

      #endregion

      #region Ввод табличных данных

      if (riItem is AgeyevAV.RI.InputGridDataDialog)
        return new InputGridDataDialogItem((AgeyevAV.RI.InputGridDataDialog)riItem);

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
        _RIDialog.Path = new AgeyevAV.IO.AbsPath(_WinDlg.SelectedPath);
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
        _RIDialog.Path = new AgeyevAV.IO.AbsPath(_WinDlg.FileName);
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog(EFPApp.DialogOwnerWindow);
      }

      #endregion
    }

    #endregion

    #region Диалоги ввода значений

    private class TextInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public TextInputDialogItem(AgeyevAV.RI.TextInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.TextInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;

        switch (riDialog.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: _WinDlg.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: _WinDlg.CanBeEmpty = true; _WinDlg.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: _WinDlg.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riDialog.CanBeEmptyMode.ToString());
        }

        _WinDlg.MaxLength = riDialog.MaxLength;
        _WinDlg.IsPassword = riDialog.IsPassword;

        _WinDlg.ErrorRegExPattern = riDialog.ErrorRegExPattern;
        _WinDlg.ErrorRegExMessage = riDialog.ErrorRegExMessage;
        _WinDlg.WarningRegExPattern = riDialog.WarningRegExPattern;
        _WinDlg.WarningRegExMessage = riDialog.WarningRegExMessage;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.TextInputDialog _RIDialog;
      private AgeyevAV.ExtForms.TextInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Value = _RIDialog.Value;
      }

      public void ReadValues()
      {
        _RIDialog.Value = _WinDlg.Value;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class ComboTextInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public ComboTextInputDialogItem(AgeyevAV.RI.ComboTextInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.ComboTextInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;

        switch (riDialog.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: _WinDlg.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: _WinDlg.CanBeEmpty = true; _WinDlg.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: _WinDlg.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riDialog.CanBeEmptyMode.ToString());
        }

        _WinDlg.Items = riDialog.Items;
        _WinDlg.MaxLength = riDialog.MaxLength;

        _WinDlg.ErrorRegExPattern = riDialog.ErrorRegExPattern;
        _WinDlg.ErrorRegExMessage = riDialog.ErrorRegExMessage;
        _WinDlg.WarningRegExPattern = riDialog.WarningRegExPattern;
        _WinDlg.WarningRegExMessage = riDialog.WarningRegExMessage;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.ComboTextInputDialog _RIDialog;
      private AgeyevAV.ExtForms.ComboTextInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Value = _RIDialog.Value;
      }

      public void ReadValues()
      {
        _RIDialog.Value = _WinDlg.Value;
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

      public IntInputDialogItem(AgeyevAV.RI.IntInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.IntInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
        _WinDlg.ShowUpDown = riDialog.ShowUpDown;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.IntInputDialog _RIDialog;
      private AgeyevAV.ExtForms.IntInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NullableValue = _RIDialog.NullableValue;
      }

      public void ReadValues()
      {
        _RIDialog.NullableValue = _WinDlg.NullableValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class SingleInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public SingleInputDialogItem(AgeyevAV.RI.SingleInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.SingleInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.SingleInputDialog _RIDialog;
      private AgeyevAV.ExtForms.SingleInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NullableValue = _RIDialog.NullableValue;
      }

      public void ReadValues()
      {
        _RIDialog.NullableValue = _WinDlg.NullableValue;
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

      public DoubleInputDialogItem(AgeyevAV.RI.DoubleInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.DoubleInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.DoubleInputDialog _RIDialog;
      private AgeyevAV.ExtForms.DoubleInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NullableValue = _RIDialog.NullableValue;
      }

      public void ReadValues()
      {
        _RIDialog.NullableValue = _WinDlg.NullableValue;
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

      public DecimalInputDialogItem(AgeyevAV.RI.DecimalInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.DecimalInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.DecimalInputDialog _RIDialog;
      private AgeyevAV.ExtForms.DecimalInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NullableValue = _RIDialog.NullableValue;
      }

      public void ReadValues()
      {
        _RIDialog.NullableValue = _WinDlg.NullableValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class DateInputDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public DateInputDialogItem(AgeyevAV.RI.DateInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.DateInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;

        switch (riDialog.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: _WinDlg.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: _WinDlg.CanBeEmpty = true; _WinDlg.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: _WinDlg.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riDialog.CanBeEmptyMode.ToString());
        }

        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
        _WinDlg.UseCalendar = riDialog.UseCalendar;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.DateInputDialog _RIDialog;
      private AgeyevAV.ExtForms.DateInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Value = _RIDialog.Value;
      }

      public void ReadValues()
      {
        _RIDialog.Value = _WinDlg.Value;
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

      public MultiLineTextInputDialogItem(AgeyevAV.RI.MultiLineTextInputDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.MultiLineTextInputDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.ReadOnly = riDialog.ReadOnly;

        switch (riDialog.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: _WinDlg.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: _WinDlg.CanBeEmpty = true; _WinDlg.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: _WinDlg.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riDialog.CanBeEmptyMode.ToString());
        }

        _WinDlg.Maximized = riDialog.Maximized;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.MultiLineTextInputDialog _RIDialog;
      private AgeyevAV.ExtForms.MultiLineTextInputDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.Value = _RIDialog.Value;
      }

      public void ReadValues()
      {
        _RIDialog.Value = _WinDlg.Value;
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

      public IntRangeDialogItem(AgeyevAV.RI.IntRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.IntRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
        //WinDlg.ShowUpDown = Dialog.ShowUpDown;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.IntRangeDialog _RIDialog;
      private AgeyevAV.ExtForms.IntRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NullableFirstValue = _RIDialog.NullableFirstValue;
        _WinDlg.NullableLastValue = _RIDialog.NullableLastValue;
      }

      public void ReadValues()
      {
        _RIDialog.NullableFirstValue = _WinDlg.NullableFirstValue;
        _RIDialog.NullableLastValue = _WinDlg.NullableLastValue;
      }

      public DialogResult ShowDialog()
      {
        return (DialogResult)(int)_WinDlg.ShowDialog();
      }

      #endregion
    }

    private class SingleRangeDialogItem : IEFPAppRIStandardDialogItem
    {
      #region Конструктор

      public SingleRangeDialogItem(AgeyevAV.RI.SingleRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.SingleRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.SingleRangeDialog _RIDialog;
      private AgeyevAV.ExtForms.SingleRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NullableFirstValue = _RIDialog.NullableFirstValue;
        _WinDlg.NullableLastValue = _RIDialog.NullableLastValue;
      }

      public void ReadValues()
      {
        _RIDialog.NullableFirstValue = _WinDlg.NullableFirstValue;
        _RIDialog.NullableLastValue = _WinDlg.NullableLastValue;
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

      public DoubleRangeDialogItem(AgeyevAV.RI.DoubleRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.DoubleRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.DoubleRangeDialog _RIDialog;
      private AgeyevAV.ExtForms.DoubleRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NullableFirstValue = _RIDialog.NullableFirstValue;
        _WinDlg.NullableLastValue = _RIDialog.NullableLastValue;
      }

      public void ReadValues()
      {
        _RIDialog.NullableFirstValue = _WinDlg.NullableFirstValue;
        _RIDialog.NullableLastValue = _WinDlg.NullableLastValue;
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

      public DecimalRangeDialogItem(AgeyevAV.RI.DecimalRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.DecimalRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.CanBeEmpty = riDialog.CanBeEmpty;
        _WinDlg.DecimalPlaces = riDialog.DecimalPlaces;
        _WinDlg.MinValue = riDialog.MinValue;
        _WinDlg.MaxValue = riDialog.MaxValue;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.DecimalRangeDialog _RIDialog;
      private AgeyevAV.ExtForms.DecimalRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.NullableFirstValue = _RIDialog.NullableFirstValue;
        _WinDlg.NullableLastValue = _RIDialog.NullableLastValue;
      }

      public void ReadValues()
      {
        _RIDialog.NullableFirstValue = _WinDlg.NullableFirstValue;
        _RIDialog.NullableLastValue = _WinDlg.NullableLastValue;
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

      public DateRangeDialogItem(AgeyevAV.RI.DateRangeDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.DateRangeDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;

        switch (riDialog.CanBeEmptyMode)
        {
          case CanBeEmptyMode.Error: _WinDlg.CanBeEmpty = false; break;
          case CanBeEmptyMode.Warning: _WinDlg.CanBeEmpty = true; _WinDlg.WarningIfEmpty = true; break;
          case CanBeEmptyMode.Ok: _WinDlg.CanBeEmpty = true; break;
          default:
            throw new BugException("CanBeEmptyMode=" + riDialog.CanBeEmptyMode.ToString());
        }

        _WinDlg.Minimum = riDialog.MinValue;
        _WinDlg.Maximum = riDialog.MaxValue;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.DateRangeDialog _RIDialog;
      private AgeyevAV.ExtForms.DateRangeDialog _WinDlg;

      #endregion

      #region Чтение и запись значений

      public void WriteValues()
      {
        _WinDlg.FirstDate = _RIDialog.FirstValue;
        _WinDlg.LastDate = _RIDialog.LastValue;
      }

      public void ReadValues()
      {
        _RIDialog.FirstValue = _WinDlg.FirstDate;
        _RIDialog.LastValue = _WinDlg.LastDate;
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

      public ListSelectDialogItem(AgeyevAV.RI.ListSelectDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.ListSelectDialog();
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

      private AgeyevAV.RI.ListSelectDialog _RIDialog;
      private AgeyevAV.ExtForms.ListSelectDialog _WinDlg;

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

      public RadioSelectDialogItem(AgeyevAV.RI.RadioSelectDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.RadioSelectDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Items = riDialog.Items;
        _WinDlg.GroupTitle = riDialog.GroupTitle;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.RadioSelectDialog _RIDialog;
      private AgeyevAV.ExtForms.RadioSelectDialog _WinDlg;

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

      public InputGridDataDialogItem(AgeyevAV.RI.InputGridDataDialog riDialog)
      {
        _RIDialog = riDialog;
        _WinDlg = new AgeyevAV.ExtForms.InputGridDataDialog();
        _WinDlg.Title = riDialog.Title;
        _WinDlg.Prompt = riDialog.Prompt;
        _WinDlg.FixedRows = riDialog.FixedRows;
        _WinDlg.ReadOnly = riDialog.ReadOnly;
        _WinDlg.InfoText = riDialog.InfoText;
      }

      #endregion

      #region Свойства

      private AgeyevAV.RI.InputGridDataDialog _RIDialog;
      private AgeyevAV.ExtForms.InputGridDataDialog _WinDlg;

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
