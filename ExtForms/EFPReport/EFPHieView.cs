// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using FreeLibSet.Config;
using FreeLibSet.Remoting;


/*
 * Иерархические отчеты на базе EFPDataGridView
 */

#if XXX
namespace FreeLibSet.Forms
{

  /// <summary>
  /// Основной объект для построения иерархического отчета.
  /// Должны быть установлены свойства - исходные данные для отчета, а затем вызван метод CreateDataView().
  /// Объект является "одноразовым", то есть, повторный вызов CreateDataView() не допускается.
  /// Для работы с табличным просмотром EFPDataGridView рекомендуется использовать EFPDataGridViewHieHandler
  /// </summary>
  public class EFPHieView : IReadOnlyObject
  {
#region Конструктор

    /// <summary>
    /// Конструктор построителя
    /// </summary>
    public EFPHieView()
    {
      FHideExtraSumRows = false;
      FTotalSumRowCount = 1;
      FSumColumnNames = DataTools.EmptyStrings;
    }

#endregion

#region Свойства - исходные данные для отчета

    /// <summary>
    /// Исходная таблица данных для отчета
    /// </summary>
    public DataTable SourceTable
    {
      get { return FSourceTable; }
      set
      {
        CheckNotReadOnly();

        if (value == null)
          throw new ArgumentNullException();

        FSourceTable = value;
      }
    }
    private DataTable FSourceTable;

    /// <summary>
    /// Уровни иерархии. Нулевым элементом массива идет самый внутренний уровень,
    /// то есть строки исходных данных, последним - самый внешний уровень.
    /// Свойство должно быть установлено до иницализации просмотра.
    /// Массив должен содержать, как минимум, один уровень иерархии.
    /// </summary>
    public EFPHieViewLevel[] Levels
    {
      get { return FLevels; }
      set
      {
        CheckNotReadOnly();

        if (value == null)
          throw new ArgumentNullException();
        if (value.Length < 1)
          throw new ArgumentException("Массив должен содержать не менее одного элемента");
        for (int i = 0; i < value.Length; i++)
        {
          if (value[i] == null)
            throw new ArgumentNullException();

          for (int j = 0; j < i; j++)
          {
            if (value[j].Name == value[i].Name)
            {
              if (object.ReferenceEquals(value[j], value[i]))
                throw new InvalidOperationException("Повторное присоединение уровня " + value[i] + "не допускается");
              else
                throw new ArgumentException("Элемент с именем " + value[i].Name + " входит в список дважды");
            }
          }
        }
        FLevels = value;
      }
    }
    private EFPHieViewLevel[] FLevels;

    /// <summary>
    /// Имена столбцов, которые должны суммироваться
    /// </summary>
    public string[] SumColumnNames
    {
      get { return FSumColumnNames; }
      set
      {
        CheckNotReadOnly();
        if (value == null)
          value = DataTools.EmptyStrings;

        FSumColumnNames = value;
      }
    }
    private string[] FSumColumnNames;

    /// <summary>
    /// Если True, то будут скрыты строки сумм, для которых есть
    /// только одна строка суммируемых данных
    /// По умолчанию (false) итоговые строки показываются всегда
    /// </summary>
    public bool HideExtraSumRows
    {
      get { return FHideExtraSumRows; }
      set
      {
        CheckNotReadOnly();

        FHideExtraSumRows = value;
      }
    }
    private bool FHideExtraSumRows;

    /// <summary>
    /// Количество итоговых строк внизу просмотра. По умолчанию - одна строка.
    /// Можно добавить дополнительные строки, например, для свернутого сальдо
    /// </summary>
    public int TotalSumRowCount
    {
      get { return FTotalSumRowCount; }
      set
      {
        CheckNotReadOnly();
        if (value < 1)
          throw new ArgumentOutOfRangeException("TotalSumRowCount", value, "Число итоговых строк не может быть меньше 1");
        FTotalSumRowCount = value;
      }
    }
    private int FTotalSumRowCount;

#endregion

#region Создание иерархического просмотра

#region Внутренние поля и методы, используемые при построении отчета

    private class KeyInfo
    {
#region Поля

      public string Key;
      public string Text;
      public int Order;

#endregion

      public override string ToString()
      {
        return Key.ToString() + " = " + Text;
      }
    }

    /// <summary>
    /// Внутренние данные для одного уровня
    /// </summary>
    private class InternalLevelData
    {
#region Конструктор

      public InternalLevelData(EFPHieViewLevel Level)
      {
        FLevel = Level;
      }

      private EFPHieViewLevel FLevel;

#endregion

#region Пары Ключ-Текст

