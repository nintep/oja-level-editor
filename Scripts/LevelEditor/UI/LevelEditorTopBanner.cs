using Godot;
using System;

public partial class LevelEditorTopBanner : Control
{
  [Signal]
  public delegate void FileDialogStateChangedEventHandler(bool opened);

  [Signal]
  public delegate void SaveInitiatedEventHandler();

  [Signal]
  public delegate void SaveAsInitiatedEventHandler(string filePath);

  [Signal]
  public delegate void LoadInitiatedEventHandler(string filePath);


  [Export]
  private LineEdit _LevelNameField;

  [Export]
  private FileDialog _loadLevelDialog;

  [Export]
  private FileDialog _saveLevelDialog;

  public string LevelNameText => _LevelNameField.Text.Trim();


  public void OnLevelLoaded(string levelName)
  {
    _LevelNameField.Text = levelName;
  }

  public void OnSaveButtonPressed()
  {
    GD.Print("Save pressed");
    EmitSignal(SignalName.SaveInitiated);
  }


  public void OnSaveAsButtonPressed()
  {
    _saveLevelDialog.Show();
    EmitSignal(SignalName.FileDialogStateChanged, true);
  }

  public void OnSaveLevelDialogFileSelected(string path)
  {
    GD.Print("TopBanner: save file selected " + path);
    EmitSignal(SignalName.SaveAsInitiated, path);
    EmitSignal(SignalName.FileDialogStateChanged, false);
  }

  public void OnLoadButtonPressed()
  {
    _loadLevelDialog.Show();
    _loadLevelDialog.CurrentFile = "";
    _loadLevelDialog.FileNameFilter = "";
    EmitSignal(SignalName.FileDialogStateChanged, true);
  }

  public void OnLoadLevelDialogFileSelected(string path)
  {
    GD.Print("TopBanner: load file selected " + path);
    EmitSignal(SignalName.LoadInitiated, path);
    EmitSignal(SignalName.FileDialogStateChanged, false);
  }


}
