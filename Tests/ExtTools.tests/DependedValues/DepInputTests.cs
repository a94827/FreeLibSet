using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DependedValues;

namespace ExtTools_tests.DependedValues
{
  [TestFixture]
  class DepInputTests
  {
    #region Основные тесты

    [Test]
    public void Constructor()
    {
      DepInput<int> sut = new DepInput<int>(1, Dummy_ValueChanged);
      Assert.AreEqual(1, sut.Value, "Value");
      Assert.IsFalse(sut.InsideSetValue, "InsideSetValue");
      Assert.IsFalse(sut.HasValueChanged, "HasValueChanged");
      Assert.IsFalse(sut.HasSource, "HasSource");
      Assert.IsNull(sut.Source, "Source");
      Assert.IsFalse(sut.HasOutputs, "HasOutputs");
      Assert.IsFalse(sut.IsConnected, "IsConnected");
      Assert.IsFalse(sut.IsConst, "IsConst");
    }

    static void Dummy_ValueChanged(object sender, EventArgs args)
    {
    }

    [Test]
    public void Output_added()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      DepInput<int> dummy = new DepInput<int>(0, null);
      dummy.Source = sut;

      Assert.AreEqual(1, sut.Value, "Value"); // не должна испортиться
      Assert.IsFalse(sut.InsideSetValue, "InsideSetValue");
      Assert.IsFalse(sut.HasValueChanged, "HasValueChanged");
      Assert.IsFalse(sut.HasSource, "HasSource");
      Assert.IsTrue(sut.HasOutputs, "HasOutputs");
      Assert.IsTrue(sut.IsConnected, "IsConnected");
    }

    [Test]
    public void Source_added()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      DepInput<int> src = new DepInput<int>(2, null);
      sut.Source = src;

