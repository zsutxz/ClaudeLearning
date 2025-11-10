---
name: translate-it-article
description: Professional IT technical article translation tool that translates English technical documentation, blog posts, and academic papers into easy-to-understand, engaging Simplified Chinese while preserving complete formatting and saving locally.
license: MIT
allowed-tools: [WebFetch, Read, Write, mcp__filesystem__write_file, mcp__filesystem__read_file]
---

## Usage
When users need to translate IT technical articles, use the following calling methods:
```
/translate-it-article [English article URL]
or
/translate-it-article [Paste English article content directly]
or
/翻译it文章 [英文文章URL]
or
/翻译it文章 [直接粘贴英文文章内容]
```

## Skill Trigger Timing
Use this skill when users make the following requests:
- "translate this technical article"
- "help me translate this IT documentation"
- "translate this English technical blog"
- "convert this technical paper to Chinese"
- "翻译这篇技术文章"
- "把这段英文翻译成中文"
- "翻译一下这个IT文档"
- "帮我翻译技术博客"
- Similar translation requests

## Features
Input English technical article URL or directly paste English article content, the system will automatically:
1. **Smart Translation** - Maintain technical accuracy while using easy-to-understand language
2. **Format Preservation** - Completely preserve original Markdown formatting, images, links, and other elements
3. **Terminology Standardization** - Use standard translations for professional terms, with English original noted on first appearance
4. **Local Saving** - Automatically save translation results as local Markdown files

## Translation Principles

### 🎯 Target Audience
- General readers interested in IT technology
- Non-native English speaking technical practitioners
- Developers who need to quickly understand foreign technical information

### ✨ Translation Style
- **Storytelling Narrative** - Explain technical concepts like telling stories, avoiding dry academic style
- **Colloquial Expression** - Use natural Chinese word order, avoiding translation-ese
- **Clear Logic** - Maintain complete logical chain of original text, clear organization

### 🔍 Technical Accuracy
- **Factual Consistency** - Core data, technical principles, logical relationships must be completely consistent with original text
- **Standard Terminology** - Use industry-recognized Chinese technical term translations
  - First appearance: `人工智能 (Artificial Intelligence)`
  - Subsequent appearances: `AI` or `人工智能`
- **Code Integrity** - Code blocks, command lines, API calls remain untranslated

### 📝 Format Handling
- **Title Hierarchy** - Maintain H1/H2/H3 title hierarchy structure
- **Emphasis Formatting** - Preserve bold, italic, strikethrough and other emphasis formats
- **List Structure** - Maintain ordered and unordered list structures
- **Link References** - Preserve hyperlink and reference link formats
- **Image Captions** - Translate image alt text, preserve image links

### 🌐 Cultural Adaptation
- **Technical Background** - Provide appropriate explanations for Western-specific technical backgrounds or cultural phenomena
- **Case Examples** - Replace unfamiliar foreign cases with more easily understood Chinese equivalents
- **Annotation Standards** - Use `**（annotation content）**` format for content requiring additional explanation

### 📋 Processing Workflow
1. **Content Analysis** - Identify article type (technical blog/academic paper/news article)
2. **Terminology Extraction** - Extract professional terms, establish translation comparison table
3. **Structural Translation** - Translate by paragraph and title structure
4. **Format Check** - Ensure Markdown format is completely intact
5. **Quality Proofreading** - Check technical accuracy and expression fluency
6. **Local Saving** - Save as `[original filename]_zh.md`

## Output Format

### File Naming
- URL articles: `[website name]_[article title]_zh.md`
- Direct paste: `translated article_[date time]_zh.md`

### File Structure
```markdown
# [Translated article title]

**Original Link**: [Original URL]
**Translation Time**: [Translation date]
**Article Type**: [Technical blog/Academic paper/News article]

---

[Translated article content, maintaining complete format]
```

## Quality Assurance

### ✅ Accuracy Check
- [ ] Technical terms translated correctly
- [ ] Data and facts consistent with original text
- [ ] Logical relationships maintained
- [ ] Code and commands not incorrectly translated

### ✅ Readability Check
- [ ] Chinese expression natural and fluent
- [ ] Long sentences appropriately split
- [ ] Paragraph structure clear
- [ ] No obvious translation-ese

### ✅ Format Check
- [ ] Markdown format complete
- [ ] Image links working
- [ ] Code block format correct
- [ ] Lists and title hierarchy correct

## Example Comparison

### Original
```
## Machine Learning in Production

Deploying ML models at scale requires careful consideration of latency, throughput, and monitoring.
```

### Translated
```
## 生产环境中的机器学习应用

大规模部署机器学习模型时，需要仔细考虑延迟、吞吐量和监控等关键因素。
```

---

*💡 Tip: This skill focuses on high-quality translation of technical articles, ensuring technical accuracy while enhancing the reading experience for Chinese readers.*