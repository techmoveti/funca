# Funca.Abstractions

Abstrações leves para aplicações .NET que seguem um desenho funcional no núcleo e uma shell imperativa nas bordas.

O pacote entrega containers para modelar sucesso, falha e ausência de valor sem depender de exceções como fluxo principal, além de contratos para casos de uso, mensageria, consultas, event sourcing, multi-tenant e metadados customizáveis.

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
- `IUnitOfWork` para confirmação de mudanças.
- `GuidModule.Sequential()` para criação de GUID v7 e `ToGuid()` para parse seguro retornando `Result<Guid>`.

### Event sourcing e multi-tenancy

- `IEvent` para eventos com `Timestamp`.
- `IEventStore` para append e leitura de envelopes de eventos.
- `EventEnvelopeState` com dados de sequência, versão, tenant, agregado, ator, correlação, tipo do evento e payload JSON.
- `TenantId` e `IRequireTenantPartition` para contratos que exigem particionamento por tenant.
- Extensões em `RequestContext` para definir tenant e envelopar eventos com contexto de usuário e correlação.

### Mensageria

- `IProducer` para publicação de mensagens.
- `IConsumer` para consumo de mensagens por fila com handler assíncrono.

### Metadados e campos customizados

- `FieldMetadata<TState>` e `IStateMetadata<TState>` para descrever campos de um estado.
- `FieldType` para tipos comuns de campo: texto, inteiro, decimal, booleano, data, seleção única e múltipla.
- `CustomFieldMetadata`, `FieldId`, `IHaveCustomData` e `CustomData` para cenários com campos customizados por tenant e entidade.
- `MetadataModule.Field(...)` para criar metadados de campo de forma concisa.

## Invariantes importantes

- Todo `Result<T>` em estado de erro carrega pelo menos um `ErrorResult` não nulo.
- Coleções de erro são copiadas na criação e na materialização, impedindo mutação externa do resultado depois de criado.
- `Result<T>.IsOk` independe de `T` ser nullable ou do valor interno ser `null`; o estado de sucesso é controlado por uma flag explícita.
- `Option<T>.Some(...)` não aceita valor `null`; use `Option.From(value)` quando quiser converter `null` em `None`.

## Quando usar

Use `Funca.Abstractions` para separar regras de negócio puras de detalhes de infraestrutura, deixando erros, ausência de valor, consultas, eventos, mensagens e contexto de requisição com contratos pequenos e consistentes.
