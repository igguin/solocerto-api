# Evidências · requisições e respostas reais

Gerado em 2026-11-19 22:21 UTC, com a API rodando localmente (`dotnet run`) sobre um banco SQLite novo, criado pelas migrations. Dados 100% fictícios.

## 1. Listar agricultores (dados fictícios de demonstração)

`GET /api/agricultores` → **HTTP 200**

Resposta:
```json
[
  {
    "id": 1,
    "nome": "João da Silva (fictício)",
    "cidade": "Campinas",
    "email": "joao.demo@exemplo.com",
    "criadoEm": "2026-11-19T22:21:37.7158511Z",
    "totalPlantacoes": 2
  },
  {
    "id": 2,
    "nome": "Maria Souza (fictícia)",
    "cidade": "Bragança Paulista",
    "email": null,
    "criadoEm": "2026-11-19T22:21:37.7162735Z",
    "totalPlantacoes": 1
  }
]
```

## 2. Cadastrar agricultor

`POST /api/agricultores` → **HTTP 201**

Requisição:
```json
{
  "nome": "Carlos Pereira (fictício)",
  "cidade": "Amparo",
  "email": "carlos.demo@exemplo.com"
}
```

Resposta:
```json
{
  "id": 3,
  "nome": "Carlos Pereira (fictício)",
  "cidade": "Amparo",
  "email": "carlos.demo@exemplo.com",
  "criadoEm": "2026-11-19T22:21:39.1864543Z",
  "totalPlantacoes": 0
}
```

## 3. Atualizar agricultor (PUT)

`PUT /api/agricultores/3` → **HTTP 204**

Requisição:
```json
{
  "nome": "Carlos Pereira (fictício)",
  "cidade": "Amparo",
  "email": "carlos@exemplo.com"
}
```

Resposta:
```json
(sem corpo)
```

## 4. Cadastrar plantação de feijão para o novo agricultor

`POST /api/plantacoes` → **HTTP 201**

Requisição:
```json
{
  "agricultorId": 3,
  "cultura": "Feijão",
  "areaHectares": 2.5,
  "umidadeCritica": 24,
  "umidadeIdeal": 34
}
```

Resposta:
```json
{
  "id": 4,
  "agricultorId": 3,
  "cultura": "Feijão",
  "areaHectares": 2.5,
  "umidadeCritica": 24,
  "umidadeIdeal": 34,
  "criadoEm": "2026-11-19T22:21:39.3496018Z"
}
```

## 5. Plantações de um agricultor

`GET /api/agricultores/1/plantacoes` → **HTTP 200**

Resposta:
```json
[
  {
    "id": 1,
    "agricultorId": 1,
    "cultura": "Milho",
    "areaHectares": 5,
    "umidadeCritica": 25,
    "umidadeIdeal": 35,
    "criadoEm": "2026-11-19T22:21:37.8571754Z"
  },
  {
    "id": 2,
    "agricultorId": 1,
    "cultura": "Tomate",
    "areaHectares": 1.5,
    "umidadeCritica": 30,
    "umidadeIdeal": 45,
    "criadoEm": "2026-11-19T22:21:37.8575047Z"
  }
]
```

## 6. Módulo de IA envia previsão de chuva de 82% para o milho

`POST /api/previsoes-chuva` → **HTTP 201**

Requisição:
```json
{
  "plantacaoId": 1,
  "probabilidade": 0.82,
  "fonte": "modelo-random-forest-inmet"
}
```

Resposta:
```json
{
  "id": 1,
  "plantacaoId": 1,
  "probabilidade": 0.82,
  "validaAPartirDe": "2026-11-19T22:21:39.3897217Z",
  "validaAte": "2026-11-20T22:21:39.3897217Z",
  "fonte": "modelo-random-forest-inmet"
}
```

## 7. Medição 31% no milho + chuva provável → AguardarChuva (irrigação evitada)

`POST /api/medicoes` → **HTTP 201**

Requisição:
```json
{
  "plantacaoId": 1,
  "umidadePercentual": 31,
  "origem": "Sensor"
}
```

