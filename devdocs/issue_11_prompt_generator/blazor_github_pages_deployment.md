# Deploying Blazor WebAssembly to GitHub Pages

This document details how to deploy a Blazor WebAssembly (WASM) application to GitHub Pages, based on research into official documentation and community examples.

**Use case:** StoryCAD Prompt Generator and API documentation site, hosted on the public API-Samples repository.

**Model:** [pollydocs.org](https://www.pollydocs.org/) - Polly .NET library documentation site using docfx + GitHub Pages.

---

## Overview

Blazor WebAssembly compiles .NET code to WebAssembly, which runs entirely in the browser. The published output is a set of static files (HTML, CSS, JS, WASM) that can be hosted on any static file server, including GitHub Pages.

**Key benefits:**
- Free hosting via GitHub Pages
- Direct access to .NET libraries (e.g., StoryCADLib) running client-side
- No server-side runtime required
- Automatic deployment via GitHub Actions

---

## Prerequisites

1. GitHub repository (must be **public** for free GitHub Pages, or paid plan for private)
2. .NET SDK installed locally for development
3. Blazor WebAssembly project

---

## Required Configuration Files

### 1. `.gitattributes` (Repository Root)

**Purpose:** Prevents Git from converting line endings in JavaScript files, which would change file hashes and break Blazor's integrity checks.

**CRITICAL:** This file must be committed **before the first commit that includes Blazor WASM JavaScript output** (`_framework/*.js` files). If JS files are committed without this setting, integrity checks will fail at runtime.

```
# Prevent line ending conversion for JavaScript files
# Required for Blazor WASM integrity checks
*.js binary
```

**Reference:** [Swimburger: How to deploy Blazor WASM to GitHub Pages](https://swimburger.net/blog/dotnet/how-to-deploy-aspnet-blazor-webassembly-to-github-pages)

---

### 2. `.nojekyll` (wwwroot/)

**Purpose:** Tells GitHub Pages not to process the site with Jekyll. Without this, Jekyll ignores folders starting with underscore (like `_framework`), breaking the Blazor app.

**Content:** Empty file (no content needed)

```bash
# Create empty file
touch wwwroot/.nojekyll
```

**Reference:** [GitHub Pages documentation on Jekyll](https://docs.github.com/en/pages/getting-started-with-github-pages/about-github-pages#static-site-generators)

---

### 3. `404.html` (wwwroot/)

**Purpose:** Enables SPA (Single Page Application) routing. When a user navigates directly to a route like `/generator/character`, GitHub Pages would return a 404. By providing a `404.html` that redirects to `index.html`, the Blazor router can handle the route.

**Option A: Copy of index.html**
```bash
cp wwwroot/index.html wwwroot/404.html
```

**Option B: Redirect script**
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Redirecting...</title>
    <script>
        // Redirect to index.html with the path as a query parameter
        var path = window.location.pathname;
        window.location.replace('/' + window.location.search + '#' + path);
    </script>
</head>
<body>
    Redirecting...
</body>
</html>
```

**Note:** The HTTP response still returns status code 404, which may affect SEO. For a lead-generation tool, this is generally acceptable.

**Reference:** [SPA GitHub Pages](https://github.com/rafgraph/spa-github-pages)

---

### 4. `CNAME` (wwwroot/, optional)

**Purpose:** Configures a custom domain for GitHub Pages.

**Content:** Single line with the domain name
```
api.storybuilder.org
```

**Note:** Also requires DNS configuration (A records or CNAME) pointing to GitHub Pages.

**Reference:** [GitHub Pages custom domains](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site)

---

## Base Path Configuration

### The Problem

GitHub Pages serves project sites from a subdirectory: `https://username.github.io/repository-name/`

Blazor needs to know this base path to correctly resolve static assets and routes.

### The Solution

In `wwwroot/index.html`, set the `<base>` tag:

```html
<base href="/repository-name/" />
```

Replace `repository-name` with your actual GitHub repository name (e.g., `API-Samples`).

**For local development:** You may want to use `/` locally and only set the repository path for production. The GitHub Actions workflow can handle this transformation automatically.

---

## GitHub Actions Workflow

### Recommended Workflow

Create `.github/workflows/deploy-gh-pages.yml`:

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: "pages"
  cancel-in-progress: false

env:
  DOTNET_VERSION: '10.0.x'
  PROJECT_PATH: 'src/PromptGenerator/PromptGenerator.csproj'
  PUBLISH_DIR: 'src/PromptGenerator/bin/Release/net10.0/publish/wwwroot'

jobs:
  build-and-deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore ${{ env.PROJECT_PATH }}

      - name: Publish application
        run: dotnet publish ${{ env.PROJECT_PATH }} -c Release

      - name: Rewrite base href
        uses: SteveSandersonMS/ghaction-rewrite-base-href@v1
        with:
          html_path: ${{ env.PUBLISH_DIR }}/index.html
          base_href: /API-Samples/

      - name: Copy index.html to 404.html
        run: cp ${{ env.PUBLISH_DIR }}/index.html ${{ env.PUBLISH_DIR }}/404.html

      - name: Setup Pages
        uses: actions/configure-pages@v4

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: ${{ env.PUBLISH_DIR }}

      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

### Key Actions Used

| Action | Purpose | Reference |
|--------|---------|-----------|
| `actions/checkout@v4` | Checkout repository | [GitHub](https://github.com/actions/checkout) |
| `actions/setup-dotnet@v4` | Install .NET SDK | [GitHub](https://github.com/actions/setup-dotnet) |
| `SteveSandersonMS/ghaction-rewrite-base-href@v1` | Update base href for GitHub Pages | [GitHub](https://github.com/AdrienTorris/awesome-blazor) |
| `actions/configure-pages@v4` | Configure GitHub Pages | [GitHub](https://github.com/actions/configure-pages) |
| `actions/upload-pages-artifact@v3` | Upload build artifacts | [GitHub](https://github.com/actions/upload-pages-artifact) |
| `actions/deploy-pages@v4` | Deploy to GitHub Pages | [GitHub](https://github.com/actions/deploy-pages) |

---

## GitHub Repository Settings

1. Navigate to repository **Settings** > **Pages**
2. Under **Build and deployment**:
   - **Source:** Select "GitHub Actions"
3. Under **Actions** > **General**:
   - Enable "Allow all actions and reusable workflows" or allow specific actions

---

## Known Issues and Solutions

### Issue 1: JavaScript Integrity Check Failures

**Symptom:** Console errors about integrity check failures, app fails to load.

**Cause:** Git converted line endings in JS files, changing their hashes.

**Solution:** Add `.gitattributes` with `*.js binary` BEFORE committing JS files.

**If already committed incorrectly:**
```bash
# Remove cached files and re-add
git rm --cached -r .
git add .
git commit -m "Fix line endings"
```

---

### Issue 2: Missing `_framework` Folder

**Symptom:** 404 errors for `_framework/blazor.webassembly.js`

**Cause:** Jekyll ignores folders starting with underscore.

**Solution:** Add empty `.nojekyll` file to wwwroot.

---

### Issue 3: Routes Return 404

**Symptom:** Direct navigation to `/generator` returns GitHub's 404 page.

**Cause:** GitHub Pages is a static file server; it doesn't understand SPA routing.

**Solution:** Add `404.html` that redirects to index.html (see above).

---

### Issue 4: PWA Service Worker Integrity Failures

**Symptom:** PWA caching fails after modifying index.html.

**Cause:** Service worker has cached hashes of original files.

**Solution:** Either:
- Disable PWA for GitHub Pages deployment
- Use `PublishSPAforGitHubPages.Build` NuGet package to regenerate hashes
- Manually regenerate `service-worker-assets.js`

**Reference:** [PublishSPAforGitHubPages.Build](https://www.nuget.org/packages/PublishSPAforGitHubPages.Build)

---

### Issue 5: Brotli Compression Not Served

**Symptom:** Larger download sizes than expected.

**Cause:** GitHub Pages doesn't serve pre-compressed `.br` files with correct headers.

**Solution:** Accept gzip compression (automatic) or implement client-side Brotli decompression.

**Reference:** [BlazorWebAssemblyXrefGenerator decode.js example](https://github.com/AdrienTorris/awesome-blazor)

---

## Project Structure Example

```
API-Samples/
├── .github/
│   └── workflows/
│       └── deploy-gh-pages.yml
├── .gitattributes                    # *.js binary
├── docs/                             # docfx documentation source
│   └── docfx.json
├── src/
│   └── PromptGenerator/
│       ├── PromptGenerator.csproj
│       ├── Program.cs
│       ├── Pages/
│       │   ├── Index.razor
│       │   └── Generator.razor
│       └── wwwroot/
│           ├── index.html
│           ├── .nojekyll
│           └── css/
└── README.md
```

---

## Local Development

### Running Locally with Correct Base Path

```bash
# Standard local development (base href = "/")
dotnet run --project src/PromptGenerator

# Testing with GitHub Pages base path
dotnet run --project src/PromptGenerator --urls "http://localhost:5000/API-Samples/"
```

### Building for GitHub Pages Manually

```bash
dotnet publish src/PromptGenerator -c Release

# Output will be in:
# src/PromptGenerator/bin/Release/net10.0/publish/wwwroot/
```

---

## References

### Official Documentation

- [Microsoft: Host and deploy Blazor WASM](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly) - Comprehensive hosting guide
- [Microsoft: GitHub Pages deployment](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/github-pages) - Specific GitHub Pages instructions
- [Microsoft: App base path](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/app-base-path) - Base href configuration

### Example Repositories

- [dotnet/blazor-samples](https://github.com/dotnet/blazor-samples) - Official Microsoft samples with GitHub Pages deployment
- [fernandreu/blazor-pages](https://github.com/fernandreu/blazor-pages) - Community example with automated deployment
- [AdrienTorris/awesome-blazor](https://github.com/AdrienTorris/awesome-blazor) - Curated list of Blazor resources

### Tutorials

- [Swimburger: How to deploy Blazor WASM to GitHub Pages](https://swimburger.net/blog/dotnet/how-to-deploy-aspnet-blazor-webassembly-to-github-pages) - Detailed walkthrough
- [I Love DotNet: Blazor WASM Publishing to GitHub Pages](https://ilovedotnet.org/blogs/blazor-wasm-publishing-to-github-pages/) - Step-by-step guide
- [David Guida: How to deploy Blazor WASM on GitHub Pages](https://www.davidguida.net/how-to-deploy-blazor-webassembly-on-github-pages-using-github-actions/) - GitHub Actions focus

### GitHub Actions

- [SteveSandersonMS/ghaction-rewrite-base-href](https://github.com/AdrienTorris/awesome-blazor) - Base href rewrite action
- [blazor-github-pages Action](https://github.com/marketplace/actions/github-pages-blazor-wasm) - Marketplace action for simplified deployment

---

## Summary Checklist

Before first deployment:
- [ ] Add `.gitattributes` with `*.js binary` to repo root
- [ ] Add `.nojekyll` to wwwroot
- [ ] Add `404.html` to wwwroot (copy of index.html)
- [ ] Set `<base href="/repo-name/" />` in index.html (or use workflow to rewrite)
- [ ] Create GitHub Actions workflow
- [ ] Configure GitHub Pages to use "GitHub Actions" as source
- [ ] (Optional) Add CNAME file for custom domain

After deployment:
- [ ] Verify site loads at `https://username.github.io/repo-name/`
- [ ] Test direct navigation to routes
- [ ] Check browser console for integrity errors
