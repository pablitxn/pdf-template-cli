# PDF Template CLI - E2E Validation Test Runner
# This script runs the E2E validation tests using fixtures and LLM validation

Write-Host "🧪 PDF Template CLI - E2E Validation Test Runner" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if API key is configured
$appsettings = Get-Content "src\Adapters\CLI\appsettings.json" | ConvertFrom-Json
$apiKey = $appsettings.OpenAI.ApiKey
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Host "⚠️  WARNING: OpenAI API key not configured!" -ForegroundColor Yellow
    Write-Host "Please set the OpenAI API key in src\Adapters\CLI\appsettings.json" -ForegroundColor Yellow
    Write-Host "Or set the OPENAI_API_KEY environment variable" -ForegroundColor Yellow
    Write-Host ""
    # Check if environment variable is set
    if ($env:OPENAI_API_KEY) {
        Write-Host "✓ Found OPENAI_API_KEY environment variable" -ForegroundColor Green
    } else {
        $response = Read-Host "Continue anyway? (y/N)"
        if ($response -ne 'y' -and $response -ne 'Y') {
            exit 1
        }
    }
}

# Note: We're not cleaning test-outputs anymore since we're using timestamped folders
Write-Host "📁 Test outputs will be saved in timestamped folder..." -ForegroundColor Gray

# Build the E2E test project
Write-Host "🔨 Building E2E test project..." -ForegroundColor Gray
dotnet build tests\E2E\E2E.csproj -c Release --nologo -v q

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Run the validation tests
Write-Host ""
Write-Host "🚀 Running validation tests with LLM..." -ForegroundColor Green
Write-Host "This will process all fixtures and validate outputs using AI" -ForegroundColor Gray
Write-Host ""

dotnet run --project tests\E2E\E2E.csproj -c Release --no-build

# Check if validation summary was created
if (Test-Path "tests\E2E\validation-summary.json") {
    Write-Host ""
    Write-Host "✅ Validation tests completed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Summary:" -ForegroundColor Cyan
    
    # Parse and display summary
    $summary = Get-Content "tests\E2E\validation-summary.json" | ConvertFrom-Json
    Write-Host "Total Tests: $($summary.summary.totalTests)"
    Write-Host "Passed: $($summary.summary.passed) ✅" -ForegroundColor Green
    Write-Host "Failed: $($summary.summary.failed) ❌" -ForegroundColor Red
    Write-Host "Average Confidence: $([math]::Round($summary.summary.averageConfidence * 100, 2))%"
    
    Write-Host ""
    Write-Host "📁 Generated outputs: tests\E2E\test-outputs\" -ForegroundColor Gray
    Write-Host "📄 Full report: tests\E2E\validation-summary.json" -ForegroundColor Gray
} else {
    Write-Host "❌ Validation tests failed or did not complete" -ForegroundColor Red
    exit 1
}