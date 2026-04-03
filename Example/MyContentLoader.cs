using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SimpleContentLoader.Example
{
    [CreateAssetMenu(menuName = "SimpleContentLoader/Example/ContentLoader", fileName = "ContentLoader.asset")]
    public class MyContentLoader : ContentLoader<Material>
    {
        void OnEnable()
        {
            onLoadedAssets.RemoveListener(HandleAssetsLoaded);
            onLoadedAssets.AddListener(HandleAssetsLoaded);
        }

        void OnDisable()
        {
            onLoadedAssets.RemoveListener(HandleAssetsLoaded);
        }

        public override void HandleAssetLoaded(Material asset)
        {
            Debug.Log($"Loaded mat: {asset.name}");
        }

        public void HandleAssetsLoaded(IList<Material> assets)
        {
            Debug.Log($"Loaded all mats: {assets.ToCommaSeparatedString()}");
            // or just use the handles: _assetLocationHandles, _assetHandles
        }
    }
}
