using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;
using System.ComponentModel;
using FreeLibSet.Models.Tree;
using FreeLibSet.Collections;

namespace FreeLibSet.UICore
{
  /// <summary>
  /// Интерфейс вспомогательного объекта, предназначенного для ручной перестановки строк табличного просмотра, связанного с DataTable.
  /// Требуемый порядок строк в просмотре определяется числовым полем.
  /// Интерфейс реализуется классами DataTableReorderHelper (плоская таблица) и DataTableTreeReorderHelper (дерево).
  /// Используется для реализации команд локального меню в EFPDataGridViewCommandItems и EFPDataTreeViewCommandItems.
  /// </summary>
  public interface IDataReorderHelper
  {
    #region Методы

    /// <summary>
    /// Перемещение выбранных строк вниз.
    /// Метод меняет значение числового поля, предназначенного для сортировки, не только для выбранных строк, но и, вероятно,
    /// для других строк в просмотре.
    /// </summary>
    /// <param name="rows">Строки, выбранные в просмотре</param>
    /// <returns>True, если перемещение было выполнено.
    /// False, если строки уже находятся внизу списка и их некуда перемещать.</returns>
    bool MoveDown(DataRow[] rows);

    /// <summary>
    /// Перемещение выбранных строк вверх.
    /// Метод меняет значение числового поля, предназначенного для сортировки, не только для выбранных строк, но и, вероятно,
    /// для других строк в просмотре.
    /// </summary>
    /// <param name="rows">Строки, выбранные в просмотре</param>
    /// <returns>True, если перемещение было выполнено.
    /// False, если строки уже находятся вверху списка и их некуда перемещать.</returns>
    bool MoveUp(DataRow[] rows);

    /// <summary>
    /// Инициализация строки после добавления в просмотр или редактирования.
    /// Если числовое поле для сортировки имеет значение 0, то для него устанавливается такое значение, чтобы строка
    /// оказалась в конце списка (для дерева - в пределах своего уровня).
    /// Если поле уже содержит нулевое значение, то никаких действий не выполняется.
    /// </summary>
    /// <param name="rows">Строки данных, которые нужно инициализировать</param>
    /// <param name="otherRowsChanged">Сюда записывается значение true, если были изменены другие строки в просмотре, кроме выбранных.
    /// Используется только для иерархической структуры, для плоской таблицы (DataTableReorderHelper) всегда записывается false.</param>
    /// <returns>True, если строки (одна или несколько) содержали нулевое значение и были инициализированы.
    /// Если все строки уже содержали ненулевое значение, то возвращается false.</returns>
    bool InitRows(DataRow[] rows, out bool otherRowsChanged);

    /// <summary>
    /// Инициализация числового поля значениями 1,2,3,... Порядок строк в просмотре от этого не меняется.
    /// </summary>
    /// <returns>True, если значение было изменено хотя бы для одной строки</returns>
    bool Reorder();

    /// <summary>
    /// Инициализация числового поля значениями 1,2,3,... в соответствии с требуемым порядком.
    /// Предполагается, что <paramref name="desiredOrder"/> содержит все строки просмотра.
    /// Для дерева результирующий порядок сортировки может отличаться, так как он учитывает порядок обхода узлов дерева,
    /// а <paramref name="desiredOrder"/> не обязан его учитывать.
    /// </summary>
    /// <returns>True, если значение было изменено хотя бы для одной строки</returns>
    bool Reorder(DataRow[] desiredOrder);

    #endregion
  }

  /// <summary>
  /// Объект для ручной перестановки строк в плоской таблице
  /// </summary>
  public class DataTableReorderHelper : IDataReorderHelper
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="dv">Просмотр DataView</param>
    /// <param name="orderColumnName">Числовой столбец, используемый для сортировки</param>
    public DataTableReorderHelper(DataView dv, string orderColumnName)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
      if (String.IsNullOrEmpty(orderColumnName))
        throw new ArgumentNullException("orderColumnName");
#endif

