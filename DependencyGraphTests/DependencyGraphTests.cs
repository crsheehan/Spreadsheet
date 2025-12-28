namespace DependencyGraphTests;
using DependencyGraph;

[TestClass]
public sealed class DependencyGraphTests
{
    /// <summary>
    ///     This test works by creating two sets, dependents and dependees used to check correctness at the end of the test,
    ///     and a list of strings a0......a199. It then adds a few dependencies, removes some, adds some again, then removes some,
    ///     and then checks if the graph is equivalent to the correct dependents and dependees sets.  On top of that, it makes
    ///     sure the test runs in 2 seconds. If it takes longer than 2 seconds or the dependencyGraph is not correct after
    ///     adding and removing relationships, the test will fail.
    ///  
    /// </summary>
    [TestMethod]
    [Timeout(2000)] // 2 second run time limit. Basically means that this whole test has to finish running before 2 seconds.
                    // ?Does this change based on what computer I'm using????
    public void StressTest()
    {
        DependencyGraph dg = new();

        // A bunch of strings to use
        const int size = 200;
        var letters = new string[size];
        for (var i = 0; i < size; i++) letters[i] = string.Empty + (char)('a' + i); //This makes letters look like: [a0, a1, a2.... a199]

        // The correct answers
        var dependents = new HashSet<string>[size];
        var dependees = new HashSet<string>[size];
        for (var i = 0; i < size; i++)
        {
            dependents[i] = []; //same as dependents[i] = new HashSet<string>();
            dependees[i] = [];
        }

        // Add a bunch of dependencies. Uses a double for loop to create dependencies like "a1 depends on a0" and so on. 
        for (var i = 0; i < size; i++)
        for (var j = i + 1; j < size; j++)
        {
            dg.AddDependency(letters[i], letters[j]);
            dependents[i].Add(letters[j]);
            dependees[j].Add(letters[i]);
        }

        // Remove a bunch of dependencies
        for (var i = 0; i < size; i++)
        for (var j = i + 4; j < size; j += 4)
        {
            dg.RemoveDependency(letters[i], letters[j]);
            dependents[i].Remove(letters[j]);
            dependees[j].Remove(letters[i]);
        }

        // Add some back
        for (var i = 0; i < size; i++)
        for (var j = i + 1; j < size; j += 2)
        {
            dg.AddDependency(letters[i], letters[j]);
            dependents[i].Add(letters[j]);
            dependees[j].Add(letters[i]);
        }

        // Remove some more
        for (var i = 0; i < size; i += 2)
        for (var j = i + 3; j < size; j += 3)
        {
            dg.RemoveDependency(letters[i], letters[j]);
            dependents[i].Remove(letters[j]);
            dependees[j].Remove(letters[i]);
        }

        // Make sure everything is right. Compares dependency graph to our dependents and dependees sets we made 
        //to check correctness. 
        for (var i = 0; i < size; i++)
        {
            Assert.IsTrue(dependents[i].SetEquals(new HashSet<string>(dg.GetDependents(letters[i]))));
            Assert.IsTrue(dependees[i].SetEquals(new HashSet<string>(dg.GetDependees(letters[i]))));
        }
    }

    //Testing HasDependents ---------------------------------------------------------------------------------------   

