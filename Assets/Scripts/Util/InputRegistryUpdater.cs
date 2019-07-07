using UnityEngine;
using Keiwando;

namespace Keiwando.Evolution {
    public class InputRegistryUpdater: MonoBehaviour {
        void Update() {
            InputRegistry.shared.Update();
        }
    }
}