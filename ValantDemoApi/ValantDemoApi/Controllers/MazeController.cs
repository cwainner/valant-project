using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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

  }
}
