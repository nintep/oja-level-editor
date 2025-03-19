using Godot;
using System;

[GodotClassName("TilePlacementData")]
public partial class TilePlacementData : Resource
{
  [Export] public TileUtils.TileType tileType;
  [Export] public Vector2I coords;

  public TilePlacementData() : this(TileUtils.TileType.ground_grass, new Vector2I(0,0)) {}

  public TilePlacementData(TileUtils.TileType tileType, Vector2I coords)
  {
    this.tileType = tileType;
    this.coords = coords;
  }
}
