using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Core;
using System.Diagnostics;

namespace FreeLibSet.Data.Docs
{

  #region Перечисление RefDocFilterMode

  /// <summary>
  /// Режимы работы <see cref="RefDocCommonFilter"/>.
  /// Поддерживается режим фильтрации по выбранным записям, режим исключения выбранных записей, проверки на "NULL" и "NOT NULL".
  /// </summary>
  public enum RefDocFilterMode
  {
    /// <summary>
    /// Фильтр не установлен
    /// </summary>
    NoFilter,

    /// <summary>
    /// Режим фильтрации по выбранным записям
    /// </summary>
    Include,

    /// <summary>
    /// Режим исключения выбранных записей
    /// </summary>
    Exclude,

    /// <summary>
    /// Фильтр по любой ссылке.
    /// Должно использоваться только для ссылочных полей, поддерживающих значение NULL (<see cref="DBxColumnStruct.Nullable"/>=true).
    /// </summary>
    NotNull,

    /// <summary>
    /// Фильтр по значению Null.
    /// Должно использоваться только для ссылочных полей, поддерживающих значение NULL (<see cref="DBxColumnStruct.Nullable"/>=true).
    /// </summary>
    Null
  }

  #endregion

  /// <summary>
  /// Фильтр по значению ссылочного поля на документ.
  /// Возможен фильтр по нескольким идентификаторам и режим "Исключить".
  /// Для полей, поддерживающих пустое значение (<see cref="DBxColumnStruct.Nullable"/>=true), возможны фильтры на NULL и NOT NULL.
  /// </summary>
  public class RefDocCommonFilter : OneColumnCommonFilter
  {
    #region Конструкторы

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docType">Описание вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocCommonFilter(DBxDocProvider docProvider, DBxDocType docType, string columnName)
      : base(columnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (docType == null)
        throw new ArgumentNullException("docType");

      _DocProvider = docProvider;
      _DocType = docType;
      _Mode = RefDocFilterMode.NoFilter;
    }

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docTypeName">Имя вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocCommonFilter(DBxDocProvider docProvider, string docTypeName, string columnName)
      : this(docProvider, docProvider.DocTypes[docTypeName], columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private readonly DBxDocProvider _DocProvider;

    /// <summary>
    /// Описание вида документа, из которого осуществляется выбор.
    /// Задается в конструкторе объекта фильтра.
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } }
    private readonly DBxDocType _DocType;

    /// <summary>
    /// Имя таблицы документа
    /// </summary>
    public string DocTypeName { get { return _DocType.Name; } }

    /// <summary>
    /// Режим фильтра
    /// </summary>
    public RefDocFilterMode Mode { get { return _Mode; } }
    private RefDocFilterMode _Mode;

    /// <summary>
    /// Идентификаторы документов, если установлен фильтр <see cref="RefDocFilterMode.Include"/> или <see cref="RefDocFilterMode.Exclude"/>.
    /// В список входят только непосредственно выбранные документы.
    /// </summary>
    public IdList SelectedDocIds { get { return _SelectedDocIds; } }
    private IdList _SelectedDocIds;

    /// <summary>
    /// Идентификаторы документов, если установлен фильтр <see cref="RefDocFilterMode.Include"/> или <see cref="RefDocFilterMode.Exclude"/>.
    /// Если вид документов поддерживает иерархию (установлено свойство <see cref="DBxDocTypeBase.TreeParentColumnName"/>), то в список входят как выбранные документы, так и все вложенные.
    /// Если иерархия не поддерживается, то возвращается <see cref="SelectedDocIds"/>
    /// </summary>
    public IdList DocIds 
    {
      get
      {
        if (_DocIds == null)
        {
          if (SelectedDocIds == null)
            return null;

          if (String.IsNullOrEmpty(DocType.TreeParentColumnName))
            _DocIds = SelectedDocIds;
          else
          {
            IdList list = new IdList();
            AddDocIds(list, SelectedDocIds); // рекурсивная процедура
            list.SetReadOnly();
            _DocIds = list;
          }
          Debug.Assert(_DocIds.Count >= _SelectedDocIds.Count);
        }
        return _DocIds;
      }
    }
    private IdList _DocIds;

