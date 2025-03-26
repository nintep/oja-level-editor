using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

  public Vector2 GetCenterOfTile(Vector2I coords)
  {
    return _backgroundTiles.ToGlobal(_backgroundTiles.MapToLocal(coords));
  }

  public bool ContainsPoint(Vector2 globalPos)
  {
    Vector2I coords = _backgroundTiles.LocalToMap(_backgroundTiles.ToLocal(globalPos));
    Rect2I rect = _backgroundTiles.GetUsedRect();

    return rect.HasPoint(coords);
  }

  public float GetTileSize()
  {
    Vector2 tile_1 = GetCenterOfTile(new Vector2I(0, 0));
    Vector2 tile_2 = GetCenterOfTile(new Vector2I(0, 1));

    return tile_2.Y - tile_1.Y;
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

  private TileUtils.TileType GetTypeOfTileAt(Vector2I coords, TileMapLayer layer)
  {
    TileData tileData = layer.GetCellTileData(coords);

    if (tileData == null)
    {
      return TileUtils.TileType.none;
    }

    TileUtils.TileType tileType = TileUtils.GetTileType(tileData);
    return tileType;
  }

  public bool ContainsStartTile()
  {
    foreach (Vector2I tile in _obstacleTiles.GetUsedCells())
    {
      if (GetTypeOfTileAt(tile, _obstacleTiles) == TileUtils.TileType.startTile)
      {
        return true;
      }
    }
    return false;
  }

  public Vector2 RemoveStartTile()
  {
    Vector2I coords = new Vector2I(0, 0);
    foreach (Vector2I tile in _obstacleTiles.GetUsedCells())
    {
      if (GetTypeOfTileAt(tile, _obstacleTiles) == TileUtils.TileType.startTile)
      {
        _obstacleTiles.EraseCell(tile);
        coords = tile;
      }
    }

    return GetCenterOfTile(coords);
  }

  private List<Vector2I> GetNeighborsOfTypes(Vector2I coords, List<TileUtils.TileType> tileTypes)
  {
    List<Vector2I> neighbors = new List<Vector2I>();
    foreach (TileUtils.TileType type in tileTypes)
    {
      neighbors = neighbors.Concat(GetNeighborsOfType(coords, type)).ToList();
    }

    return neighbors;
  }

  private List<Vector2I> GetNeighborsOfType(Vector2I coords, TileUtils.TileType tileType)
  {
    
    List<Vector2I> neighbors = new List<Vector2I>();

    TileUtils.TileMapLayerType layerType = TileUtils.GetTargetLayerType(tileType);
    TileMapLayer layer = GetLayer(layerType);

    foreach (Vector2I neighbor in layer.GetSurroundingCells(coords))
    {
      if (GetTypeOfTileAt(neighbor, layer) == tileType)
      {
        neighbors.Add(neighbor);
      }
    }
    return neighbors;
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

  public void PaintTile(Vector2I coords, TileUtils.TileType tileType)
  {
    if (tileType == TileUtils.TileType.none)
    {
      GD.PrintErr("LevelTileMap: trying to paint tile with tiletype none");
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

    //If painting player start tile, remove all other player starts
    if (TileUtils.GetTileType(_obstacleTiles.GetCellTileData(coords)) == TileUtils.TileType.startTile)
    {

    }

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

    //Delete all other start tiles
    if (obstacleTileType == TileUtils.TileType.startTile)
    {
      foreach (Vector2I tile in _obstacleTiles.GetUsedCells())
      {
        if (tile == coords) continue;

        if (GetTypeOfTileAt(tile, _obstacleTiles) == TileUtils.TileType.startTile)
        {
          _obstacleTiles.EraseCell(tile);
        }
      }
    }    
  }

  //////////////////////////

  public void RefreshWater()
  {
    //Collect spring tiles
    List<Vector2I> springTiles = new List<Vector2I>();
    foreach (Vector2I waterTile in _waterTiles.GetUsedCells())
    {
      if (GetTypeOfTileAt(waterTile, _waterTiles) == TileUtils.TileType.water_spring)
      {
        springTiles.Add(waterTile);
      }
    }

    if (springTiles.Count == 0) return;

    //Collect connected water tiles
    List<Vector2I> connectedWaterTiles = new List<Vector2I>();
    foreach (Vector2I spring in springTiles)
    {
      AddConnectedWaterTiles(connectedWaterTiles, spring);
    }

    //Remove unconnected water tiles and add holes
    foreach (Vector2I waterTile in _waterTiles.GetUsedCells())
    {
      if (!connectedWaterTiles.Contains(waterTile))
      {
        _waterTiles.EraseCell(waterTile);
        SetTile(waterTile, TileUtils.TileType.hole);
      }
    }

    //Fill holes next to water tiles
    foreach (Vector2I waterTile in _waterTiles.GetUsedCells())
    {
      List<Vector2I> holes = GetNeighborsOfType(waterTile, TileUtils.TileType.hole);
      foreach (Vector2I hole in holes)
      {
        //Remove hole tile
        _obstacleTiles.EraseCell(hole);
        //Add water tile
        SetTile(hole, TileUtils.TileType.water);
      }
    }
  }

  //Recursively collect all connected water tiles
  private void AddConnectedWaterTiles(List<Vector2I> currentList, Vector2I startingPoint)
  {
    if (currentList.Contains(startingPoint))
    {
      return;
    }

    currentList.Add(startingPoint);
    List<Vector2I> neighbors = GetNeighborsOfTypes(startingPoint, [TileUtils.TileType.water, TileUtils.TileType.water_spring]);

    for (int i = 0; i < neighbors.Count; i++)
    {
      AddConnectedWaterTiles(currentList, neighbors[i]);
    }
  }

  public void Dig(Vector2 playercoords, Vector2I direction)
  {
    //Check if diggin is allowed
    Vector2I playerCoords = _backgroundTiles.LocalToMap(_backgroundTiles.ToLocal(playercoords));

    TileSet.CellNeighbor targetCell;
    TileSet.CellNeighbor behindCell;

    if (direction == Vector2.Up) {targetCell = TileSet.CellNeighbor.TopSide; behindCell = TileSet.CellNeighbor.BottomSide;}
    else if (direction == Vector2.Right) {targetCell = TileSet.CellNeighbor.RightSide; behindCell = TileSet.CellNeighbor.LeftSide;}
    else if (direction == Vector2.Left) {targetCell = TileSet.CellNeighbor.LeftSide; behindCell = TileSet.CellNeighbor.RightSide;}
    else {targetCell = TileSet.CellNeighbor.BottomSide; behindCell = TileSet.CellNeighbor.TopSide;}

    Vector2I targetCoords = _backgroundTiles.GetNeighborCell(playerCoords, targetCell);
    Vector2I behindCoords = _backgroundTiles.GetNeighborCell(playerCoords, behindCell);

    TileUtils.TileType targetTile = GetTypeOfTopTileAt(targetCoords);
    TileUtils.TileType behindTile = GetTypeOfTopTileAt(behindCoords);

    if (TileUtils.CanReceiveDirt(behindTile) && TileUtils.CanBeDug(targetTile))
    {
      RemoveDirt(targetCoords);
      AddDirt(behindCoords);
    }
  }

  private void RemoveDirt(Vector2I coords)
  {
    TileUtils.TileType currentTile = GetTypeOfTopTileAt(coords);

    if (currentTile == TileUtils.TileType.ground_grass)
    {
      //Add hole tile
      SetTile(coords, TileUtils.TileType.hole);
    }
    else if (currentTile == TileUtils.TileType.dirtPile)
    {
      //Remove dirt pile
      _obstacleTiles.EraseCell(coords);
    }
  }

  private void AddDirt(Vector2I coords)
  {
    TileUtils.TileType currentTile = GetTypeOfTopTileAt(coords);

    if (currentTile == TileUtils.TileType.hole)
      {
        //Remove hole tile
        _obstacleTiles.EraseCell(coords);
      }
      else if (currentTile == TileUtils.TileType.water)
      {
        //Remove water tile
        _waterTiles.EraseCell(coords);
      }
      else
      {
        //Add dirt pile
        SetTile(coords, TileUtils.TileType.dirtPile);
      }
  }  
}