      public SortedList<string, KeyInfo> KeyPairs;

      public void CreateKeyPairs(DataTable Table)
      {
        //if (Mode == HieViewLevelMode.UserDefined)
        //  return;
        KeyPairs = new SortedList<string, KeyInfo>();

        foreach (DataRow Row in Table.Rows)
        {
          string Key = FLevel.GetRowSortKey(Row);
          //if (!IsEmptyKey(Key))
          if (!String.IsNullOrEmpty(Key))
          {
            if (!KeyPairs.ContainsKey(Key))
            {
              KeyInfo ki = new KeyInfo();
              ki.Key = Key;
              ki.Text = FLevel.GetRowText(Row);
              KeyPairs.Add(Key, ki);
            }
          }
        }
      }

      /// <summary>
      /// Инициализация поля Order в массиве KeyPairs
      /// </summary>
      public void InitKeyPairsOrder()
      {
        SortedList<String, KeyInfo> TmpList = new SortedList<string, KeyInfo>();
        int cnt = 0;
        foreach (KeyInfo ki in KeyPairs.Values)
        {
          // В массиве KeyPairs могут встречаться одинаковые названия
          // Поэтому, добавляем к названию порядковый номер
          cnt++;
          TmpList.Add(ki.Key + "_" + cnt.ToString(), ki);
        }
        for (int i = 0; i < TmpList.Count; i++)
          TmpList.Values[i].Order = i + 1;
      }

      /// <summary>
      /// Получить числовой код для сортировки для строки
      /// </summary>
      /// <param name="Row"></param>
      /// <returns></returns>
      public int GetRowOrder(DataRow Row)
      {
        string Key = FLevel.GetRowSortKey(Row);
        if (String.IsNullOrEmpty(Key))
          return 0;
        KeyInfo ki = KeyPairs[Key];
        if (ki == null)
          return -1;
        else
          return ki.Order;
      }

#endregion
    }

#endregion

    /// <summary>
    /// Создает иерархический просмотр.
    /// После вызова, свойство DataView получает значение.
    /// Объект переводится в режиме IsReadOnly=true.
    /// Этот метод можно вызывать только один раз.
    /// </summary>
    public void CreateDataView()
    {
      CheckNotReadOnly();

      if (SourceTable == null)
        throw new NullReferenceException("Не задано свойство SourceTable");
      if (Levels == null)
        throw new NullReferenceException("Не задано свойство Levels");

      // Для каждого заданного уровня иерархии создаем словарь "значение поля"-"текст".
      // Словарь сортируется по тексту (а не по коду) и элементы нумеруются по порядку
      // Эти номера используются для сортировки уровней иерархии.
      InternalLevelData[] LevelData = new InternalLevelData[Levels.Length];
      for (int i = 0; i < Levels.Length; i++)
      {
        LevelData[i] = new InternalLevelData(Levels[i]);
        LevelData[i].CreateKeyPairs(SourceTable);
        LevelData[i].InitKeyPairsOrder();
      }

      // Создаем новую таблицу
      DataTable ResTable = CreateResTable();

      // Копируем строки нулевого уровня иерархии
      CopyLevel0(ResTable, LevelData);

      // Создаем просмотр, отсортированный нужным образом
      FDataView = new DataView(ResTable);
      string s = "";
      for (int i = Levels.Length - 1; i >= 1; i--)
        s += "Hie_Order" + i.ToString() + ",";
      s += "Hie_Order0";
      FDataView.Sort = s;

      // Добавляем для каждой исходной строки данных нужное число уровней иерархии
      AddLevels(LevelData);

      // Вычисление итоговых строк
      CalcSums();

      if (HideExtraSumRows)
        DoHideExtraSumRows();

      // Перемещаем подытоговые строки для уровней с установленным признаком
      MoveSumRowsFirst();

      OnAfterCreateView();

      // Нумерация строк
      int cnt = 0;
      for (int j = 0; j < FDataView.Count; j++)
      {
        DataRowView drv = FDataView[j];
        if (((int)(drv.Row["Hie_Level"])) == 0)
        {
          cnt++;
          drv.Row["Hie_RowNumber"] = cnt;
        }
      }
    }

