﻿using UnityEngine;
using UnityEngine.XR;
using WIGU;
using System.Collections.Generic;
using EmuVR.InputManager;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;

namespace WIGUx.Modules.sr3MotionSim
{
    public class sr3MotionSimController : MonoBehaviour
    {
        static IWiguLogger logger = ServiceProvider.Instance.GetService<IWiguLogger>();

        private readonly float keyboardVelocityX = 1f;  // Velocity for keyboard input
        private readonly float keyboardVelocityY = 2f;  // Velocity for keyboard input
        private readonly float keyboardVelocityZ = 2f;  // Velocity for keyboard input
        private readonly float vrVelocity = 30f;        // Velocity for VR controller input

        private readonly float centeringVelocityX = 15f;  // Velocity for centering rotation
        private readonly float centeringVelocityY = 30f;  // Velocity for centering rotation
        private readonly float centeringVelocityZ = 30f;  // Velocity for centering rotation

        private float adjustSpeed = 1.0f;  // Adjust this adjustment speed as needed a lower number will lead to smaller adustments

        private float rotationLimitX = 2f;  // Rotation limit for X-axis
        private float rotationLimitY = 6f;  // Rotation limit for Y-axis
        private float rotationLimitZ = 6f;  // Rotation limit for Z-axis

        private float currentRotationX = 0f;  // Current rotation for X-axis
        private float currentRotationY = 0f;  // Current rotation for Y-axis
        private float currentRotationZ = 0f;  // Current rotation for Z-axis

        private Transform sr3XObject; // Reference to the main X object
        private Transform sr3YObject; // Reference to the main Y object
        private Transform sr3ZObject; // Reference to the main Z object
        private GameObject cockpitCam;    // Reference to the cockpit camera

        // Initial positions and rotations for resetting
        private Vector3 sr3XStartPosition;
        private Quaternion sr3XStartRotation;
        private Vector3 sr3YStartPosition;
        private Quaternion sr3YStartRotation;
        private Vector3 sr3ZStartPosition;
        private Quaternion sr3ZStartRotation;
        private Vector3 cockpitCamStartPosition;
        private Quaternion cockpitCamStartRotation;

        // Controller animation 
        // Speeds for the animation of the in game flight stick or wheel
        private readonly float keyboardControllerVelocityX = 400f;  // Velocity for keyboard input
        private readonly float keyboardControllerVelocityY = 400f;  // Velocity for keyboard input
        private readonly float keyboardControllerVelocityZ = 400f;  // Velocity for keyboard input
        private readonly float vrControllerVelocity = 350f;        // Velocity for VR controller input

        private float controllerrotationLimitX = 0f;  // Rotation limit for X-axis (stick or wheel)
        private float controllerrotationLimitY = 0f;  // Rotation limit for Y-axis (stick or wheel)
        private float controllerrotationLimitZ = 100f;  // Rotation limit for Z-axis (stick or wheel)

        private float currentControllerRotationX = 0f;  // Current rotation for X-axis (stick or wheel)
        private float currentControllerRotationY = 0f;  // Current rotation for Y-axis (stick or wheel)
        private float currentControllerRotationZ = 0f;  // Current rotation for Z-axis (stick or wheel)

        private readonly float centeringControllerVelocityX = 400f;  // Velocity for centering rotation (stick or wheel)
        private readonly float centeringControllerVelocityY = 400f;  // Velocity for centering rotation (stick or wheel)
        private readonly float centeringControllerVelocityZ = 400f;  // Velocity for centering rotation (stick or wheel)

