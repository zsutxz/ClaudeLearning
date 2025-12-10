#!/usr/bin/env python3
"""
RAG系统单元测试
"""

import os
import sys
import unittest
from pathlib import Path
from unittest.mock import Mock, patch, MagicMock

# 添加src目录到Python路径
sys.path.append(str(Path(__file__).parent.parent / "src"))

from rag_system import (
    RAGConfig, DocumentProcessor, VectorStoreManager,
    RAGRetriever, RAGSystem, create_rag_system
)


class TestRAGConfig(unittest.TestCase):
    """测试RAGConfig配置类"""

    def test_default_config(self):
        """测试默认配置"""
        config = RAGConfig()
        self.assertEqual(config.vector_store_type, "chroma")
        self.assertEqual(config.embedding_model, "openai")
        self.assertEqual(config.chunk_size, 1000)
        self.assertEqual(config.retrieval_strategy, "semantic")
        self.assertEqual(config.top_k, 5)

    def test_custom_config(self):
        """测试自定义配置"""
        config = RAGConfig(
            data_path="./test_data",
            vector_store_type="faiss",
            chunk_size=500,
            top_k=10
        )
        self.assertEqual(config.data_path, "./test_data")
        self.assertEqual(config.vector_store_type, "faiss")
        self.assertEqual(config.chunk_size, 500)
        self.assertEqual(config.top_k, 10)


class TestDocumentProcessor(unittest.TestCase):
    """测试文档处理器"""

    def setUp(self):
        """设置测试环境"""
        self.config = RAGConfig(chunk_strategy="recursive")
        self.processor = DocumentProcessor(self.config)

    @patch('rag_system.OpenAIEmbeddings')
    def test_get_embeddings_openai(self, mock_embeddings):
        """测试获取OpenAI嵌入模型"""
        mock_embeddings.return_value = Mock()
        embeddings = self.processor._get_embeddings()
        self.assertIsNotNone(embeddings)

    @patch('rag_system.HuggingFaceEmbeddings')
    def test_get_embeddings_huggingface(self, mock_embeddings):
        """测试获取HuggingFace嵌入模型"""
        self.config.embedding_model = "huggingface"
        mock_embeddings.return_value = Mock()
        embeddings = self.processor._get_embeddings()
        self.assertIsNotNone(embeddings)

    def test_text_splitter_recursive(self):
        """测试递归文本分割器"""
        splitter = self.processor._get_text_splitter()
        self.assertIsNotNone(splitter)

    @patch('rag_system.DirectoryLoader')
    def test_load_documents(self, mock_loader):
        """测试文档加载"""
        # Mock文档加载
        mock_doc = Mock()
        mock_doc.page_content = "测试内容"
        mock_doc.metadata = {"source": "test.txt"}
        mock_loader_instance = Mock()
        mock_loader_instance.load.return_value = [mock_doc]
        mock_loader.return_value = mock_loader_instance

        documents = self.processor.load_documents("./test_path")
        self.assertIsInstance(documents, list)

    def test_process_documents(self):
        """测试文档处理"""
        # 创建模拟文档
        mock_doc = Mock()
        mock_doc.page_content = "这是一个测试文档，包含一些测试内容。" * 50
        mock_doc.metadata = {"source": "test.txt"}

        chunks = self.processor.process_documents([mock_doc])
        self.assertIsInstance(chunks, list)
        self.assertGreater(len(chunks), 0)

        # 检查元数据增强
        self.assertIn("chunk_id", chunks[0].metadata)
        self.assertIn("chunk_size", chunks[0].metadata)


class TestVectorStoreManager(unittest.TestCase):
    """测试向量存储管理器"""

    def setUp(self):
        """设置测试环境"""
        self.config = RAGConfig(vector_store_type="chroma")
        with patch('rag_system.OpenAIEmbeddings'):
            self.manager = VectorStoreManager(self.config)

    def test_get_embeddings(self):
        """测试获取嵌入模型"""
        with patch('rag_system.OpenAIEmbeddings') as mock_embeddings:
            mock_embeddings.return_value = Mock()
            embeddings = self.manager._get_embeddings()
            self.assertIsNotNone(embeddings)

    @patch('rag_system.Chroma')
    def test_create_chroma_store(self, mock_chroma):
        """测试创建Chroma向量存储"""
        mock_vector_store = Mock()
        mock_chroma.from_documents.return_value = mock_vector_store

        mock_chunk = Mock()
        mock_chunk.page_content = "测试内容"
        mock_chunk.metadata = {}

        vector_store = self.manager.create_vector_store([mock_chunk])
        self.assertEqual(vector_store, mock_vector_store)


