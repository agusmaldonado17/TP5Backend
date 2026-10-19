using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Services
{
    public interface IReportePdfService
    {
        byte[] GenerarReporteStock(IReadOnlyCollection<Producto> productos);

        byte[] GenerarReporteVentas(IReadOnlyCollection<Venta> ventas);

        byte[] GenerarReporteEntradas(IReadOnlyCollection<Compra> compras);
    }


    public class ReportePdfService : IReportePdfService
    {
        public byte[] GenerarReporteStock(
            IReadOnlyCollection<Producto> productos)
        {
            var cultura =
                System.Globalization.CultureInfo
                    .GetCultureInfo("es-AR");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);

                    page.Header()
                        .Text("Reporte de Stock")
                        .FontSize(20)
                        .SemiBold();

                    page.Content()
                        .PaddingVertical(16)
                        .Column(column =>
                        {
                            column.Item()
                                .Text(
                                    $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(12)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("ID");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Producto");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Categoría");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .AlignRight()
                                            .Text("Precio");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .AlignRight()
                                            .Text("Stock");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .AlignRight()
                                            .Text("Reposición");
                                    });

                                    foreach (var producto in productos)
                                    {
                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(producto.Id.ToString());

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(producto.Nombre);

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                producto.CategoriaProducto?
                                                    .Nombre ?? "-");

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .AlignRight()
                                            .Text(
                                                producto.PrecioVenta
                                                    .ToString("C2", cultura));

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .AlignRight()
                                            .Text(
                                                producto.StockActual
                                                    .ToString());

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .AlignRight()
                                            .Text(
                                                producto.PuntoReposicion
                                                    .ToString());
                                    }
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Total de productos: ");

                            text.Span(
                                productos.Count.ToString())
                                .SemiBold();
                        });
                });
            }).GeneratePdf();
        }


        public byte[] GenerarReporteVentas(
            IReadOnlyCollection<Venta> ventas)
        {
            var cultura =
                System.Globalization.CultureInfo
                    .GetCultureInfo("es-AR");

            var totalVentas =
                ventas.Sum(v => v.Total);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);

                    page.Header()
                        .Text("Reporte de Ventas")
                        .FontSize(20)
                        .SemiBold();

                    page.Content()
                        .PaddingVertical(16)
                        .Column(column =>
                        {
                            column.Item()
                                .Text(
                                    $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(12)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.5f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Nº");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Fecha");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Cliente");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Usuario");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .AlignRight()
                                            .Text("Total");
                                    });

                                    foreach (var venta in ventas)
                                    {
                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                venta.Id.ToString());

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                venta.Fecha
                                                    .ToString(
                                                        "dd/MM/yyyy HH:mm"));

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                venta.Cliente?.Nombre
                                                ?? "-");

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                venta.Usuario?.Nombre
                                                ?? "-");

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .AlignRight()
                                            .Text(
                                                venta.Total
                                                    .ToString(
                                                        "C2",
                                                        cultura));
                                    }
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                $"Cantidad de ventas: {ventas.Count} | Total: ");

                            text.Span(
                                totalVentas.ToString(
                                    "C2",
                                    cultura))
                                .SemiBold();
                        });
                });
            }).GeneratePdf();
        }


        public byte[] GenerarReporteEntradas(
            IReadOnlyCollection<Compra> compras)
        {
            var cultura =
                System.Globalization.CultureInfo
                    .GetCultureInfo("es-AR");

            var totalCompras =
                compras.Sum(c => c.Total);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);

                    page.Header()
                        .Text("Reporte de Entradas de Mercadería")
                        .FontSize(20)
                        .SemiBold();

                    page.Content()
                        .PaddingVertical(16)
                        .Column(column =>
                        {
                            column.Item()
                                .Text(
                                    $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(12)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.5f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Nº");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Fecha");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Proveedor");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .Text("Usuario");

                                        header.Cell()
                                            .Element(EstiloEncabezado)
                                            .AlignRight()
                                            .Text("Total");
                                    });

                                    foreach (var compra in compras)
                                    {
                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                compra.Id.ToString());

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                compra.Fecha
                                                    .ToString(
                                                        "dd/MM/yyyy HH:mm"));

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                compra.Proveedor?
                                                    .RazonSocial ?? "-");

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .Text(
                                                compra.Usuario?.Nombre
                                                ?? "-");

                                        table.Cell()
                                            .Element(EstiloCelda)
                                            .AlignRight()
                                            .Text(
                                                compra.Total
                                                    .ToString(
                                                        "C2",
                                                        cultura));
                                    }
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span(
                                $"Cantidad de entradas: {compras.Count} | Total: ");

                            text.Span(
                                totalCompras.ToString(
                                    "C2",
                                    cultura))
                                .SemiBold();
                        });
                });
            }).GeneratePdf();
        }


        private static IContainer EstiloEncabezado(
            IContainer container)
        {
            return container
                .Background(Colors.Blue.Darken2)
                .Padding(6)
                .DefaultTextStyle(
                    x => x
                        .FontColor(Colors.White)
                        .SemiBold()
                );
        }


        private static IContainer EstiloCelda(
            IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6);
        }
    }
}