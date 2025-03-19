using Godot;
using System;

public partial class LevelData : Resource
{
  [Export] private int _width = 10;
  [Export] private int _height = 10;
  [Export] string _name = "Level";
  [Export] public string SaveName {get; private set;} = "level";

  
}
