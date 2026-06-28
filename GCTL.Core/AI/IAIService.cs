namespace GCTL.Core.AI
{
    public interface IAIService
    {
        // Simple question → answer
        Task<string> AskAsync(string question, string context = "");

        // Summarize long data into short insight
        Task<string> SummarizeAsync(string data);

        // Analyze data with a specific instruction
        Task<string> AnalyzeAsync(string data, string instruction);

        // Stream responses word by word (like ChatGPT typing effect)
        IAsyncEnumerable<string> StreamAsync(string question, string context = "");
    }
}