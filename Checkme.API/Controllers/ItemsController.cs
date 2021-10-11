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
        public ActionResult UpdateItem([FromRoute] Guid listId, [FromRoute] string item, [FromBody] ItemState state)
        {
            try
            {
                _listService.UpdateItem(listId, item, state);
                return Accepted($"/api/v1/lists/{listId}/{System.Net.WebUtility.UrlEncode(item)}", $"\"{item}\"");
            }
            catch (Exception ex)
            {
                return NotFound(listId);
            }
        }

        [Route("/api/v1/lists/{listId}/items/{item}")]
        [HttpDelete]
        public ActionResult RemoveItem([FromRoute] Guid listId, [FromRoute] string item)
        {
            try
            {
                _listService.RemoveItemFromList(listId, item);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return NotFound(listId);
            }
        }

        [Route("/api/v1/lists/{listId}/items/{oldItem}")]
        [HttpPut]
        public ActionResult EditItem([FromRoute] Guid listId, [FromRoute] string oldItem, [FromBody] string newItem)
        {
            try
            {
                _listService.EditItem(listId, oldItem, newItem);
                return Accepted($"/api/v1/lists/{listId}/{System.Net.WebUtility.UrlEncode(newItem)}");
            }
            catch (Exception ex)
            {
                return NotFound(listId);
            }
        }


        [Route("/api/v1/lists/{listId}/items")]
        [HttpPost]
        public ActionResult AddItem([FromRoute] Guid listId, [FromBody] string item)
        {
            try
            {
                _listService.AddItemToList(listId, item);
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
