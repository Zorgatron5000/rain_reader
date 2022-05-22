using Microsoft.VisualStudio.TestTools.UnitTesting;
using utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace utils.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void OutputToScreenTest()
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            String[] arr = new String[] { "1", "2", "3" };
            
            var dt = new DataTable();
            dt.Columns.Add("First");
            dt.Columns.Add("Second");
            dt.Columns.Add("Third");

            dt.Rows.Add(arr);
            dt.Rows.Add(arr);


            Utils.OutputToScreen("Banana", dt.Rows);

            Assert.AreEqual("Banana\r\n\r\n1, 2, 3, \r\n1, 2, 3, \r\n", stringWriter.ToString());
        }
        [TestMethod()]
        public void OutputToScreenTestBlankValue()
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            String[] arr = new String[] { "1", "2" };

            var dt = new DataTable();
            dt.Columns.Add("First");
            dt.Columns.Add("Second");
            dt.Columns.Add("Third");

            dt.Rows.Add(arr);


            Utils.OutputToScreen("Banana", dt.Rows);

            Assert.AreEqual("Banana\r\n\r\n1, 2, , \r\n", stringWriter.ToString());
        }
        [TestMethod()]
        public void OutputToScreenTestNoValues()
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            String[] arr = new String[] { "1", "2" };

            var dt = new DataTable();
            dt.Columns.Add("First");
            dt.Columns.Add("Second");
            dt.Columns.Add("Third");

            Utils.OutputToScreen("Banana", dt.Rows);

            Assert.AreEqual("Banana\r\n\r\n", stringWriter.ToString());
        }
    }
}