using LibGit2Sharp;

namespace SpecEditor.Lib;

public class GitClient
{

    private string _repoPath;

    public void git_clone(string url, string localPath, string branchName)
    {
        try
        {
            Repository.Clone(url, localPath, new CloneOptions {
                BranchName = branchName,
                Checkout = true
            });
            //TODO: log success
        }
        catch(Exception ex)
        {
            //TODO: log the exception
            throw;
        }
    }


    public void git_commit(string message, string authorName, string authorEmail)
    {
        try
        {
            using var repo = new Repository(_repoPath);
            Commands.Stage(repo, "*");
            var author = new Signature(authorName, authorEmail, DateTime.Now);
            var commiter = author;
            var commit = repo.Commit(message, author, commiter);
            //TODO: log success
        }
        catch(Exception ex)
        {
            //TODO: log the exception
            throw;
        }
    }


}
