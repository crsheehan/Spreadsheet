// <copyright file="Spreadsheet.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>


// Written by Joe Zachary for CS 3500, September 2013
// Update by Profs Kopta, de St. Germain, Martin, Fall 2021, Fall 2024, Fall 2025
//     - Updated return types
//     - Updated documentation
// Completed by Chancellor Sheehan, September 24th, 2025

using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace Spreadsheets;

using Formula;
using DependencyGraph;
using System.Text.Json;

/// <summary>
///   <para>
///     Thrown to indicate that a change to a cell will cause a circular dependency.
///   </para>
/// </summary>
public class CircularException : Exception
{
}

/// <summary>
/// <para>
///   Thrown to indicate that a read or write attempt has failed with
///   an expected error message informing the user of what went wrong.
/// </para>
/// </summary>
public class SpreadsheetReadWriteException : Exception
{
    /// <summary>
    ///   <para>
    ///     Creates the exception with a message defining what went wrong.
    ///   </para>
    /// </summary>
    /// <param name="msg"> An informative message to the user. </param>
    public SpreadsheetReadWriteException(string msg)
        : base(msg)
    {
    }
}

/// <summary>
///   <para>
///     Thrown to indicate that a name parameter was invalid.
///   </para>
/// </summary>
public class InvalidNameException : Exception
{
}

/// <summary>
///   <para>
///     A Spreadsheet object represents the state of a simple spreadsheet.  A
///     spreadsheet represents an infinite number of named cells.
///   </para>
/// <para>
///     Valid Cell Names: A string is a valid cell name if and only if it is one or
///     more letters followed by one or more numbers, e.g., A5, BC27.
/// </para>
/// <para>
///    Cell names are case-insensitive, so "x1" and "X1" are the same cell name.
///    Your code should normalize (uppercased) any stored name but accept either.
/// </para>
/// <para>
///     A spreadsheet represents a cell corresponding to every possible cell name.  (This
///     means that a spreadsheet contains an infinite number of cells.)  In addition to
///     a name, each cell has a contents and a value.  The distinction is important.
/// </para>
/// <para>
///     The <b>contents</b> of a cell can be (1) a string, (2) a double, or (3) a Formula.
///     If the contents of a cell is set to the empty string, the cell is considered empty.
/// </para>
/// <para>
///     By analogy, the contents of a cell in Excel is what is displayed on
///     the editing line when the cell is selected.
/// </para>
/// <para>
///     In a new spreadsheet, the contents of every cell is the empty string. Note:
///     this is by definition (it is IMPLIED, not stored).
/// </para>
/// <para>
///     The <b>value</b> of a cell can be (1) a string, (2) a double, or (3) a FormulaError.
///     (By analogy, the value of an Excel cell is what is displayed in that cell's position
///     in the grid.) We are not concerned with cell values yet, only with their contents,
///     but for context:
/// </para>
/// <list type="number">
///   <item>If a cell's contents is a string, its value is that string.</item>
///   <item>If a cell's contents is a double, its value is that double.</item>
///   <item>
///     <para>
///       If a cell's contents is a Formula, its value is either a double or a FormulaError,
///       as reported by the Evaluate method of the Formula class.  For this assignment,
///       you are not dealing with values yet.
///     </para>
///   </item>
/// </list>
/// <para>
///     Spreadsheets are never allowed to contain a combination of Formulas that establish
///     a circular dependency.  A circular dependency exists when a cell depends on itself,
///     either directly or indirectly.
///     For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
///     A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
///     dependency.
/// </para>
/// </summary>
public class Spreadsheet
{
    #region Fields and Intance Variables

    /// <summary>
    /// Keeps track of dependencies in Spreadsheet
    /// </summary>
    private DependencyGraph _dependencies;

    /// <summary>
    /// Is a dictionary of all existing cells
    /// </summary>
    [JsonInclude] [JsonPropertyName("Cells")] private Dictionary<string, Cell> _cells;

