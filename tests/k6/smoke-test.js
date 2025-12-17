import http from 'k6/http';
import { check, sleep } from 'k6';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

// Smoke test: Quick validation that the API is up and basic endpoints work
// Run with: k6 run --vus 1 --duration 10s smoke-test.js

export const options = {
  vus: 1,
  duration: '10s',
  thresholds: {
    http_req_failed: ['rate<0.01'], // Less than 1% errors
    http_req_duration: ['p(95)<500'], // 95% of requests should complete in <500ms
  },
};

const API_URL = __ENV.K6_API_URL || 'http://localhost:9001';

export default function () {
  // Test health endpoint
  const healthRes = http.get(`${API_URL}/health`);
  check(healthRes, {
    'health endpoint returns 200': r => r.status === 200,
  });

  // Test root endpoint (Swagger redirect or API info)
  const rootRes = http.get(`${API_URL}/`);
  check(rootRes, {
    'root endpoint responds': r => r.status === 200 || r.status === 301 || r.status === 302,
  });

  // Test Swagger UI (if enabled)
  const swaggerRes = http.get(`${API_URL}/swagger/index.html`);
  check(swaggerRes, {
    'swagger UI loads': r => r.status === 200,
  });

  sleep(1);
}

export function handleSummary(data) {
  return {
    stdout: textSummary(data, { indent: ' ', enableColors: true }),
    './test-results/k6-smoke-summary.json': JSON.stringify(data),
  };
}
