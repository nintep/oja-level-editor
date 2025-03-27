using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

//Helper methods for managing leveltilemap
//methods don't change tilemap state
public class LevelTilemapUtility
{
  private TileMapLayer _backgroundTiles;
  private TileMapLayer _groundTiles;
  private TileMapLayer _waterTiles;
  private TileMapLayer _obstacleTiles;
  private Dictionary<int, Vector2I> _flowers;

  public LevelTilemapUtility(TileMapLayer[] layers, Dictionary<int, Vector2I> flowerDict)
  {
    _backgroundTiles = layers[0];
    _groundTiles = layers[1];
    _waterTiles = layers[2];
    _obstacleTiles = layers[3];
    _flowers = flowerDict;
  }

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

  public int GetFlowerAt(Vector2I coords)
  {
    if (_flowers.Values.Contains(coords))
    {
      return _flowers.FirstOrDefault(x => x.Value == coords).Key;
    }
    else
    {
      return -1;
    }
  }

  public TileUtils.TileType GetTypeOfTopTileAt(Vector2I coords)
  {
    //Check if flower tile
    if (GetFlowerAt(coords) != -1)
    {
      return TileUtils.TileType.flower;
    }

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

  public TileUtils.TileType GetTypeOfTileAt(Vector2I coords, TileMapLayer layer)
  {
    //Check if flower tile
    if (layer == _obstacleTiles && GetFlowerAt(coords) != -1)
    {
      return TileUtils.TileType.flower;
    }

    TileData tileData = layer.GetCellTileData(coords);
    if (tileData == null)
    {
      return TileUtils.TileType.none;
    }

    TileUtils.TileType tileType = TileUtils.GetTileType(tileData);
    return tileType;
  }

  public TileMapLayer GetLayer(TileUtils.TileMapLayerType layerType)
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

  public List<Vector2I> GetNeighborsOfType(Vector2I coords, TileUtils.TileType tileType)
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

  public List<Vector2I> GetNeighborsOfTypes(Vector2I coords, TileUtils.TileType[] tileTypes)
  {
    List<Vector2I> neighbors = new List<Vector2I>();
    foreach (TileUtils.TileType type in tileTypes)
    {
      neighbors = neighbors.Concat(GetNeighborsOfType(coords, type)).ToList();
    }

    return neighbors;
  }
}