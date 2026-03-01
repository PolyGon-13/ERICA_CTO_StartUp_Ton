using UnityEngine;
using System.Collections.Generic;

public class ParkingLotManager : MonoBehaviour
{
    public List<Transform> parkingSpots;
    public List<Light> ledLights;
    public LayerMask carLayer;
    public float detectionRadius = 1.0f;
    public Color occupiedColor = Color.red;
    public Color emptyColor = Color.green;

    void Update()
    {
        int count = Mathf.Min(parkingSpots.Count, ledLights.Count);
        float sqrRadius = detectionRadius * detectionRadius;
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        for (int i = 0; i < count; i++)
        {
            if (parkingSpots[i] == null || ledLights[i] == null) continue;

            bool isOccupied = false;

            foreach (GameObject obj in allObjects)
            {
                if (((1 << obj.layer) & carLayer) != 0)
                {
                    float sqrDistance = (parkingSpots[i].position - obj.transform.position).sqrMagnitude;
                    if (sqrDistance <= sqrRadius)
                    {
                        isOccupied = true;
                        break;
                    }
                }
            }

            if (isOccupied)
            {
                ledLights[i].color = occupiedColor;
            }
            else
            {
                ledLights[i].color = emptyColor;
            }
        }
    }
}