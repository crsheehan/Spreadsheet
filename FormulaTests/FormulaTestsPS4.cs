namespace FormulaTests;

using Formula;

[TestClass]
public class FormulaTestsPs4
{
    //Creating Lookup methods for testing Evaluate
    private double ReturnZero(string variableName) => 0;
    private double ThrowsArgException(string var) => throw new ArgumentException("Variable not found");
    
    
    // Tests for == operator ------------------------------------------------------------------------------
    [TestMethod]
    public void TestEqualsOperator_True()
    {
        Formula f1 = new Formula("1+1");
        Formula f2 = new Formula("1+1");

        Assert.IsTrue(f1 == f2);
    }

    [TestMethod]
    public void TestEqualsOperator_False()
    {
        Formula f1 = new Formula("1+1");
        Formula f2 = new Formula("2+2");

        Assert.IsFalse(f1 == f2);
    }

    [TestMethod]
    public void TestEqualsOperatorWithSameEvaluatedValue_False()
    {
        Formula f1 = new Formula("1+2");
        Formula f2 = new Formula("2+1");

        Assert.IsFalse(f1 == f2);
    }

    // Tests for != operator ------------------------------------------------------------------------------
    [TestMethod]
    public void TestNotEqualsOperator_False()
    {
        Formula f1 = new Formula("1+1");
        Formula f2 = new Formula("1+1");

        Assert.IsFalse(f1 != f2);
    }

    [TestMethod]
    public void TestNotEqualsOperator_True()
    {
        Formula f1 = new Formula("1+1");
        Formula f2 = new Formula("2+2");

        Assert.IsTrue(f1 != f2);
    }

    [TestMethod]
    public void TestNotEqualsOperatorWithSameEvaluatedValue_True()
    {
        Formula f1 = new Formula("1+2");
        Formula f2 = new Formula("2+1");

        Assert.IsTrue(f1 != f2);
    }

    // Tests for Equals() ---------------------------------------------------------------------------------
    [TestMethod]
    public void TestEquals_True()
    {
        Formula f1 = new Formula("1 +       1");
        Formula f2 = new Formula("1+1");
        
        Assert.IsTrue(f1.Equals(f2));
    }
    
    [TestMethod]
    public void TestEquals_False()
    {
        Formula f1 = new Formula("1 +       1");
        Formula f2 = new Formula("1+2");
        
        Assert.IsFalse(f1.Equals(f2));
    }
    
    [TestMethod]
    public void TestEqualsSameEvaluatedValue_False()
    {
        Formula f1 = new Formula("2+1");
        Formula f2 = new Formula("1+2");
        
        Assert.IsFalse(f1.Equals(f2));
    }

    [TestMethod]
    public void TestEqualsWithNullFormula_False()
    {
        Formula f1 = new Formula("1+1");
        Formula? f2 = null;
        
        Assert.IsFalse(f1.Equals(f2));
    }


    //Tests Evaluate --------------------------------------------------------------------------------------
    [TestMethod]
    public void TestEvaluateNoVariablePlus_True()
    {
        
        
        Formula plus = new Formula("5+5");
        Assert.AreEqual(10.0, plus.Evaluate(ReturnZero));
    }

    [TestMethod]
    public void TestEvaluateNoVariableOnlyMinus_True()
    {
        Formula minus = new Formula("5 -3 - 1");
        Assert.AreEqual(1.0, minus.Evaluate(ReturnZero));
    }
    
    [TestMethod]
    public void TestEvaluateNoVariableMinusAndPlus_True()
    {
        Formula minus = new Formula("5 -3 + 1 - 2 - 4 + 1 + 6 - 3");
        Assert.AreEqual(1.0, minus.Evaluate(ReturnZero));
    }
    
    [TestMethod]
    public void TestEvaluateNoVariableMultiply_True()
    {
        Formula multiply = new Formula("1 * 1 *2 * 5");
        Assert.AreEqual(10.0, multiply.Evaluate(ReturnZero));
    }
    
