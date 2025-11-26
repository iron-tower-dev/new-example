import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { TestEntryRoutingModule } from './test-entry-routing.module';

// Note: Test Entry Components are standalone and imported directly in routing

@NgModule({
    imports: [
        SharedModule,
        TestEntryRoutingModule
    ],
    declarations: [
        // Note: Standalone components cannot be declared here
        // They are imported directly in routing or where used
    ]
})
export class TestEntryModule { }