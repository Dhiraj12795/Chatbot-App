using Microsoft.AspNetCore.Mvc;

[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet("/")]
    public IActionResult GetRoot()
    {
        return Ok("Welcome to the chatbot application!");
    }
}
