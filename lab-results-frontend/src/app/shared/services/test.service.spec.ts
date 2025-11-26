import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestService } from './test.service';
import { Test, TestTemplate, TestResult, SaveTestResultRequest, TestCalculationRequest } from '../models/test.model';

describe('TestService', () => {
    let service: TestService;
    let httpMock: HttpTestingController;
    const baseUrl = 'http://localhost:5000/api/tests';

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [TestService]
        });
        service = TestBed.inject(TestService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getTests', () => {
        it('should fetch all tests', () => {
            const mockTests: Test[] = [
                { testId: 1, testName: 'TAN by Color Indication', active: true },
                { testId: 2, testName: 'Viscosity @ 40°C', active: true }
            ];

            service.getTests().subscribe(tests => {
                expect(tests).toEqual(mockTests);
            });

            const req = httpMock.expectOne(baseUrl);
            expect(req.request.method).toBe('GET');
            req.flush(mockTests);
        });

        it('should handle errors when fetching tests', () => {
            service.getTests().subscribe({
                next: () => fail('should have failed'),
                error: (error) => {
                    expect(error).toBeDefined();
                    expect(service.error()).toContain('Failed to load tests');
                }
            });

            const req = httpMock.expectOne(baseUrl);
            req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
        });
    });

    describe('getTest', () => {
        it('should fetch a specific test', () => {
            const mockTest: Test = { testId: 1, testName: 'TAN by Color Indication', active: true };

            service.getTest(1).subscribe(test => {
                expect(test).toEqual(mockTest);
                expect(service.selectedTest()).toEqual(mockTest);
            });

            const req = httpMock.expectOne(`${baseUrl}/1`);
            expect(req.request.method).toBe('GET');
            req.flush(mockTest);
        });
    });

    describe('getTestTemplate', () => {
        it('should fetch test template', () => {
            const mockTemplate: TestTemplate = {
                testId: 1,
                testName: 'TAN by Color Indication',
                fields: [
                    { fieldName: 'sampleWeight', displayName: 'Sample Weight (g)', fieldType: 'number', isRequired: true, isCalculated: false },
                    { fieldName: 'finalBuret', displayName: 'Final Buret (mL)', fieldType: 'number', isRequired: true, isCalculated: false }
                ],
                maxTrials: 4,
                requiresCalculation: true,
                supportsFileUpload: false
            };

            service.getTestTemplate(1).subscribe(template => {
                expect(template).toEqual(mockTemplate);
                expect(service.testTemplate()).toEqual(mockTemplate);
            });

            const req = httpMock.expectOne(`${baseUrl}/1/template`);
            expect(req.request.method).toBe('GET');
            req.flush(mockTemplate);
        });
    });

    describe('getTestResults', () => {
        it('should fetch test results for a sample', () => {
            const mockResult: TestResult = {
                sampleId: 1,
                testId: 1,
                trials: [
                    { trialNumber: 1, values: { sampleWeight: 10, finalBuret: 5.5, tanResult: 3.08 }, isComplete: true }
                ],
                status: 'C',
                entryDate: new Date('2024-01-01')
            };

            service.getTestResults(1, 1).subscribe(result => {
                expect(result).toEqual(mockResult);
                expect(service.testResult()).toEqual(mockResult);
            });

            const req = httpMock.expectOne(`${baseUrl}/1/results/1`);
            expect(req.request.method).toBe('GET');
            req.flush({ ...mockResult, entryDate: mockResult.entryDate?.toISOString() });
        });
    });

    describe('saveTestResults', () => {
        it('should save test results', () => {
            const request: SaveTestResultRequest = {
                sampleId: 1,
                testId: 1,
                trials: [
                    { trialNumber: 1, values: { sampleWeight: 10, finalBuret: 5.5 }, isComplete: true }
                ],
                entryId: 'user123'
            };

            const mockResponse = { message: 'Results saved successfully', recordsSaved: 1 };

            service.saveTestResults(1, request).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(`${baseUrl}/1/results`);
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);
            req.flush(mockResponse);
        });
    });

    describe('updateTestResults', () => {
        it('should update test results', () => {
            const request: SaveTestResultRequest = {
                sampleId: 1,
                testId: 1,
                trials: [
                    { trialNumber: 1, values: { sampleWeight: 12, finalBuret: 6.0 }, isComplete: true }
                ],
                entryId: 'user123'
            };

            const mockResponse = { message: 'Results updated successfully', recordsUpdated: 1 };

            service.updateTestResults(1, 1, request).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(`${baseUrl}/1/results/1`);
            expect(req.request.method).toBe('PUT');
            expect(req.request.body).toEqual(request);
            req.flush(mockResponse);
        });
    });

    describe('deleteTestResults', () => {
        it('should delete test results', () => {
            const mockResponse = { message: 'Results deleted successfully', recordsDeleted: 1 };

            service.deleteTestResults(1, 1).subscribe(response => {
                expect(response).toEqual(mockResponse);
                expect(service.testResult()).toBeNull();
            });

            const req = httpMock.expectOne(`${baseUrl}/1/results/1`);
            expect(req.request.method).toBe('DELETE');
            req.flush(mockResponse);
        });
    });

    describe('calculateTestResult', () => {
        it('should calculate test result', () => {
            const request: TestCalculationRequest = {
                testId: 1,
                inputValues: { sampleWeight: 10, finalBuret: 5.5 }
            };

            const mockResponse = {
                result: 3.08,
                isValid: true,
                intermediateValues: { formula: '(Final Buret * 5.61) / Sample Weight' }
            };

            service.calculateTestResult(1, request).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne(`${baseUrl}/1/calculate`);
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);
            req.flush(mockResponse);
        });
    });

    describe('calculateNAS', () => {
        it('should calculate NAS values', () => {
            const request = {
                particleCounts: { 4: 1000, 6: 500, 14: 100, 21: 50, 38: 10, 70: 2 }
            };

            const mockResponse = {
                highestNAS: 9,
                channelNASValues: { 4: 9, 6: 8, 14: 7, 21: 6, 38: 5, 70: 4 },
                isValid: true
            };

            service.calculateNAS(request).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne('http://localhost:5000/api/lookups/nas/calculate');
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);
            req.flush(mockResponse);
        });
    });

    describe('getNLGIForPenetration', () => {
        it('should get NLGI grade for penetration value', () => {
            const mockResponse = { penetrationValue: 250, nlgi: '2' };

            service.getNLGIForPenetration(250).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne('http://localhost:5000/api/lookups/nlgi/penetration/250');
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });
    });

    describe('Particle Analysis Methods', () => {
        it('should get particle types', () => {
            const mockParticleTypes = [
                { id: 1, name: 'Cutting Wear', description: 'Sharp metallic particles' },
                { id: 2, name: 'Sliding Wear', description: 'Smooth metallic particles' }
            ];

            service.getParticleTypes().subscribe(types => {
                expect(types).toEqual(mockParticleTypes);
            });

            const req = httpMock.expectOne('http://localhost:5000/api/particle-analysis/particle-types');
            expect(req.request.method).toBe('GET');
            req.flush(mockParticleTypes);
        });

        it('should save Inspect Filter results', () => {
            const request = {
                sampleId: 1,
                testId: 15,
                particleTypes: [
                    { particleTypeId: 1, severity: 3, concentration: 2 }
                ]
            };

            const mockResponse = { message: 'Inspect Filter results saved', recordsSaved: 1 };

            service.saveInspectFilterResults(request).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne('http://localhost:5000/api/particle-analysis/inspect-filter');
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);
            req.flush(mockResponse);
        });

        it('should save Ferrography results', () => {
            const request = {
                sampleId: 1,
                testId: 16,
                dilutionFactor: '1:100',
                particleTypes: [
                    { particleTypeId: 1, severity: 4, concentration: 3 }
                ]
            };

            const mockResponse = { message: 'Ferrography results saved', recordsSaved: 1 };

            service.saveFerrographyResults(request).subscribe(response => {
                expect(response).toEqual(mockResponse);
            });

            const req = httpMock.expectOne('http://localhost:5000/api/particle-analysis/ferrography');
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toEqual(request);
            req.flush(mockResponse);
        });
    });

    describe('State Management', () => {
        it('should select a test and load template', () => {
            const test: Test = { testId: 1, testName: 'TAN by Color Indication', active: true };
            const mockTemplate: TestTemplate = {
                testId: 1,
                testName: 'TAN by Color Indication',
                fields: [],
                maxTrials: 4,
                requiresCalculation: true,
                supportsFileUpload: false
            };

            service.selectTest(test);
            expect(service.selectedTest()).toEqual(test);

            const req = httpMock.expectOne(`${baseUrl}/1/template`);
            req.flush(mockTemplate);
        });

        it('should clear selected test', () => {
            const testData: Test = { testId: 1, testName: 'TAN by Color Indication', active: true };
            service.selectTest(testData);

            // Handle the HTTP request made by selectTest
            const req = httpMock.expectOne(`${baseUrl}/1/template`);
            req.flush({
                testId: 1,
                testName: 'TAN by Color Indication',
                fields: [],
                maxTrials: 4,
                requiresCalculation: true,
                supportsFileUpload: false
            });

            expect(service.hasSelectedTest()).toBe(true);

            service.selectTest(null);
            expect(service.hasSelectedTest()).toBe(false);
            expect(service.selectedTest()).toBeNull();
            expect(service.testTemplate()).toBeNull();
        });

        it('should clear all data', () => {
            const testData: Test = { testId: 1, testName: 'TAN by Color Indication', active: true };
            service.selectTest(testData);

            // Handle the HTTP request made by selectTest
            const req = httpMock.expectOne(`${baseUrl}/1/template`);
            req.flush({
                testId: 1,
                testName: 'TAN by Color Indication',
                fields: [],
                maxTrials: 4,
                requiresCalculation: true,
                supportsFileUpload: false
            });

            service.clearData();

            expect(service.selectedTest()).toBeNull();
            expect(service.testTemplate()).toBeNull();
            expect(service.testResult()).toBeNull();
            expect(service.testResultHistory()).toEqual([]);
            expect(service.error()).toBeNull();
        });
    });

    describe('Computed Signals', () => {
        it('should compute hasSelectedTest correctly', () => {
            expect(service.hasSelectedTest()).toBe(false);

            const testData: Test = { testId: 1, testName: 'TAN by Color Indication', active: true };
            service.selectTest(testData);

            // Handle the HTTP request made by selectTest
            const req = httpMock.expectOne(`${baseUrl}/1/template`);
            req.flush({
                testId: 1,
                testName: 'TAN by Color Indication',
                fields: [],
                maxTrials: 4,
                requiresCalculation: true,
                supportsFileUpload: false
            });

            expect(service.hasSelectedTest()).toBe(true);
        });

        it('should compute hasTestTemplate correctly', () => {
            expect(service.hasTestTemplate()).toBe(false);

            const testData: Test = { testId: 1, testName: 'TAN by Color Indication', active: true };
            const mockTemplate: TestTemplate = {
                testId: 1,
                testName: 'TAN by Color Indication',
                fields: [],
                maxTrials: 4,
                requiresCalculation: true,
                supportsFileUpload: false
            };

            service.selectTest(testData);
            const req = httpMock.expectOne(`${baseUrl}/1/template`);
            req.flush(mockTemplate);

            expect(service.hasTestTemplate()).toBe(true);
        });
    });
});