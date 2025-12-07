#!/usr/bin/env python3
"""
Test RAG with OpenAI embeddings (requires API key)
"""

import os
import sys
from pathlib import Path

# Add src to path
sys.path.append(str(Path(__file__).parent / "src"))

def test_openai_rag():
    """Test RAG with OpenAI embeddings"""
    print("="*50)
    print("RAG系统测试 (OpenAI嵌入版本)")
    print("="*50)

    # Check for API key
    if not os.getenv("OPENAI_API_KEY"):
        print("\n错误: 未找到 OPENAI_API_KEY")
        print("请设置环境变量: export OPENAI_API_KEY=your_key_here")
        return False

    try:
        # Try OpenAI embeddings
        from langchain_community.embeddings import OpenAIEmbeddings

        print("\n1. 测试OpenAI嵌入...")
        embeddings = OpenAIEmbeddings(
            openai_api_key=os.getenv("OPENAI_API_KEY")
        )

        # Test embedding
        test_text = "这是一个测试文本"
        embedding = embeddings.embed_query(test_text)
        print(f"嵌入维度: {len(embedding)}")
        print("OpenAI嵌入测试成功！")

        # Test document loading
        from langchain_community.document_loaders import DirectoryLoader, TextLoader

        print("\n2. 测试文档加载...")
        loader = DirectoryLoader(
            "./data/sample_documents",
            glob="**/*.txt",
            loader_cls=TextLoader
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
        print("3. 使用 LLM 生成基于检索的答案")

        return True

    except Exception as e:
        print(f"\n错误: {str(e)}")
        import traceback
        traceback.print_exc()
        return False

if __name__ == "__main__":
    success = test_openai_rag()
    sys.exit(0 if success else 1)