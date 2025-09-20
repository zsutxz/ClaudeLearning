# Coin Animation Test Scene Setup

## 使用测试场景的步骤

### 1. 打开场景
1. 在Unity编辑器中打开`CoinAnimationTest.unity`场景

### 2. 创建Coin预制体
1. 在Hierarchy中创建一个空的GameObject，命名为"Coin"
2. 给"Coin"对象添加以下组件：
   - Sprite Renderer组件
   - Coin脚本组件
3. 在Sprite Renderer组件中，将Sprite设置为`Assets/icon02.png`
4. 将"Coin"对象拖拽到`Assets/Prefabs`文件夹中创建预制体
5. 删除场景中的"Coin"对象实例

### 3. 配置场景对象
1. 选择场景中的"CoinAnimationSystem"对象：
   - 添加CoinAnimationSystem组件
   - 添加CoinAnimationDemo组件

2. 选择"PoolManager"对象：
   - 添加CoinPoolManager组件
   - 在Coin Prefab字段中分配刚刚创建的Coin预制体

3. 确保"CoinAnimationSystem"对象的组件引用正确设置：
   - CoinAnimationSystem组件的Pool Manager字段应指向"PoolManager"对象
   - CoinAnimationDemo组件的Coin Animation System字段应指向自身
   - CoinAnimationDemo组件的Collection Point字段应指向"CollectionPoint"对象

### 4. 运行测试
1. 点击Unity编辑器中的播放按钮
2. 按以下按键测试不同功能：
   - 按空格键(Space)：生成带有级联效果的金币
   - 按C键：生成金币爆发效果
   - 按R键：生成普通金币收集效果

## 注意事项

1. 确保已安装DOTween插件
2. 确保所有脚本编译没有错误
3. 确保预制体正确设置并分配给了PoolManager
4. 测试时注意观察FPS是否保持稳定