using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IDocumentService
{
    byte[] GenerateOrderSlipPdf(OrderSlipDto slip);
}