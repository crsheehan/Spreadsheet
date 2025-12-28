using System.Collections;

namespace SpreadsheetTests;

using Spreadsheets;
using Formula;

[TestClass]
public sealed class TestSpreadsheet
{
    #region Test SetCellContents

    [TestMethod]
    public void TestSetAndGetStringContents()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "hello");
        Assert.AreEqual("hello", sheet.GetCellContents("A1"));
    }

    [TestMethod]
    public void TestSetAndGetDoubleContents()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "3.0");
        Assert.AreEqual(3.0, sheet.GetCellContents("A1"));
    }


    [TestMethod]
    public void TestSetAndGetFormulaContents()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "=abc123");
        Assert.IsTrue(new Formula("abc123").Equals(sheet.GetCellContents("A1")));
    }


    [TestMethod]
    public void TestEmptySpreadsheetReturnsEmptyString()
    {
        Spreadsheet sheet = new Spreadsheet();
        Assert.AreEqual("", sheet.GetCellContents("A1"));
    }


    [TestMethod]
    public void TestCaseInsensitiveNames()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("a1", "hello");
        Assert.AreEqual("hello", sheet.GetCellContents("A1"));
        Assert.AreEqual("hello", sheet.GetCellContents("a1")); // lowercase
    }

    [TestMethod]
    public void TestGetCellContents_InvalidName_ThrowsException()
    {
        Spreadsheet sheet = new Spreadsheet();
        Assert.ThrowsExactly<InvalidNameException>(() => sheet.GetCellContents("1A"));
    }

    [TestMethod]
    public void TestGetCellContents_NullName_ThrowsException()
    {
        Spreadsheet sheet = new Spreadsheet();
        Assert.ThrowsExactly<InvalidNameException>(() => sheet.GetCellContents(null!));
    }

    [TestMethod]
    public void TestSetCellContents_String()
    {
        Spreadsheet sheet = new Spreadsheet();
        var result = sheet.SetContentsOfCell("A1", "hello");

        Assert.AreEqual("hello", sheet.GetCellContents("A1"));
        CollectionAssert.AreEqual(new List<string> { "A1" }, result.ToList());
    }

    [TestMethod]
    public void TestSetCellContents_StringWithDependents()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B1", "=A1 + 2");
        sheet.SetContentsOfCell("C1", "=B1 * 2");

        var result = sheet.SetContentsOfCell("A1", "10");

        // Should return A1, then its dependents in correct order
        var expected = new List<string> { "A1", "B1", "C1" };
        CollectionAssert.AreEqual(expected, result.ToList());
    }

    [TestMethod]
    public void TestSetCellContents_Double()
    {
        Spreadsheet sheet = new Spreadsheet();
        var result = sheet.SetContentsOfCell("A1", "42.5");

        Assert.AreEqual(42.5, sheet.GetCellContents("A1"));
        CollectionAssert.AreEqual(new List<string> { "A1" }, result.ToList());
    }

    [TestMethod]
    public void TestSetCellContents_DoubleWithDependents()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "5.0");
        sheet.SetContentsOfCell("B1", "=A1 * 2");
        sheet.SetContentsOfCell("C1", "=A1 + B1");

        var result = sheet.SetContentsOfCell("A1", "10.0");

        // A1 should be first, then B1 before C1 (since C1 depends on B1)
        Assert.AreEqual("A1", result.First());
        CollectionAssert.AreEqual(new List<string> { "A1", "B1", "C1" }, result.ToList());
    }

    [TestMethod]
    public void TestSetSetContentsNumberReturnsCorrectList()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "40.4");
        sheet.SetContentsOfCell("A2", "=A1 + 2");
        sheet.SetContentsOfCell("A3", "=A2 + 2");

        List<string> expected = ["A1", "A2", "A3"];
        CollectionAssert.AreEqual(expected, (ICollection?)sheet.SetContentsOfCell("A1", "40.3"));
    }

    [TestMethod]
    public void DirectCircularDependencyThrows()
    {
        var ss = new Spreadsheet();
        Assert.ThrowsExactly<CircularException>(() =>
            ss.SetContentsOfCell("A1", "=A1+1")); // directly depends on itself
    }


    [TestMethod]
    public void TestSetCellContents_EmptyStringName_ThrowsException()
    {
        Spreadsheet sheet = new Spreadsheet();
        Assert.ThrowsExactly<InvalidNameException>(() => sheet.SetContentsOfCell("", "hello"));
    }

    [TestMethod]
    public void TestSetContentsOfCell_WhitespaceName_ThrowsException()
    {
        Spreadsheet sheet = new Spreadsheet();
        Assert.ThrowsExactly<InvalidNameException>(() => sheet.SetContentsOfCell("   ", "hello"));
    }

    [TestMethod]
    public void TestSetContentsOfCell_OnlyLetters_ThrowsException()
    {
        Spreadsheet sheet = new Spreadsheet();
        Assert.ThrowsExactly<InvalidNameException>(() => sheet.SetContentsOfCell("ABC", "hello"));
    }

    [TestMethod]
    public void TestSetContentsOfCell_OnlyNumbers_ThrowsException()
    {
        Spreadsheet sheet = new Spreadsheet();
        Assert.ThrowsExactly<InvalidNameException>(() => sheet.SetContentsOfCell("123", "hello"));
    }

    [TestMethod]
    public void TestSetContentsOfCell_EmptyString_RemovesFromNonemptyCells()
    {
        //Set A1 to hello
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "hello");

        Assert.IsTrue(sheet.GetNamesOfAllNonemptyCells().Contains("A1"));

        // Reset A1 to ""
        sheet.SetContentsOfCell("A1", "");
        
        // But it should appear in non-empty cells since it's stored
        Assert.IsTrue(!sheet.GetNamesOfAllNonemptyCells().Any());
    }

    [TestMethod]
    public void TestIndirectCircularDependency_ThreeNodes()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "=B1");
        sheet.SetContentsOfCell("B1", "=C1");

        // This should create A1 to B1 to C1 to A1
        Assert.ThrowsExactly<CircularException>(() => sheet.SetContentsOfCell("C1", "=A1"));
    }

    [TestMethod]
    public void TestReplaceCellContent_StringToDouble()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "hello");

        Assert.AreEqual("hello", sheet.GetCellContents("A1"));

        sheet.SetContentsOfCell("A1", "42.5");

        Assert.AreEqual(42.5, sheet.GetCellContents("A1"));
    }

    [TestMethod]
    public void TestReplaceCellContent_DoubleToFormula()
    {
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "42.5");
        sheet.SetContentsOfCell("B1", "10.0");

        var formula = new Formula("B1 * 2");
        sheet.SetContentsOfCell("A1", "=B1 * 2");

        Assert.IsTrue(formula.Equals(sheet.GetCellContents("A1")));
    }

    #endregion
    
    #region Test GetNamesOfAllNonemptyCells

    [TestMethod]
    public void TestGetNamesOfAllNonemptyCells_Valid()
    {
        //create spreadsheet and add cells
        Spreadsheet sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "hello");
        sheet.SetContentsOfCell("A2", "hello");
        sheet.SetContentsOfCell("A3", "hello");
        ISet<string> names = sheet.GetNamesOfAllNonemptyCells();

        //Create expected set
        ISet<string> expected = new HashSet<string>();
        expected.Add("A1");
        expected.Add("A2");
        expected.Add("A3");

        Assert.IsTrue(expected.SetEquals(names));
    }

    [TestMethod]
    public void TestGetNamesOfAllNonemptyCellsOnEmptySpreadsheet_Valid()
    {
        //create spreadsheet and add cells
        Spreadsheet sheet = new Spreadsheet();
        ISet<string> names = sheet.GetNamesOfAllNonemptyCells();

        Assert.IsTrue(names.SetEquals(new HashSet<string>()));
    }

    #endregion
    
    #region Test GetCellContents

    [TestMethod]
    public void TestGetCellContents_Number()
    {
        var sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "40.4");

        Assert.AreEqual(40.4, sheet.GetCellContents("A1"));
    }

    [TestMethod]
    public void TestGetCellContents_String()
    {
        var sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "hello");

        Assert.AreEqual("hello", sheet.GetCellContents("A1"));
    }

    [TestMethod]
    public void TestGetCellContents_Formula()
    {
        var sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "=2+2");

        Assert.IsTrue(new Formula("2 + 2").Equals(sheet.GetCellContents("A1")));
    }

    [TestMethod]
    public void TestGetCellContents_InvalidNameException()
    {
        var sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "=2+2");

        Assert.ThrowsExactly<InvalidNameException>(() => sheet.GetCellContents("1A"));
    }

    [TestMethod]
    public void TestGetCellContentsForNonExistentCellReturnsEmptyString()
    {
        var spreadsheet = new Spreadsheet();
        var result = spreadsheet.GetCellContents("A1");

        // This tests the empty string return in GetCellContents method
        // which happens when a cell doesn't exist in the _cells dictionary
        Assert.AreEqual("", result);
    }

    #endregion

    #region Testing Lookup Function and GetCellValue

    [TestMethod]
    public void TestLookupFunctionCellNotANumber()
    {
        var sheet = new Spreadsheet();
        sheet.SetContentsOfCell("A1", "hello");
        sheet.SetContentsOfCell("A2", "=A1 + 5");
        
        Assert.IsTrue(sheet.GetCellValue("A2") is FormulaError);
    }
    
    [TestMethod]
    public void GetCellValue_EmptyCell_ReturnsEmptyString()
    {
        Spreadsheet sheet = new();
        Assert.AreEqual("",  sheet.GetCellValue("A1"));
    }

    [TestMethod]
    public void GetCellValue_ReturnsString()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("A1", "hello");
        Assert.AreEqual("hello", sheet.GetCellValue("A1"));
    }
    
    [TestMethod]
    public void GetCellValue_ReturnsDouble()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("A1", "55.1");
        Assert.AreEqual(55.1, sheet.GetCellValue("A1"));
    }
    
    [TestMethod]
    public void GetCellValue_ReturnsFormula()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("ABCDEFGHIJKL1", "=5 + 5");
        Assert.AreEqual(10.0, sheet.GetCellValue("ABCDEFGHIJKL1"));
    }
    
    [TestMethod]
    public void GetCellValue_InvalidName_ThrowsInvalidNameException()
    {
       Spreadsheet sheet = new();
       Assert.Throws<InvalidNameException>(() => sheet.GetCellValue("23A"));
       Assert.Throws<InvalidNameException>(() => sheet.GetCellValue("+5"));
       Assert.Throws<InvalidNameException>(() => sheet.GetCellValue("55"));
       Assert.Throws<InvalidNameException>(() => sheet.GetCellValue("A"));
    }

    [TestMethod]
    public void CellLookup_ReturnsCorrectNumber()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("A2", "=A1 + 5.0");
        
        Assert.AreEqual(10.0,  sheet.GetCellValue("A2"));
    }
    
    [TestMethod]
    public void CellLookup_EmptyCellReturnsError()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("A2", "=B1 + 5.0");

        Assert.IsTrue(sheet.GetCellValue("a2") is FormulaError);
    }

    [TestMethod]
    public void CellLookup_MultipleReferenceVariables_Correct()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("A2", "=A1 + 5.0");
        sheet.SetContentsOfCell("A3", "=A2 * 5");
        sheet.SetContentsOfCell("A4", "=A3 - 49");
        
        Assert.AreEqual(1.0,  sheet.GetCellValue("A4"));
    }

    [TestMethod]
    public void GetCellValueIndexer_Correct()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("A2", "=A1 + 5.0");
        
        Assert.AreEqual(5.0, sheet["A1"]);
        Assert.AreEqual(10.0, sheet["A2"]);
    }
    
    #endregion

    #region New SetContentsOfCell Tests

    [TestMethod]
    public void SetContentsOfCell_ReturnsCorrectList()
    {
        Spreadsheet sheet = new();

        var list =sheet.SetContentsOfCell("A1", "44");
        Assert.IsTrue(list.Contains("A1"));
    }
    
    [TestMethod]
    public void SetContentsOfCell_UpdatesDependents()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("A1", "5");
        sheet.SetContentsOfCell("B1", "=A1*2");
        sheet.SetContentsOfCell("C1", "=B1+A1");
        
        var result = sheet.SetContentsOfCell("A1", "10");
        
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result[0].Equals("A1")); // Changed cell first
        Assert.IsTrue(result.Contains("B1"));
        Assert.IsTrue(result.Contains("C1"));
    }
    
    [TestMethod]
    public void SetContentsOfCell_CircularDependency_ThrowsException()
    {
        Spreadsheet sheet = new();
        sheet.SetContentsOfCell("A1", "=B1");
        Assert.Throws<CircularException>(() => sheet.SetContentsOfCell("B1", "=A1"));
    }

    [TestMethod]
    public void ChangedIsTrueAfterSetContentsOfCellCall()
    {
        //Test on empty/new sheet
        Spreadsheet sheet = new();
        Assert.IsFalse(sheet.Changed);

        //Test after Change
        sheet.SetContentsOfCell("A2", "test");
        Assert.IsTrue(sheet.Changed);
        
        //Test after save
        sheet.Save("test.txt");
        Assert.IsFalse(sheet.Changed);
    }
    
    #endregion

    #region Test Save and Load
    
    [TestMethod]
    public void TestSaveWithStrings()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", "Hello");
        ss.SetContentsOfCell("B2", "5");
        
        ss.Save("plz_work.txt");
        
        string content = File.ReadAllText("plz_work.txt");
        Assert.IsTrue(content.Contains("\"Hello\"" ));
        Assert.IsTrue(content.Contains("\"5\""));
        Assert.IsFalse(ss.Changed);
    }

    [TestMethod]
    public void TestLoadWithStrings()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", "Hello");
        ss.SetContentsOfCell("B2", "Chance");
        
        ss.Save("test1.txt");
        
        Spreadsheet loaded = new Spreadsheet("test1.txt");
        Assert.AreEqual("Hello", loaded.GetCellValue("A1"));
    }
    [TestMethod]
    public void TestLoadWithDependencies()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", "5");
        ss.SetContentsOfCell("B2", "=A1 + 5");
        ss.SetContentsOfCell("C3", "=B2+A1");
        
        ss.Save("test2.txt");
        
        Spreadsheet loaded = new Spreadsheet("test2.txt");
        Assert.AreEqual(5.0, loaded.GetCellValue("A1"));
        Assert.AreEqual(10.0, loaded.GetCellValue("B2"));
        Assert.AreEqual(15.0, loaded.GetCellValue("C3"));
    }

    [TestMethod]
    public void SaveWithInvalidFileNAme()
    {
        Spreadsheet ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", "5");
        
        string invalidPath = "invalid\0.txt"; 
    
        Assert.Throws<Spreadsheets.SpreadsheetReadWriteException>(() => ss.Save(invalidPath));
    }

    [TestMethod]
    public void SaveAndLoadOnEmptySpreadsheet_ThrowsError()
    {
        Spreadsheet ss = new();
        ss.Save("test.txt3");
        
       Assert.Throws<SpreadsheetReadWriteException>(() =>  new Spreadsheet("test.txt3"));
       
        
        
    }
    
    
    
    #endregion
    
    

    
}