using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ValantDemoApi.Tests
{
  [TestFixture]
  public class MazeManagerTests
  {
    MazeManager _manager;

    string mazeId;
    string mazeName;
    List<string> testRows;

    [OneTimeSetUp]
    public void Setup()
    {
      _manager = new MazeManager(new MazeManagerLogger());

      while (!_manager.Initialized)
        Thread.Sleep(200); // wait for initialization

      mazeId = Guid.NewGuid().ToString();
      mazeName = "Test Maze";
      testRows = new List<string>
      {
        "SOXXXXXXXX",
        "OOOXXXXXXX",
        "OXOOOXOOOO",
        "XXXXOXOXXO",
        "OOOOOOOXXO",
        "OXXOXXXXXO",
        "OOOOXXXXXE"
      };
    }

    [Test]
    public async Task ShouldAddNewMaze()
    {
      _manager.ClearMazes();

      var success = await _manager.AddNewMaze(testRows, mazeName, mazeId);
      Assert.IsTrue(success);

      var allMazes = _manager.GetAllMazes();

      Assert.That(allMazes.Count == 1);

      var maze = allMazes.First();
      Assert.AreEqual(maze.Id, mazeId);
      Assert.AreEqual(maze.Name, mazeName);
      Assert.That(maze.Grid.Count == 70); // should have 70 entries for the total cell count
    }
  }
}
