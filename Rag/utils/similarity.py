#!/usr/bin/env python3
"""
相似度计算工具
提供各种相似度计算方法
"""

import numpy as np
from typing import List, Tuple, Dict, Any


class SimilarityCalculator:
    """相似度计算器"""

    @staticmethod
    def cosine_similarity(a: List[float], b: List[float]) -> float:
        """
        计算余弦相似度

        Args:
            a: 向量a
            b: 向量b

        Returns:
            余弦相似度值（-1到1之间）
        """
        a = np.array(a)
        b = np.array(b)
        return np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b))

    @staticmethod
    def euclidean_distance(a: List[float], b: List[float]) -> float:
        """
        计算欧几里得距离

        Args:
            a: 向量a
            b: 向量b

        Returns:
            欧几里得距离
        """
        a = np.array(a)
        b = np.array(b)
        return np.linalg.norm(a - b)

    @staticmethod
    def manhattan_distance(a: List[float], b: List[float]) -> float:
        """
        计算曼哈顿距离

        Args:
            a: 向量a
            b: 向量b

        Returns:
            曼哈顿距离
        """
        a = np.array(a)
        b = np.array(b)
        return np.sum(np.abs(a - b))

    @staticmethod
    def dot_product(a: List[float], b: List[float]) -> float:
        """
        计算点积

        Args:
            a: 向量a
            b: 向量b

        Returns:
            点积值
        """
        a = np.array(a)
        b = np.array(b)
        return np.dot(a, b)

    @staticmethod
    def similarity_matrix(embeddings: List[List[float]]) -> np.ndarray:
        """
        计算嵌入向量之间的相似度矩阵

        Args:
            embeddings: 嵌入向量列表

        Returns:
            相似度矩阵
        """
        n = len(embeddings)
        matrix = np.zeros((n, n))

        for i in range(n):
            for j in range(n):
                if i == j:
                    matrix[i][j] = 1.0
                else:
                    matrix[i][j] = SimilarityCalculator.cosine_similarity(
                        embeddings[i], embeddings[j]
                    )

        return matrix

    @staticmethod
    def find_most_similar(
        query_embedding: List[float],
        document_embeddings: List[List[float]],
        top_k: int = 5
    ) -> List[Tuple[int, float]]:
        """
        找到与查询向量最相似的文档

        Args:
            query_embedding: 查询嵌入向量
            document_embeddings: 文档嵌入向量列表
            top_k: 返回的最相似文档数量

        Returns:
            (文档索引, 相似度分数) 元组列表
        """
        similarities = []
        for i, doc_emb in enumerate(document_embeddings):
            sim = SimilarityCalculator.cosine_similarity(query_embedding, doc_emb)
            similarities.append((i, sim))

        # 按相似度降序排序
        similarities.sort(key=lambda x: x[1], reverse=True)

        return similarities[:top_k]

    @staticmethod
    def analyze_text_similarities(
        embeddings,
        texts: List[str]
    ) -> Dict[str, Any]:
        """
        分析文本之间的相似度

        Args:
            embeddings: 嵌入模型实例
            texts: 文本列表

        Returns:
            相似度分析结果
        """
        # 生成所有文本的嵌入
        text_embeddings = embeddings.embed_documents(texts)

        # 计算相似度矩阵
        similarity_matrix = SimilarityCalculator.similarity_matrix(text_embeddings)

        # 找到最相似和最不相似的文本对
        max_sim = -1
        min_sim = 1
        max_pair = (0, 0)
        min_pair = (0, 0)

        n = len(texts)
        for i in range(n):
            for j in range(i + 1, n):
                sim = similarity_matrix[i][j]
                if sim > max_sim:
                    max_sim = sim
                    max_pair = (i, j)
                if sim < min_sim:
                    min_sim = sim
                    min_pair = (i, j)

        return {
            "text_count": n,
            "similarity_matrix": similarity_matrix.tolist(),
            "most_similar_pair": {
                "indices": max_pair,
                "texts": [texts[max_pair[0]], texts[max_pair[1]]],
                "similarity": float(max_sim)
            },
            "least_similar_pair": {
                "indices": min_pair,
                "texts": [texts[min_pair[0]], texts[min_pair[1]]],
                "similarity": float(min_sim)
            },
            "average_similarity": float(np.mean(similarity_matrix[np.triu_indices(n, k=1)]))
        }

    @staticmethod
    def print_similarity_analysis(analysis_result: Dict[str, Any], texts: List[str]):
        """
        打印相似度分析结果

        Args:
            analysis_result: 相似度分析结果
            texts: 原始文本列表
        """
        print("\n=== 相似度分析结果 ===")

        most_similar = analysis_result["most_similar_pair"]
        least_similar = analysis_result["least_similar_pair"]
        avg_sim = analysis_result["average_similarity"]

        print(f"\n最相似的文本对 (相似度: {most_similar['similarity']:.3f}):")
        print(f"文本 1: {most_similar['texts'][0]}")
        print(f"文本 2: {most_similar['texts'][1]}")

        print(f"\n最不相似的文本对 (相似度: {least_similar['similarity']:.3f}):")
        print(f"文本 1: {least_similar['texts'][0]}")
        print(f"文本 2: {least_similar['texts'][1]}")

        print(f"\n平均相似度: {avg_sim:.3f}")

        # 打印相似度矩阵
        print("\n相似度矩阵:")
        matrix = analysis_result["similarity_matrix"]
        n = len(matrix)
        print("       ", end="")
        for i in range(min(n, 5)):
            print(f"文本{i+1:6}", end="")
        print()

        for i in range(min(n, 5)):
            print(f"文本{i+1:3}", end="  ")
            for j in range(min(n, 5)):
                print(f"{matrix[i][j]:6.3f}", end="  ")
            print()

        if n > 5:
            print("\n注: 只显示前5个文本的相似度矩阵")