using System;
using System.Collections.Generic;
using System.Linq;

namespace ValantDemoApi
{
  public class Maze
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> Grid = new Dictionary<string, string>();

    public Dictionary<string, string> GetSurroundingCells(int x, int y)
    {
      var pos = CoordinatesToKey(x, y);
      if (!Grid.ContainsKey(pos)) // If the grid doesn't contain the position return null
        return null;
      else
      {
        var surrounding = new Dictionary<string, string>();
        var up = CoordinatesToKey(x, y + 1);
        if (Grid.TryGetValue(up, out var upCell))
          surrounding.Add("up", upCell);

        var down = CoordinatesToKey(x, y - 1);
        if(Grid.TryGetValue(down, out var downCell))
          surrounding.Add("down", downCell);

        var left = CoordinatesToKey(x - 1, y);
        if (Grid.TryGetValue(left, out var leftCell))
          surrounding.Add("left", leftCell);

        var right = CoordinatesToKey(x + 1, y);
        if (Grid.TryGetValue(right, out var rightCell))
          surrounding.Add("right",rightCell);

        return surrounding;
      }
    }

    public static string CoordinatesToKey(int x, int y)
    {
      return string.Format("{0}-{1}", x, y);
    }

    public static (int x, int y) KeyToCoordinates(string key)
    {
      var entry = key.Split("-");
      var x = int.Parse(entry[0]);
      int y = int.Parse(entry[1]);
      return (x, y);
    }
  }

}
