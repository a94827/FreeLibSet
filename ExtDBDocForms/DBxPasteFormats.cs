// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;
using System.Data;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;
using FreeLibSet.Forms.Diagnostics;

namespace FreeLibSet.Forms.Docs
{
  // Классы для реализации форматов вставки из буфера обмена

  /// <summary>
  /// Формат вставки из буфера обмена выборки документов <see cref="DBxDocSelection"/>.
  /// Возможность указания конкретного вида документов.
  /// </summary>
  public class DBxDocSelectionPasteFormat : EFPPasteFormat
  {
    #region Конструкторы

    /// <summary>
    /// Создает формат, который позволяет вставлять выборки документов любых видов.
    /// Используйте событие <see cref="EFPPasteFormat.TestFormat"/> для фильтрации.
    /// </summary>
    /// <param name="ui">Интерфейс пользователя для доступа к документам</param>
    public DBxDocSelectionPasteFormat(DBUI ui)
      : base(typeof(DBxDocSelection))
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      DisplayName = "Выборка документов";
    }

    /// <summary>
    /// Создает формат, который позволяет вставлять выборки документов заданного вида.
    /// </summary>
    /// <param name="ui">Интерфейс пользователя для доступа к документам</param>
    /// <param name="docTypeName">Имя таблицы вида документа. 
    /// Если задана пустая строка или null, позволяет вставлять выборки документов любых видов</param>
    public DBxDocSelectionPasteFormat(DBUI ui, string docTypeName)
      : this(ui)
    {
      _DocTypeName = docTypeName;
      if (!String.IsNullOrEmpty(docTypeName))
        DisplayName = "Документы \"" + DocType.DocType.PluralTitle + "\"";
    }

    /// <summary>
    /// Создает формат, который позволяет вставлять выборки документов заданного вида.
    /// </summary>
    /// <param name="docTypeUI">Интерфейс вида документов. Должен быть задан</param>
    public DBxDocSelectionPasteFormat(DocTypeUI docTypeUI)
      : this(docTypeUI.UI, docTypeUI.DocType.Name)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс пользователя для доступа к документам.
    /// Задается в конструкторе. Не может быть null.
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

    /// <summary>
    /// Если свойство установлено, то при проверке будет определяться наличие
    /// в выборке документов заданного вида.
    /// </summary>
    public string DocTypeName { get { return _DocTypeName; } }
    private readonly string _DocTypeName;

    /// <summary>
    /// Интерфейс доступа к документам выбранного вида, если свойство DocTypeName установлено.
    /// Иначе null.
    /// </summary>
    public DocTypeUI DocType
    {
      get
      {
        if (String.IsNullOrEmpty(_DocTypeName))
          return null;
        else
          return _UI.DocTypes[_DocTypeName];
      }
    }

    #endregion

    #region Проверка формата

    /// <summary>
    /// Выборка, извлеченная из буфера обмена при проверке формата
    /// </summary>
    public DBxDocSelection DocSel { get { return _DocSel; } }
    private DBxDocSelection _DocSel;

    /// <summary>
    /// Идентификатор первого документа заданного вида.
    /// Свойство устанавливается при проверке формата
    /// </summary>
    public Int32 DocId { get { return _DocId; } }
    private Int32 _DocId;

