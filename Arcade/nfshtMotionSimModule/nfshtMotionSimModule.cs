﻿using UnityEngine;
using UnityEngine.XR;
using WIGU;
using System.Collections.Generic;
using EmuVR.InputManager;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;

namespace WIGUx.Modules.nfshtMotionSim
{
    public class nfshtMotionSimController : MonoBehaviour
    {
        static IWiguLogger logger = ServiceProvider.Instance.GetService<IWiguLogger>();

        private readonly float keyboardVelocityX = 1f;  // Velocity for keyboard input
        private readonly float keyboardVelocityY = 1f;  // Velocity for keyboard input
        private readonly float keyboardVelocityZ = 1f;  // Velocity for keyboard input
        private readonly float vrVelocity = 30f;        // Velocity for VR controller input

        private readonly float centeringVelocityX = 15f;  // Velocity for centering rotation
        private readonly float centeringVelocityY = 30f;  // Velocity for centering rotation
        private readonly float centeringVelocityZ = 30f;  // Velocity for centering rotation

        private float adjustSpeed = 1.0f;  // Adjust this adjustment speed as needed a lower number will lead to smaller adustments

        private float rotationLimitX = 3f;  // Rotation limit for X-axis
        private float rotationLimitY = 6f;  // Rotation limit for Y-axis
        private float rotationLimitZ = 6f;  // Rotation limit for Z-axis

        private float currentRotationX = 0f;  // Current rotation for X-axis
        private float currentRotationY = 0f;  // Current rotation for Y-axis
        private float currentRotationZ = 0f;  // Current rotation for Z-axis

        private Transform nfshtXObject; // Reference to the main X object
        private Transform nfshtYObject; // Reference to the main Y object
        private Transform nfshtZObject; // Reference to the main Z object
        private GameObject cockpitCam;    // Reference to the cockpit camera

        // Initial positions and rotations for resetting
        private Vector3 nfshtXStartPosition;
        private Quaternion nfshtXStartRotation;
        private Vector3 nfshtYStartPosition;
        private Quaternion nfshtYStartRotation;
        private Vector3 nfshtZStartPosition;
        private Quaternion nfshtZStartRotation;
        private Vector3 cockpitCamStartPosition;
        private Quaternion cockpitCamStartRotation;

        // Controller animation 
        // Speeds for the animation of the in game flight stick or wheel
        private readonly float keyboardControllerVelocityX = 600f;  // Velocity for keyboard input
        private readonly float keyboardControllerVelocityY = 600f;  // Velocity for keyboard input
        private readonly float keyboardControllerVelocityZ = 600f;  // Velocity for keyboard input
        private readonly float vrControllerVelocity = 350f;        // Velocity for VR controller input

        private float controllerrotationLimitX = 0f;  // Rotation limit for X-axis (stick or wheel)
        private float controllerrotationLimitY = 0f;  // Rotation limit for Y-axis (stick or wheel)
        private float controllerrotationLimitZ = 150f;  // Rotation limit for Z-axis (stick or wheel)

        private float currentControllerRotationX = 0f;  // Current rotation for X-axis (stick or wheel)
        private float currentControllerRotationY = 0f;  // Current rotation for Y-axis (stick or wheel)
        private float currentControllerRotationZ = 0f;  // Current rotation for Z-axis (stick or wheel)

        private readonly float centeringControllerVelocityX = 600f;  // Velocity for centering rotation (stick or wheel)
        private readonly float centeringControllerVelocityY = 600f;  // Velocity for centering rotation (stick or wheel)
        private readonly float centeringControllerVelocityZ = 600f;  // Velocity for centering rotation (stick or wheel)

        private Transform nfshtControllerX; // Reference to the main animated controller (wheel)
        private Vector3 nfshtControllerXStartPosition; // Initial controller positions and rotations for resetting
        private Quaternion nfshtControllerXStartRotation; // Initial controlller positions and rotations for resetting
        private Transform nfshtControllerY; // Reference to the main animated controller (wheel)
        private Vector3 nfshtControllerYStartPosition; // Initial controller positions and rotations for resetting
        private Quaternion nfshtControllerYStartRotation; // Initial controlller positions and rotations for resetting
        private Transform nfshtControllerZ; // Reference to the main animated controller (wheel)
        private Vector3 nfshtControllerZStartPosition; // Initial controller positions and rotations for resetting
        private Quaternion nfshtControllerZStartRotation; // Initial controlller positions and rotations for resetting

        // Initial positions and rotations for VR setup
        private Vector3 playerCameraStartPosition;
        private Quaternion playerCameraStartRotation;
        private Vector3 playerVRSetupStartPosition;
        private Quaternion playerVRSetupStartRotation;
        private Vector3 playerCameraStartScale;
        private Vector3 playerVRSetupStartScale;

        // GameObject references for PlayerCamera and VR setup
        private GameObject playerCamera;
        private GameObject playerVRSetup;

