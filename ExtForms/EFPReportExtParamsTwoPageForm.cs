using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// ������� ����� ����� ��� ���������� ������ � ��������������� �����������
  /// ������������
  /// ����� �������� TabControl � ����� ���������: "�����" � "�������".
  /// ����� ����� ���� ������ � �������� "��" � "������", � ����� ������ � ��������
  /// ��� ������ ������� ������� ����������.
  /// ����������� ����� ������ �������� ����������� �������� �� ������� MainTabPage.
  /// ��� �������� ���������� ������ � ����� � ���������������� ������ EFPReportExtParams.WriteFormValues() ������ ���� ����������
  /// EFPReportExtParamsTwoPageForm.FiltersControlProvider.Filters = Filters.
  /// </summary>
  public partial class EFPReportExtParamsTwoPageForm : EFPReportExtParamsForm
  {
    #region ����������� �����

    /// <summary>
    /// ������� ����� � ������ EFPGridFilterEditorGridView
    /// </summary>
    public EFPReportExtParamsTwoPageForm()
    {
      InitializeComponent();

      if (EFPApp.AppWasInit)
      {
        TheTabControl.ImageList = EFPApp.MainImages;
        MainTabPage.ImageKey = "Properties";
        FiltersTabPage.ImageKey = "Filter";

        EFPControlWithToolBar<DataGridView> cwt = new EFPControlWithToolBar<DataGridView>(FormProvider, FiltersTabPage);
        _FiltersControlProvider = new EFPGridFilterEditorGridView(cwt);

        FormProvider.Shown += new EventHandler(FormProvider_Shown);
        FormProvider.Hidden += new EventHandler(FormProvider_Hidden);
      }
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ���������� ��������� ��� ������� �������������� ��������.
    /// ���������������� ��� ������ ������������� �������� Filters ����� ������� � ���������������� ������ EFPReportExtParams.WriteFormValues().
    /// </summary>
    public EFPGridFilterEditorGridView FiltersControlProvider { get { return _FiltersControlProvider; } }
    private EFPGridFilterEditorGridView _FiltersControlProvider;

    #endregion

    #region ���������� ��������� ������� ����� ��������

    /// <summary>
    /// ���� - ����� �����
    /// �������� - ��������� ��������� �������
    /// </summary>
    private static Dictionary<Type, int> _LastSelectedPageIndexes = new Dictionary<Type, int>();

    void FormProvider_Shown(object sender, EventArgs args)
    {
      int PageIndex;
      if (_LastSelectedPageIndexes.TryGetValue(this.GetType(), out PageIndex))
        TheTabControl.SelectedIndex = PageIndex;
    }

    void FormProvider_Hidden(object sender, EventArgs args)
    {
      if (base.DialogResult == DialogResult.OK)
        _LastSelectedPageIndexes[this.GetType()] = TheTabControl.SelectedIndex;
    }

    #endregion
  }
}