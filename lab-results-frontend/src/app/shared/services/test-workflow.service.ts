import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { EnvironmentService } from './environment.service';

export interface TestType {
    id: number;
    name: string;
    description: string;
    requiredQualification?: string;
    testStandId?: number;
}

export interface SampleForTest {
    id: string;
    tagNumber: string;
    component: string;
    location: string;
    lubeType: string;
    sampleDate: Date;
    status: string;
    customerName: string;
    hasPartialData?: boolean;
}

export interface TestWorkflowState {
    selectedTest: TestType | null;
    availableSamples: SampleForTest[];
    selectedSample: SampleForTest | null;
    isLoading: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class TestWorkflowService {
    private http = inject(HttpClient);
    private environment = inject(EnvironmentService);
    private readonly apiUrl = this.environment.getApiEndpoint('tests');

    // Workflow state signals
    private _workflowState = signal<TestWorkflowState>({
        selectedTest: null,
        availableSamples: [],
        selectedSample: null,
        isLoading: false
    });

    // Public readonly signals
    readonly workflowState = this._workflowState.asReadonly();
    readonly selectedTest = computed(() => this._workflowState().selectedTest);
    readonly availableSamples = computed(() => this._workflowState().availableSamples);
    readonly selectedSample = computed(() => this._workflowState().selectedSample);
    readonly isLoading = computed(() => this._workflowState().isLoading);

    // Get available test types from API based on user qualifications
    getAvailableTests(): Observable<TestType[]> {
        return this.http.get<any[]>(`${this.apiUrl}/qualified`).pipe(
            map(tests => tests.map(test => ({
                id: test.testId,
                name: test.testName,
                description: test.testDescription || '',
                requiredQualification: 'TRAIN' // Default, could be enhanced to get from API
            })))
        );
    }

    selectTest(test: TestType): void {
        this._workflowState.update(state => ({
            ...state,
            selectedTest: test,
            selectedSample: null,
            isLoading: true
        }));

        // Load samples for this test
        this.loadSamplesForTest(test.id).subscribe({
            next: (samples) => {
                this._workflowState.update(state => ({
                    ...state,
                    availableSamples: samples,
                    isLoading: false
                }));
            },
            error: (error) => {
                console.error('Error loading samples:', error);
                this._workflowState.update(state => ({
                    ...state,
                    availableSamples: [],
                    isLoading: false
                }));
            }
        });
    }

    selectSample(sample: SampleForTest): void {
        this._workflowState.update(state => ({
            ...state,
            selectedSample: sample
        }));
    }

    clearSelection(): void {
        this._workflowState.update(state => ({
            ...state,
            selectedTest: null,
            availableSamples: [],
            selectedSample: null,
            isLoading: false
        }));
    }

    private loadSamplesForTest(testId: number): Observable<SampleForTest[]> {
        // For now, return mock data. In real implementation, this would call the API
        return new Observable(observer => {
            setTimeout(() => {
                const mockSamples: SampleForTest[] = [
                    {
                        id: 'S001',
                        tagNumber: 'ENG-001',
                        component: 'Engine Oil',
                        location: 'Main Engine',
                        lubeType: 'Lubricating Oil',
                        sampleDate: new Date('2024-11-10'),
                        status: 'Ready for Testing',
                        customerName: 'Acme Corp',
                        hasPartialData: false
                    },
                    {
                        id: 'S002',
                        tagNumber: 'HYD-002',
                        component: 'Hydraulic Fluid',
                        location: 'Hydraulic System',
                        lubeType: 'Hydraulic Oil',
                        sampleDate: new Date('2024-11-11'),
                        status: 'Ready for Testing',
                        customerName: 'Beta Industries',
                        hasPartialData: true
                    },
                    {
                        id: 'S003',
                        tagNumber: 'GR-003',
                        component: 'Gear Oil',
                        location: 'Gearbox',
                        lubeType: 'Gear Oil',
                        sampleDate: new Date('2024-11-12'),
                        status: 'Ready for Testing',
                        customerName: 'Gamma LLC',
                        hasPartialData: false
                    }
                ];
                observer.next(mockSamples);
                observer.complete();
            }, 500); // Simulate API delay
        });
    }

    getTestRoute(testName: string): string {
        // Map test names to their routes
        const routeMap: { [key: string]: string } = {
            'TAN': 'tan',
            'Water-KF': 'water-kf',
            'TBN': 'tbn',
            'Viscosity @ 40°C': 'viscosity-40c',
            'Viscosity @ 100°C': 'viscosity-100c',
            'Flash Point': 'flash-point',
            'Emission Spectroscopy': 'emission-spectroscopy',
            'Particle Count': 'particle-count',
            'Grease Penetration': 'grease-penetration',
            'Grease Dropping Point': 'grease-dropping-point',
            'Inspect Filter': 'inspect-filter',
            'Ferrography': 'ferrography',
            'RBOT': 'rbot',
            'TFOUT': 'tfout',
            'Rust': 'rust',
            'Deleterious': 'deleterious',
            'D-inch': 'd-inch',
            'Oil Content': 'oil-content',
            'Varnish Potential Rating': 'varnish-potential-rating'
        };

        return routeMap[testName] || testName.toLowerCase().replace(/\s+/g, '-');
    }
}