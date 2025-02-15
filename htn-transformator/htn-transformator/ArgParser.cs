using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class ArgParser
    {
        public string inputFile { get; private set; }
        public string outputFile { get; private set; }
        public TransformationType type { get; private set; }

        public ArgParser(string[] args) 
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Incorrect argument length!");
                throw new Exception();
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i"  && i != args.Length - 1)
                {
                    inputFile = args[i + 1];
                    i++;
                }
                else if (args[i] == "-o" && i != args.Length - 1)
                {
                    outputFile = args[i + 1];
                    i++;
                }
                else if (args[i] == "--between")
                {
                    type = TransformationType.RemoveBetween;
                }
                else if (args[i] == "--tocnf")
                {
                    type = TransformationType.ToCNF;
                }
                else if (args[i] == "--tognf")
                {
                    type = TransformationType.TOGNF;
                }
                else
                {
                    Console.WriteLine("Incorrect arguments!");
                    throw new Exception();
                }               
            }

            if(!File.Exists(inputFile))
            {
                Console.WriteLine("Incorrect input file!");
                throw new Exception();
            }
        }
    }
}
