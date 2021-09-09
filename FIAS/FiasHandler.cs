using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Diagnostics;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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

namespace AgeyevAV.FIAS
{
  /// <summary>
  /// Основной объект для работы с адресами, используемый в пользовательском коде.
  /// Может создаваться произвольное количество объектов.
  /// Этот класс не является потокобезопасным
  /// </summary>
  public sealed class FiasHandler
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="source">Источник данных. Должен быть задан</param>
    public FiasHandler(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Source = source;
      _AddrObExtractor = new FiasAddrObExtractor(source);
      if (source.DBSettings.UseHouse)
        _HouseExtractor = new FiasHouseExtractor(source);
      if (source.DBSettings.UseRoom)
        _RoomExtractor = new FiasRoomExtractor(source);
      _Today = DateTime.Today;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Источник данных для получения таблиц классификатора
    /// </summary>
    public IFiasSource Source { get { return _Source; } }
    private IFiasSource _Source;

    private FiasAddrObExtractor _AddrObExtractor;
    private FiasHouseExtractor _HouseExtractor;
    private FiasRoomExtractor _RoomExtractor;

    /// <summary>
    /// Текущая дата, используемая для проверки актуальности
    /// </summary>
    public DateTime Today { get { return _Today; } }
    private DateTime _Today;

    #endregion

    #region Заполнение объектов адреса

    /// <summary>
    /// Дозаполнение полей адреса.
    /// Также выполнется проверка ошибок и заново заполняется список FiasAddress.Messages.
    /// Если требуется заполнить множество адресов, используйте метод FillAddresses(), 
    /// т.к. он может быстрее загрузить нужные части классификатора из базы данных в кэш.
    /// </summary>
    /// <param name="address">Адрес</param>
    public void FillAddress(FiasAddress address)
    {
      if (address == null)
        throw new ArgumentNullException("address");

      FillAddresses(new FiasAddress[1] { address });
    }

    /// <summary>
    /// Дозаполнение полей нескольких адресов.
    /// Также выполнется проверка ошибок и заново заполняется список FiasAddress.Messages.
    /// </summary>
    /// <param name="addresses">Адреса</param>
    public void FillAddresses(FiasAddress[] addresses)
    {
      if (addresses == null)
        throw new ArgumentNullException("addresses");

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FillAddresses() started. addresses.Length=" + addresses.Length.ToString());

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "Построение списка GUIDов",
       "Загрузка страниц",
       addresses.Length==1 ? "Заполнение адреса" : "Заполнение адресов ("+addresses.Length.ToString()+")"
      });
      try
      {
        #region Собираем GUIDы

        ReplaceUnknownGuids(addresses);

        PageLoader loader = new PageLoader(_Source);
        spl.PercentMax = addresses.Length;
        spl.AllowCancel = true;
        for (int i = 0; i < addresses.Length; i++)
        {
          if (addresses[i] == null)
            throw new ArgumentNullException("addresses[" + i.ToString() + "]");

          loader.AddGuids(addresses[i]);
          spl.IncPercent();
        }

        spl.Complete();

        #endregion

        // Загружаем страницы
        loader.Load(spl);
        spl.Complete();

        // Заполняем адреса
        spl.PercentMax = addresses.Length;
        spl.AllowCancel = true;
        for (int i = 0; i < addresses.Length; i++)
        {
          DoFillAddress(addresses[i], loader);
          spl.IncPercent();
        }
        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FillAddresses() finished.");
    }

    private void ReplaceUnknownGuids(FiasAddress[] addresses)
    {
      SingleScopeList<Guid> UnknownGuids = null;
      for (int i = 0; i < addresses.Length; i++)
      {
        if (addresses[i].UnknownGuid != Guid.Empty)
        {
          if (UnknownGuids == null)
            UnknownGuids = new SingleScopeList<Guid>();
          UnknownGuids.Add(addresses[i].UnknownGuid);
        }
      }
      if (UnknownGuids == null)
        return;

      IDictionary<Guid, FiasGuidInfo> dictGuid = Source.GetGuidInfo(UnknownGuids.ToArray(), FiasTableType.Unknown);

      // 25.02.2021
      // Если GUID не найден, то это может быть идентификатор записи RecId.
      // Собираем еще одну коллекцию
      List<Guid> UnknownRecIds = null; // не обязательно использовать SingleScopeList, GUIDы уже уникальны
      foreach (KeyValuePair<Guid, FiasGuidInfo> pair in dictGuid)
      {
        if (pair.Value.Level == FiasLevel.Unknown)
        {
          if (UnknownRecIds == null)
            UnknownRecIds = new List<Guid>();
          UnknownRecIds.Add(pair.Key);
        }
      }
      IDictionary<Guid, FiasGuidInfo> dictRecId = null;
      if (UnknownRecIds != null)
        dictRecId = Source.GetRecIdInfo(UnknownRecIds.ToArray(), FiasTableType.Unknown);


      for (int i = 0; i < addresses.Length; i++)
      {
        if (addresses[i].UnknownGuid != Guid.Empty)
        {
          Guid g = addresses[i].UnknownGuid;
          if (dictGuid[g].Level == FiasLevel.Unknown)
          {
            #region Замена на RecId

            // 25.02.2021

            if (dictRecId[g].Level == FiasLevel.Unknown)
              addresses[i].AddMessage(ErrorMessageKind.Error, "Неизвестный GUID=" + g.ToString());
            else
            {
              FiasTableType tt = FiasTools.GetTableType(dictRecId[g].Level);
              switch (tt)
              {
                case FiasTableType.AddrOb:
                  addresses[i].AORecId = g;
                  break;
                case FiasTableType.House:
                  addresses[i].SetRecId(FiasLevel.House, g);
                  break;
                case FiasTableType.Room:
                  addresses[i].SetRecId(FiasLevel.Flat, g);
                  break;
                default:
                  throw new BugException("Неизвестный TableType=" + tt.ToString() + " для Level=" + dictRecId[g].Level + " при поиске для RecId=" + g.ToString());
              }
              addresses[i].UnknownGuid = Guid.Empty;
            }

            #endregion
          }
          else
          {
            #region Замена на AOGuid/HouseGuid/RoomGuid

            FiasTableType tt = FiasTools.GetTableType(dictGuid[g].Level);
            switch (tt)
            {
              case FiasTableType.AddrOb:
                addresses[i].AOGuid = g;
                break;
              case FiasTableType.House:
                addresses[i].SetGuid(FiasLevel.House, g);
                break;
              case FiasTableType.Room:
                addresses[i].SetGuid(FiasLevel.Flat, g);
                break;
              default:
                throw new BugException("Неизвестный TableType=" + tt.ToString() + " для Level=" + dictGuid[g].Level + " при поиске для Guid=" + g.ToString());
            }
            addresses[i].UnknownGuid = Guid.Empty;

            #endregion
          }
        }
      }
    }

    /*
     * Порядок обработки идентификаторов AOGuid, HouseGuid, RoomGuid, AORecId, HouseRecId и RoomRecId,
     * заданных в FiasAddress.
     * Под AOGuid и AORecId понимаются идентификаторы адресных объектов любого уровня (регион .. улица).
     * Идентификаторы XXXGuid являются "устойчивыми", а XXXRecId - идентификаторы записей (первичные ключи). Как
     * правило, используются "устойчивые" идентификаторы, а идентификаторы записей используются только при поиске адресов.
     * 
     * 1. Проверяестся наличие одного из заданных "устойчивых" идентификаторов AOGuid, HouseGuid, RoomGuid.
     * Если задан хотя бы один из них, то выполняется поиск только по актуальным записям.
     * Обрабатывается только один из идентификаторов, в указанном ниже порядке. Остальные идентификаторы,
     * игнорируются. 
     * - 1. RoomGuid
     * - 2. HouseGuid
     * - 3. AOGuid
     * 
     * 2. Проверяется наличие идентификатора записи RoomRecId.
     * Если он задан, находится запись в таблице "Room". Извлекаются поля "HouseGuid" и "EndDate" (если UseDates=true).
     * 
     * 3. Проверяется наличие идентификатора записи HouseRecId.
     * Если он задан, находится запись в таблице "House" по идентификатору. 
     * Иначе, если был задан RoomRecId, то поиск в "House" выполняется 
     * - по "HouseGuid" + "StartDate/EndDate", если UseDates=true
     * - по "HouseGuid", если UseDates=false (среди актуальных записей)
     * 
     * 4. Проверяется наличие идентификатора AORecId.
     * Если он задан, находится запись в таблице "AddrOb" по идентификатору. 
     * Иначе, если был задан HouseRecId, то поиск в "AddrOb" выполняется AOGUID(+StartDate/EndDate), аналлогично поиску домов.
     * 
     * 5. Выполняется рекурсивный поиск оставшихся уровней адресных объектов.
     * 
     * В п.3-5, при FiasDBSettings.UseDates=true выполняется поиск с учетом дат. Проверяется попадание некой тестовой даты в интервал
     * StartDate:EndDate при выборе подходящей записи в таблицах "House" и "AddrOb". 
     * Тестовая дата определяется из записи, для которой был задан идентификатор RoomRecId, HouseRecId или AORecId:
     * - Берется интервал StartDate:EndDate из записи.
     * - Проверяется, что текущая дата DateTime.Today попадает в этот интервал
     * - Если текущая дата попадает в интервал, то используется DateTime.Today 
     * - Иначе используется EndDate.
     */

    private void DoFillAddress(FiasAddress address, PageLoader loader)
    {
      address.ClearAuxInfo();
      address.ClearMessages();
      if (address.IsEmpty)
        return;

      #region По Guid'ам и по идентификаторам записей

      if (!DoFillAddressByGuid(address, loader))
      {
        Guid roomRecId = address.GetRecId(FiasLevel.Flat);
        Guid houseRecId = address.GetRecId(FiasLevel.House);
        Guid aoRecId = address.AORecId;
        if (roomRecId != Guid.Empty)
          DoFillAddressByRoomRecId(address, loader);
        else if (houseRecId != Guid.Empty)
          DoFillAddressByHouseRecId(address, loader);
        else if (aoRecId != Guid.Empty)
          DoFillAddressByAORecId(address, loader);
      }

      #endregion

      DoFillAddressByNames(address, loader);

      #region Проверка именной части

      // Делаем после DoFillAddressXXX(), так как возможно передвижение между уровнями
      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        string name = address.GetName(FiasTools.AllLevels[i]);
        if (!String.IsNullOrEmpty(name))
        {
          string errorText;
          if (!FiasTools.IsValidName(name, FiasTools.AllLevels[i], out errorText))
          {
            string sPrefix;
            if (FiasTools.GetTableType(FiasTools.AllLevels[i]) == FiasTableType.AddrOb)
              sPrefix = "Неправильное наименование. ";
            else
              sPrefix = "Неправильный номер. ";
            address.AddMessage(ErrorMessageKind.Error, sPrefix + errorText, FiasTools.AllLevels[i]); // 03.03.2021
          }
        }
      }

      #endregion

      #region Проверяем сокращения после того, как заполнили адрес

      for (int i = 0; i < FiasTools.AllLevels.Length; i++)
      {
        FiasLevel level = FiasTools.AllLevels[i];
        string n = address.GetName(level);
        string a = address.GetAOType(level);
        if (n.Length > 0)
        {
          if (a.Length == 0)
            address.AddMessage(ErrorMessageKind.Error, "Не указан тип адресного объекта", level);
          else if (!AOTypes.IsValidAOType(level, a))
            address.AddMessage(ErrorMessageKind.Error, "Неизвестный тип адресного объекта \"" + a + "\"", level);
        }
        else
        {
          if (a.Length > 0)
            address.AddMessage(ErrorMessageKind.Error, "Указан тип адресного объекта (" + a + ") без наименования", level);
        }
      }

      #endregion

      #region Проверяем почтовый индекс

      if (address.PostalCode.Length > 0)
      {
        string errorText;
        if (!FiasTools.PostalCodeMaskProvider.Test(address.PostalCode, out errorText))
          address.AddMessage(ErrorMessageKind.Error, "Неправильный почтовый индекс \"" + address.PostalCode + "\". " + errorText);
        else if (address.PostalCode[0] == '0')
          address.AddMessage(ErrorMessageKind.Error, "Неправильный почтовый индекс \"" + address.PostalCode + "\". Почтовый индекс не может начинаться с 0");
        if (address.NameBottomLevel == FiasLevel.Unknown)
          address.AddMessage(ErrorMessageKind.Error, "Задан почтовый индекс, а адрес пустой");
      }
      else if (!address.AutoPostalCode)
      {
        if (address.NameBottomLevel == FiasLevel.Unknown)
          address.AddMessage(ErrorMessageKind.Error, "Включен режим задания почтового индекса вручную при пустом адресе"); // 23.10.2020
      }


