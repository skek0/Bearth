using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;

public class SpreadSheetJson : EditorWindow
{
    [Serializable]
    public class TemplateData
    {
        public string templateName;
        public int typesX;
        public int typesY;
        public string spreadsheetId;
        public int startX;
        public int startY;

        public bool wrapWithRootKey;
        public string rootKey;

        public string saveFileName;
        public string saveDirectory;

        public bool enableDataStart;
    }

    private List<TemplateData> templates = new();
    private int selectedTemplateIndex = 0;

    private int typesX = 0;
    private int typesY = 0;
    private string spreadsheetId = "";
    private int startX = 0;
    private int startY = 0;

    // RootKey 래핑 옵션 (기본: 배열 루트)
    private bool wrapWithRootKey = false;
    private string rootKey = "rootKey";

    private string saveFileName = "game_data.json";
    private string saveDirectory = "Assets/Data/Generated";
    private bool enableDataStart = false;

    private string templateNameInput = "";

    private static string TemplateFilePath =>
        Path.Combine(Application.persistentDataPath, "spreadsheet_templates.txt");

    [MenuItem("Tools/Spreadsheet JSON")]
    public static void ShowWindow()
    {
        GetWindow<SpreadSheetJson>("Spreadsheet JSON");
    }

    private void OnEnable()
    {
        LoadTemplates();

        // (선택) 에디터 종료/재시작시 마지막 값 복구
        typesX = EditorPrefs.GetInt("ss_json_typesX", typesX);
        typesY = EditorPrefs.GetInt("ss_json_typesY", typesY);
        spreadsheetId = EditorPrefs.GetString("ss_json_spreadsheetId", spreadsheetId);
        startX = EditorPrefs.GetInt("ss_json_startX", startX);
        startY = EditorPrefs.GetInt("ss_json_startY", startY);

        wrapWithRootKey = EditorPrefs.GetBool("ss_json_wrapWithRootKey", wrapWithRootKey);
        rootKey = EditorPrefs.GetString("ss_json_rootKey", rootKey);

        saveFileName = EditorPrefs.GetString("ss_json_saveFileName", saveFileName);
        saveDirectory = EditorPrefs.GetString("ss_json_saveDirectory", saveDirectory);

        enableDataStart = EditorPrefs.GetBool("ss_json_enableDataStart", enableDataStart);
    }

    private void OnDisable()
    {
        EditorPrefs.SetInt("ss_json_typesX", typesX);
        EditorPrefs.SetInt("ss_json_typesY", typesY);
        EditorPrefs.SetString("ss_json_spreadsheetId", spreadsheetId);
        EditorPrefs.SetInt("ss_json_startX", startX);
        EditorPrefs.SetInt("ss_json_startY", startY);

        EditorPrefs.SetBool("ss_json_wrapWithRootKey", wrapWithRootKey);
        EditorPrefs.SetString("ss_json_rootKey", rootKey);

        EditorPrefs.SetString("ss_json_saveFileName", saveFileName);
        EditorPrefs.SetString("ss_json_saveDirectory", saveDirectory);

        EditorPrefs.SetBool("ss_json_enableDataStart", enableDataStart);
    }

