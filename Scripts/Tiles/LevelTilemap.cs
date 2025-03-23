using Godot;
using System;
using System.Collections.Generic;

public partial class LevelTilemap : Node
{
  [Export] 
  private TileMapLayer _backgroundTiles;
  [Export] 
  private TileMapLayer _groundTiles;
  [Export] 
  private TileMapLayer _waterTiles;
  [Export] 
  private TileMapLayer _obstacleTiles;

  public Vector2 GetCenterOfTileAt(Vector2 globalPos)
  {
    Vector2I coords = _backgroundTiles.LocalToMap(_backgroundTiles.ToLocal(globalPos));
    Vector2 worldPos = _backgroundTiles.ToGlobal(_backgroundTiles.MapToLocal(coords));

    return worldPos;
  }

  public Vector2I GetCoordsOfTileAt(Vector2 globalPos)
  {
    Vector2I coords = _backgroundTiles.LocalToMap(_backgroundTiles.ToLocal(globalPos));
    return coords;
  }

  public TileUtils.TileType GetTypeOfTopTileAt(Vector2I coords)
  {
    TileData groundData = _groundTiles.GetCellTileData(coords);
    TileData waterData = _waterTiles.GetCellTileData(coords);
    TileData obstacleData = _obstacleTiles.GetCellTileData(coords);

    // Prioritize obstacles, then water, then ground
    TileData targetTile = obstacleData != null ? obstacleData : (waterData != null ? waterData : groundData);

    if (targetTile == null)
    {
      return TileUtils.TileType.none;
    }

    TileUtils.TileType tileType = TileUtils.GetTileType(targetTile);
    return tileType;
  }

  public bool ContainsPoint(Vector2 globalPos)
  {
    Vector2I coords = _backgroundTiles.LocalToMap(_backgroundTiles.ToLocal(globalPos));
    Rect2I rect = _backgroundTiles.GetUsedRect();

    return rect.HasPoint(coords);
  }

  public void SetTiles(TilePlacementData[] tiles)
  {
    //Clear existing tiles
    _groundTiles.Clear();
    _waterTiles.Clear();
    _obstacleTiles.Clear();

    //Add tiles to tilemap
    foreach (TilePlacementData tile in tiles)
    {
      TileUtils.TileType tileType = tile.tileType;
      Vector2I coords = tile.coords;

      //Choose correct layer for tile type
      TileMapLayer targetLayer = GetLayer(TileUtils.GetTargetLayerType(tileType));

      //Verify that cell is empty
      TileData tileData = targetLayer.GetCellTileData(coords);
      if (tileData != null)
      {
        GD.PrintErr("Level data has overlapping tiles of invalid types");
        return;
      }

      //Set cell
      SetTile(coords, tileType);
    }

    //Repair broken tiles
    foreach (Vector2I coords in _backgroundTiles.GetUsedCells())
    {
      RepairTilesAtCoords(coords);
    }
  }

  //public List<(TileUtils.TileType, Vector2I)> GetTiles()
  public TilePlacementData[] GetTiles()
  {
    List<TilePlacementData> tiles = new List<TilePlacementData>();

    //Add ground tiles
    foreach (Vector2I coords in _groundTiles.GetUsedCells())
    {
      TileData tileData = _groundTiles.GetCellTileData(coords);
      TileUtils.TileType tileType = TileUtils.GetTileType(tileData);
      tiles.Add(new TilePlacementData(tileType, coords));
    }

    //Add water tiles
    foreach (Vector2I coords in _waterTiles.GetUsedCells())
    {
      TileData tileData = _waterTiles.GetCellTileData(coords);
      TileUtils.TileType tileType = TileUtils.GetTileType(tileData);
      tiles.Add(new TilePlacementData(tileType, coords));
    }

    //Add obstacle tiles
    foreach (Vector2I coords in _obstacleTiles.GetUsedCells())
    {
      TileData tileData = _obstacleTiles.GetCellTileData(coords);
      TileUtils.TileType tileType = TileUtils.GetTileType(tileData);
      tiles.Add(new TilePlacementData(tileType, coords));
    }

    return tiles.ToArray();
  }

