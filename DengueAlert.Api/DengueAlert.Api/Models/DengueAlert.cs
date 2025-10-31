namespace DengueAlert.Api.Models
{
    public class DengueAlerta
    {
        public long Id { get; set; }
        public int GeoCode { get; set; }
        public int SemanaEpidemiologica { get; set; }
        public int EndWeek { get; set; }
        public int EndYear { get; set; }

        public DateTime DataIniSE { get; set; }

        public double? CasosEst { get; set; }
        public double? CasosEstMin { get; set; }
        public double? CasosEstMax { get; set; }
        public int? Casos { get; set; }
        public int? Nivel { get; set; }

        public double? PRt1 { get; set; }
        public double? PInc100k { get; set; }
        public string? VersaoModelo { get; set; }
        public double? Rt { get; set; }
        public int? Pop { get; set; }
        public double? TempMin { get; set; }
        public double? TempMed { get; set; }
        public double? TempMax { get; set; }
        public double? UmidMin { get; set; }
        public double? UmidMed { get; set; }
        public double? UmidMax { get; set; }
        public int? Receptivo { get; set; }
        public int? Transmissao { get; set; }
        public int? NivelInc { get; set; }
        public int? CasProv { get; set; }
        public int? NotifAccumYear { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SemanaParams
    {
        public int ew { get; set; } 
        public int ey { get; set; } 
    }
}
