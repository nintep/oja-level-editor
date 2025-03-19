using Godot;
using System;

public class ResourceSaveLoad
{
  private string _savePathRoot = "res://Resources/";
  private string _levelFolder = "LevelDatas/";
  private string _levelSuffix = ".tres";

  public ResourceSaveLoad()
  {
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

  public LevelData LoadLevel(string saveName)
  {
    string loadPath = _savePathRoot + _levelFolder + saveName + _levelSuffix;
    string globalPath = ProjectSettings.GlobalizePath(loadPath);

    GD.Print("SaveLoad: attempting to load from to " + globalPath);

    if (ResourceLoader.Exists(loadPath, "LevelData"))
    {
      Resource data = ResourceLoader.Load(loadPath, "LevelData", ResourceLoader.CacheMode.Ignore);
      if (data.GetType() != typeof(LevelData))
      {
        GD.PrintErr("SaveLoad: level resource has wrong type");
      }
      else
      {
        GD.Print("SaveLoad: level resource loaded");
        return (LevelData)data;
      }

    }
    else
    {
      GD.PrintErr("SaveLoad: resource does not exist");
    }

    return null;
  }
}
