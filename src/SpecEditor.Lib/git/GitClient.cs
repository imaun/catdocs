using LibGit2Sharp;

namespace SpecEditor.Lib;

public class GitClient
{


    public void git_clone(string url, string localPath, string branchName)
    {
        try
        {
            Repository.Clone(url, localPath, new CloneOptions {
                BranchName = branchName,
                Checkout = true
            });
        }
        catch(Exception ex)
        {
            //TODO: log the exception
            throw;
        }
    }

}
