using Microsoft.AspNetCore.Mvc;
using SecureApiTemplate.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace SecureApiTemplate.Controllers
{
    [Route("/API/[controller]")]
    [ApiController]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, type: typeof(ErrorInfo))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, type: typeof(ErrorInfo))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, type: typeof(ErrorInfo))]
    public class BaseApiController : ControllerBase
    {
    }
}
