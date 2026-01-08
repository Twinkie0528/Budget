# Contributing to Budget Platform

Thank you for your interest in contributing to Budget Platform!

## Development Setup

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- Docker Desktop
- PowerShell (for helper scripts)

### Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_ORG/budget-platform.git
   cd budget-platform
   ```

2. Set up Git hooks:
   ```powershell
   .\tools\git\setup-hooks.ps1
   ```

3. Start PostgreSQL:
   ```powershell
   docker compose -f docker/compose.dev.yml up -d
   ```

4. Run the API:
   ```powershell
   cd src/Budget.Api
   dotnet run
   ```

5. Run the Web UI:
   ```powershell
   cd src/web
   npm install
   npm run dev
   ```

## Branch Naming

- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `hotfix/description` - Production hotfixes

Use the helper script:
```powershell
.\tools\git\new-feature.ps1 -Name "add export filter" -Type feature
```

## Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Formatting (no code change)
- `refactor`: Code restructuring
- `test`: Adding tests
- `chore`: Maintenance

Examples:
```
feat(import): add xlsx validation for required columns
fix(api): handle null values in export
docs: update API endpoint documentation
```

## Pull Requests

1. Create a feature branch from `develop`
2. Make your changes
3. Run PR readiness check:
   ```powershell
   .\tools\git\pr-ready.ps1
   ```
4. Push and create PR
5. Fill out the PR template
6. Request review

### PR Requirements

- [ ] All CI checks pass
- [ ] Code follows project style
- [ ] Tests added for new functionality
- [ ] Documentation updated if needed
- [ ] No merge conflicts

## Testing

### Run Unit Tests
```powershell
dotnet test tests/Budget.Tests.Unit
```

### Run Integration Tests
```powershell
dotnet test tests/Budget.Tests.Integration
```

### Run Frontend Tests
```powershell
cd src/web
npm run lint
npm run build
```

## Code Style

### C# / .NET
- Follow Microsoft C# coding conventions
- Use `dotnet format` to auto-format
- Enable nullable reference types

### TypeScript / React
- ESLint configuration in `src/web/eslint.config.js`
- Use functional components with hooks
- Prefer TypeScript strict mode

## Release Process

1. Ensure all changes are merged to `main`
2. Run the release script:
   ```powershell
   .\tools\git\create-release.ps1 -Version "1.0.0"
   ```
3. The CD workflow automatically:
   - Builds and tests
   - Deploys to production
   - Packages Teams app

## Questions?

- Create an issue for bugs or feature requests
- Use discussions for questions
- Tag maintainers for urgent items

