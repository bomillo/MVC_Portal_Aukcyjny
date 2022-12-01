namespace WebApp.Models.DTO
{
    public class QuestionDTO
    {

        public int QuestionId { get; set; }
        public string UserName { get; set; }
        public string Question { get; set; }
        public DateTime AskedTime { get; set; }
        
        public string? Answer { get; set; }
        public DateTime? AnsweredTime { get; set; }
    }
}