Resposta:
```json
{
  "medicao": {
    "id": 1,
    "plantacaoId": 1,
    "umidadePercentual": 31,
    "medidoEm": "2026-11-19T22:21:39.4304544Z",
    "origem": "Sensor",
    "faixa": "Baixa"
  },
  "recomendacao": {
    "id": 1,
    "plantacaoId": 1,
    "medicaoSoloId": 1,
    "tipo": "AguardarChuva",
    "mensagem": "Aguarde: há boa chance de chuva nas próximas 24 horas. Volte a medir amanhã.",
    "justificativa": "Umidade de 31% está abaixo do ideal (35%), mas a previsão indica 82% de chance de chuva nas próximas 24 horas.",
    "regraAplicada": "RegraChuvaPrevista",
    "umidadeConsiderada": 31,
    "probabilidadeChuvaConsiderada": 0.82,
    "irrigacaoEvitada": true,
    "geradaEm": "2026-11-19T22:21:39.4592844Z"
  }
}
```

## 8. Medição 38% no tomate sem previsão → IrrigarHoje

`POST /api/medicoes` → **HTTP 201**

Requisição:
```json
{
  "plantacaoId": 2,
  "umidadePercentual": 38,
  "origem": "Manual"
}
```

Resposta:
```json
{
  "medicao": {
    "id": 2,
    "plantacaoId": 2,
    "umidadePercentual": 38,
    "medidoEm": "2026-11-19T22:21:39.5256885Z",
    "origem": "Manual",
    "faixa": "Baixa"
  },
  "recomendacao": {
    "id": 2,
    "plantacaoId": 2,
    "medicaoSoloId": 2,
    "tipo": "IrrigarHoje",
    "mensagem": "Irrigue hoje: o solo está abaixo do ideal e não há chuva prevista.",
    "justificativa": "Umidade de 38% está abaixo do ideal (45%) e não há previsão de chuva disponível.",
    "regraAplicada": "RegraUmidadeBaixa",
    "umidadeConsiderada": 38,
    "probabilidadeChuvaConsiderada": null,
    "irrigacaoEvitada": false,
    "geradaEm": "2026-11-19T22:21:39.5273315Z"
  }
}
```

## 9. Medição 18% no café → IrrigacaoPrioritaria

`POST /api/medicoes` → **HTTP 201**

Requisição:
```json
{
  "plantacaoId": 3,
  "umidadePercentual": 18,
  "origem": "Sensor"
}
```

Resposta:
```json
{
  "medicao": {
    "id": 3,
    "plantacaoId": 3,
    "umidadePercentual": 18,
    "medidoEm": "2026-11-19T22:21:39.5338709Z",
    "origem": "Sensor",
    "faixa": "Critica"
  },
  "recomendacao": {
    "id": 3,
    "plantacaoId": 3,
    "medicaoSoloId": 3,
    "tipo": "IrrigacaoPrioritaria",
    "mensagem": "Irrigue o quanto antes: o solo está seco demais para a cultura.",
    "justificativa": "Umidade de 18% está abaixo do limite crítico de 22% para Café. o risco de estresse hídrico é alto.",
    "regraAplicada": "RegraUmidadeCritica",
    "umidadeConsiderada": 18,
    "probabilidadeChuvaConsiderada": null,
    "irrigacaoEvitada": false,
    "geradaEm": "2026-11-19T22:21:39.5351964Z"
  }
}
```

## 10. Medição 41% no feijão → NaoIrrigar

`POST /api/medicoes` → **HTTP 201**

Requisição:
```json
{
  "plantacaoId": 4,
  "umidadePercentual": 41,
  "origem": "Sensor"
}
```

Resposta:
```json
{
  "medicao": {
    "id": 4,
    "plantacaoId": 4,
    "umidadePercentual": 41,
    "medidoEm": "2026-11-19T22:21:39.5398571Z",
    "origem": "Sensor",
    "faixa": "Adequada"
  },
  "recomendacao": {
    "id": 4,
    "plantacaoId": 4,
    "medicaoSoloId": 4,
    "tipo": "NaoIrrigar",
    "mensagem": "Não precisa irrigar: a umidade do solo está adequada.",
    "justificativa": "Umidade de 41% está dentro da faixa adequada para Feijão (ideal a partir de 34%).",
    "regraAplicada": "RegraUmidadeAdequada",
    "umidadeConsiderada": 41,
    "probabilidadeChuvaConsiderada": null,
    "irrigacaoEvitada": false,
    "geradaEm": "2026-11-19T22:21:39.5412971Z"
  }
}
```

