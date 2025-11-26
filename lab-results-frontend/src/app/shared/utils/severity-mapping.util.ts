/**
 * Severity mapping utilities for ferrography particle analysis
 * 
 * This utility provides functions to map particle analysis findings to
 * overall ferrography severity levels and implement business rules.
 */

import { ParticleAnalysisData, ParticleAnalysis } from '../models/particle-analysis.model';

/**
 * Severity levels for ferrography analysis
 */
export enum SeverityLevel {
    NORMAL = 1,
    SLIGHT = 2,
    MODERATE = 3,
    SEVERE = 4
}

/**
 * Severity level descriptions
 */
export const SEVERITY_DESCRIPTIONS = {
    [SeverityLevel.NORMAL]: 'Normal - No significant wear particles detected',
    [SeverityLevel.SLIGHT]: 'Slight - Minor wear particles present, monitor condition',
    [SeverityLevel.MODERATE]: 'Moderate - Noticeable wear particles, investigate cause',
    [SeverityLevel.SEVERE]: 'Severe - Critical wear particles detected, immediate action required'
};

/**
 * Particle type severity weights for overall calculation
 */
const PARTICLE_TYPE_WEIGHTS: { [key: number]: number } = {
    // Wear particles
    1: 1.0,  // Cutting Wear - normal wear
    2: 1.2,  // Sliding Wear - moderate concern
    3: 1.8,  // Severe Sliding - high concern
    4: 1.5,  // Fatigue Particles - moderate-high concern
    5: 1.3,  // Spherical Particles - moderate concern
    8: 1.1,  // Nonferrous Particles - low-moderate concern

    // Oxide particles
    6: 1.4,  // Dark Metallic Oxides - moderate concern
    7: 1.6,  // Red Oxides - moderate-high concern

    // Contaminants
    9: 0.8,  // Fibers - low concern
    10: 0.9  // Contaminants - low concern
};

/**
 * Calculate overall severity from particle analysis data
 */
export function calculateOverallSeverity(particleData: ParticleAnalysisData): number {
    if (!particleData.analyses || particleData.analyses.length === 0) {
        return SeverityLevel.NORMAL;
    }

    const activeAnalyses = particleData.analyses.filter(analysis =>
        analysis.severity > 0 || hasSignificantSubTypeValues(analysis)
    );

    if (activeAnalyses.length === 0) {
        return SeverityLevel.NORMAL;
    }

    // Calculate weighted severity score
    let totalWeightedScore = 0;
    let totalWeight = 0;

    activeAnalyses.forEach(analysis => {
        const weight = PARTICLE_TYPE_WEIGHTS[analysis.particleTypeId] || 1.0;
        const particleSeverity = calculateParticleSeverity(analysis);

        totalWeightedScore += particleSeverity * weight;
        totalWeight += weight;
    });

    const averageScore = totalWeight > 0 ? totalWeightedScore / totalWeight : 0;

    // Apply business rules for severity escalation
    const finalSeverity = applySeverityBusinessRules(averageScore, activeAnalyses);

    return Math.min(Math.max(Math.round(finalSeverity), SeverityLevel.NORMAL), SeverityLevel.SEVERE);
}

/**
 * Calculate severity for an individual particle analysis
 */
function calculateParticleSeverity(analysis: ParticleAnalysis): number {
    // Start with explicit severity if set
    let severity = analysis.severity || 0;

    // Enhance severity based on sub-type values
    const subTypeEnhancement = calculateSubTypeSeverityEnhancement(analysis.subTypeValues);
    severity = Math.max(severity, subTypeEnhancement);

    return severity;
}

/**
 * Calculate severity enhancement based on sub-type values
 */
function calculateSubTypeSeverityEnhancement(subTypeValues: { [categoryId: number]: number | null }): number {
    let maxEnhancement = 0;

    Object.entries(subTypeValues).forEach(([categoryId, value]) => {
        if (value === null || value === undefined) return;

        const categoryIdNum = parseInt(categoryId, 10);
        let enhancement = 0;

        switch (categoryIdNum) {
            case 1: // Severity category
                enhancement = value;
                break;
            case 2: // Heat category
                if (value >= 4) enhancement = 3; // Purple, Melted, Charred
                else if (value >= 2) enhancement = 2; // Straw, Purple
                else if (value === 1) enhancement = 1; // Blue
                break;
            case 3: // Concentration category
                if (value >= 4) enhancement = 3; // Heavy
                else if (value >= 3) enhancement = 2; // Many
                else if (value >= 2) enhancement = 1; // Moderate
                break;
            case 4: // Size Average category
            case 5: // Size Max category
                if (value >= 5) enhancement = 2; // Huge (>100μm)
                else if (value >= 4) enhancement = 1; // Large (40-100μm)
                break;
            case 6: // Color category
                // Specific colors may indicate severity
                if ([1, 2].includes(value)) enhancement = 1; // Red, Black
                break;
            case 7: // Texture category
                if ([3, 4, 5].includes(value)) enhancement = 1; // Pitted, Striated, Smeared
                break;
            case 8: // Composition category
                if ([1, 2].includes(value)) enhancement = 1; // Ferrous, Cupric metals
                break;
        }

        maxEnhancement = Math.max(maxEnhancement, enhancement);
    });

    return maxEnhancement;
}

