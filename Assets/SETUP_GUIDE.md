# 背包系统配置诊断指南

## 错误排查清单

### 1. 主摄像机配置
- [ ] 场景中存在一个 Camera 组件
- [ ] 这个 Camera 的 **Tag** 设置为 **"MainCamera"**
- [ ] 验证方法：在 Hierarchy 中点击摄像机，在 Inspector 右上角看 Tag 下拉菜单

### 2. InventoryManager 配置
- [ ] 场景中存在 **InventoryManager** 物体
- [ ] 物体上挂载了 **InventoryManager** 脚本
- [ ] 可选：在 Inspector 中设置 Starting Items（初始物品）

### 3. InventoryUIManager 配置
- [ ] 场景中存在一个带 **InventoryUIManager** 脚本的物体
- [ ] 在 Inspector 中检查以下字段是否已分配：
  - **Inventory Panel**: 背包UI面板（Canvas 的子物体）
  - **Item Container**: 物品槽的父容器（Grid Layout、Vertical Layout 等）
  - **Item Slot Prefab**: 物品槽预制体（必须有 InventorySlotUI 组件）
  - **Close Button**: 关闭按钮（可选）

### 4. ItemSlotTemplate 预制体配置 ⚠️ 最常见的问题
**路径**: Assets/ItemSlotTemplate.prefab

此预制体结构应该是：
```
ItemSlotTemplate
├── Image (UI Image 组件) - 显示物品图标
├── Button (UI Button 组件) - 点击检测
└── ItemName (Text) - 显示物品名称
```

**必须执行的步骤：**
1. 打开 ItemSlotTemplate 预制体（双击或右键 Open）
2. 选择根物体 (ItemSlotTemplate)
3. 在 Inspector 中检查是否有 **InventorySlotUI** 脚本组件
4. 如果没有，点击 **Add Component** 搜索 **InventorySlotUI** 并添加
5. 保存预制体

### 5. Item 物品数据配置
- [ ] 在 Assets 中右键 → **Create** → **Inventory** → **Item**
- [ ] 创建至少一个 Item 资源文件
- [ ] 设置以下属性：
  - **Item Name**: 物品名称（必填）
  - **Icon**: 物品图标（可选，但建议设置）
  - **Description**: 物品描述（可选）
- [ ] 在 InventoryManager 的 Inspector 中将 Item 添加到 **Starting Items** 列表（用于测试）

### 6. ItemConsumer 配置（机关配置）
- [ ] 每个应该消费物品的机关都需要：
  - [ ] 挂载 **ItemConsumer** 派生类脚本（如 ClockPart）
  - [ ] 设置 **Mechanism ID**: 机制 ID（如 "clock_puzzle")
  - [ ] 设置 **Required Item**: 所需物品
  - [ ] 添加 **Collider** 组件并勾选 **Is Trigger**
  - [ ] 添加 **Tag** 为 "Player"（或自定义，脚本中取决于 CompareTag 检查）

### 7. CollectibleItem 配置（可拾取物品）
- [ ] 场景中的可拾取物品需要：
  - [ ] 挂载 **CollectibleItem** 脚本
  - [ ] 设置 **Item Data**: 关联的 Item 资源
  - [ ] 添加 **Collider** 组件
  - [ ] 添加 **Rigidbody** 组件
  - [ ] 可选：添加 **XRGrabInteractable** 用于 VR 交互

### 8. InventoryItemEntityManager 配置
- [ ] 此管理器自动在场景中创建
- [ ] 或者手动创建一个物体并挂载此脚本
- [ ] 在 Inspector 中设置：
  - **Spawn Distance**: 在玩家前方生成物品的距离（默认 1.5）
  - **Spawn Height Offset**: 高度偏移（默认 0）

## 调试步骤

### 如果看到 "物品槽预制体缺少 InventorySlotUI" 错误：
1. 检查 ItemSlotTemplate 预制体（见上面第4项）
2. 确保预制体被正确分配到 InventoryUIManager 的 **Item Slot Prefab** 字段

### 如果看到 "UIManager 为空" 错误：
1. 检查 InventorySlotUI 的 Initialize 方法是否被正确调用
2. 确保 InventoryUIManager 的 RefreshUI 方法被正确调用

### 如果物品不显示：
1. 检查 InventoryManager 是否有 Starting Items 或是否有物品被正确添加
2. 检查 InventoryUIManager 是否被激活
3. 查看 Console 中的日志输出

## 推荐的测试流程

1. **启动游戏** → 查看 Console 输出的初始化日志
2. **查找是否有任何 error** → 按照错误信息诊断问题
3. **按 Menu 键**（或配置的打开背包快捷键）→ 应该看到背包 UI
4. **观察物品槽** → 应该显示 Starting Items 中的物品

## 完整的虚拟结构示例

```
Scene
├── Main Camera (Tag: "MainCamera")
├── GameManager (带 InventoryManager 脚本)
├── Canvas (带 InventoryUIManager 脚本)
│   └── InventoryPanel
│       └── ItemGridContainer
│           ├── ItemSlotTemplate (预制体实例 1)
│           ├── ItemSlotTemplate (预制体实例 2)
│           └── ...
├── ItemsInWorld (收集物品组)
│   ├── GearItem (带 CollectibleItem 脚本)
│   ├── HourHandItem (带 CollectibleItem 脚本)
│   └── MinuteHandItem (带 CollectibleItem 脚本)
└── ClockPuzzle (带 ClockPuzzleManager 脚本)
    ├── GearSlot (带 ClockSlot 脚本)
    ├── HourHandSlot (带 ClockSlot 脚本)
    └── MinuteHandSlot (带 ClockSlot 脚本)
```
