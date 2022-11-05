
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;
using System.Drawing;

public class BuildingPlacement : MonoBehaviour
{
    public Tilemap tm;
    public Tilemap tmRoad;
    public Tilemap tmConcrete;
    public Tilemap tmHouse;
    public List<Vector3Int> tmHouseOccupiedSpaces = new List<Vector3Int>();
    public Tilemap tmPreview;
    public Tile grassTile;
    public Tile dirtTile;
    public Tile sandTile;
    public RuleTile conTile;
    public RuleTile roadTile;
    public Tile[] towerTiles;

    public GameObject dropdown;
    public RuleTile coalTile;
    public RuleTile oilTile;
    public RuleTile nuclearTile;
    public RuleTile solarTile;
    public RuleTile towerTile;
    public RuleTile currentTile = null;

    public GameObject interactGO;
    public GameObject escapeNoticeA;
    public GameObject escapeNoticeB;
    public GameObject moneyNotice;

    PowerlineBuilding pb;

    public AudioClip placed;
    public AudioClip error;
    public AudioClip bulldozed;

    public List<List<Vector3Int>> plantList = new List<List<Vector3Int>>();

    bool isDeleting = false;

    public List<GameObject> plants = new();

    int zValue = 0;
    public GameObject[] info;
    
    public int priceDivision = 2;
    public int coalPrice = 19000;
    public int oilPrice = 65000;
    public int nuclearPrice = 200000;
    public int solarPrice = 80000;

    float t;

