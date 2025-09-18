# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### 🔄 Project Awareness & Context
- **Check `PRPs/` directory** before starting a new task. PRPs contain comprehensive implementation guides for features.
- **Use consistent naming conventions, file structure, and architecture patterns** as described in existing PRPs.
- **Always read the specific PRP file** related to the feature you're implementing.



### 🧱 Code Structure & Modularity
- **Organize code into clearly separated modules**, grouped by feature or responsibility.
- **Use clear, consistent imports** and follow existing patterns in the codebase.
- **For HTML/JavaScript projects**: Keep all code in a single HTML file when specified, or organize into separate CSS/JS files for larger projects.

### 🧪 Testing & Reliability
- **Test HTML/JavaScript features** in multiple browsers to ensure compatibility.
- **Verify functionality** matches the requirements specified in the PRP.
- **Check for cross-browser compatibility** and responsive design.


### ✅ Task Completion
- **Mark completed tasks in the PRP file** immediately after finishing them.
- Add new sub-tasks or TODOs discovered during development to the PRP under a "Discovered During Work" section.

### 📎 Style & Conventions
- **Follow existing code patterns** in the codebase.
- Write **comments for complex logic** explaining the why, not just the what.

### 📚 Documentation & Explainability
- **Comment non-obvious code** and ensure everything is understandable to a mid-level developer.
- When writing complex logic, **add inline comments** explaining the why, not just the what.

### 🧠 AI Behavior Rules
- **Never assume missing context. Ask questions if uncertain.**
- **Never hallucinate libraries or functions** – only use known, verified packages.
- **Always confirm file paths and module names** exist before referencing them in code or tests.
- **Never delete or overwrite existing code** unless explicitly instructed to or if part of a task from the PRP.

### ⚙️ Development Commands
- **Testing TypeScript files**: Use jest for running tests
- **For development**: Use browser developer tools for debugging

### 🏗️ Project Architecture
This repository follows a PRP (Problem Resolution Protocol) driven architecture where functionality is implemented based on comprehensive PRP templates. The project uses PRP templates for implementing new features, which can be found in the `PRPs/templates/` directory.

The project is a TypeScript library for fetching financial share data from multiple providers including Alpha Vantage, Yahoo Finance, and IEX Cloud.

The project structure includes:
- `PRPs/` - Contains PRP templates and documentation for feature implementation
- `examples/` - Example code and implementations
- `tools/` - Utility tools and scripts
- `.claude/` - Claude Code configuration and settings
- `src/` - Source code directory with components
- `tests/` - Test files for the components

### 🔧 Common Development Tasks
1. **Implementing new features**: Use the PRP template from `PRPs/templates/prp_base.md` as a guide
2. **Testing**: Run tests with jest using `npm test` or the equivalent command
3. **Code quality**: Follow existing patterns and maintain consistent styling
4. **Documentation**: Update relevant documentation files when making changes

### 📦 Project Dependencies and Structure
- This is a TypeScript library with a modular structure
- Core components are in `src/components/`:
  - `shareDataFetcher.ts`: Main class for fetching share data
  - `shareDataTypes.ts`: Type definitions and interfaces
  - `shareDataCache.ts`: Cache implementation for fetched data
- Tests are in the `tests/` directory and use jest
- The library supports multiple financial data providers through a provider pattern

### 🧪 Testing Approach
- Tests are written using jest
- Test files follow the naming pattern `*.test.ts`
- Tests cover:
  - Successful data fetching scenarios
  - Error handling
  - Cache functionality
- Mocking is used for API calls to ensure tests are isolated and fast
- 测试生成的文档保存到examples/doc,代码存储在examples/src