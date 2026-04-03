# SimpleContentLoader

A simple implementation of a data-driven content loading system for Unity3D. A ConfigLoader discovers content configurations and content loaders as ScriptableObjects based on Addressable labels. It then tries to match the specified labels within the configs to the labels of the loaders.

**Note:** AsyncOperationHandles of the loader and config SOs are kept until either Unload(), or the next Load() is called.

Last tested on Unity 6.3
## Dependencies
- [UniTask](https://docs.unity3d.com/Packages/com.unity.addressables@2.10/manual/index.html)
- [Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@2.10/manual/index.html)

## Usage
1. Create your config
```C#
[CreateAssetMenu(menuName = "SimpleContentLoader/Example/ContentConfig", fileName = "ContentConfig.asset")]
public class MyContentConfig : Config { }
```

2. Create your content loader
```C#
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
```

3. Instantiate both in your project hierarchy as ScriptableObjects and populate them with values
	- configs define labels and merge modes for your actual content
	- loaders define a specific implementation for loading typed content
4. Mark both as addressable and assign your labels in the inspector
	- the config loader will search for asset locations with the specified labels and merge modes
	- you need at least two labels for configs and content loaders: e.g. `contentConfig`, `contentLoader
5. Drop the ConfigLoader component into a scene and configure its config and loader labels
	- the config loader will initialize itself OnEnable (see [doc](https://docs.unity3d.com/ScriptReference/ScriptableObject.OnEnable.html) for OnEnable of ScriptableObjects)
	- content loaders are discovered first, then configs
	- after loading the configs, the config loader will immediately try to start dispatching configs to content loaders. Set loadContentAfterConfigsLoaded to false to disable this

## Example Scene
- 2 materials
- 2 AssetBundles
- 2 content configs
- 2 content loaders
- 4 Addressable Groups
	- `content1`, `content2` (actual content labels)
	- `contentLoader`
	- `contentConfig`
- both materials are applied to spheres
	- disabling a sphere releases the handles of all the assets the loader has previously loaded in
