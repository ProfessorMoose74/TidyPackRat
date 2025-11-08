# TidyPackRat Logo Guide

You have two beautiful logos for TidyPackRat! Here's how to use them effectively.

## 📦 Your Logos

### 1. Title Logo (Colorful 3D Text)
**File**: `assets/title-logo.png`

**Description**: "TIDY PACK RAT" in colorful 3D letters (red, yellow, teal, green)

**Best Used For**:
- ✅ README header (splash page) - **Already configured!**
- ✅ Social media posts
- ✅ Blog articles about TidyPackRat
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
- ✅ App Store listings (if you expand)
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
assets/title-logo.png    ← Save the colorful "TIDY PACK RAT" image here
assets/logo.png          ← Save the pack rat mascot image here
```

### 2. Create Application Icon
Convert the mascot logo to ICO format:
- Go to: https://convertio.co/png-ico/
- Upload `assets/logo.png`
- Select sizes: 16, 32, 48, 256
- Download and save as: `src/gui/Assets/icon.ico`

### 3. Optional: Create Combined Banner
For extra polish, create a banner that combines both:
- Title logo on top
- Mascot logo on the side or bottom
- Perfect for GitHub social preview image (1200×630px)

### 4. Take Screenshots
Capture the application in action:
- Main configuration window
- Category settings
- Schedule configuration
- Test run/dry run
- Save to: `assets/screenshots/`

## 🖼️ File Checklist

- [ ] `assets/title-logo.png` - Colorful title (saved from Copilot)
- [ ] `assets/logo.png` - Pack rat mascot (saved from Copilot)
- [ ] `src/gui/Assets/icon.ico` - Windows icon (converted from logo.png)
- [ ] `assets/screenshots/` - Application screenshots (take after building)

## 🚀 Quick Commands

```powershell
# After saving both logos, integrate them:
.\tools\integrate-logos.ps1 -MascotLogoPath "C:\Downloads\mascot.png" `
                             -TitleLogoPath "C:\Downloads\title.png"

# Create the application icon:
.\tools\prepare-assets.ps1 -LogoPath "assets\logo.png"

# Build the project with the new icon:
.\build.ps1 -Configuration Release
```

## 📱 Social Media Dimensions

If you want to share TidyPackRat on social media:

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

**Your branding is now complete!** Both logos work beautifully together and give TidyPackRat a professional, friendly identity. 🎨✨
