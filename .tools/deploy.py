# -*- coding: utf-8 -*-
"""一把梭：编译 + 部署 + 导出 PCK（绕过 MSBuild 写不进游戏目录的问题）。

为什么需要这个脚本：
  `dotnet build` 的 CopyMod 目标、以及 Godot 导出 pck，
  在**覆盖已存在文件**时都会被拒（`Access Denied`），但「先删后建」可以通过。
  所以这里统一改成：跳过 MSBuild 的 mod 拷贝 → 用 Python 删除+重建 → 再调 Godot 导出。

  ★ 但 **RitsuLib 的自动部署必须保留**（不传 -p:RitsuLibAutoCopy=false）——
    否则游戏里的 RitsuLib 会与 mod 引用的版本脱节，导致整个 mod 加载失败。
    编译后会自动做一次一致性自检。

用法：
  python deploy.py            # 编译 + 部署 dll/json + 导出 pck
  python deploy.py --no-pck   # 只编译 + 部署，不导 pck（Godot 不在时用）
"""
import os, sys, shutil, subprocess, time, hashlib

PY = r'C:\Users\wangx\.workbuddy\binaries\python\versions\3.13.12\python.exe'
DOTNET = r'C:\Program Files\dotnet\dotnet.exe'
PROJ = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin'
SLN = os.path.join(PROJ, 'WanJieRuLin.sln')
BUILD_OUT = os.path.join(PROJ, r'.godot\mono\temp\bin\Release')
GAME = r'C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2'
MODS = os.path.join(GAME, r'mods\WanJieRuLin')
RITSU_DIR = os.path.join(GAME, r'mods\STS2-RitsuLib')
GODOT = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\tools\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64_console.exe'
GODOT_WORKDIR = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\tools\Godot_v4.5.1-stable_mono_win64'


def run(cmd, cwd=None, label=''):
    print('>' , label or ' '.join(cmd[:3]), flush=True)
    r = subprocess.run(cmd, cwd=cwd, capture_output=True, text=True,
                       encoding='utf-8', errors='replace')
    if r.returncode != 0:
        print('  !! 退出码', r.returncode)
        for l in (r.stdout or '').splitlines()[-12:]:
            print('    ', l)
        for l in (r.stderr or '').splitlines()[-8:]:
            print('    ', l)
    return r.returncode, (r.stdout or '') + (r.stderr or '')


def rm(path, tries=20, label=''):
    """删除文件。★ 这里的删除是**偶发失败**的（safe-delete 转回收站有时会
    'operations were aborted'），实测重试 10 次成功率 10/10，所以必须重试。"""
    for i in range(tries):
        if not os.path.exists(path):
            return True
        try:
            os.remove(path)
            return True
        except Exception:
            time.sleep(0.35)
    return not os.path.exists(path)


def place(src, dst, label, tries=20):
    """先删后建（就地覆盖会被拒，删除重试后可过）。"""
    last = ''
    for i in range(tries):
        try:
            rm(dst)
            shutil.copy2(src, dst)
            print('  OK  %-20s %9d B (第 %d 次)' % (label, os.path.getsize(dst), i + 1))
            return True
        except PermissionError as e:
            last = str(e)[:70]
            time.sleep(0.4)
        except Exception as e:
            last = '%s: %s' % (type(e).__name__, str(e)[:60])
            time.sleep(0.4)
    print('  FAIL %s（重试 %d 次）%s' % (label, tries, last))
    return False


def check_ritsulib():
    """★ 一致性自检：游戏里的 RitsuLib 必须与 NuGet 包里的一致。

    不一致 = mod 引用的运行库版本与游戏实际加载的不同 → 整个 mod 加载失败。
    用 MD5 比对（比读程序集版本简单，且足够可靠）。
    """
    import glob as _glob, hashlib

    print()
    print('-' * 68)
    print('RitsuLib 一致性自检')
    print('-' * 68)

    # 1. NuGet 包里最新版本的 Runtime DLL
    pkg_root = os.path.expanduser('~/.nuget/packages/sts2.ritsulib')
    pkg_dlls = _glob.glob(os.path.join(pkg_root, '*', 'lib', 'net9.0', 'STS2-RitsuLib.Runtime.dll'))
    if not pkg_dlls:
        print('   ⚠ 找不到 NuGet 包里的 RitsuLib.Runtime.dll，跳过自检')
        return
    pkg_dll = sorted(pkg_dlls)[-1]
    pkg_ver = os.path.basename(os.path.dirname(os.path.dirname(os.path.dirname(pkg_dll))))

    # 2. 游戏里的 Runtime DLL（compat/<ver>/ 下）
    game_dlls = _glob.glob(os.path.join(
        RITSU_DIR, 'compat', '*', 'STS2-RitsuLib.Runtime.dll'))
    if not game_dlls:
        print('   ⚠ 游戏里找不到 RitsuLib.Runtime.dll —— mod 会加载失败！')
        print('      请先运行一次不带 RitsuLibAutoCopy=false 的 dotnet build')
        return
    game_dll = game_dlls[0]

    def md5(p):
        h = hashlib.md5()
        with open(p, 'rb') as f:
            for chunk in iter(lambda: f.read(1 << 20), b''):
                h.update(chunk)
        return h.hexdigest()

    a, b = md5(pkg_dll), md5(game_dll)
    if a == b:
        print('   ✓ 一致（NuGet %s  ==  游戏 mods/STS2-RitsuLib）' % pkg_ver)
    else:
        print('   ✗✗ 不一致！')
        print('      NuGet 包 : %s  md5=%s' % (pkg_ver, a[:12]))
        print('      游戏里   : %s  md5=%s' % (os.path.basename(os.path.dirname(game_dll)), b[:12]))
        print('      → mod 会因运行库版本不匹配而**完全加载失败**')
        print('      → 修复：不传 -p:RitsuLibAutoCopy=false 重新 build 一次')