    private DataTable CreateResTable()
    {
      // Нельзя сразу скопировать вместе с данными, т.к. надо добавить столбцы
      DataTable ResTable = SourceTable.Clone();
      ResTable.TableName = "HieTable";

      // 31.01.2009
      // Убираем ограничения для всех столбцов таблицы
      foreach (DataColumn Column in ResTable.Columns)
      {
        Column.AllowDBNull = true;
        Column.MaxLength = -1;
        Column.AutoIncrement = false;
      }
      ResTable.PrimaryKey = null;

      // Добавляем столбцы

      // Здесь будет уровень иереархии (0-самый внутренний - исходные данные)
      ResTable.Columns.Add("Hie_Level", typeof(int));
#if DEBUG
      ResTable.Columns.Add("Hie_LevelName", typeof(string)); // отладка
#endif

      // True для строк итогов, false - для заголовков (при Hie_Level > 0)
      ResTable.Columns.Add("Hie_Sum", typeof(bool));

      // Здесь будет текст с названиями уровней иерархии
      ResTable.Columns.Add("Hie_Text", typeof(string));

      // Здесь будет номер строки (нумерация с единицы) для строк с Hie_Level=0
      ResTable.Columns.Add("Hie_RowNumber", typeof(int));

      // Для каждого уровня иерархии добавляем числовое поле для сортировки
      for (int i = 0; i < Levels.Length; i++)
      {
        ResTable.Columns.Add("Hie_Order" + i.ToString(), typeof(int));
      }

      // Флажок для удаления лишних строк
      ResTable.Columns.Add("Hie_Hidden", typeof(bool));

      return ResTable;
    }

    private void CopyLevel0(DataTable ResTable, InternalLevelData[] LevelData)
    {
      for (int i = 0; i < SourceTable.Rows.Count; i++)
      {
        DataRow SrcRow = SourceTable.Rows[i];
        DataRow DstRow = ResTable.NewRow();
        for (int j = 0; j < SourceTable.Columns.Count; j++)
          DstRow[j] = SrcRow[j];

        DstRow["Hie_Level"] = 0;
#if DEBUG
        DstRow["Hie_LevelName"] = Levels[0].Name;
#endif
        //        DstValues["Hie_Text"] = new string(' ', Levels.Length*2)+Levels[0].GetRowText(DstValues);
        DstRow["Hie_Text"] = Levels[0].GetRowText(DstRow);
        DstRow["Hie_Order0"] = LevelData[0].GetRowOrder(DstRow);

        ResTable.Rows.Add(DstRow);
      }
    }

    /// <summary>
    /// Создание строк уровней иерархии
    /// </summary>
    private void AddLevels(InternalLevelData[] LevelData)
    {
      int nLevels = Levels.Length; // число уровней иерархии
      DataRowView drv;
      int i, j, k;
      for (i = 0; i < SourceTable.Rows.Count; i++)
      {
        DataRow MainRow = FDataView.Table.Rows[i];
        object[] Orders = new object[nLevels];
        for (j = 0; j < nLevels; j++)
        {
          if (j > 0)
            MainRow["Hie_Order" + j.ToString()] = LevelData[j].GetRowOrder(MainRow);
          Orders[nLevels - j - 1] = (int)(MainRow["Hie_Order" + j.ToString()]);
        }

        for (j = 0; j < (Levels.Length - 1); j++)
        {
          StringBuilder sb = new StringBuilder();
          //string spc = new string(' ', (Levels.Length - j - 1) * 2);

          // Добавляем заголовок строки
          if (!Levels[j + 1].SubTotalRowFirst)
          {
            Orders[nLevels - j - 1] = -2;
            if (FDataView.Find(Orders) < 0)
            {
              drv = FDataView.AddNew();
              for (k = 0; k < nLevels; k++)
                drv.Row["Hie_Order" + k.ToString()] = Orders[nLevels - k - 1];
              drv.Row["Hie_Level"] = j + 1;
#if DEBUG
              drv.Row["Hie_LevelName"] = Levels[j + 1].Name;
#endif
              drv.Row["Hie_Sum"] = false;
              drv.Row["Hie_Text"] = /*spc + *//*"Начало: " + */Levels[j + 1].GetRowText(MainRow, false);
              // Копирование "ключевых" полей уровней
              for (k = j + 1; k < Levels.Length; k++)
              {
                string[] FieldNames = Levels[k].ColumnNameArray;
                for (int l = 0; l < FieldNames.Length; l++)
                  drv.Row[FieldNames[l]] = MainRow[FieldNames[l]];
              }
              drv.EndEdit();
            }
          }

          // Добавляем итоговую строку
          Orders[nLevels - j - 1] = Int32.MaxValue; // порядок строк поменяем после расчета
          if (FDataView.Find(Orders) < 0)
          {
            drv = FDataView.AddNew();
            for (k = 0; k < nLevels; k++)
              drv.Row["Hie_Order" + k.ToString()] = Orders[nLevels - k - 1];
            drv.Row["Hie_Level"] = j + 1;
#if DEBUG
            drv.Row["Hie_LevelName"] = Levels[j + 1].Name;
#endif
            drv.Row["Hie_Sum"] = true;
            if (Levels[j + 1].SubTotalRowFirst)
              drv.Row["Hie_Text"] = Levels[j + 1].GetRowText(MainRow, false);
            else
              drv.Row["Hie_Text"] = Levels[j + 1].GetRowText(MainRow, true);
            // Копирование "ключевых" полей уровней
            for (k = j + 1; k < Levels.Length; k++)
            {
              string[] FieldNames = Levels[k].ColumnNameArray;
              for (int l = 0; l < FieldNames.Length; l++)
                drv.Row[FieldNames[l]] = MainRow[FieldNames[l]];
            }
            drv.EndEdit();
          }
        }
      }

      // Добавляем итоговую строку (или несколько строк)
      FTotalSumRows = new DataRow[TotalSumRowCount];
      for (i = 0; i < TotalSumRowCount; i++)
      {
        drv = FDataView.AddNew();
        for (k = 0; k < nLevels; k++)
          drv.Row["Hie_Order" + k.ToString()] = Int32.MaxValue;
        drv.Row["Hie_Level"] = nLevels;
#if DEBUG
        drv.Row["Hie_LevelName"] = "Итоговая строка";
#endif
        drv.Row["Hie_Sum"] = true;
        drv.Row["Hie_Text"] = "Итого";
        FTotalSumRows[i] = drv.Row;
        drv.EndEdit();
      }
    }

