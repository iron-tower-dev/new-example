import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { LoginComponent } from './shared/components/login/login.component';
import { UnauthorizedComponent } from './shared/components/unauthorized/unauthorized.component';
// Authentication guards removed for SSO migration
// import { authGuard, noAuthGuard } from './shared/guards/auth.guard';

export const routes: Routes = [
    {
        path: 'login',
        component: LoginComponent
        // canActivate: [noAuthGuard] // Removed for SSO migration
    },
    {
        path: 'unauthorized',
        component: UnauthorizedComponent
    },
    {
        path: '',
        component: MainLayoutComponent,
        // canActivate: [authGuard], // Removed for SSO migration
        children: [
            {
                path: '',
                redirectTo: '/samples',
                pathMatch: 'full'
            },
            {
                path: 'samples',
                loadChildren: () => import('./features/sample-management/sample-management.module').then(m => m.SampleManagementModule),
                data: { preload: true }
            },
            {
                path: 'tests',
                loadChildren: () => import('./features/test-entry/test-entry.module').then(m => m.TestEntryModule),
                data: { preload: false }
            },
            {
                path: 'migration',
                loadChildren: () => import('./features/migration/migration.module').then(m => m.MigrationModule),
                data: { preload: false }
            }
        ]
    },
    {
        path: '**',
        redirectTo: '/samples'
    }
];
