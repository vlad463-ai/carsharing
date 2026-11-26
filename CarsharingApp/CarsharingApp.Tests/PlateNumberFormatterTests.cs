using CarsharingApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CarsharingApp.Tests;

[TestClass]
public class PlateNumberFormatterTests
{
    [DataTestMethod]
    [DataRow("\u201A456\u0453\u201E69", "В456ГД69")]
    [DataRow("\u0409111\u2039\u040A69", "К111ЛМ69")]
    [DataRow("А123ВС777", "А123ВС777")]
    [DataRow(" a123bc77 ", "A123BC77")]
    [DataRow("UNKNOWN", "UNKNOWN")]
    [DataRow("", "Не указан")]
    public void Format_RepairsLegacyEncodingWithoutInventingPlate(string input, string expected)
    {
        var car = new Car { PlateNumber = input };
        Assert.AreEqual(expected, car.DisplayPlateNumber);
        Assert.AreEqual(input, car.PlateNumber, "Display formatting must not overwrite stored data.");
    }
}
