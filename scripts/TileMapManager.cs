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

  public override void _Ready()
  {
    if (_groundTiles == null || _waterTiles == null || _obstacleTiles == null)
    {
      GD.PrintErr("Tilemap layer not found");
    }
    _flowers = new Dictionary<int, Flower>();
    _flowerCoords = new Dictionary<int, Vector2I>();

    _tileLibrary = new TileLibrary();

  }

  public override void _Process(double delta)
  {
    
  }

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

  private void RefreshFlowers()
  {

  }

  private void RefreshFlower(int flowerId)
  {
    bool alive = IsNextToWater(_flowerCoords[flowerId]);
    _flowers[flowerId].SetAlive(alive);
  }

  private bool IsNextToWater(Vector2I coords)
  {
    GD.Print("Checking if next to water: " + coords);
    Vector2I[] neighbors = _waterTiles.GetSurroundingCells(coords).ToArray<Vector2I>();

    foreach (Vector2I i in neighbors)
    {
      if (_waterTiles.GetCellAtlasCoords(i) != new Vector2I(-1, -1))
      {
        GD.Print("-- true");
        return true;
      }
    }
    GD.Print("-- false");
    return false;
  }

}
