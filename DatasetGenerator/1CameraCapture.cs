using System;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A Unity script that places cameras along roads in a scene, captures images, and performs raycasting for object detection.
/// </summary>
public class CameraPlacer : MonoBehaviour
{
    public GameObject cameraPrefab; // Prefab for the camera to be instantiated
    public float maxSpacing = 50f; // Maximum spacing between cameras
    public float minSpacing = 30f; // Minimum spacing between cameras
    public int gridWidth = 960; // Grid width, corresponding to resolution
    public int gridHeight = 540; // Grid height, corresponding to resolution
    public float fieldOfView = 10f; // Camera's field of view in degrees
    public float aspectRatio = 960f / 540f; // Camera's aspect ratio (width/height)
    public float farClipPlane = 200f; // Camera's far clipping plane distance

    /// <summary>
    /// Called when the script is first initialized. Triggers camera placement on roads.
    /// </summary>
    void Start()
    {
        PlaceCamerasOnEachRoad();
    }

    /// <summary>
    /// Places cameras along each road object tagged with "Road" in the scene.
    /// </summary>
    void PlaceCamerasOnEachRoad()
    {
        GameObject[] roads = GameObject.FindGameObjectsWithTag("Road");
        foreach (GameObject road in roads)
        {
            Renderer renderer = road.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                Vector3 start;
                Vector3 end;
                Vector3 direction;
                Quaternion camRotation;

                float cameraHeight = 6f; // Height at which cameras are placed above the road

                // Determine road orientation and set camera direction
                if (bounds.size.x < bounds.size.z) // Road is longer along Z-axis
                {
                    start = new Vector3(bounds.center.x, bounds.min.y + cameraHeight, bounds.min.z);
                    end = new Vector3(bounds.center.x, bounds.min.y + cameraHeight, bounds.max.z);
                    direction = Vector3.forward;
                    camRotation = Quaternion.LookRotation(direction);
                }
                else // Road is longer along X-axis
                {
                    start = new Vector3(bounds.min.x, bounds.min.y + cameraHeight, bounds.center.z);
                    end = new Vector3(bounds.max.x, bounds.min.y + cameraHeight, bounds.center.z);
                    direction = Vector3.right;
                    camRotation = Quaternion.LookRotation(direction);
                }

                float remainingDistance = Vector3.Distance(start, end) - 50;
                Vector3 currentPosition = start;

                while (remainingDistance > 0)
                {
                    // Random rotation to the base orientation
                    Quaternion randomRotation = Quaternion.Euler(0, UnityEngine.Random.Range(-10, 10), 0);
                    Quaternion finalRotation = camRotation * randomRotation;

                    // Instantiate and configure the center camera
                    GameObject centerCameraInstance = Instantiate(cameraPrefab, currentPosition, finalRotation);
                    SetupCamera(centerCameraInstance);

                    // Calculate offset for side cameras (1/4 of the shorter road dimension)
                    float sideOffset = Math.Min(bounds.size.x, bounds.size.z) / 4;
                    Vector3 sideDirection = (bounds.size.x < bounds.size.z) ? Vector3.right : Vector3.forward;
                    Quaternion sideRotation = Quaternion.Euler(0, UnityEngine.Random.Range(-3, 3), 0);

                    // Instantiate and configure the left-side camera
                    GameObject leftCameraInstance = Instantiate(cameraPrefab, currentPosition - sideDirection * sideOffset, finalRotation * sideRotation);
                    SetupCamera(leftCameraInstance);

                    // Instantiate and configure the right-side camera
                    GameObject rightCameraInstance = Instantiate(cameraPrefab, currentPosition + sideDirection * sideOffset, finalRotation * sideRotation);
                    SetupCamera(rightCameraInstance);

                    // Move to the next position with random spacing
                    float spacing = Mathf.Min(remainingDistance, UnityEngine.Random.Range(minSpacing, maxSpacing));
                    currentPosition += direction * spacing;
                    remainingDistance -= spacing;
                }
            }
        }
    }

    /// <summary>
    /// Configures a camera instance with a render texture and initiates image capture and raycasting.
    /// </summary>
    /// <param name="cameraInstance">The camera GameObject to set up.</param>
    void SetupCamera(GameObject cameraInstance)
    {
        RenderTexture rt = new RenderTexture(960, 540, 24);
        cameraInstance.GetComponent<Camera>().targetTexture = rt;

        StartCoroutine(CaptureImage(rt, 0, cameraInstance.GetComponent<Camera>()));
        CastRaysFromCamera(cameraInstance.GetComponent<Camera>());
    }

    /// <summary>
    /// Captures an image from the camera, saves it, and generates binary images for detected objects.
    /// </summary>
    /// <param name="rt">The render texture to capture from.</param>
    /// <param name="cameraIndex">Index of the camera (for naming purposes).</param>
    /// <param name="camera">The camera component to use for raycasting.</param>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    IEnumerator CaptureImage(RenderTexture rt, int cameraIndex, Camera camera)
    {
        yield return new WaitForEndOfFrame();

        RenderTexture.active = rt;
        Texture2D originalTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        originalTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        originalTexture.Apply();

        // Save the raw image
        string rawFolderPath = Path.Combine(Application.dataPath, "ScreenShoot", "Raw_image");
        if (!Directory.Exists(rawFolderPath))
        {
            Directory.CreateDirectory(rawFolderPath);
        }

        int maxIndex = GetMaxIndex(rawFolderPath, "Cam_", cameraIndex);
        int newCameraIndex = maxIndex + 1;

        string rawFilename = $"Cam_{newCameraIndex}.png";
        string rawPath = Path.Combine(rawFolderPath, rawFilename);
        byte[] rawBytes = originalTexture.EncodeToPNG();
        File.WriteAllBytes(rawPath, rawBytes);

        Debug.Log($"Saved raw image to {rawPath}");

        // Store textures and bounding boxes for detected objects
        Dictionary<GameObject, (Texture2D texture, int minX, int maxX, int minY, int maxY)> objectTextures = new Dictionary<GameObject, (Texture2D, int, int, int, int)>();

        for (int i = 0; i < originalTexture.height; i++)
        {
            for (int j = 0; j < originalTexture.width; j++)
            {
                Vector3 screenPoint = new Vector3(j, i, camera.nearClipPlane);
                Ray ray = camera.ScreenPointToRay(screenPoint);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, camera.farClipPlane))
                {
                    GameObject hitObject = hit.collider.gameObject;
                    if (hitObject.CompareTag("CubeTag"))
                    {
                        if (!objectTextures.ContainsKey(hitObject))
                        {
                            Texture2D newTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                            objectTextures[hitObject] = (newTex, j, j, i, i);

                            // Initialize texture with black pixels
                            for (int y = 0; y < newTex.height; y++)
                            {
                                for (int x = 0; x < newTex.width; x++)
                                {
                                    newTex.SetPixel(x, y, Color.black);
                                }
                            }
                            newTex.Apply();
                        }

                        var entry = objectTextures[hitObject];
                        entry.texture.SetPixel(j, i, Color.white);
                        objectTextures[hitObject] = (entry.texture, Math.Min(entry.minX, j), Math.Max(entry.maxX, j), Math.Min(entry.minY, i), Math.Max(entry.maxY, i));
                    }
                }
            }
        }

        // Save binary images for detected objects
        foreach (var entry in objectTextures)
        {
            GameObject go = entry.Key;
            var (texture, minX, maxX, minY, maxY) = entry.Value;

            if (maxX - minX > 30 && maxY - minY > 30) // Only save if object is large enough
            {
                byte[] bytes = texture.EncodeToPNG();
                string objectName = go.name.Replace("(Clone)", "");
                string filename = $"Cam_{newCameraIndex}_{objectName}.png";

                string folderPath = Path.Combine(Application.dataPath, "ScreenShoot", "Binary_image");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string path = Path.Combine(folderPath, filename);

                File.WriteAllBytes(path, bytes);

                Debug.Log($"Saved segmented image for {go.name} to {path}");
            }
        }

        Destroy(originalTexture); // Clean up
    }


    /// <summary>
    /// Retrieves the highest index from existing files in a folder matching a prefix.
    /// </summary>
    /// <param name="folderPath">The folder to search in.</param>
    /// <param name="prefix">The file name prefix (e.g., "Cam_").</param>
    /// <param name="startIndex">The starting index to compare against.</param>
    /// <returns>The highest index found.</returns>
    int GetMaxIndex(string folderPath, string prefix, int startIndex)
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath);
        FileInfo[] files = directory.GetFiles($"{prefix}*");

        int maxIndex = startIndex;
        foreach (FileInfo file in files)
        {
            string fileName = file.Name;
            string numberPart = fileName.Substring(prefix.Length, fileName.LastIndexOf('.') - prefix.Length);
            if (int.TryParse(numberPart, out int index))
            {
                if (index > maxIndex)
                {
                    maxIndex = index;
                }
            }
        }

        return maxIndex;
    }

    /// <summary>
    /// Casts rays from the camera to simulate its field of view across the far clip plane.
    /// </summary>
    /// <param name="camera">The camera to cast rays from.</param>
    void CastRaysFromCamera(Camera camera)
    {
        Vector3[] corners = CalculateFarClipPlaneCorners(fieldOfView, aspectRatio, farClipPlane, camera.transform.position, camera.transform.eulerAngles);

        // Define the four corners of the far clip plane
        Vector3 topLeft = corners[0];
        Vector3 topRight = corners[1];
        Vector3 bottomLeft = corners[2];
        Vector3 bottomRight = corners[3];

        float maxDistance = farClipPlane;

        // Cast rays across the grid defined by gridWidth and gridHeight
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                float xFactor = j / (float)gridWidth;
                float yFactor = i / (float)gridHeight;

                // Interpolate to find the grid point on the far clip plane
                Vector3 gridPoint = Vector3.Lerp(Vector3.Lerp(corners[0], corners[1], xFactor), Vector3.Lerp(corners[2], corners[3], xFactor), yFactor);
                Vector3 direction = gridPoint - camera.transform.position;

                Ray ray = new Ray(camera.transform.position, direction.normalized);
                Vector3 endPoint = ray.origin + ray.direction * maxDistance;
            }
        }
    }

    /// <summary>
    /// Calculates the four corners of the camera's far clip plane based on its parameters.
    /// </summary>
    /// <param name="fov">Field of view in degrees.</param>
    /// <param name="aspect">Aspect ratio (width/height).</param>
    /// <param name="far">Distance to the far clip plane.</param>
    /// <param name="position">Camera's position in world space.</param>
    /// <param name="rotation">Camera's rotation in Euler angles.</param>
    /// <returns>An array of four Vector3 points representing the corners.</returns>
    Vector3[] CalculateFarClipPlaneCorners(float fov, float aspect, float far, Vector3 position, Vector3 rotation)
    {
        float farHeight = 2.0f * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * far;
        float farWidth = farHeight * aspect;

        Quaternion rot = Quaternion.Euler(rotation);
        Vector3 forward = rot * Vector3.forward;
        Vector3 right = rot * Vector3.right;
        Vector3 up = rot * Vector3.up;

        Vector3 farCenter = position + forward * far;
        Vector3[] corners = new Vector3[4];

        corners[0] = farCenter - (right * farWidth / 2) + (up * farHeight / 2); // 左上角
        corners[1] = farCenter + (right * farWidth / 2) + (up * farHeight / 2); // 右上角
        corners[2] = farCenter - (right * farWidth / 2) - (up * farHeight / 2); // 左下角
        corners[3] = farCenter + (right * farWidth / 2) - (up * farHeight / 2); // 右下角

        return corners;
    }
}