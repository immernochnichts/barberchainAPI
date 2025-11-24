using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using barberchainAPI.Data;
using System.ComponentModel.DataAnnotations;

namespace barberchainAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly BarberchainDbContext _db;

        public AuthController(BarberchainDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            Console.WriteLine("=== Auth controller login endpoint entered ===");

            var acc = _db.Accounts.FirstOrDefault(a => a.Email == email);
            if (acc == null)
            {
                return BadRequest("Invalid login or email");
            }

            Console.WriteLine($"Logging into {acc.Email}");

            var hashString = System.Text.Encoding.UTF8.GetString(acc.Hash);
            if (!BCrypt.Net.BCrypt.Verify(password, hashString))
            {
                return BadRequest("Invalid login or email");
            }

            Console.WriteLine("Email and password are correct");
            Console.WriteLine("Creating claims");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, acc.Id.ToString()),
                new Claim(ClaimTypes.Name, acc.Email ?? ""),
                new Claim(ClaimTypes.Role, acc.Role.ToString())
            };

            Console.WriteLine("=== CLAIMS ===");
            foreach (var c in claims)
                Console.WriteLine($"{c.Type} = {c.Value}");

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            Console.WriteLine($"Created identity {identity.Name}");

            var principal = new ClaimsPrincipal(identity);

            Console.WriteLine("Created claims principal");

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                }
            );

            Console.WriteLine("Signed in");

            return Ok();
        }

        public class RegisterRequest
        {
            [Required] public string Lastname { get; set; }
            public string? Restname { get; set; }
            [Required, EmailAddress] public string Email { get; set; }
            [Required] public string Password { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_db.Accounts.Any(a => a.Email == request.Email))
                return BadRequest(new { error = "Email уже зарегистрирован" });

            var hashString = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var hashBytes = System.Text.Encoding.UTF8.GetBytes(hashString);

            var account = new Account
            {
                Lastname = request.Lastname,
                Restname = string.IsNullOrWhiteSpace(request.Restname) ? null : request.Restname,
                Email = request.Email,
                Hash = hashBytes,
                RegTime = DateTime.UtcNow,
                Role = AccountRole.User
            };

            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new { message = "Logged out" });
        }
    }
}
