import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { SampleManagementRoutingModule } from './sample-management-routing.module';

// Components
import { SampleSelectionComponent } from './components/sample-selection/sample-selection.component';
import { SampleInfoDisplayComponent } from './components/sample-info-display/sample-info-display.component';
import { SampleDataTemplateComponent } from './components/sample-data-template/sample-data-template.component';
import { SampleManagementDemoComponent } from './components/sample-management-demo/sample-management-demo.component';
import { CommonNavigationComponent } from '../../shared/components/common-navigation/common-navigation.component';

@NgModule({
    imports: [
        SharedModule,
        SampleManagementRoutingModule,
        SampleSelectionComponent,
        SampleInfoDisplayComponent,
        SampleDataTemplateComponent,
        SampleManagementDemoComponent,
        CommonNavigationComponent
    ],
    exports: [
        SampleSelectionComponent,
        SampleInfoDisplayComponent,
        SampleDataTemplateComponent,
        SampleManagementDemoComponent,
        CommonNavigationComponent
    ]
})
export class SampleManagementModule { }