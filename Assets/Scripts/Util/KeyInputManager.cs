using System;
using UnityEngine;

// TODO: Remove in favor of InputRegistry
public class KeyInputManager {

    public static KeyInputManager shared = new KeyInputManager();

    private KeyInputManager(){}

    public bool IsBusy { 
        get { return registeredInputFields > 0; }
    }

    private int registeredInputFields = 0;

    public bool GetKeyDown(KeyCode code) {

        if (IsBusy) { return false; }
        //if (!Input.anyKeyDown) { return false; }

        return Input.GetKeyDown(code);
    }

    public void Register() {
        registeredInputFields++;
    }

    public void Deregister() {
        registeredInputFields = Math.Max(registeredInputFields - 1, 0);
    }
}