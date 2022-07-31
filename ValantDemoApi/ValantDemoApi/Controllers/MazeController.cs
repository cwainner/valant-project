using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ValantDemoApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class MazeController : ControllerBase
  {
    private readonly ILogger<MazeController> _logger;
    private readonly MazeManager _manager;

    public MazeController(ILogger<MazeController> logger, MazeManager manager)
    {
      _logger = logger;
      _manager = manager;
    }

    [HttpGet]
    public IEnumerable<string> GetNextAvailableMoves()
    {
      return new List<string> { "Up", "Down", "Left", "Right" };
    }

    [HttpGet("/getmoves")]
    public IEnumerable<string> GetAvailableMoves(string id, int x, int y)
    {
      return _manager.GetNextAvailableMoves(id, x, y);
    }

    [HttpPost("/addmaze")]
    public async Task<StatusCodeResult> AddNewMaze()
    {
      try
      {
        using (var reader = new StreamReader(Request.Body))
        {
          var json = await reader.ReadToEndAsync();
          var jsonObject = JObject.Parse(json);

          var mazeId = "";
          if (jsonObject.ContainsKey("mazeId"))
            mazeId = jsonObject.Value<string>("mazeId");
          var mazeName = "";
          if (jsonObject.ContainsKey("mazeName"))
            mazeName = jsonObject.Value<string>("mazeName");
          var mazeGrid = jsonObject.Value<string>("mazeGrid");

          _logger.LogDebug($"Received request to add maze: {mazeGrid} ({mazeName} {mazeId})");

          var rows = mazeGrid.Split(new[] { ",", "\n", "\n\r" }, System.StringSplitOptions.RemoveEmptyEntries);
          var success = await _manager.AddNewMaze(new List<string>(rows), mazeName, mazeId);

          if (success)
            return StatusCode((int)HttpStatusCode.OK);
          else
            return StatusCode((int)HttpStatusCode.BadRequest);
        }
      }
      catch (System.Exception ex)
      {
        _logger.LogError(ex, "Error processing new maze");
        return StatusCode((int)HttpStatusCode.InternalServerError);
      }
    }

    [HttpGet("/allmazes")]
    public JsonResult GetAllMazes()
    {
      var allMazes = _manager.GetAllMazes();

      return new JsonResult(new { mazes = allMazes });
    }
  }
}
