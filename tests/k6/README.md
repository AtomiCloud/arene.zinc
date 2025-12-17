# K6 Load Tests

Performance and load testing for Zinc API using [K6](https://k6.io/).

## Overview

These tests are designed to run against the **tauros** (system integration) environment with real infrastructure deployed to a k3d cluster.

## Test Types

### Smoke Test (`smoke-test.js`)
Quick validation that the API is up and basic endpoints work.

**Purpose**: Verify deployment was successful and API is responding.

**Configuration**:
- 1 virtual user
- 10 second duration
- Checks: health, root, swagger endpoints

**Run**:
```bash
# Via Garden (recommended - runs in tauros)
garden test zinc-smoke-tests --env tauros

# Manually
k6 run --vus 1 --duration 10s smoke-test.js
```

**Expected Result**: < 1% errors, 95th percentile < 500ms

---

### Load Test (`load-test.js`)
Sustained load to test performance under normal conditions.

**Purpose**: Validate API performance under expected traffic.

**Configuration**:
- 10 virtual users (configurable)
- 30 second duration (configurable)
- Tests: health, list projects, create project

**Run**:
```bash
# Via Garden (recommended - runs in tauros)
garden test zinc-load-tests --env tauros

# Manually with custom settings
K6_VUS=20 K6_DURATION=60s k6 run load-test.js

# With API token
K6_API_URL=http://api.zinc.arene.tauros.lvh.me:20010 k6 run load-test.js
```

**Expected Result**: < 5% errors, 95th percentile < 500ms, 99th percentile < 1s

---

### Stress Test (`stress-test.js`)
Gradually increase load to find breaking point.

**Purpose**: Discover system limits and performance degradation patterns.

**Configuration**:
- Stages: 10→50→100 users over 90 seconds
- Ramp up and ramp down pattern
- Measures response times under increasing stress

**Run**:
```bash
# Via Garden (recommended - runs in tauros)
garden test zinc-stress-tests --env tauros

# Manually with custom stages
K6_STAGES="10s:10,30s:50,10s:100,30s:0" k6 run stress-test.js
```

**Expected Result**: Identify breaking point, < 10% errors at moderate load

---

## Test Results

Test results are saved to `./test-results/`:
- `k6-smoke-summary.json` - Smoke test results
- `k6-load-summary.json` - Load test results
- `k6-stress-summary.json` - Stress test results

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `K6_API_URL` | Base API URL | `http://localhost:9001` |
| `K6_VUS` | Number of virtual users | `10` |
| `K6_DURATION` | Test duration | `30s` |
| `K6_STAGES` | Stress test stages (format: `10s:10,30s:50`) | See stress-test.js |
| `K6_THRESHOLDS` | Custom thresholds | - |
| `API_TOKEN` | Authentication token (if required) | - |

## Writing Custom Tests

### Basic Structure

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 10,
  duration: '30s',
  thresholds: {
    http_req_duration: ['p(95)<500'],
  },
};

export default function () {
  const res = http.get('http://api.example.com/endpoint');
  check(res, {
    'status is 200': (r) => r.status === 200,
  });
  sleep(1);
}
```

### Using Checks

```javascript
check(response, {
  'status is 200': (r) => r.status === 200,
  'response time < 200ms': (r) => r.timings.duration < 200,
  'body contains data': (r) => r.body.includes('data'),
});
```

### Custom Metrics

```javascript
import { Trend, Counter } from 'k6/metrics';

const customDuration = new Trend('custom_duration');
const errorCount = new Counter('errors');

export default function () {
  const start = Date.now();
  // ... do work ...
  customDuration.add(Date.now() - start);
}
```

## Integration with Garden

K6 tests are defined in `garden.yml` as Test actions:

```yaml
kind: Test
type: exec
name: zinc-load-tests
include: [tauros]

dependencies:
  - deploy.zinc

spec:
  command: [k6, run, ./tests/k6/load-test.js]
  env:
    K6_API_URL: http://zinc-api.arene.svc.cluster.local:9001
```

Run via Garden:
```bash
# All tests
garden test --env tauros

# Specific test
garden test zinc-load-tests --env tauros
```

## CI/CD Integration

### Example GitHub Actions

```yaml
name: System Integration Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Install K6
        run: |
          sudo gpg -k
          sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
          echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
          sudo apt-get update
          sudo apt-get install k6

      - name: Run K6 Tests
        run: |
          garden test zinc-load-tests --env tauros
```

## Best Practices

1. **Start Small**: Begin with smoke tests, then load tests, then stress tests
2. **Realistic Data**: Use production-like data and scenarios
3. **Monitor Resources**: Watch CPU, memory, and database during tests
4. **Set Thresholds**: Define acceptable performance criteria
5. **Iterate**: Adjust load patterns based on real usage
6. **Cleanup**: Delete test data after runs to avoid pollution

## Thresholds Reference

Common thresholds for different scenarios:

**Smoke Tests (Quick Validation)**:
```javascript
thresholds: {
  http_req_failed: ['rate<0.01'],    // < 1% errors
  http_req_duration: ['p(95)<500'],  // 95% < 500ms
}
```

**Load Tests (Normal Traffic)**:
```javascript
thresholds: {
  http_req_failed: ['rate<0.05'],     // < 5% errors
  http_req_duration: ['p(95)<500', 'p(99)<1000'], // 95% < 500ms, 99% < 1s
}
```

**Stress Tests (Find Limits)**:
```javascript
thresholds: {
  http_req_failed: ['rate<0.1'],      // < 10% errors (higher tolerance)
  http_req_duration: ['p(95)<1000'],  // 95% < 1s (degraded acceptable)
}
```

## Troubleshooting

### Test Fails: Connection Refused

**Problem**: K6 can't reach the API

**Solution**:
```bash
# Verify API is deployed
kubectl get pods -n arene-zinc-tauros

# Check API URL is correct
echo $K6_API_URL

# Test from cluster
kubectl run -it --rm debug --image=curlimages/curl --restart=Never -- \
  curl http://zinc-api.arene-zinc-tauros.svc.cluster.local:9001/health
```

### Test Fails: High Error Rate

**Problem**: Many requests failing

**Solution**:
1. Check API logs: `kubectl logs -n arene-zinc-tauros deployment/zinc-api`
2. Check database: `kubectl get pods -n arene-zinc-tauros | grep maindb`
3. Reduce VUs: `K6_VUS=5 k6 run load-test.js`
4. Increase resources in values.tauros.yaml

### Test Fails: Slow Response Times

**Problem**: Requests timing out or very slow

**Solution**:
1. Check resource limits: `kubectl top pods -n arene-zinc-tauros`
2. Check database performance
3. Reduce test duration: `K6_DURATION=10s k6 run load-test.js`
4. Scale up API replicas in values.tauros.yaml

## References

- [K6 Documentation](https://k6.io/docs/)
- [K6 Examples](https://k6.io/docs/examples/)
- [K6 Thresholds](https://k6.io/docs/using-k6/thresholds/)
- [K6 Checks](https://k6.io/docs/using-k6/checks/)
- [Garden Testing](https://docs.garden.io/guides/testing)
