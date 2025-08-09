//using UnityEngine;
//using UnityEditor;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;
//using Newtonsoft.Json;
//using System.Xml;

//public class SpreadSheetJsonFlexible : EditorWindow
//{
//    [Serializable]
//    public class FieldDefinition
//    {
//        public string fieldName;
//        public FieldType fieldType;
//    }

//    public enum FieldType
//    {
//        String,
//        Int,
//        Float,
//        Bool,
//    }

//    [Serializable]
//    public class TemplateData
//    {
//        public string templateName;
//        public int typesX;
//        public int typesY;
//        public string spreadsheetId;
//        public int startX;
//        public int startY;
//        public string saveFileName;
//        public List<FieldDefinition> fieldDefinitions = new();
//    }

//    private List<TemplateData> templates = new();
//    private int selectedTemplateIndex = 0;

//    private int typesX = 0;
//    private int typesY = 0;
//    private string spreadsheetId = "";
//    private int startX = 0;
//    private int startY = 0;
//    private string saveFileName = "game_data.json";

//    private const string PREF_TYPESX = "ss_json_typesX";
//    private const string PREF_TYPESY = "ss_json_typesY";
//    private const string PREF_ID = "ss_json_spreadsheetId";
//    private const string PREF_X = "ss_json_startX";
//    private const string PREF_Y = "ss_json_startY";
//    private const string PREF_FILENAME = "ss_json_saveFileName";

//    private List<FieldDefinition> fieldDefinitions = new();
//    private Vector2 scroll;

//    private string templateNameInput = "";
//    private static string TemplateFilePath =>
//        Path.Combine(Application.persistentDataPath, "spreadsheet_templates.json");


//    [MenuItem("Tools/Flexible Spreadsheet JSON")]
//    public static void ShowWindow()
//    {
//        GetWindow<SpreadSheetJsonFlexible>("Spreadsheet JSON");
//    }

//    private void OnEnable()
//    {
//        LoadTemplates();
//        //spreadsheetId = EditorPrefs.GetString(PREF_ID, "");
//        //startX = EditorPrefs.GetInt(PREF_X, 0);
//        //startY = EditorPrefs.GetInt(PREF_Y, 0);
//        //saveFileName = EditorPrefs.GetString(PREF_FILENAME, "game_data.json");
//    }

//    private void OnDisable()
//    {
//        EditorPrefs.SetInt(PREF_TYPESX, typesX);
//        EditorPrefs.SetInt(PREF_TYPESY, typesY);
//        EditorPrefs.SetString(PREF_ID, spreadsheetId);
//        EditorPrefs.SetInt(PREF_X, startX);
//        EditorPrefs.SetInt(PREF_Y, startY);
//        EditorPrefs.SetString(PREF_FILENAME, saveFileName);
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("🗂 템플릿 관리", EditorStyles.boldLabel);

//        if (templates.Count > 0)
//        {
//            string[] names = templates.Select(t => t.templateName).ToArray();
//            selectedTemplateIndex = EditorGUILayout.Popup("템플릿 선택", selectedTemplateIndex, names);
//        }
//        else
//        {
//            EditorGUILayout.LabelField("저장된 템플릿 없음");
//        }

//        EditorGUILayout.BeginHorizontal();
//        if (GUILayout.Button("불러오기") && templates.Count > 0)
//        {
//            LoadTemplate(templates[selectedTemplateIndex]);
//        }

//        if (GUILayout.Button("삭제") && templates.Count > 0)
//        {
//            templates.RemoveAt(selectedTemplateIndex);
//            SaveTemplates();
//            selectedTemplateIndex = Mathf.Clamp(selectedTemplateIndex - 1, 0, templates.Count - 1);
//        }
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("새 템플릿 이름:");
//        templateNameInput = EditorGUILayout.TextField(templateNameInput);

//        if (GUILayout.Button("현재 설정 저장") && !string.IsNullOrWhiteSpace(templateNameInput))
//        {
//            var newTemplate = new TemplateData
//            {
//                templateName = templateNameInput,
//                typesX = typesX,
//                typesY = typesY,
//                spreadsheetId = spreadsheetId,
//                startX = startX,
//                startY = startY,
//                saveFileName = saveFileName,
//                fieldDefinitions = new List<FieldDefinition>(fieldDefinitions)
//            };
//            templates.Add(newTemplate);
//            SaveTemplates();
//            templateNameInput = "";
//        }

//        GUILayout.Space(15);
//        GUILayout.Label("📄 Spreadsheet 설정", EditorStyles.boldLabel);


//        spreadsheetId = EditorGUILayout.TextField("스프레드시트 ID", spreadsheetId);
//        EditorGUILayout.LabelField("타입 정의 위치");
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("가로", GUILayout.Width(40));
//        typesX = EditorGUILayout.IntField(typesX, GUILayout.Width((position.width - 40) / 2 - 40));
//        EditorGUILayout.LabelField("세로", GUILayout.Width(40));
//        typesY = EditorGUILayout.IntField(typesY, GUILayout.Width(position.width / 2 - 40));
//        EditorGUILayout.EndHorizontal();


