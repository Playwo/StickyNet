using CommandLine;

namespace StickyNet.Arguments
{
    [Verb("delete", HelpText = "Removes a StickyNet from the given port")]
    public class DeleteOptions
    {
        [Option('p', "port")]
        public int Port { get; set; }
    }
}