      Assert.AreEqual(2, sut.Value, "Value");
      Assert.IsFalse(sut.InsideSetValue, "InsideSetValue");
      Assert.IsFalse(sut.HasValueChanged, "HasValueChanged");
      Assert.IsTrue(sut.HasSource, "HasSource");
      Assert.IsFalse(sut.HasOutputs, "HasOutputs");
      Assert.IsTrue(sut.IsConnected, "IsConnected");
    }

    [Test]
    public void Source_removed()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      DepInput<int> src = new DepInput<int>(2, null);
      sut.Source = src;
      // теперь отключаем
      sut.Source = null;

      Assert.AreEqual(2, sut.Value, "Value"); // не должна вернуться в 1
      Assert.IsFalse(sut.HasSource, "HasSource");
      Assert.IsFalse(sut.IsConnected, "IsConnected");
    }

    [Test]
    public void Source_changes_value()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual("1", resprod.ToString(), "Original");

      DepInput<int> src = new DepInput<int>(2, null);
      sut.Source = src;
      Assert.AreEqual("1|2", resprod.ToString(), "Source attached");

      src.Value = 3;
      Assert.AreEqual("1|2|3", resprod.ToString(), "Source value changed");
    }

    #region Событие ValueChanged

    [Test]
    public void ValueChanged()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual(1, sut.Value, "Original value");

      sut.Value = 2;
      Assert.AreEqual(2, sut.Value, "Changed value");
      Assert.AreEqual("1|2", resprod.ToString(), "ValueChangedCalled");

      sut.Value = 2;
      Assert.AreEqual("1|2", resprod.ToString(), "Not changed");
    }

    #endregion

    #region Событие CheckValue

    [Test]
    public void CheckValue_nothing()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      sut.CheckValue += CheckValue_Nothing_Handler;
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual("1", resprod.ToString(), "Original");

      sut.Value = 2;
      Assert.AreEqual("1|2", resprod.ToString(), "Changed");
    }

    static void CheckValue_Nothing_Handler(object sender, DepInputCheckEventArgs<int> args)
    {
    }

    [Test]
    public void CheckValue_cancel()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      sut.CheckValue += CheckValue_Cancel_Handler;
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual("1", resprod.ToString(), "Original");

      sut.Value = 2;
      Assert.AreEqual("1", resprod.ToString(), "Not changed");
    }

    static void CheckValue_Cancel_Handler(object sender, DepInputCheckEventArgs<int> args)
    {
      args.Cancel = true;
    }

    [Test]
    public void CheckValue_NewValue()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      sut.CheckValue += CheckValue_NewValue_Handler;
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual("1", resprod.ToString(), "Original");

      sut.Value = 2;
      Assert.AreEqual("1|3", resprod.ToString(), "Original");
    }

    static void CheckValue_NewValue_Handler(object sender, DepInputCheckEventArgs<int> args)
    {
      args.NewValue = 3;
    }

    [Test]
    public void CheckValue_Forced()
    {
      DepInput<int> sut = new DepInput<int>(1, null);
      sut.CheckValue += CheckValue_Forced_Handler;
      DepResultProducer<int> resprod = new DepResultProducer<int>(sut);
      Assert.AreEqual("1", resprod.ToString(), "Original");

      sut.Value = 1; // не меняем
      Assert.AreEqual("1|1", resprod.ToString(), "Second call");
    }

    static void CheckValue_Forced_Handler(object sender, DepInputCheckEventArgs<int> args)
    {
      args.Forced = true;
    }

    #endregion

    #endregion

    #region Тестирование комбинации "Обычное свойство - управляемое свойство"

    #region Тестовый объект

    private class PropertyPair
    {
      /// <summary>
      /// Тестирование изменения устанавливаемого значения, как в EFPInsideSubDoc.SubDocId
      /// </summary>
      public bool TestModifySetValue;

      /// <summary>
      /// Основное свойство
      /// </summary>
      public int Value
      {
        get { return _Value; }
        set
        {
          if (TestModifySetValue)
            _Value = 0;
          else
            _Value = value;


          if (_ValueEx != null)
            _ValueEx.Value = _Value;
        }
      }
      private int _Value;

      /// <summary>
      /// Управляемое свойство
      /// </summary>
      public DepValue<int> ValueEx
      {
        get
        {
          InitValueEx();
          return _ValueEx;
        }
        set
        {
          InitValueEx();
          _ValueEx.Source = value;
        }
      }
      private DepInput<int> _ValueEx;

      private void InitValueEx()
      {
        if (_ValueEx == null)
        {
          _ValueEx = new DepInput<int>(Value, ValueChanged);
          _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        }
      }

      private void ValueChanged(object sender, EventArgs args)
      {
        Value = _ValueEx.Value;
      }
    }

    #endregion

    [Test]
    public void PairedProperty_Value_normal()
    {
      PropertyPair sut = new PropertyPair();
      sut.Value = 1;
      Assert.AreEqual(1, sut.Value, "Value #1");
      Assert.AreEqual(1, sut.ValueEx.Value, "ValueEx #1");

      sut.Value = 2;
      Assert.AreEqual(2, sut.Value, "Value #2");
      Assert.AreEqual(2, sut.ValueEx.Value, "ValueEx #2");
    }


    [Test]
    public void PairedProperty_ValueEx_normal()
    {
      PropertyPair sut = new PropertyPair();
      DepOutput<int> src = new DepOutput<int>(1);
      sut.ValueEx = src;

      Assert.AreEqual(1, sut.Value, "Value #1");
      Assert.AreEqual(1, sut.ValueEx.Value, "ValueEx #1");

      src.OwnerSetValue(2);
      Assert.AreEqual(2, sut.Value, "Value #2");
      Assert.AreEqual(2, sut.ValueEx.Value, "ValueEx #2");
    }

    [Test]
    public void PairedProperty_Value_modify()
    {
      PropertyPair sut = new PropertyPair();
      sut.Value = 1;
      sut.TestModifySetValue = true;
      sut.Value = 2;
      Assert.AreEqual(0, sut.Value, "Value #2");
      Assert.AreEqual(0, sut.ValueEx.Value, "ValueEx #2");
    }

    [Test]
    public void PairedProperty_ValueEx_modify()
    {
      PropertyPair sut = new PropertyPair();
      DepOutput<int> src = new DepOutput<int>(1);
      sut.ValueEx = src;

      sut.TestModifySetValue = true;
      src.OwnerSetValue(2);
      Assert.AreEqual(0, sut.Value, "Value #2");
      Assert.AreEqual(0, sut.ValueEx.Value, "ValueEx #2");
    }

    #endregion

    #region "Короткое замыкание" входа на самого себя

    [Test]
    public void ShortCircuit_1()
    {
      DepInput<int> sut = new DepInput<int>(1, Dummy_ValueChanged);
      sut.Source = sut;

      Assert.AreEqual(1, sut.Value, "Original");

      sut.Value = 2;
      Assert.AreEqual(2, sut.Value, "Changed");
    }

    [Test]
    public void ShortCircuit_2()
    {
      DepInput<int> sut1 = new DepInput<int>(1, Dummy_ValueChanged);
      DepInput<int> sut2 = new DepInput<int>(1, Dummy_ValueChanged);
      sut1.Source = sut2;
      sut2.Source = sut1;

      Assert.AreEqual(1, sut1.Value, "Value #1 original");
      Assert.AreEqual(1, sut2.Value, "Value #2 original");

      sut1.Value = 2;
      Assert.AreEqual(2, sut1.Value, "Value #1 first change");
      Assert.AreEqual(2, sut2.Value, "Value #2 first change");

      sut2.Value = 3;
      Assert.AreEqual(3, sut1.Value, "Value #1 second change");
      Assert.AreEqual(3, sut2.Value, "Value #2 second change");
    }

    [Test]
    public void ShortCircuit_InputNotInputNot()
    { 
      // Такая схема используется в ExtDBDocForms, метод DocValueControlBase.CreateGrayedCheckBox
      DepInput<bool> sut1 = new DepInput<bool>(false, null);
      DepNot not1 = new DepNot(sut1);
      DepInput<bool> sut2 = new DepInput<bool>(false, null);
      sut2.Source = not1;
      DepNot not2 = new DepNot(sut2);
      sut1.Source = not2;

      Assert.IsFalse(sut1.Value, "#1 original");
      Assert.IsTrue(sut2.Value, "#2 original");

      sut1.Value = true;
      Assert.IsTrue(sut1.Value, "#1 first change");
      Assert.IsFalse(sut2.Value, "#2 first change");

      sut2.Value = true;
      Assert.IsFalse(sut1.Value, "#1 second change");
      Assert.IsTrue(sut2.Value, "#2 second change");
    }

    #endregion
  }
}
