using Godot;
using System;

public partial class Player : AnimatedSprite2D
{

  private GridMovement _gridMovement;

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
      Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_down", "move_up");
      bool turnLeft = Input.IsActionPressed("turn_left");
      bool turnRight = Input.IsActionPressed("turn_right");

      if (inputDir.Length() > 0)
      {
        _gridMovement.Move(inputDir);
        UpdateAnimation();
      }
      else if (turnLeft || turnRight)
      {
        _gridMovement.Turn(turnRight);
        UpdateAnimation();
      }
  }

  private void UpdateAnimation()
  {
    string facingDir = _gridMovement.GetDirectionString(_gridMovement.FacingDirection);
    string animationState = "idle_" + facingDir;

    Play(animationState);
  }

}
