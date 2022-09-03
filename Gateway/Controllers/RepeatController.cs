using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("[controller]")]
public class RepeatController : ControllerBase
{
    private readonly IHttpClientFactory httpClientFactory;

    public RepeatController(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var httpClient = httpClientFactory.CreateClient("SubService");
        var result = await httpClient.GetAsync("");
        var body = await result.Content.ReadAsStringAsync();
        if (result.IsSuccessStatusCode)
        {
            return Ok(body);
        }

        return BadRequest(body);
    }
}

