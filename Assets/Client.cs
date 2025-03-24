using UnityEngine;
using TMPro;

//Lec04
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;


public class Client : MonoBehaviour
{
    public GameObject myCube;
    public GameObject otherCube;
    [SerializeField] GameObject[] cubes;
    public TextMeshProUGUI ErrorText;
    private static byte[] buffer = new byte[2048];
    private static byte[] outBuffer = new byte[2048];
    private static IPEndPoint remoteEP;
    private static EndPoint remoteServer;

    private static Socket client;

    public int ClientIndex = 0;

    public bool serverBeingUpdated = false;

    public static void StartClient()
    {
        try
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            remoteEP = new IPEndPoint(ip, 1111);
            remoteServer = (EndPoint)remoteEP;

            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.Blocking = false;
            client.ReceiveBufferSize = 2048;

        } catch (Exception e)
        {
            Debug.Log("Exception: " + e.ToString());
        }
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartClient();

        byte[] bufferNoUpdate = new byte[2048];
        client.SendTo(bufferNoUpdate, remoteEP);


        int recv = client.ReceiveFrom(outBuffer, ref remoteServer);
        Vector3 newPos = StringToVector3(Encoding.ASCII.GetString(outBuffer, 0, recv), out ClientIndex);

        myCube = cubes[ClientIndex - 1];
        switch (ClientIndex)
        {
            case 1:
                otherCube = cubes[1];
                break;
            case 2:
                otherCube = cubes[0];
                break;
        }
        otherCube.GetComponent<cube>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!serverBeingUpdated)
        {
            byte[] bufferNoUpdate = new byte[2048];
            client.SendTo(bufferNoUpdate, remoteEP);


            int recv = client.ReceiveFrom(outBuffer, ref remoteServer);

            int CubeIndex;
            Vector3 newPos = StringToVector3(Encoding.ASCII.GetString(outBuffer, 0, recv), out CubeIndex);

            if (ClientIndex == 0)
            {
                ClientIndex = CubeIndex;
            }

            if (!myCube)
            {
                myCube = cubes[ClientIndex - 1];
                switch (ClientIndex)
                {
                    case 1:
                        otherCube = cubes[1];
                        break;
                    case 2:
                        otherCube = cubes[0];
                        break;
                }
                otherCube.GetComponent<cube>().enabled = false;
            }

            if (CubeIndex != ClientIndex)
            {
                otherCube.transform.position = newPos;
            }
        }

        //Debug.Log($"Data: {Encoding.ASCII.GetString(outBuffer, 0, recv)}");
    }

    private void OnApplicationQuit()
    {
        client.Disconnect(false);
    }

    public void UpdateServer(Vector3 cubePose)
    {
        serverBeingUpdated = true;
        buffer = Encoding.ASCII.GetBytes($"{myCube.transform.position.x},{myCube.transform.position.y},{myCube.transform.position.z}, {ClientIndex}");
        client.SendTo(buffer, remoteEP);
    }

    public void JoinServer()
    {

    }

    Vector3 StringToVector3(string str, out int index)
    {
        Vector3 vector = Vector3.zero;

        string[] strings = str.Split(',');

        for (int i = 0; i < 3; i++)
        {
            vector[i] = float.Parse(strings[i]);
        }

        index = int.Parse(strings[3]);

        return vector;
    }
}
