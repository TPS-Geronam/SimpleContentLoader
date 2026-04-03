using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleContentLoader
{
    public interface IContentLoader
    {
        public string GetLoaderId();
        public List<string> GetContentLabels();
        public UniTask Load(Config config);
        public void Unload();
    }
}
