# Process Manager Application

## Challenge Overview

This project implements a Windows-based Process Manager application that allows users to monitor system processes, modify their priorities, and view system information through both a console interface and a web interface.

### Core Requirements:

- List all running Windows processes with detailed information (name, PID, memory usage, CPU time)
- Allow process priority modification (boosting to High)
- Log operations with timestamps
- Implement a lightweight HTTP server for web-based monitoring
- Windows platform compatibility
- Use only built-in .NET Framework libraries (no external dependencies)

## Design Patterns & Architecture Decisions

### Object-Oriented Principles

1. **Single Responsibility Principle (SRP)**
   - Each class has a single, well-defined purpose
   - ProcessService handles only process operations
   - LogService manages only logging functionality
   - HttpServer manages only web communication

2. **Interface Segregation**
   - Clean interfaces with specific purposes (IProcessService, ILogService, IHttpServer)
   - No bloated interfaces with unrelated methods

3. **Dependency Injection**
   - Services receive dependencies through constructors
   - Improves testability and decouples implementation details

4. **Separation of Concerns**
   - Clear boundaries between UI, business logic, and data
   - Web interface separate from console interface
   - Process management separate from logging

### Architecture Overview

The solution follows a layered architecture:

- **Core Layer**: Contains domain models and interfaces
  - Models: ProcessInfo
  - Interfaces: IProcessService, ILogService, IHttpServer, IUserInterface

- **Services Layer**: Implements core interfaces
  - WindowsProcessService: Process management
  - FileLogService: File-based logging

- **UI Layer**: User interface implementations
  - ConsoleUserInterface: Command-line interface
  - Web UI: Browser-based interface

- **Web Layer**: HTTP server implementation
  - SimpleHttpServer: Built using HttpListener

### Manual JSON Implementation

Since external libraries couldn't be used, custom JSON serialization/deserialization was implemented:

- Simple JSON string building for API responses
- Manual parsing for request data
- Property name standardization for JavaScript compatibility

### Polyfill Implementation for Modern C# Features

To maintain modern C# code practices while targeting .NET Framework, a "Polyfill" folder was created to implement newer language features:

- **Index and Range Operators**: Implementation of C# 8.0's Index and Range features
  - Enables concise array slicing syntax (e.g., `array[..30]` instead of `array.Take(30)`)
  - Custom implementation of the `Index` struct to support both from-start and from-end indexing
  - Custom implementation of the `Range` struct to support slicing operations
  - Extension methods for common collection types to support these new operators
  - Compatibility layer that maintains the same behavior as native .NET Core implementations

This polyfill allows for more readable, modern C# code while maintaining compatibility with the .NET Framework runtime. The implementation follows the same patterns and behaviors as the official .NET Core implementation, ensuring code portability between the platforms.

## Proactive Enhancements

Several enhancements were added beyond the basic requirements:

1. **Enhanced Web UI**
   - Tabbed interface (Processes, Logs, Actions)
   - Real-time data updates with configurable refresh intervals
   - Process filtering and sorting capabilities
   - Direct process management from the web interface
   - Log viewing and management within the browser

2. **Improved Error Handling**
   - Comprehensive exception management
   - User-friendly error messages
   - Detailed logging of errors and operations
   - Graceful degradation when operations fail

3. **Thread Safety**
   - Thread-safe logging implementation
   - Proper async/await pattern for non-blocking operations
   - Concurrent request handling in the HTTP server

4. **Resource Management**
   - Proper disposal of Process objects
   - Clean shutdown of HTTP server
   - Memory-efficient process filtering

5. **Security Considerations**
   - Input validation for all user inputs
   - Protection against basic input attacks
   - Limited API surface to prevent misuse

## Technical Implementation Details

### Process Management

The Windows Process API is accessed through System.Diagnostics.Process to:

- Retrieve process information (WorkingSet64 for memory usage)
- Modify process priorities (requires admin privileges for some processes)
- Handle access denied scenarios (some system processes are protected)

### HTTP Server Implementation

A lightweight HTTP server is implemented using HttpListener:

- Routes: GET / (main page), GET /api/processes, GET /api/logs, POST /api/boost, POST /api/log-top-processes
- Manual JSON response creation (without JSON libraries)
- Asynchronous request handling
- Basic MIME type support (HTML, JSON, plain text)

### Logging System

File-based logging with:

- Timestamp prefixes
- Different log levels (INFO, ERROR)
- Thread-safe write operations
- Operation recording for auditing

### .NET Framework Compatibility

The application is built using .NET Framework to ensure maximum compatibility with Windows systems without requiring newer .NET Core/5/6 runtimes:

- Compatible with Windows 7 and newer
- Uses only built-in Framework libraries
- No external dependencies
- Polyfills for modern C# features not available in .NET Framework

## Setup and Usage

### Requirements

- Windows operating system (Windows 7 or newer)
- .NET Framework 4.5 or higher
- Administrator privileges (for some process priority modification)

### Console Interface

1. View the list of running processes with details
2. Enter a PID to boost its priority to High
3. Process information is automatically refreshed on each cycle

### Web Interface

1. Access http://localhost:8080/ in any browser
2. Use the Processes tab to view running processes
3. Click "Boost" next to any process to elevate its priority
4. View the log file in the Logs tab
5. Use the Actions tab for additional operations

## Known Limitations

- Requires administrator privileges to modify some process priorities
- Some system processes cannot be accessed due to Windows security restrictions
- Performance may degrade with large number of processes

## Future Improvements

- Process filtering by name/resource usage
- Process grouping by application
- Historical usage graphs
- Advanced process control (start/stop/restart)
- Process dependency visualization
- Service control integration
- System performance metrics