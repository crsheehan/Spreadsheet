// Skeleton implementation written by Joe Zachary for CS 3500, September 2013
// Version 1.1 - Joe Zachary
//   (Fixed error in comment for RemoveDependency)
// Version 1.2 - Daniel Kopta Fall 2018
//   (Clarified meaning of dependent and dependee)
//   (Clarified names in solution/project structure)
// Version 1.3 - H. James de St. Germain Fall 2024
// Completed by Chancellor Sheehan, Fall 2025

namespace DependencyGraph;

/// <summary>
///     <para>
///         (s1,t1) is an ordered pair of strings, meaning t1 depends on s1.
///         (in other words: s1 must be evaluated before t1.)
///     </para>
///     <para>
///         A DependencyGraph can be modeled as a set of ordered pairs of strings.
///         Two ordered pairs (s1,t1) and (s2,t2) are considered equal if and only
///         if s1 equals s2 and t1 equals t2.
///     </para>
///     <remarks>
///         Recall that sets never contain duplicates.
///         If an attempt is made to add an element to a set, and the element is already
///         in the set, the set remains unchanged.
///     </remarks>
///     <para>
///         Given a DependencyGraph DG:
///     </para>
///     <list type="number">
///         <item>
///             If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///             (The set of things that depend on s.)
///         </item>
///         <item>
///             If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///             (The set of things that s depends on.)
///         </item>
///     </list>
///     <para>
///         For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}.
///     </para>
///     <code>
///     dependents("a") = {"b", "c"}
///     dependents("b") = {"d"}
///     dependents("c") = {}
///     dependents("d") = {"d"}
///     dependees("a")  = {}
///     dependees("b")  = {"a"}
///     dependees("c")  = {"a"}
///     dependees("d")  = {"b", "d"}
///   </code>
/// </summary>
public class DependencyGraph
{
    //Instance variables. Dependents is a dictionary mapping a string to a set of strings (it's dependents).
    //I'm using a set so that there are no duplicates. Likewise for dependees, it's just the other way around. 
    private readonly Dictionary<string, HashSet<string>> _dependents;
    private readonly Dictionary<string, HashSet<string>> _dependees;
    
    /// <summary>
    ///     Initializes a new instance of the <see cref="DependencyGraph" /> class.
    ///     The initial DependencyGraph is empty.
    /// </summary>
    public DependencyGraph()
    {
        //Initializing our two dictionaries
        _dependents = new Dictionary<string, HashSet<string>>();
        _dependees = new Dictionary<string, HashSet<string>>();
        Size = 0;
    }

    /// <summary>
    ///     The number of ordered pairs in the DependencyGraph.
    /// </summary>
    public int Size { get; private set; }
    
    /// <summary>
    ///     Reports whether the given node has dependents (i.e., other nodes depend on it).
    /// </summary>
    /// <param name="nodeName"> The name of the node.</param>
    /// <returns> true if the node has dependents. </returns>
    public bool HasDependents(string nodeName)
    {
        return _dependents.ContainsKey(nodeName);
    }

    /// <summary>
    ///     Reports whether the given node has dependees (i.e., depends on one or more other nodes).
    /// </summary>
    /// <returns> true if the node has dependees.</returns>
    /// <param name="nodeName">The name of the node.</param>
    public bool HasDependees(string nodeName)
    {
        return _dependees.ContainsKey(nodeName);
    }

    /// <summary>
    ///     <para>
    ///         Returns the dependents of the node with the given name.
    ///     </para>
    /// </summary>
    /// <param name="nodeName"> The node we are looking at.</param>
    /// <returns> The dependents of nodeName. </returns>
    public IEnumerable<string> GetDependents(string nodeName)
    {
        if (_dependents.TryGetValue(nodeName, out var dependents))
            return dependents;
        return new List<string>();
    }

    /// <summary>
    ///     <para>
    ///         Returns the dependees of the node with the given name.
    ///     </para>
    /// </summary>
    /// <param name="nodeName"> The node we are looking at.</param>
    /// <returns> The dependees of nodeName. </returns>
    public IEnumerable<string> GetDependees(string nodeName)
    {
        if (_dependees.TryGetValue(nodeName, out var dependees))
            return dependees;  // key exists  -> return the set
        return new List<string>();
    }

