using SimpleContentLoader.Example;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereColorChange : MonoBehaviour
{
#pragma warning disable CS0108
    [SerializeField]
    MeshRenderer renderer;
#pragma warning restore CS0108

    [SerializeField]
    MyContentLoader contentLoader;

    Material _mat;

    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        _mat = renderer.material;
        contentLoader.onLoadedAssets.AddListener(SetMaterial);
    }

    void OnDisable()
    {
        contentLoader.Unload();
        renderer.material = _mat;
    }

    void SetMaterial(IList<Material> materials)
    {
        renderer.material = materials.First();
    }
}
