# RAG系统 - 智能检索增强生成系统

一个模块化、高性能的RAG（Retrieval-Augmented Generation）系统，支持多种向量数据库和嵌入模型，专为生产环境设计。

## 🌟 核心特性

### 🔧 完整RAG流程
- **智能检索**：基于相似度的文档检索
- **答案生成**：集成DeepSeek LLM生成智能答案
- **无缝集成**：检索和生成的完美结合

### 🧩 模块化架构
- **核心解耦**：各组件独立，易于扩展和维护
- **插件化设计**：支持自定义嵌入模型和向量存储
- **灵活配置**：适应不同场景的需求

### 🚀 多模型支持
- **嵌入模型**：Sentence-Transformers、OpenAI、Hugging Face
- **向量数据库**：Chroma、FAISS、Pinecone、Weaviate、Qdrant
- **LLM集成**：DeepSeek、OpenAI、Anthropic Claude

### 🛡️ 企业级特性
- **本地部署**：支持完全离线运行，保护数据隐私
- **高性能**：批量处理、GPU加速、缓存机制
- **易于扩展**：支持自定义组件和扩展

## 📁 项目结构

```
rag/
├── core/                           # 核心RAG模块
│   ├── __init__.py
│   ├── document_loader.py          # 文档加载器
│   ├── vector_store.py             # 向量存储管理
│   └── rag_system.py               # 完整RAG系统实现
├── embeddings/                     # 嵌入模型
│   ├── __init__.py
│   └── sentence_transformers_embeddings.py  # Sentence-Transformers实现
├── llm/                            # LLM集成模块
│   ├── __init__.py
│   ├── base_llm.py                 # LLM基类
│   └── deepseek_llm.py             # DeepSeek LLM实现
├── config/                         # 配置管理
│   ├── __init__.py
│   ├── environment.py              # 环境配置
│   └── huggingface_mirror.py       # HuggingFace镜像配置
├── utils/                          # 工具模块
│   ├── __init__.py
│   └── similarity.py               # 相似度计算
├── tests/                          # 测试模块
│   ├── __init__.py
│   └── test_sentence_transformers.py
├── data/                           # 数据目录
│   └── sample_documents/           # 示例文档
│       └── rag_introduction.md
├── main.py                         # 主程序入口
├── demo_rag.py                     # RAG系统演示脚本
├── .env.example                    # 环境变量示例
├── requirements.txt                # 依赖列表
└── README.md                       # 项目文档
```

## 🚀 快速开始

### 1. 环境准备

```bash
# 克隆项目
git clone <repository-url>
cd rag

# 创建虚拟环境
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate

# 安装依赖
pip install -r requirements.txt
```

### 2. 配置环境变量

创建 `.env` 文件：

```env
# Hugging Face镜像（国内用户推荐）
HF_ENDPOINT=https://hf-mirror.com

# DeepSeek API（用于答案生成）
DEEPSEEK_API_KEY=your_deepseek_api_key_here

# OpenAI API（可选）
OPENAI_API_KEY=your_openai_api_key_here

# 其他配置...
```

### 3. 运行示例

```bash
# 运行完整RAG系统测试
python main.py

# 运行交互式演示
python demo_rag.py

# 仅运行检索部分测试
python main.py --mode retrieval

# 运行完整功能
python main.py --mode complete
```

## 💻 使用示例

### 基础RAG系统使用

```python
from core.rag_system import CompleteRAGSystem, RAGConfig
from embeddings.sentence_transformers_embeddings import SentenceTransformersEmbeddings
from config.environment import setup_environment

# 1. 创建RAG配置
config = RAGConfig(
    model_name="shibing624/text2vec-base-chinese",
    vector_store_type="chroma",
    llm_provider="deepseek",
    chunk_size=1000,
    chunk_overlap=200
)

# 2. 初始化RAG系统
rag_system = CompleteRAGSystem(config)

# 3. 添加文档
documents = ["你的文档内容1", "你的文档内容2"]
rag_system.add_documents(documents)

# 4. 查询并生成答案
query = "什么是RAG技术？"
response = rag_system.query(query)
print(f"回答: {response['answer']}")
print(f"来源: {response['source_documents']}")
```

### 独立使用检索功能

