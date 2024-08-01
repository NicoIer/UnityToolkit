# Unity Toolkits :sob:

`Unity's development tools`
开发中积累的Unity开发工具包，提供一些游戏开发中经常会用到的内容，避免重复造轮子，为了隔离，功能间使用程序集定义进行了划分。目前文档还没有很完善，持续补全中:
briefcase:

Unity版本>=2021.3.15

[//]: # (# 推荐的框架结构)

## UIFramework

- 首先在场景中创建一个UIRoot对象，用于管理UI面板
- 创建后会自动生成一个UIDatabase对象，存放在Assets目录下，可以根据自己的需要进行移动
- 可以尝试双击打开UIDatabase(未实现的功能)
- 首先实现一个自定的UI资源加载器(下面分别时基于Addressabels和Resouces的两种IUILoader实现)
    - 三种加载方式分别对应同步加载 ，回调加载 ，Task加载
    - 本质上是加载一个UI面板的Prefab

```csharp
    internal struct AddressablesUILoader : IUILoader
    {
        public GameObject Load<T>() where T : IUIPanel
        {
            string path = $"UI/{typeof(T).Name}/{typeof(T).Name}.prefab";
            return Addressables.LoadAssetAsync<GameObject>(path).WaitForCompletion();
        }

        public void LoadAsync<T>(Action<GameObject> callback) where T : IUIPanel
        {
            string path = $"UI/{typeof(T).Name}/{typeof(T).Name}.prefab";
            var handle = Addressables.LoadAssetAsync<GameObject>(path);
            handle.Completed += operation => { callback(handle.Result); };
        }

        public async Task<GameObject> LoadAsync<T>() where T : IUIPanel
        {
            string path = $"UI/{typeof(T).Name}/{typeof(T).Name}.prefab";
            return await Addressables.LoadAssetAsync<GameObject>(path);
        }

        public void Dispose(GameObject panel)
        {
            Addressables.Release(panel);
        }
    }
```

```csharp
        private struct DefaultLoader : IUILoader
        {
            public GameObject Load<T>() where T : IUIPanel
            {
                return Resources.Load<GameObject>(typeof(T).Name);
            }

            public void LoadAsync<T>(Action<GameObject> callback) where T : IUIPanel
            {
                var handle = Resources.LoadAsync<GameObject>(typeof(T).Name);
                handle.completed += operation => { callback(handle.asset as GameObject); };
            }

            public async Task<GameObject> LoadAsync<T>() where T : IUIPanel
            {
                TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
                var handle = Resources.LoadAsync<GameObject>(typeof(T).Name);
                tcs.SetResult(handle.asset as GameObject);
                await tcs.Task;
                return tcs.Task.Result;
            }

            public void Dispose(GameObject panel)
            {
                DestroyImmediate(panel);
            }
        }
```

- 然后在代码中初始化UI加载器

```csharp
 UIRoot.Singleton.UIDatabase.Loader = new AddressablesUILoader();
```

- 然后可以按照需要进行UI的加载和销毁

```csharp
UIRoot.Singleton.OpenPanel<TXXXPanel>(); // 打开一个面板
UIRoot.Singleton.ClosePanel<TXXXPanel>(); // 关闭一个面板
UIRoot.Singleton.CloseAllPanel(); // 关闭所有面板
UIRoot.Singleton.Dispose<TXXXPanel>(); // 销毁一个面板
```

- 如何制作一个UIPanel
    - 在UIRoot的Canvas下创建一个Canvas，修改名字，然后拖到Project下，生成一个Prefab
    - 创建一个脚本继承UIPanel，将UIPanel挂载到prefab上
    - 也可以实现IUIPanel接口，实现自己的UIPanel
    - 注意你的prefab路径，需要能够被你的IUILoader加载到

## Debugger

- 拷贝自GameFramework的调试器，在场景中新建一个空物体，挂上DebuggerManager脚本即可

## Other

- TypeEventSystem：事件系统，支持事件的注册和注销，事件的派发，事件的监听，事件的移除
- EasyGameObjectPool : 简单的对象池仅支持单一GameObject，支持对象的创建，回收，销毁，清空等操作
- GameObjectPoolManager: 管理多个EasyGameObjectPool，支持对象的创建，回收，销毁，清空等操作
- PlayerLoopHelper：源自Mirror的PlayerLoopHelper，用于自定义PlayerLoop
- CharacterController2D：2D角色控制器源自github.com/prime31/CharacterController2D
- ModelCenter:类似于MVC中的Model层，提供数据访问和数据管理的功能
- Timer: 计时器
- MonoSingleton
    - 单例模式的基类，继承MonoSingleton\<T>即可实现单例模式
    - 通过MonoSingleton\<T>.Singleton获取单例对象
    - 通过MonoSingleton\<T>.SingletonNullable==null判断是否存在单例对象
    - 重写DontDestroyOnLoad()方法可以实现场景切换不销毁单例对象
    - 实现IAutoCreateSingleton接口可以实现自动创建单例对象
    - 实现IOnlyPlayModeSingleton接口可以实现只在PlayMode下创建单例对象
- LoopScrollRect: 无限循环滚动列表 github.com/qiankanglai/LoopScrollRect

# Network

提供网络游戏开发工具包