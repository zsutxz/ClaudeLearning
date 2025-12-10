"""
Research Agent 核心功能测试

测试ResearchAgent的主要功能:
- 初始化和配置
- 基础研究流程
- 报告生成
- 错误处理
"""

import asyncio
import sys
import os
import unittest
from unittest.mock import Mock, patch

# 添加项目路径
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

try:
    from research_agent import ResearchAgent, ResearchConfig, quick_research
except ImportError as e:
    print(f"导入错误: {e}")
    print("请确保在正确的目录运行测试")
    ResearchAgent = None
    ResearchConfig = None
    quick_research = None

class TestResearchAgent(unittest.TestCase):
    """ResearchAgent测试类"""

    def setUp(self):
        """测试前的设置"""
        if ResearchAgent is None:
            self.skipTest("ResearchAgent未正确导入")

        # 创建测试用的Research Agent
        self.agent = ResearchAgent(
            research_domain="人工智能测试",
            provider="mock",  # 使用模拟模式避免API调用
            model="mock-model"
        )

    def test_agent_initialization(self):
        """测试代理初始化"""
        self.assertIsNotNone(self.agent)
        self.assertEqual(self.agent.research_domain, "人工智能测试")
        self.assertEqual(self.agent.provider, "mock")
        self.assertEqual(self.agent.model, "mock-model")

    def test_config_creation(self):
        """测试配置创建"""
        config = ResearchConfig(
            research_domain="测试领域",
            max_sources=15,
            output_format="markdown"
        )

        self.assertEqual(config.research_domain, "测试领域")
        self.assertEqual(config.max_sources, 15)
        self.assertEqual(config.output_format, "markdown")
        self.assertTrue(config.include_github)

    def test_basic_chat_functionality(self):
        """测试基础聊天功能"""
        response = self.agent.chat("你好，这是一个测试消息")
        self.assertIsInstance(response, str)
        self.assertIn("模拟", response)  # Mock模式的响应应包含"模拟"

    @patch('research_agent.asyncio.sleep')  # Mock异步延迟
    async def test_conduct_research_basic(self, mock_sleep):
        """测试基础研究功能"""
        result = await self.agent.conduct_research(
            query="测试研究主题",
            max_sources=5,
            output_format="markdown"
        )

        # 验证结果结构
        self.assertIsNotNone(result)
        self.assertEqual(result.query, "测试研究主题")
        self.assertIsNotNone(result.report)
        self.assertIsInstance(result.metadata, dict)

    def test_markdown_report_generation(self):
        """测试Markdown报告生成"""
        research_data = {
            'query': '测试查询',
            'literature': {'status': 'test'},
            'data': {'status': 'test'},
            'config': ResearchConfig(output_format='markdown')
        }

        report = self.agent._generate_markdown_report(research_data)

        self.assertIsInstance(report, str)
        self.assertIn("测试查询", report)
        self.assertIn("#", report)  # Markdown标题格式

    def test_text_report_generation(self):
        """测试文本报告生成"""
        research_data = {
            'query': '测试查询',
            'config': ResearchConfig(output_format='text')
        }

        report = self.agent._generate_text_report(research_data)

        self.assertIsInstance(report, str)
        self.assertIn("测试查询", report)
        self.assertIn("=", report)  # 文本分隔符

    async def test_error_handling(self):
        """测试错误处理"""
        # 测试无效查询
        result = await self.agent.conduct_research("")
        self.assertIsNotNone(result)

        # 测试无效配置
        try:
            invalid_config = ResearchConfig(max_sources=-1)  # 无效值
            # 应该能创建，但在实际使用时会处理
            self.assertIsInstance(invalid_config, ResearchConfig)
        except:
            pass  # 如果抛出异常也是可以接受的

    def test_module_initialization(self):
        """测试模块初始化"""
        # 测试基础模块初始化
        self.agent._init_basic_modules()

        self.assertIsNotNone(self.agent.literature_retriever)
        self.assertIsNotNone(self.agent.data_processor)
        self.assertIsNotNone(self.agent.report_generator)
        self.assertIsNotNone(self.agent.quality_checker)

class TestQuickResearch(unittest.TestCase):
    """快速研究功能测试"""

    def setUp(self):
        """测试前的设置"""
        if quick_research is None:
            self.skipTest("quick_research函数未正确导入")

    @patch('research_agent.asyncio.sleep')
    async def test_quick_research_function(self, mock_sleep):
        """测试快速研究函数"""
        result = await quick_research(
            query="快速测试查询",
            research_domain="测试领域",
            provider="mock"
        )

        self.assertIsNotNone(result)
        self.assertEqual(result.query, "快速测试查询")

class TestIntegration(unittest.TestCase):
    """集成测试"""

    def test_full_workflow_simulation(self):
        """模拟完整工作流程"""
        if ResearchAgent is None:
            self.skipTest("ResearchAgent未正确导入")

        # 创建代理
        agent = ResearchAgent(
            research_domain="集成测试",
            provider="mock"
        )

        # 测试系统提示词设置
        self.assertTrue(len(agent.conversation_history) > 0)
        self.assertEqual(agent.conversation_history[0]["role"], "system")

        # 测试基本对话
        response1 = agent.chat("这是一个集成测试")
        response2 = agent.chat("请继续测试")

        # 验证对话历史维护
        self.assertGreater(len(agent.conversation_history), 2)

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
        """测试前的设置"""
        if ResearchAgent is None:
            self.skipTest("ResearchAgent未正确导入")

        self.agent = ResearchAgent(
            research_domain="异步测试",
            provider="mock"
        )

    def test_async_research_execution(self):
        """测试异步研究执行"""
        result = run_async_test(
            self.agent.conduct_research(
                query="异步测试查询",
                max_sources=3
            )
        )

        self.assertIsNotNone(result)
        self.assertEqual(result.query, "异步测试查询")

    def test_async_literature_search(self):
        """测试异步文献搜索"""
        result = run_async_test(
            self.agent._search_literature(
                query="测试搜索",
                config=ResearchConfig(max_sources=5)
            )
        )

        self.assertIsInstance(result, dict)
        self.assertIn('status', result)

def run_tests():
    """运行所有测试"""
    print("=== Research Agent 测试开始 ===\n")

    # 创建测试套件
    test_suite = unittest.TestSuite()

    # 添加测试用例
    test_classes = [
        TestResearchAgent,
        TestQuickResearch,
        TestIntegration,
        TestAsyncFunctions
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

    if result.failures:
        print("\n失败的测试:")
        for test, traceback in result.failures:
            print(f"- {test}: {traceback}")

    if result.errors:
        print("\n错误的测试:")
        for test, traceback in result.errors:
            print(f"- {test}: {traceback}")

    return result.wasSuccessful()

if __name__ == "__main__":
    # 检查依赖
    if ResearchAgent is None:
        print("错误: ResearchAgent未正确导入，请检查依赖和路径")
        sys.exit(1)

    success = run_tests()
    sys.exit(0 if success else 1)