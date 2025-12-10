#!/usr/bin/env python3
"""
RAG系统交互式演示
提供命令行界面来测试RAG系统的各种功能
"""

import os
import sys
import time
import json
from pathlib import Path

# 添加src目录到Python路径
sys.path.append(str(Path(__file__).parent / "src"))

# ANSI颜色代码
class Colors:
    RESET = '\033[0m'
    RED = '\033[91m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    BLUE = '\033[94m'
    PURPLE = '\033[95m'
    CYAN = '\033[96m'
    WHITE = '\033[97m'
    BOLD = '\033[1m'


def print_colored(text, color=Colors.WHITE):
    """打印彩色文本"""
    print(f"{color}{text}{Colors.RESET}")


def print_header(title):
    """打印标题"""
    print("\n" + "="*60)
    print_colored(title.center(60), Colors.BOLD + Colors.CYAN)
    print("="*60)


def print_info(message):
    """打印信息"""
    print_colored(f"ℹ️  {message}", Colors.BLUE)


def print_success(message):
    """打印成功信息"""
    print_colored(f"✅ {message}", Colors.GREEN)


def print_warning(message):
    """打印警告"""
    print_colored(f"⚠️  {message}", Colors.YELLOW)


def print_error(message):
    """打印错误"""
    print_colored(f"❌ {message}", Colors.RED)


def get_user_choice(prompt, options):
    """获取用户选择"""
    while True:
        print_colored(f"\n{prompt}", Colors.BOLD)
        for i, option in enumerate(options, 1):
            print(f"  {i}. {option}")

        try:
            choice = input("\n请选择 (输入数字): ").strip()
            if choice.isdigit():
                choice_num = int(choice)
                if 1 <= choice_num <= len(options):
                    return choice_num - 1
            print_error("无效选择，请重试")
        except KeyboardInterrupt:
            print("\n\n👋 再见！")
            sys.exit(0)


def get_user_input(prompt, default=None):
    """获取用户输入"""
    if default:
        full_prompt = f"{prompt} [{default}]: "
    else:
        full_prompt = f"{prompt}: "

    try:
        response = input(full_prompt).strip()
        return response if response else default
    except KeyboardInterrupt:
        print("\n\n👋 再见！")
        sys.exit(0)


def create_rag_system_interactive():
    """交互式创建RAG系统"""
    print_header("创建RAG系统")

    # 数据路径
    data_path = get_user_input(
        "文档数据路径",
        "./data/sample_documents"
    )

    # 向量数据库选择
    vector_options = ["Chroma (轻量级)", "FAISS (高性能)", "Pinecone (云端)"]
    vector_choice = get_user_choice("选择向量数据库:", vector_options)
    vector_types = ["chroma", "faiss", "pinecone"]
    vector_type = vector_types[vector_choice]

    # 嵌入模型选择
    embed_options = ["OpenAI (推荐)", "HuggingFace (开源)", "Instructor (指令型)"]
    embed_choice = get_user_choice("选择嵌入模型:", embed_options)
    embed_types = ["openai", "huggingface", "instruct"]
    embed_type = embed_types[embed_choice]

    # 检索策略选择
    retrieval_options = [
        "语义搜索 (默认)",
        "混合搜索 (语义+关键词)",
        "多查询检索",
        "上下文压缩"
    ]
    retrieval_choice = get_user_choice("选择检索策略:", retrieval_options)
    retrieval_types = ["semantic", "hybrid", "multi_query", "contextual"]
    retrieval_type = retrieval_types[retrieval_choice]

    # 配置参数
    chunk_size = int(get_user_input("文档块大小", "1000"))
    top_k = int(get_user_input("检索返回文档数量", "5"))

    print_info("\n正在创建RAG系统...")

    try:
        from rag_system import RAGSystem, RAGConfig

        config = RAGConfig(
            data_path=data_path,
            vector_store_type=vector_type,
            embedding_model=embed_type,
            retrieval_strategy=retrieval_type,
            chunk_size=chunk_size,
            top_k=top_k
        )

        rag = RAGSystem(config)

        # 索引文档
        print_info("正在索引文档...")
        start_time = time.time()
        rag.index_documents()
        index_time = time.time() - start_time

        print_success(f"RAG系统创建完成！(耗时: {index_time:.2f}秒)")
        return rag

    except Exception as e:
        print_error(f"创建失败: {str(e)}")
        return None


def demo_basic_query(rag):
    """基础查询演示"""
    print_header("基础查询演示")

    while True:
        print("\n" + "-"*40)
        query = get_user_input("请输入您的问题 (输入 'back' 返回菜单)")

        if query.lower() == 'back':
            break

        if not query:
            continue

        print_info(f"\n🔍 查询: {query}")
        print("正在搜索...")

        try:
            start_time = time.time()
            result = rag.query(query, return_source=True)
            query_time = time.time() - start_time

            print(f"\n{Colors.GREEN}💬 回答:{Colors.RESET}")
            print(result['answer'])

            print(f"\n{Colors.CYAN}⏱️  响应时间: {query_time:.2f}秒{Colors.RESET}")

            if result.get('sources'):
                print(f"\n{Colors.YELLOW}📚 相关来源:{Colors.RESET}")
                for i, source in enumerate(result['sources'], 1):
                    print(f"\n来源 {i}:")
                    print(f"  文件: {source['metadata'].get('source', '未知')}")
                    print(f"  预览: {source['content'][:150]}...")

        except Exception as e:
            print_error(f"查询失败: {str(e)}")


def demo_conversational_mode(rag):
    """对话模式演示"""
    print_header("对话模式演示")
    print_info("这是一个连续对话模式，系统会记住之前的对话内容。")

    try:
        from rag_system import RAGConfig

        # 重新创建支持对话的RAG系统
        config = RAGConfig(retrieval_strategy="contextual")
        rag_conv = RAGSystem(config)
        rag_conv.index_documents()

        print("\n" + "-"*40)
        print_colored("开始对话吧！输入 'back' 返回主菜单", Colors.BOLD)

        while True:
            message = get_user_input("\n您:")

            if message.lower() == 'back':
                break

            if not message:
                continue

            try:
                start_time = time.time()
                response = rag_conv.chat(message)
                response_time = time.time() - start_time

                print(f"\n{Colors.GREEN}助手:{Colors.RESET} {response}")
                print(f"{Colors.CYAN}(响应时间: {response_time:.2f}秒){Colors.RESET}")

            except Exception as e:
                print_error(f"对话失败: {str(e)}")

    except Exception as e:
        print_error(f"对话模式初始化失败: {str(e)}")


def demo_batch_query(rag):
    """批量查询演示"""
    print_header("批量查询演示")

    # 预定义查询列表
    queries = [
        "什么是RAG系统？",
        "机器学习有哪些类型？",
        "深度学习的应用有哪些？",
        "如何评估RAG系统？",
        "向量数据库的选择？"
    ]

    print_info(f"将执行 {len(queries)} 个预设查询...")
    print("\n查询列表:")
    for i, query in enumerate(queries, 1):
        print(f"  {i}. {query}")

    if get_user_input("\n是否继续？ (y/n)", "y").lower() != 'y':
        return

    print("\n" + "-"*40)
    results = []

    for i, query in enumerate(queries, 1):
        print(f"\n查询 {i}/{len(queries)}: {query}")
        try:
            start_time = time.time()
            result = rag.query(query)
            query_time = time.time() - start_time

            answer_preview = result['answer'][:100] + "..." if len(result['answer']) > 100 else result['answer']
            print(f"回答预览: {answer_preview}")
            print(f"响应时间: {query_time:.2f}秒")

            results.append({
                "query": query,
                "answer": result['answer'],
                "response_time": query_time
            })

        except Exception as e:
            print_error(f"查询失败: {str(e)}")

    # 保存结果
    if get_user_input("\n是否保存结果？ (y/n)", "y").lower() == 'y':
        filename = f"batch_query_results_{int(time.time())}.json"
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(results, f, ensure_ascii=False, indent=2)
        print_success(f"结果已保存到: {filename}")


def demo_system_info(rag):
    """系统信息演示"""
    print_header("系统信息")

    # 获取统计信息
    stats = rag.get_stats()

    print("\n📊 系统配置:")
    for key, value in stats.get("config", {}).items():
        print(f"  {key}: {value}")

    # 向量存储信息
    if hasattr(rag, 'vector_store_manager') and rag.vector_store_manager.vector_store:
        print(f"\n📚 向量存储类型: {rag.config.vector_store_type}")

        # 尝试获取文档数量
        try:
            if rag.config.vector_store_type == "chroma":
                collection = rag.vector_store_manager.vector_store.get()
                doc_count = len(collection["ids"])
                print(f"📄 索引文档数: {doc_count}")
        except:
            print("📄 索引文档数: 无法获取")

    # 嵌入模型信息
    print(f"\n🧠 嵌入模型: {rag.config.embedding_model}")
    if rag.config.embedding_model == "openai":
        print(f"  模型名称: {rag.config.embedding_model_name}")

    # LLM信息
    print(f"\n💬 LLM模型: {rag.config.llm_model}")
    print(f"  温度参数: {rag.config.llm_temperature}")


def run_quick_demo():
    """快速演示（使用默认配置）"""
    print_header("快速演示")
    print_info("将使用默认配置创建RAG系统并演示基础功能...")

    try:
        from rag_system import create_rag_system

        print("正在创建RAG系统...")
        start_time = time.time()
        rag = create_rag_system(
            data_path="./data/sample_documents",
            vector_store_type="chroma",
            embedding_model="openai"
        )
        creation_time = time.time() - start_time
        print_success(f"RAG系统创建完成！(耗时: {creation_time:.2f}秒)")

        # 执行几个示例查询
        demo_queries = [
            "什么是RAG系统？",
            "机器学习有哪些主要类型？",
            "RAG系统有什么优势？"
        ]

        print("\n" + "-"*40)
        for query in demo_queries:
            print(f"\n{Colors.YELLOW}问题: {query}{Colors.RESET}")
            print("搜索中...")

            start_time = time.time()
            result = rag.query(query)
            query_time = time.time() - start_time

            print(f"\n{Colors.GREEN}回答: {Colors.RESET}{result['answer']}")
            print(f"{Colors.CYAN}响应时间: {query_time:.2f}秒{Colors.RESET}")

        return rag

    except Exception as e:
        print_error(f"快速演示失败: {str(e)}")
        return None


def main():
    """主程序"""
    print_colored("\n🚀 RAG系统交互式演示", Colors.BOLD + Colors.CYAN)
    print("="*60)
    print_info("这是一个RAG（检索增强生成）系统的交互式演示程序")

    # 检查API密钥
    if not os.getenv("OPENAI_API_KEY"):
        print_warning("\n未检测到OPENAI_API_KEY环境变量")
        print("请确保已设置API密钥，或创建.env文件添加API密钥")
        if get_user_input("是否继续？ (y/n)", "n").lower() != 'y':
            return

    rag = None

    while True:
        print("\n" + "="*60)
        print_colored("主菜单", Colors.BOLD)
        menu_options = [
            "快速演示（推荐首次使用）",
            "自定义创建RAG系统",
            "基础查询测试",
            "对话模式演示",
            "批量查询演示",
            "查看系统信息",
            "退出"
        ]

        choice = get_user_choice("\n请选择功能:", menu_options)

        if choice == 0:  # 快速演示
            rag = run_quick_demo()

        elif choice == 1:  # 自定义创建
            rag = create_rag_system_interactive()

        elif choice == 2:  # 基础查询
            if rag:
                demo_basic_query(rag)
            else:
                print_warning("请先创建RAG系统")

        elif choice == 3:  # 对话模式
            if rag:
                demo_conversational_mode(rag)
            else:
                print_warning("请先创建RAG系统")

        elif choice == 4:  # 批量查询
            if rag:
                demo_batch_query(rag)
            else:
                print_warning("请先创建RAG系统")

        elif choice == 5:  # 系统信息
            if rag:
                demo_system_info(rag)
            else:
                print_warning("请先创建RAG系统")

        elif choice == 6:  # 退出
            print("\n👋 感谢使用RAG系统演示！")
            break


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n👋 再见！")
    except Exception as e:
        print_error(f"程序错误: {str(e)}")