```python
from embeddings.sentence_transformers_embeddings import SentenceTransformersEmbeddings
from core.document_loader import DocumentLoader
from core.vector_store import VectorStoreManager

# 初始化嵌入模型
embeddings = SentenceTransformersEmbeddings(
    model_name="paraphrase-multilingual-MiniLM-L12-v2"
)

# 加载文档
loader = DocumentLoader()
documents = loader.load_text_documents()

# 创建向量存储
vector_store_manager = VectorStoreManager()
vector_store = vector_store_manager.create_vector_store(
    documents=documents,
    embeddings=embeddings
)

# 执行搜索
query = "什么是RAG技术？"
results = vector_store_manager.similarity_search(query, k=3)
vector_store_manager.print_search_results(results)
```

### 高级用法

```python
# 批量嵌入优化
batch_results = embeddings.test_batch_embedding(
    texts=["文本1", "文本2", "文本3"],
    batch_size=32
)

# 相似度分析
from utils.similarity import SimilarityCalculator

text1_emb = embeddings.embed_query("人工智能正在改变世界")
text2_emb = embeddings.embed_query("AI技术影响我们的生活")
similarity = SimilarityCalculator.cosine_similarity(text1_emb, text2_emb)
print(f"相似度: {similarity:.3f}")
```

## 🔧 配置选项

### 嵌入模型选择

| 模型名称 | 大小 | 语言 | 特点 | 适用场景 |
|---------|------|------|------|---------|
| `shibing624/text2vec-base-chinese` | 420MB | 中文 | 中文优化 | 中文应用 |
| `paraphrase-multilingual-MiniLM-L12-v2` | 420MB | 多语言 | 轻量多语言 | 国际化应用 |
| `all-mpnet-base-v2` | 420MB | 英文 | 高质量英文 | 英文应用 |
| `paraphrase-multilingual-mpnet-base-v2` | 1.1GB | 多语言 | 高质量多语言 | 高质量要求 |

### 向量数据库配置

```python
# Chroma（默认，轻量级本地）
vector_store = VectorStoreManager().create_vector_store(
    documents=documents,
    embeddings=embeddings,
    vector_store_type="chroma"
)

# FAISS（高性能本地）
vector_store = VectorStoreManager().create_vector_store(
    documents=documents,
    embeddings=embeddings,
    vector_store_type="faiss"
)

# Pinecone（云端托管）
vector_store = VectorStoreManager().create_vector_store(
    documents=documents,
    embeddings=embeddings,
    vector_store_type="pinecone"
)
```

### LLM配置

```python
# DeepSeek（推荐，高性价比）
config = RAGConfig(
    llm_provider="deepseek",
    model="deepseek-chat"
)

# OpenAI
config = RAGConfig(
    llm_provider="openai",
    model="gpt-3.5-turbo"
)
```

## 📊 性能优化

### 1. 批量处理

```python
# 批量嵌入，提高效率
embeddings_list = embeddings.embed_documents(
    texts=large_text_list,
    batch_size=32
)
```

### 2. GPU加速

```python
# 自动检测并使用GPU
import torch
device = 'cuda' if torch.cuda.is_available() else 'cpu'
model = SentenceTransformer(model_name).to(device)
```

### 3. 缓存机制

```python
# 嵌入结果自动缓存
embeddings = SentenceTransformersEmbeddings(
    model_name=model_name,
    cache_folder="./embeddings_cache"
)
```

### 4. 分块优化

```python
# 优化文档分块策略
config = RAGConfig(
    chunk_size=1000,
    chunk_overlap=200,
    separators=["\n\n", "\n", " ", ""]
)
```

## 🌐 网络问题解决

### 国内用户优化

```python
# 自动配置HuggingFace镜像
from config.huggingface_mirror import setup_huggingface_mirror
setup_huggingface_mirror()
```

### 离线部署

1. 下载模型到本地目录
2. 配置本地模型路径
3. 完全离线运行

```python
embeddings = SentenceTransformersEmbeddings(
    model_name="/path/to/local/model"
)
```

## 🧪 测试

```bash
# 运行所有测试
python -m pytest tests/

# 运行特定测试
python -m pytest tests/test_sentence_transformers.py

# 运行性能测试
python tests/test_sentence_transformers.py --benchmark

# 运行RAG系统完整测试
python main.py --mode complete
```

