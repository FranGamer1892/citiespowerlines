using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class WorldGeneration : MonoBehaviour
{
    public Tilemap tm;
    public Tilemap tmRoad;
    public Tilemap tmConcrete;
    public Tilemap tmHouse;
    public Tilemap tmDecoration;
    public Tile grassTile;
    public Tile woreTile;
    public Tile sandTile;
    public Tile dirtTile;
    public RuleTile conTile;
    public RuleTile[] houseTile;
    public RuleTile roadTile;

    List<Vector3Int> possibleHousePos;
    List<Vector3Int> allRoads;

    [Range(0.0f, 0.1f)]
    public float worldSize = 1;
    [Range(10f, 150f)]
    public int worldWidth = 40;
    [Range(0f, 1f)]
    public float margin = .5f;
    [Range(0f, 1f)]
    public float sandMargin = .1f;
    [Range(0f, 1f)]
    public float dirtMargin = .1f;
    [Range(0, 10)]
    public int reset = 0;
    [Range(0, 100)]
    public int setRoadNum = 15;
    [Range(0, 100)]
    public int skipRoadProbability = 10;
    [Range(0, 100)]
    public int endRoadProbability = 10;
    [Range(0, 100)]
    public int concreteBlocks = 10;
    [Range(0,1)]
    public float houseProbability = 0.5f;
    public int divisibleBy = 4;
    int roadNum = 15;

    int offset;
    EnergyConsumption ec;
    // Start is called before the first frame update
    void Start()
    {
        ec = gameObject.GetComponent<EnergyConsumption>();
        Reset();
        GenerateRoadMap();
        Generate();
        GenerateConcrete();
        GenerateHouses();

    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.Backspace))
        {
            Reset();
            GenerateRoadMap();
            Generate();
            GenerateConcrete();
            GenerateHouses();
            reset = 0;
        }
        */
    }
    private void Reset()
    {
        for (int f = -worldWidth / 2; f < worldWidth / 2; f++)
        {
            for (int i = -worldWidth / 2; i < worldWidth / 2; i++)
            {
                tm.SetTile(new Vector3Int(i, f, 0), woreTile);
            }
        }
        offset = Random.Range(500, 5000);
        tmRoad.ClearAllTiles();
        tmConcrete.ClearAllTiles();
       
        roadNum = setRoadNum;
    }
    void GenerateRoadMap()
    {
        allRoads = new List<Vector3Int>();
        possibleHousePos = new List<Vector3Int>();
        List<Vector3Int> roadPositions = new List<Vector3Int>();
        int va = (worldWidth / roadNum);
        int roadsPlaced = 0;
        while (roadsPlaced < roadNum)
        {
            int a = Random.Range(1, 101);
            if (a <= skipRoadProbability) { roadsPlaced++; }
            Vector3Int pos = new Vector3Int(-worldWidth/2 + va * roadsPlaced, worldWidth/2 + 2, 0);

            while (pos.y > -worldWidth/2 - 4)
            {
                tmRoad.SetTile(pos, roadTile);
                allRoads.Add(pos);
                pos.y--;                 
            }
            roadsPlaced++;

        }
        int roadssPlaced = 0;
        while (roadssPlaced < roadNum)
        {
            int a = Random.Range(1, 101);
            if (a <= skipRoadProbability) { roadssPlaced++; }

            Vector3Int pos = new Vector3Int(-worldWidth/2 - 2, -worldWidth / 2 + va * roadssPlaced, 0);

            while (pos.x < worldWidth / 2 + 2)
            {
                if(tmRoad.GetTile(pos) == roadTile) 
                {
                    roadPositions.Add(pos + new Vector3Int(1, 0, 0));
                    roadPositions.Add(pos + new Vector3Int(0, -1, 1));
                }
                else
                {
                    tmRoad.SetTile(pos, roadTile);
                    allRoads.Add(pos);
                }
                
                pos.x++;

            }
            roadssPlaced++;
        }
        foreach (Vector3Int po in roadPositions)
        {
            int a = Random.Range(1, 101);
            if (a <= endRoadProbability) {  
                if(po.z == 0)
                {
                    Vector3Int position = po;
                    bool running = true;
                    while(running)
                    {
                        tmRoad.SetTile(position, null);
                        allRoads.Remove(position);
                        position.x++;
                        if (tmRoad.GetTile(position + new Vector3Int(0, 1, 0)) == roadTile || tmRoad.GetTile(position + new Vector3Int(0, -1, 0)) == roadTile || tmRoad.GetTile(position)!= roadTile)
                        {
                            running = false;
                        }
                    }
                    
                }
                else
                {
                    Vector3Int position = po;
                    position.z = 0;
                    bool running = true;
                    while (running)
                    {
                        tmRoad.SetTile(position, null);
                        allRoads.Remove(position);
                        position.y--;
                        if (tmRoad.GetTile(position + new Vector3Int(1, 0, 0)) == roadTile || tmRoad.GetTile(position + new Vector3Int(-1, 0, 0)) == roadTile || tmRoad.GetTile(position) != roadTile)
                        {
                            running = false;
                        }
                    }

                }                        
            }
        }
    }
    void GenerateConcrete()
    {
        int blocksPlaced = 0;
        bool running = true;
        while (running)
        {
            if(blocksPlaced < concreteBlocks)
            {
                Vector3Int ranPos = new Vector3Int(Random.Range(-worldWidth / 2, worldWidth / 2), Random.Range(-worldWidth / 2, worldWidth / 2), 0);
                if(tmRoad.GetTile(ranPos) != roadTile && (tm.GetTile(ranPos) == grassTile || tm.GetTile(ranPos) == dirtTile) && tmConcrete.GetTile(ranPos)!= conTile)
                {
                    tmConcrete.SetTile(ranPos, conTile);
                    List<Vector3Int> positions = new List<Vector3Int>();
                    positions.Add(ranPos);
                    bool run2 = true;
                    while (run2)
                    {
                        int zCount = 0;
                        for (int i = 0; i < positions.Count; i++)
                        {                            
                            if (positions[i].z == 0)
                            {
                                Vector3Int vec = new Vector3Int(1, 0, 0);
                                if (tmRoad.GetTile(positions[i] + vec) != roadTile && (tm.GetTile(positions[i] + vec) == grassTile || tm.GetTile(positions[i] + vec) == dirtTile) && tmConcrete.GetTile(positions[i] + vec) != conTile)
                                {
                                    positions.Add(positions[i] + vec);
                                    tmConcrete.SetTile(positions[i] + vec, conTile);
                                }
                                else if(tmRoad.GetTile(positions[i] + vec) == roadTile) 
                                { 
                                    tmConcrete.SetTile(positions[i] + vec, conTile);
                                    if(tmRoad.GetTile(positions[i] + vec + new Vector3Int(1,0,0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(1, 0, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(-1, 0, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(-1, 0, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(0, 1, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(0, 1, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(0, -1, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(0, -1, 0), conTile); }
                                }
                                vec = new Vector3Int(-1, 0, 0);
                                if (tmRoad.GetTile(positions[i] + vec) != roadTile && (tm.GetTile(positions[i] + vec) == grassTile || tm.GetTile(positions[i] + vec) == dirtTile) && tmConcrete.GetTile(positions[i] + vec) != conTile)
                                {
                                    positions.Add(positions[i] + vec);
                                    tmConcrete.SetTile(positions[i] + vec, conTile);
                                }
                                else if (tmRoad.GetTile(positions[i] + vec) == roadTile)
                                {
                                    tmConcrete.SetTile(positions[i] + vec, conTile);
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(1, 0, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(1, 0, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(-1, 0, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(-1, 0, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(0, 1, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(0, 1, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(0, -1, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(0, -1, 0), conTile); }
                                }
                                vec = new Vector3Int(0, 1, 0);
                                if (tmRoad.GetTile(positions[i] + vec) != roadTile && (tm.GetTile(positions[i] + vec) == grassTile || tm.GetTile(positions[i] + vec) == dirtTile) && tmConcrete.GetTile(positions[i] + vec) != conTile)
                                {
                                    positions.Add(positions[i] + vec);
                                    tmConcrete.SetTile(positions[i] + vec, conTile);
                                }
                                else if (tmRoad.GetTile(positions[i] + vec) == roadTile)
                                {
                                    tmConcrete.SetTile(positions[i] + vec, conTile);
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(1, 0, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(1, 0, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(-1, 0, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(-1, 0, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(0, 1, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(0, 1, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(0, -1, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(0, -1, 0), conTile); }
                                }
                                vec = new Vector3Int(0, -1, 0);
                                if (tmRoad.GetTile(positions[i] + vec) != roadTile && (tm.GetTile(positions[i] + vec) == grassTile || tm.GetTile(positions[i] + vec) == dirtTile) && tmConcrete.GetTile(positions[i] + vec) != conTile)
                                {
                                    positions.Add(positions[i] + vec);
                                    tmConcrete.SetTile(positions[i] + vec, conTile);
                                }
                                else if (tmRoad.GetTile(positions[i] + vec) == roadTile)
                                {
                                    tmConcrete.SetTile(positions[i] + vec, conTile);
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(1, 0, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(1, 0, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(-1, 0, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(-1, 0, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(0, 1, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(0, 1, 0), conTile); }
                                    if (tmRoad.GetTile(positions[i] + vec + new Vector3Int(0, -1, 0)) == roadTile) { tmConcrete.SetTile(positions[i] + vec + new Vector3Int(0, -1, 0), conTile); }
                                }
                                positions[i] = new Vector3Int(positions[i].x, positions[i].y, 1);
                            }
                            else
                            {
                                zCount++;
                            }
                            if(zCount >= positions.Count)
                            {
                                run2 = false;
                            }
                        }

                    }
                    blocksPlaced++;
                }
            }
            else
            {
                running = false;
            }
        }
        ec.CheckConcrete(worldWidth, tmConcrete, conTile);
    }
    void GenerateHouses()
    {
        for (int i = -worldWidth/2; i < worldWidth/2; i++)
        {
            for (int u = -worldWidth / 2; u < worldWidth / 2; u++)
            {
                Vector3Int v = new Vector3Int(i, u, 0);
                if (tmRoad.GetTile(v) == roadTile || tmConcrete.GetTile(v) != conTile)
                {
                    tmHouse.SetTile(v, null);
                    tmDecoration.SetTile(v, null);
                }
            }
        }
       
    }
    void Generate()
    {
        for (int f = -worldWidth / 2; f < worldWidth / 2; f++)
        {
            for (int i = -worldWidth / 2; i < worldWidth / 2; i++)
            {
                if (Mathf.PerlinNoise(offset + i * worldSize, offset + f * worldSize) > margin + dirtMargin)
                {
                    tm.SetTile(new Vector3Int(i, f, 0), dirtTile);
                }
                else if (Mathf.PerlinNoise(offset + i * worldSize, offset + f * worldSize) > margin)
                {
                    tm.SetTile(new Vector3Int(i, f, 0), grassTile);
                }
                else if (Mathf.PerlinNoise(offset + i * worldSize, offset + f * worldSize) > margin - sandMargin)
                {
                    tm.SetTile(new Vector3Int(i, f, 0), sandTile);
                }
                else
                {
                    tm.SetTile(new Vector3Int(i, f, 0), woreTile);
                }
            }
        }
        for (int i = -worldWidth/2; i < worldWidth/2; i++)
        {
            for (int u = -worldWidth/2; u < worldWidth/2; u++)
            {
                if(tm.GetTile(new Vector3Int(i,u,0)) != dirtTile && tm.GetTile(new Vector3Int(i, u, 0))!= grassTile)
                {
                    tmRoad.SetTile(new Vector3Int(i, u, 0), null);
                }
            }
        }

        
    }

}