        //Lights and Emissives
        public float dimIntensity = 1.0f;    // Set the dim intensity level
        public string fire1Button = "Fire1"; // Name of the fire button
        public string fire2Button = "Fire2"; // Name of the fire button 
        public string fire3Button = "Fire3"; // Name of the fire button 
        public string JumpButton = "Jump"; // Name of the fire button 
        private Light[] strobes;
        private Renderer[] hazardRenderers;
        public Light strobe1_light;
        public Light strobe2_light;
        public Light strobe3_light;
        public Light strobe4_light;
        private float flashDuration = 0.05f;
        private float flashInterval = 0.05f;
        private float lightDuration = 0.5f; // Duration during which the lights will be on
        private bool arenfshtLighsOn = false; // track strobe lights
        private bool isnfshtFlashing = false; //set the flashing flag
        private bool isnfshtinHigh = false; //set the gear flag
        private bool inFocusMode = false;  // Flag to track focus mode state
        private bool areStrobesOn = false; // track police lights
        private Coroutine strobeCoroutine; // Coroutine variable to control the strobe flashing
        private Transform nfshthazard1Object;
        private Transform nfshthazard2Object;
        private Transform nfshthazard3Object;
        private Transform nfshthazard4Object;

        private readonly string[] compatibleGames = { "Need For Speed HEAT Takedown" };

        private Dictionary<GameObject, Transform> originalParents = new Dictionary<GameObject, Transform>();  // Dictionary to store original parents of objects

