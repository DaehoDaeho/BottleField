using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class WeaponDataCsvImporter
{
    private const string csvAssetPath = "Assets/Data/WeaponDataCSV.csv";
    private const string weaponDataAssetPath = "Assets/Data/WeaponData.asset";

    /// <summary>
    /// CSV 파일을 읽어서 WeaponData ScriptableObject 에셋을 생성하거나 갱신한다.
    /// </summary>
    [MenuItem("Tools/FPS/Import Weapon Data CSV")]
    public static void Import()
    {
        TextAsset csvTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(csvAssetPath);

        if (csvTextAsset == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + csvAssetPath);
            return;
        }

        EnsureFolder("Assets/Data");

        WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(weaponDataAssetPath);

        if (weaponData == null)
        {
            weaponData = ScriptableObject.CreateInstance<WeaponData>();
            AssetDatabase.CreateAsset(weaponData, weaponDataAssetPath);
        }

        List<GunData> parsedGunDatas = ParseCsv(csvTextAsset.text);

        weaponData.ReplaceGunDatas(parsedGunDatas);

        EditorUtility.SetDirty(weaponData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = weaponData;

        Debug.Log("WeaponData CSV 가져오기 완료. 생성된 총기 데이터 개수: " + parsedGunDatas.Count);
    }

    /// <summary>
    /// CSV 전체 문자열을 읽어서 GunData 리스트로 변환한다.
    /// </summary>
    private static List<GunData> ParseCsv(string csvText)
    {
        List<List<string>> rows = ReadCsvRows(csvText);
        List<GunData> gunDatas = new List<GunData>();

        if (rows.Count <= 1)
        {
            Debug.LogWarning("CSV에 데이터 행이 없습니다.");
            return gunDatas;
        }

        Dictionary<string, int> headerIndexes = BuildHeaderIndexMap(rows[0]);

        for (int rowIndex = 1; rowIndex < rows.Count; ++rowIndex)
        {
            List<string> row = rows[rowIndex];

            if (IsEmptyRow(row) == true)
            {
                continue;
            }

            GunData gunData = new GunData();

            gunData.gunName = GetString(row, headerIndexes, "gunName", "Unnamed");
            gunData.isAutomatic = GetBool(row, headerIndexes, "isAutomatic", false);
            gunData.fireInterval = GetFloat(row, headerIndexes, "fireInterval", 0.25f);
            gunData.maxDistance = GetFloat(row, headerIndexes, "maxDistance", 100.0f);
            gunData.debugRayColor = GetColor(row, headerIndexes, "debugRayColor", Color.yellow);
            gunData.damage = GetFloat(row, headerIndexes, "damage", 10.0f);
            gunData.hitLayerMask = GetLayerMask(row, headerIndexes, "hitLayerNames");
            gunData.hitEffectPrefab = GetPrefab(row, headerIndexes, "hitEffectPrefabPath", rowIndex + 1);
            gunData.magazineSize = GetInt(row, headerIndexes, "magazineSize", 30);
            gunData.startReserveAmmo = GetInt(row, headerIndexes, "startReserveAmmo", 90);
            gunData.reloadDuration = GetFloat(row, headerIndexes, "reloadDuration", 1.5f);
            gunData.pelletCount = GetInt(row, headerIndexes, "pelletCount", 1);
            gunData.spreadAngle = GetFloat(row, headerIndexes, "spreadAngle", 0.0f);
            gunData.recoilPitch = GetFloat(row, headerIndexes, "recoilPitch", 2.0f);
            gunData.recoilReturnSpeed = GetFloat(row, headerIndexes, "recoilReturnSpeed", 14.0f);
            gunData.shakeDuration = GetFloat(row, headerIndexes, "shakeDuration", 0.08f);
            gunData.shakeStrength = GetFloat(row, headerIndexes, "shakeStrength", 0.03f);

            gunDatas.Add(gunData);
        }

        return gunDatas;
    }

    /// <summary>
    /// CSV 문자열을 행과 셀 단위로 분리한다.
    /// </summary>
    private static List<List<string>> ReadCsvRows(string csvText)
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> currentRow = new List<string>();
        StringBuilder currentCell = new StringBuilder();

        bool isInsideQuote = false;

        for (int characterIndex = 0; characterIndex < csvText.Length; ++characterIndex)
        {
            char currentCharacter = csvText[characterIndex];

            if (currentCharacter == '"')
            {
                bool isEscapedQuote = isInsideQuote == true
                    && characterIndex + 1 < csvText.Length
                    && csvText[characterIndex + 1] == '"';

                if (isEscapedQuote == true)
                {
                    currentCell.Append('"');
                    ++characterIndex;
                }
                else
                {
                    isInsideQuote = !isInsideQuote;
                }
            }
            else if (currentCharacter == ',' && isInsideQuote == false)
            {
                currentRow.Add(currentCell.ToString().Trim());
                currentCell.Clear();
            }
            else if ((currentCharacter == '\n' || currentCharacter == '\r') && isInsideQuote == false)
            {
                if (currentCharacter == '\r'
                    && characterIndex + 1 < csvText.Length
                    && csvText[characterIndex + 1] == '\n')
                {
                    ++characterIndex;
                }

                currentRow.Add(currentCell.ToString().Trim());
                currentCell.Clear();

                if (IsEmptyRow(currentRow) == false)
                {
                    rows.Add(currentRow);
                }

                currentRow = new List<string>();
            }
            else
            {
                currentCell.Append(currentCharacter);
            }
        }

        currentRow.Add(currentCell.ToString().Trim());

        if (IsEmptyRow(currentRow) == false)
        {
            rows.Add(currentRow);
        }

        return rows;
    }

    /// <summary>
    /// CSV 첫 번째 행을 읽어서 컬럼 이름과 인덱스를 연결한다.
    /// </summary>
    private static Dictionary<string, int> BuildHeaderIndexMap(List<string> headerRow)
    {
        Dictionary<string, int> headerIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < headerRow.Count; ++index)
        {
            string headerName = headerRow[index].Trim().TrimStart('\uFEFF');

            if (string.IsNullOrWhiteSpace(headerName) == true)
            {
                continue;
            }

            if (headerIndexes.ContainsKey(headerName) == false)
            {
                headerIndexes.Add(headerName, index);
            }
        }

        return headerIndexes;
    }

    /// <summary>
    /// 특정 컬럼의 문자열 값을 가져온다.
    /// </summary>
    private static string GetString(
        List<string> row,
        Dictionary<string, int> headerIndexes,
        string columnName,
        string defaultValue)
    {
        if (headerIndexes.ContainsKey(columnName) == false)
        {
            return defaultValue;
        }

        int columnIndex = headerIndexes[columnName];

        if (columnIndex < 0 || columnIndex >= row.Count)
        {
            return defaultValue;
        }

        string value = row[columnIndex];

        if (string.IsNullOrWhiteSpace(value) == true)
        {
            return defaultValue;
        }

        return value.Trim();
    }

    /// <summary>
    /// 특정 컬럼의 int 값을 가져온다.
    /// </summary>
    private static int GetInt(
        List<string> row,
        Dictionary<string, int> headerIndexes,
        string columnName,
        int defaultValue)
    {
        string value = GetString(row, headerIndexes, columnName, string.Empty);

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue) == true)
        {
            return parsedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// 특정 컬럼의 float 값을 가져온다.
    /// </summary>
    private static float GetFloat(
        List<string> row,
        Dictionary<string, int> headerIndexes,
        string columnName,
        float defaultValue)
    {
        string value = GetString(row, headerIndexes, columnName, string.Empty);

        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue) == true)
        {
            return parsedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// 특정 컬럼의 bool 값을 가져온다.
    /// </summary>
    private static bool GetBool(
        List<string> row,
        Dictionary<string, int> headerIndexes,
        string columnName,
        bool defaultValue)
    {
        string value = GetString(row, headerIndexes, columnName, string.Empty).ToLowerInvariant();

        if (value == "true" || value == "1" || value == "yes" || value == "y")
        {
            return true;
        }

        if (value == "false" || value == "0" || value == "no" || value == "n")
        {
            return false;
        }

        return defaultValue;
    }

    /// <summary>
    /// 특정 컬럼의 색상 값을 가져온다.
    /// </summary>
    private static Color GetColor(
        List<string> row,
        Dictionary<string, int> headerIndexes,
        string columnName,
        Color defaultValue)
    {
        string value = GetString(row, headerIndexes, columnName, string.Empty);

        if (ColorUtility.TryParseHtmlString(value, out Color parsedColor) == true)
        {
            return parsedColor;
        }

        return defaultValue;
    }

    /// <summary>
    /// CSV의 레이어 이름 문자열을 LayerMask로 변환한다.
    /// </summary>
    private static LayerMask GetLayerMask(
        List<string> row,
        Dictionary<string, int> headerIndexes,
        string columnName)
    {
        string value = GetString(row, headerIndexes, columnName, string.Empty);

        LayerMask layerMask = new LayerMask();

        if (string.IsNullOrWhiteSpace(value) == true)
        {
            layerMask.value = 0;
            return layerMask;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int directMaskValue) == true)
        {
            layerMask.value = directMaskValue;
            return layerMask;
        }

        string[] layerNames = value.Split('|');
        int maskValue = 0;

        for (int index = 0; index < layerNames.Length; ++index)
        {
            string layerName = layerNames[index].Trim();

            if (string.IsNullOrWhiteSpace(layerName) == true)
            {
                continue;
            }

            int layerIndex = LayerMask.NameToLayer(layerName);

            if (layerIndex < 0)
            {
                Debug.LogWarning("존재하지 않는 레이어 이름입니다: " + layerName);
                continue;
            }

            maskValue |= 1 << layerIndex;
        }

        layerMask.value = maskValue;
        return layerMask;
    }

    /// <summary>
    /// CSV에 적힌 프리팹 경로를 GameObject로 변환한다.
    /// </summary>
    private static GameObject GetPrefab(
        List<string> row,
        Dictionary<string, int> headerIndexes,
        string columnName,
        int rowNumber)
    {
        string assetPath = GetString(row, headerIndexes, columnName, string.Empty);

        if (string.IsNullOrWhiteSpace(assetPath) == true)
        {
            return null;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        if (prefab == null)
        {
            Debug.LogWarning("프리팹을 찾을 수 없습니다. CSV 행: " + rowNumber + ", 경로: " + assetPath);
        }

        return prefab;
    }

    /// <summary>
    /// 비어 있는 CSV 행인지 확인한다.
    /// </summary>
    private static bool IsEmptyRow(List<string> row)
    {
        for (int index = 0; index < row.Count; ++index)
        {
            if (string.IsNullOrWhiteSpace(row[index]) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 지정한 Unity 폴더가 없으면 생성한다.
    /// </summary>
    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath) == true)
        {
            return;
        }

        string[] pathParts = folderPath.Split('/');

        if (pathParts.Length == 0 || pathParts[0] != "Assets")
        {
            Debug.LogError("Assets로 시작하는 폴더 경로만 사용할 수 있습니다: " + folderPath);
            return;
        }

        string currentPath = "Assets";

        for (int index = 1; index < pathParts.Length; ++index)
        {
            string nextPath = currentPath + "/" + pathParts[index];

            if (AssetDatabase.IsValidFolder(nextPath) == false)
            {
                AssetDatabase.CreateFolder(currentPath, pathParts[index]);
            }

            currentPath = nextPath;
        }
    }
}