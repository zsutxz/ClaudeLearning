# RAG系统 - 完整部署指南

本项目提供了多种RAG（Retrieval-Augmented Generation）系统的实现方案，从简单的本地嵌入到专业的Sentence-Transformers部署。

## 📋 目录

- [项目概述](#项目概述)
- [环境准备](#环境准备)
- [快速开始](#快速开始)
- [部署方案](#部署方案)
- [示例代码](#示例代码)
- [性能对比](#性能对比)
- [常见问题](#常见问题)
- [进阶用法](#进阶用法)

## 🎯 项目概述

本项目包含多种RAG实现方案：

1. **DeepSeek API方案** (`test_deepseek_rag.py`) - 使用DeepSeek的聊天API
2. **TF-IDF本地方案** (`test_local_embedding_simple.py`) - 轻量级本地实现
3. **Sentence-Transformers方案** (`test_sentence_transformers_rag.py`) - 专业的本地嵌入
4. **完整RAG系统** (`src/rag_system_final.py`) - 支持多种向量数据库的完整实现

## 🚀 特性

- **多种向量数据库支持**：Chroma, FAISS, Pinecone, Weaviate, Qdrant
- **多种嵌入模型**：OpenAI, HuggingFace, Sentence-Transformers, TF-IDF
- **灵活的检索策略**：语义搜索、混合搜索、多查询检索、上下文压缩
- **完整的文档处理**：支持PDF、Markdown、Word、CSV等格式
- **本地部署选项**：完全离线运行，保护数据隐私
- **易于使用**：简洁的API和详细的文档

## 📁 项目结构

```
rag/
├── src/                       # 源代码
│   ├── rag_system_final.py    # 完整RAG系统实现
│   ├── simple_rag.py          # 简化版RAG系统
│   └── evaluation.py          # 评估模块
├── data/                      # 数据目录
│   └── sample_documents/      # 示例文档
├── test_deepseek_rag.py       # DeepSeek API版本
├── test_local_embedding_simple.py  # TF-IDF本地版本
├── test_sentence_transformers_rag.py # Sentence-Transformers版本
├── test_openai_rag.py         # OpenAI版本测试
├── deploy_sentence_transformers_guide.md  # 详细部署指南
└── README.md                  # 项目说明
```

## 🔧 环境准备

### 基础依赖

```bash
# 必需的Python包
pip install langchain langchain-community chromadb
pip install openai python-dotenv numpy scikit-learn
```

### Sentence-Transformers 依赖

```bash
# 安装sentence-transformers和PyTorch
pip install sentence-transformers torch

# GPU支持（可选，提升性能）
pip install sentence-transformers torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118
```

### 环境变量配置

创建 `.env` 文件：

```env
# DeepSeek API配置
DEEPSEEK_API_KEY=your_deepseek_api_key_here
DEEPSEEK_BASE_URL=https://api.deepseek.com

# OpenAI API（如果需要）
OPENAI_API_KEY=your_openai_api_key_here

# Hugging Face镜像（国内加速）
HF_ENDPOINT=https://hf-mirror.com
```

## 🚀 快速开始

### 1. 测试本地TF-IDF方案（无需网络）

```bash
python test_local_embedding_simple.py
```

**特点**：
- ✅ 完全本地运行
- ✅ 无需下载模型
- ✅ 适合快速验证

### 2. 测试DeepSeek API方案

```bash
python test_deepseek_rag.py
```

**注意**：需要配置DeepSeek API密钥

### 3. 测试Sentence-Transformers方案（推荐）

```bash
python test_sentence_transformers_rag.py
```

**首次运行**会自动下载模型，请耐心等待。

### 4. 使用完整RAG系统

```bash
# 离线版本
python test_offline_rag.py

# OpenAI版本（需要API密钥）
export OPENAI_API_KEY=your_key_here
python test_openai_rag.py
```

## 📦 部署方案

### 方案一：Sentence-Transformers（推荐）

这是最适合生产环境的方案，提供高质量的语义理解。

#### 1.1 模型选择

| 模型名称 | 大小 | 特点 | 适用场景 |
|---------|------|------|---------|
| `shibing624/text2vec-base-chinese` | 420MB | 中文优化 | 中文为主的应用 |
| `paraphrase-multilingual-MiniLM-L12-v2` | 420MB | 多语言轻量 | 国际化应用 |
| `all-mpnet-base-v2` | 420MB | 英文高质量 | 英文应用 |
| `paraphrase-multilingual-mpnet-base-v2` | 1.1GB | 多语言高质量 | 对质量要求高的场景 |

#### 1.2 实现代码

```python
from sentence_transformers import SentenceTransformer
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity

class SentenceTransformersEmbeddings:
    """专业的本地嵌入实现"""

    def __init__(self, model_name="paraphrase-multilingual-MiniLM-L12-v2"):
        print(f"加载模型: {model_name}")
        self.model = SentenceTransformer(model_name)

    def embed_query(self, text):
        """生成查询嵌入"""
        return self.model.encode(text)

    def embed_documents(self, texts, batch_size=32):
        """批量生成文档嵌入"""
        return self.model.encode(texts, batch_size=batch_size)

    def similarity_search(self, query, documents, k=3):
        """语义搜索"""
        query_emb = self.embed_query(query)
        doc_embs = self.embed_documents([doc.page_content for doc in documents])

        # 计算相似度
        similarities = cosine_similarity([query_emb], doc_embs)[0]

        # 获取Top-K结果
        top_indices = np.argsort(similarities)[::-1][:k]

        results = []
        for idx in top_indices:
            results.append({
                'document': documents[idx],
                'similarity': similarities[idx]
            })

        return results

# 使用示例
embeddings = SentenceTransformersEmbeddings("shibing624/text2vec-base-chinese")
results = embeddings.similarity_search("什么是RAG？", documents)
```

#### 1.3 集成到ChromaDB

```python
from langchain_community.vectorstores import Chroma

# 创建向量存储
vector_store = Chroma.from_documents(
    documents=documents,
    embedding=embeddings,  # 使用上面的embeddings实例
    persist_directory="./chroma_store"
)

# 搜索
results = vector_store.similarity_search("查询文本", k=5)
```

### 方案二：TF-IDF本地方案

适合快速原型开发和资源受限的环境。

```python
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity

class LocalTfidfEmbeddings:
    """轻量级TF-IDF嵌入"""

    def __init__(self, max_features=1000):
        self.vectorizer = TfidfVectorizer(
            max_features=max_features,
            ngram_range=(1, 2)
        )
        self.fitted = False

    def fit(self, documents):
        """训练TF-IDF"""
        texts = [doc.page_content for doc in documents]
        self.vectorizer.fit(texts)
        self.fitted = True

    def search(self, query, documents, k=3):
        """搜索相关文档"""
        if not self.fitted:
            self.fit(documents)

        # 转换查询
        query_vec = self.vectorizer.transform([query])

        # 转换文档
        doc_texts = [doc.page_content for doc in documents]
        doc_vecs = self.vectorizer.transform(doc_texts)

        # 计算相似度
        similarities = cosine_similarity(query_vec, doc_vecs)[0]

        # 返回结果
        results = []
        for idx in np.argsort(similarities)[::-1][:k]:
            results.append({
                'document': documents[idx],
                'similarity': similarities[idx]
            })

        return results
```

### 方案三：API混合方案

结合本地嵌入和云端LLM的优势。

```python
import os
from openai import OpenAI

class HybridRAGSystem:
    """混合RAG系统：本地嵌入 + 云端LLM"""

    def __init__(self, embedding_model, api_key, base_url):
        self.embeddings = embedding_model
        self.client = OpenAI(
            api_key=api_key,
            base_url=base_url
        )

    def query(self, question, documents, k=3):
        """完整查询流程"""
        # 1. 检索相关文档
        retrieved_docs = self.embeddings.similarity_search(
            question, documents, k
        )

        # 2. 构建提示
        context = "\n".join([
            doc['document'].page_content for doc in retrieved_docs
        ])

        prompt = f"""基于以下信息回答问题：

信息：
{context}

问题：{question}

回答："""

        # 3. 调用LLM生成答案
        response = self.client.chat.completions.create(
            model="deepseek-chat",
            messages=[{"role": "user", "content": prompt}],
            max_tokens=500
        )

        return {
            "answer": response.choices[0].message.content,
            "sources": retrieved_docs
        }
```

## 📊 性能对比

| 方案 | 嵌入质量 | 速度 | 成本 | 隐私 | 部署难度 |
|------|---------|------|------|------|---------|
| Sentence-Transformers | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | 低 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| TF-IDF | ⭐⭐ | ⭐⭐⭐⭐⭐ | 最低 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| OpenAI API | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | 高 | ⭐ | ⭐⭐⭐⭐⭐ |
| DeepSeek API | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | 中 | ⭐ | ⭐⭐⭐⭐ |

## 🔧 高级配置

### GPU加速

```python
import torch

# 检查GPU
device = 'cuda' if torch.cuda.is_available() else 'cpu'
print(f"使用设备: {device}")

# 加载模型到GPU
model = SentenceTransformer(model_name)
model = model.to(device)

# 使用GPU编码
embeddings = model.encode(texts, device=device)
```

### 批量处理优化

```python
def batch_encode(texts, batch_size=32):
    """高效的批量编码"""
    embeddings = []
    for i in range(0, len(texts), batch_size):
        batch = texts[i:i+batch_size]
        batch_emb = model.encode(batch, show_progress_bar=True)
        embeddings.extend(batch_emb)
    return embeddings
```

### 缓存机制

```python
import pickle
import hashlib

class CachedEmbeddings:
    """带缓存的嵌入系统"""

    def __init__(self, model, cache_dir="./cache"):
        self.model = model
        self.cache_dir = Path(cache_dir)
        self.cache_dir.mkdir(exist_ok=True)

    def get_cache_path(self, text):
        """生成缓存路径"""
        hash_key = hashlib.md5(text.encode()).hexdigest()
        return self.cache_dir / f"{hash_key}.pkl"

    def encode(self, text):
        """带缓存的编码"""
        cache_path = self.get_cache_path(text)

        # 尝试从缓存读取
        if cache_path.exists():
            with open(cache_path, 'rb') as f:
                return pickle.load(f)

        # 生成新的嵌入
        embedding = self.model.encode(text)

        # 保存到缓存
        with open(cache_path, 'wb') as f:
            pickle.dump(embedding, f)

        return embedding
```

## 🌐 网络问题解决方案

### 使用国内镜像

```python
import os
# 使用Hugging Face镜像
os.environ['HF_ENDPOINT'] = 'https://hf-mirror.com'
```

### 离线部署

1. **在有网络的环境下载模型**：
```python
model = SentenceTransformer('model-name')
model.save('./local_model')
```

2. **打包到离线环境**：
```bash
tar -czf sentence_transformers_model.tar.gz ./local_model
```

3. **离线环境加载**：
```python
model = SentenceTransformer('./local_model')
```

## 📚 详细功能

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

# 本地Sentence-Transformers
from sentence_transformers import SentenceTransformer
model = SentenceTransformer('shibing624/text2vec-base-chinese')
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

## ❓ 常见问题

### Q1: 模型下载失败怎么办？

**解决方案**：
1. 使用国内镜像：`export HF_ENDPOINT=https://hf-mirror.com`
2. 手动下载模型文件
3. 使用代理

### Q2: 内存不足如何处理？

**解决方案**：
1. 使用更小的模型（如MiniLM）
2. 减小batch_size
3. 增加虚拟内存
4. 使用CPU而非GPU

### Q3: 如何选择合适的模型？

**选择指南**：
- 中文为主 → `shibing624/text2vec-base-chinese`
- 多语言需求 → `paraphrase-multilingual-MiniLM-L12-v2`
- 追求质量 → `paraphrase-multilingual-mpnet-base-v2`
- 资源受限 → `all-MiniLM-L6-v2`

### Q4: 如何提高检索准确率？

**优化方法**：
1. 使用更高质量的嵌入模型
2. 增加文档数量和质量
3. 优化查询语句
4. 使用混合检索（BM25 + 向量）

## 🚀 进阶用法

### 微调自定义模型

```python
from sentence_transformers import SentenceTransformer, losses
from sentence_transformers.readers import InputExample

# 准备训练数据
train_examples = [
    InputExample(texts=['句子1', '相似句子'], label=1.0),
    InputExample(texts=['句子1', '不相似句子'], label=0.0),
]

# 加载预训练模型
model = SentenceTransformer('base-model')

# 定义损失函数
train_loss = losses.CosineSimilarityLoss(model)

# 微调
model.fit(
    train_objectives=[(train_dataloader, train_loss)],
    epochs=3,
    warmup_steps=100,
    output_path='./fine-tuned-model'
)
```

### 多模态RAG

```python
from sentence_transformers import SentenceTransformer, util
from PIL import Image

# 加载多模态模型
model = SentenceTransformer('clip-ViT-B-32')

# 图像和文本嵌入
image_emb = model.encode(Image.open('image.jpg'))
text_emb = model.encode('图片描述')

# 计算相似度
similarity = util.cos_sim(image_emb, text_emb)
```

## 📝 最佳实践

1. **生产环境部署**
   - 使用专业的嵌入模型
   - 实现缓存机制
   - 监控性能指标
   - 定期更新模型

2. **性能优化**
   - GPU加速批量处理
   - 合理的batch_size
   - 向量化操作
   - 异步处理

3. **安全考虑**
   - 本地部署保护数据隐私
   - API密钥安全管理
   - 输入内容过滤
   - 访问权限控制

## 📚 相关资源

- [Sentence-Transformers官方文档](https://www.sbert.net/)
- [Hugging Face模型库](https://huggingface.co/models)
- [LangChain文档](https://python.langchain.com/)
- [ChromaDB文档](https://docs.trychroma.com/)
- [详细部署指南](./deploy_sentence_transformers_guide.md)

## 🤝 贡献

欢迎提交问题和改进建议！

## 📄 许可证

本项目采用 MIT 许可证。