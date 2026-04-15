# ItemSlotTemplate.prefab 重建完成 ✅

## 预制体结构

```
ItemSlotTemplate (根物体)
├── Image (UI Image) - 物品图标显示
├── Button (UI Button) - 点击交互
└── InventorySlotUI (脚本组件) ⭐ 关键组件
    ├── Text 子元素 (ItemName) - 物品名称显示
    └── TMP_Text 组件 - 文本显示
```

## 预制体包含的组件

1. **RectTransform** - UI 布局（大小 100x100）
2. **Image** - 显示物品图标（使用默认 UI 背景）
3. **Button** - 处理点击事件
4. **InventorySlotUI** ⭐ 
   - 脚本 GUID: `b041a3f9f5059234597a300d39d540b1`
   - 关键方法: `Initialize()`, `OnSlotClicked()`
5. **CanvasRenderer** - UI 渲染

## 子物体: ItemName
- **TMP_Text** - TextMesh Pro 文本显示
- 用于显示物品名称

---

## 场景配置检查清单

运行游戏前，确保以下配置已完成：

### ✅ 必须完成的配置

1. **主摄像机 Tag**
   - [ ] 在 Hierarchy 中找到 Main Camera
   - [ ] Inspector 右上角 Tag 设置为 **"MainCamera"**

2. **InventoryManager 配置**
   - [ ] 场景中存在带 InventoryManager 脚本的物体
   - [ ] 可选：设置 Starting Items（初始物品列表）

3. **InventoryUIManager 配置**
   - [ ] 场景中存在带 InventoryUIManager 脚本的物体
   - [ ] Inspector 配置：
     - **Inventory Panel**: 背包 UI 面板
     - **Item Container**: 物品槽的容器
     - **Item Slot Prefab**: `ItemSlotTemplate` （已重建）
     - **Close Button**: 关闭按钮（可选）

4. **创建测试用物品**
   ```
   右键 → Create → Inventory → Item
   设置 ItemName 为某个名称
   ```

5. **配置机关（可选）**
   - [ ] ItemConsumer 派生类配置
   - [ ] Mechanism ID 和 Required Item 设置

---

## 如何测试

1. 保存场景
2. 启动游戏
3. 查看 Console 输出
4. 按 Menu 键打开背包
5. 验证物品槽显示正确

---

## 常见问题排查

| 问题 | 解决方案 |
|------|--------|
| 物品槽预制体缺少InventorySlotUI | ✅ 已修复 - 新预制体包含此组件 |
| 无法获取玩家变换 | 检查主摄像机 Tag 是否为 "MainCamera" |
| 物品不显示 | 检查是否设置 Starting Items 或物品是否被收集 |
| 点击无反应 | 检查 InventoryUIManager 配置 |

