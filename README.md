# Unity Toolkits :sob:

`Unity's development tools` 开发中积累的Unity开发工具包，提供一些游戏开发中经常会用到的内容，避免重复造轮子，为了隔离，功能间使用程序集定义进行了划分。目前文档还没有很完善，持续补全中:briefcase:

Unity版本>=2021.3.15

## 目录

[toc]

## C# Tool :cry:

| 名称              | 内容                                                         |
| ----------------- | ------------------------------------------------------------ |
| MonoBehavior单例  | 基于泛型的MonoBehavior单例                                   |
| 事件系统          | 基于接口的强约束事件系统                                     |
| 定时器            | Update生命周期驱动的定时器                                   |
| 主线程调度器      | 用于在子线程中执行Unity API                                  |
| GameObject 对象池 | 使用接口约束的游戏物体对象池                                 |
| 状态机            | 泛型状态机                                                   |
| UIManager         | 基于UGUI和优先级队列实现的简易UI框架，支持自动代码生成和绑定 |
|                   |                                                              |
|                   |                                                              |

## Editor Tool :laughing:

| 名称           | 内容                                                         | 使用方法                                  |
| -------------- | ------------------------------------------------------------ | ----------------------------------------- |
| 对话节点编辑器 | 参考ShaderGraph制作的对话节点编辑器，数据保存成So方便运行时加载 | 菜单栏Nico/Tools/DialogGraph              |
| 热更工具       | 基于Hybrid CLR和addressables制作的热更工具，点个按钮就能一键更新 | 菜单栏Nico/Tools/AddressablesEditorWindow |
| Excel导表工具  | 用Excel表格进行代码生成和转换SO的工具                        | 菜单栏Nico/Tools/DataTable                |

## 热更工具

![addressables-editor-window](./images/addressables-editor-window.png)

## 导表工具

![datatable](./images/datatable.png)

## 对话编辑器

![dialog-graph](./images/dialog-graph.png)
