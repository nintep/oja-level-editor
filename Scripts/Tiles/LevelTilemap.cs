using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LevelTilemap : Node
{
  [Export] private TileMapLayer _backgroundTiles;
  [Export] private TileMapLayer _groundTiles;
  [Export] private TileMapLayer _waterTiles;
  [Export] private TileMapLayer _obstacleTiles;

  //Flowers have unique IDs
  private Dictionary<int, Flower> _flowers;
  private Dictionary<int, Vector2I> _flowerCoords;

  private LevelTilemapUtility _util;

  public override void _Ready()
  {
    _flowers = new Dictionary<int, Flower>();
    _flowerCoords = new Dictionary<int, Vector2I>();

    _util = new LevelTilemapUtility([_backgroundTiles, _groundTiles, _waterTiles, _obstacleTiles], _flowerCoords);
  }

  //Method's that don't change tilemap state
  public Vector2 GetCenterOfTileAt(Vector2 globalPos) => _util.GetCenterOfTileAt(globalPos);
  public Vector2I GetCoordsOfTileAt(Vector2 globalPos) => _util.GetCoordsOfTileAt(globalPos);
  public Vector2 GetCenterOfTile(Vector2I coords) => _util.GetCenterOfTile(coords);
  public bool ContainsPoint(Vector2 globalPos) => _util.ContainsPoint(globalPos);
  public float GetTileSize() => _util.GetTileSize();
  public TileUtils.TileType GetTypeOfTopTileAt(Vector2I coords) => _util.GetTypeOfTopTileAt(coords);
  public bool ContainsStartTile() => _util.ContainsStartTile();

  public void OnFlowerInstantiated(Vector2 pos, Flower flower)
  {
    int id = _flowers.Count;
    Vector2I coords = _util.GetCoordsOfTileAt(pos);

    _flowers[id] = flower;
    _flowerCoords[id] = coords;

    GD.Print("Flower added " + coords);
  }

  public void PauseAnimations(bool paused, bool resetAnimations)
  {
    //pause flowers
    foreach (Flower flower in _flowers.Values)
    {
      flower.SetPaused(paused, resetAnimations);
    }
  }

  public Vector2 RemoveStartTile()
  {
    Vector2I coords = new Vector2I(0, 0);
    foreach (Vector2I tile in _obstacleTiles.GetUsedCells())
    {
      if (_util.GetTypeOfTileAt(tile, _obstacleTiles) == TileUtils.TileType.startTile)
      {
        RemoveTile(tile, _obstacleTiles);
        coords = tile;
      }
    }

    return GetCenterOfTile(coords);
  }

  public void SetTiles(TilePlacementData[] tiles)
  {
    //Clear existing tiles
    _groundTiles.Clear();
    _waterTiles.Clear();
    _obstacleTiles.Clear();
    _flowerCoords.Clear();
    _flowers.Clear();

    //Add tiles to tilemap
    foreach (TilePlacementData tile in tiles)
    {
      TileUtils.TileType tileType = tile.tileType;
      Vector2I coords = tile.coords;

      //Choose correct layer for tile type
      TileMapLayer targetLayer = _util.GetLayer(TileUtils.GetTargetLayerType(tileType));

      //Verify that cell is empty
      TileUtils.TileType currentTileType = _util.GetTypeOfTileAt(coords, targetLayer);
      if (currentTileType != TileUtils.TileType.none)
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
      if (tileType != TileUtils.TileType.none)
      {
        tiles.Add(new TilePlacementData(tileType, coords));
      }
    }

    //Add flower tiles
    foreach (int flowerID in _flowers.Keys)
    {
      tiles.Add(new TilePlacementData(TileUtils.TileType.flower, _flowerCoords[flowerID]));
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

    GD.Print("Tilemap: paint tile " + tileType);
    
    SetTile(coords, tileType);

    //Handle special cases - generally painted tile takes priority over existing tiles

    //If painting ground tiles over water tiles, remove water tile
    if (tileType == TileUtils.TileType.ground_grass || tileType == TileUtils.TileType.ground_stone)
    {
      RemoveTile(coords, _waterTiles);
    }

    //If painting stone ground tiles, remove holes and flowers
    if (tileType == TileUtils.TileType.ground_stone)
    {
      TileUtils.TileType currentTile = _util.GetTypeOfTileAt(coords, _obstacleTiles);
      if (currentTile == TileUtils.TileType.hole || currentTile == TileUtils.TileType.flower)
      {
        RemoveTile(coords, _obstacleTiles);
      }
    }

    //If painting spring tiles, erase rocks
    if (tileType == TileUtils.TileType.water_spring)
    {
      if (_util.GetTypeOfTileAt(coords, _obstacleTiles) == TileUtils.TileType.rock)
      {
        RemoveTile(coords, _obstacleTiles);
      }
    }

    //If painting obstacle tiles other than rocks, erase water (and add grass)
    if (TileUtils.GetTargetLayerType(tileType) == TileUtils.TileMapLayerType.obstacle && tileType != TileUtils.TileType.rock)
    {
      if (_util.GetTypeOfTileAt(coords, _waterTiles) != TileUtils.TileType.none)
      {
        RemoveTile(coords, _waterTiles);
        SetTile(coords, TileUtils.TileType.ground_grass);
      }
    }

    //If painting flowers, add grass underneath
    if (tileType == TileUtils.TileType.flower)
    {
      SetTile(coords, TileUtils.TileType.ground_grass);
    }

    RepairTilesAtCoords(coords);
  }

  //Tiles erased in level editor
  public void EraseTile(Vector2I coords)
  {
    //If contains obstacle, erase it
    if (_util.GetTypeOfTileAt(coords, _obstacleTiles) != TileUtils.TileType.none)
    {
      RemoveTile(coords, _obstacleTiles);
    }

    //If contains spring, change it to normal water
    if (_util.GetTypeOfTileAt(coords, _waterTiles) == TileUtils.TileType.water_spring)
    {
      SetTile(coords, TileUtils.TileType.water);
    }
  }

  private void SetTile(Vector2I coords, TileUtils.TileType tileType)
  {
    TileMapLayer targetLayer = _util.GetLayer(TileUtils.GetTargetLayerType(tileType));

    //Set cell
    (int, Vector2I, int) tileAtlasInfo = TileUtils.GetTileAtlasInfo(tileType);
    targetLayer.SetCell(coords, tileAtlasInfo.Item1, tileAtlasInfo.Item2, tileAtlasInfo.Item3);
  }

  private void RemoveTile(Vector2I coords, TileMapLayer layer)
  {
    if (layer == _obstacleTiles && _flowerCoords.Values.Contains(coords))
    {
      int flowerID = _flowerCoords.FirstOrDefault(x => x.Value == coords).Key;
      _flowerCoords.Remove(flowerID);
    }
    
    layer.EraseCell(coords);
  }

  private void RepairTilesAtCoords(Vector2I coords)
  {
    TileUtils.TileType waterTileType = _util.GetTypeOfTileAt(coords, _waterTiles);
    TileUtils.TileType obstacleTileType = _util.GetTypeOfTileAt(coords, _obstacleTiles);

    //Ground under water must be grass
    if (waterTileType != TileUtils.TileType.none)
    {
      SetTile(coords, TileUtils.TileType.ground_grass);
    }

    //Obstacles other than rocks cannot be on top of water
    if (waterTileType != TileUtils.TileType.none && obstacleTileType != TileUtils.TileType.rock)
    {
      RemoveTile(coords, _obstacleTiles);
    }

    //Water under rocks must not be spring
    if (obstacleTileType == TileUtils.TileType.rock && waterTileType == TileUtils.TileType.water_spring)
    {
      SetTile(coords, TileUtils.TileType.water);
    }

    //Ground under holes or flowers must be grass
    if (obstacleTileType == TileUtils.TileType.hole || obstacleTileType == TileUtils.TileType.flower)
    {
      SetTile(coords, TileUtils.TileType.ground_grass);
    }

    //Delete all other start tiles
    if (obstacleTileType == TileUtils.TileType.startTile)
    {
      foreach (Vector2I tile in _obstacleTiles.GetUsedCells())
      {
        if (tile == coords) continue;

        if (_util.GetTypeOfTileAt(tile, _obstacleTiles) == TileUtils.TileType.startTile)
        {
          RemoveTile(tile, _obstacleTiles);
        }
      }
    }    
  }

  //////////////////////////
  

  public void RefreshFlowers()
  {
    foreach (int id in _flowers.Keys)
    {
      Flower flower = _flowers[id];
      Vector2I flowerCoords = _flowerCoords[id];
      int waterCount = _util.GetNeighborsOfTypes(flowerCoords, [TileUtils.TileType.water, TileUtils.TileType.water_spring]).Count;

      flower.SetAlive(waterCount != 0);
    }
  } 


  public void RefreshWater()
  {
    //Collect spring tiles
    List<Vector2I> springTiles = new List<Vector2I>();
    foreach (Vector2I waterTile in _waterTiles.GetUsedCells())
    {
      if (_util.GetTypeOfTileAt(waterTile, _waterTiles) == TileUtils.TileType.water_spring)
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
        RemoveTile(waterTile, _waterTiles);
        SetTile(waterTile, TileUtils.TileType.hole);
      }
    }

    //Fill holes next to water tiles
    foreach (Vector2I waterTile in _waterTiles.GetUsedCells())
    {
      List<Vector2I> holes = _util.GetNeighborsOfType(waterTile, TileUtils.TileType.hole);
      foreach (Vector2I hole in holes)
      {
        //Remove hole tile
        RemoveTile(hole, _obstacleTiles);
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
    List<Vector2I> neighbors = _util.GetNeighborsOfTypes(startingPoint, [TileUtils.TileType.water, TileUtils.TileType.water_spring]);

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
      RemoveTile(coords, _obstacleTiles);
    }
  }

  private void AddDirt(Vector2I coords)
  {
    TileUtils.TileType currentTile = GetTypeOfTopTileAt(coords);

    if (currentTile == TileUtils.TileType.hole)
      {
        //Remove hole tile
        RemoveTile(coords, _obstacleTiles);
      }
      else if (currentTile == TileUtils.TileType.water)
      {
        //Remove water tile
        RemoveTile(coords, _waterTiles);
      }
      else
      {
        //Add dirt pile
        SetTile(coords, TileUtils.TileType.dirtPile);
      }
  }  
}
