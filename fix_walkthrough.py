path = "/Users/yildirim/.gemini/antigravity-ide/brain/da091d6d-fc19-4a15-8c3b-7d9bf3998033/walkthrough.md"
with open(path, "r") as f:
    text = f.read()

text = text.replace("- `index.html` ve `app.js` içerisine **Kasko/Sigorta, Ceza ve HGS** sayfaları (ve panelleri)### Bug Fixes", "- `index.html` ve `app.js` içerisine **Kasko/Sigorta, Ceza ve HGS** sayfaları (ve panelleri) eklendi.\n### Bug Fixes")
text = text.replace(" değiştiğini görebilirsiniz.\n Excel'e aktarma özelliği eklendi", " değiştiğini görebilirsiniz.\n\n- **Excel İndir:** Yeni eklenen modüller ve mevcut tüm rapor ekranları için tek tıklamayla Excel'e aktarma özelliği eklendi")

with open(path, "w") as f:
    f.write(text)
