using Microsoft.Data.SqlClient;
using StudentResultApp.Models;

namespace StudentResultApp.Services
{
    public class StudentService
    {
        private readonly string _connectionString;

        public StudentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public List<Student> GetAll()
        {
            var students = new List<Student>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT StudentID, FirstName, LastName, Email, Phone FROM dbo.Students ORDER BY LastName, FirstName",
                connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                students.Add(MapStudent(reader));
            }

            return students;
        }

        public Student? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "SELECT StudentID, FirstName, LastName, Email, Phone FROM dbo.Students WHERE StudentID = @Id",
                connection);

            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            using var reader = command.ExecuteReader();

            return reader.Read() ? MapStudent(reader) : null;
        }

        public int Add(Student student)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                @"INSERT INTO dbo.Students (FirstName, LastName, Email, Phone)
                  OUTPUT INSERTED.StudentID
                  VALUES (@FirstName, @LastName, @Email, @Phone)",
                connection);

            AddStudentParameters(command, student);

            connection.Open();
            var newId = (int)command.ExecuteScalar();
            student.StudentID = newId;
            return newId;
        }

        public void Update(Student student)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                @"UPDATE dbo.Students
                  SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Phone = @Phone
                  WHERE StudentID = @StudentID",
                connection);

            AddStudentParameters(command, student);
            command.Parameters.AddWithValue("@StudentID", student.StudentID);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(
                "DELETE FROM dbo.Students WHERE StudentID = @Id",
                connection);

            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        private static void AddStudentParameters(SqlCommand command, Student student)
        {
            command.Parameters.AddWithValue("@FirstName", (object?)student.FirstName ?? DBNull.Value);
            command.Parameters.AddWithValue("@LastName", (object?)student.LastName ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object?)student.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@Phone", (object?)student.Phone ?? DBNull.Value);
        }

        private static Student MapStudent(SqlDataReader reader)
        {
            return new Student
            {
                StudentID = reader.GetInt32(0),
                FirstName = reader.IsDBNull(1) ? null : reader.GetString(1),
                LastName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                Phone = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }
    }
}