using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnergyConsumption : MonoBehaviour
{
    public int conTiles = 0;
    public int houseMW = 0;

    public int coalTileCount = 0;
    public int oilTileCount = 0;
    public int nuclearTileCount = 0;
    public int solarTileCount = 0;

    public int coalTileMW = 0;
    public int oilTileMW = 0;
    public int nuclearTileMW = 0;
    public int solarTileMW = 0;

    public int totalMW = 0;
    public int netMW = 0;

    public Economy ec;
    public void CheckConcrete(int worldWidth, Tilemap tmConcrete, RuleTile conTile)
    {
        conTiles = 0;
        for (int i = -worldWidth / 2; i < worldWidth / 2; i++)
        {
            for (int u = -worldWidth / 2; u < worldWidth / 2; u++)
            {
                Vector3Int v = new Vector3Int(i, u, 0);
                if (tmConcrete.GetTile(v) == conTile)
                {
                conTiles++;
                }
            }
        }
        houseMW = Mathf.RoundToInt(conTiles * 0.8f / 2);
        ec.SetBalance(houseMW);
    }
    void Update()
    {
        coalTileMW = coalTileCount * 140;
        oilTileMW = oilTileCount * 140;
        nuclearTileMW = nuclearTileCount * 360;
        solarTileMW = solarTileCount * 120;

        totalMW = coalTileMW + oilTileMW + nuclearTileMW + solarTileMW;
        netMW = houseMW - totalMW;
    }
}
