# Project Rules for ChoNo Unity Project

## Direct Unity Editor & Scene Manipulation
- Khi người dùng yêu cầu chỉnh sửa Scene, UI, GameObject hoặc cài đặt game, AI luôn sử dụng MCP tool `unity-synaptic` (`execute`, `run_csharp`) để tự động tạo, gắn component và lưu Scene (`EditorSceneManager.SaveOpenScenes()`) trực tiếp trong Unity Editor.
- Chi tiết xem tại quy tắc bổ sung: [.agents/rules/direct_scene_edit.md](.agents/rules/direct_scene_edit.md)
