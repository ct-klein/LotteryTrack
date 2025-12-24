# LotteryTracker - Project Guidelines

## Project Overview
LotteryTracker is a Windows desktop application built with C# 13 and .NET 9 using WinUI 3 for tracking lottery numbers, results, and statistics.

## Technology Stack
- **Framework**: .NET 9
- **Language**: C# 13
- **UI Framework**: WinUI 3 (Windows App SDK)
- **Architecture**: MVVM (Model-View-ViewModel)
- **Minimum OS**: Windows 10 version 1809 (build 17763)

## Project Structure
```
LotteryTracker/
├── src/
│   ├── LotteryTracker.App/           # WinUI 3 application (entry point, views, view models)
│   ├── LotteryTracker.Core/          # Domain models, interfaces, business logic
│   └── LotteryTracker.Infrastructure/ # Data access, external services
├── tests/
│   ├── LotteryTracker.Core.Tests/
│   └── LotteryTracker.Infrastructure.Tests/
├── .editorconfig                      # Code style configuration
├── CLAUDE.md                          # Project guidelines
└── LotteryTracker.sln
```

## C# Best Practices

### Code Style
- Use file-scoped namespaces
- Use primary constructors where appropriate
- Prefer `record` types for immutable data transfer objects
- Use `required` modifier for mandatory properties
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use collection expressions (`[1, 2, 3]` syntax)
- Prefer pattern matching over type checking

### Naming Conventions
- **PascalCase**: Classes, methods, properties, public fields
- **camelCase**: Local variables, parameters
- **_camelCase**: Private fields
- **IPascalCase**: Interfaces (prefix with I)
- **TPascalCase**: Generic type parameters (prefix with T)

### MVVM Pattern
- ViewModels should implement `INotifyPropertyChanged` (use CommunityToolkit.Mvvm)
- Use `[ObservableProperty]` attribute for bindable properties
- Use `[RelayCommand]` attribute for commands
- Views (XAML) should have minimal code-behind
- Use dependency injection for ViewModel dependencies
- Use `CommunityToolkit.WinUI` for WinUI-specific MVVM helpers

### WinUI 3 Specific
- Use `x:Bind` for compiled bindings (preferred over `Binding`)
- Use `Microsoft.UI.Xaml` namespace (not `Windows.UI.Xaml`)
- Leverage WinUI 3 controls: NavigationView, InfoBar, ProgressRing, etc.
- Use `DispatcherQueue` for UI thread marshalling
- Support both light and dark themes via `RequestedTheme`

## Build Commands
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Build in Release mode
dotnet build -c Release

# Run application
dotnet run --project src/LotteryTracker.App

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Publish (packaged)
dotnet publish src/LotteryTracker.App -c Release

# Format code
dotnet format
```

## Dependencies (Recommended)
- **CommunityToolkit.Mvvm**: MVVM helpers and source generators
- **CommunityToolkit.WinUI.UI**: WinUI 3 UI helpers and controls
- **Microsoft.Extensions.DependencyInjection**: Dependency injection
- **Microsoft.Extensions.Hosting**: Generic host for app lifecycle
- **Microsoft.EntityFrameworkCore.Sqlite**: Local database storage
- **System.Text.Json**: JSON serialization (built-in)

## Code Quality
- Enable `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in csproj
- Enable `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`
- Use `.editorconfig` for consistent formatting (included in project)
- Run `dotnet format` before committing
- Enable `<Nullable>enable</Nullable>` for nullable reference types
- Enable `<ImplicitUsings>enable</ImplicitUsings>` for cleaner code

## Testing
- Use xUnit for unit tests
- Use Moq or NSubstitute for mocking
- Follow AAA pattern (Arrange, Act, Assert)
- Name tests: `MethodName_StateUnderTest_ExpectedBehavior`
- Test ViewModels independently from Views

## Git Practices
- Commit messages: Use imperative mood ("Add feature" not "Added feature")
- Branch naming: `feature/`, `bugfix/`, `hotfix/` prefixes
- Keep commits focused and atomic

## Deployment
WinUI 3 apps can be deployed as:
- **MSIX packaged**: For Microsoft Store or enterprise deployment
- **Unpackaged**: Traditional exe deployment (requires additional configuration)
