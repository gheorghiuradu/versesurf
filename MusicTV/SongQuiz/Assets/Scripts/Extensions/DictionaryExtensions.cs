using Assets.Scripts.Serialization;
using Newtonsoft.Json;
using PlayFab.ClientModels;
using System.Collections.Generic;

namespace Assets.Scripts.Extensions
{
    public static class DictionaryExtensions
    {
        public static GameOptions ToGameOptions(this Dictionary<string, UserDataRecord> dictionary)
        {
            if (dictionary.Count == 0)
            {
                return GameOptions.Default;
            }

            var simpleDictionary = new Dictionary<string, string>();
            foreach (var item in dictionary)
            {
                simpleDictionary.Add(item.Key, item.Value.Value);
            }

            return JsonConvert.DeserializeObject<GameOptions>(JsonConvert.SerializeObject(simpleDictionary));
        }
    }
}