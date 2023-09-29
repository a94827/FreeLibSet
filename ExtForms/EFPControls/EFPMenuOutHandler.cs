using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Collections;
using FreeLibSet.Core;

#pragma warning disable 1591

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Описание одной команды "Отправить" для <see cref="EFPMenuOutItem "/>
  /// </summary>
  public sealed class EFPSendToItem : IObjectWithCode
  {
    #region Конструктор

    public EFPSendToItem(string mainCode, string auxCode)
    {
      if (String.IsNullOrEmpty(mainCode))
        throw new ArgumentNullException("mainCode");
      _MainCode = mainCode;
      _AuxCode = auxCode ?? String.Empty;
    }

    public EFPSendToItem(string mainCode)
      :this(mainCode, String.Empty)
    {
    }

    #endregion

      #region Код команды

      /// <summary>
      /// Основной код команды, например, "HTML".
      /// Задается в конструкторе, не может быть пустой строкой
      /// </summary>
    public string MainCode { get { return _MainCode; } }
    private readonly string _MainCode;

    /// <summary>
    /// Дополнительный код команды, например, "1".
    /// Задается в конструкторе. Может быть пустой строкой
    /// </summary>
    public string AuxCode { get { return _AuxCode; } }
    private readonly string _AuxCode;

    /// <summary>
    /// Код команды меню
    /// </summary>
    public string Code
    {
      get
      {
        if (_AuxCode.Length == 0)
          return _MainCode;
        else
          return _MainCode + "_" + _AuxCode;
      }
    }

    /// <summary>
    /// Текстовое представление
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Code;
    }

    /// <summary>
    /// Используется для передачи дополнительных данных в обработчик
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region Свойства команды меню

    /// <summary>
    /// Текст команды меню, например, "Internet Explorer". Обычно берется из файловой ассоциации
    /// </summary>
    public string MenuText
    {
      get { return _MenuText ?? Code; }
      set { _MenuText = value; }
    }
    private string _MenuText;

    /// <summary>
    /// Значок для кнопки и команды меню
    /// </summary>
    public Image Image
    {
      get { return _Image??EFPApp.MainImages.Images["UnknownState"]; }
      set { _Image = value; }
    }
    private Image _Image;

    /// <summary>
    /// Текст всплывающей подсказки
    /// </summary>
    public string ToolTipText
    {
      get
      {
        if (_ToolTipText == null)
        {
          StringBuilder sb = new StringBuilder();
          sb.Append("Отправить ");
          if (!String.IsNullOrEmpty(SubMenuText))
          {
            sb.Append(SubMenuText);
            sb.Append(" ");
          }
          sb.Append("в ");
          sb.Append(MenuText);
          return sb.ToString();
        }
        else
          return _ToolTipText;
      }
      set { _ToolTipText = value; }
    }
    private string _ToolTipText;

    /// <summary>
    /// Текст подменю, если есть несколько команд с разными доп. кодами
    /// </summary>
    public string SubMenuText
    {
      get { return _SubMenuText ?? MainCode; }
      set { _SubMenuText = value; }
    }
    private string _SubMenuText;

    #endregion
  }

  public abstract class EFPMenuOutItem : IObjectWithCode
  {
    #region Конструктор

    public EFPMenuOutItem(string code)
    {
      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      _Code = code;
      _SendToItems = new NamedList<EFPSendToItem>();
    }

    #endregion

    #region Свойства

    public string Code { get { return _Code; } }
    private readonly string _Code;

    public virtual string DisplayName
    {
      get { return _DisplayName ?? Code; }
      set { _DisplayName = value; }
    }
    private string _DisplayName;

    public virtual bool CanPrint { get { return false; } }

    public virtual bool CanPageSetup { get { return false; } }
    public virtual bool CanPrintPreview { get { return false; } }
    public virtual bool CanExport { get { return false; } }

    /// <summary>
    /// Список команд "Отправить".
    /// Несколько объектов <see cref="EFPMenuOutItem"/> в списке <see cref="EFPMenuOutHandler.Items"/> могут иметь одинаковые команды.
    /// В этом случае в меню добавляется только одна команда, которая показывает диалог для выбора варианта отправки
    /// </summary>
    public NamedList<EFPSendToItem> SendToItems { get { return _SendToItems; } }
    private NamedList<EFPSendToItem> _SendToItems;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; }set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region Методы

    public virtual bool Print(bool defaultPrinter)
    {
      throw new NotImplementedException();
    }

    public virtual bool PageSetup()
    {
      throw new NotImplementedException();
    }

    public virtual void PrintPreview()
    {
      throw new NotImplementedException();
    }

    public virtual void Export()
    {
      throw new NotImplementedException();
    }

    public virtual void SendTo(EFPSendToItem item)
    {
      throw new NotImplementedException();
    }

    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region Событие Prepare

    internal void PerformPrepare()
    {
      OnPrepare();
    }

    /// <summary>
    /// Событие вызывается однократно при подготовке списка команд меню к использованию.
    /// На этот момент еще не сформированы окончательно списки команд для <see cref="EFPMenuOutHandler"/>.
    /// </summary>
    public event EventHandler Prepare;

    /// <summary>
    /// Вызывает событие <see cref="Prepare"/>
    /// </summary>
    protected virtual void OnPrepare()
    {
      if (Prepare != null)
        Prepare(this, EventArgs.Empty);
    }

    #endregion
  }

  /// <summary>
  /// Обработчик команд локального меню "Печать", "Параметры страницы", "Предварительный просмотр", "Экспорт в файл" и "Отправить"
  /// </summary>
  public class EFPMenuOutHandler
  {
    #region Конструктор

    public EFPMenuOutHandler(EFPContextCommandItems commandItems)
    {
      ciPrintDefault = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PrintDefault);
      ciPrintDefault.Click += PrintDefault_Click;
      commandItems.Add(ciPrintDefault);

      ciPrint = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Print);
      ciPrint.Usage &= (~EFPCommandItemUsage.ToolBar); // два значка с принтерами не нужны :)
      ciPrint.Click += Print_Click;
      commandItems.Add(ciPrint);

      ciPageSetup = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PageSetup);
      ciPageSetup.Click += PageSetup_Click;
      commandItems.Add(ciPageSetup);

      ciPrintPreview = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PrintPreview);
      ciPrintPreview.Click += PrintPreview_Click;
      commandItems.Add(ciPrintPreview);

      ciExport = new EFPCommandItem("File", "Export");
      ciExport.MenuText = "Экспорт в файл...";
      ciExport.Click += Export_Click;
      commandItems.Add(ciExport);

      _MenuSendTo = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSendTo);
      _MenuSendTo.Usage = EFPCommandItemUsage.Menu;
      commandItems.Add(_MenuSendTo);
      // Заполнение подменю выполняется в обработчике Prepare. Прикладной код может добавлять свои команды "Отправить", не связанные с отчетом

      _Items = new NamedList<EFPMenuOutItem>();
      _Enabled = true;
      commandItems.Prepare += CommandItems_Prepare;
    }

    private class SendToGroupInfo
    {
      #region Поля

      public EFPSendToItem FirstSendToItem;

      public EFPCommandItem FirstCommandInMenu;

      public EFPCommandItem FirstCommandInToolBar;

      public EFPCommandItem SubMenu;

      public EFPCommandItem SubToolBar;

      #endregion
    }

    private void CommandItems_Prepare(object sender, EventArgs args)
    {
      EFPCommandItems commandItems = (EFPCommandItems)sender;

      bool canPrint = false;
      bool canPageSetup = false;
      bool canPrintPreview = false;
      bool canExport = false;

      // Ключ - свойство EFPSendToItem.MainCode
      // Значение созданные команды меню
      Dictionary<string, SendToGroupInfo> dictSendTo = new Dictionary<string, SendToGroupInfo>();
      
      foreach (EFPMenuOutItem item in Items)
      {
        canPrint |= item.CanPrint;
        canPageSetup |= item.CanPageSetup;
        canPrintPreview |= item.CanPrintPreview;
        canExport |= item.CanExport;

        SendToGroupInfo firstGroup = null;

        foreach (EFPSendToItem sendToItem in item.SendToItems)
        {
          if (commandItems.Contains("SendTo", sendToItem.Code))
            continue;

          // Создаем две команды: одну для меню (ci1), вторую - для панели инструментов

          EFPCommandItem ci1 = new EFPCommandItem("SendTo", sendToItem.Code);
          ci1.MenuText = sendToItem.MenuText;
          ci1.Image = sendToItem.Image;
          ci1.ToolTipText = sendToItem.ToolTipText;
          ci1.Usage = EFPCommandItemUsage.Menu;
          ci1.Tag = sendToItem.Code;
          ci1.Click += ciSendTo_Click;
          //_SendToItems.Add(ci);

          EFPCommandItem ci2 = new EFPCommandItem("SendTo", sendToItem.Code+"_TB");
          ci2.MenuText = sendToItem.MenuText;
          ci2.Image = sendToItem.Image;
          ci2.Usage = EFPCommandItemUsage.ToolBar;
          ci2.ToolTipText = sendToItem.ToolTipText;
          ci2.Tag = sendToItem.Code;
          ci2.Click += ciSendTo_Click;
          //_SendToItems.Add(ci);

          SendToGroupInfo groupInfo;
          if (!dictSendTo.TryGetValue(sendToItem.MainCode, out groupInfo))
          {
            // Первая команда для заданного вида
            ci1.Parent = MenuSendTo;
            ci2.Parent = MenuSendTo; // чтобы кнопки были по порядку

            groupInfo = new SendToGroupInfo();
            groupInfo.FirstSendToItem = sendToItem;
            groupInfo.FirstCommandInMenu = ci1; // ее потребуется переместить
            groupInfo.FirstCommandInToolBar = ci2; 
            dictSendTo.Add(sendToItem.MainCode, groupInfo);

            if (firstGroup == null)
              firstGroup = groupInfo;
          }
          else
          {
            if (groupInfo.SubMenu == null)
            {
              // Вторая команда для заданного вида
              groupInfo.SubMenu = new EFPCommandItem("SendTo", sendToItem.MainCode+"_SubMenu");
              groupInfo.SubMenu.MenuText = sendToItem.SubMenuText; 
              groupInfo.SubMenu.Parent = MenuSendTo;
              groupInfo.SubMenu.Usage = EFPCommandItemUsage.Menu;
              commandItems.Add(groupInfo.SubMenu);
              // Меняем родителя у первой команды
              groupInfo.FirstCommandInMenu.Parent = groupInfo.SubMenu;
              groupInfo.FirstCommandInMenu.GroupEnd = true;

              groupInfo.SubToolBar = new EFPCommandItem("SendTo", sendToItem.MainCode + "_SubTB");
              //groupInfo.SubToolBar.MenuText = sendToItem.SubMenuText; 
              groupInfo.SubToolBar.Parent = MenuSendTo;
              groupInfo.SubToolBar.Usage = EFPCommandItemUsage.ToolBarDropDown;
              //groupInfo.SubToolBar.ImageKey = "UnknownState";
              commandItems.Add(groupInfo.SubToolBar);
              // У первой команды панели инструментов родителя не меняем
            }

            ci1.MenuText = sendToItem.MenuText;
            ci1.Parent = groupInfo.SubMenu;
            ci2.Parent = groupInfo.SubToolBar;
            ci2.Usage = EFPCommandItemUsage.Menu; // было ToolBar
          }
          commandItems.Add(ci1);
          commandItems.Add(ci2);
        } // SendToItems

        // При запуске в Wine+Mono многие типы файлов открываются с помощью приложения winebrowser.exe, а других подходящих приложений нет.
        // Надо добавить в текст меню тип файла, иначе будет несколько команд с одинаковыми именами
        foreach (KeyValuePair<string, SendToGroupInfo> pair in dictSendTo)
        {
          if (pair.Value.SubMenu == null)
          {
            pair.Value.FirstCommandInMenu.MenuText = pair.Value.FirstSendToItem.SubMenuText + " -> " + pair.Value.FirstCommandInMenu.MenuText;
          }
        }


        if (firstGroup != null)
        {
          if (firstGroup.SubMenu == null)
          {
            firstGroup.FirstCommandInMenu.GroupBegin = true;
            firstGroup.FirstCommandInToolBar.GroupBegin = true;
          }
          else
          {
            firstGroup.SubMenu.GroupBegin = true;
            firstGroup.SubToolBar.GroupBegin = true;
          }
        }
      }

      if (!canPrint)
      {
        ciPrintDefault.Usage = EFPCommandItemUsage.None;
        ciPrint.Usage = EFPCommandItemUsage.None;
      }
      if (!canPageSetup)
        ciPageSetup.Usage = EFPCommandItemUsage.None;
      if (!canPrintPreview)
        ciPrintPreview.Usage = EFPCommandItemUsage.None;
      if (!canExport)
        ciExport.Usage = EFPCommandItemUsage.None;
      if (_MenuSendTo.Children.Count==0)
        _MenuSendTo.Usage = EFPCommandItemUsage.None;
    }

    #endregion

    #region Список

    public NamedList<EFPMenuOutItem> Items { get { return _Items; } }

    public bool Enabled
    {
      get { return _Enabled; }
      set
      {
        _Enabled = value;
        ciPrintDefault.Enabled = value;
        ciPrint.Enabled = value;
        ciPageSetup.Enabled = value;
        ciPrintPreview.Enabled = value;
        ciExport.Enabled = value;
      }
    }
    private bool _Enabled;

    private readonly NamedList<EFPMenuOutItem> _Items;

    #endregion

    #region Команды

    private readonly EFPCommandItem ciPrintDefault, ciPrint, ciPageSetup, ciPrintPreview;
    private readonly EFPCommandItem ciExport;
    public EFPCommandItem MenuSendTo { get { return _MenuSendTo; } }
    private readonly EFPCommandItem _MenuSendTo;

    //private NamedList<EFPCommandItem> _SendToItems;

    private void PrintDefault_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem item = SelectItem("Печать", ciPrint.ImageKey, delegate (EFPMenuOutItem item2) { return item2.CanPrint; });
      if (item != null)
        item.Print(true);
    }

    private void Print_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem item = SelectItem("Печать", ciPrint.ImageKey, delegate (EFPMenuOutItem item2) { return item2.CanPrint; });
      if (item != null)
        item.Print(false);
    }

    private void PageSetup_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem item = SelectItem(ciPageSetup.MenuTextWithoutMnemonic, ciPageSetup.ImageKey, delegate (EFPMenuOutItem item2) { return item2.CanPageSetup; });
      if (item != null)
        item.PageSetup();
    }

    private void PrintPreview_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem item = SelectItem(ciPrintPreview.MenuTextWithoutMnemonic, ciPrintPreview.ImageKey, delegate (EFPMenuOutItem item2) { return item2.CanPrintPreview; });
      if (item != null)
        item.PrintPreview();
    }

    private void Export_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem item = SelectItem(ciExport.MenuTextWithoutMnemonic, "Save", delegate (EFPMenuOutItem item2) { return item2.CanExport; });
      if (item != null)
        item.Export();
    }

    private void ciSendTo_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      string sendToCode = (string)(ci.Tag);
      EFPMenuOutItem item = SelectItem("Отправить в " + ci.MenuTextWithoutMnemonic, ci.ImageKey,
        delegate (EFPMenuOutItem item2) { return item2.SendToItems.Contains(sendToCode); });
      if (item == null)
        return;

      EFPSendToItem sendToItem = item.SendToItems.GetRequired(sendToCode);
      item.SendTo(sendToItem);
    }

    private static string _LastSelectedItemCode=String.Empty;

    private delegate bool ItemTester(EFPMenuOutItem item);
    private EFPMenuOutItem SelectItem(string title, string imageKey, ItemTester tester)
    {
      NamedList<EFPMenuOutItem> list2 = new NamedList<EFPMenuOutItem>();
      foreach (EFPMenuOutItem item in Items)
      {
        item.PerformPrepare();

        if (tester(item))
          list2.Add(item);
      }

      switch (list2.Count)
      {
        case 0:
          throw new BugException("Нет подходящих вариантов");
        case 1:
          return list2[0];
      }

      ListSelectDialog dlg = new ListSelectDialog();
      string[] a = new string[list2.Count];
      int selIndex = 0;
      for (int i = 0; i < list2.Count; i++)
      {
        a[i] = list2[i].DisplayName;
        if (String.Equals(list2[i].Code, _LastSelectedItemCode, StringComparison.Ordinal))
          selIndex = i;
      }
      dlg.Items = a;
      dlg.Title = title;
      dlg.ImageKey = imageKey;
      dlg.SelectedIndex = selIndex;
      if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        _LastSelectedItemCode = list2[dlg.SelectedIndex].Code;
        return list2[dlg.SelectedIndex];
      }
      else
        return null;
    }

    #endregion
  }

}