    private void AddDocIds(IdList list, IdList docIds)
    {
      IdList newList = new IdList();
      foreach (Int32 docId in docIds)
      {
        if (list.Contains(docId))
          continue;
        newList.Add(docId);
        list.Add(docId);
      }

      if (newList.Count == 0)
        return; // нечего больше добавлять

      List<DBxFilter> filters = new List<DBxFilter>();
      filters.Add(new ValuesFilter(DocType.TreeParentColumnName, newList.ToArray()));
      if (DocProvider.DocTypes.UseDeleted)
        filters.Add(DBSDocType.DeletedFalseFilter);
      IdList children = DocProvider.GetIds(DocType.Name, AndFilter.FromList(filters));
      AddDocIds(list, children); // рекурсия
    }

    /// <summary>
    /// Установить или очистить фильтр
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    /// <param name="docIds">Массив идентификаторов выбранных документов, если применимо к выбранному режиму.
    /// Если неприменимо, аргумент игнорируется</param>
    public void SetFilter(RefDocFilterMode mode, Int32[] docIds)
    {
      IdList docIds2 = null;
      if (docIds != null)
        docIds2 = new IdList(docIds);
      SetFilter(mode, docIds2);
    }

    /// <summary>
    /// Установить или очистить фильтр.
    /// Эта перегрузка предназначена только для режимов <paramref name="mode"/>=NoFilter, Null и NotNull.
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    public void SetFilter(RefDocFilterMode mode)
    {
      SetFilter(mode, (IdList)null);
    }

    /// <summary>
    /// Установить или очистить фильтр
    /// </summary>
    /// <param name="mode">Режим фильтра</param>
    /// <param name="docIds">Список идентификаторов выбранных документов, если фильтр устанавливается</param>
    public void SetFilter(RefDocFilterMode mode, IdList docIds)
    {
      switch (mode)
      {
        case RefDocFilterMode.NoFilter:
          Clear();
          return;
        case RefDocFilterMode.Include:
        case RefDocFilterMode.Exclude:
          if (docIds == null)
            throw new ArgumentNullException("docIds");
          // 24.07.2019 - разрешаем пустой список идентификаторов
          //if (DocIds.Count == 0)
          //  throw new ArgumentException("Не задан список идентификаторов для фильтра", "DocIds");
          _Mode = mode;
          _SelectedDocIds = docIds;
          _SelectedDocIds.SetReadOnly();
          _DocIds = null;
          OnChanged();
          break;
        case RefDocFilterMode.NotNull:
        case RefDocFilterMode.Null:
          if (mode == _Mode)
            return;
          _Mode = mode;
          _SelectedDocIds = null;
          _DocIds = null;
          OnChanged();
          break;
        default:
          throw ExceptionFactory.ArgUnknownValue("mode", mode);
      }
    }

    /// <summary>
    /// Упрощенная установка фильтра по единственному значению. 
    /// Свойство возвращает идентификатор документа или поддокумента для ссылочного поля, 
    /// если установлен режим "включить" (<see cref="Mode"/>=<see cref="RefDocFilterMode.Include"/>) и задан один идентификатор в списке <see cref="DocIds"/>. 
    /// Возвращает 0 во всех остальных режимах.
    /// Установка значения свойства в ненулевое значение устанавливает фильтр по
    /// одному документу (<see cref="Mode"/>=<see cref="RefDocFilterMode.Include"/>), а в нулевое значение - очищает фильтр ((<see cref="Mode"/>=<see cref="RefDocFilterMode.NoFilter"/>)).
    /// </summary>
    public Int32 SingleDocId
    {
      get
      {
        if (_Mode == RefDocFilterMode.Include && _SelectedDocIds.Count == 1)
          return _SelectedDocIds.SingleId;
        else
          return 0;
      }
      set
      {
        if (value == SingleDocId)
          return;
        if (value == 0)
          Clear();
        else
          SetFilter(RefDocFilterMode.Include, new Int32[1] { value });
      }
    }

