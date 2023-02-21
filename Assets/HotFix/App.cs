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
            GLog.Log("AOT���򼯼������!");
#endif
            TestAOTGeneric();
            LoadScene();
            return 0;
        }
        /// <summary>
        /// ���� aot����
        /// </summary>
        public static void TestAOTGeneric()
        {
            var arr = new List<MyValue>();
            arr.Add(new MyValue() { x = 1, y = 10, s = "abc" });
            GLog.Log("AOT���Ͳ���Ԫ���ݻ��Ʋ�������???????????????????????");
        }
        /// <summary>
        /// �л�����
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
            /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
            /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in LoadDll.AOTMetaAssemblyNames)
            {
                byte[] dllBytes = LoadDll.GetAssetData(aotDllName);
                // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                GLog.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }

    }
}

