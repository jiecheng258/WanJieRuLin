import zipfile, re, html

p = r'C:\Users\wangx\Desktop\名字：万界如林(1).docx'
z = zipfile.ZipFile(p)
x = z.read('word/document.xml').decode('utf-8')

runs = re.findall(r'<w:t[^>]*>(.*?)</w:t>', x, re.S)
text = ''.join(html.unescape(r) for r in runs).replace('\ufeff', '')

# Strip any leftover XML tag fragments that leaked into text
text = re.sub(r'<[^>]*>', '', text)
# Drop the escaped-tag remnants like "</w:r>"
text = text.replace('</w:r>', '')
text = re.sub(r'\\s+', ' ', text).strip()

markers = ['初始遗物：', '先古之民升级遗物：', '初始牌：', '先古之民：',
           '达佛给的能力卡：', '普通卡：', '罕见卡：', '稀有卡：', '特殊遗物']
for m in markers:
    text = text.replace(m, '\n\n【' + m.rstrip('：') + '】\n')

prefixes = ['攻击牌', '技能牌', '能力牌']
for pf in prefixes:
    text = re.sub(r'(?<!\\n)' + pf, '\n' + pf, text)

lines = [ln.strip() for ln in text.split('\n') if ln.strip()]

out_path = r'C:\Users\wangx\Documents\Default Project\WanJieRuLin\DESIGN.md'
with open(out_path, 'w', encoding='utf-8') as f:
    f.write('# 万界如林 — 原始设计文档\n\n')
    f.write('> 来源：`名字：万界如林(1).docx`（用户提供，逐条转录）\n\n')
    f.write('## 全文\n\n')
    f.write(text.strip())
    f.write('\n\n---\n\n## 逐条列表\n\n')
    for ln in lines:
        f.write('- ' + ln + '\n')

print('PARTS', len(lines))
print(text.strip())
