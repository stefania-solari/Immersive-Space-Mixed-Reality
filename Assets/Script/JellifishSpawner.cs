using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using System.Threading;
using System;

public class JellyfishSpawner : MonoBehaviour
{
    //socket
    private Socket udpSocket;  // Socket for UDP communication
    private IPEndPoint remoteEndPoint;
    private IPEndPoint localEndPoint;
    private Thread udpReceiveThread;
    private bool isReceiving = true;

    // IP and Port Configuration
    public string remoteIPAddress = "192.168.43.201";  // The IP of the TouchDesigner machine
    public int remotePort = 8000;  // Port number on the TouchDesigner machine

    public string localIPAddress = "192.168.43.5";  // IP address of the receiver (e.g., Meta Quest or PC)
    public int localPort = 9000;  // Port of the receiver (Meta Quest or PC)

    public delegate void MessageReceivedHandler(string message);
    public event MessageReceivedHandler OnMessageReceived;




    //instances
    public GameObject objectPrefab;  // Reference to the Jellyfish Prefab
    public GameObject secondJellyfish;  // Reference to the second Jellyfish Prefab

    public ARPlaneManager planeManager;  // Reference to the ARPlaneManager
    private List<ARPlane> detectedPlanes = new List<ARPlane>();  // List to store detected planes
    private bool planesDetected = false; // Flag to check if planes are detected
    private bool messageReceived = false; // Flag to check if the message has been received

    public List<GameObject> jellyfishList = new List<GameObject>();  // List to store spawned jellyfish instances
    public Vector3 spawnPosition = new Vector3(0, 0.5f, 0);  // Position to spawn the jellyfish
    private GameObject spawnedJellyfish;  // Store the most recently spawned jellyfish instance

    //interactions

    // Track whether each hand is touching the jellyfish
    private bool isLeftHandTouching = false;
    private bool isRightHandTouching = false;

    public bool isTouched = false;  // Tracks if the jellyfish has been touched



    public int portToCheck = 9000;
    private UdpClient udpListener;
    private bool isListening = true;

    void Start()
    {
          // Initialize UDP socket
        SetupUDPSocket();
        // Start receiving messages
        //StartReceiving();
 

        // Collecting detected planes
        planeManager.planesChanged += OnPlanesChanged;



          // Start listening for incoming data on the specified port
        if (CheckIfPortCanReceiveData(portToCheck))
        {
            SendMessageToTouchDesigner("Listening for incoming data on UDP port " + portToCheck);

            Debug.Log("Listening for incoming data on UDP port " + portToCheck);
        }
        else
        {
            SendMessageToTouchDesigner("Failed to listen on UDP port " + portToCheck);
            Debug.LogError("Failed to listen on UDP port " + portToCheck);
        }



  
    }


    bool CheckIfPortCanReceiveData(int port)
    {
        try
        {
            // Initialize the UDP listener
            udpListener = new UdpClient(port);
            SendMessageToTouchDesigner("Started listening on port " + port);

            Debug.Log("Started listening on port " + port);

            // Begin receiving data asynchronously
            udpListener.BeginReceive(OnDataReceived, null);
            SendMessageToTouchDesigner("is listening");

            return true;


        }
        catch (SocketException ex)
        {
            SendMessageToTouchDesigner("SocketException while trying to receive on port " + port + ": " + ex.Message);

            Debug.LogError("SocketException while trying to receive on port " + port + ": " + ex.Message);
            return false;
        }
    }


        private void OnDataReceived(IAsyncResult result)
    {
        try
        {
           SendMessageToTouchDesigner("OnDataReceived");

            // Get the received data
            IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, portToCheck);
            byte[] receivedData = udpListener.EndReceive(result, ref senderEndPoint);

           SendMessageToTouchDesigner("message received");

            // Convert the received data to a string
            string receivedMessage = Encoding.UTF8.GetString(receivedData);
            Debug.Log("Data received on port " + portToCheck + ": " + receivedMessage);
            SendMessageToTouchDesigner("Data received on port " +  portToCheck + ": " + receivedMessage);
            HandleMessageReceived(receivedMessage);


            // Restart listening for more data
            if (isListening)
            {
                udpListener.BeginReceive(OnDataReceived, null);

            }
        }
        catch (SocketException ex)
        {
            SendMessageToTouchDesigner("Error receiving data: " + ex.Message);

            Debug.LogError("Error receiving data: " + ex.Message);
        }
    }





