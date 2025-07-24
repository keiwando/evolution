using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Keiwando {

    public delegate void OnBackButtonPressed();

    [Flags]
    public enum InputType {
        Click = 1,
        Touch = 2,
        Key = 4,
        Scroll = 8,
        AndroidBack = 16,
        All = 1 | 2 | 4 | 8 | 16
    }

    /// <summary>
    /// Describes how an input event handler handles the event.
    /// </summary>
    public enum EventHandleMode {

        /// <summary>
        /// The event handler consumes the event. No other
        /// handlers in the chain get to handle events consumed
        /// by this handler. The same event might have been 
        /// handled by a previous event handler in the chain.
        /// </summary>
        ConsumeEvent,

        /// <summary>
        /// The event handler handles the event but doesn't consume
        /// it. Other handlers in the chain get to handle this 
        /// event as well. The same event might have been 
        /// handled by a previous event handler in the chain.
        /// </summary>
        PassthroughEvent,

        /// <summary>
        /// The event handler consumes the event. No other
        /// handlers in the chain get to handle events consumed
        /// by this handler. The same event has not been 
        /// handled by a previous event handler in the chain.
        /// </summary>
        RequireUnique
    }

    public class InputRegistry {

        private struct Handler {
            public InputType inputType;
            public object handler;
            public EventHandleMode eventHandleMode;
        }

        public static InputRegistry shared = new InputRegistry();

        private List<Handler> handlerStack = new List<Handler>();

        public void Register(
            InputType inputType, 
            object handler, 
            EventHandleMode mode = EventHandleMode.ConsumeEvent
        ) {
            var receiver = new Handler() {
                handler = handler,
                inputType = inputType,
                eventHandleMode = mode
            };
            handlerStack.Insert(0, receiver);
        }

        public void Deregister(object handler) {
            
            var oldStack = this.handlerStack;
            this.handlerStack = new List<Handler>();
            for (int i = 0; i < oldStack.Count; i++) {
                var element = oldStack[i];
                if (element.handler != handler)
                    handlerStack.Add(element);
            }
        }

        public void DeregisterAll() {
            handlerStack.Clear();
        } 

        /// <summary>
        /// Returns whether or not the given handler is allowed to handle 
        /// the specified event type.
        /// </summary>
        /// <param name="handler">The object that is trying to handle the 
        /// given type of event.</param>
        /// <param name="inputType">The type of event that is trying ot be handled.</param>
        /// <returns></returns>
        public bool MayHandle(InputType inputType, object handler) {

            bool alreadyHandled = false;

            for (int i = 0; i < handlerStack.Count; i++) {

                var currentHandler = handlerStack[i];
                var handledType = currentHandler.inputType;
                var handleMode = currentHandler.eventHandleMode;
                
                if ((handledType & inputType) == 0) continue;

                if (currentHandler.handler == handler) {
                    return handleMode != EventHandleMode.RequireUnique || !alreadyHandled;
                } else if (currentHandler.handler != handler) {
                    if (handleMode == EventHandleMode.ConsumeEvent ||
                        (handleMode == EventHandleMode.RequireUnique && !alreadyHandled)) {
                        return false;
                    }
                    alreadyHandled = true;
                }
            }

            return false;
        }
    }
}