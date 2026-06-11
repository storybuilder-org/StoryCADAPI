# Serve StoryCAD documentation locally for testing
# Usage: pwsh serve-docs.ps1 [port]
# Requires: Ruby and Bundler installed
#
# The Jekyll site lives in docs/ (Gemfile + _config.yml are there), so this
# script runs Bundler from that directory.

param(
    [int]$Port = 4000
)

Write-Host "StoryCAD Documentation Server" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan

# Check if Ruby is installed
if (-not (Get-Command ruby -ErrorAction SilentlyContinue)) {
    Write-Host "Error: Ruby is not installed." -ForegroundColor Red
    Write-Host "Install Ruby first:"
    Write-Host "  Windows: https://rubyinstaller.org/"
    Write-Host "  macOS: brew install ruby"
    exit 1
}

# Run everything from the docs/ site directory
$docsDir = Join-Path $PSScriptRoot "docs"
Push-Location $docsDir
try {
    # Check if bundler is installed and working
    $bundlerOk = $false
    if (Get-Command bundle -ErrorAction SilentlyContinue) {
        bundle --version *> $null
        if ($LASTEXITCODE -eq 0) { $bundlerOk = $true }
    }

    if (-not $bundlerOk) {
        Write-Host "Installing/Reinstalling Bundler..." -ForegroundColor Yellow
        gem install bundler
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Failed to install Bundler" -ForegroundColor Red
            exit 1
        }
    }

    # Install dependencies if needed
    if (-not (Test-Path "Gemfile.lock")) {
        Write-Host "Installing Jekyll dependencies..." -ForegroundColor Yellow
        bundle install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Error: Failed to install dependencies. Try: gem install bundler" -ForegroundColor Red
            exit 1
        }
    }

    Write-Host ""
    Write-Host "Starting Jekyll server on http://localhost:$Port" -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
    Write-Host ""

    # Serve the site
    bundle exec jekyll serve --port $Port --livereload
}
finally {
    Pop-Location
}