        private Transform sr3ControllerX; // Reference to the main animated controller (wheel)
        private Vector3 sr3ControllerXStartPosition; // Initial controller positions and rotations for resetting
        private Quaternion sr3ControllerXStartRotation; // Initial controlller positions and rotations for resetting
        private Transform sr3ControllerY; // Reference to the main animated controller (wheel)
        private Vector3 sr3ControllerYStartPosition; // Initial controller positions and rotations for resetting
        private Quaternion sr3ControllerYStartRotation; // Initial controlller positions and rotations for resetting
        private Transform sr3ControllerZ; // Reference to the main animated controller (wheel)
        private Vector3 sr3ControllerZStartPosition; // Initial controller positions and rotations for resetting
        private Quaternion sr3ControllerZStartRotation; // Initial controlller positions and rotations for resetting

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
        private Transform sr3taillight1Object;
        private Transform sr3taillight2Object;
        private Transform sr3hazardObject;
        private Transform sr3hazardlObject;
        private Transform sr3hazardrObject;
        public float brightIntensity = 5.0f; // Set the brightness intensity level
        public float dimIntensity = 1.0f;    // Set the dim intensity level
        public string fire1Button = "Fire1"; // Name of the fire button
        public string fire2Button = "Fire2"; // Name of the fire button 
        public string fire3Button = "Fire3"; // Name of the fire button 
        public string JumpButton = "Jump"; // Name of the fire button 
        public Light sr3brakelight1;
        public Light sr3brakelight2;
        public Light strobe1_light;
        public Light strobe2_light;
        public Light strobe3_light;
        public Light strobe4_light;
        private float flashDuration = 0.01f;
        private float flashInterval = 0.05f;
        private Renderer[] hazzardEmissiveObjects;
        private Renderer[] leftEmissiveObjects;
        private Renderer[] rightEmissiveObjects;
        public float frontFlashDuration = 0.7f;
        public float frontFlashDelay = 0.7f;
        public float sideFlashDuration = 0.5f;
        public float sideFlashDelay = 0.5f;
        public float frontDangerDuration = 0.7f;
        public float frontDangerDelay = 0.7f;
        public float sideDangerDuration = 0.55f;
        public float sideDangerDelay = 0.55f;
        private Coroutine frontCoroutine;
        private Coroutine leftCoroutine;
        private Coroutine rightCoroutine;
        public float lightDuration = 0.35f; // Duration during which the lights will be on
        private bool inFocusMode = false;  // Flag to track focus mode state
        private bool areStrobesOn = false; // track strobe lights
        private Coroutine hazardsCoroutine; // Coroutine variable to control the strobe flashing
        //array of lights
        private Light[] lights;

        // Array of strobe lights
        private List<Light> strobeLights = new List<Light>();