  private TileMapLayer GetLayer(TileUtils.TileMapLayerType layerType)
  {
    switch (layerType)
    {
      case TileUtils.TileMapLayerType.ground:
        return _groundTiles;
      case TileUtils.TileMapLayerType.water:
        return _waterTiles;
      default:
        return _obstacleTiles;
    }
  }

  private void SetTile(Vector2I coords, TileUtils.TileType tileType)
  {
    TileMapLayer targetLayer = GetLayer(TileUtils.GetTargetLayerType(tileType));

    //Set cell
    (int, Vector2I) tileAtlasInfo = TileUtils.GetTileAtlasInfo(tileType);
    targetLayer.SetCell(coords, tileAtlasInfo.Item1, tileAtlasInfo.Item2);
  }

  private void RepairTilesAtCoords(Vector2I coords)
  {
    TileUtils.TileType groundTileType = TileUtils.GetTileType(_groundTiles.GetCellTileData(coords));
    TileUtils.TileType waterTileType = TileUtils.GetTileType(_waterTiles.GetCellTileData(coords));
    TileUtils.TileType obstacleTileType = TileUtils.GetTileType(_obstacleTiles.GetCellTileData(coords));

    //Ground under water must be grass
    if (waterTileType != TileUtils.TileType.none)
    {
      SetTile(coords, TileUtils.TileType.ground_grass);
    }

    //Obstacles other than rocks cannot be on top of water
    if (waterTileType != TileUtils.TileType.none && obstacleTileType != TileUtils.TileType.rock)
    {
      _obstacleTiles.EraseCell(coords);
    }

    //Water under rocks must not be spring
    if (obstacleTileType == TileUtils.TileType.rock && waterTileType == TileUtils.TileType.water_spring)
    {
      SetTile(coords, TileUtils.TileType.water);
    }

    //Ground under holes or TODO: flowers must be grass
    if (obstacleTileType == TileUtils.TileType.hole)
    {
      SetTile(coords, TileUtils.TileType.ground_grass);
    }
  }

  public void PaintTile(Vector2I coords, TileUtils.TileType tileType)
  {
    if (tileType == TileUtils.TileType.none)
    {
      GD.PrintErr("LevelTileMap: trying to pain tile with tiletype none");
      return;
    }
    
    SetTile(coords, tileType);

    //Handle special cases - generally painted tile takes priority over existing tiles

    //If painting ground tiles over water tiles, remove water tile
    if (tileType == TileUtils.TileType.ground_grass || tileType == TileUtils.TileType.ground_stone)
    {
      if (_waterTiles.GetCellTileData(coords) != null)
      {
        _waterTiles.EraseCell(coords);
      }
    }

    //If painting stone ground tiles, remove holes and TODO: flowers
    if (tileType == TileUtils.TileType.ground_stone)
    {
      if (TileUtils.GetTileType(_obstacleTiles.GetCellTileData(coords)) == TileUtils.TileType.hole)
      {
        _obstacleTiles.EraseCell(coords);
      }
    }

    //If painting spring tiles, erase rocks
    if (tileType == TileUtils.TileType.water_spring)
    {
      if (TileUtils.GetTileType(_obstacleTiles.GetCellTileData(coords)) == TileUtils.TileType.rock)
      {
        _obstacleTiles.EraseCell(coords);
      }
    }

    //If painting obstacle tiles other than rocks, erase water (and add grass)
    if (TileUtils.GetTargetLayerType(tileType) == TileUtils.TileMapLayerType.obstacle && tileType != TileUtils.TileType.rock)
    {
      if (_waterTiles.GetCellTileData(coords) != null)
      {
        _waterTiles.EraseCell(coords);
        SetTile(coords, TileUtils.TileType.ground_grass);
      }
    }

    //If painting hole tiles

    RepairTilesAtCoords(coords);
  }

  public void EraseTile(Vector2I coords)
  {
    //If contains obstacle, erase it
    if (_obstacleTiles.GetCellTileData(coords) != null)
    {
      _obstacleTiles.EraseCell(coords);
    }

    //If contains spring, change it to normal water
    if (TileUtils.GetTileType(_waterTiles.GetCellTileData(coords)) == TileUtils.TileType.water_spring)
    {
      SetTile(coords, TileUtils.TileType.water);
    }
  }

}
