# ⚠️ 快速修复指南 - 常见错误

## 错误 1: "物品槽预制体缺少 InventorySlotUI"

**原因**: ItemSlotTemplate.prefab 预制体上没有 InventorySlotUI 脚本

**修复步骤**:
1. 在 Project 面板中找到 **Assets/ItemSlotTemplate.prefab**
2. **双击打开**预制体编辑模式
3. 在 Hierarchy 中选择根物体 **ItemSlotTemplate**
4. 在 Inspector 右侧点击 **Add Component**
5. 搜索 **InventorySlotUI** 并添加
6. **Ctrl+S** 保存预制体
7. 关闭预制体编辑

---

## 错误 2: "无法获取玩家变换，主摄像机未初始化"

**原因**: 主摄像机没有被正确标记

**修复步骤**:
1. 在 Hierarchy 中找到 **Main Camera** 物体
2. 点击它选中
3. 在 Inspector 右上角找到 **Tag** 下拉菜单
4. 选择 **"MainCamera"** (如果没有，选择 Add Tag 添加)
5. 应该显示 `√ MainCamera`

---

## 错误 3: 物品不显示在背包中

**检查清单**:
1. ✅ 确保 InventoryManager 在场景中存在
2. ✅ 确保有物品资源被创建 (Assets → Create → Inventory → Item)
3. ✅ 确保物品被添加到 InventoryManager 的 **Starting Items**
4. ✅ 按 Menu 键查看背包是否打开
5. ✅ 检查 Console 中的错误日志

---

## 错误 4: 点击物品槽没有反应

**检查清单**:
1. ✅ itemSlotPrefab 是否有 **InventorySlotUI** 组件 (见错误1的修复)
2. ✅ itemSlotPrefab 是否有 **Button** 组件
3. ✅ InventoryUIManager 是否被分配 **Item Container** 和 **Item Slot Prefab**
4. ✅ 检查 Console 中的警告

---

## 最重要的三件事 🎯

1. **ItemSlotTemplate.prefab 必须有 InventorySlotUI 脚本**
2. **主摄像机必须标记为 "MainCamera"**
3. **InventoryManager 和 InventoryUIManager 必须存在于场景中**

---

## 如何验证配置正确

运行游戏，查看 Console 输出，应该看到类似的日志：

```
[InventoryItemEntityManager] 自动创建管理器实例
[InventorySlotUI] 初始化物品槽: GearItem
[InventorySlotUI] 初始化物品槽: HourHandItem
物品已收集: GearItem
```

如果没有这些输出，检查上述步骤。

---

## 更多帮助

完整的配置指南见: **Assets/SETUP_GUIDE.md**
