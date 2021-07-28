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
    public class ListsController : ControllerBase
    {
        IListService _listService;
        public ListsController(IListService listService)
        {
            _listService = listService;
        }

        [ProducesResponseType(typeof(CheckList), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("{listId}")]
        [HttpGet]
        public IActionResult GetList([FromRoute] Guid listId)
        {
            try
            {
                return Ok(_listService.GetListById(listId));
            }
            catch (Exception)
            {
                return NotFound(listId);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<CheckList>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        public IActionResult GetAllList()
        {
            try
            {
                return Ok(_listService.GetLists());
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [ProducesResponseType(typeof(CheckList), StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost]
        public IActionResult CreateList([FromBody]CheckList list)
        {
            try
            {
                Guid listId = list.Id == Guid.Empty ? Guid.NewGuid(): list.Id;

                list.Id = listId;
                _listService.AddList(list);
                return Created($"api/v1/lists/{listId.ToString()}", _listService.GetListById(listId));
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 500);
            }
        }
    }
}
