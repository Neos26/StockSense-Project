using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockSense.Domain.Entities;
using StockSense.Infrastructure.Data;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Domain.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepo;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly IBuildRequestRepository _buildRepo;

    public ProductsController(
        IProductRepository productRepo,
        IEmailSender<ApplicationUser> emailSender,
        IBuildRequestRepository buildRepo)
    {
        _productRepo = productRepo;
        _emailSender = emailSender;
        _buildRepo = buildRepo;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        var products = await _productRepo.GetAllProductsAsync();
        return Ok(products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Category,
            p.Brand,
            p.Price,
            p.CurrentStock,
            p.ReorderTarget,
            p.SupplierId,
            p.Supplier?.Name ?? ""
        )).ToList());
    }

    [HttpPost("submit-build")]
    public async Task<IActionResult> SubmitBuild([FromBody] BuildRequest request)
    {
        _buildRepo.Add(request);
        await _buildRepo.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("send-quote")]
    public async Task<IActionResult> SendQuote([FromBody] EmailQuoteRequest request)
    {
        var selectedProducts = await _productRepo.GetByIdsAsync(request.ProductIds);
        if (!selectedProducts.Any()) return BadRequest("No valid products found.");

        decimal grandTotal = selectedProducts.Sum(p => p.Price);

        var sb = new StringBuilder();
        sb.AppendLine("<h1>StockSense Build Quotation</h1>");
        sb.AppendLine($"<p>Hello {request.UserEmail}, here is the quote for your custom build:</p>");
        sb.AppendLine("<table border='1' cellpadding='10' cellspacing='0' style='border-collapse:collapse; width:100%; text-align:left;'>");
        sb.AppendLine("<tr style='background-color:#f2f2f2;'><th>Part Name</th><th>Category</th><th>Price</th></tr>");

        foreach (var p in selectedProducts)
        {
            sb.AppendLine($"<tr><td>{p.Name}</td><td>{p.Category}</td><td>P {p.Price:N2}</td></tr>");
        }
        sb.AppendLine("</table>");
        sb.AppendLine($"<h3>Grand Total: P {grandTotal:N2}</h3>");

        try
        {
            await _emailSender.SendEmailAsync(request.UserEmail, "Custom Build Quote", sb.ToString());
            return Ok(new { message = "Email sent" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct)
    {
        if (id != updatedProduct.Id) return BadRequest();

        var dbProduct = await _productRepo.GetByIdAsync(id);
        if (dbProduct == null) return NotFound();

        dbProduct.Price = updatedProduct.Price;
        dbProduct.ReorderTarget = updatedProduct.ReorderTarget;

        _productRepo.Update(dbProduct);
        try
        {
            await _productRepo.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    public class EmailQuoteRequest
    {
        public string UserEmail { get; set; } = "";
        public List<int> ProductIds { get; set; } = new();
    }
}