    private void OnGUI()
    {
        GUILayout.Label("템플릿 관리", EditorStyles.boldLabel);

        if (templates.Count > 0)
        {
            string[] names = templates.Select(t => t.templateName).ToArray();
            selectedTemplateIndex = EditorGUILayout.Popup("템플릿 선택", selectedTemplateIndex, names);
        }
        else
        {
            EditorGUILayout.LabelField("저장된 템플릿 없음");
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("불러오기") && templates.Count > 0)
        {
            LoadTemplate(templates[selectedTemplateIndex]);
        }

        if (GUILayout.Button("삭제") && templates.Count > 0)
        {
            templates.RemoveAt(selectedTemplateIndex);
            SaveTemplates();
            selectedTemplateIndex = Mathf.Clamp(selectedTemplateIndex - 1, 0, templates.Count - 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("새 템플릿 이름:");
        templateNameInput = EditorGUILayout.TextField(templateNameInput);

        if (GUILayout.Button("현재 설정 저장") && !string.IsNullOrWhiteSpace(templateNameInput))
        {
            var newTemplate = new TemplateData
            {
                templateName = templateNameInput,
                typesX = typesX,
                typesY = typesY,
                spreadsheetId = spreadsheetId,
                startX = startX,
                startY = startY,
                saveFileName = saveFileName,
                saveDirectory = saveDirectory,

                wrapWithRootKey = wrapWithRootKey,
                rootKey = rootKey,

                enableDataStart = enableDataStart,
            };
            templates.Add(newTemplate);
            SaveTemplates();
            templateNameInput = "";
        }

        GUILayout.Space(15);
        GUILayout.Label("Spreadsheet 설정", EditorStyles.boldLabel);

        spreadsheetId = EditorGUILayout.TextField("스프레드시트 ID", spreadsheetId);

        EditorGUILayout.LabelField("타입명 위치 ( 0 ~ )");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("가로 시작 (X)", GUILayout.Width(80));
        typesX = EditorGUILayout.IntField(typesX);
        EditorGUILayout.LabelField("세로 시작 (Y)", GUILayout.Width(80));
        typesY = EditorGUILayout.IntField(typesY);
        EditorGUILayout.EndHorizontal();

        enableDataStart = EditorGUILayout.ToggleLeft("데이터 시작위치 수동 지정", enableDataStart);

        // startX는 항상 typesX에 고정
        startX = typesX;

        // startY만 자동/수동
        if (!enableDataStart)
        {
            startY = typesY + 2;
        }

        using (new EditorGUI.DisabledScope(!enableDataStart))
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("가로 (X)", GUILayout.Width(80));
            EditorGUILayout.IntField(startX); // 표시만 (잠김)

            EditorGUILayout.LabelField("세로 (Y)", GUILayout.Width(80));
            startY = EditorGUILayout.IntField(startY);

            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("JSON 출력 포맷", EditorStyles.boldLabel);

        wrapWithRootKey = EditorGUILayout.ToggleLeft("RootKey로 감싸기 (구형 포맷/다중테이블용)", wrapWithRootKey);
        using (new EditorGUI.DisabledScope(!wrapWithRootKey))
        {
            rootKey = EditorGUILayout.TextField("Json 루트 키", rootKey);
        }

        saveFileName = EditorGUILayout.TextField("저장파일 이름", saveFileName);

        GUILayout.Space(10);
        GUILayout.Label("저장 경로", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        saveDirectory = EditorGUILayout.TextField("폴더 경로", saveDirectory);

        if (GUILayout.Button("선택", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel(
                "JSON 저장 폴더 선택",
                Application.dataPath,
                ""
            );

            if (!string.IsNullOrEmpty(selected))
            {
                if (!selected.StartsWith(Application.dataPath))
                {
                    Debug.LogError("Assets 폴더 내부만 선택 가능합니다.");
                }
                else
                {
                    saveDirectory = "Assets" + selected[Application.dataPath.Length..];
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("스프레드시트에서 다운로드"))
            FetchSheet();
    }

    private void LoadTemplate(TemplateData t)
    {
        typesX = t.typesX;
        typesY = t.typesY;
        spreadsheetId = t.spreadsheetId;

        // startX는 typesX로 고정
        startX = typesX;
        startY = t.startY;

        wrapWithRootKey = t.wrapWithRootKey;
        rootKey = t.rootKey;

        saveFileName = t.saveFileName;
        saveDirectory = t.saveDirectory;
        enableDataStart = t.enableDataStart;
    }

    private void SaveTemplates()
    {
        string json = JsonConvert.SerializeObject(templates, Formatting.Indented);
        File.WriteAllText(TemplateFilePath, json, Encoding.UTF8);
    }

    private void LoadTemplates()
    {
        if (!File.Exists(TemplateFilePath))
            return;

        string json = File.ReadAllText(TemplateFilePath, Encoding.UTF8);
        
        templates = JsonConvert.DeserializeObject<List<TemplateData>>(json) ?? new List<TemplateData>();

        for (int i = 0; i < templates.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(templates[i].rootKey))
                templates[i].rootKey = "rootKey";
            if (string.IsNullOrWhiteSpace(templates[i].saveFileName))
                templates[i].saveFileName = "game_data.json";
            if (string.IsNullOrWhiteSpace(templates[i].saveDirectory))
                templates[i].saveDirectory = "Assets/Data/Generated";
        }
    }

    private void FetchSheet()
    {
        if (string.IsNullOrWhiteSpace(spreadsheetId))
        {
            Debug.LogError("Spreadsheet ID를 입력하세요.");
            return;
        }

        string url = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv";

        try
        {
            using WebClient client = new();
            string csvText = client.DownloadString(url);

            List<string> rows = SplitCsvRows(csvText);

            List<List<string>> grid = rows
                .Select(r => ParseCsvLine(r))
                .ToList();

            if (grid.Count == 0)
            {
                Debug.LogError("CSV가 비어있습니다.");
                return;
            }

            if (typesY < 0 || typesY >= grid.Count)
            {
                Debug.LogError("typesY가 범위를 벗어났습니다.");
                return;
            }

            // 필드 타입과 이름 추출
            List<string> types = grid[typesY].Skip(typesX).ToList();

            int namesRow = typesY + 1;
            if (namesRow < 0 || namesRow >= grid.Count)
            {
                Debug.LogError("필드명 행(typesY + 1)이 범위를 벗어났습니다.");
                return;
            }

            List<string> names = grid[namesRow].Skip(typesX).ToList();
            int fieldCount = Mathf.Min(types.Count, names.Count);

            // ✅ (변경) name/type + 실제 컬럼 인덱스(colIndex) 같이 저장
            var fields = new List<(string name, string type, int colIndex)>(fieldCount);
            for (int i = 0; i < fieldCount; i++)
            {
                if (string.IsNullOrWhiteSpace(names[i]))
                    continue;

                int colIndex = startX + i; // startX(=typesX) 기준
                fields.Add((names[i], types[i], colIndex));
            }

            List<Dictionary<string, object>> allRows = new();

            for (int y = startY; y < grid.Count; y++)
            {
                var columns = grid[y];
                var row = new Dictionary<string, object>();

                // ✅ (변경) startX+i 대신, 헤더에서 확보한 실제 colIndex로 접근
                for (int i = 0; i < fields.Count; i++)
                {
                    int colIndex = fields[i].colIndex;
                    if (colIndex >= columns.Count)
                        continue;

                    string raw = columns[colIndex];
                    row[fields[i].name] = ParseValue(raw, fields[i].type);
                }

                allRows.Add(row);
            }

            SaveJson(allRows);
            Debug.Log($"저장 완료: {saveFileName}");
        }
        catch (Exception ex)
        {
            Debug.LogError("오류 발생: " + ex.Message);
        }
    }
    private List<string> SplitCsvRows(string csv)
    {
        List<string> rows = new();
        StringBuilder current = new();
        bool inQuotes = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];

            if (c == '"')
            {
                // "" → 이스케이프된 따옴표
                if (inQuotes && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
            }
            else if (c == '\n' && !inQuotes)
            {
                rows.Add(current.ToString().TrimEnd('\r'));
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            rows.Add(current.ToString().TrimEnd('\r'));

        return rows;
    }
    private void SaveJson(List<Dictionary<string, object>> data)
    {
        if (!AssetDatabase.IsValidFolder(saveDirectory))
        {
            Debug.LogError($"유효하지 않은 폴더 경로: {saveDirectory}");
            return;
        }

        object output;

        if (wrapWithRootKey)
        {
            if (string.IsNullOrWhiteSpace(rootKey))
            {
                Debug.LogError("RootKey로 감싸기가 켜져 있지만 rootKey가 비어있습니다.");
                return;
            }

            output = new Dictionary<string, object>
            {
                [rootKey] = data
            };
        }
        else
        {
            output = data;
        }

        string json = JsonConvert.SerializeObject(output, Formatting.Indented);

        string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
        string absDir = Path.Combine(projectRoot, saveDirectory);
        string fullPath = Path.Combine(absDir, saveFileName);

        File.WriteAllText(fullPath, json, Encoding.UTF8);
        AssetDatabase.Refresh();
    }

    private object ParseValue(string raw, string type)
    {
        if (raw == null) return null;
        raw = raw.Trim();

        // 빈칸은 null 처리 (원하면 ""로 바꿔도 됨)
        if (raw.Length == 0)
            return null;

        try
        {
            switch (type.Trim().ToLowerInvariant())
            {
                case "int":
                    return int.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture);

                case "float":
                    return float.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture);

                case "bool":
                    // 스프레드시트에서 TRUE/FALSE, true/false, 0/1 등 대응
                    if (raw.Equals("1")) return true;
                    if (raw.Equals("0")) return false;
                    return bool.Parse(raw);

                case "string":
                    return raw;

                default:
                    // 알 수 없는 타입은 raw 그대로
                    return raw;
            }
        }
        catch
        {
            Debug.LogWarning($"파싱 실패: '{raw}' 를 {type}로 변환할 수 없음");
            return null;
        }
    }

    private List<string> ParseCsvLine(string line)
    {
        List<string> fields = new();
        bool inQuotes = false;
        StringBuilder current = new();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