//---------------------------------------------------------- Udp send/receive
    void SetupUDPSocket()
    {
        try
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPAddress), remotePort);
            Debug.Log("UDP socket setup completed.");
            SendMessageToTouchDesigner("Hello before local endpoint");


             // Bind the socket to the local IP and port (Meta Quest)
            localEndPoint = new IPEndPoint(IPAddress.Parse(localIPAddress), localPort);
            udpSocket.Bind(localEndPoint);

            SendMessageToTouchDesigner("Hello after local endopint!");
        }
        catch (SocketException e)
        {
            Debug.LogError("Failed to setup UDP socket: " + e.Message);
        }
    }

    void SendMessageToTouchDesigner(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        try
        {
            udpSocket.SendTo(data, remoteEndPoint);
            Debug.Log("Message sent to TouchDesigner: " + message);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error sending data via UDP: " + e.Message);
        }
    }

    // Start receiving UDP messages in a background thread

    /*
    void StartReceiving()
    {
        udpReceiveThread = new Thread(new ThreadStart(ReceiveMessages));
        udpReceiveThread.IsBackground = true;
        SendMessageToTouchDesigner("daje!");

        udpReceiveThread.Start();

        SendMessageToTouchDesigner("daje 2!");


    }

*/



/*
void ReceiveMessages()
{
    while (isReceiving)
    {
        SendMessageToTouchDesigner("is receiving");  // Confirm method is running

        try
        {
            SendMessageToTouchDesigner("Starting the try block for receiving message");

            byte[] data = new byte[1024]; // Buffer for incoming messages
            EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Receive data from the UDP socket
            int receivedLength = udpSocket.ReceiveFrom(data, ref senderEndPoint);
            SendMessageToTouchDesigner("mmm");

            // If data is received
            if (receivedLength > 0)
            {
                SendMessageToTouchDesigner("The message");

                // Convert the received data to a string
                string message = Encoding.UTF8.GetString(data, 0, receivedLength);

                Debug.Log("Message received: " + message);  // Log the received message
                SendMessageToTouchDesigner("Received message: " + message);

                // Check if the message is "InstantiateJellyfish"
                if (message == "InstantiateJellyfish")
                {
                    Debug.Log("InstantiateJellyfish message received. Spawning a new jellyfish.");
                    SendMessageToTouchDesigner("InstantiateJellyfish message received!");

                    // Call the method to instantiate a new jellyfish
                    //SpawnJellyfish();
                    SpawnSecondJellyfishOnRandomPlane();

                }
                else
                {
                    Debug.Log("Received different message: " + message);
                }

                // Trigger the message received event
                OnMessageReceived?.Invoke(message);
            }
            else
            {
                SendMessageToTouchDesigner("No data received, buffer length is 0");
            }
        }
        catch (SocketException e)
        {
            SendMessageToTouchDesigner("SocketException occurred: " + e.Message);
            Debug.LogError("Error receiving UDP message: " + e.Message);
        }
        catch (Exception ex)
        {
            SendMessageToTouchDesigner("General exception occurred: " + ex.Message);
            Debug.LogError("General error occurred: " + ex.Message);
        }
    }
}

*/



    void OnApplicationQuit()
    {
        /*
        isReceiving = false;

        if (udpReceiveThread != null)
        {
            udpReceiveThread.Join();
        }
*/
           // Stop listening and close the UDP listener when the application exits
        isListening = false;
        if (udpListener != null)
        {
            udpListener.Close();
            udpListener = null;
        }

        if (udpSocket != null)
        {
            udpSocket.Close();
        }
    }


