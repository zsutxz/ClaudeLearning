# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### 🔄 Project Awareness & Context
- This repository is a **Claude Code Skills Marketplace** containing specialized skills for various tasks
- It serves as a plugin marketplace for Claude Code with diverse capabilities ranging from creative arts to technical development
- The repository includes both example skills and document processing skills developed by Anthropic
- Skills are organized as self-contained directories with `SKILL.md` files containing instructions and metadata

### 🧱 Code Structure & Modularity
- Skills are organized in the `.claude/skills/` directory with each skill in its own folder
- Commands are available in `.claude/commands/` for custom slash commands
- Each skill follows a standard structure: `SKILL.md` with YAML frontmatter and instructional content
- Skills are dynamically loaded by Claude based on task requirements and user requests

### ⚙️ Development Commands
- **Plugin Management**:
  - `/plugin marketplace add anthropics/skills` - Register as Claude Code plugin marketplace
  - `/plugin install document-skills@anthropic-agent-skills` - Install document processing skills
  - `/plugin install example-skills@anthropic-agent-skills` - Install example skills
- **Custom Commands**:
  - `/toppt` - Convert text to PowerPoint presentations using specialized prompting
  - `/generate-prp` - Generate project requirement documents
  - `/execute-prp` - Execute project requirement documents
  - `/pr-ready` - Prepare pull requests
- **MCP Server**:
  - Context7 MCP server configured via `.mcp.json` for enhanced context management
  - Uses `@upstash/context7-mcp` package for context services
- **Skill Usage**:
  - Skills are automatically activated based on task context
  - Mention skill names directly (e.g., "Use the PDF skill to extract text")
  - Skills can be manually activated via the Skill tool

### 🏗️ Project Architecture
This repository is organized as a skills marketplace with the following structure:

**Core Directories:**
- `.claude/` - Claude Code configuration and custom components
  - `skills/` - Individual skill directories with specialized capabilities
  - `commands/` - Custom slash commands for enhanced functionality
- `.claude-plugin/` - Plugin-specific configurations and resources

**Available Skills Categories:**
- **Creative & Design**: algorithmic-art, canvas-design, slack-gif-creator, theme-factory
- **Document Processing**: docx, pdf, xlsx (document creation and manipulation)
- **Development & Technical**: artifacts-builder, mcp-builder, webapp-testing
- **Enterprise & Communication**: brand-guidelines, internal-comms
- **Meta Skills**: skill-creator, template-skill

### 🎯 Skill Development Guidelines
- Each skill requires a `SKILL.md` file with YAML frontmatter (`name`, `description`)
- Skills should be self-contained and focused on specific capabilities
- Use the template-skill as a starting point for new custom skills
- Test skills thoroughly before deployment in production environments

### 📚 Available Capabilities
**Document Skills** (Anthropic-developed):
- **docx**: Create, edit, and analyze Word documents with tracked changes and comments
- **pdf**: Extract text, create PDFs, merge/split documents, handle forms
- **xlsx**: Create and manipulate Excel spreadsheets with formulas and formatting

**Example Skills**:
- **algorithmic-art**: Generative art using p5.js with flow fields and particle systems
- **artifacts-builder**: Complex HTML artifacts using React and Tailwind CSS
- **webapp-testing**: Test local web applications using Playwright
- **brand-guidelines**: Apply consistent branding to artifacts
- **internal-comms**: Generate status reports, newsletters, and FAQs

### 🧠 AI Behavior Rules
- Never assume missing context. Ask questions if uncertain.
- Never hallucinate libraries or functions – only use known, verified packages.
- Always confirm file paths and module names exist before referencing them.
- Never delete or overwrite existing code unless explicitly instructed.
- Skills are tools - use them appropriately for their intended purpose

### 🔧 Configuration Files
- `.mcp.json` - MCP server configuration (Context7 integration)
- `CLAUDE.local.md` - Local Claude configuration overrides
- `.claude/settings.json` - Claude Code settings and preferences

### 📖 Documentation
- Comprehensive documentation available in `.claude/skills/README.md`
- Each skill includes its own documentation and examples
- Refer to Anthropic's skills documentation for best practices
- Skills are open source (Apache 2.0) except document skills (source-available)