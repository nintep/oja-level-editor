using Godot;
using System;

public partial class LevelContainer : Node
{
  [Export]
  private LevelData LevelData;

  [Export]
  private LevelTilemap _levelTileMap;

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
    _levelTileMap.SetTiles(levelData.Tiles);
  }

  public LevelData GetLevelData()
  {
    //Refresh tiles
    LevelData.Tiles = _levelTileMap.GetTiles();
    LevelData.NumTiles = _levelTileMap.GetTiles().Length;
    return LevelData;
  }

  public void PaintTile(Vector2I coords, TileUtils.TileType tileType, bool eraseModeActive)
  {
    if (eraseModeActive)
    {
      _levelTileMap.EraseTile(coords);
    }
    else
    {
      _levelTileMap.PaintTile(coords, tileType);
    }
  }
}
