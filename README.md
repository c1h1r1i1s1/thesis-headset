# thesis-headset

Unity project for the headset interface used in the **Real-Time Diminished Reality Privacy System**.  
This repository contains the Unity-side components that communicate with the PC backend (AIO_server) and allow a headset user (e.g. Meta Quest 3) to interactively select objects for removal from the live ZED camera feed.

---

## 🎮 How It Works

1. **Start the application** on the headset.  
2. A **ZED bounding box** appears in front of the user.  
   - The user physically aligns this box with the real-world ZED camera to ensure correct perspective matching.  
   - Press **OK** to confirm alignment.  
3. The system then displays **bounding boxes** around detected objects.  
4. The user can **point and click** on boxes to select or deselect objects for removal.  
   - Selected objects are sent back to the PC server.  
   - The backend inpaints these objects in real-time and streams the updated feed.  

---

## 🧩 Project Structure

### `Assets/`
Contains all the key Unity content:

- **Shaders**
  - `BBoxOcclusionUnlit` → custom unlit shader for rendering bounding boxes with occlusion handling.

- **Scripts**
  - `BBoxHandler` → handles user interaction with individual bounding boxes.  
  - `BoundingBoxManager` → manages creation, updating, and removal of all bounding boxes in the scene.  
  - `SocketManager` → handles network communication with the backend server (receives detection data, sends selection states).  
  - `ZEDCameraBox` → manages the initial ZED alignment box for camera-to-headset calibration.

- **Prefabs**
  - `BoundingBoxPrefab` → template for interactive bounding boxes (clickable/selectable).  
  - `ZEDBB` → prefab for the ZED alignment box.

### `Packages/` and `ProjectSettings/`
Standard Unity project metadata and package definitions.

---

## ⚙️ Requirements

- **Unity 2022+** (tested version).  
- **Meta Quest headset** (Quest 3 recommended).  
- **PC backend** (`AIO_server`) running with ZED camera + segmentation/inpainting engine.  
- Local network connection between headset and server.

---

## 🚀 Running the App

1. Open the project in Unity Hub.  
2. Build and deploy to your headset (e.g., Oculus Link or Android build).  
3. Start the backend (`AIO_server` and `inpaint_manager`) on your PC.  
4. Run the headset app and follow the alignment procedure.  
5. Select bounding boxes to remove objects in real time.
