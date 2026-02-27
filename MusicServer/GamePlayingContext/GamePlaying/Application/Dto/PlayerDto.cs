namespace GamePlaying.Application.Dto
{
    public class PlayerDto
    {
        public string Id { get; set; }

        public string Nick { get; set; }

        public string Code { get; set; }

        public string CharacterCode { get; set; }
        public string ColorCode { get; set; }
        public bool IsConnected { get; set; }
    }
}