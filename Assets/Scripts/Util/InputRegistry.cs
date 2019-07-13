using System;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando {

    public delegate void InputHandler(InputType inputTypes);
    public delegate void OnBackButtonPressed();

    [Flags]
        public enum InputType {
            Click = 1,
            Touch = 2,
            Key = 4,
            Scroll = 8,
            All = 1 | 2 | 4 | 8
        }

    public class InputRegistry {

        private struct Receiver {
            public InputHandler handler;
            public InputType inputTypes;
        }

        public static InputRegistry shared = new InputRegistry();

        private List<Receiver> receiverStack = new List<Receiver>();
        private List<OnBackButtonPressed> backButtonStack = new List<OnBackButtonPressed>();

        public void Register(InputType inputTypes, InputHandler handler) {
            var receiver = new Receiver() {
                handler = handler,
                inputTypes = inputTypes
            };
            receiverStack.Insert(0, receiver);
        }

        public void Deregister() {
            if (receiverStack.Count > 0)
                receiverStack.RemoveAt(0);
        }

        public void DeregisterAll() {
            receiverStack.Clear();
        } 

        public void RegisterForAndroidBackButton(OnBackButtonPressed onPress) {
            backButtonStack.Insert(0, onPress);
        }

        public void DeregisterBackButton() {
            if (backButtonStack.Count > 0)
                backButtonStack.RemoveAt(0);
        }

        public void DeregisterAllBackButtonCallbacks() {
            backButtonStack.Clear();
        }

        public void Update() {

            InputType availableInputTypes = InputType.All;
            for (int i = 0; i < receiverStack.Count; i++) {
                var receiver = receiverStack[i];
                InputType acceptedAndAvailable = receiver.inputTypes & availableInputTypes;
                if (acceptedAndAvailable != 0) {
                    receiver.handler(acceptedAndAvailable);
                }
                availableInputTypes &= ~acceptedAndAvailable;
            }

            #if PLATFORM_ANDROID
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (backButtonStack.Count > 0) {
                    backButtonStack[0]();
                }
            }
            #endif
        }

        private static bool DoesReceiverHandleInput(Receiver receiver, InputType inputType) {
        
            return (inputType & receiver.inputTypes) != 0;
        }
    }
}