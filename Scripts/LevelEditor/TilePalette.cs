using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TilePalette : Control
{

  [Signal]
  public delegate void TilePlacedEventHandler(Vector2I coords, TileUtils.TileType tileType);

  [Signal]
  public delegate void TileErasedEventHandler(Vector2I coords);

  [Export] private TileMapMouseUtil _mouseUtility;
  [Export] private ItemList _itemList;

  private bool _allowPlacement = true;
  private float _placementDelayRemaining = 0;

  private TileUtils.TileType _currentTile = TileUtils.TileType.none;

  private readonly TileUtils.TileType[] _tileIndexes = 
  [
    TileUtils.TileType.ground_grass,
    TileUtils.TileType.ground_stone,
    TileUtils.TileType.water,
    TileUtils.TileType.water_spring,
    TileUtils.TileType.hole,
    TileUtils.TileType.rock,
    TileUtils.TileType.dirtPile,
    TileUtils.TileType.flower,
    TileUtils.TileType.startTile,
  ];

  public override void _Ready()
  {
    _mouseUtility.TileClicked += HandleMouseClick;
    _mouseUtility.TileCopied += SetCurrentTileType;

    _mouseUtility.SetDrawModeActive(true);
  }

  public override void _Process(double delta)
  {
    if (_placementDelayRemaining > 0)
    {
      _placementDelayRemaining -= (float)delta;
      if (_placementDelayRemaining <= 0)
      {
        _allowPlacement = true;
      }
    }
  }

  public void SetAllowPlacement(bool allow)
  {
    _allowPlacement = false;
    if (allow)
    {
      _placementDelayRemaining = 0.2f;
    }

    _mouseUtility.SetDrawModeActive(allow);
  }

  public void SetCurrentTileType(TileUtils.TileType tileType)
  {
    GD.Print("TilePalette: set current tile type to " + tileType);
    int index = Array.IndexOf(_tileIndexes, tileType);
    
    if (index == -1)
    {
      _itemList.DeselectAll();
      _currentTile = TileUtils.TileType.none;
    }
    else
    {
      _itemList.Select(index);
      _itemList.EnsureCurrentIsVisible();
      _currentTile = tileType;
    }
  }

  private void OnItemListItemSelected(int index)
  {
    _currentTile = _tileIndexes[index];
    GD.Print("TilePalette: Tile type selected: " + _currentTile);
  }

  private void HandleMouseClick(Vector2I coords, MouseButton button)
  {
    if (!_allowPlacement)
    {
      return;
    }

    if (button == MouseButton.Left)
    {
      EmitSignal(SignalName.TilePlaced, coords, (int)_currentTile);
    }
    else if (button == MouseButton.Right)
    {
      EmitSignal(SignalName.TileErased, coords);
    }
  }
}