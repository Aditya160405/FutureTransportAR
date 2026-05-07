using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageTracker : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;

    public GameObject CarPrefab;
    public GameObject BoatPrefab;
    public GameObject BulletPrefab;
    public GameObject HyperPrefab;
    public GameObject FlightPrefab;
    public GameObject SpacePrefab;

    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    private Dictionary<string, string> vehicleInfo = new Dictionary<string, string>()
    {
        { "Car",    "AI Electric Car\nAI Navigation | Range: 800km" },
        { "Boat",   "AI Solar Boat\nSolar Powered | Zero Emission" },
        { "Bullet", "Bullet Train\nSpeed: 350km/h | Eco Friendly" },
        { "Hyper",  "Hyperloop System\nSpeed: 1200km/h | Supersonic" },
        { "Flight", "Solar Aircraft\nRenewable Energy | Clean Flight" },
        { "Space",  "Space Travel System\nInterplanetary Transport" },
    };

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            SpawnPrefab(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (spawnedObjects.ContainsKey(trackedImage.referenceImage.name))
            {
                bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
                GameObject obj = spawnedObjects[trackedImage.referenceImage.name];
                obj.SetActive(isTracking);

                VideoPlayer vp = obj.GetComponentInChildren<VideoPlayer>();
                if (isTracking)
                {
                    StopAllExcept(trackedImage.referenceImage.name);
                    if (vp != null && !vp.isPlaying) vp.Play();
                }
                else
                {
                    if (vp != null) vp.Stop();
                }
            }
        }
    }

    void SpawnPrefab(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        GameObject prefabToSpawn = null;

        switch (imageName)
        {
            case "Car":    prefabToSpawn = CarPrefab;    break;
            case "Boat":   prefabToSpawn = BoatPrefab;   break;
            case "Bullet": prefabToSpawn = BulletPrefab; break;
            case "Hyper":  prefabToSpawn = HyperPrefab;  break;
            case "Flight": prefabToSpawn = FlightPrefab; break;
            case "Space":  prefabToSpawn = SpacePrefab;  break;
        }

        if (prefabToSpawn != null)
        {
            GameObject spawnedObj = Instantiate(prefabToSpawn);
            spawnedObj.transform.SetParent(trackedImage.transform);
            spawnedObj.transform.localPosition = Vector3.zero;
            spawnedObj.transform.localRotation = Quaternion.identity;

            Transform quad = spawnedObj.GetComponentInChildren<MeshRenderer>().transform;
            quad.localScale = new Vector3(0.16f, 0.09f, 0.1f);
            quad.localRotation = Quaternion.Euler(90f, 0f, 0f);

            if (vehicleInfo.ContainsKey(imageName))
                AddInfoText(spawnedObj, vehicleInfo[imageName]);

            VideoPlayer vp = spawnedObj.GetComponentInChildren<VideoPlayer>();
            if (vp != null) vp.Play();

            spawnedObjects[imageName] = spawnedObj;
        }
    }

    void AddInfoText(GameObject parent, string info)
    {
        // Just text, no background quad at all
        GameObject textObj = new GameObject("InfoText");
        textObj.transform.SetParent(parent.transform);
        textObj.transform.localPosition = new Vector3(0f, 0f, -0.07f);
        textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        textObj.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = info;
        tm.fontSize = 24;
        tm.color = Color.white;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.UpperCenter;
    }

    void StopAllExcept(string currentName)
    {
        foreach (var kvp in spawnedObjects)
        {
            if (kvp.Key != currentName)
            {
                kvp.Value.SetActive(false);
                VideoPlayer vp = kvp.Value.GetComponentInChildren<VideoPlayer>();
                if (vp != null) vp.Stop();
            }
        }
    }
}