    /// <summary>
    /// Вычисление итоговых сумм
    /// </summary>
    private void CalcSums()
    {
      int i, j, k;
      DataColumn[] Columns = new DataColumn[SumColumnNames.Length];
      for (j = 0; j < SumColumnNames.Length; j++)
      {
        if (!FDataView.Table.Columns.Contains(SumColumnNames[j]))
          throw new BugException("Суммируемого столбца \"" + SumColumnNames[j] + "\" нет в таблице иерархического отчета");
        Columns[j] = FDataView.Table.Columns[SumColumnNames[j]];
      }

      decimal[,] aaVals = new decimal[Levels.Length + 1, Columns.Length];
      Array.Clear(aaVals, 0, aaVals.Length);


      for (i = 0; i < FDataView.Count; i++)
      {
        DataRow Row = FDataView[i].Row;
        int lvl = (int)Row["Hie_Level"];
        if (lvl == 0)
        {
          // Для строки данных выполняем запоминание значения
          for (k = 0; k <= Levels.Length; k++)
          {
            for (j = 0; j < Columns.Length; j++)
              aaVals[k, j] += DataTools.GetDecimal(Row[Columns[j]]);
          }
        }
        else
        {
          bool IsSum = (bool)Row["Hie_Sum"];
          if (IsSum)
          {
            FDataView[i].BeginEdit();
            for (j = 0; j < Columns.Length; j++)
            {
              Row[Columns[j]] = aaVals[lvl, j];
              for (k = 0; k <= lvl; k++)
                aaVals[k, j] = 0;
            }
            FDataView[i].EndEdit();
          }
        }
      }
    }

    /// <summary>
    /// Удаление лишних строк сумм
    /// </summary>
    private void DoHideExtraSumRows()
    {
      // Число найденных строк для каждого уровня
      int[] Counters = new int[Levels.Length + 1];
      Array.Clear(Counters, 0, Counters.Length);

      for (int i = 0; i < FDataView.Count; i++)
      {
        DataRow Row = FDataView[i].Row;

        int lvl = (int)Row["Hie_Level"];

        bool IsSum;
        if (lvl > 0)
          IsSum = (bool)Row["Hie_Sum"];
        else
          IsSum = false;

        if (IsSum)
        {
          if ((Counters[lvl - 1] == 1) && (lvl < Levels.Length))
          {
            Row.BeginEdit();
            Row["Hie_Hidden"] = true;
            Row.EndEdit();
          }
          Counters[lvl - 1] = 0;
        }
        else
        {
          Counters[lvl]++;
        }
      }

      // Удаляем ненужные строки
      DeleteHiddenRows(FDataView.Table);
    }


