using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using HybridCLR;

namespace HotFix
{
    struct MyValue
    {
        public int x;
        public float y;
        public string s;
    }
    public class App
    {
        public static int Main()
        {
#if !UNITY_EDITOR
            LoadMetadataForAOTAssemblies();
            GLog.Log("AOT程序集加载完毕!");
#endif
            TestAOTGeneric();
            LoadScene();
            return 0;
        }
        /// <summary>
        /// 测试 aot泛型
        /// </summary>
        public static void TestAOTGeneric()
        {
            var arr = new List<MyValue>();
            arr.Add(new MyValue() { x = 1, y = 10, s = "abc" });
            GLog.Log("AOT泛型补充元数据机制测试正常???????????????????????");
        }
        /// <summary>
        /// 切换场景
        /// </summary>
        static async void LoadScene()
        {
            var handler = await Addressables.LoadSceneAsync("MainScene").Task;
            handler.ActivateAsync();

            //var test = GameObject.Find("Test");
            //test.AddComponent<Test>();
        }

        private static void LoadMetadataForAOTAssemblies()
        {
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in LoadDll.AOTMetaAssemblyNames)
            {
                byte[] dllBytes = LoadDll.GetAssetData(aotDllName);
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                GLog.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }

    }
}

