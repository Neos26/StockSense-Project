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
            var matchingPackages = await _context.PreBuildPackages
                .Include(p => p.IncludedProducts)
                .Where(p => p.CompatibleBrand == brand &&
                            p.CompatibleModel == model &&
                            p.TargetCC == cc &&
                            p.IsActive == true) // <--- THIS IS THE MAGIC FIX
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
                EstimatedAddedCC = dto.EstimatedAddedCC,
                IncludedProducts = productsToInclude // Attach the fetched products!
            };

            // 3. Save to the database
            _context.PreBuildPackages.Add(newPackage);
            await _context.SaveChangesAsync();

            return Ok(newPackage);

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePreBuild(int id, [FromBody] CreatePreBuildDto dto)
        {
            var pkg = await _context.PreBuildPackages.Include(p => p.IncludedProducts).FirstOrDefaultAsync(p => p.Id == id);
            if (pkg == null) return NotFound();

            pkg.Name = dto.Name;
            pkg.Description = dto.Description;
            pkg.CompatibleBrand = dto.CompatibleBrand;
            pkg.CompatibleModel = dto.CompatibleModel;
            pkg.TargetCC = dto.TargetCC;
            pkg.EstimatedAddedCC = dto.EstimatedAddedCC;
            pkg.IncludedProducts = await _context.Products.Where(p => dto.ProductIds.Contains(p.Id)).ToListAsync();

            await _context.SaveChangesAsync();
            return Ok(pkg);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePreBuild(int id)
        {
            var pkg = await _context.PreBuildPackages.FindAsync(id);
            if (pkg == null) return NotFound();

            _context.PreBuildPackages.Remove(pkg);
            await _context.SaveChangesAsync();
            return Ok();
        }
        public class ToggleActiveDto
        {
            public bool IsActive { get; set; }
        }
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id, [FromBody] ToggleActiveDto dto)
        {
            var pkg = await _context.PreBuildPackages.FindAsync(id);
            if (pkg == null) return NotFound();

            pkg.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
