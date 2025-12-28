// <summary>
//   <para>
//     This code is provided to start your assignment.  It was written
//     by Profs Joe, Danny, Jim, and Travis.  You should keep this attribution
//     at the top of your code where you have your header comment, along
//     with any other required information.
//   </para>
// </summary>

using System.Text.RegularExpressions;

namespace Formula;

/// <summary>
///     <para>
///         This class represents formulas written in standard infix notation using standard precedence
///         rules.  The allowed symbols are non-negative numbers written using double-precision
///         floating-point syntax; variables that consist of one or more letters followed by
///         one or more numbers; parentheses; and the four operator symbols +, -, *, and /.
///     </para>
///     <para>
///         Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
///         a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
///         and "x 23" consists of a variable "x" and a number "23".  Otherwise, spaces are to be removed.
///     </para>
///     <para>
///         For Assignment Two, you are to implement the following functionality:
///     </para>
///     <list type="bullet">
///         <item>
///             Formula Constructor which checks the syntax of a formula.
///         </item>
///         <item>
///             Get Variables
///         </item>
///         <item>
///             ToString
///         </item>
///     </list>
/// </summary>
public class Formula
{
    
    /// <summary>
    ///     All variables are letters followed by numbers.  This pattern
    ///     represents valid variable name strings.
    /// </summary>
    private const string VariableRegExPattern = @"[a-zA-Z]+\d+";

    private readonly Regex _numberRegex = new(@"^(\d+(\.\d+)?|\.\d+)([eE][+-]?\d+)?$");
    private readonly Regex _operatorRegex = new("^[+\\-*/]$");

    private readonly string _stringRep;

    /// <summary>
    ///     A private variable that holds a list of tokens in this formula.
    /// </summary>
    private readonly List<string> _tokens;
    
    /// <summary>
    ///     Initializes a new instance of the <see cref="Formula" /> class.
    ///     <para>
    ///         Creates a Formula from a string that consists of an infix expression written as
    ///         described in the class comment.  If the expression is syntactically incorrect,
    ///         throws a FormulaFormatException with an explanatory Message.  See the assignment
    ///         specifications for the syntax rules you are to implement.
    ///     </para>
    ///     <para>
    ///         Non-Exhaustive Example Errors:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             Invalid variable name, e.g., x, x1x  (Note: x1 is valid, but would be normalized to X1)
    ///         </item>
    ///         <item>
    ///             Empty formula, e.g., string.Empty
    ///         </item>
    ///         <item>
    ///             Mismatched Parentheses, e.g., "(("
    ///         </item>
    ///         <item>
    ///             Invalid Following Rule, e.g., "2x+5"
    ///         </item>
    ///     </list>
    /// </summary>
    /// <param name="formula"> The string representation of the formula to be created.</param>
    public Formula(string formula)
    {
        //Initializing Instance variables. 
        _tokens = new List<string>(GetTokens(formula));
        var openParen = 0;
        var closeParen = 0;
        _stringRep = string.Empty;

        //Check One token rule (make sure there is at least one token)
        if (_tokens.Count == 0) throw new FormulaFormatException("There must be at least one token in formula");

        //Assigns a previous token variable to check Parenthesis/Operator Following Rule and Extra Following rule. 
        var prevToken = string.Empty;

        //Check with foreach loop that each rule applies to every token. 
        foreach (var token in _tokens)
        {
            //Checks if each token is valid. 
            if (!(IsNum(token) || IsVar(token) || IsOperator(token) || token == "(" || token == ")"))
                throw new FormulaFormatException($"Invalid token: {token}");

            //checks Operator/Parenthesis Following Rules AND First token rule
            ValidateFollowingRules(token, prevToken);

            //Add parenthesis to variables to check Balanced Parenthesis rule. 
            if (token.Equals("("))
                openParen++;
            else if (token.Equals(")")) closeParen++;

            //Checks closing parentheses rule (Number of closing is never greater than number of opening)
            if (closeParen > openParen)
                throw new FormulaFormatException("Imbalanced Parentheses. (Violates closing parentheses rule)");

            //Builds string representation of formula
            if (IsVar(token)) //if variable, capitalize!
                _stringRep += token.ToUpper();
            else if (IsNum(token)) //if number, parse to simplify decimals and scientific notation.
                _stringRep += double.Parse(token);
            else // Is operator, just add. 
                _stringRep += token;

            //Changes prev token to current
            prevToken = token;
        }

        //Check to make sure last token is either a number, variable, or a closing parenthesis.
        ValidateLastTokenRule();

        //Check Balanced Parenthesis rule, throws if number isn't equivalent
        if (openParen != closeParen)
            throw new FormulaFormatException("Imbalanced number of Opening and Closing Parenthesis");
    }

