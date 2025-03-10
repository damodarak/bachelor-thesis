using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    static class Common
    {
        /// <summary>
        /// Calculate all subsets of the given set.
        /// </summary>
        /// <param name="set"></param>
        /// <returns>Power set.</returns>
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
        /// <summary>
        /// Enumerates over the given collection and looks for the given HashSet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="lookingFor"></param>
        /// <returns>Returns true if the set was found, false otherwise.</returns>
        public static bool ContaintsSet<T>(IEnumerable<HashSet<T>> where, HashSet<T> lookingFor)
        {
            foreach (var set in where)
            {
                if (set.SetEquals(lookingFor)) return true;
            }

            return false;
        }
        /// <summary>
        /// Find all methods in collectin with the given TaskName in the Head.
        /// </summary>
        /// <param name="methods"></param>
        /// <param name="searchedHead"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Find all SingleTaskStateConstraint targetins the Task t in the given collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<T> TargetedStateConstraint<T>(IEnumerable<T> collection, Task t) where T : SingleTaskStateConstraint
        {
            List<T> result = new();

            foreach (var constr in collection)
            {
                if (constr.Task == t) result.Add(constr);
            }

            return result;
        }
    }
}