    /// <summary>
    /// Идентификатор базы данных.
    /// Используется в операциях с буфером обмена
    /// </summary>
    public override string DBIdentity
    {
      get { return DocProvider.DBIdentity; }
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Очистить фильтр
    /// </summary>
    public override void Clear()
    {
      if (IsEmpty)
        return;
      _Mode = RefDocFilterMode.NoFilter;
      _SelectedDocIds = null;
      _DocIds = null;
      OnChanged();
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get { return Mode == RefDocFilterMode.NoFilter; }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Int32 id = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return TestValue(id);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      switch (Mode)
      {
        case RefDocFilterMode.NoFilter:
          return null;
        case RefDocFilterMode.Include:
          if (_SelectedDocIds.Count == 0)
            return DummyFilter.AlwaysFalse; // 24.07.2019
          else
            return new IdsFilter(ColumnName, DocIds);
        case RefDocFilterMode.Exclude:
          if (_SelectedDocIds.Count == 0)
            return null; // 24.07.2019
          else
            return new NotFilter(new IdsFilter(ColumnName, DocIds));
        case RefDocFilterMode.NotNull:
          return new NotNullFilter(ColumnName, typeof(Int32));
        case RefDocFilterMode.Null:
          return new ValueFilter(ColumnName, null, typeof(Int32));
        default:
          throw new BugException("Unknown mode=" + Mode.ToString());
      }
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      RefDocFilterMode newMode = config.GetEnum<RefDocFilterMode>("Mode");
      Int32[] newDocIds = StdConvert.ToInt32Array(config.GetString("Ids"));
      SetFilter(newMode, newDocIds);
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetEnum("Mode", Mode);
      if (SelectedDocIds != null)
        config.SetString("Ids", StdConvert.ToString(SelectedDocIds.ToArray()));
      else
        config.Remove("Ids");
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля <see cref="OneColumnCommonFilter.ColumnName"/>, если в фильтре выбрано единственное значение при <see cref="Mode"/>=<see cref="RefDocFilterMode.Include"/>.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewValue(DBxExtValue docValue)
    {
      switch (Mode)
      {
        case RefDocFilterMode.Include:
          Int32 id = SingleDocId;
          if (id != 0)
            docValue.SetInteger(id);
          break;
        case RefDocFilterMode.Null:
          docValue.SetNull();
          break;
      }
    }

#pragma warning disable 1591

    public override bool CanAsCurrRow(DataRow row)
    {
      Int32 thisId = DataTools.GetInt(row, ColumnName);
      if (thisId == 0 || thisId == SingleDocId)
        return false;
      return true;
    }

    public override void SetAsCurrRow(DataRow row)
    {
      Int32 thisId = DataTools.GetInt(row, ColumnName);
      SingleDocId = thisId;
    }

#pragma warning restore 1591

    ///// <summary>
    ///// Вызывает DBxDocTextHandlers.GetTextValue() для получения текстового представления
    ///// </summary>
    ///// <param name="ColumnValues">Значения полей</param>
    ///// <returns>Текстовые представления значений</returns>
    //protected override string[] GetColumnStrValues(object[] ColumnValues)
    //{
    //  return new string[] { UI.TextHandlers.GetTextValue(DocTypeName, DataTools.GetInt(ColumnValues[0])) };
    //}

    #endregion

    #region Другие методы

    /// <summary>
    /// Проверить соответствие идентификатора фильтру
    /// </summary>
    /// <param name="id">Идентификатор</param>
    /// <returns>true, если идентификатор проходит условие фильтра</returns>
    public bool TestValue(Int32 id)
    {
      switch (Mode)
      {
        case RefDocFilterMode.NoFilter:
          return true;
        case RefDocFilterMode.Include:
          return DocIds.Contains(id);
        case RefDocFilterMode.Exclude:
          return !DocIds.Contains(id);
        case RefDocFilterMode.NotNull:
          return id != 0;
        case RefDocFilterMode.Null:
          return id == 0;
        default:
          throw new BugException("Unknown mode " + Mode.ToString());
      }
    }

    #endregion
  }

  /// <summary>
  /// Набор из фильтра по группе и фильтра по документам
  /// </summary>
  public class RefDocCommonFilterSet : DBxCommonFilterSet
  {
    #region Конструкторы

    /// <summary>
    /// Создает набор из одного или двух фильтров
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docType">Описание вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocCommonFilterSet(DBxDocProvider docProvider, DBxDocType docType, string columnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (docType == null)
        throw new ArgumentNullException("docType");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      if (!String.IsNullOrEmpty(docType.GroupRefColumnName))
      {
        DBxColumnStruct groupIdCol = docType.Struct.Columns[docType.GroupRefColumnName];
        if (groupIdCol == null)
          throw new ArgumentException(String.Format(Res.RefDocCommonFilterSet_Arg_NoGroupIdColumn,
            docType.Name, docType.GroupRefColumnName), "docType");
        if (String.IsNullOrEmpty(groupIdCol.MasterTableName))
          throw new ArgumentException(String.Format(Res.RefDocCommonFilterSet_Arg_GroupIdColumnIsNotRef,
            docType.Name, docType.GroupRefColumnName), "docType");
        DBxDocType groupDocType = docProvider.DocTypes[groupIdCol.MasterTableName];
        if (groupDocType == null)
          throw new NullReferenceException(String.Format(Res.RefDocCommonFilterSet_Err_UnknownMasterTable, groupIdCol.MasterTableName));
        _GroupFilter = new RefDocCommonFilter(docProvider, groupDocType, columnName + "." + docType.GroupRefColumnName);
        _GroupFilter.DisplayName = groupDocType.SingularTitle;
        Add(_GroupFilter);
      }

      _DocFilter = new RefDocCommonFilter(docProvider, docType, columnName);
      Add(_DocFilter);
    }

    /// <summary>
    /// Создает набор из одного или двух фильтров
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="docTypeName">Имя вида документа, из которого осуществляется выбор</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    public RefDocCommonFilterSet(DBxDocProvider docProvider, string docTypeName, string columnName)
      : this(docProvider, docProvider.DocTypes[docTypeName], columnName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Фильтр по группе документов.
    /// Если вид документов, на которое ссылается поле, не использует группы, свойство возвращает null
    /// </summary>
    public RefDocCommonFilter GroupFilter { get { return _GroupFilter; } }
    private readonly RefDocCommonFilter _GroupFilter;

    /// <summary>
    /// Основной фильтр
    /// </summary>
    public RefDocCommonFilter DocFilter { get { return _DocFilter; } }
    private readonly RefDocCommonFilter _DocFilter;

    #endregion
  }

  /// <summary>
  /// Фильтр по полю "GroupId" в таблице документов.
  /// Возможные режимы фильтрации:
  /// <list type="bullet">
  /// <item><description>Для выбранной группы и вложенных групп</description></item>
  /// <item><description>Для выбранной группы без вложенных групп</description></item>
  /// <item><description>Для документов без группы</description></item>
  /// Использует SQL-фильтр <see cref="IdsFilter"/> (по списку идентификаторов групп, если выбраны вложенные группы) или
  /// <see cref="ValueFilter"/> (если фильтр по одной группе).
  /// </list>
  /// </summary>
  public class RefGroupDocCommonFilter : OneColumnCommonFilter
  {
    #region Конструктор

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="groupDocType">Описание вида документа групп</param>
    /// <param name="groupRefColumnName">Имя ссылочного поля, определяющего группу</param>
    public RefGroupDocCommonFilter(DBxDocProvider docProvider, DBxDocType groupDocType, string groupRefColumnName)
      : base(groupRefColumnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (groupDocType == null)
        throw new ArgumentNullException("groupDocType");
      if (String.IsNullOrEmpty(groupDocType.TreeParentColumnName))
        throw new ArgumentException(String.Format(Res.RefGroupDocCommonFilter_Arg_NoTreeParentColumnName, groupDocType.Name));

      _DocProvider = docProvider;
      _GroupDocType = groupDocType;

      base.DisplayName = groupDocType.SingularTitle; // лучше, чем "GroupId"

      _GroupId = 0;
      _IncludeNestedGroups = true;
    }

    /// <summary>
    /// Создает новый фильтр
    /// </summary>
    /// <param name="docProvider">Провайдер для доступа к документам</param>
    /// <param name="groupDocTypeName">Имя вида документов дерева групп</param>
    /// <param name="groupRefColumnName">Имя ссылочного поля, определяющего группу</param>
    public RefGroupDocCommonFilter(DBxDocProvider docProvider, string groupDocTypeName, string groupRefColumnName)
      : this(docProvider, docProvider.DocTypes[groupDocTypeName], groupRefColumnName)
    {
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private readonly DBxDocProvider _DocProvider;

    /// <summary>
    /// Описание вида документа групп.
    /// Задается в конструкторе объекта фильтра.
    /// </summary>
    public DBxDocType GroupDocType { get { return _GroupDocType; } }
    private readonly DBxDocType _GroupDocType;

    /// <summary>
    /// Имя таблицы документа групп.
    /// </summary>
    public string GroupDocTypeName { get { return _GroupDocType.Name; } }

    #endregion

    #region Текущие установки фильтра

    /// <summary>
    /// Идентификатор выбранной группы.
    /// Фильтр считается не установленным при <see cref="GroupId"/>=0 и <see cref="IncludeNestedGroups"/>=true.
    /// </summary>
    public Int32 GroupId
    {
      get { return _GroupId; }
      set
      {
        if (value == _GroupId)
          return;
        _GroupId = value;
        OnChanged();
      }
    }
    private Int32 _GroupId;

    /// <summary>
    /// Если true, то включаются также вложенные группы.
    /// Фильтр считается не установленным при <see cref="GroupId"/>=0 и <see cref="IncludeNestedGroups"/>=true.
    /// </summary>
    public bool IncludeNestedGroups
    {
      get { return _IncludeNestedGroups; }
      set
      {
        if (value == _IncludeNestedGroups)
          return;
        _IncludeNestedGroups = value;
        OnChanged();
      }
    }
    private bool _IncludeNestedGroups;

    /// <summary>
    /// Идентификатор базы данных.
    /// Используется в операциях с буфером обмена.
    /// </summary>
    public override string DBIdentity
    {
      get { return DocProvider.DBIdentity; }
    }

    #endregion

    #region Свойство AuxFilterGroupIds

    /// <summary>
    /// Возвращает массив идентификаторов отфильтрованных групп документов.
    /// Используется для фильтрации документов в основном просмотре.
    /// Если выбраны "Все документы", возвращает null.
    /// Если выбраны "Документы без групп", возвращает массив нулевой длины.
    /// Если есть выбранная группа, возвращает массив из одного или нескольких элементов,
    /// в зависимости от <see cref="IncludeNestedGroups"/>.
    /// </summary>
    public IdList AuxFilterGroupIdList
    {
      get
      {
        if (!_AuxFilterGroupIdsReady)
        {
          _AuxFilterGroupIdList = GetAuxFilterGroupIdList();
          _AuxFilterGroupIdsReady = true;
        }
        return _AuxFilterGroupIdList;
      }
    }

    private IdList _AuxFilterGroupIdList;

    /// <summary>
    /// Флажок устанавливается в true, если <see cref="AuxFilterGroupIdList"/> содержит корректное значение
    /// </summary>
    private bool _AuxFilterGroupIdsReady;

    private IdList GetAuxFilterGroupIdList()
    {
      if (GroupId == 0)
      {
        if (IncludeNestedGroups)
          return null;
        else
          return IdList.Empty;
      }
      else
      {
        if (IncludeNestedGroups)
        {
          DBxDocTreeModel model = new DBxDocTreeModel(DocProvider,
            GroupDocType,
            new DBxColumns(new string[] { "Id", GroupDocType.TreeParentColumnName }));

          return new IdList(model.GetIdWithChildren(GroupId));
        }
        else
          return IdList.FromId(GroupId);
      }
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Этот метод вызывается при изменении значения фильтра.
    /// </summary>
    protected override void OnChanged()
    {
      _AuxFilterGroupIdsReady = false;
      base.OnChanged();
    }

    /// <summary>
    /// Очистить фильтр.
    /// Устанавливает <see cref="GroupId"/>=0 и <see cref="IncludeNestedGroups"/>=true.
    /// </summary>
    public override void Clear()
    {
      if (IsEmpty)
        return;
      _GroupId = 0;
      _IncludeNestedGroups = true;
      OnChanged();
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен:
    /// <see cref="GroupId"/>=0 и <see cref="IncludeNestedGroups"/>=true.
    /// </summary>
    public override bool IsEmpty
    {
      get { return GroupId == 0 && IncludeNestedGroups; }
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Int32 id = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return TestValue(id);
    }

    /// <summary>
    /// Проверяет попадание значения поля в фильтр
    /// </summary>
    /// <param name="id">Проверяемое значение ссылочного поля</param>
    /// <returns>Результат проверки</returns>
    public bool TestValue(Int32 id)
    {
      if (GroupId == 0)
      {
        if (IncludeNestedGroups)
          return true;
        else
          return id == 0;
      }
      else
        return AuxFilterGroupIdList.Contains(id);
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (IncludeNestedGroups)
      {
        if (GroupId == 0)
          return null;
        else
          return new IdsFilter(ColumnName, AuxFilterGroupIdList);
      }
      else
        return new ValueFilter(ColumnName, GroupId);
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      GroupId = config.GetInt("GroupId");
      IncludeNestedGroups = config.GetBoolDef("IncludeNestedGroups", true);
    }

    /// <summary>
    /// Записать параметры фильтра в XML-конфигурацию
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetInt("GroupId", GroupId);
      config.SetBool("IncludeNestedGroups", IncludeNestedGroups);
    }

    /// <summary>
    /// Вызывается при создании нового документа из просмотра.
    /// Устанавливает начальное значение поля ColumnName, если в фильтре выбрано единственное значение.
    /// </summary>
    /// <param name="docValue">Значение поля, которое можно установить</param>
    protected override void OnInitNewValue(DBxExtValue docValue)
    {
      if (IncludeNestedGroups)
        return;

      docValue.SetInteger(GroupId);
    }

    #endregion
  }

  /// <summary>
  /// Фильтр по виду документа.
  /// Текущим значением числового поля является идентификатор таблицы документа <see cref="DBxDocType.TableId"/>.
  /// </summary>
  public class DocTableIdCommonFilter : OneColumnCommonFilter
  {
    // TODO: Подумать, может класс DocTableIdCommonFilter не нужен, а использовать RefDocCommonFilter?

    #region Конструктор

    /// <summary>
    /// Создает фильтр
    /// </summary>
    /// <param name="columnName">Столбец типа <see cref="Int32"/>, хранящий идентификатор вида документа из служебной таблицы "DocTables"</param>
    public DocTableIdCommonFilter(string columnName)
      : base(columnName)
    {
      _CurrentTableId = 0;
    }

    #endregion

    #region Текущее состояние

    /// <summary>
    /// Выбранный тип документа (значение <see cref="DBxDocType.TableId"/>) или 0, если фильтр не установлен.
    /// </summary>
    public Int32 CurrentTableId
    {
      get { return _CurrentTableId; }
      set
      {
        if (value == _CurrentTableId)
          return;
        _CurrentTableId = value;
        OnChanged();
      }
    }
    private Int32 _CurrentTableId;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public override void Clear()
    {
      CurrentTableId = 0;
    }

    /// <summary>
    /// Возвращает true, если фильтр не установлен
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return CurrentTableId == 0;
      }
    }

    /// <summary>
    /// Получение фильтра для фильтрации строк таблицы данных
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (CurrentTableId == 0)
        return null;
      return new ValueFilter(ColumnName, CurrentTableId);
    }

    /// <summary>
    /// Непосредственное тестирование фильтра исходя из переданных значений.
    /// Объект <paramref name="rowValues"/> должен содержать поле <see cref="OneColumnCommonFilter.ColumnName"/>, иначе будет сгенерирована ошибка.
    /// </summary>
    /// <param name="rowValues">Интерфейc доступа к значениям полей.</param>
    /// <returns>True, если строка проходит условия фильтра</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int v = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return v == CurrentTableId;
    }

