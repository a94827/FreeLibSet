using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Russian;
using FreeLibSet.Tests;

namespace ExtRussian_tests
{
  [TestFixture]
  public class RusFullNameTests
  {
    #region Конструктор

    [TestCase("Иванов", "Иван", "Иванович")]
    [TestCase("Петров", "Пётр", "")]
    [TestCase("Сидоров", "С", "С")]
    [TestCase("Иванова", "", "")]
    [TestCase("", "", "")]
    public void Constructor(string surname, string name, string patronymic)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      Assert.AreEqual(surname, sut.Surname, "Surname");
      Assert.AreEqual(name, sut.Name, "Name");
      Assert.AreEqual(patronymic, sut.Patronymic, "Patronymic");
      Assert.AreEqual(surname, sut[RusFullNamePart.Surname], "this[Surname]");
      Assert.AreEqual(name, sut[RusFullNamePart.Name], "this[Name]");
      Assert.AreEqual(patronymic, sut[RusFullNamePart.Patronymic], "this[Patronymic]");
    }

    #endregion

    #region Свойства

    [TestCase("Иванов", "Иван", "Иванович", "Иванов Иван Иванович")]
    [TestCase("Иванов", "Иван", "", "Иванов Иван")]
    [TestCase("Иванов", "", "", "Иванов")]
    [TestCase("", "", "", "")]
    public void FullName(string surname, string name, string patronymic, string wantedRes)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      Assert.AreEqual(wantedRes, sut.FullName);
    }

    [TestCase("Петров-Водкин", "Кузьма", "Сергеевич", "Петров-Водкин К.С.")]
    [TestCase("Иванов", "И", "И", "Иванов И.И.")]
    [TestCase("Иванов", "Иван", "", "Иванов И.")]
    [TestCase("Иванов", "И", "", "Иванов И.")]
    [TestCase("Иванов", "", "", "Иванов")]
    [TestCase("", "", "", "")]
    public void NameWithInitials(string surname, string name, string patronymic, string wantedRes)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      Assert.AreEqual(wantedRes, sut.NameWithInitials);
    }


    [TestCase("Петров-Водкин", "Кузьма", "Сергеевич", "Петров-Водкин", "К", "С")]
    [TestCase("Иванов", "И", "И", "Иванов", "И", "И")]
    [TestCase("Иванов", "Иван", "", "Иванов", "И", "")]
    [TestCase("Иванов", "И", "", "Иванов", "И", "")]
    [TestCase("Иванов", "", "", "Иванов", "", "")]
    [TestCase("", "", "", "", "", "")]
    public void NameWithInitialsObj(string surname, string name, string patronymic, string wantedSurname, string wantedName, string wantedPatronymic)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      RusFullName res = sut.NameWithInitalsObj;

      DoCompare(res, wantedSurname, wantedName, wantedPatronymic, "");
    }

    private static void DoCompare(RusFullName res, string wantedSurname, string wantedName, string wantedPatronymic, string messagePrefix)
    {
      if (messagePrefix.Length > 0)
        messagePrefix += " - ";

      Assert.AreEqual(wantedSurname, res.Surname, messagePrefix + "Surname");
      Assert.AreEqual(wantedName, res.Name, messagePrefix + "Name");
      Assert.AreEqual(wantedPatronymic, res.Patronymic, messagePrefix + "Patronymic");
    }

    [TestCase("Петров-Водкин", "Кузьма", "Сергеевич", "К.С.Петров-Водкин")]
    [TestCase("Иванов", "И", "И", "И.И.Иванов")]
    [TestCase("Иванов", "Иван", "", "И.Иванов")]
    [TestCase("Иванов", "И", "", "И.Иванов")]
    [TestCase("Иванов", "", "", "Иванов")]
    [TestCase("", "", "", "")]
    public void InvNameWithInitials(string surname, string name, string patronymic, string wantedRes)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      Assert.AreEqual(wantedRes, sut.InvNameWithInitials);
    }

    [TestCase("Иванов", "Иван", "Иванович", false)]
    [TestCase("Иванов", "И", "И", true)]
    [TestCase("Иванов", "Иван", "", false)]
    [TestCase("Иванов", "И", "", true)]
    [TestCase("Иванов", "", "", false)]
    [TestCase("", "", "", false)]
    public void IsInitials(string surname, string name, string patronymic, bool wantedRes)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      Assert.AreEqual(wantedRes, sut.IsInitials);
    }

    [TestCase("Иванов", "Иван", "Иванович", false)]
    [TestCase("Иванов", "Иван", "", false)]
    [TestCase("Иванов", "", "", false)]
    [TestCase("", "", "", true)]
    public void IsEmpty(string surname, string name, string patronymic, bool wantedRes)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      Assert.AreEqual(wantedRes, sut.IsEmpty);
    }

    [Test]
    public void Empty()
    {
      Assert.AreEqual("", RusFullName.Empty.Surname, "Surname");
      Assert.AreEqual("", RusFullName.Empty.Name, "Name");
      Assert.AreEqual("", RusFullName.Empty.Patronymic, "Patronymic");
      Assert.IsTrue(RusFullName.Empty.IsEmpty, "IsEmpty");
      Assert.AreEqual(SexOfPerson.Undefined, RusFullName.Empty.Sex, "Sex");
    }

    #endregion

    #region Парсинг

    [TestCase("Иванов Иван Иванович", true, "Иванов", "Иван", "Иванович")]
    [TestCase("Иванов Иван", true, "Иванов", "Иван", "")]
    // одной фамилии недостаточно [TestCase("Иванов", true, "Иванов", "", "")]
    [TestCase("Иванов", false, "Иванов", "", "")]
    [TestCase("", true, "", "", "")]
    [TestCase("", false, "", "", "")]
    [TestCase("Петров-Водкин Кузьма Сергеевич", true, "Петров-Водкин", "Кузьма", "Сергеевич")]
    [TestCase("Мамедов Полад Муртуза оглы", true, "Мамедов", "Полад", "Муртуза Оглы")]
    [TestCase("Мамедова Аделаида Рза кызы", true, "Мамедова", "Аделаида", "Рза Кызы")]
    [TestCase("Яхъяева Севда Эльдар гызы", true, "Яхъяева", "Севда", "Эльдар Гызы")]
    public void Parse_TryParse_true(string s, bool autoCorrect, string wantedSurname, string wantedName, string wantedPatronymic)
    {
      RusFullName res1 = RusFullName.Parse(s, autoCorrect);
      DoCompare(res1, wantedSurname, wantedName, wantedPatronymic, "Parse 2 args");

      if (autoCorrect)
      {
        RusFullName res2 = RusFullName.Parse(s);
        DoCompare(res2, wantedSurname, wantedName, wantedPatronymic, "Parse 1 arg");
      }

      RusFullName res3;
      string errorText;
      Assert.IsTrue(RusFullName.TryParse(s, out res3, out errorText, autoCorrect), "TryParse() result");
      DoCompare(res3, wantedSurname, wantedName, wantedPatronymic, "TryParse()");
    }

    [TestCase("666", true)]
    [TestCase("Иванов 1 2", true)]
    // Парсинг строки выполняется [TestCase("Иванов 1 2", false)]
    public void Parse_TryParse_false(string s, bool autoCorrect)
    {
      Assert.Catch<InvalidCastException>(delegate () { RusFullName.Parse(s, autoCorrect); }, "Parse 2 args");


      if (autoCorrect)
      {
        Assert.Catch<InvalidCastException>(delegate () { RusFullName.Parse(s); }, "Parse 1 arg");
      }

      RusFullName res3;
      string errorText;
      Assert.IsFalse(RusFullName.TryParse(s, out res3, out errorText, autoCorrect), "TryParse() result");
      ExtStringAssert.IsNotNullOrEmpty(errorText, "TryParse errorText");
    }

    #endregion

    #region Нормализация

    [TestCase("ПЕТРОВ ВОДКИН", "кузьма", "сЕрГеЕвИч", "Петров-Водкин", "Кузьма", "Сергеевич")]
    [TestCase("иванов", "и.", "и.", "Иванов", "И", "И")]
    public void Normalized(string surname, string name, string patronymic, string wantedSurname, string wantedName, string wantedPatronymic)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      RusFullName res = sut.Normalized;

      DoCompare(res, wantedSurname, wantedName, wantedPatronymic, "");
    }

    [TestCase("ПЕТРОВ ВОДКИН", "кузьма", "сЕрГеЕвИч", "Петров Водкин", "Кузьма", "Сергеевич")]
    [TestCase("иванов", "и.", "и.", "Иванов", "И.", "И.")]
    public void NormalCases(string surname, string name, string patronymic, string wantedSurname, string wantedName, string wantedPatronymic)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      RusFullName res = sut.NormalCased;

      DoCompare(res, wantedSurname, wantedName, wantedPatronymic, "");
    }

    [TestCase("Достоевский", "Фёдор", "Михайлович", "ДОСТОЕВСКИЙ", "ФЁДОР", "МИХАЙЛОВИЧ")]
    [TestCase("петров водкин", "кузьма", "сЕрГеЕвИч", "ПЕТРОВ ВОДКИН", "КУЗЬМА", "СЕРГЕЕВИЧ")]
    public void UpperCased(string surname, string name, string patronymic, string wantedSurname, string wantedName, string wantedPatronymic)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      RusFullName res = sut.UpperCased;

      DoCompare(res, wantedSurname, wantedName, wantedPatronymic, "");
    }

    [TestCase("Фёдоров", "Фёдор", "Фёдорович", "Федоров", "Федор", "Федорович")]
    [TestCase("ФЁДОРОВ", "ФЁДОР", "ФЁДОРОВИЧ", "ФЕДОРОВ", "ФЕДОР", "ФЕДОРОВИЧ")]
    public void YoReplaced(string surname, string name, string patronymic, string wantedSurname, string wantedName, string wantedPatronymic)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      RusFullName res = sut.YoReplaced;

      DoCompare(res, wantedSurname, wantedName, wantedPatronymic, "");
    }

    #endregion

    #region Замена частей

    [Test]
    public void SetSurname()
    {
      RusFullName sut = new RusFullName("Иванов", "Иван", "Иванович");
      RusFullName res = sut.SetSurname("Петров");

      DoCompare(res, "Петров", "Иван", "Иванович", "");
    }

    [Test]
    public void SetName()
    {
      RusFullName sut = new RusFullName("Иванов", "Иван", "Иванович");
      RusFullName res = sut.SetName("Петр");

      DoCompare(res, "Иванов", "Петр", "Иванович", "");
    }

    [Test]
    public void SetPatronymic()
    {
      RusFullName sut = new RusFullName("Иванов", "Иван", "Иванович");
      RusFullName res = sut.SetPatronymic("Петрович");

      DoCompare(res, "Иванов", "Иван", "Петрович", "");
    }

    #endregion

    #region Склонение ФИО

    [TestCase(
      "Иванов,Иван,Иванович|" +
      "Иванова,Ивана,Ивановича|" +
      "Иванову,Ивану,Ивановичу|" +
      "Иванова,Ивана,Ивановича|" +
      "Ивановым,Иваном,Ивановичем|" +
      "Иванове,Иване,Ивановиче", SexOfPerson.Undefined, true)]
    [TestCase(
      "Иванова,Ирина,Илинична|" +
      "Ивановой,Ирины,Илиничны|" +
      "Ивановой,Ирине,Илиничне|" +
      "Иванову,Ирину,Илиничну|" +
      "Ивановой,Ириной,Илиничной|" +
      "Ивановой,Ирине,Илиничне", SexOfPerson.Undefined, true)]
    [TestCase(
      "Петров-Водкин,Кузьма,Сергеевич|" +
      "Петрова-Водкина,Кузьмы,Сергеевича|" +
      "Петрову-Водкину,Кузьме,Сергеевичу|" +
      "Петрова-Водкина,Кузьму,Сергеевича|" +
      "Петровым-Водкиным,Кузьмой,Сергеевичем|" +
      "Петрове-Водкине,Кузьме,Сергеевиче", SexOfPerson.Undefined, true)]
    [TestCase(
      "Стругацкий,Борис,Натанович|" +
      "Стругацкого,Бориса,Натановича|" +
      "Стругацкому,Борису,Натановичу|" +
      "Стругацкого,Бориса,Натановича|" +
      "Стругацким,Борисом,Натановичем|" +
      "Стругацком,Борисе,Натановиче", SexOfPerson.Undefined, true)]
    [TestCase(
      "Линкольн,Авраам,|" +
      "Линкольна,Авраама,|" +
      "Линкольну,Аврааму,|" +
      "Линкольна,Авраама,|" +
      "Линкольном,Авраамом,|" +
      "Линкольне,Аврааме,", SexOfPerson.Male, true)]
    [TestCase(
      "Жердер,Римма,Александровна|" +
      "Жердер,Риммы,Александровны|" +
      "Жердер,Римме,Александровне|" +
      "Жердер,Римму,Александровну|" +
      "Жердер,Риммой,Александровной|" +
      "Жердер,Римме,Александровне", SexOfPerson.Undefined, true)]
    [TestCase(
      "Мечников,Илья,Ильич|" +
      "Мечникова,Ильи,Ильича|" +
      "Мечникову,Илье,Ильичу|" +
      "Мечникова,Илью,Ильича|" +
      "Мечниковым,Ильей,Ильичем|" +
      "Мечникове,Илье,Ильиче", SexOfPerson.Undefined, true)]
    [TestCase(
      "Пастер,Луи,|" +
      "Пастера,Луи,|" +
      "Пастеру,Луи,|" +
      "Пастера,Луи,|" +
      "Пастером,Луи,|" +
      "Пастере,Луи,", SexOfPerson.Male, true)]
    [TestCase(
      "Пастер,Мари,|" +
      "Пастер,Мари,|" +
      "Пастер,Мари,|" +
      "Пастер,Мари,|" +
      "Пастер,Мари,|" +
      "Пастер,Мари,", SexOfPerson.Female, true)]
    [TestCase(
      "Бойко,А,Н|" +
      "Бойко,А,Н|" +
      "Бойко,А,Н|" +
      "Бойко,А,Н|" +
      "Бойко,А,Н|" +
      "Бойко,А,Н", SexOfPerson.Male, true)]
    [TestCase(
      "Бойко,А,Н|" +
      "Бойко,А,Н|" +
      "Бойко,А,Н|" +
      "Бойко,А,Н|" +
      "Бойко,А,Н|" +
      "Бойко,А,Н", SexOfPerson.Female, true)]
    public void GetCases(string sForms, SexOfPerson sex, bool wantedRes)
    {
      RusFullName[] wantedCases = GetTestCases(sForms);
      RusFullName sut = wantedCases[0];

      RusFullName[] cases1;
      bool res1 = sut.GetCases(sex, out cases1);
      Assert.AreEqual(12, cases1.Length, "Length #1");
      Assert.AreEqual(wantedRes, res1, "Result #1");
      for (int i = 0; i < wantedCases.Length; i++)
        DoCompare(cases1[i], wantedCases[i].Surname, wantedCases[i].Name, wantedCases[i].Patronymic, "#1 - " + RussianTools.CaseNames4[i]);

      RusFullName[] cases2 = sut.GetCases(sex);
      Assert.AreEqual(12, cases2.Length, "Length #2");
      for (int i = 0; i < wantedCases.Length; i++)
        DoCompare(cases2[i], wantedCases[i].Surname, wantedCases[i].Name, wantedCases[i].Patronymic, "#2 - " + RussianTools.CaseNames4[i]);
    }

    private static RusFullName[] GetTestCases(string sForms)
    {
      // Добавляем в текст исключения исходную строку, т.к. NUnit не выводит кириллицу в аргументов тестов

      string[] aForms1 = sForms.Split('|');
      if (aForms1.Length != 6)
        throw new ArgumentException("Неправильное количество падежей: " + aForms1.Length.ToString() + " (" + sForms + ")");

      RusFullName[] wantedCases = new RusFullName[aForms1.Length];
      for (int i = 0; i < aForms1.Length; i++)
      {
        string[] a2 = aForms1[i].Split(',');
        if (a2.Length != 3)
          throw new ArgumentException("Неправильное количество частей: " + a2.Length.ToString() + " (" + aForms1[i] + ")");
        wantedCases[i] = new RusFullName(a2[0], a2[1], a2[2]);
      }
      return wantedCases;
    }

    #endregion

    #region Пол

    [TestCase("Иванов", "Иван", "Иванович", SexOfPerson.Male)]
    [TestCase("Иванова", "Ирина", "Ивановна", SexOfPerson.Female)]
    [TestCase("Иванова", "Ирина", "Ильинична", SexOfPerson.Female)]
    [TestCase("", "", "", SexOfPerson.Undefined)]
    [TestCase("Мамедов", "Полад", "Муртуза оглы", SexOfPerson.Male)]
    [TestCase("Мамедова", "Аделаида", "Рза кызы", SexOfPerson.Female)]
    [TestCase("Яхъяева", "Севда", "Эльдар гызы", SexOfPerson.Female)]
    [TestCase("Гилагаев", "Хас-Магомед", "Саид-Магомедович", SexOfPerson.Male)]
    public void Sex(string surname, string name, string patronymic, SexOfPerson wantedRes)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      Assert.AreEqual(wantedRes, sut.Sex);
    }

    #endregion

    #region Сравнение

    [TestCase("Фёдоров", "Фёдор", "Фёдорович", "ФЕДОРОВ", "ФЕДОР", "ФЕДОРОВИЧ", true)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "Иван", "И", true)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "Иван", "", true)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "", "", true)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "И", "И", true)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "И", "", true)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "", "", true)]

    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "Иван", "П", false)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "И", "П", false)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "П", "И", false)]
    [TestCase("Иванов", "Иван", "Иванович", "Иванов", "П", "", false)]
    [TestCase("Иванов", "Иван", "Иванович", "Петров", "", "", false)]
    [TestCase("Иванов", "Иван", "Иванович", "", "", "", false)]

    [TestCase("Иванов", "Иван", "", "Иванов", "Иван", "", true)]
    [TestCase("Иванов", "Иван", "", "Иванов", "И", "", true)]
    [TestCase("Иванов", "Иван", "", "Иванов", "", "", true)]

    [TestCase("Иванов", "Иван", "", "Иванов", "П", "", false)]
    [TestCase("Иванов", "Иван", "", "Петров", "", "", false)]
    [TestCase("Иванов", "Иван", "", "", "", "", false)]

    [TestCase("Иванов", "", "", "Иванов", "", "", true)]
    [TestCase("Иванов", "", "", "Петров", "", "", false)]
    [TestCase("Иванов", "", "", "", "", "", false)]

    public void Equals(string surname1, string name1, string patronymic1, string surname2, string name2, string patronymic2, bool wantedRes)
    {
      RusFullName sut1 = new RusFullName(surname1, name1, patronymic1);
      RusFullName sut2 = new RusFullName(surname2, name2, patronymic2);

      DoEquals(sut1, sut2, wantedRes);
      DoEquals(sut2, sut1, wantedRes);
    }

    private static void DoEquals(RusFullName sut1, RusFullName sut2, bool wantedRes)
    {
      bool res1 = (sut1 == sut2);
      Assert.AreEqual(wantedRes, res1, "Operator ==");

      bool res2 = (sut1 != sut2);
      Assert.AreEqual(!wantedRes, res2, "Operator !=");

      bool res3 = sut1.Equals(sut2);
      Assert.AreEqual(wantedRes, res3, "Equals(RusFullName)");

      bool res4 = sut1.Equals((object)sut2);
      Assert.AreEqual(wantedRes, res4, "Equals(Object)");
    }

    #endregion

    #region Проверка корректности

    [TestCase("Петров-Водкин", "Кузьма", "Сергеевич", RusFullNameValidateOptions.Default, true)]
    [TestCase("Фёдоров", "Фёдор", "Фёдорович", RusFullNameValidateOptions.InitialsAllowed, true)]
    [TestCase("Иванов", "И", "И", RusFullNameValidateOptions.Default, false, Description = "Initials are not allowed by default")]
    [TestCase("Иванов", "И", "И", RusFullNameValidateOptions.InitialsAllowed, true)]
    [TestCase("Иванов", "И", "И", RusFullNameValidateOptions.InitialsOnly, true)]
    [TestCase("Иванов", "Иван", "Иванович", RusFullNameValidateOptions.InitialsOnly, false, Description = "Initials required")]
    [TestCase("Иванов", "Иван", "", RusFullNameValidateOptions.Default, true, Description = "Patronymic is always optional")]
    [TestCase("Иванов", "", "", RusFullNameValidateOptions.Default, false, Description = "Name required")]
    [TestCase("", "", "", RusFullNameValidateOptions.Default, false, Description = "Empty object is not valid")]
    [TestCase("Иванов", "Иван", "И", RusFullNameValidateOptions.InitialsAllowed, false, Description = "Initials and full-form mixed")]
    [TestCase("Иванов", "И", "Иванович", RusFullNameValidateOptions.InitialsAllowed, false, Description = "Initials and full-form mixed")]
    [TestCase("Иванов", "", "Иванович", RusFullNameValidateOptions.InitialsAllowed, false, Description = "Name missed")]
    [TestCase("", "Иван", "Иванович", RusFullNameValidateOptions.InitialsAllowed, false, Description = "Surname missed")]
    [TestCase("Петров Водкин", "Кузьма", "Сергеевич", RusFullNameValidateOptions.Default, false, Description = "Space not allowed in surname")]
    [TestCase("Гилагаев", "Хас-Магомед", "Саид-Магомедович", RusFullNameValidateOptions.Default, true)]
    [TestCase("Мамедов", "Полад", "Муртуза оглы", RusFullNameValidateOptions.Default, true)]
    [TestCase("Мамедова", "Аделаида", "Рза кызы", RusFullNameValidateOptions.Default, true)]
    [TestCase("ИВАНОВ", "ИВАН", "ИВАНОВИЧ", RusFullNameValidateOptions.Default, false, Description = "Case mismatch")]
    [TestCase("Петров-водкин", "Кузьма", "Сергеевич", RusFullNameValidateOptions.Default, true, Description = "Case mismatch in surname part")]
    [TestCase("иВаНоВ", "иВаН", "иВаНоВиЧ", RusFullNameValidateOptions.IgnoreCases, true, Description = "Case ignored")]
    [TestCase("Иванов", "Иван", "123", RusFullNameValidateOptions.Default, false, Description = "Bad chars")]
    [TestCase("Мамедова", "Аделаида", "Рза  кызы", RusFullNameValidateOptions.Default, false, Description = "Double space")]
    [TestCase("Петров--Водкин", "Кузьма", "Сергеевич", RusFullNameValidateOptions.Default, false, Description = "Double --")]
    [TestCase(" Иванов", "Иван", "Иванович", RusFullNameValidateOptions.Default, false, Description = "Starts with space")]
    [TestCase("Иванов ", "Иван", "Иванович", RusFullNameValidateOptions.Default, false, Description = "Ends with space")]
    [TestCase("-Иванов", "Иван", "Иванович", RusFullNameValidateOptions.Default, false, Description = "Starts with -")]
    [TestCase("Иванов-", "Иван", "Иванович", RusFullNameValidateOptions.Default, false, Description = "Ends with -")]
    [TestCase("Иванов", "И.", "И", RusFullNameValidateOptions.InitialsAllowed, false, Description = "Dots not allowed")]
    public void IsValid(string surname, string name, string patronymic, RusFullNameValidateOptions options, bool wantedRes)
    {
      RusFullName sut = new RusFullName(surname, name, patronymic);
      string errorText;
      bool res = sut.IsValid(options, out errorText);
      Assert.AreEqual(wantedRes, res, "Result");
      Assert.AreEqual(wantedRes, String.IsNullOrEmpty(errorText), "ErrorText");
    }

    #endregion
  }
}
