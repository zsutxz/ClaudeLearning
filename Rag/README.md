# RAG系统实现

一个完整的检索增强生成（RAG）系统实现，支持多种向量数据库、嵌入模型和检索策略。

## 🚀 特性

- **多种向量数据库支持**：Chroma, FAISS, Pinecone, Weaviate, Qdrant
- **多种嵌入模型**：OpenAI, HuggingFace, Instructor Embeddings
- **灵活的检索策略**：语义搜索、混合搜索、多查询检索、上下文压缩
- **完整的文档处理**：支持PDF、Markdown、Word、CSV等格式
- **高级RAG模式**：父文档检索、重排序、元数据过滤
- **评估系统**：多维度评估指标和报告生成
- **易于使用**：简洁的API和详细的文档

## 📁 项目结构

```
rag/
├── src/                    # 源代码
│   ├── rag_system_final.py # RAG系统核心实现（最终版本）
│   ├── simple_rag.py       # 简化版RAG系统
│   └── evaluation.py       # 评估模块
├── config/                 # 配置文件
│   └── config.yaml         # 主配置文件
├── data/                   # 数据目录
│   └── sample_documents/   # 示例文档
├── examples/               # 使用示例
│   ├── basic_usage.py      # 基础使用示例
│   └── advanced_usage.py   # 高级使用示例
├── tests/                  # 测试文件
├── test_offline_rag.py     # 离线版本测试（推荐）
├── test_openai_rag.py      # OpenAI版本测试
├── interactive_demo.py     # 交互式演示
├── benchmark.py            # 性能基准测试
├── requirements.txt        # 依赖包
├── .env.example           # 环境变量示例
└── README.md              # 项目说明
```

## 🛠️ 安装

1. 克隆项目并进入目录：
```bash
cd rag
```

2. 创建Python虚拟环境（推荐）：
```bash
python -m venv venv
source venv/bin/activate  # Linux/Mac
# 或
venv\Scripts\activate     # Windows
```

3. 安装依赖：
```bash
pip install -r requirements.txt
```

4. 配置API密钥：
```bash
cp .env.example .env
# 编辑.env文件，添加你的API密钥
```

## 🎯 快速开始

### 离线测试（无需API密钥）

```bash
# 运行离线版本测试
python test_offline_rag.py
```

### 使用API密钥的测试

```bash
# 设置OpenAI API密钥后运行
export OPENAI_API_KEY=your_key_here
python test_openai_rag.py
```

### 基础使用

```python
from src.rag_system_final import create_rag_system

# 创建RAG系统
rag = create_rag_system(
    data_path="./data/sample_documents",
    vector_store_type="chroma",
    embedding_model="openai"
)

# 查询
result = rag.query("什么是RAG系统？")
print(result['answer'])
```

### 高级配置

```python
from src.rag_system_final import RAGSystem, RAGConfig

# 自定义配置
config = RAGConfig(
    data_path="./data/documents",
    vector_store_type="pinecone",
    embedding_model="huggingface",
    chunk_size=1000,
    retrieval_strategy="hybrid",
    top_k=10
)

# 创建并使用RAG系统
rag = RAGSystem(config)
rag.index_documents()

# 对话模式
response = rag.chat("能详细解释一下机器学习吗？")
print(response)
```

## 📚 文件说明

### 核心测试文件（推荐使用顺序）

1. **`test_offline_rag.py`** - **离线版本**，无需API密钥，可直接运行
   - 使用TF-IDF模拟嵌入
   - 适合测试RAG系统基本流程
   - 不需要网络连接

2. **`test_openai_rag.py`** - OpenAI版本，需要API密钥
   - 使用真实的OpenAI嵌入模型
   - 需要设置OPENAI_API_KEY环境变量

### 系统实现

- **`src/rag_system_final.py`** - 完整的RAG系统实现（最终版本）
  - 支持多种向量数据库（Chroma, FAISS）
  - 支持多种嵌入模型（OpenAI, HuggingFace）
  - 包含文档处理、索引、检索功能

- **`src/simple_rag.py`** - 简化版RAG系统
  - 仅支持基本功能
  - 使用HuggingFace嵌入模型
  - 适合快速原型开发

- **`src/evaluation.py`** - RAG系统评估工具
  - 多维度评估指标
  - ROUGE、BERT Score等评估方法
  - 评估报告生成

### 示例和演示

