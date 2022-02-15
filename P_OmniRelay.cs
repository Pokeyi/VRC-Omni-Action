// Copyright © 2021 Pokeyi - https://pokeyi.dev - pokeyi@pm.me - This work is licensed under the MIT License.

using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.Udon.Common.Interfaces;

namespace Pokeyi.UdonSharp
{
    [AddComponentMenu("Pokeyi.VRChat/P.VRC Omni-Relay")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]

    public class P_OmniRelay : UdonSharpBehaviour
    {   // Multi-purpose event receiver for VRChat:
        [Header(":: VRC Omni-Relay by Pokeyi ::")]

        [Header("- Event Relay -")]
        [Space]
        [Tooltip("Event receivers to send custom events to.")]
        [SerializeField] private UdonSharpBehaviour[] eventReceivers;
        [Tooltip("Events triggered for local player.")]
        [SerializeField] private string[] localEvents;
        [Tooltip("Multi-purpose override.")]
        [SerializeField] private bool overrideBool;

        [Header("- Text Control -")]
        [Space]
        [Tooltip("Target text field for text events.")]
        [SerializeField] private Text textField;
        [Tooltip("Value for text fields.")]
        [SerializeField] private string textValue;

        private int localCount = 0;

        private void SendCustomEvents()
        {
            if (eventReceivers == null) return;
            if (eventReceivers.Length != localEvents.Length)
            {
                Debug.Log("[ Event array Size fields must be the same even if left empty. ]");
                return;
            }
            for (int i = 0; i < eventReceivers.Length; i++)
                if ((eventReceivers[i] != null) && (localEvents[i] != "")) eventReceivers[i].SendCustomEvent(localEvents[i]);
        }

        public void _TextLocalName() // *Public/Protected*
        {   // Set text field to local player name:
            if (textField != null) textField.text = Networking.LocalPlayer.displayName;
        }

        public void _TextValue() // *Public/Protected*
        {   // Set text field to text value string:
            if (textField != null) textField.text = textValue;
        }

        public void _TextLocalCount() // *Public/Protected*
        {
            if (textField == null) return;
            localCount += 1;
            textField.text = localCount.ToString();
        }

        public void _FogToggle() // *Public/Protected*
        {
            if (RenderSettings.fog) RenderSettings.fog = false;
            else RenderSettings.fog = true;
        }

        public void _DayCheck() // *Public/Protected*
        {
            string weekday = DateTime.UtcNow.DayOfWeek.ToString();
            if ((weekday == textValue) || (overrideBool)) SendCustomEvents();
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