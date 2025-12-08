# Sentence-Transformers 部署指南

## 概述
Sentence-Transformers 是一个强大的Python库，用于生成高质量的文本嵌入向量。它特别适合在本地部署，提供以下优势：
- 完全离线运行，保护数据隐私
- 无需API密钥
- 支持多种预训练模型
- 批量处理效率高

## 1. 安装依赖

```bash
# 基础安装
pip install sentence-transformers torch

# 可选：GPU支持（需要CUDA）
pip install sentence-transformers torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

# 额外依赖（如果需要）
pip install scikit-learn  # 用于相似度计算
pip install numpy        # 数值计算
pip install tqdm         # 进度条
```

## 2. 基础使用示例

### 2.1 简单嵌入生成

```python
from sentence_transformers import SentenceTransformer

# 加载预训练模型
model = SentenceTransformer('all-MiniLM-L6-v2')

# 生成嵌入
sentences = [
    "这是一个测试句子",
    "这是另一个测试句子",
    "完全不同的内容"
]
embeddings = model.encode(sentences)

print(f"嵌入形状: {embeddings.shape}")
# 输出: 嵌入形状: (3, 384)  # 3个句子，每个384维
```

### 2.2 中文优化模型

```python
# 中文优化模型
chinese_model = SentenceTransformer('shibing624/text2vec-base-chinese')

# 多语言模型
multilingual_model = SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')
```

## 3. 推荐的模型

### 3.1 轻量级模型（适合快速部署）
- `all-MiniLM-L6-v2` - 384维，英文
- `paraphrase-multilingual-MiniLM-L12-v2` - 384维，多语言

### 3.2 中等质量模型
- `all-mpnet-base-v2` - 768维，英文
- `shibing624/text2vec-base-chinese` - 768维，中文优化

### 3.3 高质量模型（需要更多资源）
- `paraphrase-multilingual-mpnet-base-v2` - 768维，多语言
- `stsb-roberta-large` - 1024维，英文

## 4. 性能优化

### 4.1 批量处理

```python
# 批量处理更高效
texts = ["文本1", "文本2", "文本3", ...]  # 大量文本
embeddings = model.encode(texts, batch_size=32, show_progress_bar=True)
```

### 4.2 GPU加速

```python
import torch

# 检查GPU是否可用
device = 'cuda' if torch.cuda.is_available() else 'cpu'
model = model.to(device)

# 使用GPU生成嵌入
embeddings = model.encode(texts, device=device)
```

### 4.3 缓存机制

```python
import pickle
import os

def cache_embeddings(texts, cache_file='embeddings_cache.pkl'):
    if os.path.exists(cache_file):
        with open(cache_file, 'rb') as f:
            return pickle.load(f)

    embeddings = model.encode(texts)
    with open(cache_file, 'wb') as f:
        pickle.dump(embeddings, f)

    return embeddings
```

## 5. 集成到RAG系统

### 5.1 创建嵌入类

```python
class LocalSentenceEmbeddings:
    def __init__(self, model_name='paraphrase-multilingual-MiniLM-L12-v2'):
        from sentence_transformers import SentenceTransformer
        self.model = SentenceTransformer(model_name)

    def embed_query(self, text):
        return self.model.encode(text)

    def embed_documents(self, texts):
        return self.model.encode(texts, batch_size=32)
```

### 5.2 与Chromadb集成

```python
from langchain_community.vectorstores import Chroma

# 使用本地嵌入
embeddings = LocalSentenceEmbeddings()

# 创建向量存储
vector_store = Chroma.from_documents(
    documents=documents,
    embedding=embeddings,
    persist_directory="./local_vector_store"
)
```

## 6. 模型下载和管理

### 6.1 离线部署

对于无法联网的环境，可以：

1. **提前下载模型**：
   ```python
   # 在有网络的环境下载
   model = SentenceTransformer('model-name')
   model.save('./local_model_directory')
   ```

2. **离线加载**：
   ```python
   # 在离线环境加载
   model = SentenceTransformer('./local_model_directory')
   ```

### 6.2 使用镜像

配置Hugging Face镜像加速下载：

```python
import os
os.environ['HF_ENDPOINT'] = 'https://hf-mirror.com'
```

## 7. 实际应用建议

### 7.1 模型选择策略

1. **原型阶段**：使用轻量级模型如 `all-MiniLM-L6-v2`
2. **生产环境**：根据语言选择：
   - 英文为主：`all-mpnet-base-v2`
   - 中文为主：`shibing624/text2vec-base-chinese`
   - 多语言：`paraphrase-multilingual-mpnet-base-v2`

### 7.2 资源需求

| 模型 | 大小 | RAM需求 | GPU需求 |
|------|------|---------|---------|
| MiniLM-L6 | 90MB | 1GB | 2GB |
| MiniLM-L12 | 420MB | 2GB | 4GB |
| MPNet-Base | 420MB | 2GB | 4GB |
| Roberta-Large | 1.3GB | 4GB | 8GB |

### 7.3 性能指标

在标准测试集上的表现：
- MiniLM-L6: ~0.85 cosine similarity
- MPNet-Base: ~0.88 cosine similarity
- Chinese-text2vec: ~0.87 cosine similarity (中文)

## 8. 故障排除

### 8.1 常见问题

1. **内存不足**：
   - 使用更小的模型
   - 减小batch_size
   - 增加交换内存

2. **下载失败**：
   ```bash
   # 使用代理
   export HF_ENDPOINT=https://hf-mirror.com

   # 或手动下载模型文件
   ```

3. **CUDA错误**：
   ```python
   # 强制使用CPU
   device = 'cpu'
   model = model.to(device)
   ```

## 9. 高级用法

### 9.1 自定义微调

```python
from sentence_transformers import losses

# 准备训练数据
train_examples = [
    InputExample(texts=['句子1', '句子2'], label=1.0),
    InputExample(texts=['句子1', '句子3'], label=0.0),
]

# 定义损失函数
train_loss = losses.CosineSimilarityLoss(model=model)

# 微调模型
model.fit(
    train_objectives=[(train_dataloader, train_loss)],
    epochs=3,
    warmup_steps=100
)
```

### 9.2 语义搜索

```python
from sentence_transformers import util

# 语义搜索
query_embedding = model.encode(query, convert_to_tensor=True)
passage_embeddings = model.encode(passages, convert_to_tensor=True)

# 计算相似度
cos_scores = util.cos_sim(query_embedding, passage_embeddings)

# 获取Top-K结果
top_results = util.semantic_search(query_embedding, passage_embeddings, top_k=3)
```

## 10. 总结

Sentence-Transformers提供了完整的本地嵌入解决方案：

**优点**：
- 数据隐私保护
- 无API限制
- 成本效益高（一次性投入）
- 可定制化

**缺点**：
- 需要本地计算资源
- 初始模型下载时间
- 需要自己维护和更新

**适用场景**：
- 对数据隐私要求高的应用
- 大量文本处理需求
- 需要离线运行的环境
- 长期运行的项目

通过合理选择模型和优化策略，可以在保证质量的同时获得良好的性能表现。