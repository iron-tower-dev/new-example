import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SwUpdateService } from './shared/services/sw-update.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet></router-outlet>',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  title = 'Laboratory Test Results Entry System';

  private swUpdateService = inject(SwUpdateService);

  ngOnInit(): void {
    // SwUpdateService is initialized automatically via injection
    // This ensures service worker update handling is active
  }
}
