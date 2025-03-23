using Godot;
using System;

public partial class TileMapMouseUtil : Node
{

  [Signal]
  public delegate void TileClickedEventHandler(Vector2I coords, MouseButton button);

  [Signal]
  public delegate void TileCopiedEventHandler(TileUtils.TileType tileType);

  [Export] private LevelTilemap _tileMap;

  [Export] private Sprite2D _highliht;

  private bool _tileChangedLastFrame;
  private Vector2 _latestTilePos;
  private MouseButton _latestButtonPressed;

  public override void _Process(double delta)
  {
    //If tile has been changed, forget latest button press
    if (_tileChangedLastFrame)
    {
      _latestButtonPressed = MouseButton.None;
    }

    //Get highest priority mouse button
    MouseButton primaryButton = Input.IsMouseButtonPressed(MouseButton.Middle) ? MouseButton.Middle : MouseButton.None;
    primaryButton = Input.IsMouseButtonPressed(MouseButton.Right) ? MouseButton.Right : primaryButton;
    primaryButton = Input.IsMouseButtonPressed(MouseButton.Left) ? MouseButton.Left : primaryButton;

    if (!_tileChangedLastFrame && primaryButton == _latestButtonPressed)
    {
      return;
    }

    if (primaryButton != MouseButton.None)
    {
      Vector2 mousePos = GetViewport().GetMousePosition();
      if (!_tileMap.ContainsPoint(mousePos))
      {
        return;
      }

      Vector2I tilePos = _tileMap.GetCoordsOfTileAt(mousePos);

      if (primaryButton == MouseButton.Middle)
      {
        //Trigger selection of tiletype under mouse cursor
        TileUtils.TileType tileType = _tileMap.GetTypeOfTopTileAt(tilePos);
        EmitSignal(SignalName.TileCopied, (int)tileType);
      }
      else
      {
        EmitSignal(SignalName.TileClicked, tilePos, (int)primaryButton);
      }

      _latestButtonPressed = primaryButton;
    }
  }

  public override void _Input(InputEvent @event)
  {
      if (@event is InputEventMouseMotion eventMouseMotion)
      {
        HandleMouseMovement(eventMouseMotion);
      }
  }

  private void HandleMouseMovement(InputEventMouseMotion eventMouseMotion)
  {
    //GD.Print("Mouse Motion at: ", eventMouseMotion.Position);
    Vector2 mousePos = eventMouseMotion.Position;
    if (_tileMap.ContainsPoint(mousePos))
    {
      _highliht.Show();
      Vector2 tilePos = _tileMap.GetCenterOfTileAt(mousePos);
      _highliht.Position = tilePos;

      if (_latestTilePos != tilePos)
      {
        _latestTilePos = tilePos;
        _tileChangedLastFrame = true;
      }
      else
      {
        _tileChangedLastFrame = false;
      }
    }
    else
    {
      _highliht.Hide();
      _tileChangedLastFrame = true;
    }
  }
}
