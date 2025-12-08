#!/usr/bin/env python3
"""
Test RAG with DeepSeek embeddings (requires API key)
"""

import os
import sys
from pathlib import Path
from openai import OpenAI

# 加载.env文件中的环境变量
try:
    from dotenv import load_dotenv
    load_dotenv()
except ImportError:
    # 如果没有python-dotenv，手动读取.env文件
    env_file = '.env'
    if os.path.exists(env_file):
        with open(env_file, 'r') as f:
            for line in f:
                if '=' in line and not line.strip().startswith('#'):
                    key, value = line.strip().split('=', 1)
                    os.environ[key] = value

# Add src to path
sys.path.append(str(Path(__file__).parent / "src"))

class DeepSeekEmbeddings:
    """自定义DeepSeek嵌入类"""

    def __init__(self, api_key=None, base_url=None):
        # 由于DeepSeek没有专门的嵌入API，我们使用一个替代方案
        # 这里使用简单的文本哈希作为嵌入向量（仅用于演示）
        # 实际应用中可以考虑使用其他嵌入服务或本地模型
        import hashlib
        self.api_key = api_key or os.getenv('DEEPSEEK_API_KEY')
        self.base_url = base_url or os.getenv('DEEPSEEK_BASE_URL', 'https://api.deepseek.com')
        self._embedding_dim = 1536  # 设置默认嵌入维度

    def _text_to_embedding(self, text):
        """将文本转换为嵌入向量（简化版本）"""
        # 注意：这是一个简化的实现，仅用于演示
        # 实际应用中应该使用真正的嵌入模型
        import numpy as np
        import hashlib

        # 使用哈希值创建伪嵌入向量
        hash_obj = hashlib.sha256(text.encode('utf-8'))
        hash_hex = hash_obj.hexdigest()

        # 将哈希值转换为向量
        embedding = []
        for i in range(0, min(len(hash_hex), self._embedding_dim), 2):
            val = int(hash_hex[i:i+2], 16) / 255.0
            embedding.append(val)

        # 填充到指定维度
        while len(embedding) < self._embedding_dim:
            embedding.append(0.0)

        return embedding[:self._embedding_dim]

    def embed_query(self, text):
        """生成单个文本的嵌入向量"""
        return self._text_to_embedding(text)

    def embed_documents(self, texts):
        """生成多个文本的嵌入向量"""
        return [self._text_to_embedding(text) for text in texts]

    def __call__(self, text):
        """兼容LangChain的调用方式"""
        if isinstance(text, str):
            return self.embed_query(text)
        elif isinstance(text, list):
            return self.embed_documents(text)
        else:
            raise ValueError("输入必须是字符串或字符串列表")


def test_deepseek_rag():
    """Test RAG with DeepSeek embeddings"""
    print("="*50)
    print("RAG系统测试 (DeepSeek嵌入版本)")
    print("="*50)

    # Check for API key
    if not os.getenv("DEEPSEEK_API_KEY"):
        print("\n错误: 未找到 DEEPSEEK_API_KEY")
        print("请设置环境变量或在.env文件中配置: DEEPSEEK_API_KEY=your_key_here")
        return False

    try:
        # 创建DeepSeek嵌入实例
        print("\n1. 测试DeepSeek嵌入...")
        print("注意：由于DeepSeek没有专门的嵌入API，这里使用简化的哈希方法作为演示")
        print("实际应用中建议使用OpenAI、Cohere或本地嵌入模型")
        embeddings = DeepSeekEmbeddings()

        # Test embedding
        test_text = "这是一个测试文本"
        embedding = embeddings.embed_query(test_text)
        print(f"嵌入维度: {len(embedding)}")
        print("DeepSeek嵌入测试成功！")

        # Test document loading
        from langchain_community.document_loaders import DirectoryLoader, TextLoader

        print("\n2. 测试文档加载...")
        loader = DirectoryLoader(
            "./data/sample_documents",
            glob="**/*.txt",
            loader_cls=TextLoader,
            loader_kwargs={'encoding': 'utf-8'}
        )
        documents = loader.load()
        print(f"加载了 {len(documents)} 个文档")

        if not documents:
            print("警告: 没有找到文档")
            print("请确保 ./data/sample_documents 目录中有.txt文件")
            return False

        # Test vector store
        from langchain_community.vectorstores import Chroma

        print("\n3. 创建向量存储...")
        # 创建简单的测试文档
        test_chunks = [
            {
                "page_content": "RAG是一种结合信息检索和文本生成的AI技术。",
                "metadata": {"source": "test1.txt"}
            },
            {
                "page_content": "RAG系统可以从知识库中检索相关信息。",
                "metadata": {"source": "test2.txt"}
            }
        ]

        # 使用文档列表创建向量存储（需要Document对象）
        try:
            from langchain_core.documents import Document
        except ImportError:
            from langchain.schema import Document

        docs = []
        for chunk in test_chunks:
            docs.append(Document(
                page_content=chunk["page_content"],
                metadata=chunk["metadata"]
            ))

        vector_store = Chroma.from_documents(
            documents=docs,
            embedding=embeddings,
            persist_directory="./test_vector_store"
        )
        print("向量存储创建成功！")

        # Test similarity search
        print("\n4. 测试相似度搜索...")
        query = "RAG是什么？"
        results = vector_store.similarity_search(query, k=2)

        print(f"\n查询: {query}")
        print(f"找到 {len(results)} 个相关文档:")

        for i, doc in enumerate(results, 1):
            print(f"\n文档 {i}:")
            print(f"  来源: {doc.metadata.get('source', '未知')}")
            print(f"  内容: {doc.page_content}")

        print("\n" + "="*50)
        print("测试完成！RAG系统基本功能正常")
        print("\n下一步:")
        print("1. 添加更多文档到 data/sample_documents 目录")
        print("2. 运行完整的RAG系统进行问答")
        print("3. 使用 DeepSeek LLM 生成基于检索的答案")
        print("\n注意：当前使用简化的嵌入实现，生产环境建议:")
        print("- 使用OpenAI、Cohere等专业的嵌入服务")
        print("- 或部署本地嵌入模型（如sentence-transformers）")

        return True

    except Exception as e:
        print(f"\n错误: {str(e)}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    success = test_deepseek_rag()
    sys.exit(0 if success else 1)