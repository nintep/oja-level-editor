using Godot;
using System;

public partial class Player : AnimatedSprite2D
{

  [Signal]
  public delegate void PlayerDiggingEventHandler(Vector2 playerPosition, Vector2I facingDirection);

  private GridMovement _gridMovement;

  private bool _digInProgress = false;

  public void SetTileSize(float tileSize)
  {
    _gridMovement._tileSize = tileSize;
  }

  public override void _Ready()
  {
      _gridMovement = GetNodeOrNull<GridMovement>("GridMovement");
      if (_gridMovement == null)
      {
        GD.PrintErr("Player's grid movement not found");
      }

      UpdateAnimation();
  }

  public override void _Process(double delta)
  {
      bool dig = Input.IsActionJustPressed("dig");

      Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_down", "move_up");
      bool turnLeft = Input.IsActionPressed("turn_left");
      bool turnRight = Input.IsActionPressed("turn_right");

      if (dig)
      {
        _digInProgress = true;
        EmitSignal(SignalName.PlayerDigging, GlobalPosition, _gridMovement.FacingDirection);
      }

      if (!_digInProgress)
      {
        if (inputDir.Length() > 0)
        {
          _gridMovement.Move(inputDir);
        }
        else if (turnLeft || turnRight)
        {
          _gridMovement.Turn(turnRight);
        }
      }

      UpdateAnimation();
  }

  private void OnAnimationFinished()
  {
    string currentAnimation = Animation;
    if (currentAnimation.Substr(0,4) == "dig_")
    {
      _digInProgress = false;      
    }
  }

  private void UpdateAnimation()
  {
    string facingDir = _gridMovement.GetDirectionString(_gridMovement.FacingDirection);
    string animationPrefix = _digInProgress ? "dig_" : "idle_";

    string animationState = animationPrefix + facingDir;
    Play(animationState);
  }

}
