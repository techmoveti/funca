# Funca.Abstractions

Abstrações leves para aplicações .NET que seguem um desenho funcional no núcleo e uma shell imperativa nas bordas.

O pacote entrega containers para modelar sucesso e falha sem depender de exceções como fluxo principal,
além de contratos para casos de uso, mensageria, consultas, agregados, event sourcing, multi-tenancy e metadados
customizáveis, com implementações de persistência para Entity Framework Core.

`Option<T>` foi removido. A ausência de valor é representada por retornos nullable, como `TState?` em
`IQueryStore.GetAsync` e `T?` em `RequestContext.Detach<T>`. Quando a ausência representar uma falha de negócio,
use `Result<T>.Fail(Error.NotFound(...))`.

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
- `Attach<T>` para guardar anexos por chave e `Detach<T>` para consultá-los, retornando `null` quando não houver
  um valor do tipo solicitado. Apesar do nome, `Detach<T>` não remove o anexo.
- `UserContext` para representar usuário autenticado.

Exemplo:

```csharp
using Funca.Abstractions.Containers;
using Funca.Abstractions.Shell;

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

O registro dos interactors em injeção de dependência e a configuração de logging ficam a cargo da aplicação.
Cada caso de uso também pode definir uma union própria como saída:

```csharp
public sealed record ApprovalRequired(decimal Total);
public union CreateOrderOutcome(Success<CreateOrderOutput>, ApprovalRequired, ErrorCollection);
// O interactor pode implementar IInteractor<CreateOrderCommand, CreateOrderOutcome>.
```

`IInteractor` exige que a entrada seja uma classe que implemente `IMessage`; a saída não possui restrição de tipo.
O contrato retorna `ValueTask<TOutput>` e recebe `CancellationToken`.

### Resultados e validação

`Result<T>` é uma union customizada de `Success<T>` e `ErrorCollection`, implementada como `readonly struct`
com armazenamento tipado e um discriminador de caso.
Use `Result<T>.Ok(value)` para sucesso e `Result<T>.Fail(error)` para falha. O caso `Success<T>`
preserva sucesso com `null` e evita sobreposição entre o valor e o tipo dos erros.
Um resultado `default` não possui valor e deve ser rejeitado ao consumir o resultado.
`IsOk` indica se o resultado contém `Success<T>`. `Unwrap()` extrai seu valor, inclusive `null`, e lança
`InvalidOperationException` em falhas ou resultados não inicializados, com mensagens distintas para cada caso.

As factories, operações e o pattern matching diretamente sobre `Result<T>` evitam boxing dos casos. `HasValue`
indica se o resultado está inicializado; os overloads de `TryGetValue` extraem `Success<T>` ou `ErrorCollection`
sem boxing. A propriedade `Value` retorna o caso como `object?` e faz boxing a cada acesso a um resultado
inicializado; prefira `result switch`, `Unwrap()` ou `TryGetValue`. Converter o próprio resultado para `object`
ou uma interface também causa boxing. A criação e combinação de coleções de erros ainda podem alocar memória.
No compilador .NET 11 RC, a captura de casos por pattern matching pode ser limitada em código genérico;
nesses cenários, use os overloads tipados de `TryGetValue` ou `Match`.

`ResultModule` também oferece helpers estáticos: `Ok<T>` retorna `Success<T>`, `Of<T>` retorna `Result<T>` e
`Fail` retorna `ErrorCollection`, aceitando mensagem, um erro ou um array de erros. `Fail(string)` cria um erro
do tipo `Invalid`. `Error` permite informar chave, tipo e mensagem, com factories para `Failure`, `Invalid`,
`NotFound`, `Unauthorized` e `Forbidden`.

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

`ResultBuilder.Combine()` inicia um builder imutável para combinar de um a oito resultados de validações
independentes. Cada `Add` recebe um `Result<T>` e acrescenta um parâmetro tipado à função de `Build`, na ordem
de inclusão. Valores do mesmo tipo e sucessos com `null` são preservados. Os erros são acumulados em ordem,
incluindo duplicatas; `Build` executa a função apenas quando todos os resultados têm sucesso e retorna
`Result<TOut>`. A função deve ser não nula e suas exceções são propagadas; resultados `default` são rejeitados.

```csharp
Result<(string Name, int Age, string? Email)> person = ResultBuilder.Combine()
    .Add(nameResult)
    .Add(ageResult)
    .Add(Result<string?>.Ok(null))
    .Build((name, age, email) => (name, age, email));
