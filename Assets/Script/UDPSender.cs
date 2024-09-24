/*using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.XR;

public class UDPSender : MonoBehaviour
{
    private Socket udpSocket;  // Create a Socket for UDP
    public string remoteIPAddress = "172.20.10.10";//"10.20.6.122";  // Public IP of the TouchDesigner machine
    public int remotePort = 8000;  // Port number on the TouchDesigner machine
    private IPEndPoint remoteEndPoint;

    void Start()
    {
        // Create the socket for UDP
        udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Set up the remote endpoint (IP and port)
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPAddress), remotePort);
    }

    void Update()
    {
        // Get hand position using Unity's XR Input System (works on Quest 3)
        var rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        rightHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 handPosition);

        // Prepare the hand position data to send as a string
        //string message = handPosition.x + "," + handPosition.y + "," + handPosition.z;
        string message = "daje!";
        byte[] data = Encoding.UTF8.GetBytes(message);

        // Send the data via the UDP socket
        udpSocket.SendTo(data, remoteEndPoint);
    }

    void OnApplicationQuit()
    {
        // Close the socket when the application quits
        if (udpSocket != null)
        {
            udpSocket.Close();
        }
    }
}


*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;  // Required for XR interaction components

public class UDPSender : MonoBehaviour
{
    private Socket udpSocket;  // Create a Socket for UDP
    public string remoteIPAddress = "192.168.61.162";//"10.20.6.122";  // Public IP of the TouchDesigner machine
    public int remotePort = 8000;  // Port number on the TouchDesigner machine
    private IPEndPoint remoteEndPoint;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;  // XR component to detect grabs

    void Start()
    {
        // Create the socket for UDP
        udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Set up the remote endpoint (IP and port)
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPAddress), remotePort);

        // Get the XRGrabInteractable component attached to the cube
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Add listeners to handle the grab and release events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    // Called when the cube is grabbed
    private void OnGrab(SelectEnterEventArgs args)
    {
        // Send message that the cube was grabbed
        string message = "Cube grabbed!";
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpSocket.SendTo(data, remoteEndPoint);
        Debug.Log("Sent: " + message);
    }

    // Called when the cube is released
    private void OnRelease(SelectExitEventArgs args)
    {
        // Send message that the cube was released
        string message = "Cube released!";
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpSocket.SendTo(data, remoteEndPoint);
        Debug.Log("Sent: " + message);
    }

    void OnApplicationQuit()
    {
        // Close the socket when the application quits
        if (udpSocket != null)
        {
            udpSocket.Close();
        }
    }
}
