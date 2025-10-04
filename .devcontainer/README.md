# Buenaventura Dev Container Configuration

This directory contains the configuration for GitHub Codespaces and VS Code Dev Containers to ensure a consistent development environment.

## Files

- `devcontainer.json` - Main configuration file that sets up:
  - .NET 9.0 SDK
  - Required VS Code extensions
  - Port forwarding for the web application
  - Automatic package restore after container creation

## Usage

When you create a new GitHub Codespace or open this repository in VS Code with the Dev Containers extension, it will automatically:

1. Create a container with .NET 9.0 SDK
2. Install the required VS Code extensions
3. Restore NuGet packages
4. Forward ports 5000 and 5001 for the web application

## Building the Project

After the container is created, you can build the project with:

```bash
dotnet build
```

## Running the Application

To run the application:

```bash
dotnet run --project Buenaventura
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.