      _DV = dv;
      _OrderColumnPos = _DV.Table.Columns.IndexOf(orderColumnName);
      if (_OrderColumnPos < 0)
        throw new ArgumentException("Таблица " + dv.Table.TableName + " не содержит столбца с именем \"" + orderColumnName + "\"", "orderColumnName");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Просмотр DataView.
    /// </summary>
    public DataView DV { get { return _DV; } }
    private DataView _DV;

    /// <summary>
    /// Имя числового столбца, используемого для сортировки
    /// </summary>
    public string OrderColumnName { get { return _DV.Table.Columns[_OrderColumnPos].ColumnName; } }
    /// <summary>
    /// Позиция числового столбца, используемого для сортировки
    /// </summary>
    private int _OrderColumnPos;

    /// <summary>
    /// Возвращает true, если просмотр DV отсортирован по OrderColumnName
    /// </summary>
    private bool IsSuitableDV
    {
      get
      {
        if (String.IsNullOrEmpty(_DV.Sort))
          return false; // нет сортировки
        if (_DV.Sort.IndexOf(',') >= 0)
          return false; // сортировка по нескольким полям

        if (_DV.AllowNew)
          return false; // иначе можно получить "виртуальную" строку

        string realColName;
        ListSortDirection realDir;
        DataTools.GetDataViewSortSingleColumnName(_DV.Sort, out realColName, out realDir);
        return String.Equals(realColName, OrderColumnName, StringComparison.OrdinalIgnoreCase) &&
          realDir == ListSortDirection.Ascending;
      }
    }

    #endregion

    #region IDataReorderHelper Members

    /// <summary>
    /// Перемещение выбранных строк вниз.
    /// Метод меняет значение числового поля, предназначенного для сортировки, не только для выбранных строк, но и для других строк в просмотре.
    /// </summary>
    /// <param name="rows">Строки, выбранные в просмотре</param>
    /// <returns>True, если перемещение было выполнено.
    /// False, если строки уже находятся внизу списка и их некуда перемещать.</returns>
    public bool MoveDown(DataRow[] rows)
    {
      return DoMove(rows, true);
    }

    /// <summary>
    /// Перемещение выбранных строк вверх.
    /// Метод меняет значение числового поля, предназначенного для сортировки, не только для выбранных строк, но и для других строк в просмотре.
    /// </summary>
    /// <param name="rows">Строки, выбранные в просмотре</param>
    /// <returns>True, если перемещение было выполнено.
    /// False, если строки уже находятся вверху списка и их некуда перемещать.</returns>
    public bool MoveUp(DataRow[] rows)
    {
      return DoMove(rows, false);
    }

    private bool DoMove(DataRow[] selRows, bool down)
    {
#if DEBUG
      if (selRows == null)
        throw new ArgumentNullException("rows");
#endif

      if (selRows.Length == 0)
        return false;

      // 1. Загружаем полный список строк DataRow в массив
      DataRow[] rows1 = DataTools.GetDataViewRows(_DV);

      // 2. Получаем позиции выбранных строк в массиве всех строк
      int[] selPoss = GetSelRowPositions(ref selRows, rows1);
      if (selRows.Length == 0)
        return false; // еще одна проверка, т.к. строки могли быть удалены

      // 3. Проверяем, что не уперлись в границы списка
      if (down)
      {
        if (selPoss[selPoss.Length - 1] == rows1.Length - 1)
          return false;
      }
      else
      {
        if (selPoss[0] == 0)
          return false;
      }

      // 4. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataRow[] rows2 = new DataRow[rows1.Length];

      // 5. Копируем в rows2 строки из Rows1 со сдвигом для позиций, существующих в selRows.
      // В процессе перемещения будем очищать массив Rows1
      int delta = down ? 1 : -1; // значение смещения
      for (int i = 0; i < selPoss.Length; i++)
      {
        int thisPos = selPoss[i];
        rows2[thisPos + delta] = rows1[thisPos];
        rows1[thisPos] = null;
      }

      // 6. Перебираем исходный массив и оставшиеся непустые строки размещаем в
      // новом массиве, отыскивая пустые места. Для этого используем переменную FreePos
      // для указания на очередную пустую позицию второго массива
      int freePos = 0;
      for (int i = 0; i < rows1.Length; i++)
      {
        if (rows1[i] == null) // перемещенная позиция
          continue;
        // Поиск места
        while (rows2[freePos] != null)
          freePos++;
        // Нашли дырку
        rows2[freePos] = rows1[i];
        freePos++;
      }

      // 7. Записываем номера строк в поле согласно новому порядку в Rows2
      bool changed = false;
      for (int i = 0; i < rows2.Length; i++)
      {
        if (DataTools.GetInt(rows2[i][_OrderColumnPos]) != (i + 1))
        {
          rows2[i][_OrderColumnPos] = i + 1;
          changed = true;
        }
      }

      return changed;
    }

