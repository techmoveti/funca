# 🚀 Funca

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)

C# library with resources for **separation of concerns** in software, implementing **Railway Pattern** and **Option Pattern** for robust error handling and business flows.

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
