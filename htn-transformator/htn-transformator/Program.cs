namespace htn_transformator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
			{
				ArgParser ap = new ArgParser(args);

                InputOutputDomain iod = new InputOutputDomain(ap.inputFile, ap.outputFile);
                PlanningDomain pd = iod.LoadDomain();

                ITransformable trns;

                switch (ap.type)
                {
                    case TransformationType.RemoveBetween:
                        trns = new RemoveBetween(pd);
                        break;
                    case TransformationType.ToCNF:
                        trns = new ToCNF(pd);
                        break;
                    case TransformationType.TOGNF:
                        trns = new ToGNF(pd);
                        break;
                    default:
                        throw new Exception();
                }

                PlanningDomain result = trns.Transform();

                iod.StoreDomain(result);
            }
			catch (Exception)
			{
				return;
			}
        }
    }
}
