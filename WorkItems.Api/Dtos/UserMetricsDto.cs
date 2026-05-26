namespace WorkItems.Api.Dtos
{
    /// <summary>
    /// DTO que consolida las métricas de carga actuales de un usuario en el sistema.
    /// </summary>
    public class UserMetricsDto
    {
        public string Username { get; set; } = string.Empty;
        public int PendingCount { get; set; }
        public int HighRelevancePendingCount { get; set; }
    }

}
