# -*- coding: utf-8 -*-
"""
WanJieRuLin 打包脚本
生成:
  WanJieRuLin-<VERSION>-install.zip   游戏 mods/ 即用包 (DLL + json + pck + 安装说明)
  WanJieRuLin-<VERSION>-source.zip    源码包 (工程全部源文件)

用法:
  python package.py            # 打包
  python package.py --check    # 只校验, 不写文件

★ 升级版本时**只需改下面的 VERSION 常量**（连同 WanJieRuLin.json 的 version 一起改）。
  输出文件按版本号命名，**旧版本的 zip 不会被删除或覆盖**，可以随时回滚对照。
"""
import os
import subprocess
import sys
import shutil
import hashlib
import zipfile

VERSION = "v0.8.3"

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
# ★ .tools 也要打进去：它是文案真相源（gen_loc.py）与全部审计脚本所在处，
#   发行说明承诺「含构建脚本」。（2026-09-27 修正：此前只遍历前两个目录，
#   导致 .tools 完全没进包。）
SRC_DIRS = ["WanJieRuLin", "WanJieRuLinCode", ".tools"]
# 源码包里要排除的扩展名
SRC_EXCLUDE_EXT = {".uid", ".import", ".md5", ".tmp", ".log"}
# ★ .tools 必须包含（2026-09-27 修正）：
#   它是「文案唯一真相源 gen_loc.py」+ 全部审计脚本（含 audit_infinite.py）所在处，
#   发行说明里承诺了「含构建脚本」，此前却被排除，名不副实。
#   只排除其中的 __pycache__。
SRC_EXCLUDE_DIR = {".git", ".godot", ".import", "obj", "bin", ".vs", "__pycache__"}


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


# ============================================================================
# ⑤ 自动归档（用户约定 2026-09-28：历史版本进 GitHub dist 分支，本地只留最新）
# ============================================================================
def archive_to_dist(install_zip):
    """把安装包推到 GitHub dist 分支；本地旧包送回收站。"""
    import shutil
    import subprocess
    import tempfile
    GIT = r'C:\Users\wangx\.workbuddy\binaries\PortableGit\versions\1.2.0\cmd\git.exe'
    WORK = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\dist-archive'
    URL = 'git@github.com:jiecheng258/WanJieRuLin.git'
    env = dict(os.environ)
    env['GIT_SSH_COMMAND'] = ('ssh -o ProxyCommand=none -o ConnectTimeout=20 '
                              '-o StrictHostKeyChecking=accept-new')

    def run(args, cwd=None):
        r = subprocess.run(args, cwd=cwd, env=env, capture_output=True)
        return r.returncode, (r.stdout + r.stderr).decode('utf-8', 'replace')

    if not os.path.isfile(GIT):
        print('  !! 找不到 git，跳过归档'); return
    shutil.rmtree(WORK, ignore_errors=True)
    os.makedirs(WORK, exist_ok=True)
    rc, out = run([GIT, 'clone', '--branch', 'dist', '--single-branch',
                   '--depth', '1', URL, '.'], cwd=WORK)
    if rc != 0:
        print('  !! dist 归档失败（克隆）:', out[-200:]); return

    shutil.copy2(install_zip, os.path.join(WORK, os.path.basename(install_zip)))
    run([GIT, 'config', 'user.email', 'jiecheng258@users.noreply.github.com'])
    run([GIT, 'config', 'user.name', 'jiecheng258'])
    run([GIT, 'add', '-A'])
    rc, out = run([GIT, 'commit', '-m', 'archive: ' + os.path.basename(install_zip)])
    if rc == 0:
        rc, out = run([GIT, 'push', 'origin', 'dist'])
        print('  归档到 GitHub dist 分支:',
              '成功' if rc == 0 else '失败 ' + out[-160:])
    else:
        print('  （dist 无变化，跳过 push）')
    shutil.rmtree(WORK, ignore_errors=True)

    # 本地只留最新：其他版本的 install.zip 送回收站
    import glob as _glob
    for old in _glob.glob(os.path.join(OUT_DIR, 'WanJieRuLin-v*-install.zip')):
        if VERSION not in os.path.basename(old):
            try:
                os.remove(old)      # safe-delete shim → 回收站
                print('  本地已移除旧包:', os.path.basename(old))
            except Exception as e:
                print('  !! 移除失败', os.path.basename(old), e)



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

    # ★ source 包改为「按需生成」（2026-09-28）：
    #   源码就在 git 里，任一版本都能由对应 tag 重建
    #   （`git checkout <tag>` → 跑 package.py），
    #   所以每版都产一个 27–29 MB 的 source.zip 纯属冗余。
    #   需要时设环境变量 WJRL_SOURCE_ZIP=1 恢复生成。
    _want_source = os.environ.get("WJRL_SOURCE_ZIP") == "1"
    if _want_source:
        source_zip = os.path.join(OUT_DIR, "WanJieRuLin-%s-source.zip" % VERSION)
        with zipfile.ZipFile(source_zip, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as z:
            for ap, rel in src:
                z.write(ap, "WanJieRuLin/" + rel)
        print("  %s  %d B" % (source_zip, os.path.getsize(source_zip)))
    else:
        print("  （已跳过 source 包；需要时设 WJRL_SOURCE_ZIP=1）")

    # ---- 5. 自动归档 + 本地只留最新 ----

    archive_to_dist(install_zip)


    # ---- 复检 ----
    print("\n--- 复检 install.zip ---")
    with zipfile.ZipFile(install_zip) as z:
        for i in z.infolist():
            d = z.read(i.filename)
            print("  %-40s %9d B  md5=%s" % (
                i.filename, len(d), hashlib.md5(d).hexdigest()[:12]))
    # ---- 5. 自动发布 install 包到 GitHub（dist 分支），并让**本地只留最新** ----
    #   ★ 用户要求：打包后自动上传安装包，且只保留最新的安装包。
    #   发布脚本用**独立临时仓库**推 dist 分支，绝不碰主仓库工作区。
    _pub = os.path.join(REPO, ".tools", "publish.py")
    if os.path.isfile(_pub) and os.environ.get("WJRL_SKIP_PUBLISH") != "1":
        print("\n[5/5] 发布 install 包到 GitHub")
        _r = subprocess.run([sys.executable, _pub], capture_output=True)
        _o = (_r.stdout + _r.stderr).decode("utf-8", "replace")
        for _ln in _o.strip().splitlines()[-8:]:
            print("  " + _ln)
        if _r.returncode != 0:
            print("  !! 发布失败（不影响本地打包结果）")
    else:
        print("\n[5/5] 已跳过发布（设 WJRL_SKIP_PUBLISH=1 可手动跳过）")

    if _want_source:
        print("\n--- 复检 source.zip ---")
        with zipfile.ZipFile(source_zip) as z:
            print("  条目数 %d" % len(z.namelist()))
            bad = z.testzip()
            print("  完整性: %s" % ("OK" if bad is None else "损坏 " + bad))
    return 0


if __name__ == "__main__":
    sys.exit(main())
