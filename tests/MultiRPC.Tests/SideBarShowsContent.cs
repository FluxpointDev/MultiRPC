using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno.UITest.Helpers.Queries;
// Alias to simplify the creation of element queries
using Query = System.Func<Uno.UITest.IAppQuery, Uno.UITest.IAppQuery>;

namespace MultiRPC.Tests
{
    class SideBarShowsContent : TestBase
    {
        [Test]
        public void Testing()
        {
            //TODO: Make testsTM
            Query checkBoxSelector = q => q.Button("btnMultiRPC");
            App.WaitForElement(checkBoxSelector);

            Query cb1 = q => q.All("cb1");
            App.WaitForElement(cb1);

            var value1 = App.Query(q => cb1(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
            Assert.IsFalse(value1);

            App.Tap(cb1);

            var value2 = App.Query(q => cb1(q).GetDependencyPropertyValue("IsChecked").Value<bool>()).First();
            Assert.IsTrue(value2);
        }
    }
}
