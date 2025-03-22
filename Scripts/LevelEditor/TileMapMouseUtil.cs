using Godot;
using System;

public partial class TileMapMouseUtil : Node
{

  [Export] private LevelTilemap _tileMap;

  [Export] private Sprite2D _highliht;

  public override void _Process(double delta)
  {
    Vector2 mousePos = GetViewport().GetMousePosition();
    if (_tileMap.ContainsPoint(mousePos))
    {
      _highliht.Show();
      Vector2 tilePos = _tileMap.GetCenterOfTileAt(mousePos);
      _highliht.Position = tilePos;
    }
    else
    {
      _highliht.Hide();
    }
  }
}
