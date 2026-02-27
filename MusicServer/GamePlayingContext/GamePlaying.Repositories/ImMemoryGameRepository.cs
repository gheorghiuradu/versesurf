using GamePlaying.Domain.GameAggregate;
using System.Collections.Concurrent;

namespace GamePlaying.Repositories
{
    public class InMemoryGameRepository : IGameRepository
    {
        private readonly ConcurrentDictionary<string, Game> games = new ConcurrentDictionary<string, Game>();

        public void AddGame(Game game)
        {
            this.games.TryAdd(game.Id, game);
        }

        public Game GetGame(string id)
        {
            this.games.TryGetValue(id, out var game);
            return game;
        }

        public void UpdateGame(Game game)
        {
            //Does nothing because it is in memory
        }

        public void RemoveGame(string gameId)
        {
            this.games.TryRemove(gameId, out var _);
        }
    }
}