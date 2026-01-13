#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
批量运行所有示例脚本

自动运行所有示例并收集结果。

运行方式:
    python run_all_examples.py
    # Windows 下如果遇到编码问题，使用:
    PYTHONIOENCODING=utf-8 python run_all_examples.py
"""

import sys
import subprocess
import time
import os
from pathlib import Path
from datetime import datetime

# Windows 控制台编码修复
if sys.platform == 'win32' and os.environ.get('PYTHONIOENCODING') != 'utf-8':
    print("提示: 如遇到编码问题，请使用以下命令运行:")
    print("PYTHONIOENCODING=utf-8 python run_all_examples.py")
    print()

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent
sys.path.insert(0, str(project_root))

from lib.config import get_config


def run_example(script_path: Path, timeout: int = 120) -> dict:
    """
    运行单个示例

    Args:
        script_path: 脚本路径
        timeout: 超时时间（秒）

    Returns:
        dict: 运行结果
    """
    if not script_path.exists():
        return {
            "name": str(script_path.relative_to(project_root)),
            "status": "skipped",
            "reason": "文件不存在"
        }

    script_name = str(script_path.relative_to(project_root))
    print(f"\n{'='*60}")
    print(f"🚀 运行: {script_name}")
    print(f"{'='*60}")

    start_time = time.time()

    try:
        result = subprocess.run(
            [sys.executable, str(script_path)],
            cwd=str(project_root),
            timeout=timeout,
            capture_output=True,
            text=True,
            encoding='utf-8',
            errors='replace',
        )

        elapsed = time.time() - start_time

        if result.returncode == 0:
            return {
                "name": script_name,
                "status": "success",
                "duration": elapsed,
                "stdout": result.stdout,
            }
        else:
            return {
                "name": script_name,
                "status": "failed",
                "duration": elapsed,
                "error": result.stderr,
                "returncode": result.returncode,
            }

    except subprocess.TimeoutExpired:
        return {
            "name": script_name,
            "status": "timeout",
            "duration": timeout,
        }
    except Exception as e:
        return {
            "name": script_name,
            "status": "error",
            "error": str(e),
        }


def print_summary(results: list):
    """打印运行结果摘要"""
    print("\n" + "="*60)
    print("📊 运行结果摘要")
    print("="*60)

    total = len(results)
    success = sum(1 for r in results if r["status"] == "success")
    failed = sum(1 for r in results if r["status"] in ["failed", "error"])
    timeout = sum(1 for r in results if r["status"] == "timeout")
    skipped = sum(1 for r in results if r["status"] == "skipped")

    print(f"\n总示例数: {total}")
    print(f"✅ 成功: {success}")
    print(f"❌ 失败: {failed}")
    print(f"⏱️  超时: {timeout}")
    print(f"⏭️  跳过: {skipped}")

    print("\n详细结果:")
    print("-" * 60)

    for r in results:
        status_icon = {
            "success": "✅",
            "failed": "❌",
            "error": "❌",
            "timeout": "⏱️",
            "skipped": "⏭️",
        }.get(r["status"], "❓")

        duration = f" ({r.get('duration', 0):.1f}s)" if "duration" in r else ""

        print(f"{status_icon} {r['name']}: {r['status'].upper()}{duration}")

        if r["status"] in ["failed", "error"] and "error" in r:
            print(f"   └─ 错误: {r['error'][:100]}...")


def discover_examples() -> dict:
    """
    自动扫描 examples 目录下的所有 Python 示例文件

    Returns:
        dict: 按目录分组的示例文件列表
    """
    examples_dir = project_root / "examples"
    categories = {}

    if not examples_dir.exists():
        print(f"❌ 警告: examples 目录不存在")
        return categories

    # 扫描所有子目录
    for category_dir in sorted(examples_dir.iterdir()):
        if not category_dir.is_dir():
            continue

        # 跳过 __pycache__ 等特殊目录
        if category_dir.name.startswith("__") or category_dir.name.startswith("."):
            continue

        category_name = category_dir.name
        examples = []

        # 扫描该目录下的所有 .py 文件
        for py_file in sorted(category_dir.glob("*.py")):
            # 跳过 __ 开头的文件
            if py_file.name.startswith("__"):
                continue

            examples.append(py_file)

        if examples:
            categories[category_name] = examples

    return categories


def print_menu(categories: dict) -> None:
    """打印示例选择菜单"""
    print("\n" + "="*60)
    print("📁 可用的示例目录:")
    print("="*60)

    idx = 1
    for category_name, examples in categories.items():
        print(f"\n{idx}. {category_name}/")
        for ex in examples:
            print(f"   └─ {ex.name}")
        idx += 1

    print(f"\n{idx}. 运行所有示例")
    print("="*60)


def main():
    """主函数"""
    print("""
╔══════════════════════════════════════════════════════════════╗
║                                                                ║
║          🤖 Claude Agent SDK - 批量测试运行器 🤖              ║
║                                                                ║
╚══════════════════════════════════════════════════════════════╝
""")

    # 检查配置
    config = get_config()
    if not config.anthropic_api_key:
        print("❌ 警告: 未设置 ANTHROPIC_API_KEY")
        print("提示: 即使没有 API 密钥，也可以使用 Mock 模型运行部分示例")

    # 扫描示例文件
    categories = discover_examples()

    if not categories:
        print("❌ 未找到任何示例文件")
        return

    # 打印菜单
    print_menu(categories)

    # 获取用户选择
    total_categories = len(categories)
    try:
        choice = input(f"\n请选择 (1-{total_categories + 1}): ").strip()
        choice_idx = int(choice) - 1
    except (ValueError, KeyboardInterrupt):
        print("\n已取消")
        return

    # 收集要运行的示例
    examples_to_run = []

    if choice_idx == total_categories:
        # 运行所有示例
        for examples in categories.values():
            examples_to_run.extend(examples)
        print(f"\n🚀 将运行所有 {len(examples_to_run)} 个示例...")
    elif 0 <= choice_idx < total_categories:
        # 运行选定分类的示例
        category_name = list(categories.keys())[choice_idx]
        examples_to_run = categories[category_name]
        print(f"\n🚀 将运行 {category_name} 分类的 {len(examples_to_run)} 个示例...")
    else:
        print("❌ 无效的选择")
        return

    # 确认
    response = input("\n是否继续？(y/n): ").strip().lower()
    if response != 'y':
        print("已取消")
        return

    # 运行所有示例
    results = []
    start_time = time.time()

    for example_path in examples_to_run:
        result = run_example(example_path)
        results.append(result)
        time.sleep(0.5)  # 示例之间间隔

    total_time = time.time() - start_time

    # 打印摘要
    print_summary(results)

    print(f"\n⏱️  总耗时: {total_time:.1f}秒")
    print("\n" + "="*60)

    # 保存报告
    report_file = project_root / "test_report.txt"
    with open(report_file, "w", encoding="utf-8") as f:
        f.write(f"Claude Agent SDK 示例测试报告\n")
        f.write(f"测试时间: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
        f.write(f"总耗时: {total_time:.1f}秒\n\n")
        f.write("="*60 + "\n\n")

        for r in results:
            f.write(f"{r['name']}: {r['status']}\n")
            if "stdout" in r:
                f.write(f"输出:\n{r['stdout']}\n")
            if "error" in r:
                f.write(f"错误: {r['error']}\n")
            f.write("\n")

    print(f"📄 详细报告已保存到: {report_file}")
    print("\n✅ 所有测试完成！")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\n⚠️  测试被用户中断")
    except Exception as e:
        print(f"\n❌ 发生错误: {e}")
        import traceback
        traceback.print_exc()
