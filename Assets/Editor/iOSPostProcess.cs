using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class iOSPostProcess : MonoBehaviour {
    
    [PostProcessBuild(2)]
    public static void OnPostProcessingBuild(BuildTarget target, string path) {

        if (target != BuildTarget.iOS) return;

        var plistPath = Path.Combine(path, "Info.plist");
		var plist = new PlistDocument();
		plist.ReadFromFile(plistPath);
        var rootDict = plist.root;

        var existsOnSuspendKey = "UIApplicationExitsOnSuspend";
        if (rootDict.values.ContainsKey(existsOnSuspendKey)) {
            rootDict.values.Remove(existsOnSuspendKey);
        }

        rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        plist.WriteToFile(plistPath);
    }
}
