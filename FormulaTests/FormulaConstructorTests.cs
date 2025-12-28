// <copyright file="FormulaSyntaxTests.cs" company="UofU-CS3500">
// Copyright © 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <authors> [Insert Your Name] </authors>
// <date> [Insert the Date] </date>




namespace FormulaTests;

using Formula;
// Change this using statement to use different formula implementations.

/// <summary>
///     <para>
///         The following class shows the basics of how to use the MSTest framework,
///         including:
///     </para>
///     <list type="number">
///         <item> How to catch exceptions. </item>
///         <item> How a test of valid code should look. </item>
///     </list>
/// </summary>
[TestClass]
public class FormulaSyntaxTests
{
    // --- Tests for One Token Rule ------------------------------------------------------------------------------------

    /// <summary>
    ///     <para>
    ///         This test makes sure the right kind of exception is thrown
    ///         when trying to create a formula with no tokens.
    ///     </para>
    ///     <remarks>
    ///         <list type="bullet">
    ///             <item>
    ///                 We use the _ (discard) notation because the formula object
    ///                 is not used after that point in the method.  Note: you can also
    ///                 use _ when a method must match an interface but does not use
    ///                 some of the required arguments to that method.
    ///             </item>
    ///             <item>
    ///                 string.Empty is often considered best practice (rather than using "") because it
    ///                 is explicit in intent (e.g., perhaps the coder forgot to but something in "").
    ///             </item>
    ///             <item>
    ///                 The name of a test method should follow the MS standard:
    ///                 https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
    ///             </item>
    ///             <item>
    ///                 All methods should be documented, but perhaps not to the same extent
    ///                 as this one.  The remarks here are for your educational
    ///                 purposes (i.e., a developer would assume another developer would know these
    ///                 items) and would be superfluous in your code.
    ///             </item>
    ///             <item>
    ///                 Notice the use of the attribute tag [ExpectedException] which tells the test
    ///                 that the code should throw an exception, and if it doesn't an error has occurred;
    ///                 i.e., the correct implementation of the constructor should result
    ///                 in this exception being thrown based on the given poorly formed formula.
    ///             </item>
    ///         </list>
    ///     </remarks>
    ///     <example>
    ///         <code>
    ///        // here is how we call the formula constructor with a string representing the formula
    ///        _ = new Formula( "5+5" );
    ///     </code>
    ///     </example>
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestNoTokens_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(string.Empty));
    }

    [TestMethod]
    public void FormulaConstructor_TestEmptySpace_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(" "));
    }

    // --- Tests for Valid Token Rule ----------------------------------------------------------------------------------

    /// <summary>
    ///     This test tests to see if every token in an expression containing all valid tokens runs.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestTokensVariable_Valid()
    {
        _ = new Formula("a167 + (4/6) - 5 * 1234567890 - 1.5");
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensVariableE_Valid()
    {
        _ = new Formula("3.5E-6");
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensVariableDollarSign_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("2+$3"));
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensQuestionMark_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("-5 + 6"));
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensValidNegativeNumber_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("-5"));
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensValidPowerSign_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(a167)^5"));
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensValidVariable_Invalid()
    {
        Assert.ThrowsExactly<FormulaFormatException>(() => _ = new Formula("15A"));
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensValidVariableNoDigit_Invalid()
    {
        Assert.ThrowsExactly<FormulaFormatException>(() => _ = new Formula("ABC"));
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensValidVariableUnderscore_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("A_55"));
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensValidDoubleDecimal_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("5..1"));
    }

    [TestMethod]
    public void FormulaConstructor_TestTokensValidExtraSpaces_Valid()
    {
        _ = new Formula("       5 +                        6               ");
    }

    [TestMethod]
    public void FormulaConstructor_TestWhitespaceMisclassifiedAsOperator_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("5 6"));
    }
    
    [TestMethod]
    public void FormulaConstructor_OnlyOperator_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("*"));
    }

    // --- Tests for Closing Parenthesis Rule --------------------------------------------------------------------------

    /// <summary>
    ///     This tests to see that the number of
    ///     opening parenthesis in a formula equals the number of closing parenthesis.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesisRuleValid()
    {
        _ = new Formula("((( 5 + 5)))");
    }

    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesesRule_Invalid()
    {
        Assert.ThrowsExactly<FormulaFormatException>(() => _ = new Formula("(5)) + 5(4"));
    }

    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesesRuleExtraClosing_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("((5 + 5)))"));
    }

    // --- Tests for Balanced Parentheses Rule -------------------------------------------------------------------------
    [TestMethod]
    public void FormulaConstructor_TestBalancedParenthesesRuleValid()
    {
        _ = new Formula("(((5 + 5)) - (6) + (a45 - 666))");
    }

    [TestMethod]
    public void FormulaConstructor_TestBalancedParenthesesRule_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(1+2"));
    }


    // --- Tests for First Token Rule ----------------------------------------------------------------------------------

    /// <summary>
    ///     <para>
    ///         Make sure a simple well-formed formula is accepted by the constructor (the constructor
    ///         should not throw an exception).
    ///     </para>
    ///     <remarks>
    ///         This is an example of a test that is not expected to throw an exception, i.e., it succeeds.
    ///         In other words, the formula "1+1" is a valid formula which should not cause any errors.
    ///     </remarks>
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenNumber_Valid()
    {
        _ = new Formula("1+1");
    }

    /// <summary>
    ///     <para>
    ///         Make sure a simple well-formed formula is accepted by the constructor (the constructor
    ///         should not throw an exception).
    ///     </para>
    ///     <remarks>
    ///         This is an example of a test that is not expected to throw an exception, i.e., it succeeds.
    ///         In other words, the formula "1+1" is a valid formula which should not cause any errors.
    ///     </remarks>
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenParenthesis_Valid()
    {
        _ = new Formula("(1+1)");
    }

    [TestMethod]
    public void FormulaConstructor_TestFirstTokenVariables_Valid()
    {
        _ = new Formula("A8 + 7");
    }

    [TestMethod]
    public void FormulaConstructor_TestFirstTokenRuleStartingOperator_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("+ 5 - 6"));
    }

    [TestMethod]
    public void FormulaConstructor_TestFirstTokenRuleOpenParenthesis_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(")5 + 4"));
    }

    // --- Tests for  Last Token Rule ----------------------------------------------------------------------------------

    /// <summary>
    ///     This test checks to see that the last token in a formula is valid, i.e. a number, variable, or closing parenthesis.
    ///     If not, it should throw a FormulaFormatException.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestLastTokenRuleNumber_Valid()
    {
        _ = new Formula("1+1");
    }

    [TestMethod]
    public void FormulaConstructor_TestLastTokenRuleVariable_Valid()
    {
        _ = new Formula("1 + abc2");
    }

    [TestMethod]
    public void FormulaConstructor_TestLastTokenRuleParenthesis_Valid()
    {
        _ = new Formula("(abc2)");
    }

    [TestMethod]
    public void FormulaConstructor_TestLastTokenRuleOpeningParenthesis_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(1 + abc2 + 3("));
    }

    [TestMethod]
    public void FormulaConstructor_TestLastTokenRuleOperator_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(1 + abc2 + 3) +"));
    }

