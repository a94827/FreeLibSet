using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data.Docs;

namespace ExtDBDocs_tests.Data_Docs
{
  [TestFixture]
  public class DBxDocProviderTests_TimeOut : DBxDocSetTestsBase
  {
    #region Ошибка тайм-аута при записи поддокументов

    // 03.09.2019
    // Сценарий, приводящий к записи неправильного документа в случае тайм-аута: 
    // 1. Открывается на редактирование документ.
    // 2. Добавляется поддокумент.
    // 3. В редакторе нажимается кнопка «Запись». Вызывается DBxDocProvider.ApplyChanges() на стороне сервера.
    // 4. Возникает ошибка тайм-аута, если сервер занят или временно оборван канал связи. 
    //    Исключение передается клиенту, хотя DBxRealDocProvider.ApplyChanges() продолжает работать.
    // 5. DBxRealDocProvider.ApplyChanges() добавляет поддокумент, но результат не передается клиенту.
    // 6. Пользователь повторно нажимает «Запись». DBxDocProvider.ApplyChanges() вызывается еще раз. 
    //    Поддокумент добавляется еще раз, так как клиент передает строку в состоянии «новая». 
    //    Даже если при записи выполняется проверка корректности документа на наличие повторов, она не сработает, так как документ не перечитывается непосредственно перед записью. 
    // В результате, документ будет содержать два одинаковых поддокумента. 

    /// <summary>
    /// Эмуляция выброса исключения после выполнения ApplyChanges().
    /// Не имеет большого значения, что в реальном сценарии исключение выбрасывается до завершения ApplyChanges()
    /// </summary>
    private class TestRealDocProvider : DBxRealDocProvider
    {
      #region Конструктор

      public TestRealDocProvider(DBxRealDocProviderSource source)
        : base(source, 0, false)
      {
      }

      #endregion

      #region Переопределенный метод

      public bool ThrowException;

      protected override System.Data.DataSet OnApplyChanges(System.Data.DataSet dataSet, bool reloadData)
      {
        System.Data.DataSet res = base.OnApplyChanges(dataSet, reloadData);
        if (ThrowException)
          throw new TimeoutException();
        else
          return res;
      }

      #endregion
    }

    [Test]
    public void ApplyChanges_TimeOut_AddSubDoc([Values(false, true)] bool useDeleted, [Values(false, true)] bool editMode)
    {
      TestDBInfo info = this[useDeleted, false, false];

      // Провайдер, выбрасывающий исключение
      TestRealDocProvider provider = new TestRealDocProvider(info.Source);

      DBxDocSet docSet = new DBxDocSet(provider);
      DBxSingleDoc doc = docSet["D1"].Insert();
      doc.Values["F101"].SetBoolean(false);
      doc.Values["F102"].SetInteger(1);
      DBxSubDoc sd = doc.SubDocs["SD11"].Insert();
      sd.Values["F111"].SetString("ABC");
      sd.Values["F112"].SetString("XYZ");
      Int32 docId;
      if (editMode)
      {
        docSet.ApplyChanges(true);
        docId = docSet["D1"][0].DocId;
        docSet = new DBxDocSet(provider);
        doc = docSet["D1"].Edit(docId);

        sd = doc.SubDocs["SD11"][0];
        sd.Values["F111"].SetString("DEF"); // обязательно нужно что-нибудь поменять, чтобы ApplyChanges() выполнил запись
      }

      provider.ThrowException = true;
      Assert.Catch<TimeoutException>(delegate() { docSet.ApplyChanges(true); }, "timeout exception");

      provider.ThrowException = false;
      Assert.DoesNotThrow(delegate() { docSet.ApplyChanges(true); }, "no exception");

      docId = docSet["D1"][0].DocId;

      docSet = new DBxDocSet(info.Provider);
      doc = docSet["D1"].View(docId);

      Assert.AreEqual(1, doc.SubDocs["SD11"].SubDocCount, "SubDocCount");
      sd = doc.SubDocs["SD11"][0];
      Assert.AreEqual("XYZ", sd.Values["F112"].AsString, "F112");
    }

    #endregion
  }
}
