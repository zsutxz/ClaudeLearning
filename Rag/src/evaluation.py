"""
RAG系统评估模块
提供多种评估指标和方法
"""

import json
import numpy as np
from typing import List, Dict, Any, Tuple
from dataclasses import dataclass
import logging
from pathlib import Path

# 评估指标库
from rouge_score import rouge_scorer
from bert_score import score as bert_score
from sklearn.metrics import precision_recall_fscore_support
import nltk
from nltk.tokenize import word_tokenize

# 下载NLTK数据
try:
    nltk.data.find('tokenizers/punkt')
except LookupError:
    nltk.download('punkt')

logger = logging.getLogger(__name__)


@dataclass
class EvaluationResult:
    """评估结果数据类"""
    metric_name: str
    score: float
    details: Dict[str, Any] = None


class RAGEvaluator:
    """RAG系统评估器"""

    def __init__(self):
        self.rouge_scorer = rouge_scorer.RougeScorer(
            ['rouge1', 'rouge2', 'rougeL'],
            use_stemmer=True
        )

    def evaluate_answer_accuracy(self, predicted: str, expected: str) -> EvaluationResult:
        """评估答案准确性"""
        # 使用ROUGE评分
        scores = self.rouge_scorer.score(expected, predicted)

        # 综合ROUGE-L F1作为准确性分数
        accuracy = scores['rougeL'].fmeasure

        return EvaluationResult(
            metric_name="answer_accuracy",
            score=accuracy,
            details={
                "rouge1": scores['rouge1'].fmeasure,
                "rouge2": scores['rouge2'].fmeasure,
                "rougeL": scores['rougeL'].fmeasure
            }
        )

    def evaluate_retrieval_precision(self, retrieved_docs: List[str],
                                   relevant_docs: List[str], k: int = 5) -> EvaluationResult:
        """评估检索精度"""
        if not retrieved_docs or not relevant_docs:
            return EvaluationResult("retrieval_precision", 0.0)

        # 取前k个检索结果
        retrieved_k = retrieved_docs[:k]

        # 计算精确度：检索到的相关文档数 / 检索到的总文档数
        relevant_count = sum(1 for doc in retrieved_k if doc in relevant_docs)
        precision = relevant_count / min(len(retrieved_k), len(relevant_docs))

        return EvaluationResult(
            metric_name="retrieval_precision",
            score=precision,
            details={
                "relevant_retrieved": relevant_count,
                "total_retrieved": min(len(retrieved_k), k),
                "total_relevant": len(relevant_docs)
            }
        )

    def evaluate_retrieval_recall(self, retrieved_docs: List[str],
                                relevant_docs: List[str]) -> EvaluationResult:
        """评估检索召回率"""
        if not retrieved_docs or not relevant_docs:
            return EvaluationResult("retrieval_recall", 0.0)

        # 计算召回率：检索到的相关文档数 / 总相关文档数
        relevant_retrieved = sum(1 for doc in retrieved_docs if doc in relevant_docs)
        recall = relevant_retrieved / len(relevant_docs)

        return EvaluationResult(
            metric_name="retrieval_recall",
            score=recall,
            details={
                "relevant_retrieved": relevant_retrieved,
                "total_relevant": len(relevant_docs)
            }
        )

    def evaluate_answer_relevance(self, answer: str, query: str) -> float:
        """评估答案相关性（简化版）"""
        # 这里使用简单的关键词匹配作为示例
        # 实际应用中可以使用更复杂的语义相似度模型
        query_tokens = set(word_tokenize(query.lower()))
        answer_tokens = set(word_tokenize(answer.lower()))

        if not query_tokens:
            return 0.0

        # 计算Jaccard相似度
        intersection = len(query_tokens.intersection(answer_tokens))
        union = len(query_tokens.union(answer_tokens))

        return intersection / union if union > 0 else 0.0

    def evaluate_faithfulness(self, answer: str, context: List[str]) -> float:
        """评估答案的忠实度（基于上下文的程度）"""
        # 简化实现：检查答案中的词汇是否出现在上下文中
        context_text = " ".join(context).lower()
        answer_tokens = word_tokenize(answer.lower())

        if not answer_tokens:
            return 0.0

        # 计算出现在上下文中的词的比例
        context_tokens = set(word_tokenize(context_text))
        in_context = sum(1 for token in answer_tokens if token in context_tokens)

        return in_context / len(answer_tokens)

    def evaluate_bert_score(self, predictions: List[str],
                          references: List[str]) -> EvaluationResult:
        """使用BERT Score评估"""
        try:
            P, R, F1 = bert_score(
                predictions,
                references,
                lang="zh",
                verbose=False
            )

            return EvaluationResult(
                metric_name="bert_score",
                score=F1.mean().item(),
                details={
                    "precision": P.mean().item(),
                    "recall": R.mean().item(),
                    "f1": F1.mean().item()
                }
            )
        except Exception as e:
            logger.error(f"BERT Score计算失败: {str(e)}")
            return EvaluationResult("bert_score", 0.0)

    def evaluate_dataset(self, rag_system, test_data: List[Dict]) -> Dict[str, float]:
        """评估整个数据集"""
        results = {
            "answer_accuracy": [],
            "retrieval_precision": [],
            "retrieval_recall": [],
            "answer_relevance": [],
            "faithfulness": []
        }

        predictions = []
        references = []

        for i, test_case in enumerate(test_data):
            query = test_case["question"]
            expected_answer = test_case.get("expected_answer", "")
            relevant_docs = test_case.get("relevant_docs", [])

            # 获取RAG系统回答
            result = rag_system.query(query, return_source=True)
            predicted_answer = result["answer"]

            # 获取检索到的文档（简化处理）
            retrieved_docs = result.get("sources", [])
            retrieved_content = [doc.get("content", "") for doc in retrieved_docs]

            # 评估各项指标
            if expected_answer:
                acc_result = self.evaluate_answer_accuracy(predicted_answer, expected_answer)
                results["answer_accuracy"].append(acc_result.score)
                predictions.append(predicted_answer)
                references.append(expected_answer)

            if relevant_docs and retrieved_content:
                prec_result = self.evaluate_retrieval_precision(
                    retrieved_content, relevant_docs
                )
                rec_result = self.evaluate_retrieval_recall(
                    retrieved_content, relevant_docs
                )
                results["retrieval_precision"].append(prec_result.score)
                results["retrieval_recall"].append(rec_result.score)

            relevance = self.evaluate_answer_relevance(predicted_answer, query)
            results["answer_relevance"].append(relevance)

            if retrieved_content:
                faithfulness = self.evaluate_faithfulness(predicted_answer, retrieved_content)
                results["faithfulness"].append(faithfulness)

            if (i + 1) % 10 == 0:
                logger.info(f"已评估 {i + 1}/{len(test_data)} 个样本")

        # 计算平均分
        avg_results = {}
        for metric, scores in results.items():
            if scores:
                avg_results[f"avg_{metric}"] = np.mean(scores)
                avg_results[f"std_{metric}"] = np.std(scores)
            else:
                avg_results[f"avg_{metric}"] = 0.0
                avg_results[f"std_{metric}"] = 0.0

        # 添加BERT Score（如果可用）
        if predictions and references:
            bert_result = self.evaluate_bert_score(predictions, references)
            avg_results["avg_bert_score"] = bert_result.score

        return avg_results

    def generate_evaluation_report(self, results: Dict[str, float],
                                 output_path: str = None) -> str:
        """生成评估报告"""
        report = []
        report.append("=== RAG系统评估报告 ===\n")

        report.append("评估指标：\n")
        for metric, score in results.items():
            if "avg_" in metric:
                metric_name = metric.replace("avg_", "").replace("_", " ").title()
                std_metric = metric.replace("avg_", "std_")
                std_score = results.get(std_metric, 0)
                report.append(f"  {metric_name}: {score:.4f} (±{std_score:.4f})")

        report.append("\n指标说明：\n")
        report.append("- Answer Accuracy: 答案准确性（使用ROUGE-L评估）")
        report.append("- Retrieval Precision: 检索精度（检索到的相关文档比例）")
        report.append("- Retrieval Recall: 检索召回率（相关文档中被检索到的比例）")
        report.append("- Answer Relevance: 答案相关性（与查询的语义相似度）")
        report.append("- Faithfulness: 忠实度（答案基于上下文的程度）")
        report.append("- Bert Score: BERT相似度分数")

        # 添加评估建议
        report.append("\n改进建议：\n")

        if results.get("avg_retrieval_precision", 0) < 0.5:
            report.append("- 检索精度较低，建议：")
            report.append("  * 使用更好的嵌入模型")
            report.append("  * 实施混合搜索策略")
            report.append("  * 添加重排序机制")

        if results.get("avg_answer_accuracy", 0) < 0.5:
            report.append("- 答案准确性较低，建议：")
            report.append("  * 优化提示模板")
            report.append("  * 增加上下文窗口大小")
            report.append("  * 使用更强大的生成模型")

        if results.get("avg_faithfulness", 0) < 0.7:
            report.append("- 忠实度较低，建议：")
            report.append("  * 改进检索质量")
            report.append("  * 添加事实核查步骤")
            report.append("  * 优化上下文压缩")

        report_text = "\n".join(report)

        # 保存报告
        if output_path:
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write(report_text)
            logger.info(f"评估报告已保存到: {output_path}")

        return report_text


