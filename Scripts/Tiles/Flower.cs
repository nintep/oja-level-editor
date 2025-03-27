using Godot;
using System;

public partial class Flower : AnimatedSprite2D
{
  public bool IsAlive {get; private set;}
  
  private bool _isAnimationPaused;

  //Animation names
  private const string _animationIdleAlive = "idle_alive";
  private const string _animationIdleDead = "idle_dead";
  private const string _animationWake = "awake";
  private const string _animationDie = "die";

  private Node test;
  public override void _Ready()
  {
    //Find TileMap class in parent
    Node parentNode = FindParent("LevelTileMap");
    if (parentNode == null || parentNode.GetType() != typeof(LevelTilemap))
    {
      GD.PrintErr("Flower: Level container not found");
      return;
    }

    IsAlive = true;
    SetPaused(true, true);

    LevelTilemap tileMap = (LevelTilemap)parentNode;
    tileMap.OnFlowerInstantiated(GlobalPosition, this);
  }

  public void SetPaused(bool paused, bool resetAnimation)
  {
    if (_isAnimationPaused == paused) return;
    _isAnimationPaused = paused;

    if (resetAnimation)
    {
      SetFrameAndProgress(0, 0);
    }

    if (_isAnimationPaused)
    {
      Pause();
    }
    else
    {
      Play();
    }
  }

  public void SetAlive(bool alive)
  {
    if (IsAlive == alive) return;

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

