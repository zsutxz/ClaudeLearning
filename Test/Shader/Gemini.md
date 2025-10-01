# Gemini 指示

## 关于本项目
- 这是一个使用 Unity 进行游戏开发的 C# 项目，侧重于 Shader (着色器) 的编写与学习。
- 主要使用 C# 进行脚本编程，使用 ShaderLab/HLSL 语言编写着色器。

## 编码规范
- **C# 脚本:**
  - 遵循 Microsoft C# 命名规范 (例如，类名和方法名使用 `PascalCase`，局部变量和参数使用 `camelCase`)。
  - 在关键逻辑或复杂算法处添加必要的注释。
  - 优先使用 Unity 的 API，例如 `Vector3.up` 而不是 `new Vector3(0, 1, 0)`。
- **Shader (着色器):**
  - 变量名 (Uniforms) 使用下划线开头，例如 `_MainTex`, `_Color`。
  - 保持 ShaderLab 结构清晰，将 `Properties`, `SubShader`, `Pass` 等模块组织好。
  - 对复杂的 HLSL/CG 代码块添加注释，解释算法意图。
- **文件命名:**
  - C# 脚本文件名应与主类名完全一致。
  - Shader 文件使用 `.shader` 后缀，并shi'yshiy