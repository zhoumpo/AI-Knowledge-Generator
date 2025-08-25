# AI Knowledge Generator

A WPF desktop application built with .NET 8 that helps generate and manage AI-related knowledge content.

## Features

- Modern WPF interface with MVVM pattern
- File aggregation functionality
- User settings management
- Exception handling for file operations

## Technology Stack

- **.NET 8.0** - Target framework
- **WPF (Windows Presentation Foundation)** - UI framework
- **C#** - Primary programming language
- **MVVM Pattern** - Architecture pattern

## Project Structure

```
├── Commands/          # RelayCommand implementations
├── Converters/        # Value converters for UI
├── Exceptions/        # Custom exception classes
├── Models/            # Data models and settings
├── Properties/        # Assembly information
├── Resources/         # Icons and resources
├── Services/          # Business logic services
├── Utils/             # Utility classes
├── ViewModels/        # MVVM view models
└── Views/             # WPF user controls and windows
```

## Getting Started

### Prerequisites

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code

### Building the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/zhoumpo/AI-Knowledge-Generator.git
   ```

2. Navigate to the project directory:
   ```bash
   cd AI-Knowledge-Generator
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

## Contributing

Feel free to submit issues and enhancement requests!

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