    /// <summary>
    /// Перемещаем строки подытогов для уровней с установленным признаком
    /// SubTotalRowFirst в начало группы строк
    /// </summary>
    private void MoveSumRowsFirst()
    {
      bool HasMovement = false;
      for (int i = 0; i < Levels.Length; i++)
      {
        if (Levels[i].SubTotalRowFirst)
          HasMovement = true;
      }
      if (!HasMovement)
        return;

      // Используем таблицу, а не DataView, т.к порядок строк меняемся
      foreach (DataRow Row in FDataView.Table.Rows)
      {
        //if (DataTools.GetInt(Row, "Hie_Level")==0)
        //  continue;
        if (!DataTools.GetBool(Row, "Hie_Sum"))
          continue;
        //for (int i = 0; i < Levels.Length; i++)
        //{
        //  if (Levels[i].SubTotalRowFirst)
        //  {
        //    string ColName = "Hie_Order" + (Levels.Length - i - 1).ToString();
        //    if (DataTools.GetInt(Row[ColName]) == int.MaxValue)
        //      Row[ColName.ToString()] = -2;
        //  }
        //}
        int Level = DataTools.GetInt(Row, "Hie_Level");
        if (Level < 1 || Level >= Levels.Length)
          continue;
        if (Levels[Level].SubTotalRowFirst)
        {
          string ColName = "Hie_Order" + (Level - 1).ToString();
          int CurrOrder = DataTools.GetInt(Row[ColName]);
          if (CurrOrder == int.MaxValue)
            Row[ColName] = -2;
        }

      }
    }


    /// <summary>
    /// Удаление строк, помеченных в поле "Hie_Hidden"
    /// </summary>
    /// <param name="Table">Таблица данных созданного просмотра</param>
    private void DeleteHiddenRows(DataTable Table)
    {
      for (int i = Table.Rows.Count - 1; i >= 0; i--)
      {
        if (DataTools.GetBool(Table.Rows[i], "Hie_Hidden"))
          Table.Rows[i].Delete();
      }
      Table.AcceptChanges();
    }

#endregion

#region Свойства и события, устанавливаемые методом CreateDataView()

    /// <summary>
    /// Созданный объект иерархического просмотра.
    /// Свойство получает значение после вызова CreateDataView()
    /// </summary>
    public DataView DataView { get { return FDataView; } }
    private DataView FDataView;

    /// <summary>
    /// Массив итоговых строк просмотра. (обычно одна строка). Свойство заполняется
    /// после формирования просмотра и содержит количество строк, равное TotalSumsRowCount
    /// </summary>
    public DataRow[] TotalSumRows { get { return FTotalSumRows; } }
    private DataRow[] FTotalSumRows;

    /// <summary>
    /// Событие вызывается перед завершением метода CreateDataView, когда итог и подытоги
    /// расчитаны, но нумерация строк еще не выполнена
    /// Обработчик может инициализировать TotalSumRows, удалять ненужные строки
    /// </summary>
    public event EventHandler AfterCreateView;

    /// <summary>
    /// Вызывает событие AfterCreateView
    /// </summary>
    protected virtual void OnAfterCreateView()
    {
      if (AfterCreateView != null)
        AfterCreateView(this, EventArgs.Empty);
    }

#if DEBUG

    private void CheckIfCreated()
    {
      if (FDataView == null)
        throw new InvalidOperationException("Просмотр не был создан. Не было вызова CreateDataView()");
    }

    /// <summary>
    /// Проверка строки, что она относится к результирующей таблице
    /// </summary>
    /// <param name="Row"></param>
    private void CheckRow(DataRow Row)
    {
      CheckIfCreated();
      if (Row == null)
        throw new ArgumentNullException("Row");
      if (Row.Table != FDataView.Table)
        throw new ArgumentException("Строка относится к другой таблице", "Row");
    }

#endif

#endregion

#region Методы, которые могут использоваться после создания просмотра

    /// <summary>
    /// Получить имя уровня для заданной строки (свойство HieViewLevel.NamePart)
    /// Для итоговой строки возвращается ""
    /// </summary>
    /// <param name="Row">Строка из просмотра, созданного CreateView</param>
    /// <returns>Имя уровня</returns>
    public string GetLevelName(DataRow Row)
    {
      if (Row == null)
        return String.Empty;

#if DEBUG
      CheckRow(Row);
#endif

      int Level = DataTools.GetInt(Row, "Hie_Level");
      if (Level >= Levels.Length)
        return String.Empty;
      return Levels[Level].Name;
    }

    /// <summary>
    /// Получить индекс уровня для заданной строки (0 .. Levels.Length-1)
    /// Для итоговой строки возвращается (-1)
    /// </summary>
    /// <param name="Row">Строка из просмотра, созданного CreateView</param>
    /// <returns>Имя уровня</returns>
    public int GetLevelIndex(DataRow Row)
    {
      if (Row == null)
        return -1;

#if DEBUG
      CheckRow(Row);
#endif

      int Level = DataTools.GetInt(Row, "Hie_Level");
      if (Level >= Levels.Length)
        return -1;
      return Level;
    }

