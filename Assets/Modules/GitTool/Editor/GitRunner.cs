#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace DXLab.GitTool.Editor
{
    public static class GitRunner
    {
        // Hàm thực thi lệnh Git và trả về kết quả dưới dạng string
        public static string RunCommand(string command)
        {
            // Lấy đường dẫn thư mục gốc của dự án Unity (nơi chứa file .git)
            string projectPath = Path.GetDirectoryName(Application.dataPath);

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = projectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error) && process.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"[Git Error]: {error}");
                    return $"Error: {error}";
                }

                return output;
            }
        }
        
        public static async System.Threading.Tasks.Task<string> RunCommandAsync(string command, System.Action<string> onProgress = null)
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                WorkingDirectory = projectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                
                System.Text.StringBuilder errorBuilder = new System.Text.StringBuilder();
                char[] buffer = new char[1024];
                int bytesRead;
                
                while ((bytesRead = await process.StandardError.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string chunk = new string(buffer, 0, bytesRead);
                    errorBuilder.Append(chunk);

                    if (onProgress != null)
                    {
                        string[] parts = chunk.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            string msg = parts[parts.Length - 1].Trim();
                            if (!string.IsNullOrEmpty(msg))
                            {
                                // Đảm bảo UI update chạy trên Main Thread của Unity
                                UnityEditor.EditorApplication.delayCall += () =>
                                {
                                    onProgress.Invoke(msg);
                                };
                            }
                        }
                    }
                }

                process.WaitForExit();
                string output = await outputTask;
                string error = errorBuilder.ToString();

                if (!string.IsNullOrEmpty(error) && process.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"[Git Error]: {error}");
                    return $"Error: {error}";
                }

                return output;
            }
        }


        public static void SetupUnitySmartMerge()
        {
            // Tìm đường dẫn file UnityYAMLMerge nằm trong thư mục cài đặt Unity Editor
            string editorPath = UnityEditor.EditorApplication.applicationContentsPath;
            
            // Xử lý cả Windows (.exe) và macOS/Linux (không có .exe) và các thay đổi cấu trúc của Unity 6+
            string[] potentialPaths = new string[]
            {
                System.IO.Path.Combine(editorPath, "Tools", "UnityYAMLMerge.exe"),
                System.IO.Path.Combine(editorPath, "Tools", "UnityYAMLMerge"),
                System.IO.Path.Combine(editorPath, "Resources", "UnityYAMLMerge.exe"),
                System.IO.Path.Combine(editorPath, "Resources", "UnityYAMLMerge"),
                System.IO.Path.Combine(editorPath, "Helpers", "UnityYAMLMerge.exe"),
                System.IO.Path.Combine(editorPath, "Helpers", "UnityYAMLMerge")
            };

            string yamlMergePath = null;
            foreach (var path in potentialPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    yamlMergePath = path;
                    break;
                }
            }

            if (yamlMergePath == null)
            {
                UnityEngine.Debug.LogError($"Không tìm thấy công cụ UnityYAMLMerge trong thư mục {editorPath}");
                return;
            }

            // Cấu hình git config để sử dụng UnityYAMLMerge làm tool merge mặc định cho file YAML/Scene
            RunCommand($"config merge.unityyaml.name \"Unity SmartMerge\"");
            RunCommand($"config merge.unityyaml.driver \"\\\"{yamlMergePath}\\\" merge -h -p \\\"%O\\\" \\\"%B\\\" \\\"%A\\\" \\\"%A\\\"\"");
            
            UnityEngine.Debug.Log(" Đã cấu hình thành công Unity Smart Merge cho Git!");
        }

        // Gỡ conflict bằng cách chọn bản của Mình (Ours) hoặc bản từ Remote (Theirs)
        public static void ResolveConflict(string filePath, bool keepOurs)
        {
            string option = keepOurs ? "--ours" : "--theirs";
            
            // 1. Checkout lấy đúng phiên bản file được chọn
            RunCommand($"checkout {option} -- \"{filePath}\"");
            
            // 2. Mark file đó là đã gỡ conflict (git add)
            RunCommand($"add \"{filePath}\"");
            
            UnityEngine.Debug.Log($"[Git Resolve]: Đã gỡ conflict file {filePath} bằng bản {(keepOurs ? "CỦA TÔI" : "TỪ REMOTE")}");
        }

        public static string[] GetBranches()
        {
            string output = RunCommand("branch -a");
            if (output.StartsWith("Error")) return new string[0];
            
            var lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var branches = new System.Collections.Generic.List<string>();
            foreach (var line in lines)
            {
                string cleanLine = line.Replace("*", "").Trim();
                if (!cleanLine.Contains("->")) // Bỏ qua con trỏ HEAD
                {
                    branches.Add(cleanLine);
                }
            }
            return branches.ToArray();
        }

        public static string GetCurrentBranch()
        {
            return RunCommand("branch --show-current").Trim();
        }

        public static void CreateAndCheckoutBranch(string branchName)
        {
            RunCommand($"checkout -b \"{branchName}\"");
        }

        public static void CheckoutBranch(string branchName)
        {
            if (branchName.StartsWith("remotes/origin/"))
            {
                string localName = branchName.Replace("remotes/origin/", "");
                RunCommand($"checkout -b \"{localName}\" \"{branchName}\"");
            }
            else
            {
                RunCommand($"checkout \"{branchName}\"");
            }
        }

        public static string MergeBranch(string branchName)
        {
            return RunCommand($"merge \"{branchName}\"");
        }

        public static void RenameBranch(string oldName, string newName)
        {
            RunCommand($"branch -m \"{oldName}\" \"{newName}\"");
        }

        public static string GetGitTreeLog(int maxCommits = 20)
        {
            return RunCommand($"log --graph --abbrev-commit --decorate --date=relative --format=format:\"%h - (%ar) %s %d\" -n {maxCommits} --all");
        }

        public static void Fetch()
        {
            RunCommand("fetch");
        }

        public static string GetGitDiff(int maxChars = 4000)
        {
            // Ưu tiên lấy file đã Staged (git diff --staged)
            string diff = RunCommand("diff --staged");
            if (string.IsNullOrWhiteSpace(diff) || diff.StartsWith("Error"))
            {
                // Nếu chưa stage, lấy diff toàn bộ thay đổi chưa stage
                diff = RunCommand("diff");
            }
            if (string.IsNullOrWhiteSpace(diff) || diff.StartsWith("Error"))
            {
                // Nếu vẫn rỗng (ví dụ file mới chưa được track), lấy git status
                diff = RunCommand("status -s");
            }

            if (string.IsNullOrWhiteSpace(diff) || diff.StartsWith("Error"))
            {
                return "";
            }

            if (diff.Length > maxChars)
            {
                diff = diff.Substring(0, maxChars) + "\n...[Nội dung diff đã được cắt bớt do quá dài]";
            }

            return diff;
        }
    }
}
#endif