    /// <summary>
    /// Находит позиции выбранных для перемещения строк в просмотре.
    /// Из массива выбранных строк выбрасываются ненайденные строки.
    /// Затем массив выбранных строк сортируется по возрастанию номеров позиций.
    /// </summary>
    /// <param name="selRows">Выбранные строки. Этот массив сортируется и, возможно, сокращается</param>
    /// <param name="rows1"></param>
    /// <returns></returns>
    internal static int[] GetSelRowPositions(ref DataRow[] selRows, DataRow[] rows1)
    {
      // Промежуточная коллекция.
      // Используется для сортировки и поиска повторов.
      // Ключ - позиция строки из selRows, значение - строка.
      SortedDictionary<int, DataRow> dict = new SortedDictionary<int, DataRow>();

      for (int i = 0; i < selRows.Length; i++)
      {
        if (selRows[i] == null)
          throw new ArgumentException("rows[" + i.ToString() + "]=null", "rows");

        int p = Array.IndexOf<DataRow>(rows1, selRows[i]);
        if (p < 0)
          continue;
        if (dict.ContainsKey(p))
          continue; // строка входит дважды
        dict.Add(p, selRows[i]);
      }

      if (dict.Count != selRows.Length)
        selRows = new DataRow[dict.Count];
      int[] selPoss = new int[dict.Count];
      dict.Keys.CopyTo(selPoss, 0);
      dict.Values.CopyTo(selRows, 0);

      return selPoss;
    }

    /// <summary>
    /// Инициализация строки после добавления в просмотр или редактирования.
    /// Если числовое поле для сортировки имеет значение 0, то для него устанавливается такое значение, чтобы строка
    /// оказалась в конце списка (для дерева - в пределах своего уровня).
    /// Если поле уже содержит нулевое значение, то никаких действий не выполняется.
    /// </summary>
    /// <param name="rows">Строки данных, которые нужно инициализировать</param>
    /// <param name="otherRowsChanged">Сюда записывается false.</param>
    /// <returns>True, если строки (одна или несколько) содержали нулевое значение и были инициализированы.
    /// Если все строки уже содержали ненулевое значение, то возвращается false.</returns>
    public bool InitRows(DataRow[] rows, out bool otherRowsChanged)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif
      otherRowsChanged = false; // всегда
      if (rows.Length == 0)
        return false;

      bool changed = false;
      int maxVal = -1; // пока не определено
      for (int i = 0; i < rows.Length; i++)
      {
        if (DataTools.GetInt(rows[i][_OrderColumnPos]) == 0)
        {
          changed = true;
          if (maxVal < 0)
          {
            if (IsSuitableDV)
            {
              if (_DV.Count == 0)
                // Строки может и не быть пока в просмотре, поэтому вполне допускается DataView.Count=0.
                maxVal = 0;
              else
                maxVal = DataTools.GetInt(_DV[_DV.Count - 1].Row[_OrderColumnPos]);
            }
            else
            {
              // Нет смысла создавать одноразовый DataView, чтобы найти максимальное значение
              maxVal = DataTools.MaxInt(_DV, OrderColumnName, true) ?? 0;
            }
          }

          maxVal++;
          rows[i][_OrderColumnPos] = maxVal;
        }
      }

