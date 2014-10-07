using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectDumperTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Only_Properties_With_Getter_Are_Dump()
        {
            var dumper = new ObjectDumper<Test1Class>();
            var desc = dumper.Dump(new Test1Class());
            Assert.AreEqual(1, desc.Count());
        }

        [TestMethod]
        public void Dump_Is_Sorted_By_Property_Name()
        {
            var dumper = new ObjectDumper<Test3Class>();
            var desc = dumper.Dump(new Test3Class()).Select(kvp => kvp.Key);

            CollectionAssert.AreEqual(desc.ToList(), new List<string> { "AProperty", "BProperty", "ZProperty" });
        }

        [TestMethod]
        public void Default_Template_Is_To_String()
        {

            var dumper = new ObjectDumper<Test2Class>();
            var desc = dumper.Dump(new Test2Class());
            Assert.AreEqual(new Test2Class.Test2Inner().ToString(), desc.First().Value);

        }

        [TestMethod]
        public void Template_For_Simple_Type_Is_Applied()
        {
            const string IS_42 = "Answer to everything";
            const string IS_NOT_42 = "not meaningful";

            var dumper = new ObjectDumper<Test2Class.Test2Inner>();
            dumper.AddTemplateFor(o => o.Value, v => v == 42 ? IS_42 : IS_NOT_42);

            var data = new Test2Class.Test2Inner
            {
                Name = "Some name",
                Value = 42
            };

            var desc = dumper.Dump(data);
            Assert.IsNotNull(desc.SingleOrDefault(kvp => kvp.Key == "Value" && kvp.Value == IS_NOT_42));
        }

        [TestMethod]
        public void Template_For_Complex_Type_Is_Applied()
        {
            var ufo = new Ufo()
            {
                Name = "Conqueror III",
                Speed = 10,
                Origin = new Planet()
                {
                    Name = "Alpha Centauri 3",
                    DaysPerYear = 452
                }
            };
            var dumper = new ObjectDumper<Ufo>();
            dumper.AddTemplateFor(u => u.Origin, o => string.Format("Planet: {0} DaysPerYear: {1}", o.Name, o.DaysPerYear));

            var desc = dumper.Dump(ufo);

            Assert.IsNotNull(desc.SingleOrDefault(kvp =>
                kvp.Key == "Origin" && kvp.Value == string.Format("Planet: {0} DaysPerYear: {1}", ufo.Origin.Name, ufo.Origin.DaysPerYear)));
        }

        [TestMethod]
        public void Not_Listed_Property_Is_Not_Invoked()
        {
            var dumper = new ObjectDumper<CrashedUfo>();
            var crashed = new CrashedUfo
            {
                Name = "Conqueror III",
                Speed = 10,
                Origin = new Planet
                {
                    Name = "Alpha Centauri 3",
                    DaysPerYear = 452
                }
            };

            var desc = dumper.Dump(crashed);
            var twoPropertiesList = desc.Take(2).ToList();
            // No exception at this point because ZLastProperty is *never* invoked
            Assert.AreEqual(2, twoPropertiesList.Count);
        }

        [TestMethod]
        public void Null_properties_return_empty_string_value()
        {
            var ufo = new Ufo
            {
                Name = null,
                Speed = 10,
                Origin = new Planet
                {
                    Name = "",
                    DaysPerYear = 453
                }
            };

            var dumper = new ObjectDumper<Ufo>();

            var desc = dumper.Dump(ufo);

            Assert.AreEqual("", desc.SingleOrDefault(x => x.Key == "Name").Value);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void Null_Complex_throw_exception_if_try_format_its_properties()
        {
            var ufo = new Ufo()
            {
                Name = "Conqueror III",
                Speed = 10,
                Origin = null
            };

            var dumper = new ObjectDumper<Ufo>();
            dumper.AddTemplateFor(u => u.Origin, o => string.Format("Planet: {0} DaysPerYear: {1}", o.Name, o.DaysPerYear));

            var desc = dumper.Dump(ufo);

            Assert.AreEqual(string.Format("Planet: {0} DaysPerYear: {1}", "", ufo.Origin.DaysPerYear), desc.SingleOrDefault(x => x.Key == "Origin").Value);
        }
    }
}
