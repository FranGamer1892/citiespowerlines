using TMPro;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class PowerlineBuilding : MonoBehaviour
{
    public bool selected = false;
    bool wasSelected = false;

    public GameObject elecPar;
    public GameObject par2;

    public TMP_Text zoneText;
    public TMP_Text notice;

    bool electric = false;
    bool hasConnected = false;
    public Vector3Int[] sidePositions = { new Vector3Int(-1, 0),new Vector3Int(1, 0),  new Vector3Int(0, 1), new Vector3Int(0,-1) };
    List<List<Vector3Int>> electrifiedLines = new();
    public List<List<Vector3Int>> electrifiedZones = new();
    public Tilemap tmPreview;
    public Tilemap tmRoad;
    public Tilemap tmTerrain;
    public Tilemap tmConcrete;
    public Tilemap tmHouse;
    public Tilemap defTmHouse;

    public Tile woreTile;
    public RuleTile conTile;

    public Tile[] towerTile;
    public RuleTile[] plantTiles;
    public AudioClip zap;
    public AudioClip placed;
    public AudioClip error;
    public AudioClip bulldozed;

    public Material lineMat;
    public float maxTowerDistance;


    BuildingPlacement bp;
    int towersEverBuilt = 0;
    GameObject[] previewLines = new GameObject[2];
    GameObject[] previousLines = new GameObject[2];
    public List<Vector3Int> towers = new();
    public List<List<GameObject>> towerLines = new();
    Vector3Int selectedTower = new Vector3Int(-10000, 0, 100000);
    Vector3Int prePreviousTower = new Vector3Int(-10000, 0, 100000);

    Tile currentTile;
    Tile previousTile;

    public GameObject thisPlant;

    public float lastWeight;
    Economy eco;
    public int priceDivision = 2;
    public int price = 100;
    Color initColor;
    Notifications notif;

    // Start is called before the first frame update
    void Start()
    {
        bp = GetComponent<BuildingPlacement>();
        eco = GetComponent<Economy>();
        notif = GetComponent<Notifications>();
        initColor = zoneText.color;

        NotSelected();
    }

    // Update is called once per frame
    void Update()
    {
        notice.text = "";
        if (selected)
        {
            Vector3Int mousePos = tmHouse.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (selectedTower != new Vector3Int(-10000, 0, 100000) && Physics2D.Linecast(new Vector2(selectedTower.x + 0.5f,selectedTower.y + 0.5f), new Vector2(mousePos.x + 0.5f, mousePos.y + 0.5f), (1 << 7)))
            {
                notice.text = "No hay espacio para los cables.";
                if(previewLines[0] != null)
                {
                    Destroy(previewLines[0]);
                    Destroy(previewLines[1]);
                }
                tmPreview.ClearAllTiles();
            }
            else
            {
                bool encima = false;
                bool nextToPlant = false;
                for (int i = 0; i < sidePositions.Length; i++)
                {
                    
                    Vector3Int pos = mousePos + sidePositions[i];
                    if (bp.tmHouseOccupiedSpaces.Contains(pos))
                    {
                        List<Vector3Int> l = new();
                        foreach (List<Vector3Int> li in bp.plantList)
                        {
                            if (li.Contains(mousePos))
                            {
                                notice.text = "No puedes construir sobre una planta.";
                                encima = true;
                                break;
                            }
                            if (li.Contains(pos))
                            {
                                if (nextToPlant)
                                {
                                    notice.text = "Construye a la torre junto a una única planta.";
                                    break;

                                }
                                nextToPlant = true;
                                l = li;
                                break;
                            }
                        }
                        if (encima)
                            break;
                        if (notice.text == "Construye a la torre junto a una única planta.")
                            break;
                       
                       thisPlant = bp.plants[bp.plantList.IndexOf(l)];
                       thisPlant.GetComponent<TextMeshPro>().enabled = true;
                        
                        
                        if (thisPlant.GetComponent<RectTransform>().position.z >= -1.0f)
                        {
                            notice.text = "Esta planta ya tiene el máximo de líneas conectadas.";
                            break;
                        }                                           
                    }
                }
                if (!nextToPlant && thisPlant != null)
                {
                    thisPlant.GetComponent<TextMeshPro>().enabled = false;
                }
                if (!nextToPlant && !encima)
                    notice.text = "Coloca una torre junto a una planta para comenzar la línea.";
                if (selectedTower == new Vector3Int(-10000, 0, 100000)) {
                        
                    if (nextToPlant && notice.text == "")
                    {
                        PreviewElectricLine();
                        if (Input.GetMouseButtonDown(0))
                        {
                            BuildElectricLine();
                        }
                    }
                    else
                    {
                        tmPreview.ClearAllTiles();
                    }
                }
                else
                {
                    if (nextToPlant)
                    {
                        notice.text = "La línea ya está conectada a una planta.";
                        if (previewLines[0] != null)
                        {
                            Destroy(previewLines[0]);
                            Destroy(previewLines[1]);
                        }
                        tmPreview.ClearAllTiles();
                    }                        
                    else
                    {                    
                        PreviewElectricLine();
                        if (Input.GetMouseButtonDown(0))
                        {
                            BuildElectricLine();
                        }
                    }
                }
            }
        }
        if (selected != wasSelected && !selected)
            NotSelected();

        wasSelected = selected;
        zoneText.text = electrifiedZones.Count.ToString() + "/" + notif.zones.ToString();
        if (electrifiedZones.Count >= notif.zones)
            zoneText.color = Color.green;
        else zoneText.color = initColor;
    }
    void NotSelected()
    {
        int towersBulldozed = 0;
        if (previewLines[0] != null)
        {
            Destroy(previewLines[0]);
            Destroy(previewLines[1]);
        }
        //tmHouse.SetTile(selectedTower, previousTile);
        selectedTower = new Vector3Int(-10000, 0, 100000);
        prePreviousTower = new Vector3Int(-10000, 0, 100000);
        currentTile = towerTile[0];
        if(!hasConnected)
        {            
            List<Vector3Int> towersToDelete = new();
            if (electric)
            {
                if (thisPlant != null)
                {
                    thisPlant.transform.position += new Vector3(0, 0, -.1f);
                    thisPlant.GetComponent<TextMeshPro>().enabled = false;
                }                    
                electrifiedLines.Remove(electrifiedLines[electrifiedLines.Count - 1]);
            }
            foreach (Vector3Int tower in towers)
            {
                bool isElectric = false;
                foreach (List<Vector3Int> list in electrifiedLines)
                {
                    foreach (Vector3Int x in list)
                    {
                        if(x == tower)
                        {
                            isElectric = true;
                            break;
                        }
                    }
                    if (isElectric) { break; }
                }
                if (!isElectric)
                {
                    tmHouse.SetTile(tower, null);
                    towersToDelete.Add(tower);
                }
            }
            foreach (Vector3Int x in towersToDelete)
            {
                foreach (GameObject y in towerLines[towers.IndexOf(x)])
                {
                    Destroy(y);
                }
                towerLines.Remove(towerLines[towers.IndexOf(x)]);
                towers.Remove(x);
                towersBulldozed++;
            }
            eco.balance += price * towersBulldozed;
        }
        electric = false;
        hasConnected = false;
        notice.text = "";
    }
    void BuildElectricLine()
    {
        bool outOfThis = false;
        if (notice.text == "")
        {
            Vector3Int mousePos = tmHouse.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (tmRoad.GetTile(mousePos) == null && defTmHouse.GetTile(mousePos) == null && tmHouse.GetTile(mousePos) == null && !bp.tmHouseOccupiedSpaces.Contains(new Vector3Int(mousePos.x, mousePos.y, 0)))
            {
                towersEverBuilt++;
                tmHouse.SetTile(mousePos, currentTile);
                towers.Add(mousePos);
                eco.balance -= price;

                if (!electric)
                {
                    if (thisPlant.transform.position.z < -1.0f)
                    {
                        electric = true;
                        electrifiedLines.Add(new List<Vector3Int>());
                        electrifiedLines[electrifiedLines.Count - 1].Add(mousePos);
                        thisPlant.transform.position += new Vector3(0, 0, .1f);

                    }
                }
                else
                {
                    electrifiedLines[electrifiedLines.Count -1].Add(mousePos);
                    if(tmConcrete.GetTile(mousePos) == conTile)
                    {
                        ElectrifyConcrete(mousePos);
                        outOfThis = true;
                    }
                }

                if (selectedTower != new Vector3Int(-10000, 0, 100000))
                {
                    GameObject line1 = Instantiate(previewLines[0]);
                    GameObject line2 = Instantiate(previewLines[1]);
                    line1.GetComponent<LineRenderer>().startColor = Color.black;
                    line1.GetComponent<LineRenderer>().endColor = Color.black;
                    line2.GetComponent<LineRenderer>().startColor = Color.black;
                    line2.GetComponent<LineRenderer>().endColor = Color.black;

                    towerLines.Add(new List<GameObject>() { line1, line2 });
                   
                    towerLines[towerLines.Count - 2].Add(line1);
                    towerLines[towerLines.Count - 2].Add(line2);

                    previousLines[0] = line1;
                    previousLines[1] = line2;

                    if (!electric && !gameObject.GetComponent<AudioSource>().isPlaying)
                    {
                        gameObject.GetComponent<AudioSource>().clip = placed;
                        gameObject.GetComponent<AudioSource>().Play();
                    }
                }
                else {
                    towerLines.Add(new List<GameObject>() { null, null });
                    if (!electric && !gameObject.GetComponent<AudioSource>().isPlaying)
                    {
                        gameObject.GetComponent<AudioSource>().clip = placed;
                        gameObject.GetComponent<AudioSource>().Play();
                    }

                }               
                if (electric)
                {
                    Instantiate(elecPar, mousePos + new Vector3(.5f, .5f), Quaternion.Euler(Vector3.zero)).GetComponent<ParticleDeleter>().isReference = false;

                    if (!gameObject.GetComponent<AudioSource>().isPlaying)
                    {
                        gameObject.GetComponent<AudioSource>().clip = zap;
                        gameObject.GetComponent<AudioSource>().Play();
                    }

                }                

                prePreviousTower = selectedTower;
                selectedTower = mousePos;
                previousTile = (Tile)tmHouse.GetTile(mousePos);
                
            }
            else if (!gameObject.GetComponent<AudioSource>().isPlaying)
            {
                gameObject.GetComponent<AudioSource>().clip = error;
                gameObject.GetComponent<AudioSource>().Play();
            }
            
        }
        if (outOfThis)
        {
            bp.Diselect();
        }
    }
    void PreviewElectricLine()
    {
        
        tmPreview.ClearAllTiles();
        if (previewLines[0] != null)
        {
            Destroy(previewLines[0]);
            Destroy(previewLines[1]);
        }
        Vector3Int mousePos = tmHouse.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        bool electricConcrete = false;
        foreach (List<Vector3Int> list in electrifiedZones)
        {
            foreach (Vector3Int v in list)
            {
                if (v == mousePos)
                {
                    electricConcrete = true;
                    break;
                }
            }
            if (electricConcrete)
                break;
        }
        if (electricConcrete)
        {
            notice.text = "Esta zona ya tiene conexión eléctrica.";
            return;
        }
        else
        {
            notice.text = "";
        }
        if (selectedTower != new Vector3Int(-10000, 0, 100000) && Mathf.Sqrt(Mathf.Pow(mousePos.x - selectedTower.x, 2) + Mathf.Pow(mousePos.y - selectedTower.y, 2)) > maxTowerDistance)
        { notice.text = "La distancia es demasiado grande."; }
        else if(selectedTower == new Vector3Int(-10000, 0, 100000))
        {
            if (tmRoad.GetTile(mousePos) == null && defTmHouse.GetTile(mousePos) == null && tmHouse.GetTile(mousePos) == null && !bp.tmHouseOccupiedSpaces.Contains(new Vector3Int(mousePos.x, mousePos.y, 0)))
            {
                tmPreview.SetTile(mousePos, towerTile[0]);
            }
            notice.text = "";
        }
        
        else
        {
            notice.text = "";
            if (tmRoad.GetTile(mousePos) == null && defTmHouse.GetTile(mousePos) == null && tmHouse.GetTile(mousePos) == null && !bp.tmHouseOccupiedSpaces.Contains(new Vector3Int(mousePos.x, mousePos.y, 0)))
            {
                Vector2 direction = new Vector2(mousePos.x - selectedTower.x, mousePos.y - selectedTower.y).normalized;
                float angle = Vector2.Angle(Vector2.right, direction);
                if (direction.y < 0)
                    angle = 360 - angle;
                currentTile = towerTile[DirectionCase(angle)];
                tmPreview.SetTile(mousePos, towerTile[DirectionCase(angle)]);

                if (prePreviousTower != new Vector3Int(-10000, 0, 100000))
                {
                    Vector2 dir1 = new Vector2(prePreviousTower.x - selectedTower.x, prePreviousTower.y - selectedTower.y).normalized;
                    Vector2 overallDir = dir1 + direction * lastWeight;
                    float thisAngle = Vector2.Angle(Vector2.right, overallDir);
                    if (overallDir.y < 0)
                        thisAngle = 360 - thisAngle;
                    tmHouse.SetTile(selectedTower, towerTile[DirectionCase(thisAngle)]);
                    PreLineCreation(new int[] { DirectionCase(thisAngle), DirectionCase(angle) }, new Vector3Int[] { selectedTower, mousePos }, true);

                    previousLines[0].GetComponent<LineRenderer>().SetPosition(1, LinePos(selectedTower, 1, DirectionCase(thisAngle)));
                    previousLines[1].GetComponent<LineRenderer>().SetPosition(1, LinePos(selectedTower, 2, DirectionCase(thisAngle)));
                    EdgeCollider2D l1c = previousLines[0].AddComponent<EdgeCollider2D>();
                    List<Vector2> line1Points = new List<Vector2>() { previousLines[0].GetComponent<LineRenderer>().GetPosition(0), previousLines[0].GetComponent<LineRenderer>().GetPosition(1)};
                    l1c.SetPoints(line1Points);
                    if (Physics2D.Linecast(previousLines[1].GetComponent<LineRenderer>().GetPosition(0), previousLines[1].GetComponent<LineRenderer>().GetPosition(1), (1 << 6)))
                    {
                        Vector3 x = previousLines[0].GetComponent<LineRenderer>().GetPosition(1);
                        previousLines[0].GetComponent<LineRenderer>().SetPosition(1, previousLines[1].GetComponent<LineRenderer>().GetPosition(1));
                        previousLines[1].GetComponent<LineRenderer>().SetPosition(1, x);
                    }
                    Destroy(l1c);
                }
                else
                {
                    Vector2 dir1 = new Vector2(mousePos.x - selectedTower.x, mousePos.y - selectedTower.y).normalized;
                    float thisAngle = Vector2.Angle(Vector2.right, dir1);
                    if (dir1.y < 0)
                        thisAngle = 360 - thisAngle;
                    tmHouse.SetTile(selectedTower, towerTile[DirectionCase(thisAngle)]);
                    PreLineCreation(new int[] { DirectionCase(thisAngle), DirectionCase(angle) }, new Vector3Int[] { selectedTower, mousePos }, true);

                }
            }
            
        }
    }
    int DirectionCase(float angle)
    {
        int dir = 0;
        if (angle < 22.5f)
        {
            dir = 1;
        }
        else if (angle < 67.5)
        {
            dir = 3;
        }
        else if (angle < 112.5f)
        {
            dir = 0;
        }
        else if (angle < 157.5)
        {
            dir = 2;
        }
        else if (angle < 202.5f)
        {
            dir = 1;
        }
        else if (angle < 247.5f)
        {
            dir = 3;
        }
        else if (angle < 292.5f)
        {
            dir = 0;
        }
        else if (angle < 337.5f)
        {
            dir = 2;
        }
        else
        {
            dir = 1;
        }
        return dir;

    }
    void PreLineCreation(int[] dirCase, Vector3Int[] tilePositions, bool isPreview)
    {
        Vector2[] linePositions = new Vector2[4];
        linePositions[0] = LinePos(tilePositions[0], 1, dirCase[0]);
        linePositions[1] = LinePos(tilePositions[1], 1, dirCase[1]);
        linePositions[2] = LinePos(tilePositions[0], 2, dirCase[0]);
        linePositions[3] = LinePos(tilePositions[1], 2, dirCase[1]);
        CreateLines(linePositions, isPreview);
    }
    Vector3 LinePos(Vector3Int position, int lineNumber, int dirCase)
    {
        Vector3 pos = position;
        switch (dirCase)
        {
            case 0:
                if (lineNumber == 1)
                    pos = position + new Vector3(.35f, 0);
                else
                    pos = position + new Vector3(-.35f, 0);
                break;
            case 1:
                if (lineNumber == 1)
                    pos = position + new Vector3(0,.35f);
                else
                    pos = position + new Vector3(0, -.35f);
                break;
            case 2:
                if (lineNumber == 1)
                    pos = position + new Vector3(-.3f, -.3f);
                else
                    pos = position + new Vector3(.3f, .3f);
                break;
            case 3:
                if (lineNumber == 1)
                    pos = position + new Vector3(-.3f, .3f);
                else
                    pos = position + new Vector3(.3f, -.3f);
                break;
            default:
                break;
        }
        pos += new Vector3(.5f, .5f);
        return pos;
    }
    void CreateLines(Vector2[] positions, bool preview)
    {
        GameObject line1 = new GameObject("Line" + towersEverBuilt.ToString() + ".1");
        GameObject line2 = new GameObject("Line" + towersEverBuilt.ToString() + ".2");
        LineRenderer l1r = line1.AddComponent<LineRenderer>();
        LineRenderer l2r = line2.AddComponent<LineRenderer>();
        EdgeCollider2D l1c = line1.AddComponent<EdgeCollider2D>();
       // EdgeCollider2D l2c = line2.AddComponent<EdgeCollider2D>();

        l1r.material = lineMat;
        l2r.material = lineMat;
        l1r.sortingOrder = 15;
        l2r.sortingOrder = 15;
        l1r.startWidth = 0.05f;
        l2r.startWidth = 0.05f;
        Color startColor = new Color(0, 0, 0, 1);
        Color endColor = new Color(0, 0, 0, 1);
        if (preview)
        {
            endColor = new Color(.85f, .85f, .85f, .15f);
            startColor = new Color(.75f, .75f, .75f, .5f);
        }
        l1r.startColor = startColor;
        l2r.startColor = startColor;
        l1r.endColor = endColor;
        l2r.endColor = endColor;

        l1r.SetPosition(0, positions[0]);
        l2r.SetPosition(0, positions[2]);
        l1r.SetPosition(1, positions[1]);
        l2r.SetPosition(1, positions[3]);

        List<Vector2> line1Points = new List<Vector2>() {positions[0], positions[1]};
        //List<Vector2> line2Points = new List<Vector2>() { positions[2], positions[3] };
        l1c.SetPoints(line1Points);
        //l2c.SetPoints(line2Points);
        line1.layer = LayerMask.NameToLayer("Cable");
        line2.layer = LayerMask.NameToLayer("Cable");

        if(Physics2D.Linecast(positions[2],positions[3], (1 << 6)))
        {
            l1r.SetPosition(1, positions[3]);
            l2r.SetPosition(1, positions[1]);
        }
        Destroy(l1c);
        //Destroy(l2c);
        if (preview) { 
        previewLines[0] = line1;
        previewLines[1] = line2;
        }
    }
    void ElectrifyConcrete(Vector3Int startPos)
    {
        foreach (List<Vector3Int> list in electrifiedZones)
        {
            if (list.Contains(startPos))
            {
                return;
            }                                
        }
        hasConnected = true;
        electrifiedZones.Add(new List<Vector3Int>());
        List<Vector3Int> positions = new();
        Vector3Int[] next = { new Vector3Int(-1, 0), new Vector3Int(1, 0), new Vector3Int(0, -1), new Vector3Int(0, 1)};
        positions.Add(startPos);
        bool running = true;
        while (running){
            int zCount = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                if(positions[i].z == 0)
                {
                    for (int u = 0; u < next.Length; u++)
                    {
                        if (tmConcrete.GetTile(positions[i] + next[u]) == conTile && tmRoad.GetTile(positions[i] + next[u]) == null && !electrifiedZones[electrifiedZones.Count - 1].Contains(positions[i] + next[u]))
                        {
                            electrifiedZones[electrifiedZones.Count - 1].Add(positions[i] + next[u]);
                            positions.Add(positions[i] + next[u]);
                            Instantiate(elecPar, positions[i] + next[u] + new Vector3(.5f, .5f), Quaternion.Euler(Vector3.zero)).GetComponent<ParticleDeleter>().isReference = false;
                        }
                        
                    }
                    positions[i] = new Vector3Int(positions[i].x, positions[i].y,1);
                }
                else
                {
                    zCount++;
                }
            }
            if(zCount == positions.Count)
            {
                running = false;
            }
        }
    }
    void DiselectrifyZone(int index)
    {
        foreach (Vector3Int v in electrifiedZones[index])
        {
            Instantiate(par2, v + new Vector3(.5f, .5f), Quaternion.Euler(Vector3.zero)).GetComponent<ParticleDeleter>().isReference = false;
        }
        electrifiedZones.Remove(electrifiedZones[index]);
    }
    public void TryToDelete(Vector3Int position, bool plantGone)
    {
        int towersBulldozed = 0;
        int toDelete = 999999;
        
        foreach (List<Vector3Int> LIST in electrifiedLines)
        {
            foreach (Vector3Int x in LIST)
            {
                if (position == x)
                {                    
                    if (!gameObject.GetComponent<AudioSource>().isPlaying)
                    {
                        gameObject.GetComponent<AudioSource>().clip = bulldozed;
                        gameObject.GetComponent<AudioSource>().Play();
                    }                    
                    
                    toDelete = electrifiedLines.IndexOf(LIST);
                    foreach (Vector3Int y in LIST)
                    {
                        towersBulldozed++;
                        tmHouse.SetTile(y, null);
                        foreach (GameObject u in towerLines[towers.IndexOf(y)])
                        {
                            Destroy(u);
                        }
                        towerLines.Remove(towerLines[towers.IndexOf(y)]);
                        towers.Remove(y);
                        if(!plantGone) {
                            GameObject thePlant = null;
                            for (int i = 0; i < sidePositions.Length; i++)
                            {
                                if (bp.tmHouseOccupiedSpaces.Contains(y + sidePositions[i]))
                                {
                                    List<Vector3Int> l = new();
                                    foreach (List<Vector3Int> li in bp.plantList)
                                    {
                                        if (li.Contains(y + sidePositions[i]))
                                        {
                                            l = li;
                                            break;
                                        }
                                    }
                                    thePlant = bp.plants[bp.plantList.IndexOf(l)];
                                    break;
                                }
                            }
                            if (thePlant != null)
                                thePlant.transform.position += new Vector3(0, 0, -.1f);
                        }
                        
                    }
                    break;
                }
            }
            if(towersBulldozed != 0)
            {
                break;
            }
        }
        if (toDelete != 999999)
        {
            electrifiedLines.Remove(electrifiedLines[toDelete]);
            DiselectrifyZone(toDelete);
        }
        eco.balance += price * towersBulldozed / priceDivision;
    }
}
