// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Logging;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

/*
 * Расширенные параметры отчеты, использующие историю использования параметров.
 * Хранение именных пользовательских наборов параметров и истории.
 * Поддерживаются три типа параметров (перечисление EFPReportExtParamsPart):
 * - обычные параметры (User).
 * - параметры, не сохраняемые в истории, а всегда использующие последнее значение (NoHistory)
 * - параметры с путями к файлам и каталогам (Files). Они хранятся в истории. Если приложение
 *   является сетевым, то оно может хранить такие секции особым образом
 * Для хранения наборов используются отдельные секции конфигурации с категориями
 * "ReportParams" (для частей User и NoHistory) и "ReportFiles".
 * Для конкретного отчета задается, какие типы параметров он использует (свойство EFPReportExtParams.UsedParts).
 * Это позволяет не записывать лишние секции
 * 
 * Для определения секций с параметрами используется еще одна секция истории
 * с категорией "ReportParamsHistory"
 * В ней хранятся соответствия между именами секций в базе данных и 
 * пользовательскими именами секций. Также там хранятся время записи соответствующей
 * секции параметров и контрольная сумма
 * Данные в секции истории:
 * 
 * <Hist1>   // Имя секции в БД
 *    <WriteTime>22.10.2009 15:13:10</WriteTime> // Дата записи
 *    <MD5>0123456789abcdef0123456789abcdef<MD5> // Контрольная сумма
 * </Hist1>
 * ...
 * <Hist9>
 *    <WriteTime>22.10.2009 15:20:15</WriteTime>
 *    <MD5>0123456789abcdef0123456789abcdef<MD5> // Контрольная сумма
 * </Hist9>
 * 
 * <User1>   // Имя секции в БД
 *    <NamePart>Моя выборка</NamePart>                   // Название для пользователя
 *    <WriteTime>21.10.2009 09:35:00</WriteTime> // Дата записи
 *    <MD5>0123456789abcdef0123456789abcdef<MD5> // Контрольная сумма
 * </User1>
 * <User2>
 *    <NamePart>Тест</NamePart>
 *    <WriteTime>22.10.2009 10:40:08</WriteTime>
 *    <MD5>0123456789abcdef0123456789abcdef<MD5> 
 * </User2>
 * ...
 * 
 * Пользователь может удалять часть своих настроек, в результате часть секций
 * UserXxx могут стать неиспользуемыми. Они сохраняются в БД и будут перезаписаны,
 * если пользователь снова добавит набор.
 * Для удаленных наборов нет частей в секции истории
 * Контрольная сумма используется для начальной активации пользовательского
 * именного набора, если последний выбор совпадает с именным набором
 * 
 * Порядок расположения описаний Hist является важным. "Последний" набор - тот, который
 * расположен последним в списке, а не с самым большим WriteTime
 * 
 * Использование контрольной суммы.
 * Контрольная сумма не является жизненно необходимой частью хранения наборов, но она позволяет
 * правильно выделить последний или именной набор.
 * ParamSetComboBox позволяет использовать только одну контрольную сумму, а для хранения настроек
 * может использоваться две секции ("ReportParams" и "ReportFiles").
 * Сумма относится к частям EFPReportExtParamsPart.User и EFPReportExtParamsPart.Files.
 * Для поиска извлекаются данные только этих частей и помещаются во временную секцию
 */

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс формы для параметров отчета с использованимем расширенных
  /// возможностей.
  /// Форма содержит пустую панель <see cref="MainPanel"/>, которая должна быть заполнена
  /// (полями ввода или элементом <see cref="System.Windows.Forms.TabControl"/>) в производной реализации формы.
  /// Внизу формы есть панель с кнопками "ОК" и "Отмена", а также список с кнопками
  /// для выбора готовых наборов параметров.
  ///
  /// Если параметры отчета содержат только фильтры DBxClientFilters, используйте форму EFPReportFilterExtParamsForm.
  /// Также можно использовать форму с двумя вкладками <see cref="EFPReportExtParamsTwoPageForm"/>.
  /// </summary>
  public partial class EFPReportExtParamsForm : Form
  {
    #region Конструктор

    /// <summary>
    /// Создает форму
    /// </summary>
    public EFPReportExtParamsForm()
    {
      InitializeComponent();

      _InsideWriteFormValues = false;
      _InsideReadFormValues = false;

      if (/*!DesignMode*/ EFPApp.AppWasInit /* 31.07.2022 */)
      {
        _FormProvider = new EFPFormProvider(this);

        EFPButton efpOkButton = new EFPButton(FormProvider, btnOk);
        efpOkButton.ToolTipText = "Построить отчет с введенными параметрами";

        EFPButton efpCancelButton = new EFPButton(FormProvider, btnCancel);
        efpCancelButton.ToolTipText = "Отказаться от построения отчета";

        // Не знаю, как использовать EFPConfigParamSetComboBox. 
        // Интерфейс IEFPConfigParamSetHandler поддерживает только одну секцию конфигурации, а не несколько.

        SetComboBox.ShowImages = EFPApp.ShowListImages;
        EFPTextComboBox efpSelCB = new EFPTextComboBox(FormProvider, SetComboBox.TheCB);
        efpSelCB.CanBeEmpty = true;
        efpSelCB.DisplayName = "Готовые наборы";
        efpSelCB.ToolTipText = "Выбор готового набора параметров отчета из выпадающего списка." + Environment.NewLine +
          "В список входят пользовательские наборы, которые вы сохранили, а также до 9 последних наборов параметров построения отчета (история)" + Environment.NewLine + Environment.NewLine +
          "Поле для ввода названия для нового набора";

        EFPButton efpSaveButton = new EFPButton(FormProvider, SetComboBox.SaveButton);
        efpSaveButton.DisplayName = "Сохранить набор";
        efpSaveButton.ToolTipText = "Сохранить введенные параметры отчета как новый пользовательский набор." + Environment.NewLine +
          "Перед нажатием кнопки в поле слева должно быть введено имя набора";

        EFPButton efpDelButton = new EFPButton(FormProvider, SetComboBox.DeleteButton);
        efpDelButton.DisplayName = "Удалить набор";
        efpDelButton.ToolTipText = "Удалить пользовательский набор параметров, имя которого задано в списке слева";
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер формы
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private readonly EFPFormProvider _FormProvider;

    /// <summary>
    /// Комбоблок выбора готового набора параметров
    /// </summary>
    public FreeLibSet.Controls.ParamSetComboBox SetComboBox { get { return FSetComboBox; } }

    #endregion

    #region Обработка для полей даты

    /// <summary>
    /// Эту функцию следует вызывать из обработчика ValueChanged, чтобы оповестить
    /// о вводе пользователем параметров даты, которые не являются частью 
    /// именных пользовательских настроек.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void NotifyHistChanged(object sender, EventArgs args)
    {
      if (!_InsideWriteFormValues)
        HistChangedFlag = true;
    }

    /// <summary>
    /// Свойство возвращает true, если в данный момент переносятся данные из параметров в текущую форму, то есть работает метод <see cref="EFPReportExtParams.WriteFormValues(EFPReportExtParamsForm, SettingsPart)"/>.
    /// Это свойство можно использовать в обработчиках, отслеживающих изменение значений в управляющих эленментах формы.
    /// </summary>
    public bool InsideWriteFormValues { get { return _InsideWriteFormValues; } }
    internal bool _InsideWriteFormValues;

    /// <summary>
    /// Свойство возвращает true, если в данный момент переносятся данные из текущей формы в параметры отчета, то есть работает метод 
    /// <see cref="EFPReportExtParams.ReadFormValues(EFPReportExtParamsForm, SettingsPart)"/> или <see cref="EFPReportExtParams.ValidateReadFormValues(EFPReportExtParamsForm)"/>.
    /// </summary>
    public bool InsideReadFormValues { get { return _InsideReadFormValues; } }
    internal bool _InsideReadFormValues;

    internal bool HistChangedFlag;

    #endregion
  }

  /// <summary>
  /// Класс формы для параметров отчета, в котором есть только фильтры DBxClientFilters.
  /// возможностей
  /// Форма содержит только табличку фильтров <see cref="EFPGridFilterEditorGridView"/>. 
  /// Этот класс формы нельзя наследовать. Используйте для этого <see cref="EFPReportExtParamsForm"/>.
  /// </summary>
  public sealed class EFPReportFilterExtParamsForm : EFPReportExtParamsForm
  {
    #region Конструктор

    /// <summary>
    /// Создает форму
    /// </summary>
    public EFPReportFilterExtParamsForm()
    {
      EFPControlWithToolBar<DataGridView> cwt = new EFPControlWithToolBar<DataGridView>(FormProvider, base.MainPanel);
      _FiltersControlProvider = new EFPGridFilterEditorGridView(cwt);

    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра для таблицы редактирования фильтров.
    /// </summary>
    public EFPGridFilterEditorGridView FiltersControlProvider { get { return _FiltersControlProvider; } }
    private readonly EFPGridFilterEditorGridView _FiltersControlProvider;

    #endregion
  }


  /// <summary>
  /// Расширенные параметры отчета.
  /// Дополнительные виртуальные методы позволяют работать с формой ввода
  /// параметров, которая поддерживает список готовых наборов.
  /// </summary>
  public abstract class EFPReportExtParams : EFPReportParams
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект параметров отчета
    /// </summary>
    public EFPReportExtParams()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Используемые части для хранения параметров. 
    /// Может задаваться несколько секций.
    /// Непереопределенное свойство возвращает <see cref="SettingsPart.User"/> - все данные хранятся в одной секции, привязанной к пользователю.
    /// </summary>
    public virtual SettingsPart UsedParts { get { return SettingsPart.User; } }

    #endregion

    #region Дополнительные абстрактные методы

    /// <summary>
    /// Переопределенный метод должен возвращать новый объект формы параметров отчета, производной
    /// от <see cref="EFPReportExtParamsForm"/>
    /// </summary>
    /// <returns>Объект формы</returns>
    public abstract EFPReportExtParamsForm CreateForm();

    /// <summary>
    /// Переопределенный метод должен записать данные из текущих параметров отчета (этого объекта)
    /// в поля формы.
    /// Аргумент <paramref name="part"/> всегда содержит единственное значение. Если <see cref="UsedParts"/> содержит комбинацию значений,
    /// метод вызывается несколько раз. В реализации метода должен быть блок switch(<paramref name="part"/>).
    /// Получаемый тип формы должен быть приведен к типу формы, которая была создана в методе <see cref="CreateForm()"/>.
    /// </summary>
    /// <param name="form">Форма для редактирования параметров</param>
    /// <param name="part">Какую часть параметров нужно записать</param>
    public abstract void WriteFormValues(EFPReportExtParamsForm form, SettingsPart part);

    /// <summary>
    /// Переопределенный метод должен прочитать данные из полей формы и сохранить их в этом объекте
    /// Аргумент <paramref name="part"/> всегда содержит единственное значение. Если <see cref="UsedParts"/> содержит комбинацию значений,
    /// метод вызывается несколько раз. В реализации метода должен быть блок switch(<paramref name="part"/>).
    /// Получаемый тип формы должен быть приведен к типу формы, которая была создана в <see cref="CreateForm()"/>
    /// </summary>
    /// <param name="form">Форма для редактирования параметров</param>
    /// <param name="part">Какую часть параметров нужно прочитать</param>
    public abstract void ReadFormValues(EFPReportExtParamsForm form, SettingsPart part);

    /// <summary>
    /// Этот метод вызывается после того, как текущие значения загружены из формы с помощью одного
    /// или нескольких вызовов <see cref="ReadFormValues(EFPReportExtParamsForm, SettingsPart)"/>.
    /// Метод может проверить корректность значений и, при наличии ошибок, выдать соответствуюшее сообщение
    /// (обязательно) и установить фокус ввода на подходящий управляющий элемент.
    /// Метод вызывается при нажатии кнопки ОК и перед сохранением пользовательского набора параметров из
    /// <see cref="EFPFormProvider.ValidateForm()"/>.
    /// </summary>
    /// <remarks>
    /// Переопределите метод, если недостаточно возможностей проверки управляюших элементов формы,
    /// а требуется комплексная проверка введенных значений, уже после считывания параметров отчета.
    /// Повторная проверка того, что проверяется внутри <see cref="EFPControlBase.Validate()"/>, не требуется.
    /// При переопределении, вызывать этот базовый метод не нужно.
    /// </remarks>
    /// <param name="form">Форма для редактирования параметров</param>
    /// <returns>true, если введенные значения корректны</returns>
    public virtual bool ValidateReadFormValues(EFPReportExtParamsForm form)
    {
      return true;
    }



    /// <summary>
    /// Переопределенный метод должен записать данные из текущих параметров отчета (этого объекта)
    /// в указанную секцию конфигурации.
    /// Аргумент <paramref name="part"/> всегда содержит единственное значение. Если <see cref="UsedParts"/> содержит комбинацию значений,
    /// метод вызывается несколько раз. В реализации метода должен быть блок switch(<paramref name="part"/>).
    /// </summary>
    /// <param name="cfg">Секция конфигурации, открытая в режиме записи, куда должны быть помещены значения</param>
    /// <param name="part">Какую часть параметров нужно записать</param>
    public abstract void WriteConfig(CfgPart cfg, SettingsPart part);

    /// <summary>
    /// Переопределенный метод должен прочитать данные из указанной секции конфигурации и поместить их
    /// в текущие параметры отчета (этот объекта)
    /// Аргумент <paramref name="part"/> всегда содержит единственное значение. Если <see cref="UsedParts"/> содержит комбинацию значений,
    /// метод вызывается несколько раз. В реализации метода должен быть блок switch(<paramref name="part"/>).
    /// </summary>
    /// <param name="cfg">Секция конфигурации, открытая в режиме чтения, откуда должны быть извлечены значения</param>
    /// <param name="part">Какую часть параметров нужно прочитать</param>
    public abstract void ReadConfig(CfgPart cfg, SettingsPart part);

    /// <summary>
    /// Этот метод вызывается перед выводом формы параметров. Значения загружены
    /// в набор этот параметров (<see cref="ReadConfig(CfgPart)"/>, но еще не переданы форме
    /// (<see cref="WriteFormValues(EFPReportExtParamsForm, SettingsPart)"/> не вызывался).
    /// Метод не вызывается в процессе показа формы при выборе готового набора.
    /// </summary>
    public virtual void BeforeQueryParams()
    {
    }

    /// <summary>
    /// Этот метод вызывается после того, как в форме нажата кнопка "ОК" и значения
    /// из формы прочитаны (<see cref="ReadFormValues(EFPReportExtParamsForm, SettingsPart)"/> вызван), но данные еще не сохранены
    /// (перед вызовом <see cref="WriteConfig(CfgPart)"/>). 
    /// Метод не вызывается в процессе показа формы при сохранении пользовательского
    /// набора
    /// </summary>
    /// <remarks>
    /// При переопределении, вызывать этот базовый метод не обязательно
    /// </remarks>
    public virtual void AfterQueryParams()
    {
    }

    /// <summary>
    /// Возвращает дополнительный текст для выпадающего списка "Готовые наборы"
    /// Если метод переопределен, то он должен возвращать одну строку текста, которая отображается в списке истории под каждым элементом.
    /// Непереопределенный метод возвращает null, что означает отсутствие дополнительного текста.
    /// Сначала метод вызывается до показа диалога параметров и до чтения секций конфигурации. Если он возвращает значение, отличное от null
    /// (в том числе, пустую строку), то в выпадающем списке "Готовые наборы" будет место для дополнительного текста и полосатая раскраска.
    /// Также, при открытии списка, будет выполняться последовательное чтение секций конфигурации для каждого набора, в том числе истории, и
    /// вызываться метод <see cref="GetAuxText()"/>. Если первый вызов вернул null, то повторные вызовы не выполняются.
    /// </summary>
    /// <returns>Текст дополнительной строки для списка "Готовые наборы" или null</returns>
    public virtual string GetAuxText()
    {
      return null;
    }

    #endregion

    #region Переопределенные метолы

    /// <summary>
    /// Заглушка для абстрактного метода базового класса.
    /// Этот метод вызывается только при записи композиции открытых окон.
    /// Он помещает отдельные части конфигурации в одну секцию.
    /// </summary>
    /// <param name="cfg">Не используется</param>
    public sealed override void WriteConfig(CfgPart cfg)
    {
      if (!EFPApp.InsideSaveComposition)
        throw new NotSupportedException("Эта перегрузка метода не должна вызываться для EFPReportExtParams");

      if ((UsedParts & SettingsPart.User) == SettingsPart.User)
        WriteConfig(cfg, SettingsPart.User);
      if ((UsedParts & SettingsPart.NoHistory) == SettingsPart.NoHistory)
        WriteConfig(cfg, SettingsPart.NoHistory);
      if ((UsedParts & SettingsPart.Machine) == SettingsPart.Machine)
        WriteConfig(cfg, SettingsPart.Machine);
    }

    /// <summary>
    /// Заглушка для абстрактного метода базового класса.
    /// Этот метод вызывается только при чтении композиции открытых окон.
    /// Он считывает отдельные части конфигурации из одной секции.
    /// </summary>
    /// <param name="cfg">Не используется</param>
    public sealed override void ReadConfig(CfgPart cfg)
    {
      if (!EFPApp.InsideLoadComposition)
        throw new NotSupportedException("Эта перегрузка метода не должна вызываться для EFPReportExtParams");

      if ((UsedParts & SettingsPart.User) == SettingsPart.User)
        ReadConfig(cfg, SettingsPart.User);
      if ((UsedParts & SettingsPart.NoHistory) == SettingsPart.NoHistory)
        ReadConfig(cfg, SettingsPart.NoHistory);
      if ((UsedParts & SettingsPart.Machine) == SettingsPart.Machine)
        ReadConfig(cfg, SettingsPart.Machine);
    }

    #endregion

    #region CheckSinglePart

    /// <summary>
    /// Вспомогательный метод для проверки аргументов.
    /// Генерирует исключение ArgumentException, если <paramref name="part"/> содержит более одной части или
    /// не содержит ни одной части.
    /// </summary>
    /// <param name="part">Перечислимое значение EFPReportExtParamsPart, которое должно ровно одну часть</param>
    public void CheckSinglePart(SettingsPart part)
    {
      switch (part)
      {
        case SettingsPart.User:
        case SettingsPart.NoHistory:
        case SettingsPart.Machine:
          break;
        case (SettingsPart)0:
          throw new ArgumentException("Часть не задана", "part");
        default:
          throw new ArgumentException("Одновременно задано несколько частей: " + part.ToString(), "part");
      }
    }

    #endregion
  }


  /// <summary>
  /// Вспомогательный класс, используемый в <see cref="EFPReport.PerformQueryExtParams()"/>,
  /// который осуществляет взаимодействие между объектом параметров и формой для
  /// их ввода.
  /// </summary>
  internal class EFPReportExtParamsHelper
  {
    #region Константы

    private const int GroupUser = 1;
    private const int GroupDefault = 2;
    private const int GroupHist = 3;

    #endregion

    #region Конструктор

    public EFPReportExtParamsHelper(EFPReport owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Внутренние поля

    /// <summary>
    /// Объект отчета (для доступа к параметрам и именам секции конфигурации)
    /// </summary>
    private EFPReport _Owner;

    /// <summary>
    /// Используется ли строка дополнительного текста
    /// </summary>
    private bool _UseAuxText;

    public EFPReportExtParams ReportParams { get { return (EFPReportExtParams)(_Owner.ReportParams); } }

    private EFPReportExtParamsForm _Form;

    /*
    /// <summary>
    /// Секция для хранения истории
    /// </summary>
    //CfgPart SectHist;
      */
    /// <summary>
    /// Здесь сохраняем начальные значения параметров (до считывания из секции конфигурации).
    /// Хранятся части User и Files
    /// Периоды времени не хранятся.
    /// </summary>
    CfgPart _CfgEmpty;

    /// <summary>
    /// Данные из секции SectHist, относящиеся к автоматически создаваемым записям, в виде таблицы
    /// </summary>
    DataTable _TableHist;

    /// <summary>
    /// Данные из секции SectHist, относящиеся к пользовательским записям, в виде таблицы
    /// </summary>
    DataTable _TableUser;

    #endregion

    #region Основной метод

    public bool PerformQueryParams()
    {
      bool res;

      #region Запоминаем данные "По умолчанию"

      _CfgEmpty = new TempCfg();
      SafeReadConfigParts(_CfgEmpty, SettingsPart.User | SettingsPart.Machine); // без этого часть параметров может быть задана неправильно
      SafeWriteConfigParts(_CfgEmpty, SettingsPart.User | SettingsPart.Machine);

      _UseAuxText = EFPApp.ShowParamSetAuxText && (!Object.ReferenceEquals(SafeGetAuxText(), null));

      #endregion

      _Form = ReportParams.CreateForm();

      try
      {
        _Form.Icon = EFPApp.MainImages.Icons[_Owner.MainImageKey];

        _Form.SetComboBox.ItemSelected += new ParamSetComboBoxItemEventHandler(SetComboBox_ItemSelected);
        _Form.SetComboBox.SaveClick += new ParamSetComboBoxSaveEventHandler(SetComboBox_SaveClick);
        _Form.SetComboBox.DeleteClick += new ParamSetComboBoxItemEventHandler(SetComboBox_DeleteClick);
        _Form.SetComboBox.CanDeleteItem += new ParamSetComboBoxItemCancelEventHandler(SetComboBox_CanDeleteItem);

        CreateSetsTables();

        FillSets(_Form.SetComboBox);

        string cfgCode;
        if (_TableHist.DefaultView.Count > 0)
          cfgCode = DataTools.GetString(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Code");
        else
          // Для совместимости с отчетами, не использующими историю
          cfgCode = String.Empty;

        SafeReadConfigParts(cfgCode, AllParts);
        if (!String.IsNullOrEmpty(cfgCode))
          SafeReadConfigParts(String.Empty, SettingsPart.NoHistory);
        try
        {
          ReportParams.BeforeQueryParams();
        }
        catch (Exception e) // 20.06.2017 Перехватываем ошибку
        {
          EFPApp.ShowException(e, "Вызов EFPReportExtParams.BeforeQueryParams()");
        }
        WriteFormValueParts(AllParts);

        _Form.HistChangedFlag = false;
        // ??? SectEditHistPart = SectData; // нужна при выборе пользовательского набора

        // Активация строки в списке
        TempCfg srchSect = new TempCfg();
        SafeWriteConfigParts(srchSect, SettingsPart.User | SettingsPart.Machine);
        _Form.SetComboBox.SelectedMD5Sum = srchSect.MD5Sum();

        _Form.FormProvider.FormChecks.Add(new UIValidatingEventHandler(CheckForm));

        res = EFPApp.ShowDialog(_Form, false) == DialogResult.OK;

        if (res)
        {
          // Вызваны в CheckForm()
          // ReadFormValueParts(AllParts);
          // ReportParams.AfterQueryParam();

          srchSect = new TempCfg();
          SafeWriteConfigParts(srchSect, SettingsPart.User | SettingsPart.Machine);
          string md5Sum = srchSect.MD5Sum();

          bool found = false;
          foreach (DataRowView drv in _TableHist.DefaultView)
          {
            if (DataTools.GetString(drv.Row, "MD5") == md5Sum)
            {
              drv.Row["Time"] = DateTime.Now;
              drv.Row["Order"] = DataTools.GetInt(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Order") + 1;
              found = true;
              break;
            }
          }

          if (!found)
          {
            // Новые данные записываем в другую секцию
            DataRow resRow = null;
            if (_TableHist.DefaultView.Count >= 9) // все позиции заняты
              resRow = _TableHist.DefaultView[0].Row;
            else
            {
              for (int i = 1; i <= 9; i++)
              {
                if (DataTools.FindOrAddPrimaryKeyRow(_TableHist, "Hist" + i.ToString(), out resRow))
                  break;
              }
            }
            cfgCode = DataTools.GetString(resRow, "Code");
            resRow["Time"] = DateTime.Now;
            resRow["MD5"] = md5Sum;
            if (_TableHist.Rows.Count > 0)
              resRow["Order"] = DataTools.GetInt(_TableHist.DefaultView[_TableHist.DefaultView.Count - 1].Row, "Order") + 1;
            else
              resRow["Order"] = 1;

            SafeWriteConfigParts(cfgCode, AllParts);
          }

          // Для совместимости с отчетами, не использующими историю
          SafeWriteConfigParts(String.Empty, AllParts);

          SaveSetsTables();
        }
      }
      finally
      {
        _Form.Dispose();
      }
      return res;
    }

    private void CheckForm(object sender, UIValidatingEventArgs args)
    {
      if (_Form.FormProvider.ValidateReason == EFPFormValidateReason.Shown)
        return;
      if (!ReadFormValueParts(AllParts))
        args.SetError("Заданы неверные параметры");
      if (_Form.FormProvider.ValidateReason == EFPFormValidateReason.Closing)
      {
        try
        {
          ReportParams.AfterQueryParams();
        }
        catch (Exception e) // 20.06.2017 Перехватываем ошибку
        {
          args.SetError("Ошибка вызова AfterQueryParams");
          EFPApp.ShowException(e, "Вызов EFPReportExtParams.AfterQueryParams()");
        }
      }
    }

    #endregion

    #region Чтение и запись информации о готовых наборах

    private void CreateSetsTables()
    {
      List<EFPConfigSectionInfo> preloadInfos = null;
      if (_UseAuxText)
        preloadInfos = new List<EFPConfigSectionInfo>();

      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(_Owner.ConfigSectionName,
        EFPConfigCategories.ReportHistory, String.Empty);
      CfgPart cfgHist;
      using (_Owner.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfgHist))
      {
        _TableHist = new DataTable();
        _TableHist.Columns.Add("Code", typeof(string));
        _TableHist.Columns.Add("Time", typeof(DateTime));
        _TableHist.Columns.Add("MD5", typeof(string));
        _TableHist.Columns.Add("Order", typeof(int));
        DataTools.SetPrimaryKey(_TableHist, "Code");

        _TableUser = new DataTable();
        _TableUser.Columns.Add("Code", typeof(string));
        _TableUser.Columns.Add("Name", typeof(string));
        _TableUser.Columns.Add("Time", typeof(DateTime));
        _TableUser.Columns.Add("MD5", typeof(string));
        DataTools.SetPrimaryKey(_TableUser, "Code");

        try
        {
          string[] names = cfgHist.GetChildNames();
          for (int i = 0; i < names.Length; i++)
          {
            if (names[i].StartsWith("Hist", StringComparison.Ordinal))
            {
              CfgPart cfgOne = cfgHist.GetChild(names[i], false);
              _TableHist.Rows.Add(names[i], cfgOne.GetNullableDateTime("Time"),
                cfgOne.GetString("MD5"), _TableHist.Rows.Count + 1);

              if (_UseAuxText)
                preloadInfos.Add(new EFPConfigSectionInfo(_Owner.ConfigSectionName, EFPConfigCategories.ReportParams, names[i]));
            }
            if (names[i].StartsWith("User", StringComparison.Ordinal))
            {
              CfgPart cfgOne = cfgHist.GetChild(names[i], false);
              _TableUser.Rows.Add(names[i], cfgOne.GetString("Name"), cfgOne.GetNullableDateTime("Time"),
                cfgOne.GetString("MD5"));

              if (_UseAuxText)
                preloadInfos.Add(new EFPConfigSectionInfo(_Owner.ConfigSectionName, EFPConfigCategories.ReportParams, names[i]));
            }
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка чтения списка истории параметров отчета");
        }

        if (_UseAuxText)
          _Owner.ConfigManager.Preload(preloadInfos.ToArray(), EFPConfigMode.Read);
      }
    }

    private void FillSets(ParamSetComboBox setComboBox)
    {
      string auxText = String.Empty;

      // Сначала - именные данные пользователя
      _TableUser.DefaultView.Sort = "Name";
      foreach (DataRowView drv in _TableUser.DefaultView)
      {
        string code = DataTools.GetString(drv.Row, "Code");
        //DateTime? dt = DataTools.GetNullableDateTime(drv.Row, "Time");
        if (_UseAuxText)
        {
          DoReadConfigParts(code);
          auxText = ReportParams.GetAuxText();
        }
        setComboBox.Items.Add(new ParamSetComboBoxItem(code,
        DataTools.GetString(drv.Row, "Name"), "User", null, GroupUser,
        DataTools.GetString(drv.Row, "MD5"), auxText));
      }

      // Затем - по умолчанию
      if (_UseAuxText)
      {
        DoReadConfigParts("Empty");
        auxText = ReportParams.GetAuxText();
      }
      _Form.SetComboBox.Items.Add(new ParamSetComboBoxItem("Empty", "(По умолчанию)", "No", null, GroupDefault, _CfgEmpty.MD5Sum(), auxText));

      // Последние - данные истории
      _TableHist.DefaultView.Sort = "Order";
      int cnt = 0;
      for (int i = _TableHist.DefaultView.Count - 1; i >= 0; i--)
      {
        DataRow row = _TableHist.DefaultView[i].Row;
        string code = DataTools.GetString(row, "Code");
        DateTime? dt = DataTools.GetNullableDateTime(row, "Time");
        cnt++;
        string name;
        switch (cnt)
        {
          case 1:
            name = "(Последний)";
            break;
          case 2:
            name = "(Предпоследний)";
            break;
          default:
            name = "(Предыдущий №" + cnt.ToString() + ")";
            break;
        }
        if (_UseAuxText)
        {
          DoReadConfigParts(code);
          auxText = ReportParams.GetAuxText();
        }
        ParamSetComboBoxItem item = new ParamSetComboBoxItem(code,
        name, "Time", dt, GroupHist,
        DataTools.GetString(row, "MD5"),
        auxText);
        setComboBox.Items.Add(item);
      }

    }

    private void SaveSetsTables()
    {
      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(_Owner.ConfigSectionName,
        EFPConfigCategories.ReportHistory, String.Empty);
      CfgPart cfgHist;
      using (_Owner.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfgHist))
      {
        cfgHist.Clear();
        foreach (DataRowView drv in _TableHist.DefaultView)
        {
          CfgPart cfgOne = cfgHist.GetChild(DataTools.GetString(drv.Row, "Code"), true);
          cfgOne.SetNullableDateTime("Time", DataTools.GetNullableDateTime(drv.Row, "Time"));
          cfgOne.SetString("MD5", DataTools.GetString(drv.Row, "MD5"));
        }
        foreach (DataRowView drv in _TableUser.DefaultView)
        {
          CfgPart cfgOne = cfgHist.GetChild(DataTools.GetString(drv.Row, "Code"), true);
          cfgOne.SetString("Name", DataTools.GetString(drv.Row, "Name"));
          cfgOne.SetNullableDateTime("Time", DataTools.GetNullableDateTime(drv.Row, "Time"));
          cfgOne.SetString("MD5", DataTools.GetString(drv.Row, "MD5"));
        }
      }
    }

    #endregion

    #region Обработчики для списка работы с готовыми наборами

    /// <summary>
    /// Здесь сохраняются параметры дат, введенных пользователем
    /// </summary>
    CfgPart _CfgEditHist;

    void SetComboBox_ItemSelected(object sender, ParamSetComboBoxItemEventArgs args)
    {
      if (_Form.HistChangedFlag)
      {
        if (_Form.FormProvider.ValidateForm())
        {
          if ((ReportParams.UsedParts & SettingsPart.NoHistory) == SettingsPart.NoHistory)
          {
            ReportParams.ReadFormValues(_Form, SettingsPart.NoHistory);
            _CfgEditHist = new TempCfg();
            ReportParams.WriteConfig(_CfgEditHist, SettingsPart.NoHistory);
          }
        }
        _Form.HistChangedFlag = false;
      }

      string cfgCode = args.Item.Code;
      DoReadConfigParts(cfgCode);

      WriteFormValueParts(AllParts);
    }

    /// <summary>
    /// Вызывает один или несколько раз метод EFPReportExtParams.ReadConfig(), чтобы прочитать конфигурацию для элемента списка "Готовые наборы".
    /// </summary>
    /// <param name="cfgCode">Имя "UserXXX", "HistXXX" или "Empty"</param>
    private void DoReadConfigParts(string cfgCode)
    {
      if (cfgCode == "Empty")
      {
        SafeReadConfigParts(_CfgEmpty, AllParts); // все части
      }
      else
      {
        EFPConfigSectionInfo configInfo1 = new EFPConfigSectionInfo(_Owner.ConfigSectionName,
          EFPConfigCategories.ReportParams, cfgCode);
        CfgPart cfgData;
        using (_Owner.ConfigManager.GetConfig(configInfo1, EFPConfigMode.Read, out cfgData))
        {
          if ((ReportParams.UsedParts & SettingsPart.User) == SettingsPart.User)
            ReportParams.ReadConfig(cfgData, SettingsPart.User); // все части
          if ((ReportParams.UsedParts & SettingsPart.NoHistory) == SettingsPart.NoHistory)
          {
            if (cfgCode.StartsWith("User", StringComparison.Ordinal) || cfgCode == "Empty") // TODO: 27.12.2020. Что имелось ввиду под "Empty"?
              ReportParams.ReadConfig(_CfgEditHist, SettingsPart.NoHistory); // используем введенные вручную даты
            else
              ReportParams.ReadConfig(cfgData, SettingsPart.NoHistory); // используем даты из истории
          }
        }

        if ((ReportParams.UsedParts & SettingsPart.Machine) == SettingsPart.Machine)
        {
          ReportParams.ReadConfig(cfgData, SettingsPart.Machine);
          EFPConfigSectionInfo ConfigInfo2 = new EFPConfigSectionInfo(_Owner.ConfigSectionName,
            EFPConfigCategories.ReportFiles, cfgCode);
          using (_Owner.ConfigManager.GetConfig(ConfigInfo2, EFPConfigMode.Read, out cfgData))
          {
            ReportParams.ReadConfig(cfgData, SettingsPart.Machine); // 30.08.2017
          }
        }
      }
    }

    //private void DoReadHistPart(CfgPart cfgData)
    //{
    //}

    void SetComboBox_SaveClick(object sender, ParamSetComboBoxSaveEventArgs args)
    {

      if (!_Form.FormProvider.ValidateForm())
        return;

      // Не надо вызывать, т.к. 
      //ReadFormValueParts(EFPReportExtParamsPart.User | EFPReportExtParamsPart.Files);

      ParamSetComboBoxItem oldItem = _Form.SetComboBox.Items.FindDisplayName(args.DisplayName);
      if (oldItem != null)
      {
        if (!oldItem.Code.StartsWith("User", StringComparison.Ordinal))
        {
          EFPApp.ShowTempMessage("Перезаписывать можно только пользовательские наборы. Введите название набора.");
          return;
        }
        if (EFPApp.MessageBox("Набор \"" + args.DisplayName + "\" уже существует. Вы хотите перезаписать его?",
          "Подтверждение перезаписи набора параметров",
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
          return;
      }

      if (args.DisplayName.StartsWith("(", StringComparison.Ordinal))
      {
        EFPApp.ShowTempMessage("Имя набора не может начинаться со скобки");
        return;
      }

      string cfgCode;
      if (oldItem != null)
      {
        cfgCode = oldItem.Code;
        _Form.SetComboBox.Items.Remove(oldItem);
      }
      else
      {
        int cnt = 1;
        while (true)
        {
          cfgCode = "User" + cnt.ToString();
          if (_TableUser.Rows.Find(cfgCode) == null)
            break;
          cnt++;
        }
      }

      //SectData.Clear();
      SafeWriteConfigParts(cfgCode, SettingsPart.User | SettingsPart.Machine);

      string md5Sum = CalcMD5Sum();
      string auxText = String.Empty;
      if (_UseAuxText)
        auxText = SafeGetAuxText();

      ParamSetComboBoxItem newItem = new ParamSetComboBoxItem(cfgCode, args.DisplayName, "User", null, GroupUser, md5Sum, auxText);
      _Form.SetComboBox.Items.Insert(0, newItem);
      _Form.SetComboBox.SelectedItem = newItem;
      DataRow Row = DataTools.FindOrAddPrimaryKeyRow(_TableUser, cfgCode);
      Row["Name"] = args.DisplayName;
      Row["Time"] = DateTime.Now;
      Row["MD5"] = newItem.MD5Sum;
      SaveSetsTables();
    }

    /// <summary>
    /// Вычисление контрольной суммы для текущих значений в секций User и Files 
    /// </summary>
    /// <returns></returns>
    private string CalcMD5Sum()
    {
      TempCfg cfg = new TempCfg();
      if ((ReportParams.UsedParts & SettingsPart.User) == SettingsPart.User)
        ReportParams.WriteConfig(cfg, SettingsPart.User);
      if ((ReportParams.UsedParts & SettingsPart.Machine) == SettingsPart.Machine)
        ReportParams.WriteConfig(cfg, SettingsPart.Machine);
      return cfg.MD5Sum();
    }

    void SetComboBox_DeleteClick(object sender, ParamSetComboBoxItemEventArgs args)
    {
      DataTable table;
      if (args.Item.Code.StartsWith("User", StringComparison.Ordinal))
        table = _TableUser;
      else if (args.Item.Code.StartsWith("Hist", StringComparison.Ordinal))
        table = _TableHist;
      else
      {
        EFPApp.ErrorMessageBox("Этот набор нельзя удалить", "Удаление готового набора");
        return;
      }

      DataRow row = table.Rows.Find(args.Item.Code);
      if (row == null)
      {
        BugException ex = new BugException("Набор с кодом \"" + args.Item.Code + "\" не найден");
        ex.Data["Item"] = args.Item;
        throw ex;
      }

      if (EFPApp.MessageBox("Удалить набор \"" + args.Item.DisplayName + "\"?",
        "Подтверждение удаления набора", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        return;

      table.Rows.Remove(row);
      SaveSetsTables();

      _Form.SetComboBox.Items.Remove(args.Item);
    }

    void SetComboBox_CanDeleteItem(object sender, ParamSetComboBoxItemCancelEventArgs args)
    {
      if (args.Item.Code.StartsWith("Hist", StringComparison.Ordinal) || args.Item.Code.StartsWith("User", StringComparison.Ordinal))
        args.Cancel = false;
      else
        args.Cancel = true;
    }

    #endregion

    #region Внутренние вспомогательные методы

    /// <summary>
    /// Массив для организации циклов
    /// </summary>
    private static readonly SettingsPart[] AllPartsArray = new SettingsPart[]{
      SettingsPart.User, 
      SettingsPart.NoHistory, 
      SettingsPart.Machine};

    const SettingsPart AllParts = SettingsPart.User |
      SettingsPart.NoHistory |
      SettingsPart.Machine;

    #region SafeRead/WriteConfigParts

    /// <summary>
    /// Чтение нескольких частей данных с перехватом исключений.
    /// Выполняет нужное число вызовов EFPReportExtParams.ReadConfig() для требуемых частей <paramref name="parts"/>, с учетом свойства UsedParts
    /// </summary>
    /// <param name="cfg">Секция конфигурации, открытая в режиме чтения</param>
    /// <param name="parts">Какие данные требуется прочитать</param>
    private void SafeReadConfigParts(CfgPart cfg, SettingsPart parts)
    {
      try
      {
        parts &= ReportParams.UsedParts;

        for (int i = 0; i < AllPartsArray.Length; i++)
        {
          if ((parts & AllPartsArray[i]) == AllPartsArray[i])
            ReportParams.ReadConfig(cfg, AllPartsArray[i]);
        }
      }
      catch (Exception e)
      {
        EFPApp.ErrorMessageBox("Возникла ошибка при загрузке ранее сохраненных параметров отчета. " +
          "Будут использованы значения по умолчанию. Сообщение об ошибке: " + e.Message,
          "Ошибка загрузки параметров");
      }
    }

    private void SafeReadConfigParts(string userSetName, SettingsPart parts)
    {
      parts &= ReportParams.UsedParts;
      if (!String.IsNullOrEmpty(userSetName))
        parts &= (~SettingsPart.NoHistory);
      CfgPart cfg;
      if ((ReportParams.UsedParts & (SettingsPart.User | SettingsPart.NoHistory)) != (SettingsPart)0)
      {
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_Owner.ConfigSectionName,
          EFPConfigCategories.ReportParams, userSetName);
        using (_Owner.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfg))
        {
          SafeReadConfigParts(cfg, parts & (SettingsPart.User | SettingsPart.NoHistory));
        }
      }
      if ((ReportParams.UsedParts & SettingsPart.Machine) != (SettingsPart)0)
      {
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_Owner.ConfigSectionName,
          EFPConfigCategories.ReportFiles, userSetName);
        using (_Owner.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfg))
        {
          SafeReadConfigParts(cfg, SettingsPart.Machine);
        }
      }
    }

    /// <summary>
    /// Запись нескольких частей данных с перехватом исключений.
    /// Выполняет нужное число вызовов EFPReportExtParams.WriteConfig() для требуемых частей <paramref name="parts"/>, с учетом свойства UsedParts
    /// </summary>
    /// <param name="cfg">Секция конфигурации, открытая в режиме записи</param>
    /// <param name="parts">Какие данные требуется записать</param>
    private void SafeWriteConfigParts(CfgPart cfg, SettingsPart parts)
    {
      try
      {
        parts &= ReportParams.UsedParts;

        for (int i = 0; i < AllPartsArray.Length; i++)
        {
          if ((parts & AllPartsArray[i]) == AllPartsArray[i])
            ReportParams.WriteConfig(cfg, AllPartsArray[i]);
        }
      }
      catch (Exception e)
      {
        EFPApp.ErrorMessageBox("Возникла ошибка при сохранении параметров отчета. " +
          "Сообщение об ошибке: " + e.Message,
          "Ошибка записи параметров");
      }
    }

    private void SafeWriteConfigParts(string userSetName, SettingsPart parts)
    {
      parts &= ReportParams.UsedParts;
      if (!String.IsNullOrEmpty(userSetName))
        parts &= (~SettingsPart.NoHistory);
      CfgPart cfg;
      if ((ReportParams.UsedParts & (SettingsPart.User | SettingsPart.NoHistory)) != (SettingsPart)0)
      {
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_Owner.ConfigSectionName,
          EFPConfigCategories.ReportParams, userSetName);
        using (_Owner.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfg))
        {
          SafeWriteConfigParts(cfg, parts & (SettingsPart.User | SettingsPart.NoHistory));
        }
      }
      if ((ReportParams.UsedParts & SettingsPart.Machine) != (SettingsPart)0)
      {
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(_Owner.ConfigSectionName,
          EFPConfigCategories.ReportFiles, userSetName);
        using (_Owner.ConfigManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfg))
        {
          SafeWriteConfigParts(cfg, SettingsPart.Machine);
        }
      }
    }

    private static bool _SafeGetAuxTextErrorLogged = false;

    private string SafeGetAuxText()
    {
      try
      {
        return ReportParams.GetAuxText();
      }
      catch (Exception e)
      {
        if (!_SafeGetAuxTextErrorLogged)
        {
          _SafeGetAuxTextErrorLogged = true;
          LogoutTools.LogoutException(e, "Ошибка вызова EFPReportExtParams.GetAuxText(). Повторные ошибки не сохраняются");
        }
        return "Ошибка. " + e.Message;
      }
    }

    #endregion

    #region Read/WriteFormValues

    private bool ReadFormValueParts(SettingsPart parts)
    {
      bool res;
      _Form._InsideReadFormValues = true;
      try
      {
        parts &= ReportParams.UsedParts;

        for (int i = 0; i < AllPartsArray.Length; i++)
        {
          if ((parts & AllPartsArray[i]) == AllPartsArray[i])
            ReportParams.ReadFormValues(_Form, AllPartsArray[i]);
        }

        res = ReportParams.ValidateReadFormValues(_Form);
      }
      finally
      {
        _Form._InsideReadFormValues = false;
      }
      return res;
    }

    private void WriteFormValueParts(SettingsPart parts)
    {
      _Form._InsideWriteFormValues = true;
      try
      {
        parts &= ReportParams.UsedParts;

        for (int i = 0; i < AllPartsArray.Length; i++)
        {
          if ((parts & AllPartsArray[i]) == AllPartsArray[i])
            ReportParams.WriteFormValues(_Form, AllPartsArray[i]);
        }
      }
      finally
      {
        _Form._InsideWriteFormValues = false;
      }
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Реализация класса параметров отчета, который содержит только фильтры DBxGridFilters.
  /// Список фильтров нужно передать конструктору объекта.
  /// Для редактирования параметров используется форма <see cref="EFPReportFilterExtParamsForm"/>.
  /// Этот класс нельзя переопределять.
  /// </summary>
  public sealed class EFPReportFilterExtParams : EFPReportExtParams
  {
    #region Конструктор

    /// <summary>
    /// Создает параметры отчета с заданным списком фильтров. Вызывает <see cref="IEFPGridFilters.SetReadOnly"/>.
    /// </summary>
    /// <param name="filters">Заполненный список фильтров. Не может быть null.</param>
    public EFPReportFilterExtParams(IEFPGridFilters filters)
    {
      if (filters == null)
        throw new ArgumentNullException("filters");
      filters.SetReadOnly();
      _Filters = filters;
    }

    /// <summary>
    /// Дополнительная перегрузка конструктора, которая позволяет сразу установить свойство <see cref="EFPReportParams.Title"/>.
    /// </summary>
    /// <param name="filters">Заполненный список фильтров. Не может быть null.</param>
    /// <param name="title">Заголовок для отчета и диалога параметров отчета</param>
    public EFPReportFilterExtParams(IEFPGridFilters filters, string title)
      : this(filters)
    {
      this.Title = title;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список фильтров. Задается в конструкторе
    /// </summary>
    public IEFPGridFilters Filters { get { return _Filters; } }
    private readonly IEFPGridFilters _Filters;

    /// <summary>
    /// Заголовок для формы запроса параметров.
    /// Если свойство не установлено в явном виде, используется свойство <see cref="EFPReportParams.Title"/>.
    /// </summary>
    public string FormTitle
    {
      get
      {
        if (_FormTitle == null)
          return Title;
        else
          return _FormTitle;
      }
      set { _FormTitle = value; }
    }
    private string _FormTitle;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Заполняет табличку фильтров
    /// </summary>
    protected override void OnInitTitle()
    {
      base.OnInitTitle();

      for (int i = 0; i < Filters.Count; i++)
      {
        if (!Filters[i].IsEmpty)
        {
          FilterInfo.Add(Filters[i].DisplayName, (Filters[i]).FilterText);
          if (EFPApp.ShowListImages)
          {
            IEFPGridFilterWithImageKey filter2 = Filters[i] as IEFPGridFilterWithImageKey;
            if (filter2 != null)
              FilterInfo.LastAdded.ImageKey = filter2.FilterImageKey;
          }
        }
      }
    }

    /// <summary>
    /// Создает <see cref="EFPReportFilterExtParamsForm"/>.
    /// </summary>
    /// <returns>Объект формы параметров отчета</returns>
    public override EFPReportExtParamsForm CreateForm()
    {
      EFPReportFilterExtParamsForm form = new EFPReportFilterExtParamsForm();

      if (_FormTitle == null)
        InitTitle(); // На случай, если используется событие TitleNeeded
      form.Text = FormTitle;
      return form;
    }

    /// <summary>
    /// Инициализирует список фильтров
    /// </summary>
    /// <param name="form"><see cref="EFPReportFilterExtParamsForm"/></param>
    /// <param name="part">Часть</param>
    public override void WriteFormValues(EFPReportExtParamsForm form, SettingsPart part)
    {
      EFPReportFilterExtParamsForm form2 = (EFPReportFilterExtParamsForm)form;
      form2.FiltersControlProvider.Filters = Filters;
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="form"></param>
    /// <param name="part"></param>
    public override void ReadFormValues(EFPReportExtParamsForm form, SettingsPart part)
    {
    }

    /// <summary>
    /// Записывает параметры отчета в секцию User
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void WriteConfig(FreeLibSet.Config.CfgPart cfg, SettingsPart part)
    {
      switch (part)
      {
        case SettingsPart.User:
          Filters.WriteConfig(cfg);
          break;
      }
    }

    /// <summary>
    /// Считывает параметры отчета
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void ReadConfig(FreeLibSet.Config.CfgPart cfg, SettingsPart part)
    {
      switch (part)
      {
        case SettingsPart.User:
          Filters.ReadConfig(cfg);
          break;
      }
    }

    #endregion
  }
}
