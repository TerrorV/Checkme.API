using Checkme.BL.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkme.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        IListService _listService;
        public ItemsController(IListService listService)
        {
            _listService = listService;
        }

        [Route("/api/v1/lists/{listId}/items/{item}/state")]
        [HttpPut]
        public async Task<IActionResult> UpdateItem([FromRoute] Guid listId, [FromRoute] string item, [FromBody] ItemState state)
        {
            try
            {
                await _listService.UpdateItem(listId, item, state);
                return Accepted($"/api/v1/lists/{listId}/{System.Net.WebUtility.UrlEncode(item)}", $"\"{item}\"");
            }
            catch (Exception ex)
            {
                return NotFound(listId);
            }
        }

        [Route("/api/v1/lists/{listId}/items/{item}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveItem([FromRoute] Guid listId, [FromRoute] string item)
        {
            try
            {
                await _listService.RemoveItemFromList(listId, item);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return NotFound(listId);
            }
        }

        [Route("/api/v1/lists/{listId}/items/{oldItem}")]
        [HttpPut]
        public async Task<IActionResult> EditItem([FromRoute] Guid listId, [FromRoute] string oldItem, [FromBody] string newItem)
        {
            try
            {
                await _listService.EditItem(listId, oldItem, newItem);
                return Accepted($"/api/v1/lists/{listId}/{System.Net.WebUtility.UrlEncode(newItem)}");
            }
            catch (Exception ex)
            {
                return NotFound(listId);
            }
        }


        [Route("/api/v1/lists/{listId}/items")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromRoute] Guid listId, [FromBody] string item)
        {
            try
            {
                await _listService.AddItemToList(listId, item);
                return Created($"/api/v1/lists/{listId}/{System.Net.WebUtility.UrlEncode(item)}", item);
            }
            catch (ArgumentException ex)
            {
                return Conflict();
            }
            catch (Exception ex)
            {
                return NotFound(listId);
            }
        }
    }
}
