using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicApi.Serverless.Extensions;
using MusicEventDbApi;
using PlayFabService;
using SharedDomain.InfraEvents;
using SharedDomain.Products;
using System;
using System.Threading.Tasks;

namespace MusicApi.Serverless.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly EconomyService economyService;
        private readonly MusicEventDbClient musicEventDb;

        public InventoryController(EconomyService economyService, MusicEventDbClient musicEventDb)
        {
            this.economyService = economyService;
            this.musicEventDb = musicEventDb;
        }

        [HttpPost]
        [ProducesDefaultResponseType(typeof(InventoryResponse))]
        [ProducesErrorResponseType(typeof(InventoryResponse))]
        public async Task<IActionResult> ActivateItem([FromBody] InventoryRequest request)
        {
            var response = new InventoryResponse();
            var startEvent = new SharedDomain.InfraEvents.MusicEvent(EventType.StartedActivateItem, request);

            await this.musicEventDb.AddEventAsync(startEvent.ConvertTo<MusicEventDbApi.MusicEvent>());
            try
            {
                var outcome = await this.economyService.ActivateItemAsync(request.PlayFabId, request.ItemInstanceId);
                response.Success = true;
                response.Outcome = outcome ? ProcessingOutcome.Processed : ProcessingOutcome.NotProcessed;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }

            var finishedEvent = new SharedDomain.InfraEvents.MusicEvent(EventType.FinishedActivateItem, (request, response));
            await this.musicEventDb.AddEventAsync(finishedEvent.ConvertTo<MusicEventDbApi.MusicEvent>());

            return response.Success ? this.Ok(response) : this.StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        [HttpPost]
        [ProducesDefaultResponseType(typeof(InventoryResponse))]
        [ProducesErrorResponseType(typeof(InventoryResponse))]
        public async Task<IActionResult> ConsumeItem([FromBody] InventoryRequest request)
        {
            var response = new InventoryResponse();
            var startEvent = new SharedDomain.InfraEvents.MusicEvent(EventType.StartConsumeItem, request);

            await this.musicEventDb.AddEventAsync(startEvent.ConvertTo<MusicEventDbApi.MusicEvent>());
            try
            {
                var outcome = await this.economyService.ConsumeItemAsync(request.PlayFabId, request.ItemInstanceId);
                response.Success = true;
                response.Outcome = outcome ? ProcessingOutcome.Processed : ProcessingOutcome.NotProcessed;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }

            var finishedEvent = new SharedDomain.InfraEvents.MusicEvent(EventType.FinishedConsumeItem, (request, response));
            await this.musicEventDb.AddEventAsync(finishedEvent.ConvertTo<MusicEventDbApi.MusicEvent>());

            return response.Success ? this.Ok(response) : this.StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        [HttpPost]
        [ProducesDefaultResponseType(typeof(InventoryResponse))]
        [ProducesErrorResponseType(typeof(InventoryResponse))]
        public async Task<IActionResult> EnsureFreeItemsPolicy([FromBody] InventoryRequest request)
        {
            var response = new InventoryResponse();
            var startEvent = new SharedDomain.InfraEvents.MusicEvent(EventType.StartEnsureFreeItemsPolicy, request);

            await this.musicEventDb.AddEventAsync(startEvent.ConvertTo<MusicEventDbApi.MusicEvent>());
            try
            {
                var outcome = await this.economyService.EnsureFreeItemsGrantedAsync(request.PlayFabId);
                response.Success = true;
                response.Outcome = outcome ? ProcessingOutcome.Processed : ProcessingOutcome.NotProcessed;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }

            var finishedEvent = new SharedDomain.InfraEvents.MusicEvent(EventType.FinishEnsureFreeItemsPolicy, (request, response));
            await this.musicEventDb.AddEventAsync(finishedEvent.ConvertTo<MusicEventDbApi.MusicEvent>());

            return response.Success ? this.Ok(response) : this.StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }
}