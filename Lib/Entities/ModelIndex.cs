namespace Lib.Entities
{
    public class ModelFile
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string ApiPath { get; set; } = "";
        public int ByteSize { get; set; } = 0;
    }

    public class ModelIndex
    {
        public List<string> sourceFiles { get; set; } = new List<string>();
        public Dictionary<string, List<ModelFile>> modelFilesForCategory { get; set; } = new Dictionary<string, List<ModelFile>>();
    }
}
