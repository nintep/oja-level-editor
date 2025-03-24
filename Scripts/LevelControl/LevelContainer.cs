using Godot;
using System;

public partial class LevelContainer : Node
{
  [Export] private LevelData LevelData;

  [Export] private LevelTilemap _levelTileMap;

  [Export] private bool _runAtStart = false;

  private bool _isLevelRunning;
  private static float _stepInterval = 0.8f;
  private float _stepDelayRemaining;

  public override void _Ready()
  {
    if (LevelData == null)
    {
      LevelData = new LevelData();
    }

    _isLevelRunning = _runAtStart;
    _stepDelayRemaining = _stepInterval;
  }

  public override void _Process(double delta)
  {
    if (!_isLevelRunning)
    {
      return;
    }

    if (_stepDelayRemaining > 0)
    {
      _stepDelayRemaining -= (float)delta;
    }

    if (_stepDelayRemaining <= 0)
    {
      TakeStep();
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

  public void Dig(Vector2 playerPos, Vector2 direction)
  {

  }

  public void SetLevelRunning(bool isRunning)
  {
    GD.Print("LevelContainer: set level running " + isRunning);

    _isLevelRunning = isRunning;
  }

  private void TakeStep()
  {
    _stepDelayRemaining = _stepInterval;
    _levelTileMap.RefreshWater();
  }
}
