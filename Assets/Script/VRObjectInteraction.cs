using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class VRObjectInteraction : MonoBehaviour
{
    private Socket udpSenderSocket;  // Socket to send data
    private UdpClient udpReceiverClient;  // UDP client to receive data
    public string remoteIPAddress = "172.20.10.10"; //"10.20.6.122";  // Replace with TouchDesigner's IP address
    public int remotePort = 8000;  // Port on TouchDesigner machine
    public int localPort = 9000;  // Local port for receiving data from TouchDesigner

    private IPEndPoint remoteEndPoint;
    private bool objectTouched = false;  // State to track if object was touched

    void Start()
    {
        // Create the UDP sender socket for sending data to TouchDesigner
        udpSenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPAddress), remotePort);

        // Set up the UDP receiver client for receiving messages from TouchDesigner
        udpReceiverClient = new UdpClient(localPort);  // Listening on a port for incoming data
        udpReceiverClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);  // Begin receiving data asynchronously

        Debug.Log("Started UDP communication. Listening for incoming data on port " + localPort);
    }

    // Called when the object is touched by a trigger (VR hand/controller)
    void OnTriggerEnter(Collider other)
    {
        // Check if the object that triggered the touch is the player's hand or controller
        if (other.CompareTag("Hand") || other.CompareTag("Controller"))
        {
            if (!objectTouched)
            {
                // Send message to TouchDesigner when object is touched
                SendUDPMessage("Object touched!");

                // Make the object disappear
                MakeObjectDisappear();
                objectTouched = true;  // Mark the object as touched
            }
        }
    }

    // Function to send a UDP message to TouchDesigner
    private void SendUDPMessage(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpSenderSocket.SendTo(data, remoteEndPoint);
        Debug.Log("Sent message to TouchDesigner: " + message);
    }

    // Function to make the object disappear (deactivate it)
    private void MakeObjectDisappear()
    {
        gameObject.SetActive(false);  // Deactivate the object
        Debug.Log("Object has disappeared.");
    }

    // Callback for when data is received from TouchDesigner
    private void ReceiveCallback(IAsyncResult ar)
    {
        IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, localPort);
        byte[] receiveBytes = udpReceiverClient.EndReceive(ar, ref senderEndPoint);
        
        // Convert the received bytes to a string (assuming the data is sent as text)
        string receivedMessage = Encoding.UTF8.GetString(receiveBytes);
        Debug.Log("Received data from TouchDesigner: " + receivedMessage);

        // Process the received data (e.g., to reactivate the object)
        ProcessReceivedData(receivedMessage);

        // Continue listening for more data
        udpReceiverClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    // Function to process received data and reactivate the object
    private void ProcessReceivedData(string data)
    {
        // If the message is "Reactivate", we re-enable the object in the scene
        if (data == "Reactivate")
        {
            gameObject.SetActive(true);  // Reactivate the object
            objectTouched = false;  // Reset the objectTouched state
            Debug.Log("Object has been reactivated.");
        }
    }

    void OnApplicationQuit()
    {
        // Clean up the UDP sockets on application quit
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