    /// <summary>
    /// Является ли заданная строка итогом или подытогом ?
    /// </summary>
    /// <param name="Row"></param>
    /// <returns></returns>
    public bool IsSumRow(DataRow Row)
    {
      if (Row == null)
        return false;

#if DEBUG
      CheckRow(Row);
#endif


      int Level = DataTools.GetInt(Row, "Hie_Level");
      if (Level > 0)
        return DataTools.GetBool(Row, "Hie_Sum");
      else
        return false;
    }

    /// <summary>
    /// Является ли заданная строка подзаголовком ?
    /// </summary>
    /// <param name="Row"></param>
    /// <returns></returns>
    public bool IsHeaderRow(DataRow Row)
    {
      if (Row == null)
        return false;

#if DEBUG
      CheckRow(Row);
#endif

      int Level = DataTools.GetInt(Row, "Hie_Level");
      if (Level > 0)
        return !DataTools.GetBool(Row, "Hie_Sum");
      else
        return false;
    }

    /// <summary>
    /// Получение значения для поля <param name="ColumnName"></param>. Значение возвращается,
    /// если для выбранной строки уровень детализации не превышает заданный.
    /// Значение не возвращается, если ключевое поле не найдено
    /// Если уровень иерархии содержит несколько полей, то FieldName должно задавать
    /// все имена полей, разделенных запятыми, так, как они объявлены в уровне.
    /// В этом случае возвращается массив значений
    /// </summary>
    /// <param name="Row">Строка в построенном иерархическом просмотре</param>
    /// <param name="ColumnName">Имя ключевого поля</param>
    /// <param name="Value">Сюда помещается возвращаемое значение или null, если значение не возвращается</param>
    /// <returns>true, если значение возщвращается</returns>
    public bool GetLevelValue(DataRow Row, string ColumnName, out object Value)
    {
      Value = null;

      if (Row == null)
        return false;

#if DEBUG
      CheckRow(Row);
#endif

      int Level = -1;
      for (int i = 0; i < Levels.Length; i++)
      {
        if (Levels[i].ColumnName == ColumnName)
        {
          Level = i;
          break;
        }
      }
      if (Level < 0)
        return false; // Уровень иерархии не найден
      int ThisLevel = DataTools.GetInt(Row, "Hie_Level");
      if (ThisLevel > Level)
        return false;
      if (ColumnName.IndexOf(',') >= 0)
      {
        object[] a = new object[Levels[Level].ColumnNameArray.Length];
        for (int i = 0; i < Levels[Level].ColumnNameArray.Length; i++)
          a[i] = Row[Levels[Level].ColumnNameArray[i]];
        Value = a;
      }
      else
        Value = Row[ColumnName];
      return true;
    }

#if XXXXX
#region Получение типизированных значений

    public string GetLevelAsString(DataRow Row, string FieldName)
    { 
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetString(Value);
    }

    public bool GetLevelAsBool(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetBool(Value);
    }

    public DateTime?GetLevelAsNullableDateTime(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetNullableDateTime(Value);
    }

    public int GetLevelAsInt(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetInt(Value);
    }

    public float GetLevelAsSingle(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetSingle(Value);
    }

    public double GetLevelAsDouble(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetDouble(Value);
    }

    public decimal GetLevelAsDeciumal(DataRow Row, string FieldName)
    {
      object Value;
      GetLevelValue(Row, FieldName, out Value);
      return DataTools.GetDecimal(Value);
    }

#endregion
#endif
    /// <summary>
    /// Получить заголовочную строку для данной строки Row заданного уровня Level
    /// Если для строки Row уровень Level не определен (то есть строка Row относится
    /// к более высокому уровню, чем Level), то возвращается null
    /// </summary>
    /// <param name="Row">Строки из просмотра dv</param>
    /// <param name="Level">Требуемый уровень</param>
    /// <returns>Строка, являющаяся заголовком для уровня Level или null</returns>
    public DataRow GetHeaderRow(DataRow Row, int Level)
    {
#if DEBUG
      CheckRow(Row);
#endif

      object[] Keys = new object[Levels.Length];
      //int RowLevel=DataTools.GetInt(Row, "Hie_Level");
      for (int i = 0; i < Levels.Length; i++)
      {
        object k;
        //if (i == Level)
        //  k = -2;
        //else
        //{
        //  if (i < Level)
        //    k = int.MaxValue;
        //  else
        //    k = Row["Hie_Order" + i.ToString()];
        //}
        // Исправлено 06.01.2011
        if (i >= Level)
          k = Row["Hie_Order" + i.ToString()];
        else if (i == (Level - 1))
          k = -2;
        else
          k = int.MaxValue; // 16.12.2014
        Keys[Levels.Length - i - 1] = k;
      }
      int p = FDataView.Find(Keys);
      if (p < 0)
        return null;
      else
        return FDataView[p].Row;
    }