    Notifications notif;
    Economy eco;
    EnergyConsumption energyConsumption;
    bool added = false;
    void Start()
    {
        notif = gameObject.GetComponent<Notifications>();
        eco = gameObject.GetComponent<Economy>();
        pb = gameObject.GetComponent<PowerlineBuilding>();
        energyConsumption = gameObject.GetComponent<EnergyConsumption>();

    }
    void Update()
    {
        t -= Time.deltaTime;
        if (t < 0 && dropdown.activeInHierarchy && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
            dropdown.SetActive(false);
        if (currentTile != null && currentTile != towerTile)
        {
            bool hasEnoughMoney = false;
            if (currentTile == coalTile && eco.balance >= coalPrice)
                hasEnoughMoney = true;
            if (currentTile == oilTile && eco.balance >= oilPrice)
                hasEnoughMoney = true;
            if (currentTile == nuclearTile && eco.balance >= nuclearPrice)
                hasEnoughMoney = true;
            if (currentTile == solarTile && eco.balance >= solarPrice)
                hasEnoughMoney = true;
            if (hasEnoughMoney) {
                moneyNotice.SetActive(false);
                PreviewStructure();
            if (Input.GetMouseButtonDown(0))
            {
                TryToBuild();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                tmPreview.ClearAllTiles();
                interactGO.SetActive(true);
                escapeNoticeA.SetActive(false);
                currentTile = null;
                dropdown.SetActive(false);
                HideInfo();
            }
            }
            else
            {
                moneyNotice.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    tmPreview.ClearAllTiles();
                    interactGO.SetActive(true);
                    escapeNoticeA.SetActive(false);
                    currentTile = null;
                    dropdown.SetActive(false);
                    HideInfo();
                }
            }
        }
        if (currentTile == towerTile)
        {
            if(eco.balance >= 100) {
                moneyNotice.SetActive(false);
                pb.selected = true;
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    pb.selected = false;
    
                    tmPreview.ClearAllTiles();
                    interactGO.SetActive(true);
                    escapeNoticeA.SetActive(false);
                    currentTile = null;
                    dropdown.SetActive(false);
                    HideInfo();
                }
            }
            else
            {
                pb.selected = false;
                moneyNotice.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    tmPreview.ClearAllTiles();
                    interactGO.SetActive(true);
                    escapeNoticeA.SetActive(false);
                    currentTile = null;
                    dropdown.SetActive(false);
                    HideInfo();
                }
            }
        }
        else if (currentTile == null)
        {
            moneyNotice.SetActive(false);
        }
            
        if (isDeleting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryToDelete();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                interactGO.SetActive(true);
                escapeNoticeB.SetActive(false);
                isDeleting = false;
            }
        }
        CheckRoadAccess();
        UpdatePlantText();
    }
    void TryToBuild()
    {
        int tilesBuilt = 0;
        List<Vector3Int> tileList = new List<Vector3Int>();
        Vector3Int mousePos = tmHouse.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        for (int i = 0; i < currentTile.m_TilingRules.Count; i++)
        {
            Vector3Int iPos = Vector3Int.zero;
            switch (i)
            {
                case 0:
                    iPos = new Vector3Int(0, 0, 0);
                    break;
                case 1:
                    iPos = new Vector3Int(0, 1, 0);
                    break;
                case 2:
                    iPos = new Vector3Int(1, 0, 0);
                    break;
                case 3:
                    iPos = new Vector3Int(1, 1, 0);
                    break;
                case 4:
                    iPos = new Vector3Int(2, 0, 0);
                    break;
                case 5:
                    iPos = new Vector3Int(2, 1, 0);
                    break;
                case 6:
                    iPos = new Vector3Int(3, 0, 0);
                    break;
                case 7:
                    iPos = new Vector3Int(3, 1, 0);
                    break;
                default:
                    break;
            }
            Vector3Int tilePos = mousePos + iPos;
            if (tmRoad.GetTile(tilePos) != roadTile && (tm.GetTile(tilePos) == grassTile || tm.GetTile(tilePos) == dirtTile) && tmConcrete.GetTile(tilePos) != conTile && !tmHouseOccupiedSpaces.Contains(new Vector3Int(tilePos.x, tilePos.y, 0)) && tmHouse.GetTile(tilePos) != towerTiles[0] && tmHouse.GetTile(tilePos) != towerTiles[1] && tmHouse.GetTile(tilePos) != towerTiles[2] && tmHouse.GetTile(tilePos) != towerTiles[3])
            {
                tileList.Add(tilePos);
            }
        }
        if (currentTile.m_TilingRules.Count == tileList.Count)
        {
            List<Vector3Int> positions = new List<Vector3Int>();

            foreach (Vector3Int x in tileList)
            {
                tmHouse.SetTile(new Vector3Int(x.x, x.y, zValue), currentTile);
                tmHouseOccupiedSpaces.Add(x);
                positions.Add(new Vector3Int(x.x, x.y, 0));
                tilesBuilt++;

            }
            zValue++;
            if (!gameObject.GetComponent<AudioSource>().isPlaying)
            {
                gameObject.GetComponent<AudioSource>().clip = placed;
                gameObject.GetComponent<AudioSource>().Play();
            }
            plantList.Add(positions);
            string plantType = "";
            if (currentTile == coalTile)
            {
                eco.balance -= coalPrice;
                plantType = "de Carbón";
            }
                
            if (currentTile == oilTile)
            {
                eco.balance -= oilPrice;
                plantType = "Geotérmica";
            }
                
            if (currentTile == nuclearTile)
            {
                eco.balance -= nuclearPrice;
                plantType = "Nuclear";
            }
             
            if (currentTile == solarTile)
            {
                eco.balance -= solarPrice;
                plantType = "Solar";
            }
                
            GameObject plant = new();
            plant.layer = 10;
            plant.name = "Planta"+ plantType + plants.Count.ToString();
            Instantiate(plant,mousePos, Quaternion.Euler(Vector3.zero));
            BoxCollider2D boxC = plant.AddComponent<BoxCollider2D>();

            plant.transform.position = mousePos;
            if (tilesBuilt == 8)
            {
                plant.transform.position += new Vector3(2, 1, -plant.transform.position.z - 1.4f);

                boxC.size = new Vector2(4, 2);
            }
            else
            {
                plant.transform.position += new Vector3(1, 1, -plant.transform.position.z - 1.2f);
                boxC.size = new Vector2(2, 2);
            }
            TextMeshPro plantText = plant.AddComponent<TextMeshPro>();
            plantText.alignment = TextAlignmentOptions.CenterGeoAligned;

            plantText.fontSize = 4;
            plantText.text = "0/" + ((-plant.transform.position.z -1) * 10).ToString();
            plantText.sortingOrder = 2500;
            plantText.enabled = false;
            plants.Add(plant);
            AddCount(currentTile);
        }
        else if (!gameObject.GetComponent<AudioSource>().isPlaying)
        {
            gameObject.GetComponent<AudioSource>().clip = error;
            gameObject.GetComponent<AudioSource>().Play();
        }
        tmPreview.ClearAllTiles();
        interactGO.SetActive(true);
        escapeNoticeA.SetActive(false);
        currentTile = null;
        dropdown.SetActive(false);
        HideInfo();
    }
    void TryToDelete()
    {
        Vector3Int mousePos = tmHouse.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Vector3Int plantPos = new Vector3Int(0, 0, 100);
        bool hasFound = false;
        List<Vector3Int> itemToRemove = new();
        foreach (List<Vector3Int> x in plantList)
        {
            foreach (Vector3Int y in x)
            {
                if (mousePos.x == y.x && mousePos.y == y.y)
                {
                    plantPos = StructureOrigin(x);
                    itemToRemove = x;
                    hasFound = true;
                    break;
                }
            }
        }
        if (hasFound)
        {
            Destroy(plants[plantList.IndexOf(itemToRemove)]);
            plants.Remove(plants[plantList.IndexOf(itemToRemove)]);
            plantList.Remove(itemToRemove);

            RuleTile plantType = coalTile;
            for (int i = 0; i < zValue; i++)
            {
                if(tmHouse.GetTile(new Vector3Int(plantPos.x,plantPos.y,i)) != null)
                {
                    plantType = (RuleTile)tmHouse.GetTile(new Vector3Int(plantPos.x, plantPos.y, i));
                }
            }
                if (plantType == coalTile)
                    eco.balance += coalPrice / priceDivision;
                if (plantType == oilTile)
                    eco.balance += oilPrice / priceDivision;
                if (plantType == nuclearTile)
                    eco.balance += nuclearPrice / priceDivision;
                if (plantType == solarTile)
                    eco.balance += solarPrice / priceDivision;
                DelCount(plantType);
                // Determine size
                int size;
                
                if (plantType == coalTile || plantType == solarTile) { size = 4; }
                else { size = 8; }
                List<Vector3Int> tileList = new List<Vector3Int>();
                for (int i = 0; i < size; i++)
                {
                    Vector3Int iPos = Vector3Int.zero;
                    switch (i)
                    {
                        case 0:
                            iPos = new Vector3Int(0, 0, 0);
                            break;
                        case 1:
                            iPos = new Vector3Int(0, 1, 0);
                            break;
                        case 2:
                            iPos = new Vector3Int(1, 0, 0);
                            break;
                        case 3:
                            iPos = new Vector3Int(1, 1, 0);
                            break;
                        case 4:
                            iPos = new Vector3Int(2, 0, 0);
                            break;
                        case 5:
                            iPos = new Vector3Int(2, 1, 0);
                            break;
                        case 6:
                            iPos = new Vector3Int(3, 0, 0);
                            break;
                        case 7:
                            iPos = new Vector3Int(3, 1, 0);
                            break;
                        default:
                            break;
                    }
                    Vector3Int tilePos = plantPos + iPos;
                    tileList.Add(tilePos);
                }
                Vector3Int[] v = pb.sidePositions;
                foreach (Vector3Int x in tileList)
                {
                    for (int i = 0; i < v.Length; i++)
                    {
                        Vector3Int vec = x + v[i];
                        if (pb.towers.Contains(vec))
                        {
                            Debug.Log(vec);
                            //tmHouse.SetTile(pos, towerTiles[0]);
                            pb.TryToDelete(vec, true);
                        }
                    }
                    for (int i = 0; i < zValue; i++)
                    {
                        tmHouse.SetTile(new Vector3Int(x.x, x.y, i), null);
                    }
                    tmHouseOccupiedSpaces.Remove(new Vector3Int(x.x, x.y, 0));
                }
                if (!gameObject.GetComponent<AudioSource>().isPlaying)
                {
                    gameObject.GetComponent<AudioSource>().clip = bulldozed;
                    gameObject.GetComponent<AudioSource>().Play();
                }

                interactGO.SetActive(true);
                escapeNoticeB.SetActive(false);
                isDeleting = false;

            
        }
        else
        {
            pb.TryToDelete(mousePos, false);
        }

    }
    void PreviewStructure()
    {
        tmPreview.ClearAllTiles();
        List<Vector3Int> tileList = new List<Vector3Int>();
        Vector3Int mousePos = tmHouse.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        for (int i = 0; i < currentTile.m_TilingRules.Count; i++)
        {
            Vector3Int iPos = Vector3Int.zero;
            switch (i)
            {
                case 0:
                    iPos = new Vector3Int(0, 0, 0);
                    break;
                case 1:
                    iPos = new Vector3Int(0, 1, 0);
                    break;
                case 2:
                    iPos = new Vector3Int(1, 0, 0);
                    break;
                case 3:
                    iPos = new Vector3Int(1, 1, 0);
                    break;
                case 4:
                    iPos = new Vector3Int(2, 0, 0);
                    break;
                case 5:
                    iPos = new Vector3Int(2, 1, 0);
                    break;
                case 6:
                    iPos = new Vector3Int(3, 0, 0);
                    break;
                case 7:
                    iPos = new Vector3Int(3, 1, 0);
                    break;
                default:
                    break;
            }
            Vector3Int tilePos = mousePos + iPos;
            if (tmRoad.GetTile(tilePos) != roadTile && (tm.GetTile(tilePos) == grassTile || tm.GetTile(tilePos) == dirtTile) && tmConcrete.GetTile(tilePos) != conTile && !tmHouseOccupiedSpaces.Contains(new Vector3Int(tilePos.x, tilePos.y, 0)) && tmHouse.GetTile(tilePos) != towerTiles[0] && tmHouse.GetTile(tilePos) != towerTiles[1] && tmHouse.GetTile(tilePos) != towerTiles[2] && tmHouse.GetTile(tilePos) != towerTiles[3])
            {
                tileList.Add(tilePos);
            }
        }
        if (currentTile.m_TilingRules.Count == tileList.Count)
        {
            foreach (Vector3Int x in tileList)
            {
                tmPreview.SetTile(new Vector3Int(x.x, x.y, zValue), currentTile);
            }
        }
    }

    void UpdatePlantText()
    {
        foreach (GameObject plant in plants)
        {
            TextMeshPro plantText = plant.GetComponent<TextMeshPro>();
            string prevText = plantText.text;
            int n = 0;
            int.TryParse(("0" + prevText[2]), out n);

            plantText.text = Mathf.Round(Mathf.Abs(((n/10 + (plant.transform.position.z + 1)) * 10))).ToString() + "/" + n.ToString();
        }
        
    }
    void AddCount(RuleTile currentTile)
    {
        if (currentTile == coalTile)
            energyConsumption.coalTileCount++;
        if (currentTile == oilTile)
            energyConsumption.oilTileCount++;
        if (currentTile == nuclearTile)
            energyConsumption.nuclearTileCount++;
        if (currentTile == solarTile)
            energyConsumption.solarTileCount++;
    }
    void DelCount(RuleTile currentTile)
    {
        if (currentTile == coalTile)
            energyConsumption.coalTileCount--;
        if (currentTile == oilTile)
            energyConsumption.oilTileCount--;
        if (currentTile == nuclearTile)
            energyConsumption.nuclearTileCount--;
        if (currentTile == solarTile)
            energyConsumption.solarTileCount--;
    }
    public void OnSelect()
    {
        if (!dropdown.activeInHierarchy)
        {
            dropdown.SetActive(true);
            t = .1f;
        }
        else
        {
            dropdown.SetActive(false);
        }
        
    }
    public void OnChange(int value)
    {

        switch (value)
        {
            case 1:
                currentTile = coalTile;
                info[0].SetActive(true);
                break;
            case 2:
                currentTile = oilTile;
                info[1].SetActive(true);
                break;
            case 3:
                currentTile = nuclearTile;
                info[2].SetActive(true);
                break;
            case 4:
                currentTile = solarTile;
                info[3].SetActive(true);
                break;
            case 5:
                info[4].SetActive(true);
                currentTile = towerTile;
                break;
            default:
                currentTile = null;
                dropdown.SetActive(false);
                HideInfo();
                break;
        }
        if (currentTile != null)
        {
            interactGO.SetActive(false);
            escapeNoticeA.SetActive(true);
        }
    }
    public void Diselect()
    {
        pb.selected = false;

        tmPreview.ClearAllTiles();
        interactGO.SetActive(true);
        escapeNoticeA.SetActive(false);
        currentTile = null;
        dropdown.SetActive(false);
        HideInfo();

    }
    public void OnClick()
    {
        isDeleting = true;
        interactGO.SetActive(false);
        escapeNoticeB.SetActive(true);
    }
    void HideInfo()
    {
        foreach (GameObject x in info)
        {
            x.SetActive(false);
        }
    }
    Vector3Int StructureOrigin(List<Vector3Int> u)
    {
        int lowerX = 10000;
        int lowerY = 10000;
        foreach (Vector3Int x in u)
        {
            if (x.x < lowerX) { lowerX = x.x; }
            if (x.y < lowerY) { lowerY = x.y; }
        }
        Vector3Int origin = new Vector3Int(lowerX, lowerY, u[0].z);
        return origin;
    }
    bool RoadAccess(Vector3Int x)
    {        
        if (tmRoad.GetTile(x + new Vector3Int(0, 1, 0)) == roadTile ||
            tmRoad.GetTile(x + new Vector3Int(0, -1, 0)) == roadTile ||
            tmRoad.GetTile(x + new Vector3Int(1, 0, 0)) == roadTile ||
            tmRoad.GetTile(x + new Vector3Int(-1, 0, 0)) == roadTile)
            return true;
        else return false;
    }
    void CheckRoadAccess()
    {
        List<List<Vector3Int>> raListList = new List<List<Vector3Int>>();
        foreach (List<Vector3Int> x in plantList)
        {
            List<Vector3Int> raList = new List<Vector3Int>();
            foreach (Vector3Int y in x)
            {
                if (RoadAccess(new Vector3Int(y.x,y.y,0)))
                    raList.Add(y);
            }
            if (raList.Count != 0)
                raListList.Add(raList);

        }
        if (raListList.Count != plantList.Count && added == false)
        {
            notif.AddNotif("Alguna de tus centrales no tiene conexión terrestre.");
            added = true;
        }
        else if (raListList.Count == plantList.Count)
        {
            notif.DelNotif("Alguna de tus centrales no tiene conexión terrestre.");
            added = false;
        }
    }
}