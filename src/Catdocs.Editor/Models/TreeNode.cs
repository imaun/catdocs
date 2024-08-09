using System.Collections.ObjectModel;

namespace Catdocs.Editor.Models;

public class TreeNode
{
    public TreeNode(string title, string? altTitle = null)
    {
        Title = title;
        AltTitle = altTitle;
    }

    public TreeNode(string title, ObservableCollection<TreeNode> nodes)
    {
        Title = title;
        Nodes = nodes;
    }

    public TreeNode(string title, string altTitle, ObservableCollection<TreeNode> nodes)
    {
        Title = title;
        AltTitle = altTitle;
        Nodes = nodes;
    }
    
    public string Title { get; }
    
    public string? AltTitle { get; }
    
    public ObservableCollection<TreeNode>? Nodes { get; }
    
}