    /// <summary>
    /// Получить текстовое значения для заданного уровня Level для строки Row
    /// Если для строки Row уровень Level не определен (то есть строка Row относится
    /// к более высокому уровню, чем Level), то возвращается пустая строка
    /// Выполняется вызов GetHeaderRow() и возвращается значение поля "Hie_Text"
    /// </summary>
    /// <param name="Row">Строки из просмотра dv</param>
    /// <param name="Level">Требуемый уровень</param>
    /// <returns>Текстовое значение или null</returns>
    public string GetHeaderText(DataRow Row, int Level)
    {
      DataRow HeaderRow = GetHeaderRow(Row, Level);
      if (HeaderRow == null)
        return String.Empty;
      else
        return DataTools.GetString(HeaderRow, "Hie_Text");
    }

#if XXX
    public DataRow GetFirstZeroLevelRow(DataRow Row)
    {
#if DEBUG
      CheckRow(Row);
#endif

      object[] Keys = new object[Levels.Length];
      int Level = DataTools.GetInt(Row, "Hie_Level");
      for (int i = 0; i < Levels.Length; i++)
      {
        object k;
        if (i < Level)
          k = 1;
        else
          k = Row["Hie_Order" + i.ToString()];
        Keys[Levels.Length - i - 1] = k;
      }
      int p = FDataView.Find(Keys);
      if (p < 0)
      {
        // 01.02.2010
        // В некоторых случаях первая строка в группе может иметь значение в поле
        // Hie_OrderXXX не 1 а, например, 2. В этом случае точный поиск ключа 
        // невозможен
        if (Level > 0)
        {
          // Создаем отдельный просмотр
          DataView dv2 = new DataView(FDataView.Table, dv.RowFilter, dv.Sort, dv.RowStateFilter);
          // и навешиваем на него доп. фильтры
          DataFilter[] Filters = new DataFilter[Levels.Length - Level];
          for (int i = 0; i < Filters.Length; i++)
            Filters[i] = new ValueFilter("Hie_Order" + (Levels.Length - i - 1).ToString(), Keys[i]);
          DataFilter Filter2 = AndFilter.FromArray(Filters);
          DataFilter.AddToDataViewAnd(dv2, Filter2);

          // Теперь вручную перебираем строки, пропуская строки заголовков и итоги.
          // Первая подходящая строка - то, что нам нужно
          for (int i = 0; i < dv2.Count; i++)
          {
            DataRow Row2 = dv2[i].Row;
            bool Found = true;
            for (int j = 0; j < Level; j++)
            {
              int ord = DataTools.GetInt(Row2, "Hie_Order" + j.ToString());
              if (ord <= 0 || ord == int.MaxValue)
              {
                Found = false;
                break;
              }
            }
            if (Found)
              return Row2;
          }
        }
        return null;
      }
      else
        return dv[p].Row;
    }

#endif

#endregion

#region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true после вызова CreateDataView()
    /// </summary>
    public bool IsReadOnly
    {
      get { return FDataView != null; }
    }

    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

#endregion
  }

  /// <summary>
  /// Редактор дополнительного параметра для уровня иерархии (абстрактный базовый класс)
  /// Присоединяется к свойству HieViewLevel.ParamEditor.
  /// Реализует редактирование, чтение и запись произвольного параметра (или параметров),
  /// относящихся к данному уровню
  /// Для хранение значения параметра используется HieViewLevel.ExtBalues, поэтому
  /// один объект может использоваться для нескольких HieViewLevel
  /// </summary>
  public abstract class EFPHieViewLevelParamEditor
  {
#region Абстрактные методы

    public abstract bool PerformEdit(EFPHieViewLevel Level);

    public abstract void WriteConfig(EFPHieViewLevel Level, CfgPart Part);

    public abstract void ReadConfig(EFPHieViewLevel Level, CfgPart Part);

