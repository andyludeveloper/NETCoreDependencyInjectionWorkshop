using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace DependencyInjectionWorkshop.Models;

public interface IProfile
{
    string GetPasswordFromDb(string accountId);
}

public class ProfileDao : IProfile
{
    public string GetPasswordFromDb(string accountId)
    {
        string passwordFromDb;
        using (var connection = new SqlConnection("my connection string"))
        {
            passwordFromDb = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                commandType: CommandType.StoredProcedure).SingleOrDefault();
        }

        return passwordFromDb;
    }
}