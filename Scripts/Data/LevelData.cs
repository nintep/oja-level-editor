using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
[GodotClassName("LevelData")]
public partial class LevelData : Resource
{
  [Export] public string Name = "Level";
  [Export] public int NumTiles = 0;

  [Export]
  public TilePlacementData[] Tiles { get; set; }

  public LevelData()
  {
    Tiles = System.Array.Empty<TilePlacementData>();
  }
}
