using Godot;
using System;

public partial class LevelContainer : Node
{
  [Export] private LevelData LevelData;

  [Export] private LevelTilemap _levelTileMap;

  [Export] private Player _player;

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

    _isLevelRunning = false;
    _stepDelayRemaining = _stepInterval;
    _player.Hide();

    if (_runAtStart)
    {
      BeginLevel();
    }
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
    _player.Hide();
  }

  public LevelData GetLevelData()
  {
    //Refresh tiles
    LevelData.Tiles = _levelTileMap.GetTiles();
    LevelData.NumTiles = _levelTileMap.GetTiles().Length;
    return LevelData;
  }

  public void BeginLevel()
  {
    //Refresh tile state before running level
    LevelData.Tiles = _levelTileMap.GetTiles();
    LevelData.NumTiles = _levelTileMap.GetTiles().Length;

    if (_levelTileMap.ContainsStartTile())
    {
      GD.Print("Tile size: " + _levelTileMap.GetTileSize());
      Vector2 playerStartPos = _levelTileMap.RemoveStartTile();

      GD.Print("Setting player position to " + playerStartPos);
      _player.GlobalPosition = playerStartPos;
      _player.SetTileSize(_levelTileMap.GetTileSize());
      _player.Show();
    }

    SetLevelPaused(false);
  }

  public void ResetLevel()
  {
    _levelTileMap.SetTiles(LevelData.Tiles);
    _player.Hide();
  }

  public void SetLevelPaused(bool paused)
  {
    GD.Print("LevelContainer: set level running " + !paused);

    _isLevelRunning = !paused;
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

  private void TakeStep()
  {
    _stepDelayRemaining = _stepInterval;
    _levelTileMap.RefreshWater();
  }
}
