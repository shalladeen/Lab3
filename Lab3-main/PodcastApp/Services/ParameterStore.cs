using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace PodcastApp.Services;

public class ParameterStore
{
    private readonly IAmazonSimpleSystemsManagement _ssm;

    public ParameterStore(IAmazonSimpleSystemsManagement ssm)
    {
        _ssm = ssm;
    }

    /// <summary>
    /// Fetches the full SQL Server connection string from AWS Parameter Store.
    /// </summary>
    public async Task<string> GetSqlConnectionAsync()
    {
        var response = await _ssm.GetParameterAsync(new GetParameterRequest
        {
            Name = "/comp306/podcast/sql-conn",
            WithDecryption = true
        });

        return response.Parameter.Value;
    }
}
