using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class FigmaToUnityCanvas : EditorWindow
{
    private string jsonString = "";

    [MenuItem("Tools/Figma to Unity UI")]
    public static void ShowWindow()
    {
        GetWindow<FigmaToUnityCanvas>("Figma UI Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Dán dữ liệu JSON từ Figma vào đây:", EditorStyles.boldLabel);
        jsonString = EditorGUILayout.TextArea(jsonString, GUILayout.Height(200));

        if (GUILayout.Button("Build Canvas UI", GUILayout.Height(40)))
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                ParseAndBuildUI(jsonString);
            }
            else
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng dán chuỗi JSON vào trước!", "OK");
            }
        }
    }

    private static void ParseAndBuildUI(string json)
    {
        // Sử dụng JsonUtility tích hợp sẵn của Unity thông qua một class bọc dữ liệu giả lập 
        // Hoặc parse thủ công/SimpleJSON để đọc nhanh cấu trúc phức tạp.
        // Để nhanh gọn và chạy ngay không cần cài thêm thư viện, ta sử dụng Newtonsoft.Json (có sẵn nếu dùng Unity mới hoặc thông qua Package Manager)
        // Nếu dự án chưa có Newtonsoft.Json, hãy cài qua Package Manager hoặc thay thế bằng thư viện bạn đang có.
        
        try
        {
            var data = Newtonsoft.Json.Linq.JObject.Parse(json);
            
            // 1. Tạo Canvas Gốc
            GameObject canvasGO = new GameObject((string)data["screen"] ?? "Canvas_Figma");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2((float)data["canvas"]["referenceResolution"]["width"], (float)data["canvas"]["referenceResolution"]["height"]);
            scaler.matchWidthOrHeight = (float)data["canvas"]["match"];
            
            canvasGO.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Figma UI");

            // 2. Tạo Background nếu có
            if (data["background"] != null)
            {
                GameObject bgGO = new GameObject((string)data["background"]["name"]);
                bgGO.transform.SetParent(canvasGO.transform, false);
                Image bgImage = bgGO.AddComponent<Image>();
                bgImage.color = Color.black; // Mặc định nếu chưa có sprite

                RectTransform rect = bgGO.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
            }

            // 3. Tạo các Elements con
            var elements = data["elements"];
            if (elements != null)
            {
                foreach (var el in elements)
                {
                    CreateUIElement(el, canvasGO.transform);
                }
            }

            EditorUtility.DisplayDialog("Thành công", "Đã dựng xong UI hoàn chỉnh lên Canvas!", "Tuyệt vời");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Lỗi Parse JSON", e.Message, "OK");
        }
    }

    private static void CreateUIElement(Newtonsoft.Json.Linq.JToken el, Transform parent)
    {
        string type = (string)el["type"];
        string name = (string)el["name"];
        
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();

        // Set Vị trí (nếu có)
        if (el["anchoredPosition"] != null)
        {
            rect.anchoredPosition = new Vector2((float)el["anchoredPosition"]["x"], (float)el["anchoredPosition"]["y"]);
        }

        // Tùy biến theo Type Component
        switch (type)
        {
            case "Text":
                TextMeshProUGUI textComp = go.AddComponent<TextMeshProUGUI>();
                textComp.text = (string)el["text"];
                textComp.fontSize = (float)el["fontSize"];
                textComp.alignment = GetTMPAlignment((string)el["alignment"]);
                if (el["color"] != null) textComp.color = HexToColor((string)el["color"]);
                break;

            case "Panel":
                Image panelImg = go.AddComponent<Image>();
                panelImg.color = new Color(1, 1, 1, 0.1f); // Trong suốt nhẹ để phân biệt

                // Xử lý Layout Groups
                string layout = (string)el["layout"];
                if (layout == "VerticalLayoutGroup")
                {
                    var vLayout = go.AddComponent<VerticalLayoutGroup>();
                    vLayout.spacing = (float)el["spacing"];
                    vLayout.childAlignment = GetTextAnchor((string)el["childAlignment"]);
                    vLayout.childControlHeight = false; vLayout.childControlWidth = false;
                    vLayout.childForceExpandHeight = false; vLayout.childForceExpandWidth = false;
                }
                else if (layout == "HorizontalLayoutGroup")
                {
                    var hLayout = go.AddComponent<HorizontalLayoutGroup>();
                    hLayout.spacing = (float)el["spacing"];
                    hLayout.childAlignment = GetTextAnchor((string)el["childAlignment"]);
                    hLayout.childControlHeight = false; hLayout.childControlWidth = false;
                    hLayout.childForceExpandHeight = false; hLayout.childForceExpandWidth = false;
                }

                // Sinh các con nằm trong Panel (đệ quy)
                if (el["children"] != null)
                {
                    foreach (var child in el["children"])
                    {
                        CreateUIElement(child, go.transform);
                    }
                }
                break;

            case "Button":
                Image btnImg = go.AddComponent<Image>();
                if (el["backgroundColor"] != null) btnImg.color = HexToColor((string)el["backgroundColor"]);
                go.AddComponent<Button>();

                // Set kích thước nút
                float w = el["width"] != null ? (float)el["width"] : (el["size"] != null ? (float)el["size"] : 100);
                float h = el["height"] != null ? (float)el["height"] : (el["size"] != null ? (float)el["size"] : 100);
                rect.sizeDelta = new Vector2(w, h);

                // Tạo text bên trong Button nếu có thuộc tính text
                if (el["text"] != null)
                {
                    GameObject txtGO = new GameObject("Text (TMP)");
                    txtGO.transform.SetParent(go.transform, false);
                    TextMeshProUGUI btnTxt = txtGO.AddComponent<TextMeshProUGUI>();
                    btnTxt.text = (string)el["text"];
                    btnTxt.fontSize = (float)el["fontSize"];
                    btnTxt.alignment = TextAlignmentOptions.Center;
                    if (el["textColor"] != null) btnTxt.color = HexToColor((string)el["textColor"]);
                    
                    RectTransform txtRect = txtGO.GetComponent<RectTransform>();
                    txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
                    txtRect.sizeDelta = Vector2.zero;
                }
                break;
        }
    }

    // Hàm phụ trợ đổi mã Hex sang Color trong Unity
    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color)) return color;
        return Color.white;
    }

    private static TextAlignmentOptions GetTMPAlignment(string align)
    {
        switch (align)
        {
            case "Center": return TextAlignmentOptions.Center;
            case "BottomRight": return TextAlignmentOptions.BottomRight;
            default: return TextAlignmentOptions.TopLeft;
        }
    }

    private static TextAnchor GetTextAnchor(string anchor)
    {
        switch (anchor)
        {
            case "MiddleCenter": return TextAnchor.MiddleCenter;
            default: return TextAnchor.UpperLeft;
        }
    }
}