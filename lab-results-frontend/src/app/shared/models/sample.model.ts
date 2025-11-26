export interface Sample {
    id: number;
    tagNumber: string;
    component: string;
    location: string;
    lubeType: string;
    qualityClass?: string;
    sampleDate: Date;
    status: string;
}

export interface SampleHistory {
    sampleId: number;
    tagNumber: string;
    sampleDate: Date;
    testName: string;
    status: string;
    entryDate?: Date;
}

export interface SampleFilter {
    tagNumber?: string;
    component?: string;
    location?: string;
    lubeType?: string;
    fromDate?: Date;
    toDate?: Date;
    status?: number;
}