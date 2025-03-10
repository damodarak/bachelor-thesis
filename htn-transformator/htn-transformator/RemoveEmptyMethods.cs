using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    class RemoveEmptyMethods : ITransformable
    {
        private PlanningDomain d;
        /// <summary>
        /// TaskName -> set of sets of symbols that can be nullified using empty methods.
        /// </summary>
        private Dictionary<TaskName, List<HashSet<PropositionalSymbol>>> nullifies = new();
        private HashSet<TaskName> toBeSearched = new();
        /// <summary>
        /// TaskName -> set of Methods which contain TaskName in subtasks.
        /// </summary>
        private Dictionary<TaskName, HashSet<Method>> containsInSubtasks = new();
        public RemoveEmptyMethods(PlanningDomain d) { this.d = d; }
        public PlanningDomain Transform()
        {
            if (!d.ContaintsEmptyMethod()) return d; // nothing to do

            d = (new RemoveBetween(d)).Transform();
            // now we know that this is totally ordered domain and does not contain any BetweenConstraints

            containsInInit();
            nullifiesBase();

            while (toBeSearched.Count != 0)
            {
                var enumerator = toBeSearched.GetEnumerator();
                enumerator.MoveNext();
                TaskName toSearch = enumerator.Current;
                toBeSearched.Remove(toSearch);
                searchNullifiedTaskName(toSearch);
            }

            int methodCount = d.Methods.Count; // may change after insertion of new nullified methods

            for (int i = 0; i < methodCount; i++)
            {
                List<Task> ordering = d.Methods[i].TaskTotalOrdering();
                List<int> nullifiableIndices = findNullifiableIndices(d.Methods[i], ordering);

                if (nullifiableIndices.Count != 0)
                {
                    List<List<int>> powerSet = Common.GetPowerSet(nullifiableIndices);

                    // we do not want empty set
                    powerSet.RemoveAt(0);
                    // we do not want to remove all tasks
                    if (powerSet[powerSet.Count - 1].Count == ordering.Count) powerSet.RemoveAt(powerSet.Count - 1);

                    foreach (var indices in powerSet)
                    {
                        nullifieMethodAndAppendToDomain(d.Methods[i], indices, 0, ordering, new List<Tuple<int, PropositionalSymbol>>());
                    }
                }
            }

            // remove all empty Methods
            for (int i = 0; i < d.Methods.Count; i++)
            {
                if (d.Methods[i].IsEmpty())
                {
                    d.RemoveMethod(d.Methods[i]);
                    i--;
                }
            }

            return d;
        }
        /// <summary>
        /// Create new Methods without some Nullable Tasks with some combinations of PropositionalSymbols in new StateConstraints.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="nullTaskIndices"></param>
        /// <param name="index"></param>
        /// <param name="ordering"></param>
        /// <param name="insert"></param>
        private void nullifieMethodAndAppendToDomain(Method m, List<int> nullTaskIndices, int index, List<Task> ordering, List<Tuple<int, PropositionalSymbol>> insert)
        {
            //append Constraints
            //shift; RemoveTaskAndShiftConstraints(Task t)
            //delete tasks; RemoveTaskAndShiftConstraints(Task t)

            for (int i = 0; i < nullifies[ordering[nullTaskIndices[index]].TaskName].Count; i++)
            {
                List<Tuple<int, PropositionalSymbol>> copy = new(insert);

                foreach (var symbol in nullifies[ordering[nullTaskIndices[index]].TaskName][i])
                {
                    copy.Add(new Tuple<int, PropositionalSymbol>(nullTaskIndices[index], symbol));
                }

                if (index == nullTaskIndices.Count - 1)
                {
                    //create null method
                    Method nulledMethod = new Method(m.Head, m);
                    var nulledMethodOrdering = nulledMethod.TaskTotalOrdering();

                    foreach (var newBefore in copy)
                    {
                        nulledMethod.AppendBefore(new BeforeConstraint(newBefore.Item2, nulledMethodOrdering[newBefore.Item1]));
                    }

                    for (int j = nullTaskIndices.Count - 1; j >= 0; j--) // nullTaskIndices are in ascending order
                    {
                        nulledMethod.RemoveTaskAndShiftConstraints(nulledMethodOrdering[nullTaskIndices[j]]);
                    }

                    d.AppendMethod(nulledMethod);
                }
                else
                    nullifieMethodAndAppendToDomain(m, nullTaskIndices, index + 1, ordering, copy);
            }
        }
        /// <summary>
        /// For the given TaskName tries to find new combination of PropositionalTasks that can be nullified using some sequence of nullifiable
        /// Methods.
        /// </summary>
        /// <param name="tn"></param>
        private void searchNullifiedTaskName(TaskName tn)
        {
            if (!containsInSubtasks.ContainsKey(tn)) return;

            foreach (Method m in containsInSubtasks[tn])
            {
                if (m.RightSidePrimitive.Count != 0) continue;
                if (!nullifiable(m)) continue;

                HashSet<PropositionalSymbol> symbols = symbolsFromConstraints(m);
                createNewNullifiedSymbols(m, 0, symbols);
            }
        }
        /// <summary>
        /// Recursive function over nullifiable Method m. Tries to find new combination of PropositionalSymbols that can be nullified.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="index"></param>
        /// <param name="symbols"></param>
        private void createNewNullifiedSymbols(Method m, int index, HashSet<PropositionalSymbol> symbols)
        {
            HashSet<PropositionalSymbol> symbolsCopy;
            int length = nullifies[m.RightSideCompound[index].TaskName].Count; // may be increased in the lowest level of recursion
            for (int i = 0; i < length; i++)
            {
                symbolsCopy = new(symbols);
                symbolsCopy.UnionWith(nullifies[m.RightSideCompound[index].TaskName][i]);

                if (index == m.RightSideCompound.Count - 1)
                {
                    if (!nullifies.ContainsKey(m.Head.TaskName)) nullifies[m.Head.TaskName] = new();

                    if (!Common.ContaintsSet(nullifies[m.Head.TaskName], symbolsCopy))
                    {
                        nullifies[m.Head.TaskName].Add(symbolsCopy);
                        toBeSearched.Add(m.Head.TaskName);
                    }
                    else 
                        return;
                    
                }
                else
                {
                    createNewNullifiedSymbols(m, index + 1, symbolsCopy);
                }
            }
        }
        /// <summary>
        /// Find all PropositionalSymbols that are contained in After/BeforeConstraints.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private HashSet<PropositionalSymbol> symbolsFromConstraints(Method m)
        {
            HashSet<PropositionalSymbol> symbols = new();

            foreach (var before in m.Befores)
            {
                symbols.Add(before.Symbol);
            }
            foreach (var after in m.Afters)
            {
                symbols.Add(after.Symbol);
            }

            return symbols;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tn"></param>
        /// <returns>true if the all subtasks from the given Method can be nullified with a sequence of Methods, false otherwise.</returns>
        private bool nullifiable(Method m)
        {
            foreach (CompoundTask ct in m.RightSideCompound)
            {
                if (!nullifiable(ct.TaskName)) return false;
            }

            return m.RightSidePrimitive.Count == 0; // all CompoundTasks are nullifiable, but there cannot be any PrimitiveTasks
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tn"></param>
        /// <returns>true if the given TaskName can be nullified with a sequence of Methods, false otherwise.</returns>
        private bool nullifiable(TaskName tn) { return nullifies.ContainsKey(tn); }
        /// <summary>
        /// Finds indices of nullifiable TaskNames in the given Method.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="ordering"></param>
        /// <returns></returns>
        private List<int> findNullifiableIndices(Method m, List<Task> ordering)
        {
            List<int> result = new();

            for (int i = 0; i < ordering.Count; i++)
            {
                if (nullifiable(ordering[i].TaskName)) result.Add(i);
            }

            return result;
        }
        /// <summary>
        /// init toBeSearched, nullifies based on empty Methods.
        /// </summary>
        private void nullifiesBase()
        {
            foreach (Method m in d.Methods)
            {
                if (m.IsEmpty())
                {
                    TaskName headName = m.Head.TaskName;

                    toBeSearched.Add(headName);

                    if (!nullifies.ContainsKey(headName))
                        nullifies[headName] = new();

                    nullifies[headName].Add(new HashSet<PropositionalSymbol>()); // empty set = nullifies to nothing
                }
            }
        }
        /// <summary>
        /// Fill containsInSubtasks (subtask -> Method) with data.
        /// </summary>
        private void containsInInit()
        {
            foreach (Method m in d.Methods)
            {
                foreach (Task t in m.RightSideCompound)
                {
                    if (!containsInSubtasks.ContainsKey(t.TaskName))
                    {
                        containsInSubtasks[t.TaskName] = new HashSet<Method>();
                    }

                    containsInSubtasks[t.TaskName].Add(m);
                }
            }
        }
    }
}
