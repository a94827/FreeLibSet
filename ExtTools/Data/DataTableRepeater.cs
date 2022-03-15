// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  #region Делегат

  /// <summary>
  /// Аргументы события DataTableRepeater.ValueNeeded
  /// </summary>
  public sealed class DataTableRepeaterValueNeededEventArgs : EventArgs
  {
    #region Инициализация

    internal void Init(DataRow sourceRow, string columnName)
    {
      _SourceRow = sourceRow;
      _ColumnName = columnName;
      _Value = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Строка в исходной таблице MasterTable
    /// </summary>
    public DataRow SourceRow { get { return _SourceRow; } }
    private DataRow _SourceRow;

    /// <summary>
    /// Имя поля в таблице SlaveTable, значение которого требуется получить
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Сюда должно быть записано вычисленное значение
    /// </summary>
    public object Value { get { return _Value; } set { _Value = value; } }
    private object _Value;

    #endregion
  }

  /// <summary>
  /// Делегат события DataTableRepeater.ValueNeeded
  /// </summary>
  /// <param name="sender">Объект DataTableRepeater</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DataTableRepeaterValueNeededEventHandler(object sender, DataTableRepeaterValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// Повторитель таблицы. Синхронизирует строки с таблицей, которая содержит меньшее число столбцов.
  /// Оставшиеся поля должны вычисляться в прикладном коде.
  /// Прикладной код должен создать структуру в таблице DataTableRepeater.SlaveTable и реализовать расчет вычисляемых
  /// столбцов в обработчике событиия ValueNeeded (или в классе-наследнике). Затем устанавливается свойство MasterTable.
  /// К SlaveTable может быть присоединен табличный просмотр, в котором можно выполнять сортировку, в том числе и по вычисляемым полям.
  /// Синхронизируется добавление/удаление строк и изменение значений полей в таблице MasterTable. Синхронизация
  /// выполняется по полям первичного ключа или с помощью внутреннего словаря.
  /// При установке свойства MasterTable, к таблице присоединяются обоработчики. Если в коде объекты DataTableRepeater
  /// создаются многократно, то следует вызывать DataTableRepeater.Dispose() или устанавливать MasterTable=null для отключения обработчиков.
  /// Класс не является потокобезопасным.
  /// </summary>
  public class DataTableRepeater : SimpleDisposableObject
  {
    #region Конструктор и Dispose()

    /// <summary>
    /// Создает объект
    /// </summary>
    public DataTableRepeater()
    {
      _SlaveTable = new DataTable();
      _ValueNeededArgs = new DataTableRepeaterValueNeededEventArgs();
      _SourceColumnMaps = new List<KeyValuePair<int, int>>();
      _CalculatedColumns = new List<int>();

      _RowMapMode = RowMapMode.None;
    }

    /// <summary>
    /// Устанавливает свойство MasterTable=null.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      if (disposing &&
        this.MasterTable != null /* добавлено 24.12.2021 */)
        this.MasterTable = null;
      base.Dispose(disposing);
    }

    #endregion

    #region Таблица-повторитель с дополнительными столбцами

    /// <summary>
    /// Таблица-повторитель.
    /// Структура таблицы должна быть заполнена до присоединения MasterTable.
    /// Вместо заполнения этой таблицы, можно подключить свою, установив значение свойства.
    /// Если в таблице установлен первичный ключ, он будет использован вместо словаря строк.
    /// После подключения ведущей таблицы, нельзя менять структуру, требуется создавать новый объект DataTableRepeater.
    /// Эта таблица может использоваться в качестве источника данных табличного просмотра (DataGridView.DataSource)
    /// </summary>
    public DataTable SlaveTable
    {
      get { return _SlaveTable; }
      set
      {
        if (_MasterTable != null)
          throw new InvalidOperationException();
        if (value == null)
          throw new ArgumentNullException();
        _SlaveTable = value;
      }
    }
    private DataTable _SlaveTable;

    /// <summary>
    /// Обработчик события вызывается, когда табличный просмотр, присоединенный к SlaveTable, меняет значение ячейки.
    /// Обработчик выполняет обратную запись значения в MasterTable
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void SlaveTable_ColumnChanged(object sender, DataColumnChangeEventArgs args)
    {
#if DEBUG
      if (_MasterTable == null)
        throw new BugException("MasterTable=null");
#endif

      if (_InsideRowChanged)
        return;

      DataRow masterRow = GetMasterRow(args.Row);
      if (masterRow == null)
        throw new NullReferenceException("Не найдена строка master-таблицы");

      masterRow[args.Column.ColumnName] = args.ProposedValue;
    }

    #endregion

    #region Ведущая таблица

    /// <summary>
    /// Ведущая таблица.
    /// Свойство должно быть установлено после того, как заполнена структура таблицы SlaveTable.
    /// Если SlaveTable имеет первичный ключ, то присоединяемая таблица должна иметь такой же первичный ключ.
    /// </summary>
    public DataTable MasterTable
    {
      get { return _MasterTable; }
      set
      {
        if (_SlaveTable.Columns.Count == 0)
          throw new InvalidOperationException("Структура таблицы SlaveTable должна быть заполнена");
        if (Object.ReferenceEquals(value, _SlaveTable))
          throw new ArgumentException("Нельзя присваивать ссылку на SlaveTable");

        _SourceColumnMaps.Clear();
        _CalculatedColumns.Clear();
        _RowMapMode = RowMapMode.None;
        _RowDict = null;

        if (_MasterTable != null)
        {
          _MasterTable.RowChanged -= new DataRowChangeEventHandler(DataSource_RowChanged);
          _MasterTable.RowDeleting -= new DataRowChangeEventHandler(DataSource_RowDeleting);
          _MasterTable.TableCleared -= new DataTableClearEventHandler(DataSource_TableCleared);
        }
        _SlaveTable.ColumnChanged -= new DataColumnChangeEventHandler(SlaveTable_ColumnChanged);

        _MasterTable = value;

        OnMasterTableChanged();

        _SlaveTable.BeginLoadData();
        try
        {
          _SlaveTable.Rows.Clear();

          if (_MasterTable != null)
          {
            for (int i = 0; i < _SlaveTable.Columns.Count; i++)
            {
              int p = _MasterTable.Columns.IndexOf(_SlaveTable.Columns[i].ColumnName);
              if (p >= 0)
              {
                _SourceColumnMaps.Add(new KeyValuePair<int, int>(p, i));
              }
              else
              {
                _CalculatedColumns.Add(i);
                _SlaveTable.Columns[i].ReadOnly = true;
              }
            }

            InitRowMapMode();

            foreach (DataRow srcRow in _MasterTable.Rows)
            {
              if (srcRow.RowState == DataRowState.Deleted)
                continue; // 16.06.2021 ? 
              DataRow resRow = _SlaveTable.NewRow();
              ProcessRow(srcRow, resRow);
              _SlaveTable.Rows.Add(resRow);
              if (_RowDict != null)
                _RowDict.Add(srcRow, resRow);
            }
          }
        }
        finally
        {
          _SlaveTable.EndLoadData();
        }

        if (_MasterTable != null)
        {

          _MasterTable.RowChanged += new DataRowChangeEventHandler(DataSource_RowChanged);
          _MasterTable.RowDeleting += new DataRowChangeEventHandler(DataSource_RowDeleting);
          _MasterTable.TableCleared += new DataTableClearEventHandler(DataSource_TableCleared);
        }

        _SlaveTable.ColumnChanged += new DataColumnChangeEventHandler(SlaveTable_ColumnChanged);
      }
    }
    private DataTable _MasterTable;

    /// <summary>
    /// Метод вызывается при установке свойства MasterTable непосредственно перед заполнением таблицы-повторителя.
    /// Может использоваться, например, для инициализации внутренних словарей класса-наследника.
    /// На момент вызова свойство MasterTable может быть null.
    /// </summary>
    protected virtual void OnMasterTableChanged()
    {
    }

    private bool _InsideRowChanged;

    void DataSource_RowChanged(object sender, DataRowChangeEventArgs args)
    {
      DataRow resRow;

      // Не бывает
      //if ((args.Action & DataRowAction.Delete) != 0)
      //{
      //  ResRow = _RowDictionary[args.Row];
      //  ResRow.Delete();
      //  _RowDictionary.Remove(args.Row);
      //}

      if (_InsideRowChanged)
        return;

      _InsideRowChanged = true;
      try
      {
        if ((args.Action & DataRowAction.Add) != 0)
        {
          resRow = _SlaveTable.NewRow();
          ProcessRow(args.Row, resRow);
          _SlaveTable.Rows.Add(resRow);
          if (_RowDict != null)
            _RowDict.Add(args.Row, resRow);
        }
        else if ((args.Action & DataRowAction.Change) != 0)
        {
          resRow = GetSlaveRow(args.Row);
          if (resRow != null) // 20.06.2021
            ProcessRow(args.Row, resRow);
        }
      }
      finally
      {
        _InsideRowChanged = false;
      }
    }

    void DataSource_RowDeleting(object sender, DataRowChangeEventArgs args)
    {
      DataRow resRow = GetSlaveRow(args.Row);
      resRow.Delete();
      if (_RowDict != null)
        _RowDict.Remove(args.Row);
    }

    void DataSource_TableCleared(object sender, DataTableClearEventArgs args)
    {
      _SlaveTable.Rows.Clear();
      if (_RowDict != null)
        _RowDict.Clear();
    }

    /// <summary>
    /// Выполняет полное перестроение строк в SlaveTable.
    /// Этот метод нет смысла использовать в прикладном коде, так как обновление обычно выполняется созданием
    /// новой ведущей таблицы (загрузкой из базы данных) и установкой свойства MasterTable.
    /// </summary>
    public void Refresh()
    {
      this.MasterTable = this.MasterTable;
    }

    #endregion

    #region Событие ValueNeeded

    /// <summary>
    /// Событие вызывается для каждого значения в таблице SlaveTable, которое требуется вычислить.
    /// </summary>
    public event DataTableRepeaterValueNeededEventHandler ValueNeeded;

    /// <summary>
    /// Вызывает событие ValueNeeded.
    /// Класс-наследник может переопределить метод для вычисления значений полей.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnValueNeeded(DataTableRepeaterValueNeededEventArgs args)
    {
      if (ValueNeeded != null)
        ValueNeeded(this, args);
    }

    /// <summary>
    /// Используем единственный экземпляр аргументов события, чтобы уменьшить количество мусора.
    /// </summary>
    private DataTableRepeaterValueNeededEventArgs _ValueNeededArgs;

    /// <summary>
    /// Список полей, копируемых из исходной таблицы
    /// Key - индекс столбца в таблице DataSource
    /// Value - индекс столбца в таблице ResultTable
    /// </summary>
    private List<KeyValuePair<int, int>> _SourceColumnMaps;

    /// <summary>
    /// Список полей, для которых вызывается событие ValueNeeded.
    /// Содержит индексы столбцов в таблице ResultTable
    /// </summary>
    private List<int> _CalculatedColumns;

    /// <summary>
    /// Обработка для одной строки
    /// </summary>
    /// <param name="srcRow">Строка в MasterTable</param>
    /// <param name="resRow">Строка в SlaveTable, которую нужно заполнить</param>
    private void ProcessRow(DataRow srcRow, DataRow resRow)
    {
      #region Копирование

      for (int i = 0; i < _SourceColumnMaps.Count; i++)
        resRow[_SourceColumnMaps[i].Value] = srcRow[_SourceColumnMaps[i].Key];

      #endregion

      #region Расчет

      for (int i = 0; i < _CalculatedColumns.Count; i++)
      {
        _ValueNeededArgs.Init(srcRow, _SlaveTable.Columns[_CalculatedColumns[i]].ColumnName);
        OnValueNeeded(_ValueNeededArgs);

        _SlaveTable.Columns[_CalculatedColumns[i]].ReadOnly = false;
        if (_ValueNeededArgs.Value == null)
          resRow[_CalculatedColumns[i]] = DBNull.Value;
        else
          resRow[_CalculatedColumns[i]] = _ValueNeededArgs.Value;
        _SlaveTable.Columns[_CalculatedColumns[i]].ReadOnly = true;
      }

      #endregion
    }

    #endregion

    #region Соответствие строк

    #region Перечисление RowMapMode

    /// <summary>
    /// Режимы сопоставления строк в MasterTable и SlaveTable
    /// </summary>
    private enum RowMapMode
    {
      /// <summary>
      /// Таблица не присоединена
      /// </summary>
      None,

      /// <summary>
      /// Используется BidirectionalDictionary
      /// </summary>
      Dictionary,

      /// <summary>
      /// Соответствие по первичному ключу, который состоит из одного поля
      /// </summary>
      SimplePrimaryKey,

      /// <summary>
      /// Соответствие по первичному ключу, который состоит из нескольких полей
      /// </summary>
      ComplexPrimaryKey
    }

    #endregion

    /// <summary>
    /// Текущий режим соответствия строк
    /// </summary>
    private RowMapMode _RowMapMode;

    /// <summary>
    /// Соответствие строк в режиме RowMapMode.Dictionary
    /// </summary>
    private BidirectionalDictionary<DataRow, DataRow> _RowDict;

    private int _MasterPKColPos, _SlavePKColPos;

    private void InitRowMapMode()
    {
      if (_SlaveTable.PrimaryKey.Length == 0)
      {
        _RowMapMode = RowMapMode.Dictionary;
        _RowDict = new BidirectionalDictionary<DataRow, DataRow>(_MasterTable.Rows.Count);
      }
      else
      {
        string masterPK = DataTools.GetPrimaryKey(_MasterTable);
        string slavePK = DataTools.GetPrimaryKey(_SlaveTable);
        if (!String.Equals(masterPK, slavePK, StringComparison.Ordinal))
          throw new InvalidOperationException("Таблица SlaveTable имеет первичный ключ \"" + slavePK + "\". Подключаемая таблица MasterTable должна иметь такой же ключ, а не \"" + masterPK + "\"");

        if (slavePK.IndexOf(',') >= 0)
          _RowMapMode = RowMapMode.ComplexPrimaryKey;
        else
        {
          _RowMapMode = RowMapMode.SimplePrimaryKey;
          _MasterPKColPos = _MasterTable.Columns.IndexOf(masterPK);
          _SlavePKColPos = _SlaveTable.Columns.IndexOf(slavePK);
        }
      }
    }

    #region Master -> Slave

    /// <summary>
    /// Возвращает строку в таблице-повторителе SlaveTable, которая соответствует строке в исходной таблице MasterTable
    /// </summary>
    /// <param name="masterRow">Строка таблицы MasterTable</param>
    /// <returns>Строка таблицы SlaveTable</returns>
    public DataRow GetSlaveRow(DataRow masterRow)
    {
      if (masterRow == null)
        return null;

      switch (_RowMapMode)
      {
        case RowMapMode.Dictionary:
          DataRow slaveRow;
          _RowDict.TryGetValue(masterRow, out slaveRow);
          return slaveRow;
        case RowMapMode.SimplePrimaryKey:
          return _SlaveTable.Rows.Find(masterRow[_MasterPKColPos]);
        case RowMapMode.ComplexPrimaryKey:
          return _SlaveTable.Rows.Find(DataTools.GetPrimaryKeyValues(masterRow));
        case RowMapMode.None:
          return null;
        default:
          throw new BugException();
      }
    }

    /// <summary>
    /// Возвращает строки в таблице-повторителе SlaveTable, которые соответствуют строкам в исходной таблице MasterTable
    /// </summary>
    /// <param name="masterRows">Массив строк таблицы MasterTable</param>
    /// <returns>Строки таблицы SlaveTable</returns>
    public DataRow[] GetSlaveRows(DataRow[] masterRows)
    {
      if (masterRows == null)
        return null;

      DataRow[] slaveRows = new DataRow[masterRows.Length];

      switch (_RowMapMode)
      {
        case RowMapMode.Dictionary:
          DataRow slaveRow;
          for (int i = 0; i < masterRows.Length; i++)
          {
            _RowDict.TryGetValue(masterRows[i], out slaveRow);
            slaveRows[i] = slaveRow;
          }
          break;
        case RowMapMode.SimplePrimaryKey:
          for (int i = 0; i < masterRows.Length; i++)
            slaveRows[i] = _SlaveTable.Rows.Find(masterRows[i][_MasterPKColPos]);
          break;
        case RowMapMode.ComplexPrimaryKey:
          for (int i = 0; i < masterRows.Length; i++)
            slaveRows[i] = _SlaveTable.Rows.Find(DataTools.GetPrimaryKeyValues(masterRows[i]));
          break;
        case RowMapMode.None:
          break;
        default:
          throw new BugException();
      }

      return slaveRows;
    }

    #endregion

    #region Slave -> Master

    /// <summary>
    /// Возвращает строку в основной таблице MasterTable, которая соответствует строке в таблице-повторителе SlaveTable.
    /// Используется табличным просмотром для реализации редактирования, т.к. изменения должны вноситься в ведущую
    /// таблицу, а не ту, которая отображается в просмотре.
    /// </summary>
    /// <param name="slaveRow">Строка таблицы SlaveTable</param>
    /// <returns>Строка таблицы MasterTable</returns>
    public DataRow GetMasterRow(DataRow slaveRow)
    {
      if (slaveRow == null)
        return null;

      switch (_RowMapMode)
      {
        case RowMapMode.Dictionary:
          DataRow masterRow;
          _RowDict.TryGetKey(slaveRow, out masterRow);
          return masterRow;
        case RowMapMode.SimplePrimaryKey:
          return _MasterTable.Rows.Find(slaveRow[_SlavePKColPos]);
        case RowMapMode.ComplexPrimaryKey:
          return _MasterTable.Rows.Find(DataTools.GetPrimaryKeyValues(slaveRow));
        case RowMapMode.None:
          return null;
        default:
          throw new BugException();
      }
    }

    /// <summary>
    /// Возвращает строки в основной таблице MasterTable, которые соответствуют строкам в таблице-повторителе SlaveTable.
    /// Используется табличным просмотром для реализации редактирования, т.к. изменения должны вноситься в ведущую
    /// таблицу, а не ту, которая отображается в просмотре.
    /// </summary>
    /// <param name="slaveRows">Строки таблицы SlaveTable</param>
    /// <returns>Строки таблицы MasterTable</returns>
    public DataRow[] GetMasterRows(DataRow[] slaveRows)
    {
      if (slaveRows == null)
        return null;

      DataRow[] masterRows = new DataRow[slaveRows.Length];

      switch (_RowMapMode)
      {
        case RowMapMode.Dictionary:
          DataRow masterRow;
          for (int i = 0; i < slaveRows.Length; i++)
          {
            _RowDict.TryGetKey(slaveRows[i], out masterRow);
            masterRows[i] = masterRow;
          }
          break;
        case RowMapMode.SimplePrimaryKey:
          for (int i = 0; i < slaveRows.Length; i++)
            masterRows[i] = _MasterTable.Rows.Find(slaveRows[i][_SlavePKColPos]);
          break;
        case RowMapMode.ComplexPrimaryKey:
          for (int i = 0; i < slaveRows.Length; i++)
            masterRows[i] = _MasterTable.Rows.Find(DataTools.GetPrimaryKeyValues(slaveRows[i]));
          break;
        case RowMapMode.None:
          break;
        default:
          throw new BugException();
      }
      return masterRows;
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      if (MasterTable != null)
      {
        sb.Append("\"");
        if (String.IsNullOrEmpty(MasterTable.TableName))
          sb.Append("(без имени)");
        else
          sb.Append(MasterTable.TableName);
        sb.Append("\" (RowCount=");
        sb.Append(MasterTable.Rows.Count);
        sb.Append(") -> ");
      }

      sb.Append("\"");
      if (String.IsNullOrEmpty(SlaveTable.TableName))
        sb.Append("(без имени)");
      else
        sb.Append(SlaveTable.TableName);
      sb.Append("\" (RowCount=");
      sb.Append(SlaveTable.Rows.Count);
      sb.Append(")");
      if (MasterTable == null)
        sb.Append(" нет источника");

      string sPK = DataTools.GetPrimaryKey(SlaveTable);
      if (!String.IsNullOrEmpty(sPK))
      {
        sb.Append(". PrimaryKey=\"");
        sb.Append(sPK);
        sb.Append("\"");
      }

      return sb.ToString();
    }

    #endregion

    #endregion
  }
}
