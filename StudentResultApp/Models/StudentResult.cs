namespace StudentResultApp.Models
{
    public class StudentResult
    {
        public int ResultID { get; set; }
        public int StudentID { get; set; }
        public string? Module { get; set; }
        public int Mark { get; set; }

        // populated via join, for display only — not stored on this table
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}