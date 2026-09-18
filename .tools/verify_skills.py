import os
import re

SKILLS = r"C:\Users\wangx\.workbuddy\skills"

for name in sorted(os.listdir(SKILLS)):
    d = os.path.join(SKILLS, name)
    p = os.path.join(d, "SKILL.md")
    if not os.path.isfile(p):
        continue
    with open(p, encoding="utf-8") as f:
        t = f.read()

    m = re.match(r"^---\s*\n(.*?)\n---\s*\n", t, re.S)
    print("=== %s (%d bytes) ===" % (name, len(t.encode("utf-8"))))
    if not m:
        print("   frontmatter MISSING")
        print()
        continue

    fm = m.group(1)
    for k in ("name", "description", "agent_created"):
        mm = re.search(r"^" + k + r":\s*(.*)$", fm, re.M)
        print("   %-15s: %s" % (k, mm.group(1)[:64] if mm else "(MISSING)"))

    for root, dirs, files in os.walk(d):
        for f in files:
            if f != "SKILL.md":
                rel = os.path.relpath(os.path.join(root, f), d)
                print("   attached       :", rel)
    print()
