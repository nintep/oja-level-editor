using Godot;
using System;

public partial class GridMovement : Node2D
{
  [Export]
  private Node2D _selfNode;

  [Export]
  private float _moveDelay = 0.2f;

  private const int _tileSize = 32;
  private float _moveDelayRemaining = 0.0f;
  private RayCast2D _raycast;

  public Vector2 FacingDirection {get; private set;}

  public override void _Ready()
  {
    //Snap position to grid intersection
    _selfNode.Position = _selfNode.Position.Snapped(Vector2.One * _tileSize);
    //Move to center of the grid
    _selfNode.Position += Vector2.Up * 0.6f * _tileSize;
    _selfNode.Position += Vector2.Right * 0.5f * _tileSize;
    FacingDirection = Vector2.Right;

    _raycast = GetNode<RayCast2D>("RayCast2D");
    if (_raycast == null)
    {
      GD.PrintErr("GridMovement: RayCast2D missing");
    }
  }

  public override void _Process(double delta)
  {
      if (_moveDelayRemaining > 0)
      {
        _moveDelayRemaining -= (float)delta;
      }
  }

  public void Turn(bool clockwise)
  {
    if (_moveDelayRemaining <= 0)
    {
      FacingDirection = GetUnitDirectionRotated(FacingDirection, clockwise);
      _moveDelayRemaining = _moveDelay;
    }
  }

  public void Move(Vector2 direction)
  {
    if (direction.Length() > 0 && _moveDelayRemaining <= 0)
    {
      FacingDirection = GetPrimaryUnitDirection(direction);
      
      _raycast.TargetPosition = FacingDirection * _tileSize;
      _raycast.ForceRaycastUpdate();

      if (!_raycast.IsColliding())
      {
        _selfNode.Position += FacingDirection * _tileSize;
        _moveDelayRemaining = _moveDelay;
      }
    }
  }

  private Vector2 GetPrimaryUnitDirection(Vector2 direction)
  {
    if (direction.Y > 0) return Vector2.Up;
    if (direction.Y < 0) return Vector2.Down;
    if (direction.X > 0) return Vector2.Right;
    if (direction.X < 0) return Vector2.Left;
    return Vector2.Zero;
  }

  private Vector2 GetUnitDirectionRotated(Vector2 direction, bool clockwise)
  {
    if (direction.Y > 0) return clockwise ? Vector2.Left : Vector2.Right;
    if (direction.Y < 0) return clockwise ? Vector2.Right : Vector2.Left; 
    if (direction.X > 0) return clockwise ? Vector2.Down : Vector2.Up;
    if (direction.X < 0) return clockwise ? Vector2.Up : Vector2.Down;
    return Vector2.Zero;
  }

  public string GetDirectionString(Vector2 direction)
  {
    if (direction.Y > 0) return "down";
    if (direction.Y < 0) return "up";
    if (direction.X > 0) return "right";
    if (direction.X < 0) return "left";
    return "";
  }

}
