# 生成 iOS 图标（1024x1024，不透明；与 Windows 版同款：蓝色渐变 + 白色 ¥）
# 用法：python3 ios/tools/make_icon.py
import os
from PIL import Image, ImageDraw, ImageFont

S = 1024
out = os.path.join(os.path.dirname(__file__), "..", "RateConverter", "Assets.xcassets", "AppIcon.appiconset", "icon-1024.png")
a, b = (0x4F, 0x86, 0xF7), (0x2F, 0x5B, 0xD3)
img = Image.new("RGB", (S, S))
px = img.load()
for y in range(S):
    for x in range(S):
        t = (x + y) / (2 * (S - 1))
        px[x, y] = tuple(round(a[i] + (b[i] - a[i]) * t) for i in range(3))
d = ImageDraw.Draw(img)
font = None
for f in ["/System/Library/Fonts/SFNS.ttf", "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", "C:/Windows/Fonts/segoeuib.ttf"]:
    if os.path.exists(f):
        font = ImageFont.truetype(f, int(S * 0.58))
        break
d.text((S / 2, S / 2 + S * 0.01), "\u00A5", font=font, fill="white", anchor="mm")
img.save(out)
print("Wrote", os.path.abspath(out))