        private readonly string[] compatibleGames = { "SEGA Rally 3" };

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
            sr3XObject = transform.Find("sr3X");
            if (sr3XObject != null)
            {
                logger.Info("sr3X object found.");
                sr3XStartPosition = sr3XObject.position;
                sr3XStartRotation = sr3XObject.rotation;

                // Find sr3Y object under sr3X
                sr3YObject = sr3XObject.Find("sr3Y");
                if (sr3YObject != null)
                {
                    logger.Info("sr3Y object found.");
                    sr3YStartPosition = sr3YObject.position;
                    sr3YStartRotation = sr3YObject.rotation;

                    // Find sr3Z object under sr3Y
                    sr3ZObject = sr3YObject.Find("sr3Z");
                    if (sr3ZObject != null)
                    {
                        logger.Info("sr3Z object found.");
                        sr3ZStartPosition = sr3ZObject.position;
                        sr3ZStartRotation = sr3ZObject.rotation;

                        // Find sr3hazard object under sr3Z
                        sr3hazardObject = sr3ZObject.Find("sr3hazard");
                        if (sr3hazardObject != null)
                        {
                            logger.Info("sr3hazard object found.");

                            // Ensure the sr3hazard object's emission is initially off for all materials
                            Renderer renderer = sr3hazardObject.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                Material[] materials = renderer.materials; // Get all materials
                                foreach (Material mat in materials)
                                {
                                    if (mat != null)
                                    {
                                        mat.DisableKeyword("_EMISSION");
                                    }
                                }
                            }
                            else
                            {
                                logger.Debug("Renderer component is not found on sr3hazard object.");
                            }
                        }
                        else
                        {
                            logger.Debug("sr3hazard object not found under sr3Z.");
                        }


                        // Find sr3ControllerX under sr3Z
                        sr3ControllerX = sr3ZObject.Find("sr3ControllerX");
                        if (sr3ControllerX != null)
                        {
                            logger.Info("sr3ControllerX object found.");
                            // Store initial position and rotation of the stick
                            sr3ControllerXStartPosition = sr3ControllerX.transform.position; // these could cause the controller to mess up
                            sr3ControllerXStartRotation = sr3ControllerX.transform.rotation;

                            // Find sr3ControllerY under sr3ControllerX
                            sr3ControllerY = sr3ControllerX.Find("sr3ControllerY");
                            if (sr3ControllerY != null)
                            {
                                logger.Info("sr3ControllerY object found.");
                                // Store initial position and rotation of the stick
                                sr3ControllerYStartPosition = sr3ControllerY.transform.position;
                                sr3ControllerYStartRotation = sr3ControllerY.transform.rotation;

                                // Find sr3ControllerZ under sr3ControllerY
                                sr3ControllerZ = sr3ControllerY.Find("sr3ControllerZ");
                                if (sr3ControllerZ != null)
                                {
                                    logger.Info("sr3ControllerZ object found.");
                                    // Store initial position and rotation of the stick
                                    sr3ControllerZStartPosition = sr3ControllerZ.transform.position;
                                    sr3ControllerZStartRotation = sr3ControllerZ.transform.rotation;
                                }
                                else
                                {
                                    logger.Error("sr3ControllerZ object not found under sr3ControllerY!");
                                }
                            }
                            else
                            {
                                logger.Error("sr3ControllerY object not found under sr3ControllerX!");
                            }
                        }
                        else
                        {
                            logger.Error("sr3ControllerX object not found under sr3Z!");
                        }
                        // Gets all Light components in the target object and its children
                        Light[] allLights = sr3ZObject.GetComponentsInChildren<Light>();

                        // Log the names of the objects containing the Light components and filter out unwanted lights
                        foreach (Light light in allLights)
                        {
                            if (light.gameObject.name == "sr3brakelight1")
                            {
                                sr3brakelight1 = light;
                                logger.Info("Included Light found in object: " + light.gameObject.name);
                            }
                            else if (light.gameObject.name == "sr3brakelight2")
                            {
                                sr3brakelight2 = light;
                                logger.Info("Included Light found in object: " + light.gameObject.name);
                            }
                            else if (light.gameObject.name == "strobe1_light")
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
                        // Find cockpit camera under cockpit
                        cockpitCam = sr3ZObject.Find("eyes/cockpitcam")?.gameObject;
                        if (cockpitCam != null)
                        {
                            logger.Info("Cockpitcam object found.");

                            // Store initial position and rotation of cockpit cam
                            cockpitCamStartPosition = cockpitCam.transform.position;
                            cockpitCamStartRotation = cockpitCam.transform.rotation;
                        }
                        else
                        {
                            logger.Error("Cockpitcam object not found under sr3Z!");
                        }
                    }
                    else
                    {
                        logger.Error("sr3Z object not found under sr3Y!");
                    }
                }
                else
                {
                    logger.Error("sr3Y object not found under sr3X!");
                }
            }
            else
            {
                logger.Error("sr3X object not found!");
            }
            hazzardEmissiveObjects = new Renderer[1];
            //leftEmissiveObjects = new Renderer[1];
            //rightEmissiveObjects = new Renderer[1];

            hazzardEmissiveObjects[0] = sr3ZObject.Find("sr3hazard")?.GetComponent<Renderer>();
            //hazzardEmissiveObjects[1] = sr3ZObject.Find("sr3hazardl")?.GetComponent<Renderer>();
            // hazzardEmissiveObjects[2] = sr3ZObject.Find("sr3hazardr")?.GetComponent<Renderer>();


            //leftEmissiveObjects[0] = sr3ZObject.Find("left1")?.GetComponent<Renderer>();
            //leftEmissiveObjects[1] = sr3ZObject.Find("left2")?.GetComponent<Renderer>();
            // leftEmissiveObjects[2] = sr3ZObject.Find("left3")?.GetComponent<Renderer>();

            //rightEmissiveObjects[0] = sr3ZObject.Find("right1")?.GetComponent<Renderer>();
            //rightEmissiveObjects[1] = sr3ZObject.Find("right2")?.GetComponent<Renderer>();
            //rightEmissiveObjects[2] = sr3ZObject.Find("right3")?.GetComponent<Renderer>();
            // Set lights to start

