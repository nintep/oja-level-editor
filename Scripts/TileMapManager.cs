using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TileMapManager : Node2D
{
  [Export] 
  private TileMapLayer _groundTiles;
  [Export] 
  private TileMapLayer _waterTiles;
  [Export] 
  private TileMapLayer _obstacleTiles;


  //Flowers have unique IDs
  private Dictionary<int, Flower> _flowers;
  private Dictionary<int, Vector2I> _flowerCoords;

  private TileLibrary _tileLibrary;

  private List<Vector2I> _springTiles;
  private const float _waterDelayTime = 1f;
  private float _waterDelayRemaining = 0;

  public override void _Ready()
  {
    if (_groundTiles == null || _waterTiles == null || _obstacleTiles == null)
    {
      GD.PrintErr("Tilemap layer not found");
    }
    _flowers = new Dictionary<int, Flower>();
    _flowerCoords = new Dictionary<int, Vector2I>();

    _tileLibrary = new TileLibrary();

    _springTiles = GetAllSpringLocations();
    if (_springTiles.Count == 0)
    {
      GD.PrintErr("No spring tiles in level!");
    }
    _waterDelayRemaining = _waterDelayTime;

  }

  public override void _Process(double delta)
  {
    _waterDelayRemaining -= (float)delta;

    if (_waterDelayRemaining <= 0)
    {
      RefreshWater();
      foreach (int flower in _flowers.Keys)
      {
        RefreshFlower(flower);
      }

      _waterDelayRemaining = _waterDelayTime;
    }
  }
  
  ///////////////////////////////////////////////////////
  ///////////////// Digging //////////////////////
  ///////////////////////////////////////////////////////

  public void Dig(Vector2 playerPos, Vector2 direction)
  {
    Vector2I playerCoords = _groundTiles.LocalToMap(_groundTiles.ToLocal(playerPos));

    if (direction.Length() != 1)
    {
      GD.PrintErr("Invalid direction given");
      return;
    }

    TileSet.CellNeighbor targetCell;
    TileSet.CellNeighbor behindCell;

    if (direction == Vector2.Up) {targetCell = TileSet.CellNeighbor.TopSide; behindCell = TileSet.CellNeighbor.BottomSide;}
    else if (direction == Vector2.Right) {targetCell = TileSet.CellNeighbor.RightSide; behindCell = TileSet.CellNeighbor.LeftSide;}
    else if (direction == Vector2.Left) {targetCell = TileSet.CellNeighbor.LeftSide; behindCell = TileSet.CellNeighbor.RightSide;}
    else {targetCell = TileSet.CellNeighbor.BottomSide; behindCell = TileSet.CellNeighbor.TopSide;}

    Vector2I targetCoords = _groundTiles.GetNeighborCell(playerCoords, targetCell);
    Vector2I behindCoords = _groundTiles.GetNeighborCell(playerCoords, behindCell);

    TileLibrary.TileType targetTile = GetTopmostTile(targetCoords);
    TileLibrary.TileType behindTile = GetTopmostTile(behindCoords);

    if (targetTile == TileLibrary.TileType.ground_grass && CanReceiveDirt(behindTile))
    {
      RemoveDirt(targetCoords, targetTile);
      AddDirt(behindCoords, behindTile);      
    }
    else if (targetTile == TileLibrary.TileType.dirtPile && CanReceiveDirt(behindTile))
    {
      RemoveDirt(targetCoords, targetTile);
      AddDirt(behindCoords, behindTile);
    }
  }

  private void RemoveDirt(Vector2I coords, TileLibrary.TileType currentTile)
  {
    if (currentTile == TileLibrary.TileType.ground_grass)
    {
      //Add hole tile
      int tileSetSourceId = _tileLibrary.GetTileSetSourceId(TileLibrary.TileType.hole);
      Vector2I atlasCoords = _tileLibrary.GetTileAtlasCoords(TileLibrary.TileType.hole);
      _obstacleTiles.SetCell(coords, tileSetSourceId, atlasCoords);
    }
    else if (currentTile == TileLibrary.TileType.dirtPile)
    {
      //Remove dirt pile
      _obstacleTiles.EraseCell(coords);
    }
  }

  private void AddDirt(Vector2I coords, TileLibrary.TileType currentTile)
  {
    if (currentTile == TileLibrary.TileType.hole)
      {
        //Remove hole tile
        _obstacleTiles.EraseCell(coords);
      }
      else if (currentTile == TileLibrary.TileType.water)
      {
        //Remove water tile
        _waterTiles.EraseCell(coords);
      }
      else
      {
        //Add dirt pile
        int tileSetSourceId = _tileLibrary.GetTileSetSourceId(TileLibrary.TileType.dirtPile);
        Vector2I atlasCoords = _tileLibrary.GetTileAtlasCoords(TileLibrary.TileType.dirtPile);
        _obstacleTiles.SetCell(coords, tileSetSourceId, atlasCoords);
      }
  }

  private bool CanReceiveDirt(TileLibrary.TileType tileType)
  {
    if (tileType == TileLibrary.TileType.ground_grass || tileType == TileLibrary.TileType.ground_stone)
    {
      return true;
    }
    if (tileType == TileLibrary.TileType.hole || tileType == TileLibrary.TileType.water)
    {
      return true;
    }
    return false;
  }

  private TileLibrary.TileType GetTopmostTile(Vector2I coords)
  {
    if (_flowerCoords.Values.Contains(coords))
    {
      return TileLibrary.TileType.flower;
    }

    TileData groundData = _groundTiles.GetCellTileData(coords);
    TileData waterData = _waterTiles.GetCellTileData(coords);
    TileData obstacleData = _obstacleTiles.GetCellTileData(coords);

    // Prioritize obstacles, then water, then ground
    TileData targetTile = obstacleData != null ? obstacleData : (waterData != null ? waterData : groundData);

    if (targetTile == null)
    {
      return TileLibrary.TileType.none;
    }

    string tileTypeName = (string)targetTile.GetCustomData("tileType");
    TileLibrary.TileType tileType = _tileLibrary.GetTileType(tileTypeName);
    
    return tileType;
  }


  ///////////////////////////////////////////////////////
  ///////////////// Water //////////////////////
  ///////////////////////////////////////////////////////
  
  private void RefreshWater()
  {
    if (_springTiles.Count == 0) return;

    //Collect connected water tiles
    List<Vector2I> connectedWaterTiles = new List<Vector2I>();
    foreach (Vector2I spring in _springTiles)
    {
      AddConnectedWaterTiles(connectedWaterTiles, spring);
    }

    //Remove unconnected water tiles and add holes
    foreach (var waterTile in _waterTiles.GetUsedCells())
    {
      if (!connectedWaterTiles.Contains(waterTile))
      {
        _waterTiles.EraseCell(waterTile);
        RemoveDirt(waterTile, TileLibrary.TileType.ground_grass);
      }
    }

    //Fill holes next to water tiles
    foreach (Vector2I waterTile in _waterTiles.GetUsedCells())
    {
      List<Vector2I> holes = GetNeighborsOfType(waterTile, TileLibrary.TileType.hole);
      foreach (Vector2I hole in holes)
      {
        //Remove hole tile
        _obstacleTiles.EraseCell(hole);
        //Add water tile
        int tileSetSourceId = _tileLibrary.GetTileSetSourceId(TileLibrary.TileType.water);
        Vector2I atlasCoords = _tileLibrary.GetTileAtlasCoords(TileLibrary.TileType.water);
        _waterTiles.SetCell(hole, tileSetSourceId, atlasCoords);
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
    List<Vector2I> neighbors = GetNeighborsOfTypes(startingPoint, [TileLibrary.TileType.water, TileLibrary.TileType.water_spring]);

    for (int i = 0; i < neighbors.Count; i++)
    {
      AddConnectedWaterTiles(currentList, neighbors[i]);
    }
    
  }

  private List<Vector2I> GetAllSpringLocations()
  {
    List<Vector2I> cells = _waterTiles.GetUsedCells().ToList();

    //Get spring tiles
    cells = cells.Where(cell => _tileLibrary.GetTileType(
      (string)_waterTiles.GetCellTileData(cell)
      .GetCustomData("tileType")) == TileLibrary.TileType.water_spring)
      .ToList();

    return cells;
  }  
  
  ///////////////////////////////////////////////////////
  ///////////////// Flowers //////////////////////
  ///////////////////////////////////////////////////////

  public void OnFlowerInstantiated(Vector2 pos, Flower flower)
  {
    int id = _flowers.Count;
    Vector2I coords = _groundTiles.LocalToMap(_groundTiles.ToLocal(pos));

    _flowers[id] = flower;
    _flowerCoords[id] = coords;

    GD.Print("Flower added " + coords);
    RefreshFlower(id);

    //IsNextToWater(coords);
  }

  private void RefreshFlower(int flowerId)
  {
    bool alive = IsNextToWater(_flowerCoords[flowerId]);
    _flowers[flowerId].SetAlive(alive);
  }

  ///////////////////////////////////////////////////////
  ///////////////// Utils //////////////////////
  ///////////////////////////////////////////////////////
  
  private bool IsNextToTileType(Vector2I coords, TileLibrary.TileType tileType)
  {
    TileMapLayer layer = GetLayerForTileType(tileType);

    foreach (Vector2I neighbor in layer.GetSurroundingCells(coords))
    {
      TileData tileData = layer.GetCellTileData(neighbor);
      if (tileData != null && _tileLibrary.GetTileType((string)tileData.GetCustomData("tileType")) == tileType)
      {
        return true;
      }
    }
    return false;
  }

  private bool IsNextToWater(Vector2I coords)
  {
    foreach (Vector2I neighbor in _waterTiles.GetSurroundingCells(coords))
    {
      TileData waterData = _waterTiles.GetCellTileData(neighbor);
      if (waterData != null)
      {
        return true;
      }
    }
    return false;
  }


  private List<Vector2I> GetNeighborsOfTypes(Vector2I coords, List<TileLibrary.TileType> tileTypes)
  {
    List<Vector2I> neighbors = new List<Vector2I>();

    foreach (TileLibrary.TileType type in tileTypes)
    {
      neighbors = neighbors.Concat(GetNeighborsOfType(coords, type)).ToList();
    }

    return neighbors;
  }

  private List<Vector2I> GetNeighborsOfType(Vector2I coords, TileLibrary.TileType tileType)
  {
    List<Vector2I> neighbors = new List<Vector2I>();

    TileMapLayer layer = GetLayerForTileType(tileType);

    foreach (Vector2I neighbor in layer.GetSurroundingCells(coords))
    {
      TileData tileData = layer.GetCellTileData(neighbor);
      if (tileData != null && _tileLibrary.GetTileType((string)tileData.GetCustomData("tileType")) == tileType)
      {
        neighbors.Add(neighbor);
      }
    }
    return neighbors;
  }

  private List<Vector2I> GetAllTilesOfType(TileLibrary.TileType tileType)
  {
    List<Vector2I> tiles = new List<Vector2I>();
    
    TileMapLayer layer = GetLayerForTileType(tileType);

    foreach (Vector2I cell in layer.GetUsedCells())
    {
      TileData tileData = layer.GetCellTileData(cell);
      if (tileData != null && _tileLibrary.GetTileType((string)tileData.GetCustomData("tileType")) == tileType)
      {
        tiles.Add(cell);
      }
    }
    return tiles;
  }

  private TileMapLayer GetLayerForTileType(TileLibrary.TileType tileType)
  {
    switch (tileType)
    {
      case TileLibrary.TileType.ground_grass:
        return _groundTiles;
      case TileLibrary.TileType.ground_stone:
        return _groundTiles;
      case TileLibrary.TileType.water:
        return _waterTiles;
      case TileLibrary.TileType.water_spring:
        return _waterTiles;
      default:
        return _obstacleTiles;
    }
  }

}