def create_sample_test_data() -> List[Dict]:
    """创建示例测试数据"""
    return [
        {
            "question": "什么是RAG系统？",
            "expected_answer": "RAG（检索增强生成）是一种结合信息检索和文本生成的AI架构，通过从知识库检索相关信息来生成答案。",
            "relevant_docs": ["检索增强生成", "RAG系统介绍", "Retrieval-Augmented Generation"]
        },
        {
            "question": "机器学习有哪些主要类型？",
            "expected_answer": "机器学习的主要类型包括监督学习、无监督学习和强化学习。",
            "relevant_docs": ["机器学习", "监督学习", "无监督学习", "强化学习"]
        },
        {
            "question": "RAG系统有什么优势？",
            "expected_answer": "RAG系统的优势包括减少幻觉、实时知识更新、可解释性和领域适应性强。",
            "relevant_docs": ["RAG优势", "检索增强生成", "AI系统对比"]
        },
        {
            "question": "深度学习使用什么类型的网络？",
            "expected_answer": "深度学习使用多层神经网络，包括CNN和RNN等。",
            "relevant_docs": ["深度学习", "神经网络", "机器学习"]
        }
    ]


def main():
    """主函数：运行评估示例"""
    import sys
    from pathlib import Path
    sys.path.append(str(Path(__file__).parent.parent))
    from rag_system import create_rag_system

    # 创建评估器
    evaluator = RAGEvaluator()

    # 创建或加载RAG系统
    print("加载RAG系统...")
    rag = create_rag_system(
        data_path="./data/sample_documents",
        vector_store_type="chroma",
        embedding_model="openai"
    )

    # 创建测试数据
    test_data = create_sample_test_data()

    # 运行评估
    print("开始评估...")
    results = evaluator.evaluate_dataset(rag, test_data)

    # 生成报告
    report = evaluator.generate_evaluation_report(
        results,
        output_path="./evaluation_report.txt"
    )

    print(report)


if __name__ == "__main__":
    main()