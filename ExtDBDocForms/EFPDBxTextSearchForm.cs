// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Расширение формы поиска текста кнопкой "Выборка документов"
  /// </summary>
  public partial class EFPDBxTextSearchForm : EFPTextSearchForm
  {
    /// <summary>
    /// Конструктор формы.
    /// Дополнительно инициализирует кнопку "Выборка документов".
    /// </summary>
    public EFPDBxTextSearchForm()
    {
      InitializeComponent();

      // 22.07.2020
      // Мерзкая затычка.
      // Если ничего не делать, кнопка не будет масштабироваться
      Control btnNo = base.NoButtonProvider.Control;
      btnDocSel.Location = btnNo.Location;
      btnDocSel.Size = new Size(btnNo.Size.Width * 2, btnNo.Size.Height);

      btnDocSel.Image = EFPApp.MainImages.Images["DBxDocSelection"];
      btnDocSel.ImageAlign = ContentAlignment.MiddleLeft;
      efpDocSel = new EFPButton(FormProvider, btnDocSel);
      efpDocSel.ToolTipText = Res.EFPDBxTextSearch_ToolTip_DocSel;
      efpDocSel.Click += efpDocSel_Click;
    }

    /// <summary>
    /// Кнопка "Выборка документов"
    /// </summary>
    public EFPButton efpDocSel;

    void efpDocSel_Click(object sender, EventArgs args)
    {
      DialogResult = System.Windows.Forms.DialogResult.Yes;
      Close();
    }
  }

  /// <summary>
  /// Контекст поиска текста в табличном просмортре с поддержкой выборки документов
  /// </summary>
  public class EFPDBxGridViewSearchContext : EFPDataGridViewSearchContext
  {
    #region Конструктор

    /// <summary>
    /// Создает контекст
    /// </summary>
    /// <param name="owner">Провайдер табличного просмотра</param>
    public EFPDBxGridViewSearchContext(EFPDBxGridView owner)
      : base(owner)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPDBxGridView Owner { get { return (EFPDBxGridView)(base.Owner); } }

    /// <summary>
    /// Вывод диалога параметров поиска текста
    /// </summary>
    /// <param name="searchInfo">Настройки поиска</param>
    /// <returns>True, если пользователь нажал "ОК" и требуется выполнить поиск</returns>
    protected override bool QueryParams(EFPTextSearchInfo searchInfo)
    {
      EFPDBxTextSearchForm form = new EFPDBxTextSearchForm();
      form.SetValues(searchInfo);
      form.btnDocSel.Enabled = Owner.HasGetDocSelHandler;
      switch (EFPApp.ShowDialog(form, true))
      {
        case DialogResult.OK:
          form.GetValues(searchInfo);
          return true;
        case DialogResult.Yes:
          form.GetValues(searchInfo);
          CreateDocSel();
          return false;
        default:
          return false;
      }
    }

    #endregion

    #region Создание выборки документов для выборки строк

    private void CreateDocSel()
    {
      int[] rowIndices = base.FindAllRowIndices();
      if (rowIndices == null)
      {
        EFPApp.ShowTempMessage(Res.EFPDBxTextSearch_Err_NoRowsFound);
        return;
      }

      DBxDocSelection docSel = Owner.CreateDocSel(EFPDBxViewDocSelReason.SendTo, rowIndices);
      Owner.UI.ShowDocSel(docSel, Res.EFPDBxTextSearch_Title_DocSel);
    }

    #endregion

  }

  /// <summary>
  /// Контекст поиска текста в табличном просмортре с поддержкой выборки документов
  /// </summary>
  public class EFPDBxTreeViewSearchContext : EFPTreeViewAdvSearchContext
  {
    #region Конструктор

    /// <summary>
    /// Создает контекст поиска
    /// </summary>
    /// <param name="owner">Провайдер иерархического просмотра</param>
    public EFPDBxTreeViewSearchContext(EFPDBxTreeView owner)
      : base(owner)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Провайдер иерархического просмотра
    /// </summary>
    public new EFPDBxTreeView Owner { get { return (EFPDBxTreeView)(base.Owner); } }

    /// <summary>
    /// Вывод диалога параметров поиска текста
    /// </summary>
    /// <param name="searchInfo">Настройки поиска</param>
    /// <returns>True, если пользователь нажал "ОК" и требуется выполнить поиск</returns>
    protected override bool QueryParams(EFPTextSearchInfo searchInfo)
    {
      EFPDBxTextSearchForm form = new EFPDBxTextSearchForm();
      form.SetValues(searchInfo);
      form.btnDocSel.Enabled = Owner.HasGetDocSelHandler;
      switch (EFPApp.ShowDialog(form, true))
      {
        case DialogResult.OK:
          form.GetValues(searchInfo);
          return true;
        case DialogResult.Yes:
          form.GetValues(searchInfo);
          CreateDocSel();
          return false;
        default:
          return false;
      }
    }

    #endregion

    #region Создание выборки документов для выборки строк

    private void CreateDocSel()
    {
      FreeLibSet.Controls.TreeNodeAdv[] nodes = base.FindAllNodes();
      if (nodes == null)
      {
        EFPApp.ShowTempMessage(Res.EFPDBxTextSearch_Err_NoRowsFound);
        return;
      }

      DBxDocSelection docSel = Owner.CreateDocSel(EFPDBxViewDocSelReason.SendTo, nodes);
      Owner.UI.ShowDocSel(docSel, Res.EFPDBxTextSearch_Title_DocSel);
    }

    #endregion
  }
}