            StartAttractPattern();
            ToggleLight1(false);
            ToggleLight2(false);
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
            logger.Info("Compatible Rom Dectected, Starting SEGA Rally 3...");
            logger.Info("SEGA Rally 3 Motion Sim starting...Make sure you wash it before you bring it back");
            logger.Info("Long hard easy left!...");
            cockpitCam.transform.position = cockpitCamStartPosition; // new hotness
            cockpitCam.transform.rotation = cockpitCamStartRotation; // new hotness
            StopCurrentPatterns();
            //  strobeCoroutine = StartCoroutine(FlashStrobes());
            // StopCoroutine(strobeCoroutine);
            //  StartDangerPattern();
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
            // StopCoroutine(strobeCoroutine);
            // TurnOffAllStrobes();
            StartAttractPattern();

            // Reset sr3X to initial positions and rotations
            if (sr3XObject != null)
            {
                sr3XObject.position = sr3XStartPosition;
                sr3XObject.rotation = sr3XStartRotation;
            }

            // Reset sr3Y object to initial position and rotation
            if (sr3YObject != null)
            {
                sr3YObject.position = sr3YStartPosition;
                sr3YObject.rotation = sr3YStartRotation;
            }
            // Reset sr3Z object to initial position and rotation
            if (sr3ZObject != null)
            {
                sr3ZObject.position = sr3ZStartPosition;
                sr3ZObject.rotation = sr3ZStartRotation;
            }

