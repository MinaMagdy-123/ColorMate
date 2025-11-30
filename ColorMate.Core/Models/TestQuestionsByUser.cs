namespace ColorMate.Core.Models
{
    public class TestQuestionsByUser
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public int TestQuestionId { get; set; }
        public TestQuestions TestQuestions { get; set; }
        public required string SelectedAnswer { get; set; }
    }
}
