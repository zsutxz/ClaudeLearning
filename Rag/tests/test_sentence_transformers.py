#!/usr/bin/env python3
"""
Sentence-Transformers RAG系统测试模块
提供全面的测试功能
"""

import sys
from pathlib import Path

# 添加项目根目录到路径
sys.path.append(str(Path(__file__).parent.parent))

from embeddings.sentence_transformers_embeddings import SentenceTransformersEmbeddings
from core.document_loader import DocumentLoader
from core.vector_store import VectorStoreManager
from utils.similarity import SimilarityCalculator, SimilarityAnalysis
from config.environment import setup_environment
from config.huggingface_mirror import setup_huggingface_mirror


class RAGSystemTester:
    """RAG系统测试器"""

    def __init__(self):
        """初始化测试器"""
        self.embeddings = None
        self.document_loader = None
        self.vector_store_manager = None

    def setup(self):
        """设置测试环境"""
        print("="*60)
        print("RAG系统测试 (Sentence-Transformers本地嵌入版本)")
        print("="*60)

        # 设置环境
        env_manager = setup_environment()

        # 设置 Hugging Face 镜像
        print("\n1. 设置 Hugging Face 镜像...")
        setup_huggingface_mirror()

        # 初始化组件
        print("\n2. 初始化系统组件...")

        # 初始化嵌入模型
        self.embeddings = SentenceTransformersEmbeddings(
            model_name="paraphrase-multilingual-MiniLM-L12-v2"
        )

        # 初始化文档加载器
        self.document_loader = DocumentLoader()

        # 初始化向量存储管理器
        self.vector_store_manager = VectorStoreManager()

        print("系统组件初始化完成！")
        return True

    def test_embedding_functionality(self):
        """测试嵌入功能"""
        print("\n2. 测试嵌入功能...")

        # 测试单个文本嵌入
        test_result = self.embeddings.test_embedding("这是一个测试文本")
        print(f"[OK] 单个文本嵌入测试通过")
        print(f"  - 嵌入维度: {test_result['embedding_dimension']}")
        print(f"  - 嵌入耗时: {test_result['embedding_time']:.2f}秒")

        # 测试批量嵌入
        batch_result = self.embeddings.test_batch_embedding()
        print(f"[OK] 批量嵌入测试通过")
        print(f"  - 批量大小: {batch_result['batch_size']}")
        print(f"  - 平均每个文本耗时: {batch_result['avg_time_per_text']:.3f}秒")

        return True

    def test_document_loading(self):
        """测试文档加载功能"""
        print("\n3. 测试文档加载...")

        # 尝试加载真实文档
        documents = self.document_loader.load_text_documents()

        if not documents:
            print("未找到真实文档，使用测试文档...")
            documents = self.document_loader.create_test_documents()

        # 获取文档统计信息
        stats = self.document_loader.get_document_stats(documents)
        print(f"[OK] 文档加载测试通过")
        print(f"  - 文档数量: {stats['document_count']}")
        print(f"  - 总字符数: {stats['total_characters']}")
        print(f"  - 平均每文档字符数: {stats['avg_characters_per_doc']:.0f}")

        return documents

    def test_vector_store(self, documents):
        """测试向量存储功能"""
        print("\n4. 测试向量存储...")

        # 限制文档数量以加快测试
        test_docs = documents[:5] if len(documents) > 5 else documents

        # 创建向量存储
        vector_store = self.vector_store_manager.create_vector_store(
            documents=test_docs,
            embeddings=self.embeddings
        )

        # 获取存储信息
        store_info = self.vector_store_manager.get_store_info()
        print(f"[OK] 向量存储测试通过")
        print(f"  - 文档数量: {store_info.get('document_count', '未知')}")
        print(f"  - 存储目录: {store_info.get('persist_directory', '未知')}")

        return vector_store

    def test_similarity_search(self):
        """测试相似度搜索功能"""
        print("\n5. 测试相似度搜索...")

        queries = [
            "什么是RAG技术？",
            "如何生成文本嵌入？",
            "RAG系统如何工作？"
        ]

        for query in queries:
            results = self.vector_store_manager.similarity_search(
                query=query,
                k=2
            )
            self.vector_store_manager.print_search_results(results)

        print("[OK] 相似度搜索测试通过")
        return True

    def test_similarity_analysis(self):
        """测试相似度分析功能"""
        print("\n6. 测试相似度分析...")

        test_texts = [
            "人工智能正在改变世界",
            "AI技术影响我们的生活",
            "今天天气很好",
            "RAG系统结合检索和生成"
        ]

        # 生成嵌入
        embeddings_list = self.embeddings.embed_documents(test_texts)

        # 计算相似度
        text1_emb = embeddings_list[0]
        text2_emb = embeddings_list[1]
        text3_emb = embeddings_list[2]

        # 使用相似度计算器
        sim12 = SimilarityCalculator.cosine_similarity(text1_emb, text2_emb)
        sim13 = SimilarityCalculator.cosine_similarity(text1_emb, text3_emb)

        print(f"\n文本1: {test_texts[0]}")
        print(f"文本2: {test_texts[1]}")
        print(f"文本3: {test_texts[2]}")
        print(f"\n文本1和文本2的相似度: {sim12:.3f} (应该较高)")
        print(f"文本1和文本3的相似度: {sim13:.3f} (应该较低)")

        # 进行完整的相似度分析
        analysis = SimilarityCalculator.analyze_text_similarities(
            self.embeddings, test_texts
        )
        SimilarityCalculator.print_similarity_analysis(analysis, test_texts)

        print("[OK] 相似度分析测试通过")
        return True

    def run_all_tests(self):
        """运行所有测试"""
        try:
            # 设置测试环境
            if not self.setup():
                return False

            # 测试嵌入功能
            if not self.test_embedding_functionality():
                return False

            # 测试文档加载
            documents = self.test_document_loading()
            if not documents:
                return False

            # 测试向量存储
            if not self.test_vector_store(documents):
                return False

            # 测试相似度搜索
            if not self.test_similarity_search():
                return False

            # 测试相似度分析
            if not self.test_similarity_analysis():
                return False

            # 打印成功信息
            print("\n" + "="*60)
            print("所有测试完成！Sentence-Transformers RAG系统工作正常")
            print("\n系统优势:")
            print("[+] 完全本地运行，无需API密钥")
            print("[+] 支持多种语言的预训练模型")
            print("[+] 可离线使用，保护数据隐私")
            print("[+] 批量处理效率高")
            print("[+] 支持自定义模型微调")

            print("\n推荐的中文优化模型:")
            print("- shibing624/text2vec-base-chinese")
            print("- sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2")
            print("- sentence-transformers/distiluse-base-multilingual-cased-v2")

            return True

        except Exception as e:
            print(f"\n错误: {str(e)}")
            import traceback
            traceback.print_exc()
            return False


def main():
    """主测试函数"""
    tester = RAGSystemTester()
    success = tester.run_all_tests()
    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()