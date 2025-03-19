using Godot;
using System;
using System.Collections.Generic;

public partial class LevelTilemap : Node
{
  [Export] 
  private TileMapLayer _groundTiles;
  [Export] 
  private TileMapLayer _waterTiles;
  [Export] 
  private TileMapLayer _obstacleTiles;

  //public void SetTiles(List<(TileUtils.TileType, Vector2I)> tiles)
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
      (int, Vector2I) tileAtlasInfo = TileUtils.GetTileAtlasInfo(tileType);
      targetLayer.SetCell(coords, tileAtlasInfo.Item1, tileAtlasInfo.Item2);
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
}
