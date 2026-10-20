using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Services;

public interface IReportePdfService
{
    byte[] GenerarReporteStock(IReadOnlyCollection<Producto> productos);
    byte[] GenerarReporteVentas(IReadOnlyCollection<Venta> ventas);
    byte[] GenerarReporteEntradas(IReadOnlyCollection<Compra> compras);
}

/// <summary>Shared print design for both downloaded and emailed reports.</summary>
public class ReportePdfService : IReportePdfService
{
    private const string Ink = "#202827";
    private const string Muted = "#697574";
    private const string Border = "#DFE7E5";
    private const string Lime = "#D0F568";
    private const string Canvas = "#F0F4F4";
    private const string Lavender = "#E5DDFB";
    private const string Mint = "#D5F1E3";
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

    public byte[] GenerarReporteStock(IReadOnlyCollection<Producto> productos) =>
        CrearReporte("Reporte de Stock", "INVENTARIO", "Productos activos y existencias disponibles",
            $"{productos.Count} productos", "Stock al momento de la emisión",
            ["ID", "Producto", "Categoría", "Precio", "Stock", "Reposición"],
            [0.5f, 3.1f, 2f, 1.7f, 0.8f, 1.3f], 3,
            productos.Select(p => new[] {
                p.Id.ToString(), p.Nombre, p.CategoriaProducto?.Nombre ?? "-",
                p.PrecioVenta.ToString("C2", Cultura), p.StockActual.ToString(), p.PuntoReposicion.ToString()
            }).ToList());

    public byte[] GenerarReporteVentas(IReadOnlyCollection<Venta> ventas) =>
        CrearReporte("Reporte de Ventas", "VENTAS", "Resumen de las operaciones registradas",
            $"{ventas.Count} ventas", $"Importe total  {ventas.Sum(v => v.Total).ToString("C2", Cultura)}",
            ["Nº", "Fecha", "Cliente", "Usuario", "Total"],
            [0.5f, 1.8f, 2.4f, 2.4f, 1.7f], 4,
            ventas.Select(v => new[] {
                v.Id.ToString(), v.Fecha.ToString("dd/MM/yyyy HH:mm"), v.Cliente?.Nombre ?? "-",
                v.Usuario?.Nombre ?? "-", v.Total.ToString("C2", Cultura)
            }).ToList());

    public byte[] GenerarReporteEntradas(IReadOnlyCollection<Compra> compras) =>
        CrearReporte("Entradas de Mercadería", "INVENTARIO", "Resumen de los ingresos de mercadería",
            $"{compras.Count} entradas", $"Importe total  {compras.Sum(c => c.Total).ToString("C2", Cultura)}",
            ["Nº", "Fecha", "Proveedor", "Usuario", "Total"],
            [0.5f, 1.8f, 2.4f, 2.4f, 1.7f], 4,
            compras.Select(c => new[] {
                c.Id.ToString(), c.Fecha.ToString("dd/MM/yyyy HH:mm"), c.Proveedor?.RazonSocial ?? "-",
                c.Usuario?.Nombre ?? "-", c.Total.ToString("C2", Cultura)
            }).ToList());

    private static byte[] CrearReporte(string titulo, string seccion, string descripcion,
        string cantidad, string resumen, string[] encabezados, float[] anchos,
        int primeraColumnaNumerica, IReadOnlyList<string[]> filas)
    {
        var emitido = DateTime.Now;
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(style => style.FontSize(9).FontColor(Ink));
            page.Header().PaddingBottom(20).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("SISTEMA DE VENTAS").FontSize(9).SemiBold();
                    row.AutoItem().Background(Lime).PaddingHorizontal(10).PaddingVertical(5)
                        .Text(seccion).FontSize(8).SemiBold();
                });
                column.Item().PaddingTop(18).Text(titulo).FontSize(24).SemiBold();
                column.Item().PaddingTop(5).Text(descripcion).FontColor(Muted).FontSize(10);
                column.Item().PaddingTop(8).Text($"Generado el {emitido:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(Muted);
            });
            page.Content().Column(column =>
            {
                column.Spacing(16);
                column.Item().Row(row =>
                {
                    row.Spacing(12);
                    row.RelativeItem(1).Background(Lavender).Padding(14).Text(cantidad).FontSize(12).SemiBold();
                    row.RelativeItem(2).Background(Mint).Padding(14).Text(resumen).FontSize(12).SemiBold();
                });
                if (filas.Count == 0)
                {
                    column.Item().Background(Canvas).Padding(24).Text("No hay registros para mostrar.").FontColor(Muted);
                    return;
                }
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var ancho in anchos) columns.RelativeColumn(ancho);
                    });
                    table.Header(header =>
                    {
                        for (var i = 0; i < encabezados.Length; i++)
                        {
                            var cell = header.Cell().Background(Lime).PaddingVertical(10).PaddingHorizontal(6);
                            if (i >= primeraColumnaNumerica) cell = cell.AlignRight();
                            cell.Text(encabezados[i]).FontSize(8).SemiBold();
                        }
                    });
                    for (var fila = 0; fila < filas.Count; fila++)
                    {
                        for (var i = 0; i < encabezados.Length; i++)
                        {
                            var cell = table.Cell().ShowEntire().Background(fila % 2 == 0 ? "#FFFFFF" : Canvas)
                                .BorderBottom(0.5f).BorderColor(Border).PaddingVertical(9).PaddingHorizontal(6);
                            if (i >= primeraColumnaNumerica) cell = cell.AlignRight();
                            cell.Text(filas[fila][i]);
                        }
                    }
                });
            });
            page.Footer().PaddingTop(16).BorderTop(1).BorderColor(Border).PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text("Sistema de Ventas  /  " + seccion).FontSize(8).FontColor(Muted);
                row.AutoItem().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontSize(8).FontColor(Muted));
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        })).GeneratePdf();
    }
}