    /// <summary>
    /// True if this spreadsheet has been changed since it was
    /// created or saved (whichever happened most recently),
    /// False otherwise.
    /// </summary>
    [JsonIgnore]
    public bool Changed { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor. Creates an empty spreadsheet in which every cell ins empty. 
    /// </summary>
    public Spreadsheet()
    {
        this._dependencies = new DependencyGraph();
        this._cells = new Dictionary<string, Cell>();
        this.Changed = false;
    }

    /// <summary>
    /// Constructs a spreadsheet using the saved data in the file referred to by
    /// the given filename.
    /// <see cref="Save(string)"/>
    /// </summary>
    /// <exception cref="SpreadsheetReadWriteException">
    ///   Thrown if the file can not be loaded into a spreadsheet for any reason
    /// </exception>
    /// <param name="filename">The path to the file containing the spreadsheet to load</param>
    public Spreadsheet(string filename)
    {
        _dependencies = new DependencyGraph();
        _cells = new Dictionary<string, Cell>();
        SetContentsFromJson(File.ReadAllText(filename));
    }

    #endregion

    #region public methods

    /// <summary>
    /// Sets the content of the spreadsheet to the given json 
    /// </summary>
    /// <param name="json">json of the spreadsheet object that is a dictornay of cell with their names and string values</param>
    /// <exception cref="SpreadsheetReadWriteException"></exception>
    public void SetContentsFromJson(string json)
    {
        _dependencies = new DependencyGraph();
        try
        {
            Spreadsheet sheet = JsonSerializer.Deserialize<Spreadsheet>(json)!;
            Dictionary<string, Cell> cells = sheet._cells;
            foreach (KeyValuePair<string, Cell> dataPair in cells)
            {
                SetContentsOfCell(dataPair.Key, dataPair.Value.StringForm);
            }

        }
        catch (Exception e)
        {
            throw new SpreadsheetReadWriteException(e.Message);
        }
    }
    /// <summary>
    /// Gets this spreadsheet as a json string
    /// </summary>
    /// <returns>Json representation of the spreadsheet, that being a Dictionary of non-empty cells with their names and string values </returns>
    public string GetJson()
    {
        return JsonSerializer.Serialize(this);
    }
    /// <summary>
    ///   <para>
    ///     Return the value of the named cell, as defined by
    ///     <see cref="GetCellValue(string)"/>.
    ///   </para>
    /// </summary>
    /// <param name="name"> The cell in question. </param>
    /// <returns>
    ///   <see cref="GetCellValue(string)"/>
    /// </returns>
    /// <exception cref="InvalidNameException">
    ///   If the provided name is invalid, throws an InvalidNameException.
    /// </exception>
    public object this[string name] => GetCellValue(name);


    /// <summary>
    ///   Provides a copy of the normalized names of all the cells in the spreadsheet
    ///   that contain information (i.e., non-empty cells).
    /// </summary>
    /// <returns>
    ///   A set of the names of all the non-empty cells in the spreadsheet.
    /// </returns>
    public ISet<string> GetNamesOfAllNonemptyCells()
    {
        return _cells.Keys.ToHashSet();
    }


    /// <summary>
    ///   Returns the contents (as opposed to the value) of the named cell.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   Thrown if the name is invalid.
    /// </exception>
    ///
    /// <param name="name">The name of the spreadsheet cell to query. </param>
    /// <returns>
    ///   The contents as either a string, a double, or a Formula.
    ///   See the class header summary.
    /// </returns>
    public object GetCellContents(string name)
    {
        //Check if name is Valid
        var valName = IsValidName(name);

        //If name is key in cell dictionary return value, if not, return ""
        return _cells.TryGetValue(valName, out var cell)
            ? cell.Contents
            : "";
    }


    /// <summary>
    /// Saves this spreadsheet to a file
    /// </summary>
    /// <param name="filename"> The name (with path) of the file to save to.</param>
    /// <exception cref="SpreadsheetReadWriteException">
    ///   If there are any problems opening, writing, or closing the file,
    ///   the method should throw a SpreadsheetReadWriteException with an
    ///   explanatory message.
    /// </exception>
    public void Save(string filename)
    {
        try
        {
            // Serialize the dictionary to JSON
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });


            // Write the JSON to the file
            File.WriteAllText(filename, json);

            // Reset changed flag since we just saved
            this.Changed = false;
        }
        catch (Exception ex)
        {
            throw new SpreadsheetReadWriteException(ex.Message);
        }
    }

    /// <summary>
    ///   <para>
    ///     Return the value of the named cell.
    ///   </para>
    /// </summary>
    /// <param name="name"> The cell in question. </param>
    /// <returns>
    ///   Returns the value (as opposed to the contents) of the named cell.  The return
    ///   value should be either a string, a double, or a CS3500.Formula.FormulaError.
    /// </returns>
    /// <exception cref="InvalidNameException">
    ///   If the provided name is invalid, throws an InvalidNameException.
    /// </exception>
    public object GetCellValue(string name)
    {
        // Check if the cell name is valid, if not, throw exception
        var correctName = IsValidName(name);

        // Check if the cell exists in our dictionary
        return !_cells.TryGetValue(correctName, out var cell)
            ? "" // By definition, empty cells have value ""
            : cell.Value; // Return the cell's value
    }

    /// <summary>
    ///   <para>
    ///     Set the contents of the named cell to be the provided string
    ///     which will either represent (1) a string, (2) a number, or
    ///     (3) a formula (based on the prepended '=' character).
    ///   </para>
    ///   <para>
    ///     Rules of parsing the input string:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>
    ///       <para>
    ///         If 'content' parses as a double, the contents of the named
    ///         cell becomes that double.
    ///       </para>
    ///     </item>
    ///     <item>
    ///         If the string does not begin with an '=', the contents of the
    ///         named cell becomes 'content'.
    ///     </item>
    ///     <item>
    ///       <para>
    ///         If 'content' begins with the character '=', an attempt is made
    ///         to parse the remainder of content into a Formula f using the Formula
    ///         constructor.  There are then three possibilities:
    ///       </para>
    ///       <list type="number">
    ///         <item>
    ///           If the remainder of content cannot be parsed into a Formula, a
    ///           CS3500.Formula.FormulaFormatException is thrown.
    ///         </item>
    ///         <item>
    ///           Otherwise, if changing the contents of the named cell to be f
    ///           would cause a circular dependency, a CircularException is thrown,
    ///           and no change is made to the spreadsheet.
    ///         </item>
    ///         <item>
    ///           Otherwise, the contents of the named cell becomes f.
    ///         </item>
    ///       </list>
    ///     </item>
    ///   </list>
    /// </summary>
    /// <returns>
    ///   <para>
    ///     The method returns a list consisting of the name plus the names
    ///     of all other cells whose value depends, directly or indirectly,
    ///     on the named cell. The order of the list should be any order
    ///     such that if cells are re-evaluated in that order, their dependencies
    ///     are satisfied by the time they are evaluated.
    ///   </para>
    ///   <example>
    ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    ///     list {A1, B1, C1} is returned.
    ///   </example>
    /// </returns>
    /// <exception cref="InvalidNameException">
    ///     If name is invalid, throws an InvalidNameException.
    /// </exception>
    /// <exception cref="CircularException">
    ///     If a formula results in a circular dependency, throws CircularException.
    /// </exception>
    public IList<string> SetContentsOfCell(string name, string content)
    {
        //Create variable to return and normalized name
        IList<string> toReturn;
        var correctName = IsValidName(name);

        //Figure out what content is: a double, a text string, or a formula. If it's a formula, the string will begin with "="
        //First, check if strings begins with "=" with string.StartsWith
        if (content.StartsWith("="))
        {
            toReturn = SetCellContents(correctName,
                new Formula(content
                    .TrimStart('='))); //Call SetCellContents with Formula excluding the = at the beginning. 
        }
        else if
            (double.TryParse(content,
                 out var val)) //try to parse content as a double, then call SetCellContents if it works
        {
            toReturn = SetCellContents(correctName, val);
        }
        else //string is just text
        {
            toReturn = SetCellContents(correctName, content);
        }

        // Recalculate all dependent cells
        RecalculateCells(toReturn);
        
        //Set changed to true and return
        this.Changed = true;
        return toReturn;
    }

    #endregion

    #region Private Helper Methods

    
    private void RecalculateCells(IList<string> cellsToRecalculate)
    {
        foreach (var cellName in cellsToRecalculate)
        {
            if (_cells.TryGetValue(cellName, out var cell))
            {
                // Only recalculate if the cell contains a formula
                if (cell.Contents is Formula formula)
                {
                    var newValue = formula.Evaluate(CellLookup);
                    _cells[cellName] = new Cell(formula, newValue);
                }
                // For doubles and strings, value = contents (already set correctly)
            }
        }
    }
    
    /// <summary>
    ///  Set the contents of the named cell to the given number.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    ///
    /// <param name="name"> The name of the cell. </param>
    /// <param name="number"> The new contents of the cell. </param>
    /// <returns>
    ///   <para>
    ///     This method returns an ordered list consisting of the passed in name
    ///     followed by the names of all other cells whose value depends, directly
    ///     or indirectly, on the named cell.
    ///   </para>
    ///   <para>
    ///     The order must correspond to a valid dependency ordering for recomputing
    ///     all the cells, i.e., if you re-evaluate each cell in the order of the list,
    ///     the overall spreadsheet will be correctly updated.
    ///   </para>
    ///   <para>
    ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
    ///     list [A1, B1, C1] is returned, i.e., A1 was changed, so then A1 must be
    ///     evaluated, followed by B1, followed by C1.
    ///   </para>
    /// </returns>
    private IList<string> SetCellContents(string name, double number)
    {
        //Checks to see if name is a valid name, if not, throw InvalidNameException
        var cellName = IsValidName(name);

        //Remove any dependencies this cell might have
        _dependencies.ReplaceDependees(cellName, new List<string>());

        //Set cellContents to number
        _cells[cellName] = new Cell(number, number);

        return GetCellsToRecalculate(cellName).ToList();
    }


    /// <summary>
    ///   The contents of the named cell becomes the given text.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    /// <param name="name"> The name of the cell. </param>
    /// <param name="text"> The new contents of the cell. </param>
    /// <returns>
    ///   The same list as defined in <see cref="SetCellContents(string, double)"/>.
    /// </returns>
    private IList<string> SetCellContents(string name, string text)
    {
        //if setting cell to empty string, do nothing!
        if (text.Equals(string.Empty))
        {
            //Remove from cell dictionary
            _cells.Remove(name);

            return new List<string>();
        }

        //Checks to see if name is a valid name, if not, throw InvalidNameException
        var cellName = IsValidName(name);

        //Remove any dependencies this cell might have
        _dependencies.ReplaceDependees(cellName, new List<string>());

        //Set cellContents to text
        _cells[cellName] = new Cell(text, text);

        return GetCellsToRecalculate(cellName).ToList();
    }


    /// <summary>
    ///   Set the contents of the named cell to the given formula.
    /// </summary>
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    /// <exception cref="CircularException">
    ///   <para>
    ///     If changing the contents of the named cell to be the formula would
    ///     cause a circular dependency, throw a CircularException, and no
    ///     change is made to the spreadsheet.
    ///   </para>
    /// </exception>
    /// <param name="name"> The name of the cell. </param>
    /// <param name="formula"> The new contents of the cell. </param>
    /// <returns>
    ///   The same list as defined in <see cref="SetCellContents(string, double)"/>.
    /// </returns>
    private IList<string> SetCellContents(string name, Formula formula)
    {
        //Check if nam eis valid, then get old dependencies before changing
        var cellName = IsValidName(name);
        var oldDependees = _dependencies.GetDependees(cellName).ToList();

        //Replace dependencies!
        _dependencies.ReplaceDependees(cellName, formula.GetVariables());

        try
        {
            var cellsToRecalculate = GetCellsToRecalculate(cellName).ToList();

            // Add cell with formula as both content and initial value
            // We'll recalculate it properly after
            // _cells[cellName] = new Cell(formula, new FormulaError("Not yet evaluated"));

            // Now evaluate with the cell in place
            var cellValue = formula.Evaluate(CellLookup);
            _cells[cellName] = new Cell(formula, cellValue);

            return cellsToRecalculate;
        }
        catch (CircularException)
        {
            //Reset dependencies if fails
            _dependencies.ReplaceDependees(cellName, oldDependees);
            throw;
        }
    }

    /// <summary>
    ///   Returns an enumeration, without duplicates, of the names of all cells whose
    ///   values depend directly on the value of the named cell.
    /// </summary>
    /// <param name="name"> This <b>MUST</b> be a valid name.  </param>
    /// <returns>
    ///   <para>
    ///     Returns an enumeration, without duplicates, of the names of all cells
    ///     that contain formulas containing name.
    ///   </para>
    ///   <para>For example, suppose that: </para>
    ///   <list type="bullet">
    ///      <item>A1 contains 3</item>
    ///      <item>B1 contains the formula A1 * A1</item>
    ///      <item>C1 contains the formula B1 + A1</item>
    ///      <item>D1 contains the formula B1 - C1</item>
    ///   </list>
    ///   <para> The direct dependents of A1 are B1 and C1. </para>
    /// </returns>
    private IEnumerable<string> GetDirectDependents(string name)
    {
        //Check if name is valid, then return Dependents!
        var cellName = IsValidName(name);
        return _dependencies.GetDependents(cellName);
    }


    /// <summary>
    ///   <para>
    ///     This method is implemented for you, but makes use of your GetDirectDependents.
    ///   </para>
    ///   <para>
    ///     Returns an enumeration of the names of all cells whose values must
    ///     be recalculated, assuming that the contents of the cell referred
    ///     to by name has changed.  The cell names are enumerated in an order
    ///     in which the calculations should be done.
    ///   </para>
    ///   <exception cref="CircularException">
    ///     If the cell referred to by name is involved in a circular dependency,
    ///     throws a CircularException.
    ///   </exception>
    ///   <para>
    ///     For example, suppose that:
    ///   </para>
    ///   <list type="number">
    ///     <item>
    ///       A1 contains 5
    ///     </item>
    ///     <item>
    ///       B1 contains the formula A1 + 2.
    ///     </item>
    ///     <item>
    ///       C1 contains the formula A1 + B1.
    ///     </item>
    ///     <item>
    ///       D1 contains the formula A1 * 7.
    ///     </item>
    ///     <item>
    ///       E1 contains 15
    ///     </item>
    ///   </list>
    ///   <para>
    ///     If A1 has changed, then A1, B1, C1, and D1 must be recalculated,
    ///     and they must be recalculated in an order which has A1 first, and B1 before C1
    ///     (there are multiple such valid orders).
    ///     The method will produce one of those enumerations.
    ///   </para>
    ///   <para>
    ///      PLEASE NOTE THAT THIS METHOD DEPENDS ON THE METHOD GetDirectDependents.
    ///      IT WON'T WORK UNTIL GetDirectDependents IS IMPLEMENTED CORRECTLY.
    ///   </para>
    /// </summary>
    /// <param name="name"> The name of the cell.  Requires that name be a valid cell name.</param>
    /// <returns>
    ///    Returns an enumeration of the names of all cells whose values must
    ///    be recalculated.
    /// </returns>
    private IEnumerable<string> GetCellsToRecalculate(string name)
    {
        LinkedList<string> changed = new();
        HashSet<string> visited = [];
        Visit(name, name, visited, changed);
        return changed;
    }


    /// <summary>
    ///   A helper for the GetCellsToRecalculate method.
    ///   Almost like a depth first search used to see which cells need to be recalculated after a cell is changed.
    /// </summary>
    private void Visit(string start, string name, ISet<string> visited, LinkedList<string> changed)
    {
        //This marks the first cell (name) as visited. (adds to visited set).
        visited.Add(name);

        //This looks at each cell that depends on name cell
        foreach (var n in GetDirectDependents(name))
        {
            // Checks for circular dependencies
            if (n.Equals(start))
            {
                throw new CircularException();
            }
            //If the cell has NOT been visited, call visit recursively on dependent
            else if (!visited.Contains(n))
            {
                Visit(start, n, visited, changed);
            }
        }

        //Add current cell to front of changed list
        changed.AddFirst(name);
    }


    /// <summary>
    /// Determines if a string is a valid name for a cell. Uses Variable regex from formula class.
    /// </summary>
    /// <param name="name">Name to check if valid</param>
    /// <returns> Returns a normalized version of name, if name is not valid: returns an InvalidNameException</returns>
    private static string IsValidName(string name)
    {
        if (name is null or "") throw new InvalidNameException();
        var varRegex = new Regex(@"^[a-zA-Z]+\d+$");
        return varRegex.IsMatch(name)
            ? name.ToUpper()
            : throw new InvalidNameException();
    }


    private double CellLookup(string cellName)
    {
        //Check if valid name always!
        string name = IsValidName(cellName);

        //see if it's in our dictionary
        if (!_cells.TryGetValue(name, out var cell))
        {
            // Cell is empty
            throw
                new ArgumentException(
                    "Empty Cell"); //This needs to be an argument exception because that's what our evaluate method is looking to catch! 
        }

        // Check if the value is actually a double
        if (cell.Value is double doubleValue)
        {
            return doubleValue;
        }

        // Cell isn't a number
        throw new ArgumentException("Cell isn't a number");
    }

    #endregion


    /// <summary>
    /// Cell object to be used in spreadsheet. A cell can contain either a string, double, or a formula.
    /// NOTE: IT CAN ONLY CONTAIN ONE OF THESE, NOT MULTIPLE
    /// </summary>
    private class Cell
    {
        /// <summary>
        /// This sets the cell contents to whatever is constructed. However, this type can only be a string, double, or formula. Normally I
        /// would check this in the constructor, however the only situation in which a cell is being created is through the SetCellContent methods
        /// that only take a Formula, double, or string, so we already have that guarantee!
        /// </summary>

        
        [JsonIgnore] public object Contents { get; set; }

        [JsonIgnore] public object Value { get; set; }

        [JsonInclude] public string StringForm { get; set; }

        /// <summary>
        /// Parameterless constructor to make an empty cell. (used for JSON deserialization?)
        /// </summary>
        public Cell()
        {
            Contents = "";
            Value = "";
            StringForm = "";
        }

        /// <summary>
        /// This is a constructor for a Cell that is either going to have a text or double value
        /// </summary>
        /// <param name="content">content of cell</param>
        /// <param name="value">value of cell</param>
        public Cell(object content, object value)
        {
            Contents = content;
            Value = value;

            
            //Depending on type of content, we set StringForm to string, a string double, for formula ("=A1 + 2")
            StringForm = (content switch
            {
                null => string.Empty,
                double => content.ToString(),
                Formula => "=" + content,
                _ => content.ToString()
            })!;
        }
    }
}