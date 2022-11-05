using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed;
    public float moveSmooth;
    public float zoomSpeed;
    public float zoomSmooth;
    public float maxZoomOut;

    BoxCollider2D bc;
    Rigidbody2D rb;
    Camera cam;
    float camSize = 7;

    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(7, 9);
        rb = GetComponent<Rigidbody2D>();
        cam = GetComponent<Camera>();
        bc = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
   
        Vector2 inputXY = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        rb.velocity = Vector2.Lerp(rb.velocity, inputXY * moveSpeed * camSize, Time.deltaTime * moveSmooth);
        
        camSize += -Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
        camSize = Mathf.Clamp(camSize, 5, maxZoomOut);
        bc.size = new Vector2(camSize * 19.2f / 5, camSize * 2);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, camSize, zoomSmooth * Time.deltaTime);
    }
}