## 📚 API文档

### SentenceTransformersEmbeddings

主要的嵌入模型类，提供文本嵌入功能。

#### 方法列表

- `embed_query(text: str) -> np.ndarray`: 生成查询嵌入
- `embed_documents(texts: List[str]) -> List[np.ndarray]`: 批量生成文档嵌入
- `test_embedding(text: str) -> Dict`: 测试单个文本嵌入
- `test_batch_embedding() -> Dict`: 测试批量嵌入性能

### CompleteRAGSystem

完整的RAG系统实现，集成检索和生成。

#### 方法列表

- `add_documents(documents: List[str])`: 添加文档到系统
- `query(question: str) -> Dict`: 执行RAG查询
- `search_documents(query: str, k: int = 4)`: 检索相关文档
- `clear_documents()`: 清空所有文档

### VectorStoreManager

向量存储管理器，支持多种向量数据库。

#### 方法列表

- `create_vector_store(documents, embeddings)`: 创建向量存储
- `similarity_search(query: str, k: int = 4)`: 执行相似度搜索
- `print_search_results(results)`: 格式化打印搜索结果

### DocumentLoader

文档加载器，支持多种文档格式。

#### 方法列表

- `load_text_documents(directory: str = "./data")`: 加载文本文档
- `create_test_documents()`: 创建测试文档
- `load_pdf_documents(file_path: str)`: 加载PDF文档

## 🔍 故障排除

### 常见问题

1. **模型下载失败**
   - 检查网络连接
   - 使用国内镜像：`HF_ENDPOINT=https://hf-mirror.com`
   - 手动下载模型文件

2. **内存不足**
   - 使用更小的模型
   - 减小batch_size
   - 使用CPU而非GPU

3. **性能问题**
   - 启用GPU加速
   - 使用批量处理
   - 实现缓存机制

4. **中文支持**
   - 使用中文优化模型：`shibing624/text2vec-base-chinese`
   - 确保文本编码正确

5. **API密钥错误**
   - 检查.env文件配置
   - 验证API密钥有效性

### 调试模式

```python
import logging
logging.basicConfig(level=logging.DEBUG)

# 启用详细日志
import os
os.environ["TRANSFORMERS_VERBOSITY"] = "detailed"
```

## 🎯 最佳实践

### 1. 文档预处理

```python
# 清理和预处理文档
documents = [
    doc.strip() for doc in documents
    if len(doc.strip()) > 50  # 过滤太短的文档
]
```

### 2. 查询优化

```python
# 查询扩展和重写
expanded_query = f"{query} 相关概念 实际应用"
response = rag_system.query(expanded_query)
```

### 3. 结果后处理

```python
# 对答案进行后处理
answer = response['answer']
if len(answer) > 500:
    # 截断过长的答案
    answer = answer[:500] + "..."
```

## 🤝 贡献指南

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 开发规范

- 遵循PEP 8代码风格
- 添加适当的文档字符串
- 编写单元测试
- 更新相关文档

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

- [Sentence-Transformers](https://github.com/UKPLab/sentence-transformers) - 优秀的句子嵌入库
- [LangChain](https://github.com/langchain-ai/langchain) - 强大的LLM应用框架
- [ChromaDB](https://github.com/chroma-core/chroma) - 轻量级向量数据库
- [FAISS](https://github.com/facebookresearch/faiss) - 高效的相似度搜索库
- [DeepSeek](https://github.com/deepseek-ai) - 高性能大语言模型

## 📞 联系方式

如有问题或建议，请通过以下方式联系：

- 提交 [Issue](https://github.com/your-username/rag/issues)
- 发送邮件至 your-email@example.com
- 查看 [Wiki](https://github.com/your-username/ag/wiki) 获取更多文档

---

**注意**：本系统仍在开发中，某些功能可能发生变化。建议在生产环境使用前进行充分测试。

**更新日志**：
- v1.0.0 - 初始版本，支持基础RAG功能
- v1.1.0 - 添加DeepSeek LLM支持
- v1.2.0 - 优化性能，添加批量处理
- v1.3.0 - 增加多向量数据库支持