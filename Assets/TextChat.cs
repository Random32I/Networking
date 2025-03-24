using UnityEngine;
using TMPro;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Lec05
using System.Threading;
using System.Net;
using System.Net.Sockets;

public class TextChat : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] TMP_InputField input;

    private static string text;
    private static char[] outputText;
    private static byte[] bText = new byte[1024];

    private static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private static byte[] buffer = new byte[1024];

    static bool sending;

    // Start is called before the first frame update
    void Start()
    {
        client.Connect(IPAddress.Parse("127.0.0.1"), 1111);
        Debug.Log("Connected to server...");
    }

    // Update is called once per frame
    void Update()
    {
        if (!sending)
        {
            client.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), client);
        }

        textBox.text = text;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnEnter();
        }
    }

    private void OnApplicationQuit()
    {
        outputText = $"Other Player Left the Game".ToCharArray();
        Buffer.BlockCopy(outputText, 0, bText, 0, outputText.Length * 2);
        client.BeginSend(bText, 0, bText.Length, 0, new AsyncCallback(SendCallback), client);
        client.Disconnect(false);
    }

    public void OnEnter()
    {
        sending = true;
        text = input.text;
        outputText = text.ToCharArray();
        Buffer.BlockCopy(outputText, 0, bText, 0, outputText.Length * 2);
        Debug.Log($"Sending Text: {new string(outputText)}");
        client.BeginSend(bText, 0, bText.Length, 0, new AsyncCallback(SendCallback), client);
    }

    private static void ReceiveCallback(IAsyncResult result)
    {
        Socket socket = result.AsyncState as Socket;
        int rec = socket.EndReceive(result);
        outputText = new char[rec / 2];
        Debug.Log($"Rec: {rec}, Output Text: {new string(outputText)}");
        Buffer.BlockCopy(buffer, 0, outputText, 0, rec);
        //if (outputText[0] != 0)
        //{
            text = new string(outputText);
        //}
        //else
        //{
        //    outputText = text.ToCharArray();
        //}
        Debug.Log($"Received Text: {text}");
        socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), socket);
    }
    private static void SendCallback(IAsyncResult result)
    {
        Socket socket = (Socket)result.AsyncState;
        socket.EndSend(result);

        //Thread.Sleep(1000);
        //socket.BeginSend(bText, 0, bText.Length, 0, new AsyncCallback(SendCallback), socket);
        sending = false;
    }
}