    /// <summary>
    /// Проверяет наличие в буфере обмена выборки документов (объекта <see cref="DBxDocSelection"/>).
    /// Проверяет совпадение свойства <see cref="DBxDocProvider.DBIdentity"/>.
    /// Если свойство <see cref="DocTypeName"/> установлено, проверяет наличие в выборке документов требуемого вида.
    /// Если все в порядке, вызывает событие <see cref="EFPPasteFormat.TestFormat"/> для выполнения окончательной проверки.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnTestFormat(EFPTestDataObjectEventArgs args)
    {
      _DocId = 0;
      _DocSel = (DBxDocSelection)(args.Data.GetData(typeof(DBxDocSelection)));
      if (_DocSel == null)
      {
        args.DataInfoText = "Буфер обмена не содержит выборки документов";
        args.DataImageKey = "No";
        args.Appliable=false;
        return;
      }

      if (_DocSel.DBIdentity != _UI.DocProvider.DBIdentity)
      {
        args.DataInfoText = "Выборка документов относится к другой базе данных";
        args.DataImageKey = "DifferentDatabase";
        //DataImageKey = "Error";
        args.Appliable = false;
        return;
      }

      if (String.IsNullOrEmpty(_DocTypeName))
      {
        args.DataInfoText = DisplayName;
        args.DataImageKey = "DBxDocSelection";
      }
      else
      {
        if (_DocSel.GetCount(_DocTypeName) == 0)
        {
          args.DataInfoText = "Буфер обмена не содержит выборки документов \"" + DocType.DocType.PluralTitle + "\"";
          args.DataImageKey = "No";
          args.Appliable = false;
          return;
        }

        _DocId = _DocSel.GetSingleId(_DocTypeName);

        int cnt = _DocSel.GetCount(DocTypeName);
        if (cnt == 1)
        {
          args.DataInfoText = "Документ \"" + DocType.DocType.SingularTitle + "\"";
          args.DataImageKey = DocType.GetImageKey(_DocId);
        }
        else
        {
          args.DataInfoText = "Документы \"" + DocType.DocType.PluralTitle + "\"";
          args.DataImageKey = DocType.TableImageKey;
        }
      }

      args.Appliable = true;
      base.OnTestFormatEvent(args);
    }

    #endregion

    #region Просмотр

#if XXXX
    protected override bool OnPreview(IDataObject Data, EFPPasteReason Reason)
    {
      if (HasPreviewHandler)
        return base.OnPreview(Data, Reason);

      AccDepClientExec.SendToDocSel(DocSel, PreviewTitle);
    }
#endif

