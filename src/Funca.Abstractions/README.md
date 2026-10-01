# Funca.Abstractions

Abstrações leves para aplicações .NET que seguem um desenho funcional no núcleo e uma shell imperativa nas bordas.

O pacote entrega containers para modelar sucesso, falha e ausência de valor sem depender de exceções como fluxo
principal, além de contratos para casos de uso, mensageria, consultas, event sourcing, multi-tenant e metadados
customizáveis.

## Instalação

```bash
dotnet add package Funca.Abstractions
```

## Target framework

Este pacote mira `net11.0` e usa recursos preview do .NET/C#.

## Recursos entregues

### Containers funcionais

- `Result<T>` para representar operações que podem terminar com sucesso ou erro.
- `Option<T>` para representar presença (`Some`) ou ausência (`None`) de valor.
- `ErrorResult` com tipos de erro padronizados: `Failure`, `Invalid`, `NotFound`, `Unauthorized` e `Forbidden`.
- Operações de composição como `Map`, `Bind`, `Match`, `Ensure`, `Filter`, `OrElse`, `Tee` e `IfNone`.
- Suporte síncrono, `Task` e `ValueTask` para pipelines sem quebrar o estilo funcional.
- Conversão de `Option<T>` para `Result<T>` quando a ausência precisa virar erro explícito.

Exemplo:

```csharp
using Funca.Abstractions.Containers;

Result<Customer> result =
    Result.Ok(input)
        .Ensure(x => !string.IsNullOrWhiteSpace(x.Document), () => ErrorResult.Invalid("Document is required."))
        .Map(x => new Customer(x.Name, x.Document));

return result.Match(
    customer => Results.Ok(customer),
    errors => Results.BadRequest(errors));
```

### Contratos para application shell

- `IMessage` como marcador para mensagens de entrada, saída e eventos.
- `IInteractor<TInput, TOutput>` para padronizar casos de uso que retornam `Result<TOutput>`.
- `RequestContext` para carregar `CorrelationId`, usuário atual e anexos tipados durante a execução.
- `UserContext` para representar usuário autenticado.

Exemplo:

```csharp
public sealed record CreateOrderCommand(Guid CustomerId) : IMessage;
public sealed record CreateOrderOutput(Guid OrderId) : IMessage;

public sealed class CreateOrderInteractor : IInteractor<CreateOrderCommand, CreateOrderOutput>
{
    public ValueTask<Result<CreateOrderOutput>> InteractAsync(
        CreateOrderCommand input,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Result.Ok(new CreateOrderOutput(Guid.CreateVersion7())));
    }
}
```

### Dados, consultas e persistência

- `IState<TKey>` para estados identificáveis.
- `Query<TState, TKey>` com paginação, ordenação e ponto de extensão para filtros via `IQueryable<TState>`.
- `QueryResult<T>` com cálculo de quantidade de páginas.
- `IQueryStore<TState, TKey>` para consultas por id, lote, projeção e paginação.
- EF e MongoDB expõem os recursos nativos para gravação; não há uma abstração genérica de escrita.
- `GuidModule.Sequential()` para criação de GUID v7 e `ToGuid()` para parse seguro retornando `Result<Guid>`.

### Event sourcing e multi-tenancy

- `IEvent` para eventos com `Timestamp`.
- `IEventStore` para append e leitura de envelopes de eventos.
- `EventEnvelopeState` com dados de sequência, versão, tenant, agregado, ator, correlação, tipo do evento e payload
  JSON.
- `TenantId` e `IRequireTenantPartition` para contratos que exigem particionamento por tenant.
- Extensões em `RequestContext` para definir tenant e envelopar eventos com contexto de usuário e correlação.

### Configuração dos stores

- `EFQueryStore` e as leituras de `EFEventStore` não rastreiam entidades. Para alterações, use o contexto EF
  diretamente.
- Na paginação, os stores aplicam `SortBy`/`OrderType` e desempate por `Id`. Sem `SortBy`, preservam a ordenação de
  `Query.Apply`, acrescentando `Id`; se não houver ordenação, usam `Id`.
- Os novos overloads dos stores recebem `TenantId?`. Em bancos/coleções compartilhados, informe o tenant corrente
  em todos os stores. Sem esse argumento, as consultas não são isoladas por tenant, permitindo bancos dedicados.
  O EF precisa mapear `TenantId` para permitir comparação no banco, por exemplo com um value converter.
- O overload de `MongoQueryStore` recebe conexão, coleção, sessão e tenant. A sessão deve pertencer à conexão
  fornecida e continuar válida durante toda a consulta, inclusive durante a enumeração dos eventos.
- Os event stores rejeitam envelopes de outro tenant quando um tenant é informado.
- `MongoEventStore.EnsureIndexesAsync` deve ser chamado na inicialização. O índice único por tenant, tipo, agregado
  e versão pressupõe um evento por versão. Configure a mesma restrição no modelo/migração EF. Se um comando produzir
  vários eventos, atribua uma versão diferente a cada evento ou desabilite essa unicidade e defina outra chave.
- `AppendAsync` não gera `Sequence`. A aplicação ou o mapeamento do banco deve fornecer uma sequência crescente e
  única no escopo consumido por `LoadFromSequenceAsync`. Um índice único de versão impede duplicatas, mas não valida
  a versão esperada nem impede lacunas; essa validação pertence ao fluxo de gravação e à transação.
- `LoadFromSequenceAsync` inclui a sequência informada (`>=`); o consumidor deve tratar a repetição do último evento.
- `IStateSnapshot.Version` indica o último evento incorporado; `SnapshotAt` é `DateTimeOffset` somente para leitura
  no contrato. O estado concreto também deve identificar o agregado e, quando aplicável, o tenant.

### Mensageria

- `IProducer` para publicação de mensagens.
- `IConsumer` para consumo de mensagens por fila com handler assíncrono.

### Metadados e campos customizados

- `FieldMetadata<TState>` e `IStateMetadata<TState>` para descrever campos de um estado.
- `FieldType` para tipos comuns de campo: texto, inteiro, decimal, booleano, data, seleção única e múltipla.
- `CustomFieldMetadata`, `FieldId`, `IHaveCustomData` e `CustomData` para cenários com campos customizados por tenant e
  entidade.
- `MetadataModule.Field(...)` para criar metadados de campo de forma concisa.

## Invariantes importantes

- Todo `Result<T>` em estado de erro carrega pelo menos um `ErrorResult` não nulo.
- Coleções de erro são copiadas na criação e na materialização, impedindo mutação externa do resultado depois de criado.
- `Result<T>.IsOk` independe de `T` ser nullable ou do valor interno ser `null`; o estado de sucesso é controlado por
  uma flag explícita.
- `Option<T>.Some(...)` não aceita valor `null`; use `Option.From(value)` quando quiser converter `null` em `None`.

## Quando usar

Use `Funca.Abstractions` para separar regras de negócio puras de detalhes de infraestrutura, deixando erros, ausência de
valor, consultas, eventos, mensagens e contexto de requisição com contratos pequenos e consistentes.
