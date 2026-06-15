using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=db47380.public.databaseasp.net; Database=db47380; User Id=db47380; Password=xC?7G_4za3K!; Encrypt=False; MultipleActiveResultSets=True;";
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        
        using var cmd = new SqlCommand("SELECT TOP 10 Id, AssessmentId, Score, Answers FROM CandidateAssessments ORDER BY Id DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine($"CA_Id: {reader["Id"]}, AssId: {reader["AssessmentId"]}, Score: {reader["Score"]}, Answers: {reader["Answers"]}");
        }
    }
}
