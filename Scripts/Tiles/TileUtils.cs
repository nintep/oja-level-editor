using Godot;
using System;
using System.Collections.Generic;

public static class TileUtils
{

  private static readonly Dictionary<string, TileType> _tileNames = new Dictionary<string, TileType>
  {
    { "ground_grass", TileType.ground_grass },
    { "ground_stone", TileType.ground_stone },
    { "water", TileType.water },
    { "water_spring", TileType.water_spring },
    { "hole", TileType.hole },
    { "dirt_pile", TileType.dirtPile },
    { "rock", TileType.rock }
  };

  public static TileType GetTileType(TileData tileData)
  {
    if (tileData == null)
    {
      return TileType.none;
    }

    string tileTypeName = (string)tileData.GetCustomData("tileType");
    return _tileNames.GetValueOrDefault(tileTypeName);
  }

  public static TileMapLayerType GetTargetLayerType(TileType tileType)
  {
    switch (tileType)
    {
      case TileType.ground_grass:
        return TileMapLayerType.ground;
      case TileType.ground_stone:
        return TileMapLayerType.ground;
      case TileType.water:
        return TileMapLayerType.water;
      case TileType.water_spring:
        return TileMapLayerType.water;
      default:
        return TileMapLayerType.obstacle;
    }
  }

  public static (int, Vector2I) GetTileAtlasInfo(TileType tileType)
  {
    return (GetTileSetSourceId(tileType), GetTileAtlasCoords(tileType));
  }

  private static int GetTileSetSourceId(TileType tileType)
  {
    switch (tileType)
    {
      case TileType.ground_grass:
        return 4;
      case TileType.ground_stone:
        return 4;
      case TileType.water:
        return 3;
      case TileType.water_spring:
        return 3;
      case TileType.hole:
        return 7;
      case TileType.dirtPile:
        return 7;
      case TileType.rock:
        return 7;
      case TileType.flower:
        return 2;
      default:
        return -1;
    }
  }

  private static Vector2I GetTileAtlasCoords(TileType tileType)
  {
    switch (tileType)
    {
      case TileType.ground_grass:
        return new Vector2I(0, 0);
      case TileType.ground_stone:
        return new Vector2I(1, 0);
      case TileType.water:
        return new Vector2I(1, 1);
      case TileType.water_spring:
        return new Vector2I(0, 2);
      case TileType.hole:
        return new Vector2I(0, 1);
      case TileType.dirtPile:
        return new Vector2I(1, 0);
      case TileType.rock:
        return new Vector2I(2, 1);
      case TileType.flower:
        return new Vector2I(-1, -1);
      default:
        return new Vector2I(-1, -1);
    }
  }

  public enum TileType
  {
    none,
    ground_grass,
    ground_stone,
    water,
    water_spring,
    hole,
    dirtPile,
    rock,
    flower,
    startTile
  }

  public enum TileMapLayerType
  {
    ground,
    water,
    obstacle
  }

}

