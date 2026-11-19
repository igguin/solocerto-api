using SoloCerto.Api.Models;

namespace SoloCerto.Api.DTOs;

/// <summary>Conversões entidade → DTO (métodos de extensão).</summary>
public static class Mapeamentos
{
    public static PlantacaoResponse ToResponse(this Plantacao p) =>
        new(p.Id, p.AgricultorId, p.Cultura, p.AreaHectares, p.UmidadeCritica, p.UmidadeIdeal, p.CriadoEm);

    public static MedicaoResponse ToResponse(this MedicaoSolo m, Plantacao plantacao) =>
        new(m.Id, m.PlantacaoId, m.UmidadePercentual, m.MedidoEm, m.Origem,
            plantacao.ClassificarUmidade(m.UmidadePercentual));

    public static PrevisaoChuvaResponse ToResponse(this PrevisaoChuva p) =>
        new(p.Id, p.PlantacaoId, p.Probabilidade, p.ValidaAPartirDe, p.ValidaAPartirDe.AddHours(24), p.Fonte);

    public static RecomendacaoResponse ToResponse(this RecomendacaoIrrigacao r) =>
        new(r.Id, r.PlantacaoId, r.MedicaoSoloId, r.Tipo, MensagemPara(r.Tipo), r.Justificativa, r.RegraAplicada,
            r.UmidadeConsiderada, r.ProbabilidadeChuvaConsiderada, r.IrrigacaoEvitada, r.CriadoEm);

    /// <summary>Texto curto, sem jargão, para o produtor.</summary>
    public static string MensagemPara(TipoRecomendacao tipo) => tipo switch
    {
        TipoRecomendacao.IrrigacaoPrioritaria => "Irrigue o quanto antes: o solo está seco demais para a cultura.",
        TipoRecomendacao.IrrigarHoje => "Irrigue hoje: o solo está abaixo do ideal e não há chuva prevista.",
        TipoRecomendacao.AguardarChuva => "Aguarde: há boa chance de chuva nas próximas 24 horas. Volte a medir amanhã.",
        TipoRecomendacao.NaoIrrigar => "Não precisa irrigar: a umidade do solo está adequada.",
        TipoRecomendacao.Reavaliar => "Faça uma nova medição do solo antes de decidir.",
        _ => "Sem recomendação.",
    };
}
