using Microsoft.Extensions.Configuration;
using PlayFab.AdminModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayFab.Console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var credentials = new PlayFabCredentials();
            new ConfigurationBuilder().AddUserSecrets<Program>(false).Build().Bind(credentials);
            var playFabAdmin = new PlayFabAdminInstanceAPI(credentials.ToPlayFabApiSettings());

            var playersRequest = await playFabAdmin.GetPlayersInSegmentAsync(new GetPlayersInSegmentRequest { SegmentId = "3BB559F745719C4C" });
            var usersThatHaveGamePass = new List<PlayerProfile>();
            var gamePasses = 0;
            foreach (var player in playersRequest.Result.PlayerProfiles)
            {
                var inventory = await playFabAdmin.GetUserInventoryAsync(new GetUserInventoryRequest { PlayFabId = player.PlayerId });
                var passes = inventory.Result?.Inventory.Where(ii => ii.ItemId.Equals("0e54d068-0bb5-4f6d-91f4-b7864690dab2"));
                if (passes?.Any() == true)
                {
                    //gamePasses += passes.Count();
                    //usersThatHaveGamePass.Add(player);
                    var revokeResult = await playFabAdmin.RevokeInventoryItemsAsync(new RevokeInventoryItemsRequest
                    {
                        Items = passes.Select(p => new RevokeInventoryItem
                        {
                            PlayFabId = player.PlayerId,
                            ItemInstanceId = p.ItemInstanceId
                        }).ToList()
                    });
                    if (revokeResult.Error != null)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    //foreach (var gamepass in passes)
                    //{
                    //    var grantResult = await playFabAdmin.GrantItemsToUsersAsync(new GrantItemsToUsersRequest
                    //    {
                    //        ItemGrants = new List<ItemGrant>
                    //        {
                    //            new ItemGrant
                    //            {
                    //                ItemId = "778ea256-2af0-4424-921a-743b7e7ff82a",
                    //                PlayFabId = player.PlayerId
                    //            }
                    //        }
                    //    });
                    //    if (grantResult.Error != null)
                    //    {
                    //        System.Diagnostics.Debugger.Break();
                    //    }

                    //    var revokeResult = await playFabAdmin.RevokeInventoryItemAsync(new RevokeInventoryItemRequest
                    //    {
                    //        ItemInstanceId = gamepass.ItemInstanceId,
                    //        PlayFabId = player.PlayerId
                    //    });
                    //    if (revokeResult.Error != null)
                    //    {
                    //        System.Diagnostics.Debugger.Break();
                    //    }

                    //    System.Console.WriteLine($"Removed game pass to {player.PlayerId}");
                    //}
                }
            }
            //System.Console.WriteLine($"{usersThatHaveGamePass.Count} players have game passes");
            //System.Console.WriteLine($"{gamePasses} game passes remaining");
        }
    }
}