class TestRAGSystem(unittest.TestCase):
    """测试RAG系统主类"""

    def setUp(self):
        """设置测试环境"""
        self.config = RAGConfig(
            data_path="./data/sample_documents",
            vector_store_type="chroma",
            embedding_model="openai"
        )

    @patch('rag_system.OpenAIEmbeddings')
    @patch('rag_system.Chroma')
    @patch('rag_system.ChatOpenAI')
    def test_create_rag_system(self, mock_chat, mock_chroma, mock_embeddings):
        """测试创建RAG系统"""
        # Mock所有依赖
        mock_embeddings.return_value = Mock()
        mock_vector_store = Mock()
        mock_chroma.from_documents.return_value = mock_vector_store
        mock_chat.return_value = Mock()

        rag = RAGSystem(self.config)
        self.assertIsNotNone(rag)
        self.assertIsNotNone(rag.document_processor)
        self.assertIsNotNone(rag.vector_store_manager)

    @patch('rag_system.OpenAIEmbeddings')
    @patch('rag_system.Chroma')
    @patch('rag_system.ChatOpenAI')
    def test_index_documents(self, mock_chat, mock_chroma, mock_embeddings):
        """测试索引文档"""
        # Mock依赖
        mock_embeddings.return_value = Mock()
        mock_vector_store = Mock()
        mock_chroma.from_documents.return_value = mock_vector_store
        mock_chat.return_value = Mock()

        # Mock文档加载
        with patch.object(RAGSystem.document_processor, '__get__',
                        return_value=Mock(load_documents=Mock(return_value=[]))):
            rag = RAGSystem(self.config)
            # 这里需要更多的mock设置
            pass

    def test_get_stats(self):
        """测试获取统计信息"""
        rag = RAGSystem(self.config)
        stats = rag.get_stats()
        self.assertIsInstance(stats, dict)
        self.assertIn("config", stats)


class TestCreateRAGSystem(unittest.TestCase):
    """测试便捷创建函数"""

    @patch('rag_system.RAGSystem')
    def test_create_rag_system(self, mock_rag_system):
        """测试创建RAG系统的便捷函数"""
        mock_rag_instance = Mock()
        mock_rag_system.return_value = mock_rag_instance

        rag = create_rag_system(
            data_path="./test_data",
            vector_store_type="chroma"
        )

        self.assertEqual(rag, mock_rag_instance)


class TestIntegration(unittest.TestCase):
    """集成测试（需要API密钥，默认跳过）"""

    @unittest.skipUnless(os.getenv("OPENAI_API_KEY"), "需要OPENAI_API_KEY")
    def test_real_query(self):
        """真实查询测试（需要API密钥）"""
        try:
            config = RAGConfig(
                data_path="./data/sample_documents",
                vector_store_type="chroma",
                embedding_model="openai",
                top_k=3
            )

            rag = RAGSystem(config)
            rag.index_documents()

            result = rag.query("什么是RAG？")
            self.assertIsInstance(result, dict)
            self.assertIn("answer", result)
            self.assertIn("question", result)
            self.assertTrue(result["success"])

        except Exception as e:
            self.fail(f"集成测试失败: {str(e)}")


def run_unit_tests():
    """运行单元测试"""
    # 创建测试套件
    test_suite = unittest.TestSuite()

    # 添加测试用例
    test_classes = [
        TestRAGConfig,
        TestDocumentProcessor,
        TestVectorStoreManager,
        TestRAGSystem,
        TestCreateRAGSystem,
        TestIntegration
    ]

    for test_class in test_classes:
        tests = unittest.TestLoader().loadTestsFromTestCase(test_class)
        test_suite.addTests(tests)

    # 运行测试
    runner = unittest.TextTestRunner(verbosity=2)
    result = runner.run(test_suite)

    return result.wasSuccessful()


if __name__ == "__main__":
    print("🧪 运行RAG系统单元测试")
    print("="*50)

    success = run_unit_tests()

    if success:
        print("\n✅ 所有单元测试通过")
    else:
        print("\n❌ 部分单元测试失败")

    print("\n💡 提示:")
    print("- 大部分测试使用Mock对象，不需要API密钥")
    print("- 集成测试需要设置OPENAI_API_KEY环境变量")
    print("- 运行完整测试: python test_rag_system.py")