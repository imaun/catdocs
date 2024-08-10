using System.Collections.ObjectModel;
using System.Linq;
using Catdocs.Editor.Models;
using Microsoft.OpenApi.Models;

namespace Catdocs.Editor.OpenApi;

public class OpenApiDocNodesLoader
{
    private ObservableCollection<TreeNode> _nodes;
    
    public OpenApiDocNodesLoader(OpenApiDocument document)
    {
        Document = document;
        Load();
    }
    
    public OpenApiDocument Document { get; }

    public ObservableCollection<TreeNode> Nodes
    {
        get
        {
            return _nodes;
        }
    }

    private void Load()
    {
        _nodes = new ObservableCollection<TreeNode>();

        if (!Document.Paths.Any())
        {
            return;
        }
        
        foreach (var path in Document.Paths)
        {
            // if(path.Value.Reference.IsExternal
            _nodes.Add(new TreeNode(path.Key));
        }
    }
}