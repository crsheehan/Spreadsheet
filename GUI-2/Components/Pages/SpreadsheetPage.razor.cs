// <copyright file="SpreadsheetPage.razor.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Formula;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Spreadsheets;

namespace GUI.Components.Pages;

/// <summary>
/// This class represents a spreadsheet that manages cells, formulas, and how they interact with each other. Gives the user methods for interacting with cell contents and values. 
/// </summary>
public partial class SpreadsheetPage
{
    /// <summary>
    /// This is the bridge between javascript and C#. The browser is using javascript. 
    /// </summary>
    private DotNetObjectReference<SpreadsheetPage>? _dotNetReference;
    
    /// <summary>
    /// Default selected cell when the program first runs
    /// </summary>
    private string _selectedCell = "A1";

    /// <summary>
    /// Backing spreadsheet
    /// </summary>
    private Spreadsheet _spreadsheet = new Spreadsheet();

    /// <summary>
    /// Reference to the Cell Contents Input box
    /// </summary>
    private ElementReference _contentInputBoxReference;

    /// <summary>
    /// boolean that determines whether an error is shown
    /// </summary>
    private bool _showError = false;

    /// <summary>
    /// Message the error will show
    /// </summary>
    private string ErrorMessage;

    /// <summary>
    /// Contents of the cell that is currently selected
    /// </summary>
    private string _currentCellContent = "";

    /// <summary>
    /// Current row and column the user is on
    /// </summary>
    private int currRow;

    private int currCol;

    /// <summary>
    /// booleans to determine whether to display clear warning, and if the user confirms it
    /// </summary>
    private bool _clearWarning = false;

    private bool _confirmClear = false;

    /// <summary>
    /// boolean that determines if the user is currently typing a formula
    /// </summary>
    private bool _isInFormulaMode = false;

    /// <summary>
    /// Any cells the formula that is currently being typed is referencing. 
    /// </summary>
    private List<string> _referencedCells = new List<string>();

    /// <summary>
    /// Based on your computer, you could shrink/grow this value based on performance.
    /// </summary>
    private const int Rows = 50;

    /// <summary>
    /// Number of columns, which will be labeled A-Z.
    /// </summary>
    private const int Cols = 52;

    /// <summary>
    /// Provides an easy way to convert from an index to a letter (0 -> A)
    /// </summary>
    private char[] Alphabet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    /// <summary>
    /// Converts a Name to a index ie(0->A),(27->AA),(28->AB)
    /// </summary>
    /// <param name="colName">Letters in the name of the column in Cap letters </param>
    /// <returns></returns>
    private string ColumnName(int col)
    {
        col++;
        StringBuilder sb = new StringBuilder();

        int numOfLetters = 0;
        for (int n = 1, k = -1; n + k < col;)
        {
            k += n;
            n = 26 * n;

            numOfLetters++;
        }

        for (int i = numOfLetters; i > 0; i--)
        {
            int s = 1;
            if (i != 1)
            {
                s = (int)Math.Ceiling(col / (Math.Pow(26, i - 1)) - 1);
                sb.Append(Alphabet[s - 1]);
            }
            else
                sb.Append(Alphabet[col - 1]);

            col -= (int)Math.Pow(26, i - 1) * s;
        }

        return sb.ToString();
    }


    /// <summary>
    /// Converts a Name to a index ie(A->0),(AA->27),(AB->28)
    /// </summary>
    /// <param name="colName">Letters in the name of the column in Cap letters </param>
    /// <returns></returns>
    private int ColumnNumber(string colName)
    {
        int colNum = 0;
        for (int i = 0; i < colName.Length; i++)
        {
            char letter = colName[i];
            colNum += (letter - 64) * (int)Math.Pow(26, colName.Length - i - 1);
        }

        return colNum - 1;
    }


    /// <summary>
    ///   Gets or sets the name of the file to be saved
    /// </summary>
    private string FileSaveName { get; set; } = "Spreadsheet.sprd";


    /// <summary>
    ///   <para> Gets or sets the data for all the cells in the spreadsheet GUI. </para>
    ///   <remarks>Backing Store for HTML</remarks>
    /// </summary>
    private string[,] CellsBackingStore { get; set; } = new string[Rows, Cols];


