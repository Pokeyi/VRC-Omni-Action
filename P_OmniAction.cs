// Copyright © 2021 Pokeyi - https://pokeyi.dev - pokeyi@pm.me - This work is licensed under the MIT License.

// using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.Udon.Common.Interfaces;

namespace Pokeyi.UdonSharp
{
    [AddComponentMenu("Pokeyi.VRChat/P.VRC Omni-Action")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)] // Some variables are serialized over network manually.

    public class P_OmniAction : UdonSharpBehaviour
    {   // Multi-purpose user-action/event and function-handling component for VRChat:
        // Functions: Pickup Reset, Binary Toggle, Sequence Toggle, Enable/Disable All, Animator Toggle/True/False, Player/Object Teleport, Stopwatch, Object-Pool Spawn/Reset
        // Actions: Button Interact, Entry/Exit/Occupied/Pickup Trigger, On-Enable/Disable, Timer Repeat, AudioLink, All-Active Scan, Remote Action, Player Respawn
        // Options: Local/Global, Repeatable/One-Shot, Controller Haptics, Audio Source, Custom Events, Delay, Randomize
        // Planned: (1) Wider functionality for Randomize option.

        /* Old header replaced with editor script:
        [Header(":: VRC Omni-Action by Pokeyi ::")]
        [Header("Target Object (T-i) Functions:")]
        [Header("[0] Events Only   [1] Pickup Reset")]
        [Header("[2] Binary Toggle   [3] Sequence Toggle")]
        [Header("[4] Enable All   [5] Disable All")]
        [Header("[6] Animator Toggle (Animator Bool)")]
        [Header("[7] Animator True   [8] Animator False")]
        [Header("[9] Teleport Player (Player to T-i++)")]
        [Header("[10] Teleport Object (T-0 to T-i++)")]
        [Header("[11] Stopwatch (T-0 Output Text Field)")]
        [Header("[12] Object-Pool Spawn   [13] Object-Pool Reset")]
        [Space]
        [Tooltip("Function selector. Detailed in header above.")]
        */

        [HideInInspector] [Range(0, 13)] public int targetFunction;
        [Tooltip("Targeted game objects.")]
        [SerializeField] private GameObject[] targetObjects;

        private const int EVENTS_ONLY = 0;
        private const int PICKUP_RESET = 1;
        private const int BINARY_TOGGLE = 2;
        private const int SEQUENCE_TOGGLE = 3;
        private const int ENABLE_ALL = 4;
        private const int DISABLE_ALL = 5;
        private const int ANIM_TOGGLE = 6;
        private const int ANIM_TRUE = 7;
        private const int ANIM_FALSE = 8;
        private const int TELEPORT_PLAYER = 9;
        private const int TELEPORT_OBJECT = 10;
        private const int STOPWATCH = 11;
        private const int POOL_SPAWN = 12;
        private const int POOL_RESET = 13;

        [Header("Actions:")]
        [Space]
        [Tooltip("Perform functions on button interact.")]
        [SerializeField] private bool interactButton;
        [Tooltip("Perform functions & entry events on trigger entry.")]
        [SerializeField] private bool entryTrigger;
        [Tooltip("Perform functions & exit events on trigger exit.")]
        [SerializeField] private bool exitTrigger;
        [Tooltip("Perform entry events when first occupied, exit events when first emptied.")]
        [SerializeField] private bool occupiedTrigger;
        [Tooltip("Perform functions & entry events when this object is enabled.")]
        [SerializeField] private bool onEnable;
        [Tooltip("Perform functions & exit events when this object is disabled.")]
        [SerializeField] private bool onDisable;
        [Tooltip("Perform functions on repeated timer.")]
        [SerializeField] private bool timerRepeat;
        [Tooltip("Perform functions via Udon AudioLink data.")]
        [SerializeField] private bool audioLink;
        [Tooltip("Perform entry events when all target objects are active, otherwise exit.")]
        [SerializeField] private bool allActiveScan;
        [Tooltip("Perform functions & entry events on player respawn.")]
        [SerializeField] private bool playerRespawn;

        [Header("Options:")]
        [Space]
        [Tooltip("Local or global (networked) functionality.")]
        [SerializeField] private bool isGlobal = false;
        [Tooltip("Repeat or one-shot use, optional public 'ReEnable' method to reset.")]
        [SerializeField] private bool oneShot = false;
        [Tooltip("Randomize: Sequence Toggle, Object Teleport, Object Pool.")]
        [SerializeField] private bool randomizeFunctions = false; // Restrict to ProcessFunctions() and outliers.
        [Tooltip("Name of animator bool to toggle.")]
        [SerializeField] private string animatorBool = "";
        [Tooltip("Triggered by player-owned object containing this name -instead- of player.")]
        [SerializeField] private string overrideNameContains = "";
        [Tooltip("Minimum timer repeat.")]
        [SerializeField] private float minTimer = 0F;
        [Tooltip("Maximum timer repeat.")]
        [SerializeField] private float maxTimer = 0F;
        [Tooltip("Number of seconds to delay target object function.")]
        [SerializeField] private float delayTime = 0F;
        [Tooltip("AudioLink band to activate functions.")]
        [SerializeField] [Range(0, 3)] private int audioLinkBand = 0;
        [Tooltip("AudioLink band threshold to activate functions.")]
        [SerializeField] [Range(0F, 1F)] private float audioLinkThreshold = 0.125F;
        [Tooltip("AudioLink source script.")]
        [SerializeField] private UdonSharpBehaviour audioLinkSource;

        [Header("Default / On / Entry Trigger:")]
        [Space]
        [Tooltip("(Default/Entry) Controller haptics profile to trigger locally via relay script.")]
        [SerializeField] private UdonSharpBehaviour hapticsProfile;
        [Tooltip("(Default/Entry) Audio source to play locally or globally dependent on 'isGlobal'.")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("(Default/Entry) Event receivers to send custom events to.")]
        [SerializeField] private UdonSharpBehaviour[] eventReceivers;
        [Tooltip("(Default/Entry) Events triggered only for local player.")]
        [SerializeField] private string[] localEvents;
        [Tooltip("(Default/Entry) Network events triggered for all players.")]
        [SerializeField] private string[] globalEvents;

        [Header("Off / Exit Trigger:")]
        [Space]
        [Tooltip("(Exit) Controller haptics profile to trigger locally via relay script.")]
        [SerializeField] private UdonSharpBehaviour exitHapticsProfile;
        [Tooltip("(Exit) Audio source to play locally or globally dependent on 'isGlobal'.")]
        [SerializeField] private AudioSource exitAudioSource;
        [Tooltip("(Exit) Event receivers to send custom events to.")]
        [SerializeField] private UdonSharpBehaviour[] exitEventReceivers;
        [Tooltip("(Exit) Events triggered only for local player.")]
        [SerializeField] private string[] exitLocalEvents;
        [Tooltip("(Exit) Network events triggered for all players.")]
        [SerializeField] private string[] exitGlobalEvents;

        [UdonSynced] [HideInInspector] public bool syncedIsEnabled = true; // Whether active or not based on oneShot option. *Public/Network-Synced*
        [UdonSynced] [HideInInspector] public bool syncedStopwatchActive = false; // Stopwatch function active state. *Public/Network-Synced*
        [UdonSynced] [HideInInspector] public bool[] syncedObjectActive; // Active value of each target game object. *Public/Network-Synced*
        [UdonSynced] [HideInInspector] public int syncedActiveIndex = 0; // Index of currently-active game object. *Public/Network-Synced*
        [UdonSynced] [HideInInspector] public float syncedTimeValue = 0F; // Current value of timing events. *Public/Network-Synced*

        private VRCPlayerApi playerLocal; // Reference to local player.
        private Rigidbody[] objectRigidBody; // Target objects' rigidbodies.
        private Animator[] objectAnimator; // Target objects' animators.
        private VRCObjectPool[] objectPool; // Target objects' object pools.
        private Text textField; // Field to send text data.
        private Vector3[] objectPos; // Target objects' original positions.
        private Quaternion[] objectRot; // Target objects' original rotations.
        private float timeToCount = 0F; // Generated timer frequency.
        private int triggerCount = 0; // Active trigger counter.
        private bool hasStarted = false; // Whether start event has been initialized.
        private bool lastAllActive = false; // Last all-active state to avoid repeat.
        private readonly bool LOGGING = true; // Toggle logging.
        private readonly bool LOGDB = true; // Toggle output of each log message to Unity debug log.

        public void Start()
        {   // Initialization:
            playerLocal = Networking.LocalPlayer;
            if (!interactButton) this.DisableInteractive = true; // Disable button interactivity if not set as button.
            if (timerRepeat) ResetTimer();
            InitializeFunctions();
            if (!hasStarted)
            {
                hasStarted = true;
                if (onEnable) ProcessFiltered(true);
            }
        }

        public void OnEnable()
        {   // When this object is enabled:
            if ((!hasStarted) || (!onEnable)) return;
            AddLog(playerLocal.displayName + " -> [ OnEnable ] -> " + gameObject.name);
            ProcessFiltered(true);
        }

        public void OnDisable()
        {   // When this object is disabled:
            if (!onDisable) return;
            AddLog(playerLocal.displayName + " <- [ OnDisable ] <- " + gameObject.name);
            ProcessFiltered(false);
        }

        public override void Interact()
        {   // Button interaction event:
            AddLog(playerLocal.displayName + " -> [ Interact ] -> " + gameObject.name);
            ProcessAction(true);
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {   // Player entry event:
            if (overrideNameContains != "") return; // Ignore if object override.
            if (occupiedTrigger) OccupiedTrigger(true);
            else if ((entryTrigger) && (player.isLocal))
            {
                AddLog(player.displayName + " -> [ Player Trigger Enter ] -> " + gameObject.name);
                ProcessAction(true);
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {   // Player exit event:
            if (overrideNameContains != "") return; // Ignore if object override.
            if (occupiedTrigger) OccupiedTrigger(false);
            else if ((exitTrigger) && (player.isLocal))
            {
                AddLog(player.displayName + " <- [ Player Trigger Exit ] <- " + gameObject.name);
                ProcessAction(false);
            }
        }

        public void OnTriggerEnter(Collider otherCollider)
        {   // Object entry event:
            if ((overrideNameContains == "") || (!otherCollider.name.Contains(overrideNameContains))) return; // Ignore if empty or non-matching override name.
            if (occupiedTrigger) OccupiedTrigger(true);
            else if (entryTrigger)
            {
                VRCPlayerApi colliderOwner = Networking.GetOwner(otherCollider.gameObject); // Filter for local player and override name match:
                if (!colliderOwner.isLocal) return;
                AddLog(otherCollider.name + " (" + colliderOwner.displayName + ") -> [ Object Trigger Enter ] -> " + gameObject.name);
                ProcessAction(true);
            }
        }

        public void OnTriggerExit(Collider otherCollider)
        {   // Object exit event:
            if ((overrideNameContains == "") || (!otherCollider.name.Contains(overrideNameContains))) return; // Ignore if empty or non-matching override name.
            if (occupiedTrigger) OccupiedTrigger(false);
            else if (exitTrigger)
            {
                VRCPlayerApi colliderOwner = Networking.GetOwner(otherCollider.gameObject); // Filter for local player and override name match:
                if (!colliderOwner.isLocal) return;
                AddLog(otherCollider.name + " (" + colliderOwner.displayName + ") <- [ Object Trigger Exit ] <- " + gameObject.name);
                ProcessAction(false);
            }
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {   // When the local player respawns:
            if ((playerRespawn) && (player.isLocal))
            {
                AddLog(player.displayName + " -> [ Player Respawn ] -> " + gameObject.name);
                ProcessAction(true);
            }
        }

        public override void OnDeserialization()
        {   // Call function update method for non-local players and late joiners:
            if ((!isGlobal) || (Networking.IsOwner(gameObject))) return;
            if (interactButton) this.DisableInteractive = !syncedIsEnabled; // Update button interactivity based on syncedIsEnabled value.
            AddLog(playerLocal.displayName + " <- [ Deserialization ] <- " + gameObject.name);
            UpdateFunctions();
        }

        public void Update()
        {
            if (allActiveScan)
            {   // All-active scan updates:
                bool allActive = true; // Check each target game object's active status:
                for (int i = 0; i < targetObjects.Length; i++) if ((targetObjects[i] != null) && (allActive)) allActive = targetObjects[i].activeSelf;
                if (allActive == lastAllActive) return; // Ignore if same state as previous frame.
                if (allActive)
                {   // If all objects are active, process entry action:
                    AddLog(playerLocal.displayName + " -> [ All Active ] -> " + gameObject.name);
                    ProcessFiltered(true);
                }
                else
                {   // If all objects are not active, process exit action:
                    AddLog(playerLocal.displayName + " <- [ Not All Active ] <- " + gameObject.name);
                    ProcessFiltered(false);
                }
                lastAllActive = allActive;
            }

            // ----- Synced Time ----->

            else if ((audioLink) && (audioLinkSource != null))
            {   // AudioLink updates:
                Color[] audioData = (Color[])audioLinkSource.GetProgramVariable("audioData");
                if (audioData.Length == 0) return;
                int dataIndex = audioLinkBand * 128;
                float amplitude = audioData[dataIndex].grayscale;
                if ((amplitude > audioLinkThreshold) && (syncedTimeValue >= 1F - audioLinkThreshold))
                {   // If AudioLink data is above threshold and within timing throttle:
                    syncedTimeValue = 0F;
                    ProcessFiltered(true);
                }
                else syncedTimeValue += Time.deltaTime;
            }
            else if ((timerRepeat) && (timeToCount > 0F))
            {   // Timer repeat updates:
                if (syncedTimeValue >= timeToCount)
                {   // Time's up:
                    ResetTimer();
                    ProcessFiltered(true);
                }
                else syncedTimeValue += Time.deltaTime;
            }
            else if (syncedStopwatchActive)
            {   // Stopwatch updates:
                syncedTimeValue += Time.deltaTime;
                UpdateFunctions();
            }
        }

        private void OccupiedTrigger(bool isEntry)
        {   // Adjust count to determine if trigger is newly occupied or empty and process accordingly:
            if (triggerCount < 0) triggerCount = 0;
            if (isEntry)
            {
                if (triggerCount == 0)
                {
                    AddLog(playerLocal.displayName + " -> [ Active Trigger Occupied ] -> " + gameObject.name);
                    ProcessFiltered(true);
                }
                triggerCount += 1;
            }
            else
            {
                triggerCount -= 1;
                if (triggerCount == 0)
                {
                    AddLog(playerLocal.displayName + " <- [ Active Trigger Empty ] <- " + gameObject.name);
                    ProcessFiltered(false);
                }
            }
        }

        private void ProcessFiltered(bool isEntry)
        {   // Global network owner filter for nonlocal actions:
            if (!isGlobal) ProcessAction(isEntry);
            else if (Networking.IsOwner(gameObject)) ProcessAction(isEntry);
        }

        private void ProcessAction(bool isEntry)
        {   // Perform networking, processing, function logic, and serialization for received action:
            if (!syncedIsEnabled) return;
            if (isGlobal)
            {   // Networking and synced audio:
                if (!Networking.IsOwner(gameObject)) Networking.SetOwner(playerLocal, gameObject);
                if (isEntry) SendCustomNetworkEvent(NetworkEventTarget.All, "PlayAudio");
                else SendCustomNetworkEvent(NetworkEventTarget.All, "PlayExitAudio");
            }
            else if (isEntry) PlayAudio();
            else PlayExitAudio();
            RelayHaptics(isEntry);
            if (oneShot)
            {   // Disable functionality and button-prompt if one-shot:
                syncedIsEnabled = false;
                this.DisableInteractive = true;
            }
            SendCustomEvents(isEntry);
            ProcessFunctions(isEntry);
            if (delayTime > 0F) SendCustomEventDelayedSeconds("_ProcessFinal", delayTime);
            else _ProcessFinal();
        }

        public void _ProcessFinal() // *Public/Protected*
        {   // Final function updates and serialization are separated for potential delay call:
            if (isGlobal) RequestSerialization();
            OutlierFunctions();
            UpdateFunctions();
        }

        private void InitializeFunctions()
        {   // Initialize functions dependent on targetFunction value:
            if (targetObjects == null) return;
            switch (targetFunction)
            {
                case BINARY_TOGGLE: case ENABLE_ALL: case DISABLE_ALL: // Populate bool array for each game object and assign current active value of each:
                    syncedObjectActive = new bool[targetObjects.Length];
                    for (int i = 0; i < targetObjects.Length; i++) if (targetObjects[i] != null) syncedObjectActive[i] = targetObjects[i].activeSelf;
                    break;

                case SEQUENCE_TOGGLE: case TELEPORT_PLAYER: case TELEPORT_OBJECT: // Populate bool array for each game object and update objects at active index 0:
                    if (targetFunction == SEQUENCE_TOGGLE) syncedObjectActive = new bool[targetObjects.Length];
                    syncedActiveIndex = 0;
                    break;

                case PICKUP_RESET: // Populate arrays with default positions, rotations, and rigidbodies for each resettable game object:
                    objectRigidBody = new Rigidbody[targetObjects.Length];
                    objectPos = new Vector3[targetObjects.Length];
                    objectRot = new Quaternion[targetObjects.Length];
                    for (int i = 0; i < targetObjects.Length; i++) if (targetObjects[i] != null)
                        {
                            objectRigidBody[i] = targetObjects[i].GetComponent<Rigidbody>();
                            objectPos[i] = targetObjects[i].transform.position;
                            objectRot[i] = targetObjects[i].transform.rotation;
                        }
                    break;

                case ANIM_TOGGLE: case ANIM_TRUE: case ANIM_FALSE: // Populate array for each game object's animator and bool and assign current active value of each:
                    objectAnimator = new Animator[targetObjects.Length];
                    syncedObjectActive = new bool[targetObjects.Length];
                    for (int i = 0; i < targetObjects.Length; i++) if ((targetObjects[i] != null) && (animatorBool != ""))
                        {
                            objectAnimator[i] = targetObjects[i].GetComponent<Animator>();
                            syncedObjectActive[i] = objectAnimator[i].GetBool(animatorBool);
                        }
                    break;

                case STOPWATCH: // Assign stopwatch text field:
                    if (targetObjects != null) textField = targetObjects[0].GetComponent<Text>();
                    syncedStopwatchActive = false;
                    syncedTimeValue = 0F;
                    break;

                case POOL_SPAWN: case POOL_RESET: // Populate array with target objects' object pools:
                    objectPool = new VRCObjectPool[targetObjects.Length];
                    for (int i = 0; i < targetObjects.Length; i++) if (targetObjects[i] != null) objectPool[i] = (VRCObjectPool)targetObjects[i].GetComponent(typeof(VRCObjectPool));
                    break;
            }
            UpdateFunctions(); // Update initially for each local user without serialization.
        }

        private void ProcessFunctions(bool isEntry)
        {   // Process synced variable changes specific to each target function prior to serialization:
            if (targetObjects == null) return;
            switch (targetFunction)
            {
                case BINARY_TOGGLE: case ANIM_TOGGLE: // Swap active toggle values:
                    for (int i = 0; i < targetObjects.Length; i++) if (targetObjects[i] != null) syncedObjectActive[i] = !syncedObjectActive[i];
                    break;

                case SEQUENCE_TOGGLE: case TELEPORT_PLAYER: case TELEPORT_OBJECT: // Increment active index:
                    syncedActiveIndex += 1;
                    if (randomizeFunctions) syncedActiveIndex = Random.Range(0, targetObjects.Length);
                    if (syncedActiveIndex >= targetObjects.Length) syncedActiveIndex = 0;
                    if ((targetFunction == TELEPORT_OBJECT) && (syncedActiveIndex == 0)) syncedActiveIndex = 1;
                    break;

                case ENABLE_ALL: case ANIM_TRUE: // Enable all active values:
                    for (int i = 0; i < targetObjects.Length; i++) if (targetObjects[i] != null) syncedObjectActive[i] = true;
                    break;

                case DISABLE_ALL: case ANIM_FALSE: // Disable all active values:
                    for (int i = 0; i < targetObjects.Length; i++) if (targetObjects[i] != null) syncedObjectActive[i] = false;
                    break;

                case STOPWATCH: // Stop or restart stopwatch:
                    if (entryTrigger && exitTrigger)
                    {   // If using both trigger events, start on exit and stop on re-entry:
                        if (isEntry) syncedStopwatchActive = false;
                        else
                        {
                            syncedTimeValue = 0F;
                            syncedStopwatchActive = true;
                        }
                    }
                    else
                    {   // Toggle stopwatch if binary trigger:
                        if (!syncedStopwatchActive) syncedTimeValue = 0F;
                        syncedStopwatchActive = !syncedStopwatchActive;
                    }
                    break;
            }
        }

        private void OutlierFunctions()
        {   // Perform functions that operate without serialization:
            if (targetObjects == null) return;
            switch (targetFunction)
            {
                case TELEPORT_PLAYER: // Teleport local player to designated active index object:
                    if (targetObjects[syncedActiveIndex] != null)
                        playerLocal.TeleportTo(targetObjects[syncedActiveIndex].transform.position, targetObjects[syncedActiveIndex].transform.rotation);
                    break;

                case PICKUP_RESET: // Reset all objects to their default states:
                    for (int i = 0; i < targetObjects.Length; i++) if ((targetObjects[i] != null) && (objectRigidBody[i] != null))
                        {
                            if ((isGlobal) && (!Networking.IsOwner(targetObjects[i]))) Networking.SetOwner(playerLocal, targetObjects[i]);
                            targetObjects[i].transform.position = objectPos[i];
                            targetObjects[i].transform.rotation = objectRot[i];
                            objectRigidBody[i].Sleep();
                        }
                    break;

                case POOL_SPAWN: // Attempt to spawn an object from each object pool:
                    for (int i = 0; i < targetObjects.Length; i++) if ((targetObjects[i] != null) && (objectPool[i] != null))
                        {
                            if ((isGlobal) && (!Networking.IsOwner(targetObjects[i]))) Networking.SetOwner(playerLocal, targetObjects[i]);
                            if (randomizeFunctions) objectPool[i].Shuffle();
                            objectPool[i].TryToSpawn();
                        }
                    break;

                case POOL_RESET: // Reset all objects in each object pool:
                    for (int i = 0; i < targetObjects.Length; i++) if ((targetObjects[i] != null) && (objectPool[i] != null))
                        {
                            if ((isGlobal) && (!Networking.IsOwner(targetObjects[i]))) Networking.SetOwner(playerLocal, targetObjects[i]);
                            for (int n = 0; n < objectPool[i].Pool.Length; n++)
                            {
                                GameObject resetObject = objectPool[i].Pool[n].gameObject;
                                if (resetObject != null) objectPool[i].Return(resetObject);
                            }
                            if (randomizeFunctions) objectPool[i].Shuffle();
                        }
                    break;
            }
        }

        private void UpdateFunctions()
        {   // Update synced target objects to reflect processed changes after serialization:
            if (targetObjects == null) return;
            switch (targetFunction)
            {
                case BINARY_TOGGLE: case ENABLE_ALL: case DISABLE_ALL: // Update game objects to match current active values:
                    for (int i = 0; i < targetObjects.Length; i++) if (targetObjects[i] != null) targetObjects[i].SetActive(syncedObjectActive[i]);
                    break;

                case SEQUENCE_TOGGLE: // Update game objects to match current active index:
                    for (int i = 0; i < targetObjects.Length; i++) if (targetObjects[i] != null) targetObjects[i].SetActive(i == syncedActiveIndex);
                    break;

                case ANIM_TOGGLE: case ANIM_TRUE: case ANIM_FALSE: // Update animator bools to match current active values:
                    for (int i = 0; i < targetObjects.Length; i++) if ((targetObjects[i] != null) && (animatorBool != "")) objectAnimator[i].SetBool(animatorBool, syncedObjectActive[i]);
                    break;

                case TELEPORT_OBJECT: // Reposition game object transform to match active index target:
                    if ((targetObjects[0] != null) && (targetObjects[syncedActiveIndex] != null))
                        targetObjects[0].transform.SetPositionAndRotation(targetObjects[syncedActiveIndex].transform.position, targetObjects[syncedActiveIndex].transform.rotation);
                    break;

                case STOPWATCH: // Update stopwatch text field:
                    if (textField == null) return;
                    int msec = (int)((syncedTimeValue - (int)syncedTimeValue) * 100);
                    int sec = (int)(syncedTimeValue % 60);
                    int min = (int)(syncedTimeValue / 60 % 60);
                    textField.text = string.Format("{0:00}:{1:00}:{2:00}", min, sec, msec);
                    break;
            }
        }

        private void SendCustomEvents(bool isEntry)
        {
            if (isEntry)
            {   // Send optional entry events:
                if (eventReceivers == null) return;
                if ((eventReceivers.Length != localEvents.Length) || (eventReceivers.Length != globalEvents.Length))
                {
                    AddLog("[ Event array Size fields must be the same even if left empty. ]");
                    return;
                }
                for (int i = 0; i < eventReceivers.Length; i++) if (eventReceivers[i] != null)
                {
                    if (localEvents[i] != "") eventReceivers[i].SendCustomEvent(localEvents[i]);
                    if (globalEvents[i] != "") eventReceivers[i].SendCustomNetworkEvent(NetworkEventTarget.All, globalEvents[i]);
                }
            }
            else
            {   // Send optional exit events:
                if (exitEventReceivers == null) return;
                if ((exitEventReceivers.Length != exitLocalEvents.Length) || (exitEventReceivers.Length != exitGlobalEvents.Length))
                {
                    AddLog("[ Event array Size fields must be the same even if left empty. ]");
                    return;
                }
                for (int i = 0; i < exitEventReceivers.Length; i++) if (exitEventReceivers[i] != null)
                {
                    if (exitLocalEvents[i] != "") exitEventReceivers[i].SendCustomEvent(exitLocalEvents[i]);
                    if (exitGlobalEvents[i] != "") exitEventReceivers[i].SendCustomNetworkEvent(NetworkEventTarget.All, exitGlobalEvents[i]);
                }
            }
        }

        private void RelayHaptics(bool isEntry)
        {   // Relay haptics to assigned haptics profile:
            if ((isEntry) && (hapticsProfile != null)) hapticsProfile.SendCustomEvent("_TriggerHaptics");
            else if ((!isEntry) && (exitHapticsProfile != null)) exitHapticsProfile.SendCustomEvent("_TriggerHaptics");
        }

        private void ResetTimer()
        {   // Randomize timer value within range and reset:
            if (minTimer > maxTimer) maxTimer = minTimer;
            timeToCount = Random.Range(minTimer, maxTimer);
            syncedTimeValue = 0F;
        }

        private void AddLog(string msg)
        {   // Add debug log:
            if (!LOGGING) return;
            if (LOGDB) Debug.Log(msg);
            // Future log output.
        }

        public void PlayAudio() // *Public/Network-Event-RPC*
        {   // Play assigned audio sources:
            if (audioSource != null) audioSource.Play();
        }

        public void PlayExitAudio() // *Public/Network-Event-RPC*
        {   // Play assigned exit audio sources:
            if (exitAudioSource != null) exitAudioSource.Play();
        }

        public void _ReEnable() // *Public/Protected*
        {   // Optional public method to reset if declared as one-shot action, re-enable as network object owner and request serialization:
            if ((syncedIsEnabled) || (!oneShot)) return;
            if ((isGlobal) && (!Networking.IsOwner(gameObject))) Networking.SetOwner(playerLocal, gameObject);
            if (interactButton) this.DisableInteractive = false;
            syncedIsEnabled = true;
            if (isGlobal) RequestSerialization();
            AddLog(playerLocal.displayName + " -> [ ReEnable ] -> " + gameObject.name);
        }

        public void _ReInit() // *Public/Protected*
        {
            if ((isGlobal) && (!Networking.IsOwner(gameObject))) Networking.SetOwner(playerLocal, gameObject);
            InitializeFunctions();
            if (isGlobal) RequestSerialization();
            AddLog(playerLocal.displayName + " -> [ ReInitialize ] -> " + gameObject.name);
        }

        public void _RemoteAction() // *Public/Protected*
        {   // Trigger action remotely from another script:
            AddLog(playerLocal.displayName + " -> [ Remote Action ] -> " + gameObject.name);
            ProcessAction(true);
        }
    }
}

/* MIT License

Copyright (c) 2021 Pokeyi - https://pokeyi.dev - pokeyi@pm.me

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */