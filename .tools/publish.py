# -*- coding: utf-8 -*-
r"""★ 发布 install 包到 GitHub（把最新安装包自动上传 + 保留最新）。

做什么
------
1. 把本地 `WanJieRuLin-dist` 里的 install 包同步到 GitHub 的 **dist 分支**
   （dist 分支只放打包产物，不放源码）
2. 更新分支上的 README：标出**最新版本**与下载指引
3. **本地只留最新的 install 包**，旧的删到回收站（GitHub 上已留档）

为什么用 dist 分支而不是 Releases
--------------------------------
上传 Release 附件**必须 PAT**（本机只有 SSH 密钥，也没装 gh CLI）。
SSH 能推分支和 tag，但推不了 release 附件 → 用分支达到同样目的。

★ 安全：用**独立临时仓库**推分支，**绝不在主仓库里 checkout 分支**
   （在主仓库 `git checkout --orphan` + `git rm -rf .` 会清空工作区 ——
    本项目就出过 git 操作删掉 .git 的事故）。
"""
import io, os, re, shutil, subprocess, sys, time

GIT = r'C:\Users\wangx\.workbuddy\binaries\PortableGit\versions\1.2.0\cmd\git.exe'
DIST = r'C:\Users\wangx\Desktop\WanJieRuLin-dist'
TMP = r'C:\Users\wangx\WorkBuddy\_scratch\sts2-mod\_publish_repo'
REMOTE = 'git@github.com:jiecheng258/WanJieRuLin.git'

env = dict(os.environ)
env['GIT_SSH_COMMAND'] = ('ssh -o ProxyCommand=none -o ConnectTimeout=25 '
                          '-o StrictHostKeyChecking=accept-new')


def run(args, cwd=TMP, check=True):
    r = subprocess.run(args, cwd=cwd, env=env, capture_output=True)
    out = (r.stdout + r.stderr).decode('utf-8', 'replace')
    if check and r.returncode != 0:
        print('  !! %s\n     %s' % (' '.join(args[-3:]), out.strip()[:400]))
    return r.returncode, out


def vkey(f):
    m = re.search(r'v(\d+)\.(\d+)\.(\d+)', f)
    return tuple(int(x) for x in m.groups()) if m else (0, 0, 0)


# ---------------------------------------------------------------- 收集
zips = sorted([f for f in os.listdir(DIST)
               if f.startswith('WanJieRuLin-v') and f.endswith('-install.zip')],
              key=vkey)
if not zips:
    print('没有 install 包可发布'); sys.exit(1)
# ★ 只发布**最新那一个**（用户明确：保留最新的安装包即可，不必堆历史）
all_zips = list(zips)
latest = zips[-1]
zips = [latest]
ver = re.search(r'(v\d+\.\d+\.\d+)', latest).group(1)
print('待发布（仅最新）= %s' % latest)

# ---------------------------------------------------------------- 临时仓库
if os.path.isdir(TMP):
    shutil.rmtree(TMP)
os.makedirs(TMP)
total = 0
for f in zips:
    p = os.path.join(DIST, f)
    total += os.path.getsize(p)
    shutil.copy2(p, os.path.join(TMP, f))
print('  合计 %.1f MB' % (total / 1048576))

rows = '\n'.join(
    '| `%s` | %s | %.1f MB |' % (f, '**最新**' if f == latest else '历史',
                                os.path.getsize(os.path.join(DIST, f)) / 1048576)
    for f in reversed(zips))

io.open(os.path.join(TMP, 'README.md'), 'w', encoding='utf-8', newline='\n').write('''# 万界如林 · 安装包发布分支

**最新版本：%s** ｜ 下载 `%s`

> 本分支由 `.tools` 的发布流程自动维护（打包后自动同步），只放 install 包。
> 源码在 `master` / `v0.3-card-rework` 分支；源码包可由对应 tag 重建
> （`git checkout <tag>` → 跑 `.tools/package.py`）。

## 安装

解压 `%s`，把里面的 `WanJieRuLin` 文件夹放进游戏 `mods/` 即可
（需先装 **RitsuLib**）。

## 版本列表

| 文件 | 状态 | 大小 |
|---|---|---|
%s
''' % (ver, latest, latest, rows))

io.open(os.path.join(TMP, 'LATEST.txt'), 'w', encoding='utf-8', newline='\n').write(
    '%s\t%s\n' % (ver, latest))

run([GIT, 'init', '-b', 'dist'])
run([GIT, 'config', 'user.email', 'jiecheng258@users.noreply.github.com'])
run([GIT, 'config', 'user.name', 'jiecheng258'])
run([GIT, 'add', '-A'])
rc, out = run([GIT, 'commit', '-m', 'publish: install 包同步（最新 %s）' % ver])
print('  提交: %s' % (out.strip().splitlines()[0] if out.strip() else ''))

# ---------------------------------------------------------------- 推送
run([GIT, 'remote', 'add', 'origin', REMOTE])

# ★ 实时显示上传进度：**不捕获**输出，让 git 的进度条直接打到终端。
#   实测 GitHub SSH 上传约 1.4 MB/s，20 MB ≈ 15 秒、170 MB ≈ 2 分钟。
_mb = total / 1048576
print('推送中：%.1f MB，按实测 1.4 MB/s 预计约 %.0f 秒（下方为 git 实时进度）'
      % (_mb, _mb / 1.4))
import time as _time
_t0 = _time.time()
rc = subprocess.call([GIT, 'push', '-f', '--progress', 'origin', 'dist'],
                     cwd=TMP, env=env)
print('  push 返回码 = %d，耗时 %.1f 秒' % (rc, _time.time() - _t0))

# ---------------------------------------------------------------- 校验
rc, local = run([GIT, 'rev-parse', 'HEAD'])
rc2, remote = run([GIT, 'ls-remote', 'origin', 'dist'])
ok = local.strip() and remote.split() and local.strip() == remote.split()[0]
print('  本地==远端: %s' % ('✓' if ok else '✗'))

# ---------------------------------------------------------------- 本地只留最新
print()
print('本地清理（只留 %s）：' % latest)
for f in [x for x in all_zips if x != latest]:
    p = os.path.join(DIST, f)
    try:
        os.remove(p)
        print('  已移除 %s' % f)
    except Exception as e:
        print('  !! 移除失败 %s: %s' % (f, e))
print('  dist 现有 zip:')
for f in sorted(os.listdir(DIST)):
    if f.endswith('.zip'):
        print('    %-42s %11d B' % (f, os.path.getsize(os.path.join(DIST, f))))

shutil.rmtree(TMP, ignore_errors=True)
print()
print('发布完成 ✓ 最新 %s 已在 https://github.com/jiecheng258/WanJieRuLin/tree/dist' % ver)