            // Reset cockpit cam to initial position and rotation
            if (cockpitCam != null)
            {
                cockpitCam.transform.position = cockpitCamStartPosition;
                cockpitCam.transform.rotation = cockpitCamStartRotation;
            }
            logger.Info("Resetting Positions");
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
                        sr3XObject.Rotate(rotateX, 0, 0);
                        currentRotationX -= rotateX;
                        throttleDetected = true;
                    }
                }
                if (XInput.Get(XInput.Button.LIndexTrigger))
                {
                    float rotateX = vrVelocity * Time.deltaTime;

                    if (currentRotationX + rotateX < rotationLimitX)
                    {
                        sr3XObject.Rotate(-rotateX, 0, 0);
                        currentRotationX += rotateX;
                        throttleDetected = true;
                    }
                }
                // LeftTrigger           
                if (XInput.GetDown(XInput.Button.LIndexTrigger))
                {
                    ToggleBrakeEmissive(true);
                    ToggleLight1(true);
                    ToggleLight2(true);
                    throttleDetected = true;
                }
                // Reset position on button release
                if (XInput.GetUp(XInput.Button.LIndexTrigger))
                {
                    ToggleBrakeEmissive(false);
                    ToggleLight1(false);
                    ToggleLight2(false);
                    throttleDetected = true;
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
            /*
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
            */
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

            // Handle X rotation for sr3YObject and sr3ControllerX (Down Arrow or primaryThumbstick.y > 0)
            // Thumbstick direction: down
            /*
            if ((Input.GetKey(KeyCode.DownArrow) || primaryThumbstick.y > 0))
            {
                if (currentRotationX > -rotationLimitX)
                {
                    float rotateX = (Input.GetKey(KeyCode.DownArrow) ? keyboardVelocityX : primaryThumbstick.y * vrVelocity) * Time.deltaTime;
                    sr3XObject.Rotate(rotateX, 0, 0);
                    currentRotationX -= rotateX;
                    inputDetected = true;
                }
                if (currentControllerRotationX > -controllerrotationLimitX)
                {
                    float controllerRotateX = (Input.GetKey(KeyCode.DownArrow) ? keyboardControllerVelocityX : primaryThumbstick.y * vrControllerVelocity) * Time.deltaTime;
                    sr3ControllerX.Rotate(controllerRotateX, 0, 0);
                    currentControllerRotationX -= controllerRotateX;
                    inputDetected = true;
                }
            }

            // Handle X rotation for sr3YObject and sr3ControllerX (Up Arrow or primaryThumbstick.y < 0)
            // Thumbstick direction: up
            if ((Input.GetKey(KeyCode.UpArrow) || primaryThumbstick.y < 0))
            {
                if (currentRotationX < rotationLimitX)
                {
                    float rotateX = (Input.GetKey(KeyCode.UpArrow) ? keyboardVelocityX : -primaryThumbstick.y * vrVelocity) * Time.deltaTime;
                    sr3XObject.Rotate(-rotateX, 0, 0);
                    currentRotationX += rotateX;
                    inputDetected = true;
                }
                if (currentControllerRotationX < controllerrotationLimitX)
                {
                    float controllerRotateX = (Input.GetKey(KeyCode.UpArrow) ? keyboardControllerVelocityX : -primaryThumbstick.y * vrControllerVelocity) * Time.deltaTime;
                    sr3ControllerX.Rotate(-controllerRotateX, 0, 0);
                    currentControllerRotationX += controllerRotateX;
                    inputDetected = true;
                }
            }
            */
            // Handle Z rotation for sr3XObject and sr3ControllerZ (Down Arrow or primaryThumbstick.y < 0)
            // Thumbstick direction: Left
            if ((Input.GetKey(KeyCode.LeftArrow) || primaryThumbstick.x < 0))
            {
                if (currentRotationZ > -rotationLimitZ)
                {
                    float rotateZ = (Input.GetKey(KeyCode.LeftArrow) ? keyboardVelocityZ : -primaryThumbstick.x * vrVelocity) * Time.deltaTime;
                    sr3ZObject.Rotate(0, 0, rotateZ);
                    currentRotationZ -= rotateZ;
                    inputDetected = true;
                }
                if (currentControllerRotationZ > -controllerrotationLimitZ)
                {
                    float controllerRotateZ = (Input.GetKey(KeyCode.LeftArrow) ? keyboardControllerVelocityZ : -primaryThumbstick.x * vrControllerVelocity) * Time.deltaTime;
                    sr3ControllerZ.Rotate(0, 0, controllerRotateZ);
                    currentControllerRotationZ -= controllerRotateZ;
                    inputDetected = true;
                }
            }

            // Handle Z rotation for sr3XObject and sr3ControllerZ (Up Arrow or primaryThumbstick.y > 0)
            // Thumbstick direction: right
            if ((Input.GetKey(KeyCode.RightArrow) || primaryThumbstick.x > 0))
            {
                if (currentRotationZ < rotationLimitZ)
                {
                    float rotateZ = (Input.GetKey(KeyCode.RightArrow) ? keyboardVelocityZ : primaryThumbstick.x * vrVelocity) * Time.deltaTime;
                    sr3ZObject.Rotate(0, 0, -rotateZ);
                    currentRotationZ += rotateZ;
                    inputDetected = true;
                }
                if (currentControllerRotationZ < controllerrotationLimitZ)
                {
                    float controllerRotateZ = (Input.GetKey(KeyCode.RightArrow) ? keyboardControllerVelocityZ : primaryThumbstick.x * vrControllerVelocity) * Time.deltaTime;
                    sr3ControllerZ.Rotate(0, 0, -controllerRotateZ);
                    currentControllerRotationZ += controllerRotateZ;
                    inputDetected = true;
                }
            }

            // Handle left rotation (Thumbstick left)
            if (primaryThumbstick.x < 0 && currentRotationY < rotationLimitY) // Note the change in condition
            {
                float rotateY = primaryThumbstick.x * vrVelocity * Time.deltaTime;
                sr3YObject.Rotate(0, rotateY, 0);  // Rotate Y in the opposite direction
                currentRotationY -= rotateY;  // Update current rotation (subtracting because the direction is swapped)
                inputDetected = true;
            }

            // Handle right rotation (Thumbstick right)
            if (primaryThumbstick.x > 0 && currentRotationY > -rotationLimitY) // Note the change in condition
            {
                float rotateY = primaryThumbstick.x * vrVelocity * Time.deltaTime;
                sr3YObject.Rotate(0, rotateY, 0);  // Rotate Y in the opposite direction
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
                sr3XObject.Rotate(unrotateX, 0, 0);
                currentRotationX -= unrotateX;
            }
            else if (currentRotationX < 0)
            {
                float unrotateX = Mathf.Min(centeringVelocityX * Time.deltaTime, -currentRotationX);
                sr3XObject.Rotate(-unrotateX, 0, 0);
                currentRotationX += unrotateX;
            }
        }
        void CenterRotation()
        {
            // Center Y-axis
            if (currentRotationY > 0)
            {
                float unrotateY = Mathf.Min(centeringVelocityY * Time.deltaTime, currentRotationY);
                sr3YObject.Rotate(0, unrotateY, 0);
                currentRotationY -= unrotateY;
            }
            else if (currentRotationY < 0)
            {
                float unrotateY = Mathf.Min(centeringVelocityY * Time.deltaTime, -currentRotationY);
                sr3YObject.Rotate(0, -unrotateY, 0);
                currentRotationY += unrotateY;
            }

            // Center Z-axis
            if (currentRotationZ > 0)
            {
                float unrotateZ = Mathf.Min(centeringVelocityZ * Time.deltaTime, currentRotationZ);
                sr3ZObject.Rotate(0, 0, unrotateZ);
                currentRotationZ -= unrotateZ;
            }
            else if (currentRotationZ < 0)
            {
                float unrotateZ = Mathf.Min(centeringVelocityZ * Time.deltaTime, -currentRotationZ);
                sr3ZObject.Rotate(0, 0, -unrotateZ);
                currentRotationZ += unrotateZ;
            }
            //Centering for contoller

            // Center Y-axis Controller rotation
            if (currentControllerRotationY > 0)
            {
                float unrotateY = Mathf.Min(centeringControllerVelocityY * Time.deltaTime, currentControllerRotationY);
                sr3ControllerY.Rotate(0, unrotateY, 0);   // Rotating to reduce the rotation
                currentControllerRotationY -= unrotateY;    // Reducing the positive rotation
            }
            else if (currentControllerRotationY < 0)
            {
                float unrotateY = Mathf.Min(centeringControllerVelocityY * Time.deltaTime, -currentControllerRotationY);
                sr3ControllerY.Rotate(0, -unrotateY, 0);  // Rotating to reduce the rotation
                currentControllerRotationY += unrotateY;    // Reducing the negative rotation
            }

            // Center X-Axis Controller rotation
            if (currentControllerRotationX > 0)
            {
                float unrotateX = Mathf.Min(centeringControllerVelocityX * Time.deltaTime, currentControllerRotationX);
                sr3ControllerX.Rotate(unrotateX, 0, 0);   // Rotating to reduce the rotation
                currentControllerRotationX -= unrotateX;    // Reducing the positive rotation
            }
            else if (currentControllerRotationX < 0)
            {
                float unrotateX = Mathf.Min(centeringControllerVelocityX * Time.deltaTime, -currentControllerRotationX);
                sr3ControllerX.Rotate(-unrotateX, 0, 0);   // Rotating to reduce the rotation
                currentControllerRotationX += unrotateX;    // Reducing the positive rotation
            }

            // Center Z-axis Controller rotation
            if (currentControllerRotationZ > 0)
            {
                float unrotateZ = Mathf.Min(centeringControllerVelocityZ * Time.deltaTime, currentControllerRotationZ);
                sr3ControllerZ.Rotate(0, 0, unrotateZ);   // Rotating to reduce the rotation
                currentControllerRotationZ -= unrotateZ;    // Reducing the positive rotation
            }
            else if (currentControllerRotationZ < 0)
            {
                float unrotateZ = Mathf.Min(centeringControllerVelocityZ * Time.deltaTime, -currentControllerRotationZ);
                sr3ControllerZ.Rotate(0, 0, -unrotateZ);   // Rotating to reduce the rotation
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

        // Method to change the brightness of the lights
        void ToggleBrightness(bool isBright)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = isBright ? brightIntensity : dimIntensity;
            }

            logger.Info($"Lights set to {(isBright ? "bright" : "dim")}.");
        }

        // Method to change the brightness of fire1 light
        void ToggleBrightness1(bool isBright)
        {
            if (sr3brakelight1!= null)
            {
                sr3brakelight1.intensity = isBright ? brightIntensity : dimIntensity;
                // logger.Info($"{sr3brakelight1.name} light set to {(isBright ? "bright" : "dim")}.");
            }
            else
            {
                logger.Debug("sr3brakelight1 light component is not found.");
            }
        }

        // Method to change the brightness of fire2 light
        void ToggleBrightness2(bool isBright)
        {
            if (sr3brakelight2 != null)
            {
                sr3brakelight2.intensity = isBright ? brightIntensity : dimIntensity;
                // logger.Info($"{sr3brakelight2.name} light set to {(isBright ? "bright" : "dim")}.");
            }
            else
            {
                logger.Debug("sr3brakelight2 light component is not found.");
            }
        }

        // Method to toggle the lights
        void ToggleLights(bool isActive)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = isActive;
            }

            logger.Info($"Lights turned {(isActive ? "on" : "off")}.");
        }

        // Method to toggle the sr3brakelight1 light
        void ToggleLight1(bool isActive)
        {
            if (sr3brakelight1 != null)
            {
                sr3brakelight1.enabled = isActive;
                //          logger.Info($"{sr3brakelight1.name} light turned {(isActive ? "on" : "off")}.");
            }
            else
            {
                logger.Debug("sr3brakelight1 light component is not found.");
            }
        }

        // Method to toggle the sr3brakelight2 light
        void ToggleLight2(bool isActive)
        {
            if (sr3brakelight2 != null)
            {
                sr3brakelight2.enabled = isActive;
                //       logger.Info($"{sr3brakelight2.name} light turned {(isActive ? "on" : "off")}.");
            }
            else
            {
                logger.Debug("sr3brakelight2 light component is not found.");
            }
        }

        IEnumerator FlashStrobes()
        {
            while (true)
            {
                // Choose a random strobe light to flash
                int randomIndex = Random.Range(0, 4);
                Light strobeLight = null;

                switch (randomIndex)
                {
                    case 0:
                        strobeLight = strobe1_light;
                        break;
                    case 1:
                        strobeLight = strobe2_light;
                        break;
                    case 2:
                        strobeLight = strobe3_light;
                        break;
                    case 3:
                        strobeLight = strobe4_light;
                        break;
                }

                // Turn on the chosen strobe light
                ToggleStrobeLight(strobeLight, true);

                // Wait for the flash duration
                yield return new WaitForSeconds(flashDuration);

                // Turn off the chosen strobe light
                ToggleStrobeLight(strobeLight, false);

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

        void ToggleStrobe1(bool isActive)
        {
            ToggleStrobeLight(strobe1_light, isActive);
        }

        void ToggleStrobe2(bool isActive)
        {
            ToggleStrobeLight(strobe2_light, isActive);
        }

        void ToggleStrobe3(bool isActive)
        {
            ToggleStrobeLight(strobe3_light, isActive);
        }

        void ToggleStrobe4(bool isActive)
        {
            ToggleStrobeLight(strobe4_light, isActive);
        }
        // Method to toggle the fireemissive object
        void ToggleBrakeEmissive(bool isActive)
        {
            if (sr3hazardObject != null)
            {
                Renderer renderer = sr3hazardObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material[] materials = renderer.materials; // Get all materials
                    foreach (Material mat in materials)
                    {
                        if (mat != null)
                        {
                            if (isActive)
                            {
                                mat.EnableKeyword("_EMISSION");
                            }
                            else
                            {
                                mat.DisableKeyword("_EMISSION");
                            }
                        }
                    }
                    // logger.Info($"sr3hazardObject object emission turned {(isActive ? "on" : "off")}.");
                }
                else
                {
                    logger.Debug("Renderer component is not found on sr3hazardObject object.");
                }
            }
            else
            {
                logger.Debug("sr3hazardObject object is not assigned.");
            }
        }

        // Method to disable emission
        void DisableEmission(Renderer[] emissiveObjects)
        {
            foreach (var renderer in emissiveObjects)
            {
                if (renderer != null)
                {
                    renderer.material.DisableKeyword("_EMISSION");
                }
                else
                {
                    logger.Debug("Renderer component not found on one of the emissive objects.");
                }
            }
        }

        // Method to log missing objects
        void LogMissingObject(Renderer[] emissiveObjects, string arrayName)
        {
            for (int i = 0; i < emissiveObjects.Length; i++)
            {
                if (emissiveObjects[i] == null)
                {
                    logger.Debug($"{arrayName} object at index {i} not found under sr3Z.");
                }
            }
        }

        IEnumerator sr3FrontAttractPattern()
        {
            while (true)
            {
                ToggleEmissive(hazzardEmissiveObjects[0], true);
               // ToggleEmissive(hazzardEmissiveObjects[1], false);
                yield return new WaitForSeconds(frontFlashDuration);

                ToggleEmissive(hazzardEmissiveObjects[0], false);
               // ToggleEmissive(hazzardEmissiveObjects[1], true);
                yield return new WaitForSeconds(frontFlashDuration);
            }
        }

        IEnumerator SideAttractPattern(Renderer[] emissiveObjects)
        {
            while (true)
            {
                ToggleEmissive(emissiveObjects[0], true);
                ToggleEmissive(emissiveObjects[1], false);
                ToggleEmissive(emissiveObjects[2], false);
                yield return new WaitForSeconds(sideFlashDuration);

                ToggleEmissive(emissiveObjects[0], false);
                ToggleEmissive(emissiveObjects[1], true);
                ToggleEmissive(emissiveObjects[2], false);
                yield return new WaitForSeconds(sideFlashDuration);

                ToggleEmissive(emissiveObjects[0], false);
                ToggleEmissive(emissiveObjects[1], false);
                ToggleEmissive(emissiveObjects[2], true);
                yield return new WaitForSeconds(sideFlashDuration);
            }
        }

        IEnumerator sr3FrontHazzardPattern()
        {
            while (true)
            {
                ToggleEmissive(hazzardEmissiveObjects[0], true);
                // ToggleEmissive(hazzardEmissiveObjects[1], true);
                // ToggleEmissive(hazzardEmissiveObjects[2], true);
                yield return new WaitForSeconds(frontDangerDuration);

                ToggleEmissive(hazzardEmissiveObjects[0], false);
                // ToggleEmissive(hazzardEmissiveObjects[1], false);
                // ToggleEmissive(hazzardEmissiveObjects[2], false);
                yield return new WaitForSeconds(frontDangerDelay);
            }
        }

        IEnumerator SideDangerPattern(Renderer[] emissiveObjects)
        {
            while (true)
            {
                ToggleEmissive(emissiveObjects[0], true);
                ToggleEmissive(emissiveObjects[1], false);
                ToggleEmissive(emissiveObjects[2], true);
                yield return new WaitForSeconds(sideDangerDuration);

                ToggleEmissive(emissiveObjects[0], false);
                ToggleEmissive(emissiveObjects[1], true);
                ToggleEmissive(emissiveObjects[2], false);
                yield return new WaitForSeconds(sideDangerDelay);
            }
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
        }

        public void TurnAllOff()
        {
            foreach (var renderer in hazzardEmissiveObjects)
            {
                ToggleEmissive(renderer, false);
            }

            foreach (var renderer in leftEmissiveObjects)
            {
                ToggleEmissive(renderer, false);
            }

            foreach (var renderer in rightEmissiveObjects)
            {
                ToggleEmissive(renderer, false);
            }
        }

        public void StartAttractPattern()
        {
            // Stop any currently running coroutines
            StopCurrentPatterns();

            // Start the attract pattern for front, left, and right
            frontCoroutine = StartCoroutine(sr3FrontHazzardPattern());
         //   leftCoroutine = StartCoroutine(SideAttractPattern(leftEmissiveObjects));
         //   rightCoroutine = StartCoroutine(SideAttractPattern(rightEmissiveObjects));
        }

        public void StartDangerPattern()
        {
            // Stop any currently running coroutines
            StopCurrentPatterns();

            // Start the danger pattern for front, left, and right
            frontCoroutine = StartCoroutine(sr3FrontAttractPattern());
          //  leftCoroutine = StartCoroutine(SideDangerPattern(leftEmissiveObjects));
          //  rightCoroutine = StartCoroutine(SideDangerPattern(rightEmissiveObjects));
        }

        private void StopCurrentPatterns()
        {
            if (frontCoroutine != null)
            {
                StopCoroutine(frontCoroutine);
                frontCoroutine = null;
            }

            if (leftCoroutine != null)
            {
                StopCoroutine(leftCoroutine);
                leftCoroutine = null;
            }

            if (rightCoroutine != null)
            {
                StopCoroutine(rightCoroutine);
                rightCoroutine = null;
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
