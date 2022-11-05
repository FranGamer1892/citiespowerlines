using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Tutorial : MonoBehaviour
{
    public string[] Indications;
    public GameObject[] buttons;
    public TMP_Text[] t;
    int step = -1;
    float timer = 0;
    bool waiting = false;
    bool thingDone = false;
    bool sceneTriggered = false;

    BuildingPlacement bp;
    PowerlineBuilding pb;
    SceneManage sceneManage;
    // Start is called before the first frame update
    void Start()
    {
        pb = GetComponent<PowerlineBuilding>();
        bp = GetComponent<BuildingPlacement>();
        sceneManage = GetComponent<SceneManage>();
        UpdateText();
        timer = 4;
    }

    // Update is called once per frame
    void Update()
    {
        /*if(bp.currentTile == null)
        {
            t[0].transform.position = new Vector3(t[0].transform.position.x , 35, 0);
            t[1].transform.position = new Vector3(t[1].transform.position.x, 32, 0);
        }
        else
        {
            t[0].transform.position = new Vector3(t[0].transform.position.x, 168, 0);
            t[1].transform.position = new Vector3(t[1].transform.position.x, 165, 0);
        }
        */
        if(timer > 0)
        {
            waiting = true;
        }
        if (waiting)
        {
            timer -= Time.deltaTime;
            if(timer< 0)
            {
                waiting = false;
                UpdateText();
            }
        }
        if(step >= 2)
        {
            buttons[0].SetActive(true);
        }
        if(step >= 3)
        {
            buttons[2].SetActive(true);
            buttons[1].SetActive(true);
        }
        if (!thingDone)
        {
            if (step == 1)
            {
                if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 || Input.mouseScrollDelta != Vector2.zero)
                {
                    timer = 1;
                    thingDone = true;
                }
            }
            else if (step == 2)
            {
                if(bp.plants.Count != 0)
                {
                    timer = 1;
                    thingDone = true;
                }
            }
            else if(step == 3)
            {
                if(pb.electrifiedZones.Count != 0)
                {
                    timer = 1;
                    thingDone = true;
                }
            }
            else if(step == 4)
            {
                thingDone = true;
                timer = 4;
            }
            else if (step == 5 && !sceneTriggered)
            {
                sceneManage.OnPlay();
                sceneTriggered = true;
            }
        }
        
    }
    void UpdateText()
    {
        step++;
        t[0].text = Indications[step];
        t[1].text = Indications[step];
        thingDone = false;
    }

}
