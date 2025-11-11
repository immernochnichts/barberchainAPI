using barberchainAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace barberchainAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BarbershopController : ControllerBase
    {
        private readonly BarberchainDbContext db;

        public BarbershopController(BarberchainDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public IActionResult GetAllBarbershops()
        {
            return Ok(db.Barbershops.ToList());
        }
    }
}
