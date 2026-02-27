namespace SharedDomain
{
    public class Vote<T> where T : IVotable
    {
        public Player By { get; set; }

        public string Code { get; set; }

        public T Item { get; set; }
    }
}