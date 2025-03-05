using UnityEngine;

/// <summary>
/// A Unity script that generates cylinders and prefabs along roads in a scene.
/// </summary>
public class GenerateAlongRoad : MonoBehaviour
{
    public float minDistanceBetweenCylinders; // Minimum distance between cylinders along the road
    public float maxDistanceBetweenCylinders; // Maximum distance between cylinders along the road
    public int numberOfCylinders; // Target number of cylinder pairs to generate
    private GameObject[] prefabs; // Array to store prefabs loaded from the Resources folder

    /// <summary>
    /// Called when the script is first initialized. Loads prefabs and places cylinders/prefabs along roads.
    /// </summary>
    void Start()
    {
        // Load all prefabs from the "Materials" folder in Resources
        prefabs = Resources.LoadAll<GameObject>("Materials");

        // Find all GameObjects in the scene tagged with "Road"
        GameObject[] roads = GameObject.FindGameObjectsWithTag("Road");

        foreach (GameObject roadObject in roads)
        {
            Renderer renderer = roadObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("找到的'road'对象没有Renderer组件", roadObject);
                continue;
            }

            Bounds bounds = renderer.bounds;
            Vector3 startPosition;
            float lengthAlongLongEdge, roadWidth;
            bool isXTheLongEdge = bounds.size.x > bounds.size.z;

            // Determine road orientation and set starting position, length, and width
            if (isXTheLongEdge)
            {
                startPosition = new Vector3(bounds.min.x, bounds.max.y, bounds.center.z);
                lengthAlongLongEdge = bounds.size.x;
                roadWidth = bounds.size.z;
            }
            else
            {
                startPosition = new Vector3(bounds.center.x, bounds.max.y, bounds.min.z);
                lengthAlongLongEdge = bounds.size.z;
                roadWidth = bounds.size.x;
            }

            // Generate cylinders and prefabs along the current road
            GenerateCylindersAndPrefabs(startPosition, lengthAlongLongEdge, isXTheLongEdge, roadWidth);
        }
    }

    /// <summary>
    /// Generates cylinders and prefabs along a road based on its length, orientation, and width.
    /// </summary>
    /// <param name="startPosition">The starting position on the road.</param>
    /// <param name="roadLength">The length of the road along its longer edge.</param>
    /// <param name="isXTheLongEdge">Indicates if the road's longer edge is along the X-axis (true) or Z-axis (false).</param>
    /// <param name="roadWidth">The width of the road (shorter dimension).</param>
    void GenerateCylindersAndPrefabs(Vector3 startPosition, float roadLength, bool isXTheLongEdge, float roadWidth)
    {
        Vector3 direction = isXTheLongEdge ? Vector3.right : Vector3.forward;
        float totalLengthCovered = 0f;
        int cylindersGenerated = 0;
        float sideOffset = roadWidth / 3;

        while (totalLengthCovered < roadLength)
        {
            float distanceBetweenCylinders = Random.Range(minDistanceBetweenCylinders, maxDistanceBetweenCylinders);

            if (totalLengthCovered + distanceBetweenCylinders > roadLength)
                break;

            Vector3 positionOffset = direction * (totalLengthCovered + distanceBetweenCylinders / 2); // 从中心开始偏移
            totalLengthCovered += distanceBetweenCylinders;
            CreateCylinderAndPrefab(startPosition + positionOffset + (isXTheLongEdge ? Vector3.forward : Vector3.right) * sideOffset, isXTheLongEdge);
            CreateCylinderAndPrefab(startPosition + positionOffset - (isXTheLongEdge ? Vector3.forward : Vector3.right) * sideOffset, isXTheLongEdge);
            cylindersGenerated++;
        }

        Debug.Log($"Generated {cylindersGenerated} pairs of cylinders and prefabs along the road.");
    }

    /// <summary>
    /// Creates a cylinder and a randomly selected prefab at the specified position.
    /// </summary>
    /// <param name="basePosition">The base position where the cylinder and prefab should be placed.</param>
    /// <param name="isXTheLongEdge">Indicates if the road's longer edge is along the X-axis (true) or Z-axis (false).</param>
    void CreateCylinderAndPrefab(Vector3 basePosition, bool isXTheLongEdge)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = basePosition + new Vector3(0, 1f, 0);
        cylinder.transform.localScale = new Vector3(0.05f, 1f, 0.05f);

        if (prefabs.Length > 0)
        {
            GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];

            float rotationAngle = 0f;
            if (isXTheLongEdge)
            {
                rotationAngle = 90f;
            }

            Instantiate(randomPrefab, basePosition + new Vector3(0, 2.5f, 0), Quaternion.Euler(0, rotationAngle, 0));
        }
    }
}   