    /// <summary>
    ///     <para>
    ///         Returns a set of all the variables in the formula.
    ///     </para>
    ///     <remarks>
    ///         Important: no variable may appear more than once in the returned set, even
    ///         if it is used more than once in the Formula.
    ///         Variables should be returned in canonical form, having all letters converted
    ///         to uppercase.
    ///     </remarks>
    ///     <list type="bullet">
    ///         <item>new("x1+y1*z1").GetVariables() should return a set containing "X1", "Y1", and "Z1".</item>
    ///         <item>new("x1+X1"   ).GetVariables() should return a set containing "X1".</item>
    ///     </list>
    /// </summary>
    /// <returns> the set of variables (string names) representing the variables referenced by the formula. </returns>
    public ISet<string> GetVariables()
    {
        var vars = new HashSet<string>();
        foreach (var token in _tokens)
            //Checks to see if token is a variable, then uppercases it and adds to hashset. 
            if (IsVar(token))
            {
                var tokenToAdd = token.ToUpper();
                vars.Add(tokenToAdd);
            }

        return vars;
    }

    /// <summary>
    ///     <para>
    ///         Returns a string representation of a canonical form of the formula.
    ///     </para>
    ///     <para>
    ///         The string will contain no spaces.
    ///     </para>
    ///     <para>
    ///         If the string is passed to the Formula constructor, the new Formula f
    ///         will be such that this.ToString() == f.ToString().
    ///     </para>
    ///     <para>
    ///         All the variable and number tokens in the string will be normalized.
    ///         For numbers, this means that the original string token is converted to
    ///         a number using double.Parse or double.TryParse, then converted back to a
    ///         string using double.ToString.
    ///         For variables, this means all letters are uppercased.
    ///     </para>
    ///     <para>
    ///         For example:
    ///     </para>
    ///     <code>
    ///       new("x1 + Y1").ToString() should return "X1+Y1"
    ///       new("x1 + 5.0000").ToString() should return "X1+5".
    ///   </code>
    ///     <para>
    ///         This method should execute in O(1) time.
    ///     </para>
    /// </summary>
    /// <returns>
    ///     A canonical version (string) of the formula. All "equal" formulas
    ///     should have the same value here.
    /// </returns>
    public override string ToString()
    {
        return _stringRep;
    }
    
    /// <summary>
    ///   <para>
    ///     Reports whether f1 == f2, using the notion of equality from the <see cref="Equals"/> method.
    ///   </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are the same.</returns>
    public static bool operator ==( Formula f1, Formula f2 )
    {
        return f1.Equals(f2);
    }

    /// <summary>
    ///   <para>
    ///     Reports whether f1 != f2, using the notion of equality from the <see cref="Equals"/> method.
    ///   </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are not equal to each other.</returns>
    public static bool operator !=( Formula f1, Formula f2 )
    {
        return !f1.Equals(f2);
    }

