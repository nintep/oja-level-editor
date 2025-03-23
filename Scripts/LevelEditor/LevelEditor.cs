using Godot;
using System;

public partial class LevelEditor : Node
{
  [Export] private LevelContainer _levelContainer;
  [Export] private LevelEditorTopBanner _topBanner;
  [Export] private TilePalette _tilePalette;
  [Export] private TimedMessage _timedMessage;

  
  //Store path of most recently loaded or saved level
  private string _currentSaveName;
  private string _currentSavePath;

  public override void _Ready()
  {
    GD.Print("LevelEditor: Started level editor");

    _topBanner.SaveInitiated += SaveLevel;
    _topBanner.SaveAsInitiated += SaveLevelAs;
    _topBanner.LoadInitiated += LoadLevel;

    _topBanner.FileDialogStateChanged += (bool dialogOpen) => _tilePalette.SetAllowPlacement(!dialogOpen);

    _tilePalette.TilePlaced += (Vector2I coords, TileUtils.TileType tileType) => _levelContainer.PaintTile(coords, tileType, false);
    _tilePalette.TileErased += (Vector2I coords) => _levelContainer.PaintTile(coords, TileUtils.TileType.none, true);

  }

  private void SaveLevel()
  {
    if (_currentSavePath == null)
    {
      GD.PrintErr("LevelEditor: trying to save without file name");
      return;
    }

    SaveLevelAs(_currentSavePath);
  }

  private void SaveLevelAs(string savePath)
  {
    GD.Print("LevelEditor: Save level to path " + savePath);

    if (_levelContainer == null)
    {
      GD.PrintErr("LevelEditor: levelcontainer missing");
      return;
    }

    LevelData data = _levelContainer.GetLevelData();

    //Update level name
    data.Name = _topBanner.LevelNameText;

    bool success = LevelSaveLoad.SaveLevel(data, savePath);
    if (success)
    {
      _currentSavePath = savePath;
    }

    _timedMessage?.ShowMessage("Level saved!");
  }

  private void LoadLevel(string filePath)
  {
    if (_levelContainer == null)
    {
      GD.PrintErr("LevelEditor: levelcontainer missing");
      return;
    }

    LevelData data = LevelSaveLoad.LoadLevel(filePath);
    if (data == null)
    {
      GD.PrintErr("LevelEditor: load failed");
      return;
    }

    //_currentSaveName = saveName;
    _levelContainer.SetLevelData(data);
    _topBanner.OnLevelLoaded(data.Name);
    _currentSavePath = filePath;

    _timedMessage?.ShowMessage("Level loaded");
  }
}
