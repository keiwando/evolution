using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// Based on: https://forum.unity.com/threads/canvashelper-resizes-a-recttransform-to-iphone-xs-safe-area.521107/
 
[RequireComponent(typeof(Canvas))]
public class CanvasHelper : MonoBehaviour {
   public static UnityEvent onOrientationChange = new UnityEvent();
   public static UnityEvent onResolutionChange = new UnityEvent();
   public static bool isLandscape { get; private set; }
   
   private static List<CanvasHelper> helpers = new List<CanvasHelper>();
   
   private static bool screenChangeVarsInitialized = false;
   private static ScreenOrientation lastOrientation = Screen.orientation;
   private static Vector2 lastResolution = Vector2.zero;
   private static Vector2 lastSafeArea = Vector2.zero;
   
   private static Vector2 wantedReferenceResolution = new Vector2(2048f, 2048f);
   private static Camera wantedCanvasCamera;
   
   private Canvas canvas;
   private CanvasScaler scaler;
   private RectTransform rectTransform;
   
   private RectTransform safeAreaTransform;
   
   void Awake()
   {
       if(!helpers.Contains(this))
           helpers.Add(this);
       
       canvas = GetComponent<Canvas>();
       scaler = GetComponent<CanvasScaler>();
       rectTransform = GetComponent<RectTransform>();
       
       UpdateReferenceResolution();
       UpdateCanvasCamera();
       
       safeAreaTransform = transform.Find("SafeArea") as RectTransform;
       
       if(!screenChangeVarsInitialized)
       {
           lastOrientation = Screen.orientation;
           lastResolution.x = Screen.width;
           lastResolution.y = Screen.height;
           lastSafeArea = Screen.safeArea.size;
       
           screenChangeVarsInitialized = true;
       }
   }
   
   void Start()
   {
       ApplySafeArea();
   }
   
   void Update()
   {
       if(helpers[0] != this)
           return;
           
       if(Application.isMobilePlatform)
       {
           if(Screen.orientation != lastOrientation)
               OrientationChanged();
           
           if(Screen.safeArea.size != lastSafeArea)
               SafeAreaChanged();
       }
       else
       {
           //resolution of mobile devices should stay the same always, right?
           // so this check should only happen everywhere else
           if(Screen.width != lastResolution.x || Screen.height != lastResolution.y)
               ResolutionChanged();
       }
   }
   
   void ApplySafeArea()
   {
       if(safeAreaTransform == null)
           return;
       
       var safeArea = Screen.safeArea;
       
       var anchorMin = safeArea.position;
       var anchorMax = safeArea.position + safeArea.size;
       anchorMin.x /= canvas.pixelRect.width;
       anchorMin.y /= canvas.pixelRect.height;
       anchorMax.x /= canvas.pixelRect.width;
       anchorMax.y /= canvas.pixelRect.height;
       
       safeAreaTransform.anchorMin = anchorMin;
       safeAreaTransform.anchorMax = anchorMax;
       
       //Debug.Log(
       //    "ApplySafeArea:" +
       //    "\n Screen.orientation: " + Screen.orientation +
       //    #if UNITY_IOS
       //    "\n Device.generation: " + UnityEngine.iOS.Device.generation.ToString() +
       //    #endif
       //    "\n Screen.safeArea.position: " + Screen.safeArea.position.ToString() +
       //    "\n Screen.safeArea.size: " + Screen.safeArea.size.ToString() +
       //    "\n Screen.width / height: (" + Screen.width.ToString() + ", " + Screen.height.ToString() + ")" +
       //    "\n canvas.pixelRect.size: " + canvas.pixelRect.size.ToString() +
       //    "\n anchorMin: " + anchorMin.ToString() +
       //    "\n anchorMax: " + anchorMax.ToString());
   }
   
   void UpdateCanvasCamera()
   {
       if(canvas.worldCamera == null && wantedCanvasCamera != null)
           canvas.worldCamera = wantedCanvasCamera;
   }
   
   void UpdateReferenceResolution()
   {
       if(scaler.referenceResolution != wantedReferenceResolution)
           scaler.referenceResolution = wantedReferenceResolution;
   }
   
   void OnDestroy()
   {
       if(helpers != null && helpers.Contains(this))
           helpers.Remove(this);
   }
   
   private static void OrientationChanged()
   {
       //Debug.Log("Orientation changed from " + lastOrientation + " to " + Screen.orientation + " at " + Time.time);
       
       lastOrientation = Screen.orientation;
       lastResolution.x = Screen.width;
       lastResolution.y = Screen.height;
       
       isLandscape = lastOrientation == ScreenOrientation.LandscapeLeft || lastOrientation == ScreenOrientation.LandscapeRight || lastOrientation == ScreenOrientation.Landscape;
       onOrientationChange.Invoke();
       
   }
   
   private static void ResolutionChanged()
   {
       if(lastResolution.x == Screen.width && lastResolution.y == Screen.height)
           return;
       
       //Debug.Log("Resolution changed from " + lastResolution + " to (" + Screen.width + ", " + Screen.height + ") at " + Time.time);
       
       lastResolution.x = Screen.width;
       lastResolution.y = Screen.height;
       
       isLandscape = Screen.width > Screen.height;
       onResolutionChange.Invoke();
   }
   
   private static void SafeAreaChanged()
   {
       if(lastSafeArea == Screen.safeArea.size)
           return;
       
       //Debug.Log("Safe Area changed from " + lastSafeArea + " to " + Screen.safeArea.size + " at " + Time.time);
       
       lastSafeArea = Screen.safeArea.size;
       
       for (int i = 0; i < helpers.Count; i++)
       {
           helpers[i].ApplySafeArea();
       }
   }
   
   public static void SetAllCanvasCamera(Camera cam)
   {
       if(wantedCanvasCamera == cam)
           return;
       
       wantedCanvasCamera = cam;
       
       for (int i = 0; i < helpers.Count; i++)
       {
           helpers[i].UpdateCanvasCamera();
       }
   }
   
   public static void SetAllReferenceResolutions(Vector2 newReferenceResolution)
   {
       if(wantedReferenceResolution == newReferenceResolution)
           return;
           
       //Debug.Log("Reference resolution changed from " + wantedReferenceResolution + " to " + newReferenceResolution + " at " + Time.time);
       
       wantedReferenceResolution = newReferenceResolution;
       
       for (int i = 0; i < helpers.Count; i++)
       {
           helpers[i].UpdateReferenceResolution();
       }
   }
   
   public static Vector2 CanvasSize()
   {
       return helpers[0].rectTransform.sizeDelta;
   }
   
   public static Vector2 SafeAreaSize()
   {
       for (int i = 0; i < helpers.Count; i++)
       {
           if(helpers[i].safeAreaTransform != null)
           {
               return helpers[i].safeAreaTransform.sizeDelta;
           }
       }
       
       return CanvasSize();
   }
   
   public static Vector2 GetReferenceResolution()
   {
       return wantedReferenceResolution;
   }
   
   
}