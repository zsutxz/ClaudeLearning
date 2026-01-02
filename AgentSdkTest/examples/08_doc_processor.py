#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
文档处理 MCP 服务器使用示例

展示如何使用文档处理 MCP 服务器的各种工具。
"""

import sys
import asyncio
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

from claude_agent_sdk import ClaudeSDKClient, ClaudeAgentOptions
from lib.config import get_config
from lib.utils import print_example_header


def print_separator(char="=", length=60):
    """打印分隔线"""
    print(char * length)


async def example_1_read_document():
    """示例1：读取文档"""
    print_example_header("示例1：读取文档")

    # 导入文档处理器
    from mcp_servers.doc_processor_server import get_processor

    # 读取 README.md
    file_path = str(project_root / "README.md")
    processor = get_processor(file_path)

    result = processor.read(file_path)

    if result.get("success"):
        print(f"✓ 成功读取文档: {file_path}")
        print(f"  格式: {processor.format_name}")
        print(f"  内容预览（前200字）:")
        print(f"  {result['content'][:200]}...")
        print(f"  总长度: {len(result['content'])} 字符")
    else:
        print(f"✗ 读取失败: {result.get('error')}")

    print()


async def example_2_extract_metadata():
    """示例2：提取元数据"""
    print_example_header("示例2：提取文档元数据")

    from mcp_servers.doc_processor_server import get_processor

    file_path = str(project_root / "README.md")
    processor = get_processor(file_path)

    metadata = processor.get_metadata(file_path)

    if "error" not in metadata:
        print(f"✓ 成功提取元数据: {file_path}")
        for key, value in metadata.items():
            print(f"  {key}: {value}")
    else:
        print(f"✗ 提取失败: {metadata['error']}")

    print()


async def example_3_search_in_document():
    """示例3：在文档中搜索"""
    print_example_header("示例3：在文档中搜索")

    from mcp_servers.doc_processor_server import get_processor

    file_path = str(project_root / "README.md")
    processor = get_processor(file_path)

    # 读取文档
    result = processor.read(file_path)
    if not result.get("success"):
        print(f"✗ 读取失败")
        return

    content = result["content"]
    query = "Claude"

    # 搜索
    lines = content.split('\n')
    matches = []

    for i, line in enumerate(lines):
        if query.lower() in line.lower():
            matches.append({
                "line_number": i + 1,
                "line": line.strip()
            })

    print(f"✓ 在文档中搜索 '{query}': {file_path}")
    print(f"  找到 {len(matches)} 处匹配:")
    for match in matches[:5]:  # 只显示前5个
        print(f"    行 {match['line_number']}: {match['line'][:80]}...")

    if len(matches) > 5:
        print(f"    ... 还有 {len(matches) - 5} 个匹配")

    print()


async def example_4_batch_process():
    """示例4：批量处理"""
    print_example_header("示例4：批量处理文档")

    from mcp_servers.doc_processor_server import get_processor

    # 查找所有 Markdown 文件
    md_files = list(project_root.glob("*.md"))

    print(f"✓ 找到 {len(md_files)} 个 Markdown 文件")
    print()

    results = []
    for file_path in md_files[:5]:  # 只处理前5个
        processor = get_processor(str(file_path))
        result = processor.read(str(file_path))

        results.append({
            "file": file_path.name,
            "success": result.get("success", False),
            "size": len(result.get("content", 0))
        })

    print("批量处理结果:")
    for result in results:
        status = "✓" if result["success"] else "✗"
        print(f"  {status} {result['file']}: {result['size']} 字符")

    print()


async def example_5_get_document_stats():
    """示例5：获取文档统计"""
    print_example_header("示例5：获取文档统计信息")

    from mcp_servers.doc_processor_server import get_processor

    file_path = str(project_root / "README.md")
    processor = get_processor(file_path)

    result = processor.read(file_path)
    if not result.get("success"):
        print(f"✗ 读取失败")
        return

    content = result["content"]

    # 统计信息
    stats = {
        "字符数": len(content),
        "行数": len(content.split('\n')),
        "段落数": len([p for p in content.split('\n\n') if p.strip()]),
    }

    import re
    chinese_chars = len(re.findall(r'[\u4e00-\u9fff]', content))
    english_words = len(re.findall(r'\b[a-zA-Z]+\b', content))
    stats["中文字符"] = chinese_chars
    stats["英文单词"] = english_words
    stats["总字数"] = chinese_chars + english_words

    # 阅读时间
    read_time = (chinese_chars / 400) + (english_words / 200)
    stats["估算阅读时间(分钟)"] = round(read_time, 2)

    print(f"✓ 文档统计: {Path(file_path).name}")
    for key, value in stats.items():
        print(f"  {key}: {value}")

    print()


async def example_6_mcp_integration():
    """示例6：MCP 服务器集成"""
    print_example_header("示例6：MCP 服务器集成")

    print("✓ 文档处理 MCP 服务器已创建")
    print()
    print("支持的工具:")
    tools = [
        "read_document - 读取文档内容",
        "extract_metadata - 提取元数据",
        "search_in_document - 文档搜索",
        "batch_process - 批量处理",
        "get_supported_formats - 获取支持格式",
        "validate_document - 验证文档",
        "convert_to_markdown - 转换为Markdown",
        "get_document_stats - 获取统计信息",
    ]

    for tool in tools:
        print(f"  • {tool}")

    print()
    print("配置文件:")
    print("  .mcp.json - MCP 服务器配置")
    print("  .claude/settings.local.json - 权限配置")
    print()


async def main():
    """主函数"""
    print("=" * 60)
    print("文档处理 MCP 服务器使用示例")
    print("=" * 60)
    print()

    # 运行所有示例
    await example_1_read_document()
    await example_2_extract_metadata()
    await example_3_search_in_document()
    await example_4_batch_process()
    await example_5_get_document_stats()
    await example_6_mcp_integration()

    print("=" * 60)
    print("所有示例运行完成！")
    print("=" * 60)


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n用户中断")
    except Exception as e:
        print(f"\n错误: {e}")
        import traceback
        traceback.print_exc()