//---------------------------------------------spawn auto
    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Add the detected planes to the list
        foreach (var addedPlane in args.added)
        {
            detectedPlanes.Add(addedPlane);
        }

        // Set planesDetected to true once we detect at least one plane
        if (detectedPlanes.Count > 0)
        {
            planesDetected = true;
            Debug.Log("Planes detected.");
            // Send a message
            SendMessageToTouchDesigner("Planes detected.");
           
        }

            SendMessageToTouchDesigner("All planes detected.");

        // Try to spawn the object if  planes are detected
        StartCoroutine(SpawnObjectOnRandomPlaneAuto());
        

        
    }



IEnumerator SpawnJellyfishEveryTwoSeconds()
{
    // Infinite loop to spawn jellyfish at intervals
    while (true)
    {
        // Only spawn jellyfish if there are detected planes
        if (detectedPlanes.Count > 0)
        {
            Debug.Log("Spawning jellyfish...");
            SpawnObjectOnRandomPlaneAuto();
        }
        else
        {
            Debug.Log("No planes detected yet. Waiting...");
        }

        // Wait for 2 seconds before spawning the next jellyfish
        yield return new WaitForSeconds(1f);
    }
}


    IEnumerator SpawnObjectOnRandomPlaneAuto()
    {
        if (detectedPlanes.Count == 0)
        {
            Debug.LogError("No planes available for spawning.");
            yield return null;
        }

        // Select a random plane
        ARPlane randomPlane = detectedPlanes[UnityEngine.Random.Range(0, detectedPlanes.Count)];

        // Get a random point on the plane's bounds
        Vector3 randomPosition = randomPlane.transform.position +
                                 new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f)) * randomPlane.size.x;

        // Instantiate the jellyfish at the random position on the plane
        spawnedJellyfish = Instantiate(objectPrefab, randomPosition, Quaternion.identity);
        jellyfishList.Add(spawnedJellyfish);  // Add the spawned jellyfish to the list

        // Ensure the jellyfish has the necessary components for XR interaction
        SetupJellyfishForXRInteraction(spawnedJellyfish);

        // Start the destruction coroutine to destroy the jellyfish after 20 seconds
        StartCoroutine(DestroyAfterDelay(spawnedJellyfish, 10f));
        yield return null;  // Wait 1 second before spawning the next jellyfish

    }

    
    /*
    private void SpawnObjectOnRandomPlaneAuto()
{
    // Check if there are any detected planes
    if (detectedPlanes == null || detectedPlanes.Count == 0)
    {
        Debug.LogError("No planes available for spawning.");
        return;
    }

    // Select a random plane from the list
    //ARPlane randomPlane = detectedPlanes[UnityEngine.Random.Range(0, detectedPlanes.Count)];

    // Check if the plane size is valid
    if (randomPlane == null || randomPlane.size == Vector2.zero)
    {
        Debug.LogError("Invalid plane selected or plane size is zero.");
        return;
    }
    

    // Get a random point on the plane's bounds
    Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));

    spawnedJellyfish = Instantiate(objectPrefab, randomPosition, Quaternion.identity);
    // Ensure the jellyfish has the necessary components for XR interaction
    SetupJellyfishForXRInteraction(spawnedJellyfish);

    // Start the destruction coroutine to destroy the jellyfish after 10 seconds
    //StartCoroutine(DestroyAfterDelay(spawnedJellyfish, 10f));

}
*/

    IEnumerator DestroyAfterDelay(GameObject jellyfish, float delay)
    {
        // Wait for the specified delay (20 seconds)
        yield return new WaitForSeconds(delay);

        // Destroy the jellyfish
        if (jellyfish != null)
        {
            Debug.Log("Jellyfish destroyed after " + delay + " seconds.");
            Destroy(jellyfish);
            jellyfishList.Remove(jellyfish);  // Remove it from the list
        }
    }