    #endregion
  }

  /// <summary>
  /// Формат вставки из буфера обмена набора таблиц <see cref="DataSet"/>.
  /// Возможность указания имени таблицы документа или поддокумента.
  /// </summary>
  public class DBxDataSetPasteFormat : EFPPasteFormat
  {
    #region Конструкторы

    /// <summary>
    /// Создает формат без указания имени таблицы
    /// </summary>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public DBxDataSetPasteFormat(DBUI ui)
      : base(typeof(DataSet))
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      DisplayName = "Таблицы данных";
    }

    /// <summary>
    /// Создает формат с указанием имени таблицы
    /// </summary>
    /// <param name="ui">Интерфейс доступа к документам</param>
    /// <param name="tableName">Имя таблицы документов</param>
    public DBxDataSetPasteFormat(DBUI ui, string tableName)
      : this(ui)
    {
      _TableName = tableName;
      if (!String.IsNullOrEmpty(tableName))
      {
        _UIBase = _UI.DocTypes.FindByTableName(tableName);
        if (_UIBase == null)
          throw new ArgumentException("Таблица \"" + tableName + "\" не соответствует документу или поддокументу", "tableName");
        DisplayName = "Таблица \"" + _UIBase.DocTypeBase.PluralTitle + "\"";
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс доступа к документам
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

    /// <summary>
    /// Если свойство установлено, то при проверке будет определяться наличие
    /// в выборке документов заданного вида
    /// </summary>
    public string TableName { get { return _TableName; } }
    private readonly string _TableName;

    /// <summary>
    /// Ссылка на описание документа или поддокумента, если свойство <see cref="TableName"/> было установлено в конструкторе
    /// </summary>
    public DocTypeUIBase UIBase { get { return _UIBase; } }
    private readonly DocTypeUIBase _UIBase;

    #endregion

    #region Проверка формата

    /// <summary>
    /// Набор данных, извлеченный из буфера обмена при проверке формата
    /// </summary>
    public DataSet DataSet { get { return _DataSet; } }
    private DataSet _DataSet;

    /// <summary>
    /// Таблица данных с именем <see cref="TableName"/>, извлеченная из буфера обмена при проверке формата.
    /// Если <see cref="TableName"/> не задано, то возвращает null.
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private DataTable _Table;


    /// <summary>
    /// Проверяет наличие в буфере обмена объекта <see cref="DataSet"/> с подходящей таблицей.
    /// Если данные присутствуют, вызывает событие <see cref="EFPPasteFormat.TestFormat"/> для выполнения окончательной проверки.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnTestFormat(EFPTestDataObjectEventArgs args)
    {
      _Table = null;
      _DataSet = (DataSet)(args.Data.GetData(typeof(DataSet)));
      if (_DataSet == null)
      {
        args.DataInfoText = "Буфер обмена не содержит набора данных DataSet";
        args.DataImageKey = "No";
        args.Appliable = false;
        return;
      }

      string dbIdentity = DataTools.GetString(_DataSet.ExtendedProperties["DBIdentity"]);
      if (!String.IsNullOrEmpty(dbIdentity))
      {
        // Нельзя всегда проверять идентичность.
        // Таблица в буфере обмена может быть создана пользователем, который не
        // задал значение. Или таблица может не содержать ссылочных полей, и тогда
        // вставка является вполне допустимой

        if (dbIdentity != _UI.DocProvider.DBIdentity)
        {
          _DataSet = null;
          args.DataInfoText = "Таблица в буфере обмена относится к другой базе данных";
          args.DataImageKey = "DifferentDatabase";
          args.Appliable = false;
          return;
        }
      }

      if (String.IsNullOrEmpty(_TableName))
      {
        args.DataInfoText = DisplayName;
        args.DataImageKey = "DataSet";
      }
      else
      {
        if (!_DataSet.Tables.Contains(_TableName))
        {
          args.DataInfoText = "Буфер обмена не содержит таблицы \"" + UIBase.DocTypeBase.PluralTitle + "\"";
          args.DataImageKey = "No";
          args.Appliable = false;
          return;
        }

        _Table = _DataSet.Tables[TableName];

        if (_Table.Rows.Count == 0)
        {
          args.DataInfoText = (UIBase.DocTypeBase.IsSubDoc ? "Таблица поддокументов \"" : "Таблица документов \"") + UIBase.DocTypeBase.PluralTitle + "\" не содержит строк";
          args.DataImageKey = "No";
          args.Appliable = false;
          return;
        }

        args.DataInfoText = (UIBase.DocTypeBase.IsSubDoc ? "Таблица поддокументов \"" : "Таблица документов \"") + UIBase.DocTypeBase.PluralTitle + "\" (" +
          " строк(и))";

        args.DataImageKey = UI.ImageHandlers.GetImageKey(UIBase.DocTypeBase.Name);
      }

      args.Appliable = true;
      base.OnTestFormatEvent(args);
    }

    /// <summary>
    /// Проверка наличия заполненного столбца с заданным именем.
    /// Может использоваться в обработчике события <see cref="EFPPasteFormat.TestFormat"/>, в режиме заданной таблицы.
    /// Возвращает true, если таблица <see cref="Table"/> содержит столбец и хотя бы в одной строке
    /// он содержит значение, отличное от NULL.
    /// </summary>
    /// <param name="columnName">Имя столюца</param>
    /// <returns>Наличие значения</returns>
    public bool CheckColumnFilled(string columnName)
    {
      if (Table == null)
        return false;
      int p = Table.Columns.IndexOf(columnName);
      if (p < 0)
        return false;

      for (int i = 0; i < Table.Rows.Count; i++)
      {
        if (!Table.Rows[i].IsNull(p))
          return true;
      }

      return false;
    }

    #endregion

    #region Просмотр

    /// <summary>
    /// Выполнить предварительный просмотр данных, которые будут вставлены.
    /// Переопределенный метод разрешает просмотр даже при отсутствии обработчика события <see cref="EFPPasteFormat.Preview"/>.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnPreview(EFPPreviewDataObjectEventArgs args)
    {
      if (HasPreviewHandler)
        base.OnPreview(args);
      else
      {
        DebugTools.DebugDataSet(DataSet, PreviewTitle);
        args.Cancel = false;
      }
    }

    #endregion
  }
}