    [TestMethod]
    public void TestHasDependents_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "a2");
        Assert.IsTrue(dg.HasDependents("a1"));
    }

    [TestMethod]
    public void TestHasMultipleDependents_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "a2");
        dg.AddDependency("a1", "c3");
        Assert.IsTrue(dg.HasDependents("a1"));
    }

    [TestMethod]
    public void TestHasDependents_Invalid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("a1", "a2");
        Assert.IsFalse(dg.HasDependents("a2"));
    }


    //Testing HasDependees ---------------------------------------------------------------------------------------   

  

    [TestMethod]
    public void TestHasDependees_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("a1", "a2");
        Assert.IsTrue(dg.HasDependees("a2"));
    }

    [TestMethod]
    public void TestHasMultipleDependees_Valid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("a1", "a2");
        dg.AddDependency("c3", "a2");
        Assert.IsTrue(dg.HasDependees("a2"));
    }

    [TestMethod]
    public void TestHasDependees_Invalid()
    {
        DependencyGraph dg = new DependencyGraph();
        dg.AddDependency("a1", "a2");
        Assert.IsFalse(dg.HasDependees("a1"));
    }

    [TestMethod]
    public void TestHasDependeesEmptyGraph_False()
    {
        DependencyGraph dp = new();
        Assert.IsFalse(dp.HasDependees("a6"));
    }
    
    // Test GetDependents() ----------------------------------------------------------------------------
    [TestMethod]
    public void TestGetDependents_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "Dylan");
        dg.AddDependency("Bob", "Marley");
        IEnumerable<string> dependents = dg.GetDependents("Bob");
        var enumerable = dependents.ToList();
        Assert.IsTrue(enumerable.Contains("Dylan") &&  enumerable.Contains("Marley"));
    }

    [TestMethod]
    public void TestGetDependents_Invalid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "Dylan");
        dg.AddDependency("Bob", "Marley");
        IEnumerable<string> dependents = dg.GetDependents("Bob");
        Assert.IsFalse(dependents.Contains("Ross"));
    }

    [TestMethod]
    public void TestGetDependentsOnNonExistentRelationship_Invalid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "Ross");
        dg.AddDependency("Bob", "Dylan");
        dg.AddDependency("Bob", "Marley");
        CollectionAssert.AreEqual(new List<string>(),dg.GetDependents("John").ToList());
    }
    
    [TestMethod]
    public void TestGetDependentsOnDependee_Invalid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "Ross");
        dg.AddDependency("Bob", "Dylan");
        dg.AddDependency("Bob", "Marley");
        IEnumerable<string> dependents = dg.GetDependents("Bob");
        Assert.IsFalse(dependents.Contains("Bob"));
    }
    
    //Test GetDependees() ------------------------------------------------------------------------------
     [TestMethod]
        public void TestGetDependees_Valid()
        {
            DependencyGraph dg = new();
            dg.AddDependency("Dylan", "Bob");
            dg.AddDependency("Marley", "Bob");
            IEnumerable<string> dependees = dg.GetDependees("Bob");
            var enumerable = dependees.ToList();
            Assert.IsTrue(enumerable.Contains("Dylan") &&  enumerable.Contains("Marley"));
        }
    
        [TestMethod]
        public void TestGetDependee_Invalid()
        {
            DependencyGraph dg = new();
            dg.AddDependency("Dylan", "Bob");
            dg.AddDependency("Marley", "Bob");
            dg.AddDependency("Bob", "Ross");
            IEnumerable<string> dependees = dg.GetDependees("Ross");
            Assert.IsFalse(dependees.Contains("Dylan"));
        }
    
        [TestMethod]
        public void TestGetDependeeOnNonExistentRelationship_Invalid()
        {
            DependencyGraph dg = new();
            dg.AddDependency("Dylan", "Bob");
            dg.AddDependency("Marley", "Bob");
            dg.AddDependency("Ross", "Bob");
            CollectionAssert.AreEqual(new List<string>(), dg.GetDependees("john").ToList());
        }
        
        [TestMethod]
        public void TestGetDependeeOnDependent_Invalid()
        {
            DependencyGraph dg = new();
            dg.AddDependency("Dylan", "Bob");
            dg.AddDependency("Marley", "Bob");
            dg.AddDependency("Ross", "Bob");
            IEnumerable<string> dependee = dg.GetDependees("Bob");
            Assert.IsFalse(dependee.Contains("Bob"));
        }
        
    
    
    //Test Size() --------------------------------------------------------------------------------------
    [TestMethod]
    public void TestSize_True()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Marley", "Bob");
        dg.AddDependency("Ross", "Bob");
        Assert.AreEqual(3, dg.Size);
    }
    
    [TestMethod]
    public void TestSizeAddRelationshipTwice_True()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Marley", "Bob");
        dg.AddDependency("Ross", "Bob");
        dg.AddDependency("Ross", "Bob");
        Assert.AreEqual(3, dg.Size);
    }

    [TestMethod]
    public void TestSizeEmptyGraph_True()
    {
        DependencyGraph dg = new();
        Assert.AreEqual(0, dg.Size);
    }
    
    [TestMethod]
    public void TestSizeOnCyclicGraph()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Bob", "Dylan");
        Assert.AreEqual(2, dg.Size);
    }
    
    // Test AddDependency() ----------------------------------------------------------------------------
    [TestMethod]
    public void TestAddDependencyWithSize_True()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Ross", "Bob");
        dg.AddDependency("Paul", "Mescal");
        Assert.AreEqual(3, dg.Size);
    }
    
    [TestMethod]
    public void TestAddDependencyWithGetDependents_True()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Ross", "Bob");
        dg.AddDependency("Paul", "Mescal");
        IEnumerable<string> dependents = dg.GetDependents("Paul");
        Assert.IsTrue(dependents.Contains("Mescal"));
    }
    
    [TestMethod]
    public void TestAddDependencyWithGetDependees_True()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Ross", "Bob");
        dg.AddDependency("Paul", "Mescal");
        IEnumerable<string> dependees = dg.GetDependees("Bob");
        var enumerable = dependees.ToList();
        Assert.IsTrue(enumerable.Contains("Dylan") && enumerable.Contains("Ross"));
    }
    
    [TestMethod]
    public void TestAddDependencyWithHasDependents_True()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "Ross");
        Assert.IsTrue(dg.HasDependents("Bob"));
    }
    
    [TestMethod]
    public void TestAddDependencyWithHasDependees_True()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "Ross");
        Assert.IsTrue(dg.HasDependees("Ross"));
    }
    
    
    //Test RemoveDependency() --------------------------------------------------------------------------
    [TestMethod]
    public void TestRemoveDependencyWithSize_True()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Ross", "Bob");
        dg.AddDependency("Paul", "Mescal");
        dg.RemoveDependency("Paul","Mescal");
        Assert.AreEqual(2, dg.Size);
    }
    
    [TestMethod]
    public void TestRemoveDependencyWithHasDependents()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Ross", "Bob");
        dg.AddDependency("Paul", "Mescal");
        dg.RemoveDependency("Paul","Mescal");
        Assert.IsTrue(!dg.HasDependents("Paul"));
        
    }
    
    [TestMethod]
    public void TestRemoveDependencyWithHasDependees()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Dylan", "Bob");
        dg.AddDependency("Ross", "Bob");
        dg.AddDependency("Paul", "Mescal");
        dg.RemoveDependency("Paul","Mescal");
        Assert.IsTrue(!dg.HasDependees("Mescal"));
        
    }
    //Test ReplaceDependents() -------------------------------------------------------------------------
    [TestMethod]
    public void TestReplaceDependentsWithSize_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "John");
        dg.AddDependency("Bob", "Bill");
        dg.AddDependency("Bob", "Jill");
        dg.ReplaceDependents("Bob", new List<string> { "Ross", "Dylan" });
        Assert.AreEqual(2, dg.Size);
    }
    
    [TestMethod]
    public void TestReplaceDependentsWithGetDependents_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "John");
        dg.AddDependency("Bob", "Bill");
        dg.AddDependency("Bob", "Jill");
        dg.ReplaceDependents("Bob", new List<string> { "Ross", "Dylan" });
        IEnumerable<string> dependents = dg.GetDependents("Bob");
        var enumerable = dependents.ToList();
        Assert.IsTrue(enumerable.Contains("Ross") && enumerable.Contains("Dylan"));
    }
    
    [TestMethod]
    public void TestReplaceDependentsWithHasDependents_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("Bob", "John");
        dg.AddDependency("Bob", "Bill");
        dg.AddDependency("Bob", "Jill");
        dg.ReplaceDependents("Bob", new List<string>());
        Assert.IsFalse(dg.HasDependents("Bob"));
    }
    //Test ReplaceDependees() --------------------------------------------------------------------------
    [TestMethod]
    public void TestReplaceDependeesWithSize_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("John", "Bob");
        dg.AddDependency("Kill", "Bob");
        dg.AddDependency("Greg", "Bob");
        dg.ReplaceDependees("Bob", new List<string> { "Ross", "Dylan" });
        Assert.AreEqual(2, dg.Size);
    }
    
    [TestMethod]
    public void TestReplaceDependeesWithGetDependees_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("John", "Bob");
        dg.AddDependency("Kill", "Bob");
        dg.AddDependency("Greg", "Bob");
        dg.ReplaceDependees("Bob", new List<string> { "Ross", "Dylan" });
        IEnumerable<string> dependees = dg.GetDependees("Bob");
        var enumerable = dependees.ToList();
        Assert.IsTrue(enumerable.Contains("Ross") && enumerable.Contains("Dylan"));
    }
    
    [TestMethod]
    public void TestReplaceDependeesWithHasDependees_Valid()
    {
        DependencyGraph dg = new();
        dg.AddDependency("John", "Bob");
        dg.AddDependency("Kill", "Bob");
        dg.AddDependency("Greg", "Bob");
        dg.ReplaceDependees("Bob", new List<string>( ));
        Assert.IsFalse(dg.HasDependees("Bob"));
    }
    
}