    /// <summary>
    ///   <para>
    ///     Determines if two formula objects represent the same formula.
    ///   </para>
    ///   <para>
    ///     By definition, if the parameter is null or does not reference
    ///     a Formula Object then return false.
    ///   </para>
    ///   <para>
    ///     Two Formulas are considered equal if their canonical string representations
    ///     (as defined by ToString) are equal.
    ///   </para>
    /// </summary>
    /// <param name="obj"> The other object.</param>
    /// <returns>
    ///   True if the two objects represent the same formula.
    /// </returns>
    public override bool Equals( object? obj )
    {
        //If object is null or not a Formula object, return false
        if (!(obj is Formula)) 
        {
            return false;
        }        
       
        //Checks if this objects toString and other object toString are equivalent
        if (this.ToString().Equals(obj.ToString()))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///   <para>
    ///     Evaluates this Formula, using the lookup delegate to determine the values of
    ///     variables.
    ///   </para>
    ///   <remarks>
    ///     When the lookup method is called, it will always be passed a normalized (capitalized)
    ///     variable name.  The lookup method will throw an ArgumentException if there is
    ///     not a definition for that variable token.
    ///   </remarks>
    ///   <para>
    ///     If no undefined variables or divisions by zero are encountered when evaluating
    ///     this Formula, the numeric value of the formula is returned.  Otherwise, a
    ///     FormulaError is returned (with a meaningful explanation as the Reason property).
    ///   </para>
    ///   <para>
    ///     This method should never throw an exception.
    ///   </para>
    /// </summary>
    /// <param name="lookup">
    ///   <para>
    ///     Given a variable symbol as its parameter, lookup returns the variable's value
    ///     (if it has one) or throws an ArgumentException (otherwise).  This method will expect
    ///     variable names to be normalized.
    ///   </para>
    /// </param>
    /// <returns> Either a double or a FormulaError, based on evaluating the formula.</returns>
    public object Evaluate( Lookup lookup )
    {
        //Initialize a value and operator stack
        Stack<Double> valStack = new Stack<Double>();
        Stack<string> opStack = new Stack<string>();

        //Create current number that will be assigned differently depending on the type of the current token

        foreach (var token in this._tokens)
        {
            // If token is a number or variable
            if (IsNum(token) || IsVar(token))
            {
                // If token is variable, make it into a number with lookup,
                // if not, make it into a number with Double.Parse
                double curr;
                if (IsVar(token))
                {
                    try
                    {
                        curr = lookup(token.ToUpper());
                    }
                    catch (ArgumentException)
                    {
                        return new FormulaError("Variable Lookup Failed");
                    }
                }
                else curr = Double.Parse(token);
                
                // If * or / is on the top of op_stack, Apply operator
                if (opStack.Any() && (opStack.Peek() == "*" || opStack.Peek() == "/"))
                {
                    //check for divide by 0 error
                    if (opStack.Peek() == "/" && curr == 0)
                        return new FormulaError("Division by 0 Error");
                    ApplyMultiplicationOrDivision(valStack, opStack, curr);
                    continue;
                }
                valStack.Push(curr); // push token onto val_stack
            } 
            
            // If token is + or -
            else if (token is "+" or "-") 
            {
                ApplyAdditionOrSubtraction(valStack, opStack, token);
                // Push the current token onto the operator stack
                opStack.Push(token);
            }
            
            // If token is * or /
           else if (token is "*" or "/")
            {
                //Just push to the opStack
                opStack.Push(token);
            }
            
            // If token is a left Parenthesis
           else if (token is "(")
            {
                opStack.Push(token);
            }
            
            // If token is a right Parenthesis
            else if (token is ")")
            {
                //If + or - is at the top of opStack, Apply Addition or Subtraction
               ApplyAdditionOrSubtraction(valStack, opStack, token);
                
                //Pop "(" from opStack
                opStack.Pop();

                // if * or / is at the top of the operator stack,
                // pop the value stack twice and the operator stack once.
                // Apply the popped operator to the popped numbers. Push the result onto the value stack.

                if (opStack.Any() && (opStack.Peek() is "*" or "/"))
                {
                    //Get numbers and operator
                    var num1 = valStack.Pop();
                    var num2 = valStack.Pop();
                    var op =  opStack.Pop();

                    if (op == "*")
                    {
                        valStack.Push(num2 * num1);
                    }
                    else
                    {
                        if (num1 == 0)
                        {
                            return new FormulaError("Division by 0 Error");
                        }
                        else
                        {
                            valStack.Push(num2 / num1);
                        }
                    }
                }
            }
        }
        
        //Final check 
        if (opStack.Count == 0)
        {
            return valStack.Pop();
        }
        else
        {
            //Get last two numbers and last operator and evaluate
            var num1 = valStack.Pop();
            var num2 = valStack.Pop();
            var op =  opStack.Pop();

            if (op is "+")
            {
                return num2 + num1;
            }

            return num2 - num1;
        }
    }

    /// <summary>
    ///   <para>
    ///     Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    ///     case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    ///     randomly-generated unequal Formulas have the same hash code should be miniscule.
    ///   </para>
    /// </summary>
    /// <returns> The hashcode for the object. </returns>
    public override int GetHashCode( )
    {
        return this.ToString().GetHashCode();
    }
    
    
    // Private Helper Methods -------------------------------------------------------------------------------------
    
    /// <summary>
    ///     Validates that the  last token of an expression must be a number, a variable, or a closing parenthesis.
    /// </summary>
    private void ValidateLastTokenRule()
    {
        var lastToken = _tokens.Last();

        if (IsNum(lastToken) || IsVar(lastToken) || lastToken.Equals(")")) return;

        throw new FormulaFormatException("Invalid Last Token in Formula");
    } 
    
    /// <summary>
    ///     This private helper method ensures that the formula passes the:
    ///     <list type="bullet">
    ///         <item>First Token Rule</item>
    ///         <item>Parenthesis/Operator following rule</item>
    ///         <item>Extra Following rule</item>
    ///     </list>
    /// </summary>
    /// <param name="curr">A string that represents the current token the foreach loop in the constructor is on</param>
    /// <param name="prev">A string that represents the previous token the foreach loop in the constructor was on</param>
    /// <exception cref="FormulaFormatException">Thrown if token doesn't follow each of the rules listed above</exception>
    private void ValidateFollowingRules(string curr, string prev)
    {
        //checks first token rule. The first token of an expression must be a number, a variable, or an opening parenthesis.
        if (prev.Equals(string.Empty))
        {
            if (IsNum(curr) || IsVar(curr) || curr.Equals("(")) return;

            throw new FormulaFormatException("Invalid First Token in Formula");
        }


        //Checks Parenthesis/Operator following rule
        if (prev.Equals("(") || IsOperator(prev))
        {
            if (IsNum(curr) || IsVar(curr) || curr.Equals("(")) return;

            throw new FormulaFormatException("Invalid Token following Parenthesis/Operator");
        }

        //Checks Extra Following rule
        if (IsNum(prev) || IsVar(prev) || prev.Equals(")"))
        {
            if (!(IsOperator(curr) || curr.Equals(")")))
            {
                throw new FormulaFormatException(
                    "Invalid Token following number/Variable/Closing Parenthesis. (Extra Following Rule)");
            }
        }
    }
    
    /// <summary>
    ///     <para>
    ///         Given an expression, enumerates the tokens that compose it.
    ///     </para>
    ///     <para>
    ///         Tokens returned are:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>left paren</item>
    ///         <item>right paren</item>
    ///         <item>one of the four operator symbols</item>
    ///         <item>a string consisting of one or more letters followed by one or more numbers</item>
    ///         <item>a double literal</item>
    ///         <item>and anything that doesn't match one of the above patterns</item>
    ///     </list>
    ///     <para>
    ///         There are no empty tokens; white space is ignored (except to separate other tokens).
    ///     </para>
    /// </summary>
    /// <param name="formula"> A string representing an infix formula such as 1*B1/3.0. </param>
    /// <returns> The ordered list of tokens in the formula. </returns>
    private static List<string> GetTokens(string formula)
    {
        List<string> results = [];

        var lpPattern = @"\(";
        var rpPattern = @"\)";
        var opPattern = @"[\+\-*/]";
        var doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        var spacePattern = @"\s+";

        // Overall pattern
        var pattern = string.Format(
            "({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
            lpPattern,
            rpPattern,
            opPattern,
            VariableRegExPattern,
            doublePattern,
            spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (var s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                results.Add(s);

        return results;
    }
    
    /// <summary>
    ///     Reports whether "token" is a valid operator.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private bool IsOperator(string token)
    {
        return _operatorRegex.IsMatch(token);
    }
    
    /// <summary>
    ///     Reports whether "token" is a variable.  It must be one or more letters
    ///     followed by one or more numbers.
    /// </summary>
    /// <param name="token"> A token that may be a variable. </param>
    /// <returns> true if the string matches the requirements, e.g., A1 or a1. </returns>
    private static bool IsVar(string token)
    {
        // notice the use of ^ and $ to denote that the entire string being matched is just the variable
        var standaloneVarPattern = $"^{VariableRegExPattern}$";
        return Regex.IsMatch(token, standaloneVarPattern);
    }

    /// <summary>
    ///     Reports whether "token" is a valid number. This includes scientific notation.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private bool IsNum(string token)
    {
        return _numberRegex.IsMatch(token);
    }

   /// <summary>
   /// In the evaluate method, if the top of the operator stack is a * or /, then this method applies that operator
   /// and pushes the value to the valStack in Evaluate.
   /// </summary>
   /// <param name="valStack"> Stack of Values</param>
   /// <param name="opStack">Stack of Operators</param>
   /// <param name="curr"> Current token the Evaluate method is operating on</param>
    private static void ApplyMultiplicationOrDivision(Stack<double> valStack, Stack<String> opStack, double curr)
    {
        // pop value and operator
        Double poppedVal = valStack.Pop();
        var poppedOp = opStack.Pop();

        //check if operator is * or /, then apply operator
        if (poppedOp.Equals("*"))
        {
            valStack.Push(poppedVal * curr);
        }
        else
        {
            valStack.Push(poppedVal / curr);
        }
    }

   /// <summary>
   /// If + or - is at the top of the operator stack, pop valSTack twice and the opStack once,
   /// then apply the popped operator to the pooped numbers, then push the rsult onto the value stack. 
   /// </summary>
   /// <param name="valStack">Stack of Values</param>
   /// <param name="opStack">Stack of Operators</param>
   /// <param name="token">Current token the evaluate method is operating on.</param>
    private static void ApplyAdditionOrSubtraction(Stack<double> valStack, Stack<string> opStack, string token)
    {
        if (opStack.Any() && opStack.Peek() is "+" or "-")
        {
            var num1 = valStack.Pop();
            var num2 = valStack.Pop();
            var op = opStack.Pop();

            //if op is -
            if (op.Equals("-"))
            {
                valStack.Push(num2 - num1);
            }
            else
            {
                valStack.Push(num2 + num1);
            }
        }
    }
  
        // End of Private Helper Methods ----------------------------------------------------------------------------------- 
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public class FormulaError
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormulaError"/> class.
    ///   <para>
    ///     Constructs a FormulaError containing the explanatory reason.
    ///   </para>
    /// </summary>
    /// <param name="message"> Contains a message for why the error occurred.</param>
    public FormulaError( string message )
    {
        Reason = message;
    }

    /// <summary>
    ///  Gets the reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

/// <summary>
///   Any method meeting this type signature can be used for
///   looking up the value of a variable.
/// </summary>
/// <exception cref="ArgumentException">
///   If a variable name is provided that is not recognized by the implementing method,
///   then the method should throw an ArgumentException.
/// </exception>
/// <param name="variableName">
///   The name of the variable (e.g., "A1") to lookup.
/// </param>
/// <returns> The value of the given variable (if one exists). </returns>
public delegate double Lookup( string variableName );

/// <summary>
///     Used to report syntax errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FormulaFormatException" /> class.
    ///     <para>
    ///         Constructs a FormulaFormatException containing the explanatory message.
    ///     </para>
    /// </summary>
    /// <param name="message"> A developer defined message describing why the exception occured.</param>
    public FormulaFormatException(string message)
        : base(message)
    {
        // All this does is call the base constructor. No extra code needed.
    }
}