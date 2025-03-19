using Godot;
using System;

public partial class LevelContainer : Node
{
  [Export]
  private LevelData LevelData;

  [Export]
  public LevelTilemap LevelTileMap {get; private set;}

  public override void _Ready()
  {
    if (LevelData == null)
    {
      LevelData = new LevelData();
    }
  }

  public void SetLevelData(LevelData levelData)
  {
    if (levelData == null)
    {
      GD.PrintErr("LevelContainer: level data was null");
      return;
    }

    LevelData = levelData;    
    LevelTileMap.SetTiles(levelData.Tiles);
  }

  public LevelData GetLevelData()
  {
    //Refresh tiles
    LevelData.Tiles = LevelTileMap.GetTiles();
    LevelData.NumTiles = LevelTileMap.GetTiles().Length;
    return LevelData;
  }


}
