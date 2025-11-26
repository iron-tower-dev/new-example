/**
 * Particle Analysis Data Models
 * 
 * These interfaces define the data structures for particle analysis functionality
 * used in ferrography and inspect filter test components.
 */

/**
 * Main particle analysis data structure containing all analyses and summary
 */
export interface ParticleAnalysisData {
    analyses: ParticleAnalysis[];
    overallSeverity: number;
    isValid: boolean;
    summary: {
        totalParticles: number;
        criticalParticles: number;
        recommendations: string[];
    };
}

/**
 * Individual particle analysis for a specific particle type
 */
export interface ParticleAnalysis {
    particleTypeId: number;
    subTypeValues: { [categoryId: number]: number | null };
    comments: string;
    severity: number;
    status: 'active' | 'review' | 'complete';
}

/**
 * Definition of a particle type with display information
 */
export interface ParticleTypeDefinition {
    id: number;
    type: string;
    description: string;
    image1: string;
    image2: string;
    active: boolean;
    sortOrder: number;
    category: 'wear' | 'oxide' | 'contaminant' | 'other';
}

/**
 * Category of particle sub-types (e.g., Heat, Concentration, Size)
 */
export interface ParticleSubTypeCategory {
    id: number;
    description: string;
    active: boolean;
    sortOrder: number;
    subTypes: ParticleSubTypeDefinition[];
}

/**
 * Individual sub-type definition within a category
 */
export interface ParticleSubTypeDefinition {
    particleSubTypeCategoryId: number;
    value: number;
    description: string;
    active: boolean;
    sortOrder: number;
}

/**
 * Category definition from the database
 */
export interface ParticleSubTypeCategoryDefinition {
    id: number;
    description: string;
    active: boolean;
    sortOrder: number;
}

/**
 * Control data configuration from the database
 */
export interface ControlData {
    id: number;
    name: string;
    controlValue: string;
}

/**
 * Request/response interfaces for API integration
 */
export interface ParticleAnalysisRequest {
    testId: number;
    sampleId: number;
    particleAnalysisData: ParticleAnalysisData;
}

export interface ParticleAnalysisResponse {
    success: boolean;
    data?: ParticleAnalysisData;
    error?: string;
}

/**
 * Configuration interface for particle analysis card component
 */
export interface ParticleAnalysisConfig {
    testId: number;
    readonly: boolean;
    showImages: boolean;
    viewFilter: 'all' | 'review';
    enableSeverityCalculation: boolean;
}

/**
 * Validation result for particle analysis data
 */
export interface ParticleAnalysisValidation {
    isValid: boolean;
    errors: { [particleTypeId: number]: string[] };
    warnings: { [particleTypeId: number]: string[] };
}