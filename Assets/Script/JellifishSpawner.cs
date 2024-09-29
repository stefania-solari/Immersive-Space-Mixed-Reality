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


public class JellifishSpawner : MonoBehaviour
{


    public GameObject objectPrefab;  // Reference to the Cube Prefab
    public GameObject secondJellifish;  // Reference to the Cube Prefab


    public ARPlaneManager planeManager;  // Reference to the ARPlaneManager
    private List<ARPlane> detectedPlanes = new List<ARPlane>();  // List to store detected planes
    private bool planesDetected = false; // Flag to check if planes are detected
    private bool messageReceived = false; // Flag to check if the message has been received


    public Vector3 spawnPosition = new Vector3(0, 0.5f, 0);  // Position to spawn the cube
    private GameObject spawnedJellifish;  // Store the spawned cube instance

    private Socket udpSocket;  // Socket for UDP communication
    public string remoteIPAddress = "192.168.43.201";//"192.168.61.162";  // The IP of the TouchDesigner machine
    public int remotePort = 8000;  // Port number on the TouchDesigner machine
    private IPEndPoint remoteEndPoint;

    private UDPReceiver udpReceiver;  // Reference to the UDPReceiver


    // Track whether each hand is touching the cube
    private bool isLeftHandTouching = false;
    private bool isRightHandTouching = false;