/**
 * Check if analysis has significant sub-type values
 */
function hasSignificantSubTypeValues(analysis: ParticleAnalysis): boolean {
    return Object.values(analysis.subTypeValues || {}).some(value =>
        value !== null && value !== undefined && value > 0
    );
}

/**
 * Apply business rules for severity escalation
 */
function applySeverityBusinessRules(averageScore: number, analyses: ParticleAnalysis[]): number {
    let finalSeverity = averageScore;

    // Rule 1: Multiple severe sliding particles escalate to severe
    const severeSlidingCount = analyses.filter(a =>
        a.particleTypeId === 3 && (a.severity >= 3 || calculateParticleSeverity(a) >= 3)
    ).length;

    if (severeSlidingCount >= 2) {
        finalSeverity = Math.max(finalSeverity, SeverityLevel.SEVERE);
    }

    // Rule 2: High concentration of any critical particle type
    const highConcentrationCritical = analyses.some(a => {
        const concentrationValue = a.subTypeValues[3]; // Concentration category
        const particleSeverity = calculateParticleSeverity(a);
        return concentrationValue !== null && concentrationValue !== undefined &&
            concentrationValue >= 4 && particleSeverity >= 3; // Heavy concentration + high severity
    });

    if (highConcentrationCritical) {
        finalSeverity = Math.max(finalSeverity, SeverityLevel.SEVERE);
    }

    // Rule 3: Multiple particle types with moderate+ severity
    const moderateParticleCount = analyses.filter(a => calculateParticleSeverity(a) >= 2).length;
    if (moderateParticleCount >= 4) {
        finalSeverity = Math.max(finalSeverity, SeverityLevel.MODERATE);
    }

    // Rule 4: Large particles with high heat treatment
    const largeHotParticles = analyses.some(a => {
        const sizeValue = Math.max(a.subTypeValues[4] || 0, a.subTypeValues[5] || 0); // Size categories
        const heatValue = a.subTypeValues[2] || 0; // Heat category
        return sizeValue >= 4 && heatValue >= 3; // Large+ size and Straw+ heat
    });

    if (largeHotParticles) {
        finalSeverity = Math.max(finalSeverity, SeverityLevel.MODERATE);
    }

    return finalSeverity;
}

/**
 * Get severity level description
 */
export function getSeverityDescription(severity: number): string {
    return SEVERITY_DESCRIPTIONS[severity as SeverityLevel] || 'Unknown severity level';
}

/**
 * Get severity color class for UI styling
 */
export function getSeverityColorClass(severity: number): string {
    switch (severity) {
        case SeverityLevel.NORMAL:
            return 'severity-normal';
        case SeverityLevel.SLIGHT:
            return 'severity-slight';
        case SeverityLevel.MODERATE:
            return 'severity-moderate';
        case SeverityLevel.SEVERE:
            return 'severity-severe';
        default:
            return 'severity-unknown';
    }
}

/**
 * Validate severity level
 */
export function isValidSeverityLevel(severity: number): boolean {
    return Object.values(SeverityLevel).includes(severity);
}

/**
 * Get recommendations based on severity level
 */
export function getSeverityRecommendations(severity: number, analyses: ParticleAnalysis[]): string[] {
    const recommendations: string[] = [];

    switch (severity) {
        case SeverityLevel.NORMAL:
            recommendations.push('Continue normal monitoring schedule');
            break;
        case SeverityLevel.SLIGHT:
            recommendations.push('Monitor more frequently');
            recommendations.push('Review operating conditions');
            break;
        case SeverityLevel.MODERATE:
            recommendations.push('Investigate root cause of wear');
            recommendations.push('Consider maintenance intervention');
            recommendations.push('Increase sampling frequency');
            break;
        case SeverityLevel.SEVERE:
            recommendations.push('Immediate investigation required');
            recommendations.push('Consider equipment shutdown');
            recommendations.push('Implement corrective maintenance');
            break;
    }

    // Add specific recommendations based on particle types
    const criticalParticles = analyses.filter(a => calculateParticleSeverity(a) >= 3);
    criticalParticles.forEach(analysis => {
        switch (analysis.particleTypeId) {
            case 3: // Severe Sliding
                recommendations.push('Check for inadequate lubrication or excessive loading');
                break;
            case 4: // Fatigue Particles
                recommendations.push('Inspect for surface fatigue and spalling');
                break;
            case 6: // Dark Metallic Oxides
            case 7: // Red Oxides
                recommendations.push('Check for water contamination and corrosion');
                break;
        }
    });

    return [...new Set(recommendations)]; // Remove duplicates
}