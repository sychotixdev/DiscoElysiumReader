using UnityEngine;

namespace DiscoElysiumReader.Mod
{
    public class Loader
    {
        public static void Init()
        {
            Loader.Load = new GameObject();
            Loader.Load.AddComponent<DialogReader>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.Load);
        }

        public static void Unload()
        {
            UnityEngine.Object.Destroy(Loader.Load);
            Load = null;
        }

        private static GameObject Load;
    }
}
