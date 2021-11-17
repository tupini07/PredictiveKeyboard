using CommandLine;

namespace Utils
{
    class Program
    {

        class Options
        {
            // Omitting long name, defaults to name of property, ie "--verbose"
            [Option(
              Default = false,
              HelpText = "Generates pre-made-models from testing data to be used in the actual application")]
            public bool GenerateModels { get; set; }
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }
        static void RunOptions(Options opts)
        {
            if (opts.GenerateModels)
            {
                Console.WriteLine("Generating Models");
                GenerateModels.RecreateBakedModelsDir();
                var modelIndex = GenerateModels.BakeMarkovModels();
                GenerateModels.BakeEnsembleMarkovModels(modelIndex);
            }
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
            throw new ArgumentException("Could not parse arguments");
        }
    }
}
