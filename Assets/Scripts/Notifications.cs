using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Notifications : MonoBehaviour
{
    public Transform ContentContainer;
    public GameObject ItemPrefab;
    public List<GameObject> notifList = new List<GameObject>();
    [Range(0, 4)]
    public int test = 0;

    public GameObject[] fireworks;
    float deltaT;

    string str;
    public int zones;

    public TMP_Text zonesText;
    public TMP_Text powerText;
    public Animator animator;
    public Image buttonImg;
    public TMP_Text buttonText;
    void Start()
    {
        zones = Random.Range(7,8);
        str = "Objetivo: Satisfacer el consumo energético y conectar al menos " + zones + " zonas.";
    }
    void Update()
    {
        if (zonesText.color == Color.green && powerText.color == Color.green && ContentContainer.childCount == 2)
        {
            DelNotif(str);
            AddNotif(str, Color.green);
            deltaT += Time.deltaTime;
            Win();
            animator.enabled = true;
        }
        else
        {
            DelNotif(str);
            AddNotif(str, default(Color));
            animator.enabled = false;
            buttonImg.color = new Color(255, 255, 255, 255);
            buttonText.color = new Color(255, 255, 255, 255);
        }
        switch (test)
        {
            case 1:
                AddNotif("1");
                test = 0;
                break;
            case 2:
                AddNotif("2");
                test = 0;
                break;
            case 3:
                DelNotif("1");
                test = 0;
                break;
            case 4:
                DelNotif("2");
                test = 0;
                break;
            default:
                test = 0;
                break;
        }
    }

    public void AddNotif(string text, Color color = default(Color))
    {
        GameObject item_go = Instantiate(ItemPrefab);
        item_go.transform.SetParent(ContentContainer);
        item_go.transform.localScale = Vector2.one;
        item_go.SetActive(true);
        TMP_Text buttonText = item_go.GetComponentInChildren<TMP_Text>();
        buttonText.text = text;
        if(color != default(Color))
            buttonText.color = color;
        notifList.Add(item_go);
    }
    public void DelNotif(string text)
    {
        GameObject notifToDelete = null;
        foreach(GameObject x in notifList)
        {
            TMP_Text buttonText = x.GetComponentInChildren<TMP_Text>();
            if(buttonText.text == text)
            {
                notifToDelete = x;
                break;
            }
        }
        notifList.Remove(notifToDelete);
        GameObject.Destroy(notifToDelete);
    }
    void Win()
    {
        if (deltaT> .05f)
        {
            Transform t = Instantiate(fireworks[Random.Range(0, 4)], new Vector3(Random.Range(-60, 60), Random.Range(-60, 60), 0), Quaternion.Euler(Vector3.zero)).transform;
            t.localScale = t.localScale * Random.Range(1.2f, 3.5f);
            deltaT = 0;
        }
    }
}