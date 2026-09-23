# TidyFlow Logo Guide

You have two beautiful logos for TidyFlow! Here's how to use them effectively.

## 📦 Your Logos

### 1. Title Logo
**File**: `assets/TidyFlow-logo.png` (used at the top of the README). `assets/title-logo.png` is the original "TIDY PACK RAT" title artwork from before the rename, kept for reference.

**Best Used For**:
- ✅ README header (splash page) - **Already configured!**
- ✅ Social media posts
- ✅ Blog articles about TidyFlow
- ✅ Presentation slides
- ✅ Marketing materials
- ✅ Website banner
- ✅ YouTube thumbnail
- ✅ Documentation headers

**Size Recommendations**:
- README: 600px width
- Social media: Full resolution
- Banners: 1200px+ width

### 2. Mascot Logo (Pack Rat with Glasses)
**File**: `assets/logo.png`

**Description**: Friendly pack rat character wearing glasses

**Best Used For**:
- ✅ Application icon (.ico)
- ✅ GitHub repository avatar
- ✅ Favicon for documentation site
- ✅ GitHub Releases and social preview images
- ✅ Small icons and badges
- ✅ Profile pictures
- ✅ Watermarks

**Size Recommendations**:
- README: 150px width (secondary logo)
- Icon: 256x256px
- Avatar: 200x200px

## 🎨 How They Work Together

**Current README Layout** (Perfect!):
1. Title Logo (big, colorful, attention-grabbing)
2. Tagline: "Sorting your files, to clean up your mess."
3. Mascot Logo (smaller, reinforces brand)
4. Badges (MIT license, version, platform)

This creates a **visual hierarchy**: Title → Tagline → Mascot → Info

## 📋 Next Steps to Complete Branding

### 1. Save Both Logos
```
assets/TidyFlow-logo.png ← The title logo shown at the top of the README
assets/logo.png          ← The pack rat mascot (source for all icons)
```

### 2. Create Application Icon and Package Images
No online converters or extra tools needed; the scripts in `tools/` build everything from `assets/logo.png`:
- `.\tools\New-AppIcon.ps1` writes `src/TidyFlow/Assets/icon.ico` (16–256 px) for the window, taskbar and notification area
- `.\tools\Generate-MsixAssets.ps1` writes the tile and splash images for the optional MSIX package to `src/TidyFlow.Package/Images`

Use a square PNG with a transparent background, at least 512×512. See [tools/README.md](tools/README.md).

### 3. Optional: Create Combined Banner
For extra polish, create a banner that combines both:
- Title logo on top
- Mascot logo on the side or bottom
- Perfect for GitHub social preview image (1200×630px)

### 4. Screenshots
Screenshots live in `assets/screenshots/` and are used by the README: `dashboard.png`, `preview.png`, `history.png` and `rules.png`. When the UI changes, retake them at the same names. A Schedule tab shot would be a nice addition.

## 🖼️ File Checklist

- [x] `assets/TidyFlow-logo.png` - Title logo
- [x] `assets/logo.png` - Pack rat mascot
- [x] `src/TidyFlow/Assets/icon.ico` - Windows icon (built by `tools/New-AppIcon.ps1`)
- [x] `src/TidyFlow.Package/Images/` - Optional MSIX package tile images (built by `tools/Generate-MsixAssets.ps1`)
- [x] `assets/screenshots/` - Dashboard, Preview changes, History and Rules screenshots

## 🚀 Quick Commands

```powershell
# Replace the mascot logo:
Copy-Item C:\path\to\new-logo.png assets\logo.png

# Rebuild the application icon and the MSIX images:
.\tools\New-AppIcon.ps1
.\tools\Generate-MsixAssets.ps1

# Build the project with the new icon:
.\build.ps1
```

## 📱 Social Media Dimensions

If you want to share TidyFlow on social media:

| Platform | Dimensions | Logo to Use |
|----------|-----------|-------------|
| Twitter/X Header | 1500×500 | Title logo |
| GitHub Social Preview | 1200×630 | Combined or Title |
| LinkedIn Post | 1200×627 | Title logo |
| Facebook Post | 1200×630 | Title logo |
| Instagram Post | 1080×1080 | Square crop of mascot |
| YouTube Thumbnail | 1280×720 | Title logo centered |

## 🎯 Color Palette (from logos)

**From Title Logo**:
- Red: `#C84A3D` (T)
- Yellow/Gold: `#E9A93D` (I, A)
- Teal: `#5A8B91` (D, T)
- Green: `#6B8E5F` (Y, K)

**From Mascot Logo**:
- Brown/Grey: `#8B7D6B` (fur)
- Peach: `#D4927A` (ears, nose)
- Black: `#2C2C2C` (outlines, glasses)

These colors can be used in:
- GUI accent colors (already using blues)
- Documentation themes
- Marketing materials

## ✨ Brand Personality

Your logos convey:
- **Friendly** (cute mascot, rounded letters)
- **Smart** (glasses on mascot)
- **Organized** (clean, orderly design)
- **Playful** (colorful, 3D style)
- **Trustworthy** (professional quality)

Keep this personality in all communications!

---

**Your branding is now complete!** Both logos work beautifully together and give TidyFlow a professional, friendly identity. 🎨✨
