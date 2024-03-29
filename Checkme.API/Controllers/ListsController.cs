﻿using Checkme.BL.Abstract;
using Checkme.BL.Abstract.Exceptions;
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
        [Route("{listId}/events")]
        [HttpGet]
        public async Task GetListEvents([FromRoute] Guid listId)
        {
                Response.Headers["Content-Type"] = "text/event-stream";
                Response.Headers["Cache-Control"] = "no-cache";
                await _listService.SubscribeClient(listId, Response.Body);
                ////var response = Response;
                //response.Body 
                return;
        }

        [ProducesResponseType(typeof(CheckList), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("{listId}")]
        [HttpGet]
        public async Task<IActionResult> GetList([FromRoute] Guid listId, [FromQuery] DateTime timestamp)
        {
            try
            {
                if (timestamp != default(DateTime))
                {
                    return Ok(_listService.GetListById(listId, timestamp).Result);
                }
                else
                {
                    return Ok(_listService.GetListById(listId).Result);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
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
                return Ok(_listService.GetLists().Result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [ProducesResponseType(typeof(IEnumerable<CheckList>), StatusCodes.Status202Accepted)]
        [Route("{listId}")]
        [HttpPut]
        public async Task<IActionResult> UpdateList([FromRoute]Guid listId, [FromBody] CheckList list)
        {
            try
            {
                await _listService.UpdateList(listId, list);
                return Accepted($"api/v1/lists/{listId.ToString()}", await _listService.GetListById(listId));
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 500);
            }
        }

        [ProducesResponseType(typeof(CheckList), StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost]
        public async Task<IActionResult> CreateList([FromBody] CheckList list)
        {
            try
            {
                Guid listId = list.Id == Guid.Empty ? Guid.NewGuid() : list.Id;

                list.Id = listId;
                await _listService.AddList(list);
                return Created($"api/v1/lists/{listId.ToString()}", await _listService.GetListById(listId));
            }
            catch (ItemExistsException ex2)
            {
                return Problem(ex2.Message, statusCode: 409);

            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: 500);
            }
        }
    }
}
