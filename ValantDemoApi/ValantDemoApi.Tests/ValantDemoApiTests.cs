using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace ValantDemoApi.Tests
{
  [TestFixture]
  public class ValantDemoApiTests
  {
    private HttpClient client;

    [OneTimeSetUp]
    public void Setup()
    {
      var factory = new APIWebApplicationFactory();
      client = factory.CreateClient();
    }

    [Test]
    public async Task ShouldReturnAllFourDirectionsForMovementThroughMaze()
    {
      var result = await client.GetAsync("/Maze");
      result.EnsureSuccessStatusCode();
      var content = JsonConvert.DeserializeObject<string[]>(await result.Content.ReadAsStringAsync());
      content.Should().Contain("Up");
      content.Should().Contain("Down");
      content.Should().Contain("Left");
      content.Should().Contain("Right");
    }

    [Test]
    public async Task ShouldReturnTrueForMazeAdd()
    {
      var json = new
      {
        mazeName = "Test Maze",
        mazeGrid = @"SOXXXXXXXX
OOOXXXXXXX
OXOOOXOOOO
XXXXOXOXXO
OOOOOOOXXO
OXXOXXXXXO
OOOOXXXXXE"
      };

      var result = await client.PostAsync("/addmaze", new StringContent(JObject.FromObject(json).ToString()));
      Assert.IsTrue(result.IsSuccessStatusCode);
    }

    [Test]
    public async Task ShouldReturnAllMazes()
    {
      var toSend = new
      {
        mazeName = "Test Maze",
        mazeGrid = @"SOXXXXXXXX
OOOXXXXXXX
OXOOOXOOOO
XXXXOXOXXO
OOOOOOOXXO
OXXOXXXXXO
OOOOXXXXXE"
      };

      var result = await client.PostAsync("/addmaze", new StringContent(JObject.FromObject(toSend).ToString()));
      Assert.IsTrue(result.IsSuccessStatusCode);

      result = await client.GetAsync("/allmazes");
      Assert.IsTrue(result.IsSuccessStatusCode);

      using (var reader = new StreamReader(result.Content.ReadAsStream()))
      {
        var json = await reader.ReadToEndAsync();
        Console.WriteLine(json);
        var mazes = JObject.Parse(json).Values<Maze>("mazes");

        Assert.IsNotNull(mazes);
        Assert.IsTrue(mazes.Count() > 0);
      }
    }
  }
}
