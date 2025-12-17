import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Stress test: Gradually increase load to find breaking point
// Run with: k6 run stress-test.js

const errorRate = new Rate('errors');

export const options = {
  stages: parseStages(__ENV.K6_STAGES) || [
    { duration: '10s', target: 10 }, // Ramp up to 10 users
    { duration: '30s', target: 50 }, // Increase to 50 users
    { duration: '10s', target: 100 }, // Spike to 100 users
    { duration: '30s', target: 100 }, // Stay at 100 users
    { duration: '10s', target: 0 }, // Ramp down to 0
  ],
  thresholds: {
    http_req_failed: ['rate<0.1'], // Less than 10% errors (stress allows more)
    http_req_duration: ['p(95)<1000'], // 95th percentile < 1s
    errors: ['rate<0.1'],
  },
};

const API_URL = __ENV.K6_API_URL || 'http://localhost:9001';

// Parse K6_STAGES environment variable
// Format: "10s:10,30s:50,10s:100,30s:0"
function parseStages(stagesStr) {
  if (!stagesStr) return null;

  return stagesStr.split(',').map(stage => {
    const [duration, target] = stage.split(':');
    return {
      duration: duration,
      target: parseInt(target),
    };
  });
}

export default function () {
  // Test health endpoint
  let res = http.get(`${API_URL}/health`);
  let success = check(res, {
    'health check status is 200': r => r.status === 200,
  });
  errorRate.add(!success);

  sleep(1);

  // Test API endpoints under stress
  res = http.get(`${API_URL}/api/v1/projects`, {
    headers: {
      Accept: 'application/json',
    },
  });
  success = check(res, {
    'projects endpoint responds': r => r.status === 200 || r.status === 500 || r.status === 503,
  });
  errorRate.add(res.status !== 200);

  sleep(1);

  // Create project (write operation under stress)
  const createPayload = JSON.stringify({
    name: `Stress Test Project ${Date.now()}-${__VU}-${__ITER}`,
    description: 'K6 stress test project',
  });

  res = http.post(`${API_URL}/api/v1/projects`, createPayload, {
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
  });
  success = check(res, {
    'create project responds': r => [200, 201, 400, 500, 503].includes(r.status),
  });
  errorRate.add(![200, 201].includes(res.status));

  sleep(Math.random() * 2); // Random sleep 0-2s
}

export function handleSummary(data) {
  console.log('\n========================================');
  console.log('Stress Test Summary');
  console.log('========================================');
  console.log(`Total Requests: ${data.metrics.http_reqs.values.count}`);
  console.log(`Failed Requests: ${data.metrics.http_req_failed.values.rate * 100}%`);
  console.log(`Avg Response Time: ${data.metrics.http_req_duration.values.avg}ms`);
  console.log(`95th Percentile: ${data.metrics.http_req_duration.values['p(95)']}ms`);
  console.log(`99th Percentile: ${data.metrics.http_req_duration.values['p(99)']}ms`);
  console.log('========================================\n');

  return {
    stdout: textSummary(data, { indent: ' ', enableColors: true }),
    './test-results/k6-stress-summary.json': JSON.stringify(data),
  };
}