      return changed;
    }

    /// <summary>
    /// Инициализация числового поля значениями 1,2,3,... Порядок строк в просмотре от этого не меняется.
    /// </summary>
    /// <returns>True, если значение было изменено хотя бы для одной строки</returns>
    public bool Reorder()
    {
      return Reorder(DataTools.GetDataViewRows(_DV));
    }

    /// <summary>
    /// Инициализация числового поля значениями 1,2,3,... в соответствии с требуемым порядком.
    /// Предполагается, что <paramref name="desiredOrder"/> содержит все строки просмотра.
    /// </summary>
    /// <returns>True, если значение было изменено хотя бы для одной строки</returns>
    public bool Reorder(DataRow[] desiredOrder)
    {
#if DEBUG
      if (desiredOrder == null)
        throw new ArgumentNullException("desiredOrder");
#endif

      bool changed = false;
      for (int i = 0; i < desiredOrder.Length; i++)
      {
        if (DataTools.GetInt(desiredOrder[i][_OrderColumnPos]) != i + 1)
        {
          changed = true;
          desiredOrder[i][_OrderColumnPos] = i + 1;
        }
      }
      return changed;
    }

    #endregion
  }

  /// <summary>
  /// Объект для ручной перестановки строк в таблице иерархического просмотра на основании IDataTableTreeModel.
  /// Перестановка строк выполняется в пределах одного родительского узла, но при этом может меняться порядок остальных
  /// строк, чтобы обычный табличный просмотр отображал строки в том же порядке, что и дерево.
  /// </summary>
  public class DataTableTreeReorderHelper : IDataReorderHelper
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="model">Модель иерархического просмотра</param>
    /// <param name="orderColumnName">Числовой столбец, используемый для сортировки</param>
    public DataTableTreeReorderHelper(IDataTableTreeModel model, string orderColumnName)
    {
#if DEBUG
      if (model == null)
        throw new ArgumentNullException("model");
      if (String.IsNullOrEmpty(orderColumnName))
        throw new ArgumentNullException("orderColumnName");
#endif

      _Model = model;
      _OrderColumnPos = model.Table.Columns.IndexOf(orderColumnName);
      if (_OrderColumnPos < 0)
        throw new ArgumentException("Таблица " + model.Table.TableName + " не содержит столбца с именем \"" + orderColumnName + "\"", "orderColumnName");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Модель иерархического просмотра
    /// </summary>
    public IDataTableTreeModel Model { get { return _Model; } }
    private IDataTableTreeModel _Model;

    /// <summary>
    /// Имя числового столбца, используемого для сортировки
    /// </summary>
    public string OrderColumnName { get { return _Model.Table.Columns[_OrderColumnPos].ColumnName; } }
    /// <summary>
    /// Позиция числового столбца, используемого для сортировки
    /// </summary>
    private int _OrderColumnPos;

    /// <summary>
    /// Возвращает true, если просмотр Model.Table.DefaultView отсортирован по OrderColumnName
    /// </summary>
    private bool IsSuitableDV
    {
      get
      {
        if (String.IsNullOrEmpty(_Model.Table.DefaultView.Sort))
          return false; // нет сортировки
        if (_Model.Table.DefaultView.Sort.IndexOf(',') >= 0)
          return false; // сортировка по нескольким полям

        if (_Model.Table.DefaultView.AllowNew)
          return false; // иначе можно получить "виртуальную" строку

        string realColName;
        ListSortDirection realDir;
        DataTools.GetDataViewSortSingleColumnName(_Model.Table.DefaultView.Sort, out realColName, out realDir);
        return String.Equals(realColName, OrderColumnName, StringComparison.OrdinalIgnoreCase) &&
          realDir == ListSortDirection.Ascending;
      }
    }

    #endregion

    #region IDataReorderHelper Members

    /// <summary>
    /// Перемещение выбранных строк вниз.
    /// Метод меняет значение числового поля, предназначенного для сортировки, не только для выбранных строк, но и для других строк в просмотре.
    /// Если строки относятся к разным родительским узлам, то никаких действий не выполняется.
    /// </summary>
    /// <param name="rows">Строки, выбранные в просмотре</param>
    /// <returns>True, если перемещение было выполнено.
    /// False, если строки уже находятся внизу списка и их некуда перемещать.</returns>
    public bool MoveDown(DataRow[] rows)
    {
      return DoMove(rows, true);
    }

    /// <summary>
    /// Перемещение выбранных строк вверх.
    /// Метод меняет значение числового поля, предназначенного для сортировки, не только для выбранных строк, но и для других строк в просмотре.
    /// Если строки относятся к разным родительским узлам, то никаких действий не выполняется.
    /// </summary>
    /// <param name="rows">Строки, выбранные в просмотре</param>
    /// <returns>True, если перемещение было выполнено.
    /// False, если строки уже находятся вверху списка и их некуда перемещать.</returns>
    public bool MoveUp(DataRow[] rows)
    {
      return DoMove(rows, false);
    }

    private bool DoMove(DataRow[] selRows, bool down)
    {
#if DEBUG
      if (selRows == null)
        throw new ArgumentNullException("rows");
#endif

      if (selRows.Length == 0)
        return false;

      // 0. Проверяем, что все строки относятся к одному родительскому узлу и получаем путь к родительскому узлу
      TreePath parentPath = Model.TreePathFromDataRow(selRows[0]).Parent;
      for (int i = 1; i < selRows.Length; i++)
      {
        TreePath thisPath = Model.TreePathFromDataRow(selRows[i]);
        if (!thisPath.IsChildOf(parentPath))
          return false;
      }

      // 1. Загружаем полный список строк DataRow для одного уровня
      List<DataRow> rows1 = new List<DataRow>();
      foreach (object node in Model.GetChildren(parentPath))
      {
        TreePath rowPath = new TreePath(parentPath, node);
        rows1.Add(Model.TreePathToDataRow(rowPath));
      }

      // 2. Получаем позиции выбранных строк в массиве всех строк
      int[] selPoss = DataTableReorderHelper.GetSelRowPositions(ref selRows, rows1.ToArray());
      if (selRows.Length == 0)
        return false; // еще одна проверка, т.к. строки могли быть удалены

      // 3. Проверяем, что не уперлись в границы списка в пределах уровня
      if (down)
      {
        if (selPoss[selPoss.Length - 1] == rows1.Count - 1)
          return false;
      }
      else
      {
        if (selPoss[0] == 0)
          return false;
      }

      // 4. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataRow[] rows2 = new DataRow[rows1.Count];

      // 5. Копируем в rows2 строки из rows1 со сдвигом для позиций, существующих в selRows.
      // В процессе перемещения будем очищать массив Rows1
      int delta = down ? 1 : -1; // значение смещения
      for (int i = 0; i < selPoss.Length; i++)
      {
        int thisPos = selPoss[i];
        rows2[thisPos + delta] = rows1[thisPos];
        rows1[thisPos] = null;
      }

      // 6. Перебираем исходный массив и оставшиеся непустые строки размещаем в
      // новом массиве, отыскивая пустые места. Для этого используем переменную FreePos
      // для указания на очередную пустую позицию второго массива
      int freePos = 0;
      for (int i = 0; i < rows1.Count; i++)
      {
        if (rows1[i] == null) // перемещенная позиция
          continue;
        // Поиск места
        while (rows2[freePos] != null)
          freePos++;
        // Нашли дырку
        rows2[freePos] = rows1[i];
        freePos++;
      }

      // 7. Записываем номера строк в поле согласно новому порядку в Rows2
      bool changed = false;
      for (int i = 0; i < rows2.Length; i++)
      {
        if (DataTools.GetInt(rows2[i][_OrderColumnPos]) != (i + 1))
        {
          rows2[i][_OrderColumnPos] = i + 1;
          changed = true;
        }
      }

      // 8. Переставляем нумерацию во всем дереве
      if (changed)
        Reorder();

      return changed;
    }

    /// <summary>
    /// Инициализация строки после добавления в просмотр или редактирования.
    /// Если числовое поле для сортировки имеет значение 0, то для него устанавливается такое значение, чтобы строка
    /// оказалась в конце списка в пределах своего родтельского элемента.
    /// Если поле уже содержит нулевое значение, то никаких действий не выполняется.
    /// </summary>
    /// <param name="rows">Строки данных, которые нужно инициализировать.
    /// В отличие от методов MoveUp() и MoveDown(), строки не обязаны относиться к одному родительскому узлу</param>
    /// <param name="otherRowsChanged">Сюда записывается значение true, если были изменены другие строки в просмотре, кроме выбранных.</param>
    /// <returns>True, если строки (одна или несколько) содержали нулевое значение и были инициализированы.
    /// Если все строки уже содержали ненулевое значение, то возвращается false.</returns>
    public bool InitRows(DataRow[] rows, out bool otherRowsChanged)
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif

      int maxVal = -1;
      bool changed = false;
      for (int i = 0; i < rows.Length; i++)
      {
        if (DataTools.GetInt(rows[i][_OrderColumnPos]) == 0)
        {
          changed = true;
          if (maxVal < 0)
          {
            if (IsSuitableDV)
            {
              if (Model.Table.DefaultView.Count == 0)
                // Строки может и не быть пока в просмотре, поэтому вполне допускается DataView.Count=0.
                maxVal = 0;
              else
                maxVal = DataTools.GetInt(Model.DataView[Model.DataView.Count - 1].Row[_OrderColumnPos]);
            }
            else
            {
              // Нет смысла создавать одноразовый DataView, чтобы найти максимальное значение
              maxVal = DataTools.MaxInt(Model.Table.DefaultView, OrderColumnName, true) ?? 0;
            }
          }

          maxVal++;
          rows[i][_OrderColumnPos] = maxVal;
        }
      }

      if (changed)
        otherRowsChanged = Reorder();
      else
        otherRowsChanged = false;
      return changed;
    }

    /// <summary>
    /// Инициализация числового поля значениями 1,2,3,... Порядок строк в просмотре от этого не меняется.
    /// </summary>
    /// <returns>True, если значение было изменено хотя бы для одной строки</returns>
    public bool Reorder()
    {
      bool changed = false;
      int cnt = 0;
      foreach (TreePath path in new TreePathEnumerable(_Model))
      {
        cnt++;
        DataRow row = _Model.TreePathToDataRow(path);
        if (DataTools.GetInt(row[_OrderColumnPos]) != cnt)
        {
          changed = true;
          row[_OrderColumnPos] = cnt;
        }
      }
      return changed;
    }

    /// <summary>
    /// Инициализация числового поля значениями 1,2,3,... в соответствии с требуемым порядком.
    /// Предполагается, что <paramref name="desiredOrder"/> содержит все строки просмотра.
    /// Результирующий порядок сортировки может отличаться, так как он учитывает порядок обхода узлов дерева,
    /// а <paramref name="desiredOrder"/> не обязан его учитывать.
    /// </summary>
    /// <returns>True, если значение было изменено хотя бы для одной строки</returns>
    public bool Reorder(DataRow[] desiredOrder)
    {
      // 1. Получаем список строк модели в текущем порядке
      List<DataRow> rows1 = new List<DataRow>();
      foreach (TreePath rowPath in new TreePathEnumerable(Model))
        rows1.Add(Model.TreePathToDataRow(rowPath));

      // 2. Проверяем, вдруг списки совпадают
      if (desiredOrder.Length == rows1.Count)
      {
        bool areSame = true;
        for (int i = 0; i < desiredOrder.Length; i++)
        {
          if (!Object.ReferenceEquals(desiredOrder[i], rows1[i]))
          {
            areSame = false;
            break;
          }
          if (DataTools.GetInt(rows1[i][_OrderColumnPos]) != (i + 1))
          {
            // строки требуется перенумеровать
            areSame = false;
            break;
          }
        }
        if (areSame)
          return false;
      }

      // 3. Выполняет нумерацию без учета структуры дерева
      for (int i = 0; i < desiredOrder.Length; i++)
        desiredOrder[i][_OrderColumnPos] = i + 1;

      // 4. Перенумеровываем еще раз для учета иерархии
      Reorder();

      return true;
    }

    #endregion
  }
}
