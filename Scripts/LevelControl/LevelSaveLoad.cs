using Godot;
using System;

public static class LevelSaveLoad
{
  private static string _savePathRoot = "res://Resources/";
  private static string _levelFolder = "LevelDatas/";
  private static string _levelSuffix = ".tres";

  public static bool SaveLevel(LevelData levelData, string savePath)
  {
    if (savePath.Trim() == "" || savePath == null)
    {
      GD.PrintErr("SaveLoad: attempting to save empty filename");
      return false;
    }

    //TODO: validate save path

    string globalPath = ProjectSettings.GlobalizePath(savePath);
    GD.Print("SaveLoad: attempting to save to " + globalPath);

    Error error = ResourceSaver.Save(levelData, savePath);
    if (error != Error.Ok)
    {
      GD.PrintErr("Failed to save level: ", error);
      return false;
    }
    else
    {
      GD.Print("Level saved to " + globalPath);
      return true;
    }
  }

  public static LevelData LoadLevel(string loadPath)
  {
    string globalPath = ProjectSettings.GlobalizePath(loadPath);
    GD.Print("SaveLoad: attempting to load from " + globalPath);

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
