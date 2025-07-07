using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCounterVisual : MonoBehaviour
{
    [SerializeField] private PlateCounter plateCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;

    private List<GameObject> spawnedPlateGameObjectList = new();
    private float plateOffsetY = 0.1f;

    private void Start()
    {
        plateCounter.OnPlateAdded += PlateCounter_OnPlateAdded;
        plateCounter.OnPlateRemoved += PlateCounter_OnPlateRemoved;
    }

    private void PlateCounter_OnPlateRemoved(object sender, System.EventArgs e)
    {
        GameObject lastPlate = spawnedPlateGameObjectList[^1];
        spawnedPlateGameObjectList.Remove(lastPlate);
        Destroy(lastPlate);
    }

    private void PlateCounter_OnPlateAdded(object sender, System.EventArgs e)
    {
        Transform plateTransform = Instantiate(plateVisualPrefab, counterTopPoint);
        plateTransform.localPosition = new Vector3(0, plateOffsetY * spawnedPlateGameObjectList.Count, 0);
        spawnedPlateGameObjectList.Add(plateTransform.gameObject);
    }
}
