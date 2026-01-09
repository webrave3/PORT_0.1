using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CardImporter : EditorWindow
{
    // Adjust this path if your CSV is elsewhere
    private const string CSV_PATH = "Assets/_Project/Resources_Data/Cards.csv";

    // Where the ScriptableObjects will be saved
    private const string OUTPUT_PATH = "Assets/_Project/Resources_Data/Cards/";

    [MenuItem("Tools/Import Cards (CSV)")]
    public static void ImportCards()
    {
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError($"CSV not found at: {CSV_PATH}");
            return;
        }

        string[] lines = File.ReadAllLines(CSV_PATH);
        // Skip header row
        if (lines.Length <= 1) return;

        // Ensure output folder exists
        if (!Directory.Exists(OUTPUT_PATH))
        {
            Directory.CreateDirectory(OUTPUT_PATH);
        }

        // Dictionary to hold the mapping of ID -> CardData for the second pass (linking)
        Dictionary<string, CardData> idToAssetMap = new Dictionary<string, CardData>();

        // --- PASS 1: Create/Update Assets ---
        Debug.Log("--- Starting Import Pass 1: Creating Assets ---");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Split by comma, but respect quotes (for descriptions with commas)
            string[] data = SplitCsvLine(line);

            // Columns: 
            // 0: ID, 1: NextTierID, 2: Name, 3: Sector, 4: Tier, 
            // 5: Cost, 6: Yield, 7: Volatility, 8: Illegal, 9: Description

            if (data.Length < 10)
            {
                Debug.LogWarning($"Skipping line {i + 1}: Insufficient columns.");
                continue;
            }

            string id = data[0].Trim();
            string fileName = $"{id}.asset";
            string fullPath = OUTPUT_PATH + fileName;

            // Load existing or create new
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(fullPath);
            if (card == null)
            {
                card = ScriptableObject.CreateInstance<CardData>();
                AssetDatabase.CreateAsset(card, fullPath);
            }

            // Fill Data
            card.id = id;
            // Store nextTierID temporarily? No, we will read CSV again in Pass 2.

            card.cardName = data[2].Trim();
            card.sector = data[3].Trim();
            int.TryParse(data[4], out card.tier);
            int.TryParse(data[5], out card.cashCost);
            int.TryParse(data[6], out card.baseYield);
            int.TryParse(data[7], out card.volatility);
            bool.TryParse(data[8], out card.isIllegal);

            // Handle Description (remove wrapping quotes if present)
            string desc = data[9].Trim();
            if (desc.StartsWith("\"") && desc.EndsWith("\""))
            {
                desc = desc.Substring(1, desc.Length - 2);
            }
            // Fix double quotes becoming single quotes from CSV standard
            desc = desc.Replace("\"\"", "\"");
            card.description = desc;

            EditorUtility.SetDirty(card);

            // Add to dictionary for linking
            if (!idToAssetMap.ContainsKey(id))
            {
                idToAssetMap.Add(id, card);
            }
        }

        AssetDatabase.SaveAssets();

        // --- PASS 2: Link Tiers ---
        Debug.Log("--- Starting Import Pass 2: Linking Evolutions ---");

        for (int i = 1; i < lines.Length; i++)
        {
            string[] data = SplitCsvLine(lines[i]);
            string currentID = data[0].Trim();
            string nextTierID = data[1].Trim();

            if (!string.IsNullOrEmpty(nextTierID))
            {
                if (idToAssetMap.ContainsKey(currentID) && idToAssetMap.ContainsKey(nextTierID))
                {
                    CardData currentCard = idToAssetMap[currentID];
                    CardData nextCard = idToAssetMap[nextTierID];

                    currentCard.nextTierCard = nextCard;
                    EditorUtility.SetDirty(currentCard);
                }
                else
                {
                    Debug.LogWarning($"Could not link {currentID} -> {nextTierID}. Target asset might be missing.");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Import Complete! All cards updated and linked.");
    }

    // Standard CSV splitter that handles commas inside quotes
    private static string[] SplitCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentEntry = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentEntry);
                currentEntry = "";
            }
            else
            {
                currentEntry += c;
            }
        }
        result.Add(currentEntry); // Add last entry

        return result.ToArray();
    }
}