namespace htn_transformator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ArgParser ap = new ArgParser(args);

            InputOutputDomain iod = new InputOutputDomain(ap.InputFile, ap.OutputFile);
            PlanningDomain pd = iod.LoadDomain();

            ITransformable trns;

            switch (ap.Type)
            {
                case TransformationType.RemoveBetween:
                    trns = new RemoveBetween(pd);
                    break;
                case TransformationType.RemoveEmptyMethods:
                    trns = new RemoveEmptyMethods(pd);
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

            //try
            //{
            //    ArgParser ap = new ArgParser(args);

            //    InputOutputDomain iod = new InputOutputDomain(ap.InputFile, ap.OutputFile);
            //    PlanningDomain pd = iod.LoadDomain();

            //    ITransformable trns;

            //    switch (ap.Type)
            //    {
            //        case TransformationType.RemoveBetween:
            //            trns = new RemoveBetween(pd);
            //            break;
            //        case TransformationType.RemoveEmptyMethods:
            //            trns = new RemoveEmptyMethods(pd);
            //            break;
            //        case TransformationType.ToCNF:
            //            trns = new ToCNF(pd);
            //            break;
            //        case TransformationType.TOGNF:
            //            trns = new ToGNF(pd);
            //            break;
            //        default:
            //            throw new Exception();
            //    }

            //    PlanningDomain result = trns.Transform();

            //    iod.StoreDomain(result);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    return;
            //}
        }
    }
}