    [TestMethod]
    public void TestEvaluateNoVariableDivide_True()
    {
        Formula divide = new Formula("2 / 2 / 1");
        Assert.AreEqual(1.0, divide.Evaluate(ReturnZero));
    }

    [TestMethod]
    public void TestEvaluateNoVariableAllOperators_Equal()
    {
        Formula f1 = new Formula("6 - 2 / 2 * 5 + 6");
        Assert.AreEqual(7.0, f1.Evaluate(ReturnZero));
    }

    [TestMethod]
    public void TestEvaluateDivideByZero_Invalid()
    {
        Formula f1 = new  Formula("6 / 0");
        Assert.IsTrue(f1.Evaluate(ReturnZero) is FormulaError);
    }

    [TestMethod]
    public void TestEvaluateWithParenthesesComplex_Valid()
    {
        Formula f = new Formula("5 + (10-5) * 1 / 1 * (1 * 2)");
        Assert.AreEqual(15.0, f.Evaluate(ReturnZero));
    }

    [TestMethod]
    public void TestEvaluateVariable_True()
    {
        Formula f = new Formula("5 + A1");
        Assert.AreEqual(5.0, f.Evaluate(ReturnZero));
    }

    [TestMethod]
    public void TestEvaluateVariableWithAllOperators()
    {
        Formula f = new Formula("5 + 6 * (A1 * 5) - (A7 +2) / 2 + Me5");
        Assert.AreEqual(4.0, f.Evaluate(ReturnZero));
    }
    
    [TestMethod]
    public void TestEvaluateDivisionInParentheses()
    {
        Formula f = new Formula("6/(3/1)");
        Assert.AreEqual(2.0, f.Evaluate(ReturnZero));
    }
    
    [TestMethod]
    public void TestEvaluateDivisionInParenthesesDivisionByZero()
    {
        Formula f = new Formula("32/(32/0)");
        Assert.IsTrue(f.Evaluate(ReturnZero) is FormulaError);
    }
    
    [TestMethod]
    public void TestEvaluateDivisionByZeroAtRightParen()
    {
        Formula f = new Formula("(8/(4-4))");
        Assert.IsTrue(f.Evaluate(ReturnZero) is FormulaError);
    }

    [TestMethod]
    public void TestEvaluateVariableLookupFailed()
    {
        Formula f = new Formula("A1");
        Assert.IsTrue(f.Evaluate(ThrowsArgException) is FormulaError);
    }

    [TestMethod]
    public void TestEvaluateWithDoubles_True()
    {
        Formula f = new Formula("5.6 - 3.6");
        Assert.AreEqual(2, (double)f.Evaluate(ReturnZero), 1e-9);
    }
    
    
    
    // Tests for GetHashCode ------------------------------------------------------------------------------
    [TestMethod]
    public void TestGetHashCodeEquals_True()
    {
        Formula f1 = new Formula("5 + 5");
        Formula f2 = new Formula("5+5");
        Assert.AreEqual(f1.GetHashCode(), f2.GetHashCode());
    }
    
    [TestMethod]
    public void TestGetHashCode_NotEqual()
    {
        Formula f1 = new Formula("5 + 5");
        Formula f2 = new Formula("6+4");
        Assert.AreNotEqual(f1.GetHashCode(), f2.GetHashCode());
    }

    
    // Test Formula Error ---------------------------------------------------------------------------------
    [TestMethod]
    public void Constructor_SetsReason()
    {
        var fe = new FormulaError("Bad variable");
        Assert.AreEqual("Bad variable", fe.Reason);
    }

    [TestMethod]
    public void ToString_ReturnsReason()
    {
        var fe = new FormulaError("Division by 0 Error");
        Assert.AreEqual("Division by 0 Error", fe.Reason);
    }
    
    
    
}