namespace TP5ProgramacionMovil.DTOs
{
    public class EnviarReporteRequestDto
    {
        public string Email { get; set; } = string.Empty;

        public string TipoReporte { get; set; } = string.Empty;
    }
}