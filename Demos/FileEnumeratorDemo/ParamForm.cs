using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.IO;

namespace FileEnumeratorDemo
{
  public partial class ParamForm : Form
  {
    #region Конструктор формы

    public ParamForm()
    {
      InitializeComponent();

      efpForm = new EFPFormProvider(this);

      cbClass.Items.Add("AbsPathEnumerable");
      cbClass.Items.Add("RelPathEnumerable");
      efpClass = new EFPListComboBox(efpForm, cbClass);
      efpClass.SelectedIndex = 0;

      efpRootDirectory = new EFPTextBox(efpForm, edRootDirectory);
      efpRootDirectory.CanBeEmpty = false;
      EFPFolderBrowserButton efpBrowse = new EFPFolderBrowserButton(efpRootDirectory, btnBrowse);
      efpBrowse.Description = "Корневой каталог, с которого начинается перебор";
      efpBrowse.ShowNewFolderButton = false;

      efpEnumerateKind = new EFPListComboBox(efpForm, cbEnumerateKind);
      FillEnumComboBox(efpEnumerateKind, typeof(PathEnumerateKind));

      efpEnumerateMode = new EFPListComboBox(efpForm, cbEnumerateMode);
      FillEnumComboBox(efpEnumerateMode, typeof(PathEnumerateMode));


      efpFileSearchPattern = new EFPTextBox(efpForm, edFileSearchPattern);
      efpFileSearchPattern.CanBeEmpty = true;

      efpFileSort = new EFPListComboBox(efpForm, cbFileSort);
      FillEnumComboBox(efpFileSort, typeof(PathEnumerateSort));

      efpReverseFiles = new EFPCheckBox(efpForm, cbReverseFiles);


      efpDirectorySearchPattern = new EFPTextBox(efpForm, edDirectorySearchPattern);
      efpDirectorySearchPattern.CanBeEmpty = true;

      efpDirectorySort = new EFPListComboBox(efpForm, cbDirectorySort);
      FillEnumComboBox(efpDirectorySort, typeof(PathEnumerateSort));

      efpReverseDirectories = new EFPCheckBox(efpForm, cbReverseDirectories);


      btnOk.Image = EFPApp.MainImages.Images["OK"];
      btnOk.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpOk = new EFPButton(efpForm, btnOk);
      efpOk.Click += new EventHandler(efpOk_Click);
    }

    private static void FillEnumComboBox(EFPListComboBox efpCB, Type enumType)
    {
      efpCB.Control.Items.AddRange(Enum.GetNames(enumType));
      efpCB.SelectedIndex = 0;
    }

    #endregion

    #region Поля

    EFPFormProvider efpForm;
    EFPListComboBox efpClass;
    EFPTextBox efpRootDirectory;
    EFPListComboBox efpEnumerateKind, efpEnumerateMode;
    EFPTextBox efpFileSearchPattern, efpDirectorySearchPattern;
    EFPListComboBox efpFileSort, efpDirectorySort;
    EFPCheckBox efpReverseFiles, efpReverseDirectories;

    #endregion

    #region Построение списка

    List<string> TextList;
    int IndentLevel;

    /// <summary>
    /// Строка для форматирования отступа
    /// </summary>
    string Indent { get { return new string(' ', IndentLevel * 2); } }

    void efpOk_Click(object sender, EventArgs args)
    {
      if (!efpForm.ValidateForm())
        return;

      TextList = new List<string>();

      using (Splash spl = new Splash("Идет перебор"))
      {
        spl.AllowCancel = true;


        switch (efpClass.Control.SelectedItem.ToString())
        {
          case "AbsPathEnumerable":
            AbsPathEnumerable en1 = new AbsPathEnumerable(new AbsPath(efpRootDirectory.Text),
              GetEnumValue<PathEnumerateKind>(efpEnumerateKind));
            InitProps(en1);
            foreach (AbsPath item in en1)
            {
              TextList.Add(Indent+item.Path);
              spl.CheckCancelled();
            }
            break;

          case "RelPathEnumerable":
            RelPathEnumerable en2 = new RelPathEnumerable(new AbsPath(efpRootDirectory.Text),
              GetEnumValue<PathEnumerateKind>(efpEnumerateKind));
            InitProps(en2);
            foreach (RelPath item in en2)
            {
              TextList.Add(Indent + item.Path);
              spl.CheckCancelled();
            }
            break;
        }

      }
      EFPApp.ShowTextView(String.Join(Environment.NewLine, TextList.ToArray()), "Результат перебора");
    }

    /// <summary>
    /// Инициализация свойств перечислителя
    /// </summary>
    /// <param name="en"></param>
    private void InitProps(PathEnumerableBase en)
    {
      en.EnumerateMode = GetEnumValue<PathEnumerateMode>(efpEnumerateMode);

      en.FileSearchPattern = efpFileSearchPattern.Text;
      en.FileSort = GetEnumValue<PathEnumerateSort>(efpFileSort);
      en.ReverseFiles = efpReverseFiles.Checked;

      en.DirectorySearchPattern = efpDirectorySearchPattern.Text;
      en.DirectorySort = GetEnumValue<PathEnumerateSort>(efpDirectorySort);
      en.ReverseDirectories = efpReverseDirectories.Checked;

      en.BeforeDirectory += new EnumDirectoryEventHandler(en_BeforeDirectory);
      en.AfterDirectory += new EnumDirectoryEventHandler(en_AfterDirectory);
    }

    private static T GetEnumValue<T>(EFPListComboBox efpCB)
    {
      string s = efpCB.Control.SelectedItem.ToString();
      return (T)(Enum.Parse(typeof(T), s));
    }

    void en_BeforeDirectory(object sender, EnumDirectoryEventArgs args)
    {
      IndentLevel = args.Level;
      TextList.Add(Indent + "BeforeDirectory: " + args.ToString());
      IndentLevel = args.Level+1; // для файлов
    }

    void en_AfterDirectory(object sender, EnumDirectoryEventArgs args)
    {
      IndentLevel = args.Level;
      TextList.Add(Indent + "AfterDirectory: " + args.ToString());
    }

    #endregion
  }
}
