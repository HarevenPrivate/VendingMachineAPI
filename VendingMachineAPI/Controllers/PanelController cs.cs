//using Microsoft.AspNetCore.Mvc;
//using Vending.Api.Services;

//namespace Vending.Api.Controllers;

//[ApiController]
//[Route("api/panel")]
//public class PanelController : ControllerBase
//{
//    private readonly VendingMachineService _svc;
//    public PanelController(VendingMachineService svc) => _svc = svc;

//    [HttpGet("snapshot")]
//    public IActionResult Snapshot() => Ok(_svc.Snapshot());
//}
