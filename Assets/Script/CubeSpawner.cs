/*using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;  // Reference to the Cube Prefab
    public Vector3 spawnPosition = new Vector3(0, 0.5f, 0);  // Position to spawn the cube
    private GameObject spawnedCube;  // Store the spawned cube instance

    private Socket udpSocket;  // Socket for UDP communication
    public string remoteIPAddress = "192.168.61.162";/// <summary>
    //"10.20.6.122";  // The IP of the TouchDesigner machine
    /// </summary>
    public int remotePort = 8000;  // Port number on the TouchDesigner machine
    private IPEndPoint remoteEndPoint;

    void Start()
    {
        // Setup the UDP socket
        SetupUDPSocket();

        // Spawn the cubePrefab at the specified spawn position
        spawnedCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

        // Get the XRGrabInteractable component from the cube
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = spawnedCube.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Add listener to detect when the cube is touched or grabbed
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnCubeTouched);
        }
        else
        {
            Debug.LogError("XRGrabInteractable not found on the instantiated cube.");
        }

        // Initialize the cube's material color to white (default)
        SetCubeColor(Color.white);
        
    }

    void SetupUDPSocket()
    {
        try
        {
            // Initialize the UDP socket
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIPAddress), remotePort);
            Debug.Log("UDP socket setup completed.");

            // Send the message
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

    // This method is called when the cube is touched or grabbed
    private void OnCubeTouched(SelectEnterEventArgs args)
    {
        // Change the color of the cube to green to indicate interaction
        SetCubeColor(Color.green);

            // Send the message
             SendMessageToTouchDesigner("Cube was touched!");

     
    }

    // Helper function to set the cube's color
    private void SetCubeColor(Color newColor)
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

    void OnApplicationQuit()
    {
        // Close the socket when the application quits to clean up resources
        if (udpSocket != null)
        {
            udpSocket.Close();
            udpSocket = null;
        }
    }
} */



using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab;  // Reference to the Cube Prefab
    public Vector3 spawnPosition = new Vector3(0, 0.5f, 0);  // Position to spawn the cube
    private GameObject spawnedCube;  // Store the spawned cube instance

    private Socket udpSocket;  // Socket for UDP communication
    public string remoteIPAddress = "192.168.61.162";  // The IP of the TouchDesigner machine
    public int remotePort = 8000;  // Port number on the TouchDesigner machine
    private IPEndPoint remoteEndPoint;

    // Track whether each hand is touching the cube
    private bool isLeftHandTouching = false;
    private bool isRightHandTouching = false;

    void Start()
    {
        // Setup the UDP socket
        SetupUDPSocket();

        // Spawn the cubePrefab at the specified spawn position
        spawnedCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

        // Ensure the cube has the necessary components for XR interaction
        SetupCubeForXRInteraction(spawnedCube);

        // After cube is instantiated, set its material color to white (default)
        SetCubeColor(Color.white);  // This is now safe to call after instantiating the cube
    }

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
        SendMessageToTouchDesigner("Setup Cube done");

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
                SetCubeColor(Color.green);

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
                SetCubeColor(Color.white);
            }
        }
    }

    // Helper function to set the cube's color
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
