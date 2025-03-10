using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace htn_transformator
{
    internal class ArgParser
    {
        /// <summary>
        /// Input text file passed as an argument to the program. Must exist.
        /// </summary>
        public string InputFile { get; private set; }
        /// <summary>
        /// Output file passed as an argument to the program. May not exist.
        /// If nothing is passed then the result is passed to the standard outpout.
        /// </summary>
        public string OutputFile { get; private set; } = "";
        /// <summary>
        /// Specifies the transformation type.
        /// </summary>
        public TransformationType Type { get; private set; }
        /// <summary>
        /// Constructs an object and also parses the input arguments.
        /// </summary>
        /// <param name="args">cmd arguments</param>
        /// <exception cref="Exception"></exception>
        public ArgParser(string[] args) 
        {
            // the input file must be always given
            if (args.Length != 5 && args.Length != 3)
            {
                throw new Exception("Incorrect argument length!");
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i"  && i != args.Length - 1)
                {
                    InputFile = args[i + 1];
                    i++;
                }
                else if (args[i] == "-o" && i != args.Length - 1)
                {
                    OutputFile = args[i + 1];
                    i++;
                }
                else if (args[i] == "--between")
                {
                    Type = TransformationType.RemoveBetween;
                }
                else if (args[i] == "--empty")
                {
                    Type = TransformationType.RemoveEmptyMethods;
                }
                else if (args[i] == "--tocnf")
                {
                    Type = TransformationType.ToCNF;
                }
                else if (args[i] == "--tognf")
                {
                    Type = TransformationType.TOGNF;
                }
                else
                {
                    throw new Exception("Incorrect arguments!");
                }               
            }

            if(!File.Exists(InputFile))
            {
                throw new Exception("Incorrect input file!");
            }
        }
    }
}
