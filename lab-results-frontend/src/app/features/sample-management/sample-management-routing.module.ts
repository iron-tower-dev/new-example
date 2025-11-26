import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SampleManagementDemoComponent } from './components/sample-management-demo/sample-management-demo.component';

const routes: Routes = [
    {
        path: '',
        component: SampleManagementDemoComponent
    },
    {
        path: 'demo',
        component: SampleManagementDemoComponent
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class SampleManagementRoutingModule { }