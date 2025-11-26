export interface Test {
    testId: number;
    testName: string;
    testDescription?: string;
    active: boolean;
}

export interface TestTemplate {
    testId: number;
    testName: string;
    testDescription?: string;
    fields: TestField[];
    maxTrials: number;
    requiresCalculation: boolean;
    supportsFileUpload: boolean;
    calculationFormula?: string;
    validationRules?: { [key: string]: any };
}

export interface TestField {
    fieldName: string;
    displayName: string;
    fieldType: 'number' | 'text' | 'dropdown' | 'file';
    isRequired: boolean;
    isCalculated: boolean;
    validationPattern?: string;
    minValue?: number;
    maxValue?: number;
    decimalPlaces?: number;
    dropdownOptions?: string[];
    unit?: string;
    helpText?: string;
}

export interface TestResult {
    sampleId: number;
    testId: number;
    trials: TrialResult[];
    comments?: string;
    status: 'X' | 'E' | 'C'; // X = Pending, E = In Progress, C = Complete
    entryId?: string;
    entryDate?: Date;
}

export interface TrialResult {
    trialNumber: number;
    values: { [key: string]: any };
    calculatedResult?: number;
    isComplete: boolean;
}

export interface SaveTestResultRequest {
    sampleId: number;
    testId: number;
    trials: TrialResult[];
    comments?: string;
    entryId: string;
}

export interface TestCalculationRequest {
    testId: number;
    inputValues: { [key: string]: any };
}

export interface TestCalculationResult {
    result?: number;
    isValid: boolean;
    errorMessage?: string;
    intermediateValues?: { [key: string]: any };
}

// TAN-specific interfaces
export interface TanTrialData {
    sampleWeight?: number;
    finalBuret?: number;
    tanResult?: number;
}

export interface TanTestResult extends TestResult {
    trials: TanTrialResult[];
}

export interface TanTrialResult extends TrialResult {
    values: TanTrialData;
}

// Water-KF specific interfaces
export interface WaterKfTrialData {
    waterContent?: number;
    dataFile?: File;
}

export interface WaterKfTestResult extends TestResult {
    trials: WaterKfTrialResult[];
}

export interface WaterKfTrialResult extends TrialResult {
    values: WaterKfTrialData;
}

// TBN specific interfaces
export interface TbnTrialData {
    tbnResult?: number;
}

export interface TbnTestResult extends TestResult {
    trials: TbnTrialResult[];
}

export interface TbnTrialResult extends TrialResult {
    values: TbnTrialData;
}

// Emission Spectroscopy specific interfaces
export interface EmissionSpectroscopyTrialData {
    na?: number;
    cr?: number;
    sn?: number;
    si?: number;
    mo?: number;
    ca?: number;
    al?: number;
    ba?: number;
    mg?: number;
    ni?: number;
    mn?: number;
    zn?: number;
    p?: number;
    ag?: number;
    pb?: number;
    h?: number;
    b?: number;
    cu?: number;
    fe?: number;
    scheduleFerrography?: boolean;
    dataFile?: File;
}

export interface EmissionSpectroscopyTestResult extends TestResult {
    trials: EmissionSpectroscopyTrialResult[];
}

export interface EmissionSpectroscopyTrialResult extends TrialResult {
    values: EmissionSpectroscopyTrialData;
}

export interface EmissionSpectroscopyData {
    id: number;
    testId: number;
    trialNum: number;
    na?: number;
    cr?: number;
    sn?: number;
    si?: number;
    mo?: number;
    ca?: number;
    al?: number;
    ba?: number;
    mg?: number;
    ni?: number;
    mn?: number;
    zn?: number;
    p?: number;
    ag?: number;
    pb?: number;
    h?: number;
    b?: number;
    cu?: number;
    fe?: number;
    trialDate?: Date;
    status?: string;
    scheduleFerrography?: boolean;
}

export interface EmissionSpectroscopyCreateRequest {
    id: number;
    testId: number;
    trialNum: number;
    na?: number;
    cr?: number;
    sn?: number;
    si?: number;
    mo?: number;
    ca?: number;
    al?: number;
    ba?: number;
    mg?: number;
    ni?: number;
    mn?: number;
    zn?: number;
    p?: number;
    ag?: number;
    pb?: number;
    h?: number;
    b?: number;
    cu?: number;
    fe?: number;
    status?: string;
    scheduleFerrography?: boolean;
}

export interface EmissionSpectroscopyUpdateRequest {
    na?: number;
    cr?: number;
    sn?: number;
    si?: number;
    mo?: number;
    ca?: number;
    al?: number;
    ba?: number;
    mg?: number;
    ni?: number;
    mn?: number;
    zn?: number;
    p?: number;
    ag?: number;
    pb?: number;
    h?: number;
    b?: number;
    cu?: number;
    fe?: number;
    status?: string;
    scheduleFerrography?: boolean;
}