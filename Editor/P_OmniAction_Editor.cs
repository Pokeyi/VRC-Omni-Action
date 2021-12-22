// Editor code and guidance provided by @Vowgan. Thank you!

using UdonSharpEditor;
using UnityEditor;

namespace Pokeyi.UdonSharp
{
    [CustomEditor(typeof(P_OmniAction))]

    public class P_OmniAction_Editor : Editor
    {   // Custom editor script for VRC Omni-Action:
        private P_OmniAction targetScript; // Target script reference.
        private SerializedProperty propTargetFunction; // Function property reference.
        
        private void OnEnable()
        {   // When the object loads in the inspector:
            targetScript = target as P_OmniAction; // Assign reference to target script.
            if (targetScript != null) propTargetFunction = serializedObject.FindProperty(nameof(targetScript.targetFunction)); // Assign reference to function property.
        }

        public override void OnInspectorGUI()
        {   // Start drawing GUI:
            // if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target)) return; // Draw 'Convert to Udon' button if needed. // Seems to still happen automatically already.
            UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target); // Draw UdonSharp header.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(":: VRC Omni-Action by Pokeyi ::", EditorStyles.boldLabel); // Title header.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Main Function:", EditorStyles.boldLabel); // Title header.
            EditorGUILayout.Space();
            propTargetFunction.intValue = (int)(OmniActions)EditorGUILayout.EnumPopup("Target Function", (OmniActions)propTargetFunction.intValue); // Draw dropdown with enum conversion.
            if (serializedObject != null) serializedObject.ApplyModifiedProperties(); // Apply changes to target Udon script.
            OmniActions action = (OmniActions)propTargetFunction.intValue;  // Reference selected function for HelpBox.
            string helpBoxInfo = "";
            switch (action)
            {   // Change help-box text based on which function is selected:
                case OmniActions.EventsOnly:
                    helpBoxInfo = "No target-object functionality outside of options and event routing.";
                    break;
                case OmniActions.PickupReset:
                    helpBoxInfo = "Reset target VRC-Pickup objects to their original positions and rotations from instance start.";
                    break;
                case OmniActions.BinaryToggle:
                    helpBoxInfo = "Swap current active status of all target objects.";
                    break;
                case OmniActions.SequenceToggle:
                    helpBoxInfo = "Enable active status of one target object at a time (disabling all others) in sequence.";
                    break;
                case OmniActions.EnableAll:
                    helpBoxInfo = "Enable active status of all target objects.";
                    break;
                case OmniActions.DisableAll:
                    helpBoxInfo = "Disable active status of all target objects.";
                    break;
                case OmniActions.AnimatorToggle:
                    helpBoxInfo = "Toggle specified boolean variable on target objects' animator components.";
                    break;
                case OmniActions.AnimatorTrue:
                    helpBoxInfo = "Enable specified boolean variable on target objects' animator components.";
                    break;
                case OmniActions.AnimatorFalse:
                    helpBoxInfo = "Disable specified boolean variable on target objects' animator components.";
                    break;
                case OmniActions.TeleportPlayer:
                    helpBoxInfo = "Teleport player to the location of the first target object or in sequence if multiple.";
                    break;
                case OmniActions.TeleportObject:
                    helpBoxInfo = "Teleport first target object to all other target-object locations in sequence.";
                    break;
                case OmniActions.Stopwatch:
                    helpBoxInfo = "Toggle a stopwatch counter that outputs its accrued time to Text component of first target object. " +
                        "If using both entry & exit trigger actions, stopwatch will start on exit and stop on re-entry. Otherwise, it will toggle.";
                    break;
                case OmniActions.ObjectPoolSpawn:
                    helpBoxInfo = "Attempt to spawn next object from each target object's VRC-Object-Pool.";
                    break;
                case OmniActions.ObjectPoolReset:
                    helpBoxInfo = "Reset contents of each target object's VRC-Object-Pool.";
                    break;
            }
            EditorGUILayout.HelpBox(helpBoxInfo, MessageType.Info); // Draw the help box.
            EditorGUILayout.Space();
            base.OnInspectorGUI(); // Draw original inspector.
        }
    }

    public enum OmniActions
    {   // Dropdown enum (order must match):
        EventsOnly,
        PickupReset,
        BinaryToggle,
        SequenceToggle,
        EnableAll,
        DisableAll,
        AnimatorToggle,
        AnimatorTrue,
        AnimatorFalse,
        TeleportPlayer,
        TeleportObject,
        Stopwatch,
        ObjectPoolSpawn,
        ObjectPoolReset,
    }
}