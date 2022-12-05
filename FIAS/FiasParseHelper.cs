// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// Парсинг адреса с разбиением на колонки
  /// </summary>
  internal class FiasParseHelper
  {
    #region Фиксированные данные

    public FiasHandler Handler;

    public FiasParseSettings ParseSettings;

    public string[] CellStrings;

    #endregion

    #region Основной метод

    public FiasAddress Parse()
    {
      if (CellStrings.Length != ParseSettings.CellLevels.Length)
        throw new BugException();

      if (CellStrings.Length == 0)
        throw new ArgumentNullException();

      for (int i = 0; i < CellStrings.Length; i++)
      {
        string s = CellStrings[i].Replace("--", "-");
        s = s.Replace("- ", "-");
        s = s.Replace(" -", "-");
        s = s.Replace("  ", " ");
        s = s.Replace(" .", ".");

        CellStrings[i] = s.Trim();

        if (i > 0)
        {
          // TODO: Проверка совместимости уровней
        }
      }

      //_Handler.FillAddress(_BaseAddress);
      _BestAddress = null;
      //_BestTail = String.Join(",", CellStrings); // надо бы пропускать пустые строки
      _BestTail = String.Empty;

      AddPart(0, CellStrings[0], ParseSettings.CellLevels[0], ParseSettings.BaseAddress.Clone());

      if (!String.IsNullOrEmpty(_BestTail))
        _BestAddress.AddMessage(ErrorMessageKind.Error, "Некуда добавить фрагмент текста \"" + _BestTail + "\", так как все уровни адреса заполнены");

      if (_BestAddress == null)
        return ParseSettings.BaseAddress; // ничего не нашли
      else
        return _BestAddress;
    }

    #endregion

    #region Лучший адрес

    private FiasAddress _BestAddress;

    private string _BestTail;

    /// <summary>
    /// Сравнение адреса-претендента с лучшим существующим адресом.
    /// Если претендент "лучше", чем старый адрес, возвращается true, и старый адрес заменяется на новый
    /// </summary>
    /// <param name="currentCellIndex"></param>
    /// <param name="s"></param>
    /// <param name="address">Адрес-претендент</param>
    /// <returns></returns>
    private bool CompareWithTheBest(int currentCellIndex, string s, FiasAddress address)
    {
      // 09.03.2021
      // Если есть "хвост", не пытаемся сохранить адрес

      if (!String.IsNullOrEmpty(s))
        return false;

      // Собираем хвост
      for (int i = currentCellIndex + 1; i < ParseSettings.CellLevels.Length; i++)
      {
        if (!String.IsNullOrEmpty(CellStrings[i]))
        {
          if (s.Length > 0)
            return false;
          //  s += ", ";
          //s += CellStrings[i];
        }
      }

      // 29.11.2022. Расширенный поиск
      Handler.FillAddress(address, ParseSettings.ExtSearch);
      if (DoCompareWithTheBest(s, address))
      {
        _BestAddress = address.Clone();
        _BestTail = s;
        return true;
      }

      return false;
    }

    private bool DoCompareWithTheBest(string s, FiasAddress address)
    {
      if (address.NameBottomLevel == FiasLevel.Unknown)
        return false; // пустышка

      if (_BestAddress == null)
        return true;

      // 09.09.2021. Проверяем наличие ошибок
      int cmpSeverity = ErrorMessageList.Compare(address.Messages.Severity, _BestAddress.Messages.Severity);
      if (cmpSeverity != 0)
        return cmpSeverity < 0;

      // Проверяем наличие хвоста
      if (s.Length > 0 != _BestTail.Length > 0)
        return s.Length == 0;

      FiasLevel testLevel = address.NameBottomLevel;

      int ex1 = GetRBExistance(address, testLevel); // претендент
      int ex2 = GetRBExistance(_BestAddress, testLevel); // текущий чемпион
      if (ex1 != ex2)
        return ex1 < ex2;

      return testLevel < _BestAddress.NameBottomLevel; // ???
    }

    private int GetRBExistance(FiasAddress address, FiasLevel testLevel)
    {
      if (address.GetGuid(testLevel) != Guid.Empty)
        return 1;

      switch (FiasTools.GetTableType(testLevel))
      {
        case FiasTableType.House:
          if (!Handler.Source.DBSettings.UseHouse)
            return 2; // нет справочника
          break;
        case FiasTableType.Room:
          if (!Handler.Source.DBSettings.UseRoom)
            return 2; // нет справочника
          break;
      }

      if (address.GetMessages(testLevel).Severity != ErrorMessageKind.Info)
        return 3;
      else
        return 2;
    }

    #endregion

    #region Рекурсивный метод

    private bool AddPart(int currentCellIndex, string s, FiasLevelSet levels, FiasAddress address)
    {
      if (!levels.IsEmpty)
        address.ClearStartingWith(levels.TopLevel);


      if (String.IsNullOrEmpty(s))
        return AddNextPart(currentCellIndex, address);

      FiasLevel lastLevel = address.NameBottomLevel; // последний заполненный уровень

      // Проверяем все доступные уровни, для которых доступно наследование
      bool res = false;
      if (!levels.IsEmpty)
      {
        foreach (FiasLevel level in levels)
        {
          // оставшиеся уровни
          address.ClearStartingWith(levels.TopLevel);
          FiasLevelSet levels2 = levels.GetBelow(level);

          if (FiasTools.IsInheritableLevel(lastLevel, level, true))
          {
            if (AddPart2(currentCellIndex, s, levels2, address, level))
              res = true;
          }
        }
      }

      if (res)
        return true;


      return CompareWithTheBest(currentCellIndex, s, address);
    }

    private bool AddNextPart(int currentCellIndex, FiasAddress address)
    {
      if (currentCellIndex < (ParseSettings.CellLevels.Length - 1))
        return AddPart(currentCellIndex + 1, CellStrings[currentCellIndex + 1], ParseSettings.CellLevels[currentCellIndex + 1], address); // рекурсивный вызов для следующего уровня
      else
        return CompareWithTheBest(currentCellIndex, string.Empty, address);
    }

    private bool AddPart2(int currentCellIndex, string s, FiasLevelSet levels, FiasAddress address, FiasLevel level)
    {
      string s2 = s;
      string sOthers = String.Empty;
      int pComma = s.IndexOf(',');
      if (pComma >= 0)
      {
        s2 = s.Substring(0, pComma);
        sOthers = s.Substring(pComma); // включая запятую
      }

      bool res = false;
      bool aoTypeFound = false;

      // Может ли тип адресного объекта быть до или после именной части?
      bool aoTypeBeforeName, aoTypeAfterName;
      FiasTools.GetAOTypePlace(level, out aoTypeBeforeName, out aoTypeAfterName);

      int[] pSpaces = FindSpacePositions(s2);
      // Сколько частей, разделенных пробелами, может быть в типе адресного объекта?
      int maxAOTypeParts = Math.Min(pSpaces.Length, Handler.AOTypes.GetMaxSpaceCount(level) + 1);

      if (aoTypeBeforeName)
      {
        int pDot = s2.IndexOf('.');
        if (pDot >= 0 && (pSpaces.Length == 0 || pDot < pSpaces[0]))
        {
          // Предполагаем, что используется сокращение с точкой плюс номер дома, например, "д. 1"

          string aoType = s2.Substring(0, pDot + 1); // включая точку
          string nm = s2.Substring(pDot + 1).Trim(); // тут могут быть пробелы
          string fullAOType;
          if (IsValidAOType(level, aoType, out fullAOType))
          {
            // Сокращение подходит для уровня
            aoTypeFound = true;
            RemoveNumChar(ref nm, level);
            if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
            if (AddHouseNumWithSpace(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
          }
        }
      }

      // Перебираем варианты, когда идет тип адресного объекта, а потом - наименование ("город Тюмень")
      if (aoTypeBeforeName)
      {
        for (int i = 0; i < maxAOTypeParts; i++)
        {
          // Предполагаем, что используется сокращение с пробелом, например, "дом 1"
          string aoType = s2.Substring(0, pSpaces[i]);
          string nm = s2.Substring(pSpaces[i] + 1); // тут могут быть вложенные пробелы
          string fullAOType;
          if (IsValidAOType(level, aoType, out fullAOType))
          {
            // Сокращение подходит для уровня
            aoTypeFound = true;
            RemoveNumChar(ref nm, level);
            if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
            if (AddHouseNumWithSpace(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
          }
        }
      }

      // Перебираем варианты, когда идет наименование, а потом - тип адресного объекта ("Тюменский район")
      if (aoTypeAfterName)
      {
        for (int i = 0; i < maxAOTypeParts; i++)
        {
          // Предполагаем, что используется сокращение с пробелом, например, "дом 1"
          string nm = s2.Substring(0, pSpaces[pSpaces.Length - i - 1]);
          string aoType = s2.Substring(pSpaces[pSpaces.Length - i - 1] + 1); // тут могут быть вложенные пробелы
          string fullAOType;
          if (IsValidAOType(level, aoType, out fullAOType))
          {
            // Сокращение подходит для уровня
            aoTypeFound = true;
            RemoveNumChar(ref nm, level);
            if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
              res = true;
          }
        }
      }

      if (aoTypeBeforeName && (!aoTypeAfterName) && pSpaces.Length > 0)
      {
        // 29.11.2022
        // Проверяем неправильный порядок, например, "Тюмень г"
        // Не используем maxAOTypeParts, так как предполагаем, что в конце идет сокращение, а не полный тип адресного объекта

        string nm = s2.Substring(0, pSpaces[pSpaces.Length - 1]);
        string aoType = s2.Substring(pSpaces[pSpaces.Length - 1] + 1);
        string fullAOType;
        if (IsValidAOType(level, aoType, out fullAOType))
        {
          // Сокращение подходит для уровня
          aoTypeFound = true;
          RemoveNumChar(ref nm, level);
          if (AddPartialSubsts(currentCellIndex, fullAOType, nm, sOthers, levels, address, level))
            res = true;
        }
      }

      if ((!aoTypeFound) ||
        level == FiasLevel.Region) // В ФИАСе с лета 2020 года заданы наименования регионов "Тюменская область"
      {
        // Предполагаем номер дома без сокращения, например, "1"
        RemoveNumChar(ref s2, level);
        if (AddPartialSubsts(currentCellIndex, String.Empty, s2, sOthers, levels, address, level))
          res = true;
        if (AddHouseNumWithSpace(currentCellIndex, String.Empty, s2, sOthers, levels, address, level))
          res = true;
        if (AddNumWithoutSep(currentCellIndex, s2, sOthers, levels, address, level))
          res = true;
      }

      return res;
    }

    private bool IsValidAOType(FiasLevel level, string aoType, out string fullAOType)
    {
      if (String.IsNullOrEmpty(aoType))
      {
        fullAOType = String.Empty;
        return false;
      }

      if (level == FiasLevel.Structure)
      {
        switch (aoType)
        {
          case "с":
          case "с.":
            // это может быть строение или сооружение
            fullAOType = String.Empty;
            return true;
        }
      }

      Int32 id;
      if (Handler.AOTypes.IsValidAOType(level, aoType, out fullAOType, out id))
        return true;

      // Ищем сокращение с точкой или без точки
      if (aoType[aoType.Length - 1] != '.')
      {
        if (Handler.AOTypes.IsValidAOType(level, aoType + ".", out fullAOType, out id))
          return true;
      }
      else
      {
        if (Handler.AOTypes.IsValidAOType(level, aoType.Substring(0, aoType.Length - 1), out fullAOType, out id))
          return true;
      }

      return false;
    }

    /// <summary>
    /// Удаляет ведущий символ "№"
    /// </summary>
    /// <param name="s"></param>
    /// <param name="level"></param>
    private static void RemoveNumChar(ref string s, FiasLevel level)
    {
      if (s.Length < 1)
        return; // 30.11.2022

      if (s[0] == '№' || s[0] == 'N')
      {
        switch (level)
        {
          case FiasLevel.House:
          case FiasLevel.Building:
          case FiasLevel.Structure:
          case FiasLevel.Flat:
          case FiasLevel.Room:
            break;
          default:
            return; // адресный объект может начинаться с номера
        }

        s = s.Substring(1).Trim(); // может быть пробел после знака номера
      }
    }

    /// <summary>
    /// Возвращает массив позиций пробелов в строке
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static int[] FindSpacePositions(string s)
    {
      if (String.IsNullOrEmpty(s))
        return DataTools.EmptyInts;

      List<int> lst = null;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] == ' ')
        {
          if (lst == null)
            lst = new List<int>();
          lst.Add(i);
        }
      }

      if (lst == null)
        return DataTools.EmptyInts;
      else
        return lst.ToArray();
    }

    /// <summary>
    /// Перебор номеров домов с учетом возможных слитных сокращений
    /// </summary>
    /// <param name="currentCellIndex"></param>
    /// <param name="fullAOType">Сокращение, которое точно подходит для заданного уровня</param>
    /// <param name="s">Строка с номером дома. Может содержать пробел</param>
    /// <param name="sOthers">Оставшаяся часть строки, начинающаяся с запятой</param>
    /// <param name="levels">Оставшиеся уровни, которые можно использовать при анализе строки</param>
    /// <param name="address"></param>
    /// <param name="level">Текущий уровень</param>
    /// <returns>true, если что-нибудь найдено</returns>
    private bool AddPartialSubsts(int currentCellIndex, string fullAOType, string s, string sOthers, FiasLevelSet levels, FiasAddress address, FiasLevel level)
    {
      if (s.Length == 0)
        return false;

      bool res = false;

      // Проверяем строку целиком
      if (FiasTools.IsValidName(s, level))
      {
        address.ClearStartingWith(level);
        address.SetName(level, s);
        address.SetAOType(level, fullAOType);
        address.ClearGuidsStartingWith(level);
        address.ClearRecIdsStartingWith(level);
        if (AddPart(currentCellIndex, sOthers.TrimStart(',', ' '), levels, address)) // рекурсивный вызов
          res = true;
      }

      // Проверяем строку целиком
      if (FiasTools.IsValidName(s, level))
      {
        address.ClearStartingWith(level);
        address.SetName(level, s);
        address.SetAOType(level, fullAOType);
        address.ClearGuidsStartingWith(level);
        address.ClearRecIdsStartingWith(level);
        if (AddPart(currentCellIndex, sOthers.TrimStart(',', ' '), levels, address)) // рекурсивный вызов
          res = true;
      }

      int pSpace = s.IndexOf(' ');
      if (pSpace >= 0)
      {
        sOthers = s.Substring(pSpace) + sOthers; // остаток, начиная с пробела
        s = s.Substring(0, pSpace); // часть строки, в которой можно искать 

        if (!Handler.AOTypes.GetAOTypeLevels(s).IsEmpty)
          return false; // текст является сокращением
      }

      if (String.IsNullOrEmpty(s))
        return false;

      // Ищем промежуточные разделители
      // Лучше в обратном порядке
      bool charSepFound = false;
      for (int p = s.Length - 2; p >= 1; p--)
      {
        if (s[p] == '-' || s[p] == '/' || s[p] == '\\')
        {
          charSepFound = true;
          string leftPart = s.Substring(0, p);
          string rightPart = s.Substring(p + 1);
          if (FiasTools.IsValidName(leftPart, level))
          {
            address.ClearStartingWith(level);
            address.SetName(level, leftPart);
            address.SetAOType(level, fullAOType);
            address.ClearGuidsStartingWith(level);
            address.ClearRecIdsStartingWith(level);
            if (AddPart(currentCellIndex,
              rightPart + sOthers, // здесь не надо отрезать ведущий разделитель от sOthers
              levels, address)) // рекурсивный вызов
              res = true;
          }
        }
      }

      // Ищем переходы Буква<-->Цифра
      if (!charSepFound)
      {
        switch (level)
        {
          case FiasLevel.House:
          case FiasLevel.Building:
          case FiasLevel.Flat:
            // Только для уровней, после которых может быть продолжение.
            // Например, дом "1а" - дом 1, литер А
            // Но только, если есть единственный такой переход
            int transPos = -1;
            int transCount = 0;
            for (int i = 1; i < s.Length; i++)
            {
              if ((Char.IsDigit(s[i - 1]) && Char.IsLetter(s[i])) ||
                (Char.IsLetter(s[i - 1]) && Char.IsDigit(s[i])))
              {
                transCount++;
                transPos = i;
              }
            }

            if (transCount == 1)
            {
              string leftPart = s.Substring(0, transPos);
              string rightPart = s.Substring(transPos);
              if (FiasTools.IsValidName(leftPart, level))
              {
                address.ClearStartingWith(level);
                address.SetName(level, leftPart);
                address.SetAOType(level, fullAOType);
                address.ClearGuidsStartingWith(level);
                address.ClearRecIdsStartingWith(level);
                if (AddPart(currentCellIndex,
                  rightPart + sOthers, // здесь не надо отрезать ведущий разделитель от sOthers
                  levels, address)) // рекурсивный вызов
                  res = true;
              }
            }
            break;
        }
      }

      return res;
    }


    /// <summary>
    /// Сбор номера дома из двух частей, например "1 а" может быть "1а"
    /// </summary>
    /// <param name="currentCellIndex"></param>
    /// <param name="fullAOType"></param>
    /// <param name="s"></param>
    /// <param name="sOthers"></param>
    /// <param name="levels"></param>
    /// <param name="address"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private bool AddHouseNumWithSpace(int currentCellIndex, string fullAOType, string s, string sOthers, FiasLevelSet levels, FiasAddress address, FiasLevel level)
    {
      if (level != FiasLevel.House)
        return false; // для других уровней не бывает

      if (s.Length == 0)
        return false;

      int pSpace = s.IndexOf(' ');
      if (pSpace < 0)
        return false;

      if (s.LastIndexOf(' ') != pSpace)
        return false; // больше одного пробела

      // До пробела должны быть цифры, а после - буквы

      string s1 = s.Substring(0, pSpace);
      string s2 = s.Substring(pSpace + 1);
      for (int i = 0; i < s1.Length; i++)
      {
        if (!Char.IsDigit(s1[i]))
          return false;
      }
      for (int i = 0; i < s2.Length; i++)
      {
        if (!Char.IsLetter(s2[i]))
          return false;
      }

      // Можно проверить
      string s3 = s1 + s2; // без пробела

      if (FiasTools.IsValidName(s3, level))
      {
        address.ClearStartingWith(level);
        address.SetName(level, s3);
        address.SetAOType(level, fullAOType);
        address.ClearGuidsStartingWith(level);
        address.ClearRecIdsStartingWith(level);
        if (AddPart(currentCellIndex, sOthers.TrimStart(',', ' '), levels, address)) // рекурсивный вызов
          return true;
      }

      return false;
    }

    /// <summary>
    /// Парсинг номеров домов, в которых нет разделителей, например, "дом1корпус2строение3".
    /// Находим переход "Буква-Цифра" (но не наоборот)
    /// </summary>
    /// <param name="currentCellIndex"></param>
    /// <param name="s"></param>
    /// <param name="sOthers"></param>
    /// <param name="levels"></param>
    /// <param name="address"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private bool AddNumWithoutSep(int currentCellIndex, string s, string sOthers, FiasLevelSet levels, FiasAddress address, FiasLevel level)
    {
      switch (level)
      {
        case FiasLevel.House:
        case FiasLevel.Building:
        case FiasLevel.Structure:
        case FiasLevel.Flat:
        case FiasLevel.Room:
          break;
        default:
          return false;
      }

      if (s.Length == 0)
        return false;

      // Проверяем, что есть только цифры и 
      if (!Char.IsLetter(s[0]))
        return false;

      int digitStart = -1;
      bool moreDigitGroups = false;
      int nextLetterStart = -1;
      for (int i = 1; i < s.Length; i++)
      {
        if (Char.IsDigit(s[i]))
        {
          if (digitStart < 0)
            digitStart = i;
          else if (!Char.IsDigit(s[i - 1]))
            moreDigitGroups = true; // Есть другие числовые группы
        }
        else if (Char.IsLetter(s[i]))
        {
          if (nextLetterStart < 0 && Char.IsDigit(s[i - 1]))
            nextLetterStart = i;
        }
        else
        {
          // Не буква и не цифра
          return false;
        }
      }

      if (digitStart < 0)
        return false; // текст состоит только из букв, например "дом"

      if (nextLetterStart >= 0)
      {
#if DEBUG
        if (nextLetterStart < 2)
          throw new BugException("NextLetterStart=" + nextLetterStart.ToString());
#endif
        if (moreDigitGroups)
        {
          // Если есть несколько цифровых групп, например, "дом1стр2", то "стр2" распознаем на следующем такте
          sOthers = s.Substring(nextLetterStart) + sOthers;
          s = s.Substring(0, nextLetterStart);
        }
        // а иначе - это часть текущего уровня, например, "дом1а"
      }


      string aoType = s.Substring(0, digitStart);
      string nm = s.Substring(digitStart);
      string fullAOType;
      if (IsValidAOType(level, aoType, out fullAOType) &&
        FiasTools.IsValidName(nm, level)) // это - фиктивное условие, тут все равно только цифры (и, может быть, буквы в конце)
      {
        address.ClearStartingWith(level);
        address.SetName(level, nm);
        address.SetAOType(level, fullAOType);
        address.ClearGuidsStartingWith(level);
        address.ClearRecIdsStartingWith(level);
        if (AddPart(currentCellIndex,
          sOthers,
          levels, address)) // рекурсивный вызов
          return true;
      }

      return false;
    }

    #endregion
  }
}