//        EditorGUILayout.LabelField("읽기 시작할 위치");
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("가로", GUILayout.Width(40));
//        startX = EditorGUILayout.IntField(startX, GUILayout.Width((position.width - 40) / 2 - 40));
//        EditorGUILayout.LabelField("세로", GUILayout.Width(40));
//        startY = EditorGUILayout.IntField(startY, GUILayout.Width(position.width / 2 - 40));
//        EditorGUILayout.EndHorizontal();
//        saveFileName = EditorGUILayout.TextField("저장파일 이름", saveFileName);

//        GUILayout.Space(10);
//        GUILayout.Label("필드 정의", EditorStyles.boldLabel);

//        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(200));
//        int removeIndex = -1;
//        for (int i = 0; i < fieldDefinitions.Count; i++)
//        {
//            EditorGUILayout.BeginHorizontal();

//            fieldDefinitions[i].fieldName = EditorGUILayout.TextField(fieldDefinitions[i].fieldName);
//            fieldDefinitions[i].fieldType = (FieldType)EditorGUILayout.EnumPopup(fieldDefinitions[i].fieldType);

//            if (GUILayout.Button("-", GUILayout.Width(20)))
//                removeIndex = i;

//            EditorGUILayout.EndHorizontal();
//        }
//        if (removeIndex >= 0)
//            fieldDefinitions.RemoveAt(removeIndex);
//        EditorGUILayout.EndScrollView();

//        if (GUILayout.Button("필드 추가"))
//            fieldDefinitions.Add(new FieldDefinition());

//        GUILayout.Space(10);
//        if (GUILayout.Button("스프레드시트에서 다운로드"))
//            FetchSheet();
//    }
//    private void LoadTemplate(TemplateData t)
//    {
//        typesX = t.typesX;
//        typesY = t.typesY;
//        spreadsheetId = t.spreadsheetId;
//        startX = t.startX;
//        startY = t.startY;
//        saveFileName = t.saveFileName;
//        fieldDefinitions = new List<FieldDefinition>(t.fieldDefinitions);
//    }
//    private void SaveTemplates()
//    {
//        string json = JsonConvert.SerializeObject(templates, Formatting.Indented);
//        File.WriteAllText(TemplateFilePath, json);
//    }

//    private void LoadTemplates()
//    {
//        if (File.Exists(TemplateFilePath))
//        {
//            string json = File.ReadAllText(TemplateFilePath);
//            templates = JsonConvert.DeserializeObject<List<TemplateData>>(json) ?? new List<TemplateData>();
//        }
//    }
//    private void FetchSheet()
//    {
//        if (string.IsNullOrWhiteSpace(spreadsheetId))
//        {
//            Debug.LogError("Spreadsheet ID를 입력하세요.");
//            return;
//        }

//        if (fieldDefinitions.Count == 0)
//        {
//            Debug.LogError("필드를 하나 이상 정의하세요.");
//            return;
//        }

//        string url = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv";

//        try
//        {
//            using WebClient client = new();
//            string csvText = client.DownloadString(url);
//            string[] lines = csvText.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

//            List<Dictionary<string, object>> allRows = new();

//            for (int i = startY; i < lines.Length; i++)
//            {
//                var row = new Dictionary<string, object>();
//                List<string> columns = ParseCsvLine(lines[i].Trim());

//                for (int j = 0; j < fieldDefinitions.Count; j++)
//                {
//                    int columnIndex = startX + j;
//                    if (columnIndex >= columns.Count)
//                        continue;

//                    string key = fieldDefinitions[j].fieldName;
//                    FieldType type = fieldDefinitions[j].fieldType;
//                    string raw = columns[columnIndex];

//                    row[key] = ParseValue(raw, type);
//                }

//                allRows.Add(row);
//            }

//            SaveJson(allRows);
//            Debug.Log($"✔ 저장 완료: {saveFileName}");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError("❌ 오류 발생: " + ex.Message);
//        }
//    }

//    private object ParseValue(string raw, FieldType type)
//    {
//        try
//        {
//            return type switch
//            {
//                FieldType.Int => int.Parse(raw),
//                FieldType.Float => float.Parse(raw),
//                FieldType.Bool => bool.Parse(raw),
//                _ => raw
//            };
//        }
//        catch
//        {
//            Debug.LogWarning($"⚠️ 파싱 실패: '{raw}' 를 {type}로 변환할 수 없음");
//            return null;
//        }
//    }

//    private void SaveJson(List<Dictionary<string, object>> data)
//    {
//        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
//        string path = Path.Combine(Application.dataPath, saveFileName);
//        File.WriteAllText(path, json, Encoding.UTF8);
//        AssetDatabase.Refresh();
//    }

//    private List<string> ParseCsvLine(string line)
//    {
//        List<string> fields = new();
//        bool inQuotes = false;
//        StringBuilder current = new();

//        for (int i = 0; i < line.Length; i++)
//        {
//            char c = line[i];
//            if (c == '"')
//            {
//                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
//                {
//                    current.Append('"');
//                    i++;
//                }
//                else
//                {
//                    inQuotes = !inQuotes;
//                }
//            }
//            else if (c == ',' && !inQuotes)
//            {
//                fields.Add(current.ToString());
//                current.Clear();
//            }
//            else
//            {
//                current.Append(c);
//            }
//        }

//        fields.Add(current.ToString());
//        return fields;
//    }
//}
