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

### Contratos para application shell

- `IMessage` como marcador para mensagens de entrada, saída e eventos.
- `IInteractor<TInput, TOutput>` para padronizar casos de uso com retorno livre, incluindo `Result<T>` e unions
  próprias.
- `RequestContext` para carregar `CorrelationId`, usuário atual e anexos tipados durante a execução.
- `UserContext` para representar usuário autenticado.

Exemplo:

```csharp
public sealed record CreateOrderCommand(Guid CustomerId) : IMessage;
public sealed record CreateOrderOutput(Guid OrderId) : IMessage;

public sealed class CreateOrderInteractor
    : IInteractor<CreateOrderCommand, Result<CreateOrderOutput>>
{
    public ValueTask<Result<CreateOrderOutput>> InteractAsync(
        CreateOrderCommand input,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Result<CreateOrderOutput>.Ok(new CreateOrderOutput(Guid.CreateVersion7())));
    }
}
```

Registre o interactor pela extensão para receber o decorator de logging ao resolver a interface:

```csharp
services.AddInteractor<CreateOrderCommand, Result<CreateOrderOutput>, CreateOrderInteractor>();
```

O registro usa lifetime scoped e requer logging registrado nos serviços (por exemplo, com `services.AddLogging()`).
Cada caso de uso também pode definir uma union própria, sem interfaces marcadoras:

```csharp
public sealed record ApprovalRequired(decimal Total);
public union CreateOrderOutcome(Success<CreateOrderOutput>, ApprovalRequired, ErrorCollection);
// O interactor pode implementar IInteractor<CreateOrderCommand, CreateOrderOutcome>.
```

O decorator identifica `Error` e `ErrorCollection` como erros conhecidos. Para os demais retornos,
registra "Execução concluída", sem classificar os casos próprios do domínio como sucesso.
Uma union sem valor é rejeitada; retornos comuns podem ser nulos quando seu contrato permitir.

### Resultados e validação

`Result<T>` é uma union nativa de `Success<T>` e `ErrorCollection`.
Use `Result<T>.Ok(value)` para sucesso e `Result<T>.Fail(error)` para falha. O caso `Success<T>`
preserva sucesso com `null` e evita sobreposição entre o valor e o tipo dos erros.
Um resultado `default` não possui valor e deve ser rejeitado ao consumir o resultado.

```csharp
Result<string?> result = Result<string?>.Ok(null);

var message = result switch
{
    Success<string?> success => success.Value ?? "Sucesso sem conteúdo",
    ErrorCollection errors => string.Join("; ", errors.Errors.Select(error => error.Message)),
    null => throw new InvalidOperationException("Resultado sem valor"),
};
```

`Of` inicia um encadeamento com sucesso. `Ensure` valida o valor e pode receber um erro personalizado;
após a primeira falha, os próximos predicados não são executados. `Map` transforma o valor de sucesso,
enquanto `Bind` encadeia uma operação que já retorna `Result<TOut>`, sem criar resultados aninhados.
Em caso de falha, ambos propagam os erros sem executar a função recebida.

```csharp
Result<int> ParseAge(string text) => int.TryParse(text, out var age)
    ? Result<int>.Of(age)
    : Result<int>.Fail(Error.Invalid("idade", "Informe um número inteiro"));

var ageResult = Result<string>.Of("25")
    .Ensure(text => !string.IsNullOrWhiteSpace(text), Error.Invalid("idade", "Informe a idade"))
    .Bind(ParseAge)
    .Ensure(age => age >= 18, Error.Invalid("idade", "Deve ser maior de idade"));

var nameResult = Result<string>.Of("Ana")
    .Ensure(name => !string.IsNullOrWhiteSpace(name), Error.Invalid("nome", "Informe o nome"));

Result<string> description = nameResult
    .Combine(ageResult, (name, age) => (Name: name, Age: age))
    .Map(person => $"{person.Name}: {person.Age} anos");
```

`Combine(other, combiner)` junta os valores quando ambos têm sucesso e acumula os erros em ordem,
da esquerda para a direita, quando há falhas. `Combine(other)` e `Result<T>.Combine(left, right)`
mantêm o valor da esquerda quando ambos têm sucesso. As funções fornecidas a essas operações
devem ser não nulas; exceções lançadas por elas são propagadas. `Bind` rejeita um resultado `default`
retornado pela função.

`Match` produz um valor comum executando apenas o handler de sucesso ou de falha. `Tap` executa
uma ação no sucesso e devolve o mesmo resultado; em falhas, a ação não é executada. `Recover`
recebe os erros e retorna um resultado alternativo apenas em falhas, preservando sucessos existentes.
O resultado alternativo pode ser sucesso ou falha, mas não pode ser `default`.

```csharp
var message = description
    .Tap(text => Console.WriteLine(text))
    .Match(
        text => text,
        errors => string.Join("; ", errors.Errors.Select(error => error.Message)));

var withFallback = Result<string>.Fail(Error.NotFound("Descrição não encontrada"))
    .Recover(_ => Result<string>.Of("Não informado"));
```

`EnsureAsync` recebe `Func<T, CancellationToken, ValueTask<bool>>`, com erro personalizado opcional.
`BindAsync` recebe `Func<T, CancellationToken, ValueTask<Result<TOut>>>`. `TapAsync` recebe
`Func<T, CancellationToken, ValueTask>`, aguarda a ação no sucesso e devolve o mesmo resultado.
Essas operações retornam `ValueTask` e recebem um `CancellationToken` opcional.
Aguarde cada etapa com `await` antes de encadear a próxima.
Em falhas, os callbacks não são executados. O cancelamento é verificado antes da operação, inclusive
em resultados de falha, e novamente após aguardar o callback. `BindAsync` rejeita um resultado `default`
retornado pelo callback. Exceções dos callbacks são propagadas, assim como nas operações síncronas.

Para acumular erros de validações independentes, crie um `Result` para cada valor e use `Combine`,
como no exemplo acima. O encadeamento de `Ensure` interrompe as validações na primeira falha.
`ErrorCollection.Errors` é um `ImmutableArray<Error>` com uma cópia dos erros fornecidos.

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

## Quando usar

Use `Funca.Abstractions` para separar regras de negócio puras de detalhes de infraestrutura, deixando erros, ausência de
valor, consultas, eventos, mensagens e contexto de requisição com contratos pequenos e consistentes.