      #endregion
    }

    #region Заполнение по "устойчивым" GUID'ам

    /// <summary>
    /// Заполнение адреса, если есть хотя бы один заданный GUID
    /// </summary>
    /// <param name="address"></param>
    /// <param name="loader"></param>
    /// <returns>true, если был найден хотя бы один GUID</returns>
    private bool DoFillAddressByGuid(FiasAddress address, PageLoader loader)
    {
      bool res = false;

      address.ClearMessages();

      if (DoFillAddressByRoomGuid(address, loader, !res))
        res = true;
      if (DoFillAddressByHouseGuid(address, loader, !res))
        res = true;
      if (DoFillAddressByAOGuid(address, loader, !res))
        res = true;

      if (res)
        address.Actuality = FiasActuality.Actual;

      return res;
    }

    private bool DoFillAddressByRoomGuid(FiasAddress address, PageLoader loader, bool writeRecInfo)
    {
      bool res = false;

      if (_Source.DBSettings.UseRoom)
      {
        Guid gRoom = address.GetGuid(FiasLevel.Flat);
        if (gRoom != Guid.Empty)
        {
          res = true;

          Guid parentG = loader.RoomGuidInfoDict[gRoom].ParentGuid;
          if (parentG == FiasTools.GuidNotFound)
            address.AddMessage(ErrorMessageKind.Error, "В справочнике помещений не найдена запись для ROOMGUID=" + gRoom.ToString(), FiasLevel.Flat);
          else
          {
            FiasCachedPageRoom page = loader.RoomPages[parentG];
            _RoomExtractor.Row = page.FindRowByGuid(gRoom);

            if (_RoomExtractor.Row == null)
              throw new BugException("Не нашли строку для ROOMGUID=" + gRoom.ToString() + " на странице " + page.ToString());

            DoFillAddressRoomPart(address, writeRecInfo);

            address.SetGuid(FiasLevel.House, parentG);
          }
        }
      }

      return res;
    }

    private void DoFillAddressRoomPart(FiasAddress address, bool writeRecInfo)
    {
      if (_RoomExtractor.FlatNumber.Length > 0)
      {
        address.SetName(FiasLevel.Flat, _RoomExtractor.FlatNumber);
        address.SetAOType(FiasLevel.Flat, FiasEnumNames.ToString(_RoomExtractor.FlatType));
      }
      else
      {
        address.SetName(FiasLevel.Flat, String.Empty);
        address.SetAOType(FiasLevel.Flat, String.Empty);
      }
      if (_RoomExtractor.RoomNumber.Length > 0)
      {
        address.SetName(FiasLevel.Room, _RoomExtractor.RoomNumber);
        address.SetAOType(FiasLevel.Room, FiasEnumNames.ToString(_RoomExtractor.RoomType));
      }
      else
      {
        address.SetName(FiasLevel.Room, String.Empty);
        address.SetAOType(FiasLevel.Room, String.Empty);
      }
      address.SetGuid(FiasLevel.Flat, _RoomExtractor.RoomGuid);
      address.SetRecId(FiasLevel.Flat, _RoomExtractor.RecId);
      address.UnknownGuid = Guid.Empty; // 02.11.2020

      if (writeRecInfo || address.FiasPostalCode.Length == 0) // Если индекс был найден для улицы то не надо заменять на обобщенный индекс для города
        address.FiasPostalCode = _RoomExtractor.PostalCode;

      if (writeRecInfo)
      {
        address.StartDate = _RoomExtractor.StartDate;
        address.EndDate = _RoomExtractor.EndDate;
        address.Live = _RoomExtractor.Live;
      }
    }

    private bool DoFillAddressByHouseGuid(FiasAddress address, PageLoader loader, bool writeRecInfo)
    {
      bool res = false;

      if (_Source.DBSettings.UseHouse)
      {
        Guid gHouse = address.GetGuid(FiasLevel.House);
        if (gHouse != Guid.Empty)
        {
          res = true;

          Guid parentG = loader.HouseGuidInfoDict[gHouse].ParentGuid;
          if (parentG == FiasTools.GuidNotFound)
            address.AddMessage(ErrorMessageKind.Error, "В справочнике зданий не найдена запись для HOUSEGUID=" + gHouse.ToString(), FiasLevel.House);
          else
          {
            FiasLevel parentLevel;
            FiasGuidInfo info;
            if (loader.HouseGuidInfoDict.TryGetValue(parentG, out info)) // оптимизация 22.10.2020
              parentLevel = info.Level;
            else
              parentLevel = GetAOGuidLevel(parentG);
            if (!FiasTools.IsInheritableLevel(parentLevel, FiasLevel.House, false))
              address.AddHouseMessage(ErrorMessageKind.Error, "Дом не может быть задан для адресного объекта с уровнем [" + FiasEnumNames.ToString(parentLevel, true) + "]");

            FiasCachedPageHouse page = loader.HousePages[parentG];
            _HouseExtractor.Row = page.FindRowByGuid(gHouse);

            if (_HouseExtractor.Row == null)
              throw new BugException("Не нашли строку для HOUSEGUID=" + gHouse.ToString() + " на странице " + page.ToString());

            DoFillAddressHousePart(address, writeRecInfo);

            address.AOGuid = parentG;
          }
        }
      }

      return res;
    }

    private void DoFillAddressHousePart(FiasAddress address, bool writeRecInfo)
    {
      if (_HouseExtractor.HouseNum.Length > 0)
      {
        address.SetName(FiasLevel.House, _HouseExtractor.HouseNum);
        address.SetAOType(FiasLevel.House, FiasEnumNames.ToString(_HouseExtractor.EstStatus));
      }
      else
      {
        address.SetName(FiasLevel.House, String.Empty);
        address.SetAOType(FiasLevel.House, String.Empty);
      }

      if (_HouseExtractor.BuildNum.Length > 0)
      {
        address.SetName(FiasLevel.Building, _HouseExtractor.BuildNum);
        address.SetAOType(FiasLevel.Building, "Корпус");
      }
      else
      {
        address.SetName(FiasLevel.Building, String.Empty);
        address.SetAOType(FiasLevel.Building, String.Empty);
      }

      if (_HouseExtractor.StrucNum.Length > 0)
      {
        address.SetName(FiasLevel.Structure, _HouseExtractor.StrucNum);
        address.SetAOType(FiasLevel.Structure, FiasEnumNames.ToString(_HouseExtractor.StrStatus));
      }
      else
      {
        address.SetName(FiasLevel.Structure, String.Empty);
        address.SetAOType(FiasLevel.Structure, String.Empty);
      }

      address.SetGuid(FiasLevel.House, _HouseExtractor.HouseGuid);
      address.SetRecId(FiasLevel.House, _HouseExtractor.RecId);
      address.UnknownGuid = Guid.Empty; // 02.11.2020

      if (writeRecInfo || address.FiasPostalCode.Length == 0) // Если индекс был найден для улицы то не надо заменять на обобщенный индекс для города
        address.FiasPostalCode = _HouseExtractor.PostalCode;

      if (writeRecInfo || address.OKATO.Length == 0)
        address.OKATO = _HouseExtractor.OKATO;
      if (writeRecInfo || address.OKTMO.Length == 0)
        address.OKTMO = _HouseExtractor.OKTMO;
      if (writeRecInfo || address.IFNSFL.Length == 0)
        address.IFNSFL = _HouseExtractor.IFNSFL;
      if (writeRecInfo || address.IFNSUL.Length == 0)
        address.IFNSUL = _HouseExtractor.IFNSUL;

      if (writeRecInfo)
      {
        address.StartDate = _HouseExtractor.StartDate;
        address.EndDate = _HouseExtractor.EndDate;
      }
    }

    private bool DoFillAddressByAOGuid(FiasAddress address, PageLoader loader, bool writeRecInfo)
    {
      bool res = false;
      bool first = true;

      Guid gAO = address.AOGuid;
      while (gAO != Guid.Empty)
      {
        res = true;

        Guid parentG = loader.AOGuidInfoDict[gAO].ParentGuid;
        if (parentG == FiasTools.GuidNotFound)
        {
          address.AddMessage(ErrorMessageKind.Error, "В справочнике адресных объектов не найдена запись для AOGUID=" + gAO.ToString());
          break;
        }

        _AddrObExtractor.Row = null;
        foreach (KeyValuePair<FiasLevel, FiasCachedPageAddrOb> pair in loader.AOPages[parentG])
        {
          _AddrObExtractor.Row = pair.Value.FindRowByGuid(gAO);
          if (_AddrObExtractor.Row != null)
          {
            FiasLevel parentLevel;
            FiasGuidInfo info;
            if (parentG == Guid.Empty)
              parentLevel = FiasLevel.Unknown;
            else if (loader.AOGuidInfoDict.TryGetValue(parentG, out info)) // оптимизация 22.10.2020
              parentLevel = info.Level;
            else
              parentLevel = GetAOGuidLevel(parentG);
            if (!FiasTools.IsInheritableLevel(parentLevel, pair.Key, false))
            {
              if (parentLevel == FiasLevel.Unknown)
                address.AddMessage(ErrorMessageKind.Error, "Адресный объект с уровнем [" + FiasEnumNames.ToString(pair.Key, true) + "] не может быть объектом верхнего уровня", pair.Key);
              else
                address.AddMessage(ErrorMessageKind.Error, "Адресный объект с уровнем [" + FiasEnumNames.ToString(pair.Key, true) + "] не может иметь родителя с уровнем [" + FiasEnumNames.ToString(parentLevel, true) + "]", pair.Key);
            }
            break;
          }
        }

        if (_AddrObExtractor.Row == null)
          throw new BugException("Не нашли строку для AOGUID=" + gAO.ToString() + " на страницах для родительского объекта " + parentG.ToString());


        if (first)
        {
          // Очищаем все наименования для промежуточных уровней, по которым не было идентификатов
          // Например, в адресе было задано "Тюменская область", "Тюменский район" и 
          // GUID {9ae64229-9f7b-4149-b27a-d1f6ec74b5ce} для города Тюмени
          // Вызовы заполнят уровни "Регион" и "Город", а уровень "Район" останется без изменений

          first = false;
          //address.ClearAOLevels();
        }

        DoFillAddressAOPart(address, writeRecInfo);
        writeRecInfo = false;

        gAO = parentG;
      }

      return res;
    }

    private void DoFillAddressAOPart(FiasAddress address, bool writeRecInfo)
    {

      address.SetName(_AddrObExtractor.Level, _AddrObExtractor.Name);
      address.SetAOType(_AddrObExtractor.Level, AOTypes.GetAOType(_AddrObExtractor.AOTypeId, FiasAOTypeMode.Full));
      address.SetGuid(_AddrObExtractor.Level, _AddrObExtractor.AOGuid);
      address.SetRecId(_AddrObExtractor.Level, _AddrObExtractor.RecId);
      address.AOGuid = Guid.Empty; // 02.11.2020
      address.AORecId = Guid.Empty; // 02.11.2020
      address.UnknownGuid = Guid.Empty; // 02.11.2020

      if (writeRecInfo || address.FiasPostalCode.Length == 0) // Если индекс был найден для улицы то не надо заменять на обобщенный индекс для города
        address.FiasPostalCode = _AddrObExtractor.PostalCode;
      if (writeRecInfo || address.OKATO.Length == 0)
        address.OKATO = _AddrObExtractor.OKATO;
      if (writeRecInfo || address.OKTMO.Length == 0)
        address.OKTMO = _AddrObExtractor.OKTMO;
      if (writeRecInfo || address.IFNSFL.Length == 0)
        address.IFNSFL = _AddrObExtractor.IFNSFL;
      if (writeRecInfo || address.IFNSUL.Length == 0)
        address.IFNSUL = _AddrObExtractor.IFNSUL;

      if (writeRecInfo)
      {
        address.Live = _AddrObExtractor.Live;
        address.StartDate = _AddrObExtractor.StartDate;
        address.EndDate = _AddrObExtractor.EndDate;
      }

      if (_AddrObExtractor.Level == FiasLevel.Region) // независимо от аргумента writeRecInfo
        address.RegionCode = _AddrObExtractor.RegionCode; // 09.08.2020
    }

    #endregion

    #region Заполнение по "неустойчивым" идентификаторам записей

    private void DoFillAddressByAORecId(FiasAddress address, PageLoader loader)
    {
      Guid recId = address.AORecId; // может быть задан отдельно или на каком-либо уровне

      Guid parentG = loader.AORecIdInfoDict[recId].ParentGuid;
      if (parentG == FiasTools.GuidNotFound)
      {
        address.AddMessage(ErrorMessageKind.Error, "В справочнике адресных объектов не найдена запись для AOID=" + recId.ToString());
        return;
      }
      _AddrObExtractor.Row = null;
      foreach (KeyValuePair<FiasLevel, FiasCachedPageAddrOb> pair in loader.AOPages[parentG])
      {
        _AddrObExtractor.Row = pair.Value.FindRowByRecId(recId);
        if (_AddrObExtractor.Row != null)
        {
          FiasLevel parentLevel = GetAOGuidLevel(parentG);
          if (!FiasTools.IsInheritableLevel(parentLevel, pair.Key, false))
          {
            if (parentLevel == FiasLevel.Unknown)
              address.AddMessage(ErrorMessageKind.Error, "Адресный объект с уровнем [" + FiasEnumNames.ToString(pair.Key, true) + "] не может быть объектом верхнего уровня", pair.Key);
            else
              address.AddMessage(ErrorMessageKind.Error, "Адресный объект с уровнем [" + FiasEnumNames.ToString(pair.Key, true) + "] не может иметь родителя с уровнем [" + FiasEnumNames.ToString(parentLevel, true) + "]", pair.Key);
          }
          break;
        }
      }

      if (_AddrObExtractor.Row == null)
        throw new BugException("Не нашли строку для AOID=" + recId.ToString() + " на страницах для родительского объекта " + parentG.ToString());

      // Надо стереть все уровни, начиная с региона и кончая текущим.
      // Нижележащие уровни стирать нельзя. Там могут быть текстовые наименования, которые надо будет найти

      //address.ClearAOLevels();
      int pLastLevel = FiasTools.AOLevelIndexer.IndexOf(_AddrObExtractor.Level);
      if (pLastLevel < 0)
        throw new BugException("Найдена строка с неправильным LEVEL=" + _AddrObExtractor.Level.ToString());
      for (int i = 0; i <= pLastLevel; i++)
        address.ClearLevel(FiasTools.AOLevels[i]);

      DoFillAddressAOPart(address, true);
      FiasActuality actuality = _AddrObExtractor.Actual ? FiasActuality.Actual : FiasActuality.Historical; // запоминаем до обработки остальных частей
      if (actuality == FiasActuality.Actual && _Source.DBSettings.UseDates)
      {
        if (!DataTools.DateInRange(_Today, _AddrObExtractor.StartDate, _AddrObExtractor.EndDate))
          actuality = FiasActuality.Historical;
      }

      if (parentG != Guid.Empty)
      {
        address.AOGuid = parentG;
        DoFillAddressByAOGuid(address, loader, false);
        address.AOGuid = Guid.Empty;
      }
      address.Actuality = actuality;

      //if (_Source.DBSettings.UseDates)
      //{
      //  testDate = DateTime.Today;
      //  if (!DataTools.DateInRange(testDate.Value, _AddrObExtractor.StartDate, _AddrObExtractor.EndDate))
      //    testDate = _AddrObExtractor.EndDate;
      //}

      //DoFillAddressByParentAOGuid(address, loader, parentG, testDate);
    }

    private void DoFillAddressByHouseRecId(FiasAddress address, PageLoader loader)
    {
      Guid recId = address.GetRecId(FiasLevel.House);
      if (recId == Guid.Empty)
        throw new BugException("HOUSEID is empty");

      Guid parentG = loader.HouseRecIdInfoDict[recId].ParentGuid;
      if (parentG == FiasTools.GuidNotFound)
      {
        address.AddMessage(ErrorMessageKind.Error, "В справочнике домов не найдена запись для HOUSEID=" + recId.ToString());
        return;
      }

      FiasLevel parentLevel = GetAOGuidLevel(parentG);
      if (!FiasTools.IsInheritableLevel(parentLevel, FiasLevel.House, false))
        address.AddHouseMessage(ErrorMessageKind.Error, "Дом не может быть задан для адресного объекта с уровнем [" + FiasEnumNames.ToString(parentLevel, true) + "]");

      try
      {
        _HouseExtractor.Row = loader.HousePages[parentG].FindRowByRecId(recId);
      }
      catch (Exception e)
      {
        e.Data["AOGUID"] = parentG;
      }
      if (_HouseExtractor.Row == null)
        throw new BugException("Не нашли строку для HOUSEID=" + recId.ToString() + " на странице для родительского объекта " + parentG.ToString());

      address.AOGuid = Guid.Empty; // очищаем "неопределенный" GUID
      address.AORecId = Guid.Empty;

      DoFillAddressHousePart(address, true);
      FiasActuality actuality = FiasActuality.Actual; // запоминаем до обработки остальных частей
      if (_Source.DBSettings.UseDates)
      {
        if (!DataTools.DateInRange(_Today, _HouseExtractor.StartDate, _HouseExtractor.EndDate))
          actuality = FiasActuality.Historical;
      }

      if (parentG != Guid.Empty)
      {
        address.AOGuid = parentG;
        DoFillAddressByAOGuid(address, loader, false);
      }
      address.Actuality = actuality;

      //if (_Source.DBSettings.UseDates)
      //{
      //  testDate = DateTime.Today;
      //  if (!DataTools.DateInRange(testDate.Value, _AddrObExtractor.StartDate, _AddrObExtractor.EndDate))
      //    testDate = _AddrObExtractor.EndDate;
      //}

      //DoFillAddressByParentAOGuid(address, loader, parentG, testDate);
    }

    private void DoFillAddressByRoomRecId(FiasAddress address, PageLoader loader)
    {
      Guid recId = address.GetRecId(FiasLevel.Flat);
      if (recId == Guid.Empty)
        throw new BugException("ROOMID is empty");

      Guid parentG = loader.RoomRecIdInfoDict[recId].ParentGuid;
      if (parentG == FiasTools.GuidNotFound)
      {
        address.AddMessage(ErrorMessageKind.Error, "В справочнике помещений не найдена запись для ROOMID=" + recId.ToString());
        return;
      }

      try
      {
        _RoomExtractor.Row = loader.RoomPages[parentG].FindRowByRecId(recId);
      }
      catch (Exception e)
      {
        e.Data["HOUSEGUID"] = parentG;
      }
      if (_RoomExtractor.Row == null)
        throw new BugException("Не нашли строку для ROOMID=" + recId.ToString() + " на странице для родительского объекта " + parentG.ToString());

      address.AOGuid = Guid.Empty; // очищаем "неопределенный" GUID
      address.AORecId = Guid.Empty;
      address.SetGuid(FiasLevel.House, Guid.Empty);
      address.SetRecId(FiasLevel.House, Guid.Empty);

      DoFillAddressRoomPart(address, true);
      FiasActuality actuality = FiasActuality.Actual; // запоминаем до обработки остальных частей
      if (_Source.DBSettings.UseDates)
      {
        if (!DataTools.DateInRange(_Today, _RoomExtractor.StartDate, _RoomExtractor.EndDate))
          actuality = FiasActuality.Historical;
      }

      if (parentG != Guid.Empty)
      {
        address.SetGuid(FiasLevel.House, parentG);
        DoFillAddressByHouseGuid(address, loader, false);
        DoFillAddressByAOGuid(address, loader, false);
      }
      address.Actuality = actuality;

      //if (_Source.DBSettings.UseDates)
      //{
      //  testDate = DateTime.Today;
      //  if (!DataTools.DateInRange(testDate.Value, _AddrObExtractor.StartDate, _AddrObExtractor.EndDate))
      //    testDate = _AddrObExtractor.EndDate;
      //}

      //DoFillAddressByParentAOGuid(address, loader, parentG, testDate);
    }

