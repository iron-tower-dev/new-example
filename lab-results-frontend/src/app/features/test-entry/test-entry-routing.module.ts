import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
    // Test list - shows all qualified tests
    {
        path: '',
        loadComponent: () => import('./components/test-list/test-list.component').then(c => c.TestListComponent),
        data: { title: 'Available Tests' }
    },

    // Specific test entry components
    {
        path: 'tan/:sampleId',
        loadComponent: () => import('./components/tan-test-entry/tan-test-entry.component').then(c => c.TanTestEntryComponent),
        data: { title: 'TAN Test Entry' }
    },
    {
        path: 'water-kf/:sampleId',
        loadComponent: () => import('./components/water-kf-test-entry/water-kf-test-entry.component').then(c => c.WaterKfTestEntryComponent),
        data: { title: 'Water-KF Test Entry' }
    },
    {
        path: 'tbn/:sampleId',
        loadComponent: () => import('./components/tbn-test-entry/tbn-test-entry.component').then(c => c.TbnTestEntryComponent),
        data: { title: 'TBN Test Entry' }
    },
    {
        path: 'viscosity-40c/:sampleId',
        loadComponent: () => import('./components/viscosity-40c-test-entry/viscosity-40c-test-entry.component').then(c => c.Viscosity40cTestEntryComponent),
        data: { title: 'Viscosity @ 40°C Test Entry' }
    },
    {
        path: 'viscosity-100c/:sampleId',
        loadComponent: () => import('./components/viscosity-100c-test-entry/viscosity-100c-test-entry.component').then(c => c.Viscosity100cTestEntryComponent),
        data: { title: 'Viscosity @ 100°C Test Entry' }
    },
    {
        path: 'flash-point/:sampleId',
        loadComponent: () => import('./components/flash-point-test-entry/flash-point-test-entry.component').then(c => c.FlashPointTestEntryComponent),
        data: { title: 'Flash Point Test Entry' }
    },
    {
        path: 'emission-spectroscopy/:sampleId',
        loadComponent: () => import('./components/emission-spectroscopy-test-entry/emission-spectroscopy-test-entry.component').then(c => c.EmissionSpectroscopyTestEntryComponent),
        data: { title: 'Emission Spectroscopy Test Entry' }
    },
    {
        path: 'particle-count/:sampleId',
        loadComponent: () => import('./components/particle-count-test-entry/particle-count-test-entry.component').then(c => c.ParticleCountTestEntryComponent),
        data: { title: 'Particle Count Test Entry' }
    },
    {
        path: 'grease-penetration/:sampleId',
        loadComponent: () => import('./components/grease-penetration-test-entry/grease-penetration-test-entry.component').then(c => c.GreasePenetrationTestEntryComponent),
        data: { title: 'Grease Penetration Test Entry' }
    },
    {
        path: 'grease-dropping-point/:sampleId',
        loadComponent: () => import('./components/grease-dropping-point-test-entry/grease-dropping-point-test-entry.component').then(c => c.GreaseDroppingPointTestEntryComponent),
        data: { title: 'Grease Dropping Point Test Entry' }
    },
    {
        path: 'inspect-filter/:sampleId',
        loadComponent: () => import('./components/inspect-filter-test-entry/inspect-filter-test-entry.component').then(c => c.InspectFilterTestEntryComponent),
        data: { title: 'Inspect Filter Test Entry' }
    },
    {
        path: 'ferrography/:sampleId',
        loadComponent: () => import('./components/ferrography-test-entry/ferrography-test-entry.component').then(c => c.FerrographyTestEntryComponent),
        data: { title: 'Ferrography Test Entry' }
    },
    {
        path: 'rbot/:sampleId',
        loadComponent: () => import('./components/rbot-test-entry/rbot-test-entry.component').then(c => c.RbotTestEntryComponent),
        data: { title: 'RBOT Test Entry' }
    },
    {
        path: 'tfout/:sampleId',
        loadComponent: () => import('./components/tfout-test-entry/tfout-test-entry.component').then(c => c.TfoutTestEntryComponent),
        data: { title: 'TFOUT Test Entry' }
    },
    {
        path: 'rust/:sampleId',
        loadComponent: () => import('./components/rust-test-entry/rust-test-entry.component').then(c => c.RustTestEntryComponent),
        data: { title: 'Rust Test Entry' }
    },
    {
        path: 'deleterious/:sampleId',
        loadComponent: () => import('./components/deleterious-test-entry/deleterious-test-entry.component').then(c => c.DeleteriousTestEntryComponent),
        data: { title: 'Deleterious Test Entry' }
    },
    {
        path: 'd-inch/:sampleId',
        loadComponent: () => import('./components/d-inch-test-entry/d-inch-test-entry.component').then(c => c.DInchTestEntryComponent),
        data: { title: 'D-inch Test Entry' }
    },
    {
        path: 'oil-content/:sampleId',
        loadComponent: () => import('./components/oil-content-test-entry/oil-content-test-entry.component').then(c => c.OilContentTestEntryComponent),
        data: { title: 'Oil Content Test Entry' }
    },
    {
        path: 'varnish-potential-rating/:sampleId',
        loadComponent: () => import('./components/varnish-potential-rating-test-entry/varnish-potential-rating-test-entry.component').then(c => c.VarnishPotentialRatingTestEntryComponent),
        data: { title: 'Varnish Potential Rating Test Entry' }
    },
    {
        path: 'ft-ir/:sampleId',
        loadComponent: () => import('./components/ftir-test-entry/ftir-test-entry.component').then(c => c.FtirTestEntryComponent),
        data: { title: 'FT-IR Test Entry' }
    },
    {
        path: 'filter-residue/:sampleId',
        loadComponent: () => import('./components/filter-residue-test-entry/filter-residue-test-entry.component').then(c => c.FilterResidueTestEntryComponent),
        data: { title: 'Filter Residue Test Entry' }
    },
    {
        path: 'debris-identification/:sampleId',
        loadComponent: () => import('./components/debris-identification-test-entry/debris-identification-test-entry.component').then(c => c.DebrisIdentificationTestEntryComponent),
        data: { title: 'Debris Identification Test Entry' }
    },
    {
        path: 'rheometer/:sampleId',
        loadComponent: () => import('./components/rheometer-test-entry/rheometer-test-entry.component').then(c => c.RheometerTestEntryComponent),
        data: { title: 'Rheometer Test Entry' }
    },

    // Test workspace routes (fallback for tests without dedicated components)
    {
        path: ':testType',
        loadComponent: () => import('./components/test-workspace/test-workspace.component').then(c => c.TestWorkspaceComponent),
        data: { title: 'Test Workspace' }
    },

    // Test workspace with sample selection
    {
        path: ':testType/:sampleId',
        loadComponent: () => import('./components/test-workspace/test-workspace.component').then(c => c.TestWorkspaceComponent),
        data: { title: 'Test Entry' }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class TestEntryRoutingModule { }