//-------------------------------------------------spawn message


    private void HandleMessageReceived(string message)
    {
        SpawnSecondJellyfishOnRandomPlane();

        SendMessageToTouchDesigner("Message received, need to check");
        if (message == "InstantiateJellyfish")
        {
            messageReceived = true;
            Debug.Log("Message received: " + message);
            SendMessageToTouchDesigner("Message received");

            // Try to spawn the object if both planes are detected and the message has been received
            //SpawnSecondJellyfishOnRandomPlane();
            
        }
    }

/*
    private void TrySpawnObject()
    {
        // Only spawn the object if both the planes are detected and the message has been received
        if (planesDetected && messageReceived)
        {
            SpawnSecondJellyfishOnRandomPlane();
        }
    }*/

    void SpawnSecondJellyfishOnRandomPlane()
    {
        if (detectedPlanes.Count == 0)
        {
            Debug.Log("No planes available for spawning.");
            return;
        }

        // Select a random plane
        ARPlane randomPlane = detectedPlanes[UnityEngine.Random.Range(0, detectedPlanes.Count)];

        // Get a random point on the plane's bounds
        Vector3 randomPosition = randomPlane.transform.position +
                                 new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f)) * randomPlane.size.x;

        // Instantiate the second jellyfish at the random position on the plane
        GameObject newJellyfish = Instantiate(secondJellyfish, randomPosition, Quaternion.identity);
        jellyfishList.Add(newJellyfish);  // Add the new jellyfish to the list

        // Ensure the jellyfish has the necessary components for XR interaction
        SetupJellyfishForXRInteraction(newJellyfish);
        StartCoroutine(DestroyAfterDelay(spawnedJellyfish, 10f));

    }

//------------------------------------------------------------------- change color
    // Change color of all jellyfish in the environment

public Color newColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);  // Red color
    void ChangeAllJellyfishColor(Color newColor)
    {
        foreach (GameObject jellyfish in jellyfishList)
        {
             SendMessageToTouchDesigner("Inside the jellifish");

            Transform secondChild = jellyfish.transform.GetChild(0).GetChild(0);  // Assuming second child is index 1
            Renderer renderer = secondChild.GetComponent<Renderer>();
            if (renderer != null)
            {
                 SendMessageToTouchDesigner("Jellifish color");

                renderer.material.color = newColor;
                Debug.Log("Jellyfish color changed to: " + newColor);

            }
        }
    }



    //---------------------------------------------------------jellyfish interaction

    void SetupJellyfishForXRInteraction(GameObject jellyfish)
    {
        XRGrabInteractable grabInteractable = jellyfish.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = jellyfish.AddComponent<XRGrabInteractable>();
        }

        //SendMessageToTouchDesigner("Jellyfish setup done");

        // Add interaction listeners
        grabInteractable.selectEntered.AddListener(OnJellyfishTouched);
        grabInteractable.selectExited.AddListener(OnJellyfishReleased);
    }

    private void OnJellyfishTouched(SelectEnterEventArgs args)
    {
        SendMessageToTouchDesigner("Jellyfish arg! touch");

        GameObject touchedJellyfish = args.interactableObject.transform.gameObject;
        Transform secondChild = touchedJellyfish.transform.GetChild(0).GetChild(0);  // Assuming second child is index 1

        //some actions?
          Renderer renderer = secondChild.GetComponent<Renderer>();
            if (renderer != null)
            {
                SendMessageToTouchDesigner("Jellifish color");

                renderer.material.color = newColor;
                Debug.Log("Jellyfish color changed to: " + newColor);
            }


    }

    private void OnJellyfishReleased(SelectExitEventArgs args)
    {
    
        SendMessageToTouchDesigner("Jellyfish arg! release");

        GameObject releasedJellyfish = args.interactableObject.transform.gameObject;
        SendMessageToTouchDesigner("Jellyfish released!");

        // Destroy the jellyfish after release
        if (releasedJellyfish != null)
        {
            StartCoroutine(DestroyAfterDelay(releasedJellyfish, 0.5f));  // Destroy after 1 second
        }

    }




}
