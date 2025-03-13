using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace htn_transformator
{
    class RemoveUnitMethods : ITransformable
    {
        /// <summary>
        /// Helper struct which tells whether the Symbol should be checked before or after the whole method.
        /// </summary>
        private struct ConstraintHelper
        {
            public PropositionalSymbol ps;
            public Type type; // AfterConstraint/BeforeConstraint
        }

        private PlanningDomain d;
        /// <summary>
        /// nullifies(Compound1,Compound2) = {{constr, ...  }, ... }
        /// </summary>
        private Dictionary<TaskName, Dictionary<TaskName, List<HashSet<ConstraintHelper>>>> unitsConstraints;
        private HashSet<ValueTuple<TaskName, TaskName>> toBeSearched;
        private HashSet<ValueTuple<TaskName, TaskName>> searched;
        public RemoveUnitMethods(PlanningDomain d)
        {
            this.d = d;
            unitsConstraints = new();
            toBeSearched = new();
            searched = new();
        }
        public PlanningDomain Transform()
        {
            foreach (var m in d.Methods)
            {
                unitMethodBase(m);
            }

            while (toBeSearched.Count != 0)
            {
                var enumerator = toBeSearched.GetEnumerator();
                enumerator.MoveNext();
                var toSearch = enumerator.Current;
                toBeSearched.Remove(toSearch);
                searched.Add(toSearch);
                searchNullifiedUnitPair(toSearch);
            }

            // create new methods based on the previous search, new methods containt >= 2 subtasks
            foreach (var pair in searched)
            {
                // new methods are combined from the nullifieUnitPair search and methods with >= 2 subtasks
                List<Method> appendMethods = Common.MethodsWithHead(d.Methods, pair.Item2);

                // empty methods were removed in the previous transformation, here we remove unit methods
                for (int i = 0; i < appendMethods.Count; i++)
                {
                    if (appendMethods[i].isUnit())
                    {
                        appendMethods.RemoveAt(i);
                        i--;
                    }
                }

                constructNewNonUnitMethods(pair.Item1, appendMethods);
            }

            // remove unit methods
            for (int i = 0; i < d.Methods.Count; i++)
            {
                if (d.Methods[i].isUnit())
                {
                    d.RemoveMethod(d.Methods[i]);
                    i--;
                }
            }

            return d;
        }
        /// <summary>
        /// Create new tasks with the TaskName head, bodies from appendMethods, additional before/after constraints are added based on the previous
        /// search.
        /// </summary>
        /// <param name="appendMethods"></param>
        /// <param name="pair"></param>
        private void constructNewNonUnitMethods(TaskName head, List<Method> appendMethods)
        {
            foreach (var m in appendMethods)
            {
                // unitsConstraints for each valid pair of TaskNames it returns the list of sets
                // each set is consisted of state constraints that should be added to new methods
                foreach (var constraintsSet in unitsConstraints[head][m.Head.TaskName])
                {
                    Method newConnectedMethod = new Method(new CompoundTask(head, -1), m);
                    var ordering = newConnectedMethod.TaskTotalOrdering();
                    foreach (var constr in constraintsSet)
                    {
                        if (constr.type == typeof(BeforeConstraint))
                        {
                            var bc = new BeforeConstraint(constr.ps, ordering[0]); // before first task in the method
                            newConnectedMethod.AppendBefore(bc);
                        }
                        else
                        {
                            var ac = new AfterConstraint(constr.ps, ordering[newConnectedMethod.TaskCount() - 1]); // after the last task
                            newConnectedMethod.AppendAfter(ac);
                        }
                    }
                    d.AppendMethod(newConnectedMethod);
                }
            }
        }
        /// <summary>
        /// Induction part of the search. For the input param pair (A,B) we search other pairs (B,C) s.t. new constraints HashSet
        /// (A,C) is found. If we find one then (A,C) is added to the future search.
        /// </summary>
        /// <param name="pair"></param>
        private void searchNullifiedUnitPair(ValueTuple<TaskName, TaskName> pair)
        {
            List<HashSet<ConstraintHelper>> firstPairCons = unitsConstraints[pair.Item1][pair.Item2]; // always exists
            List<Method> searchMethods = Common.MethodsWithHead(d.Methods, pair.Item2);

            // we search only for unit methods X->Y s.t. Y != pair.Item1 because we do not want to create cycles of nullifie methods
            for (int i = 0; i < searchMethods.Count; i++)
            {
                if (!searchMethods[i].isUnit() ||
                    pair.Item1 == searchMethods[i].RightSideCompound[0].TaskName)
                {
                    searchMethods.RemoveAt(i);
                    i--;
                }
            }

            foreach (var m in searchMethods)
            {
                CompoundTask sub = m.RightSideCompound[0];

                // unitsConstraints[m.Head.TaskName] must exist at this point

                if (!unitsConstraints[pair.Item1].ContainsKey(sub.TaskName))
                {
                    unitsConstraints[pair.Item1][sub.TaskName] = new();
                }

                // unitsConstraints[pair.Item1][sub.TaskName] exists

                List<HashSet<ConstraintHelper>> secondPairCons = unitsConstraints[pair.Item2][sub.TaskName]; // always exists

                int secondLength = secondPairCons.Count;

                foreach (var firstCons in firstPairCons)
                {
                    for (int i = 0; i < secondLength; i++)
                    {
                        HashSet<ConstraintHelper> combined = new(firstCons);
                        combined.UnionWith(secondPairCons[i]);

                        // check if this set is already containted
                        if (!Common.ContaintsSet(unitsConstraints[pair.Item1][sub.TaskName], combined))
                        {
                            unitsConstraints[pair.Item1][sub.TaskName].Add(combined);
                            toBeSearched.Add(new(pair.Item1, sub.TaskName));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Create base case for the future search of Nullified unit methods. For each Unit method A->B we create a HashSet
        /// of constraints that target B. There may be multiple unit methods A->B with different sets of state-constraints.
        /// Each such method is one HashSet.
        /// </summary>
        /// <param name="m"></param>
        private void unitMethodBase(Method m)
        {
            if (!m.isUnit()) return;

            if (!unitsConstraints.ContainsKey(m.Head.TaskName))
            {
                unitsConstraints[m.Head.TaskName] = new(); // unitsConstraints[A] ->

            }

            if (!unitsConstraints[m.Head.TaskName].ContainsKey(m.RightSideCompound[0].TaskName))
            {
                unitsConstraints[m.Head.TaskName][m.RightSideCompound[0].TaskName] = new(); // unitsConstraints[A][B] -> List
            }

            HashSet<ConstraintHelper> nulledConstraints = new();

            foreach (var after in m.Afters)
            {
                nulledConstraints.Add(new ConstraintHelper { ps = after.Symbol, type = typeof(AfterConstraint) });
            }
            foreach (var before in m.Befores)
            {
                nulledConstraints.Add(new ConstraintHelper { ps = before.Symbol, type = typeof(BeforeConstraint) });
            }

            if (!Common.ContaintsSet(unitsConstraints[m.Head.TaskName][m.RightSideCompound[0].TaskName], nulledConstraints))
            {
                // unitsConstraints[A][B] -> List.Add(HashSet)
                unitsConstraints[m.Head.TaskName][m.RightSideCompound[0].TaskName].Add(nulledConstraints);
                toBeSearched.Add(new ValueTuple<TaskName, TaskName>(m.Head.TaskName, m.RightSideCompound[0].TaskName));
            }
        }
    }
}
