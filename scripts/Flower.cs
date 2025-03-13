using Godot;
using System;

public partial class Flower : AnimatedSprite2D
{
  private TileMapManager _tileMap;

  private Node test;
  public override void _Ready()
  {
    Play("idle_alive");

    //Find TileMap class in parent
    Node parentNode = FindParent("TileMapManager");
    if (parentNode == null || parentNode.GetType() != typeof(TileMapManager))
    {
      GD.PrintErr("Tilemap not found");
      return;
    }

    _tileMap = (TileMapManager)parentNode;
    _tileMap.OnFlowerInstantiated(GlobalPosition, this);
  }
}
