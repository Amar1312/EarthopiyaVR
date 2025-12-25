using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public TextAsset csvFile;


    // Sheet Date is incremet Form Like 25.12.2025 is always before 26.12.2025 and after 24.12.2025 and Format is Also Like that
    [Header("Excel Data (Visible in Inspector)")]
    public List<LocationData> locations = new List<LocationData>();

    [ContextMenu("Load CSV Data")]
    public void LoadCSV()
    {
        locations.Clear();

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] fields = lines[i].Split(',');

            LocationData data = new LocationData
            {
                date = fields[0].Trim(),
                latitude = float.Parse(fields[1]),
                longitude = float.Parse(fields[2])
            };

            locations.Add(data);
        }

        Debug.Log($"Loaded {locations.Count} locations");
    }
}
