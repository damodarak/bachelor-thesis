namespace htn_transformator
{
    internal class ToCNF : ITransformable
    {
        private PlanningDomain d;
        public ToCNF(PlanningDomain pd)
        {
            d = pd;
        }
        public PlanningDomain Transform()
        {
            d = (new RemoveBetween(d)).Transform();
            d = (new RemoveEmptyMethods(d)).Transform();
            d = (new RemoveUnitMethods(d)).Transform();

            // now we need to interchange Primitives with new Unit Compounds
            int methodCount = d.Methods.Count;
            for (int i = 0; i < methodCount; i++)
            {
                swapPrimitivesWithNewCompounds(d.Methods[i]);
            }

            // then split large methods with 2 > Compounds and

            return d;
        }
        private void swapPrimitivesWithNewCompounds(Method m)
        {
            int taskCount = m.TaskCount();
            var ordering = m.TaskOrdering();
            if (taskCount == 1 && m.RightSidePrimitive.Count == 1) return;

            for (int i = 0; i < taskCount; i++)
            {
                if (ordering[i] is CompoundTask) continue;

                PrimitiveTask pt = (PrimitiveTask)ordering[i];
            }
        }
    }
}
