namespace SharedDomain.Messages.Commands
{
    public class AskMessage : BaseMessage
    {
        public string SongId { get; set; }
        public string CorrectAnswer { get; set; }
    }
}