## 11. Reavaliar o tomate agora

`POST /api/recomendacoes/avaliar/2` → **HTTP 201**

Resposta:
```json
{
  "id": 5,
  "plantacaoId": 2,
  "medicaoSoloId": 2,
  "tipo": "IrrigarHoje",
  "mensagem": "Irrigue hoje: o solo está abaixo do ideal e não há chuva prevista.",
  "justificativa": "Umidade de 38% está abaixo do ideal (45%) e não há previsão de chuva disponível.",
  "regraAplicada": "RegraUmidadeBaixa",
  "umidadeConsiderada": 38,
  "probabilidadeChuvaConsiderada": null,
  "irrigacaoEvitada": false,
  "geradaEm": "2026-11-19T22:21:39.5746965Z"
}
```

## 12. Situação atual do milho

`GET /api/plantacoes/1/situacao` → **HTTP 200**

Resposta:
```json
{
  "plantacao": {
    "id": 1,
    "agricultorId": 1,
    "cultura": "Milho",
    "areaHectares": 5,
    "umidadeCritica": 25,
    "umidadeIdeal": 35,
    "criadoEm": "2026-11-19T22:21:37.8571754Z"
  },
  "ultimaMedicao": {
    "id": 1,
    "plantacaoId": 1,
    "umidadePercentual": 31,
    "medidoEm": "2026-11-19T22:21:39.4304544Z",
    "origem": "Sensor",
    "faixa": "Baixa"
  },
  "previsaoVigente": {
    "id": 1,
    "plantacaoId": 1,
    "probabilidade": 0.82,
    "validaAPartirDe": "2026-11-19T22:21:39.3897217Z",
    "validaAte": "2026-11-20T22:21:39.3897217Z",
    "fonte": "modelo-random-forest-inmet"
  },
  "ultimaRecomendacao": {
    "id": 1,
    "plantacaoId": 1,
    "medicaoSoloId": 1,
    "tipo": "AguardarChuva",
    "mensagem": "Aguarde: há boa chance de chuva nas próximas 24 horas. Volte a medir amanhã.",
    "justificativa": "Umidade de 31% está abaixo do ideal (35%), mas a previsão indica 82% de chance de chuva nas próximas 24 horas.",
    "regraAplicada": "RegraChuvaPrevista",
    "umidadeConsiderada": 31,
    "probabilidadeChuvaConsiderada": 0.82,
    "irrigacaoEvitada": true,
    "geradaEm": "2026-11-19T22:21:39.4592844Z"
  }
}
```

## 13. Histórico do milho (7 dias)

`GET /api/plantacoes/1/historico?dias=7` → **HTTP 200**

Resposta:
```json
{
  "plantacaoId": 1,
  "desde": "2026-11-12T22:21:39.6190484Z",
  "totalMedicoes": 1,
  "umidadeMedia": 31,
  "umidadeMinima": 31,
  "umidadeMaxima": 31,
  "medicoes": [
    {
      "id": 1,
      "plantacaoId": 1,
      "umidadePercentual": 31,
      "medidoEm": "2026-11-19T22:21:39.4304544Z",
      "origem": "Sensor",
      "faixa": "Baixa"
    }
  ],
  "recomendacoes": [
    {
      "id": 1,
      "plantacaoId": 1,
      "medicaoSoloId": 1,
      "tipo": "AguardarChuva",
      "mensagem": "Aguarde: há boa chance de chuva nas próximas 24 horas. Volte a medir amanhã.",
      "justificativa": "Umidade de 31% está abaixo do ideal (35%), mas a previsão indica 82% de chance de chuva nas próximas 24 horas.",
      "regraAplicada": "RegraChuvaPrevista",
      "umidadeConsiderada": 31,
      "probabilidadeChuvaConsiderada": 0.82,
      "irrigacaoEvitada": true,
      "geradaEm": "2026-11-19T22:21:39.4592844Z"
    }
  ]
}
```

## 14. Recomendações do tipo AguardarChuva

