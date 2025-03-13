using Godot;
using System;
using System.Collections.Generic;

public partial class TileMapManager : Node2D
{
  [Export] 
  private TileMapLayer _groundTiles;
  [Export] 
  private TileMapLayer _waterTiles;
  [Export] 
  private TileMapLayer _obstacleTiles;

  private Dictionary<Vector2I, Flower> _flowers;

  public override void _Ready()
  {
    if (_groundTiles == null || _waterTiles == null || _obstacleTiles == null)
    {
      GD.PrintErr("Tilemap layer not found");
    }
    _flowers = new Dictionary<Vector2I, Flower>();
  }


  public void OnFlowerInstantiated(Vector2 pos, Flower flower)
  {
    Vector2I coords = _groundTiles.LocalToMap(_groundTiles.ToLocal(pos));
    _flowers[coords] = flower;

    GD.Print("Flower added " + coords);
  }

}
