using Microsoft.AspNetCore.Mvc;
using ZenTam.Api.Features.Calendars.Services;

namespace ZenTam.Api.Features.Calendars;

[ApiController]
[Route("api/hoang-dao")]
public class HoangDaoController : ControllerBase
{
    private readonly IHoangDaoService _hoangDaoService;

    public HoangDaoController(IHoangDaoService hoangDaoService)
    {
        _hoangDaoService = hoangDaoService;
    }

    [HttpGet("{date}")]
    public IActionResult GetByDate(DateTime date)
    {
        var response = _hoangDaoService.GetHoangDaoResponse(date);
        return Ok(response);
    }

    [HttpGet]
    public IActionResult GetByQuery([FromQuery] DateTime solarDate)
    {
        var response = _hoangDaoService.GetHoangDaoResponse(solarDate);
        return Ok(response);
    }
}
