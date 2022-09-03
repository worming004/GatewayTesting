using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("[controller]")]
public class RepeatController : ControllerBase
{
    private readonly HttpClient httpClient;

    public RepeatController(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Get()
    {
        var result = await httpClient.GetAsync("");
        var body = result.Content.ReadAsStringAsync();
        if (result.IsSuccessStatusCode)
        {
            return Ok(body);
        }

        return BadRequest(body);
    }
}

