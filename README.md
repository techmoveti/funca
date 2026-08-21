# 🚀 Funca

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)

Biblioteca C# de abstrações para estruturar aplicações com responsabilidades explícitas e fluxos de negócio previsíveis. Ela oferece padrões e tipos reutilizáveis para separar regras de negócio, efeitos colaterais e infraestrutura, facilitando código mais testável, legível e desacoplado.

## ✨ Resumo

A Funca incentiva a **separação de conceitos** ao organizar contratos e utilitários em módulos como Containers, Data, Messaging e Shell. Seus tipos funcionais, como `Result<T>` e `Option<T>`, tornam sucesso, falha e ausência de valor explícitos, permitindo compor operações sem depender de exceções, verificações de `null` ou condicionais aninhadas.

Esse modelo apoia o padrão **Functional Core, Imperative Shell**: mantenha as regras de negócio no núcleo funcional, determinístico e fácil de testar; concentre I/O, persistência, mensageria e demais efeitos no shell imperativo. As abstrações da biblioteca ajudam a definir as fronteiras entre esses conceitos e a conectar as camadas sem acoplá-las a implementações concretas.

## 📋 Features

### 🏗️ Clean Architecture
- **Separation of Concerns**: Well-defined abstractions to keep code organized and testable
- **Modular Structure**: Organization by domains (Containers, Data, Messaging, Shell)
- **SOLID Principles**: Implementation of recommended interfaces and patterns

### 🛤️ Railway Pattern
Elegant error handling using the Railway pattern (Success/Failure Track):
- Chaining operations that may fail
- Avoids nested error checks
- Keeps code clean and readable

### 🎁 Option Pattern
Work with optional values safely:
- `Some<T>` / `None<T>` to represent presence/absence of values
- Chained operations without null checks
- Prevents `NullReferenceException`

## 📦 Project Structure

```
src/
├── Funca.Abstractions/     # Main abstractions and interfaces
│   ├── Containers/         # Dependency injection
│   ├── Data/              # Data abstractions
│   ├── Messaging/         # Messaging patterns
│   └── Shell/             # Helper utilities
└── [Implementation projects]
```

## 🚀 Quick Start

### Installation
```bash
dotnet add package Funca
```

### Railway Pattern Example
```csharp
using Funca.Abstractions;

var result = Operation1()
    .Bind(output => Operation2(output))
    .Bind(output => Operation3(output))
    .Match(
        success => HandleSuccess(success),
        failure => HandleFailure(failure)
    );
```

### Option Pattern Example
```csharp
using Funca.Abstractions;

var value = GetOptionalValue()
    .Map(v => TransformValue(v))
    .FlatMap(v => GetAnotherOption(v))
    .Match(
        some => ProcessValue(some),
        () => HandleNone()
    );
```

## 🧩 Modules

### Containers
Abstractions for dependency injection and lifecycle management.

### Data
Interfaces and patterns for data access layer and persistence.

### Messaging
Support for communication patterns and events.

### Shell
Utilities and helper extensions to facilitate library usage.

## 📊 Benchmarks

The `benchmarks/Funca.Benchmarks` project uses [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure throughput and allocations for key pipelines.

### Scenarios covered

| Benchmark | Description |
|---|---|
| `SuccessChain_MapBindEnsureMatch` | Full sync success chain (Map → Bind → Ensure → Match) |
| `FailureChain_EarlyExit` | Sync chain with early failure — all combinators bypassed |
| `NullableValueSuccess_IsOk` | `Result<string?>.Ok(null)` — verifies `_isSuccess` flag independence from nullability |
| `TaskChain_Success` | Async Task success pipeline (MapAsync → BindAsync → Match) |
| `ValueTaskChain_Success` | Async ValueTask success pipeline |
| `TaskChain_EarlyFailure` | Async Task pipeline with early failure |

### Running the benchmarks

```bash
# From the repository root
cd benchmarks/Funca.Benchmarks
dotnet run -c Release
```

To run a specific benchmark class:

```bash
dotnet run -c Release -- --filter *ResultSync*
dotnet run -c Release -- --filter *ResultAsync*
```

Results are written to `BenchmarkDotNet.Artifacts/` in the benchmark project folder.

---

## 📝 License
MIT License - see [LICENSE](LICENSE) for more details.

## 🤝 Contributing
Contributions are welcome! Please:
1. Fork the project
2. Create a branch for your feature (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📧 Support
For questions or suggestions, open an [Issue](https://github.com/techmoveti/funca/issues) on the repository.

---

**Developed by [TechMove](https://github.com/techmoveti)**