    /// <summary>
    ///     <para>Adds the ordered pair (dependee, dependent), if it doesn't exist.</para>
    ///     <para>
    ///         This can be thought of as: dependee must be evaluated before dependent
    ///     </para>
    /// </summary>
    /// <param name="dependee"> the name of the node that must be evaluated first</param>
    /// <param name="dependent"> the name of the node that cannot be evaluated until after dependee</param>
    public void AddDependency(string dependee, string dependent)
    {
        // Get or create dependents set for dependee
        if (!_dependents.TryGetValue(dependee, out var dependents))
        {
            dependents = new HashSet<string>();
            _dependents[dependee] = dependents;
        }

        // Get or create dependees set for dependent
        if (!_dependees.TryGetValue(dependent, out var dependees))
        {
            dependees = new HashSet<string>();
            _dependees[dependent] = dependees;
        }

        // Only increment size if we're adding a new relationship
        // HashSet.Add returns true if element was added, false if it already existed
        if (dependents.Add(dependent) && dependees.Add(dependee))
        {
            Size++;
        }
    }

    /// <summary>
    ///     <para>
    ///         Removes the ordered pair (dependee, dependent), if it exists.
    ///     </para>
    /// </summary>
    /// <param name="dependee"> The name of the node that must be evaluated first</param>
    /// <param name="dependent"> The name of the node that cannot be evaluated until after dependee</param>
    public void RemoveDependency(string dependee, string dependent)
    {
        //Assign a variable to keep track of if something was removed
        var removed = false;
        
        //Remove pair from Dependents
        if (_dependents.ContainsKey(dependee)  && _dependents[dependee].Contains(dependent)) //Dependee exists
        {
                _dependents[dependee].Remove(dependent);
                removed = true;
                if (_dependents[dependee].Count == 0) // If there are no more dependents, remove whole set
                    _dependents.Remove(dependee); 
        }
        
        //Remove pair from Dependees
        if (_dependees.ContainsKey(dependent) && _dependees[dependent].Contains(dependee))
        {
                _dependees[dependent].Remove(dependee);
                removed = true;
                if (_dependees[dependent].Count == 0)   _dependees.Remove(dependent);// If there are no more dependees, remove whole set
        }
        if (removed) Size--;
    }

    /// <summary>
    ///     Removes all existing ordered pairs of the form (nodeName, *).  Then, for each
    ///     t in newDependents, adds the ordered pair (nodeName, t).
    /// </summary>
    /// <param name="nodeName"> The name of the node whose dependents are being replaced </param>
    /// <param name="newDependents"> The new dependents for nodeName</param>
    public void ReplaceDependents(string nodeName, IEnumerable<string> newDependents)
    {
        // Convert newDependents to HashSet for efficient operations
        var newDependentsSet = new HashSet<string>(newDependents);

        // Get current dependents (or empty set if none exist)
        var currentDependents = _dependents.TryGetValue(nodeName, out var current) 
            ? new HashSet<string>(current) 
            : new HashSet<string>();

        // Remove relationships that are no longer needed
        var toRemove = currentDependents.Except(newDependentsSet);
        foreach (var dependent in toRemove)
        {
            RemoveDependency(nodeName, dependent);
        }

        // Add new relationships
        var toAdd = newDependentsSet.Except(currentDependents);
        foreach (var dependent in toAdd)
        {
            AddDependency(nodeName, dependent);
        }
    }

    /// <summary>
    ///     <para>
    ///         Removes all existing ordered pairs of the form (*, nodeName).  Then, for each
    ///         t in newDependees, adds the ordered pair (t, nodeName).
    ///     </para>
    /// </summary>
    /// <param name="nodeName"> The name of the node who's dependees are being replaced</param>
    /// <param name="newDependees"> The new dependees for nodeName</param>
    public void ReplaceDependees(string nodeName, IEnumerable<string> newDependees)
    {
        // Convert newDependees to HashSet for efficient operations
        var newDependeesSet = new HashSet<string>(newDependees);

        // Get current dependees (or empty set if none exist)
        var currentDependees = _dependees.TryGetValue(nodeName, out var current) 
            ? new HashSet<string>(current) 
            : new HashSet<string>();

        // Remove relationships that are no longer needed
        var toRemove = currentDependees.Except(newDependeesSet);
        foreach (var dependee in toRemove)
        {
            RemoveDependency(dependee, nodeName);
        }

        // Add new relationships
        var toAdd = newDependeesSet.Except(currentDependees);
        foreach (var dependee in toAdd)
        {
            AddDependency(dependee, nodeName);
        }
    }
}