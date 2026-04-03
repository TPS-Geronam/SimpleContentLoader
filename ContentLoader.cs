using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace SimpleContentLoader
{
    public abstract class ContentLoader<T> : ScriptableObject, IContentLoader
        where T : UnityEngine.Object
    {
        [field: SerializeField, Tooltip("Identifier of this content loader.")]
        public string LoaderId { get; private set; }

        [Tooltip("The content labels this loader is responsible for.")]
        public List<string> contentLabels = new();
        [Tooltip("Event to invoke when this loader finishes loading its assets.")]
        public UnityEvent<IList<T>> onLoadedAssets = new();

        public List<string> GetContentLabels() => contentLabels;
        public string GetLoaderId() => LoaderId;

        protected AsyncOperationHandle<IList<IResourceLocation>> _assetLocationHandles;
        protected AsyncOperationHandle<IList<T>> _assetHandles;

        public async UniTask Load(Config config)
        {
            Unload();

            _assetLocationHandles = Addressables.LoadResourceLocationsAsync(contentLabels, config.MergeMode, typeof(T));
            await _assetLocationHandles.Task.AsUniTask();

            if (HandleStateIsInvalid(_assetLocationHandles))
                throw new Exception($"Content loader {LoaderId} could not load asset locations");

            _assetHandles = Addressables.LoadAssetsAsync<T>(_assetLocationHandles.Result, HandleAssetLoaded);
            await _assetHandles.Task.AsUniTask();

            if (HandleStateIsInvalid(_assetHandles))
                throw new Exception($"Content loader {LoaderId} could not load assets");

            onLoadedAssets?.Invoke(_assetHandles.Result);
        }

        public void Unload()
        {
            if (_assetLocationHandles.IsValid())
                Addressables.Release(_assetLocationHandles);
            if (_assetHandles.IsValid())
                Addressables.Release(_assetHandles);
        }

        public abstract void HandleAssetLoaded(T asset);

        protected bool HandleStateIsInvalid<U>(AsyncOperationHandle<IList<U>> handles)
        {
            return !handles.Task.IsCompletedSuccessfully || handles.Status != AsyncOperationStatus.Succeeded;
        }
    }
}
