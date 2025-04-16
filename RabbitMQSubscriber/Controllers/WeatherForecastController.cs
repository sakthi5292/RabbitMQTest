using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQSubscriber.Data;

namespace RabbitMQSubscriber.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    private readonly AppDbContext _context;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, AppDbContext appContext)
    {
        _logger = logger;
        _context = appContext;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpPost("search")]
    public IActionResult SearchDataRecords(
    [FromQuery] int pageno = 1,
    [FromQuery] int pagesize = 10,
    [FromQuery] string filter = null)
    {
        if (pageno <= 0 || pagesize <= 0)
            return BadRequest("Page number and size must be greater than 0.");

        var query = _context.Records.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
            query = query.Where(x => x.Content!.Contains(filter));

        var total = query.Count();

        var result = query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageno - 1) * pagesize)
            .Take(pagesize)
            .ToList();

        return Ok(new
        {
            Page = pageno,
            PageSize = pagesize,
            TotalItems = total,
            Items = result
        });
    }

}
