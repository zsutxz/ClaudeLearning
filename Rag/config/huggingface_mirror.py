#!/usr/bin/env python3
"""
Hugging Face 镜像配置
设置国内镜像以加速模型下载
"""

import os


def setup_huggingface_mirror():
    """设置 Hugging Face 国内镜像"""

    # Hugging Face Hub 国内镜像
    mirror_urls = {
        'default': 'https://hf-mirror.com',
        'aliyun': 'https://hf-mirror.com',
        'tencent': 'https://hf-mirror.com'
    }

    # 设置环境变量
    os.environ['HF_ENDPOINT'] = mirror_urls['default']

    # 可选：设置缓存目录
    cache_dir = os.path.join(os.path.expanduser('~'), '.cache', 'huggingface', 'hub')
    os.environ['HF_HOME'] = cache_dir

    print("已设置 Hugging Face 镜像:")
    print(f"  - 镜像地址: {os.environ['HF_ENDPOINT']}")
    print(f"  - 缓存目录: {cache_dir}")

    return True


def get_mirror_status():
    """获取镜像状态"""
    endpoint = os.environ.get('HF_ENDPOINT', 'https://huggingface.co')
    home = os.environ.get('HF_HOME', '默认缓存目录')

    return {
        'endpoint': endpoint,
        'cache_dir': home,
        'is_using_mirror': 'hf-mirror.com' in endpoint
    }


if __name__ == "__main__":
    setup_huggingface_mirror()
    status = get_mirror_status()
    print(f"\n当前配置: {status}")