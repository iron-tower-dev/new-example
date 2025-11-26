import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SampleService } from './sample.service';
import { Sample, SampleHistory, SampleFilter } from '../models/sample.model';

describe('SampleService', () => {
    let service: SampleService;
    let httpMock: HttpTestingController;
    const baseUrl = 'http://localhost:5000/api/samples';

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [SampleService]
        });
        service = TestBed.inject(SampleService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getSamplesByTest', () => {
        it('should fetch samples for a specific test', () => {
            const mockSamples: Sample[] = [
                {
                    id: 1,
                    tagNumber: 'TAG001',
                    component: 'Engine',
                    location: 'Plant A',
                    lubeType: 'Oil',
                    qualityClass: 'Q',
                    sampleDate: new Date('2024-01-01'),
                    status: 'Active'
                },
                {
                    id: 2,
                    tagNumber: 'TAG002',
                    component: 'Gearbox',
                    location: 'Plant B',
                    lubeType: 'Grease',
                    qualityClass: 'QAG',
                    sampleDate: new Date('2024-01-02'),
                    status: 'Active'
                }
            ];

            service.getSamplesByTest(1).subscribe(samples => {
                expect(samples).toEqual(mockSamples);
                expect(service.samples()).toEqual(mockSamples);
                expect(service.isLoading()).toBe(false);
            });

            expect(service.isLoading()).toBe(true);

            const req = httpMock.expectOne(`${baseUrl}/by-test/1`);
            expect(req.request.method).toBe('GET');
            req.flush(mockSamples.map(s => ({ ...s, sampleDate: s.sampleDate.toISOString() })));
        });

        it('should handle errors when fetching samples by test', () => {
            service.getSamplesByTest(1).subscribe({
                next: () => fail('should have failed'),
                error: (error) => {
                    expect(error).toBeDefined();
                    expect(service.error()).toContain('Failed to load samples for test 1');
                    expect(service.isLoading()).toBe(false);
                }
            });

            const req = httpMock.expectOne(`${baseUrl}/by-test/1`);
            req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
        });
    });

    describe('getSamples', () => {
        it('should fetch samples without filter', () => {
            const mockSamples: Sample[] = [
                {
                    id: 1,
                    tagNumber: 'TAG001',
                    component: 'Engine',
                    location: 'Plant A',
                    lubeType: 'Oil',
                    qualityClass: 'Q',
                    sampleDate: new Date('2024-01-01'),
                    status: 'Active'
                }
            ];

            service.getSamples().subscribe(samples => {
                expect(samples).toEqual(mockSamples);
            });

            const req = httpMock.expectOne(baseUrl);
            expect(req.request.method).toBe('GET');
            req.flush(mockSamples.map(s => ({ ...s, sampleDate: s.sampleDate.toISOString() })));
        });

        it('should fetch samples with filter parameters', () => {
            const filter: SampleFilter = {
                tagNumber: 'TAG001',
                component: 'Engine',
                fromDate: new Date('2024-01-01'),
                toDate: new Date('2024-01-31'),
                status: 1
            };

            service.getSamples(filter).subscribe();

            const req = httpMock.expectOne(req => req.url === baseUrl && req.params.has('tagNumber'));
            expect(req.request.params.get('tagNumber')).toBe('TAG001');
            expect(req.request.params.get('component')).toBe('Engine');
            expect(req.request.params.get('status')).toBe('1');
            req.flush([]);
        });
    });

    describe('getSample', () => {
        it('should fetch a specific sample', () => {
            const mockSample: Sample = {
                id: 1,
                tagNumber: 'TAG001',
                component: 'Engine',
                location: 'Plant A',
                lubeType: 'Oil',
                qualityClass: 'Q',
                sampleDate: new Date('2024-01-01'),
                status: 'Active'
            };

            service.getSample(1).subscribe(sample => {
                expect(sample).toEqual(mockSample);
                expect(service.selectedSample()).toEqual(mockSample);
            });

            const req = httpMock.expectOne(`${baseUrl}/1`);
            expect(req.request.method).toBe('GET');
            req.flush({ ...mockSample, sampleDate: mockSample.sampleDate.toISOString() });
        });

        it('should handle errors when fetching a specific sample', () => {
            service.getSample(999).subscribe({
                next: () => fail('should have failed'),
                error: (error) => {
                    expect(error).toBeDefined();
                    expect(service.error()).toContain('Failed to load sample 999');
                }
            });

            const req = httpMock.expectOne(`${baseUrl}/999`);
            req.flush('Not found', { status: 404, statusText: 'Not Found' });
        });
    });

    describe('getSampleHistory', () => {
        it('should fetch sample history', () => {
            const mockHistory: SampleHistory[] = [
                {
                    sampleId: 1,
                    tagNumber: 'TAG001',
                    sampleDate: new Date('2024-01-01'),
                    testName: 'TAN by Color Indication',
                    status: 'Complete',
                    entryDate: new Date('2024-01-02')
                },
                {
                    sampleId: 1,
                    tagNumber: 'TAG001',
                    sampleDate: new Date('2024-01-15'),
                    testName: 'TAN by Color Indication',
                    status: 'Complete',
                    entryDate: new Date('2024-01-16')
                }
            ];

            service.getSampleHistory(1, 1).subscribe(history => {
                expect(history).toEqual(mockHistory);
                expect(service.sampleHistory()).toEqual(mockHistory);
            });

            const req = httpMock.expectOne(`${baseUrl}/1/history/1`);
            expect(req.request.method).toBe('GET');
            req.flush(mockHistory.map(h => ({
                ...h,
                sampleDate: h.sampleDate.toISOString(),
                entryDate: h.entryDate?.toISOString()
            })));
        });
    });

    describe('State Management', () => {
        it('should select a sample', () => {
            const sample: Sample = {
                id: 1,
                tagNumber: 'TAG001',
                component: 'Engine',
                location: 'Plant A',
                lubeType: 'Oil',
                qualityClass: 'Q',
                sampleDate: new Date('2024-01-01'),
                status: 'Active'
            };

            service.selectSample(sample);
            expect(service.selectedSample()).toEqual(sample);
            expect(service.hasSelectedSample()).toBe(true);
        });

        it('should clear selected sample', () => {
            const sample: Sample = {
                id: 1,
                tagNumber: 'TAG001',
                component: 'Engine',
                location: 'Plant A',
                lubeType: 'Oil',
                qualityClass: 'Q',
                sampleDate: new Date('2024-01-01'),
                status: 'Active'
            };

            service.selectSample(sample);
            expect(service.hasSelectedSample()).toBe(true);

            service.selectSample(null);
            expect(service.hasSelectedSample()).toBe(false);
            expect(service.selectedSample()).toBeNull();
        });

        it('should clear error state', () => {
            // Trigger an error first
            service.getSample(999).subscribe({
                error: () => {
                    expect(service.hasError()).toBe(true);

                    service.clearError();
                    expect(service.hasError()).toBe(false);
                    expect(service.error()).toBeNull();
                }
            });

            const req = httpMock.expectOne(`${baseUrl}/999`);
            req.flush('Not found', { status: 404, statusText: 'Not Found' });
        });

        it('should clear all data', () => {
            const sample: Sample = {
                id: 1,
                tagNumber: 'TAG001',
                component: 'Engine',
                location: 'Plant A',
                lubeType: 'Oil',
                qualityClass: 'Q',
                sampleDate: new Date('2024-01-01'),
                status: 'Active'
            };

            service.selectSample(sample);
            service.clearData();

            expect(service.selectedSample()).toBeNull();
            expect(service.samples()).toEqual([]);
            expect(service.sampleHistory()).toEqual([]);
            expect(service.error()).toBeNull();
        });
    });

    describe('Computed Signals', () => {
        it('should compute hasSelectedSample correctly', () => {
            expect(service.hasSelectedSample()).toBe(false);

            const sample: Sample = {
                id: 1,
                tagNumber: 'TAG001',
                component: 'Engine',
                location: 'Plant A',
                lubeType: 'Oil',
                qualityClass: 'Q',
                sampleDate: new Date('2024-01-01'),
                status: 'Active'
            };

            service.selectSample(sample);
            expect(service.hasSelectedSample()).toBe(true);
        });

        it('should compute hasError correctly', () => {
            expect(service.hasError()).toBe(false);

            service.getSample(999).subscribe({
                error: () => {
                    expect(service.hasError()).toBe(true);
                }
            });

            const req = httpMock.expectOne(`${baseUrl}/999`);
            req.flush('Not found', { status: 404, statusText: 'Not Found' });
        });
    });
});