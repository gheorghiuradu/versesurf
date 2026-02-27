namespace GamePlaying.Domain.GameAggregate
{
    public interface IGameRepository
    {
        void AddGame(Game game);

        Game GetGame(string gameId);

        void UpdateGame(Game game);

        void RemoveGame(string gameId);
    }
}