    /// <summary>
    /// Прочитать значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void ReadConfig(CfgPart config)
    {
      CurrentTableId = config.GetInt("TableId");
    }

    /// <summary>
    /// Записать параметры фильтра в секцию конфигурации
    /// </summary>
    /// <param name="config">Секция конфигурации</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetInt("TableId", CurrentTableId);
    }

    ///// <summary>
    ///// Возвращает свойство DBxDocType.PluralTitle, если задан фильтр по виду документов.
    ///// </summary>
    ///// <param name="ColumnValues">Значения полей</param>
    ///// <returns>Текстовые представления значений</returns>
    //protected override string[] GetColumnStrValues(object[] ColumnValues)
    //{
    //  string s;
    //  Int32 ThisTableId = DataTools.GetInt(ColumnValues[0]);
    //  if (ThisTableId == 0)
    //    s = "Нет";
    //  else
    //  {
    //    DBxDocType DocType = UI.DocProvider.DocTypes.FindByTableId(ThisTableId);
    //    if (DocType == null)
    //      s = "Неизвестный тип документа с TableId=" + ThisTableId.ToString();
    //    else
    //    {
    //      s = DocType.PluralTitle;
    //      if (UI.DebugShowIds)
    //        s += " (TableId=" + DocType.TableId.ToString() + ")";
    //    }
    //  }
    //  return new string[] { s };
    //}

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка значения для фильтра отчета.
    /// Если фильтр не установлен (<see cref="IsEmpty"/>=true), возвращается true.
    /// </summary>
    /// <param name="rowValue">Проверяемое значение</param>
    /// <returns>true, если значение проходит условие фильтра</returns>
    public bool TestValue(Int32 rowValue)
    {
      if (IsEmpty)
        return true;
      return rowValue == CurrentTableId;
    }

    #endregion
  }
}
