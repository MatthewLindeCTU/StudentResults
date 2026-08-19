using Microsoft.Data.SqlClient;
using StudentResultApp.Models;

namespace StudentResultApp.Services
{
    public class StudentResultService
    {
        private readonly string _connectionString;

        public StudentResultService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public List<StudentResult> GetAll()
        {
            var results = new List<StudentResult>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                @"SELECT r.ResultID, r.StudentID, r.Module, r.Mark, s.FirstName, s.LastName
                  FROM dbo.StudentResults r
                  JOIN dbo.Students s ON s.StudentID = r.StudentID
                  ORDER BY r.ResultID DESC",
                connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                results.Add(new StudentResult
                {
                    ResultID = reader.GetInt32(0),
                    StudentID = reader.GetInt32(1),
                    Module = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Mark = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    FirstName = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LastName = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return results;
        }

        public void Add(StudentResult result)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                @"INSERT INTO dbo.StudentResults (StudentID, Module, Mark)
                  VALUES (@StudentID, @Module, @Mark)",
                connection);

            command.Parameters.AddWithValue("@StudentID", result.StudentID);
            command.Parameters.AddWithValue("@Module", (object?)result.Module ?? DBNull.Value);
            command.Parameters.AddWithValue("@Mark", result.Mark);

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}