    public abstract string GetText(EFPHieViewLevel Level);

#endregion
  }

#if XXX
  /// <summary>
  /// Неабстрактная реализация редактора уровня иерархии, когда режим задается
  /// перечислением возможных значений.
  /// Для хранения значения используется код в HieViewLevel.ExtValues["Режим"]
  /// Для записи в секции конфигурации используется строковый параметр "Режим"
  /// </summary>
  public class EFPHieViewLevelEnumParamEditor : EFPHieViewLevelParamEditor
  {
#region Конструкторы

    public EFPHieViewLevelEnumParamEditor(string[] Codes, string[] Names)
    {
#if DEBUG
      if (Codes == null)
        throw new ArgumentNullException("Codes");
      if (Names == null)
        throw new ArgumentNullException("Names");
      if (Names.Length != Codes.Length)
        throw new ArgumentException("Длина массива имен не совпадает с длиной массива кодов", "Names");
#endif
      FCodes = Codes;
      FNames = Names;
    }

    public EFPHieViewLevelEnumParamEditor(string[] Names)
      : this(GetCodes(Names), Names)
    {
    }

    private static string[] GetCodes(string[] Names)
    {
      string[] Codes = new string[Names.Length];
      for (int i = 0; i < Codes.Length; i++)
        Codes[i] = i.ToString();
      return Codes;
    }

#endregion

#region Свойства

    /// <summary>
    /// Коды используются для хранения текущего значения параметра
    /// Задается в конструкторе
    /// </summary>
    public string[] Codes { get { return FCodes; } }
    private string[] FCodes;

    /// <summary>
    /// Имена используются для отображения на экране
    /// Задается в конструкторе
    /// </summary>
    public string[] Names { get { return FNames; } }
    private string[] FNames;

#endregion

#region Получение значения

    public string GetSelectedCode(EFPHieViewLevel Level)
    {
      string Code = Level.ExtValues.GetString("Режим");
      int p = Array.IndexOf<string>(FCodes, Code);
      if (p < 0)
        return FCodes[0];
      else
        return FCodes[p];
    }

    public void SetSelectedCode(EFPHieViewLevel Level, string Code)
    {
      Level.ExtValues["Режим"] = Code;
    }

    public int GetSelectedIndex(EFPHieViewLevel Level)
    {
      string Code = Level.ExtValues.GetString("Режим");
      int p = Array.IndexOf<string>(FCodes, Code);
      if (p < 0)
        return 0;
      else
        return p;
    }

    public void SetSelectedIndex(EFPHieViewLevel Level, int Index)
    {
      Level.ExtValues["Режим"] = FCodes[Index];
    }

#endregion

#region Переопределенные методы

    public override bool PerformEdit(EFPHieViewLevel Level)
    {
      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.ImageKey = Level.ImageKey;
      dlg.Title = Level.DisplayName;
      dlg.GroupTitle = "Режим";
      dlg.Items = Names;
      dlg.SelectedIndex = GetSelectedIndex(Level);

      if (dlg.ShowDialog() != DialogResult.OK)
        return false;

      SetSelectedIndex(Level, dlg.SelectedIndex);
      return true;
    }

    public override void WriteConfig(EFPHieViewLevel Level, CfgPart Part)
    {
      Part.SetString("Режим", GetSelectedCode(Level));
    }

    public override void ReadConfig(EFPHieViewLevel Level, CfgPart Part)
    {
      string Code = Part.GetString("Режим");
      int p = Array.IndexOf<string>(FCodes, Code);
      if (p < 0)
        SetSelectedIndex(Level, 0);
      else
        SetSelectedIndex(Level, p);
    }

    public override string GetText(EFPHieViewLevel Level)
    {
      int p = GetSelectedIndex(Level);
      return Names[p];
    }

#endregion
  }
#endif

#if XXX
  /// <summary>
  /// Реализация редактора уровня иерархии типа "Дата" с возможностью выбора
  /// уровня обобщения Дата-Месяц-Квартал-Год
  /// </summary>
  public class EFPHieViewLevelDateGeneralizationParamEditor : EFPHieViewLevelEnumParamEditor
  {
#region Конструктор

    private static readonly string[] DateTypeNames=new string[]{"Дата", "Месяц", "Квартал", "Год"};

    public EFPHieViewLevelDateGeneralizationParamEditor()
      : base(DateTypeNames, DateTypeNames)
    { 
    }

#endregion

#region Дополнительные методы

    public AccDepDateGeneralization GetSelectedMode(EFPHieViewLevel Level)
    {
      return (AccDepDateGeneralization)(base.GetSelectedIndex(Level));
    }

    public void SetSelectedMode(EFPHieViewLevel Level, AccDepDateGeneralization Mode)
    {
      base.SetSelectedIndex(Level, (int)Mode);
    }

#endregion
  }
#endif

}
#endif

