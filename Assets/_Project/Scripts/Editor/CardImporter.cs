using UnityEngine;
using UnityEditor;
using System.IO;

public class CardImporter : EditorWindow
{
    [MenuItem("Quantitative Easing/Import Cards from CSV")]
    public static void ImportCards()
    {
        // 1. Find the CSV file
        string path = "Assets/_Project/Resources_Data/Cards.csv";
        if (!File.Exists(path))
        {
            Debug.LogError($"Could not find CSV at {path}");
            return;
        }

        // 2. Read all lines
        string[] lines = File.ReadAllLines(path);

        // 3. Loop through lines (Skip row 0 because it is headers)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // SPLIT by Semicolon (European Standard)
            string[] data = line.Split(';');

            // Safety check: Do we have enough columns?
            if (data.Length < 9)
            {
                Debug.LogWarning($"Skipping row {i}: Not enough columns.");
                continue;
            }

            // 4. Parse Data
            string id = data[0];
            string cardName = data[1];
            string sector = data[2];
            int tier = int.Parse(data[3]);
            int cost = int.Parse(data[4]);
            int yieldVal = int.Parse(data[5]);
            int vol = int.Parse(data[6]);
            bool illegal = bool.Parse(data[7].ToLower()); // Handles "TRUE" or "true"
            string desc = data[8];

            // 5. Create or Update the Asset
            CreateCardAsset(id, cardName, sector, tier, cost, yieldVal, vol, illegal, desc);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Success! Cards Imported.");
    }

    static void CreateCardAsset(string id, string name, string sector, int tier, int cost, int yieldVal, int vol, bool illegal, string desc)
    {
        string folderPath = "Assets/_Project/Resources_Data/Cards";

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            // Simple check: assuming Resources_Data exists, create Cards inside it
            // For robust code, we manually create folders, but for now assuming you made the folder manually
        }

        string assetPath = $"{folderPath}/{id}.asset";

        // Load existing or create new
        CardData card = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);
        if (card == null)
        {
            card = ScriptableObject.CreateInstance<CardData>();
            AssetDatabase.CreateAsset(card, assetPath);
        }

        // Apply Data
        card.id = id;
        card.cardName = name;
        card.sector = sector;
        card.tier = tier;
        card.cashCost = cost;
        card.baseYield = yieldVal;
        card.volatility = vol;
        card.isIllegal = illegal;
        card.description = desc;

        // Mark as "Dirty" so Unity knows to save it
        EditorUtility.SetDirty(card);
    }
}