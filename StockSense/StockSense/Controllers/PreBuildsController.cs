using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data; // Adjust this using statement to your actual DbContext namespace
using StockSense.Shared;
using StockSense.shared;

namespace StockSense.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreBuildsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PreBuildsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- FOR THE CUSTOMER WIZARD ---
        [HttpGet]
        public async Task<ActionResult<List<PreBuildPackage>>> GetMatchingPackages(
            [FromQuery] string brand,
            [FromQuery] string model,
            [FromQuery] string cc,
            [FromQuery] decimal minBudget,
            [FromQuery] decimal maxBudget)
        {
            // 1. Get packages that match the specific motor specs
            // CRITICAL: We use .Include() to eagerly load the actual products attached to the package
            var matchingPackages = await _context.PreBuildPackages
                .Include(p => p.IncludedProducts)
                .Where(p => p.CompatibleBrand == brand &&
                            p.CompatibleModel == model &&
                            p.TargetCC == cc)
                .ToListAsync();

            // 2. Filter out the ones that are outside the user's budget range
            var affordablePackages = matchingPackages
                .Where(p => p.TotalPrice >= minBudget && p.TotalPrice <= maxBudget)
                .ToList();

            return Ok(affordablePackages);
        }

        // --- FOR THE ADMIN DASHBOARD (View all packages) ---
        [HttpGet("all")]
        public async Task<ActionResult<List<PreBuildPackage>>> GetAllPackages()
        {
            var packages = await _context.PreBuildPackages
                .Include(p => p.IncludedProducts)
                .ToListAsync();

            return Ok(packages);
        }

        // --- FOR THE ADMIN DASHBOARD (Create a new package) ---
        [HttpPost]
        public async Task<IActionResult> CreatePreBuild([FromBody] CreatePreBuildDto dto)
        {
            if (dto.ProductIds == null || !dto.ProductIds.Any())
            {
                return BadRequest("A package must contain at least one product.");
            }

            // 1. Fetch all the products matching the IDs sent by the Admin
            var productsToInclude = await _context.Products
                .Where(p => dto.ProductIds.Contains(p.Id))
                .ToListAsync();

            if (!productsToInclude.Any())
            {
                return BadRequest("None of the selected products were found in the database.");
            }

            // 2. Map the DTO data to your actual database model
            var newPackage = new PreBuildPackage
            {
                Name = dto.Name,
                Description = dto.Description,
                CompatibleBrand = dto.CompatibleBrand,
                CompatibleModel = dto.CompatibleModel,
                TargetCC = dto.TargetCC,
                IncludedProducts = productsToInclude // Attach the fetched products!
            };

            // 3. Save to the database
            _context.PreBuildPackages.Add(newPackage);
            await _context.SaveChangesAsync();

            return Ok(newPackage);
        }
    }
}