`GET /api/recomendacoes?tipo=AguardarChuva` → **HTTP 200**

Resposta:
```json
[
  {
    "id": 1,
    "plantacaoId": 1,
    "medicaoSoloId": 1,
    "tipo": "AguardarChuva",
    "mensagem": "Aguarde: há boa chance de chuva nas próximas 24 horas. Volte a medir amanhã.",
    "justificativa": "Umidade de 31% está abaixo do ideal (35%), mas a previsão indica 82% de chance de chuva nas próximas 24 horas.",
    "regraAplicada": "RegraChuvaPrevista",
    "umidadeConsiderada": 31,
    "probabilidadeChuvaConsiderada": 0.82,
    "irrigacaoEvitada": true,
    "geradaEm": "2026-11-19T22:21:39.4592844Z"
  }
]
```

## 15. Indicadores de impacto

`GET /api/indicadores` → **HTTP 200**

Resposta:
```json
{
  "totalAgricultores": 3,
  "totalPlantacoes": 4,
  "totalMedicoes": 4,
  "totalRecomendacoes": 5,
  "recomendacoesPorTipo": [
    {
      "tipo": "IrrigarHoje",
      "quantidade": 2
    },
    {
      "tipo": "IrrigacaoPrioritaria",
      "quantidade": 1
    },
    {
      "tipo": "AguardarChuva",
      "quantidade": 1
    },
    {
      "tipo": "NaoIrrigar",
      "quantidade": 1
    }
  ],
  "irrigacoesEvitadas": 1,
  "percentualIrrigacoesEvitadas": 33.3,
  "areaMonitoradaHectares": 12.0,
  "umidadePorCultura": [
    {
      "cultura": "Café",
      "medicoes": 1,
      "umidadeMedia": 18
    },
    {
      "cultura": "Feijão",
      "medicoes": 1,
      "umidadeMedia": 41
    },
    {
      "cultura": "Milho",
      "medicoes": 1,
      "umidadeMedia": 31
    },
    {
      "cultura": "Tomate",
      "medicoes": 1,
      "umidadeMedia": 38
    }
  ],
  "plantacoesEmSituacaoCritica": [
    {
      "plantacaoId": 3,
      "cultura": "Café",
      "agricultor": "Maria Souza (fictícia)",
      "cidade": "Bragança Paulista",
      "umidadeConsiderada": 18,
      "recomendadoEm": "2026-11-19T22:21:39.5351964Z"
    }
  ]
}
```

## 16. Erro: umidade crítica maior que a ideal (regra de negócio)

`POST /api/plantacoes` → **HTTP 400**

Requisição:
```json
{
  "agricultorId": 1,
  "cultura": "Alface",
  "areaHectares": 1,
  "umidadeCritica": 50,
  "umidadeIdeal": 40
}
```

Resposta:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Regra de negócio violada",
  "status": 400,
  "detail": "A umidade crítica deve ser menor que a umidade ideal.",
  "traceId": "00-b98ade6e78eaf37ecd410a2fc4f3fee4-7db059460d3514fb-00"
}
```

## 17. Erro: umidade fora de 0 a 100 (validação)

`POST /api/medicoes` → **HTTP 400**

Requisição:
```json
{
  "plantacaoId": 1,
  "umidadePercentual": 150
}
```

Resposta:
```json
{
  "title": "Dados inválidos",
  "status": 400,
  "errors": {
    "UmidadePercentual": [
      "A umidade deve estar entre 0 e 100."
    ]
  }
}
```

## 18. Erro: plantação inexistente

`GET /api/plantacoes/999` → **HTTP 404**

Resposta:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "Plantação 999 não encontrado(a).",
  "traceId": "00-46116544b06b9337ad01f2f305bf77d9-6dd34a3e1d5fde70-00"
}
```

## 19. Excluir agricultor (cascata)

`DELETE /api/agricultores/3` → **HTTP 204**

Resposta:
```json
(sem corpo)
```

## 20. Confirma exclusão

`GET /api/agricultores/3` → **HTTP 404**

Resposta:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "Agricultor 3 não encontrado(a).",
  "traceId": "00-e1c3c055703d526fc0327437c025b292-5eacf1d7275736bb-00"
}
```
