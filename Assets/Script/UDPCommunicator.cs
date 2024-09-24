using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

public class UDPCommunicator : MonoBehaviour
{
    private Socket udpSenderSocket;  // Socket for sending data
    private UdpClient udpReceiverClient;  // UdpClient for receiving data
    public string remoteIPAddress = "79.152.133.185";//"10.20.6.122";  // IP of the TouchDesigner machine
    public int remotePort = 8000;  // Port on the TouchDesigner machine
    public int localPort = 9000;  // Local port for receiving data from TouchDesigner

    private IPEndPoint remoteEndPoint;

    void Start()
    {
        // Create the UDP sender socket
        udpSenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPAddress), remotePort);

        // Set up the UDP receiver client
        udpReceiverClient = new UdpClient(localPort);  // Listening on a port for incoming data
        udpReceiverClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);  // Begin receiving data asynchronously

        Debug.Log("Started UDP communication. Listening for incoming data on port " + localPort);
    }

    void Update()
    {
        // Send data from the Meta Quest 3 (like hand positions)
        var rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        rightHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 handPosition);

        // Prepare the data to send (can be hand position or a custom message)
        string message = "daje!";  // Example message
        byte[] data = Encoding.UTF8.GetBytes(message);

        // Send the data via the UDP socket
        udpSenderSocket.SendTo(data, remoteEndPoint);
    }

    // Callback for when data is received
    private void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, localPort);
        byte[] receiveBytes = udpReceiverClient.EndReceive(ar, ref senderEndPoint);
        
        // Convert the received bytes to a string (assuming the data is sent as text)
        string receivedMessage = Encoding.UTF8.GetString(receiveBytes);
        Debug.Log("Received data from TouchDesigner: " + receivedMessage);

        // Process the received data (this is where you can trigger an event in Unity)
        ProcessReceivedData(receivedMessage);

        // Continue listening for more data
        udpReceiverClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    private void ProcessReceivedData(string data)
    {
        // Process the incoming data and apply it to your Unity project
        // For example, you could control the position of an object based on the received data
        Debug.Log("Processing received data: " + data);

        // Example: Parse data if it's a position and apply it
        // string[] values = data.Split(',');
        // float x = float.Parse(values[0]);
        // float y = float.Parse(values[1]);
        // float z = float.Parse(values[2]);
        // Vector3 newPosition = new Vector3(x, y, z);
        // transform.position = newPosition;  // Move an object in Unity based on the data
    }

    void OnApplicationQuit()
    {
        // Clean up the sockets on quit
        if (udpSenderSocket != null)
        {
            udpSenderSocket.Close();
        }
        if (udpReceiverClient != null)
        {
            udpReceiverClient.Close();
        }
    }
}
