# Unity Tookits

## 1. 介绍
提供即开即用的游戏开发工具，工具间无依赖关系，需要哪个就使用哪个。

| 工具                  | 介绍                                                         |
| --------------------- | ------------------------------------------------------------ |
| Singleton            | 为C#和MonoBehavior设计的单例模式，支持全局/跨场景单例，单例加锁 |
| Excel Importer        | Excel导表工具，支持自动生成代码和自动生成ScriptableObject    |
| TableDataManager      | Excel生成数据管理                                            |
| Addressables Importer | 对Unity Addressables进行的编辑器拓展，自动刷新资源到Group    |
| HybridCLR Extentions  | 对HybridCLR的编辑器拓展，一键更新热更dll                     |
| Event Manger          | 事件管理器，基于泛型和接口实现的事件管理器，每个事件即是一个struct |
| PriorityQueue         | C#9没有内置优先级队列，这是一个基于堆的高效实现              |
| ObjectPool            | 通用对象池，支持GameObject和C#对象                           |
| Model Manager         | 参考MVC中的M实现的数据管理器                                 |
| Dialog System         | 通用的对话系统节点编辑器                                     |
| Utils                 | 一些通用的工具类，包括文件，反射等                           |

## 2. 安装

1. 通过release的*.unitypackage安装，下载之后拖到Unity的Project文件夹即可



## 3.使用

### 3.1 Singleton
