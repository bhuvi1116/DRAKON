namespace DrakonNx.Core.Layout;

public sealed class DrakonLayoutReport
{
    public int RepositionedNodes { get; set; }

    public bool AppliedSilhouette { get; set; }

    public List<DrakonLayoutIssue> Issues { get; } = new();

    public string Summary => $"Layout: repositioned={RepositionedNodes}, silhouette={(AppliedSilhouette ? "on" : "off")}, issues={Issues.Count}";
}
