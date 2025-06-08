#!/bin/bash

# PDF Template CLI - E2E Validation Test Runner
# This script runs the E2E validation tests using fixtures and LLM validation

echo "🧪 PDF Template CLI - E2E Validation Test Runner"
echo "================================================"
echo ""

# Check if API key is configured
API_KEY=$(grep -Po '"ApiKey"\s*:\s*"\K[^"]*' src/Adapters/CLI/appsettings.json 2>/dev/null || echo "")
if [ -z "$API_KEY" ]; then
    echo "⚠️  WARNING: OpenAI API key not configured!"
    echo "Please set the OpenAI API key in src/Adapters/CLI/appsettings.json"
    echo "Or set the OPENAI_API_KEY environment variable"
    echo ""
    # Check if environment variable is set
    if [ -n "$OPENAI_API_KEY" ]; then
        echo "✓ Found OPENAI_API_KEY environment variable"
    else
        read -p "Continue anyway? (y/N): " -n 1 -r
        echo ""
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    fi
fi

# Clean previous test outputs
echo "🧹 Cleaning previous test outputs..."
rm -rf tests/E2E/test-outputs
rm -f tests/E2E/validation-summary.json

# Build the E2E test project
echo "🔨 Building E2E test project..."
dotnet build tests/E2E/E2E.csproj -c Release --nologo -v q

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

# Run the validation tests
echo ""
echo "🚀 Running validation tests with LLM..."
echo "This will process all fixtures and validate outputs using AI"
echo ""

dotnet run --project tests/E2E/E2E.csproj -c Release --no-build

# Check if validation summary was created
if [ -f "tests/E2E/validation-summary.json" ]; then
    echo ""
    echo "✅ Validation tests completed!"
    echo ""
    echo "📊 Summary:"
    # Extract and display key metrics from the JSON summary
    if command -v jq &> /dev/null; then
        # If jq is installed, use it for pretty output
        jq -r '.summary | "Total Tests: \(.totalTests)\nPassed: \(.passed) ✅\nFailed: \(.failed) ❌\nAverage Confidence: \(.averageConfidence | tonumber | .*100 | round/100)%"' tests/E2E/validation-summary.json
    else
        # Fallback to grep/sed
        echo "View detailed results in: tests/E2E/validation-summary.json"
    fi
    echo ""
    echo "📁 Generated outputs: tests/E2E/test-outputs/"
    echo "📄 Full report: tests/E2E/validation-summary.json"
else
    echo "❌ Validation tests failed or did not complete"
    exit 1
fi