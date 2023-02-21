using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadDll : MonoBehaviour
{
    static Assembly gameAsm;

    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
        "HotFix.dll"
    };

    void Start()
    {
        StartCoroutine(StartUpdate());

        //LoadGameDll();
        //RunMain();
    }

    private IEnumerator StartUpdate()
    {
        yield return StartCoroutine(CheckCatalog());
        LoadGameDll();
        //RunMain();
    }

    bool isUpdating = false;
    List<string> keysAsset = new List<string>();
    AsyncOperationHandle<List<IResourceLocator>> updateHandle;
    private IEnumerator CheckCatalog()
    {
        Debug.Log("CheckCatalog");
        yield return Addressables.InitializeAsync();
        Debug.Log("CheckForCatalogUpdates");
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        Debug.Log("checkHandle.IsDone");
        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            if (checkHandle.Result.Count > 0)
            {
                Debug.Log($"Catalog: {checkHandle.Result.Count} files");
                isUpdating = true;

                updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
                yield return updateHandle;
                foreach (var item in updateHandle.Result)
                {
                    foreach (var key in item.Keys)
                    {
                        if (key is string)
                        {
                            var strKey = key.ToString();
                            if (!keysAsset.Contains(strKey))
                            {
                                keysAsset.Add(key.ToString());
                            }
                        }
                    }
                }
                isUpdating = false;
                Debug.Log("Catalog: update finish");
                Addressables.Release(updateHandle);
            }
            else
            {
                Debug.Log("Catalog: no update content");
            }
        }
        else
        {
            Debug.Log("Catalog: " + checkHandle.Status);
            Debug.Log("Catalog: fail to connenct to server");
        }

        Addressables.Release(checkHandle);
    }

    bool isDownloading = false;
    int byte2mbRate = 1048576;
    AsyncOperationHandle downloadHandle;
    private IEnumerator Download()
    {
        if (keysAsset == null || keysAsset.Count == 0) yield return null;
        var downloadSizeHandle = Addressables.GetDownloadSizeAsync(keysAsset);
        yield return downloadSizeHandle;
        if (downloadSizeHandle.Result > 0)
        {
            float totalSizeMb = downloadSizeHandle.Result / byte2mbRate;
            Debug.Log("发现更新,进行下载");
            Debug.Log($"共有{totalSizeMb}MB");
            downloadHandle = Addressables.DownloadDependenciesAsync(keysAsset, Addressables.MergeMode.Union, false);
            isDownloading = true;
            yield return downloadHandle;
            isDownloading = false;
            Debug.Log("下载完成");
            Addressables.Release(downloadHandle);
            keysAsset.Clear();
        }
        else
        {
            Debug.Log("无下载内容");
            keysAsset.Clear();
        }

        Addressables.Release(downloadSizeHandle);
    }

    private IEnumerator ExecUpdate()
    {
        yield return null;
    }

    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    public static byte[] GetAssetData(string dllName)
    {
        return s_assetDatas[dllName];
    }

    async void LoadGameDll()
    {
        Debug.Log("LoadGameDll");
        //var assets = new List<string>
        //{
        //"Assembly-CSharp.dll",
        //"HotFix.dll"
        //}.Concat(AOTMetaAssemblyNames);

#if !UNITY_EDITOR
         foreach (var asset in AOTMetaAssemblyNames) 
         {
            var dll = await Addressables.LoadAssetAsync<TextAsset>(asset).Task;
            Debug.Log(asset + ".dll:" + dll.ToString());
            //var dll = Addressables.LoadAssetAsync<TextAsset>(asset).WaitForCompletion();
            s_assetDatas.Add(asset, dll.bytes);
            if ("HotFix.dll" == asset) 
            {
                gameAsm = Assembly.Load(dll.bytes);
            }
         }
#else
        gameAsm = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "HotFix");
#endif
        RunMain();
    }

    void RunMain()
    {
        if (null == gameAsm)
        {
            Debug.LogError("dll未加载");
            return;
        }
        var appType = gameAsm.GetType("HotFix.App");
        if (null == appType)
        {
            Debug.LogError("HotFix.App not found!");
            return;
        }
        var mainMethod = appType.GetMethod("Main");
        if (null == mainMethod)
        {
            Debug.LogError("Method HotFix.App.Main not found!");
            return;
        }
        mainMethod.Invoke(null, null);
    }

    public static Type GetTypeByName(string name)
    {
        if (gameAsm != null)
        {
            return gameAsm.GetType(name);
        }
        return null;
    }
}