#if XXX
    private void DoFillAddressByParentAOGuid(FiasAddress address, PageLoader loader, Guid gAO, DateTime? testDate)
    {
      while (gAO != Guid.Empty)
      {
        Guid parentG = loader.AOParentGuidDict[gAO];
        if (parentG == FiasTools.GuidNotFound)
        {
          address.AddMessage(ErrorMessageKind.Error, "В справочнике адресных объектов не найдена запись для AOGUID=" + gAO.ToString());
          break;
        }
        _AddrObExtractor.Row = null;
        foreach (KeyValuePair<FiasLevel, FiasCachedPageAddrOb> pair in loader.AOPages[parentG])
        {
          _AddrObExtractor.Row = pair.Value.FindRowByGuid(gAO);
          if (_AddrObExtractor.Row != null)
            break;
        }

        if (_AddrObExtractor.Row == null)
          throw new BugException("Не нашли строку для AOGUID=" + gAO.ToString() + " на страницах для родительского объекта " + parentG.ToString());

        address.AOGuid = Guid.Empty; // очищаем "неопределенный" GUID
        address.AORecId = Guid.Empty;

        DoFillAddressAOPart(address);

        gAO = parentG;
      }
    }
#endif
    #endregion

    #region По наименованиям и сокращениям

    private void DoFillAddressByNames(FiasAddress address, PageLoader loader)
    {
      bool HasErrors = false;
      FiasLevel BottomLevel = address.GuidBottomLevel;

      int i = FiasTools.AllLevelIndexer.IndexOf(BottomLevel) + 1;
      while (i < FiasTools.AOLevels.Length)
      {
        FiasLevel thisLevel = FiasTools.AOLevels[i];

        string name = address.GetName(thisLevel);
        string aoType = address.GetAOType(thisLevel);
        if (name.Length == 0)
        {
          address.ClearLevel(thisLevel);
          i++;
          continue;
        }

        FiasSearchRowResult searchRes = FiasSearchRowResult.NotFound;
        _AddrObExtractor.Row = null;

        for (int j = i; j < FiasTools.AOLevels.Length; j++)
        {
          if (j > i && address.GetName(FiasTools.AOLevels[j]).Length > 0)
            break; // следующий заполненный уровень

          Guid pageAOGuid = Guid.Empty;
          if (BottomLevel != FiasLevel.Unknown)
            pageAOGuid = address.GetGuid(BottomLevel);

          FiasCachedPageAddrOb page = loader.GetAOPage(pageAOGuid, FiasTools.AOLevels[j]);
          Int32 aoTypeId = AOTypes.FindAOTypeId(FiasTools.AOLevels[j], aoType);
          searchRes = page.FindRow(name, aoTypeId);
          if (searchRes.Count == FiasSearchRowCount.Ok)
          {
            // очищаем уровни до "передвижения"
            for (int k = i; k < j; k++)
              address.ClearLevel(FiasTools.AOLevels[k]);

            i = j;
            break;
          }
        }

        //FiasLevel testLevel = BottomLevel;
        FiasLevel testLevel = address.GetPrevLevel(thisLevel); // 02.10.2020
        if (!FiasTools.IsInheritableLevel(testLevel, thisLevel, false))
        {
          if (testLevel == FiasLevel.Unknown)
            address.AddMessage(ErrorMessageKind.Error, "Адресный объект с уровнем [" + FiasEnumNames.ToString(thisLevel, true) + "] не может быть объектом верхнего уровня", thisLevel);
          else
            address.AddMessage(ErrorMessageKind.Error, "Адресный объект с уровнем [" + FiasEnumNames.ToString(thisLevel, true) + "] не может иметь родителя с уровнем [" + FiasEnumNames.ToString(testLevel, true) + "]", thisLevel);
          HasErrors = true;
        }
        else
        {
          switch (searchRes.Count)
          {
            case FiasSearchRowCount.Ok:
              _AddrObExtractor.Row = searchRes.Row;
              DoFillAddressAOPart(address, true);
              BottomLevel = _AddrObExtractor.Level;
              break;
            case FiasSearchRowCount.NotFound:
              address.AddMessage(ErrorMessageKind.Warning, "Не найден адресный объект для уровня [" + FiasEnumNames.ToString(thisLevel, false) + "]: \"" + name + " " + aoType + "\"", FiasTools.AOLevels[i]);
              HasErrors = true;
              break;
            case FiasSearchRowCount.Multi:
              address.AddMessage(ErrorMessageKind.Warning, "Найдено больше одного адресного объекта для уровня [" + FiasEnumNames.ToString(thisLevel, false) + "]: \"" + name + " " + aoType + "\"", FiasTools.AOLevels[i]);
              HasErrors = true;
              break;
          }
        }
        i++;
      }

      string houseNum = address.GetName(FiasLevel.House);
      string buildNum = address.GetName(FiasLevel.Building);
      string strNum = address.GetName(FiasLevel.Structure);

      //string errorText;
      //if (!NumValidator.IsValidNum(houseNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "Неправильный номер дома \"" + houseNum + "\". " + errorText, FiasLevel.House);
      //  HasErrors = true;
      //}
      //if (!NumValidator.IsValidNum(buildNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "Неправильный номер корпуса \"" + buildNum + "\". " + errorText, FiasLevel.Building);
      //  HasErrors = true;
      //}
      //if (!NumValidator.IsValidNum(strNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "Неправильный номер строения \"" + strNum + "\". " + errorText, FiasLevel.Structure);
      //  HasErrors = true;
      //}

      bool HasHouse = (houseNum.Length + buildNum.Length + strNum.Length) > 0;
      if (HasHouse)
      {
        if (address.GetName(FiasLevel.Village).Length +
          address.GetName(FiasLevel.PlanningStructure).Length /* 02.10.2020 */ +
          address.GetName(FiasLevel.Street).Length == 0)
        {
          address.AddHouseMessage(ErrorMessageKind.Error, "Задано здание, но не задан ни населенный пункт, ни улица");
          HasErrors = true; // 26.10.2020
        }

        if (!HasErrors)
        {
          if (_Source.DBSettings.UseHouse)
          {
            FiasCachedPageHouse page = loader.GetHousePage(address.AOGuid);

            string houseAOType = GetCorrectedAOType(address, FiasLevel.House);
            FiasEstateStatus estStatus = FiasEnumNames.ParseEstateStatus(houseAOType);

            GetCorrectedAOType(address, FiasLevel.Building);

            string strAOType = GetCorrectedAOType(address, FiasLevel.Structure);
            FiasStructureStatus strStatus = FiasEnumNames.ParseStructureStatus(strAOType);

            _HouseExtractor.Row = page.FindRow(houseNum, estStatus, buildNum, strNum, strStatus);
            if (_HouseExtractor.Row != null)
              DoFillAddressHousePart(address, true);
            else
            {
              FiasLevel parentLevel = address.GuidBottomLevel;
              string rbName = "для уровня [" + FiasEnumNames.ToString(parentLevel, false) + "] \"" + address[parentLevel] + "\"";
              if (!page.IsEmpty)
                address.AddHouseMessage(ErrorMessageKind.Warning, "Не найдено здание \"" +
                  FiasCachedPageHouse.GetText(houseNum, estStatus, buildNum, strNum, strStatus) +
                  "\", хотя в справочнике " + rbName + " есть другие здания");
              else
              {
                address.AddHouseMessage(ErrorMessageKind.Info, "Справочник " + rbName + " не содержит зданий");

                Guid gVillage = address.GetGuid(FiasLevel.Village);
                if (gVillage != Guid.Empty && address.GetName(FiasLevel.Street).Length == 0)
                {
                  // Населенный пункт выбран из справочника, а улица не задана
                  FiasCachedPageAddrOb pageVillage = loader.GetAOPage(gVillage, FiasLevel.Street);
                  if (!pageVillage.IsEmpty)
                    address.AddMessage(ErrorMessageKind.Warning, "Не задана улица, хотя в ФИАС есть улицы для населенного пункта \"" + address[FiasLevel.Village] + "\"", FiasLevel.Street); // 26.10.2020
                }
              }
              HasErrors = true;
            }
          }
        } // HasErrors
      }

      string flatNum = address.GetName(FiasLevel.Flat);
      string roomNum = address.GetName(FiasLevel.Room);

      //if (!NumValidator.IsValidNum(flatNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "Неправильный номер квартиры \"" + flatNum + "\". " + errorText, FiasLevel.Flat);
      //  HasErrors = true;
      //}
      //if (!NumValidator.IsValidNum(roomNum, out errorText))
      //{
      //  address.AddMessage(ErrorMessageKind.Error, "Неправильный номер помещения \"" + strNum + "\". " + errorText, FiasLevel.Room);
      //  HasErrors = true;
      //}

      bool HasRoom = (flatNum.Length + roomNum.Length) > 0;
      if (HasRoom)
      {
        if (!HasHouse)
        {
          address.AddRoomMessage(ErrorMessageKind.Error, "Задано помещение, но не задано здание");
          HasErrors = true; // 26.10.2020
        }
        if (_Source.DBSettings.UseRoom && (!HasErrors))
        {
          string flatAOType = GetCorrectedAOType(address, FiasLevel.Flat);
          FiasFlatType flatType = FiasEnumNames.ParseFlatType(flatAOType);

          string roomAOType = GetCorrectedAOType(address, FiasLevel.Room);
          FiasRoomType roomType = FiasEnumNames.ParseRoomType(roomAOType);

          FiasCachedPageRoom page = loader.GetRoomPage(address.GetGuid(FiasLevel.House));
          _RoomExtractor.Row = page.FindRow(flatNum, flatType, roomNum, roomType);
          if (_RoomExtractor.Row != null)
          {
            DoFillAddressRoomPart(address, true);
          }
          else
          {
            if (!page.IsEmpty)
              address.AddRoomMessage(ErrorMessageKind.Warning, "Не найдено помещение \"" +
                FiasCachedPageRoom.GetText(flatNum, flatType, roomNum, roomType) +
                "\" в справочнике для здания, хотя другие помещения есть");
            else
              address.AddHouseMessage(ErrorMessageKind.Info, "Справочник здания не содержит помещений");
            HasErrors = true;
          }
        }
      }
    }

    /// <summary>
    /// Изавлекает из адреса тип элемента для заданного уровня.
    /// Если название задано, а тип - нет, то устанавливается тип по умолчанию, например, "Дом".
    /// </summary>
    /// <param name="address"></param>
    /// <param name="level"></param>
    /// <returns>Исправленная аббревиатура</returns>
    private static string GetCorrectedAOType(FiasAddress address, FiasLevel level)
    {
      string name = address.GetName(level);
      string aoType = address.GetAOType(level);
      if ((name.Length > 0) != (aoType.Length > 0))
      {
        if (name.Length > 0)
        {
          aoType = FiasTools.GetDefaultAOType(level);
#if DEBUG
          if (aoType.Length == 0)
            throw new BugException("Не определен тип адресообразующего элемента по умолчанию");
#endif
        }
        else
          aoType = String.Empty;
        address.SetAOType(level, aoType); // исправили
      }
      return aoType;
    }

    #region Корректность номеров

