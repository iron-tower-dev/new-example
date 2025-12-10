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
        const samplesUrl = this.environment.getApiEndpoint('samples');
        return this.http.get<any[]>(`${samplesUrl}/by-test/${testId}`).pipe(
            map(samples => samples.map(sample => ({
                id: sample.id.toString(),
                tagNumber: sample.tagNumber,
                component: sample.component,
                location: sample.location,
                lubeType: sample.lubeType,
                sampleDate: new Date(sample.sampleDate),
                status: sample.status,
                customerName: sample.customerName || 'Unknown',
                hasPartialData: sample.hasPartialData || false
            })))
        );
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