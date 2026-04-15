# 物品名称显示完整指南 ✅

## 代码升级完成

**InventorySlotUI.cs** 已升级，现在支持：
- ✅ TextMeshPro (TMP_Text) - 现代 Unity 默认文本组件
- ✅ 旧版 UI Text 组件 - 向后兼容
- ✅ 详细的调试日志

---

## 物品名称显示的完整流程

```
1. Item 资源创建
   └─ 设置 ItemName 属性
   
2. InventoryManager
   └─ 收集物品到背包
   
3. InventoryUIManager.RefreshUI()
   └─ 为每个物品创建槽位
   
4. InventorySlotUI.Initialize()
   ├─ 获取子元素中的文本组件
   ├─ 调用 SetItemName()
   └─ 显示物品名称
   
5. 用户看到背包中的物品名称 ✨
```

---

## 验证清单 ✓

### 1. Item 资源配置
- [ ] 在 Assets 中创建 Item (右键 → Create → Inventory → Item)
- [ ] **ItemName 字段有值** ⭐ 重要
- [ ] 可选：设置 Icon (图标)

示例:
```
ItemName: "齿轮"
Icon: (可选)
Description: (可选)
```

### 2. 场景配置验证
- [ ] InventoryManager 存在
- [ ] InventoryUIManager 已配置
- [ ] Item Slot Prefab 已分配为 **ItemSlotTemplate.prefab**
- [ ] Item Container 已分配

### 3. 预制体检查
ItemSlotTemplate.prefab 应该包含：
- [ ] Image (UI Image) - 物品图标
- [ ] Button (UI Button) - 点击事件
- [ ] InventorySlotUI (脚本)
- [ ] ItemName (TextMeshPro 或 Text 子元素)

---

## 调试技巧

### 如果物品名称不显示

**第1步：检查 Console 日志**

运行游戏并打开背包，查看 Console，应该看到：
```
[InventorySlotUI] 初始化物品槽: 齿轮
[InventorySlotUI] 设置物品名称（TMP）: 齿轮
```

如果有错误提示，按照错误信息修复：
- "找不到 Text 组件" → 预制体中的子物体名称不对
- "UIManager 未初始化" → InventoryUIManager 配置有误
- "Text 组件为空" → 预制体缺少文本元素

**第2步：检查 ItemName 属性**

在 Inspector 中选择 Item 资源，确保：
- **ItemName** 字段不为空
- **ItemId** 字段自动生成

**第3步：验证预制体结构**

在 Project 中双击 **ItemSlotTemplate.prefab** 打开，检查：
- 根物体有 InventorySlotUI 脚本
- 子物体 "ItemName" 存在
- 子物体有 TextMeshProUGUI 或 Text 组件

---

## 如何创建测试用物品

1. 右键 Assets → **Create** → **Inventory** → **Item**
2. 命名为 "TestItem"  
3. 在 Inspector 中：
   - **ItemName**: "测试物品"
   - **Icon**: (可选，拖拽一个图片)
4. **保存** (Ctrl+S)

然后在 InventoryManager 中：
- 找到 **InventoryUIManager** 物体
- 在 Inspector 中找到 **InventoryManager** 脚本
- 修改 **Starting Items** 列表大小为 1
- 把 "TestItem" 拖拽到第一个元素

---

## 完整示例

### Console 输出示例（正常）
```
[InventorySlotUI] 初始化物品槽: 齿轮
[InventorySlotUI] 设置物品名称（TMP）: 齿轮
[InventorySlotUI] 初始化物品槽: 时针
[InventorySlotUI] 设置物品名称（TMP）: 时针
```

### 预期效果
```
背包 UI:
┌─────────────────┐
│ ┌────┐ ┌────┐   │
│ │Icon│ │Icon│   │
│ └────┘ └────┘   │
│  齿轮   时针    │
└─────────────────┘
    ▲      ▲
  物品名称显示
```

---

## 快速修复

如果仍然有问题，按以下顺序检查：

1. ✅ Item 资源是否有 ItemName
2. ✅ 预制体是否分配正确
3. ✅ Console 中是否有错误日志（按照错误提示修复）
4. ✅ 预制体中是否有 TextMeshProUGUI 子元素

如果以上都检查过了还有问题，查看 Console 的具体错误信息。
