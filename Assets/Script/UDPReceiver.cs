using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    public int listenPort = 9000;  // Port to listen for incoming data from TouchDesigner
    private IPEndPoint remoteEndPoint;

    public GameObject cubePrefab;  // Reference to a cube prefab for instantiation
    public Vector3 spawnPosition = new Vector3(0, 1, 0);  // Default spawn position for cubes
    public float spawnRange = 3.0f;  // Range for random cube placement

    void Start()
    {
        // Initialize the UDP Client to listen for incoming messages
        udpClient = new UdpClient(listenPort);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
        // Start listening for incoming data
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
 InstantiateNewCube();

        Debug.Log("UDP Receiver started, listening on port " + listenPort);
        
    }



    // Callback function when data is received
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            
            // Receive the incoming UDP data
            byte[] receiveBytes = udpClient.EndReceive(ar, ref remoteEndPoint);

            // Convert the received bytes to a string
            string receivedMessage = Encoding.UTF8.GetString(receiveBytes);
            Debug.Log("Received message from TouchDesigner: " + receivedMessage);

            // Process the received message (you can add your custom logic here)
            ProcessReceivedMessage(receivedMessage);

            // Continue listening for more data
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving UDP data: " + e.Message);
        }
    }

    // Function to process the received message
    private void ProcessReceivedMessage(string message)
    {
        // If we receive the "InstantiateCube" command, spawn a new cube
        if (message == "InstantiateCube")
        {
            Debug.Log("Instantiating a new cube...");
            InstantiateNewCube();
        }
        else
        {
            Debug.Log("Unknown message received: " + message);
        }
    }

    // Function to instantiate a new cube
    private void InstantiateNewCube()
    {
        // If a cube prefab is provided, instantiate it at a random position
        if (cubePrefab != null)
        {
            // Generate a random spawn position within a defined range
            Vector3 randomPosition = new Vector3(
                spawnPosition.x + UnityEngine.Random.Range(-spawnRange, spawnRange),
                spawnPosition.y,
                spawnPosition.z + UnityEngine.Random.Range(-spawnRange, spawnRange)
            );

            // Instantiate the cube at the random position
            Instantiate(cubePrefab, randomPosition, Quaternion.identity);
            Debug.Log("New cube instantiated at position: " + randomPosition);
        }
        else
        {
            Debug.LogError("No cube prefab assigned!");
        }
    }

    // Cleanup the UDP client when the application quits
    void OnApplicationQuit()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
