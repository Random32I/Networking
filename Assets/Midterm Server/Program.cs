using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Lec05
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace asyncServer
{
    internal class Program
    {
        static IPAddress ip = IPAddress.Parse("127.0.0.1");

        private static Socket serverTCP = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] TCPBuffer = new byte[1024];
        private static byte[] TCPOutBuffer = new byte[1024];
        private static char[] outputText;
        private static string text = "";

        static void Main(string[] args)
        {
            byte[] outBuffer = new byte[2048];
            byte[] inBuffer = new byte[2048];

            IPEndPoint localEP = new IPEndPoint(ip, 1111);
            Socket serverUDP = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            // Create an EP to capture the info from the sending client
            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0); // 0 means any available port
            EndPoint remoteClient = (EndPoint)client;

            serverTCP.Bind(localEP);

            serverTCP.Listen(1);
            serverTCP.BeginAccept(new AsyncCallback(AcceptCallback), null);

            // Bind, send/receive data
            try
            {
                serverUDP.Bind(localEP);
                Console.WriteLine("Waiting for data...");
                while (true)
                {
                    int recv = serverUDP.ReceiveFrom(inBuffer, ref remoteClient);

                    float[] newPos = new float[3];
                    bool failed = false;
                    try
                    {
                        newPos = StringToVector3(Encoding.ASCII.GetString(inBuffer, 0, recv));
                        Console.WriteLine("Recieved positions x:" + newPos[0] + "," + newPos[1] + "," + newPos[2] + " from " + client.Address.ToString());
                    }
                    catch(Exception e)
                    {
                        //Client Updated
                        failed = true;
                        serverUDP.SendTo(outBuffer, 0, outBuffer.Length, SocketFlags.None, remoteClient);
                    }

                    if (!failed)
                    {
                        //Server Updated
                        outBuffer = Encoding.ASCII.GetBytes($"{newPos[0]},{newPos[1]},{newPos[2]}");
                        serverUDP.SendTo(outBuffer, 0, outBuffer.Length, SocketFlags.None, remoteClient);
                        Console.WriteLine("Sent to " + client.Address.ToString());
                    }

                    /*Server Updated
                    else
                    {
                        outBuffer = Encoding.ASCII.GetBytes(newPos.ToString());
                        olderPos = oldPos;
                        oldPos = newPos;
                        
                    }*/

                    //Console.WriteLine("Data: {0}", Encoding.ASCII.GetString(inBuffer, 0, recv));
                }
                //server.Shutdown(SocketShutdown.Both);
                //server.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static float[] StringToVector3(string str)
        {
            float[] Vector = new float[3];

            string[] strings = str.Split(',');

            for (int i = 0; i < 3; i++)
            {
                Vector[i] = float.Parse(strings[i]);
            }

            return Vector;
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            Socket socket = serverTCP.EndAccept(result);
            Console.WriteLine("Client connected!!");
            socket.BeginReceive(TCPBuffer, 0, TCPBuffer.Length, 0, new AsyncCallback(ReceiveCallback), socket);
        }
        private static void ReceiveCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            int rec = socket.EndReceive(result);
            outputText = new char[rec / 2];
            Buffer.BlockCopy(TCPBuffer, 0, outputText, 0, rec);

            if (outputText[0] != 0)
            {
                text = new string(outputText);
                Console.WriteLine("Received: " + text);
            }
            else
            {
                outputText = text.ToCharArray();
                Console.WriteLine("Not Received");
            }
        

            socket.BeginReceive(TCPBuffer, 0, TCPBuffer.Length, 0, new AsyncCallback(ReceiveCallback), socket);
            Buffer.BlockCopy(TCPBuffer, 0, TCPOutBuffer, 0, rec);
            Console.WriteLine("Sending: " + text);
            socket.BeginSend(TCPOutBuffer, 0, TCPOutBuffer.Length, 0, new AsyncCallback(SendCallback), socket);
        }
        private static void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }
    }
}