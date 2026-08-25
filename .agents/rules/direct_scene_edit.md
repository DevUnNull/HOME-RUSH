---
description: Hướng dẫn chi tiết cách Agent chỉnh sửa trực tiếp Scene, UI và GameObject trong Unity bằng MCP.
---

# Direct Unity Scene Editing Rule

Khi có yêu cầu chỉnh sửa trực tiếp trên Unity Scene, GameObject, UI, hoặc cấu hình Editor, Agent phải tuân thủ nghiêm ngặt các bước sau đây để đảm bảo thay đổi được áp dụng chính xác thông qua công cụ MCP `unity-synaptic`.

## 1. Ưu tiên Tự Động Hóa (Không Hướng Dẫn Thủ Công)
Tuyệt đối **KHÔNG** chỉ đưa ra hướng dẫn để người dùng tự thao tác bằng tay trên Unity Editor (Ví dụ: "Bạn hãy mở cửa sổ Hierarchy, tạo GameObject mới và kéo thả Script...").
Thay vào đó, **PHẢI** tự động hóa toàn bộ công việc đó bằng cách gọi tool MCP để tác động trực tiếp vào Editor. Người dùng chỉ đóng vai trò yêu cầu, mọi thao tác còn lại AI sẽ làm thay.

## 2. Sử dụng MCP `unity-synaptic`
Tùy thuộc vào yêu cầu, sử dụng các công cụ được cung cấp bởi `unity-synaptic`:
- **`create` / `modify` / `inspect`**: Dùng để xem xét cấu trúc Scene, tạo mới hoặc chỉnh sửa trực tiếp thuộc tính của các GameObject / Component.
- **`run_csharp` / `execute`**: Thực thi mã C# (Editor Script) khi cần thực hiện các thao tác phức tạp như khởi tạo Prefab, chỉnh sửa Material, cấu hình UI phức tạp, gán Script tự động, thiết lập Component liên kết, hay thay đổi Layer/Tag.

## 3. Lưu ý khi dùng `run_csharp` thao tác với Scene
Khi cần chạy script C# để tự động sửa Scene, hãy luôn nhớ:
- Import đầy đủ các namespace liên quan tới Editor: 
  ```csharp
  using UnityEngine;
  using UnityEditor;
  using UnityEditor.SceneManagement;
  ```
- **LƯU CẢNH BÁO QUAN TRỌNG:** Luôn lưu lại Scene hiện tại sau khi có bất kỳ thay đổi nào (thêm/xóa/sửa GameObject, Component) để tránh mất dữ liệu. Hãy sử dụng:
  ```csharp
  // Đánh dấu Scene đã bị thay đổi (Dirty) để Unity nhận diện
  EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
  
  // Lưu trực tiếp các Scene đang mở
  EditorSceneManager.SaveOpenScenes();
  ```

### Ví dụ về đoạn mã `run_csharp` chuẩn mực:
```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneModifier 
{
    public static void Execute() 
    {
        // 1. Tìm hoặc tạo GameObject
        GameObject go = GameObject.Find("MyTargetObject");
        if (go == null) 
        {
            go = new GameObject("MyTargetObject");
        }
        
        // 2. Thêm component và cấu hình
        if (go.GetComponent<BoxCollider>() == null) 
        {
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(2, 2, 2);
        }
        
        // 3. Đánh dấu dirty và lưu lại Scene
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        
        Debug.Log("✅ Đã chỉnh sửa GameObject trên Scene và lưu thành công!");
    }
}
```

## 4. Xác nhận tiến trình và xử lý lỗi
- Sau khi chạy lệnh MCP để thay đổi Scene, hãy thông báo ngắn gọn cho người dùng những gì đã được tự động áp dụng.
- Nếu tool MCP trả về thông báo lỗi, hãy tự động đọc log lỗi, điều chỉnh lại Script C# của bạn và chạy lại công cụ MCP thay vì ngay lập tức dừng lại và hỏi người dùng.