    private void ContentsChanged(ChangeEventArgs e)
    {
        try //Had to wrap this whole method in a try catch for the case that an entered formula is invalid!
        {
            _currentCellContent = e.Value.ToString();

            IList<string> cellsToUpdate = _spreadsheet.SetContentsOfCell(_selectedCell, e.Value.ToString());

            // Update all effected cells in the backing store
            foreach (string cellName in cellsToUpdate)
            {
                // Parse the cell name to get row and column
                int splitIndex = cellName.IndexOfAny(['1', '2', '3', '4', '5', '6', '7', '8', '9']);
                string letters = cellName.Substring(0, splitIndex);
                int c = ColumnNumber(letters);

                string r = cellName.Substring(splitIndex);

                // Update the backing store with the new value
                CellsBackingStore[int.Parse(r) - 1, c] = _spreadsheet[cellName].ToString();
            }

            StateHasChanged();
        }
        catch (Exception ex)
        {
            // If there's an error, revert the cell back to its actual value
            CellsBackingStore[currRow, currCol] = _spreadsheet[_selectedCell]?.ToString() ?? "";

            // Catch any exception from invalid formulas, circular dependencies, etc.
            _showError = true;
            ErrorMessage = "Invalid Formula: " + ex.Message;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handler for when a cell is clicked
    /// </summary>
    /// <param name="row">The row component of the cell's coordinates</param>
    /// <param name="col">The column component of the cell's coordinates</param>
    private void CellClicked(int row, int col)
    {
        //Set currently selected cell to the cell the user clicked. 
        _selectedCell = ColumnName(col) + (row + 1);

        // Get the cell contents
        object contents = _spreadsheet.GetCellContents(_selectedCell);

        // If it's a Formula, add the "=" back because when you click away from a formula cell and then back to it, you need to re insert the "="
        if (contents is Formula.Formula)
        {
            _currentCellContent = "=" + contents;
        }
        else
        {
            _currentCellContent = contents.ToString();
        }

        //focus the user's mouse on the contents box
        _contentInputBoxReference.FocusAsync();
        currCol = col;
        currRow = row;

        //Set Formula mode to false and clear referenced cells!
        _isInFormulaMode = false;
        _referencedCells.Clear();
    }

    /// <summary>
    /// this method updates the cell to match what's being typed in the content box.
    /// Note: THIS DOES NOT ACTUALLY UPDATE THE SPREADSHEET, JUST WHAT'S BEING DISPLAYED IN THE CELL. 
    /// </summary>
    /// <param name="e">What the user is typing in the current cell. </param>
    private void ContentsInput(ChangeEventArgs e)
    {
        // Just update the current cell content for display
        _currentCellContent = e.Value?.ToString() ?? "";

        //Check if what the user is typing is a formula, if it is, then put into formula mode!
        _isInFormulaMode = _currentCellContent.StartsWith("=");

        // Get referenced cells if in formula mode using ParseCellReferences
        if (_isInFormulaMode)
        {
            _referencedCells = ParseCellReferences(_currentCellContent);
        }
        else
        {
            _referencedCells.Clear();
        }

        // Update the backing store to show what they're typing 
        CellsBackingStore[currRow, currCol] = _currentCellContent;

        StateHasChanged();
    }

    /// <summary>
    /// This gets a list of all of the names of the cells being referenced in the formula the user is currently typing
    /// </summary>
    /// <param name="formula">The formula the user is currently typing. </param>
    /// <returns>List of Cell names referenced by current formula. </returns>
    private List<string> ParseCellReferences(string formula)
    {
        var cells = new List<string>();
        if (string.IsNullOrEmpty(formula) || !formula.StartsWith("="))
            return cells;

        // use regex to check if formula being entered is a valid cell name. (Matches gets ALL parts of the string that matches the given pattern)
        var matches = Regex.Matches(formula, @"\b([a-zA-Z]+[0-9]+)\b");

        //Loop through matches and add to reference list if not already in it (in case the user references the same cell twice in the same formula. 
        foreach (Match match in matches)
        {
            string cellRef = match.Value.ToUpper();
            if (!cells.Contains(cellRef))
                cells.Add(cellRef);
        }

        return cells;
    }

    /// <summary>
    /// This cell determines if a cell is either currently being clicked, or if it's being referenced by a formula, and returns a string of the name of a css class to apply to the cell. 
    /// </summary>
    /// <param name="row">row of cell</param>
    /// <param name="col">column of cell </param>
    /// <returns>name of css class</returns>
    private string GetCellClass(int row, int col)
    {
        string cellName = ColumnName(col) + (row + 1);

        // Current selected cell
        if (row == currRow && col == currCol)
            return "cell-selected";

        // Referenced cells in formula
        if (_isInFormulaMode && _referencedCells.Contains(cellName))
            return "cell-referenced";

        return "";
    }

    /// <summary>
    /// Saves the current spreadsheet, by providing a download of a file
    /// containing the json representation of the spreadsheet.
    /// </summary>
    private async void SaveFile()
    {
        await JsRuntime.InvokeVoidAsync("downloadFile", FileSaveName,
            _spreadsheet.GetJson());
    }

    /// <summary>
    /// This method will run when the file chooser is used, for loading a file.
    /// Uploads a file containing a json representation of a spreadsheet, and 
    /// replaces the current sheet with the loaded one.
    /// </summary>
    /// <param name="args">The event arguments, which contains the selected file name</param>
    private async void HandleFileChooser(EventArgs args)
    {
        try
        {
            string fileContent = string.Empty;

            InputFileChangeEventArgs eventArgs =
                args as InputFileChangeEventArgs ?? throw new Exception("unable to get file name");
            if (eventArgs.FileCount == 1)
            {
                var file = eventArgs.File;
                if (file is null)
                {
                    return;
                }

                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);

                // fileContent will contain the contents of the loaded file
                fileContent = await reader.ReadToEndAsync();

                _spreadsheet.SetContentsFromJson(fileContent);
                foreach (string name in _spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    //split name into letter and number to get row and column and set equal to value 
                    int splitIndex = name.IndexOfAny(['1', '2', '3', '4', '5', '6', '7', '8', '9']);
                    string letters = name.Substring(0, splitIndex);
                    int c = 0;
                    foreach (char letter in letters)
                        c = ColumnNumber(letters);

                    string r = name.Substring(splitIndex);

                    CellsBackingStore[int.Parse(r) - 1, c] = _spreadsheet[name].ToString();
                }

                StateHasChanged();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("an error occurred while loading the file..." + e);
        }
    }

    /// <summary>
    /// Dismesses an Error that might appear when and invalid formula is put into the spreadsheet
    /// </summary>
    private void DismissError()
    {
        _showError = false;
    }

    /// <summary>
    /// This private method makes a warning box appear that checks whether the user wants to clear the spreadsheet
    /// </summary>
    private void ClearCheck()
    {
        _clearWarning = true;
        _confirmClear = false;
        StateHasChanged();
    }

    /// <summary>
    /// confirms that the user wants to clear the spreadsheet
    /// </summary>
    private void ConfirmClear()
    {
        _confirmClear = true;
        _clearWarning = false;
        ClearSpreadsheet();
        StateHasChanged();
    }

    /// <summary>
    /// Cancels the Clear warning box and DOES NOT clear the spreadsheet. 
    /// </summary>
    private void CancelClear()
    {
        _clearWarning = false;
        _confirmClear = false;
        StateHasChanged();
    }

    /// <summary>
    /// Clears the spreadsheet of all of it's contents. 
    /// </summary>
    private void ClearSpreadsheet()
    {
        _spreadsheet = new();
        _selectedCell = "A1";
        _showError = false;
        _currentCellContent = "";

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                CellsBackingStore[row, col] = string.Empty;
            }
        }
    }
    
