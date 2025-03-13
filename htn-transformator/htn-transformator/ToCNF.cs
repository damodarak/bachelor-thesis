namespace htn_transformator
{
    internal class ToCNF : ITransformable
    {
        private PlanningDomain d;
        private Dictionary<TaskName, TaskName> primitiveToNewCompound;
        private readonly string newCompoundToPrimitivePrefix = "NewComToPri:"; // no task name can have this prefix, so no conflict will arise
        private readonly string newCompoundSplitPrefix = "NewComSplit:"; // no task name can have this prefix, so no conflict will arise
        public ToCNF(PlanningDomain pd)
        {
            d = pd;
            primitiveToNewCompound = new();
        }
        public PlanningDomain Transform()
        {
            d = (new RemoveBetween(d)).Transform();
            d = (new RemoveEmptyMethods(d)).Transform();
            d = (new RemoveUnitMethods(d)).Transform();
            // now all methods have >= 2 subtasks

            // now we need to interchange Primitives with new Unit Compounds
            int methodCount = d.Methods.Count;
            for (int i = 0; i < methodCount; i++)
            {
                swapPrimitivesWithCompoundsAndMethods(d.Methods[i]);
            }

            // and lastly we need to split large methods with 2 > Compounds
            methodCount = d.Methods.Count; // new methods might be added
            for (int i = 0; i < methodCount; i++)
            {
                splitLargeMethods(d.Methods[i]);
            }

            // remove large methods with 2 > Compounds
            for (int i = 0; i < d.Methods.Count; i++)
            {
                if (d.Methods[i].TaskCount() > 2)
                {
                    d.RemoveMethod(d.Methods[i]);
                    i--;
                }
            }

            return d;
        }
        /// <summary>
        /// For each Method with >= 2 subtasks we interchange PrimitiveTasks with CompoundTask and creat a single method that 
        /// decomposes this new CompoundTask to the interchanged PrimitiveTask.
        /// </summary>
        /// <param name="m"></param>
        private void swapPrimitivesWithCompoundsAndMethods(Method m)
        {
            int taskCount = m.TaskCount();
            var ordering = m.TaskTotalOrdering();
            if (taskCount == 1 && m.RightSidePrimitive.Count == 1) return;

            for (int i = 0; i < taskCount; i++)
            {
                if (ordering[i] is CompoundTask) continue;
                // ordering[i] is PrimitiveTask here

                // same PrimitiveTask might be handled in some previous Method
                if (!primitiveToNewCompound.ContainsKey(ordering[i].TaskName))
                {
                    primitiveToNewCompound[ordering[i].TaskName] = new TaskName($"{newCompoundToPrimitivePrefix}{ordering[i].TaskName.Name}");
                    Method primitiveUnitMethod = new Method(new CompoundTask(primitiveToNewCompound[ordering[i].TaskName], -1));
                    primitiveUnitMethod.AppendTask(new PrimitiveTask(ordering[i].TaskName, 1)); // append removed Primitive to a new Method
                    d.AppendMethod(primitiveUnitMethod);
                }

                CompoundTask exchange = new CompoundTask(primitiveToNewCompound[ordering[i].TaskName], ordering[i].TaskIndex);
                m.SwapTask(ordering[i], exchange);
            }
        }
        /// <summary>
        /// For each Method with >=3 subtasks (in this case all must be CompoundTask) create new Methods (with 2 subtask) that gradually simulate
        /// the old Method.
        /// </summary>
        /// <param name="m"></param>
        private void splitLargeMethods(Method m)
        {
            if (m.TaskCount() < 3) return;

            var ordering = m.TaskTotalOrdering();

            TaskName nextHead = m.Head.TaskName;
            int nextSplitNum = 1;

            for (int i = 0; i < ordering.Count - 2; i++)
            {
                List<BeforeConstraint> targetedBefores = Common.TargetedStateConstraint(m.Befores, ordering[i]);
                List<AfterConstraint> targetedAfters = Common.TargetedStateConstraint(m.Afters, ordering[i]);

                CompoundTask left = new CompoundTask(ordering[i].TaskName, ordering[i].TaskIndex);
                CompoundTask right = new CompoundTask($"{newCompoundSplitPrefix}{nextSplitNum++}", 1);

                Method splitMethod = new Method(new CompoundTask(nextHead, -1));
                nextHead = right.TaskName;
                splitMethod.AppendTask(left);
                splitMethod.AppendTask(right);

                splitMethod.AppendOrderingConstraint(new OrderConstraint(left, right));

                foreach (var before in targetedBefores)
                {
                    splitMethod.AppendBefore(new BeforeConstraint(before.Symbol, left));
                }
                foreach (var after in targetedAfters)
                {
                    splitMethod.AppendAfter(new AfterConstraint(after.Symbol, left));
                }

                d.AppendMethod(splitMethod);
            }

            // create last method
            List<BeforeConstraint> targetedBeforesLeft = Common.TargetedStateConstraint(m.Befores, ordering[ordering.Count - 2]);
            List<AfterConstraint> targetedAftersLeft = Common.TargetedStateConstraint(m.Afters, ordering[ordering.Count - 2]);
            List<BeforeConstraint> targetedBeforesRight = Common.TargetedStateConstraint(m.Befores, ordering[ordering.Count - 1]);
            List<AfterConstraint> targetedAftersRight = Common.TargetedStateConstraint(m.Afters, ordering[ordering.Count - 1]);

            CompoundTask leftLast = new CompoundTask(ordering[ordering.Count - 2].TaskName, ordering[ordering.Count - 2].TaskIndex);
            CompoundTask rightLast = new CompoundTask(ordering[ordering.Count - 1].TaskName, ordering[ordering.Count - 1].TaskIndex);

            Method splitMethodLast = new Method(new CompoundTask(nextHead, -1));
            splitMethodLast.AppendTask(leftLast);
            splitMethodLast.AppendTask(rightLast);

            splitMethodLast.AppendOrderingConstraint(new OrderConstraint(leftLast, rightLast));

            foreach (var before in targetedBeforesLeft)
            {
                splitMethodLast.AppendBefore(new BeforeConstraint(before.Symbol, leftLast));
            }
            foreach (var after in targetedAftersLeft)
            {
                splitMethodLast.AppendAfter(new AfterConstraint(after.Symbol, leftLast));
            }
            foreach (var before in targetedBeforesRight)
            {
                splitMethodLast.AppendBefore(new BeforeConstraint(before.Symbol, rightLast));
            }
            foreach (var after in targetedAftersRight)
            {
                splitMethodLast.AppendAfter(new AfterConstraint(after.Symbol, rightLast));
            }

            d.AppendMethod(splitMethodLast);
        }
    }
}