// --- Tests for Parentheses/Operator Following Rule -------------------------------------------------------------------

/// <summary>
///     These tests ensure that Any token that immediately follows an opening parenthesis
///     or an operator must be either a number, a variable, or an opening parenthesis.
/// </summary>
[TestMethod]
    public void FormulaConstructor_TestParenthesisOperatorRuleNumber_Valid()
    {
        _ = new Formula("(1 + 3)");
    }

    [TestMethod]
    public void FormulaConstructor_TestParenthesisOperatorRuleVariable_Valid()
    {
        _ = new Formula("(g6 + 3)");
    }

    [TestMethod]
    public void FormulaConstructor_TestParenthesisOperatorRuleOpeningParenthesis_Valid()
    {
        _ = new Formula("((1 + 3))");
    }

    [TestMethod]
    public void FormulaConstructor_TestParenthesisOperatorFollowingRuleOperator_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(- + 3)"));
    }

    [TestMethod]
    public void FormulaConstructor_TestParenthesisOperatorRuleDoubleOperator_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("5 + + 5"));
    }


// --- Tests for Extra Following Rule ----------------------------------------------------------------------------------

/// <summary>
///     These tests ensure that Any token that immediately follows a number, a variable, or a closing parenthesis must
///     be either an operator or a closing parenthesis.
/// </summary>
[TestMethod]
    public void FormulaConstructor_TestExtraFollowingRuleVariableAfter_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(1 + 3)B6 "));
    }

    [TestMethod]
    public void FormulaConstructor_TestLastTokenRule_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(1 + 3) - "));
    }

    [TestMethod]
    public void FormulaConstructor_TestExtraFollowingRuleNumberAfterParenthesis_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("(5)2"));
    }

    [TestMethod]
    public void FormulaConstructor_TestExtraFollowingRuleNumberBeforeParenthesis_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("2(5)"));
    }

    //Testing toString -----------------------------------------------------------------------------

    [TestMethod]
    public void Formula_TestToString_Valid()
    {
        Assert.AreEqual("A5+2", new Formula("a5 + 2").ToString());
    }

    [TestMethod]
    public void Formula_TestToString_Valid2()
    {
        var expected = "A1+(B2/C3)+D4";
        Assert.AreEqual(expected,
            new Formula("A1      +(b2            /C3)        +                d4").ToString());
    }

    [TestMethod]
    public void Formula_TestToStringDecimal_Valid()
    {
        var expected = "5+200000";
        Assert.AreEqual(expected, new Formula("5.000000  +  2e5").ToString());
    }


    //Testing GetVar -------------------------------------------------------------------------------

    [TestMethod]
    public void Formula_TestGetVar_Valid()
    {
        ISet<string> expectedVars = new HashSet<string>();
        expectedVars.Add("ABCD1");
        expectedVars.Add("B2");
        expectedVars.Add("C3");
        expectedVars.Add("D4");

        Assert.IsTrue(expectedVars.SetEquals(
            new Formula("AbCd1      +(b2            /C3)        +                d4").GetVariables()));
    }
    
    
    //These are downloaded tests that were used in the auto-grader for PS2 ------------------------------
    
     // --- Tests One Token Rule ---
    
        /// <summary>
        ///   Test that an empty formula throws the formula format exception.
        /// </summary>
        [TestMethod]
        [TestCategory( "1" )]
        public void FormulaConstructor_TestOneToken_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( string.Empty ) );
        }
    
        /// <summary>
        ///   Test that an empty formula, but with spaces, also fails.
        /// </summary>
        [TestMethod]
        [TestCategory( "2" )]
        public void FormulaConstructor_TestOneTokenSpaces_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "  " ) );
        }
    
        // --- Test Valid Token Rules ---
    
        /// <summary>
        ///   Test that invalid tokens throw the appropriate exception.
        /// </summary>
        [TestMethod]
        [TestCategory( "3" )]
        public void FormulaConstructor_TestInvalidTokensOnly_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "$" ) );
        }
    
        /// <summary>
        ///   Test for another invalid token in the formula.
        /// </summary>
        [TestMethod]
        [TestCategory( "4" )]
        public void FormulaConstructor_TestInvalidTokenInFormula_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5 + 5 ," ) );
        }
    
        /// <summary>
        ///   Test that _all_ the valid tokens can be parsed,
        ///   e.g., math operators, numbers, variables, parens.
        /// </summary>
        [TestMethod]
        [TestCategory( "5" )]
        public void FormulaConstructor_TestValidTokenTypes_Succeeds( )
        {
            _ = new Formula( "5 + (1-2) * 3.14 / 1e6 + 0.2E-9 - A1 + bb22" );
        }
    
        // --- Test Closing Parenthesis Rule ---
    
        /// <summary>
        ///   Test that a closing paren cannot occur without
        ///   an opening paren first.
        /// </summary>
        [TestMethod]
        [TestCategory( "6" )]
        public void FormulaConstructor_TestClosingWithoutOpening_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5 )" ) );
        }
    
        /// <summary>
        ///   Test that the number of closing parens cannot be larger than
        ///   the number of opening parens already seen.
        /// </summary>
        [TestMethod]
        [TestCategory( "7" )]
        public void FormulaConstructor_TestClosingAfterBalanced_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "(5 + 5))" ) );
        }
    
        /// <summary>
        ///   Test that even when "balanced", the order of parens must be correct.
        /// </summary>
        [TestMethod]
        [TestCategory( "8" )]
        public void FormulaConstructor_TestClosingBeforeOpening_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5)(" ) );
        }
    
        /// <summary>
        ///   Make sure multiple/nested parens that are correct, are accepted.
        /// </summary>
        [TestMethod]
        [TestCategory( "9" )]
        public void FormulaConstructor_TestValidComplexParens_Succeeds( )
        {
            _ = new Formula( "(5 + ((3+2) - 5 / 2))" );
        }
    
        // --- Test Balanced Parentheses Rule ---
    
        /// <summary>
        ///   Make sure that an unbalanced parentheses set throws an exception.
        /// </summary>
        [TestMethod]
        [TestCategory( "10" )]
        public void FormulaConstructor_TestUnclosedParens_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "(5 + 2" ) );
        }
    
        /// <summary>
        ///   Test that multiple sets of balanced parens work properly.
        /// </summary>
        [TestMethod]
        [TestCategory( "11" )]
        public void FormulaConstructor_TestManyParens_Succeeds( )
        {
            _ = new Formula( "(1 + 2) - (1 + 2) - (1 + 2)" );
        }
    
        /// <summary>
        ///   Test that lots of balanced nested parentheses are accepted.
        /// </summary>
        [TestMethod]
        [TestCategory( "12" )]
        public void FormulaConstructor_TestDeeplyNestedParens_Succeeds( )
        {
            _ = new Formula( "(((5)))" );
        }
    
        // --- Test First Token Rule ---
    
        /// <summary>
        ///   The first token cannot be a closing paren.
        /// </summary>
        [TestMethod]
        [TestCategory( "13" )]
        public void FormulaConstructor_TestInvalidFirstTokenClosingParen_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( ")" ) );
        }
    
        /// <summary>
        ///   Test that the first token cannot be a math operator (+).
        /// </summary>
        [TestMethod]
        [TestCategory( "14" )]
        public void FormulaConstructor_TestInvalidFirstTokenPlus_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "+" ) );
        }
    
        /// <summary>
        ///   Test that the first token cannot be a math operator (*).
        /// </summary>
        [TestMethod]
        [TestCategory( "15" )]
        public void FormulaConstructor_TestInvalidFirstTokenMultiply_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "*" ) );
        }
    
        /// <summary>
        ///   Test that an integer number can be a valid first token.
        /// </summary>
        [TestMethod]
        [TestCategory( "16" )]
        public void FormulaConstructor_TestValidFirstTokenInteger_Succeeds( )
        {
            _ = new Formula( "1" );
        }
    
        /// <summary>
        ///   Test that a floating point number can be a valid first token.
        /// </summary>
        [TestMethod]
        [TestCategory( "17" )]
        public void FormulaConstructor_TestValidFirstTokenFloat_Succeeds( )
        {
            _ = new Formula( "1.0" );
        }
    
        // --- Test Last Token Rule ---
    
        /// <summary>
        ///   Make sure the last token is valid, in this case, not an operator (plus).
        /// </summary>
        [TestMethod]
        [TestCategory( "18" )]
        public void FormulaConstructor_TestInvalidLastTokenPlus_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5 +" ) );
        }
    
        /// <summary>
        ///   Make sure the last token is valid, in this case, not a closing paren.
        /// </summary>
        [TestMethod]
        [TestCategory( "19" )]
        public void FormulaConstructor_TestInvalidLastTokenClosingParen_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5 (" ) );
        }
    
        // --- Test Parentheses/Operator Following Rule ---
    
        /// <summary>
        ///   Test that after an opening paren, there cannot be an invalid token, in this
        ///   case a math operator (+).
        /// </summary>
        [TestMethod]
        [TestCategory( "20" )]
        public void FormulaConstructor_TestOpAfterOpenParen_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "( + 2)" ) );
        }
    
        /// <summary>
        ///   Test that a closing paren cannot come after an opening paren.
        /// </summary>
        [TestMethod]
        [TestCategory( "21" )]
        public void FormulaConstructor_TestEmptyParens_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "()" ) );
        }
    
        // --- Test Extra Following Rule ---
    
        /// <summary>
        ///   Make sure that two consecutive numbers are invalid.
        /// </summary>
        [TestMethod]
        [TestCategory( "22" )]
        public void FormulaConstructor_TestConsecutiveNumbers_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5 5" ) );
        }
    
        /// <summary>
        ///   Test that two consecutive operators is invalid.
        /// </summary>
        [TestMethod]
        [TestCategory( "23" )]
        public void FormulaConstructor_TestConsecutiveOps_Fails( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5+-2" ) );
        }
    
        /// <summary>
        ///   Test that a closing paren cannot come after an operator (plus).
        /// </summary>
        [TestMethod]
        [TestCategory( "24" )]
        public void TestCloseParenAfterOp( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "(5+)2" ) );
        }
    
        /// <summary>
        ///   Test bad variable name.
        /// </summary>
        [TestMethod]
        [TestCategory( "25" )]
        public void FormulaConstructor_TestInvalidVariableName_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "a" ) );
        }
    
        /// <summary>
        ///   Test the get variables method for a simple single variable.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "26" )]
        public void GetVars_BasicVariable_ReturnsVariable( )
        {
            Formula f = new("2+X1");
            ISet<string> vars = f.GetVariables();
    
            Assert.IsTrue( vars.SetEquals( ["X1"] ) );
        }
    
        /// <summary>
        ///   Test that a formula with a number of distinct variables returns them all.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "27" )]
        public void GetVariables_ManyVariables_ReturnsThemAll( )
        {
            Formula f = new("X1+X2+X3+X4+A1+B1+C5");
            ISet<string> vars = f.GetVariables();
    
            Assert.IsTrue( vars.SetEquals( ["X1", "X2", "X3", "X4", "A1", "B1", "C5"] ) );
        }
    
        /// <summary>
        ///   Test that repeated use of a variable does not result
        ///   in more than one of those coming back.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "28" )]
        public void TestGetVars_ManySameVariable_ReturnsUniqueVariable( )
        {
            Formula f = new("X1+X1+X1+X1+X1+X1+X1+X1+X1+X1+X1");
            ISet<string> vars = f.GetVariables();
    
            Assert.IsTrue( vars.SetEquals( ["X1"] ) );
        }
    
        /// <summary>
        ///   Test that ToString works for the very basic case.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "29" )]
        public void ToString_BasicFormula_ReturnsSameFormula( )
        {
            Formula f1 = new("2+A1");
    
            Assert.IsTrue( f1.ToString().Equals( "2+A1" ) );
        }
    
        /// <summary>
        ///   Test that multiple forms of the same number evaluate
        ///   to the same number in the canonical form.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "30" )]
        public void ToString_Numbers_UsesCanonicalForm( )
        {
            Formula f1 = new("2.0000+A1");
            Assert.IsTrue( f1.ToString().Equals( "2+A1" ) );
            f1 = new( "2.0000-3" );
            Assert.IsTrue( f1.ToString().Equals( "2-3" ) );
            f1 = new( "2.0000-3e2" );
            Assert.IsTrue( f1.ToString().Equals( "2-300" ) );
            f1 = new( "1e20" );
            Assert.IsTrue( f1.ToString().Equals( "1E+20" ) );
        }
    
        /// <summary>
        ///   Test that spaces are ignored in the canonical ToString form.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "31" )]
        public void ToString_SpacesInFormula_SpacesRemoved( )
        {
            Formula f1 = new("        2             +                    A1          ");
            Assert.IsTrue( f1.ToString().Equals( "2+A1" ) );
        }
    
        /// <summary>
        ///   Test that variable names are normalized and that if that is the
        ///   only difference, the canonical forms are the same.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "32" )]
        public void NormalizerAndToString_LowerCaseAndUpperCase_ResultInSameString( )
        {
            Formula f1 = new("2+x1");
            Formula f2 = new("2+X1");
    
            Assert.IsTrue( f1.ToString().Equals( "2+X1" ) );
            Assert.IsTrue( f1.ToString().Equals( f2.ToString() ) );
        }
    
        /// <summary>
        ///   Test that a variable normalizes to capitalization.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "32" )]
        public void NormalizerAndGetVars_LowerCaseVariable_UpCasesVariable( )
        {
            Formula f = new("2+x1");
            ISet<string> vars = f.GetVariables();
    
            Assert.IsTrue( vars.SetEquals( ["X1"] ) );
        }
    
        /// <summary>
        ///   Test that all variables are capitalized and returned in that
        ///   manner by GetVars.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "33" )]
        public void GetVars_ManyCaseSwappingVariables_UpCasesAll( )
        {
            Formula f = new("x1+X2+x3+X4+a1+B1+c5");
            ISet<string> vars = f.GetVariables();
    
            Assert.IsTrue( vars.SetEquals( ["X1", "X2", "X3", "X4", "A1", "B1", "C5"] ) );
        }
    
        // Some general syntax errors detected by the constructor
    
        /// <summary>
        ///   Test that a single operator is an invalid formula.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "34" )]
        public void FormulaConstructor_SingleOperator_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "+" ) );
        }
    
        /// <summary>
        ///    Test that the equation cannot end with an operator.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "35" )]
        public void FormulaConstructor_ExtraOperator_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "2+5+" ) );
        }
    
        /// <summary>
        ///   Test that an unmatched parentheses causes an error.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "36" )]
        public void FormulaConstructor_ExtraCloseParen_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "2+5*7)" ) );
        }
    
        /// <summary>
        ///   Test that too many opening parentheses fail.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "37" )]
        public void FormulaConstructor_ExtraOpenParen_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "((3+5*7)" ) );
        }
    
        /// <summary>
        ///   Test that x is not a multiplication token. In this case
        ///   we really should get two tokens, the number 5 and the variable x5, but this
        ///   is an invalid formula.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "38" )]
        public void FormulaConstructor_XAsMultiply_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5x5" ) );
        }
    
        /// <summary>
        ///   Test that 5x is an invalid name of a variable (really
        ///   5+5 is valid, but x is an invalid variable).
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "39" )]
        public void FormulaConstructor_InvalidVariableName_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5+5x" ) );
        }
    
        /// <summary>
        ///   Cannot have implicit multiplication: (5)8.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "40" )]
        public void FormulaConstructor_ImplicitMultiplication_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5+7+(5)8" ) );
        }
    
        /// <summary>
        ///   Cannot have two operators in a row (likely user missed
        ///   a number between them :^).
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "42" )]
        public void FormulaConstructor_TwoOperatorsInARow_Throws( )
        {
            Assert.ThrowsExactly<FormulaFormatException>( ( ) => _ = new Formula( "5 + + 3" ) );
        }
    
        // Some more complicated formula evaluations
    
        /// <summary>
        ///   A large expression with lots of operators, numbers, and parens.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "43" )]
        public void FormulaConstructor_TestComplex_IsValid( )
        {
            _ = new Formula( "y1*3-8/2+4*(8-9*2)/14*x7" );
        }
    
        /// <summary>
        ///   Another large valid nested paren structure with lots of closing parens at the end.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "44" )]
        public void FormulaConstructor_MatchingParens_EachLeftHasARight( )
        {
            _ = new Formula( "x1+(x2+(x3+(x4+(x5+x6))))" );
        }
    
        /// <summary>
        ///   A valid large expression with lots of opening parens at the
        ///   start.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "45" )]
        public void FormulaConstructor_LotsOfLeftParens_IsValidAndMatching( )
        {
            _ = new Formula( "((((x1+x2)+x3)+x4)+x5)+x6" );
        }
    
        /// <summary>
        ///   Whitespace should be removed from all formulas.  Test constructor and ToString.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "46" )]
        public void ToString_Whitespace_RemovedInCanonicalForm( )
        {
            Formula f1 = new("X1+X2");
            Formula f2 = new(" X1  +  X2   ");
            Assert.IsTrue( f1.ToString().Equals( f2.ToString() ) );
        }
    
        /// <summary>
        ///   Another test of number handling.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "47" )]
        public void ToString_DifferentNumberRepresentations_EquateToSameCanonicalForm( )
        {
            Formula f1 = new("2+X1*3.00");
            Formula f2 = new("2.00+X1*3.0");
            Assert.IsTrue( f1.ToString().Equals( f2.ToString() ) );
        }
    
        /// <summary>
        ///    Another test of canonical form, tested via ToString, but really
        ///    tests constructor as well.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "48" )]
        public void ToString_DifferentNumberRepresentations_EquateToSameCanonicalForm2( )
        {
            Formula f1 = new("1e-2 + X5 + 17.00 * 19 ");
            Formula f2 = new("   0.0100  +     X5+ 17 * 19.00000 ");
            Assert.IsTrue( f1.ToString().Equals( f2.ToString() ) );
        }
    
        /// <summary>
        ///   On the odd chance that the code states all formula are equivalent,
        ///   test that this is not true.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "49" )]
        public void ToString_DifferentFormulas_HaveDifferentStrings( )
        {
            Formula f1 = new("2");
            Formula f2 = new("5");
            Assert.AreNotEqual( f1.ToString(), f2.ToString() );
        }
    
        // Tests of GetVariables method
    
        /// <summary>
        ///   Lack of variables should return an empty set.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "50" )]
        public void GetVariables_NoVariables_ReturnsEmptySet( )
        {
            Formula f = new("2*5");
            Assert.IsFalse( f.GetVariables().Any() );
        }
    
        /// <summary>
        ///   A few more tests that a single variable formula should
        ///   return one variable.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "51" )]
        public void GetVariables_OneVariable_ReturnsTheOne( )
        {
            Formula f = new("2*X2");
            List<string> actual = [ .. f.GetVariables() ];
            HashSet<string> expected = ["X2"];
            Assert.HasCount( 1, actual );
            Assert.IsTrue( expected.SetEquals( actual ) );
        }
    
        /// <summary>
        ///   Another test to validate get variables returns
        ///   the expected variables.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "52" )]
        public void GetVariables_TwoVariables_ReturnsBoth( )
        {
            Formula f = new("2*X2+Y3");
            List<string> actual = [ .. f.GetVariables() ];
            HashSet<string> expected = ["Y3", "X2"];
            Assert.HasCount( 2, actual );
            Assert.IsTrue( expected.SetEquals( actual ) );
        }
    
        /// <summary>
        ///   Another version of the duplicated variable name and
        ///   get variables only returning it once.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "53" )]
        public void GetVariables_Duplicated_ReturnsOnlyOneValue( )
        {
            Formula f = new("2*X2+X2");
            List<string> actual = [ ..f.GetVariables() ];
            HashSet<string> expected = ["X2"];
            Assert.HasCount( 1, actual );
            Assert.IsTrue( expected.SetEquals( actual ) );
        }
    
        /// <summary>
        ///   Another example of a long list of multiple variables, some of which
        ///   are repeated, and each one is in the returned set only once.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "54" )]
        public void GetVariables_LotsOfVariablesWithOperatorsAndRepeats_ReturnsCompleteList( )
        {
            Formula f = new("X1+Y2*X3*Y2+Z7+X1/Z8");
            List<string> actual = [ .. f.GetVariables() ];
            HashSet<string> expected = ["X1", "Y2", "X3", "Z7", "Z8"];
            Assert.HasCount( 5, actual );
            Assert.IsTrue( expected.SetEquals( actual ) );
        }
    
        // Test some longish valid formulas.
    
        /// <summary>
        ///   A large but syntactically correct formula.
        ///   More tests for valid formulas.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "55" )]
        public void FormulaConstructor_LongComplexFormula_IsAValidFormula( )
        {
            _ = new Formula( "(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)" );
        }
    
        /// <summary>
        ///   Another long formula testing many of the properties of the parsing in the constructor.
        /// </summary>
        [TestMethod]
        [Timeout( 2000 )]
        [TestCategory( "56" )]
        public void FormulaConstructor_LongComplexFormula2_IsAValidFormula( )
        {
            _ = new Formula( "5 + (1-2) * 3.14 / 1e6 + 0.2E-9 - A1 + bb22" );
        }
    
    
    
}