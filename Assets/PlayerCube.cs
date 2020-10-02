using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Drawing;

public class PlayerCube : MonoBehaviour
{
    // Start is called before the first frame update
    public NetworkMan.Player playerRef;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(playerRef.color.R * 5, playerRef.color.G * 5, playerRef.color.B * 5);
        GetComponent<Renderer>().material.color = new UnityEngine.Color(playerRef.color.R, playerRef.color.G, playerRef.color.B);
    }
}
