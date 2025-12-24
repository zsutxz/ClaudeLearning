#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
批量运行所有示例脚本

自动运行所有示例并收集结果。
"""

import sys
import subprocess
import time
from pathlib import Path
from datetime import datetime

# 添加项目根目录到 Python 路径
project_root = Path(__file__).parent
sys.path.insert(0, str(project_root))

from lib.config import get_config


def run_example(script_name: str, timeout: int = 120) -> dict:
    """
    运行单个示例

    Args:
        script_name: 脚本名称
        timeout: 超时时间（秒）

    Returns:
        dict: 运行结果
    """
    script_path = project_root / "examples" / script_name

    if not script_path.exists():
        return {
            "name": script_name,
            "status": "skipped",
            "reason": "文件不存在"
        }

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
        print("❌ 错误: 未设置 ANTHROPIC_API_KEY")
        print("请在 config/.env 文件中配置 API 密钥")
        return

    print("⚠️  注意: 运行所有示例需要较长时间和多次 API 调用")
    response = input("\n是否继续？(y/n): ").strip().lower()

    if response != 'y':
        print("已取消")
        return

    # 示例列表
    examples = [
        "01_basic_chat.py",
        "02_multi_model.py",
        "03_tools_usage.py",
        "04_mcp_integration.py",
        "05_session_management.py",
        "06_stream_response.py",
        "07_advanced_agent.py",
    ]

    # 运行所有示例
    results = []
    start_time = time.time()

    for example in examples:
        result = run_example(example)
        results.append(result)

        time.sleep(1)  # 示例之间间隔

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
