using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    static class Common
    {
        public static List<List<int>> GetPowerSet(List<int> set) // ChatGPT & GitHub
        {
            int n = set.Count;
            int powerSetCount = 1 << n; // 2^n
            List<List<int>> powerSet = new List<List<int>>();

            for (int i = 0; i < powerSetCount; i++)
            {
                List<int> subset = new List<int>();

                for (int j = 0; j < n; j++)
                {
                    if ((i & (1 << j)) != 0)
                    {
                        subset.Add(set[j]);
                    }
                }

                powerSet.Add(subset);
            }

            return powerSet;
        }
        public static bool ContaintsSet<T>(IEnumerable<HashSet<T>> where, HashSet<T> lookingFor)
        {
            foreach (var set in where)
            {
                if (set.SetEquals(lookingFor)) return true;
            }

            return false;
        }
        public static List<Method> MethodsWithHead(IEnumerable<Method> methods, TaskName searchedHead)
        {
            List<Method> result = new();

            foreach (Method m in methods)
            {
                if (m.Head.TaskName == searchedHead)
                    result.Add(m);
            }

            return result;
        }
    }
}