- **`examples/basic_usage.py`** - 基础使用示例
- **`examples/advanced_usage.py`** - 高级使用示例
- **`interactive_demo.py`** - 交互式演示

### 其他

- **`benchmark.py`** - 性能基准测试
- **`tests/test_rag_system.py`** - 单元测试
- **`requirements.txt`** - 依赖列表

## 🔧 配置和数据

- **`.env.example`** - 环境变量示例
- **`config/config.yaml`** - 配置文件
- **`data/`** - 数据目录

## 📖 详细功能

### 1. 向量数据库选择

```python
# Chroma（轻量级，本地使用）
config = RAGConfig(vector_store_type="chroma")

# FAISS（高性能，本地）
config = RAGConfig(vector_store_type="faiss")

# Pinecone（云端，可扩展）
config = RAGConfig(
    vector_store_type="pinecone",
    pinecone_api_key="your-key",
    pinecone_environment="us-west1-gcp"
)

# Weaviate（混合搜索）
config = RAGConfig(vector_store_type="weaviate")
```

### 2. 嵌入模型选择

```python
# OpenAI
config = RAGConfig(embedding_model="openai", embedding_model_name="text-embedding-ada-002")

# HuggingFace Sentence Transformers
config = RAGConfig(
    embedding_model="huggingface",
    embedding_model_name="sentence-transformers/all-MiniLM-L6-v2"
)

# Instructor Embeddings
config = RAGConfig(
    embedding_model="instruct",
    embedding_model_name="hkunlp/instructor-large"
)
```

### 3. 检索策略

```python
# 语义搜索（默认）
config = RAGConfig(retrieval_strategy="semantic")

# 多查询检索（生成多个查询变体）
config = RAGConfig(retrieval_strategy="multi_query")

# 上下文压缩（只保留相关部分）
config = RAGConfig(retrieval_strategy="contextual")

# 混合搜索（语义+关键词）
config = RAGConfig(retrieval_strategy="hybrid")
```

### 4. 文档分割策略

```python
# 递归字符分割
config = RAGConfig(
    chunk_strategy="recursive",
    chunk_size=1000,
    chunk_overlap=200
)

# Token分割
config = RAGConfig(
    chunk_strategy="token",
    chunk_size=512,
    chunk_overlap=50
)

# 语义分割
config = RAGConfig(chunk_strategy="semantic")

# Markdown标题分割
config = RAGConfig(chunk_strategy="markdown")
```

## 🧪 评估系统

使用内置的评估系统来评估RAG性能：

```python
from src.evaluation import RAGEvaluator, create_sample_test_data

# 创建评估器
evaluator = RAGEvaluator()

# 加载或创建测试数据
test_data = create_sample_test_data()

# 评估系统
results = evaluator.evaluate_dataset(rag_system, test_data)

# 生成报告
report = evaluator.generate_evaluation_report(results)
print(report)
```

评估指标包括：
- **Answer Accuracy**: 答案准确性（ROUGE）
- **Retrieval Precision**: 检索精度
- **Retrieval Recall**: 检索召回率
- **Answer Relevance**: 答案相关性
- **Faithfulness**: 忠实度
- **BERT Score**: 语义相似度

## 📝 最佳实践

1. **文档预处理**：确保文档质量，去除无关内容
2. **分块优化**：根据文档类型选择合适的分块策略
3. **元数据增强**：添加有用的元数据便于过滤和检索
4. **定期评估**：使用评估系统持续优化性能
5. **监控日志**：关注系统运行状态和错误

## 🔧 扩展开发

### 添加新的向量数据库

1. 在 `rag_system_final.py` 的 `VectorStoreManager` 类中添加新的向量数据库支持
2. 在配置中添加相应的配置项

### 添加新的评估指标

1. 在 `evaluation.py` 的 `RAGEvaluator` 类中添加新的评估方法
2. 在 `evaluate_dataset` 方法中调用新的评估指标

### 添加新的文档加载器

1. 在 `DocumentProcessor` 类的 `load_documents` 方法中添加新的文件类型支持
2. 导入相应的LangChain文档加载器

## 🤝 贡献

欢迎提交Issue和Pull Request来改进这个项目。

## 📄 许可证

MIT License

## 🙏 致谢

- [LangChain](https://python.langchain.com/) - 强大的LLM开发框架
- [Chroma](https://www.trychroma.com/) - 开源向量数据库
- [Sentence Transformers](https://www.sbert.net/) - 优秀的嵌入模型库
- 所有开源社区的贡献者