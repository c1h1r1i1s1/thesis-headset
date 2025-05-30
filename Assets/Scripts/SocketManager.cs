using NativeWebSocket;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance;
    private WebSocket websocket;

    // Event for bounding boxes
    public delegate void OnBoxesReceived(List<BoundingBoxData> boxes);
    public static event OnBoxesReceived BoxesReceived;

    private void Awake() => Instance = this;

    async void Start()
    {
        websocket = new WebSocket("ws://192.168.1.13:12345");

        websocket.OnMessage += (bytes) =>
        {
            var json = Encoding.UTF8.GetString(bytes);

            var boxList = JsonUtility.FromJson<BoxListWrapper>(json);
            BoxesReceived?.Invoke(boxList.boxes);

        };

        await websocket.Connect();
    }

    public async void SendSelectionUpdate(int id, bool isSelected)
    {
        string json = $"{{\"comType\":\"selection\",\"id\":{id},\"selected\":{isSelected.ToString().ToLower()}}}";
        await websocket.SendText(json);
    }

    public async void SendPosConfirmation()
    {
        string json = $"{{\"comType\":\"posConfirm\"}}";
        await websocket.SendText(json);
    }
    
    private void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
    #endif
    }

    // --- JSON helper classes and structs --- //
    
    [System.Serializable]
    public class BoundingBoxData
    {
        public int id;
        public int label;
        public float x, y, z;
        public float w, h, d;
    }

    [System.Serializable]
    private class BoxListWrapper
    {
        public List<BoundingBoxData> boxes;
    }
}
