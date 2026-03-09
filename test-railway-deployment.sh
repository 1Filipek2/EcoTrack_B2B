#!/bin/bash
# EcoTrack Railway Deployment Test Script
# Usage: ./test-railway-deployment.sh https://your-app.railway.app

set -e

if [ -z "$1" ]; then
    echo "❌ Chyba: Musíš zadať Railway URL"
    echo "Usage: $0 https://your-app.railway.app"
    exit 1
fi

BASE_URL="$1"
API_URL="${BASE_URL}/api"

echo "════════════════════════════════════════"
echo "🧪 EcoTrack Railway Deployment Test"
echo "════════════════════════════════════════"
echo "Base URL: $BASE_URL"
echo "API URL:  $API_URL"
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

pass_count=0
fail_count=0

# Test function
test_endpoint() {
    local name="$1"
    local url="$2"
    local method="${3:-GET}"
    local data="$4"
    local expected_status="${5:-200}"
    
    echo -n "Testing: $name ... "
    
    if [ -n "$data" ]; then
        response=$(curl -s -w "\n%{http_code}" -X "$method" "$url" \
            -H "Content-Type: application/json" \
            -d "$data" 2>/dev/null)
    else
        response=$(curl -s -w "\n%{http_code}" -X "$method" "$url" 2>/dev/null)
    fi
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | head -n-1)
    
    if [ "$http_code" = "$expected_status" ]; then
        echo -e "${GREEN}✓ PASS${NC} (HTTP $http_code)"
        ((pass_count++))
        return 0
    else
        echo -e "${RED}✗ FAIL${NC} (Expected HTTP $expected_status, got $http_code)"
        echo "  Response: $body"
        ((fail_count++))
        return 1
    fi
}

echo "────────────────────────────────────────"
echo "1️⃣  HEALTH CHECKS"
echo "────────────────────────────────────────"

test_endpoint "Root endpoint" "$BASE_URL/" "GET" "" "200"
test_endpoint "Health check" "$BASE_URL/health" "GET" "" "200"

echo ""
echo "────────────────────────────────────────"
echo "2️⃣  AUTHENTICATION FLOW"
echo "────────────────────────────────────────"

# Generate random email for test
TEST_EMAIL="test-$(date +%s)@ecotrack.com"
TEST_PASSWORD="TestPassword123!"

echo "Test email: $TEST_EMAIL"

# Register
register_data="{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}"
if test_endpoint "Register user" "$API_URL/auth/register" "POST" "$register_data" "200"; then
    echo "  ✓ User registration successful"
fi

# Login
login_data="{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASSWORD\"}"
login_response=$(curl -s -X POST "$API_URL/auth/login" \
    -H "Content-Type: application/json" \
    -d "$login_data" 2>/dev/null)

if echo "$login_response" | grep -q "token"; then
    TOKEN=$(echo "$login_response" | grep -o '"token":"[^"]*' | cut -d'"' -f4)
    echo -e "Testing: Login user ... ${GREEN}✓ PASS${NC}"
    echo "  ✓ JWT token received (length: ${#TOKEN})"
    ((pass_count++))
else
    echo -e "Testing: Login user ... ${RED}✗ FAIL${NC}"
    echo "  Response: $login_response"
    ((fail_count++))
    TOKEN=""
fi

echo ""
echo "────────────────────────────────────────"
echo "3️⃣  PROTECTED ENDPOINTS (with JWT)"
echo "────────────────────────────────────────"

if [ -n "$TOKEN" ]; then
    # Get emission categories
    categories_response=$(curl -s -X GET "$API_URL/emissions/categories" \
        -H "Authorization: Bearer $TOKEN" 2>/dev/null)
    
    if echo "$categories_response" | grep -q "Electricity\|Natural Gas"; then
        echo -e "Testing: Get emission categories ... ${GREEN}✓ PASS${NC}"
        category_count=$(echo "$categories_response" | grep -o '"id"' | wc -l)
        echo "  ✓ Retrieved $category_count categories"
        ((pass_count++))
    else
        echo -e "Testing: Get emission categories ... ${RED}✗ FAIL${NC}"
        echo "  Response: $categories_response"
        ((fail_count++))
    fi
    
    # Test without auth (should fail)
    unauth_response=$(curl -s -w "\n%{http_code}" -X GET "$API_URL/emissions/categories" 2>/dev/null)
    unauth_code=$(echo "$unauth_response" | tail -n1)
    
    if [ "$unauth_code" = "401" ]; then
        echo -e "Testing: Unauthorized access blocked ... ${GREEN}✓ PASS${NC}"
        echo "  ✓ Returns 401 without JWT token"
        ((pass_count++))
    else
        echo -e "Testing: Unauthorized access blocked ... ${YELLOW}⚠ WARNING${NC}"
        echo "  ⚠ Expected 401, got $unauth_code (auth may be disabled)"
        ((fail_count++))
    fi
else
    echo -e "${YELLOW}⚠ Skipping protected endpoint tests (no token)${NC}"
fi

echo ""
echo "────────────────────────────────────────"
echo "4️⃣  DATABASE CONNECTIVITY"
echo "────────────────────────────────────────"

# Check if categories endpoint works (proves DB connection)
db_test=$(curl -s -X GET "$API_URL/emissions/categories" \
    -H "Authorization: Bearer $TOKEN" 2>/dev/null)

if echo "$db_test" | grep -q '\[.*\]'; then
    echo -e "Testing: Database connection ... ${GREEN}✓ PASS${NC}"
    echo "  ✓ Successfully querying database"
    ((pass_count++))
else
    echo -e "Testing: Database connection ... ${RED}✗ FAIL${NC}"
    echo "  ✗ Cannot retrieve data from database"
    ((fail_count++))
fi

echo ""
echo "════════════════════════════════════════"
echo "📊 TEST SUMMARY"
echo "════════════════════════════════════════"
echo -e "${GREEN}Passed:${NC} $pass_count"
echo -e "${RED}Failed:${NC} $fail_count"
echo "Total:  $((pass_count + fail_count))"
echo ""

if [ $fail_count -eq 0 ]; then
    echo -e "${GREEN}✅ All tests passed! Deployment is healthy.${NC}"
    echo ""
    echo "🚀 Next steps:"
    echo "  1. Deploy frontend to Vercel"
    echo "  2. Update CORS in Railway with frontend URL"
    echo "  3. Test full stack integration"
    exit 0
else
    echo -e "${RED}❌ Some tests failed. Check logs above.${NC}"
    echo ""
    echo "Common fixes:"
    echo "  - Check Railway environment variables"
    echo "  - Verify database connection string"
    echo "  - Check Railway logs for errors"
    exit 1
fi

