import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Load test: Sustained load to test performance under normal conditions
// Run with: k6 run --vus 10 --duration 30s load-test.js

const errorRate = new Rate('errors');

export const options = {
  vus: parseInt(__ENV.K6_VUS) || 10,
  duration: __ENV.K6_DURATION || '30s',
  thresholds: {
    http_req_failed: ['rate<0.05'], // Less than 5% errors
    http_req_duration: ['p(95)<500', 'p(99)<1000'], // 95th percentile < 500ms, 99th < 1s
    errors: ['rate<0.05'],
  },
};

const API_URL = __ENV.K6_API_URL || 'http://localhost:9001';

export default function () {
  // Test health endpoint
  let res = http.get(`${API_URL}/health`);
  let success = check(res, {
    'health check status is 200': r => r.status === 200,
  });
  errorRate.add(!success);

  sleep(1);

  // Test API endpoints (customize based on your API)
  // Example: Projects endpoint
  res = http.get(`${API_URL}/api/v1/projects`, {
    headers: {
      Accept: 'application/json',
      // Add auth headers if needed:
      // 'Authorization': `Bearer ${__ENV.API_TOKEN}`,
    },
  });
  success = check(res, {
    'projects endpoint returns 200': r => r.status === 200,
    'response has projects array': r => {
      try {
        const body = JSON.parse(r.body);
        return Array.isArray(body) || body.hasOwnProperty('items');
      } catch (e) {
        return false;
      }
    },
  });
  errorRate.add(!success);

  sleep(1);

  // Example: Create project (POST request)
  const createPayload = JSON.stringify({
    name: `Test Project ${Date.now()}`,
    description: 'K6 load test project',
  });

  res = http.post(`${API_URL}/api/v1/projects`, createPayload, {
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
      // 'Authorization': `Bearer ${__ENV.API_TOKEN}`,
    },
  });
  success = check(res, {
    'create project returns 201 or 200': r => r.status === 201 || r.status === 200,
  });
  errorRate.add(!success);

  sleep(2);
}

export function handleSummary(data) {
  return {
    stdout: textSummary(data, { indent: ' ', enableColors: true }),
    './test-results/k6-load-summary.json': JSON.stringify(data),
  };
}