        void Start()
        {
            // Find references to PlayerCamera and VR setup objects
            playerCamera = PlayerVRSetup.PlayerCamera.gameObject;
            playerVRSetup = PlayerVRSetup.PlayerRig.gameObject;

            // Check if objects are found
            CheckObject(playerCamera, "PlayerCamera");
            CheckObject(playerVRSetup, "PlayerVRSetup.PlayerRig");

            GameObject cameraObject = GameObject.Find("OVRCameraRig");

            // Find gfpce2X object in hierarchy
            nfshtXObject = transform.Find("nfshtX");
            if (nfshtXObject != null)
            {
                logger.Info("nfshtX object found.");
                nfshtXStartPosition = nfshtXObject.position;
                nfshtXStartRotation = nfshtXObject.rotation;

                // Find nfshtY object under nfshtX
                nfshtYObject = nfshtXObject.Find("nfshtY");
                if (nfshtYObject != null)
                {
                    logger.Info("nfshtY object found.");
                    nfshtYStartPosition = nfshtYObject.position;
                    nfshtYStartRotation = nfshtYObject.rotation;

                    // Find nfshtZ object under nfshtY
                    nfshtZObject = nfshtYObject.Find("nfshtZ");
                    if (nfshtZObject != null)
                    {
                        logger.Info("nfshtZ object found.");
                        nfshtZStartPosition = nfshtZObject.position;
                        nfshtZStartRotation = nfshtZObject.rotation;

                        // Find nfshtControllerX under nfshtZ
                        nfshtControllerX = nfshtZObject.Find("nfshtControllerX");
                        if (nfshtControllerX != null)
                        {
                            logger.Info("nfshtControllerX object found.");
                            // Store initial position and rotation of the stick
                            nfshtControllerXStartPosition = nfshtControllerX.transform.position; // these could cause the controller to mess up
                            nfshtControllerXStartRotation = nfshtControllerX.transform.rotation;

                            // Find nfshtControllerY under nfshtControllerX
                            nfshtControllerY = nfshtControllerX.Find("nfshtControllerY");
                            if (nfshtControllerY != null)
                            {
                                logger.Info("nfshtControllerY object found.");
                                // Store initial position and rotation of the stick
                                nfshtControllerYStartPosition = nfshtControllerY.transform.position;
                                nfshtControllerYStartRotation = nfshtControllerY.transform.rotation;

                                // Find nfshtControllerZ under nfshtControllerY
                                nfshtControllerZ = nfshtControllerY.Find("nfshtControllerZ");
                                if (nfshtControllerZ != null)
                                {
                                    logger.Info("nfshtControllerZ object found.");
                                    // Store initial position and rotation of the stick
                                    nfshtControllerZStartPosition = nfshtControllerZ.transform.position;
                                    nfshtControllerZStartRotation = nfshtControllerZ.transform.rotation;
                                }
                                else
                                {
                                    logger.Error("nfshtControllerZ object not found under nfshtControllerY!");
                                }
                            }
                            else
                            {
                                logger.Error("nfshtControllerY object not found under nfshtControllerX!");
                            }
                        }
                        else
                        {
                            logger.Error("nfshtControllerX object not found under nfshtZ!");
                        }
                  
                        // Find cockpit camera under cockpit
                        cockpitCam = nfshtZObject.Find("eyes/cockpitcam")?.gameObject;
                        if (cockpitCam != null)
                        {
                            logger.Info("Cockpitcam object found.");

                            // Store initial position and rotation of cockpit cam
                            cockpitCamStartPosition = cockpitCam.transform.position;
                            cockpitCamStartRotation = cockpitCam.transform.rotation;
                        }
                        else
                        {
                            logger.Error("Cockpitcam object not found under nfshtZ!");
                        }
                    }
                    else
                    {
                        logger.Error("nfshtZ object not found under nfshtY!");
                    }
                }
                else
                {
                    logger.Error("nfshtY object not found under nfshtX!");
                }
            }
            else
            {
                logger.Error("nfshtX object not found!");
            }
            // Find nfshthazard1 object under sharrierX
            nfshthazard1Object = transform.Find("nfshthazard1");
            if (nfshthazard1Object != null)
            {
                logger.Info("nfshthazard1 object found.");
                // Ensure the nfshthazard1 object is initially off
                Renderer renderer = nfshthazard1Object.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.DisableKeyword("_EMISSION");
                }
                else
                {
                    logger.Debug("Renderer component is not found on nfshthazard1 object.");
                }
            }
            else
            {
                logger.Debug("nfshthazard1object not found.");
            }
            // Find nfshthazard2 object under sharrierX
            nfshthazard2Object = transform.Find("nfshthazard2");
            if (nfshthazard2Object != null)
            {
                logger.Info("nfshthazard2 object found.");
                // Ensure the nfshthazard2 object is initially off
                Renderer renderer = nfshthazard2Object.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.DisableKeyword("_EMISSION");
                }
                else
                {
                    logger.Debug("Renderer component is not found on nfshthazard2 object.");
                }
            }
            else
            {
                logger.Debug("nfshthazard2object not found.");
            }
            // Find nfshthazard2 object under sharrierX
            nfshthazard3Object = transform.Find("nfshthazard3");
            if (nfshthazard3Object != null)
            {
                logger.Info("nfshthazard3 object found.");
                // Ensure the nfshthazard3 object is initially off
                Renderer renderer = nfshthazard3Object.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.DisableKeyword("_EMISSION");
                }
                else
                {
                    logger.Debug("Renderer component is not found on nfshthazard3 object.");
                }
            }
            else
            {
                logger.Debug("nfshthazard3object not found.");
            }
            // Find nfshthazard4 object under sharrierX
            nfshthazard4Object = transform.Find("nfshthazard4");
            if (nfshthazard4Object != null)
            {
                logger.Info("nfshthazard4 object found.");
                // Ensure the nfshthazard4 object is initially off
                Renderer renderer = nfshthazard4Object.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.DisableKeyword("_EMISSION");
                }
                else
                {
                    logger.Debug("Renderer component is not found on nfshthazard4 object.");
                }
            }
            else
            {
                logger.Debug("nfshthazard4object not found.");
            }

            // Initialize the hazardRenderers array with the corresponding hazard renderers
            hazardRenderers = new Renderer[]
            {
                nfshthazard1Object.GetComponent<Renderer>(),
                nfshthazard2Object.GetComponent<Renderer>(),
                nfshthazard3Object.GetComponent<Renderer>(),
                nfshthazard4Object.GetComponent<Renderer>()
            };

            // Ensure all hazards are initially turned off
            foreach (var renderer in hazardRenderers)
            {
                if (renderer != null)
                {
                    ToggleEmissive(renderer, false);
                }
            }
            // Gets all Light components in the target object and its children
            // Initialize the strobes array with all strobe lights
            // Gets all Light components in the target object and its children
            Light[] allLights = transform.GetComponentsInChildren<Light>();

            // Log the names of the objects containing the Light components and filter out unwanted lights
            foreach (Light light in allLights)
            {
                if (light.gameObject.name == "strobe1_light")
                {
                    strobe1_light = light;
                    logger.Info("Included Light found in object: " + light.gameObject.name);
                }
                else if (light.gameObject.name == "strobe2_light")
                {
                    strobe2_light = light;
                    logger.Info("Included Light found in object: " + light.gameObject.name);
                }
                else if (light.gameObject.name == "strobe3_light")
                {
                    strobe3_light = light;
                    logger.Info("Included Light found in object: " + light.gameObject.name);
                }
                else if (light.gameObject.name == "strobe4_light")
                {
                    strobe4_light = light;
                    logger.Info("Included Light found in object: " + light.gameObject.name);
                }
                else
                {
                    logger.Info("Excluded Light found in object: " + light.gameObject.name);
                }
            }

            // StartAttractPattern();
            TurnOffAllStrobes();
        }

        void Update()
        {
            bool inputDetected = false;
            bool throttleDetected = false;// Initialize at the beginning of the Update method
           /*
                // Check if the "L" key is pressed
                if (Input.GetKeyDown(KeyCode.L))
                {
                    if (areStrobesOn)
                    {
                        // Strobe lights are currently on, stop the coroutine to turn them off
                        logger.Info("Stopping strobes");
                        StopCoroutine(strobeCoroutine);
                        strobeCoroutine = null;
                        TurnAllOff(); // Turn all emissives off
                        TurnOffAllStrobes();
                    }
                    else
                    {
                        // Strobe lights are currently off, start the coroutine to flash them
                        logger.Info("Starting strobes");
                        strobeCoroutine = StartCoroutine(FlashStrobes());
                    }

                    // Toggle the strobe state
                    areStrobesOn = !areStrobesOn;
                }

            */
                if (GameSystem.ControlledSystem != null && !inFocusMode)
            {
                string controlledSystemGamePathString = GameSystem.ControlledSystem.Game.path != null ? GameSystem.ControlledSystem.Game.path.ToString() : null;
                bool containsString = false;

                foreach (var gameString in compatibleGames)
                {
                    if (controlledSystemGamePathString != null && controlledSystemGamePathString.Contains(gameString))
                    {
                        containsString = true;
                        break;
                    }
                }

                if (containsString)
                {
                    StartFocusMode();
                }
            }

            if (GameSystem.ControlledSystem == null && inFocusMode)
            {
                EndFocusMode();
            }
            if (inFocusMode)
            {
                HandleTransformAdjustment();
                HandleInput(ref inputDetected, ref throttleDetected); // Pass by reference
            }
        }

        void StartFocusMode()
        {
            string controlledSystemGamePathString = GameSystem.ControlledSystem.Game.path != null ? GameSystem.ControlledSystem.Game.path.ToString() : null;
            logger.Info($"Controlled System Game path String: {controlledSystemGamePathString}");
            logger.Info("Compatible Rom Dectected, Need For Speed HEAT Takedown!...");
            logger.Info("Need For Speed HEAT Takedown Motion Sim starting...Take them all down!");
            logger.Info("Watch out for cops!...");
            cockpitCam.transform.position = cockpitCamStartPosition; // new hotness
            cockpitCam.transform.rotation = cockpitCamStartRotation; // new hotness
            // Set objects as children of cockpit cam for focus mode

            if (cockpitCam != null)
            {
                if (playerCamera != null)
                {
                    // Store initial position, rotation, and scale of PlayerCamera
                    playerCameraStartPosition = playerCamera.transform.position;
                    playerCameraStartRotation = playerCamera.transform.rotation;
                    playerCameraStartScale = playerCamera.transform.localScale; // Store initial scale
                    SaveOriginalParent(playerCamera);  // Save original parent of PlayerCamera

                    // Set PlayerCamera as child of cockpit cam and maintain scale
                    playerCamera.transform.SetParent(cockpitCam.transform, false);
                    playerCamera.transform.localScale = playerCameraStartScale;  // Reapply initial scale
                    playerCamera.transform.localRotation = Quaternion.identity;
                    logger.Info("PlayerCamera set as child of CockpitCam.");
                }

                if (playerVRSetup != null)
                {
                    // Store initial position, rotation, and scale of PlayerVRSetup
                    playerVRSetupStartPosition = playerVRSetup.transform.position;
                    playerVRSetupStartRotation = playerVRSetup.transform.rotation;
                    playerVRSetupStartScale = playerVRSetup.transform.localScale; // Store initial scale
                    SaveOriginalParent(playerVRSetup);  // Save original parent of PlayerVRSetup

                    // Set PlayerVRSetup as child of cockpit cam and maintain scale
                    playerVRSetup.transform.SetParent(cockpitCam.transform, false);
                    playerVRSetup.transform.localScale = playerVRSetupStartScale;
                    playerVRSetup.transform.localRotation = Quaternion.identity;
                    logger.Info("PlayerVRSetup.PlayerRig set as child of CockpitCam.");
                }
            }
            else
            {
                logger.Error("CockpitCam object not found under gforceZ!");
            }
            inFocusMode = true;  // Set focus mode flag
        }

        void EndFocusMode()
        {
            logger.Info("Exiting Focus Mode...");
            // Restore original parents of objects
            RestoreOriginalParent(playerCamera, "PlayerCamera");
            RestoreOriginalParent(playerVRSetup, "PlayerVRSetup.PlayerRig");
  
            // Reset nfshtX to initial positions and rotations
            if (nfshtXObject != null)
            {
                nfshtXObject.position = nfshtXStartPosition;
                nfshtXObject.rotation = nfshtXStartRotation;
            }

            // Reset nfshtY object to initial position and rotation
            if (nfshtYObject != null)
            {
                nfshtYObject.position = nfshtYStartPosition;
                nfshtYObject.rotation = nfshtYStartRotation;
            }
            // Reset nfshtZ object to initial position and rotation
            if (nfshtZObject != null)
            {
                nfshtZObject.position = nfshtZStartPosition;
                nfshtZObject.rotation = nfshtZStartRotation;
            }

            // Reset cockpit cam to initial position and rotation
            if (cockpitCam != null)
            {
                cockpitCam.transform.position = cockpitCamStartPosition;
                cockpitCam.transform.rotation = cockpitCamStartRotation;
            }
            logger.Info("Resetting Positions");
            if (strobeCoroutine != null)
            {
                StopCoroutine(strobeCoroutine);
                strobeCoroutine = null;
                isnfshtFlashing = false;
            }
            TurnOffAllStrobes();
            ResetPositions();
            inFocusMode = false;  // Clear focus mode flag
        }

        void HandleInput(ref bool inputDetected, ref bool throttleDetected) // Pass by reference
        {
            if (!inFocusMode) return;

            Vector2 primaryThumbstick = Vector2.zero;
            Vector2 secondaryThumbstick = Vector2.zero;

            // VR controller input
            if (PlayerVRSetup.VRMode == PlayerVRSetup.VRSDK.Oculus)
            {
                primaryThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                secondaryThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
                Vector2 ovrPrimaryThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                Vector2 ovrSecondaryThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
                float ovrPrimaryIndexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
                float ovrSecondaryIndexTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
                float ovrPrimaryHandTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
                float ovrSecondaryHandTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);

                /*
                // Check if the A button on the right controller is pressed
                if (OVRInput.GetDown(OVRInput.Button.One))
                {
                    Debug.Log("OVR A button pressed");
                }

                // Check if the B button on the right controller is pressed
                if (OVRInput.GetDown(OVRInput.Button.Two))
                {
                    Debug.Log("OVR B button pressed");
                }

                // Check if the X button on the left controller is pressed
                if (OVRInput.GetDown(OVRInput.Button.Three))
                {
                    Debug.Log("OVR X button pressed");
                }

                // Check if the Y button on the left controller is pressed
                if (OVRInput.GetDown(OVRInput.Button.Four))
                {
                    Debug.Log("OVR Y button pressed");
                }

                // Check if the primary index trigger on the right controller is pressed
                if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
                {
                    Debug.Log("OVR Primary index trigger pressed");
                }

                // Check if the secondary index trigger on the left controller is pressed
                if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
                {
                    Debug.Log("OVR Secondary index trigger pressed");
                }

                // Check if the primary hand trigger on the right controller is pressed
                if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
                {
                    Debug.Log("OVR Primary hand trigger pressed");
                }

                // Check if the secondary hand trigger on the left controller is pressed
                if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
                {
                    Debug.Log("OVR Secondary hand trigger pressed");
                }

                // Check if the primary thumbstick is pressed
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
                {
                    Debug.Log("OVR Primary thumbstick pressed");
                }

                // Check if the secondary thumbstick is pressed
                if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
                {
                    Debug.Log("OVR Secondary thumbstick pressed");
                }
                */
            
            }
            else if (PlayerVRSetup.VRMode == PlayerVRSetup.VRSDK.OpenVR)
            {
                var leftController = SteamVRInput.GetController(HandType.Left);
                var rightController = SteamVRInput.GetController(HandType.Right);
                primaryThumbstick = leftController.GetAxis();
                secondaryThumbstick = rightController.GetAxis();
            }

            // Ximput controller input
            if (XInput.IsConnected)
            {
                primaryThumbstick = XInput.Get(XInput.Axis.LThumbstick);
                Vector2 xboxPrimaryThumbstick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                Vector2 xboxSecondaryThumbstick = new Vector2(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical"));

                // Combine VR and Xbox inputs
                primaryThumbstick += xboxPrimaryThumbstick;
                secondaryThumbstick += xboxSecondaryThumbstick;

                // Get the trigger axis values
                // Detect input from Xbox triggers

                if (XInput.Get(XInput.Button.RIndexTrigger))
                {
                    float rotateX = keyboardVelocityX * Time.deltaTime;

                    if (currentRotationX - rotateX > -rotationLimitX)
                    {
                        nfshtXObject.Rotate(rotateX, 0, 0);
                        currentRotationX -= rotateX;
                        throttleDetected = true;
                    }
                }
                if (XInput.Get(XInput.Button.LIndexTrigger))
                {
                    float rotateX = vrVelocity * Time.deltaTime;

                    if (currentRotationX + rotateX < rotationLimitX)
                    {
                        nfshtXObject.Rotate(-rotateX, 0, 0);
                        currentRotationX += rotateX;
                        throttleDetected = true;
                    }
                }
                // LeftTrigger           
                if (XInput.GetDown(XInput.Button.LIndexTrigger))
                {
                    throttleDetected = true;
                }
                // Reset position on button release
                if (XInput.GetUp(XInput.Button.LIndexTrigger))
                {
                    throttleDetected = true;
                }
                // Thunbstick button pressed
                if (XInput.GetDown(XInput.Button.LThumbstick))
                {
                    if (!isnfshtFlashing)
                    {
                        // Start the flashing if not already flashing
                        strobeCoroutine = StartCoroutine(FlashStrobes());
                        isnfshtFlashing = true;
                    }
                    else
                    {
                        // Stop the flashing if it's currently active
                        StopCoroutine(strobeCoroutine);
                        TurnOffAllStrobes();
                        strobeCoroutine = null;
                        isnfshtFlashing = false;
                    }
                    inputDetected = true;
                }
                // Handle RB button press for plunger position
                if (XInput.GetDown(XInput.Button.RShoulder) || Input.GetKeyDown(KeyCode.JoystickButton5))
                {

                    inputDetected = true;
                }

                // Reset position on RB button release
                if (XInput.GetUp(XInput.Button.RShoulder) || Input.GetKeyUp(KeyCode.JoystickButton5))
                {
                    inputDetected = true;
                }

                // Handle LB button press for plunger position
                if (XInput.GetDown(XInput.Button.LShoulder) || Input.GetKeyDown(KeyCode.JoystickButton4))
                {
                    inputDetected = true;
                }

                // Reset position on LB button release
                if (XInput.GetUp(XInput.Button.LShoulder) || Input.GetKeyUp(KeyCode.JoystickButton4))
                {
                    inputDetected = true;
                }
            }
            /*
                 // Fire1
                 if (Input.GetButtonDown("Fire1"))
                 {
                     inputDetected = true;
                 }

                 // Reset position on button release
                 if (Input.GetButtonUp("Fire1"))
                 {
                     inputDetected = true;
                 }

                 // Fire2
                 if (Input.GetButtonDown("Fire2"))
                 {
                     // Set lights to bright
                     ToggleBrightness1(true);
                     ToggleBrightness2(true);
                     ToggleBrakeEmissive(true);
                     ToggleLight1(true);
                     ToggleLight2(true);
                     inputDetected = true;
                 }

                 // Reset position on button release
                 if (Input.GetButtonUp("Fire2"))
                 {
                     ToggleBrakeEmissive(false);
                     ToggleBrightness1(false);
                     ToggleBrightness2(false);
                     ToggleLight1(false);
                     ToggleLight2(false);
                     ToggleBrightness(false);
                     inputDetected = true;
                 }

                 // Fire3
                 if (Input.GetButtonDown("Fire3"))
                 {
                     inputDetected = true;
                 }

                 // Reset position on button release
                 if (Input.GetButtonUp("Fire3"))
                 {
                     inputDetected = true;
                 }

                 // Jump
                 if (Input.GetButtonDown("Jump"))
                 {
                     inputDetected = true;
                 }

                 // Reset position on button release
                 if (Input.GetButtonUp("Jump"))
                 {

                     inputDetected = true;
                 }
               
            // Handle X rotation for nfshtYObject and nfshtControllerX (Down Arrow or primaryThumbstick.y > 0)
            // Thumbstick direction: down

            if ((Input.GetKey(KeyCode.DownArrow) || primaryThumbstick.y > 0))
            {
                if (currentRotationX > -rotationLimitX)
                {
                    float rotateX = (Input.GetKey(KeyCode.DownArrow) ? keyboardVelocityX : primaryThumbstick.y * vrVelocity) * Time.deltaTime;
                    nfshtXObject.Rotate(rotateX, 0, 0);
                    currentRotationX -= rotateX;
                    inputDetected = true;
                }
                if (currentControllerRotationX > -controllerrotationLimitX)
                {
                    float controllerRotateX = (Input.GetKey(KeyCode.DownArrow) ? keyboardControllerVelocityX : primaryThumbstick.y * vrControllerVelocity) * Time.deltaTime;
                    nfshtControllerX.Rotate(controllerRotateX, 0, 0);
                    currentControllerRotationX -= controllerRotateX;
                    inputDetected = true;
                }
            }

            // Handle X rotation for nfshtYObject and nfshtControllerX (Up Arrow or primaryThumbstick.y < 0)
            // Thumbstick direction: up
            if ((Input.GetKey(KeyCode.UpArrow) || primaryThumbstick.y < 0))
            {
                if (currentRotationX < rotationLimitX)
                {
                    float rotateX = (Input.GetKey(KeyCode.UpArrow) ? keyboardVelocityX : -primaryThumbstick.y * vrVelocity) * Time.deltaTime;
                    nfshtXObject.Rotate(-rotateX, 0, 0);
                    currentRotationX += rotateX;
                    inputDetected = true;
                }
                if (currentControllerRotationX < controllerrotationLimitX)
                {
                    float controllerRotateX = (Input.GetKey(KeyCode.UpArrow) ? keyboardControllerVelocityX : -primaryThumbstick.y * vrControllerVelocity) * Time.deltaTime;
                    nfshtControllerX.Rotate(-controllerRotateX, 0, 0);
                    currentControllerRotationX += controllerRotateX;
                    inputDetected = true;
                }
            }
              */
            // Handle Z rotation for nfshtXObject and nfshtControllerZ (Down Arrow or primaryThumbstick.y < 0)
            // Thumbstick direction: Left
            if ((Input.GetKey(KeyCode.LeftArrow) || primaryThumbstick.x < 0))
            {
                if (currentRotationZ > -rotationLimitZ)
                {
                    float rotateZ = (Input.GetKey(KeyCode.LeftArrow) ? keyboardVelocityZ : -primaryThumbstick.x * vrVelocity) * Time.deltaTime;
                    nfshtZObject.Rotate(0, 0, rotateZ);
                    currentRotationZ -= rotateZ;
                    inputDetected = true;
                }
                if (currentControllerRotationZ > -controllerrotationLimitZ)
                {
                    float controllerRotateZ = (Input.GetKey(KeyCode.LeftArrow) ? keyboardControllerVelocityZ : -primaryThumbstick.x * vrControllerVelocity) * Time.deltaTime;
                    nfshtControllerZ.Rotate(0, 0, controllerRotateZ);
                    currentControllerRotationZ -= controllerRotateZ;
                    inputDetected = true;
                }
            }

            // Handle Z rotation for nfshtXObject and nfshtControllerZ (Up Arrow or primaryThumbstick.y > 0)
            // Thumbstick direction: right
            if ((Input.GetKey(KeyCode.RightArrow) || primaryThumbstick.x > 0))
            {
                if (currentRotationZ < rotationLimitZ)
                {
                    float rotateZ = (Input.GetKey(KeyCode.RightArrow) ? keyboardVelocityZ : primaryThumbstick.x * vrVelocity) * Time.deltaTime;
                    nfshtZObject.Rotate(0, 0, -rotateZ);
                    currentRotationZ += rotateZ;
                    inputDetected = true;
                }
                if (currentControllerRotationZ < controllerrotationLimitZ)
                {
                    float controllerRotateZ = (Input.GetKey(KeyCode.RightArrow) ? keyboardControllerVelocityZ : primaryThumbstick.x * vrControllerVelocity) * Time.deltaTime;
                    nfshtControllerZ.Rotate(0, 0, -controllerRotateZ);
                    currentControllerRotationZ += controllerRotateZ;
                    inputDetected = true;
                }
            }

            // Handle left rotation (Thumbstick left)
            if (primaryThumbstick.x < 0 && currentRotationY < rotationLimitY) // Note the change in condition
            {
                float rotateY = primaryThumbstick.x * vrVelocity * Time.deltaTime;
                nfshtYObject.Rotate(0, rotateY, 0);  // Rotate Y in the opposite direction
                currentRotationY -= rotateY;  // Update current rotation (subtracting because the direction is swapped)
                inputDetected = true;
            }

            // Handle right rotation (Thumbstick right)
            if (primaryThumbstick.x > 0 && currentRotationY > -rotationLimitY) // Note the change in condition
            {
                float rotateY = primaryThumbstick.x * vrVelocity * Time.deltaTime;
                nfshtYObject.Rotate(0, rotateY, 0);  // Rotate Y in the opposite direction
                currentRotationY -= rotateY;  // Update current rotation (subtracting because the direction is swapped)
                inputDetected = true;
            }

            // Center the rotation if no input is detected (i think this is redundant)

            if (!inputDetected)
            {
                CenterRotation();
            }
            if (!throttleDetected)
            {
                CenterThrottle();
            }
        }

        void ResetPositions()
        {
            cockpitCam.transform.position = cockpitCamStartPosition;
            cockpitCam.transform.rotation = cockpitCamStartRotation;
            playerVRSetup.transform.position = playerVRSetupStartPosition;
            playerVRSetup.transform.rotation = playerVRSetupStartRotation;
            playerVRSetup.transform.localScale = playerVRSetupStartScale;
            //playerVRSetup.transform.localScale = new Vector3(1f, 1f, 1f);
            playerCamera.transform.position = playerCameraStartPosition;
            playerCamera.transform.rotation = playerCameraStartRotation;
            //playerCamera.transform.localScale = new Vector3(1f, 1f, 1f);
            playerCamera.transform.localScale = playerCameraStartScale;
            // Reset rotation allowances and current rotation values
            currentRotationX = 0f;
            currentRotationY = 0f;
            currentRotationZ = 0f;
        }

        void CenterThrottle()
        {
            // Center X-axis
            if (currentRotationX > 0)
            {
                float unrotateX = Mathf.Min(centeringVelocityX * Time.deltaTime, currentRotationX);
                nfshtXObject.Rotate(unrotateX, 0, 0);
                currentRotationX -= unrotateX;
            }
            else if (currentRotationX< 0)
            {
                float unrotateX = Mathf.Min(centeringVelocityX * Time.deltaTime, -currentRotationX);
                nfshtXObject.Rotate(-unrotateX, 0, 0);
                currentRotationX += unrotateX;
            }
        }


        void CenterRotation()
        {
            // Center Y-axis
            if (currentRotationY > 0)
            {
                float unrotateY = Mathf.Min(centeringVelocityY * Time.deltaTime, currentRotationY);
                nfshtYObject.Rotate(0, unrotateY, 0);
                currentRotationY -= unrotateY;
            }
            else if (currentRotationY < 0)
            {
                float unrotateY = Mathf.Min(centeringVelocityY * Time.deltaTime, -currentRotationY);
                nfshtYObject.Rotate(0, -unrotateY, 0);
                currentRotationY += unrotateY;
            }

            // Center Z-axis
            if (currentRotationZ > 0)
            {
                float unrotateZ = Mathf.Min(centeringVelocityZ * Time.deltaTime, currentRotationZ);
                nfshtZObject.Rotate(0, 0, unrotateZ);
                currentRotationZ -= unrotateZ;
            }
            else if (currentRotationZ < 0)
            {
                float unrotateZ = Mathf.Min(centeringVelocityZ * Time.deltaTime, -currentRotationZ);
                nfshtZObject.Rotate(0, 0, -unrotateZ);
                currentRotationZ += unrotateZ;
            }
            //Centering for contoller

            // Center Y-axis Controller rotation
            if (currentControllerRotationY > 0)
            {
                float unrotateY = Mathf.Min(centeringControllerVelocityY * Time.deltaTime, currentControllerRotationY);
                nfshtControllerY.Rotate(0, unrotateY, 0);   // Rotating to reduce the rotation
                currentControllerRotationY -= unrotateY;    // Reducing the positive rotation
            }
            else if (currentControllerRotationY < 0)
            {
                float unrotateY = Mathf.Min(centeringControllerVelocityY * Time.deltaTime, -currentControllerRotationY);
                nfshtControllerY.Rotate(0, -unrotateY, 0);  // Rotating to reduce the rotation
                currentControllerRotationY += unrotateY;    // Reducing the negative rotation
            }

            // Center X-Axis Controller rotation
            if (currentControllerRotationX > 0)
            {
                float unrotateX = Mathf.Min(centeringControllerVelocityX * Time.deltaTime, currentControllerRotationX);
                nfshtControllerX.Rotate(unrotateX, 0, 0);   // Rotating to reduce the rotation
                currentControllerRotationX -= unrotateX;    // Reducing the positive rotation
            }
            else if (currentControllerRotationX < 0)
            {
                float unrotateX = Mathf.Min(centeringControllerVelocityX * Time.deltaTime, -currentControllerRotationX);
                nfshtControllerX.Rotate(-unrotateX, 0, 0);   // Rotating to reduce the rotation
                currentControllerRotationX += unrotateX;    // Reducing the positive rotation
            }

            // Center Z-axis Controller rotation
            if (currentControllerRotationZ > 0)
            {
                float unrotateZ = Mathf.Min(centeringControllerVelocityZ * Time.deltaTime, currentControllerRotationZ);
                nfshtControllerZ.Rotate(0, 0, unrotateZ);   // Rotating to reduce the rotation
                currentControllerRotationZ -= unrotateZ;    // Reducing the positive rotation
            }
            else if (currentControllerRotationZ < 0)
            {
                float unrotateZ = Mathf.Min(centeringControllerVelocityZ * Time.deltaTime, -currentControllerRotationZ);
                nfshtControllerZ.Rotate(0, 0, -unrotateZ);   // Rotating to reduce the rotation
                currentControllerRotationZ += unrotateZ;    // Reducing the positive rotation
            }
        }

        void HandleTransformAdjustment()
        {
            // Handle position adjustments
            if (Input.GetKey(KeyCode.Home))
            {
                // Move forward
                cockpitCam.transform.position += cockpitCam.transform.forward * adjustSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.End))
            {
                // Move backward
                cockpitCam.transform.position -= cockpitCam.transform.forward * adjustSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.PageUp))
            {
                // Move up
                cockpitCam.transform.position += cockpitCam.transform.up * adjustSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.Insert))
            {
                // Move down
                cockpitCam.transform.position -= cockpitCam.transform.up * adjustSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.Delete))
            {
                // Move left
                cockpitCam.transform.position -= cockpitCam.transform.right * adjustSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.PageDown))
            {
                // Move right
                cockpitCam.transform.position += cockpitCam.transform.right * adjustSpeed * Time.deltaTime;
            }

            // Handle rotation with Backspace key
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                cockpitCam.transform.Rotate(0, 90, 0);
            }

            // Save the new position and rotation
            cockpitCamStartPosition = cockpitCam.transform.position;
            cockpitCamStartRotation = cockpitCam.transform.rotation;
        }

        // Check if object is found and log appropriate message
        void CheckObject(GameObject obj, string name)
        {
            if (obj == null)
            {
                logger.Error($"{name} not found!");
            }
            else
            {
                logger.Info($"{name} found.");
            }
        }

        // Save original parent of object in dictionary
        void SaveOriginalParent(GameObject obj)
        {
            if (obj != null && !originalParents.ContainsKey(obj))
            {
                originalParents[obj] = obj.transform.parent;
            }
        }

        // Restore original parent of object and log appropriate message
        void RestoreOriginalParent(GameObject obj, string name)
        {
            if (obj != null && originalParents.ContainsKey(obj))
            {
                obj.transform.SetParent(originalParents[obj]);
                logger.Info($"{name} restored to original parent.");
            }
        }

        // Unset parent of object and log appropriate message
        void UnsetParentObject(GameObject obj, string name)
        {
            if (obj != null)
            {
                obj.transform.SetParent(null);
                logger.Info($"{name} unset from parent.");
            }
        }

        IEnumerator FlashStrobes()
        {
            while (true)
            {
                // Randomly select a pair (0 for 1 & 3, 1 for 2 & 4)
                int pairIndex = Random.Range(0, 2);

                // Get the strobes for the selected pair
                Light strobeLight1 = null;
                Light strobeLight2 = null;

                switch (pairIndex)
                {
                    case 0: // Pair 1 and 3
                        strobeLight1 = strobe1_light;
                        strobeLight2 = strobe3_light;
                        break;
                    case 1: // Pair 2 and 4
                        strobeLight1 = strobe2_light;
                        strobeLight2 = strobe4_light;
                        break;
                }

                // Turn on both strobes in the selected pair
                ToggleStrobeLight(strobeLight1, true);
                ToggleStrobeLight(strobeLight2, true);

                // Wait for the flash duration
                yield return new WaitForSeconds(flashDuration);

                // Turn off both strobes
                ToggleStrobeLight(strobeLight1, false);
                ToggleStrobeLight(strobeLight2, false);

                // Wait for the next flash interval
                yield return new WaitForSeconds(flashInterval - flashDuration);
            }
        }

        void ToggleStrobeLight(Light strobeLight, bool isActive)
        {
            if (strobeLight != null)
            {
                strobeLight.enabled = isActive;
                // logger.Info($"{strobeLight.name} light turned {(isActive ? "on" : "off")}.");
            }
            else
            {
                logger.Debug($"{strobeLight?.name} light component is not found.");
            }
        }
        void TurnOffAllStrobes()
        {
            ToggleStrobeLight(strobe1_light, false);
            ToggleStrobeLight(strobe2_light, false);
            ToggleStrobeLight(strobe3_light, false);
            ToggleStrobeLight(strobe4_light, false);
        }

        void ToggleEmissive(Renderer renderer, bool isOn)
        {
            if (renderer != null)
            {
                Material[] materials = renderer.materials; // Get all materials
                foreach (Material mat in materials)
                {
                    if (mat != null)
                    {
                        if (isOn)
                        {
                            mat.EnableKeyword("_EMISSION");
                        }
                        else
                        {
                            mat.DisableKeyword("_EMISSION");
                        }
                    }
                }
            }
            else
            {
                logger.Debug("Renderer component not found on hazard object.");
            }
        }

        // Method to log missing objects
        void LogMissingObject(Renderer[] emissiveObjects, string arrayName)
        {
            for (int i = 0; i < emissiveObjects.Length; i++)
            {
                if (emissiveObjects[i] == null)
                {
                    logger.Debug($"{arrayName} object at index {i} not found under nfshtZ.");
                }
            }
        }

        // Method to check if VR input is active
        bool VRInputActive()
        {
            // Assuming you have methods to check VR input
            return GetPrimaryThumbstick() != Vector2.zero || GetSecondaryThumbstick() != Vector2.zero;
        }

        // Placeholder methods to get VR thumbstick input (to be implemented with actual VR input handling)
        Vector2 GetPrimaryThumbstick()
        {
            // Implement VR primary thumbstick input handling here
            return Vector2.zero;
        }

        Vector2 GetSecondaryThumbstick()
        {
            // Implement VR secondary thumbstick input handling here
            return Vector2.zero;
        }
    }
}
