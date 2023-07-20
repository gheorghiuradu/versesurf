using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharedDomain
{
    public class ScoreBoard
    {
        public ConcurrentDictionary<Player, int> Scores { get; } = new ConcurrentDictionary<Player, int>();

        public ScoreBoard()
        {
        }

        public ScoreBoard(IEnumerable<Player> players)
        {
            foreach (var player in players)
            {
                this.Scores.TryAdd(player, 0);
            }
        }

        public void AddScore(string playerId, int score)
        {
            this.Scores[new Player { Id = playerId }] += score;
        }

        public void AddScore(Player player, int score)
        {
            this.Scores[player] += score;
        }

        public void AddPlayer(Player player)
        {
            this.Scores.TryAdd(player, 0);
        }

        public void AddScores(IEnumerable<KeyValuePair<Player, int>> scores)
        {
            foreach (var score in scores)
            {
                if (!this.Scores.ContainsKey(score.Key))
                {
                    this.Scores.TryAdd(score.Key, score.Value);
                }
                else
                {
                    this.Scores[score.Key] += score.Value;
                }
            }
        }

        public void RemovePlayer(Player player)
        {
            this.Scores.TryRemove(player, out var _);
        }
    }
}