using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;
using TP5ProgramacionMovil.DTOs;
using TP5ProgramacionMovil.Services;
using System.Security.Claims;

namespace TP5ProgramacionMovil.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger<ReportesController> _logger;
        private readonly IReportePdfService _reportePdfService;
        private readonly IEmailService _emailService;

        public ReportesController(
            DataContext context,
            ILogger<ReportesController> logger,
            IReportePdfService reportePdfService,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _reportePdfService = reportePdfService;
            _emailService = emailService;
        }


        // GET: api/Reportes/stock
        [HttpGet("stock")]
        public async Task<IActionResult> Stock()
        {
            try
            {
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Include(p => p.CategoriaProducto)
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                var pdf =
                    _reportePdfService
                        .GenerarReporteStock(productos);

                var usuario =
                     User.FindFirstValue(ClaimTypes.Name)
                     ?? "Usuario desconocido";

                _logger.LogInformation(
                    "AUDITORIA - Reporte de stock generado. Usuario={Usuario}, CantidadProductos={CantidadProductos}",
                    usuario,
                    productos.Count
                );

                return File(
                    pdf,
                    "application/pdf",
                    "stock.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el PDF de stock"
                );

                return BadRequest(
                    "No se pudo generar el PDF de stock."
                );
            }
        }


        // GET: api/Reportes/ventas
        [HttpGet("ventas")]
        public async Task<IActionResult> Ventas()
        {
            try
            {
                var ventas = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Cliente)
                    .Include(v => v.Usuario)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();

                var pdf =
                    _reportePdfService
                        .GenerarReporteVentas(ventas);

                var usuario =
                   User.FindFirstValue(ClaimTypes.Name)
                   ?? "Usuario desconocido";

                _logger.LogInformation(
                    "AUDITORIA - Reporte de ventas generado. Usuario={Usuario}, CantidadVentas={CantidadVentas}",
                    usuario,
                    ventas.Count
                );

                return File(
                    pdf,
                    "application/pdf",
                    "ventas.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el PDF de ventas"
                );

                return BadRequest(
                    "No se pudo generar el PDF de ventas."
                );
            }
        }


        // GET: api/Reportes/entradas
        [HttpGet("entradas")]
        public async Task<IActionResult> Entradas()
        {
            try
            {
                var compras = await _context.Compras
                    .AsNoTracking()
                    .Include(c => c.Proveedor)
                    .Include(c => c.Usuario)
                    .OrderByDescending(c => c.Fecha)
                    .ToListAsync();

                var pdf =
                    _reportePdfService
                        .GenerarReporteEntradas(compras);

                var usuario =
                    User.FindFirstValue(ClaimTypes.Name)
                    ?? "Usuario desconocido";

                _logger.LogInformation(
                    "AUDITORIA - Reporte de entradas generado. Usuario={Usuario}, CantidadEntradas={CantidadEntradas}",
                    usuario,
                    compras.Count
                );

                return File(
                    pdf,
                    "application/pdf",
                    "entradas.pdf"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al generar el PDF de entradas"
                );

                return BadRequest(
                    "No se pudo generar el PDF de entradas."
                );
            }
        }


        // POST: api/Reportes/enviar
        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarReporte(
            EnviarReporteRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(
                    "Debe ingresar un correo electrónico."
                );
            }

            if (string.IsNullOrWhiteSpace(dto.TipoReporte))
            {
                return BadRequest(
                    "Debe indicar el tipo de reporte."
                );
            }

            try
            {
                byte[] pdf;
                string nombreArchivo;
                string asunto;

                switch (dto.TipoReporte.Trim().ToLower())
                {
                    case "stock":
                        {
                            var productos = await _context.Productos
                                .AsNoTracking()
                                .Include(p => p.CategoriaProducto)
                                .Where(p => p.Activo)
                                .OrderBy(p => p.Nombre)
                                .ToListAsync();

                            pdf =
                                _reportePdfService
                                    .GenerarReporteStock(productos);

                            nombreArchivo = "stock.pdf";
                            asunto = "Reporte de Stock";

                            break;
                        }

                    case "ventas":
                        {
                            var ventas = await _context.Ventas
                                .AsNoTracking()
                                .Include(v => v.Cliente)
                                .Include(v => v.Usuario)
                                .OrderByDescending(v => v.Fecha)
                                .ToListAsync();

                            pdf =
                                _reportePdfService
                                    .GenerarReporteVentas(ventas);

                            nombreArchivo = "ventas.pdf";
                            asunto = "Reporte de Ventas";

                            break;
                        }

                    case "entradas":
                        {
                            var compras = await _context.Compras
                                .AsNoTracking()
                                .Include(c => c.Proveedor)
                                .Include(c => c.Usuario)
                                .OrderByDescending(c => c.Fecha)
                                .ToListAsync();

                            pdf =
                                _reportePdfService
                                    .GenerarReporteEntradas(compras);

                            nombreArchivo = "entradas.pdf";
                            asunto = "Reporte de Entradas de Mercadería";

                            break;
                        }

                    default:
                        return BadRequest(
                            "Tipo de reporte inválido. Valores permitidos: stock, ventas, entradas."
                        );
                }


                await _emailService.EnviarReporteAsync(
                    dto.Email,
                    asunto,
                    "Se adjunta el reporte solicitado desde el Sistema de Ventas.",
                    pdf,
                    nombreArchivo
                );

                var usuario =
                   User.FindFirstValue(ClaimTypes.Name)
                   ?? "Usuario desconocido";

                _logger.LogInformation(
                    "AUDITORIA - Reporte enviado por correo. Usuario={Usuario}, TipoReporte={TipoReporte}, Destinatario={Destinatario}",
                    usuario,
                    dto.TipoReporte,
                    dto.Email
                );


                return Ok(new
                {
                    mensaje = "Reporte enviado correctamente."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al enviar el reporte por correo"
                );

                return BadRequest(
                    "No se pudo enviar el reporte por correo."
                );
            }
        }
    }
}