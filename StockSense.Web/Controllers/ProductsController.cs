using System.Text;
using Microsoft.AspNetCore.Mvc;
using StockSense.Domain.Entities;
using StockSense.Infrastructure.Data;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;

namespace StockSense.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IBuildService _buildService;
    private readonly IEmailSender<ApplicationUser> _emailSender;

    public ProductsController(
        IProductService productService,
        IBuildService buildService,
        IEmailSender<ApplicationUser> emailSender)
    {
        _productService = productService;
        _buildService = buildService;
        _emailSender = emailSender;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpPost("send-quote")]
    public async Task<IActionResult> SendQuote([FromBody] EmailQuoteRequest request)
    {
        var selectedProducts = await _productService.GetByIdsAsync(request.ProductIds);
        if (!selectedProducts.Any()) return BadRequest(ApiResponse.Error("No valid products found."));

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
            return StatusCode(500, ApiResponse.Error(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        if (id != dto.Id) return BadRequest(ApiResponse.Error("ID mismatch."));

        var updated = await _productService.UpdateProductAsync(dto);
        if (!updated) return NotFound(ApiResponse.NotFound("Product"));

        return NoContent();
    }

    public class EmailQuoteRequest
    {
        public string UserEmail { get; set; } = "";
        public List<int> ProductIds { get; set; } = new();
    }
}
