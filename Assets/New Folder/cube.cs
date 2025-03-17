using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour
{
    [SerializeField] Client client;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * 2f, 0, Input.GetAxis("Vertical") * Time.deltaTime *2f);  

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            client.UpdateServer(transform.position);
        }
        else
        {
            client.serverBeingUpdated = false;
        }
    }
}
