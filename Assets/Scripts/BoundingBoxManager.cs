using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoundingBoxManager : MonoBehaviour
{
    public static BoundingBoxManager Instance;

    [Tooltip("Prefab with an BBoxHandler component to display each box.")]
    public GameObject boxPrefab;

    public GameObject zedBox;

    // Holds instantiated boxes by their ID
    private readonly Dictionary<int, GameObject> boxes = new();

    // Calibration: camera-space → headset-space
    private Vector3 cameraWorldPos;
    private Quaternion cameraWorldRot;
    private Transform headTransform; // Head position tracking from headset origin

    private enum GameState {
        Posing,
        Selecting,
        Hiding
    }
    private GameState gameState;

    private Dictionary<int, bool> categoryDict = new Dictionary<int, bool>();

    private void Awake()
    {
        // Assume your MainCamera is your XR camera
        headTransform = Camera.main.transform;
    }

    private void Start() {
        gameState = GameState.Posing;

        for (int i=0; i<80; i++) {
            categoryDict.Add(i, false);
        }
    }

    public void PosConfim()
    {
        Vector3 headSpacePos = headTransform.InverseTransformPoint(zedBox.transform.position);
        Quaternion headSpaceRot = Quaternion.Inverse(headTransform.rotation) * zedBox.transform.rotation;

        cameraWorldPos = headTransform.TransformPoint(headSpacePos);
        cameraWorldRot = headTransform.rotation * headSpaceRot;
        
        gameState = GameState.Selecting;
        Debug.Log($"Calibrated! cameraWorldPos={cameraWorldPos}, cameraWorldRot={cameraWorldRot.eulerAngles}");
    }

    private void OnEnable()
    {
        SocketManager.BoxesReceived += OnBoxesReceived;
    }

    private void OnDisable()
    {
        SocketManager.BoxesReceived -= OnBoxesReceived;
    }

    private void OnBoxesReceived(List<SocketManager.BoundingBoxData> newBoxes)
    {
        if (gameState != GameState.Selecting) return;

        var currentIds = new HashSet<int>(newBoxes.Select(b => b.id));

        // Destroy any existing boxes whose IDs are NOT in that set
        var keysToRemove = boxes.Keys
            .Where(id => !currentIds.Contains(id))
            .ToList();

        foreach (int id in keysToRemove)
        {
            Destroy(boxes[id]);
            boxes.Remove(id);
        }

        // Now update or create the remaining boxes
        foreach (var data in newBoxes)
        {
            if (!boxes.TryGetValue(data.id, out var box))
            {
                box = Instantiate(boxPrefab);
                var metadata = box.GetComponent<BBoxHandler>();
                
                if (categoryDict[data.label]) {
                    metadata.Init(data.id, data.label.ToString(), true);
                    boxes[data.id] = box;
                } else {
                    metadata.Init(data.id, data.label.ToString(), false);
                    boxes[data.id] = box;
                }
            }

            UpdateBoxTransform(box, data);
        }
    }

    public async void OnToggleSelection(int id, bool isSelected) {
        categoryDict[id] = isSelected;
    }

    private void UpdateBoxTransform(GameObject box, SocketManager.BoundingBoxData data)
    {
        Vector3 centerCam = new Vector3(-data.x, data.y, data.z);
        Vector3 sizeM = new Vector3(data.w, data.h, data.d);

        Vector3 worldPos = cameraWorldRot * centerCam + cameraWorldPos;
        box.transform.SetPositionAndRotation(worldPos, cameraWorldRot);
        box.transform.localScale = sizeM;
    }
}