    //TODO: Currently when you move away from a cell with an arrow key, it changes the backign store but not the actual content of the cell. IT's messed Up!
    
    /// <summary>
    /// Runs once automatically after the page renders. Creates the _dotNetReference object. 
    /// </summary>
    /// <param name="firstRender"></param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetReference = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("addKeyPressListener", _dotNetReference);
        }
    }
    
    /// <summary>
    /// This method handles key presses from the user, moving the selected cell accordingly
    /// </summary>
    /// <param name="key">Key that was pressed</param>
    [JSInvokable] //Lets javascript call this method!
    public void HandleKeyPress(string key)
    {
        Console.WriteLine($"Key pressed: {key}");

        switch (key) //Checks which key is pressed and moves selection accordingly
        {
            case "ArrowUp":
                MoveSelection(0, -1);  // Move up (row - 1)
                break;
        
            case "ArrowDown":
                MoveSelection(0, 1);   // Move down (row + 1)
                break;
        
            case "ArrowLeft":
                MoveSelection(-1, 0);  // Move left (col - 1)
                break;
        
            case "ArrowRight":
                MoveSelection(1, 0);   // Move right (col + 1)
                break;
        }

        StateHasChanged();
    }
    
    /// <summary>
    /// Moves the currently selected cell
    /// </summary>
    /// <param name="deltaCol">Change in column</param>
    /// <param name="deltaRow">Chance in row</param>
    private void MoveSelection(int deltaCol, int deltaRow)
    {
        // Parse current cell name to get row and column numbers
        int splitIndex = _selectedCell.IndexOfAny(['1', '2', '3', '4', '5', '6', '7', '8', '9']);
        string letters = _selectedCell.Substring(0, splitIndex);
        string numbers = _selectedCell.Substring(splitIndex);
    
        int currentCol = ColumnNumber(letters);
        int currentRow = int.Parse(numbers) - 1;

        // Calculate new position
        int newCol = currentCol + deltaCol;
        int newRow = currentRow + deltaRow;

        // Make sure we stay within bounds
        if (newCol < 0) newCol = 0;
        if (newCol >= Cols) newCol = Cols - 1;
        if (newRow < 0) newRow = 0;
        if (newRow >= Rows) newRow = Rows - 1;

        // Only move if position actually changed
        if (newCol != currentCol || newRow != currentRow)
        {
            // Use your existing CellClicked method to handle the selection
            CellClicked(newRow, newCol);
        }
    }
    
    /// <summary>
    /// Cleans up when page is exited
    /// </summary>
    public void Dispose()
    {
        _dotNetReference?.Dispose();
    }
    
}