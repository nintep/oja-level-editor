using Godot;
using System;

public class ResourceSaveLoad
{
  private string _savePathRoot = "res://Resources/";
  private string _levelFolder = "LevelDatas/";
  private string _levelSuffix = ".tres";

  public ResourceSaveLoad()
  {
    GD.Print("SaveLoad created");
  }

  public void SaveLevel(LevelData levelData)
  {
    string savePath = _savePathRoot + _levelFolder + levelData.SaveName + _levelSuffix;

    string globalPath = ProjectSettings.GlobalizePath(savePath);

    GD.Print("SaveLoad: attempting to save to " + globalPath);

    Error error = ResourceSaver.Save(levelData, savePath);
    if (error != Error.Ok)
    {
      GD.PrintErr("Failed to save level: ", error);
    }
    else
    {
      GD.Print("Level saved to " + globalPath);
    }
  }
}
