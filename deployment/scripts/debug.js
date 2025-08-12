#!/usr/bin/env node

// Debug script for Docker Compose
// This script helps manage the debug process

console.log('🚀 Starting Document Upload API Debug Session...');
console.log('📦 Docker Compose should be starting the services...');
console.log('🌐 Application will be available at: http://localhost:8080');
console.log('📊 Scalar API Documentation: http://localhost:8080/scalar/v1');
console.log('❤️  Health Check: http://localhost:8080/health');
console.log('');
console.log('💡 Use Ctrl+C to stop the debug session');
console.log('📝 Check the terminal for container logs');

// Keep the process alive to maintain the debug session
process.stdin.setRawMode(true);
process.stdin.resume();
process.stdin.on('data', (key) => {
  // Ctrl+C
  if (key.toString('hex') === '03') {
    console.log('\n🛑 Stopping debug session...');
    process.exit(0);
  }
});

// Handle cleanup
process.on('SIGINT', () => {
  console.log('\n🧹 Cleaning up...');
  process.exit(0);
});

// Simulate a long-running process
setInterval(() => {
  // Keep alive
}, 1000);