def main():
    want_pck = '--no-pck' not in sys.argv

    print('=' * 68)
    print('1/3  编译（★ 保留 RitsuLibAutoCopy，让 RitsuLib 随构建同步进游戏）')
    print('=' * 68)
    # ★★★ 2026-10-09 事故修复：
    #   这里曾传 '-p:RitsuLibAutoCopy=false'，会把 NuGet 包里 RitsuLib 的
    #   「自动部署到游戏 mods/STS2-RitsuLib」这步关掉。
    #   而 csproj 里是 <PackageReference Version="*" />（跟随 NuGet 最新），
    #   于是 NuGet 一升版（0.6.2 → 0.6.7），mod 就引用了新版 RitsuLib，
    #   但游戏里的 RitsuLib 还是老的 → 启动报
    #     Could not load file or assembly 'STS2-RitsuLib.Runtime, Version=0.6.7.0'
    #   → 整个 mod 加载失败、游戏里完全识别不到。
    #   现在不传该开关，RitsuLib 会随每次构建自动同步 ✓
    code, out = run([DOTNET, 'build', SLN, '-c', 'Release',
                     '-p:CopyModOnBuild=false',
                     '-p:RunPckExport=false'], cwd=PROJ, label='dotnet build')
    tail = [l for l in out.splitlines() if ('个错误' in l or '个警告' in l or 'error' in l.lower())]
    for l in tail[-6:]:
        print('   ', l.strip())
    if code != 0:
        print('编译失败，中止。')
        return 1

    # ★ 一致性自检：mod 引用的 RitsuLib 版本 必须 == 游戏里装的版本
    check_ritsulib()

    print()
    print('=' * 68)
    print('2/3  部署 DLL + 清单')
    print('=' * 68)
    os.makedirs(MODS, exist_ok=True)
    ok = place(os.path.join(BUILD_OUT, 'WanJieRuLin.dll'),
               os.path.join(MODS, 'WanJieRuLin.dll'), 'WanJieRuLin.dll')
    ok &= place(os.path.join(PROJ, 'WanJieRuLin.json'),
                os.path.join(MODS, 'WanJieRuLin.json'), 'WanJieRuLin.json')

    # 校验 md5
    a = hashlib.md5(open(os.path.join(BUILD_OUT, 'WanJieRuLin.dll'), 'rb').read()).hexdigest()
    b = hashlib.md5(open(os.path.join(MODS, 'WanJieRuLin.dll'), 'rb').read()).hexdigest()
    print('  DLL md5 %s %s' % (a[:12], '一致 ✓' if a == b else '不一致 !!'))

    if not want_pck:
        print()
        print('（--no-pck，跳过 PCK 导出）')
        return 0 if ok else 1

    print()
    print('=' * 68)
    print('3/3  导出 PCK')
    print('=' * 68)
    if not os.path.exists(GODOT):
        print('  !! 找不到 Godot：', GODOT)
        print('  （把 Godot 解压到该路径，或改本脚本顶部的 GODOT 常量）')
        return 1
    pck = os.path.join(MODS, 'WanJieRuLin.pck')
    if os.path.exists(pck):
        if rm(pck, label='pck'):
            print('  已删除旧 pck')
        else:
            print('  !! 旧 pck 删不掉，导出会失败')
    run([GODOT, '--headless', '--path', PROJ, '--export-pack', 'Windows Desktop', pck],
        cwd=PROJ, label='godot --export-pack')
    if os.path.exists(pck):
        print('  OK  pck %d B (%.1f MB)' % (os.path.getsize(pck), os.path.getsize(pck) / 1048576))
    else:
        print('  !! pck 未生成')
        return 1

    print()
    print('全部完成。记得**完全退出游戏再重开**（RitsuLib 只在启动时加载 DLL）。')
    return 0


if __name__ == '__main__':
    sys.exit(main())
