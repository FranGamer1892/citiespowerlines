using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDeleter : MonoBehaviour
{
    public float timeAlive;
    public bool isReference = false;
    // Update is called once per frame
    void Update()
    {
        if (!isReference) {
            timeAlive -= Time.deltaTime;
            if (timeAlive < 0)
            {
                Destroy(this.gameObject);
            }
        }
        
    }
}
