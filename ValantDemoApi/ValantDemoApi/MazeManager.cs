using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ValantDemoApi
{
  public class MazeManager
  {
    private readonly ILogger<MazeManager> _logger;

    private readonly string _defaultFileLocation;

    private ConcurrentDictionary<string, Maze> _mazes;

    public bool Initialized { get; private set; }

    public MazeManager(ILogger<MazeManager> logger)
    {
      _logger = logger;
      _mazes = new ConcurrentDictionary<string, Maze>();

      try
      {
        _defaultFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Mazes");
      }
      catch
      {
        _defaultFileLocation = Path.Combine(Directory.GetCurrentDirectory(), "Mazes");
      }

      Initialize();
    }

    private void Initialize()
    {
      Task.Run(async () =>
      {
        try
        {
          if (!Initialized)
          {
            await LoadMazes();
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error initializing MazeManager");
        }
        finally
        {
          Initialized = true;
        }
      });
    }

    private async Task LoadMazes()
    {
      try
      {
        if (Directory.Exists(_defaultFileLocation))
        {
          // Find existing files and create mazes
          var existing = Directory.EnumerateFiles(_defaultFileLocation);
          foreach (var file in existing)
          {
            try
            {
              var lines = await File.ReadAllLinesAsync(file);
              if (lines.Length > 0)
              {
                string id = "";
                string name = "";
                Dictionary<string, string> grid = new Dictionary<string, string>();
                for (int i = 0; i < lines.Length; i++)
                {
                  var line = lines[i];

                  if (line.StartsWith("Id=", StringComparison.OrdinalIgnoreCase))
                    id = line.Replace("Id=", "", StringComparison.OrdinalIgnoreCase);
                  else if (line.StartsWith("Name=", StringComparison.OrdinalIgnoreCase))
                    name = line.Replace("Name=", "", StringComparison.OrdinalIgnoreCase);
                  else
                  {
                    var cells = line.Split();
                    for (int j = 0; j < cells.Length; j++)
                    {
                      var key = Maze.CoordinatesToKey(i, j);
                      var cell = cells[j];
                      grid.Add(key, cell);
                    }
                  }
                }

                if (string.IsNullOrEmpty(id))
                  id = Guid.NewGuid().ToString();

                if (grid.Count > 0)
                {
                  var maze = new Maze()
                  {
                    Id = id,
                    Name = name,
                    Grid = grid
                  };
                  _mazes.TryAdd(maze.Id, maze);
                  _logger.LogDebug("Added existing maze with id {0} and name {1}", id, name);
                }
              }
              else
                _logger.LogInformation("File {0} returned no lines. Skipping...", file);
            }
            catch (Exception ex)
            {
              _logger.LogInformation(ex, "Failed to load existing file {0}. Skipping.", file);
            }
          }
        }
        else
        {
          Directory.CreateDirectory(_defaultFileLocation); // create if it doesn't exist
        }
      }
      catch (Exception ex)
      {
        _logger?.LogError(ex, "Error loading existing mazes");
      }
    }

    public async Task<bool> AddNewMaze(List<string> mazeRows, string mazeName = null, string mazeId = null)
    {
      Dictionary<string, string> grid = new Dictionary<string, string>();
      bool start = false;
      bool end = false;

      for (int i = 0; i < mazeRows.Count; i++)
      {
        var line = mazeRows[i];
        _logger.LogDebug($"Row: {line}");
        var cells = line.ToCharArray();
        for (int j = 0; j < cells.Length; j++)
        {
          var key = Maze.CoordinatesToKey(i, j);
          var cell = cells[j];

          if (cell.Equals('s') || cell.Equals('S'))
            start = true;
          if (cell.Equals('e') || cell.Equals('E'))
            end = true;

          grid.Add(key, cell.ToString());
        }
      }

      if (grid.Count > 0)
      {
        _logger.LogDebug($"{grid.Count} cells found.");

        if (!start)
        {
          _logger.LogWarning("No start position found for maze");
          return false;
        }
        if (!end)
        {
          _logger.LogWarning("No end position found for maze");
          return false;
        }

        var maze = new Maze()
        {
          Id = mazeId ?? Guid.NewGuid().ToString(),
          Name = mazeName ?? "",
          Grid = grid
        };

        if (_mazes.TryAdd(maze.Id, maze))
        {
          return await SerializeMazeToFile(maze);
        }
        else
          return false;
      }
      else
      {
        _logger.LogWarning("No grid cells found for maze");
        return false;
      }
    }

    private async Task<bool> SerializeMazeToFile(Maze maze)
    {
      try
      {
        var formattedGrid = new Dictionary<int, Dictionary<int, string>>();
        foreach (var entry in maze.Grid)
        {
          (var x, var y) = Maze.KeyToCoordinates(entry.Key);
          if (formattedGrid.TryGetValue(y, out var row))
          {
            row[x] = entry.Value;
          }
          else
          {
            row = new Dictionary<int, string>()
            {
              { x, entry.Value }
            };
            formattedGrid.Add(y, row);
          }
        }

        // Order inner rows by key, then convert them into rows of strings to serialize to the file
        var lines = formattedGrid.OrderBy(g => g.Key).Select(g => string.Join("", g.Value.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value))).ToList();

        var path = Path.Combine(_defaultFileLocation, $"{maze.Name}.txt");

        if (File.Exists(path))
          File.Delete(path);

        using (var file = File.OpenWrite(path))
        {
          using (var writer = new StreamWriter(file))
          {
            await writer.WriteLineAsync($"Id={maze.Id}");
            await writer.WriteLineAsync($"Name={maze.Name}");

            foreach (var line in lines)
            {
              await writer.WriteLineAsync(line);
            }
          }
        }

        _logger.LogDebug("Maze successfully written to file");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error serializing maze {0} to file", maze.Id);
      }

      return false;
    }

    public List<string> GetNextAvailableMoves(string mazeId, int x, int y)
    {
      if (_mazes.TryGetValue(mazeId, out var maze))
      {
        var surrounding = maze.GetSurroundingCells(x, y);
        if (surrounding == null)
        {
          _logger.LogWarning("Invalid position for maze: {0}-{1}", x, y);
          return null;
        }
        else
        {
          var cells = new List<string>();
          foreach (var cell in surrounding)
          {
            if (!cell.Value.Equals("x", StringComparison.OrdinalIgnoreCase))
              cells.Add(cell.Key);
          }
          return cells;
        }
      }
      else
      {
        _logger.LogWarning("No maze found with id {0}", mazeId);
        return null;
      }
    }

    public List<Maze> GetAllMazes()
    {
      return _mazes.Values.ToList();
    }

    public bool DeleteMaze(string mazeId)
    {
      return _mazes.TryRemove(mazeId, out _);
    }

    public void ClearMazes()
    {
      _mazes.Clear();
    }
  }
}