```

As expressões passadas a cada `Add` são avaliadas mesmo quando um resultado anterior falha. Para mais de oito
valores, agrupe validações relacionadas em outros builders e combine seus resultados.

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

As operações de `Result<T>` são síncronas. Aguarde operações de I/O na application shell e use os resultados
com `Ensure`, `Map`, `Bind`, `Tap`, `Match`, `Recover` e `Combine`.

Para acumular erros de validações independentes, crie um `Result` para cada valor e use `Combine`,
como no exemplo acima. O encadeamento de `Ensure` interrompe as validações na primeira falha.
`ErrorCollection.Errors` é um `ImmutableArray<Error>`. Entradas mutáveis são copiadas; um `ImmutableArray<Error>`
fornecido diretamente ao constructor é reutilizado. O constructor para um único `Error` cria apenas o array
imutável. `ErrorCollection.Combine` preserva ordem e duplicatas e reutiliza a coleção existente quando a outra
está vazia. A igualdade e o hash consideram o conteúdo em ordem; `default(ErrorCollection)` equivale à coleção vazia.

`Error.Empty` equivale a `default(Error)`: chave `null`, tipo `Failure` e mensagem vazia. `Message` sempre retorna
uma string, inclusive no valor `default`. `IsEmpty()` reconhece um erro `Failure` sem chave nem mensagem, também
aceitando chave vazia. A igualdade e o hash de `Error` usam os valores expostos de chave, tipo e mensagem.

### Dados, consultas e persistência

- `IState` como marcador de estado e `IState<TKey>` para estados identificáveis.
- `Query<TState, TKey>` com paginação, ordenação e ponto de extensão para filtros via `IQueryable<TState>`.
- `Query` usa página inicial `1` e tamanho de página `10`. `Skip()` exige valores positivos e verifica overflow.
- `QueryResult<T>` com dados, página, tamanho de página, total de registros (`long`) e cálculo de `PageCount`.
- `IQueryStore<TState, TKey>` para consultas por id, lote, projeção e paginação, com retorno nullable nas consultas
  individuais e listas nos resultados paginados.
- `EFQueryStore<TDataContext, TState, TKey>` como implementação de consultas para Entity Framework Core.
- `IDataContext.ExecuteTransactionAsync` para executar um callback assíncrono em uma transação.
- `EFDbContextWrapper` como base de `DbContext` que implementa `IDataContext`: inicia a transação, executa o
  callback, chama `SaveChangesAsync` e faz o commit. Rejeita uma transação já ativa no contexto.
- Para gravação de estados, use as APIs do contexto EF; não há uma abstração genérica de escrita de estados.
- `GuidModule.Sequential()` para criação de GUID v7.

### Event sourcing e multi-tenancy

- `IAggregate` como marcador de agregado e `IAggregate<TState>` para expor seu estado atual.
- `IAggregateEvent<TState>` para expor snapshot, consultar e limpar eventos pendentes e reconstituir o estado com
  `Replay`.
- `AggregateEventBase<TState>` como base para agregados orientados a eventos. `Emit` aplica o evento e o adiciona
  à lista de pendentes; `Replay` aplica eventos sem adicioná-los à lista. A classe derivada implementa `Apply`.
  `Snapshot` expõe o estado atual, sem criar uma cópia.
- `IEvent` estende `IMessage` e expõe `Timestamp`.
- `IEventStore` para append e leitura de envelopes de eventos, com implementação `EFEventStore<TDataContext>`.
- `EventEnvelopeState` com dados de sequência, versão, tenant, agregado, ator, correlação, tipo do evento e payload
  JSON.
- `TenantId` e `IRequireTenantPartition` para contratos que exigem particionamento por tenant.
- `RequestContext.SetTenant` para definir o tenant e `GetTenant` para obtê-lo, lançando exceção quando não estiver
  definido. `WrapEvent` envelopa eventos com tenant, usuário, correlação e payload JSON; há overload com tipo de
  agregado explícito e overload genérico `WrapEvent<TEvent, TAggregate>`.

### Configuração dos stores

- `EFQueryStore` e as leituras de `EFEventStore` não rastreiam entidades. Para alterações, use o contexto EF
  diretamente.
- Na paginação, os stores aplicam `SortBy`/`OrderType` e desempate por `Id`. Sem `SortBy`, preservam a ordenação de
  `Query.Apply`, acrescentando `Id`; se não houver ordenação, usam `Id`.
- Os stores EF recebem apenas o contexto no construtor; `TDataContext` deve herdar de `DbContext` e implementar
  `IDataContext`.
- `TenantId` e `IRequireTenantPartition` descrevem o particionamento, mas os stores não aplicam filtros nem validam
  o tenant dos envelopes automaticamente. Em bancos compartilhados, configure o isolamento na aplicação ou no
  modelo EF, por exemplo com filtros globais de consulta, e valide o tenant nas gravações.
- Configure no modelo/migração EF o mapeamento de `EventEnvelopeState`, `TenantId` e `Payload`, além dos índices
  necessários. Um índice único por tenant, tipo de agregado, id do agregado e versão pressupõe um evento por versão.
  Se um comando produzir vários eventos, atribua uma versão diferente a cada evento ou defina outra chave única.
- `AppendAsync` não gera `Sequence`. A aplicação ou o mapeamento do banco deve fornecer uma sequência crescente e
  única no escopo consumido por `LoadFromSequenceAsync`. Um índice único de versão impede duplicatas, mas não valida
  a versão esperada nem impede lacunas; essa validação pertence ao fluxo de gravação e à transação.
- `EFEventStore.AppendAsync` adiciona o envelope e chama `SaveChangesAsync` imediatamente.
- `LoadAsync` lê os eventos por tipo e id do agregado, ordenados por versão e depois por sequência.
- `LoadFromSequenceAsync` inclui a sequência informada (`>=`) e ordena por sequência; o consumidor deve tratar a
  repetição do último evento.
- `IStateSnapshot.Version` indica o último evento incorporado; `SnapshotAt` é `DateTimeOffset` somente para leitura
  no contrato. O estado concreto também deve identificar o agregado e, quando aplicável, o tenant.

### Mensageria

- `IProducer` para publicação de mensagens.
- `IConsumer` para consumo de mensagens por fila com handler assíncrono.

### Metadados e campos customizados

- `FieldMetadata<TState>` e `IStateMetadata<TState>` para descrever campos de um estado.
- `FieldType` para tipos comuns de campo: identificador, texto, inteiro, decimal, booleano, data, data/hora,
  seleção única e múltipla.
- `CustomFieldMetadata`, `FieldId`, `IHaveCustomData` e `CustomData` para cenários com campos customizados por tenant e
  entidade.
- `MetadataModule.Field(...)` para criar metadados de campo de forma concisa.
- `IHaveCustomData.CustomData` expõe uma coleção `ImmutableArray<CustomData>`; cada entrada associa `FieldId` a um
  valor textual.

## Quando usar

Use `Funca.Abstractions` para separar regras de negócio puras de detalhes de infraestrutura, deixando resultados,
consultas, agregados, eventos, mensagens e contexto de requisição com contratos pequenos e consistentes.
