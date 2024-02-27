
using System.Runtime.CompilerServices;

namespace BusinessLayer
{
    public static class LocalCache
    {
        private static Dictionary<int, (int, int)> logCache = new Dictionary<int, (int, int)>();
        private static int activeIndex = 0;
        public static void ClearCache()
        {
            logCache.Clear();
        }

        public static void SetActiveIndex(int id)
        {
            activeIndex = id;
        }

        public static int GetActiveIndex()
        {
            return activeIndex;
        }

        public static void AddItem(int key, int value1, int value2)
        {
            logCache.Add(key, (value1, value2));
        }

        public static (int, int) GetItemByKey(int key)
        {
            return logCache[key];
        }

        public static List<int> FindKeysByItemInRange(int value)
        {
            List<int> keysInRange = new List<int>();

            foreach (var kvp in logCache)
            {
                int itemMinValue = kvp.Value.Item1;
                int itemMaxValue = kvp.Value.Item2;

                if (itemMinValue <= value && value < itemMaxValue)
                {
                    keysInRange.Add(kvp.Key);
                    break;
                }
            }
            return keysInRange;
        }
    }
}
