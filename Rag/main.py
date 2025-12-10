#!/usr/bin/env python3
"""
Test RAG with Sentence-Transformers embeddings (本地部署方案)
"""

import os
import sys
from pathlib import Path

# 加载.env文件中的环境变量
try:
    from dotenv import load_dotenv
    load_dotenv()
except ImportError:
    env_file = '.env'
    if os.path.exists(env_file):
        with open(env_file, 'r') as f:
            for line in f:
                if '=' in line and not line.strip().startswith('#'):
                    key, value = line.strip().split('=', 1)
                    os.environ[key] = value

# Add src to path
sys.path.append(str(Path(__file__).parent / "src"))

class SentenceTransformersEmbeddings:
    """使用Sentence-Transformers的本地嵌入类"""

    def __init__(self, model_name="paraphrase-multilingual-MiniLM-L12-v2"):
        """
        初始化嵌入模型
        推荐的多语言模型：
        - paraphrase-multilingual-MiniLM-L12-v2: 轻量级多语言模型
        - paraphrase-multilingual-mpnet-base-v2: 更好的质量但更大
        - shibing624/text2vec-base-chinese: 专门优化中文
        """
        print(f"正在加载Sentence-Transformers模型: {model_name}")
        print("首次运行时会自动下载模型，请耐心等待...")

        from sentence_transformers import SentenceTransformer
        self.model = SentenceTransformer(model_name)
        print("模型加载完成！")

    def embed_query(self, text):
        """生成单个文本的嵌入向量"""
        embedding = self.model.encode(text, convert_to_tensor=False)
        # 确保返回的是列表格式，Chroma期望这个格式
        return embedding.tolist() if hasattr(embedding, 'tolist') else embedding

    def embed_documents(self, texts):
        """批量生成文本的嵌入向量（更高效）"""
        embeddings = self.model.encode(texts, convert_to_tensor=False, batch_size=32)
        # 确保返回的是列表的列表格式
        return [emb.tolist() if hasattr(emb, 'tolist') else emb for emb in embeddings]

    def __call__(self, text):
        """兼容LangChain的调用方式"""
        if isinstance(text, str):
            return self.embed_query(text)
        elif isinstance(text, list):
            return self.embed_documents(text)
        else:
            raise ValueError("输入必须是字符串或字符串列表")

def test_sentence_transformers_rag():
    """测试使用Sentence-Transformers的RAG系统"""
    print("="*60)
    print("RAG系统测试 (Sentence-Transformers本地嵌入版本)")
    print("="*60)

    try:
        # 创建Sentence-Transformers嵌入实例
        print("\n1. 初始化本地嵌入模型...")
        embeddings = SentenceTransformersEmbeddings(
            model_name="paraphrase-multilingual-MiniLM-L12-v2"  # 轻量级多语言模型
        )

        # 测试单个文本嵌入
        test_text = "这是一个测试文本"
        print("\n2. 测试文本嵌入...")
        print(f"测试文本: {test_text}")

        import time
        start_time = time.time()
        embedding = embeddings.embed_query(test_text)
        end_time = time.time()

        print(f"嵌入维度: {len(embedding)}")
        print(f"嵌入耗时: {end_time - start_time:.2f}秒")
        print("本地嵌入测试成功！")

        # 测试批量嵌入
        print("\n3. 测试批量嵌入...")
        test_texts = [
            "人工智能是计算机科学的一个分支",
            "机器学习是人工智能的子领域",
            "深度学习使用神经网络来模拟人脑"
        ]

        start_time = time.time()
        batch_embeddings = embeddings.embed_documents(test_texts)
        end_time = time.time()

        print(f"批量处理{len(test_texts)}个文本")
        print(f"平均每个文本耗时: {(end_time - start_time)/len(test_texts):.3f}秒")

        # 加载文档
        from langchain_community.document_loaders import DirectoryLoader, TextLoader

        print("\n4. 测试文档加载...")
        loader = DirectoryLoader(
            "./data/sample_documents",
            glob="**/*.txt",
            loader_cls=TextLoader,
            loader_kwargs={'encoding': 'utf-8'}
        )
        documents = loader.load()
        print(f"加载了 {len(documents)} 个文档")

        # 创建向量存储
        from langchain_community.vectorstores import Chroma

        print("\n5. 创建向量存储...")

        # 使用实际的文档（如果有的话）或使用测试文档
        if documents:
            # 使用真实文档
            docs = documents[:5]  # 限制文档数量以加快测试
        else:
            # 使用测试文档
            try:
                from langchain_core.documents import Document
            except ImportError:
                from langchain.schema import Document

            docs = [
                Document(
                    page_content="RAG（Retrieval-Augmented Generation）是一种结合信息检索和文本生成的AI技术。",
                    metadata={"source": "test1.txt"}
                ),
                Document(
                    page_content="RAG系统可以从知识库中检索相关信息，然后基于这些信息生成答案。",
                    metadata={"source": "test2.txt"}
                ),
                Document(
                    page_content="Sentence-Transformers是一个优秀的Python库，用于生成高质量的文本嵌入向量。",
                    metadata={"source": "test3.txt"}
                )
            ]

        # 创建向量存储
        start_time = time.time()
        vector_store = Chroma.from_documents(
            documents=docs,
            embedding=embeddings,
            persist_directory="./test_sentence_transformers_store"
        )
        end_time = time.time()

        print(f"向量存储创建成功！耗时: {end_time - start_time:.2f}秒")

        # 测试相似度搜索
        print("\n6. 测试相似度搜索...")
        queries = [
            "什么是RAG技术？",
            "如何生成文本嵌入？",
            "RAG系统如何工作？"
        ]

        for query in queries:
            print(f"\n查询: {query}")

            start_time = time.time()
            results = vector_store.similarity_search(query, k=2)
            end_time = time.time()

            print(f"搜索耗时: {end_time - start_time:.3f}秒")
            print(f"找到 {len(results)} 个相关文档:")

            for i, doc in enumerate(results, 1):
                print(f"\n文档 {i}:")
                print(f"  来源: {doc.metadata.get('source', '未知')}")
                print(f"  内容: {doc.page_content[:100]}...")

        # 计算嵌入相似度示例
        print("\n7. 嵌入相似度分析...")
        text1 = "人工智能正在改变世界"
        text2 = "AI技术影响我们的生活"
        text3 = "今天天气很好"

        emb1 = embeddings.embed_query(text1)
        emb2 = embeddings.embed_query(text2)
        emb3 = embeddings.embed_query(text3)

        # 计算余弦相似度
        import numpy as np
        def cosine_similarity(a, b):
            return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))

        sim12 = cosine_similarity(emb1, emb2)
        sim13 = cosine_similarity(emb1, emb3)

        print(f"\n文本1: {text1}")
        print(f"文本2: {text2}")
        print(f"文本3: {text3}")
        print(f"\n文本1和文本2的相似度: {sim12:.3f} (应该较高)")
        print(f"文本1和文本3的相似度: {sim13:.3f} (应该较低)")

        print("\n" + "="*60)
        print("测试完成！Sentence-Transformers RAG系统工作正常")
        print("\n优势:")
        print("+ 完全本地运行，无需API密钥")
        print("+ 支持多种语言的预训练模型")
        print("+ 可离线使用，保护数据隐私")
        print("+ 批量处理效率高")
        print("+ 支持自定义模型微调")

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

if __name__ == "__main__":
    success = test_sentence_transformers_rag()
    sys.exit(0 if success else 1)