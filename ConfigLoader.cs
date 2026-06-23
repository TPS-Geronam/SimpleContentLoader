using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace SimpleContentLoader
{
    public class ConfigLoader : MonoBehaviour
    {
        [Header("Content Loaders")]
        [SerializeField, Tooltip("All instantiated content loaders.")]
        Dictionary<string, IContentLoader> contentLoaders;

        [SerializeField, Tooltip("Keys for loading content loaders.")]
        List<string> loaderLabels = new() { "contentLoader" };

        [SerializeField, Tooltip("Merge mode for loading config locations.")]
        Addressables.MergeMode loaderMergeMode = Addressables.MergeMode.Union;

        [Header("Configs")]
        [SerializeField, Tooltip("All loaded content configs.")]
        Dictionary<string, Config> configs;

        [SerializeField, Tooltip("Keys for loading config locations.")]
        List<string> configLabels = new() { "contentConfig" };

        [SerializeField, Tooltip("Merge mode for loading config locations.")]
        Addressables.MergeMode configMergeMode = Addressables.MergeMode.Union;

        [SerializeField, Tooltip("Whether to try to start dispatching configs to content loaders in Start().")]
        bool loadContentOnStart = true;

        AsyncOperationHandle<IList<IResourceLocation>> _loaderLocationHandles;
        AsyncOperationHandle<IList<IResourceLocation>> _configLocationHandles;
        AsyncOperationHandle<IList<IContentLoader>> _loaderHandles;
        AsyncOperationHandle<IList<Config>> _configHandles;

        void Awake()
        {
            _ = Initialize();
        }

        void Start()
        {
            if (loadContentOnStart)
                _ = LoadContent();
        }

        async UniTask Initialize()
        {
            await LoadLoaders();
            await LoadConfigs();
        }

        public async UniTask LoadContent()
        {
            while (configs == null)
                await UniTask.NextFrame();

            foreach (var config in configs)
                _ = LoadContentOfConfig(config.Value);
        }

        public async UniTask LoadContentOfConfig(Config config)
        {
            var matchedLoader = contentLoaders.FirstOrDefault(
                pair => LoaderMatchesConfigLabels(pair.Key, pair.Value, config)
            );
            var loader = matchedLoader.Value;
            if (loader != null)
                await loader.Load(config);
            else
                throw new Exception("Config loader could not match a content loader");
        }

        public async UniTask LoadLoaders()
        {
            Unload(UnloadTarget.Loader);

            var loaderLocationHandles = await LoadLocations(loaderLabels, loaderMergeMode, typeof(IContentLoader));
            var loaderLocations = loaderLocationHandles.Result;
            var loaderHandles = await LoadAssets<IContentLoader>(loaderLocations);
            var loaders = loaderHandles.Result;

            contentLoaders = loaders
                .OrderBy(loader => loader.GetLoaderId())
                .ToDictionary(loader => loader.GetLoaderId(), loader => loader);
        }

        public async UniTask LoadConfigs()
        {
            Unload(UnloadTarget.Configs);

            var configLocationHandles = await LoadLocations(configLabels, configMergeMode, typeof(Config));
            var configLocations = configLocationHandles.Result;
            var configHandles = await LoadAssets<Config>(configLocations);
            var configs = configHandles.Result;

            this.configs = configs
                .OrderBy(config => config.ConfigId)
                .ToDictionary(config => config.ConfigId, config => config);
        }

        async UniTask<AsyncOperationHandle<IList<IResourceLocation>>> LoadLocations(List<string> keys, Addressables.MergeMode mergeMode, Type type)
        {
            var locations = Addressables.LoadResourceLocationsAsync(keys, mergeMode, type);
            await locations.Task.AsUniTask();

            if (locations.Task.IsCompletedSuccessfully && locations.Status == AsyncOperationStatus.Succeeded)
                return locations;

            throw new Exception("Config loader could not load locations");
        }

        async UniTask<AsyncOperationHandle<IList<T>>> LoadAssets<T>(IList<IResourceLocation> locations)
        {
            var handle = Addressables.LoadAssetsAsync<T>(locations, null);
            await handle.Task.AsUniTask();

            if (handle.Task.IsCompletedSuccessfully && handle.Status == AsyncOperationStatus.Succeeded)
                return handle;

            throw new Exception("Config loader could not load assets");
        }

        bool LoaderMatchesConfigLabels(string loaderId, IContentLoader contentLoader, Config config)
        {
            var configLabels = config.ContentLabels;
            bool loaderInvalid = string.IsNullOrEmpty(loaderId) || contentLoader == null;
            bool configInvalid = configLabels == null || configLabels.Count == 0;
            if (loaderInvalid || configInvalid) 
                return false;

            var loaderContentLabels = contentLoader.GetContentLabels();
            bool loaderContainsConfigLabels = configLabels.All(loaderContentLabels.Contains);
            bool loaderContainsExactLabelCount = !config.ExactLoaderMatch || loaderContentLabels.Count == configLabels.Count;
            return loaderContainsConfigLabels && loaderContainsExactLabelCount;
        }

        public void Unload(UnloadTarget configsOrLoader = UnloadTarget.Both)
        {
            bool unloadLoaders = configsOrLoader == UnloadTarget.Loader;
            bool unloadConfigs = configsOrLoader == UnloadTarget.Configs;
            bool unloadBoth = configsOrLoader == UnloadTarget.Both;

            if (unloadLoaders || unloadBoth)
            {
                if (_loaderLocationHandles.IsValid())
                    Addressables.Release(_loaderLocationHandles);
                if (_loaderHandles.IsValid())
                    Addressables.Release(_loaderHandles);
            }
            if (unloadConfigs || unloadBoth)
            {
                if (_configLocationHandles.IsValid())
                    Addressables.Release(_configLocationHandles);
                if (_configHandles.IsValid())
                    Addressables.Release(_configHandles);
            }
        }

        public enum UnloadTarget
        {
            Loader,
            Configs,
            Both
        }
    }
}
