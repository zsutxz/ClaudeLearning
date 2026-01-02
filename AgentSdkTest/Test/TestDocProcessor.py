#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
文档处理 MCP 服务器测试

测试文档处理器的各个功能和工具。
"""

import sys
import unittest
import asyncio
from pathlib import Path

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

try:
    from mcp_servers.doc_processor_server import (
        PDFProcessor,
        WordProcessor,
        MarkdownProcessor,
        HTMLProcessor,
        TextProcessor,
        get_processor,
        read_document,
        extract_metadata,
        search_in_document,
        batch_process,
        get_supported_formats,
        validate_document,
        convert_to_markdown,
        get_document_stats,
    )
except ImportError as e:
    print(f"导入错误: {e}")
    print("请确保已安装所有依赖: pip install -r requirements.txt")
    sys.exit(1)


class TestProcessors(unittest.TestCase):
    """测试文档处理器"""

    def setUp(self):
        """测试前设置"""
        self.test_file = project_root / "README.md"

    def test_markdown_processor(self):
        """测试 Markdown 处理器"""
        processor = MarkdownProcessor()

        # 读取文档
        result = processor.read(str(self.test_file))
        self.assertTrue(result.get("success"))
        self.assertIn("content", result)
        self.assertGreater(len(result["content"]), 0)

        # 获取元数据
        metadata = processor.get_metadata(str(self.test_file))
        self.assertNotIn("error", metadata)
        self.assertIn("format", metadata)

    def test_get_processor(self):
        """测试处理器工厂"""
        processor = get_processor(str(self.test_file))
        self.assertIsInstance(processor, MarkdownProcessor)

    def test_file_validation(self):
        """测试文件验证"""
        processor = MarkdownProcessor()

        # 有效文件
        valid, error = processor.validate_file(str(self.test_file))
        self.assertTrue(valid)

        # 不存在的文件
        valid, error = processor.validate_file("nonexistent.md")
        self.assertFalse(valid)


class TestTools(unittest.TestCase):
    """测试 MCP 工具函数"""

    def setUp(self):
        """测试前设置"""
        self.test_file = str(project_root / "README.md")

    async def test_read_document_tool(self):
        """测试读取文档工具"""
        args = {"file_path": self.test_file}
        result = await read_document(args)

        self.assertFalse(result.get("is_error"))
        self.assertIn("content", result)

    async def test_extract_metadata_tool(self):
        """测试提取元数据工具"""
        args = {"file_path": self.test_file}
        result = await extract_metadata(args)

        self.assertFalse(result.get("is_error"))
        self.assertIn("content", result)

    async def test_search_in_document_tool(self):
        """测试文档搜索工具"""
        args = {
            "file_path": self.test_file,
            "query": "Claude",
            "case_sensitive": False,
        }
        result = await search_in_document(args)

        self.assertFalse(result.get("is_error"))
        self.assertIn("content", result)

    async def test_batch_process_tool(self):
        """测试批量处理工具"""
        args = {
            "file_paths": [self.test_file],
            "operation": "read"
        }
        result = await batch_process(args)

        self.assertFalse(result.get("is_error"))

    async def test_get_supported_formats_tool(self):
        """测试获取支持格式工具"""
        result = await get_supported_formats({})

        self.assertFalse(result.get("is_error"))
        self.assertIn("content", result)

    async def test_validate_document_tool(self):
        """测试验证文档工具"""
        args = {"file_path": self.test_file}
        result = await validate_document(args)

        self.assertFalse(result.get("is_error"))

    async def test_convert_to_markdown_tool(self):
        """测试转换为 Markdown 工具"""
        import tempfile

        args = {
            "file_path": self.test_file,
            "output_path": None  # 自动生成
        }
        result = await convert_to_markdown(args)

        self.assertFalse(result.get("is_error"))

    async def test_get_document_stats_tool(self):
        """测试获取文档统计工具"""
        args = {"file_path": self.test_file}
        result = await get_document_stats(args)

        self.assertFalse(result.get("is_error"))
        self.assertIn("content", result)


class TestIntegration(unittest.TestCase):
    """集成测试"""

    def setUp(self):
        """测试前设置"""
        self.test_file = str(project_root / "README.md")

    async def test_complete_workflow(self):
        """测试完整工作流程"""
        # 1. 验证文档
        args = {"file_path": self.test_file}
        result = await validate_document(args)
        self.assertFalse(result.get("is_error"))

        # 2. 读取文档
        result = await read_document(args)
        self.assertFalse(result.get("is_error"))

        # 3. 提取元数据
        result = await extract_metadata(args)
        self.assertFalse(result.get("is_error"))

        # 4. 搜索内容
        args = {
            "file_path": self.test_file,
            "query": "Claude"
        }
        result = await search_in_document(args)
        self.assertFalse(result.get("is_error"))

        # 5. 获取统计
        args = {"file_path": self.test_file}
        result = await get_document_stats(args)
        self.assertFalse(result.get("is_error"))


def run_async_test(coro):
    """运行异步测试的辅助函数"""
    loop = asyncio.new_event_loop()
    try:
        return loop.run_until_complete(coro)
    finally:
        loop.close()


class TestAsyncFunctions(unittest.TestCase):
    """异步函数测试"""

    def setUp(self):
        """测试前设置"""
        self.test_file = str(project_root / "README.md")

    def test_read_document_sync(self):
        """同步包装器测试"""
        result = run_async_test(
            read_document({"file_path": self.test_file})
        )
        self.assertFalse(result.get("is_error"))

    def test_extract_metadata_sync(self):
        """同步包装器测试"""
        result = run_async_test(
            extract_metadata({"file_path": self.test_file})
        )
        self.assertFalse(result.get("is_error"))

    def test_search_sync(self):
        """同步包装器测试"""
        result = run_async_test(
            search_in_document({
                "file_path": self.test_file,
                "query": "Claude"
            })
        )
        self.assertFalse(result.get("is_error"))


def run_tests():
    """运行所有测试"""
    print("=== 文档处理 MCP 服务器测试开始 ===\n")

    # 创建测试套件
    test_suite = unittest.TestSuite()

    # 添加测试用例
    test_classes = [
        TestProcessors,
        TestTools,
        TestIntegration,
        TestAsyncFunctions,
    ]

    for test_class in test_classes:
        tests = unittest.TestLoader().loadTestsFromTestCase(test_class)
        test_suite.addTests(tests)

    # 运行测试
    runner = unittest.TextTestRunner(verbosity=2)
    result = runner.run(test_suite)

    print(f"\n=== 测试完成 ===")
    print(f"运行测试: {result.testsRun}")
    print(f"失败: {len(result.failures)}")
    print(f"错误: {len(result.errors)}")
    print(f"跳过: {len(result.skipped)}")

    if result.wasSuccessful():
        print("\n✓ 所有测试通过！")
    else:
        print("\n✗ 部分测试失败")
        if result.failures:
            print("\n失败的测试:")
            for test, traceback in result.failures:
                print(f"  - {test}")

        if result.errors:
            print("\n错误的测试:")
            for test, traceback in result.errors:
                print(f"  - {test}")

    return result.wasSuccessful()


if __name__ == "__main__":
    success = run_tests()
    sys.exit(0 if success else 1)
