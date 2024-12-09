namespace OpenKh.Patcher.BarEntryExtractor
{
    public record SourceBuilderArg(
        string DestName, 
        string DestType, 
        string SourceName, 
        string OriginalRelativePath
    );
}