#if XXX // используем FiasTools.IsValidName()
    private static class NumValidator
    {
    #region Статический конструктор

      static NumValidator()
      {
        string s1 = "АБВГДЕЁЖЗИЙКЛМОПРСТУФХЦЧШЩЪЫЬЭЮЯ0123456789";
        string s2 = "-/.";

        _AllValidChars = new CharArrayIndexer(s1 + s2, true);
        _SpecChars = new CharArrayIndexer(s2, false);
      }

      /// <summary>
      /// Все символы, которые могут быть в номере
      /// </summary>
      private static CharArrayIndexer _AllValidChars;

      /// <summary>
      /// Символы из списка _AllValidChars, которые не могут идти в начале или в конце, а также располагаться подряд
      /// (все, кроме цифр и букв)
      /// </summary>
      private static CharArrayIndexer _SpecChars;

    #endregion

    #region Проверка

      /// <summary>
      /// Проверяет корректность номера дома/корпуса/строения/квартиры/помещения.
      /// Пустая строка считается правильной.
      /// </summary>
      /// <param name="s">Проверяемый номер</param>
      /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
      /// <returns>true, если номер правильный</returns>
      public static bool IsValidNum(string s, out string errorText)
      {
        if (String.IsNullOrEmpty(s))
        {
          errorText = null;
          return true;
        }

        for (int i = 0; i < s.Length; i++)
        {
          if (!_AllValidChars.Contains(s[i]))
          {
            errorText = "Недопустимый символ \"" + s[i] + "\" в позиции " + (i + 1).ToString();
            return false;
          }

          if (_SpecChars.Contains(s[i]))
          {
            if (i == 0 || i == (s.Length - 1))
            {
              errorText = "Номер не может начинаться или заканчиваться на символ \"" + s[i] + "\"";
              return false;
            }

            if (i > 0)
            {
              if (_SpecChars.Contains(s[i - 1]))
              {
                if (s[i - 1] == s[i])
                  errorText = "Два символа \"" + s.Substring(i - 1, 2) + "\" подряд в позиции " + i.ToString();
                else
                  errorText = "Недопустимое сочетание символов \"" + s.Substring(i - 1, 2) + "\" подряд в позиции " + i.ToString();
                return false;
              }
            }
          }
        }
        errorText = null;
        return true;
      }

    #endregion
    }
