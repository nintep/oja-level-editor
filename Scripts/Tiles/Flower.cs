using Godot;
using System;

public partial class Flower : AnimatedSprite2D
{
  private TileMapManager _tileMap;

  public bool IsAlive {get; private set;}

  //Animation names
  private const string _animationIdleAlive = "idle_alive";
  private const string _animationIdleDead = "idle_dead";
  private const string _animationWake = "awake";
  private const string _animationDie = "die";

  private Node test;
  public override void _Ready()
  {
    //Find TileMap class in parent
    Node parentNode = FindParent("TileMapManager");
    if (parentNode == null || parentNode.GetType() != typeof(TileMapManager))
    {
      GD.PrintErr("Flower: Tilemap not found");
      return;
    }

    IsAlive = false;
    Play(_animationIdleDead);

    _tileMap = (TileMapManager)parentNode;
    _tileMap.OnFlowerInstantiated(GlobalPosition, this);
  }

  public void SetAlive(bool alive)
  {
    if (IsAlive == alive)
    {
      return;
    }

    if (alive)
    {
      Play(_animationWake);
      IsAlive = true;
    }
    else
    {
      Play(_animationDie);
      IsAlive = false;
    }
  }

  private void _onAnimationFinished()
  {
    switch ((string)Animation)
    {
      case _animationWake:
        Play(_animationIdleAlive);
        break;
      case _animationDie:
        Play(_animationIdleDead);
        break;
      default:
        break;
    }
  }
}

