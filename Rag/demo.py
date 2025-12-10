#!/usr/bin/env python3
"""
RAG系统演示程序（无需下载模型）
展示模块化结构和基本功能
"""

import sys
from pathlib import Path

# 添加项目路径
sys.path.append(str(Path(__file__).parent))

# 导入自定义模块
from config.environment import setup_environment
from core.document_loader import DocumentLoader
from core.vector_store import VectorStoreManager
import numpy as np


def demo_modular_structure():
    """演示模块化结构"""
    print("="*60)
    print("RAG系统模块化结构演示")
    print("="*60)

    # 1. 环境设置
    print("\n1. 环境设置模块演示...")
    env_manager = setup_environment()

    # 2. 文档加载器演示
    print("\n2. 文档加载器演示...")
    doc_loader = DocumentLoader()

    # 创建测试文档
    test_docs = doc_loader.create_test_documents()
    print(f"创建了 {len(test_docs)} 个测试文档")

    # 获取文档统计
    stats = doc_loader.get_document_stats(test_docs)
    print(f"文档统计: {stats}")

    # 3. 向量存储管理器演示（使用模拟嵌入）
    print("\n3. 向量存储管理器演示...")

    # 创建模拟嵌入类
    class MockEmbedding:
        def __init__(self, dimension=384):
            self.dimension = dimension

        def embed_documents(self, texts):
            return [np.random.rand(self.dimension).tolist() for _ in texts]

        def embed_query(self, text):
            return np.random.rand(self.dimension).tolist()

        def __call__(self, text):
            if isinstance(text, str):
                return self.embed_query(text)
            else:
                return self.embed_documents(text)

    # 使用模拟嵌入
    mock_embedding = MockEmbedding()

    # 创建向量存储管理器
    vector_manager = VectorStoreManager(persist_directory="./demo_vector_store")

    # 注意：实际创建向量存储需要真实的嵌入模型
    # 这里只是展示结构
    print("向量存储管理器已初始化")

    # 4. 展示模块结构
    print("\n4. 模块结构展示:")
    print("""
    rag/
    ├── main.py                    # 主程序入口
    ├── demo.py                    # 演示程序（当前文件）
    ├── requirements.txt           # 依赖列表
    ├── embeddings/                # 嵌入模型模块
    │   ├── __init__.py
    │   └── sentence_transformers_embeddings.py
    ├── core/                      # 核心功能模块
    │   ├── __init__.py
    │   ├── document_loader.py     # 文档加载
    │   └── vector_store.py        # 向量存储
    ├── utils/                     # 工具模块
    │   ├── __init__.py
    │   └── similarity.py          # 相似度计算
    ├── config/                    # 配置模块
    │   ├── __init__.py
    │   └── environment.py         # 环境管理
    └── tests/                     # 测试模块
        ├── __init__.py
        └── test_sentence_transformers.py
    """)

    # 5. 展示模块使用示例
    print("\n5. 模块使用示例:")
    print("""
    # 使用文档加载器
    from core.document_loader import DocumentLoader
    loader = DocumentLoader()
    documents = loader.load_text_documents("./data")

    # 使用向量存储管理器
    from core.vector_store import VectorStoreManager
    store_manager = VectorStoreManager()
    vector_store = store_manager.create_vector_store(documents, embeddings)

    # 使用相似度计算工具
    from utils.similarity import SimilarityCalculator
    similarity = SimilarityCalculator.cosine_similarity(vec1, vec2)

    # 使用嵌入模型（需要先安装）
    from embeddings.sentence_transformers_embeddings import SentenceTransformersEmbeddings
    embeddings = SentenceTransformersEmbeddings(model_name="模型名")
    """)

    print("\n" + "="*60)
    print("模块化结构演示完成！")
    print("\n要运行完整功能，请:")
    print("1. 安装依赖: pip install -r requirements.txt")
    print("2. 确保网络连接正常（需要下载模型）")
    print("3. 运行主程序: python main.py")
    print("4. 或运行测试: python tests/test_sentence_transformers.py")

    return True


if __name__ == "__main__":
    success = demo_modular_structure()
    sys.exit(0 if success else 1)