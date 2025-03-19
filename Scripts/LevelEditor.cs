using Godot;
using System;

public partial class LevelEditor : Node
{
  [Export] public LevelData CurrentLevel {get; private set;}

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
    if (shouldSave && CurrentLevel != null)
    {
      GD.Print("LevelEditor: Trying to save level");
      _saveLoad.SaveLevel(CurrentLevel);
    }
  }
}
