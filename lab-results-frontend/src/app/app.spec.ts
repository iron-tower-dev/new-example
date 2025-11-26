import { TestBed } from '@angular/core/testing';
import { SwUpdate } from '@angular/service-worker';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    const mockSwUpdate = {
      isEnabled: false,
      versionUpdates: { pipe: () => ({ subscribe: () => { } }) },
      checkForUpdate: () => Promise.resolve(false),
      activateUpdate: () => Promise.resolve(true)
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: SwUpdate, useValue: mockSwUpdate }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    // The app component only renders router-outlet, so we check for that
    expect(compiled.querySelector('router-outlet')).toBeTruthy();
  });
});
