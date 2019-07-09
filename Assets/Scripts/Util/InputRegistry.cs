using System;
using System.Collections.Generic;

namespace Keiwando {

    public delegate void InputHandler(InputType inputTypes);

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
        }

        private static bool DoesReceiverHandleInput(Receiver receiver, InputType inputType) {
        
            return (inputType & receiver.inputTypes) != 0;
        }
    }
}