#endif

    #endregion

    #endregion

    #endregion

    #region Загрузка буферизованных страниц

    private class PageLoader
    {
      #region Конструктор

      internal PageLoader(IFiasSource source)
      {
        _Source = source;
        AOGuids = new SingleScopeList<Guid>();
        AORecIds = new SingleScopeList<Guid>();
        if (source.DBSettings.UseHouse)
        {
          HouseGuids = new SingleScopeList<Guid>();
          HouseRecIds = new SingleScopeList<Guid>();
        }
        if (source.DBSettings.UseRoom)
        {
          RoomGuids = new SingleScopeList<Guid>();
          RoomRecIds = new SingleScopeList<Guid>();
        }
      }

      #endregion

      #region Свойства

      private readonly IFiasSource _Source;

      /// <summary>
      /// Сюда нужно добавить идентификаторы адресных объектов, на которые заданы ссылки в адресах
      /// </summary>
      internal readonly SingleScopeList<Guid> AOGuids;
      /// <summary>
      /// Сюда нужно добавить идентификаторы зданий, на которые заданы ссылки в адресах
      /// </summary>
      internal readonly SingleScopeList<Guid> HouseGuids;
      /// <summary>
      /// Сюда нужно добавить идентификаторы помещений, на которые заданы ссылки в адресах
      /// </summary>
      internal readonly SingleScopeList<Guid> RoomGuids;

      /// <summary>
      /// Сюда нужно добавить идентификаторы записей адресных объектов, на которые заданы ссылки в адресах.
      /// </summary>
      internal readonly SingleScopeList<Guid> AORecIds;

      /// <summary>
      /// Сюда нужно добавить идентификаторы записей зданий, на которые заданы ссылки в адресах
      /// </summary>
      internal readonly SingleScopeList<Guid> HouseRecIds;

      /// <summary>
      /// Сюда нужно добавить идентификаторы записей помещений, на которые заданы ссылки в адресах
      /// </summary>
      internal readonly SingleScopeList<Guid> RoomRecIds;

      /// <summary>
      /// После вызова метода Load() сюда будут добавлены пары "дочерний-адресный-объект: FiasGuidInfo".
      /// Список не создается, если в AOGuids не было добавлено ни одного идентификатора.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> AOGuidInfoDict;

      /// <summary>
      /// После вызова метода Load() сюда будут добавлены пары "GUID-здания: FiasGuidInfo".
      /// Список не создается, если в HouseGuids не было добавлено ни одного идентификатора.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> HouseGuidInfoDict;

      /// <summary>
      /// После вызова метода Load() сюда будут добавлены пары "GUID-помещения: FiasGuidInfo".
      /// Список не создается, если в RoomGuids не было добавлено ни одного идентификатора.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> RoomGuidInfoDict;

      /// <summary>
      /// После вызова метода Load() сюда будут добавлены пары "RecId-дочернего-адресного-объекта: FiasGuidInfo".
      /// Список не создается, если в AORecIds не было добавлено ни одного идентификатора.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> AORecIdInfoDict;

      /// <summary>
      /// После вызова метода Load() сюда будут добавлены пары "RecId-здания: FiasGuidInfo".
      /// Список не создается, если в HouseRecIds не было добавлено ни одного идентификатора.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> HouseRecIdInfoDict;

      /// <summary>
      /// После вызова метода Load() сюда будут добавлены пары "RecId-помещения: FiasGuidInfo".
      /// Список не создается, если в RoomRecIds не было добавлено ни одного идентификатора.
      /// </summary>
      internal Dictionary<Guid, FiasGuidInfo> RoomRecIdInfoDict;

      /// <summary>
      /// После вызова метода Load() здесь будут все нужные буферизованные страницы адресных объектов, включая
      /// родительские страницы, до справочника регионов включительно.
      /// Первым ключом является GUID родительского объекта
      /// Вторым ключом является уровень адресных объектов, которые есть в таблице для данной страницы (районы - улицы)
      /// Значением являются буферизованные страницы, у которых свойство Level совпадает со вторым ключом таблицы
      /// </summary>
      internal Dictionary<Guid, Dictionary<FiasLevel, FiasCachedPageAddrOb>> AOPages;

      /// <summary>
      /// После вызова метода Load() здесь будут все нужные буферизованные страницы зданий.
      /// Ключ - идентификатор родительского адресного объекта
      /// Значение - страница со списком зданий
      /// </summary>
      internal Dictionary<Guid, FiasCachedPageHouse> HousePages;

      /// <summary>
      /// После вызова метода Load() здесь будут все нужные буферизованные страницы помещений.
      /// Ключ - идентификатор здания.
      /// Значение - страница со списком помещений.
      /// </summary>
      internal Dictionary<Guid, FiasCachedPageRoom> RoomPages;

      /// <summary>
      /// Устанавливается в true, если адреса содержат номера домов
      /// </summary>
      private bool _HaveHouses;

      /// <summary>
      /// Устанавливается в true, если адреса содержат номера помещений
      /// </summary>
      private bool _HaveRooms;

      #endregion

      #region Заполнение списка GUID'ов

      /// <summary>
      /// Извлекает из адреса все GUIDы, кроме UnknownGuid, и добавляет их в списке XXGuids.
      /// </summary>
      /// <param name="address"></param>
      public void AddGuids(FiasAddress address)
      {
        if (!DoAddGuids(address))
          DoAddRecIds(address);

        if (!_HaveHouses && address.ContainsHouseNum)
          _HaveHouses = true;
        if (!_HaveRooms && address.ContainsRoomNum)
          _HaveRooms = true;

      }

      private bool DoAddGuids(FiasAddress address)
      {
        bool res = false;

        Guid g;

        g = address.AOGuid;
        if (g != Guid.Empty)
        {
          AOGuids.Add(g);
          res = true;
        }

        if (_Source.DBSettings.UseRoom)
        {
          g = address.GetGuid(FiasLevel.Flat);
          if (g != Guid.Empty)
          {
            RoomGuids.Add(g);
            res = true;
          }
        }
        if (_Source.DBSettings.UseHouse)
        {
          g = address.GetGuid(FiasLevel.House);
          if (g != Guid.Empty)
          {
            HouseGuids.Add(g);
            res = true;
          }
        }

        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          g = address.GetGuid(FiasTools.AOLevels[i]);
          if (g != Guid.Empty)
          {
            AOGuids.Add(g);
            res = true;
          }
        }

        return res;
      }

      /// <summary>
      /// Извлекает из адреса все идентификаторы записей, и добавляет их в списке XXRecIds.
      /// </summary>
      /// <param name="address"></param>
      private bool DoAddRecIds(FiasAddress address)
      {
        bool res = false;

        Guid g;

        g = address.AORecId;
        if (g != Guid.Empty)
        {
          AORecIds.Add(g);
          res = true;
        }

        if (_Source.DBSettings.UseRoom)
        {
          g = address.GetRecId(FiasLevel.Flat);
          if (g != Guid.Empty)
          {
            RoomRecIds.Add(g);
            res = true;
          }
        }

        if (_Source.DBSettings.UseHouse)
        {
          g = address.GetRecId(FiasLevel.House);
          if (g != Guid.Empty)
          {
            HouseRecIds.Add(g);
            res = true;
          }
        }

        for (int i = 0; i < FiasTools.AOLevels.Length; i++)
        {
          g = address.GetRecId(FiasTools.AOLevels[i]);
          if (g != Guid.Empty)
          {
            AORecIds.Add(g);
            res = true;
          }
        }

        return res;
      }

      #endregion

      #region Загрузка страниц

      /// <summary>
      /// Выполняет загрузку страниц, возможно, вызывая GetParentAOGuids() и GetPages() несколько раз 
      /// </summary>
      internal void Load(ISplash spl)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.PageLoader.Load() started.");

        #region Room

        if (RoomGuids != null)
        {
#if DEBUG
          if (HouseGuids == null)
            throw new BugException("HouseGuids==null при RoomGuids!=null");
#endif

          RoomPages = new Dictionary<Guid, FiasCachedPageRoom>();
          if (RoomGuids.Count > 0 || RoomRecIds.Count > 0)
          {
            #region Вызов GetParentGuids()

            spl.PhaseText = "Получение GUIDов зданий для помещений (" + (RoomGuids.Count + RoomRecIds.Count).ToString() + ")";

            SingleScopeList<Guid> PageGuids = new SingleScopeList<Guid>();
            if (RoomGuids.Count > 0)
            {
              RoomGuidInfoDict = new Dictionary<Guid, FiasGuidInfo>();
              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetGuidInfo(RoomGuids.ToArray(), FiasTableType.Room);
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                RoomGuidInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                PageGuids.Add(pair1.Value.ParentGuid);
              }
            }

            if (RoomRecIds.Count > 0)
            {
              RoomRecIdInfoDict = new Dictionary<Guid, FiasGuidInfo>();
              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetRecIdInfo(RoomRecIds.ToArray(), FiasTableType.Room);
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                RoomRecIdInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                PageGuids.Add(pair1.Value.ParentGuid);
              }
            }

            #endregion

            #region Загрузка страниц

            spl.PhaseText = "Загрузка страниц помещений (" + PageGuids.Count.ToString() + ")";

            IDictionary<Guid, FiasCachedPageRoom> dict2 = _Source.GetRoomPages(PageGuids.ToArray());
            foreach (KeyValuePair<Guid, FiasCachedPageRoom> pair2 in dict2)
            {
              RoomPages[pair2.Value.PageHouseGuid] = pair2.Value;
              if (pair2.Value.PageHouseGuid != Guid.Empty)
                HouseGuids.Add(pair2.Value.PageHouseGuid); // на уровень вверх
              else
                throw new BugException("Для помещения с GUID=" + pair2.Key.ToString() + " возвращен идентификатор здания GUID.Empty");
            }

            #endregion
          }
        }

        #endregion

        #region House

        if (HouseGuids != null)
        {
          HousePages = new Dictionary<Guid, FiasCachedPageHouse>();
          if (HouseGuids.Count > 0 || HouseRecIds.Count > 0)
          {
            #region Вызов GetParentGuids()

            spl.PhaseText = "Получение GUIDов адресных объектов для зданий (" + (HouseGuids.Count + HouseRecIds.Count).ToString() + ")";

            SingleScopeList<Guid> PageGuids = new SingleScopeList<Guid>();

            if (HouseGuids.Count > 0)
            {
              HouseGuidInfoDict = new Dictionary<Guid, FiasGuidInfo>();
              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetGuidInfo(HouseGuids.ToArray(), FiasTableType.House);
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                HouseGuidInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                PageGuids.Add(pair1.Value.ParentGuid);
              }
            }

            if (HouseRecIds.Count > 0)
            {
              HouseRecIdInfoDict = new Dictionary<Guid, FiasGuidInfo>();
              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetRecIdInfo(HouseRecIds.ToArray(), FiasTableType.House);
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                HouseRecIdInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                PageGuids.Add(pair1.Value.ParentGuid);
              }
            }

            #endregion

            #region Загрузка страниц

            spl.PhaseText = "Загрузка страниц зданий (" + PageGuids.Count.ToString() + ")";

            IDictionary<Guid, FiasCachedPageHouse> dict2 = _Source.GetHousePages(PageGuids.ToArray());
            foreach (KeyValuePair<Guid, FiasCachedPageHouse> pair2 in dict2)
            {
              HousePages[pair2.Value.PageAOGuid] = pair2.Value;
              if (pair2.Value.PageAOGuid != Guid.Empty)
                AOGuids.Add(pair2.Value.PageAOGuid); // на уровень вверх
              else
                throw new BugException("Для здания с GUID=" + pair2.Key.ToString() + " возвращен идентификатор адресного объекта GUID.Empty");
            }

            #endregion
          }
        }

        #endregion

        #region AddrOb

        AOPages = new Dictionary<Guid, Dictionary<FiasLevel, FiasCachedPageAddrOb>>();

        SingleScopeList<Guid> AuxHousePageGuids = null; // Собираем дополнительные идентификаторы адресных объектов, для которых нужно загрузить список зданий

        if (AOGuids.Count > 0 || AORecIds.Count > 0)
        {
          #region Вызов GetParentGuids()

          Dictionary<FiasLevel, SingleScopeList<Guid>> PageGuidDict = new Dictionary<FiasLevel, SingleScopeList<Guid>>();
          SingleScopeList<Guid> ResidualAOGuids = new SingleScopeList<Guid>();

          if (AORecIds.Count > 0)
          {
            spl.PhaseText = "Получение GUIDов адресных объектов по идентификаторам записей (" + AORecIds.Count.ToString() + ")";

            AORecIdInfoDict = new Dictionary<Guid, FiasGuidInfo>();

            IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetRecIdInfo(AORecIds.ToArray(), FiasTableType.AddrOb);
            foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
            {
              AORecIdInfoDict.Add(pair1.Key, pair1.Value);
              if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                continue;
              if (pair1.Value.ParentGuid != Guid.Empty)
                AOGuids.Add(pair1.Value.ParentGuid);

              if (!PageGuidDict.ContainsKey(pair1.Value.Level))
                PageGuidDict.Add(pair1.Value.Level, new SingleScopeList<Guid>());
              PageGuidDict[pair1.Value.Level].Add(pair1.Value.ParentGuid);
            }
          }

          if (AOGuids.Count > 0)
          {
            AOGuidInfoDict = new Dictionary<Guid, FiasGuidInfo>();
            ResidualAOGuids.AddRange(AOGuids);
            bool IsFirstRound = true;

            while (ResidualAOGuids.Count > 0)
            {
              spl.PhaseText = "Получение GUIDов родительских адресных объектов (" + ResidualAOGuids.Count.ToString() + ")";

              IDictionary<Guid, FiasGuidInfo> dict1 = _Source.GetGuidInfo(ResidualAOGuids.ToArray(), FiasTableType.AddrOb);

              if (IsFirstRound && _HaveHouses && _Source.DBSettings.UseHouse)
                AuxHousePageGuids = new SingleScopeList<Guid>();

              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                //if (pair1.Key == new Guid("{54049357-326d-4b8f-b224-3c6dc25d6dd3}"))
                //{
                //}
                AOGuidInfoDict.Add(pair1.Key, pair1.Value);
                if (pair1.Value.ParentGuid == FiasTools.GuidNotFound)
                  continue;
                if (!PageGuidDict.ContainsKey(pair1.Value.Level))
                  PageGuidDict.Add(pair1.Value.Level, new SingleScopeList<Guid>());

                PageGuidDict[pair1.Value.Level].Add(pair1.Value.ParentGuid);

                if (IsFirstRound &&
                  AuxHousePageGuids != null &&
                  FiasTools.IsInheritableLevel(pair1.Value.Level, FiasLevel.House, false)) // могут ли для этого уровня быть дома?
                {
                  AuxHousePageGuids.Add(pair1.Key);
                }
              }

              // 21.01.2020
              // Заполняем ResidualAOGuids в отдельном цикле, после того, как AOParentDict уже заполнен

              ResidualAOGuids.Clear();
              foreach (KeyValuePair<Guid, FiasGuidInfo> pair1 in dict1)
              {
                if (pair1.Value.ParentGuid != Guid.Empty && (!AOGuidInfoDict.ContainsKey(pair1.Value.ParentGuid)))
                  ResidualAOGuids.Add(pair1.Value.ParentGuid);
              }
              IsFirstRound = false;
            }
          }

          #endregion

          #region Загрузка страниц

          foreach (KeyValuePair<FiasLevel, SingleScopeList<Guid>> pair2 in PageGuidDict)
          {
            spl.PhaseText = "Загрузка страниц адресных объектов \"" + FiasEnumNames.ToString(pair2.Key, false) + "\" (" + pair2.Value.Count.ToString() + ")";

            IDictionary<Guid, FiasCachedPageAddrOb> dict3 = _Source.GetAddrObPages(pair2.Key, pair2.Value.ToArray());
            foreach (KeyValuePair<Guid, FiasCachedPageAddrOb> pair3 in dict3)
            {
              //if (pair3.Key == new Guid("{54049357-326d-4b8f-b224-3c6dc25d6dd3}"))
              //{ 
              //}

              if (!AOPages.ContainsKey(pair3.Value.PageAOGuid))
                AOPages.Add(pair3.Value.PageAOGuid, new Dictionary<FiasLevel, FiasCachedPageAddrOb>());
              AOPages[pair3.Value.PageAOGuid][pair2.Key] = pair3.Value;
            }
          }

          #endregion
        }

        #endregion

        #region Houses - проход 2

        // Пока не знаю, как сразу добавить помещения
        //SingleScopeList<Guid> AuxRoomPageGuids =null;
        //if (_Source.DBSettings.UseRoom && _HaveRooms)
        //{
        //  AuxRoomPageGuids = new SingleScopeList<Guid>(); // Собираем дополнительные идентификаторы зданий, для которых нужно загрузить список помещений
        //  if (RoomGuids.Count == 0)
        //    AuxRoomPageGuids.AddRange(HouseGuids); // Были заданы GUIDы зданий и номера помещений вручную.
        //}

        if (AuxHousePageGuids != null)
        {
          SingleScopeList<Guid> PageGuids = new SingleScopeList<Guid>();
          foreach (Guid g in AuxHousePageGuids)
          {
            if (!HousePages.ContainsKey(g))
              PageGuids.Add(g);
          }

          if (PageGuids.Count > 0)
          {
            #region Загрузка страниц

            spl.PhaseText = "Загрузка страниц зданий (" + PageGuids.Count.ToString() + ")";

            IDictionary<Guid, FiasCachedPageHouse> dict2 = _Source.GetHousePages(PageGuids.ToArray());
            foreach (KeyValuePair<Guid, FiasCachedPageHouse> pair2 in dict2)
              HousePages[pair2.Value.PageAOGuid] = pair2.Value;

            #endregion
          }
        }

        #endregion

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.PageLoader.Load() finished.");
      }

      #endregion

      #region Дополнительные методы получения страниц

      public FiasCachedPageAddrOb GetAOPage(Guid pageAOGuid, FiasLevel level)
      {
        Dictionary<FiasLevel, FiasCachedPageAddrOb> dict1;
        if (!AOPages.TryGetValue(pageAOGuid, out dict1))
        {
          dict1 = new Dictionary<FiasLevel, FiasCachedPageAddrOb>();
          AOPages.Add(pageAOGuid, dict1);
        }

        FiasCachedPageAddrOb page;
        if (!dict1.TryGetValue(level, out page))
        {
          // Требуется получить страницу
          page = _Source.GetAddrObPages(level, new Guid[1] { pageAOGuid })[pageAOGuid];
          dict1.Add(level, page);
        }
        return page;
      }

      public FiasCachedPageHouse GetHousePage(Guid pageAOGuid)
      {
        FiasCachedPageHouse page;
        if (!HousePages.TryGetValue(pageAOGuid, out page))
        {
          page = _Source.GetHousePages(new Guid[1] { pageAOGuid })[pageAOGuid];
          HousePages.Add(pageAOGuid, page);
        }
        return page;
      }

      public FiasCachedPageRoom GetRoomPage(Guid pageHouseGuid)
      {
        FiasCachedPageRoom page;
        if (!RoomPages.TryGetValue(pageHouseGuid, out page))
        {
          page = _Source.GetRoomPages(new Guid[1] { pageHouseGuid })[pageHouseGuid];
          RoomPages.Add(pageHouseGuid, page);
        }
        return page;
      }

      #endregion
    }

    /// <summary>
    /// Таблица сокращений
    /// </summary>
    public FiasCachedAOTypes AOTypes
    {
      get
      {
        if (_AOTypes == null)
          _AOTypes = _Source.GetAOTypes();
        return _AOTypes;
      }
    }
    private FiasCachedAOTypes _AOTypes;

    #endregion

    #region Методы для редактора

    /// <summary>
    /// Не используется в прикладном коде
    /// </summary>
    /// <param name="level"></param>
    /// <param name="pageAOGuid"></param>
    /// <returns></returns>
    public FiasCachedPageAddrOb GetAddrObPage(FiasLevel level, Guid pageAOGuid)
    {
      return _Source.GetAddrObPages(level, new Guid[1] { pageAOGuid })[pageAOGuid];
    }

    /// <summary>
    /// Не используется в прикладном коде
    /// </summary>
    /// <param name="pageAOGuid"></param>
    /// <returns></returns>
    public FiasCachedPageHouse GetHousePage(Guid pageAOGuid)
    {
      return _Source.GetHousePages(new Guid[1] { pageAOGuid })[pageAOGuid];
    }

    /// <summary>
    /// Не используется в прикладном коде
    /// </summary>
    /// <param name="pageHouseGuid"></param>
    /// <returns></returns>
    public FiasCachedPageRoom GetRoomPage(Guid pageHouseGuid)
    {
      return _Source.GetRoomPages(new Guid[1] { pageHouseGuid })[pageHouseGuid];
    }

    #endregion

    #region Поиск адресов

    /// <summary>
    /// Возвращает true, если поддерживается поиск адресов
    /// </summary>
    public bool AddressSearchEnabled { get { return _Source.InternalSettings.FTSMode != FiasFTSMode.None; } }

    /// <summary>
    /// Выполнить поиск адресов по заданным параметрам
    /// </summary>
    /// <param name="searchParams">Параметры поиска</param>
    /// <returns>Массив найденных адресов</returns>
    public FiasAddress[] FindAddresses(FiasAddressSearchParams searchParams)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FindAddresss() started.");

      if (!AddressSearchEnabled)
        throw new InvalidOperationException("Поиск адресов не поддерживается");

      if (searchParams == null)
        throw new ArgumentNullException("searchParams");
      if (String.IsNullOrEmpty(searchParams.Text))
        throw new ArgumentNullException("searchParams", "searchParams.Text==null");
      if (searchParams.Levels != null)
      {
        if (searchParams.Levels.Length == 0)
          throw new ArgumentException("Задан пустой список уровней searchParams.Levels.Length==0", "searchParams");
      }
      DataTable table = _Source.FindAddresses(searchParams).Tables[0];

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FindAddresss(). Table.Rows.Count=" + table.Rows.Count.ToString());

      if (table.Rows.Count == 0)
      {
        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FindAddresss() finished.");
        return new FiasAddress[0];
      }

      // Убираем повторяющиеся адреса
      RemoveRepeatedAOGuids(table);

      FiasAddress[] a = new FiasAddress[table.Rows.Count];
      for (int i = 0; i < table.Rows.Count; i++)
      {
        a[i] = new FiasAddress();
        //a[i].AOGuid = DataTools.GetGuid(table.Rows[i], "AOGUID");
        a[i].AORecId = DataTools.GetGuid(table.Rows[i], "AOID");
      }

      FillAddresses(a);

      //if (_Source.DBSettings.UseHistory)
      //{
      //  for (int i = 0; i < table.Rows.Count; i++)
      //  {
      //    if (!DataTools.GetBool(table.Rows[i], "Actual"))
      //      a[i].Actuality = FiasActuality.Historical;
      //  }
      //}

      // Выполняем сортировку адресов
      DataTable table2 = new DataTable();
      table2.Columns.Add("Index", typeof(int));
      table2.Columns.Add("AddressText", typeof(string));
      table2.Columns.Add("Actuality", typeof(int)); // актуальные перед историческими
      if (_Source.DBSettings.UseDates)
      {
        table2.Columns.Add("STARTDATE", typeof(DateTime));
        table2.Columns.Add("ENDDATE", typeof(DateTime));
      }
      for (int i = 0; i < a.Length; i++)
      {
        // Почтовый индекс мешается
        string PostalCode = a[i].PostalCode;
        a[i].PostalCode = String.Empty;
        DataRow row2 = table2.Rows.Add(i, a[i].ToString(), (int)(a[i].Actuality));
        a[i].PostalCode = PostalCode;

#if XXX // Куда засунуть даты?
        if (_Source.DBSettings.UseDates)
        {
          if (_Source.InternalSettings.UseOADates)
          {
            row2["STARTDATE"] = DateTime.FromOADate(DataTools.GetInt(table.Rows[i], "dStartDate"));
            row2["ENDDATE"] = DateTime.FromOADate(DataTools.GetInt(table.Rows[i], "dEndDate"));
          }
          else
          {
            row2["STARTDATE"] = table.Rows[i]["STARTDATE"];
            row2["ENDDATE"] = table.Rows[i]["ENDDATE"];
          }
        }
#endif
      }

      table2.DefaultView.Sort = "AddressText,Actuality";
      FiasAddress[] a2 = new FiasAddress[a.Length];
      for (int i = 0; i < table2.DefaultView.Count; i++)
      {
        int index = DataTools.GetInt(table2.DefaultView[i].Row, "Index");
        a2[i] = a[index];
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.FindAddresss() finished.");

      return a2;
    }

    private void RemoveRepeatedAOGuids(DataTable table)
    {
      string s = "AOGUID";
      if (_Source.DBSettings.UseHistory)
        s += ",Actual DESC";
      if (_Source.DBSettings.UseDates)
      {
        if (_Source.InternalSettings.UseOADates)
          s += ",dEndDate DESC,dStartDate DESC";
        else
          s += ",ENDDATE DESC,STARTDATE DESC";
      }
      table.DefaultView.Sort = s;
      Guid prevG = Guid.Empty;
      List<DataRow> delRows = null;
      for (int i = 0; i < table.DefaultView.Count; i++)
      {
        Guid g = DataTools.GetGuid(table.DefaultView[i].Row, "AOGUID");
        if (g == prevG)
        {
          if (delRows == null)
            delRows = new List<DataRow>();
          delRows.Add(table.DefaultView[i].Row);
        }
        else
          prevG = g;
      }
      if (delRows != null)
      {
        for (int i = 0; i < delRows.Count; i++)
          delRows[i].Delete();
        table.AcceptChanges();

        if (FiasTools.TraceSwitch.Enabled)
          Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.RemoveRepeatedAOGuids() deleted " + delRows.Count.ToString() + " row(s).");
      }
    }

    #endregion

    #region Текстовое представление адреса

    /// <summary>
    /// Получить текстовое представление адреса, включая почтовый индекс
    /// </summary>
    /// <param name="address">Заполненный адрес</param>
    /// <returns>Текстовое представление</returns>
    public string GetText(FiasAddress address)
    {
      StringBuilder sb = new StringBuilder();
      GetText(sb, address);
      return sb.ToString();
    }

    /// <summary>
    /// Получить текстовое представление адреса, включая почтовый индекс
    /// </summary>
    /// <param name="sb">Буфер для записи строки</param>
    /// <param name="address">Заполненный адрес</param>
    public void GetText(StringBuilder sb, FiasAddress address)
    {
#if DEBUG
      if (sb == null)
        throw new ArgumentNullException("sb");
      if (address == null)
        throw new ArgumentNullException("address");
#endif
      if (!String.IsNullOrEmpty(address.PostalCode))
      {
        sb.Append(address.PostalCode);
        if (address.NameBottomLevel != FiasLevel.Unknown)
          sb.Append(", ");
      }
      GetTextWithoutPostalCode(sb, address);
    }


    /// <summary>
    /// Получить текстовое представление адреса без почтового индекса
    /// </summary>
    /// <param name="address">Заполненный адрес</param>
    /// <returns>Текстовое представление</returns>
    public string GetTextWithoutPostalCode(FiasAddress address)
    {
      StringBuilder sb = new StringBuilder();
      GetTextWithoutPostalCode(sb, address);
      return sb.ToString();
    }

    /// <summary>
    /// Получить текстовое представление адреса без почтового индекса
    /// </summary>
    /// <param name="sb">Буфер для записи строки</param>
    /// <param name="address">Заполненный адрес</param>
    public void GetTextWithoutPostalCode(StringBuilder sb, FiasAddress address)
    {
#if DEBUG
      if (sb == null)
        throw new ArgumentNullException("sb");
      if (address == null)
        throw new ArgumentNullException("address");
#endif

      // Нельзя сразу использовать sb, т.к. он может быть непустой.
      // При записи компонентов добавляется запятая
      if (_InternalSB == null)
        _InternalSB = new StringBuilder();
      _InternalSB.Length = 0;
      GetComponentTextAt(address, FiasLevel.Region);
      sb.Append(_InternalSB);
    }

    #endregion

    #region Format()

    private Dictionary<string, FiasParsedFormatString> _FormatDict;
    private StringBuilder _InternalSB;

    /// <summary>
    /// Получение текстового представления адреса или его компонентов с использованием форматирования.
    /// Выполняет парсинг строки форматирования <paramref name="format"/> методом FiasFormatStringParser.Parse().
    /// В случае ошибки в строке выбрасывается исключения. Выполняется кэширование результатов парсинга.
    /// </summary>
    /// <param name="address">Заполненный объект адреса. Не может быть null</param>
    /// <param name="format">Строка форматирования</param>
    /// <returns>Текстовое представление в соответствии со строкой форматирования</returns>
    public string Format(FiasAddress address, string format)
    {
      StringBuilder sb = new StringBuilder();
      Format(sb, address, format);
      return sb.ToString();
    }

    /// <summary>
    /// Получение текстового представления адреса или его компонентов с использованием форматирования.
    /// Выполняет парсинг строки форматирования <paramref name="format"/> методом FiasFormatStringParser.Parse().
    /// В случае ошибки в строке выбрасывается исключения. Выполняется кэширование результатов парсинга.
    /// </summary>
    /// <param name="sb">Буфер, куда добавляется текстовое представление в соответствии со строкой форматирования</param>
    /// <param name="address">Заполненный объект адреса. Не может быть null</param>
    /// <param name="format">Строка форматирования</param>
    public void Format(StringBuilder sb, FiasAddress address, string format)
    {
      if (_FormatDict == null)
        _FormatDict = new Dictionary<string, FiasParsedFormatString>();

      FiasParsedFormatString parsedString;
      if (!_FormatDict.TryGetValue(format, out parsedString))
      {
        parsedString = FiasFormatStringParser.Parse(format);
        _FormatDict.Add(format, parsedString);
      }

      Format(sb, address, parsedString);
    }

    /// <summary>
    /// Получение текстового представления адреса или его компонентов с использованием форматирования.
    /// </summary>
    /// <param name="address">Заполненный объект адреса. Не может быть null.</param>
    /// <param name="format">Результат парсинга строки форматирования, выполненный методом FiasFormatStringParser.Parse(). Не может быть null.</param>
    /// <returns>Текстовое представление в соответствии со строкой форматирования</returns>
    public string Format(FiasAddress address, FiasParsedFormatString format)
    {
      StringBuilder sb = new StringBuilder();
      Format(sb, address, format);
      return sb.ToString();
    }

    /// <summary>
    /// Получение текстового представления адреса или его компонентов с использованием форматирования.
    /// </summary>
    /// <param name="sb">Буфер, куда добавляется текстовое представление в соответствии со строкой форматирования</param>
    /// <param name="address">Заполненный объект адреса. Не может быть null.</param>
    /// <param name="format">Результат парсинга строки форматирования, выполненный методом FiasFormatStringParser.Parse(). Не может быть null.</param>
    public void Format(StringBuilder sb, FiasAddress address, FiasParsedFormatString format)
    {
      if (sb == null)
        throw new ArgumentNullException("sb");
      if (address == null)
        throw new ArgumentNullException("address");
      if (format == null)
        throw new ArgumentNullException("format");

      if (_InternalSB == null)
        _InternalSB = new StringBuilder();


      int count = 0;
      for (int i = 0; i < format.Items.Count; i++)
      {
        FiasParsedFormatString.FormatItem item = format.Items[i];
        if (item.ItemType != FiasFormatStringParser.TypeFormConst)
        {
          _InternalSB.Length = 0;
          GetComponentText(address, item.ItemType);
          if (_InternalSB.Length == 0)
            continue;
        }

        if (count > 0)
          sb.Append(item.Separator);
        sb.Append(item.Prefix);
        sb.Append(_InternalSB.ToString());
        sb.Append(item.Suffix);
        count++;
      }
    }

    private void GetComponentText(FiasAddress address, int itemType)
    {
      FiasLevel level = (FiasLevel)(itemType & FiasFormatStringParser.TypeLevelMask);
      int form = itemType & FiasFormatStringParser.TypeFormMask;
      switch (form)
      {
        case FiasFormatStringParser.TypeFormText:
          if (!String.IsNullOrEmpty(address.PostalCode))
          {
            _InternalSB.Append(address.PostalCode);
            //_InternalSB.Append(0, ", ");
          }
          GetComponentTextAt(address, FiasLevel.Region);
          break;
        case FiasFormatStringParser.TypePostalCode:
          _InternalSB.Append(address.PostalCode);
          break;

        case FiasFormatStringParser.TypeFormNameAndAbbr:
          switch (level)
          {
            case FiasLevel.House:
              GetComponentNameAndAbbreviation(address, FiasLevel.House);
              GetComponentNameAndAbbreviation(address, FiasLevel.Building);
              GetComponentNameAndAbbreviation(address, FiasLevel.Structure);
              break;
            case FiasLevel.Room:
              GetComponentNameAndAbbreviation(address, FiasLevel.Flat);
              GetComponentNameAndAbbreviation(address, FiasLevel.Room);
              break;
            default:
              GetComponentNameAndAbbreviation(address, level);
              break;
          }
          break;

        case FiasFormatStringParser.TypeFormName:
          switch (level)
          {
            case FiasLevel.House:
              GetComponentNameAndAbbreviation(address.GetName(FiasLevel.House), String.Empty, FiasLevel.House);
              GetComponentNameAndAbbreviation(address, FiasLevel.Building);
              GetComponentNameAndAbbreviation(address, FiasLevel.Structure);
              break;
            case FiasLevel.Room:
              GetComponentNameAndAbbreviation(address.GetName(FiasLevel.Flat), String.Empty, FiasLevel.Flat);
              GetComponentNameAndAbbreviation(address, FiasLevel.Room);
              break;
            default:
              _InternalSB.Append(address.GetName(level));
              break;
          }
          break;

        case FiasFormatStringParser.TypeFormType:
          _InternalSB.Append(address.GetAOType(level));
          break;

        case FiasFormatStringParser.TypeFormAbbr:
          _InternalSB.Append(AOTypes.GetAbbreviation(address.GetAOType(level), level));
          break;

        case FiasFormatStringParser.TypeFormNum:
          switch (level)
          {
            case FiasLevel.House:
            case FiasLevel.Building:
            case FiasLevel.Structure:
            case FiasLevel.Flat:
            case FiasLevel.Room:
              _InternalSB.Append(address.GetName(level));
              break;
            default:
              throw new BugException("Неправильный Level=" + level.ToString() + " для TypeFormNum");
          }
          break;

        case FiasFormatStringParser.TypeFormAt:
          GetComponentTextAt(address, level);
          break;

        case FiasFormatStringParser.TypeRegionCode:
          _InternalSB.Append(address.RegionCode);
          break;

        case FiasFormatStringParser.TypeFormGuid:
          GetGuid(address.GetGuid(level));
          break;
        case FiasFormatStringParser.TypeFormRecId:
          GetGuid(address.GetRecId(level));
          break;
        case FiasFormatStringParser.TypeAOGuid:
          GetGuid(address.AOGuid);
          break;
        case FiasFormatStringParser.TypeAORecId:
          GetGuid(address.AORecId);
          break;
        case FiasFormatStringParser.TypeAnyGuid:
          GetGuid(address.AnyGuid);
          break;
        //case FiasFormatStringParser.TypeAOGuid:
        //  GetGuid(address.AnyRecId);
        //  break;

        case FiasFormatStringParser.TypeOKATO:
          _InternalSB.Append(address.OKATO);
          break;
        case FiasFormatStringParser.TypeOKTMO:
          _InternalSB.Append(address.OKTMO);
          break;

        case FiasFormatStringParser.TypeIFNSFL:
          _InternalSB.Append(address.IFNSFL);
          break;
        //case FiasFormatStringParser.TypeTerrIFNSFL:
        //  _InternalSB.Append(address.TerrIFNSFL);
        //  break;
        case FiasFormatStringParser.TypeIFNSUL:
          _InternalSB.Append(address.IFNSUL);
          break;
        //case FiasFormatStringParser.TypeTerrIFNSUL:
        //  _InternalSB.Append(address.TerrIFNSUL);
        //  break;

        default:
          throw new BugException("Неизвестная форма " + form.ToString());
      }
    }

    private void GetComponentTextAt(FiasAddress address, FiasLevel topLevel)
    {
      for (int i = FiasTools.AllLevelIndexer.IndexOf(topLevel); i < FiasTools.AllLevels.Length; i++)
        GetComponentNameAndAbbreviation(address, FiasTools.AllLevels[i]);
    }

    private void GetComponentNameAndAbbreviation(FiasAddress address, FiasLevel level)
    {
      string nm = address.GetName(level);
      string aoType = address.GetAOType(level);
      string abbr = AOTypes.GetAbbreviation(aoType, level);

      if (level == FiasLevel.Region)
      {
        if (nm.IndexOf(aoType, StringComparison.OrdinalIgnoreCase) >= 0)
          abbr = String.Empty; // 05.10.2020
      }

      GetComponentNameAndAbbreviation(nm, abbr, level);
    }
    private void GetComponentNameAndAbbreviation(string name, string abbr, FiasLevel level)
    {
      if (String.IsNullOrEmpty(name))
        return;
      if (_InternalSB.Length > 0)
        _InternalSB.Append(", ");

      if (GetComponentNameAndAbbreviationSpecialCases(name, abbr, level))
        return;

      FiasAOTypePlace place = GetAOTypePlace(level, name, abbr, FiasAOTypeMode.Abbreviation);

      if (String.IsNullOrEmpty(abbr))
        place = FiasAOTypePlace.None;

      switch (place)
      {
        case FiasAOTypePlace.AfterName:
          _InternalSB.Append(name);
          _InternalSB.Append(" ");
          _InternalSB.Append(abbr);
          break;
        case FiasAOTypePlace.BeforeName:
          _InternalSB.Append(abbr);
          if (abbr[abbr.Length - 1] != '.' && AOTypes.IsAbbreviationDotRequired(abbr, level))
            _InternalSB.Append('.'); // 02.09.2021
          _InternalSB.Append(" ");
          _InternalSB.Append(name);
          break;
        case FiasAOTypePlace.None:
          _InternalSB.Append(name);
          break;
        default:
          throw new BugException("place=" + place.ToString());
      }
    }

    private bool GetComponentNameAndAbbreviationSpecialCases(string name, string abbr, FiasLevel level)
    {
      int p;

      switch (abbr)
      {
        case "линия":
          p = name.IndexOf("-я ");
          if (p >= 0)
          {
            // Город Санкт-Петербург, линия "14-я В.О." -> 14-я линия В.О.

            _InternalSB.Append(name.Substring(0, p + 2)); // включая "-я", но без пробела
            _InternalSB.Append(" линия");
            _InternalSB.Append(name.Substring(p + 2)); // начиная с пробела
            return true;
          }
          if (name.EndsWith("-я"))
          {
            // Город Санкт-Петербург, линия "сдт Ленмашзавод 1-я" -> сдт Ленмашзавод 1-я линия
            _InternalSB.Append(name);
            _InternalSB.Append(" линия");
            return true;
          }
          // Город Санкт-Петербург, линия "Фруктовая (Апраксин двор)" -> Фруктовая линия (Апраксин двор)
          p = name.IndexOf("я (");
          if (p >= 0)
          {
            _InternalSB.Append(name.Substring(0, p + 1)); // без пробела
            _InternalSB.Append(" линия");
            _InternalSB.Append(name.Substring(p + 1));
            return true;
          }
          break;
      }
      return false;
    }

    /// <summary>
    /// Возвращает место, в котором должно располагаться сокращение для уровня.
    /// Например, сокращение "р-н" должно идти после наименования, а "г."- до наименования
    /// </summary>
    /// <param name="level">Уровень адресообразующего элемента</param>
    /// <param name="name">Именная часть адресного объекта</param>
    /// <param name="aoType">Сокращение или типа адресного объекта</param>
    /// <param name="aoTypeMode">Тип или сокращение в <paramref name="aoType"/>?</param>
    /// <returns>Положение сокращения относительно наименования</returns>
    public FiasAOTypePlace GetAOTypePlace(FiasLevel level, string name, string aoType, FiasAOTypeMode aoTypeMode)
    {
      if (String.IsNullOrEmpty(aoType))
        return FiasAOTypePlace.None;
      switch (level)
      {
        case FiasLevel.Region:
        case FiasLevel.District:
          return FiasAOTypePlace.AfterName;
        default:
          switch (aoType.ToUpperInvariant())
          {
            case "КМ":
              return FiasAOTypePlace.AfterName;
          }
          return FiasAOTypePlace.BeforeName;
      }
    }

    private void GetGuid(Guid guid)
    {
      if (guid == Guid.Empty)
        return;
      _InternalSB.Append(guid.ToString("D")); // без скобок
    }

    #endregion

    #region Получение адресов из текста без колонок

    /// <summary>
    /// Парсинг нескольких строк в адреса.
    /// Предполагается, что компоненты адреса разделены запятыми.
    /// Для пустых строк возвращаются пустые адреса (FiasAddress.IsEmpty=true).
    /// Ошибки парсинга записываются в списки FiasAddress.Messages. 
    /// </summary>
    /// <param name="lines">Строки. Каждому элементу массива (строке) соответствует один адрес</param>
    /// <param name="parseSettings">Параметры парсинга</param>
    /// <returns>Массив адресов. Длина массива соответствует <paramref name="lines"/>.</returns>
    public FiasAddress[] ParseAddresses(string[] lines, OldFiasParseSettings parseSettings)
    {
      // TODO: Надо уменьшить количество обращений к серверу, группируя их, а не каждый адрес отдельно

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddresses() started. lines.Length=" + lines.Length.ToString());

      FiasAddress[] a = new FiasAddress[lines.Length];

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "Построение списка GUIDов",
       "Загрузка страниц",
       lines.Length==1 ? "Распознание адреса" : "Распознание адресов ("+lines.Length.ToString()+")",
       "Проверка"
      });
      try
      {
        PageLoader loader = new PageLoader(_Source);
        loader.AddGuids(parseSettings.BaseAddress);
        spl.Complete();

        loader.Load(spl);
        spl.Complete();

        ErrorMessageList[] aParseErrors = new ErrorMessageList[lines.Length]; // Дополнительные ошибки парсинга
        spl.PercentMax = lines.Length;
        spl.AllowCancel = true;

        for (int i = 0; i < lines.Length; i++)
        {
          aParseErrors[i] = new ErrorMessageList();
          a[i] = DoOldParseAddress(lines[i], parseSettings, loader, aParseErrors[i]);
          spl.IncPercent();
        }
        spl.Complete();

        FillAddresses(a);
        for (int i = 0; i < lines.Length; i++)
          a[i].AddMessages(aParseErrors[i]);
        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddresses() finished.");

      return a;
    }

    /// <summary>
    /// Парсинг строки в адрес.
    /// Предполагается, что компоненты адреса разделены запятыми.
    /// Для пустой строки возвращается пустой адрес (FiasAddress.IsEmpty=true).
    /// Ошибки парсинга записываются в списки FiasAddress.Messages. 
    /// </summary>
    /// <param name="s">Строка для парсинга</param>
    /// <param name="parseSettings">Параметры парсинга</param>
    /// <returns>Адрес</returns>
    public FiasAddress ParseAddress(string s, OldFiasParseSettings parseSettings)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddress() started.");

      FiasAddress addr;
      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "Построение списка GUIDов",
       "Загрузка страниц",
       "Распознание адреса",
       "Проверка",
      });
      try
      {
        PageLoader loader = new PageLoader(_Source);
        loader.AddGuids(parseSettings.BaseAddress);
        spl.Complete();

        loader.Load(spl);
        spl.Complete();

        ErrorMessageList parseErrors = new ErrorMessageList();
        addr = DoOldParseAddress(s, parseSettings, loader, parseErrors);
        spl.Complete();

        FillAddress(addr);
        addr.AddMessages(parseErrors);
        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddress() finished.");

      return addr;
    }

    private FiasAddress DoOldParseAddress(string s, OldFiasParseSettings parseSettings, PageLoader loader, ErrorMessageList parseErrors)
    {
      // 01.10.2020
      s = s.Replace("--", "-");
      s = s.Replace("- ", "-");
      s = s.Replace(" -", "-");
      s = s.Replace("  ", " ");

      s = s.Trim();
      if (s.Length == 0)
      {
        parseErrors.AddInfo("Пустая строка");
        return new FiasAddress();
      }
      FiasAddress address = parseSettings.BaseAddress.Clone();

      bool useRB = (address.NameBottomLevel == address.GuidBottomLevel);

      string[] a = s.Split(',');
      for (int i = 0; i < a.Length; i++)
        DoOldParseAddressPart(a[i].Trim(), address, parseSettings, loader, parseErrors, ref useRB, i == (a.Length - 1));

      return address;
    }

    private static readonly char[] AuxSepChars = new char[] { ' ', '-' };

    private void DoOldParseAddressPart(string part, FiasAddress address, OldFiasParseSettings parseSettings, PageLoader loader, ErrorMessageList parseErrors, ref bool useRB, bool isLastPart)
    {
      if (part.Length == 0)
        return;

      if (!useRB) // не можем использовать справочник
      {
        OldAddPartToAddress(parseSettings, address, part, null, parseErrors);
        return;
      }


      while (part.Length > 0)
      {
        if (address.GuidBottomLevel == FiasLevel.Street &&
          parseSettings.EditorLevel == FiasEditorLevel.Room &&
          isLastPart &&
          part[0] >= '0' && part[0] <= '9' &&
          part[part.Length - 1] >= '0' && part[part.Length - 1] <= '9' &&
          part.IndexOf('-') > 0 && part.LastIndexOf('-') == part.IndexOf('-'))
        {
          // 02.10.2020
          // Задан номер дома и квартиры в формате "Д-К"
          int p = part.IndexOf('-');

          // Пытаемся добавить дом
          if (OldAddPartToAddress(parseSettings, address, part.Substring(0, p), loader, parseErrors))
          {
            // Удалось. Теперь добавляем квартиру. 
            if (!OldAddPartToAddress(parseSettings, address, part.Substring(p + 1), loader, parseErrors))
            {
              useRB = false;
              OldAddPartToAddress(parseSettings, address, part.Substring(p + 1), null, parseErrors);
            }
            return;
          }
        }

        // Пытаемся найти в справочнике
        if (OldAddPartToAddress(parseSettings, address, part, loader, parseErrors))
          return;

        // 01.10.2020
        // Если в строке есть пробелы, то можно попробовать разобрать строку на части и подобрать самый большой кусок
#if XXX
        string[] a2 = part.Split(' ');
        bool PartFound = false;
        for (int j = a2.Length - 2; j >= 0; j--)
        {
          string s2 = String.Join(" ", a2, 0, j + 1).Trim();
          if (AddPartToAddress(parseSettings, address, s2, loader, parseErrors))
          {
            PartFound = true;
            part = String.Join(" ", a2, j + 1, a2.Length - j - 1).Trim();
          }
        }
        if (!PartFound)
          break;
#endif

        int cnt = part.Length;
        bool PartFound = false;
        while ((cnt = part.LastIndexOfAny(AuxSepChars, cnt - 1, cnt)) > 0)
        {
          string s2 = part.Substring(0, cnt);
          if (OldAddPartToAddress(parseSettings, address, s2, loader, parseErrors))
          {
            part = part.Substring(cnt + 1);
            PartFound = true;
            break;
          }
        }
        if (!PartFound)
          break;
      }

      // Ничего не вышло со справочником
      useRB = false;
      OldAddPartToAddress(parseSettings, address, part, null, parseErrors);
    }

    /// <summary>
    /// Добавить к адресу фрагмент текста
    /// </summary>
    private bool OldAddPartToAddress(OldFiasParseSettings parseSettings, FiasAddress address, string part, PageLoader loader, ErrorMessageList parseErrors)
    {
      part = part.Trim();
      part = part.Replace("  ", " ");
      part = part.Replace(" .", ".");

      if (part.Length == 0)
        return true; // нечего добавлять

      string nm, aoType;

      #region "г.Тюмень"

      int p = part.IndexOf('.');
      if (p > 0 && p < (part.Length - 1) && part.IndexOf(' ', 0, p) < 0)
      {
        // Найдена точка, перед которой нет пробелов.
        // Например, "г. Тюмень", "д.22"
        aoType = part.Substring(0, p + 1); // включая точку
        nm = part.Substring(p + 1);

        if (OldTryAddPartToAddress(parseSettings, address, nm, aoType, loader))
          return true;
      }

      #endregion

      p = part.IndexOf(' ');
      if (p >= 0)
      {
        #region "город Тюмень"

        aoType = part.Substring(0, p);
        nm = part.Substring(p + 1);

        if (OldTryAddPartToAddress(parseSettings, address, nm, aoType, loader))
          return true;

        #endregion

        #region "Тюменский район"

        p = part.LastIndexOf(' '); // всегда больше 0
        nm = part.Substring(0, p);
        aoType = part.Substring(p + 1);


        if (OldTryAddPartToAddress(parseSettings, address, nm, aoType, loader))
          return true;

        #endregion
      }

      #region Без сокращения "Карла Маркса" или "Ленина"

      if (OldTryAddPartToAddress(parseSettings, address, part, String.Empty, loader))
        return true;

      if (loader == null)
      {
        int pLevel = FiasTools.ParseLevelIndexer.IndexOf(address.NameBottomLevel) + 1;
        int pBottomLevel = FiasTools.ParseLevelIndexer.IndexOf(parseSettings.InternalBottomLevel);
        if (pLevel <= pBottomLevel)
        {
          FiasLevel level = FiasTools.ParseLevels[pLevel];
          address.SetName(level, part);
        }
        else
          parseErrors.AddError("Некуда добавить фрагмент текста \"" + part + "\", так как все уровни адреса заполнены");
        return true;
      }

      #endregion

      return false;
    }

    private bool OldTryAddPartToAddress(OldFiasParseSettings parseSettings, FiasAddress address, string name, string aoType, PageLoader loader)
    {
      name = name.Trim();
      aoType = aoType.Trim();

      if (name.Length == 0)
        return false;

      FiasLevel PrevLevel = address.NameBottomLevel;
      int pBottomLevel = FiasTools.ParseLevelIndexer.IndexOf(parseSettings.InternalBottomLevel);
      for (int pLevel = FiasTools.ParseLevelIndexer.IndexOf(PrevLevel) + 1; pLevel <= pBottomLevel; pLevel++)
      {
        FiasLevel level = FiasTools.ParseLevels[pLevel];
        if (!FiasTools.IsInheritableLevel(PrevLevel, level, true /* 01.03.2021 */))
          continue;

        switch (FiasTools.GetTableType(level))
        {
          case FiasTableType.AddrOb:
            Int32 aoTypeId;
            if (aoType.Length > 0)
            {
              string fullAOType;
              if (!AOTypes.IsValidAOType(level, aoType, out fullAOType, out aoTypeId))
                continue; // если сокращение задано, но оно неправильное - не подходит
            }
            else
              aoTypeId = 0;

            // Выполняем поиск
            if (loader != null)
            {
              FiasCachedPageAddrOb page1 = loader.GetAOPage(address.AOGuid, level);
              FiasSearchRowResult searchRes = page1.FindRow(name, aoTypeId, true);
              if (searchRes.Count == FiasSearchRowCount.NotFound)
              {
                // Если для улицы не задано сокращение, то пытаемся использовать "Улица"
                // Это действует, если есть и улица и не-улица с таким названием
                // Например, в Абатский район, с. Ощепково (AOGUID=2e830a89-4f75-4af1-b6bc-639abf8ed050)
                // есть ул.Пушкина (AOGUID=eff533bc-06cd-4809-84ca-d687e86b61f3) и пер.Пушкина (AOGUID=fd1f752a-6716-4884-b164-65b511d6277c).
                if (level == FiasLevel.Street && aoType.Length == 0)
                {
                  aoTypeId = AOTypes.FindAOTypeId(FiasLevel.Street, "Улица");
                  searchRes = page1.FindRow(name, aoTypeId, true);
                }

                if (searchRes.Count != FiasSearchRowCount.Ok)
                  continue;
              }

              _AddrObExtractor.Row = searchRes.Row;
              address.AOGuid = Guid.Empty; // очищаем неуточненный уровень
              address.SetGuid(level, _AddrObExtractor.AOGuid);
              address.SetName(level, _AddrObExtractor.Name);
              address.SetAOType(level, AOTypes.GetAOType(_AddrObExtractor.AOTypeId, FiasAOTypeMode.Full));
              return true;
            }
            break;

          case FiasTableType.House:
            if (loader != null && Source.DBSettings.UseHouse)
            {
              FiasCachedPageHouse page2 = loader.GetHousePage(address.AOGuid);
              DataRow row = page2.FindRow(name, FiasEstateStatus.Unknown, String.Empty, String.Empty, FiasStructureStatus.Unknown);
              if (row == null)
                return false;

              _HouseExtractor.Row = row;
              address.AOGuid = Guid.Empty; // очищаем неуточненный уровень
              address.SetGuid(FiasLevel.House, _HouseExtractor.HouseGuid);
              address.SetName(FiasLevel.House, _HouseExtractor.HouseNum);
              address.SetAOType(FiasLevel.House, FiasEnumNames.ToString(_HouseExtractor.EstStatus));
              return true;
            }
            break;

        } // switch(GetTableType)

        if (aoType.Length > 0)
        {
          if (!AOTypes.IsValidAOType(level, aoType))
            continue;
        }

        address.SetName(level, name);
        if (aoType.Length > 0)
          address.SetAOType(level, aoType);
        else
          address.SetAOType(level, FiasTools.GetDefaultAOType(level));
        return true;
      }
      return false;
    }

    #endregion

    #region Получение адресов из текста с разбиением на колонки

    /// <summary>
    /// Парсинг нескольких строк в адреса.
    /// Предполагается, что компоненты адреса разделены запятыми.
    /// Для пустых строк возвращаются пустые адреса (FiasAddress.IsEmpty=true).
    /// Ошибки парсинга записываются в списки FiasAddress.Messages. 
    /// </summary>
    /// <param name="textMatrix">Ячейки адреса. Каждой строке соответствует один адрес, а число столбцов должно быть равно <paramref name="parseSettings"/>.CellLevels.Length.</param>
    /// <param name="parseSettings">Параметры парсинга</param>
    /// <returns>Массив адресов. Длина массива соответствует количеству строк в <paramref name="textMatrix"/>.</returns>
    public FiasAddress[] ParseAddresses(string[,] textMatrix, FiasParseSettings parseSettings)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddresses() started. Cells.Length=" + textMatrix.GetLength(0).ToString());

      FiasAddress[] a = new FiasAddress[textMatrix.GetLength(0)];

      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "Построение списка GUIDов",
       "Загрузка страниц",
       a.Length==1 ? "Распознание адреса" : "Распознание адресов ("+a.Length.ToString()+")",
       "Проверка"
      });
      try
      {
        FillAddress(parseSettings.BaseAddress);

        PageLoader loader = new PageLoader(_Source);
        loader.AddGuids(parseSettings.BaseAddress);
        spl.Complete();

        loader.Load(spl);
        spl.Complete();

        spl.PercentMax = a.Length;
        spl.AllowCancel = true;

        for (int i = 0; i < a.Length; i++)
        {
          a[i] = DoParseAddress(DataTools.GetArray2Row<string>(textMatrix, i), parseSettings, loader);
          spl.IncPercent();
        }
        spl.Complete();

        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddresses() finished.");

      return a;
    }

    /// <summary>
    /// Парсинг строки в адрес.
    /// Предполагается, что компоненты адреса разделены запятыми.
    /// Для пустой строки возвращается пустой адрес (FiasAddress.IsEmpty=true).
    /// Ошибки парсинга записываются в списки FiasAddress.Messages. 
    /// </summary>
    /// <param name="cells">Строка для парсинга</param>
    /// <param name="parseSettings">Параметры парсинга</param>
    /// <returns>Адрес</returns>
    public FiasAddress ParseAddress(string[] cells, FiasParseSettings parseSettings)
    {
      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddress() started.");

      FiasAddress addr;
      ISplash spl = SplashTools.ThreadSplashStack.BeginSplash(new string[]{
       "Построение списка GUIDов",
       "Загрузка страниц",
       "Распознание адреса",
       "Проверка",
      });
      try
      {
        FillAddress(parseSettings.BaseAddress);


        PageLoader loader = new PageLoader(_Source);
        loader.AddGuids(parseSettings.BaseAddress);
        spl.Complete();

        loader.Load(spl);
        spl.Complete();

        addr = DoParseAddress(cells, parseSettings, loader);
        spl.Complete();

        spl.Complete();
      }
      finally
      {
        SplashTools.ThreadSplashStack.EndSplash();
      }

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.ParseAddress() finished.");

      return addr;
    }

    private FiasAddress DoParseAddress(string[] cellStrings, FiasParseSettings parseSettings, PageLoader loader)
    {
      FiasParseHelper helper = new FiasParseHelper();
      helper.Handler = this;
      helper.ParseSettings = parseSettings;
      helper.CellStrings = (string[])(cellStrings.Clone());

      return helper.Parse();
    }

    #endregion

    #region Список регионов

    private Dictionary<string, Guid> _RegionCodeGuids;

    private void PrepareRegionCodeGuids()
    {
      if (_RegionCodeGuids != null)
        return;

      FiasCachedPageAddrOb page = Source.GetAddrObPages(FiasLevel.Region, new Guid[1] { Guid.Empty })[Guid.Empty];

      using (DataView dv = page.CreateDataView())
      {
        Dictionary<string, Guid> dict = new Dictionary<string, Guid>(dv.Count);
        foreach (DataRowView drv in dv)
        {
          Guid g = DataTools.GetGuid(drv.Row, "AOGUID");
          int nRegCode = DataTools.GetInt(drv.Row, "REGIONCODE");
          dict[nRegCode.ToString("00")] = g;
        }
        _RegionCodeGuids = dict;
      }
    }

    /// <summary>
    /// Возвращает GUID адресного элемента верхнего уровня по коду региона.
    /// Если задан код региона
    /// </summary>
    /// <param name="regionCode">Код региона "01"-"99"</param>
    /// <returns></returns>
    public Guid GetRegionAOGuid(string regionCode)
    {
      if (String.IsNullOrEmpty(regionCode))
        return Guid.Empty;

      PrepareRegionCodeGuids();
      Guid g;
      _RegionCodeGuids.TryGetValue(regionCode, out g);
      return g;
    }

    /// <summary>
    /// Возвращает true, если в загруженном справочнике есть регион с заданным кодом
    /// </summary>
    /// <param name="regionCode">Проверяемый код региона</param>
    /// <returns>Наличие региона в ФИАС</returns>
    public bool RegionCodeExists(string regionCode)
    {
      if (String.IsNullOrEmpty(regionCode))
        return false;
      PrepareRegionCodeGuids();
      return _RegionCodeGuids.ContainsKey(regionCode);
    }


    /// <summary>
    /// Список кодов регионов "01"-"99" которые есть в загруженном справочнике.
    /// Массив является осортированным.
    /// </summary>
    public string[] RegionCodes
    {
      get
      {
        if (_RegionCodes == null)
        {
          PrepareRegionCodeGuids();
          string[] a = new string[_RegionCodeGuids.Count];
          _RegionCodeGuids.Keys.CopyTo(a, 0);
          Array.Sort<string>(a);
          _RegionCodes = a;
        }
        return _RegionCodes;
      }
    }
    private string[] _RegionCodes;

    #endregion

    #region Вспомогательные информационные методы

    /// <summary>
    /// Возвращает уровень адресного объекта для заданного AOGUID
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public FiasLevel GetAOGuidLevel(Guid guid)
    {
      if (guid == Guid.Empty)
        return FiasLevel.Unknown;
      if (guid == FiasTools.GuidNotFound)
        throw new ArgumentException("Неправильный guid=" + guid.ToString(), "guid");

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.GetAOGuidLevel() started.");

      FiasLevel level = Source.GetGuidInfo(new Guid[1] { guid }, FiasTableType.AddrOb)[guid].Level;

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.GetAOGuidLevel() finsihed.");

      return level;
    }

    /// <summary>
    /// Возвращает уровень адресного объекта для заданного идентификатора записи.
    /// </summary>
    /// <param name="recId">Идентификатор записи</param>
    /// <returns></returns>
    public FiasLevel GetAORecIdLevel(Guid recId)
    {
      if (recId == Guid.Empty)
        return FiasLevel.Unknown;

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.GetAORecIdLevel() started.");

      FiasLevel level = Source.GetRecIdInfo(new Guid[1] { recId }, FiasTableType.AddrOb)[recId].Level;

      if (FiasTools.TraceSwitch.Enabled)
        Trace.WriteLine(FiasTools.GetTracePrefix() + "FiasHandler.GetAORecIdLevel() finished.");

      return level;
    }

    /// <summary>
    /// Дата актуальности классификатора
    /// </summary>
    public DateTime ActualDate { get { return _Source.ActualDate; } }

    /// <summary>
    /// Возвращает статистику по базе данных классификатора.
    /// </summary>
    public FiasDBStat DBStat { get { return _Source.DBStat; } }

    #endregion
  }
}
