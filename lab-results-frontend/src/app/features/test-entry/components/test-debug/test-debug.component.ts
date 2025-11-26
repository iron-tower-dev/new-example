import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../shared/services/auth.service';

@Component({
    selector: 'app-test-debug',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div style="padding: 20px; background: #f0f0f0; margin: 20px; border-radius: 8px;">
      <h1>🎉 TEST DEBUG COMPONENT</h1>
      <p><strong>✅ SUCCESS:</strong> The routing to /tests is working!</p>
      <p><strong>Time:</strong> {{ currentTime }}</p>
      <hr>
      <h3>Authentication Status:</h3>
      <p><strong>Is Authenticated:</strong> {{ authService.isAuthenticated() ? '✅ YES' : '❌ NO' }}</p>
      <p><strong>Current User:</strong> {{ authService.currentUser()?.fullName || 'None' }}</p>
      <p><strong>User Role:</strong> {{ authService.currentUser()?.role || 'None' }}</p>
      <p><strong>Token Present:</strong> {{ authService.getToken() ? '✅ YES' : '❌ NO' }}</p>
      <hr>
      <p><strong>Next Step:</strong> If authentication looks good, we can switch back to the dashboard!</p>
    </div>
  `
})
export class TestDebugComponent {
    currentTime = new Date().toLocaleString();
    authService = inject(AuthService);
}