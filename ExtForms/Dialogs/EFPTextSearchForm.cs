// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Collections;
using FreeLibSet.Controls;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма параметров поиска текста
  /// </summary>
  public partial class EFPTextSearchForm : OKCancelForm
  {
    #region Конструктор

    /// <summary>
    /// Создает форму
    /// </summary>
    public EFPTextSearchForm()
    {
      // 31.08.2020
      // Так тоже не работает при разрешении, большем 96 dpi.
      // Ну его в болото, отменю выравнивание группы по правому краю формы
      //// 20.04.2020
      //// Какая-то странность в Windows Forms.
      //// Базовая форма OKCancelForm имеет меньший размер.
      //// Когда в этом классе мы устанавливаем размер формы, не меняется размер
      //// панели MainPanel, она остается маленькой. Группа grpText имеет больший
      //// размер и привязку к ширине MainPanel. При добавлении она начинает
      //// выходить за границу MainPanel. Когда InitializeComponent() вызывает
      //// Form.ResumeLayout(), размер панели MainPanel увеличивается. Но вместе с ним
      //// увеличивает и размер grpText. В результате, комбоблок не входит.
      //// Пытаемся заранее установить размер формы, но все равно остается небольшой 
      //// перекос на несколько пикселей.
      //this.ClientSize = new System.Drawing.Size(411, 271);

      InitializeComponent();

      if (!base.DesignMode)
        Icon = EFPApp.MainImageIcon("Find");

      efpText = new EFPTextComboBox(FormProvider, cbText);
      efpText.CanBeEmpty = false;
      efpText.ToolTipText = "Текст, который нужно найти";

      efpCaseSens = new EFPCheckBox(FormProvider, cbCaseSens);
      efpCaseSens.ToolTipText = "Если флажок установлен, то регистр букв имеет значение," + Environment.NewLine +
        "иначе заглавные и строчные буквы (например, \"А\" и \"а\") не различаются";

      efpSimilarCharsDiff = new EFPCheckBox(FormProvider, cbSimilarCharsDiff);
      efpSimilarCharsDiff.ToolTipText = "Если флажок установлен, то похожие символы кириллицы и латиницы различаются," + Environment.NewLine +
        "иначе, например, русская буква \"А\" и латинская \"A\" считаются совпадающими";

      efpWhole = new EFPCheckBox(FormProvider, cbWhole);
      efpWhole.ToolTipText = "Если флажок установлен, то будут найдены только ячейки, содержимое которых " +
        "точно совпадает с введенным текстом. Иначе достаточно найти вхождение введенного текста " +
        "в любое место текста ячейки";

      efpWhere = new EFPRadioButtons(FormProvider, rbAllCols);
      efpWhere.ToolTipText = "Поиск по всем столбцам занимает больше времени. Используйте поиск в текущем " +
        "столбце, если Вы знаете, где расположен текст. Перед выбором этого режима в табличном просмотре " +
        "должен быть выбран нужный столбец";

      efpDirection = new EFPRadioButtons(FormProvider, rbForward);
      efpDirection.ToolTipText = "Направление перебора строк - к концу или к началу";

      efpFrom = new EFPRadioButtons(FormProvider, rbFromStart);
      efpFrom.ToolTipText = "Определяет область для поиска: все строки или часть " +
        "от текущей позиции до конца (или до начала таблицы)";

      efpDirection.SelectedIndexEx.ValueChanged += new EventHandler(efpDirection_ValueChanged);
      efpDirection_ValueChanged(null, null);
    }

    void efpDirection_ValueChanged(object sender, EventArgs args)
    {
      rbFromStart.Text = efpDirection.SelectedIndex == 0 ? "&С начала" : "&С конца"; // 24.11.2017
    }

    #endregion

    #region Поля

    EFPTextComboBox efpText;

    EFPCheckBox efpCaseSens;
    EFPCheckBox efpSimilarCharsDiff;
    EFPCheckBox efpWhole;

    /// <summary>
    /// Переключатели "Где искать"
    /// </summary>
    public EFPRadioButtons efpWhere;

    EFPRadioButtons efpDirection;
    EFPRadioButtons efpFrom;

    #endregion

    #region Чтение и запись текущего состояния

    /// <summary>
    /// Хранение истории для строки текста
    /// </summary>
    private static HistoryList _History = new HistoryList();

    /// <summary>
    /// Заполнение управляющих элементов формы значениями.
    /// Вызывается перед показом диалога поиска.
    /// </summary>
    /// <param name="values">Параметры поиска</param>
    public void SetValues(EFPTextSearchInfo values)
    {
      efpText.Text = values.Text;
      efpCaseSens.Checked = values.CaseSensitive;
      efpSimilarCharsDiff.Checked = values.SimilarCharsDiff;
      efpWhole.Checked = values.Whole;
      efpWhere.SelectedIndex = values.CurrentColumn ? 1 : 0;
      efpDirection.SelectedIndex = values.Backward ? 1 : 0;
      efpFrom.SelectedIndex = values.FromCurrent ? 1 : 0;

      cbText.Items.AddRange(_History.ToArray());
    }

    /// <summary>
    /// Заполнение значений <paramref name="values"/> из управляющих элементов формы.
    /// Вызывается после показа диалога поиска, если пользователь нажал "ОК".
    /// </summary>
    /// <param name="values">Параметры поиска</param>
    public void GetValues(EFPTextSearchInfo values)
    {
      values.Text = efpText.Text;
      values.CaseSensitive = efpCaseSens.Checked;
      values.SimilarCharsDiff = efpSimilarCharsDiff.Checked;
      values.Whole = efpWhole.Checked;
      values.CurrentColumn = efpWhere.SelectedIndex == 1;
      values.Backward = efpDirection.SelectedIndex == 1;
      values.FromCurrent = efpFrom.SelectedIndex == 1;

      _History = _History.Add(values.Text, 50);
    }

    #endregion
  }

  #region Интерфейс IEFPTextSearchContext

  /// <summary>
  /// Интерфейс поиска текста в табличном просмотре
  /// </summary>
  public interface IEFPTextSearchContext
  {
    /// <summary>
    /// Начать поиск.
    /// Метод вызывается при нажатии Ctrl-F
    /// </summary>
    void StartSearch();

    /// <summary>
    /// Продолжить поиск.
    /// Метод вызывается при нажатии F3
    /// </summary>
    void ContinueSearch();

    /// <summary>
    /// Можно ли продолжить поиск.
    /// Используется для определения видимости команды меню "Продолжить поиск"
    /// </summary>
    bool ContinueEnabled { get;}

    /// <summary>
    /// Запретить продолжение поиска.
    /// Метод вызывается табличным просмотром при выполнении поиска по первым буквам.
    /// Сбрасывает свойство ContinueEnabled в false
    /// </summary>
    void ResetContinueEnabled();
  }

  #endregion

  /// <summary>
  /// Параметры поиска текста
  /// </summary>
  public class EFPTextSearchInfo
  {
    #region Поля поиска

    /// <summary>
    /// Искомый текст
    /// </summary>
    public string Text { get { return _Text; } set { _Text = value; } }
    private string _Text;

    /// <summary>
    /// true, если при поиске должен учитываться верхний/нижний регистра
    /// </summary>
    public bool CaseSensitive { get { return _CaseSensitive; } set { _CaseSensitive = value; } }
    private bool _CaseSensitive;

    /// <summary>
    /// true, если похожие символы должны различаться.
    /// Если false, то, например, латинская буква "e" и русская "е" считаются одинаковыми символами.
    /// </summary>
    public bool SimilarCharsDiff { get { return _SimilarCharsDiff; } set { _SimilarCharsDiff = value; } }
    private bool _SimilarCharsDiff;

    /// <summary>
    /// True, если требуется найти полное совпадение.
    /// False, если требуется найти строку, содержащую искомый фрагмент.
    /// Используется в табличных и иерархических просмотрах, но не в TextBox.
    /// </summary>
    public bool Whole { get { return _Whole; } set { _Whole = value; } }
    private bool _Whole;

    /// <summary>
    /// True, если требуется выполнить поиск только в текущем столбце, а не во всех столбцах просмотра.
    /// Используется в табличных и иерархических просмотрах.
    /// </summary>
    public bool CurrentColumn { get { return _CurrentColumn; } set { _CurrentColumn = value; } }
    private bool _CurrentColumn;

    /// <summary>
    /// True, если требуется поиск в обратную сторону.
    /// </summary>
    public bool Backward { get { return _Backward; } set { _Backward = value; } }
    private bool _Backward;

    /// <summary>
    /// True, если поиск выполняется от текущей позиции, а не от начала/конца просмотра.
    /// </summary>
    public bool FromCurrent { get { return _FromCurrent; } set { _FromCurrent = value; } }
    private bool _FromCurrent;

    #endregion

    #region Методы

    /// <summary>
    /// Проверить, подходит ли текст под условия
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool TextMatch(string s)
    {
      if (Whole)
        return NormText(s) == NormText(Text);
      else
        return NormText(s).Contains(NormText(Text));
    }

    /// <summary>
    /// Словарь похожих символов.
    /// Используется, когда флажок "Отличать похожие буквы" выключен
    /// </summary>
    public static readonly Dictionary<char, char> SimilarCharDict = CreateSimilarCharDict();

    private static Dictionary<char, char> CreateSimilarCharDict()
    {
      const string SimilarCharsSrc = "АаВвЕеКкМмНнОоРрСсТтУуХх"; // Русские
      const string SimilarCharsDst = "AaBbEeKkMmHhOoPpCcTtYyXx"; // Латинские

      Dictionary<char, char> dict = new Dictionary<char, char>(SimilarCharsSrc.Length);
      for (int i = 0; i < SimilarCharsSrc.Length; i++)
        dict.Add(SimilarCharsSrc[i], SimilarCharsDst[i]);

      return dict;
    }

    // Нормализация текста в соответствии с условиями
    private string NormText(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;


      // 1. Заменяем специальные символы на пробелы.
      s = s.Replace('\r', ' ');
      s = s.Replace('\n', ' ');
      s = s.Replace('\t', ' ');

      // 2. Убираем двойные пробелы и крайние пробелы
      //while (s.IndexOf("  ") >= 0)
      //  s = s.Replace("  ", " ");
      s = DataTools.RemoveDoubleChars(s, ' '); // 25.11.2016
      s = s.Trim();

      if (!CaseSensitive)
        s = s.ToUpperInvariant();
      if (!SimilarCharsDiff)
        s = DataTools.ReplaceChars(s, SimilarCharDict);

      return s;
    }

    /// <summary>
    /// Копирует настройки в другой набор
    /// </summary>
    /// <param name="dest">Заполняемый набор</param>
    public void CopyTo(EFPTextSearchInfo dest)
    {
      dest.Text = Text;
      dest.CaseSensitive = CaseSensitive;
      dest.SimilarCharsDiff = SimilarCharsDiff;
      dest.Whole = Whole;
      dest.CurrentColumn = CurrentColumn;
      dest.Backward = Backward;
      dest.FromCurrent = FromCurrent;
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для EFPDataGridViewSearchContext и EFPTreeViewAdvSearchContext 
  /// </summary>
  public class EFPTextSearchContextBase
  {
    #region Свойства

    /// <summary>
    /// Что и как искать
    /// </summary>
    public EFPTextSearchInfo SearchInfo
    {
      get
      {
        if (_SearchInfo == null)
        {
          _SearchInfo = new EFPTextSearchInfo();
          DefaultSearchInfo.CopyTo(_SearchInfo);
        }
        return _SearchInfo;
      }
    }
    private EFPTextSearchInfo _SearchInfo;


    /// <summary>
    /// Статическая копия параметров запросов.
    /// </summary>
    protected static readonly EFPTextSearchInfo DefaultSearchInfo = new EFPTextSearchInfo();

    /// <summary>
    /// True, если запрос параметров был выполнен хотя бы один раз
    /// </summary>
    public bool ContinueEnabled { get { return _ContinueEnabled; } }
    private bool _ContinueEnabled;

    /// <summary>
    /// Очистка флага ContinueEnabled
    /// </summary>
    public void ResetContinueEnabled()
    {
      _ContinueEnabled = false;
    }

    /// <summary>
    /// Установка флага ContinueEnabled.
    /// Выполняется при запросе параметров поиска
    /// </summary>
    protected void SetContinueEnabled()
    {
      _ContinueEnabled = true;
    }

    #endregion
  }


  /// <summary>
  /// Контекст для поиска текста в текстовом редакторе.
  /// </summary>
  public class EFPTextBoxSearchContext : EFPTextSearchContextBase, IEFPTextSearchContext
  {
    #region Конструктор

    /// <summary>
    /// Создает контекст поиска
    /// </summary>
    /// <param name="owner">Провайдер управляющего элемента</param>
    public EFPTextBoxSearchContext(IEFPTextBox owner)
    {
      _Owner = owner;
      owner.TextEx.ValueChanged += new EventHandler(TextEx_ValueChanged);
    }

    void TextEx_ValueChanged(object sender, EventArgs args)
    {
      _TextCopy = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    public IEFPTextBox Owner { get { return _Owner; } }
    private IEFPTextBox _Owner;

    /// <summary>
    /// Копия текста в управляющем элементе с учетом игнорирования регистра и похожих символов
    /// </summary>
    private string _TextCopy;

    /// <summary>
    /// Копия искомого текста с учетом игнорирования регистра и похожих символов
    /// </summary>
    private string _SearchCopy;

    #endregion

    #region Методы

    /// <summary>
    /// Начать поиск текста
    /// </summary>
    public void StartSearch()
    {
      if (Owner.Text.Length == 0)
      {
        EFPApp.ShowTempMessage("Просмотр не содержит текста");
        return;
      }

      if (!QueryParams(SearchInfo))
        return;

      SearchInfo.CopyTo(DefaultSearchInfo);

      SetContinueEnabled();

      _TextCopy = null; // TODO: Можно очищать, только если поменялись флажки поиска

      _SearchCopy = SearchInfo.Text;
      if (!SearchInfo.CaseSensitive)
        _SearchCopy = _SearchCopy.ToUpperInvariant();
      if (!SearchInfo.SimilarCharsDiff)
        _SearchCopy = DataTools.ReplaceChars(_SearchCopy, EFPTextSearchInfo.SimilarCharDict);

      int startPos;
      if (SearchInfo.FromCurrent)
        startPos = Owner.SelectionStart;
      else
      {
        if (SearchInfo.Backward)
          startPos = Owner.Text.Length - SearchInfo.Text.Length;
        else
          startPos = 0;
      }

      DoSearch(startPos, true);
    }

    /// <summary>
    /// Показывает диалог поиска текста
    /// </summary>
    /// <param name="searchInfo">Параметры поиска</param>
    /// <returns>true, если пользователь нажал кнопку "ОК"</returns>
    protected virtual bool QueryParams(EFPTextSearchInfo searchInfo)
    {
      using (EFPTextSearchForm frm = new EFPTextSearchForm())
      {
        frm.efpWhere.Visible = false;
        frm.grpWhere.Visible = false;
        frm.SetValues(searchInfo);
        if (EFPApp.ShowDialog(frm, false) != DialogResult.OK)
          return false;

        frm.GetValues(searchInfo);
      }
      return true;
    }

    /// <summary>
    /// Продолжить поиск с текущими параметрами
    /// </summary>
    public void ContinueSearch()
    {
      if (!ContinueEnabled)
      {
        EFPApp.ShowTempMessage("Поиск не был начат");
        return;
      }
      //if (Owner.SelectionStart < 0)
      //{
      //  EFPApp.ShowTempMessage("Невозможно продолжить поиск, т.к. нет текущей строки");
      //  return;
      //}

      DoSearch(Owner.SelectionStart, false);
    }

    private void DoSearch(int startPos, bool firstSearch)
    {
      #region Подготовка текста для поиска

      if (_TextCopy == null)
      {
        _TextCopy = Owner.Text;
        if (_TextCopy == null)
          _TextCopy = String.Empty; // на всякий случай

        if (!SearchInfo.CaseSensitive)
          _TextCopy = _TextCopy.ToUpperInvariant();
        if (!SearchInfo.SimilarCharsDiff)
          _TextCopy = DataTools.ReplaceChars(_TextCopy, EFPTextSearchInfo.SimilarCharDict);
      }

      #endregion

      if (!firstSearch)
        MoveNext(ref startPos);

      bool found = false;
      Splash spl = new Splash("Поиск текста");
      spl.AllowCancel = true;

      try
      {
        while (startPos >= 0 && (startPos + _SearchCopy.Length - 1) < _TextCopy.Length)
        {
          spl.CheckCancelled();

          if (String.Compare(_TextCopy, startPos, _SearchCopy, 0, _SearchCopy.Length, StringComparison.Ordinal) == 0) // TODO: Equals()
          {
            Owner.Select(startPos, _SearchCopy.Length);
            found = true;
            break;
          }
          MoveNext(ref startPos);
        }
      }
      finally
      {
        spl.Close();
      }
      if (!found)
        EFPApp.MessageBox("Строка \"" + SearchInfo.Text + "\" не найдена", "Поиск текста");
    }

    /// <summary>
    /// Переход к следующей / предыдущей ячейке или строке
    /// </summary>
    /// <param name="startPos"></param>
    private void MoveNext(ref int startPos)
    {
      if (SearchInfo.Backward)
        startPos--;
      else
        startPos++;
    }

    #endregion
  }

  /// <summary>
  /// Контекст для поиска текста в табличном просмотре
  /// </summary>
  public class EFPDataGridViewSearchContext : EFPTextSearchContextBase, IEFPTextSearchContext
  {
    #region Конструктор

    /// <summary>
    /// Создает контекст поиска
    /// </summary>
    /// <param name="owner">Провайдер табличного просмотра</param>
    public EFPDataGridViewSearchContext(EFPDataGridView owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public EFPDataGridView Owner { get { return _Owner; } }
    private EFPDataGridView _Owner;

    #endregion

    #region Методы

    /// <summary>
    /// Начать поиск
    /// </summary>
    public void StartSearch()
    {
      if (Owner.Control.RowCount == 0)
      {
        EFPApp.ShowTempMessage("Табличный просмотр не содержит строк");
        return;
      }

      if (!QueryParams(SearchInfo))
        return;

      SearchInfo.CopyTo(DefaultSearchInfo);


      SetContinueEnabled();
      // Сбрасываем поиск по первым буквам
      Owner.CurrentIncSearchColumn = null;

      // Пока работал диалог, в просмотре могли произойти изменения
      if (Owner.Control.RowCount == 0)
      {
        EFPApp.ShowTempMessage("Табличный просмотр не содержит строк");
        return;
      }

      DataGridViewRow row;
      if (SearchInfo.FromCurrent)
        row = Owner.CurrentGridRow;
      else
      {
        if (SearchInfo.Backward)
          row = Owner.Control.Rows[Owner.Control.RowCount - 1];
        else
          row = Owner.Control.Rows[0];
      }

      DataGridViewColumn[] visibleColumns = WinFormsTools.GetOrderedVisibleColumns(Owner.Control);
      if (visibleColumns.Length == 0)
      {
        EFPApp.ShowTempMessage("Табличный просмотр не содержит видимых столбцов");
        return;
      }

      int columnIndex = -1;
      if (SearchInfo.CurrentColumn)
        columnIndex = Owner.CurrentColumnIndex;
      if (columnIndex < 0)
      {
        if (SearchInfo.Backward)
          columnIndex = visibleColumns[visibleColumns.Length - 1].Index; // 17.05.2016
        else
          columnIndex = visibleColumns[0].Index;
      }
      DoSearch(row, columnIndex, true);
    }

    /// <summary>
    /// Показывает блок диалога поиска текста в табличном просмотре
    /// </summary>
    /// <param name="searchInfo">Параметры поиска</param>
    /// <returns>true, если пользователь нажал кнопку "ОК"</returns>
    protected virtual bool QueryParams(EFPTextSearchInfo searchInfo)
    {
      using (EFPTextSearchForm frm = new EFPTextSearchForm())
      {
        frm.SetValues(searchInfo);
        if (EFPApp.ShowDialog(frm, false) != DialogResult.OK)
          return false;

        frm.GetValues(searchInfo);
      }
      return true;
    }

    /// <summary>
    /// Продолжить поиск с текущими параметрами
    /// </summary>
    public void ContinueSearch()
    {
      if (!ContinueEnabled)
      {
        EFPApp.ShowTempMessage("Поиск не был начат");
        return;
      }
      if (Owner.Control.CurrentRow == null)
      {
        EFPApp.ShowTempMessage("Невозможно продолжить поиск, т.к. нет текущей строки");
        return;
      }

      int columnIndex = Owner.CurrentColumnIndex;
      if (columnIndex < 0)
        columnIndex = Owner.Control.FirstDisplayedCell.ColumnIndex;
      DoSearch(Owner.Control.CurrentRow, columnIndex, false);
    }

    private void DoSearch(DataGridViewRow row, int columnIndex, bool firstSearch)
    {
      if (!firstSearch)
        MoveNext(ref row, ref columnIndex);

      bool found = false;
      Splash spl = new Splash("Поиск текста");
      spl.AllowCancel = true;
      if (row != null)
      {
        if (SearchInfo.Backward)
          spl.SetPercent(0, row.Index + 1);
        else
          spl.SetPercent(0, Owner.Control.RowCount - row.Index);

        Owner.DoGetRowAttributes(row.Index, EFPDataGridViewAttributesReason.View);
      }
      try
      {
        while (row != null)
        {
          spl.CheckCancelled();
          if (TestCell(row, columnIndex))
          {
            Owner.CurrentGridRow = row;
            Owner.CurrentColumnIndex = columnIndex;
            found = true;
            break;
          }
          DataGridViewRow prevRow = row;
          MoveNext(ref row, ref columnIndex);
          if (row != prevRow)
          {
            spl.IncPercent();
            if (row != null)
              Owner.DoGetRowAttributes(row.Index, EFPDataGridViewAttributesReason.View);
          }
        }
      }
      finally
      {
        spl.Close();
      }
      if (!found)
        EFPApp.MessageBox("Строка \"" + SearchInfo.Text + "\" не найдена", "Поиск текста");
    }

    /// <summary>
    /// Проверяем, подходит ли очередная ячейка под условие
    /// Перед вызовом метода должен быть вызов DoGetRowAttributes
    /// </summary>
    /// <param name="row"></param>
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    private bool TestCell(DataGridViewRow row, int columnIndex)
    {
#if DEBUG
      if (String.IsNullOrEmpty(SearchInfo.Text))
        throw new NullReferenceException("Не задан текст для поиска");
#endif

      //DataGridViewCell Cell = Row.Cells[ColumnIndex];
      //object v = Cell.FormattedValue; 
      // 25.03.2015
      // Не работал поиск по вычисляемым полям

      EFPDataGridViewCellAttributesEventArgs cellArgs = Owner.DoGetCellAttributes(columnIndex);
      object v = cellArgs.FormattedValue;
      string s;
      if (v == null)
        s = String.Empty;
      else
        s = v.ToString();
      return SearchInfo.TextMatch(s);
    }

    /// <summary>
    /// Переход к следующей / предыдущей ячейке или строке
    /// </summary>
    /// <param name="row"></param>
    /// <param name="columnIndex"></param>
    private void MoveNext(ref DataGridViewRow row, ref int columnIndex)
    {
      // 17.05.2016
      // В mono не реализованы методы DataGridViewColumnCollection.GetXXXColumn().
      // Методы в DataGridViewRowCollection реализованы
      DataGridViewColumn[] visibleColumns = WinFormsTools.GetOrderedVisibleColumns(Owner.Control);


      if (!SearchInfo.CurrentColumn)
      {
        // Разрешено перемещение между столбами
        DataGridViewColumn col = Owner.Control.Columns[columnIndex];

        /*
        if (SearchInfo.Backward)
          Col = Owner.Control.Columns.GetPreviousColumn(Col,
            DataGridViewElementStates.Visible, DataGridViewElementStates.None);
        else
          Col = Owner.Control.Columns.GetNextColumn(Col,
            DataGridViewElementStates.Visible, DataGridViewElementStates.None);
          
        if (Col != null)
        {
          ColumnIndex = Col.Index;
          return;
        }
         */

        int viewIndex = Array.IndexOf<DataGridViewColumn>(visibleColumns, col);
        if (SearchInfo.Backward)
        {
          if (viewIndex >= 1)
          {
            col = visibleColumns[viewIndex - 1];
            columnIndex = col.Index;
            return;
          }
        }
        else
        {
          if (viewIndex < (visibleColumns.Length - 1))
          {
            col = visibleColumns[viewIndex + 1];
            columnIndex = col.Index;
            return;
          }
        }
      }
      // Нужен переход на другую строку
      if (!SearchInfo.CurrentColumn)
      {
        DataGridViewColumn col;
        /*
        if (SearchInfo.Backward)
          Col = Owner.Control.Columns.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);
        else
          Col = Owner.Control.Columns.GetFirstColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);
         * */
        if (SearchInfo.Backward)
          col = visibleColumns[visibleColumns.Length - 1];
        else
          col = visibleColumns[0];
        columnIndex = col.Index;
      }

      int rowIndex = row.Index;
      if (SearchInfo.Backward)
        rowIndex = Owner.Control.Rows.GetPreviousRow(rowIndex, DataGridViewElementStates.Visible);
      else
        rowIndex = Owner.Control.Rows.GetNextRow(rowIndex, DataGridViewElementStates.Visible);
      if (rowIndex >= 0)
        row = Owner.Control.Rows[rowIndex];
      else
        row = null;
    }

    #endregion

    #region Вспомогательные методы поиска

    /// <summary>
    /// Возвращает массив индексов строк, подходящих по условию поиска
    /// </summary>
    /// <returns></returns>
    public int[] FindAllRowIndices()
    {
      List<int> rowIndices = new List<int>();
      Splash spl = new Splash("Поиск всех подходящих строк");
      try
      {
        spl.PercentMax = Owner.Control.RowCount;
        spl.AllowCancel = true;

        for (int i = 0; i < Owner.Control.RowCount; i++)
        {
          Owner.DoGetRowAttributes(i, EFPDataGridViewAttributesReason.View);
          bool flag;
          if (SearchInfo.CurrentColumn)
            flag = TestCell(Owner.Control.Rows[i], Owner.CurrentColumnIndex);
          else
          {
            flag = false;
            DataGridViewColumn col = Owner.Control.Columns.GetFirstColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);
            while (col != null)
            {
              if (TestCell(Owner.Control.Rows[i], col.Index))
              {
                flag = true;
                break;
              }
              col = Owner.Control.Columns.GetNextColumn(col, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
            }
          }
          if (flag)
            rowIndices.Add(i);

          spl.IncPercent();
        }
      }
      finally
      {
        spl.Close();
      }

      return rowIndices.ToArray();
    }

    #endregion
  }

  /// <summary>
  /// Контекст для поиска текста в станартном древовидном просмотре TreeView.
  /// </summary>
  public class EFPTreeViewSearchContext : EFPTextSearchContextBase, IEFPTextSearchContext
  {
    #region Конструктор

    /// <summary>
    /// Создает контекст поиска
    /// </summary>
    /// <param name="owner">Провайдер иерархического просмотра</param>
    public EFPTreeViewSearchContext(EFPTreeView owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер иерархического просмотра
    /// </summary>
    public EFPTreeView Owner { get { return _Owner; } }
    private EFPTreeView _Owner;

    #endregion

    #region Методы

    /// <summary>
    /// Начать поиск
    /// </summary>
    public void StartSearch()
    {
      if (Owner.Control.Nodes.Count == 0)
      {
        EFPApp.ShowTempMessage("Просмотр не содержит строк");
        return;
      }

      if (!QueryParams(SearchInfo))
        return;

      SearchInfo.CopyTo(DefaultSearchInfo);

      SetContinueEnabled();
      // Сбрасываем поиск по первым буквам
      // TODO: !! Owner.CurrentIncSearchColumn = null;

      // Пока работал диалог, в просмотре могли произойти изменения
      if (Owner.Control.Nodes.Count == 0)
      {
        EFPApp.ShowTempMessage("Просмотр не содержит строк");
        return;
      }

      TreeNode node;
      if (SearchInfo.FromCurrent)
        node = Owner.Control.SelectedNode;
      else
      {
        if (SearchInfo.Backward)
          node = Owner.LastTreeNode;
        else
          node = Owner.FirstTreeNode;
      }

      DoSearch(node, true);
    }

    /// <summary>
    /// Показывает блок диалога поиска текста.
    /// </summary>
    /// <param name="searchInfo">Параметры поиска</param>
    /// <returns>true, если пользователь нажал кнопку "ОК"</returns>
    protected virtual bool QueryParams(EFPTextSearchInfo searchInfo)
    {
      using (EFPTextSearchForm frm = new EFPTextSearchForm())
      {
        frm.efpWhere.Visible = false;
        frm.grpWhere.Visible = false;
        frm.SetValues(searchInfo);
        if (EFPApp.ShowDialog(frm, false) != DialogResult.OK)
          return false;

        frm.GetValues(searchInfo);
      }
      return true;
    }

    /// <summary>
    /// Продолжить поиск с текущими параметрами
    /// </summary>
    public void ContinueSearch()
    {
      if (!ContinueEnabled)
      {
        EFPApp.ShowTempMessage("Поиск не был начат");
        return;
      }
      if (Owner.Control.SelectedNode == null)
      {
        EFPApp.ShowTempMessage("Невозможно продолжить поиск, т.к. нет текущей строки");
        return;
      }

      DoSearch(Owner.Control.SelectedNode, false);
    }

    private void DoSearch(TreeNode node, bool firstSearch)
    {
      if (!firstSearch)
        MoveNext(ref node);

      bool found = false;
      Splash spl = new Splash("Поиск текста");
      spl.AllowCancel = true;

      try
      {
        while (node != null)
        {
          //int NodeIndex = node.Index;

          spl.CheckCancelled();
          if (TestCell(node))
          {
            Owner.Control.SelectedNode = node;
            found = true;
            break;
          }
          MoveNext(ref node);
        }
      }
      finally
      {
        spl.Close();
      }
      if (!found)
        EFPApp.MessageBox("Строка \"" + SearchInfo.Text + "\" не найдена", "Поиск текста");
    }

    /// <summary>
    /// Проверяем, подходит ли очередная строка под условие
    /// Перед вызовом метода должен быть вызов DoGetRowAttributes
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool TestCell(TreeNode node)
    {
#if DEBUG
      if (String.IsNullOrEmpty(SearchInfo.Text))
        throw new NullReferenceException("Не задан текст для поиска");
#endif

      if (SearchInfo.TextMatch(node.Text))
        return true;
      return false;
    }

    /// <summary>
    /// Переход к следующей / предыдущей ячейке или строке
    /// </summary>
    /// <param name="node"></param>
    private void MoveNext(ref TreeNode node)
    {
      if (SearchInfo.Backward)
        node = Owner.GetPreviousTreeNode(node);
      else
        node = Owner.GetNextTreeNode(node);
    }

    #endregion

    #region Вспомогательные методы поиска

    /// <summary>
    /// Найти все узлы, удовлетворяющие параметрам поиска
    /// </summary>
    /// <returns>Массив узлов</returns>
    protected TreeNode[] FindAllNodes()
    {
      List<TreeNode> lst = new List<TreeNode>();
      Splash spl = new Splash("Поиск всех подходящих строк");
      try
      {
        // spl.PercentMax = Owner.Control.Nod;
        spl.AllowCancel = true;

        for (TreeNode node = Owner.FirstTreeNode; node != null; node = Owner.GetNextTreeNode(node))
        {
          if (TestCell(node))
            lst.Add(node);

          //spl.IncPercent();
        }
      }
      finally
      {
        spl.Close();
      }

      return lst.ToArray();
    }

    #endregion
  }

  /// <summary>
  /// Контекст для поиска текста в древовидном просмотре TreeViewAdv.
  /// В отличие от EFPDataGridViewSearchContext, поиск по отдельным столбцам не поддерживается,
  /// то есть режима "Во всех столбцах / в текущем столбце" нет
  /// </summary>
  public class EFPTreeViewAdvSearchContext : EFPTextSearchContextBase, IEFPTextSearchContext
  {
    #region Конструктор

    /// <summary>
    /// Создает контекст поиска
    /// </summary>
    /// <param name="owner">Провайдер иерархического просмотра</param>
    public EFPTreeViewAdvSearchContext(EFPTreeViewAdv owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    public EFPTreeViewAdv Owner { get { return _Owner; } }
    private EFPTreeViewAdv _Owner;

    #endregion

    #region Методы

    /// <summary>
    /// Начать поиск
    /// </summary>
    public void StartSearch()
    {
      if (Owner.Control.ItemCount == 0)
      {
        EFPApp.ShowTempMessage("Просмотр не содержит строк");
        return;
      }

      if (!QueryParams(SearchInfo))
        return;

      SearchInfo.CopyTo(DefaultSearchInfo);

      SetContinueEnabled();
      // Сбрасываем поиск по первым буквам
      // TODO: !! Owner.CurrentIncSearchColumn = null;

      // Пока работал диалог, в просмотре могли произойти изменения
      if (Owner.Control.ItemCount == 0)
      {
        EFPApp.ShowTempMessage("Просмотр не содержит строк");
        return;
      }

      TreeNodeAdv node;
      if (SearchInfo.FromCurrent)
        node = Owner.Control.CurrentNode;
      else
      {
        if (SearchInfo.Backward)
          node = Owner.LastTreeNode;
        else
          node = Owner.FirstTreeNode;
      }

      DoSearch(node, true);
    }

    /// <summary>
    /// Показывает блок диалога поиска текста
    /// </summary>
    /// <param name="searchInfo">Параметры поиска</param>
    /// <returns>true, если пользователь нажал кнопку "ОК"</returns>
    protected virtual bool QueryParams(EFPTextSearchInfo searchInfo)
    {
      using (EFPTextSearchForm frm = new EFPTextSearchForm())
      {
        frm.efpWhere.Visible = false;
        frm.grpWhere.Visible = false;
        frm.SetValues(searchInfo);
        if (EFPApp.ShowDialog(frm, false) != DialogResult.OK)
          return false;

        frm.GetValues(searchInfo);
      }
      return true;
    }

    /// <summary>
    /// Продолжить поиск с текущими параметрами
    /// </summary>
    public void ContinueSearch()
    {
      if (!ContinueEnabled)
      {
        EFPApp.ShowTempMessage("Поиск не был начат");
        return;
      }
      if (Owner.Control.CurrentNode == null)
      {
        EFPApp.ShowTempMessage("Невозможно продолжить поиск, т.к. нет текущей строки");
        return;
      }

      DoSearch(Owner.Control.CurrentNode, false);
    }

    private void DoSearch(TreeNodeAdv node, bool firstSearch)
    {
      if (!firstSearch)
        MoveNext(ref node);

      bool found = false;
      Splash spl = new Splash("Поиск текста");
      spl.AllowCancel = true;

      try
      {
        while (node != null)
        {
          //int NodeIndex = node.Index;

          spl.CheckCancelled();
          if (TestCell(node))
          {
            Owner.Control.SelectedNode = node;
            found = true;
            break;
          }
          MoveNext(ref node);
        }
      }
      finally
      {
        spl.Close();
      }
      if (!found)
        EFPApp.MessageBox("Строка \"" + SearchInfo.Text + "\" не найдена", "Поиск текста");
    }

    /// <summary>
    /// Проверяем, подходит ли очередная строка под условие
    /// Перед вызовом метода должен быть вызов DoGetRowAttributes
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool TestCell(TreeNodeAdv node)
    {
#if DEBUG
      if (String.IsNullOrEmpty(SearchInfo.Text))
        throw new NullReferenceException("Не задан текст для поиска");
#endif

      foreach (NodeControl ctrl in Owner.Control.NodeControls)
      {
        BaseTextControl ctrl2 = ctrl as BaseTextControl;
        if (ctrl2 == null)
          continue;

        if (ctrl2.ParentColumn != null)
        {
          if (!ctrl2.ParentColumn.IsVisible)
            continue;
        }

        string s = ctrl2.GetLabel(node);

        if (SearchInfo.TextMatch(s))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Переход к следующей / предыдущей ячейке или строке
    /// </summary>
    /// <param name="node"></param>
    private void MoveNext(ref TreeNodeAdv node)
    {
      if (SearchInfo.Backward)
        node = Owner.GetPreviousTreeNode(node);
      else
        node = Owner.GetNextTreeNode(node);
    }

    #endregion

    #region Вспомогательные методы поиска

    /// <summary>
    /// Возвращает список всех узлов, удовлетворяющих параметрам поиска
    /// </summary>
    /// <returns></returns>
    protected TreeNodeAdv[] FindAllNodes()
    {
      List<TreeNodeAdv> lst = new List<TreeNodeAdv>();
      Splash spl = new Splash("Поиск всех подходящих строк");
      try
      {
        // spl.PercentMax = Owner.Control.Nod;
        spl.AllowCancel = true;

        foreach (TreeNodeAdv node in Owner.Control.AllNodes)
        {
          if (TestCell(node))
            lst.Add(node);

          //spl.IncPercent();
        }
      }
      finally
      {
        spl.Close();
      }

      return lst.ToArray();
    }

    #endregion
  }
}