#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace DXLab.GitTool.Editor
{
    public class GitManagerWindow : EditorWindow
    {
        private string gitStatusResult = "Chưa kiểm tra trạng thái.";
        private string unmergedFiles = "";
        private string gitTreeData = "";
        private string[] branchList = new string[0];
        private string currentBranch = "";

        // UI Elements
        private Label lblCurrentBranch;
        private Label lblGitTree;
        private Label lblStatus;
        private DropdownField dropdownBranch;
        private TextField txtRenameBranch;
        private TextField txtNewBranch;
        private TextField txtCommitMsg;
        private VisualElement conflictArea;
        private ScrollView scrollConflict;
        
        private VisualElement progressArea;
        private ProgressBar progressBar;
        private Label lblProgressText;
        private Button btnPush;
        private Button btnPull;

        private const string GEMINI_KEY_PREF = "GitTool_GeminiAPIKey";
        private const string DEFAULT_GEMINI_KEY = "[GCP_API_KEY]";

        private TextField txtGeminiKey;
        private Button btnAiCommit;
        private Label lblAiStatus;

        [MenuItem("Tools/Git Control Panel")]
        public static void ShowWindow()
        {
            GetWindow<GitManagerWindow>("Git Control");
        }

        private void CreateGUI()
        {
            // Load UXML & USS
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Modules/GitTool/Editor/GitManagerWindow.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Modules/GitTool/Editor/GitManagerWindow.uss");
            
            if (visualTree == null)
            {
                Debug.LogError("Không tìm thấy file GitManagerWindow.uxml tại Assets/Modules/GitTool/Editor/");
                return;
            }

            visualTree.CloneTree(rootVisualElement);
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);

            BindUIElements();
            SetupEventHandlers();
            
            // Set font for Git Tree based on platform
            string fontName = Application.platform == RuntimePlatform.OSXEditor ? "Courier" : "Consolas";
            lblGitTree.style.unityFont = Font.CreateDynamicFontFromOSFont(fontName, 12);

            RefreshBranchData();
            RefreshStatusData();
        }

        private void BindUIElements()
        {
            lblCurrentBranch = rootVisualElement.Q<Label>("lbl-current-branch");
            lblGitTree = rootVisualElement.Q<Label>("lbl-git-tree");
            lblStatus = rootVisualElement.Q<Label>("lbl-status");
            
            dropdownBranch = rootVisualElement.Q<DropdownField>("dropdown-branch");
            txtRenameBranch = rootVisualElement.Q<TextField>("txt-rename-branch");
            txtNewBranch = rootVisualElement.Q<TextField>("txt-new-branch");
            txtCommitMsg = rootVisualElement.Q<TextField>("txt-commit-msg");

            var lblPlaceholder = rootVisualElement.Q<Label>("lbl-placeholder");
            txtCommitMsg.RegisterValueChangedCallback(evt => {
                if (lblPlaceholder != null)
                {
                    lblPlaceholder.style.visibility = string.IsNullOrEmpty(evt.newValue) ? Visibility.Visible : Visibility.Hidden;
                }
            });
            
            conflictArea = rootVisualElement.Q<VisualElement>("conflict-area");
            scrollConflict = rootVisualElement.Q<ScrollView>("scroll-conflict");
            
            progressArea = rootVisualElement.Q<VisualElement>("progress-area");
            progressBar = rootVisualElement.Q<ProgressBar>("git-progress-bar");
            lblProgressText = rootVisualElement.Q<Label>("lbl-progress-text");
            btnPush = rootVisualElement.Q<Button>("btn-push");
            btnPull = rootVisualElement.Q<Button>("btn-pull");

            txtGeminiKey = rootVisualElement.Q<TextField>("txt-gemini-key");
            btnAiCommit = rootVisualElement.Q<Button>("btn-ai-commit");
            lblAiStatus = rootVisualElement.Q<Label>("lbl-ai-status");

            if (txtGeminiKey != null)
            {
                txtGeminiKey.isPasswordField = true;
                txtGeminiKey.maskChar = '*';
                string savedKey = EditorPrefs.GetString(GEMINI_KEY_PREF, DEFAULT_GEMINI_KEY);
                txtGeminiKey.value = savedKey;
                txtGeminiKey.RegisterValueChangedCallback(evt =>
                {
                    EditorPrefs.SetString(GEMINI_KEY_PREF, evt.newValue.Trim());
                });
            }
        }

        private void SetupEventHandlers()
        {
            rootVisualElement.Q<Button>("btn-smart-merge").clicked += () => GitRunner.SetupUnitySmartMerge();
            rootVisualElement.Q<Button>("btn-refresh-tree").clicked += RefreshBranchData;

            if (btnAiCommit != null)
            {
                btnAiCommit.clicked += () => GenerateCommitWithGemini();
            }
            
            // Branch buttons
            rootVisualElement.Q<Button>("btn-checkout").clicked += () => 
            {
                if (dropdownBranch.index >= 0)
                {
                    GitRunner.CheckoutBranch(dropdownBranch.value);
                    RefreshAfterAction();
                }
            };
            
            rootVisualElement.Q<Button>("btn-merge").clicked += () => 
            {
                if (dropdownBranch.index >= 0)
                {
                    string res = GitRunner.MergeBranch(dropdownBranch.value);
                    Debug.Log($"[Git Merge]: {res}");
                    RefreshAfterAction();
                }
            };

            rootVisualElement.Q<Button>("btn-rename-branch").clicked += () =>
            {
                if (!string.IsNullOrWhiteSpace(txtRenameBranch.value) && dropdownBranch.index >= 0)
                {
                    GitRunner.RenameBranch(dropdownBranch.value, txtRenameBranch.value.Trim().Replace(" ", "-"));
                    txtRenameBranch.value = "";
                    RefreshAfterAction();
                }
            };

            rootVisualElement.Q<Button>("btn-create-branch").clicked += () =>
            {
                if (!string.IsNullOrWhiteSpace(txtNewBranch.value))
                {
                    GitRunner.CreateAndCheckoutBranch(txtNewBranch.value.Trim().Replace(" ", "-"));
                    txtNewBranch.value = "";
                    RefreshAfterAction();
                }
            };

            rootVisualElement.Q<Button>("btn-fetch").clicked += () =>
            {
                GitRunner.Fetch();
                RefreshBranchData();
            };

            // Status
            rootVisualElement.Q<Button>("btn-status").clicked += RefreshStatusData;

            // Commit
            rootVisualElement.Q<Button>("btn-stage-all").clicked += () =>
            {
                GitRunner.RunCommand("add .");
                Debug.Log("[Git Add All]: Done!");
                RefreshStatusData();
            };

            rootVisualElement.Q<Button>("btn-commit").clicked += () =>
            {
                if (string.IsNullOrEmpty(txtCommitMsg.value))
                {
                    EditorUtility.DisplayDialog("Cảnh báo", "Vui lòng nhập Commit Message!", "OK");
                    return;
                }
                string res = GitRunner.RunCommand($"commit -m \"{txtCommitMsg.value}\"");
                Debug.Log($"[Git Commit]: {res}");
                txtCommitMsg.value = "";
                RefreshStatusData();
            };

            // Push/Pull
            btnPush.clicked += async () =>
            {
                SetProgressState(true, "Đang Push lên Remote...");
                string res = await GitRunner.RunCommandAsync("push --progress", UpdateProgressText);
                SetProgressState(false);
                Debug.Log($"[Git Push]: {res}");
                EditorUtility.DisplayDialog("Git Push", "Đã thực hiện xong lệnh Push!", "OK");
                RefreshAfterAction();
            };

            btnPull.clicked += async () =>
            {
                SetProgressState(true, "Đang Pull từ Remote...");
                string res = await GitRunner.RunCommandAsync("pull --progress", UpdateProgressText);
                SetProgressState(false);
                Debug.Log($"[Git Pull]: {res}");
                RefreshAfterAction();
            };
        }

        private void SetProgressState(bool isRunning, string initialText = "")
        {
            btnPush.SetEnabled(!isRunning);
            btnPull.SetEnabled(!isRunning);
            progressArea.style.display = isRunning ? DisplayStyle.Flex : DisplayStyle.None;
            if (isRunning)
            {
                progressBar.value = 0;
                progressBar.title = "0%";
                lblProgressText.text = initialText;
            }
        }

        private void UpdateProgressText(string msg)
        {
            lblProgressText.text = msg;
            // Parse percentage from msg (e.g. "Receiving objects: 15% (3/20)")
            var match = System.Text.RegularExpressions.Regex.Match(msg, @"(\d+)%");
            if (match.Success && float.TryParse(match.Groups[1].Value, out float percent))
            {
                progressBar.value = percent;
                progressBar.title = $"{percent}%";
            }
        }

        private void RefreshAfterAction()
        {
            AssetDatabase.Refresh();
            RefreshBranchData();
            RefreshStatusData();
        }

        private void RefreshBranchData()
        {
            currentBranch = GitRunner.GetCurrentBranch();
            lblCurrentBranch.text = string.IsNullOrEmpty(currentBranch) ? "Đang tải..." : currentBranch;

            branchList = GitRunner.GetBranches();
            dropdownBranch.choices = branchList.ToList();
            if (branchList.Length > 0)
            {
                int idx = branchList.ToList().IndexOf(currentBranch);
                dropdownBranch.index = idx >= 0 ? idx : 0;
            }

            gitTreeData = GitRunner.GetGitTreeLog(30);
            lblGitTree.text = ColorizeGitTree(gitTreeData);
        }

        private void RefreshStatusData()
        {
            gitStatusResult = GitRunner.RunCommand("status -s");
            if (string.IsNullOrWhiteSpace(gitStatusResult))
            {
                gitStatusResult = "Mọi thứ đã được lưu (Working tree clean).";
            }
            lblStatus.text = gitStatusResult;

            unmergedFiles = GitRunner.RunCommand("diff --name-only --diff-filter=U");
            UpdateConflictUI();
        }

        private void UpdateConflictUI()
        {
            if (string.IsNullOrWhiteSpace(unmergedFiles) || unmergedFiles.StartsWith("Error"))
            {
                conflictArea.style.display = DisplayStyle.None;
                return;
            }

            conflictArea.style.display = DisplayStyle.Flex;
            scrollConflict.Clear();

            string[] conflictList = unmergedFiles.Split('\n');
            foreach (string file in conflictList)
            {
                if (string.IsNullOrWhiteSpace(file)) continue;

                var row = new VisualElement() { style = { flexDirection = FlexDirection.Row, marginTop = 2, marginBottom = 2, alignItems = Align.Center } };
                
                var lblFile = new Label(file) { style = { unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, flexGrow = 1 } };
                
                var btnOurs = new Button(() => ResolveConflict(file, true)) { text = "Giữ bản của Tôi (Ours)", style = { width = 140 } };
                var btnTheirs = new Button(() => ResolveConflict(file, false)) { text = "Lấy bản Remote (Theirs)", style = { width = 150 } };

                row.Add(lblFile);
                row.Add(btnOurs);
                row.Add(btnTheirs);

                scrollConflict.Add(row);
            }
        }

        private void ResolveConflict(string file, bool keepOurs)
        {
            GitRunner.ResolveConflict(file.Trim(), keepOurs);
            RefreshAfterAction();
        }

        private string ColorizeGitTree(string rawTree)
        {
            if (string.IsNullOrEmpty(rawTree)) return "Chưa có dữ liệu lịch sử Git.";
            string colored = rawTree.Replace("HEAD ->", "<color=#ff4444><b>HEAD -></b></color>");
            colored = colored.Replace("origin/", "<color=#55aaff>origin/</color>");
            return colored;
        }

        private async void GenerateCommitWithGemini()
        {
            string apiKey = txtGeminiKey != null && !string.IsNullOrWhiteSpace(txtGeminiKey.value) 
                ? txtGeminiKey.value.Trim() 
                : EditorPrefs.GetString(GEMINI_KEY_PREF, DEFAULT_GEMINI_KEY);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                EditorUtility.DisplayDialog("Thiếu Gemini API Key", "Vui lòng nhập Gemini API Key trong phần '🤖 Cấu hình Gemini AI'!", "OK");
                return;
            }

            string diff = GitRunner.GetGitDiff(3500);
            if (string.IsNullOrWhiteSpace(diff))
            {
                EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy thay đổi nào trong dự án (Working tree clean).", "OK");
                return;
            }

            SetAiStatus(true, "⏳ Gemini AI đang phân tích thay đổi và tạo commit...");
            btnAiCommit.SetEnabled(false);

            try
            {
                string prompt = "Bạn là trợ lý Git. Hãy đọc nội dung git diff/status dưới đây và tạo 01 dòng commit message cực kỳ ngắn gọn, súc tích (tối đa 1 dòng, khoảng 10-15 từ) theo chuẩn Conventional Commits (ví dụ: feat: ..., fix: ..., refactor: ..., chore: ...). KHÔNG giải thích, KHÔNG thêm ký tự dư thừa nào ngoại trừ 1 dòng commit:\n\n" + diff;
                
                string commitMsg = await CallGeminiApiAsync(apiKey, prompt);

                if (!string.IsNullOrWhiteSpace(commitMsg))
                {
                    commitMsg = commitMsg.Replace("```", "").Replace("`", "").Trim();
                    var lines = commitMsg.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0) commitMsg = lines[0].Trim();

                    txtCommitMsg.value = commitMsg;
                    SetAiStatus(true, "✨ Đã tạo Commit Message thành công!");
                }
                else
                {
                    SetAiStatus(true, "⚠️ Không nhận được phản hồi từ Gemini.");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[Gemini Error]: {ex.Message}");
                EditorUtility.DisplayDialog("Lỗi Gemini API", $"Không thể tạo commit message: {ex.Message}", "OK");
                SetAiStatus(false);
            }
            finally
            {
                btnAiCommit.SetEnabled(true);
            }
        }

        private void SetAiStatus(bool visible, string message = "")
        {
            if (lblAiStatus != null)
            {
                lblAiStatus.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                lblAiStatus.text = message;
            }
        }

        private async System.Threading.Tasks.Task<string> CallGeminiApiAsync(string apiKey, string prompt)
        {
            // Thử danh sách các tên model tương thích theo thứ tự ưu tiên
            string[] modelNames = new string[] 
            { 
                "gemini-2.5-flash", 
                "gemini-2.0-flash", 
                "gemini-1.5-flash-latest", 
                "gemini-1.5-pro", 
                "gemini-pro" 
            };

            string lastError = "";

            foreach (var model in modelNames)
            {
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                var requestData = new GeminiRequest
                {
                    contents = new GeminiRequest.Content[]
                    {
                        new GeminiRequest.Content
                        {
                            parts = new GeminiRequest.Part[]
                            {
                                new GeminiRequest.Part { text = prompt }
                            }
                        }
                    }
                };

                string jsonBody = JsonUtility.ToJson(requestData);
                byte[] rawData = System.Text.Encoding.UTF8.GetBytes(jsonBody);

                using (var request = new UnityEngine.Networking.UnityWebRequest(url, "POST"))
                {
                    request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(rawData);
                    request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");

                    var asyncOp = request.SendWebRequest();
                    while (!asyncOp.isDone)
                    {
                        await System.Threading.Tasks.Task.Delay(50);
                    }

                    if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        string responseJson = request.downloadHandler.text;
                        var resObj = JsonUtility.FromJson<GeminiResponse>(responseJson);

                        if (resObj != null && resObj.candidates != null && resObj.candidates.Length > 0)
                        {
                            var cand = resObj.candidates[0];
                            if (cand.content != null && cand.content.parts != null && cand.content.parts.Length > 0)
                            {
                                return cand.content.parts[0].text;
                            }
                        }
                    }
                    else
                    {
                        lastError = $"[Model {model}] HTTP {request.responseCode}: {request.error}\n{request.downloadHandler.text}";
                    }
                }
            }

            throw new System.Exception(lastError);
        }
    }

    [System.Serializable]
    public class GeminiRequest
    {
        public Content[] contents;

        [System.Serializable]
        public class Content
        {
            public Part[] parts;
        }

        [System.Serializable]
        public class Part
        {
            public string text;
        }
    }

    [System.Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;

        [System.Serializable]
        public class Candidate
        {
            public Content content;
        }

        [System.Serializable]
        public class Content
        {
            public Part[] parts;
        }

        [System.Serializable]
        public class Part
        {
            public string text;
        }
    }
}
#endif
