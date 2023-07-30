using Checkme.BL.Abstract;
using Checkme.BL.Abstract.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Checkme.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        IResourceService _resService;
        public ResourcesController(IResourceService resService)
        {
            _resService = resService;
        }

        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost]
        public async Task<IActionResult> CreateResource(IFormFile file)
        {
            try
            {
                var fileContent = new MemoryStream();
                file.CopyTo(fileContent);
                fileContent.Position = 0;
                var id = await _resService.AddResource(fileContent, file.ContentType);
                return Created($"api/v1/resources/{id}", 0);
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
