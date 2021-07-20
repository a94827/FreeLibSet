using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtForms;
using System.Data;
using System.Windows.Forms;
using AgeyevAV;
using AgeyevAV.ExtDB.Docs;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace AgeyevAV.ExtForms.Docs
{
  // Классы для реализации форматов вставки из буфера обмена

  /// <summary>
  /// Формат вставки из буфера обмена выборки документов.
  /// Возможность указания конкретного вида документов
  /// </summary>
  public class DBxDocSelectionPasteFormat : EFPPasteFormat
  {
    #region Конструкторы

    /// <summary>
    /// Создает формат, который позволяет вставлять выборки документов любых видов
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
    /// Создает формат, который позволяет вставлять выборки документов заданного вида
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
    /// Создает формат, который позволяет вставлять выборки документов заданного вида
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
    private DBUI _UI;

    /// <summary>
    /// Если свойство установлено, то при проверке будет определяться наличие
    /// в выборке документов заданного вида.
    /// </summary>
    public string DocTypeName { get { return _DocTypeName; } }
    private string _DocTypeName;

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
    /// Проверяет наличие в буфере обмена выборки документов (объекта DBxDocSelection).
    /// Проверяет совпадение свойства DBxDocProvider.DBIdentity.
    /// Если свойство DocTypeName установлено, проверяет наличие в выборке документов требуемого вида.
    /// Если все в порядке, вызывает метод OnTestFormatEvent() для выполнения окончательной проверки
    /// </summary>
    /// <param name="data">Данные буфера обмена</param>
    /// <param name="reason">Вставка или специальная вставка</param>
    /// <param name="dataInfoText">Сюда записывается текстовое описание данных в буфере обмена или
    /// сообщение об ошибке, если нет подходящих данных</param>
    /// <param name="dataImageKey">Сюда записывается имя изображения из списка EFPApp.MainImages,
    /// которое будет использовано в качестве значка в списке "Специальная вставка".
    /// В случае ошибки сюда записывается "No" или другой подходящий значок</param>
    /// <returns>True, если данные из буфера могут быть вставлены</returns>
    protected override bool OnTestFormat(IDataObject data, EFPPasteReason reason, out string dataInfoText, out string dataImageKey)
    {
      _DocId = 0;
      _DocSel = (DBxDocSelection)(data.GetData(typeof(DBxDocSelection)));
      if (_DocSel == null)
      {
        dataInfoText = "Буфер обмена не содержит выборки документов";
        dataImageKey = "No";
        return false;
      }

      if (_DocSel.DBIdentity != _UI.DocProvider.DBIdentity)
      {
        dataInfoText = "Выборка документов относится к другой базе данных";
        dataImageKey = "DifferentDatabase";
        //DataImageKey = "Error";
        return false;
      }

      if (String.IsNullOrEmpty(_DocTypeName))
      {
        dataInfoText = DisplayName;
        dataImageKey = "DBxDocSelection";
      }
      else
      {
        if (_DocSel.GetCount(_DocTypeName) == 0)
        {
          dataInfoText = "Буфер обмена не содержит выборки документов \"" + DocType.DocType.PluralTitle + "\"";
          dataImageKey = "No";
          return false;
        }

        _DocId = _DocSel.GetSingleId(_DocTypeName);

        int cnt = _DocSel.GetCount(DocTypeName);
        if (cnt == 1)
        {
          dataInfoText = "Документ \"" + DocType.DocType.SingularTitle + "\"";
          dataImageKey = DocType.GetImageKey(_DocId);
        }
        else
        {
          dataInfoText = "Документы \"" + DocType.DocType.PluralTitle + "\"";
          dataImageKey = DocType.TableImageKey;
        }
      }

      bool Appliable = true;
      base.OnTestFormatEvent(data, reason, ref Appliable, ref dataInfoText, ref dataImageKey);
      return Appliable;
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
  /// Формат вставки из буфера обмена набора таблиц DataSet.
  /// Возможность указания имени таблицы документа или поддокумента
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
    private DBUI _UI;

    /// <summary>
    /// Если свойство установлено, то при проверке будет определяться наличие
    /// в выборке документов заданного вида
    /// </summary>
    public string TableName { get { return _TableName; } }
    private string _TableName;

    /// <summary>
    /// Ссылка на описание документа или поддокумента, если свойство TableName было установлено в конструкторе
    /// </summary>
    public DocTypeUIBase UIBase { get { return _UIBase; } }
    private DocTypeUIBase _UIBase;

    #endregion

    #region Проверка формата

    /// <summary>
    /// Набор данных, извлеченный из буфера обмена при проверке формата
    /// </summary>
    public DataSet DataSet { get { return _DataSet; } }
    private DataSet _DataSet;

    /// <summary>
    /// Таблица данных с именем TableName, извлеченная из буфера обмена при проверке формата.
    /// Если TableName не задано, то возвращает null
    /// </summary>
    public DataTable Table { get { return _Table; } }
    private DataTable _Table;


    /// <summary>
    /// Проверяет наличие в буфере обмена объекта DataSet с подходящей таблицей.
    /// Вызывает метод OnTestFormatEvent(), если данные присутствуют, для выполнения окончательной проверки
    /// </summary>
    /// <param name="data">Данные буфера обмена</param>
    /// <param name="reason">Вставка или специальная вставка</param>
    /// <param name="dataInfoText">Сюда записывается текстовое описание данных в буфере обмена или
    /// сообщение об ошибке, если нет подходящих данных</param>
    /// <param name="dataImageKey">Сюда записывается имя изображения из списка EFPApp.MainImages,
    /// которое будет использовано в качестве значка в списке "Специальная вставка".
    /// В случае ошибки сюда записывается "No" или другой подходящий значок</param>
    /// <returns>True, если данные из буфера могуь быть вставлены</returns>
    protected override bool OnTestFormat(IDataObject data, EFPPasteReason reason, out string dataInfoText, out string dataImageKey)
    {
      _Table = null;
      _DataSet = (DataSet)(data.GetData(typeof(DataSet)));
      if (_DataSet == null)
      {
        dataInfoText = "Буфер обмена не содержит набора данных DataSet";
        dataImageKey = "No";
        return false;
      }

      string DBIdentity = DataTools.GetString(_DataSet.ExtendedProperties["DBIdentity"]);
      if (!String.IsNullOrEmpty(DBIdentity))
      {
        // Нельзя всегда проверять идентичность.
        // Таблица в буфере обмена может быть создана пользователем, который не
        // задал значение. Или таблица может не содержать ссылочных полей, и тогда
        // вставка является вполне допустимой

        if (DBIdentity != _UI.DocProvider.DBIdentity)
        {
          _DataSet = null;
          dataInfoText = "Таблица в буфере обмена относится к другой базе данных";
          dataImageKey = "DifferentDatabase";
          return false;
        }
      }

      if (String.IsNullOrEmpty(_TableName))
      {
        dataInfoText = DisplayName;
        dataImageKey = "DataSet";
      }
      else
      {
        if (!_DataSet.Tables.Contains(_TableName))
        {
          dataInfoText = "Буфер обмена не содержит таблицы \"" + UIBase.DocTypeBase.PluralTitle + "\"";
          dataImageKey = "No";
          return false;
        }

        _Table = _DataSet.Tables[TableName];

        if (_Table.Rows.Count == 0)
        {
          dataInfoText = (UIBase.DocTypeBase.IsSubDoc ? "Таблица поддокументов \"" : "Таблица документов \"") + UIBase.DocTypeBase.PluralTitle + "\" не содержит строк";
          dataImageKey = "No";
          return false;
        }

        dataInfoText = (UIBase.DocTypeBase.IsSubDoc ? "Таблица поддокументов \"" : "Таблица документов \"") + UIBase.DocTypeBase.PluralTitle + "\" (" +
          " строк(и))";

        dataImageKey = UI.ImageHandlers.GetImageKey(UIBase.DocTypeBase.Name);
      }

      bool Appliable = true;
      base.OnTestFormatEvent(data, reason, ref Appliable, ref dataInfoText, ref dataImageKey);
      return Appliable;
    }

    /// <summary>
    /// Проверка наличия заполненного столбца с заданным именем.
    /// Может использоваться в обработчике TestFormat, в режиме заданной таблицы.
    /// Возвращает true, если таблица Table содержит столбец и хотя бы в одной строке
    /// он содержит значение, отличное от NULL
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
    /// Переопределенный метод разрешает просмотр даже при отсутствии обработчика события Preview
    /// </summary>
    /// <param name="data">Вставляет данные</param>
    /// <param name="reason">Вставка или специальная вставка</param>
    /// <returns>True, если просмотр выполнен.
    /// False, если обработчик события Preview установил Cancel=true</returns>
    protected override bool OnPreview(IDataObject data, EFPPasteReason reason)
    {
      if (HasPreviewHandler)
        return base.OnPreview(data, reason);

      DebugTools.DebugDataSet(DataSet, PreviewTitle);
      return true;
    }

    #endregion
  }
}