    void Start()
    {
        // Setup the UDP socket
        SetupUDPSocket();

        // Get the UDPReceiver component
        udpReceiver = GetComponent<UDPReceiver>();


        // Subscribe to the message event (e.g., UDP message)
        // Assuming you have an event from your receiver that triggers this
        udpReceiver.OnMessageReceived += HandleMessageReceived;;

        // Collecting detected planes
        planeManager.planesChanged += OnPlanesChanged;

    }

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
             // Send a hello message
            SendMessageToTouchDesigner("all Planes detected.");
        }

        // Try to spawn the object if both planes are detected and the message has been received
        SpawnObjectOnRandomPlaneAuto();
    }



    private void HandleMessageReceived(string message)
    {
        if (message == "InstantiateJellyfish")
        {
            messageReceived = true;
            Debug.Log("Message received: " + message);
            SendMessageToTouchDesigner("message received");


            // Try to spawn the object if both planes are detected and the message has been received
            TrySpawnObject();
        }
    }

    private void TrySpawnObject()
    {
        // Only spawn the object if both the planes are detected and the message has been received
        if (planesDetected && messageReceived)
        {
            SpawnSecondJellifishOnRandomPlane();
        }
    }

    void SpawnObjectOnRandomPlaneAuto()
    {
        if (detectedPlanes.Count == 0)
        {
            Debug.Log("No planes available for spawning.");
            return;
        }

        // Select a random plane
        ARPlane randomPlane = detectedPlanes[Random.Range(0, detectedPlanes.Count)];

        // Get a random point on the plane's bounds
        Vector3 randomPosition = randomPlane.transform.position +
                                 new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f)) * randomPlane.size.x;

        // Instantiate the object at the random position on the plane
        spawnedJellifish = Instantiate(objectPrefab, randomPosition, Quaternion.identity);

        // Ensure the cube has the necessary components for XR interaction
        SetupCubeForXRInteraction(spawnedJellifish);

           // Start the destruction coroutine to destroy the jellyfish after 20 seconds
        StartCoroutine(DestroyAfterDelay(spawnedJellifish, 20f));
    }



    IEnumerator DestroyAfterDelay(GameObject jellyfish, float delay)
    {
        // Wait for the specified delay (20 seconds)
        yield return new WaitForSeconds(delay);

        // Destroy the jellyfish
        if (jellyfish != null)
        {
            Debug.Log("Jellyfish destroyed after " + delay + " seconds.");
            Destroy(jellyfish);
        }
    }


    void SpawnSecondJellifishOnRandomPlane()
    {
        if (detectedPlanes.Count == 0)
        {
            Debug.Log("No planes available for spawning.");
            return;
        }

        // Select a random plane
        ARPlane randomPlane = detectedPlanes[Random.Range(0, detectedPlanes.Count)];

        // Get a random point on the plane's bounds
        Vector3 randomPosition = randomPlane.transform.position +
                                 new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f)) * randomPlane.size.x;

        // Instantiate the object at the random position on the plane
        spawnedJellifish = Instantiate(secondJellifish, randomPosition, Quaternion.identity);

        // Ensure the cube has the necessary components for XR interaction
        SetupCubeForXRInteraction(spawnedJellifish);
    }

  private void OnDestroy()
    {
        // Unsubscribe from the message event when the script is destroyed
        udpReceiver.OnMessageReceived -= HandleMessageReceived;
        planeManager.planesChanged -= OnPlanesChanged;
    }
 
   // Spawns a jellyfish directly in front of the user at a fixed distance

   /*
    void SpawnJellyfishInFrontOfUser()
    {
        // Calculate the exact position directly in front of the user at the specified distance
        Vector3 spawnPosition = userCamera.position + userCamera.forward * spawnDistanceFromUser;

        // Instantiate the jellyfish at the calculated position
        GameObject newJellyfish = Instantiate(jellyfishPrefab, spawnPosition, Quaternion.identity);

        // Start the jellyfish's organic movement toward the user (optional, or it can just float in place)
        StartCoroutine(MoveJellyfishTowardUser(newJellyfish));
    }
*/

    void SetupCubeForXRInteraction(GameObject cube)
    {
        // Add XRGrabInteractable if it's not already on the cube
        XRGrabInteractable grabInteractable = cube.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = cube.AddComponent<XRGrabInteractable>();
        }

        // Ensure the cube has a Rigidbody (required for XR interaction)
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = cube.AddComponent<Rigidbody>();
            rb.isKinematic = true;  // Disable physics when not held
        }

        // Ensure the cube has a Collider
        Collider col = cube.GetComponent<Collider>();
        if (col == null)
        {
            col = cube.AddComponent<BoxCollider>();  // Default to BoxCollider if no Collider is present
        }
        SendMessageToTouchDesigner("Setup jellifish done");

        // Add listeners for detecting when either hand touches or grabs the cube
        grabInteractable.selectEntered.AddListener(OnCubeTouched);
        grabInteractable.selectExited.AddListener(OnCubeReleased);


    }

    void SetupUDPSocket()
    {
        try
        {
            // Initialize the UDP socket
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPAddress), remotePort);
            Debug.Log("UDP socket setup completed.");

            // Send a hello message
            SendMessageToTouchDesigner("Hello from Unity!");
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
            // Send the message via the UDP socket to the specified endpoint
            udpSocket.SendTo(data, remoteEndPoint);
            Debug.Log("Message sent to TouchDesigner: " + message);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error sending data via UDP: " + e.Message);
        }
    }

    // Called when either hand touches the cube
    private void OnCubeTouched(SelectEnterEventArgs args)
    {
        SendMessageToTouchDesigner("Cube touched at first time!");
        // Use interactorObject, which replaces the deprecated interactor
        XRBaseInteractor interactor = args.interactorObject as XRBaseInteractor;

        if (interactor != null)
        {
            SendMessageToTouchDesigner("Interaction no null");

            // Identify which hand is touching the cube
            if (interactor.name.Contains("LeftHand"))
            {
                isLeftHandTouching = true;
                SendMessageToTouchDesigner("left hand");

            }
            else if (interactor.name.Contains("RightHand"))
            {
                isRightHandTouching = true;
                SendMessageToTouchDesigner("right hand");

            }

            // If both hands are touching, trigger the interaction
            if (isLeftHandTouching && isRightHandTouching)
            {
                SendMessageToTouchDesigner("both hands");

                // Change the color of the cube to green to indicate interaction
                //SetCubeColor(Color.green);

                // Send a message indicating the cube was touched by both hands
                SendMessageToTouchDesigner("Cube was touched by both hands!");
            }
        }
    }

    // Called when either hand releases the cube
    private void OnCubeReleased(SelectExitEventArgs args)
    {
         SendMessageToTouchDesigner("Cube released!");

        // Use interactorObject, which replaces the deprecated interactor
        XRBaseInteractor interactor = args.interactorObject as XRBaseInteractor;

        if (interactor != null)
        {
            // Identify which hand released the cube
            if (interactor.name.Contains("LeftHand"))
            {
                isLeftHandTouching = false;
            }
            else if (interactor.name.Contains("RightHand"))
            {
                isRightHandTouching = false;
            }

            // Reset the cube color when neither hand is touching
            if (!isLeftHandTouching && !isRightHandTouching)
            {
                //SetCubeColor(Color.white);
            }
        }
    }



    // Helper function to set the cube's color

/*
    private void SetCubeColor(Color newColor)

    {
        if (spawnedCube != null)
        {
            Renderer renderer = spawnedCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Change the cube's material color
                renderer.material.color = newColor;
            }
            else
            {
                Debug.LogError("Renderer component not found on the cube.");
            }
        }
    }
    */

    void OnApplicationQuit()
    {
        // Close the socket when the application quits to clean up resources
        if (udpSocket != null)
        {
            udpSocket.Close();
            udpSocket = null;
        }
    }
}
