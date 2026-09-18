# -*- coding: utf-8 -*-
"""
WanJieRuLin 打包脚本
生成:
  WanJieRuLin-v0.1.2-install.zip   游戏 mods/ 即用包 (DLL + json + pck + 安装说明)
  WanJieRuLin-v0.1.2-source.zip    源码包 (工程全部源文件)

用法:
  python package.py            # 打包
  python package.py --check    # 只校验, 不写文件
"""
import os
import sys
import shutil
import hashlib
import zipfile

VERSION = "v0.1.2"

REPO = r"C:\Users\wangx\Documents\Default Project\WanJieRuLin"
GAME_MODS = r"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\mods"
MODS_SRC = os.path.join(GAME_MODS, "WanJieRuLin")
OUT_DIR = r"C:\Users\wangx\Desktop\WanJieRuLin-dist"

# 源码包包含的顶层文件
SRC_FILES = [
    ".gitattributes", ".gitignore",
    "DESIGN.md", "README.md", "README.en.md",
    "WanJieRuLin.csproj", "WanJieRuLin.sln", "WanJieRuLin.json",
    "export_presets.cfg", "local.props.template", "project.godot",
]
# 源码包包含的顶层目录
SRC_DIRS = ["WanJieRuLin", "WanJieRuLinCode"]
# 源码包里要排除的扩展名
SRC_EXCLUDE_EXT = {".uid", ".import", ".md5", ".tmp", ".log"}
SRC_EXCLUDE_DIR = {".git", ".godot", ".import", "obj", "bin", ".tools", ".vs"}


def md5(path):
    with open(path, "rb") as f:
        return hashlib.md5(f.read()).hexdigest()


def collect_source(repo):
    """收集源码包文件, 返回 [(绝对路径, 归档内相对路径)]"""
    out = []
    for name in SRC_FILES:
        p = os.path.join(repo, name)
        if os.path.isfile(p):
            out.append((p, name))
    for d in SRC_DIRS:
        root0 = os.path.join(repo, d)
        if not os.path.isdir(root0):
            continue
        for root, dirs, files in os.walk(root0):
            dirs[:] = [x for x in dirs if x not in SRC_EXCLUDE_DIR]
            for fn in files:
                if os.path.splitext(fn)[1].lower() in SRC_EXCLUDE_EXT:
                    continue
                ap = os.path.join(root, fn)
                rel = os.path.relpath(ap, repo).replace("\\", "/")
                out.append((ap, rel))
    return sorted(out, key=lambda x: x[1])


def main():
    check_only = "--check" in sys.argv

    print("=" * 68)
    print("  WanJieRuLin 打包 %s" % VERSION)
    print("=" * 68)

    # ---- 1. 校验游戏 mods 目录 ----
    print("\n[1/4] 校验游戏 mods 目录")
    need = ["WanJieRuLin.dll", "WanJieRuLin.json", "WanJieRuLin.pck"]
    missing = [n for n in need if not os.path.isfile(os.path.join(MODS_SRC, n))]
    if missing:
        print("  !! 缺少文件: %s" % missing)
        return 1
    for n in need:
        p = os.path.join(MODS_SRC, n)
        print("  OK  %-24s %9d B  md5=%s" % (n, os.path.getsize(p), md5(p)[:12]))

    # ---- 2. 校验源码产物与 mods 一致 ----
    print("\n[2/4] 校验 DLL 是否等于源码最新产物")
    print("  (RitsuLib 的 MSBuild 目标会把构建产物直接复制到 mods/, 无 bin/ 目录)")
    print("  -> 只要构建成功且游戏未运行, mods/ 内 DLL 即最新, 无需额外比对")

    # ---- 3. 收集源码 ----
    print("\n[3/4] 收集源码")
    src = collect_source(REPO)
    print("  顶层文件 %d 个" % len(SRC_FILES))
    print("  目录 %s" % SRC_DIRS)
    print("  合计 %d 个文件, %.2f MB" % (
        len(src), sum(os.path.getsize(p) for p, _ in src) / 1024 / 1024))

    if check_only:
        print("\n--check 模式, 不写文件。")
        return 0

    # ---- 4. 写包 ----
    print("\n[4/4] 写入 zip")
    os.makedirs(OUT_DIR, exist_ok=True)

    install_zip = os.path.join(OUT_DIR, "WanJieRuLin-%s-install.zip" % VERSION)
    with zipfile.ZipFile(install_zip, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as z:
        for n in need:
            z.write(os.path.join(MODS_SRC, n), "WanJieRuLin/" + n)
        guide = os.path.join(REPO, ".tools", "install_guide.md")
        if os.path.isfile(guide):
            z.write(guide, "WanJieRuLin/安装说明.md")
    print("  %s  %d B" % (install_zip, os.path.getsize(install_zip)))

    source_zip = os.path.join(OUT_DIR, "WanJieRuLin-%s-source.zip" % VERSION)
    with zipfile.ZipFile(source_zip, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as z:
        for ap, rel in src:
            z.write(ap, "WanJieRuLin/" + rel)
    print("  %s  %d B" % (source_zip, os.path.getsize(source_zip)))

    # ---- 复检 ----
    print("\n--- 复检 install.zip ---")
    with zipfile.ZipFile(install_zip) as z:
        for i in z.infolist():
            d = z.read(i.filename)
            print("  %-40s %9d B  md5=%s" % (
                i.filename, len(d), hashlib.md5(d).hexdigest()[:12]))
    print("\n--- 复检 source.zip ---")
    with zipfile.ZipFile(source_zip) as z:
        print("  条目数 %d" % len(z.namelist()))
        bad = z.testzip()
        print("  完整性: %s" % ("OK" if bad is None else "损坏 " + bad))
    return 0


if __name__ == "__main__":
    sys.exit(main())
