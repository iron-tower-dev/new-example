import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { MigrationDashboardComponent } from './components/migration-dashboard/migration-dashboard.component';
import { MigrationService } from './services/migration.service';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        HttpClientModule,
        MigrationDashboardComponent,
        RouterModule.forChild([
            {
                path: '',
                component: MigrationDashboardComponent
            }
        ])
    ],
    providers: [
        MigrationService
    ]
})
export class MigrationModule { }