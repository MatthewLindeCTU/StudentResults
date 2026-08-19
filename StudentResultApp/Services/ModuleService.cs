using Microsoft.Data.SqlClient;
using StudentResultApp.Models;

namespace StudentResultApp.Services
{
    public class ModuleService
    {
        private readonly string _connectionString;

        public ModuleService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public List<Module> GetAll()
        {
            var modules = new List<Module>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                @"SELECT Id, Code, Name, AcademicYear, StudentCount, Status
                  FROM dbo.Modules
                  ORDER BY Code",
                connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                modules.Add(MapModule(reader));
            }

            return modules;
        }

        public Module? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                @"SELECT Id, Code, Name, AcademicYear, StudentCount, Status
                  FROM dbo.Modules
                  WHERE Id = @Id",
                connection);

            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            using var reader = command.ExecuteReader();

            return reader.Read() ? MapModule(reader) : null;
        }

        public void Add(Module module)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                @"INSERT INTO dbo.Modules (Code, Name, AcademicYear, StudentCount, Status)
                  OUTPUT INSERTED.Id
                  VALUES (@Code, @Name, @AcademicYear, @StudentCount, @Status)",
                connection);

            AddModuleParameters(command, module);

            connection.Open();
            module.Id = (int)command.ExecuteScalar();
        }

        public void Update(Module updatedModule)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                @"UPDATE dbo.Modules
                  SET Code = @Code,
                      Name = @Name,
                      AcademicYear = @AcademicYear,
                      StudentCount = @StudentCount,
                      Status = @Status
                  WHERE Id = @Id",
                connection);

            AddModuleParameters(command, updatedModule);
            command.Parameters.AddWithValue("@Id", updatedModule.Id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "DELETE FROM dbo.Modules WHERE Id = @Id",
                connection);

            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        private static void AddModuleParameters(SqlCommand command, Module module)
        {
            command.Parameters.AddWithValue("@Code", module.Code);
            command.Parameters.AddWithValue("@Name", module.Name);
            command.Parameters.AddWithValue("@AcademicYear", module.AcademicYear);
            command.Parameters.AddWithValue("@StudentCount", module.StudentCount);
            command.Parameters.AddWithValue("@Status", module.Status);
        }

        private static Module MapModule(SqlDataReader reader)
        {
            return new Module
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                AcademicYear = reader.GetInt32(3),
                StudentCount = reader.GetInt32(4),
                Status = reader.GetString(5)
            };
        }
    }
}