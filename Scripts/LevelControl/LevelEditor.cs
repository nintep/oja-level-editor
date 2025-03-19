using Godot;
using System;

public partial class LevelEditor : Node
{
  [Export] private LevelContainer _levelContainer;

  private ResourceSaveLoad _saveLoad;

  public override void _Ready()
  {
    GD.Print("LevelEditor: Started level editor");
    _saveLoad = new ResourceSaveLoad(); 
  }
  public override void _Process(double delta)
  {
    //mapped to E
    bool shouldSave = Input.IsActionJustPressed("turn_right");
    bool shouldLoad = Input.IsActionJustPressed("turn_left");
    if (shouldSave)
    {
      SaveLevel();
    }
    else if (shouldLoad)
    {
      LoadLevel();
    }
  }

  private void SaveLevel()
  {
    if (_levelContainer == null)
    {
      GD.PrintErr("LevelEditor: levelcontainer missing");
      return;
    }
    
    LevelData data = _levelContainer.GetLevelData();
    _saveLoad.SaveLevel(data);
  }

  private void LoadLevel()
  {
    if (_levelContainer == null)
    {
      GD.PrintErr("LevelEditor: levelcontainer missing");
      return;
    }

    LevelData data = _saveLoad.LoadLevel("level");
    if (data == null)
    {
      GD.PrintErr("LevelEditor: load failed");
      return;
    }
    _levelContainer.SetLevelData(data);
  }
}
